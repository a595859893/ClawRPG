using Godot;
using System;
using System.Collections.Generic;

public class LuckModifierTemplate
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int BaseValue { get; set; } = 0;
    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 0;
    public string Category { get; set; } = "";  // "item", "buff", "curse", "zone", "weather", "time"
    public int DefaultDuration { get; set; } = 0;  // 秒，0表示永久
}

public class LuckZone
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int LuckModifier { get; set; } = 0;
    public string Biome { get; set; } = "";
    public bool IsPositive { get; set; } = true;
}

public class LuckWeather
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public int LuckModifier { get; set; } = 0;
    public bool IsPositive { get; set; } = true;
}

public static class LuckDatabase
{
    private static List<LuckModifierTemplate> _templates = new List<LuckModifierTemplate>();
    private static List<LuckZone> _zones = new List<LuckZone>();
    private static List<LuckWeather> _weather = new List<LuckWeather>();
    
    public static void Initialize()
    {
        // 物品增益
        _templates.Add(new LuckModifierTemplate
        {
            ID = "lucky_charm",
            Name = "幸运护符",
            Description = "增加少量幸运",
            BaseValue = 10,
            MinValue = 5,
            MaxValue = 15,
            Category = "item",
            DefaultDuration = 0
        });
        
        _templates.Add(new LuckModifierTemplate
        {
            ID = "four_leaf_clover",
            Name = "四叶草",
            Description = "显著增加幸运",
            BaseValue = 25,
            MinValue = 20,
            MaxValue = 30,
            Category = "item",
            DefaultDuration = 300
        });
        
        _templates.Add(new LuckModifierTemplate
        {
            ID = "horseshoe",
            Name = "马蹄铁",
            Description = "中等幸运加成",
            BaseValue = 15,
            MinValue = 10,
            MaxValue = 20,
            Category = "item",
            DefaultDuration = 0
        });
        
        // Buffs
        _templates.Add(new LuckModifierTemplate
        {
            ID = "blessing_of_fortune",
            Name = "命运的祝福",
            Description = "大幅增加幸运",
            BaseValue = 50,
            MinValue = 40,
            MaxValue = 60,
            Category = "buff",
            DefaultDuration = 180
        });
        
        _templates.Add(new LuckModifierTemplate
        {
            ID = "momentum_boost",
            Name = "势头正盛",
            Description = "战斗连胜带来的幸运",
            BaseValue = 5,
            MinValue = 3,
            MaxValue = 10,
            Category = "buff",
            DefaultDuration = 60
        });
        
        // Curses
        _templates.Add(new LuckModifierTemplate
        {
            ID = "cursed",
            Name = "被诅咒",
            Description = "减少幸运",
            BaseValue = -15,
            MinValue = -25,
            MaxValue = -10,
            Category = "curse",
            DefaultDuration = 300
        });
        
        _templates.Add(new LuckModifierTemplate
        {
            ID = "shadow_touch",
            Name = "暗影之触",
            Description = "减少大量幸运",
            BaseValue = -30,
            MinValue = -40,
            MaxValue = -20,
            Category = "curse",
            DefaultDuration = 600
        });
        
        // Zone modifiers
        _zones.Add(new LuckZone
        {
            ID = "shrine_of_luck",
            Name = "幸运之神殿",
            Description = "古老的幸运之神殿",
            LuckModifier = 30,
            Biome = "temple",
            IsPositive = true
        });
        
        _zones.Add(new LuckZone
        {
            ID = "cursed_forest",
            Name = "被诅咒的森林",
            Description = "充满负面能量的森林",
            LuckModifier = -20,
            Biome = "forest",
            IsPositive = false
        });
        
        _zones.Add(new LuckZone
        {
            ID = "golden_grove",
            Name = "金色林地",
            Description = "传说中精灵的圣地",
            LuckModifier = 25,
            Biome = "forest",
            IsPositive = true
        });
        
        _zones.Add(new LuckZone
        {
            ID = "abandoned_mine",
            Name = "废弃矿井",
            Description = "曾经繁华的矿井",
            LuckModifier = 15,
            Biome = "cave",
            IsPositive = true
        });
        
        _zones.Add(new LuckZone
        {
            ID = "battlefield",
            Name = "古战场",
            Description = "无数战士陨落的地方",
            LuckModifier = -10,
            Biome = "plain",
            IsPositive = false
        });
        
        // Weather modifiers
        _weather.Add(new LuckWeather
        {
            ID = "clear_sky",
            Name = "晴朗天空",
            LuckModifier = 10,
            IsPositive = true
        });
        
        _weather.Add(new LuckWeather
        {
            ID = "rain",
            Name = "下雨",
            LuckModifier = 5,
            IsPositive = true
        });
        
        _weather.Add(new LuckWeather
        {
            ID = "storm",
            Name = "暴风雨",
            LuckModifier = -10,
            IsPositive = false
        });
        
        _weather.Add(new LuckWeather
        {
            ID = "fog",
            Name = "大雾",
            LuckModifier = -5,
            IsPositive = false
        });
        
        _weather.Add(new LuckWeather
        {
            ID = "snow",
            Name = "下雪",
            LuckModifier = 15,
            IsPositive = true
        });
    }
    
    public static List<LuckModifierTemplate> GetTemplates() => _templates;
    public static List<LuckModifierTemplate> GetTemplatesByCategory(string category) 
        => _templates.FindAll(t => t.Category == category);
    
    public static List<LuckZone> GetZones() => _zones;
    public static List<LuckWeather> GetWeather() => _weather;
    
    public static LuckModifierTemplate GetTemplate(string id)
        => _templates.Find(t => t.ID == id);
    
    public static LuckZone GetZone(string id)
        => _zones.Find(z => z.ID == id);
    
    public static LuckWeather GetWeather(string id)
        => _weather.Find(w => w.ID == id);
}
