using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Skills;
using ClawRPG.Scripts.Characters;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Skill Tree UI - Visual skill tree interface with animations
    /// </summary>
    public partial class SkillTreeUI : Control
    {
        [Export] public Key keyToggle = Key.K;
        
        private Control _mainPanel;
        private VBoxContainer _skillTreesContainer;
        private Label _skillPointsLabel;
        private Label _titleLabel;
        private Button _closeButton;
        
        // Skill tree tabs
        private TabContainer _tabContainer;
        private VBoxContainer _offensiveTree;
        private VBoxContainer _defensiveTree;
        private VBoxContainer _magicTree;
        private VBoxContainer _utilityTree;
        
        private Player _player;
        private bool _isVisible = false; 
        
        // Animation
        private Tween _uiTween;
        private Color _lastSkillPointsColor;
        
        public override void _Ready()
        {
            SetupUI();
            Hide();
            _lastSkillPointsColor = new Color(1, 0.8f, 0); // Gold color
        }
        
        private void SetupUI()
        {
            // Main panel
            _mainPanel = new Control();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            _mainPanel.Modulate = new Color(1, 1, 1, 0); // Start invisible for fade-in
            AddChild(_mainPanel);
            
            var panel = new PanelContainer();
            panel.SetAnchorsPreset(Control.LayoutPreset.Center);
            panel.CustomMinimumSize = new Vector2(800, 600);
            _mainPanel.AddChild(panel);
            
            var margin = new MarginContainer();
            margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            margin.AddThemeConstantOverride("margin_left", 20);
            margin.AddThemeConstantOverride("margin_right", 20);
            margin.AddThemeConstantOverride("margin_top", 20);
            margin.AddThemeConstantOverride("margin_bottom", 20);
            panel.AddChild(margin);
            
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            margin.AddChild(vbox);
            
            // Title and close button
            var header = new HBoxContainer();
            vbox.AddChild(header);
            
            _titleLabel = new Label();
            _titleLabel.Text = "技能树";
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            header.AddChild(_titleLabel);
            
            header.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            _skillPointsLabel = new Label();
            _skillPointsLabel.Text = "技能点: 0";
            _skillPointsLabel.AddThemeFontSizeOverride("font_size", 20);
            _skillPointsLabel.Modulate = new Color(1, 0.8f, 0); // Gold color
            header.AddChild(_skillPointsLabel);
            
            _closeButton = new Button();
            _closeButton.Text = "X";
            _closeButton.TooltipText = "关闭 (K)";
            _closeButton.Pressed += () => ToggleVisibility();
            header.AddChild(_closeButton);
            
            // Tab container for skill trees
            _tabContainer = new TabContainer();
            _tabContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vbox.AddChild(_tabContainer);
            
            // Create skill tree panels
            _offensiveTree = CreateSkillTreePanel("攻击系", SkillTreeType.Offensive);
            _defensiveTree = CreateSkillTreePanel("防御系", SkillTreeType.Defensive);
            _magicTree = CreateSkillTreePanel("魔法系", SkillTreeType.Magic);
            _utilityTree = CreateSkillTreePanel("辅助系", SkillTreeType.Utility);
            
            _tabContainer.AddChild(_offensiveTree);
            _tabContainer.AddChild(_defensiveTree);
            _tabContainer.AddChild(_magicTree);
            _tabContainer.AddChild(_utilityTree);
            
            _tabContainer.SetTabTitle(0, "攻击系");
            _tabContainer.SetTabTitle(1, "防御系");
            _tabContainer.SetTabTitle(2, "魔法系");
            _tabContainer.SetTabTitle(3, "辅助系");
            
            // Instructions
            var instructions = new Label();
            instructions.Text = "点击技能学习 • 点击已学习技能升级 • 橙色=可学习 • 灰色=未解锁 • 金色=已学习";
            instructions.AddThemeFontSizeOverride("font_size", 14);
            instructions.Modulate = new Color(0.7f, 0.7f, 0.7f);
            vbox.AddChild(instructions);
        }
        
        private VBoxContainer CreateSkillTreePanel(string treeName, SkillTreeType treeType)
        {
            var scroll = new ScrollContainer();
            scroll.Name = treeName;
            
            var vbox = new VBoxContainer();
            vbox.CustomMinimumSize = new Vector2(700, 450);
            scroll.AddChild(vbox);
            
            RefreshSkillTree(vbox, treeType);
            
            return vbox;
        }
        
        public void RefreshSkillTree(VBoxContainer container, SkillTreeType treeType)
        {
            // Clear existing
            foreach (Node child in container.GetChildren())
            {
                child.QueueFree();
            }
            
            if (_player == null) return;
            
            // Get available skills in this tree
            var skills = SkillDatabase.Instance.GetAvailableSkillsInTree(
                treeType, 
                _player.Level, 
                new HashSet<int>(_player.LearnedSkillIds)
            );
            
            // Group by level required
            var grouped = new Dictionary<int, List<Skill>>();
            foreach (var skill in skills)
            {
                if (!grouped.ContainsKey(skill.LevelRequired))
                    grouped[skill.LevelRequired] = new List<Skill>();
                grouped[skill.LevelRequired].Add(skill);
            }
            
            // Display by level
            foreach (var level in grouped.Keys)
            {
                var levelLabel = new Label();
                levelLabel.Text = $"等级 {level} 技能:";
                levelLabel.AddThemeFontSizeOverride("font_size", 16);
                container.AddChild(levelLabel);
                
                var hbox = new HBoxContainer();
                hbox.Alignment = BoxContainer.AlignmentMode.Center;
                container.AddChild(hbox);
                
                foreach (var skill in grouped[level])
                {
                    var btn = CreateSkillButton(skill);
                    hbox.AddChild(btn);
                }
                
                container.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 10) });
            }
        }
        
        private Button CreateSkillButton(Skill skill)
        {
            var btn = new Button();
            btn.CustomMinimumSize = new Vector2(150, 60);
            
            bool isLearned = _player.LearnedSkillIds.Contains(skill.Id);
            int currentLevel = _player.SkillLevels.GetValueOrDefault(skill.Id, 0);
            
            // Build text
            string text = $"{skill.Name}\n";
            text += isLearned ? $"[Lv.{currentLevel}]" : $"需求: Lv.{skill.LevelRequired}";
            
            if (skill.IsPassive && isLearned)
            {
                text += $"\n被动: +{skill.PassiveAttackBonus + skill.PassiveDefenseBonus + skill.PassiveHealthBonus + skill.PassiveManaBonus + skill.PassiveCritBonus}%";
            }
            
            btn.Text = text;
            btn.TooltipText = GetSkillTooltip(skill);
            
            // Color coding
            if (isLearned)
            {
                btn.Modulate = new Color(1, 0.84f, 0); // Gold for learned
            }
            else if (SkillDatabase.Instance.CanLearnSkill(skill, _player.Level, _player.SkillPoints, 
                new HashSet<int>(_player.LearnedSkillIds)))
            {
                btn.Modulate = new Color(1, 0.6f, 0.3f); // Orange for available
            }
            else
            {
                btn.Modulate = new Color(0.5f, 0.5f, 0.5f); // Gray for locked
            }
            
            // Click handler
            btn.Pressed += () => OnSkillButtonPressed(skill);
            
            // Hover animation - scale up slightly
            btn.MouseEntered += () => OnButtonHoverEnter(btn);
            btn.MouseExited += () => OnButtonHoverExit(btn);
            
            return btn;
        }
        
        private void OnButtonHoverEnter(Button btn)
        {
            if (_uiTween != null && _uiTween.IsValid())
                _uiTween.Kill();
            
            _uiTween = CreateTween();
            _uiTween.SetParallel(true);
            _uiTween.TweenProperty(btn, "scale", new Vector2(1.05f, 1.05f), 0.15f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEasing(Tween.EasingFunction.EaseOut);
        }
        
        private void OnButtonHoverExit(Button btn)
        {
            if (_uiTween != null && _uiTween.IsValid())
                _uiTween.Kill();
            
            _uiTween = CreateTween();
            _uiTween.SetParallel(true);
            _uiTween.TweenProperty(btn, "scale", Vector2.One, 0.15f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEasing(Tween.EasingFunction.EaseOut);
        }
        
        private string GetSkillTooltip(Skill skill)
        {
            string tip = $"{skill.Name}\n";
            tip += $"{skill.Description}\n";
            tip += $"类型: {skill.Type}\n";
            
            if (skill.IsPassive)
            {
                if (skill.PassiveAttackBonus > 0) tip += $"攻击 +{skill.PassiveAttackBonus}/级\n";
                if (skill.PassiveDefenseBonus > 0) tip += $"防御 +{skill.PassiveDefenseBonus}/级\n";
                if (skill.PassiveHealthBonus > 0) tip += $"生命 +{skill.PassiveHealthBonus}/级\n";
                if (skill.PassiveManaBonus > 0) tip += $"法力 +{skill.PassiveManaBonus}/级\n";
                if (skill.PassiveCritBonus > 0) tip += $"暴击 +{skill.PassiveCritBonus * 100}%/级\n";
                tip += $"最大等级: {skill.MaxLevel}";
            }
            else
            {
                if (skill.Damage > 0) tip += $"伤害: {skill.Damage}\n";
                if (skill.ManaCost > 0) tip += $"法力消耗: {skill.ManaCost}\n";
                if (skill.Cooldown > 0) tip += $"冷却: {skill.Cooldown}秒\n";
                tip += $"等级要求: {skill.LevelRequired}";
            }
            
            return tip;
        }
        
        private void OnSkillButtonPressed(Skill skill)
        {
            if (_player == null) return;
            
            bool isLearned = _player.LearnedSkillIds.Contains(skill.Id);
            
            if (isLearned)
            {
                // Try to upgrade
                if (_player.UpgradeSkill(skill))
                {
                    GD.Print($"升级技能: {skill.Name}");
                    AnimateSkillUpgrade();
                    RefreshAllTrees();
                }
                else
                {
                    GD.Print("无法升级技能 (可能已达到最大等级或技能点不足)");
                }
            }
            else
            {
                // Try to learn
                if (_player.LearnSkill(skill))
                {
                    GD.Print($"学习技能: {skill.Name}");
                    AnimateSkillLearn();
                    RefreshAllTrees();
                }
                else
                {
                    GD.Print("无法学习技能 (等级不足或前置技能未学习)");
                }
            }
        }
        
        private void AnimateSkillLearn()
        {
            // Flash animation on skill points label
            if (_uiTween != null && _uiTween.IsValid())
                _uiTween.Kill();
            
            _uiTween = CreateTween();
            _uiTween.SetParallel(false);
            
            // Pulse the skill points label
            _uiTween.TweenProperty(_skillPointsLabel, "scale", new Vector2(1.3f, 1.3f), 0.1f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEasing(Tween.EasingFunction.EaseOut);
            _uiTween.TweenProperty(_skillPointsLabel, "scale", Vector2.One, 0.2f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEasing(Tween.EasingFunction.EaseInOut);
                
            // Color flash to green then back to gold
            _uiTween.TweenProperty(_skillPointsLabel, "modulate", new Color(0.2f, 1f, 0.2f), 0.1f);
            _uiTween.TweenProperty(_skillPointsLabel, "modulate", _lastSkillPointsColor, 0.3f);
        }
        
        private void AnimateSkillUpgrade()
        {
            // Similar to learn but different color
            if (_uiTween != null && _uiTween.IsValid())
                _uiTween.Kill();
            
            _uiTween = CreateTween();
            _uiTween.SetParallel(false);
            
            // Pulse the skill points label
            _uiTween.TweenProperty(_skillPointsLabel, "scale", new Vector2(1.2f, 1.2f), 0.1f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEasing(Tween.EasingFunction.EaseOut);
            _uiTween.TweenProperty(_skillPointsLabel, "scale", Vector2.One, 0.2f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEasing(Tween.EasingFunction.EaseInOut);
                
            // Color flash to cyan then back to gold
            _uiTween.TweenProperty(_skillPointsLabel, "modulate", new Color(0.2f, 0.8f, 1f), 0.1f);
            _uiTween.TweenProperty(_skillPointsLabel, "modulate", _lastSkillPointsColor, 0.3f);
        }
        
        private void RefreshAllTrees()
        {
            _skillPointsLabel.Text = $"技能点: {_player.SkillPoints}";
            RefreshSkillTree(_offensiveTree, SkillTreeType.Offensive);
            RefreshSkillTree(_defensiveTree, SkillTreeType.Defensive);
            RefreshSkillTree(_magicTree, SkillTreeType.Magic);
            RefreshSkillTree(_utilityTree, SkillTreeType.Utility);
        }
        
        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey key && key.Pressed && key.Keycode == keyToggle)
            {
                ToggleVisibility();
            }
        }
        
        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            
            if (_isVisible)
            {
                // Find player
                _player = GetTree().GetFirstNodeInGroup("player") as Player;
                if (_player != null)
                {
                    _skillPointsLabel.Text = $"技能点: {_player.SkillPoints}";
                    RefreshAllTrees();
                }
                
                // Fade in animation
                Show();
                AnimatePanelIn();
            }
            else
            {
                // Fade out animation
                AnimatePanelOut();
            }
        }
        
        private void AnimatePanelIn()
        {
            if (_uiTween != null && _uiTween.IsValid())
                _uiTween.Kill();
            
            _uiTween = CreateTween();
            _uiTween.SetParallel(true);
            
            // Fade in
            _uiTween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.25f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEasing(Tween.EasingFunction.EaseOut);
                
            // Scale from 0.9 to 1.0
            _uiTween.TweenProperty(_mainPanel, "scale", Vector2.One, 0.25f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEasing(Tween.EasingFunction.EaseOut);
                
            // Initial state
            _mainPanel.Modulate = new Color(1, 1, 1, 0);
            _mainPanel.Scale = new Vector2(0.9f, 0.9f);
        }
        
        private void AnimatePanelOut()
        {
            if (_uiTween != null && _uiTween.IsValid())
                _uiTween.Kill();
            
            _uiTween = CreateTween();
            _uiTween.SetParallel(true);
            
            // Fade out
            _uiTween.TweenProperty(_mainPanel, "modulate:a", 0f, 0.2f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEasing(Tween.EasingFunction.EaseIn);
                
            // Scale down slightly
            _uiTween.TweenProperty(_mainPanel, "scale", new Vector2(0.95f, 0.95f), 0.2f)
                .SetTrans(Tween.TransitionType.Cubic)
                .SetEasing(Tween.EasingFunction.EaseIn);
                
            // Hide after animation
            _uiTween.TweenCallback(Callable.From(Hide));
        }
        
        public void Open()
        {
            if (!_isVisible)
            {
                ToggleVisibility();
            }
        }
    }
}
