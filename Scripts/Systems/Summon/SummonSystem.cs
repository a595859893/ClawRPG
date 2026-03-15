using Godot;
using System;
using System.Collections.Generic;

public class SummonData
{
	public int SummonId { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public SummonType Type { get; set; }
	public SummonRarity Rarity { get; set; }
	public int BaseAttack { get; set; }
	public int BaseDefense { get; set; }
	public int BaseHealth { get; set; }
	public float AttackSpeed { get; set; }
	public int ManaCost { get; set; }
	public int Cooldown { get; set; }
	public string IconPath { get; set; }
	public List<string> Abilities { get; set; }

    public override Dictionary ExportSaveData() => new();
    public override void ImportSaveData(Dictionary data) { }

}

public enum SummonType
{
	Elemental,
	Beast,
	Undead,
	Celestial,
	Chaos,
	Nature,
	Mechanical,
	Mythical
}

public enum SummonRarity
{
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary
}

public class PlayerSummonData
{
	public int SummonId { get; set; }
	public int Level { get; set; }
	public int Experience { get; set; }
	public bool IsUnlocked { get; set; }
	public int TimesSummoned { get; set; }
}

public class SummonSystem : BaseSystem
{
	private Dictionary<int, SummonData> summonDatabase = new Dictionary<int, SummonData>();
	private Dictionary<int, PlayerSummonData> playerSummons = new Dictionary<int, PlayerSummonData>();
	private int activeSummonId = -1;
	
	public override void _Ready()
	{
		InitializeSummonDatabase();
		LoadSummonData();
	}
	
