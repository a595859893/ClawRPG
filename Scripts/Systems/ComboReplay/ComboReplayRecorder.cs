using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems.Combat;
using ClawRPG.Scripts.Systems.Pets;

namespace ClawRPG.Scripts.Systems.ComboReplay
{
    /// <summary>
    /// Combo回放录制器（REQ-114-02）
    /// 职责：订阅战斗事件，录制玩家操作序列，战斗结束时生成回放数据
    /// </summary>
    public partial class ComboReplayRecorder : BaseSystem
    {
        private static ComboReplayRecorder _instance;
        public static ComboReplayRecorder Instance => _instance ??= new ComboReplayRecorder();

        // 当前回放数据
        private ComboReplayData _currentReplay;
        private float _battleStartTime = 0f;
        private bool _isRecording = false;

        // 玩家位置记录间隔
        private float _lastPositionRecordTime = 0f;
        private float _positionRecordInterval = 0.1f; // 每100ms记录一次位置

        // 系统引用
        private SkillComboSystem _skillComboSystem;
        private ComboSystem _comboSystem;

        // 信号
        public static Action<ComboReplayData> OnReplayRecorded;

        public override void _Ready()
        {
            _instance = this;

            // 获取系统引用
            _skillComboSystem = GetNodeOrNull<SkillComboSystem>("/root/Game/SkillComboSystem");
            _comboSystem = GetNodeOrNull<ComboSystem>("/root/Game/ComboSystem");

            // 订阅战斗事件
            if (EventBusManager.Instance != null)
            {
                EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
                EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
                EventBusManager.Instance.Subscribe(EventBusManager.Events.EnemyDamaged, OnEnemyDamaged);
                EventBusManager.Instance.Subscribe(EventBusManager.Events.PlayerHealthChanged, OnPlayerDamaged);
            }

            // 订阅技能使用事件（通过 SkillComboSystem）
            if (_skillComboSystem != null)
            {
                // SkillComboSystem 有 RecordSkillUse，我们通过监听技能队列变化来记录
            }

            // 订阅宠物协同攻击事件（REQ-136）
            if (PetCombatCompanionSystem.Instance != null)
            {
                PetCombatCompanionSystem.SynergyAttackTriggered += OnSynergyAttackTriggered;
            }

            GD.Print("[ComboReplayRecorder] Initialized");
        }

        public override void _Process(double delta)
        {
            if (!_isRecording)
                return;

            float currentTime = Time.GetTicksMsec() / 1000f;

            // 定期记录玩家位置
            if (currentTime - _lastPositionRecordTime >= _positionRecordInterval)
            {
                RecordPlayerPosition(currentTime);
                _lastPositionRecordTime = currentTime;
            }
        }

        public override void _ExitTree()
        {
            // 取消订阅
            if (PetCombatCompanionSystem.Instance != null)
            {
                PetCombatCompanionSystem.SynergyAttackTriggered -= OnSynergyAttackTriggered;
            }
        }

        /// <summary>
        /// 战斗开始回调
        /// </summary>
        private void OnCombatStarted()
        {
            StartRecording();
        }

        /// <summary>
        /// 战斗结束回调
        /// </summary>
        private void OnCombatEnded()
        {
            EndRecording();
        }

        /// <summary>
        /// 敌人受伤回调 - 用于记录combo完成
        /// </summary>
        private void OnEnemyDamaged(object[] args)
        {
            if (!_isRecording || _currentReplay == null)
                return;

            // 从参数中提取信息: (enemyId, damage, attackerId)
            if (args.Length >= 3)
            {
                string enemyId = args[0]?.ToString() ?? "";
                int damage = Convert.ToInt32(args[1]);
                string attackerId = args[2]?.ToString() ?? "";

                // 只有玩家造成的伤害才记录
                if (attackerId == "player")
                {
                    // 这个信息用于combo录制时的上下文
                    GD.Print($"[ComboReplayRecorder] Player dealt {damage} to {enemyId}");
                }
            }
        }

        /// <summary>
        /// 玩家受伤回调 - 用于记录闪避动作
        /// </summary>
        private void OnPlayerDamaged(object[] args)
        {
            if (!_isRecording || _currentReplay == null)
                return;

            // 记录玩家受伤动作
            float currentTime = Time.GetTicksMsec() / 1000f;
            RecordAction(currentTime, PlayerActionType.Dodge, "", "", true);
        }

        /// <summary>
        /// 宠物协同攻击回调 - 用于记录协同攻击
        /// </summary>
        private void OnSynergyAttackTriggered(string petId, string targetId, int damage)
        {
            if (!_isRecording || _currentReplay == null)
                return;

            float currentTime = Time.GetTicksMsec() / 1000f;
            RecordAction(currentTime, PlayerActionType.SkillUse, "pet_synergy", targetId);
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
            {
                GD.PrintWrn("[ComboReplayRecorder] Already recording");
                return;
            }

            _currentReplay = new ComboReplayData();
            _currentReplay.Version = 1;
            _currentReplay.Seed = (int)Time.GetTicksMsec();
            _currentReplay.StartTimestamp = Time.GetUnixTimeFromSystem();
            _battleStartTime = Time.GetTicksMsec() / 1000f;
            _lastPositionRecordTime = _battleStartTime;
            _isRecording = true;

            // 获取玩家等级
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null && player.HasMethod("GetLevel"))
            {
                _currentReplay.Metadata.PlayerLevel = (int)player.Call("GetLevel");
            }
            else
            {
                _currentReplay.Metadata.PlayerLevel = 1;
            }

