using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems.Pets.AI;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物战斗AI主控制器 - 协调各AI子系统
    /// </summary>
    public partial class PetCombatAI : BaseSystem
    {
        private static PetCombatAI _instance;
        public static PetCombatAI Instance => _instance ??= new PetCombatAI();

        // 子系统
        private PetAIDecision _decisionSystem;
        private PetBehaviorTree _behaviorTree;
        private PetSkillSelector _skillSelector;
        
        // 宠物战斗属性
        private Pet _activePet;
        private CharacterBody2D _player;
        
        // AI配置
        private float _followDistance = 80f;
        private float _attackRange = 100f;
        private float _followSpeed = 180f;
        private float _attackCooldown = 1.5f;
        private float _lastAttackTime = 0f;
        
        // 状态机
        private PetAIState _currentState = PetAIState.Idle;
        
        // 信号
        [Signal] public delegate void PetAttackedEventHandler(Node2D enemy, int damage);
        [Signal] public delegate void PetStateChangedEventHandler(PetAIState newState);
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            
            // 初始化子系统
            _decisionSystem = new PetAIDecision();
            _behaviorTree = new PetBehaviorTree();
            _skillSelector = new PetSkillSelector();
        }
        
        protected override string SystemName => "PetCombatAI";
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            GD.Print("[PetCombatAI] Initialized");
        }
        
        /// <summary>
        /// 设置活跃宠物
        /// </summary>
        public void SetActivePet(Pet pet)
        {
            _activePet = pet;
            
            // 根据宠物类型设置性格
            if (pet != null)
            {
                _decisionSystem.DeterminePersonalityFromPetType(pet.Type);
            }
        }
        
        /// <summary>
        /// 设置玩家引用
        /// </summary>
        public void SetPlayer(CharacterBody2D player)
        {
            _player = player;
        }
        
        /// <summary>
        /// 主更新循环
        /// </summary>
        public override void _Process(double delta)
        {
            if (_activePet == null || _player == null)
                return;
            
            float deltaF = (float)delta;
            
            // 更新状态机
            UpdateStateMachine(deltaF);
            
            // 执行当前状态行为
            ExecuteStateBehavior(deltaF);
            
            // 更新视觉效果
            UpdateVisuals();
        }
        
        /// <summary>
        /// 更新状态机
        /// </summary>
        private void UpdateStateMachine(float delta)
        {
            Vector2 playerPos = _player.GlobalPosition;
            Vector2 petPos = _activePet.GlobalPosition;
            float distToPlayer = petPos.DistanceTo(playerPos);
            
            // 检测附近敌人
            List<Node2D> nearbyEnemies = DetectNearbyEnemies();
            bool playerInCombat = nearbyEnemies.Count > 0;
            
            // 创建上下文
            var context = new PetBehaviorTree.PetAIContext
            {
                PlayerPosition = playerPos,
                PetPosition = petPos,
                NearbyEnemies = nearbyEnemies,
                DistanceToPlayer = distToPlayer,
                PetHealthPercent = 1.0f,  // 简化
                PlayerInCombat = playerInCombat,
                CurrentState = _currentState
            };
            
            // 使用决策系统
            var decision = _decisionSystem.MakeDecision(_currentState, playerPos, petPos, 
                                                       nearbyEnemies, distToPlayer, 
                                                       1.0f, playerInCombat);
            
            // 转换决策到状态
            if (decision.Confidence > 0.5f)
            {
                ChangeState(decision.TargetState);
            }
        }
        
        /// <summary>
        /// 执行状态行为
        /// </summary>
        private void ExecuteStateBehavior(float delta)
        {
            switch (_currentState)
            {
                case PetAIState.Idle:
                    // 待命状态
                    break;
                    
                case PetAIState.Following:
                    FollowPlayer(delta);
                    break;
                    
                case PetAIState.Engaging:
                case PetAIState.Attacking:
                    if (_activePet != null)
                    {
                        // 寻找目标并攻击
                    }
                    break;
                    
                case PetAIState.Retreating:
                    Retreat(delta);
                    break;
                    
                case PetAIState.Supporting:
                    SupportPlayer(delta);
                    break;
            }
        }
        
        /// <summary>
        /// 跟随玩家
        /// </summary>
        private void FollowPlayer(float delta)
        {
            if (_player == null || _activePet == null)
                return;
            
            Vector2 targetPos = _player.GlobalPosition;
            float dist = _activePet.GlobalPosition.DistanceTo(targetPos);
            
            // 保持跟随距离
            if (dist > _followDistance)
            {
                MoveTowards(targetPos, delta, _followSpeed);
            }
        }
        
        /// <summary>
        /// 后撤
        /// </summary>
        private void Retreat(float delta)
        {
            if (_player == null || _activePet == null)
                return;
            
            // 向玩家移动
            MoveTowards(_player.GlobalPosition, delta, _followSpeed * 1.2f);
        }
        
        /// <summary>
        /// 支援玩家
        /// </summary>
        private void SupportPlayer(float delta)
        {
            // 使用增益技能
            var skill = _skillSelector.SelectBestSkill(new PetBehaviorTree.PetAIContext
            {
                PlayerInCombat = true,
                PetHealthPercent = 1.0f
            });
            
            if (skill != null)
            {
                _skillSelector.UseSkill(skill.Id);
            }
        }
        
        /// <summary>
        /// 移动到目标
        /// </summary>
        private void MoveTowards(Vector2 targetPos, float delta, float speed)
        {
            // 简化实现
            Vector2 direction = (targetPos - _activePet.GlobalPosition).Normalized();
            _activePet.GlobalPosition += direction * speed * delta;
        }
        
        /// <summary>
        /// 检测附近敌人
        /// </summary>
        private List<Node2D> DetectNearbyEnemies()
        {
            // 简化实现，返回空列表
            // 实际需要使用 Area2D 检测
            return new List<Node2D>();
        }
        
        /// <summary>
        /// 改变状态
        /// </summary>
        private void ChangeState(PetAIState newState)
        {
            if (_currentState != newState)
            {
                _currentState = newState;
                EmitSignal(SignalName.PetStateChanged, newState);
            }
        }
        
        /// <summary>
        /// 更新视觉效果
        /// </summary>
        private void UpdateVisuals()
        {
            // 更新宠物精灵和动画
        }
        
        /// <summary>
        /// 玩家受伤时调用
        /// </summary>
        public void OnPlayerDamaged(int damage)
        {
            // 切换到支援状态
            ChangeState(PetAIState.Supporting);
        }
        
        /// <summary>
        /// 玩家死亡时调用
        /// </summary>
        public void OnPlayerDeath()
        {
            // 返回玩家身边
            ChangeState(PetAIState.Following);
        }
        
        /// <summary>
        /// 设置宠物可见性
        /// </summary>
        public void SetPetVisible(bool visible)
        {
            // 设置宠物精灵可见性
        }
        
        #region Properties
        
        public bool IsEnabled { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public PetAIState CurrentState => _currentState;
        
        #endregion
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
        }
    }
}