	private void InitializeSummonDatabase()
	{
		// Elemental summons
		summonDatabase[1] = new SummonData
		{
			SummonId = 1,
			Name = "火焰元素",
			Description = "由火焰凝聚而成的元素生物",
			Type = SummonType.Elemental,
			Rarity = SummonRarity.Common,
			BaseAttack = 25,
			BaseDefense = 10,
			BaseHealth = 50,
			AttackSpeed = 1.2f,
			ManaCost = 30,
			Cooldown = 60,
			IconPath = "res://Icons/summons/fire_element.png",
			Abilities = new List<string> { "火焰喷射", "燃烧" }
		};
		
		summonDatabase[2] = new SummonData
		{
			SummonId = 2,
			Name = "冰霜元素",
			Description = "由寒冰凝聚而成的元素生物",
			Type = SummonType.Elemental,
			Rarity = SummonRarity.Common,
			BaseAttack = 20,
			BaseDefense = 15,
			BaseHealth = 60,
			AttackSpeed = 1.0f,
			ManaCost = 30,
			Cooldown = 60,
			IconPath = "res://Icons/summons/ice_element.png",
			Abilities = new List<string> { "冰冻射线", "冰甲" }
		};
		
		summonDatabase[3] = new SummonData
		{
			SummonId = 3,
			Name = "雷电元素",
			Description = "由闪电凝聚而成的元素生物",
			Type = SummonType.Elemental,
			Rarity = SummonRarity.Uncommon,
			BaseAttack = 35,
			BaseDefense = 10,
			BaseHealth = 45,
			AttackSpeed = 1.5f,
			ManaCost = 40,
			Cooldown = 45,
			IconPath = "res://Icons/summons/lightning_element.png",
			Abilities = new List<string> { "雷电打击", "感电" }
		};
		
		// Beast summons
		summonDatabase[4] = new SummonData
		{
			SummonId = 4,
			Name = "森林巨狼",
			Description = "来自古老森林的巨型野狼",
			Type = SummonType.Beast,
			Rarity = SummonRarity.Common,
			BaseAttack = 30,
			BaseDefense = 15,
			BaseHealth = 55,
			AttackSpeed = 1.3f,
			ManaCost = 35,
			Cooldown = 50,
			IconPath = "res://Icons/summons/wolf.png",
			Abilities = new List<string> { "撕咬", "嚎叫" }
		};
		
		summonDatabase[5] = new SummonData
		{
			SummonId = 5,
			Name = "暗影豹",
			Description = "潜伏在阴影中的致命猎手",
			Type = SummonType.Beast,
			Rarity = SummonRarity.Rare,
			BaseAttack = 45,
			BaseDefense = 12,
			BaseHealth = 40,
			AttackSpeed = 1.8f,
			ManaCost = 50,
			Cooldown = 40,
			IconPath = "res://Icons/summons/panther.png",
			Abilities = new List<string> { "背刺", "隐形", "致命一击" }
		};
		
		summonDatabase[6] = new SummonData
		{
			SummonId = 6,
			Name = "泰坦巨熊",
			Description = "拥有毁天灭地力量巨型棕熊",
			Type = SummonType.Beast,
			Rarity = SummonRarity.Epic,
			BaseAttack = 60,
			BaseDefense = 35,
			BaseHealth = 100,
			AttackSpeed = 0.8f,
			ManaCost = 70,
			Cooldown = 90,
			IconPath = "res://Icons/summons/bear.png",
			Abilities = new List<string> { "粉碎", "震吼", "蛮力" }
		};
		
		// Undead summons
		summonDatabase[7] = new SummonData
		{
			SummonId = 7,
			Name = "骷髅战士",
			Description = "从死亡中复苏的战士",
			Type = SummonType.Undead,
			Rarity = SummonRarity.Common,
			BaseAttack = 20,
			BaseDefense = 20,
			BaseHealth = 45,
			AttackSpeed = 1.0f,
			ManaCost = 25,
			Cooldown = 45,
			IconPath = "res://Icons/summons/skeleton.png",
			Abilities = new List<string> { "骨刃", "重生" }
		};
		
		summonDatabase[8] = new SummonData
		{
			SummonId = 8,
			Name = "幽灵法师",
			Description = "掌握死亡魔法的幽魂",
			Type = SummonType.Undead,
			Rarity = SummonRarity.Rare,
			BaseAttack = 50,
			BaseDefense = 8,
			BaseHealth = 35,
			AttackSpeed = 1.4f,
			ManaCost = 55,
			Cooldown = 55,
			IconPath = "res://Icons/summons/wraith.png",
			Abilities = new List<string> { "死亡射线", "吸取生命", "恐惧" }
		};
		
		// Celestial summons
		summonDatabase[9] = new SummonData
		{
			SummonId = 9,
			Name = "神圣天使",
			Description = "来自天堂的光明使者",
			Type = SummonType.Celestial,
			Rarity = SummonRarity.Epic,
			BaseAttack = 40,
			BaseDefense = 30,
			BaseHealth = 80,
			AttackSpeed = 1.1f,
			ManaCost = 65,
			Cooldown = 75,
			IconPath = "res://Icons/summons/angel.png",
			Abilities = new List<string> { "神圣之光", "治疗", "祝福" }
		};
		
		summonDatabase[10] = new SummonData
		{
			SummonId = 10,
			Name = "裁决天使",
			Description = "执行神圣审判的天界使者",
			Type = SummonType.Celestial,
			Rarity = SummonRarity.Legendary,
			BaseAttack = 80,
			BaseDefense = 40,
			BaseHealth = 120,
			AttackSpeed = 1.2f,
			ManaCost = 100,
			Cooldown = 120,
			IconPath = "res://Icons/summons/seraph.png",
			Abilities = new List<string> { "天罚", "神圣之力", "复活", "光环" }
		};
		
		// Chaos summons
		summonDatabase[11] = new SummonData
		{
			SummonId = 11,
			Name = "小恶魔",
			Description = "来自深渊的调皮恶魔",
			Type = SummonType.Chaos,
			Rarity = SummonRarity.Common,
			BaseAttack = 25,
			BaseDefense = 8,
			BaseHealth = 35,
			AttackSpeed = 1.6f,
			ManaCost = 20,
			Cooldown = 30,
			IconPath = "res://Icons/summons/imp.png",
			Abilities = new List<string> { "火焰弹", "捣乱" }
		};
		
		summonDatabase[12] = new SummonData
		{
			SummonId = 12,
			Name = "深渊领主",
			Description = "统治无尽深渊的恐怖存在",
			Type = SummonType.Chaos,
			Rarity = SummonRarity.Legendary,
			BaseAttack = 100,
			BaseDefense = 45,
			BaseHealth = 150,
			AttackSpeed = 0.9f,
			ManaCost = 120,
			Cooldown = 150,
			IconPath = "res://Icons/summons/demon_lord.png",
			Abilities = new List<string> { "深渊之火", "召唤小鬼", "恐惧领域", "恶魔之魂" }
		};
		
		// Nature summons
		summonDatabase[13] = new SummonData
		{
			SummonId = 13,
			Name = "树人",
			Description = "守护森林的古老树灵",
			Type = SummonType.Nature,
			Rarity = SummonRarity.Uncommon,
			BaseAttack = 15,
			BaseDefense = 35,
			BaseHealth = 90,
			AttackSpeed = 0.7f,
			ManaCost = 40,
			Cooldown = 70,
			IconPath = "res://Icons/summons/treant.png",
			Abilities = new List<string> { "藤蔓缠绕", "自然回春" }
		};
		
		summonDatabase[14] = new SummonData
		{
			SummonId = 14,
			Name = "精灵龙",
			Description = "与自然融为一体的美丽飞龙",
			Type = SummonType.Nature,
			Rarity = SummonRarity.Epic,
			BaseAttack = 55,
			BaseDefense = 25,
			BaseHealth = 70,
			AttackSpeed = 1.5f,
			ManaCost = 60,
			Cooldown = 50,
			IconPath = "res://Icons/summons/fairy_dragon.png",
			Abilities = new List<string> { "自然之怒", "隐身", "急速" }
		};
		
		// Mechanical summons
		summonDatabase[15] = new SummonData
		{
			SummonId = 15,
			Name = "钢铁傀儡",
			Description = "古代遗迹中发现的机械守卫",
			Type = SummonType.Mechanical,
			Rarity = SummonRarity.Uncommon,
			BaseAttack = 30,
			BaseDefense = 40,
			BaseHealth = 85,
			AttackSpeed = 0.8f,
			ManaCost = 45,
			Cooldown = 80,
			IconPath = "res://Icons/summons/golem.png",
			Abilities = new List<string> { "铁拳", "铜墙铁壁" }
		};
		
		summonDatabase[16] = new SummonData
		{
			SummonId = 16,
			Name = "蒸汽机甲",
			Description = "融合蒸汽动力与魔法的战争机器",
			Type = SummonType.Mechanical,
			Rarity = SummonRarity.Rare,
			BaseAttack = 50,
			BaseDefense = 35,
			BaseHealth = 75,
			AttackSpeed = 1.0f,
			ManaCost = 55,
			Cooldown = 60,
			IconPath = "res://Icons/summons/steampunk_mech.png",
			Abilities = "蒸汽喷射",
			Abilities = new List<string> { "蒸汽喷射", "穿甲弹", "过热" }
		};
		
		// Mythical summons
		summonDatabase[17] = new SummonData
		{
			SummonId = 17,
			Name = "独角兽",
			Description = "象征纯洁与希望的神圣生物",
			Type = SummonType.Mythical,
			Rarity = SummonRarity.Rare,
			BaseAttack = 35,
			BaseDefense = 25,
			BaseHealth = 65,
			AttackSpeed = 1.4f,
			ManaCost = 45,
			Cooldown = 45,
			IconPath = "res://Icons/summons/unicorn.png",
			Abilities = new List<string> { "神圣冲击", "治疗之光", "净化" }
		};
		
		summonDatabase[18] = new SummonData
		{
			SummonId = 18,
			Name = "凤凰",
			Description = "从灰烬中重生的永恒之鸟",
			Type = SummonType.Mythical,
			Rarity = SummonRarity.Legendary,
			BaseAttack = 90,
			BaseDefense = 30,
			BaseHealth = 100,
			AttackSpeed = 1.3f,
			ManaCost = 110,
			Cooldown = 180,
			IconPath = "res://Icons/summons/phoenix.png",
			Abilities = new List<string> { "火焰风暴", "浴火重生", "灼热光环", "羽毛飞射" }
		};
		
		summonDatabase[19] = new SummonData
		{
			SummonId = 19,
			Name = "青龙",
			Description = "东方神兽，主宰风雨雷电",
			Type = SummonType.Mythical,
			Rarity = SummonRarity.Legendary,
			BaseAttack = 95,
			BaseDefense = 35,
			BaseHealth = 110,
			AttackSpeed = 1.4f,
			ManaCost = 115,
			Cooldown = 140,
			IconPath = "res://Icons/summons/azure_dragon.png",
			Abilities = new List<string> { "雷霆万钧", "呼风唤雨", "龙息", "神龙摆尾" }
		};
		
		summonDatabase[20] = new SummonData
		{
			SummonId = 20,
			Name = "白虎",
			Description = "西方神兽，主宰战争与杀伐",
			Type = SummonType.Mythical,
			Rarity = SummonRarity.Legendary,
			BaseAttack = 110,
			BaseDefense = 25,
			BaseHealth = 90,
			AttackSpeed = 1.6f,
			ManaCost = 105,
			Cooldown = 100,
			IconPath = "res://Icons/summons/white_tiger.png",
			Abilities = new List<string> { "虎啸山林", "急速利爪", "撕裂", "猛虎下山" }
		};
	}
	
