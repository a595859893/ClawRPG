using Godot;
using System;
using System.Collections.Generic;

public class DailyQuestData
{
	public enum QuestType
	{
		KillEnemies,
		CollectItems,
		VisitLocations,
		TalkToNPC,
		CompleteDungeons,
		UseSkills,
		earnGold,
		GainEXP,
		UsePotions,
		CraftItems,
		TradeItems,
		UseMounts,
		UsePets,
		CompleteTrials,
		WinArena
	}

	public enum QuestDifficulty
	{
		Easy,
		Normal,
		Hard,
		Epic,
		Legendary
	}

	public string QuestId { get; set; }
	public string QuestName { get; set; }
	public string Description { get; set; }
	public QuestType Type { get; set; }
	public QuestDifficulty Difficulty { get; set; }
	public int TargetCount { get; set; }
	public int CurrentCount { get; set; }
	public bool IsCompleted { get; set; }
	public bool IsClaimed { get; set; }
	public int GoldReward { get; set; }
	public int ExpReward { get; set; }
	public List<string> ItemRewards { get; set; }
	public DateTime QuestDate { get; set; }

	public DailyQuestData()
	{
		ItemRewards = new List<string>();
	}
}

public class DailyQuestDatabase
{
	private static Dictionary<string, Dictionary<DailyQuestData.QuestType, List<DailyQuestData>>> _quests = new Dictionary<string, Dictionary<DailyQuestData.QuestType, List<DailyQuestData>>>();

