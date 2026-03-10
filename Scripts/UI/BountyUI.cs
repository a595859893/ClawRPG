using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 赏金任务UI界面
    /// </summary>
    public class BountyUI : Control
    {
        private VBoxContainer _mainContainer;
        private HBoxContainer _headerContainer;
        private Label _titleLabel;
        private Button _closeButton;
        
        private HBoxContainer _filterContainer;
        private Button _btnAll;
        private Button _btnKill;
        private Button _btnCollect;
        private Button _btnBoss;
        private Button _btnSurvival;
        private Button _btnCombo;
        
        private ScrollContainer _scrollContainer;
        private VBoxContainer _bountyListContainer;
        
        private Label _goldLabel;
        private Label _timerLabel;
        
        private BountyType? _currentFilter;
        private List<Bounty> _displayedBounties = new List<Bounty>();

        public override void _Ready()
        {
            Visible = false;
            SetupUI();
            SetupSignals();
        }

        private void SetupUI()
        {
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainContainer.CustomMinimumSize = new Vector2(600, 500);
            _mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(_mainContainer);

            // 背景面板
            var bgPanel = new Panel();
            bgPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            bgPanel.Modulate = new Color(0, 0, 0, 0.8f);
            _mainContainer.AddChild(bgPanel);
            bgPanel.MoveChild(_mainContainer.GetChild(0), 0); // 移到最前面

            // 标题栏
            _headerContainer = new HBoxContainer();
            _headerContainer.AddThemeConstantOverride("separation", 10);
            _mainContainer.AddChild(_headerContainer);

            _titleLabel = new Label();
            _titleLabel.Text = "🎯 赏金任务";
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _headerContainer.AddChild(_titleLabel);

            _headerContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            // 金币显示
            _goldLabel = new Label();
            _goldLabel.Text = "💰 0";
            _goldLabel.AddThemeFontSizeOverride("font_size", 18);
            _headerContainer.AddChild(_goldLabel);

            // 计时器
            _timerLabel = new Label();
            _timerLabel.Text = "⏰ 刷新: --:--";
            _timerLabel.AddThemeFontSizeOverride("font_size", 16);
            _headerContainer.AddChild(_timerLabel);

            // 关闭按钮
            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _closeButton.Pressed += () => ToggleUI();
            _headerContainer.AddChild(_closeButton);

            // 筛选按钮
            _filterContainer = new HBoxContainer();
            _filterContainer.AddThemeConstantOverride("separation", 5);
            _mainContainer.AddChild(_filterContainer);

            _btnAll = CreateFilterButton("全部", BountyType.KillEnemy); // 使用 KillEnemy 作为 None 标记
            _btnKill = CreateFilterButton("击杀", BountyType.KillEnemy);
            _btnCollect = CreateFilterButton("收集", BountyType.CollectItem);
            _btnBoss = CreateFilterButton("Boss", BountyType.BossChallenge);
            _btnSurvival = CreateFilterButton("生存", BountyType.Survival);
            _btnCombo = CreateFilterButton("连击", BountyType.ComboChallenge);

            // 赏金列表
            _scrollContainer = new ScrollContainer();
            _scrollContainer.SetHExpand(true);
            _scrollContainer.CustomMinimumSize = new Vector2(0, 380);
            _mainContainer.AddChild(_scrollContainer);

            _bountyListContainer = new VBoxContainer();
            _bountyListContainer.AddThemeConstantOverride("separation", 10);
            _scrollContainer.AddChild(_bountyListContainer);
        }

        private Button CreateFilterButton(string text, BountyType type)
        {
            var btn = new Button();
            btn.Text = text;
            btn.CustomMinimumSize = new Vector2(80, 35);
            btn.Pressed += () => OnFilterPressed(type);
            _filterContainer.AddChild(btn);
            return btn;
        }

        private void OnFilterPressed(BountyType type)
        {
            _currentFilter = type == BountyType.KillEnemy && _currentFilter == null ? null : type;
            RefreshBountyList();
        }

        private void SetupSignals()
        {
            var manager = BountyManager.Instance;
            manager.OnBountiesRefreshed += RefreshBountyList;
            manager.OnBountyProgressUpdated += OnBountyProgressUpdated;
            manager.OnBountyCompleted += OnBountyCompleted;
            manager.OnBountyClaimed += OnBountyClaimed;
        }

        public void ToggleUI()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshBountyList();
                UpdateGoldDisplay();
            }
        }

        private void RefreshBountyList()
        {
            // 清除旧项目
            foreach (var child in _bountyListContainer.GetChildren())
            {
                child.QueueFree();
            }
            _displayedBounties.Clear();

            var bounties = BountyManager.Instance.ActiveBounties;
            
            // 应用筛选
            var filteredBounties = _currentFilter.HasValue 
                ? bounties.FindAll(b => b.Type == _currentFilter.Value)
                : bounties;

            foreach (var bounty in filteredBounties)
            {
                var bountyPanel = CreateBountyPanel(bounty);
                _bountyListContainer.AddChild(bountyPanel);
                _displayedBounties.Add(bounty);
            }

            if (filteredBounties.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "暂无赏金任务";
                emptyLabel.Align = Label.AlignEnum.Center;
                emptyLabel.AddThemeFontSizeOverride("font_size", 18);
                _bountyListContainer.AddChild(emptyLabel);
            }
        }

        private Control CreateBountyPanel(Bounty bounty)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(550, 100);
            
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 15);
            panel.AddChild(hbox);

            // 左侧：难度图标和标题
            var leftVBox = new VBoxContainer();
            leftVBox.AddThemeConstantOverride("separation", 5);
            hbox.AddChild(leftVBox);

            var difficultyColor = BountyDatabase.Instance.GetDifficultyColor(bounty.Difficulty);
            
            var titleLabel = new Label();
            titleLabel.Text = $"{GetDifficultyIcon(bounty.Difficulty)} {bounty.Title}";
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            titleLabel.Modulate = ColorFromHex(difficultyColor);
            leftVBox.AddChild(titleLabel);

            var descLabel = new Label();
            descLabel.Text = bounty.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 14);
            descLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
            leftVBox.AddChild(descLabel);

            var progressLabel = new Label();
            progressLabel.Text = $"进度: {bounty.CurrentProgress} / {bounty.TargetCount} ({bounty.ProgressPercent * 100:F0}%)";
            progressLabel.AddThemeFontSizeOverride("font_size", 12);
            progressLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            leftVBox.AddChild(progressLabel);

            // 进度条
            var progressBar = new ProgressBar();
            progressBar.CustomMinimumSize = new Vector2(200, 15);
            progressBar.Value = bounty.ProgressPercent * 100;
            progressBar.MaxValue = 100;
            progressBar.ShowPercentage = false;
            
            if (bounty.IsCompleted)
            {
                progressBar.Modulate = new Color(0, 1, 0, 0.5f);
            }
            leftVBox.AddChild(progressBar);

            // 右侧：奖励和操作
            var rightVBox = new VBoxContainer();
            rightVBox.AddThemeConstantOverride("separation", 10);
            hbox.AddChild(rightVBox);

            // 奖励显示
            var rewardLabel = new Label();
            rewardLabel.Text = $"💰 {bounty.GoldReward}  ✨ {bounty.XPReward}";
            rewardLabel.AddThemeFontSizeOverride("font_size", 14);
            rightVBox.AddChild(rewardLabel);

            // 到期时间
            var timeLeft = (bounty.ExpiresAt - DateTime.Now).TotalMinutes;
            var timeLabel = new Label();
            if (timeLeft > 0)
            {
                timeLabel.Text = $"⏰ {(int)timeLeft}分钟";
            }
            else
            {
                timeLabel.Text = "⏰ 已过期";
                timeLabel.Modulate = new Color(1, 0, 0);
            }
            timeLabel.AddThemeFontSizeOverride("font_size", 12);
            rightVBox.AddChild(timeLabel);

            // 操作按钮
            if (bounty.IsCompleted && !bounty.IsClaimed)
            {
                var claimBtn = new Button();
                claimBtn.Text = "领取奖励";
                claimBtn.CustomMinimumSize = new Vector2(100, 30);
                claimBtn.Pressed += () => ClaimReward(bounty);
                rightVBox.AddChild(claimBtn);
            }
            else if (bounty.IsClaimed)
            {
                var claimedLabel = new Label();
                claimedLabel.Text = "✅ 已完成";
                claimedLabel.Modulate = new Color(0, 1, 0);
                rightVBox.AddChild(claimedLabel);
            }
            else
            {
                var statusLabel = new Label();
                statusLabel.Text = bounty.IsCompleted ? "✅ 完成" : "🔄 进行中";
                statusLabel.AddThemeFontSizeOverride("font_size", 12);
                rightVBox.AddChild(statusLabel);
            }

            return panel;
        }

        private void ClaimReward(Bounty bounty)
        {
            if (BountyManager.Instance.ClaimBountyReward(bounty))
            {
                UpdateGoldDisplay();
                RefreshBountyList();
                
                // 显示消息
                var msgSys = GetNodeOrNull<Control>("/Main/GameMessageSystem");
                if (msgSys != null)
                {
                    // 通知奖励已领取
                }
            }
        }

        private void OnBountyProgressUpdated(Bounty bounty)
        {
            if (Visible)
            {
                RefreshBountyList();
            }
        }

        private void OnBountyCompleted(Bounty bounty)
        {
            if (Visible)
            {
                RefreshBountyList();
            }
        }

        private void OnBountyClaimed(Bounty bounty)
        {
            RefreshBountyList();
        }

        private void UpdateGoldDisplay()
        {
            var player = GetNodeOrNull<Player>("/Main/Player");
            if (player != null)
            {
                _goldLabel.Text = $"💰 {player.Gold}";
            }
        }

        private string GetDifficultyIcon(BountyDifficulty difficulty)
        {
            return difficulty switch
            {
                BountyDifficulty.Easy => "⭐",
                BountyDifficulty.Normal => "⭐⭐",
                BountyDifficulty.Hard => "⭐⭐⭐",
                BountyDifficulty.Elite => "⭐⭐⭐⭐",
                BountyDifficulty.Legendary => "⭐⭐⭐⭐⭐",
                _ => "⭐"
            };
        }

        private Color ColorFromHex(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            
            var r = Convert.ToByte(hex.Substring(0, 2), 16) / 255f;
            var g = Convert.ToByte(hex.Substring(2, 2), 16) / 255f;
            var b = Convert.ToByte(hex.Substring(4, 2), 16) / 255f;
            
            return new Color(r, g, b);
        }

        public override void _Process(double delta)
        {
            if (Visible && _timerLabel != null)
            {
                // 更新刷新时间
                var manager = BountyManager.Instance;
                if (manager.ActiveBounties.Count > 0)
                {
                    var nextRefresh = manager.ActiveBounties[0].ExpiresAt;
                    var timeLeft = (nextRefresh - DateTime.Now).TotalMinutes;
                    if (timeLeft > 0)
                    {
                        _timerLabel.Text = $"⏰ 刷新: {(int)timeLeft}分钟";
                    }
                    else
                    {
                        _timerLabel.Text = "⏰ 即将刷新";
                    }
                }
            }
        }
    }
}
