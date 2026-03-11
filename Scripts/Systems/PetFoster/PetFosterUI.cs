using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

namespace ClawRPG.Scripts.Systems.PetFoster
{
    /// <summary>
    /// 宠物寄养界面
    /// </summary>
    public partial class PetFosterUI : Control
    {
        private Control _mainPanel;
        private VBoxContainer _mainContainer;
        private Label _titleLabel;
        
        // 标签页
        private TabContainer _tabContainer;
        private Control _fosterTab;
        private Control _statsTab;
        
        // 寄养标签页
        private OptionButton _petSelector;
        private OptionButton _fosterTypeSelector;
        private OptionButton _configSelector;
        private Label _configInfoLabel;
        private Button _startButton;
        
        // 当前寄养列表
        private VBoxContainer _activeFostersContainer;
        
        // 统计标签页
        private Label _totalFostersLabel;
        private Label _totalExpLabel;
        private Label _totalGoldLabel;
        private Label _totalMaterialsLabel;
        
        private bool _visible = false; 
        
        public override void _Ready()
        {
            SetupUI();
            Hide();
        }
        
        private void SetupUI()
        {
            // 主面板
            _mainPanel = new Control
            {
                Name = "MainPanel",
                CustomMinimumSize = new Vector2(600, 500)
            };
            AddChild(_mainPanel);
            
            var panel = new PanelContainer
            {
                OffsetLeft = 100,
                OffsetTop = 50,
                OffsetRight = 700,
                OffsetBottom = 550
            };
            _mainPanel.AddChild(panel);
            
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.SetCornerRadiusAll(8);
            panel.AddThemeStyleboxOverride("panel", style);
            
            _mainContainer = new VBoxContainer
            {
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = 590,
                OffsetBottom = 490
            };
            panel.AddChild(_mainContainer);
            
            // 标题
            _titleLabel = new Label
            {
                Text = "宠物寄养",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(_titleLabel);
            
            // 关闭按钮
            var closeBtn = new Button
            {
                Text = "X",
                CustomMinimumSize = new Vector2(30, 30)
            };
            closeBtn.Position = new Vector2(550, 0);
            closeBtn.Pressed += () => ToggleUI();
            _mainContainer.AddChild(closeBtn);
            
            // 标签页容器
            _tabContainer = new TabContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            _mainContainer.AddChild(_tabContainer);
            
            SetupFosterTab();
            SetupStatsTab();
        }
        
        private void SetupFosterTab()
        {
            _fosterTab = new Control();
            _fosterTab.Name = "寄养";
            _tabContainer.AddChild(_fosterTab);
            
            var vbox = new VBoxContainer
            {
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = 570,
                OffsetBottom = 380
            };
            _fosterTab.AddChild(vbox);
            
            // 宠物选择
            var petLabel = new Label { Text = "选择宠物:" };
            vbox.AddChild(petLabel);
            
            _petSelector = new OptionButton();
            _petSelector.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _petSelector.ItemSelected += OnPetSelected;
            vbox.AddChild(_petSelector);
            
            // 寄养类型选择
            var typeLabel = new Label { Text = "寄养类型:" };
            vbox.AddChild(typeLabel);
            
            _fosterTypeSelector = new OptionButton();
            _fosterTypeSelector.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _fosterTypeSelector.AddItem("休息 (恢复饱食度)", (int)FosterType.Rest);
            _fosterTypeSelector.AddItem("训练 (获得经验)", (int)FosterType.Training);
            _fosterTypeSelector.AddItem("采集 (获得材料)", (int)FosterType.Gathering);
            _fosterTypeSelector.AddItem("玩耍 (提升好感度)", (int)FosterType.Play);
            _fosterTypeSelector.AddItem("守护 (获得金币)", (int)FosterType.Guard);
            _fosterTypeSelector.ItemSelected += OnTypeSelected;
            vbox.AddChild(_fosterTypeSelector);
            
            // 寄养配置选择
            var configLabel = new Label { Text = "选择寄养方案:" };
            vbox.AddChild(configLabel);
            
            _configSelector = new OptionButton();
            _configSelector.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _configSelector.ItemSelected += OnConfigSelected;
            vbox.AddChild(_configSelector);
            
            // 配置信息
            _configInfoLabel = new Label
            {
                Text = "请选择寄养方案",
                CustomMinimumSize = new Vector2(0, 60)
            };
            vbox.AddChild(_configInfoLabel);
            
            // 开始按钮
            _startButton = new Button
            {
                Text = "开始寄养",
                CustomMinimumSize = new Vector2(0, 40)
            };
            _startButton.Pressed += OnStartPressed;
            vbox.AddChild(_startButton);
            
            // 分隔
            var separator = new HSeparator();
            vbox.AddChild(separator);
            
            // 当前寄养
            var activeLabel = new Label { Text = "当前寄养:" };
            vbox.AddChild(activeLabel);
            
            _activeFostersContainer = new VBoxContainer();
            vbox.AddChild(_activeFostersContainer);
            
            RefreshPetList();
        }
        
        private void SetupStatsTab()
        {
            _statsTab = new Control();
            _statsTab.Name = "统计";
            _tabContainer.AddChild(_statsTab);
            
            var vbox = new VBoxContainer
            {
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = 570,
                OffsetBottom = 380
            };
            _statsTab.AddChild(vbox);
            
            _totalFostersLabel = new Label { Text = "总寄养次数: 0" };
            vbox.AddChild(_totalFostersLabel);
            
            _totalExpLabel = new Label { Text = "总获得经验: 0" };
            vbox.AddChild(_totalExpLabel);
            
            _totalGoldLabel = new Label { Text = "总获得金币: 0" };
            vbox.AddChild(_totalGoldLabel);
            
            _totalMaterialsLabel = new Label { Text = "总获得材料: 0" };
            vbox.AddChild(_totalMaterialsLabel);
            
            RefreshStats();
        }
        
        private void RefreshPetList()
        {
            _petSelector.Clear();
            
            // 获取玩家宠物列表
            var petSystem = GetTree().GetFirstNodeInGroup("PetSystem") as Node;
            if (petSystem != null)
            {
                var getPetsMethod = petSystem.GetType().GetMethod("GetPets");
                var pets = getPetsMethod?.Invoke(petSystem, null) as IEnumerable<object>;
                
                if (pets != null)
                {
                    int index = 0;
                    foreach (var pet in pets)
                    {
                        var petId = pet.GetType().GetProperty("Id")?.GetValue(pet) as string;
                        var petName = pet.GetType().GetProperty("Name")?.GetValue(pet) as string ?? $"Pet_{index}";
                        _petSelector.AddItem($"{petName} ({petId})", index);
                        index++;
                    }
                }
            }
            
            // 如果没有宠物，添加示例
            if (_petSelector.ItemCount == 0)
            {
                _petSelector.AddItem("暂无宠物", 0);
            }
            
            OnPetSelected(0);
        }
        
        private void OnPetSelected(long index)
        {
            // 刷新配置列表
            RefreshConfigList();
        }
        
        private void OnTypeSelected(long index)
        {
            RefreshConfigList();
        }
        
        private void RefreshConfigList()
        {
            _configSelector.Clear();
            
            var selectedType = (FosterType)_fosterTypeSelector.GetSelectedId();
            var configs = PetFosterDatabase.GetConfigsByType(selectedType);
            
            int index = 0;
            foreach (var config in configs)
            {
                string durationStr = config.Duration >= 60 ? $"{config.Duration / 60}分钟" : $"{config.Duration}秒";
                _configSelector.AddItem($"{config.Name} - {durationStr} - {config.Cost}金币", index);
                index++;
            }
            
            if (_configSelector.ItemCount > 0)
            {
                _configSelector.Select(0);
                OnConfigSelected(0);
            }
            else
            {
                _configInfoLabel.Text = "没有可用的寄养方案";
            }
        }
        
        private void OnConfigSelected(long index)
        {
            var selectedType = (FosterType)_fosterTypeSelector.GetSelectedId();
            var configs = PetFosterDatabase.GetConfigsByType(selectedType);
            
            if (index >= 0 && index < configs.Count)
            {
                var config = configs[(int)index];
                string info = $"名称: {config.Name}\n";
                info += $"时长: {config.Duration / 60}分钟\n";
                info += $"费用: {config.Cost}金币\n";
                info += $"奖励: {config.ExpReward}经验";
                
                if (config.GoldReward > 0)
                    info += $", {config.GoldReward}金币";
                if (config.AffectionReward > 0)
                    info += $", {config.AffectionReward}好感度";
                    
                _configInfoLabel.Text = info;
            }
        }
        
        private void OnStartPressed()
        {
            if (_petSelector.ItemCount == 0 || _petSelector.GetSelectedId() < 0)
            {
                GD.Print("[PetFosterUI] No pet selected");
                return;
            }
            
            if (_configSelector.ItemCount == 0 || _configSelector.GetSelectedId() < 0)
            {
                GD.Print("[PetFosterUI] No config selected");
                return;
            }
            
            var selectedType = (FosterType)_fosterTypeSelector.GetSelectedId();
            var configs = PetFosterDatabase.GetConfigsByType(selectedType);
            var configIndex = _configSelector.GetSelectedId();
            
            if (configIndex >= 0 && configIndex < configs.Count)
            {
                var config = configs[(int)configIndex];
                var fosterSystem = PetFosterSystem.Instance;
                
                if (fosterSystem != null)
                {
                    string petId = $"pet_{_petSelector.GetSelectedId()}";
                    bool success = fosterSystem.StartFoster(petId, config.Id);
                    
                    if (success)
                    {
                        RefreshActiveFosters();
                        RefreshStats();
                    }
                }
            }
        }
        
        private void RefreshActiveFosters()
        {
            // 清理当前列表
            foreach (var child in _activeFostersContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var fosterSystem = PetFosterSystem.Instance;
            if (fosterSystem == null) return;
            
            var activeFosters = typeof(PetFosterSystem)
                .GetField("_playerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(fosterSystem) as PlayerFosterData;
            
            if (activeFosters == null) return;
            
            foreach (var kvp in activeFosters.ActiveFosters)
            {
                var foster = kvp.Value;
                var config = PetFosterDatabase.GetConfig(foster.ConfigId);
                
                string statusText = foster.Status == FosterStatus.Completed ? " (可领取)" : "";
                string timeText = foster.Status == FosterStatus.Fostering 
                    ? $" 剩余{fosterSystem.GetRemainingTime(kvp.Key)}秒"
                    : "";
                
                var hbox = new HBoxContainer();
                
                var label = new Label
                {
                    Text = $"宠物 {kvp.Key}: {config.Name}{statusText}{timeText}"
                };
                hbox.AddChild(label);
                
                if (foster.Status == FosterStatus.Completed)
                {
                    var claimBtn = new Button
                    {
                        Text = "领取"
                    };
                    int petId = int.Parse(kvp.Key.Split('_')[1]);
                    claimBtn.Pressed += () =>
                    {
                        fosterSystem.ClaimFosterReward(kvp.Key);
                        RefreshActiveFosters();
                        RefreshStats();
                    };
                    hbox.AddChild(claimBtn);
                }
                
                _activeFostersContainer.AddChild(hbox);
            }
            
            if (activeFosters.ActiveFosters.Count == 0)
            {
                var label = new Label { Text = "暂无寄养中的宠物" };
                _activeFostersContainer.AddChild(label);
            }
        }
        
        private void RefreshStats()
        {
            var fosterSystem = PetFosterSystem.Instance;
            if (fosterSystem == null) return;
            
            var stats = fosterSystem.GetStatistics();
            
            _totalFostersLabel.Text = $"总寄养次数: {stats["total_fosters"]}";
            _totalExpLabel.Text = $"总获得经验: {stats["total_exp"]}";
            _totalGoldLabel.Text = $"总获得金币: {stats["total_gold"]}";
            _totalMaterialsLabel.Text = $"总获得材料: {stats["total_materials"]}";
        }
        
        public void ToggleUI()
        {
            _visible = !_visible;
            
            if (_visible)
            {
                Show();
                RefreshPetList();
                RefreshActiveFosters();
                RefreshStats();
                
                // 显示动画
                var tween = CreateTween();
                tween.TweenProperty(this, "modulate:a", 1.0, 0.3f);
            }
            else
            {
                var tween = CreateTween();
                tween.TweenProperty(this, "modulate:a", 0.0, 0.3f);
                tween.TweenCallback(Callable.From(Hide));
            }
        }
        
        public override void _Process(double delta)
        {
            if (_visible && PetFosterSystem.Instance != null)
            {
                // 更新寄养进度
                var fosterSystem = PetFosterSystem.Instance;
                var activeFosters = typeof(PetFosterSystem)
                    .GetField("_playerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .GetValue(fosterSystem) as PlayerFosterData;
                
                if (activeFosters != null)
                {
                    bool needsRefresh = false; 
                    foreach (var kvp in activeFosters.ActiveFosters)
                    {
                        var foster = kvp.Value;
                        if (foster.Status == FosterStatus.Fostering)
                        {
                            int remaining = fosterSystem.GetRemainingTime(kvp.Key);
                            if (remaining <= 0)
                            {
                                foster.Status = FosterStatus.Completed;
                                needsRefresh = true;
                            }
                        }
                    }
                    
                    if (needsRefresh)
                    {
                        RefreshActiveFosters();
                    }
                }
            }
        }
    }
}
