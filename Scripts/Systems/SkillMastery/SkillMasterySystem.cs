using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Skill Mastery System - Manages skill mastery progression
    /// </summary>
    public class SkillMasterySystem : BaseSystem
    {
        private static SkillMasterySystem _instance;
        public static SkillMasterySystem Instance => _instance ?? new SkillMasterySystem();

        public SignalWrapper OnMasteryUpdated { get; } = new SignalWrapper();
        public SignalWrapper OnTierChanged { get; } = new SignalWrapper();
        public SignalWrapper OnBonusUnlocked { get; } = new SignalWrapper();

        private SkillMasteryData.PlayerSkillMasteryData _playerData;
        private SkillMasteryDatabase _database;

        // Configuration
        private const int POINTS_PER_USE = 1;
        private const int BONUS_POINTS_MULTIPLIER = 2; // Bonus points for critical hits/kills

        public SkillMasterySystem()
        {
            _instance = this;
            _database = new SkillMasteryDatabase();
            _playerData = new SkillMasteryData.PlayerSkillMasteryData();
        }

        protected override void Initialize()
        {
            base.Initialize();
            LoadData();
        }

        public override Dictionary ExportSaveData()
        {
            var data = new Godot.Dictionary();
            
            // 保存技能熟练度数据
            var skillsData = new Godot.Dictionary();
            foreach (var kvp in _playerData.Skills)
            {
                var skillData = new Godot.Dictionary();
                skillData["current_points"] = kvp.Value.CurrentPoints;
                skillData["total_points"] = kvp.Value.TotalPoints;
                skillData["tier"] = kvp.Value.CurrentTier;
                var bonuses = new Godot.Array();
                foreach (int bonusId in kvp.Value.UnlockedBonusIds)
                {
                    bonuses.Add(bonusId);
                }
                skillData["unlocked_bonuses"] = bonuses;
                skillsData[kvp.Key] = skillData;
            }
            data["skills"] = skillsData;
            
            // 保存全局数据
            data["total_mastery_points"] = _playerData.TotalMasteryPoints;
            data["total_uses"] = _playerData.TotalUses;
            data["critical_uses"] = _playerData.CriticalUses;
            
            GD.Print($"[SkillMasterySystem] Saving {_playerData.Skills.Count} skills, {_playerData.TotalMasteryPoints} total points");
            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 加载技能熟练度数据
            if (data.Contains("skills"))
            {
                _playerData.Skills.Clear();
                var skillsData = (Godot.Dictionary)data["skills"];
                foreach (string skillId in skillsData.Keys)
                {
                    var skillData = (Godot.Dictionary)skillsData[skillId];
                    var masteryData = new SkillMasteryData.SkillMasteryEntry
                    {
                        CurrentPoints = (int)skillData["current_points"],
                        TotalPoints = (int)skillData["total_points"],
                        CurrentTier = (int)skillData["tier"]
                    };
                    var bonuses = (Godot.Array)skillData["unlocked_bonuses"];
                    foreach (int bonusId in bonuses)
                    {
                        masteryData.UnlockedBonusIds.Add(bonusId);
                    }
                    _playerData.Skills[skillId] = masteryData;
                }
            }
            
            // 加载全局数据
            if (data.Contains("total_mastery_points"))
                _playerData.TotalMasteryPoints = (int)data["total_mastery_points"];
            if (data.Contains("total_uses"))
                _playerData.TotalUses = (int)data["total_uses"];
            if (data.Contains("critical_uses"))
                _playerData.CriticalUses = (int)data["critical_uses"];
            
            GD.Print($"[SkillMasterySystem] Loaded {_playerData.Skills.Count} skills, {_playerData.TotalMasteryPoints} total points");
        }

        /// <summary>
        /// Record skill usage and grant mastery points
        /// </summary>
        public void RecordSkillUse(string skillId, string skillName, SkillMasteryData.SkillType type, bool isCriticalHit = false, bool killedEnemy = false)
        {
            int pointsGained = POINTS_PER_USE;

            // Bonus points for special events
            if (isCriticalHit) pointsGained += BONUS_POINTS_MULTIPLIER;
            if (killedEnemy) pointsGained += BONUS_POINTS_MULTIPLIER;

            // Get or create skill mastery
            if (!_playerData.Skills.ContainsKey(skillId))
            {
                _playerData.Skills[skillId] = new SkillMasteryData.SkillMastery
                {
                    SkillId = skillId,
                    SkillName = skillName,
                    Type = type,
                    TotalUses = 0,
                    MasteryPoints = 0,
                    Tier = SkillMasteryData.MasteryTier.Novice
                };
            }

            var mastery = _playerData.Skills[skillId];
            var oldTier = mastery.Tier;

            // Update mastery
            mastery.TotalUses++;
            mastery.MasteryPoints += pointsGained;
            mastery.LastUsed = DateTime.Now;

            // Check for tier upgrade
            mastery.Tier = _database.GetTierForPoints(mastery.MasteryPoints);

            // Update global stats
            _playerData.TotalMasteryPoints += pointsGained;

            if (mastery.Tier != oldTier)
            {
                _playerData.HighestTierCount++;
                _playerData.LastMastery = DateTime.Now;
                if (_playerData.FirstMastery == default(DateTime))
                    _playerData.FirstMastery = DateTime.Now;

                OnTierChanged.Emit(skillId, mastery.Tier.ToString());
            }

            // Check for new bonuses
            CheckAndUnlockBonuses(mastery);

            // Emit update signal
            OnMasteryUpdated.Emit(skillId, mastery.MasteryPoints, mastery.Tier.ToString());

            SaveData();
        }

        /// <summary>
        /// Check and unlock available bonuses for a skill
        /// </summary>
        private void CheckAndUnlockBonuses(SkillMasteryData.SkillMastery mastery)
        {
            var availableBonuses = _database.GetAvailableBonuses(
                mastery.Type, 
                mastery.MasteryPoints, 
                mastery.Tier
            );

            foreach (var bonus in availableBonuses)
            {
                if (!mastery.UnlockedBonuses.Contains(bonus.BonusId))
                {
                    mastery.UnlockedBonuses.Add(bonus.BonusId);
                    OnBonusUnlocked.Emit(mastery.SkillId, bonus.BonusId, bonus.Name);
                    GD.Print($"[SkillMasterySystem] Bonus unlocked: {bonus.Name} for skill {mastery.SkillName}");
                }
            }
        }

        /// <summary>
        /// Get mastery data for a specific skill
        /// </summary>
        public SkillMasteryData.SkillMastery GetSkillMastery(string skillId)
        {
            return _playerData.Skills.ContainsKey(skillId) ? _playerData.Skills[skillId] : null;
        }

        /// <summary>
        /// Get damage bonus based on skill mastery tier
        /// </summary>
        public float GetDamageBonus(string skillId)
        {
            var mastery = GetSkillMastery(skillId);
            if (mastery == null) return 0f;

            var tierInfo = _database.GetTierInfo(mastery.Tier);
            return tierInfo.DamageBonus;
        }

        /// <summary>
        /// Get cooldown reduction based on skill mastery tier
        /// </summary>
        public float GetCooldownReduction(string skillId)
        {
            var mastery = GetSkillMastery(skillId);
            if (mastery == null) return 0f;

            var tierInfo = _database.GetTierInfo(mastery.Tier);
            return tierInfo.CooldownReduction;
        }

        /// <summary>
        /// Get mana cost reduction based on skill mastery tier
        /// </summary>
        public float GetManaCostReduction(string skillId)
        {
            var mastery = GetSkillMastery(skillId);
            if (mastery == null) return 0f;

            var tierInfo = _database.GetTierInfo(mastery.Tier);
            return tierInfo.ManaCostReduction;
        }

        /// <summary>
        /// Get all unlocked bonuses for a skill
        /// </summary>
        public List<string> GetUnlockedBonuses(string skillId)
        {
            var mastery = GetSkillMastery(skillId);
            return mastery?.UnlockedBonuses ?? new List<string>();
        }

        /// <summary>
        /// Get statistics for display
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "totalSkills", _playerData.Skills.Count },
                { "totalMasteryPoints", _playerData.TotalMasteryPoints },
                { "highestTierCount", _playerData.HighestTierCount },
                { "grandMasterCount", CountTier(SkillMasteryData.MasteryTier.GrandMaster) },
                { "masterCount", CountTier(SkillMasteryData.MasteryTier.Master) },
                { "expertCount", CountTier(SkillMasteryData.MasteryTier.Expert) },
                { "journeymanCount", CountTier(SkillMasteryData.MasteryTier.Journeyman) },
                { "apprenticeCount", CountTier(SkillMasteryData.MasteryTier.Apprentice) },
                { "totalSkillUses", GetTotalSkillUses() }
            };
        }

        private int CountTier(SkillMasteryData.MasteryTier tier)
        {
            int count = 0;
            foreach (var skill in _playerData.Skills.Values)
            {
                if (skill.Tier == tier) count++;
            }
            return count;
        }

        private int GetTotalSkillUses()
        {
            int total = 0;
            foreach (var skill in _playerData.Skills.Values)
            {
                total += skill.TotalUses;
            }
            return total;
        }

        /// <summary>
        /// Get all skills mastery data for UI display
        /// </summary>
        public List<Dictionary<string, object>> GetAllSkillsMastery()
        {
            var result = new List<Dictionary<string, object>>();

            foreach (var skill in _playerData.Skills.Values)
            {
                var tierInfo = _database.GetTierInfo(skill.Tier);
                var nextTier = GetNextTier(skill.Tier);

                result.Add(new Dictionary<string, object>
                {
                    { "skillId", skill.SkillId },
                    { "skillName", skill.SkillName },
                    { "type", skill.Type.ToString() },
                    { "totalUses", skill.TotalUses },
                    { "masteryPoints", skill.MasteryPoints },
                    { "tier", skill.Tier.ToString() },
                    { "tierDisplayName", tierInfo.DisplayName },
                    { "damageBonus", tierInfo.DamageBonus },
                    { "cooldownReduction", tierInfo.CooldownReduction },
                    { "manaCostReduction", tierInfo.ManaCostReduction },
                    { "unlockedBonuses", skill.UnlockedBonuses.Count },
                    { "nextTierPoints", nextTier != null ? nextTier.MinPoints : skill.MasteryPoints },
                    { "progressToNextTier", GetProgressToNextTier(skill) }
                });
            }

            return result;
        }

        private SkillMasteryData.MasteryTierInfo GetNextTier(SkillMasteryData.MasteryTier currentTier)
        {
            switch (currentTier)
            {
                case SkillMasteryData.MasteryTier.Novice:
                    return _database.GetTierInfo(SkillMasteryData.MasteryTier.Apprentice);
                case SkillMasteryData.MasteryTier.Apprentice:
                    return _database.GetTierInfo(SkillMasteryData.MasteryTier.Journeyman);
                case SkillMasteryData.MasteryTier.Journeyman:
                    return _database.GetTierInfo(SkillMasteryData.MasteryTier.Expert);
                case SkillMasteryData.MasteryTier.Expert:
                    return _database.GetTierInfo(SkillMasteryData.MasteryTier.Master);
                case SkillMasteryData.MasteryTier.Master:
                    return _database.GetTierInfo(SkillMasteryData.MasteryTier.GrandMaster);
                default:
                    return null;
            }
        }

        private float GetProgressToNextTier(SkillMasteryData.SkillMastery skill)
        {
            var currentTierInfo = _database.GetTierInfo(skill.Tier);
            var nextTierInfo = GetNextTier(skill.Tier);

            if (nextTierInfo == null) return 1.0f;

            int pointsInTier = skill.MasteryPoints - currentTierInfo.MinPoints;
            int pointsNeeded = nextTierInfo.MinPoints - currentTierInfo.MinPoints;

            return Mathf.Clamp((float)pointsInTier / pointsNeeded, 0f, 1f);
        }

        /// <summary>
        /// Save mastery data (integrated with main save system)
        /// </summary>
        public void SaveData()
        {
            // Data will be saved with main game save
            // Integration with SaveSystem can be added later
            GD.Print($"[SkillMasterySystem] Saving {_playerData.Skills.Count} skills, {_playerData.TotalMasteryPoints} total points");
        }

        /// <summary>
        /// Load mastery data (integrated with main save system)
        /// </summary>
        public void LoadData()
        {
            // Data will be loaded with main game save
            // Integration with SaveSystem can be added later
            GD.Print("[SkillMasterySystem] Data will be loaded with main save");
        }

        /// <summary>
        /// Reset all mastery data (for testing)
        /// </summary>
        public void ResetData()
        {
            _playerData = new SkillMasteryData.PlayerSkillMasteryData();
            SaveData();
            GD.Print("[SkillMasterySystem] Data reset");
        }
    }

    /// <summary>
    /// Simple signal wrapper for Godot signals
    /// </summary>
    public class SignalWrapper : GodotObject
    {
        public new void Emit(params object[] args) { }
    }
}
