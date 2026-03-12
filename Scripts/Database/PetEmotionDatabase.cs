using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// Pet emotion database configuration
    /// </summary>
    public static class PetEmotionDatabase
    {
        public enum EmotionCategory
        {
            Positive,
            Negative,
            Neutral
        }

        public class EmotionConfig
        {
            public Data.PetEmotionData.EmotionType Type { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public EmotionCategory Category { get; set; }
            public Color DisplayColor { get; set; }
            public string Emoji { get; set; }
            public Dictionary<string, float> StatModifiers { get; set; } = new Dictionary<string, float>();
            public float DecayRate { get; set; } // Per minute
            public List<string> Triggers { get; set; } = new List<string>();
        }

        public static Dictionary<Data.PetEmotionData.EmotionType, EmotionConfig> Emotions { get; private set; } = new Dictionary<Data.PetEmotionData.EmotionType, EmotionConfig>
        {
            { Data.PetEmotionData.EmotionType.Happy, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Happy,
                    Name = "Happy",
                    Description = "Your pet is feeling joyful and content",
                    Category = EmotionCategory.Positive,
                    DisplayColor = new Color(1f, 0.84f, 0f), // Gold
                    Emoji = "😊",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Attack", 1.1f },
                        { "Experience", 1.15f },
                        { "DropRate", 1.1f }
                    },
                    DecayRate = 0.02f,
                    Triggers = new List<string> { "pet", "feed", "play", "battle_win", "level_up" }
                }
            },
            { Data.PetEmotionData.EmotionType.Sad, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Sad,
                    Name = "Sad",
                    Description = "Your pet is feeling down",
                    Category = EmotionCategory.Negative,
                    DisplayColor = new Color(0.5f, 0.5f, 0.8f), // Blue
                    Emoji = "😢",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Attack", 0.9f },
                        { "Defense", 0.9f }
                    },
                    DecayRate = 0.015f,
                    Triggers = new List<string> { "battle_lose", "neglect", "hunger" }
                }
            },
            { Data.PetEmotionData.EmotionType.Angry, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Angry,
                    Name = "Angry",
                    Description = "Your pet is furious",
                    Category = EmotionCategory.Negative,
                    DisplayColor = new Color(1f, 0.3f, 0.3f), // Red
                    Emoji = "😠",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Attack", 1.25f },
                        { "Defense", 0.8f },
                        { "Critical", 1.2f }
                    },
                    DecayRate = 0.03f,
                    Triggers = new List<string> { "hurt", "battle_lose", "interrupted" }
                }
            },
            { Data.PetEmotionData.EmotionType.Excited, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Excited,
                    Name = "Excited",
                    Description = "Your pet is thrilled",
                    Category = EmotionCategory.Positive,
                    DisplayColor = new Color(1f, 0.6f, 0f), // Orange
                    Emoji = "🤩",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Speed", 1.3f },
                        { "Experience", 1.2f },
                        { "Evasion", 1.15f }
                    },
                    DecayRate = 0.04f,
                    Triggers = new List<string> { "new_toy", "found_treasure", "event" }
                }
            },
            { Data.PetEmotionData.EmotionType.Tired, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Tired,
                    Name = "Tired",
                    Description = "Your pet needs rest",
                    Category = EmotionCategory.Negative,
                    DisplayColor = new Color(0.6f, 0.6f, 0.6f), // Gray
                    Emoji = "😴",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Attack", 0.7f },
                        { "Speed", 0.7f },
                        { "Defense", 0.85f }
                    },
                    DecayRate = 0.01f,
                    Triggers = new List<string> { "long_battle", "no_sleep", "exhaustion" }
                }
            },
            { Data.PetEmotionData.EmotionType.Hungry, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Hungry,
                    Name = "Hungry",
                    Description = "Your pet is hungry",
                    Category = EmotionCategory.Negative,
                    DisplayColor = new Color(0.8f, 0.5f, 0.2f), // Brown
                    Emoji = "🍽️",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Attack", 0.85f },
                        { "Health", 0.9f }
                    },
                    DecayRate = 0.005f,
                    Triggers = new List<string> { "no_food", "long_time" }
                }
            },
            { Data.PetEmotionData.EmotionType.Playful, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Playful,
                    Name = "Playful",
                    Description = "Your pet wants to play",
                    Category = EmotionCategory.Positive,
                    DisplayColor = new Color(0.4f, 1f, 0.4f), // Green
                    Emoji = "🎾",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Experience", 1.25f },
                        { "Luck", 1.15f }
                    },
                    DecayRate = 0.025f,
                    Triggers = new List<string> { "play", "toy", "new_environment" }
                }
            },
            { Data.PetEmotionData.EmotionType.Affectionate, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Affectionate,
                    Name = "Affectionate",
                    Description = "Your pet loves you very much",
                    Category = EmotionCategory.Positive,
                    DisplayColor = new Color(1f, 0.4f, 0.6f), // Pink
                    Emoji = "💕",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Defense", 1.15f },
                        { "Health", 1.1f },
                        { "Experience", 1.1f }
                    },
                    DecayRate = 0.018f,
                    Triggers = new List<string> { "pet", "bonding", "gift" }
                }
            },
            { Data.PetEmotionData.EmotionType.Scared, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Scared,
                    Name = "Scared",
                    Description = "Your pet is frightened",
                    Category = EmotionCategory.Negative,
                    DisplayColor = new Color(0.4f, 0.4f, 0.6f), // Purple
                    Emoji = "😨",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Defense", 0.7f },
                        { "Evasion", 1.3f },
                        { "Attack", 0.8f }
                    },
                    DecayRate = 0.035f,
                    Triggers = new List<string> { "loud_noise", "danger", "strange_place" }
                }
            },
            { Data.PetEmotionData.EmotionType.Neutral, new EmotionConfig
                {
                    Type = Data.PetEmotionData.EmotionType.Neutral,
                    Name = "Neutral",
                    Description = "Your pet is calm",
                    Category = EmotionCategory.Neutral,
                    DisplayColor = new Color(0.8f, 0.8f, 0.8f), // Light Gray
                    Emoji = "😐",
                    StatModifiers = new Dictionary<string, float>
                    {
                        { "Attack", 1.0f },
                        { "Defense", 1.0f },
                        { "Speed", 1.0f }
                    },
                    DecayRate = 0.0f,
                    Triggers = new List<string> { "idle", "normal" }
                }
            }
        };

        /// <summary>
        /// Get emotion configuration by type
        /// </summary>
        public static EmotionConfig GetEmotion(Data.PetEmotionData.EmotionType type)
        {
            if (Emotions.ContainsKey(type))
                return Emotions[type];
            return Emotions[Data.PetEmotionData.EmotionType.Neutral];
        }

        /// <summary>
        /// Get all positive emotions
        /// </summary>
        public static List<EmotionConfig> GetPositiveEmotions()
        {
            var result = new List<EmotionConfig>();
            foreach (var emotion in Emotions.Values)
            {
                if (emotion.Category == EmotionCategory.Positive)
                    result.Add(emotion);
            }
            return result;
        }

        /// <summary>
        /// Get all negative emotions
        /// </summary>
        public static List<EmotionConfig> GetNegativeEmotions()
        {
            var result = new List<EmotionConfig>();
            foreach (var emotion in Emotions.Values)
            {
                if (emotion.Category == EmotionCategory.Negative)
                    result.Add(emotion);
            }
            return result;
        }
    }
}
