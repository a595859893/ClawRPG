using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Scripts.Fishing;

namespace ClawRPG.Scripts.Fishing
{
    /// <summary>
    /// 钓鱼系统UI界面
    /// </summary>
    public partial class FishingUI : Control
    {
        // UI组件
        private PanelContainer _mainPanel;
        private TabContainer _tabContainer;
        
        // 状态标签
        private Label _statusLabel;
        private Label _levelLabel;
        private Label _xpLabel;
        private ProgressBar _xpProgressBar;
        
        // 当前钓鱼状态
        private Label _currentStateLabel;
        private ProgressBar _waitingProgressBar;
        private ProgressBar _reelingProgressBar;
        private Label _currentFishLabel;
        
        // 统计数据
        private Label _totalCatchesLabel;
        private Label _totalValueLabel;
        private Label _successRateLabel;
        private Label _perfectCatchesLabel;
        
        // 鱼类收藏
        private GridContainer _fishGrid;
        private ScrollContainer _fishScroll;
        
        // 位置选择
        private OptionButton _locationOption;
        
        // 鱼竿选择
        private OptionButton _rodOption;
        
        // 鱼饵选择
        private OptionButton _baitOption;
        
        // 按钮
        private Button _startButton;
        private Button _reelButton;
        private Button _cancelButton;
        
