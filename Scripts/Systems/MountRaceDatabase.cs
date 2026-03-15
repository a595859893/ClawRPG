using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 坐骑竞速数据库 - 存储所有坐骑竞速活动数据
/// </summary>
public class MountRaceDatabase
{
	private static MountRaceDatabase _instance;
	public static MountRaceDatabase Instance
	{
		get
		{
			if (_instance == null)
				_instance = new MountRaceDatabase();
			return _instance;
		}
	}

	public List<MountRaceData.MountRace> Races { get; private set; }

	public MountRaceDatabase()
	{
		Races = new List<MountRaceData.MountRace>();
		InitializeRaces();
	}

	private void InitializeRaces()
	{
		// Easy races (Difficulty 1)
		AddRace(new MountRaceData.MountRace
		{
			Id = "race_grassland",
			Name = "草地悠闲赛",
			Description = "穿越宁静的草地，适合初学者",
			Difficulty = 1,
			Distance = 500f,
			EntryFee = 50,
			RewardGold = 200,
			RewardExp = 100,
			Checkpoints = new List<string> { "起点", "草地A", "草地B", "终点" },
			RecordTime = 45f
		});

		AddRace(new MountRaceData.MountRace
		{
			Id = "race_forest_path",
			Name = "森林小径",
			Description = "穿梭在茂密的森林中",
			Difficulty = 1,
			Distance = 600f,
			EntryFee = 80,
			RewardGold = 300,
			RewardExp = 150,
			Checkpoints = new List<string> { "起点", "林间空地", "蘑菇群", "终点" },
			RecordTime = 55f
		});

		// Normal races (Difficulty 2)
		AddRace(new MountRaceData.MountRace
		{
			Id = "race_mountain_trail",
			Name = "山地赛道",
			Description = "攀登崎岖的山路",
			Difficulty = 2,
			Distance = 800f,
			EntryFee = 150,
			RewardGold = 500,
			RewardExp = 300,
			Checkpoints = new List<string> { "起点", "山脚", "山腰", "山峰", "终点" },
			RecordTime = 80f
		});

		AddRace(new MountRaceData.MountRace
		{
			Id = "race_desert_dunes",
			Name = "沙漠沙丘",
			Description = "在炽热的沙丘上奔跑",
			Difficulty = 2,
			Distance = 900f,
			EntryFee = 200,
			RewardGold = 600,
			RewardExp = 350,
			Checkpoints = new List<string> { "起点", "沙丘A", "绿洲", "沙丘B", "终点" },
			RecordTime = 90f
		});

		// Hard races (Difficulty 3)
		AddRace(new MountRaceData.MountRace
		{
			Id = "race_volcano_edge",
			Name = "火山边缘",
			Description = "沿着火山的危险边缘前进",
			Difficulty = 3,
			Distance = 1000f,
			EntryFee = 300,
			RewardGold = 1000,
			RewardExp = 600,
			Checkpoints = new List<string> { "起点", "熔岩泉", "火山口", "冷却岩浆", "终点" },
			RecordTime = 110f
		});

		AddRace(new MountRaceData.MountRace
		{
			Id = "race_thunder_cliff",
			Name = "雷鸣悬崖",
			Description = "在雷电交错的悬崖上冲刺",
			Difficulty = 3,
			Distance = 1100f,
			EntryFee = 350,
			RewardGold = 1200,
			RewardExp = 700,
			Checkpoints = new List<string> { "起点", "雷电区A", "云端", "雷电区B", "终点" },
			RecordTime = 120f
		});

		// Epic races (Difficulty 4)
		AddRace(new MountRaceData.MountRace
		{
			Id = "race_dragon_peak",
			Name = "龙之巅峰",
			Description = "到达巨龙的巢穴",
			Difficulty = 4,
			Distance = 1500f,
			EntryFee = 500,
			RewardGold = 2000,
			RewardExp = 1200,
			Checkpoints = new List<string> { "起点", "龙穴入口", "龙之走廊", "龙穴大厅", "终点" },
			RecordTime = 180f
		});

		AddRace(new MountRaceData.MountRace
		{
			Id = "race_shadow_realm",
			Name = "暗影领域",
			Description = "穿越神秘的暗影世界",
			Difficulty = 4,
			Distance = 1600f,
			EntryFee = 600,
			RewardGold = 2500,
			RewardExp = 1500,
			Checkpoints = new List<string> { "起点", "暗影门", "幽灵平原", "深渊", "终点" },
			RecordTime = 200f
		});

		// Legendary races (Difficulty 5)
		AddRace(new MountRaceData.MountRace
		{
			Id = "race_celestial_path",
			Name = "通天之路",
			Description = "踏上通往神界的道路",
			Difficulty = 5,
			Distance = 2000f,
			EntryFee = 1000,
			RewardGold = 5000,
			RewardExp = 3000,
			Checkpoints = new List<string> { "起点", "天梯", "云海", "神之门", "天庭", "终点" },
			RecordTime = 300f
		});

		AddRace(new MountRaceData.MountRace
		{
			Id = "race_world_edge",
			Name = "世界尽头",
			Description = "到达已知世界的边界",
			Difficulty = 5,
			Distance = 2500f,
			EntryFee = 1500,
			RewardGold = 8000,
			RewardExp = 5000,
			Checkpoints = new List<string> { "起点", "边境森林", "遗忘之地", "时空裂缝", "世界边缘", "终点" },
			RecordTime = 400f
		});
	}

	private void AddRace(MountRaceData.MountRace race)
	{
		Races.Add(race);
	}

	public MountRaceData.MountRace GetRace(string raceId)
	{
		return Races.Find(r => r.Id == raceId);
	}

	public List<MountRaceData.MountRace> GetRacesByDifficulty(int difficulty)
	{
		return Races.FindAll(r => r.Difficulty == difficulty);
	}

	public List<MountRaceData.MountRace> GetAllRaces()
	{
		return new List<MountRaceData.MountRace>(Races);
	}

	public string GetDifficultyName(int difficulty)
	{
		switch (difficulty)
		{
			case 1: return "简单";
			case 2: return "普通";
			case 3: return "困难";
			case 4: return "史诗";
			case 5: return "传奇";
			default: return "未知";
		}
	}

	public string GetDifficultyColor(int difficulty)
	{
		switch (difficulty)
		{
			case 1: return "#4CAF50"; // Green
			case 2: return "#2196F3"; // Blue
			case 3: return "#FF9800"; // Orange
			case 4: return "#9C27B0"; // Purple
			case 5: return "#F44336"; // Red
			default: return "#FFFFFF";
		}
	}
}
