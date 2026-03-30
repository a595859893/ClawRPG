using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProceduralStoryData : BaseSystem
{
    // Story State
    public enum StoryState { Inactive, Active, Paused, Completed, Failed }
    
    // Current active stories
    public Dictionary<string, ActiveStory> ActiveStories = new Dictionary<string, ActiveStory>();
    
    // Story history
    public List<StoryRecord> StoryHistory = new List<StoryRecord>();
    
    // Statistics
    public int TotalStoriesStarted = 0;
    public int TotalStoriesCompleted = 0;
    public int TotalStoriesFailed = 0;
    public int TotalChoicesMade = 0;
    public int TotalGoldEarned = 0;
    public int TotalExpEarned = 0;
    
    [Serializable]
    public class ActiveStory
    {
        public string StoryId;
        public string StoryName;
        public StoryState State;
        public int CurrentChapter;
        public int TotalChapters;
        public List<string> CompletedChapters = new List<string>();
        public Dictionary<string, bool> Choices = new Dictionary<string, bool>();
        public int Progress; // 0-100
        public int Tension; // 0-100, story tension level
        public DateTime StartTime;
        public DateTime LastUpdateTime;
    }
    
    [Serializable]
    public class StoryRecord
    {
        public string StoryId;
        public string StoryName;
        public StoryState FinalState;
        public int ChaptersCompleted;
        public int TotalChapters;
        public int ChoicesMade;
        public int GoldEarned;
        public int ExpEarned;
        public DateTime StartTime;
        public DateTime EndTime;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 统计数据
        data["total_stories_started"] = TotalStoriesStarted;
        data["total_stories_completed"] = TotalStoriesCompleted;
        data["total_stories_failed"] = TotalStoriesFailed;
        data["total_choices_made"] = TotalChoicesMade;
        data["total_gold_earned"] = TotalGoldEarned;
        data["total_exp_earned"] = TotalExpEarned;
        
        // 当前进行的故事
        var activeStoriesList = new Array();
        foreach (var kvp in ActiveStories)
        {
            var storyDict = new Dictionary
            {
                { "story_id", kvp.Value.StoryId },
                { "story_name", kvp.Value.StoryName },
                { "state", (int)kvp.Value.State },
                { "current_chapter", kvp.Value.CurrentChapter },
                { "total_chapters", kvp.Value.TotalChapters },
                { "progress", kvp.Value.Progress },
                { "tension", kvp.Value.Tension }
            };
            
            var completedChapters = new Array();
            foreach (var chapter in kvp.Value.CompletedChapters)
            {
                completedChapters.Add(chapter);
            }
            storyDict["completed_chapters"] = completedChapters;
            
            var choices = new Dictionary<string, object>();
            foreach (var choice in kvp.Value.Choices)
            {
                choices[choice.Key] = choice.Value;
            }
            storyDict["choices"] = choices;
            
            activeStoriesList.Add(storyDict);
        }
        data["active_stories"] = activeStoriesList;
        
        // 故事历史
        var historyList = new Array();
        foreach (var record in StoryHistory)
        {
            var recordDict = new Dictionary
            {
                { "story_id", record.StoryId },
                { "story_name", record.StoryName },
                { "final_state", (int)record.FinalState },
                { "chapters_completed", record.ChaptersCompleted },
                { "total_chapters", record.TotalChapters },
                { "choices_made", record.ChoicesMade },
                { "gold_earned", record.GoldEarned },
                { "exp_earned", record.ExpEarned }
            };
            historyList.Add(recordDict);
        }
        data["story_history"] = historyList;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 统计数据
        TotalStoriesStarted = (int)data.GetValueOrDefault("total_stories_started", 0);
        TotalStoriesCompleted = (int)data.GetValueOrDefault("total_stories_completed", 0);
        TotalStoriesFailed = (int)data.GetValueOrDefault("total_stories_failed", 0);
        TotalChoicesMade = (int)data.GetValueOrDefault("total_choices_made", 0);
        TotalGoldEarned = (int)data.GetValueOrDefault("total_gold_earned", 0);
        TotalExpEarned = (int)data.GetValueOrDefault("total_exp_earned", 0);
        
        // 当前进行的故事
        ActiveStories = new Dictionary<string, ActiveStory>();
        if (data.Contains("active_stories"))
        {
            var storiesArray = (Array)data["active_stories"];
            foreach (Dictionary storyDict in storiesArray)
            {
                var story = new ActiveStory
                {
                    StoryId = (string)storyDict["story_id"],
                    StoryName = (string)storyDict["story_name"],
                    State = (StoryState)(int)storyDict["state"],
                    CurrentChapter = (int)storyDict["current_chapter"],
                    TotalChapters = (int)storyDict["total_chapters"],
                    Progress = (int)storyDict["progress"],
                    Tension = (int)storyDict["tension"]
                };
                
                if (storyDict.Contains("completed_chapters"))
                {
                    var chaptersArray = (Array)storyDict["completed_chapters"];
                    story.CompletedChapters = new List<string>();
                    foreach (string chapter in chaptersArray)
                    {
                        story.CompletedChapters.Add(chapter);
                    }
                }
                
                if (storyDict.Contains("choices"))
                {
                    var choicesDict = (Dictionary)storyDict["choices"];
                    story.Choices = new Dictionary<string, bool>();
                    foreach (var choice in choicesDict)
                    {
                        story.Choices[choice.Key] = (bool)choice.Value;
                    }
                }
                
                ActiveStories[story.StoryId] = story;
            }
        }
        
        // 故事历史
        StoryHistory = new List<StoryRecord>();
        if (data.Contains("story_history"))
        {
            var historyArray = (Array)data["story_history"];
            foreach (Dictionary recordDict in historyArray)
            {
                var record = new StoryRecord
                {
                    StoryId = (string)recordDict["story_id"],
                    StoryName = (string)recordDict["story_name"],
                    FinalState = (StoryState)(int)recordDict["final_state"],
                    ChaptersCompleted = (int)recordDict["chapters_completed"],
                    TotalChapters = (int)recordDict["total_chapters"],
                    ChoicesMade = (int)recordDict["choices_made"],
                    GoldEarned = (int)recordDict["gold_earned"],
                    ExpEarned = (int)recordDict["exp_earned"]
                };
                StoryHistory.Add(record);
            }
        }
    }
}
