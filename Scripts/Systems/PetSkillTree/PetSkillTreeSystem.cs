using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    public class PetSkillTreeSystem
    {
        private static PetSkillTreeSystem _instance;
        public static PetSkillTreeSystem Instance => _instance ??= new PetSkillTreeSystem();

        public Dictionary<string, PetSkillTreeData.PetSkillTree> PetSkillTrees = new Dictionary<string, PetSkillTreeData.PetSkillTree>();
        
        public int TotalSkillPointsEarned { get; private set; }
        public int TotalSkillPointsSpent { get; private set; }
        
        // Statistics
        public int TotalNodesUnlocked { get; private set; }
        public int TotalUltimatesUnlocked { get; private set; }
        
        public event Action<string, string> OnSkillUnlocked;
        public event Action<string, int> OnSkillPointsChanged;

        public PetSkillTreeSystem()
        {
            LoadData();
        }

        public void InitializePetSkillTree(string petId, string petType)
        {
            if (!PetSkillTrees.ContainsKey(petId))
            {
                var skillTree = new PetSkillTreeData.PetSkillTree
                {
                    PetId = petId,
                    PetType = petType,
                    TotalSkillPoints = 5,
                    UsedSkillPoints = 0,
                    UnlockedNodes = new List<PetSkillTreeData.SkillNode>()
                };
                
                var db = PetSkillTreeDatabase.Instance;
                foreach (PetSkillTreeData.SkillTreeType treeType in Enum.GetValues(typeof(PetSkillTreeData.SkillTreeType)))
                {
                    var nodes = db.GetSkillTree(petType, treeType);
                    foreach (var node in nodes)
                    {
                        if (node.Tier == 1 && node.Prerequisites.Count == 0)
                            skillTree.NodeStatuses[node.NodeId] = PetSkillTreeData.SkillNodeStatus.Available;
                        else
                            skillTree.NodeStatuses[node.NodeId] = PetSkillTreeData.SkillNodeStatus.Locked;
                    }
                }
                
                PetSkillTrees[petId] = skillTree;
                TotalSkillPointsEarned += 5;
                SaveData();
            }
        }

        public bool CanUnlockSkill(string petId, string nodeId)
        {
            if (!PetSkillTrees.TryGetValue(petId, out var skillTree))
                return false;
            
            var db = PetSkillTreeDatabase.Instance;
            foreach (var treeType in Enum.GetValues(typeof(PetSkillTreeData.SkillTreeType)))
            {
                var nodes = db.GetSkillTree(skillTree.PetType, (PetSkillTreeData.SkillTreeType)treeType);
                foreach (var node in nodes)
                {
                    if (node.NodeId == nodeId)
                    {
                        if (skillTree.NodeStatuses.GetValueOrDefault(nodeId) != PetSkillTreeData.SkillNodeStatus.Available)
                            return false;
                        if (skillTree.UsedSkillPoints + node.Cost > skillTree.TotalSkillPoints)
                            return false;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool UnlockSkill(string petId, string nodeId)
        {
            if (!CanUnlockSkill(petId, nodeId))
                return false;
            
            var skillTree = PetSkillTrees[petId];
            var db = PetSkillTreeDatabase.Instance;
            
            PetSkillTreeData.SkillNode unlockedNode = null;
            foreach (var treeType in Enum.GetValues(typeof(PetSkillTreeData.SkillTreeType)))
            {
                var nodes = db.GetSkillTree(skillTree.PetType, (PetSkillTreeData.SkillTreeType)treeType);
                foreach (var node in nodes)
                {
                    if (node.NodeId == nodeId)
                    {
                        unlockedNode = node;
                        break;
                    }
                }
                if (unlockedNode != null) break;
            }
            
            if (unlockedNode == null) return false;
            
            skillTree.UnlockedNodes.Add(unlockedNode);
            skillTree.UsedSkillPoints += unlockedNode.Cost;
            skillTree.NodeStatuses[nodeId] = PetSkillTreeData.SkillNodeStatus.Unlocked;
            
            TotalSkillPointsSpent += unlockedNode.Cost;
            TotalNodesUnlocked++;
            if (unlockedNode.IsUltimate) TotalUltimatesUnlocked++;
            
            // Unlock next tier nodes
            foreach (var treeType in Enum.GetValues(typeof(PetSkillTreeData.SkillTreeType)))
            {
                var nodes = db.GetSkillTree(skillTree.PetType, (PetSkillTreeData.SkillTreeType)treeType);
                foreach (var node in nodes)
                {
                    if (node.Prerequisites.Contains(nodeId) && skillTree.NodeStatuses[node.NodeId] == PetSkillTreeData.SkillNodeStatus.Locked)
                    {
                        bool allPrereqsMet = true;
                        foreach (var prereq in node.Prerequisites)
                        {
                            if (skillTree.NodeStatuses.GetValueOrDefault(prereq) != PetSkillTreeData.SkillNodeStatus.Unlocked)
                            {
                                allPrereqsMet = false;
                                break;
                            }
                        }
                        if (allPrereqsMet)
                            skillTree.NodeStatuses[node.NodeId] = PetSkillTreeData.SkillNodeStatus.Available;
                    }
                }
            }
            
            OnSkillUnlocked?.Invoke(petId, nodeId);
            OnSkillPointsChanged?.Invoke(petId, GetAvailableSkillPoints(petId));
            SaveData();
            return true;
        }

        public int GetAvailableSkillPoints(string petId)
        {
            if (!PetSkillTrees.TryGetValue(petId, out var skillTree))
                return 0;
            return skillTree.TotalSkillPoints - skillTree.UsedSkillPoints;
        }

        public PetSkillTreeData.SkillNodeStatus GetNodeStatus(string petId, string nodeId)
        {
            if (!PetSkillTrees.TryGetValue(petId, out var skillTree))
                return PetSkillTreeData.SkillNodeStatus.Locked;
            return skillTree.NodeStatuses.GetValueOrDefault(nodeId, PetSkillTreeData.SkillNodeStatus.Locked);
        }

        public Dictionary<string, float> GetTotalBonuses(string petId)
        {
            var bonuses = new Dictionary<string, float>();
            if (!PetSkillTrees.TryGetValue(petId, out var skillTree))
                return bonuses;
            
            foreach (var node in skillTree.UnlockedNodes)
            {
                foreach (var bonus in node.StatBonuses)
                {
                    if (bonuses.ContainsKey(bonus.Key))
                        bonuses[bonus.Key] += bonus.Value;
                    else
                        bonuses[bonus.Key] = bonus.Value;
                }
            }
            return bonuses;
        }

        public void AddSkillPoints(string petId, int points)
        {
            if (PetSkillTrees.TryGetValue(petId, out var skillTree))
            {
                skillTree.TotalSkillPoints += points;
                TotalSkillPointsEarned += points;
                OnSkillPointsChanged?.Invoke(petId, GetAvailableSkillPoints(petId));
                SaveData();
            }
        }

        public void SaveData()
        {
            // Save to file system
            var saveDir = ProjectSettings.GetSetting("application/config/game_save_path", "user://saves").ToString();
            var dir = new Godot.Directory();
            if (!dir.DirExists(saveDir))
                dir.MakeDirRecursive(saveDir);
            
            var saveFile = saveDir + "/pet_skill_tree.save";
            // Simplified save - in production would use JSON
        }

        public void LoadData()
        {
            // Load from file system
        }

        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "total_pets", PetSkillTrees.Count },
                { "total_points_earned", TotalSkillPointsEarned },
                { "total_points_spent", TotalSkillPointsSpent },
                { "nodes_unlocked", TotalNodesUnlocked },
                { "ultimates_unlocked", TotalUltimatesUnlocked }
            };
        }
    }
}