	public void LoadSummonData()
	{
		var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
		if (saveSystem != null)
		{
			var data = saveSystem.GetSection("summon_system");
			if (data.Contains("player_summons"))
			{
				// Load player summon data
			}
		}
	}
	
	public void SaveSummonData()
	{
		var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
		if (saveSystem != null)
		{
			// Save player summon data
		}
	}
	
	public void UnlockSummon(int summonId)
	{
		if (!playerSummons.ContainsKey(summonId))
		{
			playerSummons[summonId] = new PlayerSummonData
			{
				SummonId = summonId,
				Level = 1,
				Experience = 0,
				IsUnlocked = false,
				TimesSummoned = 0
			};
		}
		playerSummons[summonId].IsUnlocked = true;
		SaveSummonData();
	}
	
	public bool IsSummonUnlocked(int summonId)
	{
		return playerSummons.ContainsKey(summonId) && playerSummons[summonId].IsUnlocked;
	}
	
	public SummonData GetSummonData(int summonId)
	{
		return summonDatabase.ContainsKey(summonId) ? summonDatabase[summonId] : null;
	}
	
	public List<SummonData> GetAllSummons()
	{
		return new List<SummonData>(summonDatabase.Values);
	}
	
	public List<SummonData> GetUnlockedSummons()
	{
		var result = new List<SummonData>();
		foreach (var kvp in playerSummons)
		{
			if (kvp.Value.IsUnlocked && summonDatabase.ContainsKey(kvp.Key))
			{
				result.Add(summonDatabase[kvp.Key]);
			}
		}
		return result;
	}
	
