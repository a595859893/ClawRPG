using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 互动系统管理器
    /// </summary>
    public partial class CompanionInteractionSystem : BaseSystem
    {
        public static CompanionInteractionSystem Instance { get; private set; }

        public delegate void InteractionStartedEventHandler(string entityId, InteractionType entityType, InteractionAction action);
        public delegate void InteractionCompletedEventHandler(string entityId, InteractionType entityType, InteractionAction action, int affectionGain, int happinessGain);
        public delegate void InteractionFailedEventHandler(string entityId, InteractionType entityType, InteractionAction action, string reason);

        private Dictionary<string, InteractionInstance> _activeInteractions;
        private PlayerInteractionData _playerData;
        private float _processTimer;

        public PlayerInteractionData PlayerData => _playerData;

        public override void _Ready()
        {
            Instance = this;
            _activeInteractions = new Dictionary<string, InteractionInstance>();
            _playerData = new PlayerInteractionData();
            _processTimer = 0f;
        }

        public override void _Process(double delta)
        {
            _processTimer += delta;
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

        public bool StartInteraction(string entityId, InteractionType entityType, InteractionAction action)
        {
            if (_activeInteractions.ContainsKey(entityId))
            {
                EmitSignal(SignalName.InteractionFailed, entityId, entityType, action, "该实体正在进行其他互动");
                return false;
            }
            var actionData = InteractionDatabase.Instance.GetAction(entityType, action);
            if (actionData == null)
            {
                EmitSignal(SignalName.InteractionFailed, entityId, entityType, action, "无效的互动动作");
                return false;
            }
            int entityLevel = GetEntityLevel(entityId, entityType);
            if (entityLevel < actionData.MinLevel)
            {
                EmitSignal(SignalName.InteractionFailed, entityId, entityType, action, $"需要实体等级 {actionData.MinLevel}");
                return false;
            }
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
            UpdateStatistics(entityId, action, actionData.AffectionGain, actionData.HappinessGain);
            EmitSignal(SignalName.InteractionStarted, entityId, entityType, action);
            return true;
        }

        private void CompleteInteraction(string entityId)
        {
            if (!_activeInteractions.TryGetValue(entityId, out var instance)) return;
            var actionData = InteractionDatabase.Instance.GetAction(instance.EntityType, instance.Action);
            if (actionData == null) return;
            ApplyInteractionRewards(entityId, instance.EntityType, actionData.AffectionGain, actionData.HappinessGain);
            EmitSignal(SignalName.InteractionCompleted, entityId, instance.EntityType, instance.Action, actionData.AffectionGain, actionData.HappinessGain);
        }

        private void ApplyInteractionRewards(string entityId, InteractionType entityType, int affectionGain, int happinessGain)
        {
            _playerData.TotalAffectionGained += affectionGain;
            _playerData.TotalHappinessGained += happinessGain;
        }

        private int GetEntityLevel(string entityId, InteractionType entityType) => 1;
        private bool HasRequiredItem(string itemId) => true;
        private void UseRequiredItem(string itemId) { }

        private void UpdateStatistics(string entityId, InteractionAction action, int affectionGain, int happinessGain)
        {
            _playerData.TotalInteractions++;
            string actionKey = action.ToString();
            if (_playerData.ActionCounts.ContainsKey(actionKey))
                _playerData.ActionCounts[actionKey]++;
            else
                _playerData.ActionCounts[actionKey] = 1;
            if (_playerData.EntityInteractions.ContainsKey(entityId))
                _playerData.EntityInteractions[entityId]++;
            else
                _playerData.EntityInteractions[entityId] = 1;
            if (_playerData.EntityInteractions[entityId] > _playerData.FavoriteEntityCount)
            {
                _playerData.FavoriteEntityCount = _playerData.EntityInteractions[entityId];
                _playerData.FavoriteEntityId = entityId;
            }
        }

        public void CancelInteraction(string entityId)
        {
            if (_activeInteractions.ContainsKey(entityId))
            {
                _activeInteractions.Remove(entityId);
            }
        }

        public float GetInteractionProgress(string entityId)
        {
            if (!_activeInteractions.TryGetValue(entityId, out var instance)) return 0f;
            var elapsed = Time.GetTicksMsec() / 1000f - instance.StartTime;
            return Mathf.Clamp(elapsed / instance.Duration, 0f, 1f);
        }

        public bool HasActiveInteraction(string entityId)
        {
            return _activeInteractions.ContainsKey(entityId);
        }

        public List<InteractionActionData> GetAvailableActions(InteractionType type)
        {
            return InteractionDatabase.Instance.GetActionsForType(type);
        }

        public PlayerInteractionData GetStatistics() => _playerData;

        public override Dictionary<string, object> ExportSaveData()
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

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            _playerData.TotalInteractions = (int)data.GetValueOrDefault("totalInteractions", 0);
            _playerData.TotalAffectionGained = (int)data.GetValueOrDefault("totalAffectionGained", 0);
            _playerData.TotalHappinessGained = (int)data.GetValueOrDefault("totalHappinessGained", 0);
            _playerData.FavoriteEntityId = (string)data.GetValueOrDefault("favoriteEntityId", "");
            if (data.ContainsKey("actionCounts"))
            {
                var actionCounts = (Dictionary)data["actionCounts"];
                _playerData.ActionCounts = new Dictionary<string, int>();
                foreach (var kvp in actionCounts)
                    _playerData.ActionCounts[kvp.Key] = Convert.ToInt32(kvp.Value);
            }
            if (data.ContainsKey("entityInteractions"))
            {
                var entityInteractions = (Dictionary)data["entityInteractions"];
                _playerData.EntityInteractions = new Dictionary<string, int>();
                foreach (var kvp in entityInteractions)
                    _playerData.EntityInteractions[kvp.Key] = Convert.ToInt32(kvp.Value);
            }
        }

        /// <summary>旧的序列化方法（保留兼容性）</summary>
        public Dictionary<string, object> GetSaveData() => ExportSaveData();

        /// <summary>旧的反序列化方法（保留兼容性）</summary>
        public void LoadSaveData(Dictionary<string, object> data) => ImportSaveData(new Dictionary<string, object>(data));
    }
}
