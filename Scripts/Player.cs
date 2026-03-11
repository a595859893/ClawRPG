using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.UI;
using ClawRPG.Systems;

namespace ClawRPG.Scripts.Characters {
    /// <summary>
    /// Player character controller with combat system
    /// </summary>
    public partial class Player : CharacterBody2D
    {
        // Movement
        [Export] public float MoveSpeed = 200f;
        [Export] public float DodgeSpeed = 400f;
        
        // Combat stats
        [Export] public int MaxHealth = 100;
        [Export] public int MaxMana = 50;
        [Export] public float AttackDamage = 15f;
        [Export] public float AttackSpeed = 0.5f;
        [Export] public float CriticalChance = 0.1f;
        [Export] public float CriticalDamage = 1.5f;
        
        // Stamina
        [Export] public float MaxStamina = 100f;
        [Export] public float StaminaRegen = 20f;
        
        // Block system
        [Export] public float BlockStaminaCost = 15f;
        [Export] public float BlockDamageReduction = 0.5f;
        [Export] public float PerfectBlockWindow = 0.2f;
        
        // Dodge system
        [Export] public float DodgeDuration = 0.3f;
        [Export] public float DodgeCooldown = 1.0f;
        
        // State
        public int CurrentHealth { get; private set; }
        public int CurrentMana { get; private set; }
        public float CurrentStamina { get; private set; }
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        
        // Currency
        public int Gold { get; set; }
        
        // Combat statistics for titles
        public int PerfectBlockCount { get; private set; }
        public int DodgeCount { get; private set; }
        
        // Tutorial tracking
        private bool _hasTriggeredFirstCrit = false;
        private bool _hasTriggeredFirstBlock = false;
        private bool _hasTriggeredFirstDodge = false;
        
        // World Event multipliers
        public float EventXPMultiplier { get; set; } = 1.0f;
        public float EventDropMultiplier { get; set; } = 1.0f;
        public float EventGoldMultiplier { get; set; } = 1.0f;
        
        // Rune system - base attributes (before runes)
        public float BaseAttackDamage { get; private set; } = 15f;
        public float BaseDefense { get; private set; } = 5f;
        public float BaseMaxHealth { get; private set; } = 100f;
        public float BaseMaxMana { get; private set; } = 50f;
        public float BaseCritChance { get; private set; } = 0.1f;
        public float BaseCritDamage { get; private set; } = 1.5f;
        public float BaseAttackSpeed { get; private set; } = 0.5f;
        public float BaseMoveSpeed { get; private set; } = 200f;
        
        // Rune system - total attributes (after runes applied)
        public float TotalAttackDamage => BaseAttackDamage + GetRuneBonus(RuneAttribute.Damage) + RelicAttackBonus + BoonAttackBonus + TalentAttackBonus;
        public float TotalDefense => BaseDefense + GetRuneBonus(RuneAttribute.Defense) + RelicDefenseBonus + BoonDefenseBonus + TalentDefenseBonus;
        public float TotalMaxHealth => (int)(BaseMaxHealth + GetRuneBonus(RuneAttribute.MaxHealth) + RelicHealthBonus + BoonHealthBonus + TalentHealthBonus);
        public float TotalMaxMana => (int)(BaseMaxMana + GetRuneBonus(RuneAttribute.MaxMana) + BoonMagicBonus);
        public float TotalCritChance => BaseCritChance + GetRuneBonus(RuneAttribute.CritChance) / 100f + RelicCritRateBonus + BoonCritRateBonus + TalentCritRate;
        public float TotalCritDamage => BaseCritDamage + GetRuneBonus(RuneAttribute.CritDamage) / 100f + RelicCritDamageBonus + BoonCritDamageBonus + TalentCritDamage;
        public float TotalAttackSpeed => BaseAttackSpeed;
        
        // Mount system bonuses (applied on top of base + runes)
        public int MountSpeedBonus { get; set; }
        public int MountCarryCapacityBonus { get; set; }
        
        // Relic system bonuses (applied on top of base + runes + mount)
        public float RelicAttackBonus { get; set; }
        public float RelicDefenseBonus { get; set; }
        public float RelicHealthBonus { get; set; }
        public float RelicSpeedBonus { get; set; }
        public float RelicCritRateBonus { get; set; }
        public float RelicCritDamageBonus { get; set; }
        public float RelicLifestealBonus { get; set; }
        public float RelicDodgeBonus { get; set; }
        
