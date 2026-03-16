using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boss Rush UI - Statistics Panel Component
/// Displays player statistics and achievements
/// </summary>
namespace ClawRPG.Scripts.UI.BossRush
{
    public class BossRushUIStatsPanel : Control
    {
        private BossRushSystem _bossRushSystem;
        private VBoxContainer _statsContainer;
        
        public BossRushUIStatsPanel()
        {
        }
        
        public void Initialize(BossRushSystem system)
        {
            _bossRushSystem = system;
        }
        
        public void Setup(Control parent, Vector2 position, Vector2 size)
        {
            SetAnchor(AnchorPreset.FullRect);
            Position = position;
            Size = size;
            Visible = false;
            parent.AddChild(this);
            
            CreateElements();
        }
        
        private void CreateElements()
        {
            var title = new Label
            {
                Text = "Statistics"
            };
            title.AddThemeFontSizeOverride("font_size", 24);
            title.Position = new Vector2(20, 20);
            AddChild(title);
            
            _statsContainer = new VBoxContainer
            {
                Position = new Vector2(20, 70),
                Size = new Vector2(760, 390)
            };
            AddChild(_statsContainer);
            
            UpdateDisplay();
        }
        
        public void UpdateDisplay()
        {
            foreach (var child in _statsContainer.GetChildren())
                child.QueueFree();
            
            if (_bossRushSystem == null) return;
            
            var stats = _bossRushSystem.GetStatistics();
            
            AddStatRow("Total Attempts:", stats["total_attempts"].ToString());
            AddStatRow("Victories:", stats["total_victories"].ToString());
            AddStatRow("Win Rate:", $"{stats["win_rate"]:P1}");
            AddStatRow("Total Bosses Defeated:", stats["total_bosses"].ToString());
            AddStatRow("Highest Stage:", stats["highest_stage"].ToString());
            AddStatRow("Best Streak:", stats["best_streak"].ToString());
            AddStatRow("Total Gold Earned:", stats["total_gold"].ToString());
            AddStatRow("Total Exp Earned:", stats["total_exp"].ToString());
        }
        
        private void AddStatRow(string label, string value)
        {
            var row = new HBoxContainer();
            
            var labelNode = new Label
            {
                Text = label,
                CustomMinimumSize = new Vector2(200, 0)
            };
            row.AddChild(labelNode);
            
            var valueNode = new Label
            {
                Text = value
            };
            valueNode.AddThemeFontSizeOverride("font_size", 20);
            row.AddChild(valueNode);
            
            _statsContainer.AddChild(row);
        }
    }
}
