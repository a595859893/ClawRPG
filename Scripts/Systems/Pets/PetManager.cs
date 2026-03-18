using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物管理器 - 管理玩家拥有的宠物
    /// </summary>
    public class PetManager : BaseSystem
    {
        private static PetManager _instance;
        public static new PetManager Instance => _instance ??= new PetManager();

        // 玩家宠物列表
        private List<Pet> _ownedPets = new List<Pet>();
        private Pet _activePet = null;
        private int _maxPets = 10;

        // 信号系统
        public Action<Pet> OnPetAdded;
        public Action<Pet> OnPetRemoved;
        public Action<Pet> OnActivePetChanged;
        public Action<Pet> OnPetLevelUp;
        public Action<Pet, int> OnPetLoyaltyChanged;

        // 宠物获得/掉落配置
        private Dictionary<PetRarity, float> _dropChances = new Dictionary<PetRarity, float>
        {
            { PetRarity.Common, 0.3f },
            { PetRarity.Uncommon, 0.25f },
            { PetRarity.Rare, 0.15f },
            { PetRarity.Epic, 0.05f },
            { PetRarity.Legendary, 0.01f }
        };

        public List<Pet> OwnedPets => _ownedPets;
        public Pet ActivePet => _activePet;
        public int MaxPets => _maxPets;

        public void Initialize()
        {
            // 初始化
        }

        /// <summary>
        /// 添加宠物
        /// </summary>
        public bool AddPet(Pet pet)
        {
            if (_ownedPets.Count >= _maxPets)
            {
                GD.Warn("已达到最大宠物数量");
                return false;
            }

            if (_ownedPets.Exists(p => p.PetId == pet.PetId))
            {
                GD.Warn("已经拥有该宠物");
                return false;
            }

            _ownedPets.Add(pet);
            
            // 为新宠物生成随机天赋
            if (PetTalentSystem.Instance != null)
            {
                PetTalentSystem.Instance.GenerateRandomTalentsForPet(pet.Id, 3);
            }
            
            OnPetAdded?.Invoke(pet);
            
            // 如果没有激活宠物，自动激活
            if (_activePet == null)
            {
                SetActivePet(pet);
            }

            GD.Print($"获得新宠物: {pet.PetName}");
            return true;
        }

        /// <summary>
        /// 移除宠物
        /// </summary>
        public bool RemovePet(Pet pet)
        {
            if (_ownedPets.Remove(pet))
            {
                OnPetRemoved?.Invoke(pet);

                // 如果移除的是激活的宠物，选择下一个
                if (_activePet == pet)
                {
                    _activePet = _ownedPets.Count > 0 ? _ownedPets[0] : null;
                    OnActivePetChanged?.Invoke(_activePet);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置激活的宠物
        /// </summary>
        public void SetActivePet(Pet pet)
        {
            if (pet == null || !_ownedPets.Contains(pet)) return;

            _activePet = pet;
            OnActivePetChanged?.Invoke(pet);
            GD.Print($"激活宠物: {pet.PetName}");
        }

        /// <summary>
        /// 切换下一个宠物
        /// </summary>
        public void SwitchToNextPet()
        {
            if (_ownedPets.Count == 0) return;

            int currentIndex = _activePet != null ? _ownedPets.IndexOf(_activePet) : -1;
            int nextIndex = (currentIndex + 1) % _ownedPets.Count;
            SetActivePet(_ownedPets[nextIndex]);
        }

        /// <summary>
        /// 根据宠物ID获取宠物
        /// </summary>
        public Pet GetPetById(string petId)
        {
            return _ownedPets.Find(p => p.PetId == petId);
        }

        /// <summary>
        /// 宠物参与战斗（获得经验）
        /// </summary>
        public void OnBattleVictory(int experienceReward)
        {
            if (_activePet == null) return;

            int exp = (int)(experienceReward * 0.5f); // 宠物获得50%经验
            _activePet.AddExperience(exp);

            if (_activePet.Experience >= _activePet.ExperienceToNextLevel)
            {
                OnPetLevelUp?.Invoke(_activePet);
            }
        }

        /// <summary>
        /// 宠物参与战斗（增加忠诚度）
        /// </summary>
        public void OnBattleEnd(bool victory)
        {
            if (_activePet == null) return;

            int loyaltyChange = victory ? 2 : -1;
            _activePet.AddLoyalty(loyaltyChange);
            OnPetLoyaltyChanged?.Invoke(_activePet, _activePet.Loyalty);
        }

        /// <summary>
        /// 尝试捕捉宠物
        /// </summary>
        public bool TryCapturePet(PetRarity minRarity = PetRarity.Common)
        {
            float roll = (float)GD.Randf();
            PetRarity rolledRarity = GetRolledRarity(roll);

            if (rolledRarity < minRarity)
            {
                GD.Print("捕捉失败: 运气不好");
                return false;
            }

            Pet newPet = PetDatabase.Instance.GetRandomPet(rolledRarity);
            if (newPet == null)
            {
                GD.Print("捕捉失败: 没有可用的宠物");
                return false;
            }

            // 创建宠物副本
            Pet capturedPet = new Pet
            {
                PetId = newPet.PetId,
                PetName = newPet.PetName,
                Type = newPet.Type,
                Rarity = newPet.Rarity,
                HealthBonus = newPet.HealthBonus,
                AttackBonus = newPet.AttackBonus,
                DefenseBonus = newPet.DefenseBonus,
                SpeedBonus = newPet.SpeedBonus,
                CriticalBonus = newPet.CriticalBonus,
                SpecialAbility = newPet.SpecialAbility,
                SpecialValue = newPet.SpecialValue,
                Level = 1,
                Experience = 0,
                Loyalty = 50
            };

            return AddPet(capturedPet);
        }

        private PetRarity GetRolledRarity(float roll)
        {
            if (roll < _dropChances[PetRarity.Legendary]) return PetRarity.Legendary;
            if (roll < _dropChances[PetRarity.Legendary] + _dropChances[PetRarity.Epic]) return PetRarity.Epic;
            if (roll < _dropChances[PetRarity.Legendary] + _dropChances[PetRarity.Epic] + _dropChances[PetRarity.Rare]) return PetRarity.Rare;
            if (roll < _dropChances[PetRarity.Legendary] + _dropChances[PetRarity.Epic] + _dropChances[PetRarity.Rare] + _dropChances[PetRarity.Uncommon]) return PetRarity.Uncommon;
            return PetRarity.Common;
        }

        /// <summary>
        /// 获取激活宠物的属性加成
        /// </summary>
        public int GetActivePetHealthBonus() => _activePet?.GetTotalHealthBonus() ?? 0;
        public int GetActivePetAttackBonus() => _activePet?.GetTotalAttackBonus() ?? 0;
        public int GetActivePetDefenseBonus() => _activePet?.GetTotalDefenseBonus() ?? 0;
        public int GetActivePetSpeedBonus() => _activePet?.GetTotalSpeedBonus() ?? 0;
        public int GetActivePetCriticalBonus() => _activePet?.GetTotalCriticalBonus() ?? 0;

        /// <summary>
        /// 获取激活宠物的特殊效果
        /// </summary>
        public string GetActivePetSpecialAbility() => _activePet?.SpecialAbility ?? "";
        public float GetActivePetSpecialValue() => _activePet?.SpecialValue ?? 0f;

        /// <summary>
        /// 序列化宠物数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            var petDataList = new List<Dictionary<string, object>>();
            foreach (var pet in _ownedPets)
            {
                petDataList.Add(new Dictionary<string, object>
                {
                    { "petId", pet.PetId },
                    { "level", pet.Level },
                    { "experience", pet.Experience },
                    { "loyalty", pet.Loyalty }
                });
            }
            data["pets"] = petDataList;
            
            data["activePetId"] = _activePet?.PetId ?? "";
            data["maxPets"] = _maxPets;
            
            return data;
        }

        /// <summary>
        /// 反序列化宠物数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            _ownedPets.Clear();
            
            if (data.TryGetValue("pets", out var petsObj) && petsObj is List<object> petList)
            {
                foreach (var petData in petList)
                {
                    if (petData is Dictionary<string, object> petDict)
                    {
                        string petId = petDict.GetValueOrDefault("petId", "").ToString();
                        int level = Convert.ToInt32(petDict.GetValueOrDefault("level", 1));
                        int experience = Convert.ToInt32(petDict.GetValueOrDefault("experience", 0));
                        int loyalty = Convert.ToInt32(petDict.GetValueOrDefault("loyalty", 50));
                        
                        var template = PetDatabase.Instance.GetPet(petId);
                        if (template != null)
                        {
                            var pet = new Pet
                            {
                                PetId = template.PetId,
                                PetName = template.PetName,
                                Type = template.Type,
                                Rarity = template.Rarity,
                                HealthBonus = template.HealthBonus,
                                AttackBonus = template.AttackBonus,
                                DefenseBonus = template.DefenseBonus,
                                SpeedBonus = template.SpeedBonus,
                                CriticalBonus = template.CriticalBonus,
                                SpecialAbility = template.SpecialAbility,
                                SpecialValue = template.SpecialValue,
                                Level = level,
                                Experience = experience,
                                Loyalty = loyalty
                            };
                            _ownedPets.Add(pet);
                        }
                    }
                }
            }
            
            if (data.TryGetValue("activePetId", out var activeIdObj))
            {
                string activeId = activeIdObj.ToString();
                _activePet = GetPetById(activeId);
            }
            
            if (data.TryGetValue("maxPets", out var maxPetsObj))
            {
                _maxPets = Convert.ToInt32(maxPetsObj);
            }
        }
    }
}
