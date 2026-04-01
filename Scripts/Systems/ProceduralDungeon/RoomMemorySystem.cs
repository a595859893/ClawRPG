using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 战斗房间LSTM记忆系统
    ///
    /// 每个房间类型是一个记忆单元，连续进入相同类型房间会增强该类型的"记忆权重"。
    /// 记忆越强，该类型在房间生成时被选择的概率越低（增加新鲜感）。
    /// 长时间不进某类型房间，权重逐渐衰减（遗忘曲线）。
    ///
    /// 集成点：
    /// - RoomLayoutSystem.SelectRoomType() — 房间类型选择受记忆影响
    /// - DungeonGeneratorSystem.GenerateDungeon() — 新地下城开始时可选择重置记忆
    /// </summary>
    public partial class RoomMemorySystem : BaseSystem
    {
        private static RoomMemorySystem _instance;
        public static RoomMemorySystem Instance => _instance;

        /// <summary>记忆数据库</summary>
        private RoomMemoryDatabase _database;

        /// <summary>上次遗忘检查的游戏内时间（分钟）</summary>
        private float _lastForgetCheckMinutes;

        // 信号
        [Signal]
        public delegate void RoomMemoryUpdatedEventHandler(RoomType roomType, int newWeight);

        [Signal]
        public delegate void RoomMemoryDecayedEventHandler(RoomType roomType, int newWeight);

        public override void _Ready()
        {
            base._Ready();

            // 订阅房间进入信号
            if (ProceduralDungeonSystem.Instance != null)
            {
                ProceduralDungeonSystem.RoomEntered += OnProceduralRoomEntered;
            }

            GD.Print("[RoomMemory] System initialized, subscribed to RoomEntered signal");
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            if (ProceduralDungeonSystem.Instance != null)
            {
                ProceduralDungeonSystem.RoomEntered -= OnProceduralRoomEntered;
            }
        }

        /// <summary>
        /// 当玩家进入房间时自动记录记忆（由信号触发）
        /// </summary>
        private void OnProceduralRoomEntered()
        {
            if (ProceduralDungeonSystem.Instance?.CurrentDungeon?.CurrentRoom == null) return;

            var roomType = ProceduralDungeonSystem.Instance.CurrentDungeon.CurrentRoom.Type;
            // 估算游戏内时间（简单累加，每次进入 +5分钟）
            float estimatedMinutes = _lastForgetCheckMinutes + 5f;
            RecordRoomEntry(roomType, estimatedMinutes);
            ApplyForgetting(estimatedMinutes);
        }

        public RoomMemorySystem()
        {
            _instance = this;
            _database = new RoomMemoryDatabase();
            _lastForgetCheckMinutes = 0f;
        }

        /// <summary>
        /// 记录玩家进入某类型房间 — 调用此方法更新记忆
        /// </summary>
        /// <param name="roomType">房间类型</param>
        /// <param name="currentGameMinutes">当前游戏内时间（分钟）</param>
        public void RecordRoomEntry(RoomType roomType, float currentGameMinutes = 0f)
        {
            if (roomType == RoomType.Entrance || roomType == RoomType.Corridor || roomType == RoomType.Boss)
                return; // 入口/走廊/Boss 不参与记忆

            var entry = _database.GetEntry(roomType);

            // 增加权重（有上限）
            entry.Weight = Mathf.Min(entry.Weight + RoomMemoryConstants.BOOST_AMOUNT, RoomMemoryConstants.MAX_WEIGHT);
            entry.LastEntryMinutes = currentGameMinutes;
            entry.EntryCount++;

            _database.SetEntry(roomType, entry);

            GD.Print($"[RoomMemory] {roomType} entered: weight={entry.Weight}, total_entries={entry.EntryCount}");

            EmitSignal(SignalName.RoomMemoryUpdated, (int)roomType, entry.Weight);
        }

        /// <summary>
        /// 获取指定房间类型的当前记忆权重
        /// </summary>
        public int GetMemoryWeight(RoomType roomType)
        {
            return _database.GetEntry(roomType).Weight;
        }

        /// <summary>
        /// 获取所有房间类型的记忆权重
        /// </summary>
        public Dictionary<RoomType, int> GetAllMemoryWeights()
        {
            var result = new Dictionary<RoomType, int>();
            foreach (var rt in _database.GetAllEntries().Keys)
            {
                result[rt] = _database.GetEntry(rt).Weight;
            }
            return result;
        }

        /// <summary>
        /// 获取所有记忆条目（含进入次数）
        /// </summary>
        public Dictionary<RoomType, RoomMemoryEntry> GetAllMemoryEntries()
        {
            return _database.GetAllEntries();
        }

        /// <summary>
        /// 根据记忆状态调整房间选择权重
        /// 高记忆类型权重降低（减少重复），低/无记忆类型权重升高（增加新鲜感）
        /// </summary>
        /// <param name="baseWeights">原始权重字典</param>
        /// <returns>调整后的权重字典</returns>
        public Dictionary<RoomType, int> GetAdjustedWeights(Dictionary<RoomType, int> baseWeights)
        {
            var adjusted = new Dictionary<RoomType, int>();

            foreach (var kvp in baseWeights)
            {
                if (kvp.Key == RoomType.Entrance || kvp.Key == RoomType.Corridor || kvp.Key == RoomType.Boss)
                {
                    // Boss/入口/走廊不受记忆影响
                    adjusted[kvp.Key] = kvp.Value;
                }
                else
                {
                    int memoryWeight = GetMemoryWeight(kvp.Key);
                    adjusted[kvp.Key] = RoomMemoryConstants.AdjustWeightByMemory(kvp.Value, memoryWeight);
                }
            }

            return adjusted;
        }

        /// <summary>
        /// 应用遗忘机制 — 检查所有房间类型，对长期未进入的进行衰减
        /// </summary>
        /// <param name="currentGameMinutes">当前游戏内时间（分钟）</param>
        public void ApplyForgetting(float currentGameMinutes)
        {
            if (currentGameMinutes - _lastForgetCheckMinutes < RoomMemoryConstants.FORGET_CHECK_INTERVAL)
                return;

            _lastForgetCheckMinutes = currentGameMinutes;

            bool anyDecayed = false;
            foreach (var rt in _database.GetAllEntries().Keys)
            {
                var entry = _database.GetEntry(rt);
                if (entry.Weight <= 0) continue;

                float elapsed = currentGameMinutes - entry.LastEntryMinutes;
                if (elapsed >= RoomMemoryConstants.FORGET_THRESHOLD_MINUTES)
                {
                    // 计算应该衰减多少
                    int decaySteps = Mathf.FloorToInt(elapsed / RoomMemoryConstants.FORGET_THRESHOLD_MINUTES);
                    int newWeight = Mathf.Max(0, entry.Weight - (decaySteps * RoomMemoryConstants.DECAY_AMOUNT));

                    if (newWeight < entry.Weight)
                    {
                        entry.Weight = newWeight;
                        _database.SetEntry(rt, entry);
                        anyDecayed = true;

                        GD.Print($"[RoomMemory] {rt} forgot: weight={newWeight} (elapsed={elapsed:F1}min)");

                        EmitSignal(SignalName.RoomMemoryDecayed, (int)rt, newWeight);
                    }
                }
            }

            if (anyDecayed)
            {
                EmitSignal(SignalName.RoomMemoryUpdated, (int)RoomType.Combat, 0); // 触发UI更新
            }
        }

        /// <summary>
        /// 重置信任系统（开始新游戏时调用）
        /// </summary>
        public void ResetMemory()
        {
            _database = new RoomMemoryDatabase();
            _lastForgetCheckMinutes = 0f;
            GD.Print("[RoomMemory] Memory reset for new game");
        }

        /// <summary>
        /// 获取某房间类型距离遗忘还有多少分钟
        /// </summary>
        public float GetMinutesUntilForget(RoomType roomType, float currentGameMinutes)
        {
            var entry = _database.GetEntry(roomType);
            if (entry.Weight <= 0) return 0f;

            float elapsed = currentGameMinutes - entry.LastEntryMinutes;
            return Mathf.Max(0f, RoomMemoryConstants.FORGET_THRESHOLD_MINUTES - elapsed);
        }

        /// <summary>
        /// 获取记忆状态的简要描述（用于调试UI）
        /// </summary>
        public string GetMemorySummary()
        {
            var lines = new List<string> { "[Room Memory Summary]" };
            foreach (var rt in _database.GetAllEntries().Keys)
            {
                var entry = _database.GetEntry(rt);
                lines.Add($"  {rt}: weight={entry.Weight}, entries={entry.EntryCount}");
            }
            return string.Join("\n", lines);
        }

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = _database.ToSerializable();
            data["_lastForgetCheckMinutes"] = _lastForgetCheckMinutes;
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0) return;

            _lastForgetCheckMinutes = data.ContainsKey("_lastForgetCheckMinutes")
                ? Convert.ToSingle(data["_lastForgetCheckMinutes"]) : 0f;

            // 移除元数据，只留房间记忆
            var memoryData = new Dictionary<string, object>(data);
            memoryData.Remove("_lastForgetCheckMinutes");

            _database = new RoomMemoryDatabase();
            _database.FromSerializable(memoryData);

            GD.Print($"[RoomMemory] Restored memory state: {_database.GetAllEntries().Count} room types tracked");
        }

        #endregion
    }
}
