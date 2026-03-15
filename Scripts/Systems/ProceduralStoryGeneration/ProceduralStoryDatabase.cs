using Godot;
using System;
using System.Collections.Generic;

public class ProceduralStoryDatabase : BaseSystem
{
    // Story Templates
    public Dictionary<string, StoryTemplate> StoryTemplates = new Dictionary<string, StoryTemplate>();
    
    // Story archetypes
    public Dictionary<string, StoryArchetype> Archetypes = new Dictionary<string, StoryArchetype>();
    
    // Chapter templates
    public Dictionary<string, ChapterTemplate> ChapterTemplates = new Dictionary<string, ChapterTemplate>();
    
    // NPC templates
    public Dictionary<string, NPCTemplate> NPCTemplates = new Dictionary<string, NPCTemplate>();
    
    // Chapter types and their weights
    public Dictionary<string, float> ChapterTypeWeights = new Dictionary<string, float>()
    {
        { "Introduction", 0.15f },
        { "RisingAction", 0.25f },
        { "Complication", 0.20f },
        { "Climax", 0.15f },
        { "Resolution", 0.15f },
        { "Epilogue", 0.10f }
    };
    
    // Story colors for UI
    public Dictionary<string, Color> StoryColors = new Dictionary<string, Color>()
    {
        { "Hero", new Color(0.2f, 0.6f, 1.0f) },
        { "Tragedy", new Color(0.8f, 0.2f, 0.2f) },
        { "Romance", new Color(1.0f, 0.4f, 0.6f) },
        { "Mystery", new Color(0.5f, 0.3f, 0.8f) },
        { "Adventure", new Color(0.2f, 0.8f, 0.4f) },
        { "Comedy", new Color(1.0f, 0.8f, 0.2f) },
        { "Legend", new Color(1.0f, 0.84f, 0.0f) }
    };
    
    [Serializable]
    public class StoryTemplate
    {
        public string Id;
        public string Name;
        public string Description;
        public string Archetype; // HeroJourney, Tragedy, RagsToRiches, ComingOfAge, Redemption, etc.
        public int MinChapters = 3;
        public int MaxChapters = 7;
        public string[] RequiredFlags = new string[0];
        public string[] ExcludedFlags = new string[0];
        public int MinPlayerLevel = 1;
        public int MaxPlayerLevel = 100;
        public int Difficulty = 1; // 1-5
        public string[] ChapterIds = new string[0];
        public Dictionary<string, int> Rewards = new Dictionary<string, int>(); // gold, exp, reputation
    }
    
    [Serializable]
    public class StoryArchetype
    {
        public string Id;
        public string Name;
        public string Description;
        public string[] TypicalThemes;
        public float TensionRate; // How fast tension builds
        public string[] RequiredEmotions;
    }
    
    [Serializable]
    public class ChapterTemplate
    {
        public string Id;
        public string Name;
        public string Type; // Introduction, RisingAction, Complication, Climax, Resolution, Epilogue
        public string Description;
        public string[] AvailableChoices;
        public string DefaultChoice;
        public int MinTension;
        public int MaxTension;
        public int BaseDuration; // in game minutes
        public Dictionary<string, int> Rewards = new Dictionary<string, int>();
    }
    
    [Serializable]
    public class NPCTemplate
    {
        public string Id;
        public string Name;
        public string Role; // Mentor, Ally, Rival, LoveInterest, Villain, Neutral
        public string[] PersonalityTraits;
        public string[] DialogueStyles;
        public Dictionary<string, string> DialogueLines; // emotion -> line
    }
    
    public override void _Ready()
    {
        InitializeStoryTemplates();
        InitializeArchetypes();
        InitializeChapterTemplates();
        InitializeNPCTemplates();
    }
    
