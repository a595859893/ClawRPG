using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物战斗AI - 宠物在战斗中自动攻击敌人
    /// </summary>
    public class PetCombatAI : Node
    {
        // 单例
        private static PetCombatAI _instance;
        public static PetCombatAI Instance => _instance ??= new PetCombatAI();

        // 宠物战斗属性
        private Pet _activePet;
        private CharacterBody2D _player;
        
        // AI配置
        private float _followDistance = 80f;      // 跟随距离
        private float _attackRange = 100f;       // 攻击范围
        private float _followSpeed = 150f;       // 跟随速度
        private float _attackCooldown = 1.5f;    // 攻击冷却
        private float _lastAttackTime = 0f;
        
        // 状态
        private enum PetAIState { Idle, Following, Attacking, Returning }
        private PetAIState _currentState = PetAIState.Idle;
        
        // 敌人检测
        private Area2D _detectionArea;
        private Area2D _attackArea;
        
        // 视觉
        private Sprite2D _petSprite;
        private Label _petNameLabel;
        
        // 信号
        public Action OnPetAttack;
        public Action<string> OnPetSpecialAbility;

        public bool IsEnabled { get; set; } = true;
        public bool IsVisible { get; set; } = true;

        public void Initialize()
        {
            _instance = this;
            _player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;
            
            if (_player != null)
            {
                CreatePetVisuals();
            }
            
            GD.Print("宠物战斗AI已初始化");
        }

        /// <summary>
        /// 设置激活的宠物
        /// </summary>
        public void SetActivePet(Pet pet)
        {
            _activePet = pet;
            if (pet != null && _petNameLabel != null)
            {
                _petNameLabel.Text = pet.PetName;
            }
        }

        private void CreatePetVisuals()
        {
            // 创建宠物精灵
            _petSprite = new Sprite2D();
            _petSprite.Name = "PetSprite";
            _petSprite.Visible = IsVisible;
            
            // 使用占位纹理
            var placeholderTexture = CreatePlaceholderTexture();
            _petSprite.Texture = placeholderTexture;
            
            // 创建名称标签
            _petNameLabel = new Label();
            _petNameLabel.Name = "PetName";
            _petNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _petNameLabel.Position = new Vector2(-30, -40);
            _petNameLabel.Visible = IsVisible;
            
            if (_activePet != null)
            {
                _petNameLabel.Text = _activePet.PetName;
            }
            
            // 创建检测区域
            _detectionArea = new Area2D();
            _detectionArea.Name = "DetectionArea";
            var detectionShape = new CollisionShape2D();
            detectionShape.Shape = new CircleShape2D { Radius = 200f };
            _detectionArea.AddChild(detectionShape);
            
            _attackArea = new Area2D();
            _attackArea.Name = "AttackArea";
            var attackShape = new CollisionShape2D();
            attackShape.Shape = new CircleShape2D { Radius = 30f };
            _attackArea.AddChild(attackShape);
            
            // 添加节点
            AddChild(_petSprite);
            AddChild(_petNameLabel);
            AddChild(_detectionArea);
            AddChild(_attackArea);
            
            // 连接敌人检测信号
            _detectionArea.AreaEntered += OnEnemyDetected;
        }

        private Texture2D CreatePlaceholderTexture()
        {
            // 创建简单占位纹理
            var image = new Image(32, 32, Image.Format.Rgba8);
            image.Fill(new Color(0.3f, 0.7f, 0.3f, 1f)); // 绿色宠物
            return ImageTexture.CreateFromImage(image);
        }

        private void OnEnemyDetected(Area2D area)
        {
            // 检测到敌人
        }

        public override void _PhysicsProcess(float delta)
        {
            if (!IsEnabled || _activePet == null || _player == null) return;

            // 更新宠物位置跟随玩家
            var playerPos = _player.GlobalPosition;
            
            // 查找最近的敌人
            var nearestEnemy = FindNearestEnemy();
            
            if (nearestEnemy != null)
            {
                float distanceToEnemy = playerPos.DistanceTo(nearestEnemy.GlobalPosition);
                
                if (distanceToEnemy <= _attackRange)
                {
                    // 在攻击范围内，攻击敌人
                    AttackEnemy(nearestEnemy, delta);
                }
                else if (distanceToEnemy <= 300f)
                {
                    // 在检测范围内，靠近敌人
                    MoveTowards(nearestEnemy.GlobalPosition, delta);
                }
                else
                {
                    // 跟随玩家
                    FollowPlayer(delta);
                }
            }
            else
            {
                // 没有敌人，跟随玩家
                FollowPlayer(delta);
            }
            
            // 应用特殊能力
            ProcessSpecialAbility(delta);
        }

        private void FollowPlayer(float delta)
        {
            if (_player == null) return;
            
            var playerPos = _player.GlobalPosition;
            var followPos = playerPos + new Vector2(0, -_followDistance);
            
            MoveTowards(followPos, delta);
        }

        private void MoveTowards(Vector2 targetPos, float delta)
        {
            var currentPos = GlobalPosition;
            var direction = (targetPos - currentPos).Normalized();
            var distance = currentPos.DistanceTo(targetPos);
            
            if (distance > 10f)
            {
                GlobalPosition += direction * _followSpeed * delta;
                
                // 翻转精灵方向
                if (_petSprite != null)
                {
                    _petSprite.FlipH = direction.x < 0;
                }
            }
        }

        private Node2D FindNearestEnemy()
        {
            if (_detectionArea == null) return null;
            
            var bodies = _detectionArea.GetOverlappingAreas();
            Node2D nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (var body in bodies)
            {
                if (body is Area2D area)
                {
                    var enemies = area.GetParent();
                    if (enemies != null && enemies.IsInGroup("enemy"))
                    {
                        float dist = GlobalPosition.DistanceTo(enemies.GlobalPosition);
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            nearest = enemies as Node2D;
                        }
                    }
                }
            }
            
            return nearest;
        }

        private void AttackEnemy(Node2D enemy, float delta)
        {
            if (enemy == null) return;
            
            // 转向敌人
            var direction = (enemy.GlobalPosition - GlobalPosition).Normalized();
            if (_petSprite != null)
            {
                _petSprite.FlipH = direction.x < 0;
            }
            
            // 检查攻击冷却
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            if (currentTime - _lastAttackTime >= _attackCooldown)
            {
                _lastAttackTime = currentTime;
                
                // 计算宠物攻击力
                int petAttack = _activePet != null ? _activePet.GetTotalAttackBonus() : 0;
                float damageMultiplier = 0.5f + (_activePet?.Level ?? 1) * 0.1f;
                int finalDamage = (int)(petAttack * damageMultiplier);
                
                // 对敌人造成伤害
                var enemyChar = enemy as CharacterBody2D;
                if (enemyChar != null)
                {
                    // 尝试调用敌人的受伤方法
                    enemyChar.CallDeferred("TakeDamage", finalDamage);
                    
                    // 击退效果
                    var knockbackDir = (enemy.GlobalPosition - GlobalPosition).Normalized();
                    enemyChar.Velocity = knockbackDir * 100f;
                }
                
                // 播放攻击特效
                ShowAttackEffect(enemy.GlobalPosition);
                
                OnPetAttack?.Invoke();
                
                GD.Print($"宠物 {_activePet?.PetName ?? "Unknown"} 攻击敌人造成 {finalDamage} 伤害");
            }
        }

        private void ShowAttackEffect(Vector2 targetPos)
        {
            // 创建简单的攻击特效
            var effect = new Sprite2D();
            effect.Position = targetPos - GlobalPosition;
            
            var tex = CreateAttackTexture();
            effect.Texture = tex;
            
            AddChild(effect);
            
            // 0.3秒后移除
            var timer = new Timer();
            timer.WaitTime = 0.3f;
            timer.OneShot = true;
            timer.Autostart = true;
            timer.Timeout += () => effect.QueueFree();
            AddChild(timer);
        }

        private Texture2D CreateAttackTexture()
        {
            var image = new Image(16, 16, Image.Format.Rgba8);
            image.Fill(new Color(1f, 1f, 0f, 0.8f)); // 黄色闪光
            return ImageTexture.CreateFromImage(image);
        }

        private void ProcessSpecialAbility(float delta)
        {
            if (_activePet == null) return;
            
            // 处理特殊能力
            switch (_activePet.SpecialAbility)
            {
                case "auto_pickup":
                    // 自动拾取已在物品系统中实现
                    break;
                    
                case "exp_boost":
                    // 经验加成 - 在Player获取经验时应用
                    break;
                    
                case "drop_boost":
                    // 掉落加成 - 在敌人死亡时应用
                    break;
                    
                case "damage_reduction":
                    // 伤害减免 - 玩家受伤时应用
                    break;
                    
                case "shield":
                    // 护盾 - 周期性给玩家添加护盾
                    break;
                    
                case "fire_breath":
                    // 火焰吐息 - 范围攻击
                    PerformFireBreath();
                    break;
                    
                case "resurrect":
                    // 复活 - 玩家死亡时触发
                    break;
                    
                case "all_stats":
                    // 全属性加成 - 已通过属性系统实现
                    break;
                    
                case "holy_protection":
                    // 神圣保护 - 周期性圣光护盾
                    PerformHolyProtection(delta);
                    break;
                    
                case "lucky":
                    // 幸运 - 暴击率加成
                    break;
            }
        }

        private float _fireBreathCooldown = 0f;
        private float _fireBreathInterval = 5f;

        private void PerformFireBreath()
        {
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            if (currentTime - _fireBreathCooldown < _fireBreathInterval) return;
            
            _fireBreathCooldown = currentTime;
            
            // 对范围内敌人造成火焰伤害
            if (_detectionArea != null)
            {
                var bodies = _detectionArea.GetOverlappingAreas();
                foreach (var body in bodies)
                {
                    var enemies = body?.GetParent();
                    if (enemies?.IsInGroup("enemy") == true)
                    {
                        var enemyChar = enemies as CharacterBody2D;
                        if (enemyChar != null)
                        {
                            int fireDamage = (_activePet?.GetTotalAttackBonus() ?? 10) * 2;
                            enemyChar.CallDeferred("TakeDamage", fireDamage);
                            
                            // 施加燃烧效果
                            enemyChar.CallDeferred("ApplyStatusEffect", "burning", fireDamage, 3f);
                        }
                    }
                }
                
                // 特效
                CreateFireBreathEffect();
                
                OnPetSpecialAbility?.Invoke("fire_breath");
            }
        }

        private void CreateFireBreathEffect()
        {
            var particles = new GPUParticles2D();
            particles.Amount = 20;
            particles.Lifetime = 0.5f;
            
            var processMat = new ParticleProcessMaterial();
            processMat.Direction = new Vector3(1, 0, 0);
            processMat.Spread = 30f;
            processMat.InitialVelocityMin = 100f;
            processMat.InitialVelocityMax = 200f;
            processMat.Color = new Color(1f, 0.5f, 0f, 1f);
            
            particles.ProcessMaterial = processMat;
            particles.Position = Vector2.Zero;
            
            // 翻转方向
            if (_petSprite?.FlipH == true)
            {
                particles.Rotation = Mathf.Pi;
            }
            
            AddChild(particles);
            
            var timer = new Timer();
            timer.WaitTime = 0.6f;
            timer.OneShot = true;
            timer.Autostart = true;
            timer.Timeout += () => particles.QueueFree();
            AddChild(timer);
        }

        private float _holyProtectionTimer = 0f;
        private float _holyProtectionInterval = 10f;

        private void PerformHolyProtection(float delta)
        {
            _holyProtectionTimer += delta;
            
            if (_holyProtectionTimer >= _holyProtectionInterval)
            {
                _holyProtectionTimer = 0f;
                
                if (_player != null)
                {
                    // 给玩家添加神圣护盾
                    _player.CallDeferred("ApplyStatusEffect", "shield", 
                        (_activePet?.GetTotalHealthBonus() ?? 50) / 2, 5f);
                    
                    OnPetSpecialAbility?.Invoke("holy_protection");
                }
            }
        }

        /// <summary>
        /// 玩家受伤时宠物响应
        /// </summary>
        public void OnPlayerDamaged(int damage)
        {
            if (_activePet == null) return;
            
            // 忠诚度影响宠物反应
            if (_activePet.Loyalty >= 70)
            {
                // 高忠诚度：宠物攻击敌人
                var enemy = FindNearestEnemy();
                if (enemy != null)
                {
                    AttackEnemy(enemy, 0.016f);
                }
            }
            else if (_activePet.Loyalty < 30)
            {
                // 低忠诚度：宠物可能会逃跑（这里简单处理为不响应）
            }
        }

        /// <summary>
        /// 玩家死亡时宠物响应
        /// </summary>
        public void OnPlayerDeath()
        {
            if (_activePet?.SpecialAbility == "resurrect")
            {
                // 尝试复活玩家
                if (_player != null && _activePet.Loyalty >= 50)
                {
                    float resChance = 0.3f + (_activePet.Loyalty - 50) * 0.01f;
                    if (GD.Randf() < resChance)
                    {
                        _player.CallDeferred("Heal", (_activePet?.GetTotalHealthBonus() ?? 100) / 2);
                        _activePet.Loyalty = Mathf.Max(0, _activePet.Loyalty - 20);
                        
                        GD.Print($"宠物 {_activePet.PetName} 复活了玩家!");
                        OnPetSpecialAbility?.Invoke("resurrect");
                    }
                }
            }
        }

        /// <summary>
        /// 设置宠物可见性
        /// </summary>
        public void SetPetVisible(bool visible)
        {
            IsVisible = visible;
            if (_petSprite != null) _petSprite.Visible = visible;
            if (_petNameLabel != null) _petNameLabel.Visible = visible;
        }

        /// <summary>
        /// 获取宠物位置
        /// </summary>
        public Vector2 GetPetPosition()
        {
            return GlobalPosition;
        }
    }
}
