using Godot;
using System;
using System.Collections.Generic;

public partial class DailyQuestSystem : BaseSystem
{
	public static DailyQuestSystem Instance { get; private set; }

	private List<DailyQuestData> _dailyQuests = new List<DailyQuestData>();
	private DateTime _lastRefreshDate;
	private int _totalQuestsCompleted;
	private int _totalQuestsClaimed;
	private int _totalGoldEarned;
	private int _totalExpEarned;

	// Quest tracking
	private int _killCount;
	private int _collectCount;
	private int _visitCount;
	private int _talkCount;
	private int _dungeonCount;
	private int _skillUseCount;
	private int _goldEarnedCount;
	private int _expGainedCount;
	private int _potionUseCount;
	private int _craftCount;
	private int _tradeCount;
	private int _mountUseCount;
	private int _petUseCount;
	private int _trialCount;
	private int _arenaWinCount;

	[Signal]
	public delegate void QuestUpdatedDelegate();
	[Signal]
	public delegate void QuestCompletedDelegate();
	[Signal]
	public delegate void QuestClaimedDelegate();

	public DailyQuestSystem()
	{
		Instance = this;
	}

	public void Initialize()
	{
		DailyQuestDatabase.Initialize();
		RefreshDailyQuests();
		GD.Print("[DailyQuestSystem] Initialized with " + _dailyQuests.Count + " quests");
	}

	public void RefreshDailyQuests()
	{
		DateTime today = DateTime.Now.Date;

		if (_lastRefreshDate.Date != today)
		{
			_dailyQuests = DailyQuestDatabase.GetRandomQuests(5);
			_lastRefreshDate = today;

			// Reset tracking
			_killCount = 0;
			_collectCount = 0;
			_visitCount = 0;
			_talkCount = 0;
			_dungeonCount = 0;
			_skillUseCount = 0;
			_goldEarnedCount = 0;
			_expGainedCount = 0;
			_potionUseCount = 0;
			_craftCount = 0;
			_tradeCount = 0;
			_mountUseCount = 0;
			_petUseCount = 0;
			_trialCount = 0;
			_arenaWinCount = 0;

			GD.Print("[DailyQuestSystem] New daily quests refreshed for " + today.ToShortDateString());
		}
	}

	public List<DailyQuestData> GetDailyQuests()
	{
		RefreshDailyQuests();
		return _dailyQuests;
	}

