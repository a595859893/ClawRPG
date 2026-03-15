using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 随机任务系统 - 生成和管理随机任务
/// </summary>
public class RandomQuestSystem
{
    private static RandomQuestSystem _instance;
    public static RandomQuestSystem Instance => _instance ??= new RandomQuestSystem();
    
    private RandomQuestData _data;
    private RandomQuestDatabase _database;
    private Timer _questTimer;
    
    public event Action<RandomQuestData.ActiveQuest> OnQuestStarted;
    public event Action<RandomQuestData.ActiveQuest> OnQuestCompleted;
    public event Action<RandomQuestData.ActiveQuest> OnQuestFailed;
    public event Action<RandomQuestData.ActiveQuest> OnQuestProgress;
    
    public RandomQuestData Data => _data;
    public List<RandomQuestData.ActiveQuest> ActiveQuests => _data?.ActiveQuests;
    public int TotalQuestsGenerated => _data?.TotalQuestsGenerated ?? 0;
    public int TotalQuestsCompleted => _data?.TotalQuestsCompleted ?? 0;
    public int TotalQuestsFailed => _data?.TotalQuestsFailed ?? 0;
    
    public RandomQuestSystem()
    {
        _database = RandomQuestDatabase.Instance;
    }
    
    public void Initialize(RandomQuestData data, Node timerParent = null)
    {
        _data = data;
        if (_data.ActiveQuests == null)
            _data.ActiveQuests = new List<RandomQuestData.ActiveQuest>();
        if (_data.CompletedQuestIds == null)
            _data.CompletedQuestIds = new List<string>();
        if (_data.FailedQuestIds == null)
            _data.FailedQuestIds = new List<string>();
        if (_data.QuestCompletionCount == null)
            _data.QuestCompletionCount = new Dictionary<string, int>();
        
        // Setup timer for quest countdown
        _questTimer = new Timer();
        _questTimer.WaitTime = 1.0f;
        _questTimer.Autostart = true;
        _questTimer.Name = "QuestTimer";
        
        if (timerParent != null)
        {
            timerParent.AddChild(_questTimer);
            _questTimer.Connect("timeout", this, nameof(OnTimerTick));
        }
    }
    
    public void Load(RandomQuestData data)
    {
        _data = data;
    }
    
    private void OnTimerTick()
    {
        if (_data == null || _data.ActiveQuests == null) return;
        
        List<RandomQuestData.ActiveQuest> toRemove = new List<RandomQuestData.ActiveQuest>();
        
        foreach (var quest in _data.ActiveQuests)
        {
            quest.TimeLimit--;
            
            if (quest.TimeLimit <= 0)
            {
                toRemove.Add(quest);
                _data.TotalQuestsFailed++;
                _data.FailedQuestIds.Add(quest.QuestId);
                OnQuestFailed?.Invoke(quest);
            }
        }
        
        foreach (var quest in toRemove)
        {
            _data.ActiveQuests.Remove(quest);
        }
    }
    
    public List<RandomQuestData.ActiveQuest> GenerateQuests(int count, int playerLevel)
    {
        if (_data == null || _database == null) return new List<RandomQuestData.ActiveQuest>();
        
        List<RandomQuestData.ActiveQuest> newQuests = new List<RandomQuestData.ActiveQuest>();
        
        // Generate random quests
        var questTemplates = _database.GetRandomQuests(count, playerLevel);
        
        foreach (var template in questTemplates)
        {
            // Check if already active or completed recently
            bool alreadyActive = false;
            if (_data.ActiveQuests != null)
            {
                foreach (var active in _data.ActiveQuests)
                {
                    if (active.QuestId == template.Id)
                    {
                        alreadyActive = true;
                        break;
                    }
                }
            }
            
            if (alreadyActive) continue;
            
            RandomQuestData.ActiveQuest quest = new RandomQuestData.ActiveQuest
            {
                QuestId = template.Id,
                Title = template.Title,
                Description = template.Description,
                Type = template.Type,
                Difficulty = template.Difficulty,
                RequiredAmount = template.RequiredAmount,
                CurrentAmount = 0,
                TimeLimit = template.TimeLimit,
                RewardGold = (int)(template.BaseRewardGold * template.DifficultyMultiplier),
                RewardExp = (int)(template.BaseRewardExp * template.DifficultyMultiplier),
                TargetId = template.TargetId,
                StartTime = DateTime.Now
            };
            
            _data.ActiveQuests.Add(quest);
            newQuests.Add(quest);
            
            _data.TotalQuestsGenerated++;
            
            OnQuestStarted?.Invoke(quest);
        }
        
        return newQuests;
    }
    
