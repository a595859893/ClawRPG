using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 互动类型枚举
    /// </summary>
    public enum InteractionType
    {
        Pet,      // 宠物互动
        Mount     // 坐骑互动
    }

    /// <summary>
    /// 互动动作类型
    /// </summary>
    public enum InteractionAction
    {
        Feed,        // 喂食
        Play,        // 玩耍
        Brush,       // 梳理
        Talk,        // 对话
        Pet,         // 抚摸
        Train,       // 训练
        Rest,        // 休息
        Explore,     // 探索
        Groom,       // 美容
        Massage      // 按摩
    }

    /// <summary>
    /// 互动数据
    /// </summary>
    public class InteractionActionData
    {
        public InteractionAction Action { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AffectionGain { get; set; }      // 好感度增加
        public int HappinessGain { get; set; }       // 快乐度增加
        public int EnergyCost { get; set; }         // 精力消耗
        public float Duration { get; set; }         // 持续时间（秒）
        public int MinLevel { get; set; }           // 最低等级要求
        public bool RequiresItem { get; set; }      // 是否需要道具
        public string RequiredItemId { get; set; }  // 所需道具ID

        public InteractionActionData()
        {
            Name = "";
            Description = "";
            AffectionGain = 0;
            HappinessGain = 0;
            EnergyCost = 0;
            Duration = 1f;
            MinLevel = 1;
            RequiresItem = false;
            RequiredItemId = "";
        }
    }

    /// <summary>
    /// 互动实例数据
    /// </summary>
    public class InteractionInstance
    {
        public string EntityId { get; set; }        // 实体ID（宠物/坐骑ID）
        public InteractionType EntityType { get; set; }
        public InteractionAction Action { get; set; }
        public float StartTime { get; set; }
        public float Duration { get; set; }
        public bool Completed { get; set; }

        public InteractionInstance()
        {
            EntityId = "";
            Action = InteractionAction.Pet;
            StartTime = 0f;
            Duration = 1f;
            Completed = false;
        }
    }

    /// <summary>
    /// 玩家互动数据
    /// </summary>
    public class PlayerInteractionData
    {
        public int TotalInteractions { get; set; }
        public Dictionary<string, int> ActionCounts { get; set; }  // 每个动作的次数
        public Dictionary<string, int> EntityInteractions { get; set; }  // 每个实体的互动次数
        public int TotalAffectionGained { get; set; }
        public int TotalHappinessGained { get; set; }
        public int FavoriteEntityCount { get; set; }  // 最喜欢的实体互动次数
        public string FavoriteEntityId { get; set; }  // 最喜欢的实体ID

        public PlayerInteractionData()
        {
            TotalInteractions = 0;
            ActionCounts = new Dictionary<string, int>();
            EntityInteractions = new Dictionary<string, int>();
            TotalAffectionGained = 0;
            TotalHappinessGained = 0;
            FavoriteEntityCount = 0;
            FavoriteEntityId = "";
        }
    }

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
            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Feed,
                Name = "喂食",
                Description = "给宠物喂食，增加好感度和快乐度",
                AffectionGain = 15,
                HappinessGain = 20,
                EnergyCost = 5,
                Duration = 2f,
                MinLevel = 1,
                RequiresItem = true,
                RequiredItemId = "pet_food"
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Play,
                Name = "玩耍",
                Description = "和宠物一起玩耍，非常开心",
                AffectionGain = 20,
                HappinessGain = 25,
                EnergyCost = 8,
                Duration = 3f,
                MinLevel = 1,
                RequiresItem = false
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Brush,
                Name = "梳理",
                Description = "给宠物梳理毛发，增进感情",
                AffectionGain = 12,
                HappinessGain = 10,
                EnergyCost = 3,
                Duration = 1.5f,
                MinLevel = 1,
                RequiresItem = false
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Talk,
                Name = "对话",
                Description = "和宠物说说话，了解它的想法",
                AffectionGain = 8,
                HappinessGain = 5,
                EnergyCost = 2,
                Duration = 1f,
                MinLevel = 1,
                RequiresItem = false
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Pet,
                Name = "抚摸",
                Description = "轻轻抚摸宠物，表达关爱",
                AffectionGain = 10,
                HappinessGain = 8,
                EnergyCost = 2,
                Duration = 1f,
                MinLevel = 1,
                RequiresItem = false
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Train,
                Name = "训练",
                Description = "进行简单训练，提升亲密度",
                AffectionGain = 18,
                HappinessGain = 12,
                EnergyCost = 10,
                Duration = 4f,
                MinLevel = 5,
                RequiresItem = false
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Rest,
                Name = "休息",
                Description = "一起休息，恢复体力",
                AffectionGain = 5,
                HappinessGain = 15,
                EnergyCost = -10,  // 恢复精力
                Duration = 5f,
                MinLevel = 1,
                RequiresItem = false
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Explore,
                Name = "探索",
                Description = "一起探索新地方，增进默契",
                AffectionGain = 22,
                HappinessGain = 18,
                EnergyCost = 12,
                Duration = 5f,
                MinLevel = 10,
                RequiresItem = false
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Groom,
                Name = "美容",
                Description = "给宠物打扮一番，更加可爱",
                AffectionGain = 15,
                HappinessGain = 20,
                EnergyCost = 6,
                Duration = 3f,
                MinLevel = 15,
                RequiresItem = true,
                RequiredItemId = "grooming_kit"
            });

            _petActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Massage,
                Name = "按摩",
                Description = "给宠物做按摩，放松身心",
                AffectionGain = 12,
                HappinessGain = 15,
                EnergyCost = 4,
                Duration = 2f,
                MinLevel = 20,
                RequiresItem = false
            });

            // 坐骑互动动作
            _mountActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Feed,
                Name = "喂食",
                Description = "给坐骑喂食，增加好感度",
                AffectionGain = 12,
                HappinessGain = 15,
                EnergyCost = 5,
                Duration = 2f,
                MinLevel = 1,
                RequiresItem = true,
                RequiredItemId = "mount_food"
            });

            _mountActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Brush,
                Name = "梳理",
                Description = "给坐骑梳理鬃毛，更加亲密",
                AffectionGain = 15,
                HappinessGain = 10,
                EnergyCost = 4,
                Duration = 2f,
                MinLevel = 1,
                RequiresItem = false
            });

            _mountActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Train,
                Name = "训练",
                Description = "训练坐骑，提升默契",
                AffectionGain = 20,
                HappinessGain = 12,
                Duration = 5f,
                MinLevel = 5,
                RequiresItem = false
            });

            _mountActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Rest,
                Name = "休息",
                Description = "一起休息，恢复体力",
                AffectionGain = 8,
                HappinessGain = 20,
                EnergyCost = -15,
                Duration = 6f,
                MinLevel = 1,
                RequiresItem = false
            });

            _mountActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Explore,
                Name = "探索",
                Description = "一起探索新领域",
                AffectionGain = 25,
                HappinessGain = 20,
                EnergyCost = 15,
                Duration = 6f,
                MinLevel = 10,
                RequiresItem = false
            });

            _mountActions.Add(new InteractionActionData
            {
                Action = InteractionAction.Groom,
                Name = "美容",
                Description = "给坐骑打扮，更具风采",
                AffectionGain = 18,
                HappinessGain = 15,
                EnergyCost = 8,
                Duration = 3f,
                MinLevel = 15,
                RequiresItem = true,
                RequiredItemId = "mount_grooming_kit"
            });
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

    /// <summary>
    /// 互动系统管理器
    /// </summary>
    public partial class CompanionInteractionSystem : Node
    {
        private static CompanionInteractionSystem _instance;
        public static CompanionInteractionSystem Instance => _instance;

        [Signal]
        public delegate void InteractionStartedEventHandler(string entityId, InteractionType entityType, InteractionAction action);

        [Signal]
        public delegate void InteractionCompletedEventHandler(string entityId, InteractionType entityType, InteractionAction action, int affectionGain, int happinessGain);

        [Signal]
        public delegate void InteractionFailedEventHandler(string entityId, InteractionType entityType, InteractionAction action, string reason);

        private Dictionary<string, InteractionInstance> _activeInteractions;
        private PlayerInteractionData _playerData;
        private float _processTimer;

        public PlayerInteractionData PlayerData => _playerData;

        public override void _Ready()
        {
            _instance = this;
            _activeInteractions = new Dictionary<string, InteractionInstance>();
            _playerData = new PlayerInteractionData();
            _processTimer = 0f;
        }

        public override void _Process(float delta)
        {
            _processTimer += delta;

            // 每0.1秒检查一次
            if (_processTimer >= 0.1f)
            {
                CheckActiveInteractions(_processTimer);
                _processTimer = 0f;
            }
        }

        private void CheckActiveInteractions(float delta)
        {
            List<string> completedKeys = new List<string>();

            foreach (var kvp in _activeInteractions)
            {
                var instance = kvp.Value;
                if (instance.Completed) continue;

                var elapsed = Time.GetTicksMsec() / 1000f - instance.StartTime;
                if (elapsed >= instance.Duration)
                {
                    CompleteInteraction(kvp.Key);
                    completedKeys.Add(kvp.Key);
                }
            }

            foreach (var key in completedKeys)
            {
                _activeInteractions.Remove(key);
            }
        }

        /// <summary>
        /// 开始互动
        /// </summary>
        public bool StartInteraction(string entityId, InteractionType entityType, InteractionAction action)
        {
            // 检查是否已有活跃互动
            if (_activeInteractions.ContainsKey(entityId))
            {
                EmitSignal(SignalName.InteractionFailed, entityId, entityType, action, "该实体正在进行其他互动");
                return false;
            }

            // 获取动作数据
            var actionData = InteractionDatabase.Instance.GetAction(entityType, action);
            if (actionData == null)
            {
                EmitSignal(SignalName.InteractionFailed, entityId, entityType, action, "无效的互动动作");
                return false;
            }

            // 检查等级要求
            int entityLevel = GetEntityLevel(entityId, entityType);
            if (entityLevel < actionData.MinLevel)
            {
                EmitSignal(SignalName.InteractionFailed, entityId, entityType, action, $"需要实体等级 {actionData.MinLevel}");
                return false;
            }

            // 检查道具需求
            if (actionData.RequiresItem)
            {
                var itemId = actionData.RequiredItemId;
                if (!HasRequiredItem(itemId))
                {
                    EmitSignal(SignalName.InteractionFailed, entityId, entityType, action, $"需要物品: {itemId}");
                    return false;
                }
                UseRequiredItem(itemId);
            }

            // 创建互动实例
            var instance = new InteractionInstance
            {
                EntityId = entityId,
                EntityType = entityType,
                Action = action,
                StartTime = Time.GetTicksMsec() / 1000f,
                Duration = actionData.Duration,
                Completed = false
            };

            _activeInteractions[entityId] = instance;

            // 更新统计
            UpdateStatistics(entityId, action, actionData.AffectionGain, actionData.HappinessGain);

            EmitSignal(SignalName.InteractionStarted, entityId, entityType, action);
            return true;
        }

        private void CompleteInteraction(string entityId)
        {
            if (!_activeInteractions.TryGetValue(entityId, out var instance)) return;

            var actionData = InteractionDatabase.Instance.GetAction(instance.EntityType, instance.Action);
            if (actionData == null) return;

            // 应用好感度和快乐度增益
            ApplyInteractionRewards(entityId, instance.EntityType, actionData.AffectionGain, actionData.HappinessGain);

            EmitSignal(SignalName.InteractionCompleted, entityId, instance.EntityType, instance.Action, actionData.AffectionGain, actionData.HappinessGain);
        }

        private void ApplyInteractionRewards(string entityId, InteractionType entityType, int affectionGain, int happinessGain)
        {
            // 这里可以与宠物/坐骑系统集成，应用实际的属性增益
            // 暂时只更新统计数据
            _playerData.TotalAffectionGained += affectionGain;
            _playerData.TotalHappinessGained += happinessGain;
        }

        private int GetEntityLevel(string entityId, InteractionType entityType)
        {
            // 从宠物/坐骑系统获取等级
            // 暂时返回1
            return 1;
        }

        private bool HasRequiredItem(string itemId)
        {
            // 检查背包是否有道具
            // 暂时假设总是有（实际应该检查背包）
            return true;
        }

        private void UseRequiredItem(string itemId)
        {
            // 使用道具
            // 暂时不实际扣除
        }

        private void UpdateStatistics(string entityId, InteractionAction action, int affectionGain, int happinessGain)
        {
            _playerData.TotalInteractions++;

            // 更新动作统计
            string actionKey = action.ToString();
            if (_playerData.ActionCounts.ContainsKey(actionKey))
                _playerData.ActionCounts[actionKey]++;
            else
                _playerData.ActionCounts[actionKey] = 1;

            // 更新实体互动统计
            if (_playerData.EntityInteractions.ContainsKey(entityId))
                _playerData.EntityInteractions[entityId]++;
            else
                _playerData.EntityInteractions[entityId] = 1;

            // 更新最喜爱的实体
            if (_playerData.EntityInteractions[entityId] > _playerData.FavoriteEntityCount)
            {
                _playerData.FavoriteEntityCount = _playerData.EntityInteractions[entityId];
                _playerData.FavoriteEntityId = entityId;
            }
        }

        /// <summary>
        /// 取消互动
        /// </summary>
        public void CancelInteraction(string entityId)
        {
            if (_activeInteractions.ContainsKey(entityId))
            {
                _activeInteractions.Remove(entityId);
            }
        }

        /// <summary>
        /// 获取互动进度
        /// </summary>
        public float GetInteractionProgress(string entityId)
        {
            if (!_activeInteractions.TryGetValue(entityId, out var instance)) return 0f;

            var elapsed = Time.GetTicksMsec() / 1000f - instance.StartTime;
            return Mathf.Clamp(elapsed / instance.Duration, 0f, 1f);
        }

        /// <summary>
        /// 是否有活跃互动
        /// </summary>
        public bool HasActiveInteraction(string entityId)
        {
            return _activeInteractions.ContainsKey(entityId);
        }

        /// <summary>
        /// 获取可用的互动动作列表
        /// </summary>
        public List<InteractionActionData> GetAvailableActions(InteractionType type)
        {
            return InteractionDatabase.Instance.GetActionsForType(type);
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public PlayerInteractionData GetStatistics()
        {
            return _playerData;
        }

        /// <summary>
        /// 序列化数据
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            return new Dictionary<string, object>
            {
                { "totalInteractions", _playerData.TotalInteractions },
                { "totalAffectionGained", _playerData.TotalAffectionGained },
                { "totalHappinessGained", _playerData.TotalHappinessGained },
                { "actionCounts", _playerData.ActionCounts },
                { "entityInteractions", _playerData.EntityInteractions },
                { "favoriteEntityId", _playerData.FavoriteEntityId }
            };
        }

        /// <summary>
        /// 反序列化数据
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            _playerData.TotalInteractions = (int)data.GetValueOrDefault("totalInteractions", 0);
            _playerData.TotalAffectionGained = (int)data.GetValueOrDefault("totalAffectionGained", 0);
            _playerData.TotalHappinessGained = (int)data.GetValueOrDefault("totalHappinessGained", 0);
            _playerData.FavoriteEntityId = (string)data.GetValueOrDefault("favoriteEntityId", "");

            // 恢复字典数据
            if (data.ContainsKey("actionCounts"))
            {
                var actionCounts = (Dictionary<string, object>)data["actionCounts"];
                _playerData.ActionCounts = new Dictionary<string, int>();
                foreach (var kvp in actionCounts)
                {
                    _playerData.ActionCounts[kvp.Key] = Convert.ToInt32(kvp.Value);
                }
            }

            if (data.ContainsKey("entityInteractions"))
            {
                var entityInteractions = (Dictionary<string, object>)data["entityInteractions"];
                _playerData.EntityInteractions = new Dictionary<string, int>();
                foreach (var kvp in entityInteractions)
                {
                    _playerData.EntityInteractions[kvp.Key] = Convert.ToInt32(kvp.Value);
                }
            }
        }
    }
}
