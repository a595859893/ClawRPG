using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Boss Ability Database - Manages boss skills and abilities
    /// Part of BossMechanicsSystem refactoring
    /// </summary>
    public partial class BossAbilityDatabase : BaseSystem
    {
        private BossMechanicsSystem _bossSystem;
        private BossMechanicsDatabase _mechanicsDb;
        
        // Ability usage tracking
        private Dictionary<string, Dictionary<string, int>> _skillUsageCounts = new Dictionary<string, Dictionary<string, int>>();
        
        public BossAbilityDatabase(BossMechanicsSystem bossSystem)
        {
            _bossSystem = bossSystem;
            _mechanicsDb = BossMechanicsDatabase.Instance;
        }
        
        /// <summary>
        /// Get available skills (not on cooldown)
        /// </summary>
        public List<string> GetAvailableSkills(BossBattleState state, BossMechanicsData bossData)
        {
            List<string> availableSkills = new List<string>();
            
            if (bossData == null || bossData.Skills == null) return availableSkills;
            
            foreach (var skill in bossData.Skills)
            {
                if (state.SkillCooldowns[skill.SkillId] <= 0)
                {
                    availableSkills.Add(skill.SkillId);
                }
            }
            
            return availableSkills;
        }
        
        /// <summary>
        /// Use a skill
        /// </summary>
        public bool UseSkill(BossBattleState state, BossMechanicsData bossData, string skillId)
        {
            // Find the skill
            BossSkillData skill = null;
            if (bossData != null && bossData.Skills != null)
            {
                foreach (var s in bossData.Skills)
                {
                    if (s.SkillId == skillId)
                    {
                        skill = s;
                        break;
                    }
                }
            }
            
            if (skill == null) return false;
            
            // Check cooldown
            if (state.SkillCooldowns[skillId] > 0) return false;
            
            // Use skill
            state.SkillCooldowns[skillId] = skill.Cooldown;
            state.SkillsUsed[skillId] = state.SkillsUsed.GetValueOrDefault(skillId, 0) + 1;
            
            // Track usage
            if (!_skillUsageCounts.ContainsKey(state.BossId))
            {
                _skillUsageCounts[state.BossId] = new Dictionary<string, int>();
            }
            if (!_skillUsageCounts[state.BossId].ContainsKey(skillId))
            {
                _skillUsageCounts[state.BossId][skillId] = 0;
            }
            _skillUsageCounts[state.BossId][skillId]++;
            
            GD.Print($"[BossAbilityDatabase] Boss used skill: {skill.SkillName}");
            return true;
        }
        
        /// <summary>
        /// Update skill cooldowns
        /// </summary>
        public void UpdateCooldowns(BossBattleState state, BossMechanicsData bossData, float delta)
        {
            if (bossData == null || bossData.Skills == null) return;
            
            foreach (var skill in bossData.Skills)
            {
                if (state.SkillCooldowns.ContainsKey(skill.SkillId))
                {
                    state.SkillCooldowns[skill.SkillId] = Mathf.Max(0, state.SkillCooldowns[skill.SkillId] - delta);
                }
            }
        }
        
        /// <summary>
        /// Get skill data
        /// </summary>
        public BossSkillData GetSkillData(BossMechanicsData bossData, string skillId)
        {
            if (bossData == null || bossData.Skills == null) return null;
            
            foreach (var skill in bossData.Skills)
            {
                if (skill.SkillId == skillId)
                {
                    return skill;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Select best skill based on current situation
        /// </summary>
        public string SelectBestSkill(BossBattleState state, BossMechanicsData bossData)
        {
            var availableSkills = GetAvailableSkills(state, bossData);
            if (availableSkills.Count == 0) return null;
            
            // Simple AI: prioritize skills with longer cooldowns (more powerful)
            // Could be enhanced with more sophisticated logic
            string bestSkill = null;
            float highestCooldown = -1f;
            
            foreach (var skillId in availableSkills)
            {
                var skill = GetSkillData(bossData, skillId);
                if (skill != null && skill.Cooldown > highestCooldown)
                {
                    highestCooldown = skill.Cooldown;
                    bestSkill = skillId;
                }
            }
            
            return bestSkill;
        }
        
        /// <summary>
        /// Get skill usage statistics
        /// </summary>
        public int GetSkillUsageCount(string bossId, string skillId)
        {
            if (_skillUsageCounts.ContainsKey(bossId) && _skillUsageCounts[bossId].ContainsKey(skillId))
            {
                return _skillUsageCounts[bossId][skillId];
            }
            return 0;
        }
        
        /// <summary>
        /// Reset skill usage tracking
        /// </summary>
        public void ResetUsageTracking(string bossId)
        {
            if (_skillUsageCounts.ContainsKey(bossId))
            {
                _skillUsageCounts[bossId].Clear();
            }
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // No persistent data needed
        }
    }
}
