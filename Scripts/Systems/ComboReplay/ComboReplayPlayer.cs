using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ComboReplay
{
    /// <summary>
    /// Combo 回放播放器（REQ-114-03）
    /// 职责：从头读取 replay，模拟时间轴推进，以只读模式重放玩家操作
    /// </summary>
    public partial class ComboReplayPlayer : Node
    {
        private static ComboReplayPlayer _instance;
        public static ComboReplayPlayer Instance => _instance ??= new ComboReplayPlayer();

        // 回放数据
        private ComboReplayData _currentReplay;
        private int _currentActionIndex = 0;
        private int _currentComboIndex = 0;

        // 回放状态机
        private ReplayState _state = ReplayState.Idle;
        private float _elapsedTime = 0f;          // 已播放时间
        private float _playbackSpeed = 1.0f;      // 播放速度倍率
        private bool _isPaused = false;

        // 定时器
        private float _processAccumulator = 0f;

        // 玩家位置预览（用于可视化）
        private Vector2 _currentPlayerPos;
        private float _currentTime;

        // 信号
        public static Action<PlayerActionRecord> OnActionReached;
        public static Action<ComboRecord> OnComboReached;
        public static Action OnReplayFinished;
        public static Action<float, float> OnTimelineUpdated; // (elapsed, total)

        public enum ReplayState
        {
            Idle,
            Playing,
            Paused,
            Finished
        }

        public override void _Ready()
        {
            _instance = this;
        }

        public override void _Process(double delta)
        {
            if (_state != ReplayState.Playing)
                return;

            float deltaSec = (float)delta * _playbackSpeed;
            _elapsedTime += deltaSec;
            _processAccumulator += deltaSec;

            // 每帧驱动时间轴更新（平滑刷新UI）
            OnTimelineUpdated?.Invoke(_elapsedTime, _currentReplay?.DurationSeconds ?? 0f);

            // 每 ~50ms 处理一次事件（避免过于频繁）
            if (_processAccumulator < 0.05f)
                return;

            float processedTime = _processAccumulator;
            _processAccumulator = 0f;

            // 处理动作事件
            while (_currentActionIndex < _currentReplay.Actions.Count)
            {
                var action = _currentReplay.Actions[_currentActionIndex];
                if (action.Time <= _elapsedTime)
                {
                    _currentPlayerPos = new Vector2(action.PlayerPosX, action.PlayerPosY);
                    _currentTime = action.Time;
                    OnActionReached?.Invoke(action);
                    _currentActionIndex++;
                }
                else
                {
                    break;
                }
            }

            // 处理 Combo 事件
            while (_currentComboIndex < _currentReplay.Combos.Count)
            {
                var combo = _currentReplay.Combos[_currentComboIndex];
                if (combo.Time <= _elapsedTime)
                {
                    OnComboReached?.Invoke(combo);
                    _currentComboIndex++;
                }
                else
                {
                    break;
                }
            }

            // 检测结束
            if (_elapsedTime >= _currentReplay.DurationSeconds)
            {
                Stop();
                OnReplayFinished?.Invoke();
            }
        }

        /// <summary>
        /// 加载并开始播放回放
        /// </summary>
        public void LoadAndPlay(ComboReplayData replay)
        {
            if (replay == null)
            {
                GD.PrintErr("[ComboReplayPlayer] Cannot load null replay");
                return;
            }

            _currentReplay = replay;
            _currentActionIndex = 0;
            _currentComboIndex = 0;
            _elapsedTime = 0f;
            _processAccumulator = 0f;
            _isPaused = false;

            if (_currentReplay.Actions.Count > 0)
            {
                var firstAction = _currentReplay.Actions[0];
                _currentPlayerPos = new Vector2(firstAction.PlayerPosX, firstAction.PlayerPosY);
            }
            else
            {
                _currentPlayerPos = Vector2.Zero;
            }

            _state = ReplayState.Playing;
            GD.Print($"[ComboReplayPlayer] Started replay: {replay.Metadata.SceneName}, {replay.Actions.Count} actions, {replay.Combos.Count} combos, duration: {replay.DurationSeconds:F2}s");
        }

        /// <summary>
        /// 播放
        /// </summary>
        public void Play()
        {
            if (_currentReplay == null)
                return;

            if (_state == ReplayState.Finished)
            {
                // 从头重播
                _elapsedTime = 0f;
                _currentActionIndex = 0;
                _currentComboIndex = 0;
                _processAccumulator = 0f;
            }

            _state = ReplayState.Playing;
            _isPaused = false;
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (_state == ReplayState.Playing)
            {
                _state = ReplayState.Paused;
                _isPaused = true;
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _state = ReplayState.Finished;
            _isPaused = false;
        }

        /// <summary>
        /// 跳转到指定时间
        /// </summary>
        public void SeekTo(float timeSeconds)
        {
            if (_currentReplay == null)
                return;

            float clampedTime = Mathf.Clamp(timeSeconds, 0f, _currentReplay.DurationSeconds);
            _elapsedTime = clampedTime;

            // 重新扫描动作和combo索引
            _currentActionIndex = 0;
            _currentComboIndex = 0;

            for (int i = 0; i < _currentReplay.Actions.Count; i++)
            {
                if (_currentReplay.Actions[i].Time <= _elapsedTime)
                    _currentActionIndex = i + 1;
                else
                    break;
            }

            for (int i = 0; i < _currentReplay.Combos.Count; i++)
            {
                if (_currentReplay.Combos[i].Time <= _elapsedTime)
                    _currentComboIndex = i + 1;
                else
                    break;
            }

            // 更新当前位置
            if (_currentReplay.Actions.Count > 0 && _currentActionIndex > 0)
            {
                var lastAction = _currentReplay.Actions[Mathf.Min(_currentActionIndex - 1, _currentReplay.Actions.Count - 1)];
                _currentPlayerPos = new Vector2(lastAction.PlayerPosX, lastAction.PlayerPosY);
            }

            OnTimelineUpdated?.Invoke(_elapsedTime, _currentReplay.DurationSeconds);
        }

        /// <summary>
        /// 设置播放速度
        /// </summary>
        public void SetPlaybackSpeed(float speed)
        {
            _playbackSpeed = Mathf.Clamp(speed, 0.25f, 4.0f);
        }

        /// <summary>
        /// 获取当前回放数据
        /// </summary>
        public ComboReplayData GetCurrentReplay() => _currentReplay;

        /// <summary>
        /// 获取当前状态
        /// </summary>
        public ReplayState GetState() => _state;

        /// <summary>
        /// 获取当前播放时间
        /// </summary>
        public float GetCurrentTime() => _elapsedTime;

        /// <summary>
        /// 获取总时长
        /// </summary>
        public float GetTotalDuration() => _currentReplay?.DurationSeconds ?? 0f;

        /// <summary>
        /// 获取当前玩家位置（可视化用）
        /// </summary>
        public Vector2 GetCurrentPlayerPos() => _currentPlayerPos;

        /// <summary>
        /// 获取播放速度
        /// </summary>
        public float GetPlaybackSpeed() => _playbackSpeed;

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying() => _state == ReplayState.Playing;
    }
}
