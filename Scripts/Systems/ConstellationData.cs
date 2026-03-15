using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 星座数据 - 存储星座配置信息
/// </summary>
public class ConstellationData : BaseSystem
{
    // Constellation types
    public enum ConstellationType
    {
        Fire,      // Aries, Leo, Sagittarius
        Water,     // Cancer, Scorpio, Pisces
        Earth,     // Taurus, Virgo, Capricorn
        Air,       // Gemini, Libra, Aquarius
        Light,     // Orion, Phoenix, Sirius
        Dark       // Shadow, Void, Eclipse
    }
    
    // Rarity levels
    public enum ConstellationRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    // Single constellation data
    public class Constellation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ConstellationType Type { get; set; }
        public ConstellationRarity Rarity { get; set; }
        public int Stars { get; set; } // Number of stars in constellation
        public float AttackBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float HealthBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float CriticalBonus { get; set; }
        public float EvasionBonus { get; set; }
        public float GoldBonus { get; set; }
        public float ExpBonus { get; set; }
        public int UnlockCost { get; set; }
        public int RequiredLevel { get; set; }
    }
    
    // Player's constellation progress
    public class ConstellationProgress
    {
        public string ConstellationId { get; set; }
        public bool Unlocked { get; set; }
        public int ActivatedStars { get; set; }
        public int TotalStars { get; set; }
        public DateTime UnlockTime { get; set; }
    }
    
    // Data storage
    public Dictionary<string, ConstellationProgress> UnlockedConstellations { get; set; } = new Dictionary<string, ConstellationProgress>();
    public int TotalActivationPoints { get; set; }
    public int UsedActivationPoints { get; set; }
    public int ConstellationFragments { get; set; }
    
    // Statistics
    public int TotalConstellationsUnlocked { get; set; }
    public int TotalStarsActivated { get; set; }
    public int GoldSpentOnConstellations { get; set; }
    public int FragmentsCollected { get; set; }
    
    public override void _Ready()
    {
        // Initialize data
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData() {
        var data = new Dictionary();
        
        // 序列化已解锁的星座
        var unlockedList = new List<Dictionary>();
        foreach (var kvp in UnlockedConstellations)
        {
            var constData = new Dictionary();
            constData["constellationId"] = kvp.Key;
            constData["unlocked"] = kvp.Value.Unlocked;
            constData["activatedStars"] = kvp.Value.ActivatedStars;
            constData["totalStars"] = kvp.Value.TotalStars;
            constData["unlockTime"] = kvp.Value.UnlockTime.ToString();
            unlockedList.Add(constData);
        }
        data["unlockedConstellations"] = unlockedList;
        
        data["totalActivationPoints"] = TotalActivationPoints;
        data["usedActivationPoints"] = UsedActivationPoints;
        data["constellationFragments"] = ConstellationFragments;
        data["totalConstellationsUnlocked"] = TotalConstellationsUnlocked;
        data["totalStarsActivated"] = TotalStarsActivated;
        data["goldSpentOnConstellations"] = GoldSpentOnConstellations;
        data["fragmentsCollected"] = FragmentsCollected;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data) {
        if (data == null) return;
        
        // 反序列化已解锁的星座
        UnlockedConstellations.Clear();
        if (data.Contains("unlockedConstellations"))
        {
            var unlockedList = (Godot.Array)data["unlockedConstellations"];
            foreach (Dictionary constData in unlockedList)
            {
                var progress = new ConstellationProgress();
                progress.ConstellationId = constData["constellationId"].ToString();
                progress.Unlocked = (bool)constData["unlocked"];
                progress.ActivatedStars = (int)constData["activatedStars"];
                progress.TotalStars = (int)constData["totalStars"];
                progress.UnlockTime = DateTime.Parse(constData["unlockTime"].ToString());
                UnlockedConstellations[progress.ConstellationId] = progress;
            }
        }
        
        if (data.Contains("totalActivationPoints")) TotalActivationPoints = (int)data["totalActivationPoints"];
        if (data.Contains("usedActivationPoints")) UsedActivationPoints = (int)data["usedActivationPoints"];
        if (data.Contains("constellationFragments")) ConstellationFragments = (int)data["constellationFragments"];
        if (data.Contains("totalConstellationsUnlocked")) TotalConstellationsUnlocked = (int)data["totalConstellationsUnlocked"];
        if (data.Contains("totalStarsActivated")) TotalStarsActivated = (int)data["totalStarsActivated"];
        if (data.Contains("goldSpentOnConstellations")) GoldSpentOnConstellations = (int)data["goldSpentOnConstellations"];
        if (data.Contains("fragmentsCollected")) FragmentsCollected = (int)data["fragmentsCollected"];
    }
}
