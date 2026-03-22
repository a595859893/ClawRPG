using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Characters
{
    /// <summary>
    /// 玩家状态机 - 管理玩家所有行为状态
    /// </summary>
    public partial class PlayerStateMachine : BaseSystem
    {
        // 状态枚举
        public enum PlayerState 
        {
            Idle,
            Walk,
            Run,
            Attack,
            Block,
            Dodge,
            Cast,
            Hurt,
            Dead,
            Interact
        }

        // 当前状态
        private PlayerState _currentState = PlayerState.Idle;
        public PlayerState CurrentState 
        { 
            get => _currentState; 
            private set => _currentState = value;
        }

        // 状态持续时间追踪
        private float _stateTimer = 0f;
        private float _stateDuration = 0f;

        // 引用
        private Player _player;
        
        // 状态事件
        public Action<PlayerState, PlayerState> OnStateChanged;
        public Action<PlayerState> OnStateEnter;
        public Action<PlayerState> OnStateExit;

        // 状态配置
        private Dictionary<PlayerState, StateConfig> _stateConfigs = new Dictionary<PlayerState, StateConfig>();

        private class StateConfig
        {
            public bool CanMove { get; set; } = true;
            public bool CanAttack { get; set; } = true;
            public bool CanBlock { get; set; } = true;
            public bool CanDodge { get; set; } = true;
            public bool CanCast { get; set; } = true;
            public bool IsInvincible { get; set; } = false; 
            public float AnimationBlend { get; set; } = 0f;
        }

        public override void _Ready()
        {
            _player = GetParent<Player>();
            InitializeStateConfigs();
            ChangeState(PlayerState.Idle);
        }

        private void InitializeStateConfigs()
        {
            // Idle 状态
            _stateConfigs[PlayerState.Idle] = new StateConfig
            {
                CanMove = true,
                CanAttack = true,
                CanBlock = true,
                CanDodge = true,
                CanCast = true,
                IsInvincible = false
            };

            // Walk 状态
            _stateConfigs[PlayerState.Walk] = new StateConfig
            {
                CanMove = true,
                CanAttack = true,
                CanBlock = true,
                CanDodge = true,
                CanCast = true,
                IsInvincible = false
            };

            // Run 状态
            _stateConfigs[PlayerState.Run] = new StateConfig
            {
                CanMove = true,
                CanAttack = false,
                CanBlock = false,
                CanDodge = true,
                CanCast = false,
                IsInvincible = false
            };

            // Attack 状态
            _stateConfigs[PlayerState.Attack] = new StateConfig
            {
                CanMove = false,
                CanAttack = false,
                CanBlock = false,
                CanDodge = false,
                CanCast = false,
                IsInvincible = false
            };

            // Block 状态
            _stateConfigs[PlayerState.Block] = new StateConfig
            {
                CanMove = false,
                CanAttack = false,
                CanBlock = true,
                CanDodge = false,
                CanCast = false,
                IsInvincible = false
            };

            // Dodge 状态
            _stateConfigs[PlayerState.Dodge] = new StateConfig
            {
                CanMove = false,
                CanAttack = false,
                CanBlock = false,
                CanDodge = false,
                CanCast = false,
                IsInvincible = true
            };

            // Cast 状态 (技能释放)
            _stateConfigs[PlayerState.Cast] = new StateConfig
            {
                CanMove = false,
                CanAttack = false,
                CanBlock = true,
                CanDodge = false,
                CanCast = false,
                IsInvincible = false
            };

            // Hurt 状态
            _stateConfigs[PlayerState.Hurt] = new StateConfig
            {
                CanMove = false,
                CanAttack = false,
                CanBlock = false,
                CanDodge = false,
                CanCast = false,
                IsInvincible = true
            };

            // Dead 状态
            _stateConfigs[PlayerState.Dead] = new StateConfig
            {
                CanMove = false,
                CanAttack = false,
                CanBlock = false,
                CanDodge = false,
                CanCast = false,
                IsInvincible = true
            };

            // Interact 状态
            _stateConfigs[PlayerState.Interact] = new StateConfig
            {
                CanMove = false,
                CanAttack = false,
                CanBlock = true,
                CanDodge = false,
                CanCast = false,
                IsInvincible = false
            };
        }

        public override void _Process(double delta)
        {
            _stateTimer += (float)delta;
            HandleStateBehavior((float)delta);
        }

        private void HandleStateBehavior(float delta)
        {
            switch (_currentState)
            {
                case PlayerState.Idle:
                    HandleIdleState(delta);
                    break;
                case PlayerState.Walk:
                    HandleWalkState(delta);
                    break;
                case PlayerState.Run:
                    HandleRunState(delta);
                    break;
                case PlayerState.Attack:
                    HandleAttackState(delta);
                    break;
                case PlayerState.Block:
                    HandleBlockState(delta);
                    break;
                case PlayerState.Dodge:
                    HandleDodgeState(delta);
                    break;
                case PlayerState.Cast:
                    HandleCastState(delta);
                    break;
                case PlayerState.Hurt:
                    HandleHurtState(delta);
                    break;
                case PlayerState.Dead:
                    HandleDeadState(delta);
                    break;
                case PlayerState.Interact:
                    HandleInteractState(delta);
                    break;
            }
        }

        private void HandleIdleState(float delta)
        {
            if (_player == null) return;
            
            // 检测移动输入
            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            if (inputDir.Length() > 0.1f)
            {
                // 按住Shift加速跑
                if (Input.IsActionPressed("sprint"))
                {
                    ChangeState(PlayerState.Run);
                }
                else
                {
                    ChangeState(PlayerState.Walk);
                }
            }
        }

        private void HandleWalkState(float delta)
        {
            if (_player == null) return;
            
            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            if (inputDir.Length() <= 0.1f)
            {
                ChangeState(PlayerState.Idle);
            }
            else if (Input.IsActionPressed("sprint"))
            {
                ChangeState(PlayerState.Run);
            }
        }

        private void HandleRunState(float delta)
        {
            if (_player == null) return;
            
            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            if (inputDir.Length() <= 0.1f)
            {
                ChangeState(PlayerState.Idle);
            }
            else if (!Input.IsActionPressed("sprint"))
            {
                ChangeState(PlayerState.Walk);
            }
        }

        private void HandleAttackState(float delta)
        {
            // 攻击动画完成后自动返回Idle
            if (_stateTimer >= _stateDuration)
            {
                ChangeState(PlayerState.Idle);
            }
        }

        private void HandleBlockState(float delta)
        {
            if (_player == null) return;
            
            // 松开格挡键返回Idle
            if (!Input.IsActionPressed("block"))
            {
                ChangeState(PlayerState.Idle);
            }
        }

        private void HandleDodgeState(float delta)
        {
            // 闪避结束后返回Idle
            if (_stateTimer >= _stateDuration)
            {
                ChangeState(PlayerState.Idle);
            }
        }

        private void HandleCastState(float delta)
        {
            // 技能释放完成后返回Idle
            if (_stateTimer >= _stateDuration)
            {
                ChangeState(PlayerState.Idle);
            }
        }

        private void HandleHurtState(float delta)
        {
            // 受伤动画完成后返回Idle
            if (_stateTimer >= _stateDuration)
            {
                ChangeState(PlayerState.Idle);
            }
        }

        private void HandleDeadState(float delta)
        {
            // 死亡状态保持不变，直到游戏结束
        }

        private void HandleInteractState(float delta)
        {
            // 交互完成后返回Idle
            if (_stateTimer >= _stateDuration)
            {
                ChangeState(PlayerState.Idle);
            }
        }

        public void ChangeState(PlayerState newState, float duration = 0f)
        {
            if (_currentState == newState && duration <= 0) return;

            PlayerState oldState = _currentState;
            
            // 退出旧状态
            OnStateExit?.Invoke(oldState);
            
            // 进入新状态
            _currentState = newState;
            _stateTimer = 0f;
            _stateDuration = duration;
            
            // 应用状态配置
            ApplyStateConfig(newState);
            
            // 触发事件
            OnStateEnter?.Invoke(newState);
            OnStateChanged?.Invoke(oldState, newState);
        }

        private void ApplyStateConfig(PlayerState state)
        {
            if (_player == null) return;
            
            if (_stateConfigs.TryGetValue(state, out var config))
            {
                _player.CanMove = config.CanMove;
                _player.CanAttackPlayer = config.CanAttack;
                _player.CanBlock = config.CanBlock;
                _player.CanDodge = config.CanDodge;
                _player.CanCast = config.CanCast;
                _player.IsInvincible = config.IsInvincible;
            }
        }

        // 公共方法：触发各种状态
        public void TriggerAttack(float duration = 0.5f)
        {
            if (CanTransitionTo(PlayerState.Attack))
            {
                ChangeState(PlayerState.Attack, duration);
            }
        }

        public void TriggerBlock()
        {
            if (CanTransitionTo(PlayerState.Block))
            {
                ChangeState(PlayerState.Block);
            }
        }

        public void TriggerDodge(float duration = 0.3f)
        {
            if (CanTransitionTo(PlayerState.Dodge))
            {
                ChangeState(PlayerState.Dodge, duration);
            }
        }

        public void TriggerCast(float duration = 1.0f)
        {
            if (CanTransitionTo(PlayerState.Cast))
            {
                ChangeState(PlayerState.Cast, duration);
            }
        }

        public void TriggerHurt(float duration = 0.3f)
        {
            if (CanTransitionTo(PlayerState.Hurt))
            {
                ChangeState(PlayerState.Hurt, duration);
            }
        }

        public void TriggerDead()
        {
            ChangeState(PlayerState.Dead);
        }

        public void TriggerInteract(float duration = 0.5f)
        {
            if (CanTransitionTo(PlayerState.Interact))
            {
                ChangeState(PlayerState.Interact, duration);
            }
        }

        private bool CanTransitionTo(PlayerState targetState)
        {
            // 死亡状态不可逆
            if (_currentState == PlayerState.Dead) return false;
            
            // 某些状态不允许被打断
            if (_currentState == PlayerState.Attack && targetState != PlayerState.Hurt)
                return false;
            if (_currentState == PlayerState.Cast && targetState != PlayerState.Hurt)
                return false;
            if (_currentState == PlayerState.Dodge && targetState != PlayerState.Hurt)
                return false;
                
            return true;
        }

        // 检查当前状态
        public bool IsInState(PlayerState state) => _currentState == state;
        public bool IsIdle => _currentState == PlayerState.Idle;
        public bool IsWalking => _currentState == PlayerState.Walk;
        public bool IsRunning => _currentState == PlayerState.Run;
        public bool IsAttacking => _currentState == PlayerState.Attack;
        public bool IsBlocking => _currentState == PlayerState.Block;
        public bool IsDodging => _currentState == PlayerState.Dodge;
        public bool IsCasting => _currentState == PlayerState.Cast;
        public bool IsHurt => _currentState == PlayerState.Hurt;
        public bool IsDead => _currentState == PlayerState.Dead;
        public bool CanAct => _currentState == PlayerState.Idle || _currentState == PlayerState.Walk;

        // ========== 持久化支持 ==========

        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                ["currentState"] = (int)_currentState,
                ["stateDuration"] = _stateDuration
            };
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            if (data.TryGetValue("currentState", out var stateVal))
            {
                var state = (PlayerState)(int)(System.Int64)stateVal;
                _currentState = state;
                _stateTimer = 0f;

                // 恢复状态配置到 Player 属性
                ApplyStateConfig(state);
            }

            if (data.TryGetValue("stateDuration", out var durVal))
            {
                _stateDuration = Convert.ToSingle(durVal);
            }
        }
    }
}
