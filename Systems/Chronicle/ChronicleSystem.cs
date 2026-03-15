using Godot;
using System;
using System.Collections.Generic;

public class ChronicleSystem : BaseSystem
{
    public static ChronicleSystem Instance { get; private set; }
    
    // Chronicle data
    public Dictionary<string, ChronicleEntry> chronicles = new Dictionary<string, ChronicleEntry>();
    public string currentChapter = "prologue";
    public int totalChapters = 10;
    
    // Chapter definitions
    private Dictionary<string, ChapterData> chapterData = new Dictionary<string, ChapterData>()
    {
        {"prologue", new ChapterData { title = "The Beginning", description = "Your adventure begins in the village of Oakhaven.", requiredLevel = 1, requiredQuests = new List<string>() }},
        {"chapter1", new ChapterData { title = "The First Challenge", description = "Prove your worth in the training grounds.", requiredLevel = 5, requiredQuests = new List<string> {"quest_training_ground"} }},
        {"chapter2", new ChapterData { title = "Into the Wild", description = "Explore the ancient forest and discover its secrets.", requiredLevel = 10, requiredQuests = new List<string> {"quest_forest_elder"} }},
        {"chapter3", new ChapterData { title = "Mountain Trek", description = "Journey through the treacherous mountains.", requiredLevel = 15, requiredQuests = new List<string> {"quest_crystal_cave"} }},
        {"chapter4", new ChapterData { title = "Desert Kingdom", description = "Uncover the mysteries of the sunken desert empire.", requiredLevel = 20, requiredQuests = new List<string> {"quest_desert_temple"} }},
        {"chapter5", new ChapterData { title = "Volcanic Depths", description = "Descend into the fiery heart of the volcano.", requiredLevel = 25, requiredQuests = new List<string> {"quest_fire_drake"} }},
        {"chapter6", new ChapterData { title = "Frozen Wastes", description = "Survive the treacherous ice wilderness.", requiredLevel = 30, requiredQuests = new List<string> {"quest_frost_wraith"} }},
        {"chapter7", new ChapterData { title = "Shadow Realm", description = "Confront the darkness that threatens the world.", requiredLevel = 35, requiredQuests = new List<string> {"quest_shadow_lord"} }},
        {"chapter8", new ChapterData { title = "Dragon's Legacy", description = "Face the ancient dragon and uncover its truth.", requiredLevel = 40, requiredQuests = new List<string> {"quest_ancient_dragon"} }},
        {"finale", new ChapterData { title = "The Final Stand", description = "Defeat the chaos and restore balance to the world.", requiredLevel = 50, requiredQuests = new List<string> {"quest_chaos_beast"} }}
    };
    
    // Lore entries
    public List<LoreEntry> discoveredLore = new List<LoreEntry>();
    public int totalLoreEntries = 50;
    
    // Story flags
    public Dictionary<string, bool> storyFlags = new Dictionary<string, bool>();
    
    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }
    
    protected override void Initialize()
    {
        InitializeChronicle();
    }
    
    private void InitializeChronicle()
    {
        // Initialize chronicles
        chronicles["main_quest"] = new ChronicleEntry 
        { 
            type = ChronicleType.MainQuest, 
            title = "Main Quest", 
            description = "The main story progression",
            progress = 0, 
            maxProgress = 100,
            isCompleted = false 
        };
        
        chronicles["side_quests"] = new ChronicleEntry 
        { 
            type = ChronicleType.SideQuest, 
            title = "Side Quests", 
            description = "Optional adventures and tasks",
            progress = 0, 
            maxProgress = 50,
            isCompleted = false 
        };
        
        chronicles["exploration"] = new ChronicleEntry 
        { 
            type = ChronicleType.Exploration, 
            title = "Exploration", 
            description = "Discover new locations and secrets",
            progress = 0, 
            maxProgress = 30,
            isCompleted = false 
        };
        
        chronicles["combat"] = new ChronicleEntry 
        { 
            type = ChronicleType.Combat, 
            title = "Combat Chronicle", 
            description = "Track your battles and victories",
            progress = 0, 
            maxProgress = 1000,
            isCompleted = false 
        };
        
        // Initialize story flags
        storyFlags["defeated_goblin_chief"] = false;
        storyFlags["met_elders"] = false;
        storyFlags["freed_prisoners"] = false;
        storyFlags["found_artifact"] = false;
        storyFlags["completed_training"] = false;
    }
    
    public void UpdateProgress(string chronicleType, int amount)
    {
        if (chronicles.ContainsKey(chronicleType))
        {
            chronicles[chronicleType].progress += amount;
            if (chronicles[chronicleType].progress >= chronicles[chronicleType].maxProgress)
            {
                chronicles[chronicleType].isCompleted = true;
            }
        }
    }
    
    public void SetStoryFlag(string flag, bool value)
    {
        storyFlags[flag] = value;
    }
    
    public bool GetStoryFlag(string flag)
    {
        return storyFlags.ContainsKey(flag) && storyFlags[flag];
    }
    
    public void AddLoreEntry(LoreEntry entry)
    {
        if (!discoveredLore.Exists(l => l.id == entry.id))
        {
            discoveredLore.Add(entry);
        }
    }
    
    public ChapterData GetCurrentChapter()
    {
        return chapterData.ContainsKey(currentChapter) ? chapterData[currentChapter] : null;
    }
    
    public void AdvanceChapter()
    {
        if (currentChapter == "prologue") currentChapter = "chapter1";
        else if (currentChapter == "chapter1") currentChapter = "chapter2";
        else if (currentChapter == "chapter2") currentChapter = "chapter3";
        else if (currentChapter == "chapter3") currentChapter = "chapter4";
        else if (currentChapter == "chapter4") currentChapter = "chapter5";
        else if (currentChapter == "chapter5") currentChapter = "chapter6";
        else if (currentChapter == "chapter6") currentChapter = "chapter7";
        else if (currentChapter == "chapter7") currentChapter = "chapter8";
        else if (currentChapter == "chapter8") currentChapter = "finale";
    }
    
    public Dictionary<string, ChronicleEntry> GetChronicles() => chronicles;
    public List<LoreEntry> GetDiscoveredLore() => discoveredLore;
    public Dictionary<string, bool> GetStoryFlags() => storyFlags;
    
    public override Dictionary ExportSaveData()
    {
        return new Dictionary<string, object>
        {
            {"currentChapter", currentChapter},
            {"chronicles", chronicles},
            {"discoveredLore", discoveredLore},
            {"storyFlags", storyFlags}
        };
    }
    
    public override void ImportSaveData(Dictionary data)
    {
        base.ImportSaveData(data);
        if (data == null) return;
        
        if (data.ContainsKey("currentChapter")) currentChapter = (string)data["currentChapter"];
        if (data.ContainsKey("chronicles")) chronicles = (Dictionary<string, ChronicleEntry>)data["chronicles"];
        if (data.ContainsKey("discoveredLore")) discoveredLore = (List<LoreEntry>)data["discoveredLore"];
        if (data.ContainsKey("storyFlags")) storyFlags = (Dictionary<string, bool>)data["storyFlags"];
    }
}

public enum ChronicleType
{
    MainQuest,
    SideQuest,
    Exploration,
    Combat
}

public class ChronicleEntry
{
    public ChronicleType type;
    public string title;
    public string description;
    public int progress;
    public int maxProgress;
    public bool isCompleted;
}

public class ChapterData
{
    public string title;
    public string description;
    public int requiredLevel;
    public List<string> requiredQuests;
}

public class LoreEntry
{
    public string id;
    public string title;
    public string content;
    public string category;
    public bool isDiscovered;
}
