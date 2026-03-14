using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Systems.CraftingMastery;

namespace ClawRPG.UI
{
    /// <summary>
    /// UI for crafting mastery system
    /// </summary>
    public class CraftingMasteryUI : Control
    {
        private static CraftingMasteryUI _instance;
        public static CraftingMasteryUI Instance => _instance;
        
        // UI Components
        private VBoxContainer _mainContainer;
        private TabContainer _tabContainer;
        
        // Category panel
        private GridContainer _categoryGrid;
        
        // Recipe panel
        private VBoxContainer _recipeList;
        private Label _recipeDetailLabel;
        
        // Statistics panel
        private Label _statsLabel;
        
        // Current category
        private CraftingCategory _currentCategory = CraftingCategory.Blacksmithing;
        private bool _isVisible = false;
        
        public override void _Ready()
        {
            _instance = this;
            SetupUI();
            Hide();
            GD.Print("[CraftingMasteryUI] Initialized");
        }
        
        private void SetupUI()
        {
            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
            _mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(_mainContainer);
            
            // Header
            var header = new Label();
            header.Text = "🎨 制作大师系统";
            header.Align = Label.AlignEnum.Center;
            header.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(header);
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SetVSizeFlags(Control.SizeFlags.ExpandFill);
            _mainContainer.AddChild(_tabContainer);
            
            // Create tabs
            CreateCategoryTab();
            CreateRecipesTab();
            CreateStatisticsTab();
            
            // Close button
            var closeButton = new Button();
            closeButton.Text = "关闭 (ESC)";
            closeButton.Align = Button.AlignEnum.Center;
            closeButton.Connect("pressed", this, nameof(OnClosePressed));
            _mainContainer.AddChild(closeButton);
        }
        
        private void CreateCategoryTab()
        {
            var tab = new ScrollContainer();
            tab.Name = "制作分类";
            _tabContainer.AddChild(tab);
            
            _categoryGrid = new GridContainer();
            _categoryGrid.Columns = 2;
            _categoryGrid.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
            _categoryGrid.AddThemeConstantOverride("h_separation", 10);
            _categoryGrid.AddThemeConstantOverride("v_separation", 10);
            tab.AddChild(_categoryGrid);
            
            RefreshCategoryButtons();
        }
        
