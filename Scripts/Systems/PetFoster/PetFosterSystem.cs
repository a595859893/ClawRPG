using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

namespace ClawRPG.Scripts.Systems.PetFoster
{
    /// <summary>
    /// 宠物寄养系统管理器
    /// </summary>
    public partial class PetFosterSystem : Node
    {
        public static PetFosterSystem Instance { get; private set; }
        
        private PlayerFosterData _playerData;
        private bool _initialized = false; 
        
        // 信号系统
        public delegate void FosterStartedHandler(string petId, string configId);
        public delegate void FosterCompletedHandler(string petId, int expReward, int goldReward);
        public delegate void FosterClaimedHandler(string petId);
        
        public event FosterStartedHandler OnFosterStarted;
        public event FosterCompletedHandler OnFosterCompleted;
        public event FosterClaimedHandler OnFosterClaimed;
        
        public override void _Ready()
        {
            Instance = this;
            PetFosterDatabase.Initialize();
            LoadData();
        }
        
        /// <summary>
        /// 开始寄养
        /// </summary>
        public bool StartFoster(string petId, string configId)
        {
            var config = PetFosterDatabase.GetConfig(configId);
            if (config == null)
            {
                GD.PrintErr($"[PetFosterSystem] Config not found: {configId}");
                return false;
            }
            
            var player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
            if (player == null)
            {
                GD.PrintErr("[PetFosterSystem] Player not found");
                return false;
            }
            
            int playerGold = player.Get("gold") as int? ?? 0;
            if (playerGold < config.Cost)
            {
                GD.PrintErr($"[PetFosterSystem] Not enough gold. Need {config.Cost}, have {playerGold}");
                return false;
            }
            
            // 检查宠物是否已在寄养中
            if (_playerData.ActiveFosters.ContainsKey(petId))
            {
                GD.PrintErr($"[PetFosterSystem] Pet {petId} is already fostering");
                return false;
            }
            
            // 扣除金币
            player.Set("gold", playerGold - config.Cost);
            
            // 创建寄养记录
            var foster = new ActiveFoster
            {
                PetId = petId,
                ConfigId = configId,
                Type = config.Type,
                StartTime = Time.GetUnixTimeFromSystem(),
                Duration = config.Duration,
                Status = FosterStatus.Fostering,
                ExpReward = config.ExpReward,
                GoldReward = config.GoldReward,
                AffectionReward = config.AffectionReward
            };
            
            _playerData.ActiveFosters[petId] = foster;
            SaveData();
            
            OnFosterStarted?.Invoke(petId, configId);
            GD.Print($"[PetFosterSystem] Started foster for pet {petId} with config {configId}");
            
            return true;
        }
        
        /// <summary>
        /// 领取寄养奖励
        /// </summary>
        public bool ClaimFosterReward(string petId)
        {
            if (!_playerData.ActiveFosters.ContainsKey(petId))
            {
                GD.PrintErr($"[PetFosterSystem] No active foster for pet {petId}");
                return false;
            }
            
            var foster = _playerData.ActiveFosters[petId];
            if (foster.Status != FosterStatus.Completed)
            {
                GD.PrintErr($"[PetFosterSystem] Foster not completed yet for pet {petId}");
                return false;
            }
            
            var config = PetFosterDatabase.GetConfig(foster.ConfigId);
            var player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
            
            if (player != null)
            {
                // 发放经验
                int currentExp = player.Get("experience") as int? ?? 0;
                player.Set("experience", currentExp + foster.ExpReward);
                
                // 发放金币
                int currentGold = player.Get("gold") as int? ?? 0;
                player.Set("gold", currentGold + foster.GoldReward);
                
                // 发放好感度（如果好感度系统存在）
                var petAffection = GetTree().GetFirstNodeInGroup("PetAffectionSystem") as Node;
                if (petAffection != null)
                {
                    var method = petAffection.GetType().GetMethod("AddAffection");
                    method?.Invoke(petAffection, new object[] { petId, foster.AffectionReward });
                }
                
                // 材料奖励
                if (config.MaterialRewards != null && config.MaterialRewards.Count > 0)
                {
                    var inventoryManager = GetTree().GetFirstNodeInGroup("InventoryManager") as Node;
                    if (inventoryManager != null)
                    {
                        var random = new Random();
                        foreach (var materialId in config.MaterialRewards)
                        {
                            if (random.NextDouble() < config.MaterialDropChance)
                            {
                                var addItemMethod = inventoryManager.GetType().GetMethod("AddItem");
                                addItemMethod?.Invoke(inventoryManager, new object[] { materialId, 1 });
                                _playerData.TotalMaterialsGained++;
                            }
                        }
                    }
                }
            }
            
            // 记录历史
            var record = new FosterRecord
            {
                PetId = petId,
                PetName = $"Pet_{petId}",
                Type = foster.Type,
                CompletedTime = Time.GetUnixTimeFromSystem(),
                ExpGained = foster.ExpReward,
                GoldEarned = foster.GoldReward,
                AffectionGained = foster.AffectionReward,
                MaterialsGained = new List<string>()
            };
            _playerData.History.Add(record);
            
            // 更新统计
            _playerData.TotalFosters++;
            _playerData.TotalExpGained += foster.ExpReward;
            _playerData.TotalGoldEarned += foster.GoldReward;
            
            // 移除活跃寄养
            _playerData.ActiveFosters.Remove(petId);
            SaveData();
            
            OnFosterClaimed?.Invoke(petId);
            OnFosterCompleted?.Invoke(petId, foster.ExpReward, foster.GoldReward);
            
            GD.Print($"[PetFosterSystem] Claimed reward for pet {petId}: {foster.ExpReward} exp, {foster.GoldReward} gold");
            
            return true;
        }
        
