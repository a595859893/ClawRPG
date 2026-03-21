using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 声望界面 - 显示玩家与各阵营的声望关系
    /// </summary>
    public class ReputationUI : Control {
        private VBoxContainer _mainContainer;
        private VBoxContainer _factionList;
        private Label _titleLabel;
        private Button _closeButton;
        
        // 阵营信息面板
        private Panel _factionInfoPanel;
        private Label _factionNameLabel;
        private Label _factionDescLabel;
        private Label _reputationValueLabel;
        private Label _tierNameLabel;
        private ProgressBar _reputationProgress;
        private Label _progressLabel;
        private Button _claimRewardButton;
        
        private string _selectedFactionId;
        
        // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
        public event Action<string> OnReputationChangedUI;
        public event Action<string, ReputationTier> OnTierChangedUI;
        
        public override void _Ready() {
            SetupUI();
            Visible = false; 
        }
        
        private void SetupUI() {
            // 背景遮罩
            var bg = new ColorRect {
                Color = new Color(0, 0, 0, 0.5),
                AnchorsPreset = Control.LayoutPreset.FullRect
            };
            AddChild(bg);
            
            // 主容器
            _mainContainer = new VBoxContainer {
                AnchorsPreset = Control.LayoutPreset.Center,
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -400,
                OffsetTop = -300,
                OffsetRight = 400,
                OffsetBottom = 300,
                GrowHorizontal = Control.GrowDirection.Center,
                GrowVertical = Control.GrowDirection.Center
            };
            AddChild(_mainContainer);
            
            // 标题栏
            var header = new HBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _mainContainer.AddChild(header);
            
            _titleLabel = new Label {
                Text = "声望系统",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            header.AddChild(_titleLabel);
            
            _closeButton = new Button {
                Text = "✕",
                CustomMinimumSize = new Vector2(30, 30)
            };
            _closeButton.Pressed += () => Hide();
            header.AddChild(_closeButton);
            
            // 内容区域
            var content = new HBoxContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _mainContainer.AddChild(content);
            
            // 左侧：阵营列表
            var listScroll = new ScrollContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(200, 0)
            };
            content.AddChild(listScroll);
            
            _factionList = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            listScroll.AddChild(_factionList);
            
            // 右侧：阵营详细信息
            _factionInfoPanel = new Panel {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(300, 0)
            };
            content.AddChild(_factionInfoPanel);
            
            SetupFactionInfoPanel();
            
            // 刷新列表
            RefreshFactionList();
            
            // 连接信号 (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
            if (ReputationSystem.Instance.ReputationChanged != null) {
                ReputationSystem.Instance.ReputationChanged += OnReputationChanged;
            }
            if (ReputationSystem.Instance.TierChanged != null) {
                ReputationSystem.Instance.TierChanged += OnTierChanged;
            }
        }
        
        private void SetupFactionInfoPanel() {
            var infoContainer = new VBoxContainer {
                AnchorsPreset = Control.LayoutPreset.FullRect,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            _factionInfoPanel.AddChild(infoContainer);
            
            _factionNameLabel = new Label {
                Text = "选择阵营",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 30)
            };
            infoContainer.AddChild(_factionNameLabel);
            
            _factionDescLabel = new Label {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                Autowrap = true
            };
            infoContainer.AddChild(_factionDescLabel);
            
            infoContainer.AddChild(new Control { SizeFlagsVertical = Control.SizeFlags.ExpandFill });
            
            // 声望值
            var repLabel = new Label {
                Text = "声望值:",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            infoContainer.AddChild(repLabel);
            
            _reputationValueLabel = new Label {
                Text = "0",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 25)
            };
            infoContainer.AddChild(_reputationValueLabel);
            
            // 等级名称
            _tierNameLabel = new Label {
                Text = "中立",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 30)
            };
            infoContainer.AddChild(_tierNameLabel);
            
            // 进度条
            _reputationProgress = new ProgressBar {
                CustomMinimumSize = new Vector2(0, 20),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _reputationProgress.ShowPercentage = false; 
            infoContainer.AddChild(_reputationProgress);
            
            _progressLabel = new Label {
                Text = "0%",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            infoContainer.AddChild(_progressLabel);
            
            infoContainer.AddChild(new Control { SizeFlagsVertical = Control.SizeFlags.ExpandFill });
            
            // 领取奖励按钮
            _claimRewardButton = new Button {
                Text = "领取奖励",
                CustomMinimumSize = new Vector2(0, 40)
            };
            _claimRewardButton.Pressed += OnClaimRewardPressed;
            infoContainer.AddChild(_claimRewardButton);
        }
        
        private void RefreshFactionList() {
            // 清除现有项
            foreach (var child in _factionList.GetChildren()) {
                child.QueueFree();
            }
            
            var factions = ReputationSystem.Instance.GetAllFactions();
            foreach (var kvp in factions) {
                var faction = kvp.Value;
                var data = ReputationSystem.Instance.GetFactionData(kvp.Key);
                
                var button = new Button {
                    Text = $"{faction.Name} ({ReputationSystem.Instance.GetTierName(data.Tier)})",
                    CustomMinimumSize = new Vector2(0, 40),
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
                };
                button.Pressed += () => OnFactionSelected(kvp.Key);
                _factionList.AddChild(button);
            }
        }
        
        private void OnFactionSelected(string factionId) {
            _selectedFactionId = factionId;
            UpdateFactionInfo();
        }
        
        private void UpdateFactionInfo() {
            if (string.IsNullOrEmpty(_selectedFactionId)) return;
            
            var faction = ReputationSystem.Instance.GetAllFactions()[_selectedFactionId];
            var data = ReputationSystem.Instance.GetFactionData(_selectedFactionId);
            
            _factionNameLabel.Text = faction.Name;
            _factionDescLabel.Text = faction.Description;
            _reputationValueLabel.Text = data.Reputation.ToString();
            _tierNameLabel.Text = ReputationSystem.Instance.GetTierName(data.Tier);
            
            // 根据等级设置颜色
            Color tierColor;
            switch (data.Tier) {
                case ReputationTier.Hated: tierColor = Colors.DarkRed; break;
                case ReputationTier.Hostile: tierColor = Colors.Red; break;
                case ReputationTier.Unfriendly: tierColor = Colors.Orange; break;
                case ReputationTier.Neutral: tierColor = Colors.Gray; break;
                case ReputationTier.Friendly: tierColor = Colors.Green; break;
                case ReputationTier.Honored: tierColor = Colors.Cyan; break;
                case ReputationTier.Revered: tierColor = Colors.Blue; break;
                case ReputationTier.Exalted: tierColor = Colors.Gold; break;
                default: tierColor = Colors.White; break;
            }
            _tierNameLabel.Modulate = tierColor;
            
            // 更新进度条
            var progress = ReputationSystem.Instance.GetTierProgress(_selectedFactionId);
            _reputationProgress.Value = progress * 100;
            _progressLabel.Text = $"{(int)(progress * 100)}%";
            
            // 检查是否有可领取的奖励
            var hasReward = false; 
            foreach (var reward in faction.Rewards) {
                if (data.Tier >= reward.RequiredTier && !data.RewardClaimed) {
                    hasReward = true;
                    break;
                }
            }
            _claimRewardButton.Visible = hasReward;
            _claimRewardButton.Disabled = !hasReward;
        }
        
        private void OnClaimRewardPressed() {
            if (string.IsNullOrEmpty(_selectedFactionId)) return;
            
            if (ReputationSystem.Instance.ClaimReward(_selectedFactionId)) {
                UpdateFactionInfo();
            }
        }
        
        private void OnReputationChanged(string factionId) {
            // REQ-058-11: Invoke new event
            OnReputationChangedUI?.Invoke(factionId);
            if (factionId == _selectedFactionId) {
                UpdateFactionInfo();
            }
            RefreshFactionList();
        }
        
        private void OnTierChanged(string factionId, ReputationTier newTier) {
            // REQ-058-11: Invoke new event
            OnTierChangedUI?.Invoke(factionId, newTier);
            if (factionId == _selectedFactionId) {
                UpdateFactionInfo();
            }
            RefreshFactionList();
        }
        
        public void Show() {
            Visible = true;
            RefreshFactionList();
        }
        
        public void Hide() {
            Visible = false; 
        }
        
        public void Toggle() {
            if (Visible) {
                Hide();
            } else {
                Show();
            }
        }
    }
}
