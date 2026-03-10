using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 教程系统 - 引导新玩家了解游戏机制
    /// </summary>
    public class TutorialStep {
        public string StepId;
        public string Title;
        public string Description;
        public string HighlightNodePath;  // 高亮节点路径
        public Vector2 HighlightPosition; // 高亮位置
        public float HighlightRadius;
        public TutorialTrigger Trigger;
        public TutorialTargetType TargetType;
        public string TargetAction;       // 目标操作
        public float Duration;            // 持续时间(秒)，0表示手动关闭
        public bool IsCompleted;
        public bool CanSkip;
    }

    public enum TutorialTrigger {
        Manual,           // 手动触发
        GameStart,        // 游戏开始
        FirstCombat,      // 首次战斗
        FirstCraft,       // 首次合成
        FirstEnchant,    // 首次附魔
        FirstQuest,      // 首次任务
        FirstMount,      // 首次坐骑
        FirstPet,        // 首次宠物
        LevelUp,         // 升级
        BossEncounter,   // 遇到Boss
        RegionEnter,     // 进入区域
        ItemCollected,   // 收集物品
        SkillUnlocked,   // 技能解锁
        FirstPotion,     // 首次使用药水
        FirstEnhance,    // 首次强化装备
        FirstRune,       // 首次镶嵌符文
        FirstPetCombat,  // 首次宠物战斗
        FirstMountRide,  // 首次骑乘坐骑
        FirstCombo,      // 首次连击
        FirstCounter,    // 首次反击
        FirstDodge,      // 首次闪避成功
        FirstBlock,      // 首次格挡
        FirstCrit,       // 首次暴击
        FirstSkillUse,   // 首次使用技能
        FirstTitle,      // 首次获得称号
        FirstAchievement // 首次获得成就
    }

    public enum TutorialTargetType {
        None,
        Key,              // 按键
        UIButton,         // UI按钮
        WorldObject,      // 世界物体
        NPC,              // NPC
        InventorySlot,    // 背包槽
        EquipmentSlot,    // 装备槽
        SkillSlot,        // 技能槽
        CraftingStation, // 合成台
        EnchantmentStation // 附魔台
    }

    public class TutorialDatabase {
        public static TutorialDatabase Instance { get; private set; }
        
        private List<TutorialStep> steps = new List<TutorialStep>();
        
        public TutorialDatabase() {
            Instance = this;
            InitializeTutorials();
        }
        
        private void InitializeTutorials() {
            // 游戏基础教程
            steps.Add(new TutorialStep {
                StepId = "welcome",
                Title = "欢迎来到 ClawRPG",
                Description = "恭喜你成为冒险者！按 WASD 或 方向键移动，点击鼠标攻击。",
                Trigger = TutorialTrigger.GameStart,
                TargetType = TutorialTargetType.None,
                Duration = 8f,
                CanSkip = true
            });

            steps.Add(new TutorialStep {
                StepId = "movement",
                Title = "移动控制",
                Description = "使用 WASD 或 方向键在世界中移动。尝试靠近敌人进行战斗！",
                Trigger = TutorialTrigger.GameStart,
                TargetType = TutorialTargetType.Key,
                TargetAction = "WASD",
                Duration = 0f,
                CanSkip = false
            });

            steps.Add(new TutorialStep {
                StepId = "attack",
                Title = "攻击",
                Description = "左键点击敌人进行普通攻击。右键点击进行重击！",
                Trigger = TutorialTrigger.GameStart,
                TargetType = TutorialTargetType.Key,
                TargetAction = "LeftClick",
                Duration = 0f,
                CanSkip = false
            });

            steps.Add(new TutorialStep {
                StepId = "dodge",
                Title = "闪避",
                Description = "按 Shift 键进行闪避，快速躲避敌人攻击！",
                Trigger = TutorialTrigger.FirstCombat,
                TargetType = TutorialTargetType.Key,
                TargetAction = "Shift",
                Duration = 0f,
                CanSkip = false
            });

            steps.Add(new TutorialStep {
                StepId = "block",
                Title = "格挡",
                Description = "按 Ctrl 键举起武器格挡，减少受到的伤害！",
                Trigger = TutorialTrigger.FirstCombat,
                TargetType = TutorialTargetType.Key,
                TargetAction = "Ctrl",
                Duration = 0f,
                CanSkip = false
            });

            // 背包与物品
            steps.Add(new TutorialStep {
                StepId = "inventory",
                Title = "背包系统",
                Description = "按 I 键打开背包，查看和管理你的物品。",
                Trigger = TutorialTrigger.Manual,
                TargetType = TutorialTargetType.Key,
                TargetAction = "I",
                Duration = 0f,
                CanSkip = false
            });

            steps.Add(new TutorialStep {
                StepId = "equipment",
                Title = "装备系统",
                Description = "在背包中点击物品可装备。装备更好的武器和防具提升战斗力！",
                Trigger = TutorialTrigger.Manual,
                TargetType = TutorialTargetType.UIButton,
                TargetAction = "Equip",
                Duration = 0f,
                CanSkip = false
            });

            // 合成系统
            steps.Add(new TutorialStep {
                StepId = "crafting",
                Title = "合成系统",
                Description = "按 C 键打开合成界面，将材料组合成强大的装备！",
                Trigger = TutorialTrigger.FirstCraft,
                TargetType = TutorialTargetType.Key,
                TargetAction = "C",
                Duration = 0f,
                CanSkip = false
            });

            steps.Add(new TutorialStep {
                StepId = "crafting_recipe",
                Title = "配方",
                Description = "选择配方，消耗材料进行合成。越高级的配方需要越多材料！",
                Trigger = TutorialTrigger.FirstCraft,
                TargetType = TutorialTargetType.UIButton,
                TargetAction = "Recipe",
                Duration = 0f,
                CanSkip = true
            });

            // 附魔系统
            steps.Add(new TutorialStep {
                StepId = "enchantment",
                Title = "附魔系统",
                Description = "按 E 键打开附魔界面，为装备附加强力魔法属性！",
                Trigger = TutorialTrigger.FirstEnchant,
                TargetType = TutorialTargetType.Key,
                TargetAction = "E",
                Duration = 0f,
                CanSkip = false
            });

            steps.Add(new TutorialStep {
                StepId = "enchantment_rune",
                Title = "符文",
                Description = "使用符文可以为装备增加额外属性。不同符文组合产生不同效果！",
                Trigger = TutorialTrigger.FirstEnchant,
                TargetType = TutorialTargetType.UIButton,
                TargetAction = "Rune",
                Duration = 0f,
                CanSkip = true
            });

            // 任务系统
            steps.Add(new TutorialStep {
                StepId = "quest",
                Title = "任务系统",
                Description = "按 Q 键打开任务界面，跟随任务指引完成目标获得奖励！",
                Trigger = TutorialTrigger.FirstQuest,
                TargetType = TutorialTargetType.Key,
                TargetAction = "Q",
                Duration = 0f,
                CanSkip = false
            });

            // 技能系统
            steps.Add(new TutorialStep {
                StepId = "skill_tree",
                Title = "技能树",
                Description = "按 K 键打开技能树，学习强大的职业技能！",
                Trigger = TutorialTrigger.SkillUnlocked,
                TargetType = TutorialTargetType.Key,
                TargetAction = "K",
                Duration = 0f,
                CanSkip = false
            });

            // 宠物系统
            steps.Add(new TutorialStep {
                StepId = "pet",
                Title = "宠物系统",
                Description = "按 P 键打开宠物界面，召唤宠物协助战斗！",
                Trigger = TutorialTrigger.FirstPet,
                TargetType = TutorialTargetType.Key,
                TargetAction = "P",
                Duration = 0f,
                CanSkip = false
            });

            // 坐骑系统
            steps.Add(new TutorialStep {
                StepId = "mount",
                Title = "坐骑系统",
                Description = "按 M 键打开坐骑界面，骑乘坐骑快速移动！",
                Trigger = TutorialTrigger.FirstMount,
                TargetType = TutorialTargetType.Key,
                TargetAction = "M",
                Duration = 0f,
                CanSkip = false
            });

            // Boss战斗
            steps.Add(new TutorialStep {
                StepId = "boss_warning",
                Title = "Boss预警",
                Description = "注意屏幕提示！Boss即将释放技能，及时闪避或格挡！",
                Trigger = TutorialTrigger.BossEncounter,
                TargetType = TutorialTargetType.None,
                Duration = 0f,
                CanSkip = false
            });

            steps.Add(new TutorialStep {
                StepId = "boss_enrage",
                Title = "狂暴模式",
                Description = "Boss血量低时会进入狂暴模式，伤害大幅提升！集中精神！",
                Trigger = TutorialTrigger.BossEncounter,
                TargetType = TutorialTargetType.None,
                Duration = 0f,
                CanSkip = true
            });

            // 反击系统
            steps.Add(new TutorialStep {
                StepId = "counter_attack",
                Title = "反击系统",
                Description = "完美格挡后可触发反击！按 Shift+C 打开反击界面释放强力反击！",
                Trigger = TutorialTrigger.FirstCombat,
                TargetType = TutorialTargetType.Key,
                TargetAction = "Shift+C",
                Duration = 0f,
                CanSkip = true
            });

            // 区域探索
            steps.Add(new TutorialStep {
                StepId = "region",
                Title = "区域探索",
                Description = "不同区域有不同怪物和资源。查看小地图了解周围环境！",
                Trigger = TutorialTrigger.RegionEnter,
                TargetType = TutorialTargetType.None,
                Duration = 5f,
                CanSkip = true
            });

            // 升级
            steps.Add(new TutorialStep {
                StepId = "level_up",
                Title = "升级了！",
                Description = "升级会获得属性点和技能点。合理分配提升战力！",
                Trigger = TutorialTrigger.LevelUp,
                TargetType = TutorialTargetType.None,
                Duration = 0f,
                CanSkip = false
            });

            // 快捷键汇总
            steps.Add(new TutorialStep {
                StepId = "hotkeys",
                Title = "快捷键汇总",
                Description = "I:背包 C:合成 E:附魔 Q:任务 K:技能 P:宠物 M:坐骑 ESC:菜单",
                Trigger = TutorialTrigger.Manual,
                TargetType = TutorialTargetType.Key,
                TargetAction = "Hotkeys",
                Duration = 0f,
                CanSkip = true
            });

            // 药水系统
            steps.Add(new TutorialStep {
                StepId = "potion",
                Title = "药水系统",
                Description = "按 H 键使用背包中的药水恢复生命值。合理使用药水是战斗的关键！",
                Trigger = TutorialTrigger.FirstPotion,
                TargetType = TutorialTargetType.Key,
                TargetAction = "H",
                Duration = 0f,
                CanSkip = false
            });

            // 强化系统
            steps.Add(new TutorialStep {
                StepId = "enhancement",
                Title = "强化系统",
                Description = "按 Ctrl+E 打开强化界面，消耗材料提升装备属性！",
                Trigger = TutorialTrigger.FirstEnhance,
                TargetType = TutorialTargetType.Key,
                TargetAction = "Ctrl+E",
                Duration = 0f,
                CanSkip = false
            });

            // 符文系统
            steps.Add(new TutorialStep {
                StepId = "rune",
                Title = "符文镶嵌",
                Description = "在装备上镶嵌符文可以获得额外属性加成。高品质符文带来更强效果！",
                Trigger = TutorialTrigger.FirstRune,
                TargetType = TutorialTargetType.UIButton,
                TargetAction = "Rune",
                Duration = 0f,
                CanSkip = true
            });

            // 宠物战斗
            steps.Add(new TutorialStep {
                StepId = "pet_combat",
                Title = "宠物战斗",
                Description = "宠物会在战斗中自动协助你作战。升级宠物提升其能力！",
                Trigger = TutorialTrigger.FirstPetCombat,
                TargetType = TutorialTargetType.None,
                Duration = 5f,
                CanSkip = true
            });

            // 骑乘坐骑
            steps.Add(new TutorialStep {
                StepId = "mount_ride",
                Title = "骑乘出行",
                Description = "骑乘坐骑可以大幅提升移动速度。某些坐骑还能在战斗中提供帮助！",
                Trigger = TutorialTrigger.FirstMountRide,
                TargetType = TutorialTargetType.Key,
                TargetAction = "M",
                Duration = 0f,
                CanSkip = true
            });

            // 连击系统
            steps.Add(new TutorialStep {
                StepId = "combo",
                Title = "连击系统",
                Description = "连续攻击敌人会积累连击数！高连击数获得额外伤害加成！",
                Trigger = TutorialTrigger.FirstCombo,
                TargetType = TutorialTargetType.None,
                Duration = 5f,
                CanSkip = true
            });

            // 暴击系统
            steps.Add(new TutorialStep {
                StepId = "critical",
                Title = "暴击机制",
                Description = "攻击时有几率触发暴击，造成双倍伤害！提升暴击率属性成为致命杀手！",
                Trigger = TutorialTrigger.FirstCrit,
                TargetType = TutorialTargetType.None,
                Duration = 5f,
                CanSkip = true
            });

            // 技能使用
            steps.Add(new TutorialStep {
                StepId = "skill_use",
                Title = "技能释放",
                Description = "点击技能栏或按数字键1-4释放技能。合理搭配技能形成强力连招！",
                Trigger = TutorialTrigger.FirstSkillUse,
                TargetType = TutorialTargetType.Key,
                TargetAction = "1-4",
                Duration = 0f,
                CanSkip = false
            });

            // 称号系统
            steps.Add(new TutorialStep {
                StepId = "title",
                Title = "称号系统",
                Description = "完成特定成就可获得炫酷称号！称号会显示在角色头顶彰显你的荣耀！",
                Trigger = TutorialTrigger.FirstTitle,
                TargetType = TutorialTargetType.None,
                Duration = 5f,
                CanSkip = true
            });

            // 成就系统
            steps.Add(new TutorialStep {
                StepId = "achievement",
                Title = "成就系统",
                Description = "完成各种挑战目标解锁成就！成就不仅有奖励还能展示你的实力！",
                Trigger = TutorialTrigger.FirstAchievement,
                TargetType = TutorialTargetType.Key,
                TargetAction = "L",
                Duration = 0f,
                CanSkip = false
            });

            // 快捷栏
            steps.Add(new TutorialStep {
                StepId = "quickslot",
                Title = "快捷栏",
                Description = "将物品或技能拖放到快捷栏，按数字键快速使用！战斗中使用更便捷！",
                Trigger = TutorialTrigger.Manual,
                TargetType = TutorialTargetType.None,
                Duration = 5f,
                CanSkip = true
            });
        }
        
        public List<TutorialStep> GetAllSteps() => steps;
        
        public TutorialStep GetStep(string stepId) {
            return steps.Find(s => s.StepId == stepId);
        }
        
        public List<TutorialStep> GetStepsByTrigger(TutorialTrigger trigger) {
            return steps.FindAll(s => s.Trigger == trigger && !s.IsCompleted);
        }
        
        public List<TutorialStep> GetIncompleteSteps() {
            return steps.FindAll(s => !s.IsCompleted);
        }
        
        /// <summary>
        /// 便捷方法：触发指定类型的教程
        /// </summary>
        public static void Trigger(TutorialTrigger trigger) {
            if (Instance != null) {
                var ui = TutorialUI.Instance;
                if (ui != null) {
                    ui.TriggerTutorial(trigger);
                }
            }
        }
        
        /// <summary>
        /// 便捷方法：通过ID开始教程
        /// </summary>
        public static void TriggerById(string stepId) {
            if (Instance != null) {
                var ui = TutorialUI.Instance;
                if (ui != null) {
                    ui.StartTutorialById(stepId);
                }
            }
        }
    }
}
