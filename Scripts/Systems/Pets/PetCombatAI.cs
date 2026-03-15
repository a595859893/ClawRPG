using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物战斗AI - 增强版
    /// - 战术行为：包围、侧翼、环绕
    /// - 智能目标选择：低血量、脆皮优先
    /// - 宠物性格系统：基于宠物类型的行为差异
    /// - 主动buff：主动为玩家提供增益
    /// - 队形跟随：更好的跟随位置
    /// </summary>
    public class PetCombatAI : BaseSystem
    {
        // 单例
        private static PetCombatAI _instance;
        public static PetCombatAI Instance => _instance ??= new PetCombatAI();

        // 宠物战斗属性
        private Pet _activePet;
        private CharacterBody2D _player;
        
        // AI配置 - 可根据宠物类型调整
        private float _followDistance = 80f;
        private float _attackRange = 100f;
        private float _followSpeed = 180f;
        private float _attackCooldown = 1.5f;
        private float _lastAttackTime = 0f;
        
        // 战术配置
        private float _tacticalDistance = 150f;    // 战术距离
        private float _flankAngle = 45f;           // 侧翼角度
        private float _circlespeed = 2f;           // 环绕速度
        
        // 状态机
        private enum PetAIState { Idle, Following, Engaging, Attacking, Retreating, Supporting }
        private PetAIState _currentState = PetAIState.Idle;
        private PetAIState _previousState = PetAIState.Idle;
        
        // 敌人检测
        private Area2D _detectionArea;
        private Area2D _attackArea;
        
        // 视觉
        private Sprite2D _petSprite;
        private Label _petNameLabel;
        
        // 信号
        public Action OnPetAttack;
        public Action<string> OnPetSpecialAbility;
        
        // 战术数据
        private Node2D _currentTarget;
        private Vector2 _tacticalPosition;
        private float _stateTimer = 0f;
        private float _supportCooldown = 0f;
        
        // 宠物性格参数
        private PetPersonality _personality = PetPersonality.Balanced;
        
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
            
            GD.Print("宠物战斗AI已初始化 (增强版)");
        }

        public void SetActivePet(Pet pet)
        {
            _activePet = pet;
            if (pet != null)
            {
                if (_petNameLabel != null)
                {
                    _petNameLabel.Text = pet.PetName;
                }
                // 根据宠物类型设置性格
                UpdatePersonality(pet.Type);
            }
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

        private void CreatePetVisuals()
        {
            _petSprite = new Sprite2D();
            _petSprite.Name = "PetSprite";
            _petSprite.Visible = IsVisible;
            
            var placeholderTexture = CreatePlaceholderTexture();
            _petSprite.Texture = placeholderTexture;
            
            _petNameLabel = new Label();
            _petNameLabel.Name = "PetName";
            _petNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _petNameLabel.Position = new Vector2(-30, -40);
            _petNameLabel.Visible = IsVisible;
            
            if (_activePet != null)
            {
                _petNameLabel.Text = _activePet.PetName;
            }
            
            _detectionArea = new Area2D();
            _detectionArea.Name = "DetectionArea";
            var detectionShape = new CollisionShape2D();
            detectionShape.Shape = new CircleShape2D { Radius = 250f };
            _detectionArea.AddChild(detectionShape);
            
            _attackArea = new Area2D();
            _attackArea.Name = "AttackArea";
            var attackShape = new CollisionShape2D();
            attackShape.Shape = new CircleShape2D { Radius = 30f };
            _attackArea.AddChild(attackShape);
            
            AddChild(_petSprite);
            AddChild(_petNameLabel);
            AddChild(_detectionArea);
            AddChild(_attackArea);
            
            _detectionArea.AreaEntered += OnEnemyDetected;
        }

        private Texture2D CreatePlaceholderTexture()
        {
            var image = new Image(32, 32, Image.Format.Rgba8);
            Color petColor = _activePet?.Rarity switch
            {
                PetRarity.Common => new Color(0.7f, 0.7f, 0.7f, 1f),
                PetRarity.Uncommon => new Color(0.3f, 0.8f, 0.3f, 1f),
                PetRarity.Rare => new Color(0.3f, 0.5f, 0.9f, 1f),
                PetRarity.Epic => new Color(0.6f, 0.3f, 0.8f, 1f),
                PetRarity.Legendary => new Color(1f, 0.6f, 0f, 1f),
                _ => new Color(0.3f, 0.7f, 0.3f, 1f)
            };
            image.Fill(petColor);
            return ImageTexture.CreateFromImage(image);
        }

        private void OnEnemyDetected(Area2D area) { }

        public override void _PhysicsProcess(float delta)
        {
            if (!IsEnabled || _activePet == null || _player == null) return;

            _stateTimer += delta;
            
            // 智能目标选择
            var newTarget = SelectSmartTarget();
            
            // 状态转换
            UpdateStateMachine(newTarget, delta);
            
            // 状态行为
            ExecuteStateBehavior(newTarget, delta);
            
            // 特殊能力处理
            ProcessSpecialAbility(newTarget, delta);
            
            // 更新视觉
            UpdateVisuals();
            
            _currentTarget = newTarget;
        }
        
        private Node2D SelectSmartTarget()
        {
            if (_detectionArea == null) return null;
            
            var bodies = _detectionArea.GetOverlappingAreas();
            if (bodies.Count == 0) return null;
            
            Node2D bestTarget = null;
            float bestScore = float.MaxValue;
            
            foreach (var body in bodies)
            {
                var enemy = body?.GetParent();
                if (enemy == null || !enemy.IsInGroup("enemy")) continue;
                
                float score = CalculateTargetScore(enemy);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = enemy as Node2D;
                }
            }
            
            return bestTarget;
        }
        
        /// <summary>
        /// 计算目标评分（越低越好）
        /// 考虑因素：距离、血量、威胁等级
        /// </summary>
        private float CalculateTargetScore(Node2D enemy)
        {
            float score = 0f;
            
            // 距离权重 (越近越好)
            float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            score += dist * 0.5f;
            
            // 血量权重 (低血量优先) - 需要从敌人获取HP
            if (enemy.HasMethod("GetCurrentHealth"))
            {
                int currentHp = (int)enemy.Call("GetCurrentHealth");
                int maxHp = (int)enemy.Call("GetMaxHealth");
                float hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 1f;
                score += hpPercent * 500f; // 低血量降低分数
            }
            
            // 性格影响
            switch (_personality)
            {
                case PetPersonality.Aggressive:
                    // 优先攻击玩家正在攻击的敌人
                    break;
                case PetPersonality.Cautious:
                    score += dist * 0.3f; // 更偏好远程
                    break;
                case PetPersonality.Defensive:
                    // 优先攻击靠近玩家的敌人
                    if (_player != null)
                    {
                        float enemyToPlayer = enemy.GlobalPosition.DistanceTo(_player.GlobalPosition);
                        score -= enemyToPlayer * 0.2f; // 靠近玩家的敌人分数更低
                    }
                    break;
            }
            
            return score;
        }
        
        private void UpdateStateMachine(Node2D target, float delta)
        {
            _previousState = _currentState;
            
            if (target == null)
            {
                _currentState = PetAIState.Following;
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
            
            switch (_personality)
            {
                case PetPersonality.Defensive:
                    // 守护型：玩家血量低时进入支援状态
                    if (playerLowHealth && _activePet.SpecialAbility != "")
                    {
                        _currentState = PetAIState.Supporting;
                        return;
                    }
                    break;
                    
                case PetPersonality.Aggressive:
                    // 攻击型：保持战斗
                    if (distToEnemy <= _attackRange)
                        _currentState = PetAIState.Attacking;
                    else if (distToEnemy <= _tacticalDistance)
                        _currentState = PetAIState.Engaging;
                    else
                        _currentState = PetAIState.Engaging;
                    return;
                    
                case PetPersonality.Cautious:
                    // 谨慎型：保持距离
                    if (distToEnemy < 80f)
                        _currentState = PetAIState.Retreating;
                    else if (distToEnemy <= _attackRange)
                        _currentState = PetAIState.Attacking;
                    else if (distToEnemy <= _tacticalDistance + 50f)
                        _currentState = PetAIState.Engaging;
                    else
                        _currentState = PetAIState.Following;
                    return;
                    
                default: // Balanced
                    if (distToEnemy <= _attackRange)
                        _currentState = PetAIState.Attacking;
                    else if (distToEnemy <= _tacticalDistance)
                        _currentState = PetAIState.Engaging;
                    else
                        _currentState = PetAIState.Engaging;
                    return;
            }
        }
        
        private void ExecuteStateBehavior(Node2D target, float delta)
        {
            switch (_currentState)
            {
                case PetAIState.Following:
                    FollowPlayer(delta);
                    break;
                    
                case PetAIState.Engaging:
                    if (target != null)
                        TacticalApproach(target, delta);
                    break;
                    
                case PetAIState.Attacking:
                    if (target != null)
                        AttackEnemy(target, delta);
                    break;
                    
                case PetAIState.Retreating:
                    if (target != null)
                        MaintainDistance(target, delta, retreat: true);
                    break;
                    
                case PetAIState.Supporting:
                    SupportPlayer(delta);
                    break;
            }
        }
        
        private void FollowPlayer(float delta)
        {
            if (_player == null) return;
            
            // 根据宠物类型选择跟随位置
            Vector2 offset = _activePet?.Type switch
            {
                PetType.Guardian => new Vector2(GD.Randf() * 60f - 30f, -30f),
                PetType.Collector => new Vector2(GD.Randf() * 80f - 40f, -60f),
                _ => new Vector2(GD.Randf() * 40f - 20f, -_followDistance)
            };
            
            // 平滑移动到目标位置
            var targetPos = _player.GlobalPosition + offset;
            MoveTowardsSmooth(targetPos, delta, _followSpeed * 0.8f);
        }
        
        /// <summary>
        /// 战术接近 - 使用侧翼或包围策略
        /// </summary>
        private void TacticalApproach(Node2D target, float delta)
        {
            if (_player == null) return;
            
            // 计算最佳战术位置
            Vector2 playerPos = _player.GlobalPosition;
            Vector2 enemyPos = target.GlobalPosition;
            
            // 获取玩家朝向
            int playerFacing = 1; // 默认朝右
            if (_player is CharacterBody2D cb && cb.Velocity.x != 0)
                playerFacing = cb.Velocity.x > 0 ? 1 : -1;
            
            // 侧翼位置：敌人侧后方
            Vector2 enemyDir = (enemyPos - playerPos).Normalized();
            Vector2 flankDir = new Vector2(-enemyDir.y, enemyDir.x);
            
            // 目标位置：在敌人侧翼，保持战术距离
            Vector2 tacticalTarget = enemyPos - enemyDir * _tacticalDistance * 0.5f + flankDir * playerFacing * _tacticalDistance * 0.3f;
            
            MoveTowardsSmooth(tacticalTarget, delta, _followSpeed);
        }
        
        /// <summary>
        /// 保持距离 - 攻击型保持近身，谨慎型保持距离
        /// </summary>
        private void MaintainDistance(Node2D target, float delta, bool retreat = false)
        {
            float currentDist = GlobalPosition.DistanceTo(target.GlobalPosition);
            float idealDist = _personality == PetPersonality.Cautious ? 150f : 80f;
            
            Vector2 direction;
            if (retreat && currentDist < idealDist)
                direction = (GlobalPosition - target.GlobalPosition).Normalized();
            else if (currentDist > idealDist + 20f)
                direction = (target.GlobalPosition - GlobalPosition).Normalized();
            else
                return; // 距离合适
            
            GlobalPosition += direction * _followSpeed * delta;
            
            if (_petSprite != null)
                _petSprite.FlipH = direction.x < 0;
        }
        
        /// <summary>
        /// 平滑移动到目标位置
        /// </summary>
        private void MoveTowardsSmooth(Vector2 targetPos, float delta, float speed)
        {
            var currentPos = GlobalPosition;
            var direction = (targetPos - currentPos).Normalized();
            var distance = currentPos.DistanceTo(targetPos);
            
            if (distance > 15f)
            {
                GlobalPosition += direction * speed * delta;
                
                if (_petSprite != null)
                    _petSprite.FlipH = direction.x < 0;
            }
        }
        
        private void MoveTowards(Vector2 targetPos, float delta)
        {
            MoveTowardsSmooth(targetPos, delta, _followSpeed);
        }

        private void AttackEnemy(Node2D enemy, float delta)
        {
            if (enemy == null) return;
            
            var direction = (enemy.GlobalPosition - GlobalPosition).Normalized();
            if (_petSprite != null)
                _petSprite.FlipH = direction.x < 0;
            
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            if (currentTime - _lastAttackTime >= _attackCooldown)
            {
                _lastAttackTime = currentTime;
                
                int petAttack = _activePet != null ? _activePet.GetTotalAttackBonus() : 0;
                float damageMultiplier = 0.5f + (_activePet?.Level ?? 1) * 0.1f;
                
                // 暴击计算
                bool isCrit = GD.Randf() * 100f < (_activePet?.GetTotalCriticalBonus() ?? 5);
                if (isCrit) damageMultiplier *= 1.5f;
                
                int finalDamage = (int)(petAttack * damageMultiplier);
                
                var enemyChar = enemy as CharacterBody2D;
                if (enemyChar != null)
                {
                    enemyChar.CallDeferred("TakeDamage", finalDamage);
                    
                    // 击退
                    var knockbackDir = (enemy.GlobalPosition - GlobalPosition).Normalized();
                    enemyChar.Velocity = knockbackDir * 120f;
                    
                    // 经验获取
                    if (_activePet != null)
                        _activePet.AddExperience(finalDamage / 10);
                }
                
                ShowAttackEffect(enemy.GlobalPosition, isCrit);
                OnPetAttack?.Invoke();
                
                string critText = isCrit ? " 暴击!" : "";
                GD.Print($"宠物 {_activePet?.PetName ?? "Unknown"} 攻击敌人造成 {finalDamage}{critText}");
            }
        }

        private void ShowAttackEffect(Vector2 targetPos, bool isCrit)
        {
            var effect = new Sprite2D();
            effect.Position = targetPos - GlobalPosition;
            
            var tex = CreateAttackTexture(isCrit);
            effect.Texture = tex;
            
            AddChild(effect);
            
            float duration = isCrit ? 0.5f : 0.3f;
            var timer = new Timer();
            timer.WaitTime = duration;
            timer.OneShot = true;
            timer.Autostart = true;
            timer.Timeout += () => effect.QueueFree();
            AddChild(timer);
        }

        private Texture2D CreateAttackTexture(bool isCrit)
        {
            var image = new Image(16, 16, Image.Format.Rgba8);
            Color color = isCrit ? new Color(1f, 0.8f, 0f, 1f) : new Color(1f, 1f, 0f, 0.8f);
            image.Fill(color);
            return ImageTexture.CreateFromImage(image);
        }
        
        /// <summary>
        /// 支援玩家 - 使用特殊能力
        /// </summary>
        private void SupportPlayer(float delta)
        {
            if (_player == null || _activePet == null) return;
            
            _supportCooldown += delta;
            if (_supportCooldown < 3f) return;
            _supportCooldown = 0f;
            
            // 根据特殊能力提供支援
            switch (_activePet.SpecialAbility)
            {
                case "heal":
                    // 治疗玩家
                    if (_player.HasMethod("Heal"))
                    {
                        int healAmount = _activePet.GetTotalAttackBonus();
                        _player.CallDeferred("Heal", healAmount);
                        OnPetSpecialAbility?.Invoke("heal");
                    }
                    break;
                    
                case "shield":
                    // 护盾
                    if (_player.HasMethod("ApplyStatusEffect"))
                    {
                        int shieldAmount = _activePet.GetTotalHealthBonus() / 2;
                        _player.CallDeferred("ApplyStatusEffect", "shield", shieldAmount, 5f);
                        OnPetSpecialAbility?.Invoke("shield");
                    }
                    break;
                    
                case "damage_reduction":
                    // 伤害减免
                    if (_player.HasMethod("ApplyStatusEffect"))
                    {
                        _player.CallDeferred("ApplyStatusEffect", "damage_reduction", 20, 10f);
                        OnPetSpecialAbility?.Invoke("damage_reduction");
                    }
                    break;
            }
            
            // 跟随玩家
            FollowPlayer(delta);
        }

        private void ProcessSpecialAbility(Node2D target, float delta)
        {
            if (_activePet == null) return;
            
            // 非守护型宠物的特殊能力
            if (_personality == PetPersonality.Defensive)
            {
                // 守护型特殊能力在SupportPlayer中处理
                return;
            }
            
            switch (_activePet.SpecialAbility)
            {
                case "fire_breath":
                    PerformFireBreath();
                    break;
                    
                case "holy_protection":
                    PerformHolyProtection(delta);
                    break;
                    
                case "resurrect":
                    // 在玩家死亡时处理
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
            
            if (_detectionArea == null) return;
            
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
                        enemyChar.CallDeferred("ApplyStatusEffect", "burning", fireDamage, 3f);
                    }
                }
            }
            
            CreateFireBreathEffect();
            OnPetSpecialAbility?.Invoke("fire_breath");
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
            
            if (_petSprite?.FlipH == true)
                particles.Rotation = Mathf.Pi;
            
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
                
                if (_player != null && _player.HasMethod("ApplyStatusEffect"))
                {
                    int shieldAmount = (_activePet?.GetTotalHealthBonus() ?? 50) / 2;
                    _player.CallDeferred("ApplyStatusEffect", "shield", shieldAmount, 5f);
                    OnPetSpecialAbility?.Invoke("holy_protection");
                }
            }
        }
        
        private void UpdateVisuals()
        {
            if (_petSprite == null) return;
            
            // 根据状态改变颜色/效果
            switch (_currentState)
            {
                case PetAIState.Attacking:
                    // 攻击时稍微放大
                    _petSprite.Scale = new Vector2(1.2f, 1.2f);
                    break;
                case PetAIState.Retreating:
                    // 后退时缩小
                    _petSprite.Scale = new Vector2(0.9f, 0.9f);
                    break;
                case PetAIState.Supporting:
                    // 支援时发光效果（通过颜色变化模拟）
                    _petSprite.Modulate = new Color(1f, 1f, 0.8f, 1f);
                    break;
                default:
                    _petSprite.Scale = Vector2.One;
                    _petSprite.Modulate = Colors.White;
                    break;
            }
        }

        public void OnPlayerDamaged(int damage)
        {
            if (_activePet == null) return;
            
            if (_activePet.Loyalty >= 70)
            {
                var enemy = SelectSmartTarget();
                if (enemy != null)
                    AttackEnemy(enemy, 0.016f);
            }
        }

        public void OnPlayerDeath()
        {
            if (_activePet?.SpecialAbility == "resurrect")
            {
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

        public void SetPetVisible(bool visible)
        {
            IsVisible = visible;
            if (_petSprite != null) _petSprite.Visible = visible;
            if (_petNameLabel != null) _petNameLabel.Visible = visible;
        }

        public Vector2 GetPetPosition() => GlobalPosition;
    }
    
    /// <summary>
    /// 宠物性格 - 决定AI行为模式
    /// </summary>
    public enum PetPersonality
    {
        Balanced,      // 平衡 - 标准战斗行为
        Aggressive,    // 攻击性 - 积极进攻，保持战斗
        Defensive,     // 防御性 - 保护玩家，优先支援
        Cautious       // 谨慎 - 保持距离，避免近身
    }
}
