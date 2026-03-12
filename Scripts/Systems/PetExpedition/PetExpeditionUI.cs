using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Systems;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 宠物探险UI
    /// </summary>
    public class PetExpeditionUI : Control
    {
        private PetExpeditionSystem _expeditionSystem;
        
        // UI组件
        private TabContainer _tabContainer;
        private VBoxContainer _activeExpeditionsList;
        private VBoxContainer _historyList;
        private VBoxContainer _statsPanel;
        
        // 探险列表
        private ItemList _expeditionTypeList;
        private ItemList _petList;
        private Label _selectedExpeditionLabel;
        private Label _selectedPetLabel;
        private Button _startButton;
        
        // 计时器
        private Timer _refreshTimer;
        
        public override void _Ready()
        {
            _expeditionSystem = PetExpeditionSystem.Instance;
            if (_expeditionSystem == null)
            {
                GD.PrintErr("[PetExpeditionUI] PetExpeditionSystem not found!");
                return;
            }
            
            SetupUI();
            SetupSignals();
            
            // 刷新计时器
            _refreshTimer = new Timer();
            _refreshTimer.WaitTime = 1.0f;
            _refreshTimer.Connect("timeout", this, nameof(OnRefreshTimer));
            AddChild(_refreshTimer);
            _refreshTimer.Start();
        }
        
        private void SetupUI()
        {
            // 主容器
            var mainContainer = new HBoxContainer();
            mainContainer.SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            mainContainer.MarginLeft = 50;
            mainContainer.MarginTop = 50;
            mainContainer.MarginRight = -50;
            mainContainer.MarginBottom = -50;
            AddChild(mainContainer);
            
            // 左侧 - 探险类型选择
            var leftPanel = new VBoxContainer;
            leftPanel.CustomMinimumSize = new Vector2(300, 0);
            mainContainer.AddChild(leftPanel);
            
            var expeditionTitle = new Label();
            expeditionTitle.Text = "Expedition Type";
            expeditionTitle.Align = Label.AlignEnum.Center;
            leftPanel.AddChild(expeditionTitle);
            
            _expeditionTypeList = new ItemList();
            _expeditionTypeList.CustomMinimumSize = new Vector2(0, 300);
            _expeditionTypeList.Connect("item_selected", this, nameof(OnExpeditionTypeSelected));
            leftPanel.AddChild(_expeditionTypeList);
            
            // 添加探险类型
            foreach (ExpeditionType type in Enum.GetValues(typeof(ExpeditionType)))
            {
                var config = PetExpeditionDatabase.Expeditions[type];
                string displayText = $"{config.Name}\nLvl {config.MinLevel} | {config.DurationMinutes}min | {config.SuccessRate * 100}%";
                _expeditionTypeList.AddItem(displayText);
            }
            
            _selectedExpeditionLabel = new Label();
            _selectedExpeditionLabel.Text = "Select an expedition type";
            leftPanel.AddChild(_selectedExpeditionLabel);
            
            // 中间 - 宠物选择
            var centerPanel = new VBoxContainer();
            centerPanel.CustomMinimumSize = new Vector2(250, 0);
            mainContainer.AddChild(centerPanel);
            
            var petTitle = new Label();
            petTitle.Text = "Select Pet";
            petTitle.Align = Label.AlignEnum.Center;
            centerPanel.AddChild(petTitle);
            
            _petList = new ItemList();
            _petList.CustomMinimumSize = new Vector2(0, 300);
            _petList.Connect("item_selected", this, nameof(OnPetSelected));
            centerPanel.AddChild(_petList);
            
            // 添加宠物（示例宠物）
            _petList.AddItem("Wolf (Pet001)");
            _petList.AddItem("Bear (Pet002)");
            _petList.AddItem("Fox (Pet003)");
            _petList.AddItem("Eagle (Pet004)");
            _petList.AddItem("Tiger (Pet005)");
            
            _selectedPetLabel = new Label();
            _selectedPetLabel.Text = "Select a pet";
            centerPanel.AddChild(_selectedPetLabel);
            
            _startButton = new Button();
            _startButton.Text = "Start Expedition";
            _startButton.Disabled = true;
            _startButton.Connect("pressed", this, nameof(OnStartPressed));
            centerPanel.AddChild(_startButton);
            
            // 右侧 - Tab容器
            _tabContainer = new TabContainer();
            _tabContainer.SetHExpandFlags(Control.ExpandLayout.Fill);
            mainContainer.AddChild(_tabContainer);
            
            // 活跃探险标签页
            var activeTab = new VBoxContainer();
            _tabContainer.AddChild(activeTab);
            _tabContainer.SetTabTitle(0, "Active Expeditions");
            
            _activeExpeditionsList = new VBoxContainer();
            activeTab.AddChild(_activeExpeditionsList);
            
            // 历史记录标签页
            var historyTab = new VBoxContainer();
            _tabContainer.AddChild(historyTab);
            _tabContainer.SetTabTitle(1, "History");
            
            _historyList = new VBoxContainer();
            historyTab.AddChild(_historyList);
            
            // 统计标签页
            var statsTab = new VBoxContainer();
            _tabContainer.AddChild(statsTab);
            _tabContainer.SetTabTitle(2, "Statistics");
            
            _statsPanel = new VBoxContainer();
            statsTab.AddChild(_statsPanel);
            
            SetupStatsPanel();
            
            // 关闭按钮
            var closeButton = new Button();
            closeButton.Text = "Close (ESC)";
            closeButton.Connect("pressed", this, nameof(OnClosePressed));
            closeButton.SetAnchorsAndMarginsPreset(Control.Preset.BottomRight);
            closeButton.MarginRight = -50;
            closeButton.MarginBottom = -50;
            AddChild(closeButton);
            
            // 更新显示
            UpdateActiveExpeditions();
            UpdateHistory();
        }
        
        private void SetupStatsPanel()
        {
            var stats = _expeditionSystem.GetPlayerStats();
            
            AddStatRow("Total Expeditions", stats.TotalExpeditions.ToString());
            AddStatRow("Successful", stats.SuccessfulExpeditions.ToString());
            AddStatRow("Failed", stats.FailedExpeditions.ToString());
            AddStatRow("Gold Earned", stats.GoldEarned.ToString());
            AddStatRow("Experience Gained", stats.ExperienceGained.ToString());
            
            string[] rarityNames = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
            string highestRarity = stats.HighestRarityFound > 0 ? rarityNames[stats.HighestRarityFound - 1] : "None";
            AddStatRow("Highest Rarity Found", highestRarity);
        }
        
        private void AddStatRow(string label, string value)
        {
            var row = new HBoxContainer();
            _statsPanel.AddChild(row);
            
            var labelNode = new Label();
            labelNode.Text = label + ": ";
            labelNode.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            row.AddChild(labelNode);
            
            var valueNode = new Label();
            valueNode.Text = value;
            valueNode.Align = Label.AlignEnum.Right;
            row.AddChild(valueNode);
        }
        
        private void SetupSignals()
        {
            PetExpeditionSystem.OnExpeditionStarted.Connect(OnExpeditionStarted);
            PetExpeditionSystem.OnExpeditionCompleted.Connect(OnExpeditionCompleted);
            PetExpeditionSystem.OnExpeditionFailed.Connect(OnExpeditionFailed);
        }
        
        private int _selectedExpeditionIndex = -1;
        private int _selectedPetIndex = -1;
        
        private void OnExpeditionTypeSelected(int index)
        {
            _selectedExpeditionIndex = index;
            
            var type = (ExpeditionType)index;
            var config = PetExpeditionDatabase.Expeditions[type];
            
            string rarityInfo = "";
            for (int i = 0; i < config.RarityWeights.Length; i++)
            {
                rarityInfo += $"{PetExpeditionDatabase.RarityNamesCN[i]}: {config.RarityWeights[i] * 100}%\n";
            }
            
            _selectedExpeditionLabel.Text = $"Selected: {config.Name}\n{config.Description}\nGold: {config.GoldReward[0]}-{config.GoldReward[1]}\nExp: {config.ExpReward[0]}-{config.ExpReward[1]}\n\nRarity:\n{rarityInfo}";
            
            UpdateStartButton();
        }
        
        private void OnPetSelected(int index)
        {
            _selectedPetIndex = index;
            _selectedPetLabel.Text = $"Selected: { _petList.GetItemText(index)}";
            UpdateStartButton();
        }
        
        private void UpdateStartButton()
        {
            _startButton.Disabled = _selectedExpeditionIndex < 0 || _selectedPetIndex < 0;
        }
        
        private void OnStartPressed()
        {
            if (_selectedExpeditionIndex < 0 || _selectedPetIndex < 0) return;
            
            var type = (ExpeditionType)_selectedExpeditionIndex;
            string petId = $"Pet{_selectedPetIndex + 1:D3}";
            string petName = _petList.GetItemText(_selectedPetIndex);
            
            bool success = _expeditionSystem.StartExpedition(petId, petName, type);
            
            if (success)
            {
                GD.Print($"[PetExpeditionUI] Started expedition for {petName}");
                UpdateActiveExpeditions();
            }
            else
            {
                GD.Print($"[PetExpeditionUI] Failed to start expedition");
            }
        }
        
        private void UpdateActiveExpeditions()
        {
            foreach (var child in _activeExpeditionsList.GetChildren())
            {
                child.QueueFree();
            }
            
            var activeExpeditions = _expeditionSystem.GetActiveExpeditions();
            
            if (activeExpeditions.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "No active expeditions";
                emptyLabel.Align = Label.AlignEnum.Center;
                _activeExpeditionsList.AddChild(emptyLabel);
                return;
            }
            
            foreach (var expedition in activeExpeditions)
            {
                var config = PetExpeditionDatabase.Expeditions[expedition.Type];
                var remaining = _expeditionSystem.GetRemainingMinutes(expedition.PetId);
                var progress = _expeditionSystem.GetProgress(expedition.PetId);
                
                var card = new VBoxContainer();
                card.AddThemeConstantOverride("separation", 5);
                
                var nameLabel = new Label();
                nameLabel.Text = $"Pet: {expedition.PetName}";
                nameLabel.Bold = true;
                card.AddChild(nameLabel);
                
                var typeLabel = new Label();
                typeLabel.Text = $"Expedition: {config.Name}";
                card.AddChild(typeLabel);
                
                var timeLabel = new Label();
                timeLabel.Text = $"Remaining: {remaining} min ({progress * 100:F1}%)";
                card.AddChild(timeLabel);
                
                // 进度条
                var progressBar = new ProgressBar();
                progressBar.Value = progress * 100;
                progressBar.CustomMinimumSize = new Vector2(0, 20);
                card.AddChild(progressBar);
                
                _activeExpeditionsList.AddChild(card);
            }
        }
        
        private void UpdateHistory()
        {
            foreach (var child in _historyList.GetChildren())
            {
                child.QueueFree();
            }
            
            var history = _expeditionSystem.GetExpeditionHistory();
            
            if (history.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "No expedition history";
                emptyLabel.Align = Label.AlignEnum.Center;
                _historyList.AddChild(emptyLabel);
                return;
            }
            
            foreach (var expedition in history.Take(20))
            {
                var config = PetExpeditionDatabase.Expeditions[expedition.Type];
                
                var card = new VBoxContainer();
                
                var nameLabel = new Label();
                nameLabel.Text = $"Pet: {expedition.PetName} - {config.Name}";
                nameLabel.Bold = true;
                card.AddChild(nameLabel);
                
                var resultLabel = new Label();
                if (expedition.Success)
                {
                    resultLabel.Text = $"✓ Success! Gold: {expedition.GoldReward}, Item: {expedition.ItemReward}";
                    resultLabel.Modulate = new Color(0, 1, 0);
                }
                else
                {
                    resultLabel.Text = "✗ Failed";
                    resultLabel.Modulate = new Color(1, 0, 0);
                }
                card.AddChild(resultLabel);
                
                _historyList.AddChild(card);
            }
        }
        
        private void OnRefreshTimer()
        {
            UpdateActiveExpeditions();
        }
        
        private void OnExpeditionStarted()
        {
            UpdateActiveExpeditions();
        }
        
        private void OnExpeditionCompleted()
        {
            UpdateActiveExpeditions();
            UpdateHistory();
            SetupStatsPanel();
        }
        
        private void OnExpeditionFailed()
        {
            UpdateActiveExpeditions();
            UpdateHistory();
        }
        
        private void OnClosePressed()
        {
            QueueFree();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
            {
                QueueFree();
            }
        }
        
        public override void _ExitTree()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
            }
        }
    }
}
