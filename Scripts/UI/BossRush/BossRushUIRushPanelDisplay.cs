using Godot;
using System;

/// <summary>
/// Boss Rush UI - Rush Panel Display Component
/// Handles the display elements (status, stage, streak, health, rewards)
/// </summary>
namespace ClawRPG.Scripts.UI.BossRush
{
    public partial class BossRushUIRushPanelDisplay : Control
    {
        private BossRushSystem _bossRushSystem;
        
        // Display elements
        private Label _statusLabel;
        private Label _stageLabel;
        private Label _streakLabel;
        private Label _healthLabel;
        private ProgressBar _healthBar;
        private VBoxContainer _rewardsContainer;
        
        public BossRushUIRushPanelDisplay()
        {
        }
        
        public void Initialize(BossRushSystem system)
        {
            _bossRushSystem = system;
        }
        
        public void CreateElements(Control parent)
        {
            // Status label
            _statusLabel = new Label
            {
                Text = "Not Started",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _statusLabel.AddThemeFontSizeOverride("font_size", 24);
            _statusLabel.SetAnchor(AnchorsPreset.TopWide);
            _statusLabel.Position = new Vector2(0, 20);
            parent.AddChild(_statusLabel);
            
            // Stage info
            _stageLabel = new Label
            {
                Text = "Stage: 1"
            };
            _stageLabel.AddThemeFontSizeOverride("font_size", 20);
            _stageLabel.SetAnchor(AnchorsPreset.TopWide);
            _stageLabel.Position = new Vector2(0, 70);
            parent.AddChild(_stageLabel);
            
            // Streak info
            _streakLabel = new Label
            {
                Text = "Current Streak: 0 | Best Streak: 0"
            };
            _streakLabel.AddThemeFontSizeOverride("font_size", 18);
            _streakLabel.SetAnchor(AnchorsPreset.TopWide);
            _streakLabel.Position = new Vector2(0, 110);
            parent.AddChild(_streakLabel);
            
            // Health bar
            CreateHealthBar(parent);
            
            // Rewards preview
            CreateRewardsSection(parent);
        }
        
        private void CreateHealthBar(Control parent)
        {
            var healthTitle = new Label
            {
                Text = "Health:"
            };
            healthTitle.SetAnchor(AnchorsPreset.TopWide);
            healthTitle.Position = new Vector2(200, 160);
            parent.AddChild(healthTitle);
            
            _healthBar = new ProgressBar
            {
                Position = new Vector2(200, 190),
                Size = new Vector2(400, 30),
                MaxValue = 100,
                Value = 100,
                ShowPercentage = false
            };
            _healthBar.SetAnchor(AnchorsPreset.TopWide);
            parent.AddChild(_healthBar);
            
            _healthLabel = new Label
            {
                Text = "100%"
            };
            _healthLabel.SetAnchor(AnchorsPreset.TopWide);
            _healthLabel.Position = new Vector2(350, 165);
            parent.AddChild(_healthLabel);
        }
        
        private void CreateRewardsSection(Control parent)
        {
            var rewardsTitle = new Label
            {
                Text = "Current Rewards:"
            };
            rewardsTitle.Position = new Vector2(600, 160);
            parent.AddChild(rewardsTitle);
            
            _rewardsContainer = new VBoxContainer
            {
                Position = new Vector2(600, 200)
            };
            parent.AddChild(_rewardsContainer);
            
            UpdateRewardsDisplay();
        }
        
        public void UpdateUI()
        {
            if (_bossRushSystem == null) return;
            
            var data = _bossRushSystem.GetData();
            var state = _bossRushSystem.GetState();
            
            // Update status
            _statusLabel.Text = state.ToString();
            
            // Update stage info
            _stageLabel.Text = $"Stage: {data.CurrentStage}";
            
            // Update streak
            _streakLabel.Text = $"Current Streak: {data.CurrentStreak} | Best Streak: {data.BestStreak}";
            
            // Update health
            float healthPercent = _bossRushSystem.GetCurrentHealthPercent() * 100;
            _healthBar.Value = healthPercent;
            _healthLabel.Text = $"{healthPercent:F0}%";
            
            UpdateRewardsDisplay();
        }
        
        private void UpdateRewardsDisplay()
        {
            foreach (var child in _rewardsContainer.GetChildren())
                child.QueueFree();
            
            if (_bossRushSystem == null) return;
            
            var data = _bossRushSystem.GetData();
            
            var goldLabel = new Label
            {
                Text = $"💰 Gold: {data.GoldEarned}"
            };
            goldLabel.AddThemeFontSizeOverride("font_size", 18);
            _rewardsContainer.AddChild(goldLabel);
            
            var expLabel = new Label
            {
                Text = $"✨ Exp: {data.ExpEarned}"
            };
            expLabel.AddThemeFontSizeOverride("font_size", 18);
            _rewardsContainer.AddChild(expLabel);
            
            var bossesLabel = new Label
            {
                Text = $"👹 Bosses: {data.BossesDefeated}"
            };
            bossesLabel.AddThemeFontSizeOverride("font_size", 18);
            _rewardsContainer.AddChild(bossesLabel);
        }
    }
}
