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
        public static PetCombatAI Instance { get; private set; }

        // 子系统
        private PetAIDecision _decisionSystem;
        private PetBehaviorTree _behaviorTree;
        private PetSkillSelector _skillSelector;
        
        // 宠物战斗属性
        private Pet _activePet;
        private Node2D _petNode;  // 宠物场景节点（用于位置/移动）
        private CharacterBody2D _player;
        private Node2D _currentTarget;  // 当前攻击目标
        
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
            Instance = this;
            base._Ready();
            
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
        /// 设置宠物场景节点（用于位置/移动）
        /// </summary>
        public void SetPetNode(Node2D petNode)
        {
            _petNode = petNode;
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
            if (_activePet == null || _player == null || _petNode == null)
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
            Vector2 petPos = _petNode.GlobalPosition;
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
                    // 待命状态 - 保持跟随距离
                    FollowPlayer(delta);
                    break;
                    
                case PetAIState.Following:
                    FollowPlayer(delta);
                    break;
                    
                case PetAIState.Engaging:
                    EngageEnemy(delta);
                    break;
                    
                case PetAIState.Attacking:
                    AttackEnemy(delta);
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
        /// 接近敌人
        /// </summary>
        private void EngageEnemy(float delta)
        {
            var enemies = DetectNearbyEnemies();
            if (enemies.Count == 0) return;
            
            // 选择最近的敌人
            Node2D target = GetClosestEnemy(enemies);
            if (target == null) return;
            
            _currentTarget = target;
            
            Vector2 enemyPos = target.GlobalPosition;
            float dist = _petNode.GlobalPosition.DistanceTo(enemyPos);
            
            // 接近到攻击范围
            if (dist > _attackRange)
            {
                MoveTowards(enemyPos, delta, _followSpeed);
            }
            else
            {
                // 进入攻击范围，切换到攻击状态
                ChangeState(PetAIState.Attacking);
            }
        }

        /// <summary>
        /// 攻击敌人
        /// </summary>
        private void AttackEnemy(float delta)
        {
            var enemies = DetectNearbyEnemies();
            
            // 目标死亡或丢失，切换到跟随
            if (_currentTarget == null || !IsInstanceValid(_currentTarget) || !enemies.Contains(_currentTarget))
            {
                _currentTarget = enemies.Count > 0 ? GetClosestEnemy(enemies) : null;
                if (_currentTarget == null)
                {
                    ChangeState(PetAIState.Following);
                    return;
                }
            }
            
            Vector2 enemyPos = _currentTarget.GlobalPosition;
            float dist = _petNode.GlobalPosition.DistanceTo(enemyPos);
            
            // 超出攻击范围，切换到接近
            if (dist > _attackRange * 1.5f)
            {
                ChangeState(PetAIState.Engaging);
                return;
            }
            
            // 攻击冷却检查
            double currentTime = Time.GetTicksMsec() / 1000.0;
            if (currentTime - _lastAttackTime < _attackCooldown)
                return;
            
            // 执行攻击
            PerformAttack();
        }

        /// <summary>
        /// 执行一次攻击
        /// </summary>
        private void PerformAttack()
        {
            if (_currentTarget == null || !IsInstanceValid(_currentTarget))
                return;
            
            // 计算伤害（基于宠物属性）
            int baseDamage = _activePet != null ? _activePet.GetTotalAttackBonus() : 5;
            int damage = Mathf.Max(1, baseDamage);
            
            // 对敌人造成伤害
            if (_currentTarget is Enemy enemy)
            {
                enemy.TakeDamage(damage);
                _lastAttackTime = Time.GetTicksMsec() / 1000.0;
                
                GD.Print($"[PetCombatAI] {_activePet?.PetName ?? "Pet"} attacks {_currentTarget.Name} for {damage}");
                EmitSignal(SignalName.PetAttacked, _currentTarget, damage);
            }
        }
        
        /// <summary>
        /// 跟随玩家
        /// </summary>
        private void FollowPlayer(float delta)
        {
            if (_player == null || _petNode == null)
                return;
            
            Vector2 targetPos = _player.GlobalPosition;
            float dist = _petNode.GlobalPosition.DistanceTo(targetPos);
            
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
            if (_player == null || _petNode == null)
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
        /// 移动到目标（使用宠物节点）
        /// </summary>
        private void MoveTowards(Vector2 targetPos, float delta, float speed)
        {
            if (_petNode == null) return;
            
            Vector2 direction = (targetPos - _petNode.GlobalPosition).Normalized();
            _petNode.GlobalPosition += direction * speed * delta;
        }
        
        /// <summary>
        /// 检测附近敌人（使用 Godot 敌人组）
        /// </summary>
        private List<Node2D> DetectNearbyEnemies()
        {
            var enemies = GetTree().GetNodesInGroup("enemy");
            var result = new List<Node2D>();
            
            foreach (var node in enemies)
            {
                if (node is Node2D enemy)
                {
                    // 过滤死亡或无效节点
                    if (!IsInstanceValid(enemy)) continue;
                    
                    // 检查距离（使用攻击范围的 2 倍作为检测范围）
                    if (_petNode != null)
                    {
                        float dist = _petNode.GlobalPosition.DistanceTo(enemy.GlobalPosition);
                        if (dist < _attackRange * 2.5f)
                        {
                            result.Add(enemy);
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 获取最近的敌人
        /// </summary>
        private Node2D GetClosestEnemy(List<Node2D> enemies)
        {
            if (_petNode == null || enemies.Count == 0) return null;
            
            Node2D closest = null;
            float minDist = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                if (!IsInstanceValid(enemy)) continue;
                
                float dist = _petNode.GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy;
                }
            }
            
            return closest;
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
        
        /// <summary>
        /// 获取宠物当前锁定的攻击目标
        /// </summary>
        public Node2D GetCurrentTarget() => _currentTarget;

        /// <summary>
        /// 获取宠物场景节点（用于屏幕坐标转换）
        /// </summary>
        public Node2D GetPetNode() => _petNode;

        /// <summary>
        /// 获取宠物周围所有可攻击的敌人
        /// </summary>
        public List<Node2D> GetNearbyEnemies()
        {
            return DetectNearbyEnemies();
        }
        
        /// <summary>
        /// 对目标敌人造成宠物战斗伤害（由 PetCombatCompanionSystem 调用 combo 伤害）
        /// </summary>
        public void ApplyComboDamageToTarget(Node2D target, float damage)
        {
            if (target == null || !IsInstanceValid(target))
                return;
            
            if (target is Enemy enemy)
            {
                enemy.TakeDamage((int)damage);
                GD.Print($"[PetCombatAI] Combo damage applied: {target.Name} took {damage} damage");
            }
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
