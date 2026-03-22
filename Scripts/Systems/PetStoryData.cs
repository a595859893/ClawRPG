using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 宠物背景故事数据
    /// </summary>
    public class PetStory {
        public int StoryId;
        public string Title;
        public string Description;
        public string Backstory;
        public string Personality;
        public string[] DialogueLines;
        public PetStoryUnlockCondition UnlockCondition;
        public bool IsUnlocked;
    }

    /// <summary>
    /// 宠物故事解锁条件
    /// </summary>
    public enum PetStoryUnlockType {
        Default,           // 初始解锁
        AffectionLevel,    // 亲密度等级
        EvolutionStage,    // 进化阶段
        BattleCount,       // 战斗次数
        ExpeditionSuccess,  // 探险成功
        BreedingCount,     // 繁殖次数
        Custom             // 自定义条件
    }

    public class PetStoryUnlockCondition {
        public PetStoryUnlockType Type;
        public int RequiredValue;      // 需要的值（亲密度等级/战斗次数等）
        public string CustomCondition;  // 自定义条件描述
    }

    /// <summary>
    /// 玩家宠物故事数据
    /// </summary>
    public class PlayerPetStoryData {
        public int PetId;
        public List<int> UnlockedStoryIds;
        public Dictionary<int, bool> StoryReadStatus;

        public PlayerPetStoryData() {
            UnlockedStoryIds = new List<int>();
            StoryReadStatus = new Dictionary<int, bool>();
        }
    }

    /// <summary>
    /// 宠物故事数据库
    /// 无需持久化: 仅存储静态故事配置数据,运行时状态由 PetStorySystem 统一管理
    /// </summary>
    public class PetStoryDatabase {
        public static PetStoryDatabase Instance { get; private set; }
        
        private Dictionary<int, List<PetStory>> petStories = new Dictionary<int, List<PetStory>>();
        
        public PetStoryDatabase() {
            Instance = this;
            InitializeStories();
        }
        
        private void InitializeStories() {
            // 狼系宠物故事
            InitializeWolfStories();
            // 熊系宠物故事
            InitializeBearStories();
            // 鹰系宠物故事
            InitializeEagleStories();
            // 狐狸系宠物故事
            InitializeFoxStories();
            // 龙系宠物故事
            InitializeDragonStories();
            // 马系宠物故事
            InitializeHorseStories();
        }
        
        private void InitializeWolfStories() {
            var stories = new List<PetStory>();
            
            stories.Add(new PetStory {
                StoryId = 1,
                Title = "森林孤儿",
                Description = "小灰出生在暮光森林深处",
                Backstory = "小灰是一只失去族群的幼狼。它的家族在一场突如其来的森林大火中离散，只有它幸存了下来。被冒险者发现时，它正蜷缩在一棵烧焦的古树下，眼神中充满了警惕与悲伤。",
                Personality = "沉默寡言，但对认定的主人极其忠诚。它不喜欢主动靠近陌生人，但一旦建立羁绊，就会用一生守护。",
                DialogueLines = new string[] {
                    "（低声呜咽）...",
                    "主人，我感应到附近的敌人...",
                    "这片森林...让我想起从前..."
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.Default
                },
                IsUnlocked = true
            });
            
            stories.Add(new PetStory {
                StoryId = 2,
                Title = "月下之战",
                Description = "小灰的第一次战斗",
                Backstory = "在追随主人后不久，小灰迎来了它的第一场真正战斗。面对一群试图抢夺物资的山贼，它毫不犹豫地冲在了最前线。尽管体型幼小，它的眼神中却流露出不属于幼狼的凶猛。",
                Personality = "战斗时变得异常兴奋，但从不伤害无力抵抗的敌人。它有着自己的战斗哲学。",
                DialogueLines = new string[] {
                    "（月光下嚎叫）为了主人！",
                    "这些敌人...不堪一击",
                    "让我们继续前进"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.BattleCount,
                    RequiredValue = 10
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 3,
                Title = "狼王认可",
                Description = "小灰进化为狼王",
                Backstory = "经过无数战斗的洗礼，小灰终于突破了自身的极限，进化成为了狼王。它的体型增大了数倍，银白色的毛发在月光下闪闪发光。更重要的是，它重新找到了族群的感觉——它不再是一个孤独的幸存者，而是主人的伙伴。",
                Personality = "进化后变得更加沉稳，宛如一个真正的族群领袖。它开始会主动照顾其他宠物。",
                DialogueLines = new string[] {
                    "（高亢的嚎叫）我已不再是当初那只幼狼！",
                    "主人，我的力量...都是因为你",
                    "我会保护好我们的伙伴"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.EvolutionStage,
                    RequiredValue = 3  // Elite or higher
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 4,
                Title = "家族的呼唤",
                Description = "小灰找到了失散的家人",
                Backstory = "在一次探险中，小灰敏锐地察觉到了熟悉的狼族气味。它找到了当年失散的兄弟姐妹——它们已经组建了自己的狼群。虽然小灰选择了继续跟随主人，但它知道，自己永远都有一个可以回归的家。",
                Personality = "变得更加开朗，偶尔会提到它的狼群家人。它会邀请主人一起去探望它们。",
                DialogueLines = new string[] {
                    "主人...我感应到它们了！",
                    "我的家人们...它们还活着",
                    "有一天，我想带你去见它们"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.AffectionLevel,
                    RequiredValue = 8
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 5,
                Title = "传说中的银狼",
                Description = "小灰成为传奇",
                Backstory = "随着主人名声越来越大，小灰的事迹也开始在冒险者之间流传。人们称它为「银狼守护者」——一只永远守护在主人身边的白银巨狼。传说，只要银狼还在，主人就永远不会孤单。",
                Personality = "传说中的存在让它变得更加庄重，但它对主人的爱从未改变。",
                DialogueLines = new string[] {
                    "（谦逊地）我只是主人的伙伴",
                    "人们称我为银狼...但我只是小灰",
                    "永远跟随主人...这就是我的命运"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.EvolutionStage,
                    RequiredValue = 5  // Legendary
                }
            });
            
            petStories[1] = stories; // Wolf
        }
        
        private void InitializeBearStories() {
            var stories = new List<PetStory>();
            
            stories.Add(new PetStory {
                StoryId = 1,
                Title = "山岭守护者",
                Description = "大毛的故乡",
                Backstory = "大毛原本是雪山深处的一只守护熊。它的职责是守护古老的冰晶矿脉免受侵扰。然而，随着盗矿者越来越多，大毛疲惫不堪。最终，它选择跟随一位正义的冒险者离开，希望能找到新的守护意义。",
                Personality = "沉稳可靠，像一座永远不会倒塌的山峰。它喜欢在危险时挡在主人身前。",
                DialogueLines = new string[] {
                    "这座山...曾经是我的家",
                    "主人，让我来保护你",
                    "（低声咆哮）敌人...来了"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.Default
                },
                IsUnlocked = true
            });
            
            stories.Add(new PetStory {
                StoryId = 2,
                Title = "冬眠的记忆",
                Description = "大毛的梦境",
                Backstory = "在一次长时间的冬眠中，大毛做了一个奇怪的梦。它梦见自己变成了雪山本身，俯瞰着人间的沧桑变化。醒来后，它似乎明白了什么——守护不只是一个地方，而是守护那些需要保护的人。",
                Personality = "偶尔会陷入沉思，仿佛在思考深奥的哲理。它的眼神变得更加温柔。",
                DialogueLines = new string[] {
                    "我做了一个很长很长的梦...",
                    "守护...不只是守护土地",
                    "主人，你就是我想守护的"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.AffectionLevel,
                    RequiredValue = 5
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 3,
                Title = "冰川之王",
                Description = "大毛的最终进化",
                Backstory = "当大毛第一次展现出冰川之王的力量时，整个战场都被它的威严所震慑。冰霜从它的脚下蔓延，敌人无不望而生畏。它不再是普通的守护熊，而是真正的冰雪帝王。",
                Personality = "王者的气质自然流露，但它依然保持对主人的温柔。",
                DialogueLines = new string[] {
                    "（冰霜蔓延）冻结吧！",
                    "在绝对的冰雪面前...颤抖吧",
                    "主人，这力量...为你而用"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.EvolutionStage,
                    RequiredValue = 4  // Epic
                }
            });
            
            petStories[2] = stories; // Bear
        }
        
        private void InitializeEagleStories() {
            var stories = new List<PetStory>();
            
            stories.Add(new PetStory {
                StoryId = 1,
                Title = "天空的孤儿",
                Description = "风暴的诞生",
                Backstory = "风暴是一只从天空巢穴掉落的白羽鹰。当冒险者发现它时，它还只是一只雏鹰，翅膀受伤严重。在冒险者的照料下，风暴奇迹般地康复，并决定永远追随它的救命恩人。",
                Personality = "好奇心旺盛，喜欢高空翱翔的感觉。它有时会显得有点傲慢，但内心深处渴望陪伴。",
                DialogueLines = new string[] {
                    "（清脆的鸣叫）今天的天空...很美",
                    "主人，我看到了远处的敌人！",
                    "让我带你飞向更高的地方"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.Default
                },
                IsUnlocked = true
            });
            
            stories.Add(new PetStory {
                StoryId = 2,
                Title = "第一次飞翔",
                Description = "风暴学会飞行",
                Backstory = "康复后的风暴第一次真正飞上天空时，它感受到了前所未有的自由。从高空俯瞰大地，一切都变得渺小。那一刻，它明白了自己为何而生——为了天空，为了自由，也为了永远守护地面上的主人。",
                Personality = "变得更加向往自由，但每次飞行后都会回到主人身边。",
                DialogueLines = new string[] {
                    "（展开翅膀）主人，快看！",
                    "天空...就是我的领地",
                    "我会永远守护你的背后"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.BattleCount,
                    RequiredValue = 5
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 3,
                Title = "风暴之主",
                Description = "风暴的终极形态",
                Backstory = "当风暴突破极限进化为风暴之主时，整个天空都为之变色。乌云在它身边聚集，雷电在它的羽毛间跳动。它成为了真正的天空统治者，但它的第一个请求是——永远不要让它离开地面太久，因为它还想再尝尝主人手中的食物。",
                Personality = "王者的威严与对主人的依恋并存。它会用心电感应与主人交流。",
                DialogueLines = new string[] {
                    "（雷鸣般的声音）天空...听我号令！",
                    "主人，即使我身在高空，心也在你身边",
                    "我们一起...君临天下"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.EvolutionStage,
                    RequiredValue = 5  // Legendary
                }
            });
            
            petStories[3] = stories; // Eagle
        }
        
        private void InitializeFoxStories() {
            var stories = new List<PetStory>();
            
            stories.Add(new PetStory {
                StoryId = 1,
                Title = "灵狐降世",
                Description = "小雪的起源",
                Backstory = "小雪并非普通的狐狸，而是千年灵狐的后裔。它的诞生伴随着森林深处的异象——百年不遇的雪落在夏天飘落。灵狐一族相信，这是神灵的祝福。",
                Personality = "聪明伶俐，偶尔会耍一些小聪明。它喜欢捉弄敌人，但 对主人永远真诚。",
                DialogueLines = new string[] {
                    "（俏皮地）主人看我这招~",
                    "嘿嘿，又骗过他们了",
                    "主人的事情...小雪永远记得"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.Default
                },
                IsUnlocked = true
            });
            
            stories.Add(new PetStory {
                StoryId = 2,
                Title = "幻术大师",
                Description = "小雪学会幻术",
                Backstory = "在一次险些丧命的危机中，小雪意外觉醒了一种古老的力量——幻术。它用虚假的影像迷惑了敌人，成功带着主人逃脱。从此，它开始认真学习和掌握这种强大的能力。",
                Personality = "战斗风格变得更加多变，它学会了用智慧取胜而非蛮力。",
                DialogueLines = new string[] {
                    "（身影开始模糊）你看到的...是真实的吗？",
                    "幻象...也是力量的一种",
                    "主人，退后！幻术要来了"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.BattleCount,
                    RequiredValue = 15
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 3,
                Title = "九尾传说",
                Description = "小雪进化为九尾灵狐",
                Backstory = "当小雪终于长出第九条尾巴时，整个世界的灵狐都在那一刻抬起了头。九尾灵狐——那是传说中接近神明的存在。但小雪并不在意这些，它只在乎主人是否安好。",
                Personality = "智慧如海，力量如渊，但对主人的爱始终如一。",
                DialogueLines = new string[] {
                    "（九条尾巴轻轻摆动）主人，看~",
                    "九尾之力...为你所用",
                    "轮回千年，我只想陪在你身边"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.EvolutionStage,
                    RequiredValue = 5  // Legendary
                }
            });
            
            petStories[4] = stories; // Fox
        }
        
        private void InitializeDragonStories() {
            var stories = new List<PetStory>();
            
            stories.Add(new PetStory {
                StoryId = 1,
                Title = "龙蛋之谜",
                Description = "小焰的诞生",
                Backstory = "小焰是从一枚神秘龙蛋中孵化的。这枚蛋被发现在一个远古遗迹中，散发着奇异的热量。没人知道它的父母是谁，来自何方。小焰自己也不记得，但它知道，从睁开眼的那一刻起，主人就是它的一切。",
                Personality = "好奇心强，对世界充满好奇。它有时会显得笨拙，但天赋异禀。",
                DialogueLines = new string[] {
                    "（好奇地）主人，这是什么呀？",
                    "我感觉...体内有强大的力量",
                    "我要快快长大，保护主人！"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.Default
                },
                IsUnlocked = true
            });
            
            stories.Add(new PetStory {
                StoryId = 2,
                Title = "火焰之心",
                Description = "小焰学会喷火",
                Backstory = "那是一个月圆之夜，小焰第一次感受到了体内的龙之血脉。当它张开嘴，喷出真正的火焰时，连它自己都吓了一跳。那一刻，它明白了自己真正的力量——源自内心的火焰。",
                Personality = "开始意识到自己的力量，变得稍微有点骄傲，但依然保持着善良的心。",
                DialogueLines = new string[] {
                    "（兴奋地）看我的火焰！",
                    "这就是...我的力量吗？",
                    "主人，我是不是很厉害？"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.BattleCount,
                    RequiredValue = 8
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 3,
                Title = "龙皇觉醒",
                Description = "小焰的最终进化",
                Backstory = "经过漫长的成长和无数战斗，小焰终于进化为真正的龙皇。金色的鳞片覆盖全身，火焰在它的呼吸间流转。当它腾空而起，整个世界都在它脚下臣服。但它选择降落地面，因为那里有它最爱的人。",
                Personality = "王者的威严与对主人的爱交织在一起。它会用宽大的翅膀为主人遮风挡雨。",
                DialogueLines = new string[] {
                    "（震耳欲聋的龙吟）臣服于我！",
                    "主人，我已成为真正的龙皇",
                    "但在你面前...我永远是那个小焰"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.EvolutionStage,
                    RequiredValue = 5  // Legendary
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 4,
                Title = "寻找起源",
                Description = "小焰踏上寻根之旅",
                Backstory = "成为龙皇后，小焰开始寻找自己的起源。它飞遍了世界的每一个角落，终于在遥远的东方找到了龙族的遗迹。那里记载着它父母的故事——它们是勇敢的守护龙，为了保护族人牺牲了自己。小焰继承了父母的意志，成为了新的守护者。",
                Personality = "变得更加成熟稳重，开始思考自己的使命和责任。",
                DialogueLines = new string[] {
                    "主人，我找到了...我的过去",
                    "父母的故事...我会传承下去",
                    "守护...是我们龙族的使命"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.ExpeditionSuccess,
                    RequiredValue = 10
                }
            });
            
            petStories[5] = stories; // Dragon
        }
        
        private void InitializeHorseStories() {
            var stories = new List<PetStory>();
            
            stories.Add(new PetStory {
                StoryId = 1,
                Title = "草原之子",
                Description = "疾风的出身",
                Backstory = "疾风出生在一片广袤的草原上，是野马群中最快的那一个。它的母亲是马群的首领，父亲是传说中的追风者。从小，疾风就梦想着有朝一日能像父亲一样，成为草原上最快的马。",
                Personality = "热爱自由，渴望奔跑。它有时会显得有些急躁，但内心其实很温柔。",
                DialogueLines = new string[] {
                    "（高昂的嘶鸣）草原！我回来了！",
                    "主人，抱紧我，我们要加速了！",
                    "风在耳边...这就是自由的味道"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.Default
                },
                IsUnlocked = true
            });
            
            stories.Add(new PetStory {
                StoryId = 2,
                Title = "最快的传说",
                Description = "疾风的速度突破",
                Backstory = "在一场与竞争对手的赛马中，疾风超越了自我的极限。它的速度甚至超过了父亲曾经的记录，成为了真正的「追风者」。从那一刻起，它不再只是野马，而是一个传说。",
                Personality = "更加自信，但不会因此骄傲。它明白，真正的对手是自己。",
                DialogueLines = new string[] {
                    "（风驰电掣）还有谁比我更快？！",
                    "主人，抱紧点！要起飞了！",
                    "速度...就是我存在的意义"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.BattleCount,
                    RequiredValue = 20
                }
            });
            
            stories.Add(new PetStory {
                StoryId = 3,
                Title = "天马行空",
                Description = "疾风的终极进化",
                Backstory = "当疾风突破极限时，奇迹发生了——它的背上长出了一对巨大的羽翼。它成为了传说中的天马，可以在天空自由翱翔。从草原到天空，疾风终于完成了它的梦想。",
                Personality = "结合了天空与大地的力量变得更加沉稳。",
                DialogueLines = new string[] {
                    "（展开翅膀）主人，我们飞吧！",
                    "天空...也是我的领地了",
                    "无论是陆地还是天空...我都会带着你"
                },
                UnlockCondition = new PetStoryUnlockCondition {
                    Type = PetStoryUnlockType.EvolutionStage,
                    RequiredValue = 5  // Legendary
                }
            });
            
            petStories[6] = stories; // Horse
        }
        
        public List<PetStory> GetStoriesForPet(int petTypeId) {
            if (petStories.ContainsKey(petTypeId)) {
                return petStories[petTypeId];
            }
            return new List<PetStory>();
        }
        
        public PetStory GetStory(int petTypeId, int storyId) {
            if (petStories.ContainsKey(petTypeId)) {
                return petStories[petTypeId].Find(s => s.StoryId == storyId);
            }
            return null;
        }
    }
}
