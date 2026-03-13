using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProceduralStorySystem : Node
{
    // References
    private ProceduralStoryData _data;
    private ProceduralStoryDatabase _database;
    
    // Settings
    private int MaxConcurrentStories = 3;
    private int StoryCooldownMinutes = 30;
    private float AutoGenerateChance = 0.3f; // 30% chance per minute
    
    // State
    private DateTime _lastStoryTime;
    private Random _random = new Random();
    
    public override void _Ready()
    {
        _data = GetNode<ProceduralStoryData>("/root/ProceduralStoryData");
        _database = GetNode<ProceduralStoryDatabase>("/root/ProceduralStoryDatabase");
        
        // Auto-generate story check timer
        var timer = new Timer();
        timer.WaitTime = 60; // Check every minute
        timer.Autostart = true;
        timer.Connect("timeout", this, "_OnAutoGenerateTimer");
        AddChild(timer);
    }
    
    // Start a new story
    public string StartStory(string templateId = "")
    {
        if (_data.ActiveStories.Count >= MaxConcurrentStories)
            return "";
        
        // Get player level (mock for now)
        int playerLevel = 1; // TODO: Get from player data
        
        // Get template
        ProceduralStoryDatabase.StoryTemplate template;
        if (string.IsNullOrEmpty(templateId))
        {
            template = _database.GetRandomStoryTemplate(playerLevel);
        }
        else
        {
            if (!_database.StoryTemplates.ContainsKey(templateId))
                return "";
            template = _database.StoryTemplates[templateId];
        }
        
        if (template == null)
            return "";
        
        // Generate story ID
        string storyId = template.Id + "_" + DateTime.Now.Ticks;
        
        // Create active story
        var story = new ProceduralStoryData.ActiveStory
        {
            StoryId = storyId,
            StoryName = template.Name,
            State = ProceduralStoryData.StoryState.Active,
            CurrentChapter = 0,
            TotalChapters = _random.Next(template.MinChapters, template.MaxChapters + 1),
            Progress = 0,
            Tension = 10,
            StartTime = DateTime.Now,
            LastUpdateTime = DateTime.Now
        };
        
        _data.ActiveStories[storyId] = story;
        _data.TotalStoriesStarted++;
        _lastStoryTime = DateTime.Now;
        
        // Generate first chapter
        GenerateNextChapter(storyId);
        
        return storyId;
    }
    
    // Generate next chapter for a story
    private void GenerateNextChapter(string storyId)
    {
        if (!_data.ActiveStories.ContainsKey(storyId))
            return;
        
        var story = _data.ActiveStories[storyId];
        
        // Determine chapter type based on position
        string chapterType;
        float progress = (float)story.CurrentChapter / story.TotalChapters;
        
        if (progress < 0.15f)
            chapterType = "Introduction";
        else if (progress < 0.5f)
            chapterType = "RisingAction";
        else if (progress < 0.7f)
            chapterType = "Complication";
        else if (progress < 0.9f)
            chapterType = "Climax";
        else
            chapterType = "Resolution";
        
        var chapterTemplate = _database.GetChapterTemplateByType(chapterType);
        
        if (chapterTemplate != null)
        {
            story.CompletedChapters.Add(chapterTemplate.Id);
            story.CurrentChapter++;
            story.Tension = Math.Min(100, story.Tension + 10);
            story.Progress = (int)(progress * 100);
        }
        
        story.LastUpdateTime = DateTime.Now;
    }
    
    // Make a choice in the story
    public bool MakeChoice(string storyId, string choice)
    {
        if (!_data.ActiveStories.ContainsKey(storyId))
            return false;
        
        var story = _data.ActiveStories[storyId];
        if (story.State != ProceduralStoryData.StoryState.Active)
            return false;
        
        // Record choice
        story.Choices[choice] = true;
        _data.TotalChoicesMade++;
        
        // Check if story should end
        if (story.CurrentChapter >= story.TotalChapters)
        {
            CompleteStory(storyId, true);
        }
        else
        {
            GenerateNextChapter(storyId);
        }
        
        return true;
    }
    
    // Complete a story
    public void CompleteStory(string storyId, bool success)
    {
        if (!_data.ActiveStories.ContainsKey(storyId))
            return;
        
        var story = _data.ActiveStories[storyId];
        story.State = success ? ProceduralStoryData.StoryState.Completed : ProceduralStoryData.StoryState.Failed;
        
        if (success)
        {
            _data.TotalStoriesCompleted++;
        }
        else
        {
            _data.TotalStoriesFailed++;
        }
        
        // Add to history
        var record = new ProceduralStoryData.StoryRecord
        {
            StoryId = story.StoryId,
            StoryName = story.StoryName,
            FinalState = story.State,
            ChaptersCompleted = story.CurrentChapter,
            TotalChapters = story.TotalChapters,
            ChoicesMade = story.Choices.Count,
            GoldEarned = 0, // Would be calculated from template rewards
            ExpEarned = 0,
            StartTime = story.StartTime,
            EndTime = DateTime.Now
        };
        
        _data.StoryHistory.Add(record);
        
        // Remove from active
        _data.ActiveStories.Remove(storyId);
    }
    
    // Update tension based on player actions
    public void UpdateTension(string storyId, int delta)
    {
        if (!_data.ActiveStories.ContainsKey(storyId))
            return;
        
        var story = _data.ActiveStories[storyId];
        story.Tension = Math.Clamp(story.Tension + delta, 0, 100);
    }
    
    // Auto-generate story timer
    private void _OnAutoGenerateTimer()
    {
        if (_data.ActiveStories.Count >= MaxConcurrentStories)
            return;
        
        if ((DateTime.Now - _lastStoryTime).TotalMinutes < StoryCooldownMinutes)
            return;
        
        // Random chance to generate
        if (_random.NextDouble() < AutoGenerateChance)
        {
            StartStory();
        }
    }
    
    // Get story info
    public ProceduralStoryData.ActiveStory GetStory(string storyId)
    {
        if (_data.ActiveStories.ContainsKey(storyId))
            return _data.ActiveStories[storyId];
        return null;
    }
    
    // Get all active stories
    public Dictionary<string, ProceduralStoryData.ActiveStory> GetActiveStories()
    {
        return _data.ActiveStories;
    }
    
    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_started", _data.TotalStoriesStarted },
            { "total_completed", _data.TotalStoriesCompleted },
            { "total_failed", _data.TotalStoriesFailed },
            { "total_choices", _data.TotalChoicesMade },
            { "active_stories", _data.ActiveStories.Count },
            { "history_count", _data.StoryHistory.Count }
        };
    }
    
    // Pause a story
    public void PauseStory(string storyId)
    {
        if (_data.ActiveStories.ContainsKey(storyId))
        {
            _data.ActiveStories[storyId].State = ProceduralStoryData.StoryState.Paused;
        }
    }
    
    // Resume a story
    public void ResumeStory(string storyId)
    {
        if (_data.ActiveStories.ContainsKey(storyId))
        {
            _data.ActiveStories[storyId].State = ProceduralStoryData.StoryState.Active;
            _data.ActiveStories[storyId].LastUpdateTime = DateTime.Now;
        }
    }
    
    // Fail a story
    public void FailStory(string storyId)
    {
        CompleteStory(storyId, false);
    }
    
    // Reset statistics
    public void ResetStatistics()
    {
        _data.TotalStoriesStarted = 0;
        _data.TotalStoriesCompleted = 0;
        _data.TotalStoriesFailed = 0;
        _data.TotalChoicesMade = 0;
        _data.TotalGoldEarned = 0;
        _data.TotalExpEarned = 0;
    }
    
    // Clear story history
    public void ClearHistory()
    {
        _data.StoryHistory.Clear();
    }
}
