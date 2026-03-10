using Godot;
using System;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物数据类型
    /// </summary>
    [GlobalClass]
    public partial class Pet : Resource
    {
        [Export] public string PetId { get; set; } = "";
        [Export] public string PetName { get; set; } = "";
        [Export] public PetType Type { get; set; } = PetType.Companion;
        [Export] public PetRarity Rarity { get; set; } = PetRarity.Common;
        [Export] public Texture2D Icon { get; set; }
        
        // 属性加成
        [Export] public int HealthBonus { get; set; } = 0;
        [Export] public int AttackBonus { get; set; } = 0;
        [Export] public int DefenseBonus { get; set; } = 0;
        [Export] public int SpeedBonus { get; set; } = 0;
        [Export] public int CriticalBonus { get; set; } = 0;
        
        // 特殊效果
        [Export] public string SpecialAbility { get; set; } = ""; // 自动拾取/经验加成/掉落加成
        [Export] public float SpecialValue { get; set; } = 0f;
        
        // 等级和经验
        [Export] public int Level { get; set; } = 1;
        [Export] public int Experience { get; set; } = 0;
        [Export] public int ExperienceToNextLevel { get; set; } = 100;
        
        // 忠诚度
        [Export] public int Loyalty { get; set; } = 50; // 0-100
        
        // 获取总属性加成（基于等级和忠诚度）
        public int GetTotalHealthBonus() => (int)(HealthBonus * GetBonusMultiplier());
        public int GetTotalAttackBonus() => (int)(AttackBonus * GetBonusMultiplier());
        public int GetTotalDefenseBonus() => (int)(DefenseBonus * GetBonusMultiplier());
        public int GetTotalSpeedBonus() => (int)(SpeedBonus * GetBonusMultiplier());
        public int GetTotalCriticalBonus() => (int)(CriticalBonus * GetBonusMultiplier());
        
        private float GetBonusMultiplier()
        {
            float levelMultiplier = 1f + (Level - 1) * 0.1f;
            float loyaltyMultiplier = 0.5f + (Loyalty / 100f) * 0.5f;
            return levelMultiplier * loyaltyMultiplier;
        }
        
        public void AddExperience(int amount)
        {
            Experience += amount;
            while (Experience >= ExperienceToNextLevel && Level < 100)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                ExperienceToNextLevel = (int)(ExperienceToNextLevel * 1.5f);
            }
        }
        
        public void AddLoyalty(int amount)
        {
            Loyalty = Mathf.Clamp(Loyalty + amount, 0, 100);
        }
    }

    public enum PetType
    {
        Companion,      // 伙伴 - 战斗辅助
        Collector,      // 收藏家 - 自动拾取
        Guardian,       // 守护者 - 战斗保护
        Explorer        // 探险家 - 探索加成
    }

    public enum PetRarity
    {
        Common,     // 普通 - 白色
        Uncommon,   // 优秀 - 绿色
        Rare,       // 稀有 - 蓝色
        Epic,       // 史诗 - 紫色
        Legendary   // 传说 - 橙色
    }
}
