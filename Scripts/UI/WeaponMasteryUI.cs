using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Weapon mastery UI - displays weapon proficiency and special attacks
    /// </summary>
    public partial class WeaponMasteryUI : Control
    {
        private Control _panel;
        private Label _titleLabel;
        private VBoxContainer _masteryContainer;
        private Label _currentWeaponLabel;
        private ProgressBar _masteryProgressBar;
        private Label _masteryLevelLabel;
        private Label _specialAttacksLabel;
        
        private bool _isVisible = false; 
        
        // Weapon buttons
        private Dictionary<WeaponType, Button> _weaponButtons = new();
        
        public override void _Ready()
        {
            Visible = false; 
            _CreateUI();
            
            // Connect to input
        }
        
        private void _CreateUI()
        {
            // Main panel
            _panel = new Panel
            {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 50,
                OffsetTop = 50,
                OffsetRight = -500,
                OffsetBottom = -50,
                CustomMinimumSize = new Vector2(400, 0)
            };
            AddChild(_panel);
            
            // Title
            _titleLabel = new Label
            {
                Text = "⚔️ 武器熟练度",
                HorizontalAlignment = HorizontalAlignment.Center,
                OffsetTop = 10
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _panel.AddChild(_titleLabel);
            
            // Current weapon display
            _currentWeaponLabel = new Label
            {
                Text = "当前武器: 剑",
                HorizontalAlignment = HorizontalAlignment.Center,
                OffsetTop = 50
            };
            _currentWeaponLabel.AddThemeFontSizeOverride("font_size", 18);
            _panel.AddChild(_currentWeaponLabel);
            
            // Mastery progress bar
            _masteryProgressBar = new ProgressBar
            {
                OffsetTop = 80,
                OffsetLeft = 20,
                OffsetRight = -20,
                OffsetBottom = 110,
                CustomMinimumSize = new Vector2(0, 20),
                Value = 0
            };
            _panel.AddChild(_masteryProgressBar);
            
            // Mastery level label
            _masteryLevelLabel = new Label
            {
                Text = "等级 1 (0%)",
                HorizontalAlignment = HorizontalAlignment.Center,
                OffsetTop = 115
            };
            _masteryLevelLabel.AddThemeFontSizeOverride("font_size", 16);
            _panel.AddChild(_masteryLevelLabel);
            
            // Weapon type buttons container
            var buttonContainer = new HBoxContainer
            {
                OffsetTop = 150,
                OffsetLeft = 20,
                OffsetRight = -20,
                OffsetBottom = 190
            };
            _panel.AddChild(buttonContainer);
            
            // Create weapon type buttons
            string[] weaponEmojis = {"⚔️", "🪓", "🗡️", "🔮", "🏹", "🔨", "🛡️"};
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                var btn = new Button
                {
                    Text = weaponEmojis[(int)type],
                    SizeFlagsHorizontal = SizeFlags.Expand,
                    CustomMinimumSize = new Vector2(50, 40)
                };
                btn.Pressed += () => _OnWeaponButtonPressed(type);
                buttonContainer.AddChild(btn);
                _weaponButtons[type] = btn;
            }
            
            // Mastery list container
            _masteryContainer = new VBoxContainer
            {
                OffsetTop = 200,
                OffsetLeft = 20,
                OffsetRight = -20,
                OffsetBottom = -20
            };
            _panel.AddChild(_masteryContainer);
            
            // Special attacks section
            _specialAttacksLabel = new Label
            {
                Text = "🎯 特殊攻击 (需等级解锁)",
                OffsetTop = 350,
                OffsetLeft = 20
            };
            _specialAttacksLabel.AddThemeFontSizeOverride("font_size", 16);
            _panel.AddChild(_specialAttacksLabel);
            
            // Update masteries display
            _UpdateMasteryDisplay();
        }
        
        private void _OnWeaponButtonPressed(WeaponType type)
        {
            WeaponMasterySystem.Instance.SwitchWeapon(type);
            _UpdateMasteryDisplay();
        }
        
        private void _UpdateMasteryDisplay()
        {
            if (WeaponMasterySystem.Instance == null) return;
            
            // Update current weapon label
            string[] weaponNames = {"剑", "斧", "匕首", "法杖", "弓", "锤", "盾"};
            _currentWeaponLabel.Text = $"当前武器: {weaponNames[(int)WeaponMasterySystem.Instance.CurrentWeaponType]}";
            
            // Update progress bar
            _masteryProgressBar.Value = WeaponMasterySystem.Instance.GetCurrentMasteryProgress() * 100;
            
            // Update level label
            int level = WeaponMasterySystem.Instance.GetCurrentMasteryLevel();
            int bonus = (int)(WeaponMasterySystem.Instance.GetMasteryDamageBonus(WeaponMasterySystem.Instance.CurrentWeaponType) * 100);
            _masteryLevelLabel.Text = $"等级 {level} (+{bonus}%伤害)";
            
            // Clear and rebuild mastery list
            foreach (var child in _masteryContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            foreach (var kvp in WeaponMasterySystem.Instance.Masteries)
            {
                var mastery = kvp.Value;
                string name = weaponNames[(int)mastery.Type];
                int dmgBonus = (int)(mastery.DamageBonus * 100);
                string isCurrent = mastery.Type == WeaponMasterySystem.Instance.CurrentWeaponType ? " ★" : "";
                
                var label = new Label
                {
                    Text = $"{name}: Lv.{mastery.Level} (+{dmgBonus}%伤害){isCurrent}"
                };
                _masteryContainer.AddChild(label);
            }
            
            // Update special attacks based on player level (would need to get from Player)
            int playerLevel = 1;  // Default, should be from Player
            var playerNode = GetTree().GetFirstNodeInGroup("player");
            if (playerNode != null && playerNode is Characters.Player player)
            {
                playerLevel = player.Level;
            }
            
            string specialText = "🎯 特殊攻击 (需等级解锁)\n";
            specialText += $"• 重击 (按住攻击): Lv.{WeaponMasterySystem.Instance.MinSkillLevelForHeavyStrike}+\n";
            specialText += $"• 快速斩 (双击): Lv.{WeaponMasterySystem.Instance.MinSkillLevelForQuickSlash}+\n";
            specialText += $"• 旋风斩 (Q键): Lv.{WeaponMasterySystem.Instance.MinSkillLevelForSpinAttack}+\n";
            specialText += $"• 冲锋 (E键): Lv.{WeaponMasterySystem.Instance.MinSkillLevelForCharge}+";
            
            _specialAttacksLabel.Text = specialText;
        }
        
        public override void _Process(double delta)
        {
            // Update display when visible
            if (_isVisible)
            {
                _UpdateMasteryDisplay();
            }
        }
        
        public void Toggle()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                _UpdateMasteryDisplay();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Cleanup if needed
            }
            base.Dispose(disposing);
        }
    }
}
