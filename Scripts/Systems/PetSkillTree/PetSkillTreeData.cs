using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Data structures for Pet Skill Tree System
    /// </summary>
    public class PetSkillTreeData
    {
        public enum SkillTreeType
        {
            Offensive,
            Defensive,
            Support,
            Special,
            Ultimate
        }

        public enum SkillNodeStatus
        {
            Locked,
            Available,
            Unlocked
        }

        [System.Serializable]
        public class PetSkillTree
        {
            public string PetId;
            public string PetType;
            public List<SkillNode> UnlockedNodes = new List<SkillNode>();
            public int TotalSkillPoints;
            public int UsedSkillPoints;
            public Dictionary<string, SkillNodeStatus> NodeStatuses = new Dictionary<string, SkillNodeStatus>();
        }

        [System.Serializable]
        public class SkillNode
        {
            public string NodeId;
            public string Name;
            public string Description;
            public SkillTreeType Type;
            public int Tier;
            public int Cost;
            public string IconName;
            public Dictionary<string, float> StatBonuses = new Dictionary<string, float>();
            public string SkillEffect;
            public List<string> Prerequisites = new List<string>();
            public bool IsUltimate;
        }

        [System.Serializable]
        public class PetSkillTreeSaveData
        {
            public Dictionary<string, PetSkillTree> PetSkillTrees = new Dictionary<string, PetSkillTree>();
            public int TotalSkillPointsEarned;
            public int TotalSkillPointsSpent;
        }
    }
}