    private void InitializeStoryTemplates()
    {
        // Hero Journey stories
        StoryTemplates["hero_chosen_one"] = new StoryTemplate
        {
            Id = "hero_chosen_one",
            Name = "The Chosen One",
            Description = "You discover you are destined to save the realm from an ancient evil.",
            Archetype = "HeroJourney",
            MinChapters = 5,
            MaxChapters = 7,
            MinPlayerLevel = 10,
            Difficulty = 3,
            Rewards = new Dictionary<string, int> { { "gold", 5000 }, { "exp", 10000 }, { "reputation", 100 } }
        };
        
        StoryTemplates["hero_redemption"] = new StoryTemplate
        {
            Id = "hero_redemption",
            Name = "Path of Redemption",
            Description = "A former enemy seeks to make amends for past wrongs.",
            Archetype = "Redemption",
            MinChapters = 4,
            MaxChapters = 6,
            MinPlayerLevel = 15,
            Difficulty = 2,
            Rewards = new Dictionary<string, int> { { "gold", 3000 }, { "exp", 8000 }, { "reputation", 50 } }
        };
        
        // Tragedy stories
        StoryTemplates["tragedy_lost_love"] = new StoryTemplate
        {
            Id = "tragedy_lost_love",
            Name = "Lost Love",
            Description = "Your beloved has been taken by dark forces. Can you save them in time?",
            Archetype = "Tragedy",
            MinChapters = 4,
            MaxChapters = 6,
            MinPlayerLevel = 5,
            Difficulty = 2,
            Rewards = new Dictionary<string, int> { { "gold", 2000 }, { "exp", 5000 }, { "reputation", 30 } }
        };
        
        // Romance stories
        StoryTemplates["romance_forbidden"] = new StoryTemplate
        {
            Id = "romance_forbidden",
            Name = "Forbidden Love",
            Description = "Love blooms between unlikely souls in a world of conflict.",
            Archetype = "Romance",
            MinChapters = 3,
            MaxChapters = 5,
            MinPlayerLevel = 1,
            Difficulty = 1,
            Rewards = new Dictionary<string, int> { { "gold", 1000 }, { "exp", 3000 }, { "reputation", 20 } }
        };
        
        // Adventure stories
        StoryTemplates["adventure_treasure"] = new StoryTemplate
        {
            Id = "adventure_treasure",
            Name = "The Lost Treasure",
            Description = "Follow the clues to discover a legendary treasure before others do.",
            Archetype = "Adventure",
            MinChapters = 3,
            MaxChapters = 5,
            MinPlayerLevel = 1,
            Difficulty = 1,
            Rewards = new Dictionary<string, int> { { "gold", 8000 }, { "exp", 5000 }, { "reputation", 40 } }
        };
        
        StoryTemplates["adventure_legendary_weapon"] = new StoryTemplate
        {
            Id = "adventure_legendary_weapon",
            Name = "Legendary Weapon",
            Description = "Embark on a quest to forge or find a weapon of immense power.",
            Archetype = "Adventure",
            MinChapters = 5,
            MaxChapters = 7,
            MinPlayerLevel = 20,
            Difficulty = 4,
            Rewards = new Dictionary<string, int> { { "gold", 10000 }, { "exp", 15000 }, { "reputation", 150 } }
        };
        
        // Mystery stories
        StoryTemplates["mystery_murder"] = new StoryTemplate
        {
            Id = "mystery_murder",
            Name = "The Murder Mystery",
            Description = "A murder has occurred. Gather clues and find the culprit.",
            Archetype = "Mystery",
            MinChapters = 4,
            MaxChapters = 6,
            MinPlayerLevel = 10,
            Difficulty = 3,
            Rewards = new Dictionary<string, int> { { "gold", 4000 }, { "exp", 7000 }, { "reputation", 80 } }
        };
        
        StoryTemplates["mystery_ancient_secret"] = new StoryTemplate
        {
            Id = "mystery_ancient_secret",
            Name = "Ancient Secret",
            Description = "Uncover the secrets of an ancient civilization.",
            Archetype = "Mystery",
            MinChapters = 4,
            MaxChapters = 6,
            MinPlayerLevel = 15,
            Difficulty = 3,
            Rewards = new Dictionary<string, int> { { "gold", 6000 }, { "exp", 9000 }, { "reputation", 100 } }
        };
        
        // Comedy stories
        StoryTemplates["comedy_chaos"] = new StoryTemplate
        {
            Id = "comedy_chaos",
            Name = "Comedic Chaos",
            Description = "Everything that can go wrong does in this hilarious adventure.",
            Archetype = "Comedy",
            MinChapters = 3,
            MaxChapters = 5,
            MinPlayerLevel = 1,
            Difficulty = 1,
            Rewards = new Dictionary<string, int> { { "gold", 1500 }, { "exp", 2000 }, { "reputation", 10 } }
        };
        
        // Legend stories
        StoryTemplates["legend_gods"] = new StoryTemplate
        {
            Id = "legend_gods",
            Name = "War of the Gods",
            Description = "Immortal beings clash, and mortals are caught in the middle.",
            Archetype = "Legend",
            MinChapters = 5,
            MaxChapters = 7,
            MinPlayerLevel = 30,
            Difficulty = 5,
            Rewards = new Dictionary<string, int> { { "gold", 20000 }, { "exp", 25000 }, { "reputation", 200 } }
        };
    }
    
