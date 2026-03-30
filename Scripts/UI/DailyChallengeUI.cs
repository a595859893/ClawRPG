using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Daily Challenge UI - displays and manages daily challenges
    /// </summary>
    public class DailyChallengeUI : Control {
        [Export] private NodePath challengeListPath;
        [Export] private NodePath progressBarPath;
        [Export] private NodePath completedCountLabelPath;
        [Export] private NodePath timerLabelPath;
        [Export] private NodePath closeButtonPath;
        
        private VBoxContainer _challengeList;
        private ProgressBar _progressBar;
        private Label _completedCountLabel;
        private Label _timerLabel;
        private Button _closeButton;
        
        private bool _isVisible = false;
        
        // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
        public event Action OnCloseButtonPressedEvent; 
        
        public override void _Ready() {
            // Get nodes
            _challengeList = GetNode<VBoxContainer>(challengeListPath);
            _progressBar = GetNode<ProgressBar>(progressBarPath);
            _completedCountLabel = GetNode<Label>(completedCountLabelPath);
            _timerLabel = GetNode<Label>(timerLabelPath);
            _closeButton = GetNode<Button>(closeButtonPath);
            
            // Connect signals (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
            if (_closeButton != null) {
                _closeButton.Pressed += OnCloseButtonPressed;
            }
            
            // Initial state
            Visible = false; 
        }
        
        public override void _Process(double delta) {
            if (!_isVisible) return;
            
            UpdateUI();
            UpdateTimer();
        }
        
        private void UpdateUI() {
            var challenges = DailyChallengeManager.Instance.GetDailyChallenges();
            
            // Clear existing
            if (_challengeList != null) {
                foreach (var child in _challengeList.GetChildren()) {
                    child.QueueFree();
                }
                
                // Add challenge items
                foreach (var challenge in challenges) {
                    var item = CreateChallengeItem(challenge);
                    _challengeList.AddChild(item);
                }
            }
            
            // Update progress bar
            if (_progressBar != null) {
                _progressBar.Value = DailyChallengeManager.Instance.GetOverallProgress() * 100;
            }
            
            // Update completed count
            if (_completedCountLabel != null) {
                int completed = DailyChallengeManager.Instance.GetCompletedCount();
                int total = challenges.Count;
                _completedCountLabel.Text = $"已完成: {completed}/{total}";
            }
        }
        
        private Control CreateChallengeItem(DailyChallenge challenge) {
            var container = new VBoxContainer();
            container.SetMeta("challenge", challenge);
            
            // Challenge name and status
            var nameLabel = new Label();
            string statusEmoji = challenge.IsCompleted ? "✅" : "⬜";
            string difficultyColor = GetDifficultyColor(challenge.Difficulty);
            nameLabel.Text = $"{statusEmoji} {challenge.Name} [{difficultyColor}]";
            
            // Description
            var descLabel = new Label();
            descLabel.Text = challenge.Description;
            descLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            
            // Progress bar
            var progress = new ProgressBar();
            progress.MinValue = 0;
            progress.MaxValue = 100;
            progress.Value = challenge.GetProgressPercentage() * 100;
            progress.CustomMinimumSize = new Vector2(200, 20);
            
            // Progress text
            var progressText = new Label();
            string progressStr = challenge.IsCompleted ? "已完成!" : $"{challenge.CurrentProgress}/{challenge.TargetCount}";
            progressText.Text = progressStr;
            progressText.Align = Label.AlignEnum.Center;
            
            // Rewards
            var rewardLabel = new Label();
            string rewards = $"奖励: {challenge.GoldReward}金币 {challenge.ExpReward}经验";
            if (challenge.ItemRewardIds.Count > 0) {
                rewards += " 物品x" + challenge.ItemRewardIds.Count;
            }
            rewardLabel.Text = rewards;
            rewardLabel.AddColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            
            // Add to container
            container.AddChild(nameLabel);
            container.AddChild(descLabel);
            container.AddChild(progress);
            container.AddChild(progressText);
            container.AddChild(rewardLabel);
            
            // Style based on completion
            if (challenge.IsCompleted) {
                nameLabel.AddColorOverride("font_color", new Color(0.5f, 1f, 0.5f));
            }
            
            return container;
        }
        
        private string GetDifficultyColor(ChallengeDifficulty difficulty) {
            return difficulty switch {
                ChallengeDifficulty.Easy => "简单",
                ChallengeDifficulty.Normal => "普通",
                ChallengeDifficulty.Hard => "困难",
                ChallengeDifficulty.Elite => "精英",
                _ => "未知"
            };
        }
        
        private void UpdateTimer() {
            if (_timerLabel == null) return;
            
            var now = DateTime.Now;
            var midnight = DateTime.Today.AddDays(1);
            var remaining = midnight - now;
            
            _timerLabel.Text = $"重置时间: {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
        
        public void Toggle() {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible) {
                UpdateUI();
            }
        }
        
        private void OnCloseButtonPressed() {
            // REQ-058-11: Invoke new event
            OnCloseButtonPressedEvent?.Invoke();
            Toggle();
        }
        
        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                // Toggle with J key
                if (keyEvent.Scancode == (int)KeyList.J) {
                    Toggle();
                }
            }
        }
    }
}
