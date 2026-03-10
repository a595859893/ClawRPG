using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Equipment set UI - displays active set bonuses
    /// </summary>
    public class EquipmentSetUI : Control
    {
        private Label _titleLabel;
        private VBoxContainer _setListContainer;
        private Label _noSetLabel;
        
        private bool _isVisible = false;
        
        public override void _Ready()
        {
            // Create main container
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainContainer.Position = new Vector2(400, 150);
            mainContainer.CustomMinimumSize = new Vector2(500, 500);
            AddChild(mainContainer);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "装备套装";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainContainer.AddChild(_titleLabel);
            
            // Separator
            var separator = new HSeparator();
            mainContainer.AddChild(separator);
            
            // Scroll container for sets
            var scrollContainer = new ScrollContainer();
            scrollContainer.CustomMinimumSize = new Vector2(480, 400);
            mainContainer.AddChild(scrollContainer);
            
            // Set list container
            _setListContainer = new VBoxContainer();
            _setListContainer.SetanchorsPreset(Control.LayoutPreset.FullRect);
            scrollContainer.AddChild(_setListContainer);
            
            // No set label
            _noSetLabel = new Label();
            _noSetLabel.Text = "暂无激活的套装效果\n\n装备套装装备可激活套装属性";
            _noSetLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _noSetLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            _noSetLabel.Position = new Vector2(150, 150);
            AddChild(_noSetLabel);
            
            // Close button
            var closeButton = new Button();
            closeButton.Text = "关闭 (ESC)";
            closeButton.Position = new Vector2(180, 560);
            closeButton.CustomMinimumSize = new Vector2(140, 40);
            closeButton.Pressed += () => HideSetUI();
            AddChild(closeButton);
            
            // Initially hide
            HideSetUI();
            GD.Print("EquipmentSetUI initialized");
        }
        
        public override void _Process(double delta)
        {
            if (_isVisible)
            {
                RefreshSetDisplay();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && _isVisible)
            {
                HideSetUI();
                GetTree().SetInputAsHandled();
            }
        }
        
        public void ToggleSetUI()
        {
            if (_isVisible)
            {
                HideSetUI();
            }
            else
            {
                ShowSetUI();
            }
        }
        
        public void ShowSetUI()
        {
            _isVisible = true;
            Visible = true;
            RefreshSetDisplay();
        }
        
        public void HideSetUI()
        {
            _isVisible = false;
            Visible = false;
        }
        
        private void RefreshSetDisplay()
        {
            // Clear existing
            foreach (Node child in _setListContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var setManager = EquipmentSetManager.Instance;
            if (setManager == null)
            {
                _noSetLabel.Visible = true;
                return;
            }
            
            var activeSets = setManager.GetActiveSetBonuses();
            
            if (activeSets.Count == 0)
            {
                _noSetLabel.Visible = true;
                return;
            }
            
            _noSetLabel.Visible = false;
            
            // Display each active set
            foreach (var activeSet in activeSets)
            {
                var setPanel = CreateSetPanel(activeSet);
                _setListContainer.AddChild(setPanel);
            }
            
            // Add separator at bottom
            var bottomSeparator = new HSeparator();
            bottomSeparator.Modulate = new Color(0.3f, 0.3f, 0.3f);
            _setListContainer.AddChild(bottomSeparator);
            
            // Total stats
            float damageBonus, defenseBonus, healthBonus, manaBonus;
            float critChance, critDamage, attackSpeed, moveSpeed;
            setManager.GetTotalSetBonusStats(out damageBonus, out defenseBonus, 
                out healthBonus, out manaBonus, out critChance, out critDamage, 
                out attackSpeed, out moveSpeed);
            
            var totalLabel = new Label();
            totalLabel.Text = $"总套装加成:\n" +
                $"  攻击: +{damageBonus:F1}%  防御: +{defenseBonus:F1}%\n" +
                $"  生命: +{healthBonus:F0}  法力: +{manaBonus:F0}\n" +
                $"  暴击率: +{critChance:F1}%  暴击伤害: +{critDamage:F1}%\n" +
                $"  攻击速度: +{attackSpeed:F1}%  移动速度: +{moveSpeed:F1}%";
            totalLabel.Modulate = new Color(1f, 0.9f, 0.5f);
            _setListContainer.AddChild(totalLabel);
        }
        
        private Control CreateSetPanel(ActiveSetBonus activeSet)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(460, 120);
            panel.Modulate = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            
            var vbox = new VBoxContainer();
            panel.AddChild(vbox);
            
            // Set name
            var nameLabel = new Label();
            nameLabel.Text = $"【{activeSet.Set.SetNameCN}】({activeSet.EquippedPieces}/5)";
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            
            // Color based on piece count
            if (activeSet.EquippedPieces >= 5)
            {
                nameLabel.Modulate = new Color(1f, 0.7f, 0.2f);  // Gold for full set
            }
            else if (activeSet.EquippedPieces >= 3)
            {
                nameLabel.Modulate = new Color(0.8f, 0.6f, 1f);  // Purple for 3+ pieces
            }
            else
            {
                nameLabel.Modulate = new Color(0.5f, 0.8f, 1f);  // Blue for 2 pieces
            }
            vbox.AddChild(nameLabel);
            
            // Set description
            var descLabel = new Label();
            descLabel.Text = activeSet.Set.Description;
            descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(descLabel);
            
            // Active bonus
            if (activeSet.ActiveBonus != null)
            {
                var bonusLabel = new Label();
                bonusLabel.Text = $"✓ 激活: {activeSet.ActiveBonus.BonusName} - {activeSet.ActiveBonus.Description}";
                bonusLabel.Modulate = new Color(0.3f, 0.9f, 0.4f);
                bonusLabel.AddThemeFontSizeOverride("font_size", 14);
                vbox.AddChild(bonusLabel);
            }
            
            // Progress bar for next bonus
            var progressContainer = new HBoxContainer();
            vbox.AddChild(progressContainer);
            
            // Piece indicators
            for (int i = 1; i <= 5; i++)
            {
                var pieceIndicator = new Label();
                if (i <= activeSet.EquippedPieces)
                {
                    pieceIndicator.Text = "●";
                    pieceIndicator.Modulate = new Color(0.2f, 0.9f, 0.4f);
                }
                else
                {
                    pieceIndicator.Text = "○";
                    pieceIndicator.Modulate = new Color(0.4f, 0.4f, 0.4f);
                }
                pieceIndicator.AddThemeFontSizeOverride("font_size", 16);
                progressContainer.AddChild(pieceIndicator);
                
                // Add bonus requirement label
                if (i == 2 || i == 3 || i == 5)
                {
                    var bonusText = new Label();
                    var setBonus = activeSet.Set.Bonuses.Find(b => b.RequiredPieceCount == i);
                    if (setBonus != null)
                    {
                        bonusText.Text = $" ({i}件: {setBonus.BonusName})";
                        bonusText.Modulate = new Color(0.6f, 0.6f, 0.6f);
                        bonusText.AddThemeFontSizeOverride("font_size", 10);
                        progressContainer.AddChild(bonusText);
                    }
                }
            }
            
            return panel;
        }
    }
}
