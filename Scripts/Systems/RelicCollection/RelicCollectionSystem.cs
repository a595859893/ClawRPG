// ============================================
// Relic Collection System - 遗物收集系统核心
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClawRPG.Scripts.Systems
{
    public class RelicCollectionSystem : BaseSystem
    {
        // 玩家遗物收集数据
        private PlayerRelicCollection _playerCollection = new PlayerRelicCollection
        {
            Relics = new Dictionary<string, PlayerRelicData>(),
            EquippedRelics = new List<string>(),
            SetCompletions = new Dictionary<string, int>()
        };

        // 当前房间的遗物
        private List<Relic> _currentRoomRelics = new List<Relic>();

        // 事件
        public event Action<string> OnRelicUnlocked;
        public event Action<string> OnRelicEquipped;
        public event Action<string> OnRelicUnequipped;
        public event Action<string> OnSetCompleted;
        public event Action<Relic> OnRelicGenerated;

        // 最大装备数量
        private const int MaxEquippedRelics = 6;

        // 初始化玩家数据
        protected override void Initialize()
        {
            // 加载圣物配置
            if (!RelicConfigLoader.IsLoaded)
            {
                if (!RelicConfigLoader.Load())
                {
                    GD.PrintErr("[RelicCollectionSystem] 警告: 圣物配置加载失败，将使用空配置");
                }
            }

            // 解锁所有普通遗物作为起始遗物
            foreach (var relic in RelicConfigLoader.Relics.Values)
            {
                if (relic.Rarity == RelicRarity.Common && !_playerCollection.Relics.ContainsKey(relic.Id))
                {
                    UnlockRelic(relic.Id);
                }
            }
        }

        // 解锁遗物
        public bool UnlockRelic(string relicId)
        {
            if (_playerCollection.Relics.ContainsKey(relicId))
            {
                _playerCollection.Relics[relicId].Unlocked = true;
                OnRelicUnlocked?.Invoke(relicId);
                return true;
            }

            if (RelicConfigLoader.Relics.ContainsKey(relicId))
            {
                var relicData = new PlayerRelicData
                {
                    RelicId = relicId,
                    Unlocked = true,
                    CurrentLevel = 1,
                    MaxLevel = 10,
                    Equipped = false
                };
                _playerCollection.Relics[relicId] = relicData;
                _playerCollection.TotalRelicsUnlocked++;
                OnRelicUnlocked?.Invoke(relicId);
                return true;
            }

            return false;
        }

        // 装备遗物
        public bool EquipRelic(string relicId)
        {
            if (!_playerCollection.Relics.ContainsKey(relicId))
                return false;

            if (_playerCollection.Relics[relicId].Equipped)
                return true;

            if (_playerCollection.EquippedRelics.Count >= MaxEquippedRelics)
                return false;

            _playerCollection.Relics[relicId].Equipped = true;
            _playerCollection.EquippedRelics.Add(relicId);
            
            CheckSetCompletion();
            OnRelicEquipped?.Invoke(relicId);
            return true;
        }

        // 卸下遗物
        public bool UnequipRelic(string relicId)
        {
            if (!_playerCollection.Relics.ContainsKey(relicId))
                return false;

            if (!_playerCollection.Relics[relicId].Equipped)
                return true;

            _playerCollection.Relics[relicId].Equipped = false;
            _playerCollection.EquippedRelics.Remove(relicId);
            
            OnRelicUnequipped?.Invoke(relicId);
            return true;
        }

        // 生成房间遗物
        public List<Relic> GenerateRoomRelics(int floorNumber)
        {
            _currentRoomRelics.Clear();
            var config = RelicConfigLoader.GenerationConfig;
            var random = new Random();
            
            int relicCount = random.Next(config.MinRelicsPerFloor, config.MaxRelicsPerFloor + 1);
            
            // 根据楼层增加稀有度概率
            var floorMultiplier = 1.0 + (floorNumber * 0.05);
            
            for (int i = 0; i < relicCount; i++)
            {
                var relic = GenerateRelicForFloor(floorNumber);
                if (relic != null)
                {
                    _currentRoomRelics.Add(relic);
                    OnRelicGenerated?.Invoke(relic);
                }
            }

            return _currentRoomRelics;
        }

        // 生成特定楼层的遗物
        private Relic GenerateRelicForFloor(int floorNumber)
        {
            var random = new Random();
            var roll = random.NextDouble();
            
            // 楼层越高，高稀有度概率越大
            double mythBonus = Math.Min(floorNumber * 0.002, 0.02);
            double legBonus = Math.Min(floorNumber * 0.005, 0.05);
            double epicBonus = Math.Min(floorNumber * 0.01, 0.10);
            
            RelicRarity rarity;
            double adjustedMythic = RelicConfigLoader.GenerationConfig.MythicChance + mythBonus;
            double adjustedLegendary = RelicConfigLoader.GenerationConfig.LegendaryChance + legBonus;
            double adjustedEpic = RelicConfigLoader.GenerationConfig.EpicChance + epicBonus;
            
            if (roll < adjustedMythic)
                rarity = RelicRarity.Mythic;
            else if (roll < adjustedMythic + adjustedLegendary)
                rarity = RelicRarity.Legendary;
            else if (roll < adjustedMythic + adjustedLegendary + adjustedEpic)
                rarity = RelicRarity.Epic;
            else if (roll < adjustedMythic + adjustedLegendary + adjustedEpic + RelicConfigLoader.GenerationConfig.RareChance)
                rarity = RelicRarity.Rare;
            else if (roll < adjustedMythic + adjustedLegendary + adjustedEpic + RelicConfigLoader.GenerationConfig.RareChance + RelicConfigLoader.GenerationConfig.UncommonChance)
                rarity = RelicRarity.Uncommon;
            else
                rarity = RelicRarity.Common;

            // 获取该稀有度的遗物
            var relicsOfRarity = RelicConfigLoader.Relics.Values
                .Where(r => r.Rarity == rarity && !_playerCollection.Relics.ContainsKey(r.Id) || 
                           (_playerCollection.Relics.ContainsKey(r.Id) && !_playerCollection.Relics[r.Id].Unlocked))
                .ToList();

            if (relicsOfRarity.Count == 0)
            {
                // 如果没有未解锁的，降级获取
                relicsOfRarity = RelicConfigLoader.Relics.Values
                    .Where(r => r.Rarity <= rarity)
                    .ToList();
            }

            if (relicsOfRarity.Count > 0)
                return relicsOfRarity[random.Next(relicsOfRarity.Count)];
            
            return null;
        }

        // 检查并更新套装完成状态
        private void CheckSetCompletion()
        {
            var equippedSets = new Dictionary<string, int>();
            
            foreach (var relicId in _playerCollection.EquippedRelics)
            {
                if (RelicConfigLoader.Relics.TryGetValue(relicId, out var relic) && 
                    !string.IsNullOrEmpty(relic.SetId))
                {
                    if (!equippedSets.ContainsKey(relic.SetId))
                        equippedSets[relic.SetId] = 0;
                    equippedSets[relic.SetId]++;
                }
            }

            foreach (var set in equippedSets)
            {
                if (RelicConfigLoader.RelicSets.TryGetValue(set.Key, out var setConfig))
                {
                    if (set.Value >= setConfig.RequiredCount && 
                        (!_playerCollection.SetCompletions.ContainsKey(set.Key) || 
                         _playerCollection.SetCompletions[set.Key] < set.Value))
                    {
                        _playerCollection.SetCompletions[set.Key] = set.Value;
                        OnSetCompleted?.Invoke(set.Key);
                    }
                }
            }
        }

        // 获取遗物属性加成
        public Dictionary<RelicEffectType, double> GetEquippedRelicBonuses()
        {
            var bonuses = new Dictionary<RelicEffectType, double>();
            
            foreach (var relicId in _playerCollection.EquippedRelics)
            {
                if (!RelicConfigLoader.Relics.TryGetValue(relicId, out var relic))
                    continue;

                var relicData = _playerCollection.Relics[relicId];
                var levelBonus = 1.0 + (relicData.CurrentLevel - 1) * 0.1;

                // 主属性
                if (!bonuses.ContainsKey(relic.PrimaryEffect))
                    bonuses[relic.PrimaryEffect] = 0;
                bonuses[relic.PrimaryEffect] += relic.PrimaryEffectValue * levelBonus;

                // 副属性
                if (relic.SecondaryEffect.HasValue && relic.SecondaryEffectValue.HasValue)
                {
                    if (!bonuses.ContainsKey(relic.SecondaryEffect.Value))
                        bonuses[relic.SecondaryEffect.Value] = 0;
                    bonuses[relic.SecondaryEffect.Value] += relic.SecondaryEffectValue.Value * levelBonus;
                }
            }

            // 套装加成
            foreach (var set in _playerCollection.SetCompletions)
            {
                if (RelicConfigLoader.RelicSets.TryGetValue(set.Key, out var setConfig))
                {
                    if (!bonuses.ContainsKey(setConfig.SetEffect))
                        bonuses[setConfig.SetEffect] = 0;
                    bonuses[setConfig.SetEffect] += setConfig.SetEffectValue;
                }
            }

            return bonuses;
        }

        // 升级遗物
        public bool UpgradeRelic(string relicId)
        {
            if (!_playerCollection.Relics.ContainsKey(relicId))
                return false;

            var relicData = _playerCollection.Relics[relicId];
            if (relicData.CurrentLevel >= relicData.MaxLevel)
                return false;

            relicData.CurrentLevel++;
            return true;
        }

        // 获取玩家收集数据
        public PlayerRelicCollection GetPlayerCollection() => _playerCollection;

        // 获取统计信息
        public RelicStatistics GetStatistics()
        {
            var stats = new RelicStatistics
            {
                TotalRelicsUnlocked = _playerCollection.Relics.Count(r => r.Value.Unlocked),
                TotalRelicsEquipped = _playerCollection.EquippedRelics.Count,
                UnlockedByRarity = new Dictionary<RelicRarity, int>(),
                UnlockedByType = new Dictionary<RelicType, int>(),
                SetsCompleted = _playerCollection.SetCompletions.Count,
                TotalRelicLevels = _playerCollection.Relics.Sum(r => r.Value.CurrentLevel)
            };

            foreach (RelicRarity rarity in Enum.GetValues(typeof(RelicRarity)))
            {
                stats.UnlockedByRarity[rarity] = _playerCollection.Relics.Count(r => 
                    r.Value.Unlocked && RelicConfigLoader.Relics.TryGetValue(r.Key, out var relic) && relic.Rarity == rarity);
            }

            foreach (RelicType type in Enum.GetValues(typeof(RelicType)))
            {
                stats.UnlockedByType[type] = _playerCollection.Relics.Count(r => 
                    r.Value.Unlocked && RelicConfigLoader.Relics.TryGetValue(r.Key, out var relic) && relic.Type == type);
            }

            return stats;
        }

        // 导出存档数据
        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                ["Relics"] = _playerCollection.Relics,
                ["EquippedRelics"] = _playerCollection.EquippedRelics,
                ["SetCompletions"] = _playerCollection.SetCompletions,
                ["TotalRelicsUnlocked"] = _playerCollection.TotalRelicsUnlocked
            };
        }

        // 导入存档数据
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            if (data.ContainsKey("Relics"))
            {
                _playerCollection.Relics = data["Relics"] as Dictionary<string, PlayerRelicData> ?? new Dictionary<string, PlayerRelicData>();
            }
            if (data.ContainsKey("EquippedRelics"))
            {
                _playerCollection.EquippedRelics = data["EquippedRelics"] as List<string> ?? new List<string>();
            }
            if (data.ContainsKey("SetCompletions"))
            {
                _playerCollection.SetCompletions = data["SetCompletions"] as Dictionary<string, int> ?? new Dictionary<string, int>();
            }
            if (data.ContainsKey("TotalRelicsUnlocked"))
            {
                _playerCollection.TotalRelicsUnlocked = Convert.ToInt32(data["TotalRelicsUnlocked"]);
            }
        }
    }
}


