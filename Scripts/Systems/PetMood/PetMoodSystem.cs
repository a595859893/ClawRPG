using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetMood {
    public class PetMoodSystem : BaseSystem {
        private PetMoodData _data = new PetMoodData();
        private PetMoodDatabase _database = PetMoodDatabase.Instance;
        private Random _random = new Random();
        
        // 配置
        private double _moodDecayInterval = 60.0; // 每60秒心情自然衰减
        private double _lastDecayTime = 0;
        private double _moodChangeChance = 0.3; // 30% 几率心情变化
        
        // 信号 (Godot 4 compatible)
        [Signal]
        public static delegate void PetMoodChangedDelegate(string petId, PetMoodType mood);
        [Signal]
        public static delegate void MoodEffectTriggeredDelegate(string petId, PetMoodEffect effect);
        [Signal]
        public static delegate void MoodDecayAppliedDelegate(string petId);
        
        // 持久化
        private const string SAVE_KEY = "pet_mood_system";
        
        public override void _Ready() {
            LoadData();
            _lastDecayTime = Time.GetUnixTimeFromSystem();
        }
        
        public override void _Process(double delta) {
            // 心情自然衰减
            var currentTime = Time.GetUnixTimeFromSystem();
            if (currentTime - _lastDecayTime >= _moodDecayInterval) {
                ApplyMoodDecay();
                _lastDecayTime = currentTime;
            }
        }

        // 初始化宠物心情
        public void InitializePetMood(string petId) {
            if (!_data.Moods.ContainsKey(petId)) {
                var mood = new PetMood {
                    PetId = petId,
                    CurrentMood = PetMoodType.Neutral,
                    Intensity = MoodIntensity.Medium,
                    MoodValue = 0.5f,
                    LastMoodChangeTime = Time.GetUnixTimeFromSystem()
                };
                _data.Moods[petId] = mood;
                SaveData();
            }
        }

        // 获取宠物心情
        public PetMood GetPetMood(string petId) {
            if (!_data.Moods.ContainsKey(petId)) {
                InitializePetMood(petId);
            }
            return _data.Moods[petId];
        }

        // 改变宠物心情
        public void ChangeMood(string petId, PetMoodType newMood) {
            if (!_data.Moods.ContainsKey(petId)) {
                InitializePetMood(petId);
            }
            
            var mood = _data.Moods[petId];
            var oldMood = mood.CurrentMood;
            
            mood.CurrentMood = newMood;
            mood.LastMoodChangeTime = Time.GetUnixTimeFromSystem();
            mood.ConsecutiveMoodDuration = 0;
            
            // 根据心情类型设置强度
            mood.Intensity = CalculateMoodIntensity(newMood);
            
            // 更新心情值
            mood.MoodValue = (float)(_random.NextDouble() * 0.3 + 0.5);
            
            // 记录到历史
            if (!mood.MoodHistory.ContainsKey(newMood)) {
                mood.MoodHistory[newMood] = 0;
            }
            mood.MoodHistory[newMood]++;
            
            // 统计心情变化次数
            var moodKey = $"{oldMood}_to_{newMood}";
            if (!_data.MoodChangesCount.ContainsKey(moodKey)) {
                _data.MoodChangesCount[moodKey] = 0;
            }
            _data.MoodChangesCount[moodKey]++;
            
            // 触发信号
            PetMoodChanged.Emit(petId, newMood);
            
            // 检查是否有效果触发
            CheckMoodEffects(petId);
            
            SaveData();
        }

        // 计算心情强度
        private MoodIntensity CalculateMoodIntensity(PetMoodType mood) {
            var value = _random.NextDouble();
            
            // 根据心情类型调整强度分布
            return mood switch {
                PetMoodType.Happy or PetMoodType.Excited or PetMoodType.Affectionate => 
                    value < 0.2 ? MoodIntensity.Extreme : 
                    value < 0.5 ? MoodIntensity.High : 
                    value < 0.8 ? MoodIntensity.Medium : MoodIntensity.Low,
                PetMoodType.Sad or PetMoodType.Tired or PetMoodType.Hungry =>
                    value < 0.15 ? MoodIntensity.Extreme :
                    value < 0.4 ? MoodIntensity.High :
                    value < 0.75 ? MoodIntensity.Medium : MoodIntensity.Low,
                _ => value switch {
                    < 0.1 => MoodIntensity.Extreme,
                    < 0.3 => MoodIntensity.High,
                    < 0.7 => MoodIntensity.Medium,
                    _ => MoodIntensity.Low
                }
            };
        }

        // 心情自然衰减
        private void ApplyMoodDecay() {
            foreach (var petId in _data.Moods.Keys) {
                var mood = _data.Moods[petId];
                
                // 30% 几率心情变化
                if (_random.NextDouble() < _moodChangeChance) {
                    var newMood = _database.GetRandomMoodTransition(mood.CurrentMood);
                    if (newMood != mood.CurrentMood) {
                        ChangeMood(petId, newMood);
                    }
                }
                
                // 调整心情值
                mood.MoodValue = Mathf.Clamp(mood.MoodValue + (float)(_random.NextDouble() * 0.1 - 0.05), 0.1f, 0.9f);
                
                // 记录持续时间
                mood.ConsecutiveMoodDuration++;
                
                MoodDecayApplied.Emit(petId);
            }
            SaveData();
        }

        // 检查心情效果
        private void CheckMoodEffects(string petId) {
            var mood = _data.Moods[petId];
            var effects = _database.GetEffectsForMood(mood);
            
            foreach (var effect in effects) {
                MoodEffectTriggered.Emit(petId, effect);
            }
        }

        // 与宠物互动 - 提升心情
        public void InteractWithPet(string petId, string interactionType) {
            _data.TotalInteractionCount++;
            
            if (!_data.Moods.ContainsKey(petId)) {
                InitializePetMood(petId);
            }
            
            var mood = _data.Moods[petId];
            
            // 根据互动类型改变心情
            var positiveInteractions = new[] { "pet", "play", "feed", "treat", "praise" };
            var negativeInteractions = new[] { "ignore", "scold", "push" };
            
            foreach (var pos in positiveInteractions) {
                if (interactionType.Contains(pos)) {
                    // 正面互动 - 转向积极心情
                    var possibleMoods = new[] { PetMoodType.Happy, PetMoodType.Affectionate, PetMoodType.Playful, PetMoodType.Excited };
                    var newMood = possibleMoods[_random.Next(possibleMoods.Length)];
                    ChangeMood(petId, newMood);
                    return;
                }
            }
            
            foreach (var neg in negativeInteractions) {
                if (interactionType.Contains(neg)) {
                    // 负面互动 - 转向消极心情
                    var possibleMoods = new[] { PetMoodType.Sad, PetMoodType.Angry, PetMoodType.Tired };
                    var newMood = possibleMoods[_random.Next(possibleMoods.Length)];
                    ChangeMood(petId, newMood);
                    return;
                }
            }
            
            // 默认 - 稍微改善心情
            if (mood.CurrentMood == PetMoodType.Sad || mood.CurrentMood == PetMoodType.Tired) {
                ChangeMood(petId, PetMoodType.Neutral);
            }
        }

        // 喂食宠物
        public void FeedPet(string petId) {
            _data.TotalInteractionCount++;
            
            if (!_data.Moods.ContainsKey(petId)) {
                InitializePetMood(petId);
            }
            
            var mood = _data.Moods[petId];
            
            // 喂食改善心情
            if (mood.CurrentMood == PetMoodType.Hungry) {
                ChangeMood(petId, PetMoodType.Happy);
            } else if (mood.CurrentMood == PetMoodType.Tired) {
                ChangeMood(petId, PetMoodType.Calm);
            } else {
                ChangeMood(petId, PetMoodType.Affectionate);
            }
        }

        // 战斗后心情变化
        public void OnBattleEnd(string petId, bool victory) {
            if (!_data.Moods.ContainsKey(petId)) {
                InitializePetMood(petId);
            }
            
            var mood = _data.Moods[petId];
            
            if (victory) {
                // 胜利 - 开心/兴奋
                var possibleMoods = new[] { PetMoodType.Happy, PetMoodType.Excited, PetMoodType.Playful };
                ChangeMood(petId, possibleMoods[_random.Next(possibleMoods.Length)]);
            } else {
                // 失败 - 难过/疲惫
                var possibleMoods = new[] { PetMoodType.Sad, PetMoodType.Tired, PetMoodType.Neutral };
                ChangeMood(petId, possibleMoods[_random.Next(possibleMoods.Length)]);
            }
        }

        // 获取心情加成
        public float GetMoodBonus(string petId, string bonusType) {
            if (!_data.Moods.ContainsKey(petId)) {
                return 0f;
            }
            
            var mood = _data.Moods[petId];
            var effects = _database.GetEffectsForMood(mood);
            
            foreach (var effect in effects) {
                switch (bonusType) {
                    case "stat":
                        return effect.StatBonus;
                    case "exp":
                        return effect.ExpBonus;
                    case "drop":
                        return effect.DropRateBonus;
                }
            }
            
            return 0f;
        }

        // 获取所有心情统计
        public Dictionary<string, int> GetMoodStatistics() {
            return _data.MoodChangesCount;
        }

        // 获取总互动次数
        public int GetTotalInteractionCount() {
            return _data.TotalInteractionCount;
        }

        // 持久化
        private void SaveData() {
            var saveGame = GetNode<("/root/SaveGame") as SaveGame;
            if (saveGame != null) {
                saveGame.SetData(SAVE_KEY, _data);
            }
        }

        private void LoadData() {
            var saveGame = GetNode<("/root/SaveGame") as SaveGame;
            if (saveGame != null) {
                var loaded = saveGame.GetData<PetMoodData>(SAVE_KEY);
                if (loaded != null) {
                    _data = loaded;
                }
            }
        }

        public override Dictionary ExportSaveData()
        {
            // 保存衰减计时器状态
            double currentTime = Time.GetUnixTimeFromSystem();
            double timeSinceLastDecay = currentTime - _lastDecayTime;

            return new Godot.Collections.Dictionary
            {
                ["mood_data"] = _data,
                ["last_decay_time_offset"] = timeSinceLastDecay,
                ["mood_decay_interval"] = _moodDecayInterval,
                ["mood_change_chance"] = _moodChangeChance
            };
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            if (data.TryGetValue("mood_data", out var moodDataObj) && moodDataObj is PetMoodData moodData)
            {
                _data = moodData;
            }

            if (data.TryGetValue("last_decay_time_offset", out var offsetObj))
            {
                double offset = Convert.ToDouble(offsetObj);
                _lastDecayTime = Time.GetUnixTimeFromSystem() - offset;
            }

            if (data.TryGetValue("mood_decay_interval", out var intervalObj))
            {
                _moodDecayInterval = Convert.ToDouble(intervalObj);
            }

            if (data.TryGetValue("mood_change_chance", out var chanceObj))
            {
                _moodChangeChance = Convert.ToDouble(chanceObj);
            }

            SaveData();
        }
    }
}
