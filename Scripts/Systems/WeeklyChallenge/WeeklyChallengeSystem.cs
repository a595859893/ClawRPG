using Godot;
using System;
using System.Collections.Generic;

public partial class WeeklyChallengeSystem : BaseSystem
{
    public static WeeklyChallengeSystem Instance { get; private set; }
    
    [Export] public WeeklyChallengeData Data { get; set; }
    
    private const int WeekDurationDays = 7;
    
    public override void _Ready()
    {
        Instance = this;
        LoadData();
        CheckWeekReset();
    }
    
    private void LoadData()
    {
        if (Data == null)
        {
            Data = new WeeklyChallengeData();
            GenerateNewWeek();
        }
    }
    
    private void CheckWeekReset()
    {
        var now = DateTime.Now;
        
        // Check if week has ended
        if (now >= Data.EndTime)
        {
            // Start new week
            GenerateNewWeek();
        }
    }
    
    private void GenerateNewWeek()
    {
        var now = DateTime.Now;
        
        // Calculate week number
        var weekNumber = GetWeekNumber(now);
        
        Data = new WeeklyChallengeData
        {
            WeekNumber = weekNumber,
            Year = now.Year,
            Challenges = new Dictionary<string, WeeklyChallenge>(),
            TotalPoints = 0,
            CompletedChallenges = 0,
            StartTime = now,
            EndTime = now.AddDays(WeekDurationDays),
            RewardsClaimed = false
        };
        
        // Generate random challenges
        var challenges = WeeklyChallengeDatabase.GenerateWeeklyChallenges();
        foreach (var challenge in challenges)
        {
            Data.Challenges[challenge.Id] = challenge;
        }
        
        SaveData();
    }
    
    private int GetWeekNumber(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
    
    public void UpdateChallengeProgress(string challengeId, int amount)
    {
        if (Data.Challenges.ContainsKey(challengeId))
        {
            var challenge = Data.Challenges[challengeId];
            challenge.CurrentValue += amount;
            
            if (challenge.CurrentValue >= challenge.TargetValue && !challenge.IsCompleted)
            {
                challenge.IsCompleted = true;
                Data.CompletedChallenges++;
                Data.TotalPoints += challenge.Points;
                
                // Emit signal
                EmitSignal(nameof(ChallengeCompleted), challengeId);
            }
            
            SaveData();
            EmitSignal(nameof(ChallengeProgressUpdated), challengeId, challenge.CurrentValue, challenge.TargetValue);
        }
    }
    
    public void SetChallengeProgress(string challengeId, int value)
    {
        if (Data.Challenges.ContainsKey(challengeId))
        {
            var challenge = Data.Challenges[challengeId];
            int oldValue = challenge.CurrentValue;
            challenge.CurrentValue = value;
            
            if (challenge.CurrentValue >= challenge.TargetValue && !challenge.IsCompleted)
            {
                challenge.IsCompleted = true;
                Data.CompletedChallenges++;
                Data.TotalPoints += challenge.Points;
                
                EmitSignal(nameof(ChallengeCompleted), challengeId);
            }
            else if (challenge.CurrentValue < challenge.TargetValue && challenge.IsCompleted)
            {
                challenge.IsCompleted = false;
                Data.CompletedChallenges--;
                Data.TotalPoints -= challenge.Points;
            }
            
            SaveData();
            EmitSignal(nameof(ChallengeProgressUpdated), challengeId, challenge.CurrentValue, challenge.TargetValue);
        }
    }
    
    public Dictionary<string, WeeklyChallenge> GetChallenges()
    {
        return Data.Challenges;
    }
    
    public List<WeeklyChallenge> GetChallengesByType(ChallengeType type)
    {
        var result = new List<WeeklyChallenge>();
        foreach (var challenge in Data.Challenges.Values)
        {
            if (challenge.Type == type)
            {
                result.Add(challenge);
            }
        }
        return result;
    }
    
    public int GetTotalPoints()
    {
        return Data.TotalPoints;
    }
    
    public int GetCompletedCount()
    {
        return Data.CompletedChallenges;
    }
    
    public int GetTotalCount()
    {
        return Data.Challenges.Count;
    }
    
    public TimeSpan GetTimeRemaining()
    {
        var now = DateTime.Now;
        if (now < Data.EndTime)
        {
            return Data.EndTime - now;
        }
        return TimeSpan.Zero;
    }
    
    public bool CanClaimRewards()
    {
        return Data.CompletedChallenges > 0 && !Data.RewardsClaimed;
    }
    
    public void ClaimRewards()
    {
        if (!CanClaimRewards()) return;
        
        // Calculate rewards based on completed challenges
        int totalGold = 0;
        int totalExp = 0;
        
        foreach (var challenge in Data.Challenges.Values)
        {
            if (challenge.IsCompleted)
            {
                totalGold += challenge.RewardGold;
                totalExp += challenge.RewardExp;
            }
        }
        
        // Bonus for completion percentage
        double completionRate = (double)Data.CompletedChallenges / Data.Challenges.Count;
        if (completionRate >= 1.0)
        {
            totalGold = (int)(totalGold * 1.5);
            totalExp = (int)(totalExp * 1.5);
        }
        else if (completionRate >= 0.75)
        {
            totalGold = (int)(totalGold * 1.25);
            totalExp = (int)(totalExp * 1.25);
        }
        
        // Add rewards to player
        var player = GetTree().GetFirstNodeInGroup("player");
        if (player != null)
        {
            // Assuming player has gold and exp properties
            player.Set("gold", (int)player.Get("gold") + totalGold);
            player.Set("experience", (int)player.Get("experience") + totalExp);
        }
        
        Data.RewardsClaimed = true;
        SaveData();
        
        EmitSignal(nameof(RewardsClaimed), totalGold, totalExp);
    }
    
    public bool IsNewWeek()
    {
        return DateTime.Now >= Data.EndTime;
    }
    
    public void SaveData()
    {
        // Save to file
        var savePath = "user://weekly_challenge_save.dat";
        using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            var json = Json.Stringify(new Dictionary<string, object>
            {
                ["week"] = Data.WeekNumber,
                ["year"] = Data.Year,
                ["total_points"] = Data.TotalPoints,
                ["completed"] = Data.CompletedChallenges,
                ["rewards_claimed"] = Data.RewardsClaimed,
                ["end_time"] = Data.EndTime.ToString("o")
            });
            file.StoreString(json);
            file.Close();
        }
    }
    
