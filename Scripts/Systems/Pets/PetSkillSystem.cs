using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物技能系统管理器
    /// </summary>
    public class PetSkillSystem : BaseSystem
    {
        private static PetSkillSystem _instance;
        public static PetSkillSystem Instance => _instance ??= new PetSkillSystem();

        // 玩家宠物技能数据
        private Dictionary<string, PetSkillData> _playerPetSkills = new Dictionary<string, PetSkillData>(); // petId -> skill data
        private int _globalSkillPoints = 0;
        
        // 信号
        public Action<string> OnSkillLearned; // skillId
        public Action<string> OnSkillUsed; // skillId
        public Action OnSkillPointsChanged;

        public void Initialize()
        {
            _instance = this;
            PetSkillDatabase.Initialize();
            GD.Print("宠物技能系统已初始化");
        }

        #region 技能点管理

        public int GetSkillPoints(string petId)
        {
            if (_playerPetSkills.TryGetValue(petId, out var data))
                return data.AvailableSkillPoints;
            return 0;
        }

        public int GetGlobalSkillPoints() => _globalSkillPoints;

        public void AddSkillPoints(int amount, string petId = "")
        {
            if (string.IsNullOrEmpty(petId))
            {
                // 添加到全局技能点，所有宠物共享
                _globalSkillPoints += amount;
            }
            else
            {
                // 添加到指定宠物
                EnsurePetData(petId);
                _playerPetSkills[petId].AvailableSkillPoints += amount;
            }
            OnSkillPointsChanged?.Invoke();
        }

        public bool SpendSkillPoints(int amount, string petId)
        {
            int available = string.IsNullOrEmpty(petId) ? _globalSkillPoints : GetSkillPoints(petId);
            if (available >= amount)
            {
                if (string.IsNullOrEmpty(petId))
                {
                    _globalSkillPoints -= amount;
                }
                else
                {
                    _playerPetSkills[petId].AvailableSkillPoints -= amount;
                }
                OnSkillPointsChanged?.Invoke();
                return true;
            }
            return false;
        }

        #endregion

        #region 技能学习

        public bool CanLearnSkill(string petId, string skillId, int petLevel)
        {
            var skill = PetSkillDatabase.GetSkill(skillId);
            if (skill == null) return false;

            // 检查宠物等级
            if (petLevel < skill.RequiredLevel) return false;

            // 检查技能点
            int available = string.IsNullOrEmpty(petId) ? _globalSkillPoints : GetSkillPoints(petId);
            if (available < skill.SkillPointCost) return false;

            // 检查是否已学习
            if (IsSkillLearned(petId, skillId)) return false;

            return true;
        }

        public bool LearnSkill(string petId, string skillId, int petLevel)
        {
            if (!CanLearnSkill(petId, skillId, petLevel)) return false;

            var skill = PetSkillDatabase.GetSkill(skillId);
            if (skill == null) return false;

            // 消耗技能点
            if (!SpendSkillPoints(skill.SkillPointCost, petId)) return false;

            // 添加技能
            EnsurePetData(petId);
            var data = _playerPetSkills[petId];
            
            data.LearnedSkills[skillId] = 1;
            data.SkillInstances[skillId] = new LearnedPetSkill
            {
                SkillId = skillId,
                CurrentLevel = 1,
                CurrentCooldown = 0f,
                TimesUsed = 0
            };

            OnSkillLearned?.Invoke(skillId);
            GD.Print($"宠物 {petId} 学会了技能: {skill.SkillName}");
            return true;
        }

        public bool IsSkillLearned(string petId, string skillId)
        {
            if (_playerPetSkills.TryGetValue(petId, out var data))
                return data.LearnedSkills.ContainsKey(skillId);
            return false;
        }

        public int GetSkillLevel(string petId, string skillId)
        {
            if (_playerPetSkills.TryGetValue(petId, out var data))
                if (data.LearnedSkills.TryGetValue(skillId, out var level))
                    return level;
            return 0;
        }

        public List<PetSkill> GetLearnedSkills(string petId)
        {
            var result = new List<PetSkill>();
            if (_playerPetSkills.TryGetValue(petId, out var data))
            {
                foreach (var skillId in data.LearnedSkills.Keys)
                {
                    var skill = PetSkillDatabase.GetSkill(skillId);
                    if (skill != null)
                        result.Add(skill);
                }
            }
            return result;
        }

        #endregion

        #region 技能使用

        public bool CanUseSkill(string petId, string skillId)
        {
            if (!IsSkillLearned(petId, skillId)) return false;

            var data = _playerPetSkills[petId];
            if (!data.SkillInstances.TryGetValue(skillId, out var instance))
                return false;

            // 检查冷却
            if (instance.CurrentCooldown > 0) return false;

            return true;
        }

        public void UseSkill(string petId, string skillId)
        {
            if (!CanUseSkill(petId, skillId)) return;

            var data = _playerPetSkills[petId];
            var instance = data.SkillInstances[skillId];
            var skill = PetSkillDatabase.GetSkill(skillId);
            
            if (skill == null) return;

            // 设置冷却
            instance.CurrentCooldown = skill.Cooldown;
            instance.TimesUsed++;

            OnSkillUsed?.Invoke(skillId);
            GD.Print($"宠物 {petId} 使用了技能: {skill.SkillName}");
        }

        public void UpdateCooldowns(float delta, string petId)
        {
            if (_playerPetSkills.TryGetValue(petId, out var data))
            {
                foreach (var instance in data.SkillInstances.Values)
                {
                    if (instance.CurrentCooldown > 0)
                    {
                        instance.CurrentCooldown = Mathf.Max(0, instance.CurrentCooldown - delta);
                    }
                }
            }
        }

        public float GetCooldown(string petId, string skillId)
        {
            if (_playerPetSkills.TryGetValue(petId, out var data))
                if (data.SkillInstances.TryGetValue(skillId, out var instance))
                    return instance.CurrentCooldown;
            return 0f;
        }

        #endregion

        #region 数据管理

        private void EnsurePetData(string petId)
        {
            if (!_playerPetSkills.ContainsKey(petId))
            {
                _playerPetSkills[petId] = new PetSkillData();
            }
        }

        public Dictionary<string, object> GetSaveData()
        {
            var data = new Dictionary<string, object>();
            data["global_skill_points"] = _globalSkillPoints;
            
            var petDataList = new List<Dictionary<string, object>>();
            foreach (var kvp in _playerPetSkills)
            {
                var petData = new Dictionary<string, object>();
                petData["pet_id"] = kvp.Key;
                petData["available_skill_points"] = kvp.Value.AvailableSkillPoints;
                
                var skillsList = new List<Dictionary<string, object>>();
                foreach (var skillKvp in kvp.Value.LearnedSkills)
                {
                    var skillData = new Dictionary<string, object>();
                    skillData["skill_id"] = skillKvp.Key;
                    skillData["level"] = skillKvp.Value;
                    
                    if (kvp.Value.SkillInstances.TryGetValue(skillKvp.Key, out var instance))
                    {
                        skillData["times_used"] = instance.TimesUsed;
                    }
                    skillsList.Add(skillData);
                }
                petData["skills"] = skillsList;
                petDataList.Add(petData);
            }
            data["pets"] = petDataList;
            
            return data;
        }

        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            _globalSkillPoints = data.GetValueOrDefault("global_skill_points", 0);
            
            _playerPetSkills.Clear();
            if (data.TryGetValue("pets", out var petsObj) && petsObj is List<object> petsList)
            {
                foreach (var petObj in petsList)
                {
                    if (petObj is Dictionary<string, object> petData)
                    {
                        var petId = petData.GetValueOrDefault("pet_id", "").ToString();
                        var skillPoints = Convert.ToInt32(petData.GetValueOrDefault("available_skill_points", 0));
                        
                        var petSkillData = new PetSkillData
                        {
                            AvailableSkillPoints = skillPoints
                        };
                        
                        if (petData.TryGetValue("skills", out var skillsObj) && skillsObj is List<object> skillsList)
                        {
                            foreach (var skillObj in skillsList)
                            {
                                if (skillObj is Dictionary<string, object> skillData)
                                {
                                    var skillId = skillData.GetValueOrDefault("skill_id", "").ToString();
                                    var level = Convert.ToInt32(skillData.GetValueOrDefault("level", 1));
                                    var timesUsed = Convert.ToInt32(skillData.GetValueOrDefault("times_used", 0));
                                    
                                    petSkillData.LearnedSkills[skillId] = level;
                                    petSkillData.SkillInstances[skillId] = new LearnedPetSkill
                                    {
                                        SkillId = skillId,
                                        CurrentLevel = level,
                                        TimesUsed = timesUsed
                                    };
                                }
                            }
                        }
                        
                        _playerPetSkills[petId] = petSkillData;
                    }
                }
            }
            
            GD.Print($"宠物技能数据已加载: {_playerPetSkills.Count} 只宠物, {_globalSkillPoints} 技能点");
        }

        public void Clear()
        {
            _playerPetSkills.Clear();
            _globalSkillPoints = 0;
        }

        #endregion

        #region Save System
        
        public override Dictionary ExportSaveData()
        {
            var data = new Godot.Dictionary();
            
            // 保存全局技能点
            data["global_skill_points"] = _globalSkillPoints;
            
            // 保存每个宠物的技能数据
            var petsData = new Godot.Dictionary();
            foreach (var kvp in _playerPetSkills)
            {
                var petData = new Godot.Dictionary();
                petData["available_skill_points"] = kvp.Value.AvailableSkillPoints;
                
                // 保存已学习的技能
                var skillsData = new Godot.Dictionary();
                foreach (var skillKvp in kvp.Value.LearnedSkills)
                {
                    var skillInstance = new Godot.Dictionary();
                    skillInstance["level"] = skillKvp.Value;
                    
                    if (kvp.Value.SkillInstances.TryGetValue(skillKvp.Key, out var instance))
                    {
                        skillInstance["times_used"] = instance.TimesUsed;
                    }
                    skillsData[skillKvp.Key] = skillInstance;
                }
                petData["skills"] = skillsData;
                
                petsData[kvp.Key] = petData;
            }
            data["pets"] = petsData;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 加载全局技能点
            if (data.Contains("global_skill_points"))
                _globalSkillPoints = (int)data["global_skill_points"];
            
            // 加载宠物技能数据
            _playerPetSkills.Clear();
            if (data.Contains("pets"))
            {
                var petsData = (Godot.Dictionary)data["pets"];
                foreach (string petId in petsData.Keys)
                {
                    var petData = (Godot.Dictionary)petsData[petId];
                    var petSkillData = new PetSkillData();
                    
                    if (petData.Contains("available_skill_points"))
                        petSkillData.AvailableSkillPoints = (int)petData["available_skill_points"];
                    
                    if (petData.Contains("skills"))
                    {
                        var skillsData = (Godot.Dictionary)petData["skills"];
                        foreach (string skillId in skillsData.Keys)
                        {
                            var skillInstance = (Godot.Dictionary)skillsData[skillId];
                            var level = (int)skillInstance["level"];
                            petSkillData.LearnedSkills[skillId] = level;
                            
                            var learnedSkill = new LearnedPetSkill
                            {
                                SkillId = skillId,
                                CurrentLevel = level
                            };
                            if (skillInstance.Contains("times_used"))
                                learnedSkill.TimesUsed = (int)skillInstance["times_used"];
                            
                            petSkillData.SkillInstances[skillId] = learnedSkill;
                        }
                    }
                    
                    _playerPetSkills[petId] = petSkillData;
                }
            }
            
            GD.Print($"[PetSkill] Loaded: {_playerPetSkills.Count} pets, {_globalSkillPoints} global points");
        }
        
        #endregion
    }
}
