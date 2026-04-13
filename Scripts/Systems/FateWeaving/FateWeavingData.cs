using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.FateWeaving {
    public enum FatePathType {
        Hero,
        AntiHero,
        Villain,
        Mercenary,
        Legend,
        Myth,
        Chaos,
        Order,
        Shadow,
        Light
    }

    public enum FateChoiceType {
        Moral,
        Combat,
        Social,
        Economic,
        Exploration,
        Mystery
    }

    public class FateChoice {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public FateChoiceType ChoiceType { get; set; }
        public Dictionary<FatePathType, float> PathInfluence { get; set; }
        public Dictionary<string, float> StatBonuses { get; set; }
        public string ConsequenceDescription { get; set; }
        public bool IsSecret { get; set; }
        public int TierRequired { get; set; }
    }

    public class FatePathData {
        public FatePathType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, float> PathBonuses { get; set; }
        public List<string> ExclusiveChoices { get; set; }
        public int UnlockTier { get; set; }
    }

    public partial class FateWeavingData : GodotObject {
        public Dictionary<FatePathType, float> PathAffinity { get; set; } = new Dictionary<FatePathType, FatePathType> {
            { FatePathType.Hero, 0f },
            { FatePathType.AntiHero, 0f },
            { FatePathType.Villain, 0f },
            { FatePathType.Mercenary, 0f },
            { FatePathType.Legend, 0f },
            { FatePathType.Myth, 0f },
            { FatePathType.Chaos, 0f },
            { FatePathType.Order, 0f },
            { FatePathType.Shadow, 0f },
            { FatePathType.Light, 0f }
        };
        
        public Dictionary<string, float> PlayerStats { get; set; } = new Dictionary<string, float> {
            { "strength", 0f },
            { "dexterity", 0f },
            { "intelligence", 0f },
            { "wisdom", 0f },
            { "charisma", 0f },
            { "luck", 0f }
        };
        
        public List<string> MadeChoices { get; set; } = new List<string>();
        public FatePathType DominantPath { get; set; } = FatePathType.Hero;
        public int WeaveLevel { get; set; } = 1;
        public int TotalWeaves { get; set; } = 0;
        public Dictionary<string, int> ChoiceTypeCount { get; set; } = new Dictionary<string, int>();
    }

    public class FateWeavingStatistics {
        public int TotalChoicesMade { get; set; }
        public int MoralChoices { get; set; }
        public int CombatChoices { get; set; }
        public int SocialChoices { get; set; }
        public int EconomicChoices { get; set; }
        public int ExplorationChoices { get; set; }
        public int MysteryChoices { get; set; }
        public Dictionary<FatePathType, int> PathChoiceCount { get; set; } = new Dictionary<FatePathType, int>();
        public float HighestPathAffinity { get; set; }
        public int PerfectWeaves { get; set; }
    }
}
