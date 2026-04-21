using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using ClawRPG.Scripts.Systems.Pets;
using Pet = ClawRPG.Scripts.Systems.Pets.Pet;
using PetRarity = ClawRPG.Scripts.Systems.Pets.PetRarity;
using PetSystem = ClawRPG.Scripts.Systems.Pets.PetManager;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 宠物界面 UI
    /// </summary>
    public partial class PetUI : Control
    {
        [Export] private Control _petListContainer;
        [Export] private Label _petCountLabel;
        [Export] private Button _closeButton;
        
        // 宠物详情面板
        [Export] private Label _petNameLabel;
        [Export] private Label _petTypeLabel;
        [Export] private Label _petRarityLabel;
        [Export] private Label _petLevelLabel;
        [Export] private Label _petExpLabel;
        [Export] private Label _petLoyaltyLabel;
        [Export] private ProgressBar _expProgressBar;
        [Export] private ProgressBar _loyaltyProgressBar;
        
        // 属性显示
        [Export] private Label _healthBonusLabel;
        [Export] private Label _attackBonusLabel;
        [Export] private Label _defenseBonusLabel;
        [Export] private Label _speedBonusLabel;
        [Export] private Label _criticalBonusLabel;
        [Export] private Label _specialAbilityLabel;
        
        // 按钮
        [Export] private Button _activateButton;
        [Export] private Button _releaseButton;
        
        private PetSystem _petManager;
        private ClawRPG.Scripts.Systems.Pets.Pet _selectedPet;
        private Array<Button> _petButtons = new Array<Button>();
        private bool _isVisible = false; 

        public override void _Ready()
        {
            _petManager = PetSystem.Instance;
            
            // 连接信号
            if (_closeButton != null)
                _closeButton.Pressed += OnClosePressed;
            
            if (_activateButton != null)
                _activateButton.Pressed += OnActivatePressed;
            
            if (_releaseButton != null)
                _releaseButton.Pressed += OnReleasePressed;
            
            // 宠物管理器信号
            _petManager.OnPetAdded += OnPetListChanged;
            _petManager.OnPetRemoved += OnPetListChanged;
            _petManager.OnActivePetChanged += OnActivePetChanged;
            
            // 初始化
            Visible = false; 
            RefreshPetList();
        }

        public override void _Input(InputEvent eventArgs)
        {
            if (eventArgs.IsActionPressed("pet_ui"))
            {
                ToggleVisibility();
            }
        }

        private void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                RefreshPetList();
            }
        }

        private void OnClosePressed()
        {
            Visible = false; 
            _isVisible = false; 
        }

        private void RefreshPetList()
        {
            if (_petListContainer == null) return;

            // 清除旧按钮
            foreach (var btn in _petButtons)
            {
                btn.QueueFree();
            }
            _petButtons.Clear();

            var pets = _petManager.OwnedPets;
            
            // 更新计数
            if (_petCountLabel != null)
                _petCountLabel.Text = $"{pets.Count}/{_petManager.MaxPets}";

            // 创建新按钮
            int index = 0;
            foreach (var pet in pets)
            {
                var btn = new Button();
                btn.Text = $"{pet.PetName} (Lv.{pet.Level})";
                btn.CustomMinimumSize = new Vector2(180, 40);
                
                // 根据稀有度设置颜色
                Color rarityColor = GetRarityColor(pet.Rarity);
                btn.Modulate = rarityColor;
                
                // 设置按钮位置
                int row = index / 2;
                int col = index % 2;
                btn.Position = new Vector2(col * 190, row * 50);
                
                btn.Pressed += () => OnPetButtonPressed(pet);
                
                _petListContainer.AddChild(btn);
                _petButtons.Add(btn);
                index++;
            }

            // 如果有选中的宠物，更新详情
            if (_selectedPet != null)
            {
                UpdatePetDetails(_selectedPet);
            }
            else if (pets.Count > 0)
            {
                OnPetButtonPressed(pets[0]);
            }
        }

        private void OnPetButtonPressed(ClawRPG.Scripts.Systems.Pets.Pet pet)
        {
            _selectedPet = pet;
            UpdatePetDetails(pet);
        }

        private void UpdatePetDetails(ClawRPG.Scripts.Systems.Pets.Pet pet)
        {
            if (pet == null) return;

            // 基本信息
            if (_petNameLabel != null)
                _petNameLabel.Text = pet.PetName;
            
            if (_petTypeLabel != null)
                _petTypeLabel.Text = $"类型: {GetPetTypeName(pet.Type)}";
            
            if (_petRarityLabel != null)
                _petRarityLabel.Text = $"稀有度: {GetRarityName(pet.Rarity)}";
            if (_petRarityLabel != null)
                _petRarityLabel.Modulate = GetRarityColor(pet.Rarity);
            
            if (_petLevelLabel != null)
                _petLevelLabel.Text = $"等级: {pet.Level}";
            
            if (_petExpLabel != null)
                _petExpLabel.Text = $"经验: {pet.Experience}/{pet.ExperienceToNextLevel}";
            
            if (_petLoyaltyLabel != null)
                _petLoyaltyLabel.Text = $"忠诚度: {pet.Loyalty}";
            
            // 进度条
            if (_expProgressBar != null)
            {
                _expProgressBar.Value = (float)pet.Experience / pet.ExperienceToNextLevel * 100;
            }
            
            if (_loyaltyProgressBar != null)
            {
                _loyaltyProgressBar.Value = pet.Loyalty;
            }

            // 属性加成
            if (_healthBonusLabel != null)
                _healthBonusLabel.Text = $"+{pet.GetTotalHealthBonus()}";
            
            if (_attackBonusLabel != null)
                _attackBonusLabel.Text = $"+{pet.GetTotalAttackBonus()}";
            
            if (_defenseBonusLabel != null)
                _defenseBonusLabel.Text = $"+{pet.GetTotalDefenseBonus()}";
            
            if (_speedBonusLabel != null)
                _speedBonusLabel.Text = $"+{pet.GetTotalSpeedBonus()}";
            
            if (_criticalBonusLabel != null)
                _criticalBonusLabel.Text = $"+{pet.GetTotalCriticalBonus()}";

            // 特殊能力
            if (_specialAbilityLabel != null)
            {
                if (!string.IsNullOrEmpty(pet.SpecialAbility))
                {
                    string abilityDesc = GetSpecialAbilityDescription(pet.SpecialAbility, pet.SpecialValue);
                    _specialAbilityLabel.Text = $"特殊: {abilityDesc}";
                }
                else
                {
                    _specialAbilityLabel.Text = "特殊: 无";
                }
            }

            // 更新按钮状态
            bool isActive = _petManager.ActivePet == (Pet)pet;
            if (_activateButton != null)
            {
                _activateButton.Text = isActive ? "已激活" : "激活";
                _activateButton.Disabled = isActive;
            }
        }

        private void OnActivatePressed()
        {
            if (_selectedPet != null)
            {
                _petManager.SetActivePet((Pet)_selectedPet);
                RefreshPetList();
            }
        }

        private void OnReleasePressed()
        {
            if (_selectedPet != null && _selectedPet != _petManager.ActivePet)
            {
                _petManager.RemovePet((Pet)_selectedPet);
                _selectedPet = null;
                RefreshPetList();
            }
        }

        private void OnPetListChanged(ClawRPG.Scripts.Systems.Pets.Pet pet)
        {
            RefreshPetList();
        }

        private void OnActivePetChanged(ClawRPG.Scripts.Systems.Pets.Pet pet)
        {
            RefreshPetList();
        }

        private Color GetRarityColor(PetRarity rarity)
        {
            return rarity switch
            {
                PetRarity.Common => Colors.White,
                PetRarity.Uncommon => Colors.Green,
                PetRarity.Rare => Colors.Blue,
                PetRarity.Epic => new Color(0.6f, 0.2f, 0.8f), // 紫色
                PetRarity.Legendary => new Color(1f, 0.5f, 0f), // 橙色
                _ => Colors.White
            };
        }

        private string GetRarityName(PetRarity rarity)
        {
            return rarity switch
            {
                PetRarity.Common => "普通",
                PetRarity.Uncommon => "优秀",
                PetRarity.Rare => "稀有",
                PetRarity.Epic => "史诗",
                PetRarity.Legendary => "传说",
                _ => "未知"
            };
        }

        private string GetPetTypeName(ClawRPG.Scripts.Systems.Pets.PetType type)
        {
            return type switch
            {
                Data.PetType.Companion => "伙伴",
                Data.PetType.Collector => "收藏家",
                Data.PetType.Guardian => "守护者",
                Data.PetType.Explorer => "探险家",
                _ => "未知"
            };
        }

        private string GetSpecialAbilityDescription(string ability, float value)
        {
            return ability switch
            {
                "auto_pickup" => $"自动拾取 +{value * 100:F0}%",
                "exp_boost" => $"经验加成 +{value * 100:F0}%",
                "drop_boost" => $"掉落加成 +{value * 100:F0}%",
                "damage_reduction" => $"伤害减免 +{value * 100:F0}%",
                "shield" => $"护盾 +{value * 100:F0}%",
                "fire_breath" => $"火焰吐息 +{value * 100:F0}%",
                "resurrect" => $"复活 +{value * 100:F0}%",
                "all_stats" => $"全属性 +{value * 100:F0}%",
                "holy_protection" => $"神圣保护 +{value * 100:F0}%",
                "lucky" => $"幸运 +{value * 100:F0}%",
                _ => "无"
            };
        }
    }
}
