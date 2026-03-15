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
}
