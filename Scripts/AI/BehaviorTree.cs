using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.AI {
    /// <summary>
    /// Behavior Tree node base class
    /// </summary>
    public abstract class BTNode {
        public enum NodeState {
            Success,
            Failure,
            Running
        }
        
        protected NodeState _state = NodeState.Running;
        
        public virtual NodeState Execute(BTContext context) {
            return _state = OnExecute(context);
        }
        
        protected abstract NodeState OnExecute(BTContext context);
        
        public virtual void Reset() {
            _state = NodeState.Running;
        }
    }
    
    /// <summary>
    /// Behavior Tree execution context
    /// </summary>
    public class BTContext {
        public Boss Boss { get; set; }
        public Character Target { get; set; }
        public float DeltaTime { get; set; }
        
        // Decision results
        public string SelectedAbility { get; set; }
        public BossAIState DesiredState { get; set; }
        public float Score { get; set; }
        
        public float DistanceToTarget => Target != null ? Boss.GlobalPosition.DistanceTo(Target.GlobalPosition) : float.MaxValue;
        public float HealthPercent => (float)Boss.CurrentHealth / Boss.MaxHealth;
        public int CurrentPhase => Boss.GetCurrentPhase();
        public bool IsEnraged => Boss.IsEnraged();
        public bool IsRageTriggered => Boss.IsRageTriggered(); // REQ-127: HP < 5% rage
        public float TimeSinceLastAbility { get; set; }
    }
    
    /// <summary>
    /// Sequence node - executes children in order until one fails
    /// </summary>
    public class BTSequence : BTNode {
        private List<BTNode> _children = new List<BTNode>();
        
        public BTSequence(params BTNode[] children) {
            _children.AddRange(children);
        }
        
        protected override NodeState OnExecute(BTContext context) {
            foreach (var child in _children) {
                var result = child.Execute(context);
                if (result == NodeState.Failure) {
                    return NodeState.Failure;
                }
                if (result == NodeState.Running) {
                    return NodeState.Running;
                }
            }
            return NodeState.Success;
        }
        
        public override void Reset() {
            base.Reset();
            foreach (var child in _children) {
                child.Reset();
            }
        }
    }
    
    /// <summary>
    /// Selector node - executes children in order until one succeeds
    /// </summary>
    public class BTSelector : BTNode {
        private List<BTNode> _children = new List<BTNode>();
        
        public BTSelector(params BTNode[] children) {
            _children.AddRange(children);
        }
        
        protected override NodeState OnExecute(BTContext context) {
            foreach (var child in _children) {
                var result = child.Execute(context);
                if (result == NodeState.Success) {
                    return NodeState.Success;
                }
                if (result == NodeState.Running) {
                    return NodeState.Running;
                }
            }
            return NodeState.Failure;
        }
        
        public override void Reset() {
            base.Reset();
            foreach (var child in _children) {
                child.Reset();
            }
        }
    }
    
    /// <summary>
    /// Condition node - checks a condition
    /// </summary>
    public class BTCondition : BTNode {
        private Func<BTContext, bool> _condition;
        
        public BTCondition(Func<BTContext, bool> condition) {
            _condition = condition;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            return _condition(context) ? NodeState.Success : NodeState.Failure;
        }
    }
    
    /// <summary>
    /// Action node - executes an action
    /// </summary>
    public class BTAction : BTNode {
        private Action<BTContext> _action;
        
        public BTAction(Action<BTContext> action) {
            _action = action;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            _action?.Invoke(context);
            return NodeState.Success;
        }
    }
    
    /// <summary>
    /// Score-based action node - evaluates and selects best action
    /// </summary>
    public class BTScoreSelector : BTNode {
        private List<BTNode> _scoringNodes = new List<BTNode>();
        
        public BTScoreSelector AddScoringNode(BTNode node) {
            _scoringNodes.Add(node);
            return this;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            BTNode bestNode = null;
            float bestScore = float.MinValue;
            
            foreach (var node in _scoringNodes) {
                node.Reset();
                var result = node.Execute(context);
                if (result != NodeState.Failure && context.Score > bestScore) {
                    bestScore = context.Score;
                    bestNode = node;
                }
            }
            
            if (bestNode != null) {
                bestNode.Reset();
                bestNode.Execute(context);
                return NodeState.Success;
            }
            
            return NodeState.Failure;
        }
        
        public override void Reset() {
            base.Reset();
            foreach (var node in _scoringNodes) {
                node.Reset();
            }
        }
    }
    
    /// <summary>
    /// Scoring leaf node - calculates a score for a particular action
    /// </summary>
    public class BTScoringAction : BTNode {
        private Action<BTContext> _evaluate;
        
        public BTScoringAction(Action<BTContext> evaluate) {
            _evaluate = evaluate;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            _evaluate?.Invoke(context);
            return context.SelectedAbility != null ? NodeState.Success : NodeState.Failure;
        }
    }
    
    /// <summary>
    /// Inverter node - inverts the result of child
    /// </summary>
    public class BTInverter : BTNode {
        private BTNode _child;
        
        public BTInverter(BTNode child) {
            _child = child;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            var result = _child.Execute(context);
            if (result == NodeState.Success) return NodeState.Failure;
            if (result == NodeState.Failure) return NodeState.Success;
            return NodeState.Running;
        }
    }
    
    /// <summary>
    /// Decorator node - runs child only when condition is true
    /// </summary>
    public class BTDecorator : BTNode {
        private BTNode _child;
        private Func<BTContext, bool> _condition;
        
        public BTDecorator(BTNode child, Func<BTContext, bool> condition) {
            _child = child;
            _condition = condition;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            if (_condition == null || _condition(context)) {
                return _child.Execute(context);
            }
            return NodeState.Failure;
        }
    }
}
