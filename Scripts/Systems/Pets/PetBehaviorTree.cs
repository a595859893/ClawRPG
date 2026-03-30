using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物行为树系统 - 执行具体行为
    /// 职责：跟随玩家、攻击敌人、战术移动、特殊能力
    /// </summary>
    public class PetBehaviorTree : BaseSystem
    {
        private static PetBehaviorTree _instance;
        public static PetBehaviorTree Instance => _instance ??= new PetBehaviorTree();

        // 宠物引用
        private Pet _activePet;
        private CharacterBody2D _player;
        private Sprite2D _petSprite;
        
        // 配置
        private float _followSpeed = 180f;
        private float _attackRange = 100f;
        private float _tacticalDistance = 150f;
        
        // 特殊能力冷却
        private float _fireBreathCooldown = 0f;
        private float _fireBreathInterval = 5f;
        private float _holyProtectionTimer = 0f;
        private float _holyProtectionInterval = 10f;
        
        // 信号
        public Action OnPetAttack;
        public Action<string> OnPetSpecialAbility;

        public void Initialize()
        {
            _instance = this;
            GD.Print("[PetBehaviorTree] Initialized");
        }

        public void SetActivePet(Pet pet)
        {
            _activePet = pet;
        }

        public void SetPlayer(CharacterBody2D player)
        {
            _player = player;
        }

        public void SetPetSprite(Sprite2D sprite)
        {
            _petSprite = sprite;
        }

        public void SetConfig(float followSpeed, float attackRange, float tacticalDistance)
        {
            _followSpeed = followSpeed;
            _attackRange = attackRange;
            _tacticalDistance = tacticalDistance;
        }

        /// <summary>
        /// 行为执行 - 根据状态执行对应行为
        /// </summary>
        public void ExecuteBehavior(PetDecisionSystem.PetAIState state, Node2D target, float delta)
        {
            int tickId = PetDecisionSystem.NextDecisionTick(); // REQ-137: 行为执行决策节点

            // REQ-137: 记录行为执行决策
            if (PetReplayTraceSystem.Instance != null)
            {
                float timestamp = (float)Time.GetTicksMsec() / 1000f;
                string reason = target != null ? $"目标:{target.Name}" : "无目标";
                var record = PetDecisionRecord.CreateBehaviorExecution(tickId, timestamp, state, reason);
                PetReplayTraceSystem.Instance.RecordDecision(record);
            }

            switch (state)
            {
                case PetDecisionSystem.PetAIState.Following:
                    FollowPlayer(target, delta);
                    break;
                    
                case PetDecisionSystem.PetAIState.Engaging:
                    if (target != null)
                        TacticalApproach(target, delta);
                    break;
                    
                case PetDecisionSystem.PetAIState.Attacking:
                    // 攻击由PetCombatAI在协调器中处理
                    break;
                    
                case PetDecisionSystem.PetAIState.Retreating:
                    if (target != null)
                        MaintainDistance(target, delta, retreat: true);
                    break;
                    
                case PetDecisionSystem.PetAIState.Supporting:
                    SupportPlayer(delta);
                    break;
            }
        }

        /// <summary>
        /// 跟随玩家
        /// </summary>
        private void FollowPlayer(Node2D target, float delta)
        {
            if (_player == null) return;
            
            Vector2 offset = _activePet?.Type switch
            {
                PetType.Guardian => new Vector2(GD.Randf() * 60f - 30f, -30f),
                PetType.Collector => new Vector2(GD.Randf() * 80f - 40f, -60f),
                _ => new Vector2(GD.Randf() * 40f - 20f, -_tacticalDistance)
            };
            
            var targetPos = _player.GlobalPosition + offset;
            MoveTowardsSmooth(targetPos, delta, _followSpeed * 0.8f);
        }

        /// <summary>
        /// 战术接近 - 使用侧翼或包围策略
        /// </summary>
        private void TacticalApproach(Node2D target, float delta)
        {
            if (_player == null) return;
            
            Vector2 playerPos = _player.GlobalPosition;
            Vector2 enemyPos = target.GlobalPosition;
            
            // 获取玩家朝向
            int playerFacing = 1;
            if (_player is CharacterBody2D cb && cb.Velocity.x != 0)
                playerFacing = cb.Velocity.x > 0 ? 1 : -1;
            
            // 侧翼位置：敌人侧后方
            Vector2 enemyDir = (enemyPos - playerPos).Normalized();
            Vector2 flankDir = new Vector2(-enemyDir.Y, enemyDir.X);
            
            // 目标位置：在敌人侧翼，保持战术距离
            Vector2 tacticalTarget = enemyPos - enemyDir * _tacticalDistance * 0.5f + flankDir * playerFacing * _tacticalDistance * 0.3f;
            
            MoveTowardsSmooth(tacticalTarget, delta, _followSpeed);
        }

        /// <summary>
        /// 保持距离
        /// </summary>
        private void MaintainDistance(Node2D target, float delta, bool retreat)
        {
            float currentDist = GlobalPosition.DistanceTo(target.GlobalPosition);
            float idealDist = 150f; // 谨慎型保持150f距离
            
            Vector2 direction;
            if (retreat && currentDist < idealDist)
                direction = (GlobalPosition - target.GlobalPosition).Normalized();
            else if (currentDist > idealDist + 20f)
                direction = (target.GlobalPosition - GlobalPosition).Normalized();
            else
                return; // 距离合适
            
            GlobalPosition += direction * _followSpeed * delta;
            
            if (_petSprite != null)
                _petSprite.FlipH = direction.X < 0;
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
                    _petSprite.FlipH = direction.X < 0;
            }
        }

        /// <summary>
        /// 攻击敌人
        /// </summary>
        public void AttackEnemy(Node2D enemy)
        {
            if (enemy == null) return;
            
            var direction = (enemy.GlobalPosition - GlobalPosition).Normalized();
            if (_petSprite != null)
                _petSprite.FlipH = direction.X < 0;
            
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
        /// 支援玩家
        /// </summary>
        public void SupportPlayer(float delta)
        {
            if (_player == null || _activePet == null) return;
            
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
            FollowPlayer(null, delta);
        }

        /// <summary>
        /// 处理特殊能力（非守护型）
        /// </summary>
        public void ProcessSpecialAbility(Node2D target, float delta)
        {
            if (_activePet == null) return;
            
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

        private void PerformFireBreath()
        {
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            if (currentTime - _fireBreathCooldown < _fireBreathInterval) return;
            
            _fireBreathCooldown = currentTime;
            
            // 获取检测区域中的敌人
            var detectionArea = GetNodeOrNull<Area2D>("../DetectionArea");
            if (detectionArea == null) return;
            
            var bodies = detectionArea.GetOverlappingAreas();
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

        /// <summary>
        /// 复活玩家
        /// </summary>
        public void TryResurrectPlayer()
        {
            if (_activePet?.SpecialAbility != "resurrect") return;
            if (_player == null) return;
            if (_activePet.Loyalty < 50) return;
            
            float resChance = 0.3f + (_activePet.Loyalty - 50) * 0.01f;
            if (GD.Randf() < resChance)
            {
                _player.CallDeferred("Heal", (_activePet?.GetTotalHealthBonus() ?? 100) / 2);
                _activePet.Loyalty = Mathf.Max(0, _activePet.Loyalty - 20);
                
                GD.Print($"宠物 {_activePet.PetName} 复活了玩家!");
                OnPetSpecialAbility?.Invoke("resurrect");
            }
        }

        /// <summary>
        /// 更新视觉状态
        /// </summary>
        public void UpdateVisuals(PetDecisionSystem.PetAIState state)
        {
            if (_petSprite == null) return;
            
            switch (state)
            {
                case PetDecisionSystem.PetAIState.Attacking:
                    _petSprite.Scale = new Vector2(1.2f, 1.2f);
                    break;
                case PetDecisionSystem.PetAIState.Retreating:
                    _petSprite.Scale = new Vector2(0.9f, 0.9f);
                    break;
                case PetDecisionSystem.PetAIState.Supporting:
                    _petSprite.Modulate = new Color(1f, 1f, 0.8f, 1f);
                    break;
                default:
                    _petSprite.Scale = Vector2.One;
                    _petSprite.Modulate = Colors.White;
                    break;
            }
        }

        /// <summary>
        /// 玩家受伤时响应
        /// </summary>
        public void OnPlayerDamaged(int damage)
        {
            if (_activePet == null) return;
            
            if (_activePet.Loyalty >= 70)
            {
                var enemy = PetTargetingSystem.Instance.SelectSmartTarget();
                if (enemy != null)
                    AttackEnemy(enemy);
            }
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "fireBreathCooldown", _fireBreathCooldown },
                { "holyProtectionTimer", _holyProtectionTimer }
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            if (data.Contains("fireBreathCooldown"))
                _fireBreathCooldown = Convert.ToSingle(data["fireBreathCooldown"]);
            if (data.Contains("holyProtectionTimer"))
                _holyProtectionTimer = Convert.ToSingle(data["holyProtectionTimer"]);
        }
    }
}