    private void LoadFromFile()
    {
        var savePath = "user://weekly_challenge_save.dat";
        if (FileAccess.FileExists(savePath))
        {
            using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                var json = file.GetAsText();
                var data = Json.ParseString(json).AsDictionary();
                
                if (data != null)
                {
                    Data.WeekNumber = (int)data["week"];
                    Data.Year = (int)data["year"];
                    Data.TotalPoints = (int)data["total_points"];
                    Data.CompletedChallenges = (int)data["completed"];
                    Data.RewardsClaimed = (bool)data["rewards_claimed"];
                    Data.EndTime = DateTime.Parse((string)data["end_time"]);
                }
                file.Close();
            }
        }
    }
    
    // Signals
public delegate void ChallengeCompletedEventHandler(string challengeId);
public delegate void ChallengeProgressUpdatedEventHandler(string challengeId, int current, int target);
public delegate void RewardsClaimedEventHandler(int gold, int exp);

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        if (Data == null) return data;

        data["week"] = Data.WeekNumber;
        data["year"] = Data.Year;
        data["total_points"] = Data.TotalPoints;
        data["completed"] = Data.CompletedChallenges;
        data["rewards_claimed"] = Data.RewardsClaimed;
        data["end_time"] = Data.EndTime.ToString("o");

        // 保存已完成挑战的进度
        var completedChallenges = new List<Dictionary<string, Variant>>();
        if (Data.Challenges != null)
        {
            foreach (var kvp in Data.Challenges)
            {
                if (kvp.Value.IsCompleted)
                {
                    completedChallenges.Add(new Dictionary<string, Variant>
                    {
                        ["id"] = kvp.Key,
                        ["current_value"] = kvp.Value.CurrentValue,
                        ["is_completed"] = kvp.Value.IsCompleted
                    });
                }
            }
        }
        data["completed_challenges"] = completedChallenges;

        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null || Data == null) return;

        if (data.TryGetValue("week", out var week))
            Data.WeekNumber = (int)week;
        if (data.TryGetValue("year", out var year))
            Data.Year = (int)year;
        if (data.TryGetValue("total_points", out var totalPoints))
            Data.TotalPoints = (int)totalPoints;
        if (data.TryGetValue("completed", out var completed))
            Data.CompletedChallenges = (int)completed;
        if (data.TryGetValue("rewards_claimed", out var rewardsClaimed))
            Data.RewardsClaimed = (bool)rewardsClaimed;
        if (data.TryGetValue("end_time", out var endTimeStr) && DateTime.TryParse((string)endTimeStr, out var parsedEndTime))
            Data.EndTime = parsedEndTime;

        // 恢复已完成挑战的进度
        if (data.TryGetValue("completed_challenges", out var completedData))
        {
            var completedList = (List<Variant>)completedData;
            foreach (var challengeData in completedList)
            {
                var cd = (Dictionary<string, Variant>)challengeData;
                if (cd.TryGetValue("id", out var id) && Data.Challenges.ContainsKey((string)id))
                {
                    if (cd.TryGetValue("current_value", out var currentValue))
                        Data.Challenges[(string)id].CurrentValue = (int)currentValue;
                    if (cd.TryGetValue("is_completed", out var isCompleted))
                        Data.Challenges[(string)id].IsCompleted = (bool)isCompleted;
                }
            }
        }
    }

}
