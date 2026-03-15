using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Pets.AI
{
    /// <summary>
    /// 宠物行为树 - 实现复杂的行为逻辑
    /// </summary>
    public partial class PetBehaviorTree : BaseSystem
    {
        /// <summary>
        /// 行为节点类型
        /// </summary>
        public enum BehaviorNodeType
        {
            Sequence,      // 顺序执行
            Selector,      // 选择执行
            Condition,     // 条件判断
            Action,        // 执行动作
            Parallel      // 并行执行
        }
        
        /// <summary>
        /// 行为结果
        /// </summary>
        public enum BehaviorResult
        {
            Success,
            Failure,
            Running
        }
        
        /// <summary>
        /// 行为节点
        /// </summary>
        public abstract class BehaviorNode
        {
            public string Name { get; set; }
            public BehaviorNodeType Type { get; set; }
            public List<BehaviorNode> Children { get; set; } = new List<BehaviorNode>();
            
            public abstract BehaviorResult Execute(PetAIContext context);
        }
        
        /// <summary>
        /// AI 上下文
        /// </summary>
        public class PetAIContext
        {
            public Vector2 PlayerPosition { get; set; }
            public Vector2 PetPosition { get; set; }
            public List<Node2D> NearbyEnemies { get; set; } = new List<Node2D>();
            public float DistanceToPlayer { get; set; }
            public float PetHealthPercent { get; set; }
            public bool PlayerInCombat { get; set; }
            public PetAIState CurrentState { get; set; }
        }
        
        private BehaviorNode _rootNode;
        
        public override void _Ready()
        {
            base._Ready();
            BuildBehaviorTree();
        }
        
        /// <summary>
        /// 构建行为树
        /// </summary>
        private void BuildBehaviorTree()
        {
            _rootNode = new SelectorNode
            {
                Name = "Root",
                Type = BehaviorNodeType.Selector,
                Children = new List<BehaviorNode>
                {
                    // 最高优先级：保护玩家
                    new SequenceNode
                    {
                        Name = "ProtectPlayer",
                        Type = BehaviorNodeType.Sequence,
                        Children = new List<BehaviorNode>
                        {
                            new ConditionNode { Name = "PlayerInDanger" },
                            new ActionNode { Name = "MoveToPlayer" },
                            new ActionNode { Name = "SupportPlayer" }
                        }
                    },
                    
                    // 第二优先级：战斗
                    new SequenceNode
                    {
                        Name = "Combat",
                        Type = BehaviorNodeType.Sequence,
                        Children = new List<BehaviorNode>
                        {
                            new ConditionNode { Name = "HasEnemies" },
                            new ActionNode { Name = "SelectTarget" },
                            new ActionNode { Name = "EngageEnemy" }
                        }
                    },
                    
                    // 第三优先级：跟随玩家
                    new SequenceNode
                    {
                        Name = "FollowPlayer",
                        Type = BehaviorNodeType.Sequence,
                        Children = new List<BehaviorNode>
                        {
                            new ConditionNode { Name = "TooFarFromPlayer" },
                            new ActionNode { Name = "FollowPlayer" }
                        }
                    },
                    
                    // 默认：待命
                    new ActionNode { Name = "Idle" }
                }
            };
        }
        
        /// <summary>
        /// 执行行为树
        /// </summary>
        public BehaviorResult Execute(PetAIContext context)
        {
            if (_rootNode == null)
                return BehaviorResult.Failure;
            
            return _rootNode.Execute(context);
        }
        
        #region Node Implementations
        
        /// <summary>
        /// 顺序节点 - 所有子节点都成功才成功
        /// </summary>
        private class SequenceNode : BehaviorNode
        {
            public SequenceNode()
            {
                Type = BehaviorNodeType.Sequence;
            }
            
            public override BehaviorResult Execute(PetAIContext context)
            {
                foreach (var child in Children)
                {
                    var result = child.Execute(context);
                    if (result != BehaviorResult.Success)
                        return result;
                }
                return BehaviorResult.Success;
            }
        }
        
        /// <summary>
        /// 选择节点 - 返回第一个成功的子节点
        /// </summary>
        private class SelectorNode : BehaviorNode
        {
            public SelectorNode()
            {
                Type = BehaviorNodeType.Selector;
            }
            
            public override BehaviorResult Execute(PetAIContext context)
            {
                foreach (var child in Children)
                {
                    var result = child.Execute(context);
                    if (result == BehaviorResult.Success)
                        return result;
                    if (result == BehaviorResult.Running)
                        return result;
                }
                return BehaviorResult.Failure;
            }
        }
        
        /// <summary>
        /// 条件节点
        /// </summary>
        private class ConditionNode : BehaviorNode
        {
            public ConditionNode()
            {
                Type = BehaviorNodeType.Condition;
            }
            
            public override BehaviorResult Execute(PetAIContext context)
            {
                // 根据节点名称判断条件
                switch (Name)
                {
                    case "PlayerInDanger":
                        return context.PlayerInCombat ? BehaviorResult.Success : BehaviorResult.Failure;
                    case "HasEnemies":
                        return context.NearbyEnemies.Count > 0 ? BehaviorResult.Success : BehaviorResult.Failure;
                    case "TooFarFromPlayer":
                        return context.DistanceToPlayer > 100f ? BehaviorResult.Success : BehaviorResult.Failure;
                    default:
                        return BehaviorResult.Failure;
                }
            }
        }
        
        /// <summary>
        /// 动作节点
        /// </summary>
        private class ActionNode : BehaviorNode
        {
            public ActionNode()
            {
                Type = BehaviorNodeType.Action;
            }
            
            public override BehaviorResult Execute(PetAIContext context)
            {
                // 实际执行由 PetCombatAI 处理
                // 这里只返回成功状态
                return BehaviorResult.Success;
            }
        }
        
        #endregion
        
        /// <summary>
        /// 更新行为树配置
        /// </summary>
        public void UpdateTreeConfig(string configName, bool enabled)
        {
            // 可以动态启用/禁用某些行为分支
            GD.Print($"[PetBehaviorTree] Updated config: {configName} = {enabled}");
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // 加载数据
        }
    }
}
