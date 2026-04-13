using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetDeparture
{
    /// <summary>
    /// Pet Action Stats Database — tracks per-pet skill usage to find "most used skill"
    /// REQ-189: Needed to populate the departure profile card's "最常用技能" field
    /// </summary>
    public class PetActionStatsDatabase
    {
        // petId → skillId → count
        private Dictionary<string, Dictionary<string, int>> _skillUsage = new Dictionary<string, Dictionary<string, int>>();
        // petId → total battles
        private Dictionary<string, int> _battleCount = new Dictionary<string, int>();

        /// <summary>
        /// Record that a pet performed a skill in a battle
        /// </summary>
        public void RecordSkillUsage(string petId, string skillId)
        {
            if (!_skillUsage.ContainsKey(petId))
                _skillUsage[petId] = new Dictionary<string, int>();

            if (!_skillUsage[petId].ContainsKey(skillId))
                _skillUsage[petId][skillId] = 0;

            _skillUsage[petId][skillId]++;
        }

        /// <summary>
        /// Record that a pet participated in a battle
        /// </summary>
        public void RecordBattle(string petId)
        {
            if (!_battleCount.ContainsKey(petId))
                _battleCount[petId] = 0;
            _battleCount[petId]++;
        }

        /// <summary>
        /// Get the most-used skill id for a pet, or empty string if none
        /// </summary>
        public string GetMostUsedSkill(string petId)
        {
            if (!_skillUsage.TryGetValue(petId, out var skills) || skills.Count == 0)
                return "";

            string bestSkill = "";
            int bestCount = 0;
            foreach (var kvp in skills)
            {
                if (kvp.Value > bestCount)
                {
                    bestCount = kvp.Value;
                    bestSkill = kvp.Key;
                }
            }
            return bestSkill;
        }

        /// <summary>
        /// Get total battle count for a pet
        /// </summary>
        public int GetBattleCount(string petId)
        {
            return _battleCount.TryGetValue(petId, out var count) ? count : 0;
        }

        /// <summary>
        /// Get all skill usage for a pet (skillId → count)
        /// </summary>
        public Dictionary<string, int> GetSkillUsage(string petId)
        {
            return _skillUsage.TryGetValue(petId, out var skills) ? new Dictionary<string, int>(skills) : new Dictionary<string, int>();
        }

        #region Persistence

        public Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            var skillData = new Dictionary<string, Dictionary<string, int>>();
            foreach (var petKvp in _skillUsage)
            {
                skillData[petKvp.Key] = new Dictionary<string, int>(petKvp.Value);
            }
            data["skill_usage"] = skillData;
            data["battle_count"] = new Dictionary<string, int>(_battleCount);
            return data;
        }

        public void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.TryGetValue("skill_usage", out var skillObj) && skillObj is Dictionary<string, Dictionary<string, int>> skillData)
            {
                _skillUsage = new Dictionary<string, Dictionary<string, int>>(skillData);
            }

            if (data.TryGetValue("battle_count", out var battleObj) && battleObj is Dictionary<string, int> battleData)
            {
                _battleCount = new Dictionary<string, int>(battleData);
            }
        }

        #endregion
    }
}
