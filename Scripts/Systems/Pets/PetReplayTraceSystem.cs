using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物决策回放与追踪系统（REQ-137）
    /// 职责：记录、存储、查询、回放宠物 AI 决策
    /// 数据来源：PetDecisionSystem.UpdateDecision / PetTargetingSystem.SelectSmartTarget / PetBehaviorTree.ExecuteBehavior
    /// </summary>
    public class PetReplayTraceSystem : BaseSystem
    {
        private static PetReplayTraceSystem _instance;
        public static PetReplayTraceSystem Instance => _instance ??= new PetReplayTraceSystem();

        // 决策记录环形缓冲区
        private List<PetDecisionRecord> _recordBuffer = new List<PetDecisionRecord>();
        private int _maxRecords = 200;          // 最多保留 200 条记录
        private int _bufferStart = 0;           // 环形缓冲区起始索引
        private int _recordCount = 0;            // 实际记录数

        // 当前战斗的决策记录（每场战斗一个列表）
        private List<PetDecisionRecord> _currentBattleRecords = new List<PetDecisionRecord>();
        private float _battleStartTime = 0f;
        private bool _inBattle = false;

        // 回放状态
        private bool _isReplaying = false;
        private int _replayIndex = 0;
        private float _replaySpeed = 1.0f;
        private float _replayTime = 0f;

        // 信号
        public Action<PetDecisionRecord> OnDecisionRecorded;
        public Action<PetDecisionRecord> OnReplayStep;
        public Action OnReplayFinished;
        public Action<string> OnTraceQueryResult;  // 查询结果

        // 指标统计
        private int _totalDecisions = 0;
        private int _successCount = 0;
        private int _failureCount = 0;

        public override void _Ready()
        {
            _instance = this;

            // REQ-137: 订阅战斗事件，自动开始/结束记录
            if (EventBusManager.Instance != null)
            {
                EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
                EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
            }

            GD.Print("[PetReplayTraceSystem] Initialized with ring buffer capacity: ", _maxRecords);
        }

        private void OnCombatStarted()
        {
            StartBattle();
        }

        private void OnCombatEnded()
        {
            EndBattle();
        }

        /// <summary>
        /// 开始新战斗（重置当前战斗记录）
        /// </summary>
        public void StartBattle()
        {
            _currentBattleRecords.Clear();
            _battleStartTime = (float)Time.GetTicksMsec() / 1000f;
            _inBattle = true;
            PetDecisionSystem.ResetDecisionTick();
            GD.Print($"[PetReplayTraceSystem] Battle started at tick 0");
        }

        /// <summary>
        /// 结束战斗（保存记录并更新统计）
        /// </summary>
        public void EndBattle()
        {
            if (!_inBattle) return;
            _inBattle = false;

            float duration = (float)Time.GetTicksMsec() / 1000f - _battleStartTime;
            GD.Print($"[PetReplayTraceSystem] Battle ended. Duration: {duration:F1}s, Decisions: {_currentBattleRecords.Count}");
        }

        /// <summary>
        /// 记录决策（由三个决策入口调用）
        /// </summary>
        public void RecordDecision(PetDecisionRecord record)
        {
            if (record == null) return;

            // 添加到环形缓冲区（全局记录）
            AddToRingBuffer(record);

            // 添加到当前战斗记录
            if (_inBattle)
            {
                _currentBattleRecords.Add(record);
            }

            _totalDecisions++;

            // 更新统计
            switch (record.Outcome)
            {
                case PetDecisionRecord.DecisionOutcome.Success:
                    _successCount++;
                    break;
                case PetDecisionRecord.DecisionOutcome.Failure:
                    _failureCount++;
                    break;
            }

            OnDecisionRecorded?.Invoke(record);
        }

        /// <summary>
        /// 添加记录到环形缓冲区
        /// </summary>
        private void AddToRingBuffer(PetDecisionRecord record)
        {
            if (_recordBuffer.Count < _maxRecords)
            {
                _recordBuffer.Add(record);
            }
            else
            {
                // 环形替换：_bufferStart 指向最旧的记录
                _recordBuffer[_bufferStart] = record;
                _bufferStart = (_bufferStart + 1) % _maxRecords;
            }
        }

        /// <summary>
        /// 获取所有记录（按时间顺序）
        /// </summary>
        public List<PetDecisionRecord> GetAllRecords()
        {
            var result = new List<PetDecisionRecord>();

            if (_recordBuffer.Count == 0) return result;

            if (_recordBuffer.Count < _maxRecords)
            {
                // 未达到容量，直接返回
                for (int i = 0; i < _recordBuffer.Count; i++)
                {
                    if (_recordBuffer[i] != null)
                        result.Add(_recordBuffer[i]);
                }
            }
            else
            {
                // 环形缓冲区：从 bufferStart（最旧）到末尾，再从0到bufferStart（最新）
                for (int i = 0; i < _maxRecords; i++)
                {
                    int idx = (_bufferStart + i) % _maxRecords;
                    if (_recordBuffer[idx] != null)
                        result.Add(_recordBuffer[idx]);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取当前战斗的决策记录
        /// </summary>
        public List<PetDecisionRecord> GetCurrentBattleRecords()
        {
            return new List<PetDecisionRecord>(_currentBattleRecords);
        }

        /// <summary>
        /// 按 Tick ID 查询记录
        /// </summary>
        public PetDecisionRecord GetRecordByTick(int tickId)
        {
            var records = GetAllRecords();
            foreach (var record in records)
            {
                if (record.TickId == tickId)
                    return record;
            }
            return null;
        }

        /// <summary>
        /// 按类型查询记录
        /// </summary>
        public List<PetDecisionRecord> GetRecordsByType(PetDecisionRecord.DecisionType type)
        {
            var result = new List<PetDecisionRecord>();
            var records = GetAllRecords();
            foreach (var record in records)
            {
                if (record.Type == type)
                    result.Add(record);
            }
            return result;
        }

        /// <summary>
        /// 查询最近 N 条记录
        /// </summary>
        public List<PetDecisionRecord> GetRecentRecords(int count)
        {
            var all = GetAllRecords();
            int start = Math.Max(0, all.Count - count);
            var result = new List<PetDecisionRecord>();
            for (int i = start; i < all.Count; i++)
            {
                result.Add(all[i]);
            }
            return result;
        }

        /// <summary>
        /// 获取可读的决策链描述
        /// </summary>
        public string GetReadableTrace(int fromTick, int toTick)
        {
            var lines = new List<string>();
            lines.Add($"═══ 决策追踪 [Tick {fromTick} → {toTick}] ═══");

            var records = GetAllRecords();
            foreach (var record in records)
            {
                if (record.TickId >= fromTick && record.TickId <= toTick)
                {
                    lines.Add(record.ToReadableString());
                }
            }

            lines.Add("═══════════════════════════");
            return string.Join("\n", lines);
        }

        /// <summary>
        /// 开始回放（从第一条记录开始）
        /// </summary>
        public void StartReplay(float speed = 1.0f)
        {
            if (_currentBattleRecords.Count == 0)
            {
                GD.Print("[PetReplayTraceSystem] No records to replay");
                return;
            }

            _isReplaying = true;
            _replayIndex = 0;
            _replaySpeed = speed;
            _replayTime = 0f;
            GD.Print($"[PetReplayTraceSystem] Replay started: {_currentBattleRecords.Count} decisions");
        }

        /// <summary>
        /// 停止回放
        /// </summary>
        public void StopReplay()
        {
            _isReplaying = false;
            _replayIndex = 0;
            GD.Print("[PetReplayTraceSystem] Replay stopped");
        }

        /// <summary>
        /// 更新回放状态（每帧调用）
        /// </summary>
        public void UpdateReplay(float delta)
        {
            if (!_isReplaying) return;
            if (_currentBattleRecords.Count == 0) return;

            _replayTime += delta * _replaySpeed;

            // 每条记录间隔 0.5 秒
            float interval = 0.5f;
            while (_replayIndex < _currentBattleRecords.Count && _replayTime >= interval * (_replayIndex + 1))
            {
                var record = _currentBattleRecords[_replayIndex];
                OnReplayStep?.Invoke(record);

                _replayIndex++;
                if (_replayIndex >= _currentBattleRecords.Count)
                {
                    _isReplaying = false;
                    OnReplayFinished?.Invoke();
                    GD.Print("[PetReplayTraceSystem] Replay finished");
                }
            }
        }

        /// <summary>
        /// 获取回放进度
        /// </summary>
        public (int current, int total) GetReplayProgress()
        {
            return (_replayIndex, _currentBattleRecords.Count);
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public (int total, int success, int failure, float successRate) GetStatistics()
        {
            int total = _totalDecisions;
            int success = _successCount;
            int failure = _failureCount;
            float rate = total > 0 ? (float)success / total : 0f;
            return (total, success, failure, rate);
        }

        /// <summary>
        /// 导出当前战斗记录（用于叙事系统或调试）
        /// </summary>
        public List<Dictionary> ExportBattleRecords()
        {
            var result = new List<Dictionary>();
            foreach (var record in _currentBattleRecords)
            {
                result.Add(record.ToDictionary());
            }
            return result;
        }

        /// <summary>
        /// 清除所有记录
        /// </summary>
        public void ClearAllRecords()
        {
            _recordBuffer.Clear();
            _currentBattleRecords.Clear();
            _bufferStart = 0;
            _recordCount = 0;
            _totalDecisions = 0;
            _successCount = 0;
            _failureCount = 0;
            GD.Print("[PetReplayTraceSystem] All records cleared");
        }

        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "totalDecisions", _totalDecisions },
                { "successCount", _successCount },
                { "failureCount", _failureCount }
            };
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            if (data.Contains("totalDecisions"))
                _totalDecisions = Convert.ToInt32(data["totalDecisions"]);
            if (data.Contains("successCount"))
                _successCount = Convert.ToInt32(data["successCount"]);
            if (data.Contains("failureCount"))
                _failureCount = Convert.ToInt32(data["failureCount"]);
        }
    }
}