    private void InitializeArchetypes()
    {
        Archetypes["HeroJourney"] = new StoryArchetype
        {
            Id = "HeroJourney",
            Name = "Hero's Journey",
            Description = "The classic hero's adventure: call to action, trials, and return transformed.",
            TypicalThemes = new string[] { "courage", "sacrifice", "transformation" },
            TensionRate = 1.0f,
            RequiredEmotions = new string[] { "determination" }
        };
        
        Archetypes["Tragedy"] = new StoryArchetype
        {
            Id = "Tragedy",
            Name = "Tragedy",
            Description = "A tale of loss, downfall, and the truest consequences.",
            TypicalThemes = new string[] { "loss", "grief", "sacrifice" },
            TensionRate = 1.5f,
            RequiredEmotions = new string[] { "sadness" }
        };
        
        Archetypes["Romance"] = new StoryArchetype
        {
            Id = "Romance",
            Name = "Romance",
            Description = "Love blossoms against all odds.",
            TypicalThemes = new string[] { "love", "devotion", "overcoming_obstacles" },
            TensionRate = 0.8f,
            RequiredEmotions = new string[] { "happiness", "longing" }
        };
        
        Archetypes["RagsToRiches"] = new StoryArchetype
        {
            Id = "RagsToRiches",
            Name = "Rags to Riches",
            Description = "From humble beginnings to great heights.",
            TypicalThemes = new string[] { "perseverance", "opportunity", "success" },
            TensionRate = 0.9f,
            RequiredEmotions = new string[] { "hope" }
        };
        
        Archetypes["ComingOfAge"] = new StoryArchetype
        {
            Id = "ComingOfAge",
            Name = "Coming of Age",
            Description = "A journey from innocence to experience.",
            TypicalThemes = new string[] { "growth", "learning", "responsibility" },
            TensionRate = 0.7f,
            RequiredEmotions = new string[] { "curiosity" }
        };
        
        Archetypes["Redemption"] = new StoryArchetype
        {
            Id = "Redemption",
            Name = "Redemption",
            Description = "A chance to make things right.",
            TypicalThemes = new string[] { "forgiveness", "change", "atonement" },
            TensionRate = 1.2f,
            RequiredEmotions = new string[] { "regret", "hope" }
        };
        
        Archetypes["Adventure"] = new StoryArchetype
        {
            Id = "Adventure",
            Name = "Adventure",
            Description = "Excitement, discovery, and the thrill of exploration.",
            TypicalThemes = new string[] { "exploration", "discovery", "excitement" },
            TensionRate = 1.1f,
            RequiredEmotions = new string[] { "excitement" }
        };
        
        Archetypes["Mystery"] = new StoryArchetype
        {
            Id = "Mystery",
            Name = "Mystery",
            Description = "Puzzles to solve and secrets to uncover.",
            TypicalThemes = new string[] { "curiosity", "investigation", "revelation" },
            TensionRate = 1.3f,
            RequiredEmotions = new string[] { "suspicion", "curiosity" }
        };
        
        Archetypes["Comedy"] = new StoryArchetype
        {
            Id = "Comedy",
            Name = "Comedy",
            Description = "Laughs, mishaps, and happy endings.",
            TypicalThemes = new string[] { "humor", "chaos", "resolution" },
            TensionRate = 0.6f,
            RequiredEmotions = new string[] { "amusement" }
        };
        
        Archetypes["Legend"] = new StoryArchetype
        {
            Id = "Legend",
            Name = "Legend",
            Description = "Tales of epic proportions that become myth.",
            TypicalThemes = new string[] { "epic", "destiny", "immortality" },
            TensionRate = 1.4f,
            RequiredEmotions = new string[] { "awe" }
        };
    }
    
