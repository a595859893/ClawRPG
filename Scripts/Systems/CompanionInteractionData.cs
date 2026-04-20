using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 互动数据
    /// </summary>
    public class InteractionActionData
    {
        public InteractionAction Action { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AffectionGain { get; set; }
        public int HappinessGain { get; set; }
        public int EnergyCost { get; set; }
        public float Duration { get; set; }
        public int MinLevel { get; set; }
        public bool RequiresItem { get; set; }
        public string RequiredItemId { get; set; }

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
        public string EntityId { get; set; }
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
        public Dictionary<string, int> ActionCounts { get; set; }
        public Dictionary<string, int> EntityInteractions { get; set; }
        public int TotalAffectionGained { get; set; }
        public int TotalHappinessGained { get; set; }
        public int FavoriteEntityCount { get; set; }
        public string FavoriteEntityId { get; set; }

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
}
