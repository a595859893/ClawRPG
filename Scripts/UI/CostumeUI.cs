using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Costume UI - costume shop and equipment interface
    /// </summary>
    public class CostumeUI : Control
    {
        private Control _mainPanel;
        private VBoxContainer _costumeList;
        private Label _goldLabel;
        private Label _titleLabel;
        
        // Category tabs
        private Button _outfitTab;
        private Button _hatTab;
        private Button _weaponSkinTab;
        private Button _effectTab;
        private Button _trailTab;
        
        private CostumeCategory _currentCategory = CostumeCategory.Outfit;
        private List<CostumeData> _currentCostumes = new();
        
        public override void _Ready()
        {
            SetupUI();
            RefreshCostumeList();
        }
        
        private void SetupUI()
        {
            // Main panel
            _mainPanel = new Control();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);
            
            var bg = new Panel();
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            bg.Modulate = new Color(0, 0, 0, 0.8);
            _mainPanel.AddChild(bg);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "时装系统";
            _titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.Position = new Vector2(0, 20);
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _mainPanel.AddChild(_titleLabel);
            
            // Close button
            var closeBtn = new Button();
            closeBtn.Text = "X";
            closeBtn.Position = new Vector2(750, 20);
            closeBtn.Size = new Vector2(30, 30);
            closeBtn.Pressed += () => Hide();
            _mainPanel.AddChild(closeBtn);
            
            // Category tabs
            var tabContainer = new HBoxContainer();
            tabContainer.Position = new Vector2(50, 80);
            tabContainer.Spacing = 10;
            _mainPanel.AddChild(tabContainer);
            
            _outfitTab = CreateTabButton("服装", CostumeCategory.Outfit);
            _hatTab = CreateTabButton("帽子", CostumeCategory.Hat);
            _weaponSkinTab = CreateTabButton("武器外观", CostumeCategory.WeaponSkin);
            _effectTab = CreateTabButton("特效", CostumeCategory.Effect);
            _trailTab = CreateTabButton("拖尾", CostumeCategory.Trail);
            
            tabContainer.AddChild(_outfitTab);
            tabContainer.AddChild(_hatTab);
            tabContainer.AddChild(_weaponSkinTab);
            tabContainer.AddChild(_effectTab);
            tabContainer.AddChild(_trailTab);
            
            // Costume list
            var scroll = new ScrollContainer();
            scroll.Position = new Vector2(50, 140);
            scroll.Size = new Vector2(700, 400);
            _mainPanel.AddChild(scroll);
            
            _costumeList = new VBoxContainer();
            _costumeList.Size = new Vector2(680, 400);
            _costumeList.Spacing = 10;
            scroll.AddChild(_costumeList);
            
            // Gold label
            _goldLabel = new Label();
            _goldLabel.Text = "金币: 0";
            _goldLabel.Position = new Vector2(50, 560);
            _goldLabel.AddThemeFontSizeOverride("font_size", 20);
            _mainPanel.AddChild(_goldLabel);
            
            UpdateGoldDisplay();
            
            // Initial category
            _outfitTab.ButtonPressed = true;
        }
        
        private Button CreateTabButton(string text, CostumeCategory category)
        {
            var btn = new Button();
            btn.Text = text;
            btn.Size = new Vector2(120, 40);
            btn.Pressed += () => OnCategorySelected(category);
            return btn;
        }
        
        private void OnCategorySelected(CostumeCategory category)
        {
            _currentCategory = category;
            RefreshCostumeList();
        }
        
        private void RefreshCostumeList()
        {
            // Clear list
            foreach (Node child in _costumeList.GetChildren())
            {
                child.QueueFree();
            }
            
            // Get costumes for category
            _currentCostumes = CostumeDatabase.Instance.GetCostumesByCategory(_currentCategory);
            
            foreach (var costume in _costumeList)
            {
                AddCostumeItem(costume);
            }
        }
        
        private void AddCostumeItem(CostumeData costume)
        {
            var itemPanel = new PanelContainer();
            itemPanel.CustomMinimumSize = new Vector2(650, 80);
            _costumeList.AddChild(itemPanel);
            
            var hbox = new HBoxContainer();
            hbox.Spacing = 20;
            itemPanel.AddChild(hbox);
            
            // Icon placeholder
            var icon = new TextureRect();
            icon.CustomMinimumSize = new Vector2(60, 60);
            icon.Modulate = GetQualityColor(costume);
            hbox.AddChild(icon);
            
            // Info
            var infoVBox = new VBoxContainer();
            infoVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hbox.AddChild(infoVBox);
            
            var nameLabel = new Label();
            nameLabel.Text = costume.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            infoVBox.AddChild(nameLabel);
            
            var descLabel = new Label();
            descLabel.Text = costume.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 14);
            descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            infoVBox.AddChild(descLabel);
            
            // Status/Action button
            var actionBtn = new Button();
            actionBtn.CustomMinimumSize = new Vector2(120, 40);
            
            if (costume.IsPurchased)
            {
                if (costume.IsEquipped)
                {
                    actionBtn.Text = "已装备";
                    actionBtn.Disabled = true;
                }
                else
                {
                    actionBtn.Text = "穿戴";
                    actionBtn.Pressed += () => OnEquipPressed(costume);
                }
            }
            else
            {
                actionBtn.Text = $"购买 ({costume.Cost})";
                actionBtn.Pressed += () => OnPurchasePressed(costume);
            }
            
            hbox.AddChild(actionBtn);
        }
        
        private void OnPurchasePressed(CostumeData costume)
        {
            if (CostumeSystem.Instance.PurchaseCostume(costume.Id))
            {
                UpdateGoldDisplay();
                RefreshCostumeList();
            }
            else
            {
                // Show error message (gold insufficient, etc.)
                GD.Print("Purchase failed");
            }
        }
        
        private void OnEquipPressed(CostumeData costume)
        {
            if (CostumeSystem.Instance.EquipCostume(costume.Id))
            {
                RefreshCostumeList();
            }
        }
        
        private void UpdateGoldDisplay()
        {
            var player = GetTree().CurrentScene?.GetNode<Player>("Player");
            if (player != null)
            {
                _goldLabel.Text = $"金币: {player.Gold}";
            }
        }
        
        private Color GetQualityColor(CostumeData costume)
        {
            // Default color based on rarity/cost
            if (costume.IsDefault) return new Color(0.5f, 0.5f, 0.5f);
            if (costume.Cost >= 1000) return new Color(1f, 0.6f, 0f); // Orange - legendary
            if (costume.Cost >= 500) return new Color(0.8f, 0.4f, 1f); // Purple - epic
            if (costume.Cost >= 250) return new Color(0.4f, 0.6f, 1f); // Blue - rare
            if (costume.Cost >= 100) return new Color(0.4f, 0.8f, 0.4f); // Green - uncommon
            return Color.White;
        }
        
        public void Show()
        {
            Visible = true;
            UpdateGoldDisplay();
            RefreshCostumeList();
        }
        
        public void Hide()
        {
            Visible = false;
        }
        
        public void Toggle()
        {
            if (Visible)
                Hide();
            else
                Show();
        }
    }
}
