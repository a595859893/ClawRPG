using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boss Rush UI - History Panel Component
/// Displays the player's rush history records
/// </summary>
namespace ClawRPG.Scripts.UI.BossRush
{
    public partial class BossRushUIHistoryPanel : Control
    {
        private BossRushSystem _bossRushSystem;
        private VBoxContainer _historyContainer;
        
        public BossRushUIHistoryPanel()
        {
        }
        
        public void Initialize(BossRushSystem system)
        {
            _bossRushSystem = system;
        }
        
        public void Setup(Control parent, Vector2 position, Vector2 size)
        {
            SetAnchor(AnchorsPreset.FullRect);
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
                Text = "Rush History"
            };
            title.AddThemeFontSizeOverride("font_size", 24);
            title.Position = new Vector2(20, 20);
            AddChild(title);
            
            _historyContainer = new VBoxContainer
            {
                Position = new Vector2(20, 70),
                Size = new Vector2(760, 390)
            };
            AddChild(_historyContainer);
            
            UpdateDisplay();
        }
        
        public void UpdateDisplay()
        {
            foreach (var child in _historyContainer.GetChildren())
                child.QueueFree();
            
            if (_bossRushSystem == null) return;
            
            var history = _bossRushSystem.GetHistory(10);
            
            if (history.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "No rush history yet. Start your first boss rush!"
                };
                _historyContainer.AddChild(emptyLabel);
                return;
            }
            
            foreach (var record in history)
            {
                var recordPanel = new HBoxContainer();
                
                var resultLabel = new Label
                {
                    Text = record.Victory ? "✅ Victory" : "❌ Defeat",
                    CustomMinimumSize = new Vector2(100, 0)
                };
                recordPanel.AddChild(resultLabel);
                
                var stageLabel = new Label
                {
                    Text = $"Stage {record.Stage}",
                    CustomMinimumSize = new Vector2(80, 0)
                };
                recordPanel.AddChild(stageLabel);
                
                var bossesLabel = new Label
                {
                    Text = $"{record.BossesDefeated} bosses",
                    CustomMinimumSize = new Vector2(100, 0)
                };
                recordPanel.AddChild(bossesLabel);
                
                var rewardsLabel = new Label
                {
                    Text = $"{record.GoldEarned}g / {record.ExpEarned}exp"
                };
                recordPanel.AddChild(rewardsLabel);
                
                _historyContainer.AddChild(recordPanel);
            }
        }
    }
}
