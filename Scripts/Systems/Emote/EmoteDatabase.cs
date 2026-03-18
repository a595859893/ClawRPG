using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Systems.Emote
{
    public class EmoteDatabase : IDatabase
    {
        private static EmoteDatabase _instance;
        public static EmoteDatabase Instance => _instance ??= new EmoteDatabase();

        private Dictionary<string, Emote> _emotes;

        public object Instance => Instance;

        public void Initialize()
        {
            _emotes = new Dictionary<string, Emote>();

            // Common emotes (default)
            AddEmote(new Emote { Id = "wave", Name = "Wave", Description = "Wave to others", Category = EmoteCategory.Greeting, Rarity = EmoteRarity.Common, Cost = 0, IsDefault = true, AnimationName = "wave" });
            AddEmote(new Emote { Id = "laugh", Name = "Laugh", Description = "Laugh out loud", Category = EmoteCategory.Happy, Rarity = EmoteRarity.Common, Cost = 0, IsDefault = true, AnimationName = "laugh" });
            AddEmote(new Emote { Id = "cry", Name = "Cry", Description = "Cry sad tears", Category = EmoteCategory.Sad, Rarity = EmoteRarity.Common, Cost = 0, IsDefault = true, AnimationName = "cry" });
            AddEmote(new Emote { Id = "clap", Name = "Clap", Description = "Clap your hands", Category = EmoteCategory.Happy, Rarity = EmoteRarity.Common, Cost = 0, IsDefault = true, AnimationName = "clap" });
            AddEmote(new Emote { Id = "shrug", Name = "Shrug", Description = "I don't know", Category = EmoteCategory.Thinking, Rarity = EmoteRarity.Common, Cost = 0, IsDefault = true, AnimationName = "shrug" });
            AddEmote(new Emote { Id = "point", Name = "Point", Description = "Point at something", Category = EmoteCategory.Misc, Rarity = EmoteRarity.Common, Cost = 0, IsDefault = true, AnimationName = "point" });
            AddEmote(new Emote { Id = "bow", Name = "Bow", Description = "Bow respectfully", Category = EmoteCategory.Greeting, Rarity = EmoteRarity.Common, Cost = 0, IsDefault = true, AnimationName = "bow" });

            // Uncommon emotes
            AddEmote(new Emote { Id = "dance", Name = "Dance", Description = "Dance with joy", Category = EmoteCategory.Excited, Rarity = EmoteRarity.Uncommon, Cost = 100, AnimationName = "dance" });
            AddEmote(new Emote { Id = "hug", Name = "Hug", Description = "Give a warm hug", Category = EmoteCategory.Love, Rarity = EmoteRarity.Uncommon, Cost = 150, AnimationName = "hug" });
            AddEmote(new Emote { Id = "thumbsup", Name = "Thumbs Up", Description = "Show approval", Category = EmoteCategory.Happy, Rarity = EmoteRarity.Uncommon, Cost = 100, AnimationName = "thumbsup" });
            AddEmote(new Emote { Id = "cheer", Name = "Cheer", Description = "Cheer loudly", Category = EmoteCategory.Excited, Rarity = EmoteRarity.Uncommon, Cost = 120, AnimationName = "cheer" });
            AddEmote(new Emote { Id = "facepalm", Name = "Facepalm", Description = "Facepalm in disbelief", Category = EmoteCategory.Angry, Rarity = EmoteRarity.Uncommon, Cost = 100, AnimationName = "facepalm" });
            AddEmote(new Emote { Id = "sorry", Name = "Sorry", Description = "Apologize sincerely", Category = EmoteCategory.Sad, Rarity = EmoteRarity.Uncommon, Cost = 100, AnimationName = "sorry" });

            // Rare emotes
            AddEmote(new Emote { Id = "flex", Name = "Flex", Description = "Show off your muscles", Category = EmoteCategory.Excited, Rarity = EmoteRarity.Rare, Cost = 300, AnimationName = "flex" });
            AddEmote(new Emote { Id = "kneel", Name = "Kneel", Description = "Kneel in respect", Category = EmoteCategory.Greeting, Rarity = EmoteRarity.Rare, Cost = 250, AnimationName = "kneel" });
            AddEmote(new Emote { Id = "angry", Name = "Angry", Description = "Show your anger", Category = EmoteCategory.Angry, Rarity = EmoteRarity.Rare, Cost = 280, AnimationName = "angry" });
            AddEmote(new Emote { Id = "love_heart", Name = "Heart Eyes", Description = "Show love with heart eyes", Category = EmoteCategory.Love, Rarity = EmoteRarity.Rare, Cost = 320, AnimationName = "love_heart" });
            AddEmote(new Emote { Id = "meditate", Name = "Meditate", Description = "Calm yourself", Category = EmoteCategory.Thinking, Rarity = EmoteRarity.Rare, Cost = 300, AnimationName = "meditate" });
            AddEmote(new Emote { Id = "celebrate", Name = "Celebrate", Description = "Throw a celebration", Category = EmoteCategory.Victory, Rarity = EmoteRarity.Rare, Cost = 350, AnimationName = "celebrate" });

            // Epic emotes
            AddEmote(new Emote { Id = "tpose", Name = "T-Pose", Description = "Strike a heroic pose", Category = EmoteCategory.Victory, Rarity = EmoteRarity.Epic, Cost = 500, AnimationName = "tpose" });
            AddEmote(new Emote { Id = "charge", Name = "Charge", Description = "Charge into battle", Category = EmoteCategory.Angry, Rarity = EmoteRarity.Epic, Cost = 550, AnimationName = "charge" });
            AddEmote(new Emote { Id = "magic", Name = "Magic Cast", Description = "Cast a magic spell", Category = EmoteCategory.Thinking, Rarity = EmoteRarity.Epic, Cost = 600, AnimationName = "magic" });
            AddEmote(new Emote { Id = "roar", Name = "Roar", Description = "Let out a mighty roar", Category = EmoteCategory.Angry, Rarity = EmoteRarity.Epic, Cost = 580, AnimationName = "roar" });
            AddEmote(new Emote { Id = "superhappy", Name = "Super Happy", Description = "Explode with happiness", Category = EmoteCategory.Happy, Rarity = EmoteRarity.Epic, Cost = 520, AnimationName = "superhappy" });

            // Legendary emotes
            AddEmote(new Emote { Id = "legendary_pose", Name = "Legendary Pose", Description = "Strike the legendary pose", Category = EmoteCategory.Victory, Rarity = EmoteRarity.Legendary, Cost = 1000, AnimationName = "legendary_pose" });
            AddEmote(new Emote { Id = "dragon_roar", Name = "Dragon Roar", Description = "Roar like a legendary dragon", Category = EmoteCategory.Angry, Rarity = EmoteRarity.Legendary, Cost = 1200, AnimationName = "dragon_roar" });
            AddEmote(new Emote { Id = "phoenix_flight", Name = "Phoenix Flight", Description = "Rise like a phoenix", Category = EmoteCategory.Victory, Rarity = EmoteRarity.Legendary, Cost = 1500, AnimationName = "phoenix_flight" });
            AddEmote(new Emote { Id = "royal_wave", Name = "Royal Wave", Description = "Wave like royalty", Category = EmoteCategory.Greeting, Rarity = EmoteRarity.Legendary, Cost = 1100, AnimationName = "royal_wave" });
        }

        public bool ValidateData()
        {
            return _emotes != null && _emotes.Count > 0;
        }

        private void AddEmote(Emote emote)
        {
            _emotes[emote.Id] = emote;
        }

        public Emote GetEmote(string id)
        {
            return _emotes.ContainsKey(id) ? _emotes[id] : null;
        }

        public List<Emote> GetAllEmotes()
        {
            return _emotes.Values.ToList();
        }

        public List<Emote> GetEmotesByCategory(EmoteCategory category)
        {
            return _emotes.Values.Where(e => e.Category == category).ToList();
        }

        public List<Emote> GetEmotesByRarity(EmoteRarity rarity)
        {
            return _emotes.Values.Where(e => e.Rarity == rarity).ToList();
        }

        public List<Emote> GetDefaultEmotes()
        {
            return _emotes.Values.Where(e => e.IsDefault).ToList();
        }

        public List<Emote> GetShopEmotes()
        {
            return _emotes.Values.Where(e => !e.IsDefault).ToList();
        }
    }
}