        // Boon system bonuses (applied on top of base + runes + relic)
        public int BoonAttackBonus { get; set; }
        public int BoonDefenseBonus { get; set; }
        public int BoonHealthBonus { get; set; }
        public int BoonMagicBonus { get; set; }
        public int BoonSpeedBonus { get; set; }
        public float BoonCritRateBonus { get; set; }
        public float BoonCritDamageBonus { get; set; }
        public float BoonLifestealBonus { get; set; }
        public float BoonDodgeBonus { get; set; }
        
        // Pet equipment enhancement bonuses
        public Dictionary<string, float> EnhancementBonuses { get; set; } = new();
        
        public float TotalMoveSpeed => BaseMoveSpeed + GetRuneBonus(RuneAttribute.MoveSpeed) + MountSpeedBonus + RelicSpeedBonus + BoonSpeedBonus;
        
        // Resistance bonuses from runes
        public float FireResistance { get; private set; }
        public float IceResistance { get; private set; }
        public float DarkResistance { get; private set; }
        
        // Skill system
        public int SkillPoints { get; private set; }
        public List<int> LearnedSkillIds { get; private set; } = new();
        public Dictionary<int, int> SkillLevels { get; private set; } = new();
        
        // Combat state
        public bool IsAttacking { get; private set; }
        public bool IsBlocking { get; private set; }
        public bool IsDodging { get; private set; }
        public bool IsInvincible { get; private set; }
        public bool IsPerfectBlock { get; private set; }
        public Vector2 AttackDirection { get; private set; } = Vector2.Right;
        
        // Timers
        private float _attackTimer;
        private float _dodgeCooldownTimer;
        private float _perfectBlockTimer;
        private float _staminaRegenTimer;
        
        // Status effects
        private List<StatusEffect> _statusEffects = new();
        
        // Components
        private AnimationPlayer _animationPlayer;
        private Sprite2D _sprite;
        private ShaderMaterial _hitFlashMaterial;
        private ShaderMaterial _skillGlowMaterial;
        private Area2D _attackArea;
        private CollisionShape2D _hitbox;
        
        // Inventory reference
        public Inventory Inventory { get; private set; }
        
        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            CurrentStamina = MaxStamina;
            Inventory = new Inventory();
            
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _sprite = GetNode<Sprite2D>("Sprite2D");
            
            // Initialize AnimationEffectManager for attack/hit effects
            if (GetTree().CurrentScene != null)
            {
                var effectManager = new AnimationEffectManager();
                GetTree().CurrentScene.AddChild(effectManager);
            }
            
            // Initialize hit flash shader material
            var shader = GD.Load<Shader>("res://Shaders/player_hit_flash.gdshader");
            if (shader != null)
            {
                _hitFlashMaterial = new ShaderMaterial();
                _hitFlashMaterial.Shader = shader;
                _hitFlashMaterial.SetShaderParameter("flash_amount", 0.0f);
                _hitFlashMaterial.SetShaderParameter("flash_color", new Color(1f, 0.3f, 0.3f));
                _sprite.Material = _hitFlashMaterial;
            }
            
            // Initialize skill glow shader material
            var skillGlowShader = GD.Load<Shader>("res://Shaders/skill_glow.gdshader");
            if (skillGlowShader != null)
            {
                _skillGlowMaterial = new ShaderMaterial();
                _skillGlowMaterial.Shader = skillGlowShader;
                _skillGlowMaterial.SetShaderParameter("glow_amount", 0.0f);
                _skillGlowMaterial.SetShaderParameter("glow_color", new Color(0.3f, 0.6f, 1.0f, 1.0f));
                _skillGlowMaterial.SetShaderParameter("enable_pulse", true);
                _skillGlowMaterial.SetShaderParameter("pulse_speed", 3.0f);
                _skillGlowMaterial.SetShaderParameter("pulse_min", 0.3f);
                _skillGlowMaterial.SetShaderParameter("pulse_max", 0.7f);
                _skillGlowMaterial.SetShaderParameter("enable_edge_glow", true);
            }
            
            _attackArea = GetNode<Area2D>("AttackArea");
            _hitbox = GetNode<CollisionShape2D>("Hitbox/CollisionShape2D");
            
            // 添加头顶称号显示
            AddChild(new UI.PlayerTitleDisplay());
            
            GD.Print("Player initialized - HP: " + CurrentHealth + "/" + MaxHealth);
        }
        
