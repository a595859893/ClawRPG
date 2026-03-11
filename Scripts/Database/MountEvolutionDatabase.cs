using Godot;
using System;
using System.Collections.Generic;

public class MountEvolutionDatabase
{
	private static List<MountEvolutionData.EvolutionConfig> _configurations;
	
	public static List<MountEvolutionData.EvolutionConfig> GetConfigurations()
	{
		if (_configurations == null)
		{
			Initialize();
		}
		return _configurations;
	}
	
	private static void Initialize()
	{
		_configurations = new List<MountEvolutionData.EvolutionConfig>();
		
		// Horse Evolution Chain
		AddEvolution(1, "普通马", MountEvolutionData.EvolutionChain.Horse, MountEvolutionData.EvolutionStage.Basic, MountEvolutionData.EvolutionType.Nature, 0, 1, 0, new List<string>(), 100f, 10f, 5f, 5f, 0f, 0f, "", "战马");
		AddEvolution(2, "战马", MountEvolutionData.EvolutionChain.Horse, MountEvolutionData.EvolutionStage.Advanced, MountEvolutionData.EvolutionType.Nature, 1000, 10, 500, new List<string>{"皮革x5"}, 200f, 20f, 10f, 10f, 1f, 5f, "冲锋", "铁甲战马");
		AddEvolution(3, "铁甲战马", MountEvolutionData.EvolutionChain.Horse, MountEvolutionData.EvolutionStage.Elite, MountEvolutionData.EvolutionType.Nature, 5000, 20, 2000, new List<string>{"秘银x3", "皮革x10"}, 400f, 40f, 20f, 15f, 2f, 10f, "践踏", "传奇战马");
		AddEvolution(4, "传奇战马", MountEvolutionData.EvolutionChain.Horse, MountEvolutionData.EvolutionStage.Epic, MountEvolutionData.EvolutionType.Nature, 15000, 35, 8000, new List<string>{"龙鳞x5", "秘银x10"}, 800f, 80f, 40f, 25f, 3f, 15f, "战争怒吼", "神话天马");
		AddEvolution(5, "神话天马", MountEvolutionData.EvolutionChain.Horse, MountEvolutionData.EvolutionStage.Legendary, MountEvolutionData.EvolutionType.Holy, 50000, 50, 30000, new List<string>{"天使羽毛x3", "龙鳞x10", "神圣水晶x5"}, 1500f, 150f, 80f, 50f, 5f, 25f, "天马之力", "");
		
		// Wolf Evolution Chain
		AddEvolution(6, "幼狼", MountEvolutionData.EvolutionChain.Wolf, MountEvolutionData.EvolutionStage.Basic, MountEvolutionData.EvolutionType.Dark, 0, 1, 0, new List<string>(), 80f, 15f, 5f, 15f, 0f, 0f, "", "森林狼");
		AddEvolution(7, "森林狼", MountEvolutionData.EvolutionChain.Wolf, MountEvolutionData.EvolutionStage.Advanced, MountEvolutionData.EvolutionType.Dark, 1200, 12, 600, new List<string>{"狼牙x5"}, 180f, 30f, 12f, 25f, 2f, 8f, "嚎叫", "暗影狼");
		AddEvolution(8, "暗影狼", MountEvolutionData.EvolutionChain.Wolf, MountEvolutionData.EvolutionStage.Elite, MountEvolutionData.EvolutionType.Dark, 6000, 22, 2500, new List<string>{"暗影之爪x3", "狼牙x10"}, 380f, 60f, 25f, 35f, 3f, 12f, "暗影冲击", "幽冥狼王");
		AddEvolution(9, "幽冥狼王", MountEvolutionData.EvolutionChain.Wolf, MountEvolutionData.EvolutionStage.Epic, MountEvolutionData.EvolutionType.Dark, 18000, 38, 10000, new List<string>{"幽冥之核x5", "暗影之爪x10"}, 750f, 100f, 50f, 50f, 5f, 18f, "狼族领袖", "芬里尔");
		AddEvolution(10, "芬里尔", MountEvolutionData.EvolutionChain.Wolf, MountEvolutionData.EvolutionStage.Legendary, MountEvolutionData.EvolutionType.Dark, 60000, 52, 35000, new List<string>{"深渊之心x3", "幽冥之核x10", "暗影帝皇之魂x1"}, 1400f, 180f, 90f, 70f, 8f, 30f, "毁灭之咬", "");
		
		// Dragon Evolution Chain
		AddEvolution(11, "龙蛋", MountEvolutionData.EvolutionChain.Dragon, MountEvolutionData.EvolutionStage.Basic, MountEvolutionData.EvolutionType.Fire, 0, 1, 0, new List<string>(), 150f, 20f, 10f, 5f, 0f, 0f, "", "幼龙");
		AddEvolution(12, "幼龙", MountEvolutionData.EvolutionChain.Dragon, MountEvolutionData.EvolutionStage.Advanced, MountEvolutionData.EvolutionType.Fire, 2000, 15, 1000, new List<string>{"龙蛋碎片x3"}, 350f, 45f, 25f, 12f, 3f, 10f, "火焰吐息", "成年龙");
		AddEvolution(13, "成年龙", MountEvolutionData.EvolutionChain.Dragon, MountEvolutionData.EvolutionStage.Elite, MountEvolutionData.EvolutionType.Fire, 10000, 28, 5000, new List<string>{"龙心x5", "火焰精华x10"}, 700f, 90f, 50f, 20f, 5f, 18f, "龙威", "远古巨龙");
		AddEvolution(14, "远古巨龙", MountEvolutionData.EvolutionChain.Dragon, MountEvolutionData.EvolutionStage.Epic, MountEvolutionData.EvolutionType.Fire, 30000, 42, 15000, new List<string>{"巨龙之魂x3", "龙心x10", "火焰帝皇之魂x1"}, 1200f, 150f, 85f, 35f, 8f, 25f, "龙之怒", "元素龙王");
		AddEvolution(15, "元素龙王", MountEvolutionData.EvolutionChain.Dragon, MountEvolutionData.EvolutionStage.Legendary, MountEvolutionData.EvolutionType.Fire, 80000, 55, 50000, new List<string>{"世界之心x1", "巨龙之魂x10", "元素帝皇之魂x1"}, 2000f, 250f, 150f, 60f, 12f, 40f, "元素湮灭", "");
		
		// Phoenix Evolution Chain
		AddEvolution(16, "凤凰蛋", MountEvolutionData.EvolutionChain.Phoenix, MountEvolutionData.EvolutionStage.Basic, MountEvolutionData.EvolutionType.Fire, 0, 1, 0, new List<string>(), 120f, 18f, 8f, 12f, 0f, 0f, "", "火焰雏鸟");
		AddEvolution(17, "火焰雏鸟", MountEvolutionData.EvolutionChain.Phoenix, MountEvolutionData.EvolutionStage.Advanced, MountEvolutionData.EvolutionType.Fire, 1500, 12, 800, new List<string>{"羽毛x5"}, 280f, 38f, 20f, 20f, 2f, 8f, "火焰之翼", "烈焰凤凰");
		AddEvolution(18, "烈焰凤凰", MountEvolutionData.EvolutionChain.Phoenix, MountEvolutionData.EvolutionStage.Elite, MountEvolutionData.EvolutionType.Fire, 8000, 25, 3500, new List<string>{"凤凰羽毛x3", "火焰精华x8"}, 550f, 75f, 40f, 30f, 4f, 15f, "浴火重生", "永恒烈焰");
		AddEvolution(19, "永恒烈焰", MountEvolutionData.EvolutionChain.Phoenix, MountEvolutionData.EvolutionStage.Epic, MountEvolutionData.EvolutionType.Fire, 25000, 40, 12000, new List<string>{"永恒之火x3", "凤凰羽毛x10"}, 1000f, 130f, 70f, 45f, 6f, 22f, "烈焰风暴", "九尾天凤");
		AddEvolution(20, "九尾天凤", MountEvolutionData.EvolutionChain.Phoenix, MountEvolutionData.EvolutionStage.Legendary, MountEvolutionData.EvolutionType.Holy, 70000, 53, 40000, new List<string>{"天使之羽x3", "永恒之火x10", "神圣凤凰之魂x1"}, 1800f, 220f, 120f, 65f, 10f, 35f, "凤凰涅槃", "");
		
		// More chains (Eagle, Bear, Griffin, Unicorn) - abbreviated for brevity
		AddEvolution(21, "雏鹰", MountEvolutionData.EvolutionChain.Eagle, MountEvolutionData.EvolutionStage.Basic, MountEvolutionData.EvolutionType.Lightning, 0, 1, 0, new List<string>(), 70f, 12f, 4f, 20f, 0f, 0f, "", "草原鹰");
		AddEvolution(22, "草原鹰", MountEvolutionData.EvolutionChain.Eagle, MountEvolutionData.EvolutionStage.Advanced, MountEvolutionData.EvolutionType.Lightning, 1100, 11, 550, new List<string>{"鹰羽x5"}, 160f, 25f, 10f, 35f, 2f, 6f, "俯冲", "雷鸣之鹰");
		AddEvolution(23, "雷鸣之鹰", MountEvolutionData.EvolutionChain.Eagle, MountEvolutionData.EvolutionStage.Elite, MountEvolutionData.EvolutionType.Lightning, 5500, 21, 2200, new List<string>{"雷电精华x3", "鹰羽x10"}, 320f, 50f, 20f, 50f, 3f, 10f, "闪电冲击", "苍穹之翼");
		AddEvolution(24, "苍穹之翼", MountEvolutionData.EvolutionChain.Eagle, MountEvolutionData.EvolutionStage.Epic, MountEvolutionData.EvolutionType.Lightning, 16000, 36, 9000, new List<string>{"天雷之羽x5", "雷电精华x10"}, 650f, 85f, 40f, 70f, 5f, 15f, "雷霆万钧", "闪电之神");
		AddEvolution(25, "闪电之神", MountEvolutionData.EvolutionChain.Eagle, MountEvolutionData.EvolutionStage.Legendary, MountEvolutionData.EvolutionType.Lightning, 55000, 51, 32000, new List<string>{"闪电之神之心x1", "天雷之羽x10", "雷电帝皇之魂x1"}, 1300f, 160f, 80f, 100f, 8f, 28f, "雷电领域", "");
		
		AddEvolution(26, "幼熊", MountEvolutionData.EvolutionChain.Bear, MountEvolutionData.EvolutionStage.Basic, MountEvolutionData.EvolutionType.Nature, 0, 1, 0, new List<string>(), 200f, 8f, 15f, 3f, 0f, 0f, "", "森林熊");
		AddEvolution(27, "森林熊", MountEvolutionData.EvolutionChain.Bear, MountEvolutionData.EvolutionStage.Advanced, MountEvolutionData.EvolutionType.Nature, 1300, 13, 650, new List<string>{"熊皮x5"}, 400f, 18f, 30f, 8f, 1f, 5f, "熊抱", "巨岩熊");
		AddEvolution(28, "巨岩熊", MountEvolutionData.EvolutionChain.Bear, MountEvolutionData.EvolutionStage.Elite, MountEvolutionData.EvolutionType.Nature, 7000, 24, 2800, new List<string>{"岩石核心x3", "熊皮x10"}, 750f, 35f, 55f, 15f, 2f, 10f, "地震", "山岭守护者");
		AddEvolution(29, "山岭守护者", MountEvolutionData.EvolutionChain.Bear, MountEvolutionData.EvolutionStage.Epic, MountEvolutionData.EvolutionType.Nature, 20000, 39, 11000, new List<string>{"大地之心x5", "岩石核心x10"}, 1300f, 60f, 95f, 25f, 3f, 18f, "山崩地裂", "泰坦巨熊");
		AddEvolution(30, "泰坦巨熊", MountEvolutionData.EvolutionChain.Bear, MountEvolutionData.EvolutionStage.Legendary, MountEvolutionData.EvolutionType.Nature, 65000, 54, 38000, new List<string>{"泰坦之魂x1", "大地之心x10", "自然帝皇之魂x1"}, 2200f, 100f, 160f, 45f, 5f, 30f, "泰坦之力", "");
		
		// Griffin
		AddEvolution(31, "狮鹫雏鸟", MountEvolutionData.EvolutionChain.Griffin, MountEvolutionData.EvolutionStage.Basic, MountEvolutionData.EvolutionType.Holy, 0, 1, 0, new List<string>(), 130f, 16f, 12f, 14f, 0f, 0f, "", "草原狮鹫");
		AddEvolution(32, "草原狮鹫", MountEvolutionData.EvolutionChain.Griffin, MountEvolutionData.EvolutionStage.Advanced, MountEvolutionData.EvolutionType.Holy, 1800, 14, 900, new List<string>{"狮鹫之羽x5"}, 300f, 35f, 28f, 28f, 2f, 8f, "俯冲爪击", "皇家狮鹫");
		AddEvolution(33, "皇家狮鹫", MountEvolutionData.EvolutionChain.Griffin, MountEvolutionData.EvolutionStage.Elite, MountEvolutionData.EvolutionType.Holy, 9000, 26, 4000, new List<string>{"神圣之羽x3", "狮鹫之羽x10"}, 580f, 70f, 50f, 40f, 4f, 14f, "神圣冲锋", "光明狮鹫");
		AddEvolution(34, "光明狮鹫", MountEvolutionData.EvolutionChain.Griffin, MountEvolutionData.EvolutionStage.Epic, MountEvolutionData.EvolutionType.Holy, 27000, 41, 13000, new List<string>{"天使之羽x5", "神圣之羽x10"}, 1050f, 120f, 85f, 55f, 6f, 20f, "光明普照", "神圣狮鹫王");
		AddEvolution(35, "神圣狮鹫王", MountEvolutionData.EvolutionChain.Griffin, MountEvolutionData.EvolutionStage.Legendary, MountEvolutionData.EvolutionType.Holy, 75000, 55, 45000, new List<string>{"神圣狮鹫之心x1", "天使之羽x10", "神圣帝皇之魂x1"}, 1900f, 210f, 150f, 80f, 10f, 35f, "神圣领域", "");
		
		// Unicorn
		AddEvolution(36, "独角兽宝宝", MountEvolutionData.EvolutionChain.Unicorn, MountEvolutionData.EvolutionStage.Basic, MountEvolutionData.EvolutionType.Holy, 0, 1, 0, new List<string>(), 100f, 14f, 10f, 18f, 0f, 0f, "", "森林独角兽");
		AddEvolution(37, "森林独角兽", MountEvolutionData.EvolutionChain.Unicorn, MountEvolutionData.EvolutionStage.Advanced, MountEvolutionData.EvolutionType.Holy, 1600, 13, 750, new List<string>{"独角x5"}, 240f, 30f, 22f, 32f, 2f, 7f, "治愈之光", "月光独角兽");
		AddEvolution(38, "月光独角兽", MountEvolutionData.EvolutionChain.Unicorn, MountEvolutionData.EvolutionStage.Elite, MountEvolutionData.EvolutionType.Holy, 7500, 25, 3200, new List<string>{"月光精华x3", "独角x10"}, 480f, 58f, 42f, 48f, 3f, 12f, "月光洗礼", "星辰独角兽");
		AddEvolution(39, "星辰独角兽", MountEvolutionData.EvolutionChain.Unicorn, MountEvolutionData.EvolutionStage.Epic, MountEvolutionData.EvolutionType.Holy, 22000, 40, 10500, new List<string>{"星辰之尘x5", "月光精华x10"}, 880f, 100f, 72f, 68f, 5f, 18f, "星辰坠落", "宇宙独角兽");
		AddEvolution(40, "宇宙独角兽", MountEvolutionData.EvolutionChain.Unicorn, MountEvolutionData.EvolutionStage.Legendary, MountEvolutionData.EvolutionType.Holy, 68000, 53, 42000, new List<string>{"宇宙之心x1", "星辰之尘x10", "独角兽帝皇之魂x1"}, 1650f, 180f, 125f, 95f, 8f, 32f, "宇宙净化", "");
	}
	