        private void RefreshCategoryButtons()
        {
            // Clear existing
            foreach (var child in _categoryGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            // Add category buttons
            foreach (CraftingCategory cat in Enum.GetValues(typeof(CraftingCategory)))
            {
                var config = CraftingMasteryDatabase.Instance.GetCategoryConfig(cat);
                if (config == null) continue;
                
                var btn = new Button();
                btn.CustomMinimumSize = new Vector2(200, 80);
                
                var mastery = CraftingMasterySystem.Instance.GetCategoryMastery(cat);
                int level = mastery?.Level ?? 1;
                int totalCrafts = mastery?.TotalCrafts ?? 0;
                
                btn.Text = $"{config.Icon} {config.DisplayName}\n等级: {level} | 制作次数: {totalCrafts}";
                btn.Connect("pressed", this, nameof(OnCategorySelected), new Godot.Collections.Array { cat });
                
                _categoryGrid.AddChild(btn);
            }
        }
        
        private void CreateRecipesTab()
        {
            var tab = new VBoxContainer();
            tab.Name = "配方列表";
            _tabContainer.AddChild(tab);
            
            // Recipe list scroll
            var scroll = new ScrollContainer();
            scroll.SetVSizeFlags(Control.SizeFlags.ExpandFill);
            tab.AddChild(scroll);
            
            _recipeList = new VBoxContainer();
            _recipeList.AddThemeConstantOverride("separation", 5);
            scroll.AddChild(_recipeList);
            
            // Recipe detail
            var detailScroll = new ScrollContainer();
            detailScroll.SetVSizeFlags(Control.SizeFlags.ExpandFill);
            detailScroll.CustomMinimumSize = new Vector2(0, 150);
            tab.AddChild(detailScroll);
            
            _recipeDetailLabel = new Label();
            _recipeDetailLabel.Text = "选择一个配方查看详情";
            _recipeDetailLabel.Autowrap = true;
            detailScroll.AddChild(_recipeDetailLabel);
            
            // Craft button
            var craftButton = new Button();
            craftButton.Text = "开始制作";
            craftButton.Align = Button.AlignEnum.Center;
            craftButton.Connect("pressed", this, nameof(OnCraftPressed));
            tab.AddChild(craftButton);
            
            RefreshRecipeList();
        }
        
        private void RefreshRecipeList()
        {
            // Clear existing
            foreach (var child in _recipeList.GetChildren())
            {
                child.QueueFree();
            }
            
            var recipes = CraftingMasterySystem.Instance.GetRecipesForCategory(_currentCategory);
            
            foreach (var recipe in recipes)
            {
                var btn = new Button();
                btn.CustomMinimumSize = new Vector2(0, 50);
                
                var mastery = CraftingMasterySystem.Instance.GetCategoryMastery(_currentCategory);
                int masteryLevel = mastery?.Level ?? 1;
                bool isUnlocked = masteryLevel >= recipe.RequiredMasteryLevel;
                
                string status = isUnlocked ? "✓ 可制作" : "🔒 需要大师等级 " + recipe.RequiredMasteryLevel;
                string difficulty = recipe.Difficulty.ToString();
                
                btn.Text = $"[{difficulty}] {recipe.Name} - {status}";
                btn.Disabled = !isUnlocked;
                btn.Connect("pressed", this, nameof(OnRecipeSelected), new Godot.Collections.Array { recipe.Id });
                
                _recipeList.AddChild(btn);
            }
        }
        
        private void CreateStatisticsTab()
        {
            var tab = new ScrollContainer();
            tab.Name = "统计";
            _tabContainer.AddChild(tab);
            
            _statsLabel = new Label();
            _statsLabel.Autowrap = true;
            _statsLabel.Text = "加载统计中...";
            tab.AddChild(_statsLabel);
            
            RefreshStatistics();
        }
        
        private void RefreshStatistics()
        {
            var stats = CraftingMasterySystem.Instance.GetStatistics();
            
            string text = "=== 制作统计 ===\n\n";
            text += $"总制作次数: {stats.TotalCrafts}\n";
            text += $"成功次数: {stats.SuccessfulCrafts}\n";
            text += $"失败次数: {stats.FailedCrafts}\n";
            text += $"杰作次数: {stats.MasterpieceCrafts}\n";
            text += $"平均成功率: {stats.AverageSuccessRate:P1}\n";
            text += $"最高连击: {stats.BestStreak}\n\n";
            
            text += "=== 分类统计 ===\n\n";
            foreach (var cat in stats.CategoryCraftCounts.Keys)
            {
                var config = CraftingMasteryDatabase.Instance.GetCategoryConfig(cat);
                int crafts = stats.CategoryCraftCounts[cat];
                int level = stats.CategoryMasteryLevels.ContainsKey(cat) ? stats.CategoryMasteryLevels[cat] : 1;
                text += $"{config.Icon} {config.DisplayName}: {crafts} 次制作, 等级 {level}\n";
            }
            
            if (!string.IsNullOrEmpty(stats.MostUsedRecipe))
            {
                var recipe = CraftingMasteryDatabase.Instance.GetRecipe(stats.MostUsedRecipe);
                text += $"\n最常用配方: {recipe?.Name ?? stats.MostUsedRecipe} ({stats.MostUsedRecipeCount} 次)\n";
            }
            
            _statsLabel.Text = text;
        }
        
        private void OnCategorySelected(CraftingCategory category)
        {
            _currentCategory = category;
            RefreshRecipeList();
            
            // Update category display
            var mastery = CraftingMasterySystem.Instance.GetCategoryMastery(category);
            var config = CraftingMasteryDatabase.Instance.GetCategoryConfig(category);
            
            GD.Print($"[CraftingMasteryUI] Selected category: {config.DisplayName}, Level: {mastery?.Level}");
        }
        
        private string _selectedRecipeId = "";
        
        private void OnRecipeSelected(string recipeId)
        {
            _selectedRecipeId = recipeId;
            var recipe = CraftingMasteryDatabase.Instance.GetRecipe(recipeId);
            
            if (recipe == null)
            {
                _recipeDetailLabel.Text = "配方不存在";
                return;
            }
            
            string text = $"=== {recipe.Name} ===\n\n";
            text += $"分类: {recipe.Category}\n";
            text += $"难度: {recipe.Difficulty}\n";
            text += $"需要等级: {recipe.RequiredLevel}\n";
            text += $"需要大师等级: {recipe.RequiredMasteryLevel}\n";
            text += $"基础成功率: {recipe.SuccessRate:P0}\n";
            text += $"经验奖励: {recipe.ExperienceReward}\n";
            text += $"杰作额外经验: {recipe.MasterpieceBonusExp}\n\n";
            
            text += "=== 材料 ===\n";
            foreach (var comp in recipe.Components)
            {
                text += $"- {comp.ItemName}: {comp.Quantity}\n";
            }
            
            _recipeDetailLabel.Text = text;
        }
        
        private void OnCraftPressed()
        {
            if (string.IsNullOrEmpty(_selectedRecipeId))
            {
                GD.Print("[CraftingMasteryUI] No recipe selected");
                return;
            }
            
            bool started = CraftingMasterySystem.Instance.StartCrafting(_selectedRecipeId);
            if (started)
            {
                GD.Print($"[CraftingMasteryUI] Started crafting: {_selectedRecipeId}");
                // Refresh after a delay
                CallDeferred(nameof(RefreshAfterCraft));
            }
        }
        
        private void RefreshAfterCraft()
        {
            // Wait a bit then refresh
            yield return new GodotObject();
            RefreshStatistics();
        }
        
        private void OnClosePressed()
        {
            Hide();
            _isVisible = false;
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Scancode == (uint)KeyList.Escape)
                {
                    if (_isVisible)
                    {
                        Hide();
                        _isVisible = false;
                    }
                }
                else if (keyEvent.Scancode == (uint)KeyList.C && keyEvent.Control)
                {
                    ToggleUI();
                }
            }
        }
        
        public void ToggleUI()
        {
            if (_isVisible)
            {
                Hide();
                _isVisible = false;
            }
            else
            {
                Show();
                RefreshAll();
                _isVisible = true;
            }
        }
        
        public void RefreshAll()
        {
            RefreshCategoryButtons();
            RefreshRecipeList();
            RefreshStatistics();
        }
        
        public bool IsVisible() => _isVisible;
    }
}
