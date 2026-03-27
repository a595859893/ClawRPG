using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物决策系统 - 决定何时行动以及进入什么状态
    /// 职责：状态机管理、战术决策、冷却计时
    /// </summary>
    public class PetDecisionSystem : BaseSystem
    {
        private static PetDecisionSystem _instance;
        public static PetDecisionSystem Instance => _instance ??= new PetDecisionSystem();

        // AI配置
        private float _followDistance = 80f;
        private float _attackRange = 100f;
        private float _followSpeed = 180f;
        private float _attackCooldown = 1.5f;
        private float _lastAttackTime = 0f;
        private float _supportCooldown = 0f;

        // 战术配置
        private float _tacticalDistance = 150f;

        // 状态机
        public enum PetAIState { Idle, Following, Engaging, Attacking, Retreating, Supporting }
        public PetAIState CurrentState { get; private set; } = PetAIState.Idle;
        private PetAIState _previousState = PetAIState.Idle;
        
        // 宠物引用
        private Pet _activePet;
        private CharacterBody2D _player;
        private Node2D _currentTarget;
        
        // 宠物性格
        private PetPersonality _personality = PetPersonality.Balanced;
        
        // 计时器
        private float _stateTimer = 0f;

        // 决策 Tick ID 计数器（REQ-137: 跨系统决策追溯）
        private static int _decisionTickId = 0;
        public static int DecisionTickId => _decisionTickId;

        /// <summary>
        /// 推进决策 Tick ID（每次决策入口调用，跨系统共享）
        /// </summary>
        public static int NextDecisionTick() => ++_decisionTickId;

        /// <summary>
        /// 重置 Tick ID（每场战斗开始时调用）
        /// </summary>
        public static void ResetDecisionTick() => _decisionTickId = 0;

        // 信号
        public Action<PetAIState, PetAIState> OnStateChanged;
        
        public void Initialize(CharacterBody2D player)
        {
            _instance = this;
            _player = player;
            GD.Print("[PetDecisionSystem] Initialized");
        }

        public void SetActivePet(Pet pet)
        {
            _activePet = pet;
            if (pet != null)
            {
                UpdatePersonality(pet.Type);
            }
        }

        public void SetPlayer(CharacterBody2D player)
        {
            _player = player;
        }

        private void UpdatePersonality(PetType type)
        {
            switch (type)
            {
                case PetType.Companion:
                    _personality = PetPersonality.Aggressive;
                    _attackCooldown = 1.2f;
                    _tacticalDistance = 120f;
                    break;
                case PetType.Collector:
                    _personality = PetPersonality.Cautious;
                    _attackCooldown = 2.0f;
                    _tacticalDistance = 200f;
                    _followDistance = 120f;
                    break;
                case PetType.Guardian:
                    _personality = PetPersonality.Defensive;
                    _attackCooldown = 1.8f;
                    _tacticalDistance = 80f;
                    _followDistance = 50f;
                    break;
                case PetType.Explorer:
                    _personality = PetPersonality.Balanced;
                    _attackCooldown = 1.5f;
                    _tacticalDistance = 150f;
                    break;
            }
        }

        /// <summary>
        /// 更新决策 - 根据目标更新状态机
        /// </summary>
        public void UpdateDecision(Node2D target, float delta)
        {
            NextDecisionTick(); // REQ-137: 每个决策周期分配唯一 Tick ID
            _stateTimer += delta;
            
            if (target == null)
            {
                SetState(PetAIState.Following);
                return;
            }
            
            float distToEnemy = GlobalPosition.DistanceTo(target.GlobalPosition);
            float distToPlayer = _player != null ? GlobalPosition.DistanceTo(_player.GlobalPosition) : float.MaxValue;
            
            // 玩家血量检查
            bool playerLowHealth = false; 
            if (_player != null && _player.HasMethod("GetCurrentHealth"))
            {
                int playerHp = (int)_player.Call("GetCurrentHealth");
                int playerMaxHp = (int)_player.Call("GetMaxHealth");
                playerLowHealth = playerMaxHp > 0 && (float)playerHp / playerMaxHp < 0.3f;
            }
            
            PetAIState newState = DecideState(target, distToEnemy, playerLowHealth);
            SetState(newState);
        }

        private PetAIState DecideState(Node2D target, float distToEnemy, bool playerLowHealth)
        {
            switch (_personality)
            {
                case PetPersonality.Defensive:
                    // 守护型：玩家血量低时进入支援状态
                    if (playerLowHealth && _activePet?.SpecialAbility != "")
                    {
                        return PetAIState.Supporting;
                    }
                    // 守护型：优先攻击靠近玩家的敌人
                    if (distToEnemy <= _attackRange)
                        return PetAIState.Attacking;
                    else if (distToEnemy <= _tacticalDistance)
                        return PetAIState.Engaging;
                    else
                        return PetAIState.Engaging;
                    
                case PetPersonality.Aggressive:
                    // 攻击型：保持战斗
                    if (distToEnemy <= _attackRange)
                        return PetAIState.Attacking;
                    else if (distToEnemy <= _tacticalDistance)
                        return PetAIState.Engaging;
                    else
                        return PetAIState.Engaging;
                    
                case PetPersonality.Cautious:
                    // 谨慎型：保持距离
                    if (distToEnemy < 80f)
                        return PetAIState.Retreating;
                    else if (distToEnemy <= _attackRange)
                        return PetAIState.Attacking;
                    else if (distToEnemy <= _tacticalDistance + 50f)
                        return PetAIState.Engaging;
                    else
                        return PetAIState.Following;
                    
                default: // Balanced
                    if (distToEnemy <= _attackRange)
                        return PetAIState.Attacking;
                    else if (distToEnemy <= _tacticalDistance)
                        return PetAIState.Engaging;
                    else
                        return PetAIState.Engaging;
            }
        }

        private void SetState(PetAIState newState)
        {
            if (CurrentState != newState)
            {
                _previousState = CurrentState;
                CurrentState = newState;
                OnStateChanged?.Invoke(_previousState, newState);
            }
        }

        /// <summary>
        /// 检查是否可以攻击
        /// </summary>
        public bool CanAttack()
        {
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            return currentTime - _lastAttackTime >= _attackCooldown;
        }

        /// <summary>
        /// 记录攻击
        /// </summary>
        public void RecordAttack()
        {
            _lastAttackTime = (float)Time.GetTicksMsec() / 1000f;
        }

        /// <summary>
        /// 获取攻击冷却剩余时间
        /// </summary>
        public float GetAttackCooldownRemaining()
        {
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            float remaining = _attackCooldown - (currentTime - _lastAttackTime);
            return Mathf.Max(0, remaining);
        }

        /// <summary>
        /// 更新支援冷却
        /// </summary>
        public void UpdateSupportCooldown(float delta)
        {
            _supportCooldown += delta;
        }

        /// <summary>
        /// 是否可以执行支援
        /// </summary>
        public bool CanSupport()
        {
            return _supportCooldown >= 3f;
        }

        /// <summary>
        /// 重置支援冷却
        /// </summary>
        public void ResetSupportCooldown()
        {
            _supportCooldown = 0f;
        }

        /// <summary>
        /// 获取跟随配置
        /// </summary>
        public float GetFollowDistance() => _followDistance;
        public float GetFollowSpeed() => _followSpeed;
        public float GetTacticalDistance() => _tacticalDistance;
        public float GetAttackRange() => _attackRange;
        public PetPersonality GetPersonality() => _personality;
        public Pet GetActivePet() => _activePet;
        public CharacterBody2D GetPlayer() => _player;

        /// <summary>
        /// 获取跟随位置偏移
        /// </summary>
        public Vector2 GetFollowOffset()
        {
            return _activePet?.Type switch
            {
                PetType.Guardian => new Vector2(GD.Randf() * 60f - 30f, -30f),
                PetType.Collector => new Vector2(GD.Randf() * 80f - 40f, -60f),
                _ => new Vector2(GD.Randf() * 40f - 20f, -_followDistance)
            };
        }

        public void SetCurrentTarget(Node2D target)
        {
            _currentTarget = target;
        }

        public Node2D GetCurrentTarget() => _currentTarget;

        public void ResetStateTimer()
        {
            _stateTimer = 0f;
        }

        public float GetStateTimer() => _stateTimer;

        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "lastAttackTime", _lastAttackTime },
                { "supportCooldown", _supportCooldown },
                { "currentState", (int)CurrentState },
                { "personality", (int)_personality }
            };
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            if (data.Contains("lastAttackTime"))
                _lastAttackTime = Convert.ToSingle(data["lastAttackTime"]);
            if (data.Contains("supportCooldown"))
                _supportCooldown = Convert.ToSingle(data["supportCooldown"]);
            if (data.Contains("currentState"))
                CurrentState = (PetAIState)(int)data["currentState"];
            if (data.Contains("personality"))
                _personality = (PetPersonality)(int)data["personality"];
        }
    }
}
