using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetInteraction {
    /// <summary>
    /// 宠物互动系统 - 核心逻辑
    /// 基于 Audio Design Patterns 学习成果
    /// </summary>
    public class PetInteractionSystem {
        private static PetInteractionSystem _instance;
        public static PetInteractionSystem Instance {
            get {
                if (_instance == null) _instance = new PetInteractionSystem();
                return _instance;
            }
        }

        private PetInteractionData _data = new PetInteractionData();
        private PetInteractionDatabase _database = PetInteractionDatabase.Instance;

        // 冷却时间追踪
        private Dictionary<string, Dictionary<InteractionType, DateTime>> _cooldowns = new Dictionary<string, Dictionary<InteractionType, DateTime>>();

        public PetInteractionData Data => _data;

        public PetInteractionSystem() {
            LoadData();
        }

        /// <summary>
        /// 执行宠物互动
        /// </summary>
        public InteractionResult PerformInteraction(string petId, string petName, string petType, InteractionType interactionType) {
            // 检查冷却
            if (IsOnCooldown(petId, interactionType)) {
                return InteractionResult.Failed;
            }

            // 获取互动效果配置
            var effect = _database.GetInteractionEffect(interactionType);
            
            // 初始化宠物记录
            if (!_data.petInteractions.ContainsKey(petId)) {
                _data.petInteractions[petId] = new PetInteractionRecord {
                    petId = petId,
                    petName = petName
                };
            }

            var record = _data.petInteractions[petId];
            
            // 计算结果
            var random = new Random();
            InteractionResult result;
            int happinessGain = effect.happinessGain;
            int affectionGain = effect.affectionGain;

            // 特殊结果判定（10%概率触发特殊效果）
            if (random.Next(100) < 10) {
                result = InteractionResult.Special;
                happinessGain = (int)(happinessGain * 1.5f);
                affectionGain = (int)(affectionGain * 1.5f);
            } else if (random.Next(100) < 5) {
                result = InteractionResult.Critical;
                happinessGain = happinessGain * 2;
                affectionGain = affectionGain * 2;
            } else {
                result = InteractionResult.Success;
            }

            // 更新记录
            record.totalInteractions++;
            record.lastInteractionTime = DateTime.Now;
            record.happinessGained += happinessGain;
            record.affectionGained += affectionGain;

            // 更新最喜欢的互动类型
            if (record.favoriteType == interactionType) {
                record.favoriteInteraction++;
            } else {
                record.favoriteInteraction = 1;
                record.favoriteType = interactionType;
            }

            // 添加历史记录
            var history = new InteractionHistory {
                type = interactionType,
                result = result,
                happinessGained = happinessGain,
                affectionGained = affectionGain,
                timestamp = DateTime.Now,
                soundPlayed = effect.soundEffect
            };
            record.history.Insert(0, history);
            
            // 限制历史记录数量
            if (record.history.Count > 50) {
                record.history.RemoveAt(record.history.Count - 1);
            }

            // 更新全局统计
            _data.totalInteractions++;
            if (result == InteractionResult.Special || result == InteractionResult.Critical) {
                _data.specialInteractions++;
            }

            if (!_data.interactionTypeCount.ContainsKey(interactionType)) {
                _data.interactionTypeCount[interactionType] = 0;
            }
            _data.interactionTypeCount[interactionType]++;

            _data.lastInteractionTime = DateTime.Now;

            // 设置冷却
            SetCooldown(petId, interactionType, effect.cooldown);

            // 保存数据
            SaveData();

            return result;
        }

        /// <summary>
        /// 检查是否在冷却中
        /// </summary>
        public bool IsOnCooldown(string petId, InteractionType interactionType) {
            if (!_cooldowns.ContainsKey(petId)) {
                return false;
            }
            
            if (!_cooldowns[petId].ContainsKey(interactionType)) {
                return false;
            }

            var lastTime = _cooldowns[petId][interactionType];
            var effect = _database.GetInteractionEffect(interactionType);
            return (DateTime.Now - lastTime).TotalSeconds < effect.cooldown;
        }

        /// <summary>
        /// 获取冷却剩余时间
        /// </summary>
        public float GetCooldownRemaining(string petId, InteractionType interactionType) {
            if (!_cooldowns.ContainsKey(petId) || !_cooldowns[petId].ContainsKey(interactionType)) {
                return 0f;
            }

            var effect = _database.GetInteractionEffect(interactionType);
            var elapsed = (DateTime.Now - _cooldowns[petId][interactionType]).TotalSeconds;
            return Math.Max(0f, (float)(effect.cooldown - elapsed));
        }

        /// <summary>
        /// 设置冷却
        /// </summary>
        private void SetCooldown(string petId, InteractionType interactionType, float cooldown) {
            if (!_cooldowns.ContainsKey(petId)) {
                _cooldowns[petId] = new Dictionary<InteractionType, DateTime>();
            }
            _cooldowns[petId][interactionType] = DateTime.Now;
        }

        /// <summary>
        /// 获取宠物互动记录
        /// </summary>
        public PetInteractionRecord GetPetRecord(string petId) {
            if (_data.petInteractions.ContainsKey(petId)) {
                return _data.petInteractions[petId];
            }
            return null;
        }

        /// <summary>
        /// 获取随机对话
        /// </summary>
        public string GetRandomDialogue(InteractionType triggerType) {
            var dialogue = _database.GetRandomDialogue(triggerType);
            if (dialogue != null && dialogue.responses.Count > 0) {
                var random = new Random();
                return dialogue.responses[random.Next(dialogue.responses.Count)];
            }
            return "";
        }

        /// <summary>
        /// 获取推荐互动类型
        /// </summary>
        public InteractionType[] GetRecommendedInteractions(string petType) {
            return _database.GetPetTypePreference(petType);
        }

        /// <summary>
        /// 获取互动效果描述
        /// </summary>
        public InteractionEffect GetInteractionEffect(InteractionType type) {
            return _database.GetInteractionEffect(type);
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int> {
                { "totalInteractions", _data.totalInteractions },
                { "specialInteractions", _data.specialInteractions },
                { "uniquePets", _data.petInteractions.Count },
                { "petInteractions", _data.petInteractions.Count },
                { "playCount", _data.interactionTypeCount.ContainsKey(InteractionType.Play) ? _data.interactionTypeCount[InteractionType.Play] : 0 },
                { "petCount", _data.interactionTypeCount.ContainsKey(InteractionType.Pet) ? _data.interactionTypeCount[InteractionType.Pet] : 0 },
                { "feedCount", _data.interactionTypeCount.ContainsKey(InteractionType.Feed) ? _data.interactionTypeCount[InteractionType.Feed] : 0 },
                { "cuddleCount", _data.interactionTypeCount.ContainsKey(InteractionType.Cuddle) ? _data.interactionTypeCount[InteractionType.Cuddle] : 0 }
            };
        }

        /// <summary>
        /// 重置统计
        /// </summary>
        public void ResetStatistics() {
            _data.totalInteractions = 0;
            _data.specialInteractions = 0;
            _data.interactionTypeCount.Clear();
            _data.petInteractions.Clear();
            _cooldowns.Clear();
            SaveData();
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveData() {
            // TODO: Implement save to file
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public void LoadData() {
            // TODO: Implement load from file
        }
    }
}
