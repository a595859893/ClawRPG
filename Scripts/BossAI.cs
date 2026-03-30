using Godot;
using System;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Boss AI - handles AI state machine and behavior
    /// </summary>
    public class BossAI
    {
        private Boss _boss;
        private BossData _data;
        
        // AI State
        private BossAIState _currentState = BossAIState.Idle;
        private float _stateTimer;
        private float _retreatThreshold = 0.3f;
        private float _wanderRadius = 50f;
        private Vector2 _wanderTarget;
        
        // AI Configuration
        private float _attackRange = 80f;
        private float _chaseRange = 400f;
        private float _predictTargetTime = 0.2f;
        
        // Events
        public event Action<BossAIState> OnStateChanged;
        
        public BossAI(Boss boss, BossData data)
        {
            _boss = boss;
            _data = data;
        }
        
        /// <summary>
        /// Get current AI state
        /// </summary>
        public BossAIState GetState() => _currentState;
        
        /// <summary>
        /// Force set AI state (for decision maker)
        /// </summary>
        public void ForceSetState(BossAIState newState)
        {
            if (_currentState == newState) return;
            
            Invariant.Assert(_stateTimer >= 0, "BossAI ForceSetState: previous timer negative from {0}: {1}", _currentState, _stateTimer);
            
            _currentState = newState;
            _stateTimer = GetStateDuration(newState);
            OnStateChanged?.Invoke(newState);
            
            GD.Print($"{_data.BossTitle} state changed to {newState}");
        }
        
        /// <summary>
        /// Process AI update
        /// </summary>
        public void Update(float dt)
        {
            _stateTimer -= dt;
            
            if (_currentState == BossAIState.Stunned && _stateTimer <= 0)
            {
                SetState(BossAIState.Idle);
                return;
            }
            
            if (_currentState == BossAIState.Retreating && _stateTimer <= 0)
            {
                SetState(BossAIState.Idle);
                return;
            }
            
            var target = _boss.GetTarget();
            if (target == null)
            {
                SetState(BossAIState.Idle);
                Wander(dt);
                return;
            }
            
            float distanceToTarget = _boss.GlobalPosition.DistanceTo(target.GlobalPosition);
            float healthPercent = (float)_boss.CurrentHealth / _boss.MaxHealth;
            
            // State machine transitions
            switch (_currentState)
            {
                case BossAIState.Idle:
                    if (distanceToTarget > _chaseRange)
                    {
                        SetState(BossAIState.Chasing);
                    }
                    else if (distanceToTarget <= _attackRange)
                    {
                        SetState(BossAIState.Attacking);
                    }
                    break;
                    
                case BossAIState.Chasing:
                    if (distanceToTarget <= _attackRange)
                    {
                        SetState(BossAIState.Attacking);
                    }
                    else if (distanceToTarget > _chaseRange * 1.5f)
                    {
                        SetState(BossAIState.Idle);
                    }
                    break;
                    
                case BossAIState.Attacking:
                    if (distanceToTarget > _attackRange * 1.5f)
                    {
                        SetState(BossAIState.Chasing);
                    }
                    break;
                    
                case BossAIState.UsingAbility:
                    if (_stateTimer <= 0)
                    {
                        SetState(BossAIState.Idle);
                    }
                    return; // Don't move while using ability
            }
            
            // Check for retreat at low health
            if (healthPercent < _retreatThreshold && _currentState != BossAIState.Retreating && _currentState != BossAIState.UsingAbility)
            {
                TryRetreat();
            }
            
            // Execute state behavior
            ExecuteStateBehavior(dt, target);
        }
        
        /// <summary>
        /// Set new state
        /// </summary>
        private void SetState(BossAIState newState)
        {
            if (_currentState == newState) return;
            
            // Invariant: state timer from previous state should have been non-negative
            Invariant.Assert(_stateTimer >= 0, "BossAI state timer was negative before transition from {0}: {1}", _currentState, _stateTimer);
            
            _currentState = newState;
            _stateTimer = GetStateDuration(newState);
            
            // Invariant: new state timer must be positive
            Invariant.Assert(_stateTimer > 0, "BossAI new state {0} has non-positive duration: {1}", newState, _stateTimer);
            
            OnStateChanged?.Invoke(newState);
        }
        
        /// <summary>
        /// Get state duration
        /// </summary>
        private float GetStateDuration(BossAIState state)
        {
            switch (state)
            {
                case BossAIState.Idle: return 1f;
                case BossAIState.Chasing: return 2f;
                case BossAIState.Attacking: return 1.5f;
                case BossAIState.UsingAbility: return 3f;
                case BossAIState.Retreating: return 2f;
                case BossAIState.Stunned: return 2f;
                default: return 1f;
            }
        }
        
        /// <summary>
        /// Execute behavior based on current state
        /// </summary>
        private void ExecuteStateBehavior(float dt, Character target)
        {
            switch (_currentState)
            {
                case BossAIState.Idle:
                    Wander(dt);
                    break;
                    
                case BossAIState.Chasing:
                    ChaseTarget(target);
                    break;
                    
                case BossAIState.Attacking:
                    AttackTarget(target);
                    break;
                    
                case BossAIState.Retreating:
                    RetreatFromTarget(target);
                    break;
            }
        }
        
        /// <summary>
        /// Wander when idle
        /// </summary>
        private void Wander(float dt)
        {
            if (_stateTimer <= 0 || _boss.GlobalPosition.DistanceTo(_wanderTarget) < 10f)
            {
                _wanderTarget = _boss.GlobalPosition + new Vector2(
                    (float)GD.RandRange(-_wanderRadius, _wanderRadius),
                    (float)GD.RandRange(-_wanderRadius, _wanderRadius)
                );
            }
            
            _boss.MoveTo(_wanderTarget, _boss.MoveSpeed * 0.3f);
        }
        
        /// <summary>
        /// Chase target
        /// </summary>
        private void ChaseTarget(Character target)
        {
            Vector2 predictedPos = target.GlobalPosition;
            if (target is Player player)
            {
                predictedPos += player.Velocity * _predictTargetTime;
            }
            
            _boss.MoveTo(predictedPos, _boss.MoveSpeed);
        }
        
        /// <summary>
        /// Attack target
        /// </summary>
        private void AttackTarget(Character target)
        {
            if (target.GlobalPosition.x < _boss.GlobalPosition.x)
                _boss.FaceDirection(-1);
            else
                _boss.FaceDirection(1);
            
            _boss.TryAttack();
        }
        
        /// <summary>
        /// Retreat from target
        /// </summary>
        private void RetreatFromTarget(Character target)
        {
            Vector2 retreatDir = (_boss.GlobalPosition - target.GlobalPosition).Normalized();
            Vector2 retreatPos = _boss.GlobalPosition + retreatDir * 200f;
            _boss.MoveTo(retreatPos, _boss.MoveSpeed * 0.8f);
        }
        
        /// <summary>
        /// Try to retreat
        /// </summary>
        private void TryRetreat()
        {
            SetState(BossAIState.Retreating);
            GD.Print($"{_data.BossTitle} is retreating to recover!");
        }
        
        /// <summary>
        /// Check if can use ability
        /// </summary>
        public bool CanUseAbility()
        {
            return _currentState != BossAIState.UsingAbility && 
                   _currentState != BossAIState.Stunned &&
                   _currentState != BossAIState.Retreating;
        }
    }
}
