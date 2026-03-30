using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 炼金界面 - GDScript UI绑定
    /// </summary>
    public partial class AlchemyUI : Control
    {
        // UI组件引用 (通过GetNode获取)
        private Label _titleLabel;
        private Label _levelLabel;
        private Label _experienceLabel;
        private ProgressBar _experienceBar;
        private OptionButton _recipeFilter;
        private ScrollContainer _recipeListContainer;
        private VBoxContainer _recipeList;
        private Label _selectedRecipeLabel;
        private Label _selectedDescriptionLabel;
        private Label _selectedMaterialsLabel;
        private Label _selectedCostLabel;
        private Label _goldLabel;
        private Button _craftButton;
        private Button _closeButton;
        private Label _messageLabel;

        private bool _isVisible = false; 
        private AlchemyRecipe _selectedRecipe;

        public override void _Ready()
        {
            InitializeUI();
            ConnectSignals();
            AlchemySystem.Instance.Initialize();
        }

        private void InitializeUI()
        {
            // 标题
            _titleLabel = GetNode<Label>("Panel/VBoxContainer/TitleLabel");
            if (_titleLabel != null) _titleLabel.Text = "炼金台";

            // 等级和经验
            _levelLabel = GetNode<Label>("Panel/VBoxContainer/LevelSection/LevelLabel");
            _experienceLabel = GetNode<Label>("Panel/VBoxContainer/LevelSection/ExperienceLabel");
            _experienceBar = GetNode<ProgressBar>("Panel/VBoxContainer/LevelSection/ExperienceBar");

            // 配方筛选
            _recipeFilter = GetNode<OptionButton>("Panel/VBoxContainer/FilterSection/RecipeFilter");
            if (_recipeFilter != null)
            {
                _recipeFilter.AddItem("全部配方");
                _recipeFilter.AddItem("已解锁");
                _recipeFilter.AddItem("可制作");
                _recipeFilter.ItemSelected += OnFilterChanged;
            }

            // 配方列表
            _recipeListContainer = GetNode<ScrollContainer>("Panel/VBoxContainer/RecipeListContainer");
            _recipeList = GetNode<VBoxContainer>("Panel/VBoxContainer/RecipeListContainer/RecipeList");

            // 详情面板
            _selectedRecipeLabel = GetNode<Label>("Panel/VBoxContainer/DetailsSection/RecipeNameLabel");
            _selectedDescriptionLabel = GetNode<Label>("Panel/VBoxContainer/DetailsSection/DescriptionLabel");
            _selectedMaterialsLabel = GetNode<Label>("Panel/VBoxContainer/DetailsSection/MaterialsLabel");
            _selectedCostLabel = GetNode<Label>("Panel/VBoxContainer/DetailsSection/CostLabel");

            // 金币显示
            _goldLabel = GetNode<Label>("Panel/VBoxContainer/GoldSection/GoldLabel");

            // 按钮
            _craftButton = GetNode<Button>("Panel/VBoxContainer/CraftButton");
            if (_craftButton != null)
            {
                _craftButton.Text = "制作";
                _craftButton.Pressed += OnCraftPressed;
            }

            _closeButton = GetNode<Button>("Panel/VBoxContainer/CloseButton");
            if (_closeButton != null)
            {
                _closeButton.Text = "关闭";
                _closeButton.Pressed += OnClosePressed;
            }

            // 消息
            _messageLabel = GetNode<Label>("Panel/VBoxContainer/MessageLabel");

            // 初始隐藏
            Visible = false; 
        }

        private void ConnectSignals()
        {
            AlchemySystem.Instance.OnCraftAttempt += OnCraftAttempt;
            AlchemySystem.Instance.OnLevelUp += OnLevelUp;
            AlchemySystem.Instance.OnRecipeUnlocked += OnRecipeUnlocked;
        }

        public override void _Process(double delta)
        {
            if (!_isVisible) return;
            
            UpdateUI();
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                RefreshRecipeList();
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            var playerData = AlchemySystem.Instance.PlayerData;
            var inventory = PlayerInventory.Instance;

            // 更新等级和经验
            if (_levelLabel != null)
                _levelLabel.Text = $"炼金等级: {playerData.AlchemyLevel}";
            
            if (_experienceLabel != null)
                _experienceLabel.Text = $"{playerData.CurrentExperience} / {playerData.ExperienceToNextLevel} 经验";
            
            if (_experienceBar != null)
            {
                float progress = (float)playerData.CurrentExperience / playerData.ExperienceToNextLevel;
                _experienceBar.Value = progress * 100;
            }

            // 更新金币
            if (_goldLabel != null)
                _goldLabel.Text = $"金币: {inventory.Gold}";

            // 更新制作按钮状态
            if (_craftButton != null && _selectedRecipe != null)
            {
                bool canCraft = AlchemySystem.Instance.CanCraft(_selectedRecipe.Id);
                _craftButton.Disabled = !canCraft;
            }
        }

        private void RefreshRecipeList()
        {
            if (_recipeList == null) return;

            // 清除现有项
            foreach (Node child in _recipeList.GetChildren())
            {
                child.QueueFree();
            }

            int filterIndex = _recipeFilter?.Selected ?? 0;
            var allRecipes = AlchemyDatabase.Instance.GetAllRecipes();
            var unlockedRecipes = AlchemySystem.Instance.GetUnlockedRecipes();

            foreach (var recipe in allRecipes)
            {
                bool shouldShow = false; 
                bool isUnlocked = AlchemySystem.Instance.IsRecipeUnlocked(recipe.Id);
                bool canCraft = AlchemySystem.Instance.CanCraft(recipe.Id);

                switch (filterIndex)
                {
                    case 0: // 全部
                        shouldShow = true;
                        break;
                    case 1: // 已解锁
                        shouldShow = isUnlocked;
                        break;
                    case 2: // 可制作
                        shouldShow = canCraft;
                        break;
                }

                if (shouldShow)
                {
                    var item = CreateRecipeItem(recipe, isUnlocked, canCraft);
                    _recipeList.AddChild(item);
                }
            }
        }

        private Control CreateRecipeItem(AlchemyRecipe recipe, bool isUnlocked, bool canCraft)
        {
            var container = new HBoxContainer();
            container.CustomMinimumSize = new Vector2(0, 40);

            // 状态图标
            var statusLabel = new Label();
            statusLabel.Text = isUnlocked ? (canCraft ? "✓" : "🔒") : "🔒";
            statusLabel.CustomMinimumSize = new Vector2(30, 0);
            container.AddChild(statusLabel);

            // 配方名称
            var nameLabel = new Label();
            nameLabel.Text = recipe.Name;
            nameLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
            container.AddChild(nameLabel);

            // 等级要求
            var levelLabel = new Label();
            levelLabel.Text = $"Lv.{recipe.RequiredAlchemyLevel}";
            levelLabel.CustomMinimumSize = new Vector2(50, 0);
            container.AddChild(levelLabel);

            // 制作按钮
            var craftBtn = new Button();
            craftBtn.Text = "制作";
            craftBtn.Disabled = !canCraft;
            craftBtn.Pressed += () => OnRecipeSelected(recipe);
            container.AddChild(craftBtn);

            // 点击整个项选择配方
            var clickable = new Button();
            clickable.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
            clickable.Modulate = new Color(1, 1, 1, 0); // 透明
            clickable.Pressed += () => OnRecipeSelected(recipe);
            container.AddChild(clickable);
            
            // 放到最底层
            container.MoveChild(clickable, 0);

            return container;
        }

        private void OnRecipeSelected(AlchemyRecipe recipe)
        {
            _selectedRecipe = recipe;
            UpdateRecipeDetails();
        }

        private void UpdateRecipeDetails()
        {
            if (_selectedRecipe == null) return;

            var recipe = _selectedRecipe;
            var inventory = PlayerInventory.Instance;

            // 名称
            if (_selectedRecipeLabel != null)
                _selectedRecipeLabel.Text = recipe.Name;

            // 描述
            if (_selectedDescriptionLabel != null)
                _selectedDescriptionLabel.Text = recipe.Description;

            // 材料需求
            var materialsText = "材料需求:\n";
            foreach (var req in recipe.Requirements)
            {
                var material = AlchemyDatabase.Instance.GetMaterial(req.MaterialId);
                if (material != null)
                {
                    int playerCount = inventory.GetItemCount(req.MaterialId);
                    bool hasEnough = playerCount >= req.Quantity;
                    string color = hasEnough ? "[color=green]" : "[color=red]";
                    materialsText += $"{color}{material.Name}[/color]: {playerCount}/{req.Quantity}\n";
                }
            }

            if (_selectedMaterialsLabel != null)
                _selectedMaterialsLabel.Text = materialsText;

            // 费用和成功率
            var costText = $"费用: {recipe.GoldCost} 金币\n";
            costText += $"成功率: {(recipe.SuccessRate * 100):F0}%\n";
            costText += $"制作时间: {recipe.CraftTime:F1}秒";

            if (_selectedCostLabel != null)
                _selectedCostLabel.Text = costText;

            UpdateUI();
        }

        private void OnFilterChanged(long index)
        {
            RefreshRecipeList();
        }

        private void OnCraftPressed()
        {
            if (_selectedRecipe == null) return;

            var success = AlchemySystem.Instance.TryCraft(
                _selectedRecipe.Id,
                out int itemId,
                out int quantity,
                out string message
            );

            if (_messageLabel != null)
            {
                _messageLabel.Text = message;
                _messageLabel.Modulate = success ? new Color(0, 1, 0) : new Color(1, 0, 0);
            }

            if (success)
            {
                RefreshRecipeList();
                UpdateRecipeDetails();
            }
        }

        private void OnClosePressed()
        {
            Toggle();
        }

        private void OnCraftAttempt(AlchemyRecipe recipe, bool success)
        {
            // 刷新UI
            RefreshRecipeList();
            UpdateRecipeDetails();
        }

        private void OnLevelUp(int newLevel)
        {
            if (_messageLabel != null)
            {
                _messageLabel.Text = $"升级了! 炼金等级 {newLevel}";
                _messageLabel.Modulate = new Color(1, 0.84, 0);
            }
            RefreshRecipeList();
        }

        private void OnRecipeUnlocked(AlchemyRecipe recipe)
        {
            if (_messageLabel != null)
            {
                _messageLabel.Text = $"解锁新配方: {recipe.Name}";
                _messageLabel.Modulate = new Color(0, 1, 1);
            }
            RefreshRecipeList();
        }

        // 快捷键处理
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && _isVisible)
            {
                Toggle();
            }
        }
    }
}
