namespace ClawRPG.Systems.Emote {
    public enum EmoteCategory {
        Happy,
        Sad,
        Angry,
        Excited,
        Thinking,
        Greeting,
        Victory,
        Defeat,
        Love,
        Misc
    }

    public enum EmoteRarity {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public class Emote {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EmoteCategory Category { get; set; }
        public EmoteRarity Rarity { get; set; }
        public string AnimationName { get; set; }
        public string IconPath { get; set; }
        public int Cost { get; set; }
        public bool IsDefault { get; set; }
    }

    public class PlayerEmoteData {
        public List<string> UnlockedEmotes { get; set; } = new List<string>();
        public List<string> FavoriteEmotes { get; set; } = new List<string>();
        public Dictionary<string, int> EmoteUsageCount { get; set; } = new Dictionary<string, int>();
        public string LastUsedEmote { get; set; }
    }
}