// ============================================
// Relic Collection System - 遗物收集系统数据定义
// ============================================

namespace ClawRPG.Systems.Relics
{
    // 遗物稀有度
    public enum RelicRarity
    {
        Common = 1,      // 普通
        Uncommon = 2,    // 优秀
        Rare = 3,        // 稀有
        Epic = 4,        // 史诗
        Legendary = 5,   // 传说
        Mythic = 6       // 神器
    }

    // 遗物类型
    public enum RelicType
    {
        Weapon,      // 武器
        Armor,       // 护甲
        Accessory,   // 饰品
        Passive,     // 被遗物
        Trigger,     // 触发遗物
        Set          // 套装遗物
    }

    // 遗物效果类型
    public enum RelicEffectType
    {
        DamageIncrease,      // 伤害增加
        DamageReduction,    // 伤害减免
        CriticalRate,       // 暴击率
        CriticalDamage,     // 暴击伤害
        AttackSpeed,        // 攻击速度
        MoveSpeed,          // 移动速度
        HealthMax,          // 最大生命
        ManaMax,            // 最大法力
        HealthRegen,        // 生命恢复
        ManaRegen,          // 法力恢复
        LifeSteal,          // 生命偷取
        CooldownReduction,  // 冷却缩减
        ElementalDamage,    // 元素伤害
        ElementalResist,    // 元素抗性
        GoldGain,           // 金币获取
        ExperienceGain,     // 经验获取
        DropRate,           // 掉落率
        EnemyScale,         // 敌人规模
        RoomReward          // 房间奖励
    }

