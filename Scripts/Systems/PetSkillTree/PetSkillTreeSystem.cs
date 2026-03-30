using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

namespace ClawRPG.Scripts.Systems
{
    public class PetSkillTreeSystem : BaseSystem
    {
        private static PetSkillTreeSystem _instance;
        public static PetSkillTreeSystem Instance => _instance ??= new PetSkillTreeSystem();

        public System.Collections.Generic.Dictionary<string, PetSkillTreeData.PetSkillTree> PetSkillTrees = new System.Collections.Generic.Dictionary<string, PetSkillTreeData.PetSkillTree>();
        
        public int TotalSkillPointsEarned { get; private set; }
        public int TotalSkillPointsSpent { get; private set; }
        
        // Statistics
        public int TotalNodesUnlocked { get; private set; }
        public int TotalUltimatesUnlocked { get; private set; }
        
        public event Action<string, string> OnSkillUnlocked;
        public event Action<string, int> OnSkillPointsChanged;

        protected override void Initialize()
        {
            base.Initialize();
            _instance = this;
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

        public System.Collections.Generic.Dictionary<string, float> GetTotalBonuses(string petId)
        {
            var bonuses = new System.Collections.Generic.Dictionary<string, float>();
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

        public System.Collections.Generic.System.Collections.Generic.Dictionary<string, object> GetStatistics()
        {
            return new System.Collections.Generic.System.Collections.Generic.Dictionary<string, object>
            {
                { "total_pets", PetSkillTrees.Count },
                { "total_points_earned", TotalSkillPointsEarned },
                { "total_points_spent", TotalSkillPointsSpent },
                { "nodes_unlocked", TotalNodesUnlocked },
                { "ultimates_unlocked", TotalUltimatesUnlocked }
            };
        }

        public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
        {
            var data = new System.Collections.Generic.Dictionary<string, object>();
            
            // 宠物技能树数据
            var petTreesData = new System.Collections.Generic.Dictionary<string, object>();
            foreach (var kvp in PetSkillTrees)
            {
                var treeData = new System.Collections.Generic.Dictionary<string, object>();
                treeData["pet_id"] = kvp.Value.PetId;
                treeData["pet_type"] = kvp.Value.PetType;
                treeData["total_skill_points"] = kvp.Value.TotalSkillPoints;
                treeData["used_skill_points"] = kvp.Value.UsedSkillPoints;
                
                // 保存已解锁节点
                var unlockedNodeIds = new Array();
                foreach (var node in kvp.Value.UnlockedNodes)
                {
                    unlockedNodeIds.Add(node.NodeId);
                }
                treeData["unlocked_nodes"] = unlockedNodeIds;
                
                // 保存节点状态
                var nodeStatuses = new System.Collections.Generic.Dictionary<string, object>();
                foreach (var statusKvp in kvp.Value.NodeStatuses)
                {
                    nodeStatuses[statusKvp.Key] = (int)statusKvp.Value;
                }
                treeData["node_statuses"] = nodeStatuses;
                
                petTreesData[kvp.Key] = treeData;
            }
            data["pet_skill_trees"] = petTreesData;
            
            // 统计信息
            data["total_skill_points_earned"] = TotalSkillPointsEarned;
            data["total_skill_points_spent"] = TotalSkillPointsSpent;
            data["total_nodes_unlocked"] = TotalNodesUnlocked;
            data["total_ultimates_unlocked"] = TotalUltimatesUnlocked;
            
            return data;
        }

        public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 导入统计信息
            if (data.Contains("total_skill_points_earned"))
                TotalSkillPointsEarned = (int)data["total_skill_points_earned"];
            if (data.Contains("total_skill_points_spent"))
                TotalSkillPointsSpent = (int)data["total_skill_points_spent"];
            if (data.Contains("total_nodes_unlocked"))
                TotalNodesUnlocked = (int)data["total_nodes_unlocked"];
            if (data.Contains("total_ultimates_unlocked"))
                TotalUltimatesUnlocked = (int)data["total_ultimates_unlocked"];
            
            // 导入宠物技能树数据
            if (data.Contains("pet_skill_trees"))
            {
                var petTreesData = (Dictionary)data["pet_skill_trees"];
                foreach (string petId in petTreesData.Keys)
                {
                    var treeData = (Dictionary)petTreesData[petId];
                    var skillTree = new PetSkillTreeData.PetSkillTree
                    {
                        PetId = (string)treeData["pet_id"],
                        PetType = (string)treeData["pet_type"],
                        TotalSkillPoints = (int)treeData["total_skill_points"],
                        UsedSkillPoints = (int)treeData["used_skill_points"],
                        UnlockedNodes = new List<PetSkillTreeData.SkillNode>(),
                        NodeStatuses = new System.Collections.Generic.Dictionary<string, PetSkillTreeData.SkillNodeStatus>()
                    };
                    
                    // 恢复节点状态
                    if (treeData.Contains("node_statuses"))
                    {
                        var nodeStatuses = (Dictionary)treeData["node_statuses"];
                        foreach (string nodeId in nodeStatuses.Keys)
                        {
                            skillTree.NodeStatuses[nodeId] = (PetSkillTreeData.SkillNodeStatus)(int)nodeStatuses[nodeId];
                        }
                    }
                    
                    // 恢复已解锁节点
                    if (treeData.Contains("unlocked_nodes"))
                    {
                        var db = PetSkillTreeDatabase.Instance;
                        var unlockedNodeIds = (Array)treeData["unlocked_nodes"];
                        foreach (string nodeId in unlockedNodeIds)
                        {
                            foreach (PetSkillTreeData.SkillTreeType treeType in Enum.GetValues(typeof(PetSkillTreeData.SkillTreeType)))
                            {
                                var nodes = db.GetSkillTree(skillTree.PetType, treeType);
                                foreach (var node in nodes)
                                {
                                    if (node.NodeId == nodeId)
                                    {
                                        skillTree.UnlockedNodes.Add(node);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    
                    PetSkillTrees[petId] = skillTree;
                }
            }
        }
    }
}