        /// <summary>
        /// Add gold to player and track for daily challenges and achievements
        /// </summary>
        public void AddGold(int amount) {
            Gold += amount;
            DailyChallengeManager.Instance.OnGoldEarned(amount);
            // Track achievement progress
            AchievementManager.Instance.TrackGoldEarned(amount);
            // Track statistics
            StatisticsManager.Instance.RecordGoldEarned(amount);
            // Check title progress
            TitleSystem.Instance.CheckAndUnlockTitle("Collection", Gold);
        }
        
        /// <summary>
        /// Add experience to player and handle level up
        /// </summary>
        public void AddExperience(int amount) {
            Experience += amount;
            
            // Track statistics
            StatisticsManager.Instance.RecordExperience(amount);
            
            // Check for level up
            int expNeeded = GetExperienceForNextLevel();
            while (Experience >= expNeeded) {
                Experience -= expNeeded;
                Level++;
                LevelUp();
                expNeeded = GetExperienceForNextLevel();
            }
        }
        
        private int GetExperienceForNextLevel() {
            return Level * 100 + 50;
        }
        
        private void LevelUp() {
            MaxHealth += 10;
            MaxMana += 5;
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            SkillPoints += 1;
            
            // Play level up sound
            if (SoundEffectSystem.Instance != null)
                SoundEffectSystem.Instance.PlayLevelUp();
            
            // Track statistics
            StatisticsManager.Instance.UpdateHighestLevel(Level);
            
            // Check level titles
            TitleSystem.Instance.CheckAndUnlockTitle("Level", Level);
            
            GD.Print($"Player leveled up! Level: {Level}");
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            
            // Update timers
            UpdateTimers(dt);
            
            // Handle movement
            HandleMovement(dt);
            
            // Update status effects
            UpdateStatusEffects(dt);
            
            // Regenerate stamina
            RegenerateStamina(dt);
            
            // Apply velocity
            Velocity = Velocity.MoveToward(Vector2.Zero, 500f * (float)delta);
            MoveAndSlide();
        }
        
        private void UpdateTimers(float dt)
        {
            if (_attackTimer > 0) _attackTimer -= dt;
            if (_dodgeCooldownTimer > 0) _dodgeCooldownTimer -= dt;
            if (_perfectBlockTimer > 0) _perfectBlockTimer -= dt;
            
            if (_perfectBlockTimer <= 0) IsPerfectBlock = false;
        }
        
        private void HandleMovement(float dt)
        {
            if (IsDodging || IsAttacking) return;
            
            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            
            // Block movement speed reduction
            float speedMod = IsBlocking ? 0.5f : 1f;
            
            if (inputDir != Vector2.Zero)
            {
                Velocity = inputDir * MoveSpeed * speedMod;
                AttackDirection = inputDir.Normalized();
                
                // Flip sprite
                if (inputDir.x < 0) _sprite.FlipH = true;
                else if (inputDir.x > 0) _sprite.FlipH = false;
            }
            else
            {
                Velocity = Vector2.Zero;
            }
        }
        