    private void InitializeChapterTemplates()
    {
        // Introduction chapters
        ChapterTemplates["intro_call"] = new ChapterTemplate
        {
            Id = "intro_call",
            Name = "The Call",
            Type = "Introduction",
            Description = "You receive a mysterious call to adventure.",
            AvailableChoices = new string[] { "accept", "decline", "investigate" },
            DefaultChoice = "accept",
            MinTension = 0,
            MaxTension = 20,
            BaseDuration = 10,
            Rewards = new Dictionary<string, int>()
        };
        
        ChapterTemplates["intro_discovery"] = new ChapterTemplate
        {
            Id = "intro_discovery",
            Name = "Discovery",
            Type = "Introduction",
            Description = "You discover something that changes everything.",
            AvailableChoices = new string[] { "pursue", "ignore", "share" },
            DefaultChoice = "pursue",
            MinTension = 10,
            MaxTension = 30,
            BaseDuration = 15,
            Rewards = new Dictionary<string, int>()
        };
        
        // Rising Action chapters
        ChapterTemplates["rising_first_trial"] = new ChapterTemplate
        {
            Id = "rising_first_trial",
            Name = "First Trial",
            Type = "RisingAction",
            Description = "Your first major challenge awaits.",
            AvailableChoices = new string[] { "face_brave", "face_careful", "seek_help" },
            DefaultChoice = "face_brave",
            MinTension = 20,
            MaxTension = 50,
            BaseDuration = 20,
            Rewards = new Dictionary<string, int> { { "exp", 500 } }
        };
        
        ChapterTemplates["rising_alliance"] = new ChapterTemplate
        {
            Id = "rising_alliance",
            Name = "New Alliance",
            Type = "RisingAction",
            Description = "You find an unlikely ally.",
            AvailableChoices = new string[] { "trust", "befriend_cautiously", "refuse" },
            DefaultChoice = "befriend_cautiously",
            MinTension = 30,
            MaxTension = 55,
            BaseDuration = 15,
            Rewards = new Dictionary<string, int> { { "reputation", 10 } }
        };
        
        ChapterTemplates["rising_revelation"] = new ChapterTemplate
        {
            Id = "rising_revelation",
            Name = "Shocking Revelation",
            Type = "RisingAction",
            Description = "Everything you thought you knew is called into question.",
            AvailableChoices = new string[] { "accept", "deny", "investigate_further" },
            DefaultChoice = "accept",
            MinTension = 40,
            MaxTension = 70,
            BaseDuration = 20,
            Rewards = new Dictionary<string, int>()
        };
        
        // Complication chapters
        ChapterTemplates["complication_betrayal"] = new ChapterTemplate
        {
            Id = "complication_betrayal",
            Name = "Betrayal",
            Type = "Complication",
            Description = "Someone you trusted has betrayed you.",
            AvailableChoices = new string[] { "confront", "forgive", "revenge" },
            DefaultChoice = "confront",
            MinTension = 50,
            MaxTension = 80,
            BaseDuration = 25,
            Rewards = new Dictionary<string, int>()
        };
        
        ChapterTemplates["complication_crisis"] = new ChapterTemplate
        {
            Id = "complication_crisis",
            Name = "Crisis Point",
            Type = "Complication",
            Description = "A crisis threatens everything you've built.",
            AvailableChoices = new string[] { "sacrifice", "persist", "alternative_solution" },
            DefaultChoice = "persist",
            MinTension = 60,
            MaxTension = 85,
            BaseDuration = 30,
            Rewards = new Dictionary<string, int> { { "exp", 1000 } }
        };
        
        // Climax chapters
        ChapterTemplates["climax_final_battle"] = new ChapterTemplate
        {
            Id = "climax_final_battle",
            Name = "Final Battle",
            Type = "Climax",
            Description = "The ultimate confrontation awaits.",
            AvailableChoices = new string[] { "fight_honorably", "fight_dirty", "negotiate" },
            DefaultChoice = "fight_honorably",
            MinTension = 70,
            MaxTension = 100,
            BaseDuration = 35,
            Rewards = new Dictionary<string, int> { { "gold", 2000 }, { "exp", 3000 } }
        };
        
        ChapterTemplates["climax_choice"] = new ChapterTemplate
        {
            Id = "climax_choice",
            Name = "The Final Choice",
            Type = "Climax",
            Description = "One choice will determine the fate of all.",
            AvailableChoices = new string[] { "selfless", "pragmatic", "sacrifice_self" },
            DefaultChoice = "selfless",
            MinTension = 80,
            MaxTension = 100,
            BaseDuration = 30,
            Rewards = new Dictionary<string, int>()
        };
        
        // Resolution chapters
        ChapterTemplates["resolution_victory"] = new ChapterTemplate
        {
            Id = "resolution_victory",
            Name = "Victory",
            Type = "Resolution",
            Description = "Your actions have led to victory.",
            AvailableChoices = new string[] { "celebrate", "humble", "prepare_next" },
            DefaultChoice = "celebrate",
            MinTension = 20,
            MaxTension = 40,
            BaseDuration = 15,
            Rewards = new Dictionary<string, int> { { "gold", 1000 }, { "reputation", 50 } }
        };
        
        ChapterTemplates["resolution_sacrifice"] = new ChapterTemplate
        {
            Id = "resolution_sacrifice",
            Name = "Sacrifice",
            Type = "Resolution",
            Description = "Victory came at a great cost.",
            AvailableChoices = new string[] { "honor_memory", "move_on", "find_redemption" },
            DefaultChoice = "honor_memory",
            MinTension = 30,
            MaxTension = 50,
            BaseDuration = 20,
            Rewards = new Dictionary<string, int> { { "exp", 2000 }, { "reputation", 100 } }
        };
        
        // Epilogue chapters
        ChapterTemplates["epilogue_new_beginning"] = new ChapterTemplate
        {
            Id = "epilogue_new_beginning",
            Name = "New Beginning",
            Type = "Epilogue",
            Description = "A new chapter in your life begins.",
            AvailableChoices = new string[] { "continue_adventure", "settle_down", "mentor_others" },
            DefaultChoice = "continue_adventure",
            MinTension = 0,
            MaxTension = 20,
            BaseDuration = 10,
            Rewards = new Dictionary<string, int> { { "gold", 500 } }
        };
        
        ChapterTemplates["epilogue_legend"] = new ChapterTemplate
        {
            Id = "epilogue_legend",
            Name = "Legend Born",
            Type = "Epilogue",
            Description = "Your tale will be told for generations.",
            AvailableChoices = new string[] { "write_chronicle", "disappear", "embark_new_quest" },
            DefaultChoice = "write_chronicle",
            MinTension = 0,
            MaxTension = 15,
            BaseDuration = 10,
            Rewards = new Dictionary<string, int> { { "gold", 3000 }, { "reputation", 200 } }
        };
    }
    