	public static void Initialize()
	{
		// Easy quests
		AddQuest("easy_kill_10", "消灭10个敌人", "在战斗中消灭10个敌人", DailyQuestData.QuestType.KillEnemies, DailyQuestData.QuestDifficulty.Easy, 10, 50, 100);
		AddQuest("easy_collect_5", "收集5个物品", "收集5个物品", DailyQuestData.QuestType.CollectItems, DailyQuestData.QuestDifficulty.Easy, 5, 50, 100);
		AddQuest("easy_visit_2", "访问2个区域", "访问2个不同区域", DailyQuestData.QuestType.VisitLocations, DailyQuestData.QuestDifficulty.Easy, 2, 50, 100);
		AddQuest("easy_talk_2", "与2个NPC对话", "与2个不同的NPC对话", DailyQuestData.QuestType.TalkToNPC, DailyQuestData.QuestDifficulty.Easy, 2, 50, 100);
		AddQuest("easy_gold_100", "赚取100金币", "通过任何方式赚取100金币", DailyQuestData.QuestType.earnGold, DailyQuestData.QuestDifficulty.Easy, 100, 50, 100);
		AddQuest("easy_exp_100", "获得100经验", "获得100点经验值", DailyQuestData.QuestType.GainEXP, DailyQuestData.QuestDifficulty.Easy, 100, 50, 100);
		AddQuest("easy_use_3_skill", "使用3次技能", "使用任意技能3次", DailyQuestData.QuestType.UseSkills, DailyQuestData.QuestDifficulty.Easy, 3, 50, 100);
		AddQuest("easy_use_2_potion", "使用2次药水", "使用2次药水", DailyQuestData.QuestType.UsePotions, DailyQuestData.QuestDifficulty.Easy, 2, 50, 100);

		// Normal quests
		AddQuest("normal_kill_30", "消灭30个敌人", "在战斗中消灭30个敌人", DailyQuestData.QuestType.KillEnemies, DailyQuestData.QuestDifficulty.Normal, 30, 150, 300);
		AddQuest("normal_collect_10", "收集10个物品", "收集10个物品", DailyQuestData.QuestType.CollectItems, DailyQuestData.QuestDifficulty.Normal, 10, 150, 300);
		AddQuest("normal_visit_4", "访问4个区域", "访问4个不同区域", DailyQuestData.QuestType.VisitLocations, DailyQuestData.QuestDifficulty.Normal, 4, 150, 300);
		AddQuest("normal_dungeon_1", "完成1个地下城", "完成任意地下城挑战", DailyQuestData.QuestType.CompleteDungeons, DailyQuestData.QuestDifficulty.Normal, 1, 200, 400);
		AddQuest("normal_gold_500", "赚取500金币", "通过任何方式赚取500金币", DailyQuestData.QuestType.earnGold, DailyQuestData.QuestDifficulty.Normal, 500, 150, 300);
		AddQuest("normal_exp_500", "获得500经验", "获得500点经验值", DailyQuestData.QuestType.GainEXP, DailyQuestData.QuestDifficulty.Normal, 500, 150, 300);
		AddQuest("normal_use_10_skill", "使用10次技能", "使用任意技能10次", DailyQuestData.QuestType.UseSkills, DailyQuestData.QuestDifficulty.Normal, 10, 150, 300);
		AddQuest("normal_craft_3", "制作3个物品", "制作任意物品3次", DailyQuestData.QuestType.CraftItems, DailyQuestData.QuestDifficulty.Normal, 3, 150, 300);
		AddQuest("normal_trade_2", "交易2次", "完成2次交易", DailyQuestData.QuestType.TradeItems, DailyQuestData.QuestDifficulty.Normal, 2, 150, 300);

		// Hard quests
		AddQuest("hard_kill_50", "消灭50个敌人", "在战斗中消灭50个敌人", DailyQuestData.QuestType.KillEnemies, DailyQuestData.QuestDifficulty.Hard, 50, 300, 600);
		AddQuest("hard_collect_20", "收集20个物品", "收集20个物品", DailyQuestData.QuestType.CollectItems, DailyQuestData.QuestDifficulty.Hard, 20, 300, 600);
		AddQuest("hard_dungeon_2", "完成2个地下城", "完成2个地下城挑战", DailyQuestData.QuestType.CompleteDungeons, DailyQuestData.QuestDifficulty.Hard, 2, 400, 800);
		AddQuest("hard_trial_1", "完成1个试炼", "完成任意元素试炼", DailyQuestData.QuestType.CompleteTrials, DailyQuestData.QuestDifficulty.Hard, 1, 350, 700);
		AddQuest("hard_arena_1", "赢得1场竞技场", "赢得宠物竞技场比赛", DailyQuestData.QuestType.WinArena, DailyQuestData.QuestDifficulty.Hard, 1, 400, 800);
		AddQuest("hard_gold_1000", "赚取1000金币", "通过任何方式赚取1000金币", DailyQuestData.QuestType.earnGold, DailyQuestData.QuestDifficulty.Hard, 1000, 300, 600);
		AddQuest("hard_exp_1000", "获得1000经验", "获得1000点经验值", DailyQuestData.QuestType.GainEXP, DailyQuestData.QuestDifficulty.Hard, 1000, 300, 600);
		AddQuest("hard_mount_5", "使用坐骑5次", "骑乘坐骑进入战斗5次", DailyQuestData.QuestType.UseMounts, DailyQuestData.QuestDifficulty.Hard, 5, 300, 600);
		AddQuest("hard_pet_5", "使用宠物5次", "召唤宠物战斗5次", DailyQuestData.QuestType.UsePets, DailyQuestData.QuestDifficulty.Hard, 5, 300, 600);

		// Epic quests
		AddQuest("epic_kill_100", "消灭100个敌人", "在战斗中消灭100个敌人", DailyQuestData.QuestType.KillEnemies, DailyQuestData.QuestDifficulty.Epic, 100, 500, 1000);
		AddQuest("epic_collect_30", "收集30个物品", "收集30个物品", DailyQuestData.QuestType.CollectItems, DailyQuestData.QuestDifficulty.Epic, 30, 500, 1000);
		AddQuest("epic_dungeon_3", "完成3个地下城", "完成3个地下城挑战", DailyQuestData.QuestType.CompleteDungeons, DailyQuestData.QuestDifficulty.Epic, 3, 600, 1200);
		AddQuest("epic_trial_2", "完成2个试炼", "完成2个元素试炼", DailyQuestData.QuestType.CompleteTrials, DailyQuestData.QuestDifficulty.Epic, 2, 550, 1100);
		AddQuest("epic_arena_3", "赢得3场竞技场", "赢得3场宠物竞技场比赛", DailyQuestData.QuestType.WinArena, DailyQuestData.QuestDifficulty.Epic, 3, 600, 1200);
		AddQuest("epic_gold_2000", "赚取2000金币", "通过任何方式赚取2000金币", DailyQuestData.QuestType.earnGold, DailyQuestData.QuestDifficulty.Epic, 2000, 500, 1000);
		AddQuest("epic_exp_2000", "获得2000经验", "获得2000点经验值", DailyQuestData.QuestType.GainEXP, DailyQuestData.QuestDifficulty.Epic, 2000, 500, 1000);

		// Legendary quests
		AddQuest("legendary_kill_200", "消灭200个敌人", "在战斗中消灭200个敌人", DailyQuestData.QuestType.KillEnemies, DailyQuestData.QuestDifficulty.Legendary, 200, 1000, 2000);
		AddQuest("legendary_dungeon_5", "完成5个地下城", "完成5个地下城挑战", DailyQuestData.QuestType.CompleteDungeons, DailyQuestData.QuestDifficulty.Legendary, 5, 1500, 3000);
		AddQuest("legendary_trial_3", "完成3个试炼", "完成3个元素试炼", DailyQuestData.QuestType.CompleteTrials, DailyQuestData.QuestDifficulty.Legendary, 3, 1200, 2400);
		AddQuest("legendary_arena_5", "赢得5场竞技场", "赢得5场宠物竞技场比赛", DailyQuestData.QuestType.WinArena, DailyQuestData.QuestDifficulty.Legendary, 5, 1500, 3000);
		AddQuest("legendary_gold_5000", "赚取5000金币", "通过任何方式赚取5000金币", DailyQuestData.QuestType.earnGold, DailyQuestData.QuestDifficulty.Legendary, 5000, 1000, 2000);
		AddQuest("legendary_exp_5000", "获得5000经验", "获得5000点经验值", DailyQuestData.QuestType.GainEXP, DailyQuestData.QuestDifficulty.Legendary, 5000, 1000, 2000);
	}