    // 遗物数据
    public class Relic
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RelicType Type { get; set; }
        public RelicRarity Rarity { get; set; }
        public RelicEffectType PrimaryEffect { get; set; }
        public double PrimaryEffectValue { get; set; }
        public RelicEffectType? SecondaryEffect { get; set; }
        public double? SecondaryEffectValue { get; set; }
        public string SetId { get; set; }
        public int Level { get; set; }
    }

    // 遗物套装
    public class RelicSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredCount { get; set; }
        public RelicEffectType SetEffect { get; set; }
        public double SetEffectValue { get; set; }
    }

    // 玩家遗物数据
    public class PlayerRelicData
    {
        public string RelicId { get; set; }
        public bool Unlocked { get; set; }
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public bool Equipped { get; set; }
    }

    // 玩家遗物收集数据
    public class PlayerRelicCollection
    {
        public Dictionary<string, PlayerRelicData> Relics { get; set; }
        public List<string> EquippedRelics { get; set; }
        public Dictionary<string, int> SetCompletions { get; set; }
        public int TotalRelicsUnlocked { get; set; }
    }

    // 遗物统计
    public class RelicStatistics
    {
        public int TotalRelicsUnlocked { get; set; }
        public int TotalRelicsEquipped { get; set; }
        public Dictionary<RelicRarity, int> UnlockedByRarity { get; set; }
        public Dictionary<RelicType, int> UnlockedByType { get; set; }
        public int SetsCompleted { get; set; }
        public int TotalRelicLevels { get; set; }
    }

    // 遗物生成配置
    public class RelicGenerationConfig
    {
        public int MinRelicsPerFloor { get; set; }
        public int MaxRelicsPerFloor { get; set; }
        public double CommonChance { get; set; }
        public double UncommonChance { get; set; }
        public double RareChance { get; set; }
        public double EpicChance { get; set; }
        public double LegendaryChance { get; set; }
        public double MythicChance { get; set; }
    }
}