        private void RegenerateStamina(float dt)
        {
            if (!IsBlocking && CurrentStamina < MaxStamina)
            {
                CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + StaminaRegen * dt);
            }
        }
        
        public override void _Process(double delta)
        {
            float dt = (float)delta;
            
            // Handle combat input
            HandleCombatInput(dt);
        }
        
        private void HandleCombatInput(float dt)
        {
            // Attack
            if (Input.IsActionPressed("attack") && !IsAttacking && !IsBlocking && _attackTimer <= 0)
            {
                PerformAttack();
            }
            
            // Dodge
            if (Input.IsActionPressed("dodge") && !IsDodging && _dodgeCooldownTimer <= 0 && CurrentStamina >= 20f)
            {
                PerformDodge();
            }
            
            // Block
            HandleBlockInput(dt);
        }
        
        private void HandleBlockInput(float dt)
        {
            bool wantsBlock = Input.IsActionPressed("block");
            
            if (wantsBlock && !IsBlocking && CurrentStamina >= BlockStaminaCost && !IsDodging && !IsAttacking)
            {
                StartBlock();
            }
            else if (!wantsBlock && IsBlocking)
            {
                EndBlock();
            }
            
            if (IsBlocking)
            {
                CurrentStamina -= BlockStaminaCost * dt;
                if (CurrentStamina <= 0)
                {
                    EndBlock();
                }
            }
        }
        
        private void StartBlock()
        {
            IsBlocking = true;
            GD.Print("Block started");
        }
        
        private void EndBlock()
        {
            IsBlocking = false;
            IsPerfectBlock = false;
            _perfectBlockTimer = 0;
            GD.Print("Block ended");
        }
        
        public void TriggerPerfectBlock()
        {
            IsPerfectBlock = true;
            _perfectBlockTimer = PerfectBlockWindow;
            // Track statistics
            StatisticsManager.Instance.RecordPerfectBlock();
            // Track perfect block for titles
            PerfectBlockCount++;
            TitleSystem.Instance.CheckAndUnlockTitle("Combat", PerfectBlockCount);
            // Track perfect block achievements
            AchievementManager.Instance?.TrackPerfectBlock(1);
            // Trigger counter attack system
            Systems.CounterAttackSystem.Instance?.OnPerfectBlock();
            
            // Trigger tutorial for first block
            if (!_hasTriggeredFirstBlock)
            {
                _hasTriggeredFirstBlock = true;
                TutorialSystem.Trigger(TutorialTrigger.FirstBlock);
            }
            
            GD.Print("PERFECT BLOCK!");
        }
        
        private void PerformAttack()
        {
            IsAttacking = true;
            _attackTimer = AttackSpeed;
            
            // Play attack animation effect
            AnimationEffectManager.Instance?.PlayAttackAnimation(GlobalPosition, 0.3f);
            
            // Calculate damage with critical
            float damage = AttackDamage;
            bool isCrit = GD.Randf() < CriticalChance;
            if (isCrit)
            {
                damage *= CriticalDamage;
                // Trigger tutorial for first critical hit
                if (!_hasTriggeredFirstCrit)
                {
                    _hasTriggeredFirstCrit = true;
                    TutorialSystem.Trigger(TutorialTrigger.FirstCrit);
                }
            }
            
            // Deal damage to enemies in range
            var enemies = GetEnemiesInAttackRange();
            foreach (var enemy in enemies)
            {
                enemy.TakeDamage((int)damage, isCrit, AttackDirection);
                
                // Play hit effect for each enemy hit
                AnimationEffectManager.Instance?.PlayHitEffect(enemy.GlobalPosition, isCrit, 1.0f);
                
                // Play particle effect - sparks for normal, crit sparkle for critical
                var particleManager = GetTree().GetFirstNodeInGroup("ParticleEffectManager") as ParticleEffectManager 
                    ?? ParticleEffectManager.Instance;
                if (particleManager != null)
                {
                    particleManager.PlayParticleEffect2D(
                        isCrit ? ParticleEffectType.CritSparkle : ParticleEffectType.Sparks,
                        enemy.GlobalPosition,
                        isCrit ? 1.5f : 1.0f
                    );
                }
                
                // Play sound effect
                var soundSystem = GetTree().GetFirstNodeInGroup("SoundEffectSystem") as SoundEffectSystem 
                    ?? SoundEffectSystem.Instance;
                if (soundSystem != null)
                {
                    if (isCrit)
                        soundSystem.PlayComboMilestone();
                    else
                        soundSystem.PlayEnemyHit();
                }
            }
            
            // Register combo hit
            var comboSystem = GetTree().GetFirstNodeInGroup("ComboSystem") as ComboSystem;
            comboSystem?.RegisterHit((int)damage);
            
            // Track combat stats
            CombatStatusSystem.Instance.RecordDamageDealt(damage, isCrit, CombatStatusData.DamageBreakdown.DamageType.Physical);
            CombatStatusSystem.Instance.IncrementCombo();
            
            GD.Print("Attack! Damage: " + damage + (isCrit ? " CRITICAL!" : ""));
            
            // Attack animation/sound would go here
            
            // Reset attack state after animation
            GetTree().CreateTimer(0.3f).Timeout += () => IsAttacking = false;
        }
        
        private void PerformDodge()
        {
            IsDodging = true;
            IsInvincible = true;
            CurrentStamina -= 20f;
            _dodgeCooldownTimer = DodgeCooldown;
            
            // Track dodge for combat status
            CombatStatusSystem.Instance.RecordDodge();
            
            // Track dodge for titles
            DodgeCount++;
            TitleSystem.Instance.CheckAndUnlockTitle("Combat", DodgeCount);
            
            // Trigger tutorial for first dodge
            if (!_hasTriggeredFirstDodge)
            {
                _hasTriggeredFirstDodge = true;
                TutorialSystem.Trigger(TutorialTrigger.FirstDodge);
            }
            
            Vector2 dodgeDir = AttackDirection;
            if (Input.GetVector("move_left", "move_right", "move_up", "move_down") != Vector2.Zero)
            {
                dodgeDir = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();
            }
            
            // Tween velocity for smooth dodge
            var tween = CreateTween();
            tween.TweenProperty(this, "velocity", dodgeDir * DodgeSpeed, DodgeDuration);
            
            GetTree().CreateTimer(DodgeDuration).Timeout += () => {
                IsDodging = false;
                IsInvincible = false;
                Velocity = Vector2.Zero;
            };
            
            GD.Print("Dodge performed!");
        }
        
        private List<Enemy> GetEnemiesInAttackRange()
        {
            var enemies = new List<Enemy>();
            if (_attackArea == null) return enemies;
            
            var bodies = _attackArea.GetOverlappingBodies();
            foreach (var body in bodies)
            {
                if (body is Enemy enemy)
                {
                    enemies.Add(enemy);
                }
            }
            return enemies;
        }
        
        /// <summary>
        /// 获取符文属性加成
        /// </summary>
        private float GetRuneBonus(RuneAttribute attribute) {
            try {
                var runeManager = Systems.RuneManager.Instance;
                if (runeManager == null) return 0;
                
                var attributes = runeManager.CalculateTotalAttributes();
                return attributes.TryGetValue(attribute, out float value) ? value : 0;
            } catch {
                return 0;
            }
        }
        
        /// <summary>
        /// 刷新符文属性加成（从符文管理器更新抗性等）
        /// </summary>
        public void RefreshRuneAttributes() {
            FireResistance = GetRuneBonus(RuneAttribute.FireResistance);
            IceResistance = GetRuneBonus(RuneAttribute.IceResistance);
            DarkResistance = GetRuneBonus(RuneAttribute.DarkResistance);
        }
        
        /// <summary>
        /// Apply boons bonuses from RandomBoonSystem
        /// </summary>
        public void ApplyBoonBonuses()
        {
            if (RandomBoonSystem.Instance == null) return;
            
            BoonAttackBonus = RandomBoonSystem.Instance.GetAttackBonus();
            BoonDefenseBonus = RandomBoonSystem.Instance.GetDefenseBonus();
            BoonHealthBonus = RandomBoonSystem.Instance.GetHealthBonus();
            BoonMagicBonus = RandomBoonSystem.Instance.GetMagicBonus();
            BoonSpeedBonus = RandomBoonSystem.Instance.GetSpeedBonus();
            BoonCritRateBonus = RandomBoonSystem.Instance.GetCritRateBonus();
            BoonCritDamageBonus = RandomBoonSystem.Instance.GetCritDamageBonus();
            BoonLifestealBonus = RandomBoonSystem.Instance.GetLifestealBonus();
            BoonDodgeBonus = RandomBoonSystem.Instance.GetDodgeBonus();
        }
        
        public void UpdateTotalAttributes()
        {
            // Recalculate all derived attributes after talent changes
            // Most derived values are computed via properties, just trigger refresh
            RefreshRuneAttributes();
            ApplyBoonBonuses();
        }
        
        public void TakeDamage(int damage, bool isCrit = false, Vector2 fromDirection = default)
        {
            if (IsInvincible) return;
            
            float finalDamage = damage;
            
            // Apply block reduction
            if (IsBlocking)
            {
                if (IsPerfectBlock)
                {
                    finalDamage = 0;
                    TriggerPerfectBlock();
                    // Show perfect block text
                    if (DamageNumberSystem.Instance != null)
                    {
                        DamageNumberSystem.Instance.ShowTextEffect2D(GetViewport().GetCamera2D().GetScreenCenterPosition(), "PERFECT!", DamageNumberSystem.DamageType.Blocked);
                    }
                    GD.Print("Perfect block! No damage taken!");
                }
                else
                {
                    finalDamage *= (1 - BlockDamageReduction);
                    // Track block
                    CombatStatusSystem.Instance.RecordBlock();
                    // Show blocked text
                    if (DamageNumberSystem.Instance != null)
                    {
                        DamageNumberSystem.Instance.ShowTextEffect2D(GetViewport().GetCamera2D().GetScreenCenterPosition(), "BLOCKED", DamageNumberSystem.DamageType.Blocked);
                    }
                    GD.Print("Blocked! Damage reduced to: " + finalDamage);
                }
            }
            
            CurrentHealth -= (int)finalDamage;
            
            // Track statistics
            StatisticsManager.Instance.RecordDamageTaken((int)finalDamage);
            
            // Track combat status - damage taken
            CombatStatusSystem.Instance.RecordDamageTaken(finalDamage, isCrit);
            CombatStatusSystem.Instance.ResetCombo();
            
            // Track boss damage for no-hit boss achievement
            AchievementManager.Instance?.TrackBossDamageTaken((int)finalDamage);
            
            if (CurrentHealth <= 0)
            {
                Die();
            }
            
            // Knockback
            if (fromDirection != default)
            {
                Velocity = -fromDirection * 100f;
            }
            
            // Visual feedback - show damage number
            if (finalDamage > 0)
            {
                ShowDamageNumber((int)finalDamage, isCrit);
                // Trigger hit flash effect
                TriggerHitFlash();
                // Play hit animation effect
                AnimationEffectManager.Instance?.PlayHitAnimation(fromDirection);
                
                // Play blood particle effect
                var particleManager = GetTree().GetFirstNodeInGroup("ParticleEffectManager") as ParticleEffectManager 
                    ?? ParticleEffectManager.Instance;
                particleManager?.PlayParticleEffect2D(ParticleEffectType.Blood, GlobalPosition, 0.8f);
                
                // Play damage sound effect
                var soundSystem = GetTree().GetFirstNodeInGroup("SoundEffectSystem") as SoundEffectSystem 
                    ?? SoundEffectSystem.Instance;
                soundSystem?.PlayDamage();
            }
            
            GD.Print("Player took " + finalDamage + " damage. HP: " + CurrentHealth + "/" + MaxHealth);
        }
        
        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            // Track statistics
            StatisticsManager.Instance.RecordHealing(amount);
            // Show heal number
            if (DamageNumberSystem.Instance != null)
            {
                DamageNumberSystem.Instance.ShowDamageOnEntity2D(this, amount, DamageNumberSystem.DamageType.Heal);
            }
            GD.Print("Healed " + amount + " HP. HP: " + CurrentHealth + "/" + MaxHealth);
        }
        
        public void UseMana(int amount)
        {
            CurrentMana = Mathf.Max(0, CurrentMana - amount);
        }
        
        public void RestoreMana(int amount)
        {
            CurrentMana = Mathf.Min(MaxMana, CurrentMana + amount);
            GD.Print("Restored " + amount + " Mana. Mana: " + CurrentMana + "/" + MaxMana);
        }
        
        // Skill system methods
        public bool CanLearnSkill(Skill skill)
        {
            if (SkillPoints < 1) return false;
            if (LearnedSkillIds.Contains(skill.Id)) return false;
            if (Level < skill.LevelRequired) return false;
            return true;
        }
        
        public bool CanUpgradeSkill(Skill skill)
        {
            if (!LearnedSkillIds.Contains(skill.Id)) return false;
            if (SkillPoints < 1) return false;
            int currentLevel = SkillLevels.GetValueOrDefault(skill.Id, 1);
            if (currentLevel >= skill.MaxLevel) return false;
            return true;
        }
        
        public bool LearnSkill(Skill skill)
        {
            if (!CanLearnSkill(skill)) return false;
            
            SkillPoints--;
            LearnedSkillIds.Add(skill.Id);
            SkillLevels[skill.Id] = 1;
            
            GD.Print("Learned skill: " + skill.Name);
            
            // Apply passive skill bonuses
            ApplySkillBonuses(skill);
            
            return true;
        }
        
        public bool UpgradeSkill(Skill skill)
        {
            if (!CanUpgradeSkill(skill)) return false;
            
            SkillPoints--;
            SkillLevels[skill.Id]++;
            
            int newLevel = SkillLevels[skill.Id];
            GD.Print("Upgraded skill: " + skill.Name + " to level " + newLevel);
            
            // Reapply skill bonuses
            ApplySkillBonuses(skill);
            
            return true;
        }
        
        private void ApplySkillBonuses(Skill skill)
        {
            int skillLevel = SkillLevels.GetValueOrDefault(skill.Id, 1);
            
            // Apply passive bonuses based on skill
            if (skill.PassiveAttackBonus > 0)
            {
                AttackDamage += skill.PassiveAttackBonus * skillLevel;
            }
            if (skill.PassiveDefenseBonus > 0)
            {
                // Would need to add defense stat
            }
            if (skill.PassiveHealthBonus > 0)
            {
                MaxHealth += skill.PassiveHealthBonus * skillLevel;
                CurrentHealth = Mathf.Min(CurrentHealth + skill.PassiveHealthBonus * skillLevel, MaxHealth);
            }
            if (skill.PassiveManaBonus > 0)
            {
                MaxMana += skill.PassiveManaBonus * skillLevel;
                CurrentMana = Mathf.Min(CurrentMana + skill.PassiveManaBonus * skillLevel, MaxMana);
            }
            if (skill.PassiveCritBonus > 0)
            {
                CriticalChance = Mathf.Min(1.0f, CriticalChance + skill.PassiveCritBonus * skillLevel);
            }
        }
        
        public void GainExperience(int amount)
        {
            Experience += amount;
            GD.Print("Gained " + amount + " XP. Total: " + Experience);
            
            // Check for level up (100 XP per level)
            int expToLevel = Level * 100;
            if (Experience >= expToLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            Level++;
            Experience -= Level * 100;
            SkillPoints += 1; // 1 skill point per level
            
            // Increase stats
            MaxHealth += 20;
            MaxMana += 10;
            AttackDamage += 5;
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            
            GD.Print("LEVEL UP! Now level " + Level + "! +1 Skill Point!");
            
            // Track achievement progress
            AchievementManager.Instance.TrackLevel(Level);
            
            // Level up effect
            var effect = new LevelUpEffect();
            GetTree().CurrentScene.AddChild(effect);
            effect.GlobalPosition = GlobalPosition;
        }
        
        private void Die()
        {
            GD.Print("Player died!");
            // Track statistics
            StatisticsManager.Instance.RecordDeath();
            // Handle death (game over, respawn, etc.)
            QueueFree();
        }
        
        public void ApplyStatusEffect(StatusEffect effect)
        {
            _statusEffects.Add(effect);
            GD.Print("Applied status effect: " + effect.Type);
        }
        
        private void UpdateStatusEffects(float dt)
        {
            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                var effect = _statusEffects[i];
                effect.Update(this, dt);
                
                if (effect.IsExpired)
                {
                    _statusEffects.RemoveAt(i);
                }
            }
        }
        
        public float GetBlockDamageReduction()
        {
            return IsPerfectBlock ? 1.0f : BlockDamageReduction;
        }
        
        /// <summary>
        /// 重置玩家数据 - 用于新游戏开始
        /// </summary>
        public void ResetPlayer()
        {
            // 重置基础属性
            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
            CurrentStamina = MaxStamina;
            Level = 1;
            Experience = 0;
            Gold = 0;
            
            // 重置战斗状态
            IsAttacking = false;
            IsBlocking = false;
            IsDodging = false;
            IsInvincible = false;
            IsPerfectBlock = false;
            
            // 重置技能
            SkillPoints = 0;
            LearnedSkillIds.Clear();
            SkillLevels.Clear();
            
            // 重置状态效果
            _statusEffects.Clear();
            
            // 重置世界事件倍率
            EventXPMultiplier = 1.0f;
            EventDropMultiplier = 1.0f;
            EventGoldMultiplier = 1.0f;
            
            // 重置位置
            GlobalPosition = new Vector2(640, 360);
            
            GD.Print("Player reset for new game");
        }
        
        /// <summary>
        /// 加载玩家数据 - 用于存档读取
        /// </summary>
        public void LoadPlayerData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 加载基础属性
            if (data.TryGetValue("Level", out var level)) Level = Convert.ToInt32(level);
            if (data.TryGetValue("Experience", out var exp)) Experience = Convert.ToInt32(exp);
            if (data.TryGetValue("Gold", out var gold)) Gold = Convert.ToInt32(gold);
            if (data.TryGetValue("CurrentHealth", out var health)) CurrentHealth = Convert.ToInt32(health);
            if (data.TryGetValue("CurrentMana", out var mana)) CurrentMana = Convert.ToInt32(mana);
            
            // 加载技能点
            if (data.TryGetValue("SkillPoints", out var skillPoints)) 
                SkillPoints = Convert.ToInt32(skillPoints);
            
            // 加载已学习技能
            if (data.TryGetValue("LearnedSkillIds", out var skillIds))
            {
                LearnedSkillIds.Clear();
                var ids = skillIds as System.Collections.IEnumerable;
                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        LearnedSkillIds.Add(Convert.ToInt32(id));
                    }
                }
            }
            
            // 加载技能等级
            if (data.TryGetValue("SkillLevels", out var skillLevels))
            {
                SkillLevels.Clear();
                var levels = skillLevels as Dictionary<object, object>;
                if (levels != null)
                {
                    foreach (var kvp in levels)
                    {
                        SkillLevels[Convert.ToInt32(kvp.Key)] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
            
            GD.Print("Player data loaded: Level " + Level + ", Gold " + Gold);
        }
        
        private void ShowDamageNumber(int damage, bool isCrit)
        {
            if (DamageNumberSystem.Instance != null)
            {
                var type = isCrit ? DamageNumberSystem.DamageType.Critical : DamageNumberSystem.DamageType.Normal;
                DamageNumberSystem.Instance.ShowDamageOnEntity2D(this, damage, type);
            }
        }
        
        /// <summary>
        /// Trigger hit flash effect using shader
        /// </summary>
        private void TriggerHitFlash()
        {
            if (_hitFlashMaterial == null || _sprite == null) return;
            
            var tween = CreateTween();
            // Flash to full intensity quickly
            tween.TweenCallback(Callable.From(() => {
                _hitFlashMaterial.SetShaderParameter("flash_amount", 0.6f);
            }));
            // Fade out
            tween.TweenInterval(0.05f);
            tween.TweenCallback(Callable.From(() => {
                _hitFlashMaterial.SetShaderParameter("flash_amount", 0.0f);
            }));
        }
        
        /// <summary>
        /// Trigger skill glow effect using shader - for magic/buff effects
        /// </summary>
        public void TriggerSkillGlow(Color? glowColor = null, float duration = 0.5f)
        {
            if (_skillGlowMaterial == null || _sprite == null) return;
            
            // Set custom glow color if provided
            if (glowColor.HasValue)
            {
                _skillGlowMaterial.SetShaderParameter("glow_color", glowColor.Value);
            }
            
            // Apply material if not already applied
            if (_sprite.Material != _skillGlowMaterial && _sprite.Material != _hitFlashMaterial)
            {
                _sprite.Material = _skillGlowMaterial;
            }
            
            var tween = CreateTween();
            // Fade in glow
            tween.TweenCallback(Callable.From(() => {
                _skillGlowMaterial.SetShaderParameter("glow_amount", 0.8f);
            }));
            // Hold
            tween.TweenInterval(duration * 0.5f);
            // Fade out
            tween.TweenCallback(Callable.From(() => {
                _skillGlowMaterial.SetShaderParameter("glow_amount", 0.0f);
            }));
            // Restore original material after effect ends
            tween.TweenInterval(duration * 0.5f);
            tween.TweenCallback(Callable.From(() => {
                if (_hitFlashMaterial != null)
                {
                    _sprite.Material = _hitFlashMaterial;
                }
            }));
        }
    }
    
    /// <summary>
    /// Status effect base class
    /// </summary>
    public class StatusEffect
    {
        public enum EffectType { Poison, Burn, Freeze, Stun, Slow, Bleed, Sleep, Paralyze, Confusion, Shield, Regeneration }
        
        public EffectType Type { get; set; }
        public float Duration { get; set; }
        public float TickInterval { get; set; } = 1f;
        public float DamagePerTick { get; set; }
        public float SpeedMultiplier { get; set; } = 1f;
        
        private float _timer;
        private float _tickTimer;
        
        public bool IsExpired => Duration <= 0;
        
        public virtual void Update(CharacterBody2D target, float dt)
        {
            Duration -= dt;
            _tickTimer += dt;
            
            if (_tickTimer >= TickInterval)
            {
                _tickTimer = 0;
                ApplyTickEffect(target);
            }
        }
        
        protected virtual void ApplyTickEffect(CharacterBody2D target)
        {
            // Override in subclasses
        }
    }
    
    /// <summary>
    /// Damage number popup effect
    /// </summary>
    public partial class DamagePopup : Node2D
    {
        private Label _label;
        private float _lifetime = 1f;
        private float _speed = 50f;
        
        public void Initialize(int damage, bool isCritical, Vector2 position)
        {
            GlobalPosition = position;
            
            _label = new Label();
            _label.Text = damage.ToString();
            _label.Modulate = isCritical ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 1f, 1f);
            _label.AddThemeFontSizeOverride("font_size", isCritical ? 32 : 24);
            AddChild(_label);
            
            // Animate
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_label, "position:y", -50f, _lifetime);
            tween.TweenProperty(_label, "modulate:a", 0f, _lifetime);
            
            QueueFree();
        }
    }
    
    /// <summary>
    /// Level up visual effect
    /// </summary>
    public partial class LevelUpEffect : Node2D
    {
        public override void _Ready()
        {
            var sprite = new Sprite2D();
            // Would load particle texture here
            AddChild(sprite);
            
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0f, 1f);
            QueueFree();
        }
    }
}