        /// <summary>
        /// 取消寄养
        /// </summary>
        public bool CancelFoster(string petId)
        {
            if (!_playerData.ActiveFosters.ContainsKey(petId))
            {
                GD.PrintErr($"[PetFosterSystem] No active foster for pet {petId}");
                return false;
            }
            
            _playerData.ActiveFosters.Remove(petId);
            SaveData();
            
            GD.Print($"[PetFosterSystem] Cancelled foster for pet {petId}");
            
            return true;
        }
        
        /// <summary>
        /// 更新寄养状态
        /// </summary>
        public void Update()
        {
            long currentTime = Time.GetUnixTimeFromSystem();
            
            foreach (var kvp in _playerData.ActiveFosters)
            {
                var foster = kvp.Value;
                if (foster.Status == FosterStatus.Fostering)
                {
                    long elapsed = currentTime - foster.StartTime;
                    if (elapsed >= foster.Duration)
                    {
                        foster.Status = FosterStatus.Completed;
                        GD.Print($"[PetFosterSystem] Foster completed for pet {kvp.Key}");
                    }
                }
            }
            
            // 自动保存
            if (Time.GetTicksMsec() % 60000 < 20)  // 每分钟自动保存
            {
                SaveData();
            }
        }
        
        /// <summary>
        /// 获取寄养状态
        /// </summary>
        public FosterStatus GetFosterStatus(string petId)
        {
            if (_playerData.ActiveFosters.ContainsKey(petId))
            {
                return _playerData.ActiveFosters[petId].Status;
            }
            return FosterStatus.Available;
        }
        
        /// <summary>
        /// 获取寄养剩余时间
        /// </summary>
        public int GetRemainingTime(string petId)
        {
            if (_playerData.ActiveFosters.ContainsKey(petId))
            {
                var foster = _playerData.ActiveFosters[petId];
                long elapsed = Time.GetUnixTimeFromSystem() - foster.StartTime;
                return Mathf.Max(0, foster.Duration - (int)elapsed);
            }
            return 0;
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            return new Dictionary<string, int>
            {
                { "total_fosters", _playerData.TotalFosters },
                { "total_exp", _playerData.TotalExpGained },
                { "total_gold", _playerData.TotalGoldEarned },
                { "total_materials", _playerData.TotalMaterialsGained }
            };
        }
        
        /// <summary>
        /// 获取历史记录
        /// </summary>
        public List<FosterRecord> GetHistory(int limit = 10)
        {
            return _playerData.History.TakeLast(limit).ToList();
        }
        
        private void LoadData()
        {
            // 从存档系统加载数据
            var saveSystem = GetTree().GetFirstNodeInGroup("SaveSystem") as Node;
            if (saveSystem != null)
            {
                var loadMethod = saveSystem.GetType().GetMethod("LoadPetFosterData");
                var data = loadMethod?.Invoke(saveSystem, null);
                if (data is PlayerFosterData loadedData)
                {
                    _playerData = loadedData;
                    _initialized = true;
                    return;
                }
            }
            
            // 默认数据
            _playerData = new PlayerFosterData
            {
                ActiveFosters = new Dictionary<string, ActiveFoster>(),
                History = new List<FosterRecord>(),
                TotalFosters = 0,
                TotalExpGained = 0,
                TotalGoldEarned = 0,
                TotalMaterialsGained = 0
            };
            _initialized = true;
        }
        
        public void SaveData()
        {
            var saveSystem = GetTree().GetFirstNodeInGroup("SaveSystem") as Node;
            saveSystem?.Set("pet_foster_data", _playerData);
        }
    }
}