    private void InitializeNPCTemplates()
    {
        NPCTemplates["mentor_wise"] = new NPCTemplate
        {
            Id = "mentor_wise",
            Name = "Wise Mentor",
            Role = "Mentor",
            PersonalityTraits = new string[] { "wise", "patient", "mysterious" },
            DialogueStyles = new string[] { "cryptic", "philosophical" },
            DialogueLines = new Dictionary<string, string>
            {
                { "greeting", "Ah, young one. Your path has led you here." },
                { "advice", "Remember: true strength comes from within." },
                { "farewell", "May the light guide your way." }
            }
        };
        
        NPCTemplates["rival_determined"] = new NPCTemplate
        {
            Id = "rival_determined",
            Name = "Determined Rival",
            Role = "Rival",
            PersonalityTraits = new string[] { "ambitious", "competitive", "respectful" },
            DialogueStyles = new string[] { "challenging", "boastful" },
            DialogueLines = new Dictionary<string, string>
            {
                { "greeting", "So we meet again. Have you improved?" },
                { "advice", "Don't expect me to go easy on you." },
                { "farewell", "Next time, I won't lose." }
            }
        };
        
        NPCTemplates["ally_loyal"] = new NPCTemplate
        {
            Id = "ally_loyal",
            Name = "Loyal Ally",
            Role = "Ally",
            PersonalityTraits = new string[] { "loyal", "brave", "straightforward" },
            DialogueStyles = new string[] { "supportive", "encouraging" },
            DialogueLines = new Dictionary<string, string>
            {
                { "greeting", "Good to see you! Ready for adventure?" },
                { "advice", "I've got your back. Always." },
                { "farewell", "Stay safe out there." }
            }
        };
        
        NPCTemplates["love_mysterious"] = new NPCTemplate
        {
            Id = "love_mysterious",
            Name = "Mysterious Love Interest",
            Role = "LoveInterest",
            PersonalityTraits = new string[] { "enigmatic", "charming", "secretive" },
            DialogueStyles = new string[] { "flirtatious", "mysterious" },
            DialogueLines = new Dictionary<string, string>
            {
                { "greeting", "Fancy meeting you here..." },
                { "advice", "Trust your heart, not just your eyes." },
                { "farewell", "Will I see you again?" }
            }
        };
        
        NPCTemplates["villain_cunning"] = new NPCTemplate
        {
            Id = "villain_cunning",
            Name = "Cunning Villain",
            Role = "Villain",
            PersonalityTraits = new string[] { "cunning", "manipulative", "powerful" },
            DialogueStyles = new string[] { "threatening", "mocking" },
            DialogueLines = new Dictionary<string, string>
            {
                { "greeting", "You've made a mistake coming here." },
                { "advice", "Weakness will be your downfall." },
                { "farewell", "This isn't over." }
            }
        };
    }
    
    // Get random story template based on player state
    public StoryTemplate GetRandomStoryTemplate(int playerLevel)
    {
        var validTemplates = StoryTemplates.Values
            .Where(t => playerLevel >= t.MinPlayerLevel && playerLevel <= t.MaxPlayerLevel)
            .ToList();
        
        if (validTemplates.Count == 0)
            return null;
        
        return validTemplates[GD.Randi() % validTemplates.Count];
    }
    
    // Get chapter template by type
    public ChapterTemplate GetChapterTemplateByType(string type)
    {
        var candidates = ChapterTemplates.Values
            .Where(c => c.Type == type)
            .ToList();
        
        if (candidates.Count == 0)
            return null;
        
        return candidates[GD.Randi() % candidates.Count];
    }
    
    // Get color for story archetype
    public Color GetArchetypeColor(string archetype)
    {
        if (StoryColors.ContainsKey(archetype))
            return StoryColors[archetype];
        return Colors.White;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        // ProceduralStoryDatabase 是静态配置数据，不需要持久化
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        // ProceduralStoryDatabase 是静态配置数据，不需要持久化
    }
}
