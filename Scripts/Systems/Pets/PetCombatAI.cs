using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物战斗AI - 协调者模式
    /// 委托给三个子系统:
    /// - PetDecisionSystem: 决策逻辑 (决定何时行动)
    /// - PetTargetingSystem: 目标选择 (选择攻击/治疗目标)
    /// - PetBehaviorTree: 行为执行 (行为树执行)
    /// </summary>
    public class PetCombatAI : BaseSystem
    {
        // 单例
        private static PetCombatAI _instance;
        public static PetCombatAI Instance => _instance ??= new PetCombatAI();

        // 子系统
        private PetDecisionSystem _decisionSystem;
        private PetTargetingSystem _targetingSystem;
        private PetBehaviorTree _behaviorTree;

        // 宠物战斗属性
        private Pet _activePet;
        private CharacterBody2D _player;
        
        // 区域
        private Area2D _detectionArea;
        private Area2D _attackArea;
        
        // 视觉
        private Sprite2D _petSprite;
        private Label _petNameLabel;
        
        // 信号
        public Action OnPetAttack;
        public Action<string> OnPetSpecialAbility;
        
        // 配置
        private float _attackCooldown = 1.5f;
        private float _lastAttackTime = 0f;
        
        public bool IsEnabled { get; set; } = true;
        public bool IsVisible { get; set; } = true;

        public void Initialize()
        {
            _instance = this;
            _player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;
            
            // 初始化子系统
            _decisionSystem = new PetDecisionSystem();
            _decisionSystem.Initialize(_player);
            _decisionSystem.OnStateChanged += OnStateChanged;
            
            _targetingSystem = new PetTargetingSystem();
            _targetingSystem.Initialize(_player);
            
            _behaviorTree = new PetBehaviorTree();
            _behaviorTree.Initialize();
            _behaviorTree.OnPetAttack += () => OnPetAttack?.Invoke();
            _behaviorTree.OnPetSpecialAbility += (ability) => OnPetSpecialAbility?.Invoke(ability);
            
            if (_player != null)
            {
                CreatePetVisuals();
            }
            
            GD.Print("宠物战斗AI已初始化 (协调者模式)");
        }

        public void SetActivePet(Pet pet)
        {
            _activePet = pet;
            _decisionSystem.SetActivePet(pet);
            _targetingSystem.SetActivePet(pet);
            _behaviorTree.SetActivePet(pet);
            
            if (pet != null)
            {
                if (_petNameLabel != null)
                {
                    _petNameLabel.Text = pet.PetName;
                }
            }
        }
        
        private void OnStateChanged(PetDecisionSystem.PetAIState oldState, PetDecisionSystem.PetAIState newState)
        {
            GD.Print($"[PetCombatAI] State: {oldState} -> {newState}");
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
            
            // 通知子系统检测区域
            _targetingSystem.SetDetectionArea(_detectionArea);
            _behaviorTree.SetPetSprite(_petSprite);
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

            // 1. 目标选择 (委托给 PetTargetingSystem)
            var newTarget = _targetingSystem.SelectSmartTarget();
            _decisionSystem.SetCurrentTarget(newTarget);
            
            // 2. 决策更新 (委托给 PetDecisionSystem)
            _decisionSystem.UpdateDecision(newTarget, delta);
            
            // 3. 状态行为 (委托给 PetBehaviorTree)
            var currentState = _decisionSystem.CurrentState;
            _behaviorTree.ExecuteBehavior(currentState, newTarget, delta);
            
            // 4. 攻击处理 (协调者直接处理)
            ProcessAttack(newTarget, delta);
            
            // 5. 特殊能力处理 (委托给 PetBehaviorTree)
            if (currentState == PetDecisionSystem.PetAIState.Supporting)
            {
                _behaviorTree.SupportPlayer(delta);
            }
            else
            {
                _behaviorTree.ProcessSpecialAbility(newTarget, delta);
            }
            
            // 6. 更新视觉 (委托给 PetBehaviorTree)
            _behaviorTree.UpdateVisuals(currentState);
        }

        private void ProcessAttack(Node2D target, float delta)
        {
            if (target == null) return;
            if (_decisionSystem.CurrentState != PetDecisionSystem.PetAIState.Attacking) return;
            if (!_decisionSystem.CanAttack()) return;
            
            _decisionSystem.RecordAttack();
            _behaviorTree.AttackEnemy(target);
        }
        
        public void SetPetVisible(bool visible)
        {
            IsVisible = visible;
            if (_petSprite != null) _petSprite.Visible = visible;
            if (_petNameLabel != null) _petNameLabel.Visible = visible;
        }

        public Vector2 GetPetPosition() => GlobalPosition;

        public void OnPlayerDamaged(int damage)
        {
            _behaviorTree.OnPlayerDamaged(damage);
        }

        public void OnPlayerDeath()
        {
            _behaviorTree.TryResurrectPlayer();
        }

        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "decisionSystem", _decisionSystem?.ExportSaveData() },
                { "targetingSystem", _targetingSystem?.ExportSaveData() },
                { "behaviorTree", _behaviorTree?.ExportSaveData() }
            };
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("decisionSystem"))
                _decisionSystem?.ImportSaveData(data["decisionSystem"] as Dictionary);
            if (data.Contains("targetingSystem"))
                _targetingSystem?.ImportSaveData(data["targetingSystem"] as Dictionary);
            if (data.Contains("behaviorTree"))
                _behaviorTree?.ImportSaveData(data["behaviorTree"] as Dictionary);
        }
    }
    
    /// <summary>
    /// 宠物性格 - 决定AI行为模式 (保留给外部引用)
    /// </summary>
    public enum PetPersonality
    {
        Balanced,      // 平衡 - 标准战斗行为
        Aggressive,    // 攻击性 - 积极进攻，保持战斗
        Defensive,     // 防御性 - 保护玩家，优先支援
        Cautious       // 谨慎 - 保持距离，避免近身
    }
}
