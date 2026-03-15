using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 公会排行数据 - 存储公会成员排名信息
/// </summary>
public class GuildRankData
{
	public int MemberId { get; set; }
	public string MemberName { get; set; } = "";
	public int RankPoints { get; set; }
	public int RankLevel { get; set; } = 1; // 1-10
	public int WinCount { get; set; }
	public int LossCount { get; set; }
	public int CurrentStreak { get; set; }
	public int BestStreak { get; set; }
	public int SeasonWins { get; set; }
	public int SeasonLosses { get; set; }
	public DateTime LastMatchTime { get; set; }
}

public enum GuildRankTier
{
	Bronze = 1,
	Silver = 2,
	Gold = 3,
	Platinum = 4,
	Diamond = 5,
	Master = 6,
	GrandMaster = 7,
	Champion = 8,
	Legend = 9,
	Supreme = 10
}

public class GuildRankSystem : Node
{
	private Dictionary<int, GuildRankData> memberRanks = new Dictionary<int, GuildRankData>();
	private int currentSeason = 1;
	private DateTime seasonStartDate;
	private int[] tierThresholds = { 0, 100, 250, 500, 1000, 2000, 3500, 5500, 8000, 12000 };
	private string[] tierNames = { "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master", "GrandMaster", "Champion", "Legend", "Supreme" };
	
	public override void _Ready()
	{
		seasonStartDate = DateTime.Now;
		GD.Print("🏆 Guild Rank System initialized - Season " + currentSeason + " started!");
	}
	
	public void InitializeMember(int memberId, string memberName)
	{
		if (!memberRanks.ContainsKey(memberId))
		{
			memberRanks[memberId] = new GuildRankData
			{
				MemberId = memberId,
				MemberName = memberName,
				RankPoints = 100,
				RankLevel = 1,
				WinCount = 0,
				LossCount = 0,
				CurrentStreak = 0,
				BestStreak = 0,
				SeasonWins = 0,
				SeasonLosses = 0,
				LastMatchTime = DateTime.MinValue
			};
			GD.Print($"✅ Member {memberName} initialized with rank Bronze (100 points)");
		}
	}
	
	public GuildRankData GetMemberRank(int memberId)
	{
		if (memberRanks.ContainsKey(memberId))
		{
			return memberRanks[memberId];
		}
		return null;
	}
	
	public int CalculateMatchResult(int memberId, bool isVictory, int matchRating = 1000)
	{
		if (!memberRanks.ContainsKey(memberId))
		{
			GD.PrintErr($"Member {memberId} not found in rank system");
			return 0;
		}
		
		var rank = memberRanks[memberId];
		int pointsChange = 0;
		
		// Base points calculation
		int basePoints = isVictory ? 25 : 10;
		
		// Streak bonus
		if (isVictory)
		{
			rank.CurrentStreak++;
			if (rank.CurrentStreak > rank.BestStreak)
			{
				rank.BestStreak = rank.CurrentStreak;
			}
			// Streak bonus: +5 points per streak level (max +50)
			int streakBonus = Math.Min(rank.CurrentStreak * 5, 50);
			pointsChange = basePoints + streakBonus;
		}
		else
		{
			rank.CurrentStreak = 0;
			// Loss penalty based on tier (higher tier = more to lose)
			int tierPenalty = rank.RankLevel * 2;
			pointsChange = -(basePoints - 5 + tierPenalty);
		}
		
		// Rating-based adjustment
		int ratingDiff = matchRating - 1000;
		if (isVictory)
		{
			// Win against stronger opponent = bonus
			if (ratingDiff > 100) pointsChange += 10;
			if (ratingDiff > 300) pointsChange += 15;
			if (ratingDiff > 500) pointsChange += 20;
		}
		else
		{
			// Lose against weaker opponent = reduced penalty
			if (ratingDiff < -100) pointsChange += 5;
			if (ratingDiff < -300) pointsChange += 10;
		}
		
		// Apply points change
		rank.RankPoints = Math.Max(0, rank.RankPoints + pointsChange);
		
		// Update win/loss counts
		if (isVictory)
		{
			rank.WinCount++;
			rank.SeasonWins++;
		}
		else
		{
			rank.LossCount++;
			rank.SeasonLosses++;
		}
		
		rank.LastMatchTime = DateTime.Now;
		
		// Update tier
		UpdateTier(rank);
		
		GD.Print($"🎮 Match result: {(isVictory ? "VICTORY" : "DEFEAT")} | Points: {pointsChange:+0;-0} | " +
			$"Total: {rank.RankPoints} | Tier: {GetTierName(rank.RankLevel)}");
		
		return pointsChange;
	}
	
	private void UpdateTier(GuildRankData rank)
	{
		int newTier = 1;
		for (int i = 9; i >= 0; i--)
		{
			if (rank.RankPoints >= tierThresholds[i])
			{
				newTier = i + 1;
				break;
			}
		}
		
		if (newTier != rank.RankLevel)
		{
			rank.RankLevel = newTier;
			GD.Print($"🎉 {rank.MemberName} promoted to {GetTierName(newTier)}!");
		}
	}
	