	private static void AddQuest(string id, string name, string desc, DailyQuestData.QuestType type, DailyQuestData.QuestDifficulty difficulty, int target, int gold, int exp)
	{
		var quest = new DailyQuestData
		{
			QuestId = id,
			QuestName = name,
			Description = desc,
			Type = type,
			Difficulty = difficulty,
			TargetCount = target,
			CurrentCount = 0,
			IsCompleted = false,
			IsClaimed = false,
			GoldReward = gold,
			ExpReward = exp,
			QuestDate = DateTime.Now.Date
		};

		string key = difficulty.ToString();
		if (!_quests.ContainsKey(key))
		{
			_quests[key] = new Dictionary<DailyQuestData.QuestType, List<DailyQuestData>>();
		}

		if (!_quests[key].ContainsKey(type))
		{
			_quests[key][type] = new List<DailyQuestData>();
		}

		_quests[key][type].Add(quest);
	}

	public static List<DailyQuestData> GetRandomQuests(int count = 5)
	{
		var result = new List<DailyQuestData>();
		var allQuests = new List<DailyQuestData>();

		// Collect all quests
		foreach (var diffDict in _quests.Values)
		{
			foreach (var questList in diffDict.Values)
			{
				allQuests.AddRange(questList);
			}
		}

		// Random selection with difficulty weighting
		var random = new Random();
		var weights = new Dictionary<DailyQuestData.QuestDifficulty, int>
		{
			{ DailyQuestData.QuestDifficulty.Easy, 40 },
			{ DailyQuestData.QuestDifficulty.Normal, 30 },
			{ DailyQuestData.QuestDifficulty.Hard, 18 },
			{ DailyQuestData.QuestDifficulty.Epic, 8 },
			{ DailyQuestData.QuestDifficulty.Legendary, 4 }
		};

		var selectedDifficulties = new List<DailyQuestData.QuestDifficulty>();
		int totalWeight = 0;
		foreach (var w in weights.Values) totalWeight += w;

		for (int i = 0; i < count; i++)
		{
			int roll = random.Next(totalWeight);
			int cumulative = 0;
			DailyQuestData.QuestDifficulty selectedDiff = DailyQuestData.QuestDifficulty.Easy;

			foreach (var w in weights)
			{
				cumulative += w.Value;
				if (roll < cumulative)
				{
					selectedDiff = w.Key;
					break;
				}
			}

			var diffKey = selectedDiff.ToString();
			if (_quests.ContainsKey(diffKey))
			{
				var diffQuests = _quests[diffKey];
				if (diffQuests.Count > 0)
				{
					var questType = diffQuests.Keys.ToList()[random.Next(diffQuests.Count)];
					var questsOfType = diffQuests[questType];
					if (questsOfType.Count > 0)
					{
						var quest = questsOfType[random.Next(questsOfType.Count)];
						if (!result.Exists(q => q.QuestId == quest.QuestId))
						{
							result.Add(quest);
						}
					}
				}
			}
		}

		return result;
	}

	public static DailyQuestData GetQuestTemplate(string questId)
	{
		foreach (var diffDict in _quests.Values)
		{
			foreach (var questList in diffDict.Values)
			{
				foreach (var quest in questList)
				{
					if (quest.QuestId == questId)
						return quest;
				}
			}
		}
		return null;
	}
}