	private static void AddEvolution(int id, string name, MountEvolutionData.EvolutionChain chain, 
		MountEvolutionData.EvolutionStage stage, MountEvolutionData.EvolutionType type,
		int requiredExp, int requiredLevel, int goldCost, List<string> requiredItems,
		float healthBonus, float attackBonus, float defenseBonus, float speedBonus,
		float critRateBonus, float critDamageBonus, string skillUnlocked, string nextEvolutionName)
	{
		var config = new MountEvolutionData.EvolutionConfig
		{
			Id = id,
			Name = name,
			Chain = chain,
			Stage = stage,
			Type = type,
			RequiredExp = requiredExp,
			RequiredLevel = requiredLevel,
			GoldCost = goldCost,
			RequiredItems = requiredItems,
			HealthBonus = healthBonus,
			AttackBonus = attackBonus,
			DefenseBonus = defenseBonus,
			SpeedBonus = speedBonus,
			CritRateBonus = critRateBonus,
			CritDamageBonus = critDamageBonus,
			SkillUnlocked = skillUnlocked,
			NextEvolutionName = nextEvolutionName
		};
		_configurations.Add(config);
	}
	
	public static MountEvolutionData.EvolutionConfig GetConfigById(int id)
	{
		var configs = GetConfigurations();
		foreach (var config in configs)
		{
			if (config.Id == id)
				return config;
		}
		return null;
	}
	
	public static List<MountEvolutionData.EvolutionConfig> GetConfigsByChain(MountEvolutionData.EvolutionChain chain)
	{
		var result = new List<MountEvolutionData.EvolutionConfig>();
		var configs = GetConfigurations();
		foreach (var config in configs)
		{
			if (config.Chain == chain)
				result.Add(config);
		}
		return result;
	}
	
	public static MountEvolutionData.EvolutionConfig GetNextEvolution(int currentConfigId)
	{
		var current = GetConfigById(currentConfigId);
		if (current == null || string.IsNullOrEmpty(current.NextEvolutionName))
			return null;
			
		var configs = GetConfigurations();
		foreach (var config in configs)
		{
			if (config.Name == current.NextEvolutionName && config.Chain == current.Chain)
				return config;
		}
		return null;
	}
}