        // 统计面板
        private Label _biggestCatchLabel;
        private Label _uniqueSpeciesLabel;
        private GridContainer _rarityStatsGrid;
        
        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
        }
        
        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);
            
            // 主容器
            var mainVBox = new VBoxContainer();
            mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainVBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(mainVBox);
            
            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "🎣 钓鱼系统";
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainVBox.AddChild(titleLabel);
            
            // 等级和经验
            var levelHBox = new HBoxContainer();
            mainVBox.AddChild(levelHBox);
            
            _levelLabel = new Label();
            _levelLabel.Text = "等级: 1";
            _levelLabel.AddThemeFontSizeOverride("font_size", 18);
            levelHBox.AddChild(_levelLabel);
            
            _xpProgressBar = new ProgressBar();
            _xpProgressBar.CustomMinimumSize = new Vector2(200, 20);
            _xpProgressBar.ShowPercentage = false;
            levelHBox.AddChild(_xpProgressBar);
            
            _xpLabel = new Label();
            _xpLabel.Text = "0 / 100 XP";
            levelHBox.AddChild(_xpLabel);
            
            // 标签页容器
            _tabContainer = new TabContainer();
            _tabContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Fill);
            mainVBox.AddChild(_tabContainer);
            
            // 钓鱼页
            CreateFishingTab();
            
            // 收藏页
            CreateCollectionTab();
            
            // 统计页
            CreateStatisticsTab();
            
            // 位置选择行
            var locationHBox = new HBoxContainer();
            mainVBox.AddChild(locationHBox);
            
            var locLabel = new Label();
            locLabel.Text = "钓鱼地点: ";
            locationHBox.AddChild(locLabel);
            
            _locationOption = new OptionButton();
            foreach (var loc in Enum.GetValues(typeof(FishingLocationType)))
            {
                _locationOption.AddItem(loc.ToString(), (int)loc);
            }
            _locationOption.CustomMinimumSize = new Vector2(200, 0);
            locationHBox.AddChild(_locationOption);
            
            // 鱼竿选择
            var rodLabel = new Label();
            rodLabel.Text = "  鱼竿: ";
            locationHBox.AddChild(rodLabel);
            
            _rodOption = new OptionButton();
            foreach (var rod in Enum.GetValues(typeof(RodType)))
            {
                _rodOption.AddItem(rod.ToString(), (int)rod);
            }
            _rodOption.CustomMinimumSize = new Vector2(150, 0);
            locationHBox.AddChild(_rodOption);
            
            // 鱼饵选择
            var baitLabel = new Label();
            baitLabel.Text = "  鱼饵: ";
            locationHBox.AddChild(baitLabel);
            
            _baitOption = new OptionButton();
            foreach (var bait in Enum.GetValues(typeof(BaitType)))
            {
                _baitOption.AddItem(bait.ToString(), (int)bait);
            }
            _baitOption.CustomMinimumSize = new Vector2(150, 0);
            locationHBox.AddChild(_baitOption);
            
            // 操作按钮
            var buttonHBox = new HBoxContainer();
            buttonHBox.Alignment = BoxContainer.AlignmentMode.Center;
            mainVBox.AddChild(buttonHBox);
            
            _startButton = new Button();
            _startButton.Text = "开始钓鱼";
            _startButton.CustomMinimumSize = new Vector2(120, 40);
            _startButton.Pressed += OnStartPressed;
            buttonHBox.AddChild(_startButton);
            
            _reelButton = new Button();
            _reelButton.Text = "提竿!";
            _reelButton.CustomMinimumSize = new Vector2(120, 40);
            _reelButton.Disabled = true;
            _reelButton.Pressed += OnReelPressed;
            buttonHBox.AddChild(_reelButton);
            
            _cancelButton = new Button();
            _cancelButton.Text = "取消";
            _cancelButton.CustomMinimumSize = new Vector2(120, 40);
            _cancelButton.Pressed += OnCancelPressed;
            buttonHBox.AddChild(_cancelButton);
            
            // 状态显示
            _statusLabel = new Label();
            _statusLabel.Text = "点击「开始钓鱼」开始";
            _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _statusLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0));
            mainVBox.AddChild(_statusLabel);
            
            // 更新UI
            UpdateUI();
        }
        
        private void CreateFishingTab()
        {
            var tab = new ScrollContainer();
            tab.Name = "钓鱼";
            _tabContainer.AddChild(tab);
            
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vbox.AddThemeConstantOverride("separation", 15);
            tab.AddChild(vbox);
            
            // 当前状态
            var stateLabel = new Label();
            stateLabel.Text = "当前状态";
            stateLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(stateLabel);
            
            _currentStateLabel = new Label();
            _currentStateLabel.Text = "空闲";
            vbox.AddChild(_currentStateLabel);
            
            // 等待进度条
            var waitLabel = new Label();
            waitLabel.Text = "等待咬钩:";
            vbox.AddChild(waitLabel);
            
            _waitingProgressBar = new ProgressBar();
            _waitingProgressBar.ShowPercentage = false;
            vbox.AddChild(_waitingProgressBar);
            
            // 收线进度条
            var reelLabel = new Label();
            reelLabel.Text = "收线进度:";
            vbox.AddChild(reelLabel);
            
            _reelingProgressBar = new ProgressBar();
            _reelingProgressBar.ShowPercentage = false;
            vbox.AddChild(_reelingProgressBar);
            
            // 当前鱼
            _currentFishLabel = new Label();
            _currentFishLabel.Text = "当前咬钩: 无";
            _currentFishLabel.AddThemeColorOverride("font_color", new Color(1, 0.5, 0));
            vbox.AddChild(_currentFishLabel);
            
            // 统计数据
            var statsLabel = new Label();
            statsLabel.Text = "本次统计";
            statsLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(statsLabel);
            
            var statsHBox = new HBoxContainer();
            vbox.AddChild(statsHBox);
            
            _totalCatchesLabel = new Label();
            _totalCatchesLabel.Text = "钓获: 0";
            statsHBox.AddChild(_totalCatchesLabel);
            
            _totalValueLabel = new Label();
            _totalValueLabel.Text = "  价值: 0";
            statsHBox.AddChild(_totalValueLabel);
            
            _successRateLabel = new Label();
            _successRateLabel.Text = "  成功率: 0%";
            statsHBox.AddChild(_successRateLabel);
            
            _perfectCatchesLabel = new Label();
            _perfectCatchesLabel.Text = "  完美: 0";
            statsHBox.AddChild(_perfectCatchesLabel);
        }
        
        private void CreateCollectionTab()
        {
            var tab = new ScrollContainer();
            tab.Name = "收藏";
            _tabContainer.AddChild(tab);
            
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vbox.AddThemeConstantOverride("separation", 10);
            tab.AddChild(vbox);
            
            // 标题
            var title = new Label();
            title.Text = "鱼类收藏";
            title.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(title);
            
            // 统计
            var countLabel = new Label();
            countLabel.Name = "CountLabel";
            vbox.AddChild(countLabel);
            
            // 鱼类网格
            _fishScroll = new ScrollContainer();
            _fishScroll.CustomMinimumSize = new Vector2(0, 400);
            vbox.AddChild(_fishScroll);
            
            _fishGrid = new GridContainer();
            _fishGrid.Columns = 4;
            _fishGrid.AddThemeConstantOverride("h_separation", 10);
            _fishGrid.AddThemeConstantOverride("v_separation", 10);
            _fishScroll.AddChild(_fishGrid);
        }
        
        private void CreateStatisticsTab()
        {
            var tab = new ScrollContainer();
            tab.Name = "统计";
            _tabContainer.AddChild(tab);
            
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            vbox.AddThemeConstantOverride("separation", 15);
            tab.AddChild(vbox);
            
            // 标题
            var title = new Label();
            title.Text = "钓鱼统计";
            title.AddThemeFontSizeOverride("font_size", 20);
            title.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(title);
            
            // 最大钓获
            _biggestCatchLabel = new Label();
            _biggestCatchLabel.Text = "最大钓获: 暂无";
            vbox.AddChild(_biggestCatchLabel);
            
            // 独特物种
            _uniqueSpeciesLabel = new Label();
            _uniqueSpeciesLabel.Name = "UniqueSpeciesLabel";
            vbox.AddChild(_uniqueSpeciesLabel);
            
            // 稀有度统计
            var rarityLabel = new Label();
            rarityLabel.Text = "稀有度统计";
            rarityLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(rarityLabel);
            
            _rarityStatsGrid = new GridContainer();
            _rarityStatsGrid.Columns = 2;
            vbox.AddChild(_rarityStatsGrid);
        }
        
        private void ConnectSignals()
        {
            if (FishingSystem.Instance != null)
            {
                FishingSystem.Instance.FishingStarted += location => OnFishingStarted(location);
                FishingSystem.Instance.FishBiting += fishName => OnFishBiting(fishName);
                FishingSystem.Instance.FishCaught += (fishName, value, weight) => OnFishCaught(fishName, value, weight);
                FishingSystem.Instance.FishEscaped += OnFishEscaped;
                FishingSystem.Instance.LevelUp += newLevel => OnLevelUp(newLevel);
            }
        }
        
        public override void _Process(double delta)
        {
            UpdateFishingState();
        }
        
        private void UpdateFishingState()
        {
            if (FishingSystem.Instance == null) return;
            
            var session = FishingSystem.Instance.GetCurrentSession();
            if (session == null)
            {
                _currentStateLabel.Text = "空闲";
                _startButton.Disabled = false;
                _reelButton.Disabled = true;
                return;
            }
            
            switch (session.CurrentState)
            {
                case FishingState.Waiting:
                    _currentStateLabel.Text = "等待中...";
                    _waitingProgressBar.Value = 1.0 - (session.CurrentProgress > 0 ? session.CurrentProgress : 0);
                    _startButton.Disabled = true;
                    _reelButton.Disabled = true;
                    break;
                    
                case FishingState.Biting:
                    _currentStateLabel.Text = "鱼咬钩了！快提竿！";
                    _reelingProgressBar.Value = session.CurrentProgress;
                    _startButton.Disabled = true;
                    _reelButton.Disabled = false;
                    break;
                    
                case FishingState.Reeling:
                    _currentStateLabel.Text = "收线中...";
                    _reelingProgressBar.Value = session.CurrentProgress;
                    _startButton.Disabled = true;
                    _reelButton.Disabled = true;
                    break;
                    
                default:
                    _currentStateLabel.Text = session.CurrentState.ToString();
                    break;
            }
            
            // 更新本次统计
            if (session.TotalAttempts > 0)
            {
                _totalCatchesLabel.Text = $"钓获: {session.SuccessfulCatches}";
                _totalValueLabel.Text = $"  价值: {session.TotalValue}";
                float rate = (float)session.SuccessfulCatches / session.TotalAttempts * 100;
                _successRateLabel.Text = $"  成功率: {rate:F1}%";
            }
        }
        
        private void UpdateUI()
        {
            if (FishingSystem.Instance == null) return;
            
            var data = FishingSystem.Instance.GetPlayerData();
            
            // 等级
            _levelLabel.Text = $"等级: {data.CurrentLevel}";
            
            // 经验
            int nextXP = FishingDatabase.GetXPForNextLevel(data.CurrentLevel);
            _xpProgressBar.MaxValue = nextXP;
            _xpProgressBar.Value = data.CurrentXP;
            _xpLabel.Text = $"{data.CurrentXP} / {nextXP} XP";
            
            // 完美钓获
            _perfectCatchesLabel.Text = $"  完美: {data.PerfectCatches}";
            
            // 更新收藏
            UpdateCollection();
            
            // 更新统计
            UpdateStatistics();
        }
        
        private void UpdateCollection()
        {
            // 清空现有
            foreach (var child in _fishGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            if (FishingSystem.Instance == null) return;
            
            int unlocked = FishingSystem.Instance.GetUnlockedFishCount();
            int total = FishingSystem.Instance.GetTotalFishCount();
            
            // 更新计数标签
            var countLabel = _fishGrid.GetParent().GetNode<Label>("CountLabel");
            if (countLabel != null)
            {
                countLabel.Text = $"已解锁: {unlocked} / {total}";
            }
            
            var data = FishingSystem.Instance.GetPlayerData();
            
            // 显示鱼类
            foreach (var fish in FishingDatabase.Fish.Values)
            {
                bool isUnlocked = data.UnlockedFish.ContainsKey(fish.ID) && data.UnlockedFish[fish.ID];
                
                var fishPanel = new PanelContainer();
                fishPanel.CustomMinimumSize = new Vector2(150, 80);
                
                if (isUnlocked)
                {
                    // 已解锁
                    var vbox = new VBoxContainer();
                    fishPanel.AddChild(vbox);
                    
                    var nameLabel = new Label();
                    nameLabel.Text = fish.Name;
                    nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    
                    // 稀有度颜色
                    Color rarityColor = GetRarityColor(fish.Rarity);
                    nameLabel.AddThemeColorOverride("font_color", rarityColor);
                    vbox.AddChild(nameLabel);
                    
                    var countLabel2 = new Label();
                    int count = data.FishCaught.ContainsKey(fish.ID) ? data.FishCaught[fish.ID] : 0;
                    countLabel2.Text = $"x{count}";
                    countLabel2.HorizontalAlignment = HorizontalAlignment.Center;
                    vbox.AddChild(countLabel2);
                    
                    var rarityLabel = new Label();
                    rarityLabel.Text = fish.Rarity.ToString();
                    rarityLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    rarityLabel.AddThemeFontSizeOverride("font_size", 10);
                    vbox.AddChild(rarityLabel);
                }
                else
                {
                    // 未解锁
                    var label = new Label();
                    label.Text = "???";
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.VerticalAlignment = VerticalAlignment.Center;
                    fishPanel.AddChild(label);
                }
                
                _fishGrid.AddChild(fishPanel);
            }
        }
        
        private void UpdateStatistics()
        {
            if (FishingSystem.Instance == null) return;
            
            var data = FishingSystem.Instance.GetPlayerData();
            
            // 最大钓获
            if (data.BiggestCatchWeight > 0)
            {
                _biggestCatchLabel.Text = $"最大钓获: {data.BiggestCatchFish} ({data.BiggestCatchWeight}g)";
            }
            
            // 独特物种
            int unique = 0;
            foreach (var unlocked in data.UnlockedFish.Values)
            {
                if (unlocked) unique++;
            }
            
            var uniqueLabel = _biggestCatchLabel.GetNode<Label>("../UniqueSpeciesLabel");
            if (uniqueLabel != null)
            {
                uniqueLabel.Text = $"独特物种: {unique} / {FishingDatabase.Fish.Count}";
            }
            
            // 稀有度统计
            foreach (var child in _rarityStatsGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            var rarityCounts = new Dictionary<FishType, int>();
            foreach (var kvp in data.FishCaught)
            {
                if (kvp.Value > 0)
                {
                    var fish = FishingDatabase.Fish[kvp.Key];
                    if (!rarityCounts.ContainsKey(fish.Rarity))
                        rarityCounts[fish.Rarity] = 0;
                    rarityCounts[fish.Rarity]++;
                }
            }
            
            foreach (FishType rarity in Enum.GetValues(typeof(FishType)))
            {
                var label1 = new Label();
                label1.Text = rarity.ToString() + ":";
                _rarityStatsGrid.AddChild(label1);
                
                var label2 = new Label();
                label2.Text = rarityCounts.ContainsKey(rarity) ? rarityCounts[rarity].ToString() : "0";
                label2.AddThemeColorOverride("font_color", GetRarityColor(rarity));
                _rarityStatsGrid.AddChild(label2);
            }
        }
        
        private Color GetRarityColor(FishType rarity)
        {
            return rarity switch
            {
                FishType.Common => new Color(0.7f, 0.7f, 0.7f),
                FishType.Uncommon => new Color(0.3f, 0.8f, 0.3f),
                FishType.Rare => new Color(0.3f, 0.5f, 1.0f),
                FishType.Epic => new Color(0.6f, 0.3f, 0.9f),
                FishType.Legendary => new Color(1.0f, 0.6f, 0.0f),
                FishType.Mythic => new Color(1.0f, 0.2f, 0.2f),
                _ => new Color(1, 1, 1)
            };
        }
        
        #region 事件处理
        
        private void OnStartPressed()
        {
            if (FishingSystem.Instance == null) return;
            
            var location = (FishingLocationType)_locationOption.GetSelectedId();
            var rod = (RodType)_rodOption.GetSelectedId();
            var bait = (BaitType)_baitOption.GetSelectedId();
            
            if (FishingSystem.Instance.StartFishing(location, rod, bait))
            {
                _statusLabel.Text = $"正在 {location} 钓鱼...";
                _startButton.Disabled = true;
            }
        }
        
        private void OnReelPressed()
        {
            if (FishingSystem.Instance == null) return;
            
            bool success = FishingSystem.Instance.ReelIn();
            _statusLabel.Text = success ? "钓到了！" : "鱼逃脱了...";
            
            // 更新UI
            UpdateUI();
        }
        
        private void OnCancelPressed()
        {
            if (FishingSystem.Instance == null) return;
            
            FishingSystem.Instance.CancelFishing();
            _statusLabel.Text = "钓鱼已取消";
            _startButton.Disabled = false;
            _reelButton.Disabled = true;
            
            UpdateUI();
        }
        
        private void OnFishingStarted(string location)
        {
            _statusLabel.Text = $"开始钓鱼 - {location}";
        }
        
        private void OnFishBiting(string fishName)
        {
            _statusLabel.Text = $"鱼咬钩了！是 {fishName}！";
            _currentFishLabel.Text = $"当前咬钩: {fishName}";
        }
        
        private void OnFishCaught(string fishName, int value, int weight)
        {
            _statusLabel.Text = $"钓到了 {fishName}！重量: {weight}g, 价值: {value}";
            _currentFishLabel.Text = "当前咬钩: 无";
            UpdateUI();
        }
        
        private void OnFishEscaped()
        {
            _statusLabel.Text = "鱼逃脱了...";
            _currentFishLabel.Text = "当前咬钩: 无";
        }
        
        private void OnLevelUp(int newLevel)
        {
            _statusLabel.Text = $"🎉 钓鱼等级提升到 {newLevel}！";
            UpdateUI();
        }
        
        #endregion
        
        #region 快捷键处理
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // F键切换UI
                if (keyEvent.Keycode == Key.F)
                {
                    Visible = !Visible;
                    if (Visible)
                    {
                        UpdateUI();
                    }
                }
            }
        }
        
        #endregion
    }
}
