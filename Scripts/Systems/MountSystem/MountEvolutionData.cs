using Godot;
using System;
using System.Collections.Generic;

public class MountEvolutionData
{
    public enum EvolutionStage
    {
        Basic = 0,
        Advanced = 1,
        Elite = 2,
        Epic = 3,
        Legendary = 4
    }

    public enum EvolutionType
    {
        Fire,      // 火系进化 - 攻击型
        Ice,       // 冰系进化 - 控制型
        Lightning, // 雷系进化 - 速度型
        Dark,      // 暗系进化 - 爆发型
        Holy,      // 光系进化 - 辅助型
        Nature     // 自然系进化 - 防御型
    }

    /// <summary>
    /// 坐骑进化配置数据
    /// </summary>
    public class MountEvolutionConfig
    {
        public string MountId { get; set; }
        public string BaseMountName { get; set; }
        public EvolutionStage Stage { get; set; }
        public EvolutionType Type { get; set; }
        public string EvolutionName { get; set; }
        public string Description { get; set; }
        public int RequiredLevel { get; set; }
        public int RequiredBattleExp { get; set; }
        public int RequiredItemId { get; set; }
        public int RequiredItemCount { get; set; }
        public int GoldCost { get; set; }
        
        // 进化后属性加成 (百分比)
        public float HealthBonus { get; set; }
        public float AttackBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float SpeedBonus { get; set; }
        
        // 特殊能力
        public List<string> UnlockSkills { get; set; }
        
        // 外观变化
        public string TextureOverride { get; set; }
        public Color TintColor { get; set; }
        
        public MountEvolutionConfig()
        {
            UnlockSkills = new List<string>();
            TintColor = Colors.White;
        }
    }

    /// <summary>
    /// 玩家坐骑进化数据
    /// </summary>
    public class PlayerMountEvolution
    {
        public string MountId { get; set; }
        public EvolutionStage CurrentStage { get; set; }
        public EvolutionType EvolvedType { get; set; }
        public int TotalBattleExp { get; set; }
        public int EvolutionCount { get; set; }
        public DateTime LastEvolutionTime { get; set; }
        
        public PlayerMountEvolution()
        {
            CurrentStage = EvolutionStage.Basic;
            TotalBattleExp = 0;
            EvolutionCount = 0;
            LastEvolutionTime = DateTime.MinValue;
        }
        
        public bool CanEvolve(MountEvolutionConfig config)
        {
            if (CurrentStage >= EvolutionStage.Legendary) return false;
            if (Player.Instance.Level < config.RequiredLevel) return false;
            if (TotalBattleExp < config.RequiredBattleExp) return false;
            if (config.RequiredItemId > 0)
            {
                var item = InventorySystem.Instance.GetItem(config.RequiredItemId);
                if (item == null || item.Quantity < config.RequiredItemCount) return false;
            }
            if (Player.Instance.Gold < config.GoldCost) return false;
            return true;
        }
    }
}
