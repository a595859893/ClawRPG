using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 互动数据库
    /// </summary>
    public class InteractionDatabase
    {
        private static InteractionDatabase _instance;
        public static InteractionDatabase Instance => _instance ??= new InteractionDatabase();

        private List<InteractionActionData> _petActions;
        private List<InteractionActionData> _mountActions;

        public InteractionDatabase()
        {
            _petActions = new List<InteractionActionData>();
            _mountActions = new List<InteractionActionData>();
            InitializeActions();
        }

        private void InitializeActions()
        {
            // 宠物互动动作
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Feed, Name = "喂食", Description = "给宠物喂食，增加好感度和快乐度", AffectionGain = 15, HappinessGain = 20, EnergyCost = 5, Duration = 2f, MinLevel = 1, RequiresItem = true, RequiredItemId = "pet_food" });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Play, Name = "玩耍", Description = "和宠物一起玩耍，非常开心", AffectionGain = 20, HappinessGain = 25, EnergyCost = 8, Duration = 3f, MinLevel = 1, RequiresItem = false });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Brush, Name = "梳理", Description = "给宠物梳理毛发，增进感情", AffectionGain = 12, HappinessGain = 10, EnergyCost = 3, Duration = 1.5f, MinLevel = 1, RequiresItem = false });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Talk, Name = "对话", Description = "和宠物说说话，了解它的想法", AffectionGain = 8, HappinessGain = 5, EnergyCost = 2, Duration = 1f, MinLevel = 1, RequiresItem = false });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Pet, Name = "抚摸", Description = "轻轻抚摸宠物，表达关爱", AffectionGain = 10, HappinessGain = 8, EnergyCost = 2, Duration = 1f, MinLevel = 1, RequiresItem = false });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Train, Name = "训练", Description = "进行简单训练，提升亲密度", AffectionGain = 18, HappinessGain = 12, EnergyCost = 10, Duration = 4f, MinLevel = 5, RequiresItem = false });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Rest, Name = "休息", Description = "一起休息，恢复体力", AffectionGain = 5, HappinessGain = 15, EnergyCost = -10, Duration = 5f, MinLevel = 1, RequiresItem = false });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Explore, Name = "探索", Description = "一起探索新地方，增进默契", AffectionGain = 22, HappinessGain = 18, EnergyCost = 12, Duration = 5f, MinLevel = 10, RequiresItem = false });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Groom, Name = "美容", Description = "给宠物打扮一番，更加可爱", AffectionGain = 15, HappinessGain = 20, EnergyCost = 6, Duration = 3f, MinLevel = 15, RequiresItem = true, RequiredItemId = "grooming_kit" });
            _petActions.Add(new InteractionActionData { Action = InteractionAction.Massage, Name = "按摩", Description = "给宠物做按摩，放松身心", AffectionGain = 12, HappinessGain = 15, EnergyCost = 4, Duration = 2f, MinLevel = 20, RequiresItem = false });

            // 坐骑互动动作
            _mountActions.Add(new InteractionActionData { Action = InteractionAction.Feed, Name = "喂食", Description = "给坐骑喂食，增加好感度", AffectionGain = 12, HappinessGain = 15, EnergyCost = 5, Duration = 2f, MinLevel = 1, RequiresItem = true, RequiredItemId = "mount_food" });
            _mountActions.Add(new InteractionActionData { Action = InteractionAction.Brush, Name = "梳理", Description = "给坐骑梳理鬃毛，更加亲密", AffectionGain = 15, HappinessGain = 10, EnergyCost = 4, Duration = 2f, MinLevel = 1, RequiresItem = false });
            _mountActions.Add(new InteractionActionData { Action = InteractionAction.Train, Name = "训练", Description = "训练坐骑，提升默契", AffectionGain = 20, HappinessGain = 12, Duration = 5f, MinLevel = 5, RequiresItem = false });
            _mountActions.Add(new InteractionActionData { Action = InteractionAction.Rest, Name = "休息", Description = "一起休息，恢复体力", AffectionGain = 8, HappinessGain = 20, EnergyCost = -15, Duration = 6f, MinLevel = 1, RequiresItem = false });
            _mountActions.Add(new InteractionActionData { Action = InteractionAction.Explore, Name = "探索", Description = "一起探索新领域", AffectionGain = 25, HappinessGain = 20, EnergyCost = 15, Duration = 6f, MinLevel = 10, RequiresItem = false });
            _mountActions.Add(new InteractionActionData { Action = InteractionAction.Groom, Name = "美容", Description = "给坐骑打扮，更具风采", AffectionGain = 18, HappinessGain = 15, EnergyCost = 8, Duration = 3f, MinLevel = 15, RequiresItem = true, RequiredItemId = "mount_grooming_kit" });
        }

        public List<InteractionActionData> GetActionsForType(InteractionType type)
        {
            return type == InteractionType.Pet ? _petActions : _mountActions;
        }

        public InteractionActionData GetAction(InteractionType type, InteractionAction action)
        {
            var actions = type == InteractionType.Pet ? _petActions : _mountActions;
            foreach (var a in actions)
            {
                if (a.Action == action) return a;
            }
            return null;
        }

        public bool HasRequiredItem(InteractionType type, InteractionAction action)
        {
            var actionData = GetAction(type, action);
            return actionData?.RequiresItem ?? false;
        }

        public string GetRequiredItemId(InteractionType type, InteractionAction action)
        {
            var actionData = GetAction(type, action);
            return actionData?.RequiredItemId ?? "";
        }
    }
}