            // 获取当前场景名称
            _currentReplay.Metadata.SceneName = GetTree().CurrentScene?.Name ?? "";

            // 获取随机种子（从 Godot 的随机数生成器）
            _currentReplay.Seed = (int)GD.Seed();

            GD.Print($"[ComboReplayRecorder] Started recording - seed: {_currentReplay.Seed}");
        }

        /// <summary>
        /// 结束录制并生成回放数据
        /// </summary>
        public void EndRecording()
        {
            if (!_isRecording)
            {
                GD.PrintWrn("[ComboReplayRecorder] Not recording");
                return;
            }

            _isRecording = false;

            if (_currentReplay == null)
            {
                GD.PrintWrn("[ComboReplayRecorder] No replay data to save");
                return;
            }

            float currentTime = Time.GetTicksMsec() / 1000f;
            _currentReplay.DurationSeconds = currentTime - _battleStartTime;
            _currentReplay.Metadata.CreatedAt = Time.GetUnixTimeFromSystem();
            _currentReplay.Metadata.GameVersion = "1.0.0"; // TODO: 从游戏配置获取

            // 战斗结果根据是否有敌人存活判断
            var enemies = GetTree().GetNodesInGroup("enemy");
            _currentReplay.Metadata.EnemyCount = enemies.Count;
            _currentReplay.Metadata.Result = enemies.Count == 0 ? "victory" : "defeat";

            GD.Print($"[ComboReplayRecorder] Recording ended - duration: {_currentReplay.DurationSeconds:F2}s, actions: {_currentReplay.Actions.Count}, combos: {_currentReplay.Combos.Count}");

            // 发射信号
            OnReplayRecorded?.Invoke(_currentReplay);
        }

        /// <summary>
        /// 记录玩家使用技能（由 SkillComboSystem 通知）
        /// </summary>
        public void RecordSkillUse(string skillId, string targetId = "")
        {
            if (!_isRecording || _currentReplay == null)
                return;

            float currentTime = Time.GetTicksMsec() / 1000f;
            RecordAction(currentTime, PlayerActionType.SkillUse, skillId, targetId);
        }

        /// <summary>
        /// 记录Combo完成（由 SkillComboSystem 通知）
        /// </summary>
        public void RecordComboCompletion(string comboId, string comboName, List<string> skillSequence, int damage, bool killed)
        {
            if (!_isRecording || _currentReplay == null)
                return;

            float currentTime = Time.GetTicksMsec() / 1000f;
            float relativeTime = currentTime - _battleStartTime;

            var comboRecord = new ComboRecord
            {
                Time = relativeTime,
                ComboId = comboId,
                ComboName = comboName,
                SkillSequence = new List<string>(skillSequence),
                Damage = damage,
                Killed = killed
            };

            _currentReplay.Combos.Add(comboRecord);

            // 同时记录为一个动作
            RecordAction(relativeTime, PlayerActionType.ComboCompleted, comboId, "");
        }

        /// <summary>
        /// 记录玩家位置
        /// </summary>
        private void RecordPlayerPosition(float absoluteTime)
        {
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player == null)
                return;

            float relativeTime = absoluteTime - _battleStartTime;

            var action = new PlayerActionRecord
            {
                Time = relativeTime,
                Type = PlayerActionType.Movement,
                PlayerPosX = player.GlobalPosition.X,
                PlayerPosY = player.GlobalPosition.Y
            };

            _currentReplay.Actions.Add(action);
        }

        /// <summary>
        /// 记录玩家动作
        /// </summary>
        private void RecordAction(float relativeTime, PlayerActionType type, string skillId, string targetId, bool isDodge = false)
        {
            var player = GetTree().GetFirstNodeInGroup("Player");
            Vector2 playerPos = player?.GlobalPosition ?? Vector2.Zero;

            string actualSkillId = skillId;
            if (isDodge)
            {
                actualSkillId = "dodge";
            }

            var action = new PlayerActionRecord
            {
                Time = relativeTime,
                Type = type,
                SkillId = actualSkillId,
                TargetId = targetId,
                PlayerPosX = playerPos.X,
                PlayerPosY = playerPos.Y
            };

            _currentReplay.Actions.Add(action);
        }

        /// <summary>
        /// 获取当前录制的回放数据（如果有）
        /// </summary>
        public ComboReplayData GetCurrentReplay()
        {
            return _currentReplay;
        }

        /// <summary>
        /// 是否正在录制
        /// </summary>
        public bool IsRecording()
        {
            return _isRecording;
        }

        /// <summary>
        /// 强制结束录制（用于异常情况）
        /// </summary>
        public void ForceStopRecording()
        {
            _isRecording = false;
            _currentReplay = null;
            GD.Print("[ComboReplayRecorder] Recording force stopped");
        }
    }
}
