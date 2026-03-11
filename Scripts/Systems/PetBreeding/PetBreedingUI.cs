using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 宠物繁殖UI界面
    /// </summary>
    public partial class PetBreedingUI : Control
    {
        private Control _mainContainer;
        private VBoxContainer _breedingsContainer;
        private VBoxContainer _historyContainer;
        private VBoxContainer _statsContainer;
        private TabContainer _tabContainer;
        
        private Label _titleLabel;
        private Label _statsLabel;
        
        // 当前选中
        private string _selectedParent1;
        private string _selectedParent2;
        private PetBreedingData.BreedingType _selectedType = PetBreedingData.BreedingType.Basic;
        
        // UI组件引用
        private OptionButton _typeOption;
        private Button _startButton;
        private ItemList _parent1List;
        private ItemList _parent2List;
        
        public override void _Ready()
        {
            SetupUI();
            SetupInput();
            RefreshData();
        }
        
        private void SetupUI()
        {
            // 主容器
            _mainContainer = new Control
            {
                Name = "MainContainer",
                LayoutMode = 1,
                AnchorsPreset = 15,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                OffsetLeft = 100,
                OffsetTop = 50,
                OffsetRight = -100,
                OffsetBottom = -50
            };
            AddChild(_mainContainer);
            
            // 背景面板
            var bgPanel = new PanelContainer
            {
                LayoutMode = 1,
                AnchorsPreset = 15,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                ThemeOverrideStyles/panel = new StyleBoxFlat
                {
                    BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                    BorderWidthLeft = 2,
                    BorderWidthTop = 2,
                    BorderWidthRight = 2,
                    BorderWidthBottom = 2,
                    BorderColor = new Color(0.3f, 0.3f, 0.4f)
                }
            };
            _mainContainer.AddChild(bgPanel);
            
            // 标题
            _titleLabel = new Label
            {
                Text = "宠物繁殖系统",
                LayoutMode = 1,
                OffsetLeft = 20,
                OffsetTop = 10,
                OffsetRight = -20,
                OffsetBottom = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                ThemeOverrideFonts/font = new FontFile()
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            bgPanel.AddChild(_titleLabel);
            
            // 关闭按钮
            var closeButton = new Button
            {
                Text = "X",
                LayoutMode = 1,
                AnchorLeft = 1.0f,
                AnchorRight = 1.0f,
                OffsetLeft = -50,
                OffsetTop = 10,
                OffsetRight = -20,
                OffsetBottom = 40,
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd
            };
            closeButton.Pressed += () => Hide();
            bgPanel.AddChild(closeButton);
            
            // Tab容器
            _tabContainer = new TabContainer
            {
                LayoutMode = 1,
                AnchorsPreset = 15,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                OffsetLeft = 10,
                OffsetTop = 60,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            bgPanel.AddChild(_tabContainer);
            
            // 繁殖标签页
            var breedingTab = new Control { Name = "Breeding" };
            _tabContainer.AddChild(breedingTab);
            SetupBreedingTab(breedingTab);
            
            // 进行中标签页
            var activeTab = new Control { Name = "Active" };
            _tabContainer.AddChild(activeTab);
            SetupActiveTab(activeTab);
            
            // 历史标签页
            var historyTab = new Control { Name = "History" };
            _tabContainer.AddChild(historyTab);
            SetupHistoryTab(historyTab);
            
            // 统计标签页
            var statsTab = new Control { Name = "Statistics" };
            _tabContainer.AddChild(statsTab);
            SetupStatsTab(statsTab);
            
            // 初始隐藏
            _mainContainer.Visible = false; 
        }
        
        private void SetupBreedingTab(Control tab)
        {
            var vbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorsPreset = 15,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            tab.AddChild(vbox);
            
            // 繁殖类型选择
            var typeLabel = new Label { Text = "繁殖类型:" };
            typeLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(typeLabel);
            
            _typeOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(200, 40)
            };
            _typeOption.AddItem("基础繁殖 (100金, 5分钟)", (int)PetBreedingData.BreedingType.Basic);
            _typeOption.AddItem("高级繁殖 (500金, 3分钟)", (int)PetBreedingData.BreedingType.Advanced);
            _typeOption.AddItem("传奇繁殖 (2000金, 1分钟)", (int)PetBreedingData.BreedingType.Legendary);
            _typeOption.ItemSelected += OnTypeSelected;
            vbox.AddChild(_typeOption);
            
            // 亲本1选择
            var parent1Label = new Label { Text = "选择亲本1:" };
            parent1Label.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(parent1Label);
            
            _parent1List = new ItemList
            {
                CustomMinimumSize = new Vector2(0, 150)
            };
            _parent1List.ItemSelected += (index) => OnParent1Selected(index);
            vbox.AddChild(_parent1List);
            
            // 亲本2选择
            var parent2Label = new Label { Text = "选择亲本2:" };
            parent2Label.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(parent2Label);
            
            _parent2List = new ItemList
            {
                CustomMinimumSize = new Vector2(0, 150)
            };
            _parent2List.ItemSelected += (index) => OnParent2Selected(index);
            vbox.AddChild(_parent2List);
            
            // 开始繁殖按钮
            _startButton = new Button
            {
                Text = "开始繁殖",
                CustomMinimumSize = new Vector2(200, 50)
            };
            _startButton.AddThemeFontSizeOverride("font_size", 20);
            _startButton.Pressed += OnStartBreedingPressed;
            vbox.AddChild(_startButton);
        }
        
        private void SetupActiveTab(Control tab)
        {
            var scroll = new ScrollContainer
            {
                LayoutMode = 1,
                AnchorsPreset = 15,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            tab.AddChild(scroll);
            
            _breedingsContainer = new VBoxContainer
            {
                LayoutMode = 1,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            scroll.AddChild(_breedingsContainer);
        }
        
        private void SetupHistoryTab(Control tab)
        {
            var scroll = new ScrollContainer
            {
                LayoutMode = 1,
                AnchorsPreset = 15,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            tab.AddChild(scroll);
            
            _historyContainer = new VBoxContainer
            {
                LayoutMode = 1,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            scroll.AddChild(_historyContainer);
        }
        
        private void SetupStatsTab(Control tab)
        {
            var vbox = new VBoxContainer
            {
                LayoutMode = 1,
                AnchorsPreset = 15,
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                OffsetLeft = 20,
                OffsetTop = 20,
                OffsetRight = -20,
                OffsetBottom = -20
            };
            tab.AddChild(vbox);
            
            _statsLabel = new Label
            {
                Text = "繁殖统计",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _statsLabel.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(_statsLabel);
            
            _statsContainer = new VBoxContainer();
            vbox.AddChild(_statsContainer);
        }
        
        private void SetupInput()
        {
            // 输入绑定
        }
        
        private void RefreshData()
        {
            RefreshPetLists();
            RefreshActiveBreedings();
            RefreshHistory();
            RefreshStats();
        }
        
        private void RefreshPetLists()
        {
            if (_parent1List == null || _parent2List == null) return;
            
            _parent1List.Clear();
            _parent2List.Clear();
            
            // 从宠物管理器获取宠物列表
            if (PetManager.Instance != null)
            {
                var pets = PetManager.Instance.GetPets();
                foreach (var pet in pets)
                {
                    string displayText = $"{pet.Name} (Lv.{pet.Level} {pet.Rarity})";
                    _parent1List.AddItem(displayText);
                    _parent2List.AddItem(displayText);
                }
            }
        }
        
        private void RefreshActiveBreedings()
        {
            if (_breedingsContainer == null) return;
            
            foreach (var child in _breedingsContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            if (PetBreedingSystem.Instance == null) return;
            
            var breedings = PetBreedingSystem.Instance.GetActiveBreedings();
            
            if (breedings.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无进行中的繁殖",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _breedingsContainer.AddChild(emptyLabel);
                return;
            }
            
            foreach (var breeding in breedings)
            {
                var panel = CreateBreedingCard(breeding);
                _breedingsContainer.AddChild(panel);
            }
        }
        
        private void RefreshHistory()
        {
            if (_historyContainer == null) return;
            
            foreach (var child in _historyContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            if (PetBreedingSystem.Instance == null) return;
            
            var history = PetBreedingSystem.Instance.GetBreedingHistory();
            
            if (history.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无繁殖历史",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _historyContainer.AddChild(emptyLabel);
                return;
            }
            
            foreach (var record in history)
            {
                var panel = CreateHistoryCard(record);
                _historyContainer.AddChild(panel);
            }
        }
        
        private void RefreshStats()
        {
            if (_statsContainer == null) return;
            
            foreach (var child in _statsContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            if (PetBreedingSystem.Instance == null) return;
            
            var stats = PetBreedingSystem.Instance.GetStatistics();
            
            AddStatRow("总繁殖次数:", stats["total_breedings"].ToString());
            AddStatRow("成功次数:", stats["successful_breedings"].ToString());
            AddStatRow("传奇繁殖次数:", stats["legendary_breedings"].ToString());
            AddStatRow("成功率:", $"{float.Parse(stats["success_rate"].ToString()):P1}");
        }
        
        private void AddStatRow(string label, string value)
        {
            var hbox = new HBoxContainer();
            _statsContainer.AddChild(hbox);
            
            var labelNode = new Label
            {
                Text = label,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            labelNode.AddThemeFontSizeOverride("font_size", 18);
            hbox.AddChild(labelNode);
            
            var valueLabel = new Label
            {
                Text = value,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            valueLabel.AddThemeFontSizeOverride("font_size", 18);
            hbox.AddChild(valueLabel);
        }
        
        private Control CreateBreedingCard(PetBreedingData.BreedingInstance breeding)
        {
            var panel = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 80),
                ThemeOverrideStyles/panel = new StyleBoxFlat
                {
                    BgColor = new Color(0.2f, 0.2f, 0.3f)
                }
            };
            
            var hbox = new HBoxContainer
            {
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            panel.AddChild(hbox);
            
            var vbox = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            hbox.AddChild(vbox);
            
            // 亲本信息
            var infoLabel = new Label
            {
                Text = $"{breeding.Parent1?.PetName} x {breeding.Parent2?.PetName}"
            };
            infoLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(infoLabel);
            
            // 类型
            var typeLabel = new Label
            {
                Text = $"类型: {PetBreedingDatabase.GetConfig(breeding.Type)?.Name}"
            };
            vbox.AddChild(typeLabel);
            
            // 进度条
            var progress = new ProgressBar
            {
                CustomMinimumSize = new Vector2(0, 20),
                Value = PetBreedingSystem.Instance.GetBreedingProgress(breeding.InstanceId) * 100
            };
            vbox.AddChild(progress);
            
            // 剩余时间
            var timeLabel = new Label
            {
                Text = $"剩余时间: {PetBreedingSystem.Instance.GetRemainingTime(breeding.InstanceId)}秒"
            };
            vbox.AddChild(timeLabel);
            
            // 取消按钮
            var cancelButton = new Button { Text = "取消" };
            cancelButton.Pressed += () => OnCancelBreeding(breeding.InstanceId);
            hbox.AddChild(cancelButton);
            
            return panel;
        }
        
        private Control CreateHistoryCard(PetBreedingData.BreedingRecord record)
        {
            var panel = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 60),
                ThemeOverrideStyles/panel = new StyleBoxFlat
                {
                    BgColor = record.Success ? new Color(0.15f, 0.3f, 0.15f) : new Color(0.3f, 0.15f, 0.15f)
                }
            };
            
            var hbox = new HBoxContainer
            {
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            panel.AddChild(hbox);
            
            var infoLabel = new Label
            {
                Text = $"{record.Parent1Name} x {record.Parent2Name} -> {(record.Success ? record.OffspringName : "失败")} ({record.OffspringRarity})",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            infoLabel.AddThemeFontSizeOverride("font_size", 14);
            hbox.AddChild(infoLabel);
            
            var timeLabel = new Label
            {
                Text = record.BreedingTime.ToString("MM-dd HH:mm"),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            hbox.AddChild(timeLabel);
            
            return panel;
        }
        
        #region 事件处理
        
        private void OnTypeSelected(long index)
        {
            _selectedType = (PetBreedingData.BreedingType)index;
        }
        
        private void OnParent1Selected(long index)
        {
            if (PetManager.Instance == null) return;
            
            var pets = PetManager.Instance.GetPets();
            if (index >= 0 && index < pets.Count)
            {
                _selectedParent1 = pets[(int)index].Id;
            }
        }
        
        private void OnParent2Selected(long index)
        {
            if (PetManager.Instance == null) return;
            
            var pets = PetManager.Instance.GetPets();
            if (index >= 0 && index < pets.Count)
            {
                _selectedParent2 = pets[(int)index].Id;
            }
        }
        
        private void OnStartBreedingPressed()
        {
            if (string.IsNullOrEmpty(_selectedParent1) || string.IsNullOrEmpty(_selectedParent2))
            {
                GD.PrintErr("[PetBreedingUI] Please select both parents");
                return;
            }
            
            if (_selectedParent1 == _selectedParent2)
            {
                GD.PrintErr("[PetBreedingUI] Please select different parents");
                return;
            }
            
            if (PetBreedingSystem.Instance == null)
            {
                GD.PrintErr("[PetBreedingUI] PetBreedingSystem not initialized");
                return;
            }
            
            bool success = PetBreedingSystem.Instance.StartBreeding(
                _selectedParent1, 
                _selectedParent2, 
                _selectedType
            );
            
            if (success)
            {
                // 切换到进行中标签页
                _tabContainer.CurrentTab = 1;
                RefreshData();
            }
        }
        
        private void OnCancelBreeding(string instanceId)
        {
            if (PetBreedingSystem.Instance != null)
            {
                PetBreedingSystem.Instance.CancelBreeding(instanceId);
                RefreshData();
            }
        }
        
        #endregion
        
        #region 显示/隐藏
        
        public void Show()
        {
            if (_mainContainer != null)
            {
                _mainContainer.Visible = true;
                RefreshData();
                
                // 显示动画
                var tween = CreateTween();
                tween.TweenProperty(_mainContainer, "modulate:a", 1.0, 0.3f);
            }
        }
        
        public void Hide()
        {
            if (_mainContainer != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(_mainContainer, "modulate:a", 0.0, 0.3f);
                tween.TweenCallback(() => _mainContainer.Visible = false);
            }
        }
        
        public void Toggle()
        {
            if (_mainContainer != null && _mainContainer.Visible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        
        #endregion
        
        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Ctrl+B 打开/关闭繁殖界面
                if (keyEvent.CtrlKey && keyEvent.Keycode == Key.B)
                {
                    Toggle();
                    GetTree().SetInputAsHandled();
                }
            }
        }
    }
}