    public void UpdateQuestProgress(string targetId, int amount = 1)
    {
        if (_data == null || _data.ActiveQuests == null) return;
        
        foreach (var quest in _data.ActiveQuests)
        {
            if (quest.TargetId == targetId || targetId == "any")
            {
                quest.CurrentAmount += amount;
                
                if (quest.CurrentAmount >= quest.RequiredAmount)
                {
                    CompleteQuest(quest);
                }
                else
                {
                    OnQuestProgress?.Invoke(quest);
                }
            }
        }
    }
    
    public void UpdateQuestProgressByType(string questType, int amount = 1)
    {
        if (_data == null || _data.ActiveQuests == null) return;
        
        foreach (var quest in _data.ActiveQuests)
        {
            if (quest.Type == questType)
            {
                quest.CurrentAmount += amount;
                
                if (quest.CurrentAmount >= quest.RequiredAmount)
                {
                    CompleteQuest(quest);
                }
                else
                {
                    OnQuestProgress?.Invoke(quest);
                }
            }
        }
    }
    
    public void UpdateQuestProgressById(string questId, int amount = 1)
    {
        if (_data == null || _data.ActiveQuests == null) return;
        
        foreach (var quest in _data.ActiveQuests)
        {
            if (quest.QuestId == questId)
            {
                quest.CurrentAmount += amount;
                
                if (quest.CurrentAmount >= quest.RequiredAmount)
                {
                    CompleteQuest(quest);
                }
                else
                {
                    OnQuestProgress?.Invoke(quest);
                }
                break;
            }
        }
    }
    
    private void CompleteQuest(RandomQuestData.ActiveQuest quest)
    {
        _data.ActiveQuests.Remove(quest);
        _data.CompletedQuestIds.Add(quest.QuestId);
        _data.TotalQuestsCompleted++;
        _data.TotalQuestRewards += quest.RewardGold;
        
        if (!_data.QuestCompletionCount.ContainsKey(quest.QuestId))
            _data.QuestCompletionCount[quest.QuestId] = 0;
        _data.QuestCompletionCount[quest.QuestId]++;
        
        OnQuestCompleted?.Invoke(quest);
    }
    
    public void AbandonQuest(string questId)
    {
        if (_data == null || _data.ActiveQuests == null) return;
        
        RandomQuestData.ActiveQuest toRemove = null;
        foreach (var quest in _data.ActiveQuests)
        {
            if (quest.QuestId == questId)
            {
                toRemove = quest;
                break;
            }
        }
        
        if (toRemove != null)
        {
            _data.ActiveQuests.Remove(toRemove);
            _data.TotalQuestsFailed++;
            _data.FailedQuestIds.Add(questId);
            OnQuestFailed?.Invoke(toRemove);
        }
    }
    
    public void RefreshQuests(int playerLevel)
    {
        if (_data == null || _database == null) return;
        
        // Clear current quests
        _data.ActiveQuests.Clear();
        
        // Generate new quests
        GenerateQuests(3, playerLevel);
    }
    
    public Dictionary<string, object> GetStatistics()
    {
        Dictionary<string, object> stats = new Dictionary<string, object>();
        
        if (_data != null)
        {
            stats["total_generated"] = _data.TotalQuestsGenerated;
            stats["total_completed"] = _data.TotalQuestsCompleted;
            stats["total_failed"] = _data.TotalQuestsFailed;
            stats["total_rewards"] = _data.TotalQuestRewards;
            stats["completion_rate"] = _data.TotalQuestsGenerated > 0 
                ? (float)_data.TotalQuestsCompleted / _data.TotalQuestsGenerated 
                : 0f;
            stats["active_quests"] = _data.ActiveQuests?.Count ?? 0;
        }
        
        return stats;
    }
    
    public float GetCompletionRate()
    {
        if (_data == null || _data.TotalQuestsGenerated == 0) return 0f;
        return (float)_data.TotalQuestsCompleted / _data.TotalQuestsGenerated;
    }
}
