using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;
using ClawRPG.Systems.Alchemy;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 炼金界面 - GDScript UI绑定
    /// 重构自 REQ-075: 移除对 AlchemySystem/AlchemyDatabase 的直接引用，改为事件驱动解耦
    /// </summary>
    public partial class AlchemyUI : Control
    {
        // ===== 事件接口（UI → System 通信） =====
        // UI 层通过事件向外部/System 发送操作请求，不直接持有 System/Database 引用

        /// <summary>请求刷新配方列表（System 收到后调用 UpdatePlayerData + UpdateRecipeList）</summary>
        public Action OnRefreshRequested;

        /// <summary>请求制作配方（System 收到后处理，调用 UpdateCraftResult）</summary>
        public Action<int> OnCraftRequested;

        /// <summary>请求查看配方详情（System 收到后调用 UpdateRecipeDetails）</summary>
        public Action<int> OnRecipeDetailsRequested;

        // ===== UI组件引用 (通过GetNode获取) =====
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
        private List<AlchemyRecipe> _currentRecipes = new List<AlchemyRecipe>();
        private int _selectedRecipeId = -1;

        // ===== 生命周期 =====

        public override void _Ready()
        {
            InitializeUI();
            // 不再直接持有 System 引用
            // 初始化数据通过 OnRefreshRequested 事件请求
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

        // ===== 公开更新接口（System → UI 通信） =====
        // REQ-075 解耦：UI 不再主动拉取数据，而是等待外部推送

        /// <summary>
        /// 更新玩家炼金数据（由外部/System 调用）
        /// </summary>
        public void UpdatePlayerData(int alchemyLevel, int currentExp, int expToNext, int gold)
        {
            if (_levelLabel != null)
                _levelLabel.Text = $"炼金等级: {alchemyLevel}";

            if (_experienceLabel != null)
                _experienceLabel.Text = $"{currentExp} / {expToNext} 经验";

            if (_experienceBar != null && expToNext > 0)
            {
                float progress = (float)currentExp / expToNext;
                _experienceBar.Value = progress * 100;
            }

            if (_goldLabel != null)
                _goldLabel.Text = $"金币: {gold}";
        }

        /// <summary>
        /// 更新配方列表显示（由外部/System 调用）
        /// </summary>
        public void UpdateRecipeList(List<AlchemyRecipe> recipes, int filterIndex)
        {
            if (_recipeList == null) return;

            // 清除现有项
            foreach (Node child in _recipeList.GetChildren())
            {
                child.QueueFree();
            }

            _currentRecipes = recipes;

            foreach (var recipe in recipes)
            {
                bool shouldShow = false;

                switch (filterIndex)
                {
                    case 0: // 全部
                        shouldShow = true;
                        break;
                    case 1: // 已解锁
                        shouldShow = recipe.IsUnlocked;
                        break;
                    case 2: // 可制作
                        shouldShow = recipe.IsUnlocked && recipe.CanCraft;
                        break;
                }

                if (shouldShow)
                {
                    var item = CreateRecipeItem(recipe);
                    _recipeList.AddChild(item);
                }
            }
        }

        /// <summary>
        /// 更新配方详情显示（由外部/System 调用）
        /// </summary>
        public void UpdateRecipeDetails(int recipeId, string name, string description,
            int goldCost, float successRate, float craftTime,
            List<MaterialDisplayData> materials)
        {
            if (_selectedRecipeLabel != null)
                _selectedRecipeLabel.Text = name;

            if (_selectedDescriptionLabel != null)
                _selectedDescriptionLabel.Text = description;

            // 材料需求
            var materialsText = "材料需求:\n";
            foreach (var mat in materials)
            {
                string color = mat.HasEnough ? "[color=green]" : "[color=red]";
                materialsText += $"{color}{mat.Name}[/color]: {mat.PlayerCount}/{mat.Required}[/color]\n";
            }

            if (_selectedMaterialsLabel != null)
                _selectedMaterialsLabel.Text = materialsText;

            // 费用和成功率
            var costText = $"费用: {goldCost} 金币\n";
            costText += $"成功率: {(successRate * 100):F0}%\n";
            costText += $"制作时间: {craftTime:F1}秒";

            if (_selectedCostLabel != null)
                _selectedCostLabel.Text = costText;

            _selectedRecipeId = recipeId;
        }

        /// <summary>
        /// 更新制作结果消息（由外部/System 调用）
        /// </summary>
        public void UpdateCraftResult(bool success, string message)
        {
            if (_messageLabel != null)
            {
                _messageLabel.Text = message;
                _messageLabel.Modulate = success ? new Color(0, 1, 0) : new Color(1, 0, 0);
            }
        }

        /// <summary>
        /// 更新制作按钮状态（由外部/System 调用）
        /// </summary>
        public void UpdateCraftButton(bool canCraft)
        {
            if (_craftButton != null)
                _craftButton.Disabled = !canCraft;
        }

        /// <summary>
        /// 显示升级提示（由外部/System 调用）
        /// </summary>
        public void ShowLevelUp(int newLevel)
        {
            if (_messageLabel != null)
            {
                _messageLabel.Text = $"升级了! 炼金等级 {newLevel}";
                _messageLabel.Modulate = new Color(1, 0.84, 0);
            }
        }

        /// <summary>
        /// 显示解锁新配方提示（由外部/System 调用）
        /// </summary>
        public void ShowRecipeUnlocked(string recipeName)
        {
            if (_messageLabel != null)
            {
                _messageLabel.Text = $"解锁新配方: {recipeName}";
                _messageLabel.Modulate = new Color(0, 1, 1);
            }
        }

        // ===== 私有辅助数据结构 =====

        /// <summary>
        /// 配方列表项数据（由 System 传入，已包含材料数据）
        /// </summary>
        public class RecipeDisplayData
        {
            public AlchemyRecipe Recipe;
            public bool IsUnlocked;
            public bool CanCraft;
        }

        /// <summary>
        /// 材料显示数据（由 System 计算后传入）
        /// </summary>
        public class MaterialDisplayData
        {
            public string Name;
            public int PlayerCount;
            public int Required;
            public bool HasEnough;
        }

        // ===== 私有方法 =====

        private Control CreateRecipeItem(AlchemyRecipe recipe)
        {
            var container = new HBoxContainer();
            container.CustomMinimumSize = new Vector2(0, 40);

            // 状态图标
            var statusLabel = new Label();
            statusLabel.Text = recipe.IsUnlocked ? (recipe.CanCraft ? "✓" : "🔒") : "🔒";
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
            craftBtn.Disabled = !recipe.CanCraft;
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
            // 通过事件请求 System 提供详情数据，而不是直接查询
            OnRecipeDetailsRequested?.Invoke(recipe.Id);
        }

        private void OnFilterChanged(long index)
        {
            // 通过事件请求 System 重新提供过滤后的列表
            OnRefreshRequested?.Invoke();
        }

        private void OnCraftPressed()
        {
            if (_selectedRecipe == null) return;

            // 通过事件请求 System 处理制作逻辑
            // 而不是直接调用 AlchemySystem.Instance.TryCraft()
            OnCraftRequested?.Invoke(_selectedRecipe.Id);
        }

        private void OnClosePressed()
        {
            Toggle();
        }

        // ===== 公开方法 =====

        public void Toggle()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;

            if (_isVisible)
            {
                // 通过事件请求 System 提供初始数据
                OnRefreshRequested?.Invoke();
            }
        }

        // ===== 快捷键处理 =====

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && _isVisible)
            {
                Toggle();
            }
        }
    }
}
