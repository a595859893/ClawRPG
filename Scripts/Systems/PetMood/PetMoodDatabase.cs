using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetMood {
    public class PetMoodDatabase {
        private static PetMoodDatabase _instance;
        public static PetMoodDatabase Instance {
            get {
                if (_instance == null) _instance = new PetMoodDatabase();
                return _instance;
            }
        }

        // 心情效果配置
        public List<PetMoodEffect> MoodEffects = new List<PetMoodEffect>() {
            // Happy 心情效果
            new PetMoodEffect {
                EffectId = "happy_battle",
                Description = "战斗时攻击力提升",
                TriggerMood = PetMoodType.Happy,
                RequiredIntensity = MoodIntensity.Medium,
                StatBonus = 0.1f
            },
            new PetMoodEffect {
                EffectId = "happy_exp",
                Description = "获得经验增加",
                TriggerMood = PetMoodType.Happy,
                RequiredIntensity = MoodIntensity.High,
                ExpBonus = 0.15f
            },
            
            // Excited 心情效果
            new PetMoodEffect {
                EffectId = "excited_drop",
                Description = "掉落率提升",
                TriggerMood = PetMoodType.Excited,
                RequiredIntensity = MoodIntensity.Medium,
                DropRateBonus = 0.2f
            },
            new PetMoodEffect {
                EffectId = "excited_battle",
                Description = "全属性提升",
                TriggerMood = PetMoodType.Excited,
                RequiredIntensity = MoodIntensity.High,
                StatBonus = 0.15f
            },
            
            // Affectionate 心情效果
            new PetMoodEffect {
                EffectId = "affection_defense",
                Description = "防御力大幅提升",
                TriggerMood = PetMoodType.Affectionate,
                RequiredIntensity = MoodIntensity.High,
                StatBonus = 0.2f
            },
            new PetMoodEffect {
                EffectId = "affection_exp",
                Description = "经验获取大幅提升",
                TriggerMood = PetMoodType.Affectionate,
                RequiredIntensity = MoodIntensity.Extreme,
                ExpBonus = 0.25f
            },
            
            // Playful 心情效果
            new PetMoodEffect {
                EffectId = "playful_speed",
                Description = "速度提升",
                TriggerMood = PetMoodType.Playful,
                RequiredIntensity = MoodIntensity.Medium,
                StatBonus = 0.08f
            },
            
            // Calm 心情效果
            new PetMoodEffect {
                EffectId = "calm_crit",
                Description = "暴击率提升",
                TriggerMood = PetMoodType.Calm,
                RequiredIntensity = MoodIntensity.High,
                StatBonus = 0.12f
            },
            
            // Tired 心情效果（负面）
            new PetMoodEffect {
                EffectId = "tired_debuff",
                Description = "全属性下降",
                TriggerMood = PetMoodType.Tired,
                RequiredIntensity = MoodIntensity.High,
                StatBonus = -0.1f
            },
            
            // Hungry 心情效果（负面）
            new PetMoodEffect {
                EffectId = "hungry_debuff",
                Description = "战斗能力下降",
                TriggerMood = PetMoodType.Hungry,
                RequiredIntensity = MoodIntensity.Medium,
                StatBonus = -0.08f
            },
            
            // Angry 心情效果
            new PetMoodEffect {
                EffectId = "angry_attack",
                Description = "攻击大幅提升",
                TriggerMood = PetMoodType.Angry,
                RequiredIntensity = MoodIntensity.High,
                StatBonus = 0.2f
            },
            new PetMoodEffect {
                EffectId = "angry_exp",
                Description = "击杀经验增加",
                TriggerMood = PetMoodType.Angry,
                RequiredIntensity = MoodIntensity.Extreme,
                ExpBonus = 0.2f
            },
            
            // Sad 心情效果（负面）
            new PetMoodEffect {
                EffectId = "sad_debuff",
                Description = "全属性下降",
                TriggerMood = PetMoodType.Sad,
                RequiredIntensity = MoodIntensity.High,
                StatBonus = -0.15f
            }
        };

        // 心情转换规则
        public Dictionary<PetMoodType, List<PetMoodType>> MoodTransitions = new Dictionary<PetMoodType, List<PetMoodType>>() {
            { PetMoodType.Happy, new List<PetMoodType> { PetMoodType.Playful, PetMoodType.Excited, PetMoodType.Affectionate, PetMoodType.Neutral } },
            { PetMoodType.Sad, new List<PetMoodType> { PetMoodType.Hungry, PetMoodType.Tired, PetMoodType.Neutral } },
            { PetMoodType.Angry, new List<PetMoodType> { PetMoodType.Happy, PetMoodType.Playful, PetMoodType.Neutral } },
            { PetMoodType.Playful, new List<PetMoodType> { PetMoodType.Excited, PetMoodType.Happy, PetMoodType.Affectionate, PetMoodType.Tired } },
            { PetMoodType.Tired, new List<PetMoodType> { PetMoodType.Hungry, PetMoodType.Sad, PetMoodType.Calm } },
            { PetMoodType.Hungry, new List<PetMoodType> { PetMoodType.Tired, PetMoodType.Sad, PetMoodType.Angry } },
            { PetMoodType.Excited, new List<PetMoodType> { PetMoodType.Happy, PetMoodType.Playful, PetMoodType.Affectionate, PetMoodType.Calm } },
            { PetMoodType.Calm, new List<PetMoodType> { PetMoodType.Happy, PetMoodType.Affectionate, PetMoodType.Tired, PetMoodType.Neutral } },
            { PetMoodType.Affectionate, new List<PetMoodType> { PetMoodType.Happy, PetMoodType.Playful, PetMoodType.Excited, PetMoodType.Calm } },
            { PetMoodType.Neutral, new List<PetMoodType> { PetMoodType.Happy, PetMoodType.Sad, PetMoodType.Calm, PetMoodType.Playful } }
        };

        // 心情颜色
        public Dictionary<PetMoodType, Color> MoodColors = new Dictionary<PetMoodType, Color>() {
            { PetMoodType.Happy, new Color(1f, 0.84f, 0f) },      // 金色
            { PetMoodType.Sad, new Color(0.5f, 0.5f, 0.8f) },    // 蓝色
            { PetMoodType.Angry, new Color(1f, 0.3f, 0.3f) },    // 红色
            { PetMoodType.Playful, new Color(1f, 0.6f, 0.8f) },  // 粉色
            { PetMoodType.Tired, new Color(0.6f, 0.6f, 0.6f) },   // 灰色
            { PetMoodType.Hungry, new Color(1f, 0.5f, 0f) },      // 橙色
            { PetMoodType.Excited, new Color(1f, 0.8f, 0.2f) },   // 黄色
            { PetMoodType.Calm, new Color(0.4f, 0.8f, 1f) },      // 天蓝色
            { PetMoodType.Affectionate, new Color(1f, 0.4f, 0.7f) }, // 玫红色
            { PetMoodType.Neutral, new Color(0.8f, 0.8f, 0.8f) }  // 银灰色
        };

        // 心情图标（使用 emoji 字符）
        public Dictionary<PetMoodType, string> MoodEmojis = new Dictionary<PetMoodType, string>() {
            { PetMoodType.Happy, "😊" },
            { PetMoodType.Sad, "😢" },
            { PetMoodType.Angry, "😠" },
            { PetMoodType.Playful, "😜" },
            { PetMoodType.Tired, "😴" },
            { PetMoodType.Hungry, "🍖" },
            { PetMoodType.Excited, "🤩" },
            { PetMoodType.Calm, "😌" },
            { PetMoodType.Affectionate, "🥰" },
            { PetMoodType.Neutral, "😐" }
        };

        // 获取心情效果
        public List<PetMoodEffect> GetEffectsForMood(PetMood mood) {
            var effects = new List<PetMoodEffect>();
            foreach (var effect in MoodEffects) {
                if (effect.TriggerMood == mood.CurrentMood && 
                    (int)effect.RequiredIntensity <= (int)mood.Intensity) {
                    effects.Add(effect);
                }
            }
            return effects;
        }

        // 获取随机心情转换
        public PetMoodType GetRandomMoodTransition(PetMoodType currentMood) {
            if (!MoodTransitions.ContainsKey(currentMood)) {
                return PetMoodType.Neutral;
            }
            var transitions = MoodTransitions[currentMood];
            var random = new Random();
            return transitions[random.Next(transitions.Count)];
        }
    }
}