	public string GetTierName(int tierLevel)
	{
		if (tierLevel >= 1 && tierLevel <= 10)
		{
			return tierNames[tierLevel - 1];
		}
		return "Bronze";
	}
	
	public string GetTierColor(int tierLevel)
	{
		switch (tierLevel)
		{
			case 1: return "#CD7F32"; // Bronze
			case 2: return "#C0C0C0"; // Silver
			case 3: return "#FFD700"; // Gold
			case 4: return "#E5E4E2"; // Platinum
			case 5: return "#B9F2FF"; // Diamond
			case 6: return "#9D4EED"; // Master
			case 7: return "#FF6B6B"; // GrandMaster
			case 8: return "#FF1493"; // Champion
			case 9: return "#FF4500"; // Legend
			case 10: return "#FF0000"; // Supreme
			default: return "#CD7F32";
		}
	}
	
	public List<GuildRankData> GetLeaderboard(int limit = 10)
	{
		var leaderboard = new List<GuildRankData>(memberRanks.Values);
		leaderboard.Sort((a, b) => b.RankPoints.CompareTo(a.RankPoints));
		
		if (leaderboard.Count > limit)
		{
			return leaderboard.GetRange(0, limit);
		}
		return leaderboard;
	}
	
	public Dictionary<string, object> GetSeasonStats()
	{
		int totalWins = 0;
		int totalLosses = 0;
		int totalMembers = memberRanks.Count;
		
		foreach (var rank in memberRanks.Values)
		{
			totalWins += rank.SeasonWins;
			totalLosses += rank.SeasonLosses;
		}
		
		return new Dictionary<string, object>
		{
			{ "season", currentSeason },
			{ "startDate", seasonStartDate.ToString("yyyy-MM-dd") },
			{ "totalMembers", totalMembers },
			{ "totalWins", totalWins },
			{ "totalLosses", totalLosses },
			{ "winRate", totalLosses > 0 ? (double)totalWins / (totalWins + totalLosses) * 100 : 0 }
		};
	}
	
	public void StartNewSeason()
	{
		currentSeason++;
		seasonStartDate = DateTime.Now;
		
		// Reset season stats but keep lifetime stats
		foreach (var rank in memberRanks.Values)
		{
			rank.SeasonWins = 0;
			rank.SeasonLosses = 0;
		}
		
		GD.Print($"🔄 New season {currentSeason} started! Previous season stats preserved.");
	}
	
	public Dictionary<string, object> Serialize()
	{
		var data = new Dictionary<string, object>();
		data["currentSeason"] = currentSeason;
		data["seasonStartDate"] = seasonStartDate.ToString("o");
		
		var members = new List<Dictionary<string, object>>();
		foreach (var kvp in memberRanks)
		{
			var rank = kvp.Value;
			members.Add(new Dictionary<string, object>
			{
				{ "memberId", rank.MemberId },
				{ "memberName", rank.MemberName },
				{ "rankPoints", rank.RankPoints },
				{ "rankLevel", rank.RankLevel },
				{ "winCount", rank.WinCount },
				{ "lossCount", rank.LossCount },
				{ "currentStreak", rank.CurrentStreak },
				{ "bestStreak", rank.BestStreak },
				{ "seasonWins", rank.SeasonWins },
				{ "seasonLosses", rank.SeasonLosses },
				{ "lastMatchTime", rank.LastMatchTime.ToString("o") }
			});
		}
		data["members"] = members;
		
		return data;
	}
	
	public void Deserialize(Dictionary<string, object> data)
	{
		if (data.ContainsKey("currentSeason"))
			currentSeason = (int)data["currentSeason"];
		
		if (data.ContainsKey("seasonStartDate"))
			DateTime.TryParse((string)data["seasonStartDate"], out seasonStartDate);
		
		if (data.ContainsKey("members"))
		{
			var members = (List<object>)data["members"];
			foreach (var memberData in members)
			{
				var dict = (Dictionary<string, object>)memberData;
				var rank = new GuildRankData
				{
					MemberId = (int)dict["memberId"],
					MemberName = (string)dict["memberName"],
					RankPoints = (int)dict["rankPoints"],
					RankLevel = (int)dict["rankLevel"],
					WinCount = (int)dict["winCount"],
					LossCount = (int)dict["lossCount"],
					CurrentStreak = (int)dict["currentStreak"],
					BestStreak = (int)dict["bestStreak"],
					SeasonWins = (int)dict["seasonWins"],
					SeasonLosses = (int)dict["seasonLosses"]
				};
				if (dict.ContainsKey("lastMatchTime"))
					DateTime.TryParse((string)dict["lastMatchTime"], out rank.LastMatchTime);
				
				memberRanks[rank.MemberId] = rank;
			}
		}
	}
}
