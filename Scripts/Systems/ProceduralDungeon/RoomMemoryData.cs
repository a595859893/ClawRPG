using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 房间记忆条目 — 追踪每个房间类型的记忆强度
    /// </summary>
    [Serializable]
    public struct RoomMemoryEntry
    {
        /// <summary>当前记忆权重 (0-MAX_WEIGHT)</summary>
        public int Weight;

        /// <summary>上次进入该类型房间的分钟数 (游戏内时间)</summary>
        public float LastEntryMinutes;

        /// <summary>累计进入次数</summary>
        public int EntryCount;

        public RoomMemoryEntry(int weight = 0, float lastEntryMinutes = 0f, int entryCount = 0)
        {
            Weight = weight;
            LastEntryMinutes = lastEntryMinutes;
            EntryCount = entryCount;
        }

        public override string ToString() => $"RoomMemoryEntry(weight={Weight}, lastEntry={LastEntryMinutes:F1}min, entries={EntryCount})";
    }

    /// <summary>
    /// 房间记忆数据库 — 存储所有房间类型的记忆状态
    /// </summary>
    [Serializable]
    public class RoomMemoryDatabase
    {
        /// <summary>每个房间类型的记忆条目</summary>
        private Dictionary<RoomType, RoomMemoryEntry> _memory = new();

        public RoomMemoryDatabase()
        {
            // 初始化所有房间类型为默认条目
            foreach (RoomType rt in Enum.GetValues(typeof(RoomType)))
            {
                if (rt != RoomType.Entrance && rt != RoomType.Corridor && rt != RoomType.Boss)
                {
                    _memory[rt] = new RoomMemoryEntry();
                }
            }
        }

        /// <summary>
        /// 获取指定房间类型的记忆条目
        /// </summary>
        public RoomMemoryEntry GetEntry(RoomType roomType)
        {
            if (_memory.TryGetValue(roomType, out var entry))
                return entry;
            return new RoomMemoryEntry();
        }

        /// <summary>
        /// 更新指定房间类型的记忆条目
        /// </summary>
        public void SetEntry(RoomType roomType, RoomMemoryEntry entry)
        {
            _memory[roomType] = entry;
        }

        /// <summary>
        /// 获取所有记忆条目
        /// </summary>
        public Dictionary<RoomType, RoomMemoryEntry> GetAllEntries() => new(_memory);

        /// <summary>
        /// 导出为可序列化字典
        /// </summary>
        public Dictionary<string, object> ToSerializable()
        {
            var result = new Dictionary<string, object>();
            foreach (var kvp in _memory)
            {
                result[kvp.Key.ToString()] = new Dictionary<string, object>
                {
                    ["weight"] = kvp.Value.Weight,
                    ["lastEntryMinutes"] = kvp.Value.LastEntryMinutes,
                    ["entryCount"] = kvp.Value.EntryCount
                };
            }
            return result;
        }

        /// <summary>
        /// 从序列化数据恢复
        /// </summary>
        public void FromSerializable(Dictionary<string, object> data)
        {
            foreach (var kvp in data)
            {
                if (Enum.TryParse<RoomType>(kvp.Key, out var rt) && kvp.Value is Dictionary<string, object> entry)
                {
                    _memory[rt] = new RoomMemoryEntry(
                        weight: entry.ContainsKey("weight") ? Convert.ToInt32(entry["weight"]) : 0,
                        lastEntryMinutes: entry.ContainsKey("lastEntryMinutes") ? Convert.ToSingle(entry["lastEntryMinutes"]) : 0f,
                        entryCount: entry.ContainsKey("entryCount") ? Convert.ToInt32(entry["entryCount"]) : 0
                    );
                }
            }
        }
    }

    /// <summary>
    /// 房间记忆系统常量
    /// </summary>
    public static class RoomMemoryConstants
    {
        /// <summary>记忆权重上限</summary>
        public const int MAX_WEIGHT = 10;

        /// <summary>遗忘阈值（分钟）：不进该类型房间此后开始遗忘</summary>
        public const float FORGET_THRESHOLD_MINUTES = 30f;

        /// <summary>每次遗忘的权重减少量</summary>
        public const int DECAY_AMOUNT = 1;

        /// <summary>每次进入的权重增加量</summary>
        public const int BOOST_AMOUNT = 1;

        /// <summary>遗忘检查间隔（分钟）</summary>
        public const float FORGET_CHECK_INTERVAL = 1f;

        /// <summary>
        /// 根据记忆权重调整房间类型选择概率
        /// 高记忆权重 → 降低选择概率（玩家已熟悉）
        /// 低记忆权重 → 提高选择概率（增加新鲜感）
        /// </summary>
        /// <param name="baseWeight">基础权重</param>
        /// <param name="memoryWeight">当前记忆权重 (0-MAX_WEIGHT)</param>
        /// <returns>调整后的权重</returns>
        public static int AdjustWeightByMemory(int baseWeight, int memoryWeight)
        {
            // 记忆越强，选择概率越低（避免刷同一类型）
            // 记忆为0时恢复正常权重，记忆为MAX时降低约50%概率
            float factor = 1f - (memoryWeight / (float)(MAX_WEIGHT * 2));
            factor = Mathf.Clamp(factor, 0.1f, 1f);
            return Mathf.Max(1, Mathf.RoundToInt(baseWeight * factor));
        }
    }
}
