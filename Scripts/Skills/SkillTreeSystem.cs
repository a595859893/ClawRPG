using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Scripts.Skills
{
    /// <summary>
    /// 技能树系统 - 负责技能树结构管理、节点解锁等
    /// </summary>
    public partial class SkillTreeSystem : BaseSystem
    {
        private static SkillTreeSystem _instance;
        public static SkillTreeSystem Instance => _instance;
        
        // 技能树存储: playerId -> treeId -> SkillTree
        private Dictionary<string, Dictionary<string, SkillTree>> _skillTrees = new Dictionary<string, Dictionary<string, SkillTree>>();
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "SkillTree";
        
        #region Tree Management
        
        /// <summary>
        /// 获取技能树
        /// </summary>
        public SkillTree GetTree(string playerId, string treeId)
        {
            if (!_skillTrees.ContainsKey(playerId))
                return null;
            
            return _skillTrees[playerId].ContainsKey(treeId) ? _skillTrees[playerId][treeId] : null;
        }
        
        /// <summary>
        /// 获取或创建技能树
        /// </summary>
        public SkillTree GetOrCreateTree(string playerId, string treeId)
        {
            if (!_skillTrees.ContainsKey(playerId))
            {
                _skillTrees[playerId] = new Dictionary<string, SkillTree>();
            }
            
            if (!_skillTrees[playerId].ContainsKey(treeId))
            {
                _skillTrees[playerId][treeId] = new SkillTree
                {
                    TreeId = treeId,
                    UnlockedNodes = new List<string>(),
                    AvailableNodes = new List<string>()
                };
            }
            
            return _skillTrees[playerId][treeId];
        }
        
        /// <summary>
        /// 解锁技能节点
        /// </summary>
        public bool UnlockNode(string playerId, string treeId, string nodeId)
        {
            var tree = GetTree(playerId, treeId);
            if (tree == null)
                return false;
            
            // Check if node can be unlocked
            if (!CanUnlockNode(playerId, treeId, nodeId))
                return false;
            
            if (!tree.UnlockedNodes.Contains(nodeId))
            {
                tree.UnlockedNodes.Add(nodeId);
            }
            
            // Update available nodes
            UpdateAvailableNodes(playerId, treeId);
            
            return true;
        }
        
        /// <summary>
        /// 检查是否可以解锁节点
        /// </summary>
        public bool CanUnlockNode(string playerId, string treeId, string nodeId)
        {
            var tree = GetTree(playerId, treeId);
            if (tree == null)
                return false;
            
            // Already unlocked
            if (tree.UnlockedNodes.Contains(nodeId))
                return false;
            
            // Check if node is available
            if (!tree.AvailableNodes.Contains(nodeId))
                return false;
            
            return true;
        }
        
        /// <summary>
        /// 获取已解锁节点
        /// </summary>
        public List<string> GetUnlockedNodes(string playerId, string treeId)
        {
            var tree = GetTree(playerId, treeId);
            return tree != null ? new List<string>(tree.UnlockedNodes) : new List<string>();
        }
        
        /// <summary>
        /// 获取可用节点
        /// </summary>
        public List<string> GetAvailableNodes(string playerId, string treeId)
        {
            var tree = GetTree(playerId, treeId);
            return tree != null ? new List<string>(tree.AvailableNodes) : new List<string>();
        }
        
        #endregion
        
        #region Node Management
        
        /// <summary>
        /// 更新可用节点
        /// </summary>
        private void UpdateAvailableNodes(string playerId, string treeId)
        {
            var tree = GetTree(playerId, treeId);
            if (tree == null)
                return;
            
            // This would typically check skill tree definitions
            // For now, just add some placeholder logic
            
            // Example: root node is always available if not unlocked
            if (!tree.UnlockedNodes.Contains("root") && !tree.AvailableNodes.Contains("root"))
            {
                tree.AvailableNodes.Add("root");
            }
        }
        
        /// <summary>
        /// 获取技能树进度百分比
        /// </summary>
        public float GetTreeProgress(string playerId, string treeId, int totalNodes)
        {
            var tree = GetTree(playerId, treeId);
            if (tree == null || totalNodes == 0)
                return 0;
            
            return (float)tree.UnlockedNodes.Count / totalNodes;
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            var treesArray = new Array();
            foreach (var playerKvp in _skillTrees)
            {
                foreach (var treeKvp in playerKvp.Value)
                {
                    var entry = new Dictionary
                    {
                        ["playerId"] = playerKvp.Key,
                        ["tree"] = JsonSerializer.Serialize(treeKvp.Value)
                    };
                    treesArray.Add(entry);
                }
            }
            data["skillTrees"] = treesArray;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            _skillTrees.Clear();
            
            if (data.Contains("skillTrees"))
            {
                var treesArray = (Array)data["skillTrees"];
                foreach (Dictionary entry in treesArray)
                {
                    var playerId = entry["playerId"].ToString();
                    var tree = JsonSerializer.Deserialize<SkillTree>(entry["tree"].ToString());
                    
                    if (!_skillTrees.ContainsKey(playerId))
                    {
                        _skillTrees[playerId] = new Dictionary<string, SkillTree>();
                    }
                    _skillTrees[playerId][tree.TreeId] = tree;
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 技能树
    /// </summary>
    public class SkillTree
    {
        public string TreeId { get; set; }
        public List<string> UnlockedNodes { get; set; } = new List<string>();
        public List<string> AvailableNodes { get; set; } = new List<string>();
    }
}
