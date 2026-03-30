using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Skills {
    /// <summary>
    /// 技能解锁条件类型
    /// </summary>
    public enum UnlockConditionType
    {
        Level,           // 需要玩家等级
        SkillMastery,    // 需要技能精通等级
        SkillLearned,    // 需要已学习某技能
        ComboComplete,   // 需要完成过某连招
        Item,            // 需要持有某物品
        Quest,           // 需要完成某任务
        Gold,            // 需要金币
        Reputation       // 需要声望
    }
    
    /// <summary>
    /// 技能解锁条件
    /// </summary>
    public class SkillUnlockCondition
    {
        public int ConditionId { get; set; }
        public UnlockConditionType Type { get; set; }
        public int RequiredValue { get; set; } // 等级/数量等
        public string RequiredSkillId { get; set; } // 技能ID (for SkillMastery/SkillLearned)
        public int RequiredComboId { get; set; } // 连招ID (for ComboComplete)
        public string RequiredItemId { get; set; } // 物品ID (for Item)
        public string RequiredQuestId { get; set; } // 任务ID (for Quest)
        public string Description { get; set; } = "";
    }
    
    /// <summary>
    /// 技能解锁状态记录
    /// </summary>
    public class SkillUnlockState
    {
        public string SkillId { get; set; } = "";
        public bool IsUnlocked { get; set; } = false;
        public int UnlockLevel { get; set; } = 0; // 解锁时的玩家等级
        public float UnlockTime { get; set; } = 0; // 解锁时间戳
        public List<int> CompletedConditionIds { get; set; } = new(); // 已完成的条件ID
    }
    
    /// <summary>
    /// 连招解锁条件定义
    /// </summary>
    public class ComboUnlockCondition
    {
        public int ComboId { get; set; }
        public int RequiredMasteryLevel { get; set; } = 1;
        public List<int> RequiredSkillIds { get; set; } = new();
        public string Description { get; set; } = "";
    }
    
    /// <summary>
    /// 技能解锁系统 - 负责技能/连招解锁条件检查和状态管理
    /// </summary>
    public partial class SkillUnlockSystem : BaseSystem
    {
        private static SkillUnlockSystem _instance;
        public static SkillUnlockSystem Instance => _instance;
        
        // 技能解锁条件定义: skillId -> conditions
        private Dictionary<string, List<SkillUnlockCondition>> _skillUnlockConditions = new();
        
        // 玩家技能解锁状态: playerId -> skillId -> state
        private Dictionary<string, Dictionary<string, SkillUnlockState>> _unlockStates = new();
        
        // 连招解锁条件: comboId -> condition
        private Dictionary<int, ComboUnlockCondition> _comboUnlockConditions = new();
        
        protected override void Initialize()
        {
            InitializeSkillUnlockConditions();
            InitializeComboUnlockConditions();
            IsInitialized = true;
            GD.Print("[SkillUnlockSystem] Initialized");
        }
        
        #region Initialize Unlock Conditions
        
        private void InitializeSkillUnlockConditions()
        {
            // 基础技能 - 默认解锁
            AddSkillUnlockCondition("skill_fireball", new SkillUnlockCondition
            {
                ConditionId = 1,
                Type = UnlockConditionType.Level,
                RequiredValue = 1,
                Description = "玩家等级达到 1 级"
            });
            
            // 高级技能 - 需要等级
            AddSkillUnlockCondition("skill_ice_bolt", new SkillUnlockCondition
            {
                ConditionId = 2,
                Type = UnlockConditionType.Level,
                RequiredValue = 5,
                Description = "玩家等级达到 5 级"
            });
            
            AddSkillUnlockCondition("skill_lightning", new SkillUnlockCondition
            {
                ConditionId = 3,
                Type = UnlockConditionType.Level,
                RequiredValue = 10,
                Description = "玩家等级达到 10 级"
            });
            
            // 需要前置技能的技能
            AddSkillUnlockCondition("skill_advanced_fire", new SkillUnlockCondition
            {
                ConditionId = 4,
                Type = UnlockConditionType.SkillLearned,
                RequiredSkillId = "skill_fireball",
                Description = "需要已学习 火球术"
            });
            
            AddSkillUnlockCondition("skill_frost_nova", new SkillUnlockCondition
            {
                ConditionId = 5,
                Type = UnlockConditionType.SkillLearned,
                RequiredSkillId = "skill_ice_bolt",
                Description = "需要已学习 寒冰箭"
            });
            
            // 需要精通等级的技能
            AddSkillUnlockCondition("skill_meteor", new SkillUnlockCondition
            {
                ConditionId = 6,
                Type = UnlockConditionType.SkillMastery,
                RequiredSkillId = "skill_fireball",
                RequiredValue = 5,
                Description = "需要火球术达到 5 级精通"
            });
            
            AddSkillUnlockCondition("skill_blizzard", new SkillUnlockCondition
            {
                ConditionId = 7,
                Type = UnlockConditionType.SkillMastery,
                RequiredSkillId = "skill_ice_bolt",
                RequiredValue = 5,
                Description = "需要寒冰箭达到 5 级精通"
            });
            
            // 需要完成连招解锁
            AddSkillUnlockCondition("skill_ultimate_fire", new SkillUnlockCondition
            {
                ConditionId = 8,
                Type = UnlockConditionType.ComboComplete,
                RequiredComboId = 7, // 终极陨石
                Description = "需要完成 终极陨石 连招"
            });
        }
        
        private void InitializeComboUnlockConditions()
        {
            // 这些与 SkillMasterySystem 中的 combo 定义保持一致
            AddComboUnlockCondition(new ComboUnlockCondition
            {
                ComboId = 1,
                RequiredMasteryLevel = 3,
                RequiredSkillIds = new List<int> { 1, 10 },
                Description = "闪电箭等级3 + 链式闪电"
            });
            
            AddComboUnlockCondition(new ComboUnlockCondition
            {
                ComboId = 2,
                RequiredMasteryLevel = 4,
                RequiredSkillIds = new List<int> { 7, 8 },
                Description = "燃烧弹等级4 + 冰霜新星"
            });
            
            AddComboUnlockCondition(new ComboUnlockCondition
            {
                ComboId = 3,
                RequiredMasteryLevel = 2,
                RequiredSkillIds = new List<int> { 4, 9 },
                Description = "暗影箭等级2 + 暗影之刺"
            });
            
            AddComboUnlockCondition(new ComboUnlockCondition
            {
                ComboId = 4,
                RequiredMasteryLevel = 5,
                RequiredSkillIds = new List<int> { 101, 102, 103 },
                Description = "治疗链 - 治疗术/群体治疗/再生 全部5级"
            });
            
            AddComboUnlockCondition(new ComboUnlockCondition
            {
                ComboId = 5,
                RequiredMasteryLevel = 4,
                RequiredSkillIds = new List<int> { 203, 204 },
                Description = "护盾链 - 魔法护盾/圣光护盾 全部4级"
            });
            
            AddComboUnlockCondition(new ComboUnlockCondition
            {
                ComboId = 6,
                RequiredMasteryLevel = 4,
                RequiredSkillIds = new List<int> { 1, 4, 7 },
                Description = "元素风暴 - 闪电/暗影/火焰 全部4级"
            });
            
            AddComboUnlockCondition(new ComboUnlockCondition
            {
                ComboId = 7,
                RequiredMasteryLevel = 5,
                RequiredSkillIds = new List<int> { 7, 2 },
                Description = "终极陨石 - 燃烧弹/陨石 全部5级"
            });
            
            AddComboUnlockCondition(new ComboUnlockCondition
            {
                ComboId = 8,
                RequiredMasteryLevel = 5,
                RequiredSkillIds = new List<int> { 3, 101 },
                Description = "圣光审判 - 圣光打击/治疗术 全部5级"
            });
        }
        
        private void AddSkillUnlockCondition(string skillId, SkillUnlockCondition condition)
        {
            if (!_skillUnlockConditions.ContainsKey(skillId))
            {
                _skillUnlockConditions[skillId] = new List<SkillUnlockCondition>();
            }
            _skillUnlockConditions[skillId].Add(condition);
        }
        
        private void AddComboUnlockCondition(ComboUnlockCondition condition)
        {
            _comboUnlockConditions[condition.ComboId] = condition;
        }
        
        #endregion
        
        #region Skill Unlock Check
        
        /// <summary>
        /// 检查技能是否可以解锁 (不实际解锁)
        /// </summary>
        public bool CanUnlockSkill(string playerId, string skillId, int playerLevel, 
            Dictionary<string, int> learnedSkills, Dictionary<string, int> skillMasteryLevels,
            HashSet<int> completedCombos)
        {
            // 默认技能无需条件
            if (!_skillUnlockConditions.ContainsKey(skillId) || _skillUnlockConditions[skillId].Count == 0)
                return true;
            
            foreach (var condition in _skillUnlockConditions[skillId])
            {
                if (!CheckCondition(condition, playerLevel, learnedSkills, skillMasteryLevels, completedCombos))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 检查单个解锁条件
        /// </summary>
        private bool CheckCondition(SkillUnlockCondition condition, int playerLevel,
            Dictionary<string, int> learnedSkills, Dictionary<string, int> skillMasteryLevels,
            HashSet<int> completedCombos)
        {
            switch (condition.Type)
            {
                case UnlockConditionType.Level:
                    return playerLevel >= condition.RequiredValue;
                    
                case UnlockConditionType.SkillLearned:
                    return learnedSkills.ContainsKey(condition.RequiredSkillId);
                    
                case UnlockConditionType.SkillMastery:
                    if (skillMasteryLevels.TryGetValue(condition.RequiredSkillId, out int level))
                        return level >= condition.RequiredValue;
                    return false;
                    
                case UnlockConditionType.ComboComplete:
                    return completedCombos.Contains(condition.RequiredComboId);
                    
                case UnlockConditionType.Gold:
                case UnlockConditionType.Reputation:
                case UnlockConditionType.Item:
                case UnlockConditionType.Quest:
                    // 这些需要额外的资源系统支持，这里暂时返回 true
                    return true;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 解锁技能
        /// </summary>
        public bool UnlockSkill(string playerId, string skillId, int playerLevel)
        {
            EnsureUnlockState(playerId, skillId);
            
            var state = _unlockStates[playerId][skillId];
            if (state.IsUnlocked)
                return false;
            
            state.IsUnlocked = true;
            state.UnlockLevel = playerLevel;
            state.UnlockTime = Time.GetUnixTimeFromSystem();
            
            GD.Print($"[SkillUnlockSystem] Player {playerId} unlocked skill: {skillId}");
            return true;
        }
        
        /// <summary>
        /// 检查技能是否已解锁
        /// </summary>
        public bool IsSkillUnlocked(string playerId, string skillId)
        {
            if (!_unlockStates.ContainsKey(playerId))
                return false;
            
            if (!_unlockStates[playerId].ContainsKey(skillId))
                return false;
            
            // 如果没有定义解锁条件，默认已解锁
            if (!_skillUnlockConditions.ContainsKey(skillId) || _skillUnlockConditions[skillId].Count == 0)
                return true;
            
            return _unlockStates[playerId][skillId].IsUnlocked;
        }
        
        /// <summary>
        /// 获取技能解锁状态
        /// </summary>
        public SkillUnlockState GetUnlockState(string playerId, string skillId)
        {
            EnsureUnlockState(playerId, skillId);
            return _unlockStates[playerId][skillId];
        }
        
        /// <summary>
        /// 获取玩家所有已解锁技能
        /// </summary>
        public List<string> GetUnlockedSkills(string playerId)
        {
            var result = new List<string>();
            
            if (!_unlockStates.ContainsKey(playerId))
                return result;
            
            foreach (var kvp in _unlockStates[playerId])
            {
                if (kvp.Value.IsUnlocked)
                    result.Add(kvp.Key);
            }
            
            return result;
        }
        
        private void EnsureUnlockState(string playerId, string skillId)
        {
            if (!_unlockStates.ContainsKey(playerId))
            {
                _unlockStates[playerId] = new Dictionary<string, SkillUnlockState>();
            }
            
            if (!_unlockStates[playerId].ContainsKey(skillId))
            {
                _unlockStates[playerId][skillId] = new SkillUnlockState
                {
                    SkillId = skillId,
                    IsUnlocked = !_skillUnlockConditions.ContainsKey(skillId) || _skillUnlockConditions[skillId].Count == 0
                };
            }
        }
        
        #endregion
        
        #region Combo Unlock Check
        
        /// <summary>
        /// 检查连招是否可以解锁 (使用)
        /// </summary>
        public bool CanUseCombo(int comboId, Dictionary<string, int> learnedSkills, 
            Dictionary<string, int> skillMasteryLevels)
        {
            if (!_comboUnlockConditions.ContainsKey(comboId))
                return true; // 没有定义条件，默认可用
            
            var condition = _comboUnlockConditions[comboId];
            
            // 检查是否有所需技能
            foreach (var skillId in condition.RequiredSkillIds)
            {
                // skillId 是 int 类型，需要转换
                string skillIdStr = skillId.ToString();
                if (!learnedSkills.ContainsKey(skillIdStr))
                    return false;
            }
            
            // 检查精通等级
            foreach (var kvp in learnedSkills)
            {
                if (skillMasteryLevels.TryGetValue(kvp.Key, out int masteryLevel))
                {
                    if (masteryLevel < condition.RequiredMasteryLevel)
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取连招解锁条件描述
        /// </summary>
        public string GetComboUnlockDescription(int comboId)
        {
            if (_comboUnlockConditions.TryGetValue(comboId, out var condition))
                return condition.Description;
            return "";
        }
        
        /// <summary>
        /// 获取所有连招解锁条件
        /// </summary>
        public Dictionary<int, ComboUnlockCondition> GetAllComboConditions()
        {
            return new Dictionary<int, ComboUnlockCondition>(_comboUnlockConditions);
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 技能解锁状态
            var statesArray = new Array();
            foreach (var playerKvp in _unlockStates)
            {
                foreach (var skillKvp in playerKvp.Value)
                {
                    var entry = new Dictionary
                    {
                        ["playerId"] = playerKvp.Key,
                        ["skillId"] = skillKvp.Key,
                        ["isUnlocked"] = skillKvp.Value.IsUnlocked,
                        ["unlockLevel"] = skillKvp.Value.UnlockLevel,
                        ["unlockTime"] = skillKvp.Value.UnlockTime
                    };
                    statesArray.Add(entry);
                }
            }
            data["unlockStates"] = statesArray;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _unlockStates.Clear();
            
            if (data.Contains("unlockStates"))
            {
                var statesArray = (Array)data["unlockStates"];
                foreach (Dictionary entry in statesArray)
                {
                    var playerId = entry["playerId"].ToString();
                    var skillId = entry["skillId"].ToString();
                    var isUnlocked = Convert.ToBoolean(entry["isUnlocked"]);
                    
                    if (!_unlockStates.ContainsKey(playerId))
                    {
                        _unlockStates[playerId] = new Dictionary<string, SkillUnlockState>();
                    }
                    
                    _unlockStates[playerId][skillId] = new SkillUnlockState
                    {
                        SkillId = skillId,
                        IsUnlocked = isUnlocked,
                        UnlockLevel = Convert.ToInt32(entry["unlockLevel"]),
                        UnlockTime = Convert.ToSingle(entry["unlockTime"])
                    };
                }
            }
        }
        
        #endregion
    }
}
