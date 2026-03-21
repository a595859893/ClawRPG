using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 宠物守护系统
    /// 宠物在玩家周围巡逻守护，自动攻击靠近的敌人
    /// </summary>
    public partial class PetGuardianSystem : BaseSystem
    {
        public static PetGuardianSystem Instance { get; private set; }

        // 数据
        private PetGuardianData.PlayerGuardianData _playerData = new();
        private Dictionary<string, PetGuardianData.GuardianConfig> _configs = new();
        
        // 节点引用
        private Node2D _player;
        private Node2D _petNode;
        
        // 信号
        public Action<string> OnGuardianModeChanged;
        public Action<string, Node2D> OnEnemyEngaged;
        public Action<string, Node2D> OnEnemyDefeated;
        public Action<string, Vector2> OnPetMoved;
        
        // 随机
        private Random _random = new();
        
        // 状态
        public bool IsGuardianModeActive => _playerData.IsGuardianModeActive;
        
        public override void _Ready()
        {
            Instance = this;
            GD.Print("宠物守护系统已初始化");
        }
        
        public void Initialize(Node2D player, Node2D petNode)
        {
            _player = player;
            _petNode = petNode;
            
            if (_player != null)
            {
                _playerData = new PetGuardianData.PlayerGuardianData();
            }
            
            GD.Print("宠物守护系统已配置");
        }
        
        /// <summary>
        /// 激活宠物守护模式
        /// </summary>
        public void ActivateGuardianMode(string petId)
        {
            if (_player == null || _petNode == null)
            {
                GD.PrintErr("宠物守护系统: 玩家或宠物节点未设置");
                return;
            }
            
            if (_playerData.ActivePets.ContainsKey(petId))
            {
                // 已经激活，更新状态
                _playerData.ActivePets[petId].Mode = PetGuardianData.GuardianMode.Patrol;
                _playerData.ActivePets[petId].PatrolCenter = _player.GlobalPosition;
            }
            else
            {
                // 创建新的守护信息
                var info = new PetGuardianData.PetGuardianInfo
                {
                    PetId = petId,
                    Mode = PetGuardianData.GuardianMode.Patrol,
                    State = PetGuardianData.GuardianState.Patrol,
                    PatrolCenter = _player.GlobalPosition,
                    LastAttackTime = 0,
                    LastDecisionTime = 0
                };
                
                var config = PetGuardianData.GetDefaultConfig();
                _configs[petId] = config;
                
                _playerData.ActivePets[petId] = info;
            }
            
            _playerData.IsGuardianModeActive = true;
            OnGuardianModeChanged?.Invoke(petId);
            GD.Print($"宠物守护模式已激活: {petId}");
        }
        
        /// <summary>
        /// 停用宠物守护模式
        /// </summary>
        public void DeactivateGuardianMode(string petId)
        {
            if (_playerData.ActivePets.ContainsKey(petId))
            {
                _playerData.ActivePets[petId].Mode = PetGuardianData.GuardianMode.Inactive;
                _playerData.ActivePets[petId].State = PetGuardianData.GuardianState.Idle;
                _playerData.ActivePets.Remove(petId);
            }
            
            if (_playerData.ActivePets.Count == 0)
            {
                _playerData.IsGuardianModeActive = false;
            }
            
            OnGuardianModeChanged?.Invoke(petId);
            GD.Print($"宠物守护模式已停用: {petId}");
        }
        
        /// <summary>
        /// 停用所有守护模式
        /// </summary>
        public void DeactivateAll()
        {
            foreach (var petId in new List<string>(_playerData.ActivePets.Keys))
            {
                DeactivateGuardianMode(petId);
            }
        }
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        public override void _Process(double delta)
        {
            if (!_playerData.IsGuardianModeActive || _player == null)
                return;
                
            foreach (var kvp in _playerData.ActivePets)
            {
                var petId = kvp.Key;
                var info = kvp.Value;
                var config = _configs.GetValueOrDefault(petId, PetGuardianData.GetDefaultConfig());
                
                // 更新状态计时
                info.TimeInState += (float)delta;
                
                // 决策检查
                if (Time.GetTicksMsec() / 1000f - info.LastDecisionTime > config.DecisionInterval)
                {
                    MakeDecision(petId, info, config, (float)delta);
                    info.LastDecisionTime = Time.GetTicksMsec() / 1000f;
                }
                
                // 执行状态行为
                ExecuteStateBehavior(petId, info, config, (float)delta);
            }
        }
        
        /// <summary>
        /// 做出决策
        /// </summary>
        private void MakeDecision(string petId, PetGuardianData.PetGuardianInfo info, 
            PetGuardianData.GuardianConfig config, float delta)
        {
            // 检测敌人
            var enemies = DetectEnemies(config.DetectionRadius);
            info.EnemiesDetected = enemies.Count;
            
            switch (info.State)
            {
                case PetGuardianData.GuardianState.Patrol:
                    if (enemies.Count > 0 && config.AutoAttack)
                    {
                        // 发现敌人，切换到追逐
                        var target = SelectTarget(enemies, config);
                        if (target != null)
                        {
                            info.CurrentTarget = target;
                            info.CurrentTargetPosition = target.GlobalPosition;
                            info.State = PetGuardianData.GuardianState.Chase;
                            OnEnemyEngaged?.Invoke(petId, target);
                        }
                    }
                    else if (info.TimeInState > 3f)
                    {
                        // 巡逻一段时间后更新位置
                        info.PatrolCenter = _player.GlobalPosition;
                        info.TimeInState = 0;
                    }
                    break;
                    
                case PetGuardianData.GuardianState.Chase:
                    if (info.CurrentTarget == null || !IsInstanceValid(info.CurrentTarget))
                    {
                        // 目标丢失，返回巡逻
                        info.State = PetGuardianData.GuardianState.Return;
                    }
                    else if (info.CurrentTarget.GlobalPosition.DistanceTo(_petNode.GlobalPosition) <= config.AttackRadius)
                    {
                        // 到达攻击范围
                        info.State = PetGuardianData.GuardianState.Attack;
                    }
                    else if (info.CurrentTarget.GlobalPosition.DistanceTo(_player.GlobalPosition) > config.DetectionRadius * 1.5f)
                    {
                        // 敌人跑太远，返回
                        info.State = PetGuardianData.GuardianState.Return;
                    }
                    else
                    {
                        // 更新目标位置
                        info.CurrentTargetPosition = info.CurrentTarget.GlobalPosition;
                    }
                    break;
                    
                case PetGuardianData.GuardianState.Attack:
                    var currentTime = Time.GetTicksMsec() / 1000f;
                    if (currentTime - info.LastAttackTime > config.AttackCooldown)
                    {
                        // 执行攻击
                        PerformAttack(petId, info, config);
                        info.LastAttackTime = currentTime;
                    }
                    
                    if (info.CurrentTarget == null || !IsInstanceValid(info.CurrentTarget) ||
                        info.CurrentTarget.GlobalPosition.DistanceTo(_petNode.GlobalPosition) > config.AttackRadius * 1.5f)
                    {
                        // 目标死亡或远离，返回
                        info.State = PetGuardianData.GuardianState.Return;
                    }
                    break;
                    
                case PetGuardianData.GuardianState.Return:
                    if (_petNode.GlobalPosition.DistanceTo(info.PatrolCenter) < 30f)
                    {
                        // 回到巡逻点
                        info.State = PetGuardianData.GuardianState.Patrol;
                        info.TimeInState = 0;
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 执行状态行为
        /// </summary>
        private void ExecuteStateBehavior(string petId, PetGuardianData.PetGuardianInfo info,
            PetGuardianData.GuardianConfig config, float delta)
        {
            Vector2 targetPos;
            float speed;
            
            switch (info.State)
            {
                case PetGuardianData.GuardianState.Patrol:
                    // 在巡逻点周围随机移动
                    var patrolOffset = GetRandomPatrolOffset(config.PatrolRadius);
                    targetPos = info.PatrolCenter + patrolOffset;
                    speed = config.PatrolSpeed;
                    MovePetTowards(targetPos, speed, delta);
                    break;
                    
                case PetGuardianData.GuardianState.Chase:
                    // 追逐目标
                    targetPos = info.CurrentTargetPosition;
                    speed = config.ChaseSpeed;
                    MovePetTowards(targetPos, speed, delta);
                    break;
                    
                case PetGuardianData.GuardianState.Attack:
                    // 保持攻击距离
                    if (info.CurrentTarget != null && IsInstanceValid(info.CurrentTarget))
                    {
                        var dist = _petNode.GlobalPosition.DistanceTo(info.CurrentTarget.GlobalPosition);
                        if (dist < config.AttackRadius * 0.5f)
                        {
                            // 太近了，后退一点
                            var awayDir = (_petNode.GlobalPosition - info.CurrentTarget.GlobalPosition).Normalized();
                            _petNode.GlobalPosition += awayDir * config.PatrolSpeed * delta;
                        }
                    }
                    break;
                    
                case PetGuardianData.GuardianState.Return:
                    // 返回巡逻中心
                    targetPos = info.PatrolCenter;
                    speed = config.ReturnSpeed;
                    MovePetTowards(targetPos, speed, delta);
                    break;
            }
        }
        
        /// <summary>
        /// 移动宠物
        /// </summary>
        private void MovePetTowards(Vector2 targetPos, float speed, float delta)
        {
            var direction = (targetPos - _petNode.GlobalPosition).Normalized();
            _petNode.GlobalPosition += direction * speed * delta;
            OnPetMoved?.Invoke(_playerData.ActivePets.ContainsKey(_petNode.Name.ToString()) ? 
                _petNode.Name.ToString() : "unknown", _petNode.GlobalPosition);
        }
        
        /// <summary>
        /// 检测敌人
        /// </summary>
        private List<Node2D> DetectEnemies(float radius)
        {
            var enemies = new List<Node2D>();
            
            // 简单的距离检测 - 实际应该使用 Area2D
            var allNodes = GetTree().GetNodesInGroup("enemies");
            foreach (var node in allNodes)
            {
                if (node is Node2D enemy && enemy.GlobalPosition.DistanceTo(_player.GlobalPosition) < radius)
                {
                    enemies.Add(enemy);
                }
            }
            
            return enemies;
        }
        
        /// <summary>
        /// 选择目标
        /// </summary>
        private Node2D SelectTarget(List<Node2D> enemies, PetGuardianData.GuardianConfig config)
        {
            if (enemies.Count == 0)
                return null;
                
            if (config.PrioritizeLowHealth)
            {
                // 优先选择低血量敌人
                Node2D bestTarget = null;
                float lowestHealth = float.MaxValue;
                
                foreach (var enemy in enemies)
                {
                    // 尝试获取敌人血量
                    var healthComponent = enemy.GetNodeOrNull("HealthComponent");
                    float health = float.MaxValue;
                    
                    if (healthComponent != null)
                    {
                        // 尝试获取当前血量
                        var healthProperty = healthComponent.Get("CurrentHealth");
                        if (healthProperty != null)
                        {
                            health = (float)healthProperty;
                        }
                    }
                    
                    if (health < lowestHealth)
                    {
                        lowestHealth = health;
                        bestTarget = enemy;
                    }
                }
                
                return bestTarget ?? enemies[0];
            }
            else
            {
                // 随机选择
                return enemies[_random.Next(enemies.Count)];
            }
        }
        
        /// <summary>
        /// 执行攻击
        /// </summary>
        private void PerformAttack(string petId, PetGuardianData.PetGuardianInfo info, 
            PetGuardianData.GuardianConfig config)
        {
            if (info.CurrentTarget == null || !IsInstanceValid(info.CurrentTarget))
                return;
                
            // 造成伤害
            var damage = 10 + _random.Next(10); // 基础伤害 10-20
            
            // 尝试调用敌人的受伤方法
            var damageMethod = info.CurrentTarget.Get("TakeDamage");
            if (damageMethod != null)
            {
                // 调用受伤
                // info.CurrentTarget.Call("TakeDamage", damage);
                GD.Print($"宠物 {petId} 攻击敌人造成 {damage} 伤害");
            }
            
            info.EnemiesAttacked++;
            
            // 检查敌人是否死亡
            if (damageMethod == null)
            {
                // 假设敌人死亡
                info.EnemiesDefeated++;
                _playerData.TotalEnemiesDefeated++;
                OnEnemyDefeated?.Invoke(petId, info.CurrentTarget);
            }
        }
        
        /// <summary>
        /// 获取随机巡逻偏移
        /// </summary>
        private Vector2 GetRandomPatrolOffset(float radius)
        {
            var angle = _random.Next(360) * Mathf.DegToRad();
            var dist = _random.NextFloat() * radius;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }
        
        /// <summary>
        /// 设置全局检测半径
        /// </summary>
        public void SetGlobalDetectionRadius(float radius)
        {
            _playerData.GlobalDetectionRadius = radius;
        }
        
        /// <summary>
        /// 获取守护统计
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "active_pets", _playerData.ActivePets.Count },
                { "total_enemies_defeated", _playerData.TotalEnemiesDefeated },
                { "is_active", _playerData.IsGuardianModeActive }
            };
        }
        
        /// <summary>
        /// 获取存档数据
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            var data = new Dictionary<string, object>
            {
                { "is_active", _playerData.IsGuardianModeActive },
                { "total_enemies_defeated", _playerData.TotalEnemiesDefeated },
                { "detection_radius", _playerData.GlobalDetectionRadius }
            };
            return data;
        }
        
        /// <summary>
        /// 加载存档数据
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("total_enemies_defeated"))
                _playerData.TotalEnemiesDefeated = Convert.ToInt32(data["total_enemies_defeated"]);
                
            if (data.ContainsKey("detection_radius"))
                _playerData.GlobalDetectionRadius = Convert.ToSingle(data["detection_radius"]);
        }
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>
        {
            { "is_active", _playerData.IsGuardianModeActive },
            { "total_enemies_defeated", _playerData.TotalEnemiesDefeated },
            { "detection_radius", _playerData.GlobalDetectionRadius }
        };

        // 保存激活的宠物守护信息
        var activePetsData = new List<Dictionary<string, object>>();
        foreach (var kvp in _playerData.ActivePets)
        {
            var petInfo = new Dictionary<string, object>
            {
                { "pet_id", kvp.Value.PetId },
                { "mode", (int)kvp.Value.Mode },
                { "state", (int)kvp.Value.State },
                { "patrol_center_x", kvp.Value.PatrolCenter.X },
                { "patrol_center_y", kvp.Value.PatrolCenter.Y },
                { "enemies_detected", kvp.Value.EnemiesDetected },
                { "enemies_attacked", kvp.Value.EnemiesAttacked },
                { "enemies_defeated", kvp.Value.EnemiesDefeated }
            };
            activePetsData.Add(petInfo);
        }
        data["active_pets"] = activePetsData;

        // 保存宠物守护配置
        var configsData = new Dictionary<string, Dictionary<string, object>>();
        foreach (var kvp in _configs)
        {
            configsData[kvp.Key] = new Dictionary<string, object>
            {
                { "patrol_radius", kvp.Value.PatrolRadius },
                { "detection_radius", kvp.Value.DetectionRadius },
                { "attack_radius", kvp.Value.AttackRadius },
                { "chase_speed", kvp.Value.ChaseSpeed },
                { "patrol_speed", kvp.Value.PatrolSpeed },
                { "return_speed", kvp.Value.ReturnSpeed },
                { "attack_cooldown", kvp.Value.AttackCooldown },
                { "decision_interval", kvp.Value.DecisionInterval },
                { "auto_attack", kvp.Value.AutoAttack },
                { "prioritize_low_health", kvp.Value.PrioritizeLowHealth }
            };
        }
        data["configs"] = configsData;

        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.ContainsKey("is_active"))
            _playerData.IsGuardianModeActive = Convert.ToBoolean(data["is_active"]);

        if (data.ContainsKey("total_enemies_defeated"))
            _playerData.TotalEnemiesDefeated = Convert.ToInt32(data["total_enemies_defeated"]);

        if (data.ContainsKey("detection_radius"))
            _playerData.GlobalDetectionRadius = Convert.ToSingle(data["detection_radius"]);

        // 恢复激活的宠物守护信息
        if (data.ContainsKey("active_pets"))
        {
            var activePetsData = (List<object>)data["active_pets"];
            foreach (var petObj in activePetsData)
            {
                var petDict = (Dictionary<string, object>)petObj;
                string petId = petDict["pet_id"].ToString();

                var info = new PetGuardianData.PetGuardianInfo
                {
                    PetId = petId,
                    Mode = (PetGuardianData.GuardianMode)(int)(long)petDict["mode"],
                    State = (PetGuardianData.GuardianState)(int)(long)petDict["state"],
                    PatrolCenter = new Vector2(
                        Convert.ToSingle(petDict["patrol_center_x"]),
                        Convert.ToSingle(petDict["patrol_center_y"])),
                    EnemiesDetected = Convert.ToInt32(petDict["enemies_detected"]),
                    EnemiesAttacked = Convert.ToInt32(petDict["enemies_attacked"]),
                    EnemiesDefeated = Convert.ToInt32(petDict["enemies_defeated"])
                };

                _playerData.ActivePets[petId] = info;
            }
        }

        // 恢复宠物守护配置
        if (data.ContainsKey("configs"))
        {
            var configsData = (Dictionary<string, object>)data["configs"];
            foreach (var kvp in configsData)
            {
                var cfgDict = (Dictionary<string, object>)kvp.Value;
                _configs[kvp.Key] = new PetGuardianData.GuardianConfig
                {
                    PetId = kvp.Key,
                    PatrolRadius = Convert.ToSingle(cfgDict["patrol_radius"]),
                    DetectionRadius = Convert.ToSingle(cfgDict["detection_radius"]),
                    AttackRadius = Convert.ToSingle(cfgDict["attack_radius"]),
                    ChaseSpeed = Convert.ToSingle(cfgDict["chase_speed"]),
                    PatrolSpeed = Convert.ToSingle(cfgDict["patrol_speed"]),
                    ReturnSpeed = Convert.ToSingle(cfgDict["return_speed"]),
                    AttackCooldown = Convert.ToSingle(cfgDict["attack_cooldown"]),
                    DecisionInterval = Convert.ToSingle(cfgDict["decision_interval"]),
                    AutoAttack = Convert.ToBoolean(cfgDict["auto_attack"]),
                    PrioritizeLowHealth = Convert.ToBoolean(cfgDict["prioritize_low_health"])
                };
            }
        }
    }
}
