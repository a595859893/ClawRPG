using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetBattleMemory
{
    /// <summary>
    /// 单条宠物战斗记忆条目（REQ-190）
    /// 记录宠物观察到的玩家每场战斗的第一个技能/combo
    /// </summary>
    public class PetBattleMemoryEntry
    {
        public string PetId { get; set; }
        public string FirstSkillUsed { get; set; }      // 该场战斗玩家第一步使用的技能
        public string AssociatedComboId { get; set; }   // 推测的 combo ID（模糊匹配）
        public int TimesObserved { get; set; }         // 观察到该 pattern 的次数
        public long LastObservedTicks { get; set; }     // 上次观察到的时间（DateTime ticks）

        public DateTime LastObserved => new DateTime(LastObservedTicks);

        public PetBattleMemoryEntry() { }

        public PetBattleMemoryEntry(string petId, string firstSkillUsed, string associatedComboId)
        {
            PetId = petId;
            FirstSkillUsed = firstSkillUsed;
            AssociatedComboId = associatedComboId;
            TimesObserved = 1;
            LastObservedTicks = DateTime.Now.Ticks;
        }
    }

    /// <summary>
    /// 宠物战斗记忆数据持久化结构（REQ-190）
    /// </summary>
    public class PetBattleMemorySaveData
    {
        public List<PetBattleMemoryEntry> Entries { get; set; } = new List<PetBattleMemoryEntry>();
        public Dictionary<string, string> ReincarnatedMemoryMap { get; set; } = new Dictionary<string, string>();
        // key: newPetId, value: inheritedFromPetId
    }

    /// <summary>
    /// 宠物战斗记忆数据库（REQ-190）
    /// 静态库，按 petId 索引记忆条目
    /// </summary>
    public static class PetBattleMemoryDatabase
    {
        // 内存中按 petId 索引的记忆条目（最多 MAX_ENTRIES_PER_PET 条，FIFO 淘汰）
        private static Dictionary<string, List<PetBattleMemoryEntry>> _memories = new Dictionary<string, List<PetBattleMemoryEntry>>();
        private const int MAX_ENTRIES_PER_PET = 50;

        // 重生继承映射（宠物名→原宠物ID，用于跨局次继承）
        private static Dictionary<string, string> _reincarnatedMap = new Dictionary<string, string>();

        /// <summary>
        /// 记录玩家在某场战斗的第一步技能
        /// </summary>
        public static void RecordFirstSkillUsed(string petId, string skillId, string comboId)
        {
            if (string.IsNullOrEmpty(petId) || string.IsNullOrEmpty(skillId))
                return;

            if (!_memories.ContainsKey(petId))
                _memories[petId] = new List<PetBattleMemoryEntry>();

            var entries = _memories[petId];

            // 查找是否有相同 FirstSkillUsed 的现有条目，更新 TimesObserved
            var existing = entries.Find(e => e.FirstSkillUsed == skillId);
            if (existing != null)
            {
                existing.TimesObserved++;
                existing.LastObservedTicks = DateTime.Now.Ticks;
                if (!string.IsNullOrEmpty(comboId))
                    existing.AssociatedComboId = comboId;
            }
            else
            {
                entries.Add(new PetBattleMemoryEntry(petId, skillId, comboId ?? ""));
                // FIFO 淘汰最老的条目
                if (entries.Count > MAX_ENTRIES_PER_PET)
                    entries.RemoveAt(0);
            }
        }

        /// <summary>
        /// 获取宠物最常用的起手技能（引导时使用）
        /// </summary>
        public static PetBattleMemoryEntry GetMostFrequentFirstSkill(string petId)
        {
            if (!_memories.ContainsKey(petId) || _memories[petId].Count == 0)
                return null;

            PetBattleMemoryEntry best = null;
            int max = 0;
            foreach (var entry in _memories[petId])
            {
                if (entry.TimesObserved > max)
                {
                    max = entry.TimesObserved;
                    best = entry;
                }
            }
            return best;
        }

        /// <summary>
        /// 获取指定宠物的所有记忆条目
        /// </summary>
        public static List<PetBattleMemoryEntry> GetMemoriesForPet(string petId)
        {
            if (!_memories.ContainsKey(petId))
                return new List<PetBattleMemoryEntry>();
            return new List<PetBattleMemoryEntry>(_memories[petId]);
        }

        /// <summary>
        /// 检查是否有可引导的记忆
        /// </summary>
        public static bool HasGuidableMemory(string petId)
        {
            return GetMostFrequentFirstSkill(petId) != null;
        }

        /// <summary>
        /// 注册重生继承关系（同名宠物继承记忆）
        /// </summary>
        public static void RegisterReincarnation(string newPetId, string inheritedFromPetId)
        {
            if (string.IsNullOrEmpty(newPetId) || string.IsNullOrEmpty(inheritedFromPetId))
                return;
            _reincarnatedMap[newPetId] = inheritedFromPetId;

            // 执行记忆继承
            if (_memories.ContainsKey(inheritedFromPetId))
            {
                if (!_memories.ContainsKey(newPetId))
                    _memories[newPetId] = new List<PetBattleMemoryEntry>();

                foreach (var entry in _memories[inheritedFromPetId])
                {
                    var inherited = new PetBattleMemoryEntry(newPetId, entry.FirstSkillUsed, entry.AssociatedComboId);
                    inherited.TimesObserved = Math.Max(1, entry.TimesObserved / 2); // 减半继承
                    inherited.LastObservedTicks = entry.LastObservedTicks;
                    _memories[newPetId].Add(inherited);
                }
            }
        }

        /// <summary>
        /// 获取重生继承的原宠物 ID
        /// </summary>
        public static string GetReincarnatedFrom(string petId)
        {
            return _reincarnatedMap.ContainsKey(petId) ? _reincarnatedMap[petId] : null;
        }

        /// <summary>
        /// 持久化导出
        /// </summary>
        public static PetBattleMemorySaveData ExportSaveData()
        {
            var data = new PetBattleMemorySaveData();
            foreach (var kvp in _memories)
            {
                data.Entries.AddRange(kvp.Value);
            }
            data.ReincarnatedMemoryMap = new Dictionary<string, string>(_reincarnatedMap);
            return data;
        }

        /// <summary>
        /// 持久化导入
        /// </summary>
        public static void ImportSaveData(PetBattleMemorySaveData data)
        {
            if (data == null) return;
            _memories.Clear();
            _reincarnatedMap.Clear();

            foreach (var entry in data.Entries)
            {
                if (!_memories.ContainsKey(entry.PetId))
                    _memories[entry.PetId] = new List<PetBattleMemoryEntry>();
                _memories[entry.PetId].Add(entry);
            }

            if (data.ReincarnatedMemoryMap != null)
                _reincarnatedMap = new Dictionary<string, string>(data.ReincarnatedMemoryMap);
        }
    }
}
