using Godot;
using System.Collections.Generic;

public class PetLifeCycleDatabase : BaseSystem
{
    // 宠物类型基础寿命配置
    public static Dictionary<string, PetTypeLifeConfig> PetTypeConfigs = new Dictionary<string, PetTypeLifeConfig>()
    {
        // 普通宠物 - 较短寿命
        {"Dog", new PetTypeLifeConfig(80, 0.8f, 1.2f)},
        {"Cat", new PetTypeLifeConfig(90, 0.9f, 1.1f)},
        {"Rabbit", new PetTypeLifeConfig(60, 0.7f, 1.3f)},
        {"Bird", new PetTypeLifeConfig(50, 0.6f, 1.4f)},
        
        // 稀有宠物 - 中等寿命
        {"Wolf", new PetTypeLifeConfig(120, 1.0f, 1.0f)},
        {"Bear", new PetTypeLifeConfig(150, 1.2f, 0.9f)},
        {"Fox", new PetTypeLifeConfig(100, 0.95f, 1.05f)},
        
        // 史诗宠物 - 较长寿命
        {"Dragon", new PetTypeLifeConfig(500, 2.0f, 0.5f)},
        {"Phoenix", new PetTypeLifeConfig(1000, 3.0f, 0.3f)},
        {"Golem", new PetTypeLifeConfig(800, 2.5f, 0.4f)},
        {"Elemental", new PetTypeLifeConfig(600, 2.0f, 0.5f)},
        
        // 传说宠物 - 极长寿命
        {"Celestial", new PetTypeLifeConfig(2000, 4.0f, 0.2f)},
        {"Mythical", new PetTypeLifeConfig(1500, 3.5f, 0.25f)},
        {"Ancient", new PetTypeLifeConfig(3000, 5.0f, 0.15f)},
    };
    
    // 默认配置
    public static PetTypeLifeConfig DefaultConfig = new PetTypeLifeConfig(100, 1.0f, 1.0f);
    
    // 生命周期阶段配置
    public static Dictionary<LifeStage, LifeStageConfig> StageConfigs = new Dictionary<LifeStage, LifeStageConfig>()
    {
        {LifeStage.Baby, new LifeStageConfig(10, 0.5f, 0.5f, 0.5f, "婴儿期", "宠物非常年幼,需要额外照顾")},
        {LifeStage.Young, new LifeStageConfig(20, 0.75f, 0.75f, 0.75f, "幼年期", "宠物正在成长,学习能力很强")},
        {LifeStage.Adult, new LifeStageConfig(40, 1.0f, 1.0f, 1.0f, "成年期", "宠物处于巅峰状态")},
        {LifeStage.Senior, new LifeStageConfig(20, 0.8f, 0.9f, 0.85f, "老年期", "宠物开始衰老,需要更多照顾")},
        {LifeStage.Final, new LifeStageConfig(10, 0.5f, 0.6f, 0.6f, "临终期", "宠物即将离世,珍惜最后的时光")},
        {LifeStage.Immortal, new LifeStageConfig(-1, 1.5f, 1.5f, 1.5f, "不朽", "宠物超越了生死的界限")},
    };
    
    // 生命延续道具
    public static List<LifeExtensionItem> LifeExtensionItems = new List<LifeExtensionItem>()
    {
        new LifeExtensionItem("life_potion_small", "小型生命药水", 100, 10),
        new LifeExtensionItem("life_potion_medium", "中型生命药水", 500, 25),
        new LifeExtensionItem("life_potion_large", "大型生命药水", 2000, 50),
        new LifeExtensionItem("life_elixir", "生命灵药", 5000, 100),
        new LifeExtensionItem("immortal_essence", "不朽精华", 20000, 999),
    };
    
    // 阶段变化事件
    public static Dictionary<LifeStage, List<string>> StageChangeEvents = new Dictionary<LifeStage, List<string>>
    {
        {LifeStage.Baby, new List<string> {
            "宠物睁开了眼睛,好奇地看着你",
            "宠物发出可爱的叫声",
            "宠物学会了摇尾巴"
        }},
        {LifeStage.Young, new List<string> {
            "宠物开始活泼地跑来跑去",
            "宠物的毛发变得更加光亮",
            "宠物开始学习新技巧"
        }},
        {LifeStage.Adult, new List<string> {
            "宠物达到了巅峰状态",
            "宠物的身姿更加矫健",
            "宠物展现出强大的力量"
        }},
        {LifeStage.Senior, new List<string> {
            "宠物的动作变得缓慢",
            "宠物的毛发开始变白",
            "宠物更喜欢静静地陪伴"
        }},
        {LifeStage.Final, new List<string> {
            "宠物静静地躺在你身边",
            "宠物的呼吸变得微弱",
            "宠物用最后的目光看着你"
        }},
    };
}

public class PetTypeLifeConfig
{
    public int BaseMaxAge;
    public float StatMultiplier;
    public float GrowthRate;
    
    public PetTypeLifeConfig(int baseMaxAge, float statMultiplier, float growthRate)
    {
        BaseMaxAge = baseMaxAge;
        StatMultiplier = statMultiplier;
        GrowthRate = growthRate;
    }
}

public class LifeStageConfig
{
    public int Duration; // 天数,-1表示无限
    public float AttackBonus;
    public float DefenseBonus;
    public float OverallBonus;
    public string StageName;
    public string Description;
    
    public LifeStageConfig(int duration, float attackBonus, float defenseBonus, float overallBonus, string stageName, string description)
    {
        Duration = duration;
        AttackBonus = attackBonus;
        DefenseBonus = defenseBonus;
        OverallBonus = overallBonus;
        StageName = stageName;
        Description = description;
    }
}

public class LifeExtensionItem
{
    public string ItemId;
    public string ItemName;
    public int Cost;
    public int DaysExtended;
    
    public LifeExtensionItem(string itemId, string itemName, int cost, int daysExtended)
    {
        ItemId = itemId;
        ItemName = itemName;
        Cost = cost;
        DaysExtended = daysExtended;
    }

        public Dictionary<string, object> ExportSaveData() => new();
        public void ImportSaveData(Dictionary<string, object> data) { }
}
