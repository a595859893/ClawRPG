using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetInteraction {
    /// <summary>
    /// 宠物互动数据库
    /// </summary>
    public class PetInteractionDatabase {
        private static PetInteractionDatabase _instance;
        public static PetInteractionDatabase Instance {
            get {
                if (_instance == null) _instance = new PetInteractionDatabase();
                return _instance;
            }
        }

        // 互动效果配置
        public Dictionary<InteractionType, InteractionEffect> interactionEffects = new Dictionary<InteractionType, InteractionEffect>();
        
        // 对话内容
        public List<DialogueContent> dialogueContents = new List<DialogueContent>();
        
        // 宠物类型偏好
        public Dictionary<string, InteractionType[]> petTypePreferences = new Dictionary<string, InteractionType[]>();

        public PetInteractionDatabase() {
            InitializeInteractionEffects();
            InitializeDialogueContents();
            InitializePetTypePreferences();
        }

        private void InitializeInteractionEffects() {
            // 抚摸
            interactionEffects[InteractionType.Pet] = new InteractionEffect {
                type = InteractionType.Pet,
                name = "抚摸",
                description = "轻轻地抚摸你的宠物",
                happinessGain = 5,
                affectionGain = 3,
                cooldown = 10f,
                soundEffect = "pet_gentle",
                particleEffect = "heart_particles",
                duration = 2f
            };

            // 玩耍
            interactionEffects[InteractionType.Play] = new InteractionEffect {
                type = InteractionType.Play,
                name = "玩耍",
                description = "和你的宠物一起玩耍",
                happinessGain = 8,
                affectionGain = 5,
                cooldown = 30f,
                soundEffect = "playful_bark",
                particleEffect = "sparkle_particles",
                duration = 3f
            };

            // 对话
            interactionEffects[InteractionType.Talk] = new InteractionEffect {
                type = InteractionType.Talk,
                name = "对话",
                description = "和你的宠物聊天",
                happinessGain = 3,
                affectionGain = 4,
                cooldown = 5f,
                soundEffect = "chat_happy",
                particleEffect = "chat_bubbles",
                duration = 1.5f
            };

            // 喂食
            interactionEffects[InteractionType.Feed] = new InteractionEffect {
                type = InteractionType.Feed,
                name = "喂食",
                description = "给你的宠物喂食",
                happinessGain = 10,
                affectionGain = 6,
                cooldown = 60f,
                soundEffect = "eating_crunch",
                particleEffect = "food_particles",
                duration = 2.5f
            };

            // 梳理
            interactionEffects[InteractionType.Groom] = new InteractionEffect {
                type = InteractionType.Groom,
                name = "梳理",
                description = "为你的宠物梳理毛发",
                happinessGain = 6,
                affectionGain = 4,
                cooldown = 45f,
                soundEffect = "grooming_brush",
                particleEffect = "shine_particles",
                duration = 3f
            };

            // 训练
            interactionEffects[InteractionType.Train] = new InteractionEffect {
                type = InteractionType.Train,
                name = "训练",
                description = "训练你的宠物学习新技巧",
                happinessGain = 4,
                affectionGain = 2,
                cooldown = 120f,
                soundEffect = "training_whistle",
                particleEffect = "star_particles",
                duration = 5f
            };

            // 抱抱
            interactionEffects[InteractionType.Cuddle] = new InteractionEffect {
                type = InteractionType.Cuddle,
                name = "抱抱",
                description = "给你的宠物一个温暖的拥抱",
                happinessGain = 7,
                affectionGain = 8,
                cooldown = 60f,
                soundEffect = "cuddle_purr",
                particleEffect = "love_particles",
                duration = 4f
            };

            // 治疗
            interactionEffects[InteractionType.Heal] = new InteractionEffect {
                type = InteractionType.Heal,
                name = "治疗",
                description = "治疗你的宠物",
                happinessGain = 2,
                affectionGain = 5,
                cooldown = 300f,
                soundEffect = "healing_chime",
                particleEffect = "heal_particles",
                duration = 2f
            };
        }

        private void InitializeDialogueContents() {
            // 抚摸时的对话
            dialogueContents.Add(new DialogueContent {
                dialogueId = "pet_1",
                triggerType = InteractionType.Pet,
                responses = new List<string> {
                    "你的毛发真柔软~",
                    "最乖了~",
                    "主人爱你哦~",
                    "嘿嘿，痒痒的~"
                },
                happinessBonus = 2,
                affectionBonus = 1
            });

            // 玩耍时的对话
            dialogueContents.Add(new DialogueContent {
                dialogueId = "play_1",
                triggerType = InteractionType.Play,
                responses = new List<string> {
                    "太好玩了！再来一次！",
                    "我喜欢和你一起玩！",
                    "追逐时间到！",
                    "我是最快的！"
                },
                happinessBonus = 3,
                affectionBonus = 2
            });

            // 对话时的回复
            dialogueContents.Add(new DialogueContent {
                dialogueId = "talk_1",
                triggerType = InteractionType.Talk,
                responses = new List<string> {
                    "主人回来啦！",
                    "今天发生了什么？",
                    "我好想你~",
                    "主人辛苦了！"
                },
                happinessBonus = 1,
                affectionBonus = 2
            });

            // 喂食时的对话
            dialogueContents.Add(new DialogueContent {
                dialogueId = "feed_1",
                triggerType = InteractionType.Feed,
                responses = new List<string> {
                    "好吃！谢谢主人！",
                    "肚子饿了~",
                    "这个味道真棒！",
                    "还要还要！"
                },
                happinessBonus = 4,
                affectionBonus = 3
            });

            // 梳理时的对话
            dialogueContents.Add(new DialogueContent {
                dialogueId = "groom_1",
                triggerType = InteractionType.Groom,
                responses = new List<string> {
                    "舒服~",
                    "我变得好看了！",
                    "主人好温柔~",
                    "毛发飘逸的感觉真好！"
                },
                happinessBonus = 2,
                affectionBonus = 2
            });

            // 训练时的对话
            dialogueContents.Add(new DialogueContent {
                dialogueId = "train_1",
                triggerType = InteractionType.Train,
                responses = new List<string> {
                    "我学会了！",
                    "看我的厉害！",
                    "我会努力的！",
                    "主人教的都记住了！"
                },
                happinessBonus = 2,
                affectionBonus = 1
            });

            // 抱抱时的对话
            dialogueContents.Add(new DialogueContent {
                dialogueId = "cuddle_1",
                triggerType = InteractionType.Cuddle,
                responses = new List<string> {
                    "最喜欢主人了！",
                    "暖暖的~",
                    "永远在一起！",
                    " heartbeat ~"
                },
                happinessBonus = 3,
                affectionBonus = 4
            });

            // 治疗时的对话
            dialogueContents.Add(new DialogueContent {
                dialogueId = "heal_1",
                triggerType = InteractionType.Heal,
                responses = new List<string> {
                    "感觉好多了！",
                    "谢谢主人~",
                    "又有力气了！",
                    "我会更努力的！"
                },
                happinessBonus = 1,
                affectionBonus = 3
            });
        }

        private void InitializePetTypePreferences() {
            // 狗 - 喜欢玩耍和训练
            petTypePreferences["Dog"] = new InteractionType[] { InteractionType.Play, InteractionType.Train, InteractionType.Pet };
            
            // 猫 - 喜欢梳理和抱抱
            petTypePreferences["Cat"] = new InteractionType[] { InteractionType.Groom, InteractionType.Cuddle, InteractionType.Pet };
            
            // 鸟 - 喜欢对话和玩耍
            petTypePreferences["Bird"] = new InteractionType[] { InteractionType.Talk, InteractionType.Play, InteractionType.Pet };
            
            // 兔子 - 喜欢喂食和抱抱
            petTypePreferences["Rabbit"] = new InteractionType[] { InteractionType.Feed, InteractionType.Cuddle, InteractionType.Pet };
            
            // 龙 - 喜欢训练和对话
            petTypePreferences["Dragon"] = new InteractionType[] { InteractionType.Train, InteractionType.Talk, InteractionType.Play };
            
            // 史莱姆 - 喜欢喂食和玩耍
            petTypePreferences["Slime"] = new InteractionType[] { InteractionType.Feed, InteractionType.Play, InteractionType.Cuddle };
            
            // 骷髅 - 喜欢治疗和对话
            petTypePreferences["Skeleton"] = new InteractionType[] { InteractionType.Heal, InteractionType.Talk, InteractionType.Train };
            
            // 元素 - 喜欢训练和治疗
            petTypePreferences["Elemental"] = new InteractionType[] { InteractionType.Train, InteractionType.Heal, InteractionType.Talk };
        }

        /// <summary>
        /// 获取互动效果
        /// </summary>
        public InteractionEffect GetInteractionEffect(InteractionType type) {
            if (interactionEffects.ContainsKey(type)) {
                return interactionEffects[type];
            }
            return interactionEffects[InteractionType.Pet]; // 默认抚摸
        }

        /// <summary>
        /// 获取随机对话
        /// </summary>
        public DialogueContent GetRandomDialogue(InteractionType triggerType) {
            var validDialogues = dialogueContents.FindAll(d => d.triggerType == triggerType);
            if (validDialogues.Count > 0) {
                var random = new Random();
                return validDialogues[random.Next(validDialogues.Count)];
            }
            return null;
        }

        /// <summary>
        /// 获取宠物类型偏好
        /// </summary>
        public InteractionType[] GetPetTypePreference(string petType) {
            if (petTypePreferences.ContainsKey(petType)) {
                return petTypePreferences[petType];
            }
            return new InteractionType[] { InteractionType.Pet, InteractionType.Talk };
        }
    }
}
