using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Characters;

namespace ClawRPG.Scripts.AI {
    /// <summary>
    /// Behavior Tree node base class
    /// REQ-165: Added Score() method for two-phase architecture (score + execute)
    /// </summary>
    public abstract partial class BTNode {
        public enum NodeState {
            Success,
            Failure,
            Running
        }
        
        protected NodeState _state = NodeState.Running;
        
        /// <summary>
        /// REQ-165: Pure scoring method — no side effects.
        /// Returns the score; stored in context.Score by the node itself.
        /// Fallback calls Execute() for nodes that haven't migrated to Score().
        /// </summary>
        public virtual float Score(BTContext context) {
            Execute(context);
            return context.Score;
        }
        
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
        
        // REQ-156: Boss AI 行为模式 (0=Strategic, 1=Enraged)
        public int BossMode { get; set; }
        
        // REQ-156: Ability cooldown tracking
        public Dictionary<string, float> AbilityCooldowns { get; set; } = new Dictionary<string, float>();
    }
    
    /// <summary>
    /// Selector node — runs children in order until one succeeds (fallback behavior).
    /// Unlike BTScoreSelector, this does not do scoring — it just picks the first succeeding child.
    /// Used as the root of the behavior tree for priority-based decisions.
    /// </summary>
    public class BTSelector : BTNode {
        private List<BTNode> _children = new List<BTNode>();
        
        public BTSelector() { }
        
        public BTSelector(params BTNode[] children) {
            foreach (var child in children) {
                _children.Add(child);
            }
        }
        
        public BTSelector AddChild(BTNode child) {
            _children.Add(child);
            return this;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            foreach (var child in _children) {
                child.Reset();
                var result = child.Execute(context);
                if (result != NodeState.Failure) {
                    return result;
                }
            }
            return NodeState.Failure;
        }
        
        public override float Score(BTContext context) {
            foreach (var child in _children) {
                child.Reset();
                var score = child.Score(context);
                if (score > 0) {
                    return score;
                }
            }
            return 0;
        }
        
        public override void Reset() {
            base.Reset();
            foreach (var child in _children) {
                child.Reset();
            }
        }
    }
    
    /// <summary>
    /// Sequence node — runs children in order until one fails
    /// </summary>
    public class BTSequence : BTNode {
        private List<BTNode> _children = new List<BTNode>();
        
        public BTSequence() { }
        
        public BTSequence(params BTNode[] children) {
            foreach (var child in children) {
                _children.Add(child);
            }
        }
        
        public BTSequence AddChild(BTNode child) {
            _children.Add(child);
            return this;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            foreach (var child in _children) {
                child.Reset();
                var result = child.Execute(context);
                if (result != NodeState.Success) {
                    return result;
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
    /// Condition node — returns Success if condition is true
    /// </summary>
    public class BTCondition : BTNode {
        private Func<BTContext, bool> _condition;
        
        public BTCondition(Func<BTContext, bool> condition) {
            _condition = condition;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            if (_condition == null || _condition(context)) {
                return NodeState.Success;
            }
            return NodeState.Failure;
        }
    }
    
    /// <summary>
    /// Action leaf node — performs a side-effect action
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
    /// REQ-165: Score selector — selects the best scoring node using two phases.
    /// 
    /// PHASE 1 (Score): Calls Score() on all children (no side effects).
    ///   → Finds best node, emits intent signal, does NOT execute ability.
    /// PHASE 2 (Execute): Calls Execute() only on the best node.
    ///   → Actually triggers the ability/effect.
    /// 
    /// Fixes the double-execution bug where scoring lambdas ran twice
    /// (once in the scoring loop, once in the final Execute() call).
    /// </summary>
    public class BTScoreSelector : BTNode {
        private List<BTNode> _scoringNodes = new List<BTNode>();
        
        public BTScoreSelector AddScoringNode(BTNode node) {
            _scoringNodes.Add(node);
            return this;
        }
        
        /// <summary>
        /// REQ-165: Score phase — evaluates all scoring nodes, finds best.
        /// Pure computation, no side effects.
        /// </summary>
        public override float Score(BTContext context) {
            BTNode bestNode = null;
            float bestScore = float.MinValue;
            
            foreach (var node in _scoringNodes) {
                node.Reset();
                var score = node.Score(context);
                if (score > bestScore) {
                    bestScore = score;
                    bestNode = node;
                }
            }
            
            return bestScore;
        }
        
        /// <summary>
        /// REQ-165: Two-phase execution.
        /// Phase 1: Score all nodes via Score() method (no side effects).
        /// Phase 2: Execute only the best node (side effects happen here).
        /// This fixes the double-execution bug where evaluate lambdas ran twice.
        /// </summary>
        protected override NodeState OnExecute(BTContext context) {
            // PHASE 1: Score all nodes (no side effects)
            BTNode bestNode = null;
            float bestScore = float.MinValue;
            
            foreach (var node in _scoringNodes) {
                node.Reset();
                var score = node.Score(context);
                if (score > bestScore) {
                    bestScore = score;
                    bestNode = node;
                }
            }
            
            // PHASE 2: Execute only the best node
            // Note: BTScoringAction.Execute() skips re-running the evaluate lambda
            // if ctx.SelectedAbility is already set (prevents double-execution bug).
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
    /// REQ-165: Scoring leaf node — evaluates a score (pure computation, no side effects).
    /// 
    /// Score() runs the evaluate lambda → sets ctx.Score and ctx.SelectedAbility.
    /// Execute() re-uses ctx.SelectedAbility without re-running the evaluate lambda
    /// (avoids double-execution bug when called from BTScoreSelector.OnExecute).
    /// </summary>
    public class BTScoringAction : BTNode {
        private Action<BTContext> _evaluate;
        private float _lastScore;
        private string _lastSelectedAbility;
        
        public BTScoringAction(Action<BTContext> evaluate) {
            _evaluate = evaluate;
        }
        
        /// <summary>
        /// REQ-165: Pure scoring — runs the evaluate lambda to compute ctx.Score.
        /// Stores the result so Execute() can re-use without double-execution.
        /// </summary>
        public override float Score(BTContext context) {
            _lastScore = context.Score;
            _lastSelectedAbility = context.SelectedAbility;
            _evaluate?.Invoke(context);
            _lastScore = context.Score;
            _lastSelectedAbility = context.SelectedAbility;
            return context.Score;
        }
        
        protected override NodeState OnExecute(BTContext context) {
            // REQ-165 fix: Only run evaluate if SelectedAbility wasn't already set
            // by a previous Score() call. This prevents the double-execution bug.
            if (string.IsNullOrEmpty(context.SelectedAbility)) {
                _evaluate?.Invoke(context);
            }
            _lastScore = context.Score;
            _lastSelectedAbility = context.SelectedAbility;
            return context.SelectedAbility != null ? NodeState.Success : NodeState.Failure;
        }
        
        public float LastScore => _lastScore;
        public string LastSelectedAbility => _lastSelectedAbility;
    }
    
    /// <summary>
    /// Inverter node — inverts the result of child
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
        
        public override float Score(BTContext context) {
            return _child.Score(context);
        }
    }
    
    /// <summary>
    /// Decorator node — runs child only when condition is true
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
        
        public override float Score(BTContext context) {
            if (_condition == null || _condition(context)) {
                return _child.Score(context);
            }
            return float.MinValue;
        }
    }
}
