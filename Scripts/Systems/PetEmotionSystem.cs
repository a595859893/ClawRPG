using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Pet Emotion System - manages pet emotional states and behaviors
    /// </summary>
    public class PetEmotionSystem : Node
    {
        public static PetEmotionSystem Instance { get; private set; }

        private Dictionary<string, Data.PetEmotionData> _petEmotions = new Dictionary<string, Data.PetEmotionData>();
        private float _tickTimer = 0f;
        private const float TICK_INTERVAL = 5f; // Process emotions every 5 seconds

        // Statistics
        public int TotalEmotionChanges { get; private set; }
        public int DominantEmotionCounts { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            LoadData();
        }

        public override void _Process(float delta)
        {
            _tickTimer += delta;
            if (_tickTimer >= TICK_INTERVAL)
            {
                _tickTimer = 0f;
                ProcessEmotionDecay(delta);
            }
        }

        /// <summary>
        /// Get or create emotion data for a pet
        /// </summary>
        public Data.PetEmotionData GetPetEmotion(string petId)
        {
            if (!_petEmotions.ContainsKey(petId))
            {
                _petEmotions[petId] = new Data.PetEmotionData
                {
                    PetId = petId,
                    CurrentEmotions = new Dictionary<Data.PetEmotionData.EmotionType, float>
                    {
                        { Data.PetEmotionData.EmotionType.Neutral, 1.0f }
                    },
                    DominantEmotion = Data.PetEmotionData.EmotionType.Neutral,
                    CurrentIntensity = Data.PetEmotionData.EmotionIntensity.Low
                };
            }
            return _petEmotions[petId];
        }

        /// <summary>
        /// Trigger an emotion change for a pet
        /// </summary>
        public void TriggerEmotion(string petId, Data.PetEmotionData.EmotionType emotionType, float intensity = 0.5f, string trigger = "unknown")
        {
            var emotionData = GetPetEmotion(petId);
            
            // Apply emotion
            if (!emotionData.CurrentEmotions.ContainsKey(emotionType))
            {
                emotionData.CurrentEmotions[emotionType] = 0f;
            }
            
            emotionData.CurrentEmotions[emotionType] = Mathf.Clamp(intensity, 0f, 1f);
            
            // Calculate intensity based on emotion value
            if (intensity >= 0.75f)
                emotionData.CurrentIntensity = Data.PetEmotionData.EmotionIntensity.Extreme;
            else if (intensity >= 0.5f)
                emotionData.CurrentIntensity = Data.PetEmotionData.EmotionIntensity.High;
            else if (intensity >= 0.25f)
                emotionData.CurrentIntensity = Data.PetEmotionData.EmotionIntensity.Medium;
            else
                emotionData.CurrentIntensity = Data.PetEmotionData.EmotionIntensity.Low;

            // Update dominant emotion
            UpdateDominantEmotion(emotionData);

            // Record history
            emotionData.EmotionHistory.Add(new Data.EmotionHistoryEntry
            {
                Emotion = emotionType,
                Intensity = emotionData.CurrentIntensity,
                Timestamp = DateTime.Now,
                Trigger = trigger
            });

            // Keep only last 50 history entries
            if (emotionData.EmotionHistory.Count > 50)
            {
                emotionData.EmotionHistory.RemoveAt(0);
            }

            emotionData.TotalEmotionChanges++;
            emotionData.LastEmotionChange = DateTime.Now;
            TotalEmotionChanges++;

            SaveData();
            GD.Print($"[PetEmotion] Pet {petId} is now {emotionType} ({emotionData.CurrentIntensity}) - Trigger: {trigger}");
        }

        /// <summary>
        /// Update dominant emotion based on current emotions
        /// </summary>
        private void UpdateDominantEmotion(Data.PetEmotionData emotionData)
        {
            float highestValue = 0f;
            Data.PetEmotionData.EmotionType dominant = Data.PetEmotionData.EmotionType.Neutral;

            foreach (var emotion in emotionData.CurrentEmotions)
            {
                if (emotion.Value > highestValue)
                {
                    highestValue = emotion.Value;
                    dominant = emotion.Key;
                }
            }

            emotionData.DominantEmotion = dominant;
        }

        /// <summary>
        /// Process natural emotion decay over time
        /// </summary>
        private void ProcessEmotionDecay(float delta)
        {
            foreach (var petEmotion in _petEmotions.Values)
            {
                var decayRates = new Dictionary<Data.PetEmotionData.EmotionType, float>();
                
                foreach (var emotion in petEmotion.CurrentEmotions)
                {
                    var config = Database.PetEmotionDatabase.GetEmotion(emotion.Key);
                    float decay = config.DecayRate * (delta / 60f); // Convert to per-tick
                    petEmotion.CurrentEmotions[emotion.Key] = Mathf.Max(0f, emotion.Value - decay);
                }

                // Normalize emotions
                NormalizeEmotions(petEmotion);
                
                // Update dominant emotion
                UpdateDominantEmotion(petEmotion);
            }

            SaveData();
        }

        /// <summary>
        /// Normalize emotion values to sum to 1.0
        /// </summary>
        private void NormalizeEmotions(Data.PetEmotionData emotionData)
        {
            float total = 0f;
            foreach (var emotion in emotionData.CurrentEmotions)
            {
                total += emotion.Value;
            }

            if (total > 0f && total != 1f)
            {
                var normalized = new Dictionary<Data.PetEmotionData.EmotionType, float>();
                foreach (var emotion in emotionData.CurrentEmotions)
                {
                    normalized[emotion.Key] = emotion.Value / total;
                }
                emotionData.CurrentEmotions = normalized;
            }
        }

        /// <summary>
        /// Get stat modifiers based on current emotions
        /// </summary>
        public Dictionary<string, float> GetStatModifiers(string petId)
        {
            var result = new Dictionary<string, float>
            {
                { "Attack", 1.0f },
                { "Defense", 1.0f },
                { "Speed", 1.0f },
                { "Health", 1.0f },
                { "Critical", 1.0f },
                { "Evasion", 1.0f },
                { "Experience", 1.0f },
                { "DropRate", 1.0f },
                { "Luck", 1.0f }
            };

            var emotionData = GetPetEmotion(petId);
            var dominantConfig = Database.PetEmotionDatabase.GetEmotion(emotionData.DominantEmotion);

            foreach (var modifier in dominantConfig.StatModifiers)
            {
                if (result.ContainsKey(modifier.Key))
                {
                    result[modifier.Key] = modifier.Value;
                }
            }

            // Apply intensity multiplier
            float intensityMultiplier = 1f;
            switch (emotionData.CurrentIntensity)
            {
                case Data.PetEmotionData.EmotionIntensity.Extreme:
                    intensityMultiplier = 1.2f;
                    break;
                case Data.PetEmotionData.EmotionIntensity.High:
                    intensityMultiplier = 1.1f;
                    break;
                case Data.PetEmotionData.EmotionIntensity.Medium:
                    intensityMultiplier = 1.0f;
                    break;
                case Data.PetEmotionData.EmotionIntensity.Low:
                    intensityMultiplier = 0.9f;
                    break;
            }

            foreach (var key in result.Keys)
            {
                result[key] = Mathf.Pow(result[key], intensityMultiplier);
            }

            return result;
        }

        /// <summary>
        /// Get emoji for current dominant emotion
        /// </summary>
        public string GetEmotionEmoji(string petId)
        {
            var emotionData = GetPetEmotion(petId);
            var config = Database.PetEmotionDatabase.GetEmotion(emotionData.DominantEmotion);
            return config.Emoji;
        }

        /// <summary>
        /// Get color for current dominant emotion
        /// </summary>
        public Color GetEmotionColor(string petId)
        {
            var emotionData = GetPetEmotion(petId);
            var config = Database.PetEmotionDatabase.GetEmotion(emotionData.DominantEmotion);
            return config.DisplayColor;
        }

        /// <summary>
        /// Trigger emotion based on battle result
        /// </summary>
        public void OnBattleResult(string petId, bool victory)
        {
            if (victory)
            {
                TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Happy, 0.7f, "battle_win");
                TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Excited, 0.4f, "battle_win");
            }
            else
            {
                TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Sad, 0.6f, "battle_lose");
                TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Angry, 0.3f, "battle_lose");
            }
        }

        /// <summary>
        /// Trigger emotion based on player interaction
        /// </summary>
        public void OnPlayerInteraction(string petId, string interactionType)
        {
            switch (interactionType.ToLower())
            {
                case "pet":
                    TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Affectionate, 0.8f, "player_pet");
                    TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Happy, 0.6f, "player_pet");
                    break;
                case "feed":
                    TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Happy, 0.7f, "player_feed");
                    break;
                case "play":
                    TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Playful, 0.8f, "player_play");
                    TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Excited, 0.5f, "player_play");
                    break;
                case "scold":
                    TriggerEmotion(petId, Data.PetEmotionData.EmotionType.Sad, 0.6f, "player_scold");
                    break;
            }
        }

        /// <summary>
        /// Get all pets with their current emotions
        /// </summary>
        public Dictionary<string, Data.PetEmotionData> GetAllPetEmotions()
        {
            return new Dictionary<string, Data.PetEmotionData>(_petEmotions);
        }

        /// <summary>
        /// Get emotion statistics
        /// </summary>
        public Dictionary<string, int> GetEmotionStatistics()
        {
            var stats = new Dictionary<string, int>
            {
                { "TotalEmotionChanges", TotalEmotionChanges },
                { "TotalPets", _petEmotions.Count }
            };

            var emotionCounts = new Dictionary<Data.PetEmotionData.EmotionType, int>();
            foreach (var pet in _petEmotions.Values)
            {
                if (!emotionCounts.ContainsKey(pet.DominantEmotion))
                    emotionCounts[pet.DominantEmotion] = 0;
                emotionCounts[pet.DominantEmotion]++;
            }

            foreach (var emotion in emotionCounts)
            {
                stats[emotion.Key.ToString()] = emotion.Value;
            }

            return stats;
        }

        /// <summary>
        /// Save data to file
        /// </summary>
        private void SaveData()
        {
            // TODO: Implement save to file
        }

        /// <summary>
        /// Load data from file
        /// </summary>
        private void LoadData()
        {
            // TODO: Implement load from file
        }

        /// <summary>
        /// Reset all emotion data
        /// </summary>
        public void ResetAll()
        {
            _petEmotions.Clear();
            TotalEmotionChanges = 0;
            DominantEmotionCounts = 0;
            SaveData();
            GD.Print("[PetEmotion] All emotion data reset");
        }
    }
}
