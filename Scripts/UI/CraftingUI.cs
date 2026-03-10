using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Crafting UI - displays crafting interface
    /// </summary>
    public class CraftingUI : Control
    {
        // UI Elements
        private VBoxContainer _mainContainer;
        private VBoxContainer _recipeList;
        private VBoxContainer _materialList;
        private Label _recipeNameLabel;
        private Label _recipeDescLabel;
        private Label _resultLabel;
        private Button _craftButton;
        private Button _closeButton;
        
        // Crafting station tabs
        private HBoxContainer _stationTabs;
        private Button _tabForge;
        private Button _tabAlchemy;
        private Button _tabEnchant;
        
        // State
        private string _currentStation = "forge";
        private CraftingRecipe _selectedRecipe;
        private bool _isVisible = false;
        
        // References
        private Player _player;
        private CraftingManager _craftingManager;
        
        public override void _Ready()
        {
            SetupUI();
            _craftingManager = CraftingManager.Instance;
            
            // Subscribe to events
            CraftingManager.OnCraftingSuccess += OnCraftingSuccess;
            CraftingManager.OnCraftingFailed += OnCraftingFailed;
            
            Hide();
        }
        
        private void SetupUI()
        {
            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainContainer.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainContainer);
            
            // Title bar
            var titleBar = new HBoxContainer();
            _mainContainer.AddChild(titleBar);
            
            var titleLabel = new Label();
            titleLabel.Text = "合成系统";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(titleLabel);
            
            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand }); // Spacer
            
            _closeButton = new Button();
            _closeButton.Text = "X";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _closeButton.Pressed += () => Hide();
            titleBar.AddChild(_closeButton);
            
            // Station tabs
            _stationTabs = new HBoxContainer();
            _mainContainer.AddChild(_stationTabs);
            
            _tabForge = new Button();
            _tabForge.Text = "锻造台";
            _tabForge.Pressed += () => SelectStation("forge");
            _stationTabs.AddChild(_tabForge);
            
            _tabAlchemy = new Button();
            _tabAlchemy.Text = "炼金台";
            _tabAlchemy.Pressed += () => SelectStation("alchemy");
            _stationTabs.AddChild(_tabAlchemy);
            
            _tabEnchant = new Button();
            _tabEnchant.Text = "附魔台";
            _tabEnchant.Pressed += () => SelectStation("enchant");
            _stationTabs.AddChild(_tabEnchant);
            
            // Content area
            var contentArea = new HBoxContainer();
            contentArea.SizeFlagsVertical = Control.SizeFlags.Expand;
            _mainContainer.AddChild(contentArea);
            
            // Recipe list (left)
            var recipeScroll = new ScrollContainer();
            recipeScroll.CustomMinimumSize = new Vector2(250, 0);
            contentArea.AddChild(recipeScroll);
            
            _recipeList = new VBoxContainer();
            recipeScroll.AddChild(_recipeList);
            
            // Details area (center)
            var detailsArea = new VBoxContainer();
            detailsArea.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            contentArea.AddChild(detailsArea);
            
            _recipeNameLabel = new Label();
            _recipeNameLabel.AddThemeFontSizeOverride("font_size", 20);
            _recipeNameLabel.Text = "选择一个配方";
            detailsArea.AddChild(_recipeNameLabel);
            
            _recipeDescLabel = new Label();
            _recipeDescLabel.Text = "";
            _recipeDescLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            detailsArea.AddChild(_recipeDescLabel);
            
            // Materials required
            var materialsTitle = new Label();
            materialsTitle.Text = "所需材料:";
            materialsTitle.AddThemeFontSizeOverride("font_size", 16);
            detailsArea.AddChild(materialsTitle);
            
            _materialList = new VBoxContainer();
            detailsArea.AddChild(_materialList);
            
            // Result preview
            var resultTitle = new Label();
            resultTitle.Text = "制作结果:";
            resultTitle.AddThemeFontSizeOverride("font_size", 16);
            detailsArea.AddChild(resultTitle);
            
            _resultLabel = new Label();
            _resultLabel.Text = "";
            detailsArea.AddChild(_resultLabel);
            
            detailsArea.AddChild(new Control() { SizeFlagsVertical = Control.SizeFlags.Expand }); // Spacer
            
            // Craft button
            _craftButton = new Button();
            _craftButton.Text = "合成";
            _craftButton.CustomMinimumSize = new Vector2(200, 50);
            _craftButton.Pressed += OnCraftPressed;
            _craftButton.Disabled = true;
            detailsArea.AddChild(_craftButton);
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Toggle with C key
                if (keyEvent.Keycode == Key.C && !keyEvent.Echo)
                {
                    Toggle();
                }
            }
        }
        
        public void Toggle()
        {
            if (_isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        
        public override void _Notification(int what)
        {
            if (what == NotificationVisibilityChanged)
            {
                _isVisible = Visible;
                if (_isVisible)
                {
                    RefreshRecipeList();
                }
            }
        }
        
        private void SelectStation(string station)
        {
            _currentStation = station;
            RefreshRecipeList();
        }
        
        private void RefreshRecipeList()
        {
            // Clear existing
            foreach (var child in _recipeList.GetChildren())
            {
                child.QueueFree();
            }
            
            // Get recipes for current station
            var recipes = RecipeDatabase.Instance.GetRecipesByStation(_currentStation);
            
            foreach (var recipe in recipes)
            {
                var btn = new Button();
                btn.Text = recipe.Name;
                btn.TooltipText = $"{recipe.Description}\n等级要求: {recipe.RequiredLevel}";
                btn.Pressed += () => SelectRecipe(recipe);
                _recipeList.AddChild(btn);
            }
        }
        
        private void SelectRecipe(CraftingRecipe recipe)
        {
            _selectedRecipe = recipe;
            
            _recipeNameLabel.Text = recipe.Name;
            _recipeDescLabel.Text = recipe.Description;
            
            // Show materials
            foreach (var child in _materialList.GetChildren())
            {
                child.QueueFree();
            }
            
            foreach (var material in recipe.Materials)
            {
                var item = ItemDatabase.Instance.GetItem(material.Key);
                var label = new Label();
                bool hasEnough = HasMaterial(material.Key, material.Value);
                string color = hasEnough ? "[color=green]" : "[color=red]";
                label.Text = $"{color}- {item?.Name ?? "Unknown"} x{material.Value}[/color]";
                _materialList.AddChild(label);
            }
            
            // Show result
            var resultItem = ItemDatabase.Instance.GetItem(recipe.ResultItemId);
            _resultLabel.Text = $"{resultItem?.Name ?? "Unknown"} x{recipe.ResultQuantity}";
            
            // Update craft button
            UpdateCraftButton();
        }
        
        private bool HasMaterial(int itemId, int quantity)
        {
            // Check if player has enough materials
            // This would integrate with inventory
            return false; // Placeholder
        }
        
        private void UpdateCraftButton()
        {
            if (_selectedRecipe == null)
            {
                _craftButton.Disabled = true;
                return;
            }
            
            // Check if can craft
            bool canCraft = true;
            foreach (var material in _selectedRecipe.Materials)
            {
                if (!HasMaterial(material.Key, material.Value))
                {
                    canCraft = false;
                    break;
                }
            }
            
            _craftButton.Disabled = !canCraft;
        }
        
        private void OnCraftPressed()
        {
            if (_selectedRecipe == null) return;
            
            // Attempt to craft
            var inventory = GetInventory();
            if (inventory != null)
            {
                int playerLevel = 1; // Get from player
                bool success = _craftingManager.Craft(inventory, _selectedRecipe.Id, playerLevel);
                
                if (success)
                {
                    RefreshRecipeList();
                    UpdateCraftButton();
                }
            }
        }
        
        private ClawRPG.Scripts.Items.Inventory GetInventory()
        {
            // Get player's inventory
            return null; // Placeholder - would get from Player
        }
        
        private void OnCraftingSuccess(CraftingRecipe recipe, int quantity)
        {
            // Show success notification
            GD.Print($"合成成功: {recipe.Name} x{quantity}");
        }
        
        private void OnCraftingFailed(string message)
        {
            // Show error message
            GD.Print($"合成失败: {message}");
        }
    }
}
