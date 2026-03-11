using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems
{
    /// <summary>
    /// 栖息地类型
    /// </summary>
    public enum HabitatType
    {
        Forest,
        Meadow,
        Mountain,
        Lake,
        Desert,
        Jungle,
        Tundra,
        Volcanic
    }
    
    /// <summary>
    /// 装饰品类型
    /// </summary>
    public enum DecorationType
    {
        Plant,
        Structure,
        WaterFeature,
        Lighting,
        Toy,
        FoodStation,
        Bed,
        Decorative
    }
    
    /// <summary>
    /// 栖息地配置
    /// </summary>
    public class HabitatConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public HabitatType Type { get; set; }
        public int MaxSlots { get; set; }
        public int UnlockCost { get; set; }
        public int ComfortBonus { get; set; }
    }
    
    /// <summary>
    /// 装饰品配置
    /// </summary>
    public class DecorationConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DecorationType Type { get; set; }
        public int Cost { get; set; }
        public int ComfortBonus { get; set; }
        public int AttractionBonus { get; set; }
        public string Icon { get; set; }
    }
    
    /// <summary>
    /// 放置的装饰品
    /// </summary>
    public class PlacedDecoration
    {
        public string DecorationId { get; set; }
        public int Slot { get; set; }
        public DateTime PlacedAt { get; set; }
    }
    
    /// <summary>
    /// 玩家栖息地数据
    /// </summary>
    public class PlayerHabitatData
    {
        public string CurrentHabitatId { get; set; } = "meadow";
        public List<PlacedDecoration> PlacedDecorations { get; set; } = new List<PlacedDecoration>();
        public int TotalComfort { get; set; }
        public int TotalAttraction { get; set; }
        public Dictionary<string, int> DecorationCounts { get; set; } = new Dictionary<string, int>();
        
        // 统计
        public int DecorationsPurchased { get; set; }
        public int GoldSpentOnDecorations { get; set; }
        public int HabitatVisits { get; set; }
        public int PetsAttracted { get; set; }
        public DateTime LastVisit { get; set; }
    }
    
    /// <summary>
    /// 栖息地访问结果
    /// </summary>
    public class HabitatVisitResult
    {
        public bool Success { get; set; }
        public int ComfortGained { get; set; }
        public int AttractionGained { get; set; }
        public List<string> AttractedPets { get; set; } = new List<string>();
        public int GoldEarned { get; set; }
    }
}