	public void AddSummonExperience(int summonId, int experience)
	{
		if (playerSummons.ContainsKey(summonId))
		{
			playerSummons[summonId].Experience += experience;
			CheckLevelUp(summonId);
			SaveSummonData();
		}
	}
	
	private void CheckLevelUp(int summonId)
	{
		var data = playerSummons[summonId];
		int expRequired = data.Level * 100;
		while (data.Experience >= expRequired)
		{
			data.Experience -= expRequired;
			data.Level++;
			expRequired = data.Level * 100;
		}
	}
	
	public int GetSummonLevel(int summonId)
	{
		return playerSummons.ContainsKey(summonId) ? playerSummons[summonId].Level : 0;
	}
	
	public void SetActiveSummon(int summonId)
	{
		if (IsSummonUnlocked(summonId))
		{
			activeSummonId = summonId;
			if (playerSummons.ContainsKey(summonId))
			{
				playerSummons[summonId].TimesSummoned++;
				SaveSummonData();
			}
		}
	}
	
	public int GetActiveSummon()
	{
		return activeSummonId;
	}
	
	public Dictionary<int, PlayerSummonData> GetPlayerSummons()
	{
		return playerSummons;
	}
	
	public int GetTotalSummons()
	{
		return summonDatabase.Count;
	}
	
	public int GetUnlockedCount()
	{
		int count = 0;
		foreach (var kvp in playerSummons)
		{
			if (kvp.Value.IsUnlocked) count++;
		}
		return count;
	}
	
	public void UnlockAllSummons()
	{
		foreach (var summon in summonDatabase.Keys)
		{
			UnlockSummon(summon);
		}
	}
	
	public Dictionary<string, int> GetSummonStatistics()
	{
		var stats = new Dictionary<string, int>
		{
			{ "total", summonDatabase.Count },
			{ "unlocked", GetUnlockedCount() },
			{ "common", 0 },
			{ "uncommon", 0 },
			{ "rare", 0 },
			{ "epic", 0 },
			{ "legendary", 0 }
		};
		
		foreach (var kvp in playerSummons)
		{
			if (kvp.Value.IsUnlocked && summonDatabase.ContainsKey(kvp.Key))
			{
				var rarity = summonDatabase[kvp.Key].Rarity;
				switch (rarity)
				{
					case SummonRarity.Common: stats["common"]++; break;
					case SummonRarity.Uncommon: stats["uncommon"]++; break;
					case SummonRarity.Rare: stats["rare"]++; break;
					case SummonRarity.Epic: stats["epic"]++; break;
					case SummonRarity.Legendary: stats["legendary"]++; break;
				}
			}
		}
		
		return stats;
	}
	
	public override Dictionary ExportSaveData() => new();
	public override void ImportSaveData(Dictionary data) { }
}
