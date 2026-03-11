using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ChoiceEvents
{
    /// <summary>
    /// Choice event types
    /// </summary>
    public enum ChoiceEventType
    {
        Upgrade,           // 升级选择
        Treasure,          // 宝藏选择
        Curse,             // 诅咒选择
        Blessing,          // 祝福选择
        Merchant,          // 商人选择
        Challenge,         // 挑战选择
        Rest,              // 休息选择
        Mystery            // 神秘选择
    }

    /// <summary>
    /// Rarity levels for events
    /// </summary>
    public enum ChoiceEventRarity
    {
        Common,        // 普通
        Uncommon,      // 优秀
        Rare,          // 稀有
        Epic,          // 史诗
        Legendary      // 传说
    }

    /// <summary>
    /// Individual choice option within an event
    /// </summary>
    public class ChoiceOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ChoiceEventRarity Rarity { get; set; }
        
        // Reward types
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
        public List<string> ItemRewards { get; set; }  // Item IDs
        
        // Stat bonuses
        public int AttackBonus { get; set; }
        public int DefenseBonus { get; set; }
        public int HealthBonus { get; set; }
        public int SpeedBonus { get; set; }
        public float CritRateBonus { get; set; }
        public float CritDamageBonus { get; set; }
        
        // Special effects
        public bool IsPermanent { get; set; }  // Permanent or temporary
        
        public ChoiceOption()
        {
            ItemRewards = new List<string>();
        }
    }

    /// <summary>
    /// Active choice event instance
    /// </summary>
    public class ActiveChoiceEvent
    {
        public string EventId { get; set; }
        public ChoiceEventType EventType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<ChoiceOption> Options { get; set; }
        public int RequiredOptionCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartTime { get; set; }
        
        public ActiveChoiceEvent()
        {
            Options = new List<ChoiceOption>();
        }
    }

    /// <summary>
    /// Player's choice event data
    /// </summary>
    public class PlayerChoiceData
    {
        public int TotalEvents { get; set; }
        public int TotalChoices { get; set; }
        public Dictionary<ChoiceEventType, int> EventCounts { get; set; }
        public Dictionary<string, int> OptionCounts { get; set; }  // OptionId -> count
        public Dictionary<ChoiceEventRarity, int> RaritySelections { get; set; }
        public List<string> ChosenOptionHistory { get; set; }
        
        public PlayerChoiceData()
        {
            EventCounts = new Dictionary<ChoiceEventType, int>();
            OptionCounts = new Dictionary<string, int>();
            RaritySelections = new Dictionary<ChoiceEventRarity, int>();
            ChosenOptionHistory = new List<string>();
        }
    }

    /// <summary>
    /// Choice event database
    /// </summary>
    public class ChoiceEventDatabase
    {
        // Upgrade choices by rarity
        private Dictionary<ChoiceEventRarity, List<ChoiceOption>> _upgradeChoices;
        
        // Treasure choices
        private List<ChoiceOption> _treasureChoices;
        
        // Blessing choices
        private List<ChoiceOption> _blessingChoices;
        
        // Curse choices
        private List<ChoiceOption> _curseChoices;
        
        // Merchant choices
        private List<ChoiceOption> _merchantChoices;
        
        // Challenge choices
        private List<ChoiceOption> _challengeChoices;
        
        // Rest choices
        private List<ChoiceOption> _restChoices;
        
        // Mystery choices
        private List<ChoiceOption> _mysteryChoices;
        
        public ChoiceEventDatabase()
        {
            InitializeUpgradeChoices();
            InitializeTreasureChoices();
            InitializeBlessingChoices();
            InitializeCurseChoices();
            InitializeMerchantChoices();
            InitializeChallengeChoices();
            InitializeRestChoices();
            InitializeMysteryChoices();
        }
        
        private void InitializeUpgradeChoices()
        {
            _upgradeChoices = new Dictionary<ChoiceEventRarity, List<ChoiceOption>>();
            
            // Common upgrades
            _upgradeChoices[ChoiceEventRarity.Common] = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "upg_atk_1", Name = "力量提升", Description = "攻击+5", AttackBonus = 5, Rarity = ChoiceEventRarity.Common, IsPermanent = true },
                new ChoiceOption { Id = "upg_def_1", Name = "防御提升", Description = "防御+5", DefenseBonus = 5, Rarity = ChoiceEventRarity.Common, IsPermanent = true },
                new ChoiceOption { Id = "upg_hp_1", Name = "生命提升", Description = "生命+20", HealthBonus = 20, Rarity = ChoiceEventRarity.Common, IsPermanent = true },
                new ChoiceOption { Id = "upg_spd_1", Name = "速度提升", Description = "速度+2", SpeedBonus = 2, Rarity = ChoiceEventRarity.Common, IsPermanent = true },
            };
            
            // Uncommon upgrades
            _upgradeChoices[ChoiceEventRarity.Uncommon] = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "upg_atk_2", Name = "强效攻击", Description = "攻击+10", AttackBonus = 10, Rarity = ChoiceEventRarity.Uncommon, IsPermanent = true },
                new ChoiceOption { Id = "upg_def_2", Name = "强效防御", Description = "防御+10", DefenseBonus = 10, Rarity = ChoiceEventRarity.Uncommon, IsPermanent = true },
                new ChoiceOption { Id = "upg_hp_2", Name = "强效生命", Description = "生命+50", HealthBonus = 50, Rarity = ChoiceEventRarity.Uncommon, IsPermanent = true },
                new ChoiceOption { Id = "upg_crit_1", Name = "暴击强化", Description = "暴击率+5%", CritRateBonus = 0.05f, Rarity = ChoiceEventRarity.Uncommon, IsPermanent = true },
            };
            
            // Rare upgrades
            _upgradeChoices[ChoiceEventRarity.Rare] = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "upg_atk_3", Name = "精通攻击", Description = "攻击+20", AttackBonus = 20, Rarity = ChoiceEventRarity.Rare, IsPermanent = true },
                new ChoiceOption { Id = "upg_crit_2", Name = "暴击大师", Description = "暴击率+10%", CritRateBonus = 0.10f, Rarity = ChoiceEventRarity.Rare, IsPermanent = true },
                new ChoiceOption { Id = "upg_crit_dmg", Name = "暴击伤害", Description = "暴击伤害+20%", CritDamageBonus = 0.20f, Rarity = ChoiceEventRarity.Rare, IsPermanent = true },
            };
            
            // Epic upgrades
            _upgradeChoices[ChoiceEventRarity.Epic] = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "upg_atk_4", Name = "大师攻击", Description = "攻击+35", AttackBonus = 35, Rarity = ChoiceEventRarity.Epic, IsPermanent = true },
                new ChoiceOption { Id = "upg_allstat", Name = "全属性强化", Description = "攻击+15 防御+15 生命+30", AttackBonus = 15, DefenseBonus = 15, HealthBonus = 30, Rarity = ChoiceEventRarity.Epic, IsPermanent = true },
            };
            
            // Legendary upgrades
            _upgradeChoices[ChoiceEventRarity.Legendary] = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "upg_legendary", Name = "传奇之力", Description = "攻击+50 暴击率+15%", AttackBonus = 50, CritRateBonus = 0.15f, Rarity = ChoiceEventRarity.Legendary, IsPermanent = true },
            };
        }
        
        private void InitializeTreasureChoices()
        {
            _treasureChoices = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "treasure_gold_1", Name = "小钱袋", Description = "获得100金币", GoldReward = 100, Rarity = ChoiceEventRarity.Common },
                new ChoiceOption { Id = "treasure_gold_2", Name = "中钱袋", Description = "获得300金币", GoldReward = 300, Rarity = ChoiceEventRarity.Uncommon },
                new ChoiceOption { Id = "treasure_gold_3", Name = "大钱袋", Description = "获得500金币", GoldReward = 500, Rarity = ChoiceEventRarity.Rare },
                new ChoiceOption { Id = "treasure_gold_4", Name = "富商钱包", Description = "获得1000金币", GoldReward = 1000, Rarity = ChoiceEventRarity.Epic },
                new ChoiceOption { Id = "treasure_gold_5", Name = "龙王宝藏", Description = "获得2500金币", GoldReward = 2500, Rarity = ChoiceEventRarity.Legendary },
                new ChoiceOption { Id = "treasure_exp_1", Name = "经验卷轴", Description = "获得100经验", ExpReward = 100, Rarity = ChoiceEventRarity.Common },
                new ChoiceOption { Id = "treasure_exp_2", Name = "高级经验卷轴", Description = "获得300经验", ExpReward = 300, Rarity = ChoiceEventRarity.Uncommon },
                new ChoiceOption { Id = "treasure_exp_3", Name = "传奇经验卷轴", Description = "获得800经验", ExpReward = 800, Rarity = ChoiceEventRarity.Rare },
            };
        }
        
        private void InitializeBlessingChoices()
        {
            _blessingChoices = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "blessing_atk", Name = "攻击祝福", Description = "攻击力+20% (当前战斗)", AttackBonus = 0, Rarity = ChoiceEventRarity.Common, IsPermanent = false },
                new ChoiceOption { Id = "blessing_def", Name = "防御祝福", Description = "防御力+20% (当前战斗)", DefenseBonus = 0, Rarity = ChoiceEventRarity.Common, IsPermanent = false },
                new ChoiceOption { Id = "blessing_gold", Name = "财富祝福", Description = "金币掉落+50% (下一场战斗)", Rarity = ChoiceEventRarity.Uncommon, IsPermanent = false },
                new ChoiceOption { Id = "blessing_exp", Name = "经验祝福", Description = "经验获取+50% (下一场战斗)", Rarity = ChoiceEventRarity.Uncommon, IsPermanent = false },
                new ChoiceOption { Id = "blessing_luck", Name = "幸运祝福", Description = "暴击率+15% (当前战斗)", CritRateBonus = 0.15f, Rarity = ChoiceEventRarity.Rare, IsPermanent = false },
                new ChoiceOption { Id = "blessing Fortune", Name = "命运祝福", Description = "所有掉落+25%", Rarity = ChoiceEventRarity.Epic, IsPermanent = false },
                new ChoiceOption { Id = "blessing_ancient", Name = "远古祝福", Description = "全属性+10% (永久)", AttackBonus = 0, DefenseBonus = 0, HealthBonus = 0, SpeedBonus = 0, Rarity = ChoiceEventRarity.Legendary, IsPermanent = true },
            };
        }
        
        private void InitializeCurseChoices()
        {
            _curseChoices = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "curse_fog", Name = "迷雾诅咒", Description = "视野范围-30% (3场战斗)", Rarity = ChoiceEventRarity.Common },
                new ChoiceOption { Id = "curse_weak", Name = "虚弱诅咒", Description = "攻击力-10% (2场战斗)", Rarity = ChoiceEventRarity.Uncommon },
                new ChoiceOption { Id = "curse_greed", Name = "贪婪诅咒", Description = "商店价格+25%", Rarity = ChoiceEventRarity.Rare },
            };
        }
        
        private void InitializeMerchantChoices()
        {
            _merchantChoices = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "merchant_potion", Name = "药水商人", Description = "购买任意药水享8折", Rarity = ChoiceEventRarity.Uncommon },
                new ChoiceOption { Id = "merchant_weapon", Name = "武器商人", Description = "解锁一把随机蓝色武器", Rarity = ChoiceEventRarity.Rare },
                new ChoiceOption { Id = "merchant_armor", Name = "防具商人", Description = "解锁一件随机蓝色防具", Rarity = ChoiceEventRarity.Rare },
                new ChoiceOption { Id = "merchant_gem", Name = "宝石商人", Description = "获得3颗随机宝石", Rarity = ChoiceEventRarity.Epic },
                new ChoiceOption { Id = "merchant_legendary", Name = "神秘商人", Description = "从三件传说装备中选择一件", Rarity = ChoiceEventRarity.Legendary },
            };
        }
        
        private void InitializeChallengeChoices()
        {
            _challengeChoices = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "challenge_easy", Name = "简单挑战", Description = "击败3个敌人，获得1.5倍奖励", Rarity = ChoiceEventRarity.Common },
                new ChoiceOption { Id = "challenge_normal", Name = "普通挑战", Description = "击败5个敌人，获得2倍奖励", Rarity = ChoiceEventRarity.Uncommon },
                new ChoiceOption { Id = "challenge_hard", Name = "困难挑战", Description = "击败8个敌人，获得3倍奖励", Rarity = ChoiceEventRarity.Rare },
                new ChoiceOption { Id = "challenge_epic", Name = "史诗挑战", Description = "击败Boss，完成获得传说宝箱", Rarity = ChoiceEventRarity.Epic },
            };
        }
        
        private void InitializeRestChoices()
        {
            _restChoices = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "rest_heal", Name = "恢复生命", Description = "恢复50%最大生命", Rarity = ChoiceEventRarity.Common },
                new ChoiceOption { Id = "rest_fullheal", Name = "完全恢复", Description = "完全恢复生命值", Rarity = ChoiceEventRarity.Uncommon },
                new ChoiceOption { Id = "rest_blessing", Name = "休息祈福", Description = "恢复生命+获得随机祝福", Rarity = ChoiceEventRarity.Rare },
            };
        }
        
        private void InitializeMysteryChoices()
        {
            _mysteryChoices = new List<ChoiceOption>
            {
                new ChoiceOption { Id = "mystery_box", Name = "神秘盒子", Description = "随机获得奖励", Rarity = ChoiceEventRarity.Common },
                new ChoiceOption { Id = "mystery_chest", Name = "神秘宝箱", Description = "从3个选项中选择1个", Rarity = ChoiceEventRarity.Rare },
                new ChoiceOption { Id = "mystery_ancient", Name = "远古遗产", Description = "从5个传说奖励中选择1个", Rarity = ChoiceEventRarity.Legendary },
            };
        }
        
        /// <summary>
        /// Get random upgrade choices based on player level
        /// </summary>
        public List<ChoiceOption> GetUpgradeChoices(int count, int playerLevel)
        {
            var result = new List<ChoiceOption>();
            var random = new Random();
            
            // Determine rarity distribution based on level
            ChoiceEventRarity maxRarity = ChoiceEventRarity.Common;
            if (playerLevel >= 50) maxRarity = ChoiceEventRarity.Legendary;
            else if (playerLevel >= 40) maxRarity = ChoiceEventRarity.Epic;
            else if (playerLevel >= 25) maxRarity = ChoiceEventRarity.Rare;
            else if (playerLevel >= 10) maxRarity = ChoiceEventRarity.Uncommon;
            
            // Add options from available rarities
            for (int i = 0; i <= (int)maxRarity && result.Count < count; i++)
            {
                var rarity = (ChoiceEventRarity)i;
                if (_upgradeChoices.ContainsKey(rarity))
                {
                    var options = _upgradeChoices[rarity];
                    var selected = options[random.Next(options.Count)];
                    if (!result.Exists(o => o.Id == selected.Id))
                    {
                        result.Add(selected);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get random treasure choices
        /// </summary>
        public List<ChoiceOption> GetTreasureChoices(int count)
        {
            var result = new List<ChoiceOption>();
            var random = new Random();
            
            var shuffled = _treasureChoices.OrderBy(x => random.Next()).ToList();
            
            for (int i = 0; i < Math.Min(count, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get random blessing choices
        /// </summary>
        public List<ChoiceOption> GetBlessingChoices(int count)
        {
            var result = new List<ChoiceOption>();
            var random = new Random();
            
            var shuffled = _blessingChoices.OrderBy(x => random.Next()).ToList();
            
            for (int i = 0; i < Math.Min(count, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get random curse choices
        /// </summary>
        public List<ChoiceOption> GetCurseChoices(int count)
        {
            var result = new List<ChoiceOption>();
            var random = new Random();
            
            var shuffled = _curseChoices.OrderBy(x => random.Next()).ToList();
            
            for (int i = 0; i < Math.Min(count, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get random merchant choices
        /// </summary>
        public List<ChoiceOption> GetMerchantChoices(int count)
        {
            var result = new List<ChoiceOption>();
            var random = new Random();
            
            var shuffled = _merchantChoices.OrderBy(x => random.Next()).ToList();
            
            for (int i = 0; i < Math.Min(count, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get random challenge choices
        /// </summary>
        public List<ChoiceOption> GetChallengeChoices(int count)
        {
            var result = new List<ChoiceOption>();
            var random = new Random();
            
            var shuffled = _challengeChoices.OrderBy(x => random.Next()).ToList();
            
            for (int i = 0; i < Math.Min(count, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get random rest choices
        /// </summary>
        public List<ChoiceOption> GetRestChoices(int count)
        {
            var result = new List<ChoiceOption>();
            var random = new Random();
            
            var shuffled = _restChoices.OrderBy(x => random.Next()).ToList();
            
            for (int i = 0; i < Math.Min(count, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get random mystery choices
        /// </summary>
        public List<ChoiceOption> GetMysteryChoices(int count)
        {
            var result = new List<ChoiceOption>();
            var random = new Random();
            
            var shuffled = _mysteryChoices.OrderBy(x => random.Next()).ToList();
            
            for (int i = 0; i < Math.Min(count, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }
            
            return result;
        }
    }
}
