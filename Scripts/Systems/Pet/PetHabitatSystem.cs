using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSystems
{
    public class PetHabitatSystem : BaseSystem
    {
        private static PetHabitatSystem _instance;
        public static PetHabitatSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("PetHabitatSystem not initialized!");
                }
                return _instance;
            }
        }
        
        public PlayerHabitatData PlayerData { get; private set; } = new PlayerHabitatData();
        
        // 信号系统
        public delegate void HabitatChangedDelegate(string habitatId);
        public delegate void DecorationPlacedDelegate(string decorationId, int slot);
        public delegate void DecorationRemovedDelegate(string decorationId, int slot);
        public delegate void ComfortChangedDelegate(int newComfort);
        public delegate void AttractionChangedDelegate(int newAttraction);
        
        public event HabitatChangedDelegate OnHabitatChanged;
        public event DecorationPlacedDelegate OnDecorationPlaced;
        public event DecorationRemovedDelegate OnDecorationRemoved;
        public event ComfortChangedDelegate OnComfortChanged;
        public event AttractionChangedDelegate OnAttractionChanged;
        
        private Random _random = new Random();
        private Player _player;
        private PetManager _petManager;
        
        public override void _Ready()
        {
            _instance = this;
            _player = GetNode<Player>("/root/Main/Player");
            _petManager = GetNode<PetManager>("/root/Main/PetManager");
            GD.Print("Pet Habitat System initialized");
        }
        
        public void Initialize()
        {
            _instance = this;
            _player = GetNode<Player>("/root/Main/Player");
            _petManager = GetNode<PetManager>("/root/Main/PetManager");
            
            // 加载保存的数据
            var saveSystem = GetNode<SaveSystem>("/root/Main/SaveSystem");
            if (saveSystem != null)
            {
                var data = saveSystem.LoadPetHabitatData();
                if (data != null)
                {
                    LoadSaveData(data);
                }
            }
            
            GD.Print("Pet Habitat System initialized");
        }
        
        /// <summary>
        /// 获取当前栖息地配置
        /// </summary>
        public HabitatConfig GetCurrentHabitat()
        {
            return PetHabitatDatabase.GetHabitat(PlayerData.CurrentHabitatId);
        }
        
        /// <summary>
        /// 获取当前已放置的装饰品数量
        /// </summary>
        public int GetPlacedDecorationCount()
        {
            return PlayerData.PlacedDecorations.Count;
        }
        
        /// <summary>
        /// 获取当前装饰品数量上限
        /// </summary>
        public int GetMaxDecorationSlots()
        {
            var habitat = GetCurrentHabitat();
            return habitat != null ? habitat.MaxSlots : 0;
        }
        
        /// <summary>
        /// 切换栖息地
        /// </summary>
        public bool ChangeHabitat(string habitatId)
        {
            var habitat = PetHabitatDatabase.GetHabitat(habitatId);
            if (habitat == null)
            {
                GD.PrintErr($"Habitat not found: {habitatId}");
                return false;
            }
            
            if (habitat.UnlockCost > 0 && PlayerData.CurrentHabitatId != habitatId)
            {
                // 需要解锁费用
                int playerGold = _player != null ? (int)_player.Gold : 0;
                if (playerGold < habitat.UnlockCost)
                {
                    GD.Print($"Not enough gold to unlock habitat: {habitat.UnlockCost} required, {playerGold} available");
                    return false;
                }
                _player.Gold -= habitat.UnlockCost;
            }
            
            PlayerData.CurrentHabitatId = habitatId;
            RecalculateStats();
            
            OnHabitatChanged?.Invoke(habitatId);
            SaveData();
            
            return true;
        }
        
        /// <summary>
        /// 放置装饰品
        /// </summary>
        public bool PlaceDecoration(string decorationId, int slot)
        {
            var decoration = PetHabitatDatabase.GetDecoration(decorationId);
            if (decoration == null)
            {
                GD.PrintErr($"Decoration not found: {decorationId}");
                return false;
            }
            
            // 检查是否有足够的金币
            int playerGold = _player != null ? (int)_player.Gold : 0;
            if (playerGold < decoration.Cost)
            {
                GD.Print($"Not enough gold to purchase decoration: {decoration.Cost} required, {playerGold} available");
                return false;
            }
            
            // 检查是否有空位
            int maxSlots = GetMaxDecorationSlots();
            if (slot < 0 || slot >= maxSlots)
            {
                GD.Print($"Invalid slot: {slot}, max is {maxSlots}");
                return false;
            }
            
            // 检查该位置是否已被占用
            if (PlayerData.PlacedDecorations.Any(d => d.Slot == slot))
            {
                GD.Print($"Slot already occupied: {slot}");
                return false;
            }
            
            // 扣除金币
            _player.Gold -= decoration.Cost;
            
            // 放置装饰品
            var placedDecoration = new PlacedDecoration
            {
                DecorationId = decorationId,
                Slot = slot,
                PlacedAt = DateTime.Now
            };
            
            PlayerData.PlacedDecorations.Add(placedDecoration);
            
            // 更新统计
            PlayerData.DecorationCounts[decorationId] = PlayerData.DecorationCounts.GetValueOrDefault(decorationId, 0) + 1;
            PlayerData.DecorationsPurchased++;
            PlayerData.GoldSpentOnDecorations += decoration.Cost;
            
            RecalculateStats();
            
            OnDecorationPlaced?.Invoke(decorationId, slot);
            SaveData();
            
            return true;
        }
        
        /// <summary>
        /// 移除装饰品
        /// </summary>
        public bool RemoveDecoration(int slot)
        {
            var placedDecoration = PlayerData.PlacedDecorations.FirstOrDefault(d => d.Slot == slot);
            if (placedDecoration == null)
            {
                GD.Print($"No decoration at slot: {slot}");
                return false;
            }
            
            var decoration = PetHabitatDatabase.GetDecoration(placedDecoration.DecorationId);
            if (decoration == null)
            {
                return false;
            }
            
            // 退还一半的金币
            int refund = decoration.Cost / 2;
            _player.Gold += refund;
            
            // 移除装饰品
            PlayerData.PlacedDecorations.Remove(placedDecoration);
            
            // 更新统计
            if (PlayerData.DecorationCounts.ContainsKey(decoration.Id))
            {
                PlayerData.DecorationCounts[decoration.Id]--;
                if (PlayerData.DecorationCounts[decoration.Id] <= 0)
                {
                    PlayerData.DecorationCounts.Remove(decoration.Id);
                }
            }
            
            RecalculateStats();
            
            OnDecorationRemoved?.Invoke(placedDecoration.DecorationId, slot);
            SaveData();
            
            return true;
        }
        
        /// <summary>
        /// 重新计算统计
        /// </summary>
        private void RecalculateStats()
        {
            int totalComfort = 0;
            int totalAttraction = 0;
            
            // 栖息地基础加成
            var habitat = GetCurrentHabitat();
            if (habitat != null)
            {
                totalComfort += habitat.ComfortBonus;
            }
            
            // 装饰品加成
            foreach (var placed in PlayerData.PlacedDecorations)
            {
                var decoration = PetHabitatDatabase.GetDecoration(placed.DecorationId);
                if (decoration != null)
                {
                    totalComfort += decoration.ComfortBonus;
                    totalAttraction += decoration.AttractionBonus;
                }
            }
            
            bool comfortChanged = PlayerData.TotalComfort != totalComfort;
            bool attractionChanged = PlayerData.TotalAttraction != totalAttraction;
            
            PlayerData.TotalComfort = totalComfort;
            PlayerData.TotalAttraction = totalAttraction;
            
            if (comfortChanged)
            {
                OnComfortChanged?.Invoke(totalComfort);
            }
            
            if (attractionChanged)
            {
                OnAttractionChanged?.Invoke(totalAttraction);
            }
        }
        
        /// <summary>
        /// 访问栖息地
        /// </summary>
        public HabitatVisitResult VisitHabitat()
        {
            var result = new HabitatVisitResult
            {
                Success = true
            };
            
            // 更新访问统计
            PlayerData.HabitatVisits++;
            PlayerData.LastVisit = DateTime.Now;
            
            // 根据舒适度获得奖励
            int comfort = PlayerData.TotalComfort;
            int attraction = PlayerData.TotalAttraction;
            
            // 舒适度奖励
            result.ComfortGained = Math.Min(comfort / 10, 20);
            result.AttractionGained = Math.Min(attraction / 5, 15);
            
            // 根据吸引力概率吸引宠物
            if (_petManager != null)
            {
                var pets = _petManager.GetAllPets();
                foreach (var pet in pets)
                {
                    // 吸引力越高，吸引概率越大
                    int attractChance = Math.Min(attraction * 2, 80);
                    if (_random.Next(100) < attractChance)
                    {
                        result.AttractedPets.Add(pet.Id);
                    }
                }
                
                result.GoldEarned = result.AttractedPets.Count * (10 + _random.Next(20));
            }
            else
            {
                result.GoldEarned = comfort + _random.Next(50);
            }
            
            // 给予金币奖励
            _player.Gold += result.GoldEarned;
            PlayerData.PetsAttracted += result.AttractedPets.Count;
            
            SaveData();
            
            return result;
        }
        
        /// <summary>
        /// 获取舒适度百分比
        /// </summary>
        public float GetComfortPercentage()
        {
            int maxComfort = 200; // 假设最大舒适度
            return (float)Math.Min(PlayerData.TotalComfort / (float)maxComfort, 1.0);
        }
        
        /// <summary>
        /// 获取吸引力百分比
        /// </summary>
        public float GetAttractionPercentage()
        {
            int maxAttraction = 150; // 假设最大吸引力
            return (float)Math.Min(PlayerData.TotalAttraction / (float)maxAttraction, 1.0);
        }
        
        /// <summary>
        /// 获取已放置的装饰品列表
        /// </summary>
        public List<PlacedDecoration> GetPlacedDecorations()
        {
            return new List<PlacedDecoration>(PlayerData.PlacedDecorations.OrderBy(d => d.Slot));
        }
        
        /// <summary>
        /// 检查是否已拥有栖息地
        /// </summary>
        public bool HasHabitat(string habitatId)
        {
            // 默认解锁 meadow，其他需要检查
            if (habitatId == "meadow") return true;
            
            // 检查是否已购买过（通过检查是否有解锁费用的栖息地记录）
            // 这里简化为只检查是否当前栖息地
            return PlayerData.CurrentHabitatId == habitatId;
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveData()
        {
            var saveSystem = GetNode<SaveSystem>("/root/Main/SaveSystem");
            if (saveSystem != null)
            {
                saveSystem.SavePetHabitatData(PlayerData);
            }
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        public void LoadSaveData(PlayerHabitatData data)
        {
            if (data == null) return;
            
            PlayerData = data;
            RecalculateStats();
            
            GD.Print($"Pet Habitat data loaded: {PlayerData.CurrentHabitatId}, {PlayerData.PlacedDecorations.Count} decorations");
        }
    }
}