	public void UpdateQuestProgress(DailyQuestData.QuestType type, int amount = 1)
	{
		RefreshDailyQuests();

		foreach (var quest in _dailyQuests)
		{
			if (quest.Type == type && !quest.IsCompleted)
			{
				switch (type)
				{
					case DailyQuestData.QuestType.KillEnemies:
						_killCount += amount;
						quest.CurrentCount = Math.Min(_killCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.CollectItems:
						_collectCount += amount;
						quest.CurrentCount = Math.Min(_collectCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.VisitLocations:
						_visitCount += amount;
						quest.CurrentCount = Math.Min(_visitCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.TalkToNPC:
						_talkCount += amount;
						quest.CurrentCount = Math.Min(_talkCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.CompleteDungeons:
						_dungeonCount += amount;
						quest.CurrentCount = Math.Min(_dungeonCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.UseSkills:
						_skillUseCount += amount;
						quest.CurrentCount = Math.Min(_skillUseCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.earnGold:
						_goldEarnedCount += amount;
						quest.CurrentCount = Math.Min(_goldEarnedCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.GainEXP:
						_expGainedCount += amount;
						quest.CurrentCount = Math.Min(_expGainedCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.UsePotions:
						_potionUseCount += amount;
						quest.CurrentCount = Math.Min(_potionUseCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.CraftItems:
						_craftCount += amount;
						quest.CurrentCount = Math.Min(_craftCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.TradeItems:
						_tradeCount += amount;
						quest.CurrentCount = Math.Min(_tradeCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.UseMounts:
						_mountUseCount += amount;
						quest.CurrentCount = Math.Min(_mountUseCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.UsePets:
						_petUseCount += amount;
						quest.CurrentCount = Math.Min(_petUseCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.CompleteTrials:
						_trialCount += amount;
						quest.CurrentCount = Math.Min(_trialCount, quest.TargetCount);
						break;
					case DailyQuestData.QuestType.WinArena:
						_arenaWinCount += amount;
						quest.CurrentCount = Math.Min(_arenaWinCount, quest.TargetCount);
						break;
				}

				if (quest.CurrentCount >= quest.TargetCount)
				{
					quest.IsCompleted = true;
					_totalQuestsCompleted++;
					QuestCompleted.Emit(quest);
					GD.Print("[DailyQuestSystem] Quest completed: " + quest.QuestName);
				}

				QuestUpdated.Emit(quest);
			}
		}
	}

	public bool ClaimReward(DailyQuestData quest)
	{
		if (quest == null || !quest.IsCompleted || quest.IsClaimed)
			return false;

		quest.IsClaimed = true;
		_totalQuestsClaimed++;

		// Give rewards
		if (quest.GoldReward > 0)
		{
			Player player = GameManager.GetPlayer();
			if (player != null)
			{
				player.AddGold(quest.GoldReward);
				_totalGoldEarned += quest.GoldReward;
			}
		}

		if (quest.ExpReward > 0)
		{
			Player player = GameManager.GetPlayer();
			if (player != null)
			{
				player.AddExp(quest.ExpReward);
				_totalExpEarned += quest.ExpReward;
			}
		}

		QuestClaimed.Emit(quest);
		GD.Print("[DailyQuestSystem] Reward claimed: " + quest.QuestName + " - " + quest.GoldReward + " gold, " + quest.ExpReward + " exp");

		return true;
	}

	public int GetTotalQuestsCompleted() => _totalQuestsCompleted;
	public int GetTotalQuestsClaimed() => _totalQuestsClaimed;
	public int GetTotalGoldEarned() => _totalGoldEarned;
	public int GetTotalExpEarned() => _totalExpEarned;

	public Dictionary<string, object> GetStatistics()
	{
		return new Dictionary<string, object>
		{
			{ "totalCompleted", _totalQuestsCompleted },
			{ "totalClaimed", _totalQuestsClaimed },
			{ "totalGoldEarned", _totalGoldEarned },
			{ "totalExpEarned", _totalExpEarned },
			{ "currentProgress", new Dictionary<string, int>
				{
					{ "kills", _killCount },
					{ "collects", _collectCount },
					{ "visits", _visitCount },
					{ "talks", _talkCount },
					{ "dungeons", _dungeonCount },
					{ "skillUses", _skillUseCount },
					{ "gold", _goldEarnedCount },
					{ "exp", _expGainedCount }
				}
			}
		};
	}

	public Dictionary<string, object> Save()
	{
		return new Dictionary<string, object>
		{
			{ "lastRefreshDate", _lastRefreshDate.ToString("o") },
			{ "totalQuestsCompleted", _totalQuestsCompleted },
			{ "totalQuestsClaimed", _totalQuestsClaimed },
			{ "totalGoldEarned", _totalGoldEarned },
			{ "totalExpEarned", _totalExpEarned },
			{ "questProgress", new Dictionary<string, int>
				{
					{ "kills", _killCount },
					{ "collects", _collectCount },
					{ "visits", _visitCount },
					{ "talks", _talkCount },
					{ "dungeons", _dungeonCount },
					{ "skillUses", _skillUseCount },
					{ "gold", _goldEarnedCount },
					{ "exp", _expGainedCount },
					{ "potions", _potionUseCount },
					{ "crafts", _craftCount },
					{ "trades", _tradeCount },
					{ "mounts", _mountUseCount },
					{ "pets", _petUseCount },
					{ "trials", _trialCount },
					{ "arenaWins", _arenaWinCount }
				}
			},
			{ "completedQuests", _dailyQuests.FindAll(q => q.IsCompleted).ConvertAll(q => q.QuestId) },
			{ "claimedQuests", _dailyQuests.FindAll(q => q.IsClaimed).ConvertAll(q => q.QuestId) }
		};
	}

	public void Load(Dictionary<string, object> data)
	{
		if (data == null) return;

		if (data.ContainsKey("lastRefreshDate"))
			DateTime.TryParse(data["lastRefreshDate"].ToString(), out _lastRefreshDate);

		if (data.ContainsKey("totalQuestsCompleted"))
			_totalQuestsCompleted = Convert.ToInt32(data["totalQuestsCompleted"]);

		if (data.ContainsKey("totalQuestsClaimed"))
			_totalQuestsClaimed = Convert.ToInt32(data["totalQuestsClaimed"]);

		if (data.ContainsKey("totalGoldEarned"))
			_totalGoldEarned = Convert.ToInt32(data["totalGoldEarned"]);

		if (data.ContainsKey("totalExpEarned"))
			_totalExpEarned = Convert.ToInt32(data["totalExpEarned"]);

		if (data.ContainsKey("questProgress"))
		{
			var progress = (Dictionary<string, object>)data["questProgress"];
			if (progress.ContainsKey("kills")) _killCount = Convert.ToInt32(progress["kills"]);
			if (progress.ContainsKey("collects")) _collectCount = Convert.ToInt32(progress["collects"]);
			if (progress.ContainsKey("visits")) _visitCount = Convert.ToInt32(progress["visits"]);
			if (progress.ContainsKey("talks")) _talkCount = Convert.ToInt32(progress["talks"]);
			if (progress.ContainsKey("dungeons")) _dungeonCount = Convert.ToInt32(progress["dungeons"]);
			if (progress.ContainsKey("skillUses")) _skillUseCount = Convert.ToInt32(progress["skillUses"]);
			if (progress.ContainsKey("gold")) _goldEarnedCount = Convert.ToInt32(progress["gold"]);
			if (progress.ContainsKey("exp")) _expGainedCount = Convert.ToInt32(progress["exp"]);
			if (progress.ContainsKey("potions")) _potionUseCount = Convert.ToInt32(progress["potions"]);
			if (progress.ContainsKey("crafts")) _craftCount = Convert.ToInt32(progress["crafts"]);
			if (progress.ContainsKey("trades")) _tradeCount = Convert.ToInt32(progress["trades"]);
			if (progress.ContainsKey("mounts")) _mountUseCount = Convert.ToInt32(progress["mounts"]);
			if (progress.ContainsKey("pets")) _petUseCount = Convert.ToInt32(progress["pets"]);
			if (progress.ContainsKey("trials")) _trialCount = Convert.ToInt32(progress["trials"]);
			if (progress.ContainsKey("arenaWins")) _arenaWinCount = Convert.ToInt32(progress["arenaWins"]);
		}

		// Update quest states
		if (data.ContainsKey("completedQuests"))
		{
			var completed = (Array)data["completedQuests"];
			foreach (var qid in completed)
			{
				var quest = _dailyQuests.Find(q => q.QuestId == qid.ToString());
				if (quest != null)
					quest.IsCompleted = true;
			}
		}

		if (data.ContainsKey("claimedQuests"))
		{
			var claimed = (Array)data["claimedQuests"];
			foreach (var qid in claimed)
			{
				var quest = _dailyQuests.Find(q => q.QuestId == qid.ToString());
				if (quest != null)
					quest.IsClaimed = true;
			}
		}

		GD.Print("[DailyQuestSystem] Loaded - Completed: " + _totalQuestsCompleted + ", Claimed: " + _totalQuestsClaimed);
	}

	/// <summary>
	/// 导出保存数据（实现 BaseSystem 接口）
	/// </summary>
	public override Dictionary<string, object> ExportSaveData()
	{
		var data = new Dictionary<string, object>();
		data["lastRefreshDate"] = _lastRefreshDate.ToString("o");
		data["totalQuestsCompleted"] = _totalQuestsCompleted;
		data["totalQuestsClaimed"] = _totalQuestsClaimed;
		data["totalGoldEarned"] = _totalGoldEarned;
		data["totalExpEarned"] = _totalExpEarned;

		// 任务进度
		var questProgress = new Dictionary<string, int>
		{
			{ "kills", _killCount },
			{ "collects", _collectCount },
			{ "visits", _visitCount },
			{ "talks", _talkCount },
			{ "dungeons", _dungeonCount },
			{ "skillUses", _skillUseCount },
			{ "gold", _goldEarnedCount },
			{ "exp", _expGainedCount },
			{ "potions", _potionUseCount },
			{ "crafts", _craftCount },
			{ "trades", _tradeCount },
			{ "mounts", _mountUseCount },
			{ "pets", _petUseCount },
			{ "trials", _trialCount },
			{ "arenaWins", _arenaWinCount }
		};
		data["questProgress"] = questProgress;

		// 已完成和已领取的任务
		data["completedQuests"] = _dailyQuests.FindAll(q => q.IsCompleted).ConvertAll(q => q.QuestId);
		data["claimedQuests"] = _dailyQuests.FindAll(q => q.IsClaimed).ConvertAll(q => q.QuestId);

		return data;
	}

	/// <summary>
	/// 导入保存数据（实现 BaseSystem 接口）
	/// </summary>
	public override void ImportSaveData(Dictionary<string, object> data)
	{
		Load(data);
	}
}
