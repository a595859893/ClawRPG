using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace ClawRPG.Scripts.Systems.ContractBounty
{
    /// <summary>
    /// Contract Bounty UI - 委托赏金系统界面
    /// </summary>
    
    public partial class ContractBountyUI : Control
    {
        private ContractBountySystem _system = ContractBountySystem.Instance;
        
        // UI Components
        private TabContainer _tabContainer;
        private VBoxContainer _availableTab;
        private VBoxContainer _activeTab;
        private VBoxContainer _completedTab;
        private VBoxContainer _statsTab;
        
        // Contract item scene
        private PackedScene _contractItemScene;
        
        // Current filter
        private ContractDifficulty? _currentDifficultyFilter;
        private ContractType? _currentTypeFilter;
        
        public override void _Ready()
        {
            _system.OnContractAccepted += OnContractAccepted;
            _system.OnContractCompleted += OnContractCompleted;
            _system.OnContractFailed += OnContractFailed;
            
            SetupUI();
            RefreshAllTabs();
        }
        
        private void SetupUI()
        {
            // Main container
            var mainContainer = new HBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(mainContainer);
            
            // Left panel - Contract list
            var leftPanel = new VBoxContainer();
            leftPanel.SetMeta("id", "left_panel");
            mainContainer.AddChild(leftPanel);
            
            // Filter buttons
            var filterContainer = new HBoxContainer();
            leftPanel.AddChild(filterContainer);
            
            var filterLabel = new Label();
            filterLabel.Text = "筛选: ";
            filterContainer.AddChild(filterLabel);
            
            var difficultyBtn = new OptionButton();
            difficultyBtn.AddItem("全部难度", 0);
            difficultyBtn.AddItem("简单", (int)ContractDifficulty.Easy + 1);
            difficultyBtn.AddItem("普通", (int)ContractDifficulty.Medium + 1);
            difficultyBtn.AddItem("困难", (int)ContractDifficulty.Hard + 1);
            difficultyBtn.AddItem("传说", (int)ContractDifficulty.Legendary + 1);
            difficultyBtn.ItemSelected += index => _on_difficulty_selected(index);
            filterContainer.AddChild(difficultyBtn);
            
            var typeBtn = new OptionButton();
            typeBtn.AddItem("全部类型", 0);
            typeBtn.AddItem("怪物狩猎", (int)ContractType.MonsterHunt + 1);
            typeBtn.AddItem("暗杀", (int)ContractType.Assassination + 1);
            typeBtn.AddItem("救援", (int)ContractType.Rescue + 1);
            typeBtn.AddItem("护送", (int)ContractType.Escort + 1);
            typeBtn.AddItem("收集", (int)ContractType.Collection + 1);
            typeBtn.AddItem("防御", (int)ContractType.Defense + 1);
            typeBtn.ItemSelected += index => _on_type_selected(index);
            filterContainer.AddChild(typeBtn);
            
            // Refresh button
            var refreshBtn = new Button();
            refreshBtn.Text = "刷新合同";
            refreshBtn.Pressed += _on_refresh_pressed;
            leftPanel.AddChild(refreshBtn);
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlagsVertical.Fill);
            leftPanel.AddChild(_tabContainer);
            
            // Available tab
            _availableTab = new VBoxContainer();
            _availableTab.Name = "可用合同";
            _tabContainer.AddChild(_availableTab);
            
            // Active tab
            _activeTab = new VBoxContainer();
            _activeTab.Name = "进行中";
            _tabContainer.AddChild(_activeTab);
            
            // Completed tab
            _completedTab = new VBoxContainer();
            _completedTab.Name = "已完成";
            _tabContainer.AddChild(_completedTab);
            
            // Stats tab
            _statsTab = new VBoxContainer();
            _statsTab.Name = "统计";
            _tabContainer.AddChild(_statsTab);
            
            SetupStatsTab();
            
            // Right panel - Contract details
            var rightPanel = new VBoxContainer();
            rightPanel.SetMeta("id", "right_panel");
            mainContainer.AddChild(rightPanel);
            
            var detailsLabel = new Label();
            detailsLabel.Text = "合同详情";
            detailsLabel.Align = Label.AlignEnum.Center;
            rightPanel.AddChild(detailsLabel);
            
            var detailsContainer = new ScrollContainer();
            detailsContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlagsVertical.Fill);
            rightPanel.AddChild(detailsContainer);
            
            var detailsContent = new VBoxContainer();
            detailsContent.SetMeta("id", "details_content");
            detailsContainer.AddChild(detailsContent);
            
            // Input handling
            SetProcessInput(true);
        }
        
        private void SetupStatsTab()
        {
            var stats = _system.Data;
            
            // Summary
            var summaryLabel = new Label();
            summaryLabel.Text = "=== 统计摘要 ===";
            _statsTab.AddChild(summaryLabel);
            
            var statsContainer = new VBoxContainer();
            _statsTab.AddChild(statsContainer);
            
            AddStatRow(statsContainer, "已完成合同", stats.totalCompleted.ToString());
            AddStatRow(statsContainer, "失败合同", stats.totalFailed.ToString());
            AddStatRow(statsContainer, "获得金币", stats.totalGoldEarned.ToString());
            AddStatRow(statsContainer, "获得经验", stats.totalExpEarned.ToString());
            AddStatRow(statsContainer, "当前连胜", stats.currentStreak.ToString());
            AddStatRow(statsContainer, "最高连胜", stats.bestStreak.ToString());
            AddStatRow(statsContainer, "已发现合同类型", stats.discoveredContracts.Count.ToString());
            
            // Completion rate
            var total = stats.totalCompleted + stats.totalFailed;
            var rate = total > 0 ? (float)stats.totalCompleted / total * 100 : 0;
            AddStatRow(statsContainer, "完成率", $"{rate:F1}%");
        }
        
        private void AddStatRow(Control parent, string label, string value)
        {
            var row = new HBoxContainer();
            parent.AddChild(row);
            
            var labelNode = new Label();
            labelNode.Text = label + ": ";
            labelNode.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            row.AddChild(labelNode);
            
            var valueNode = new Label();
            valueNode.Text = value;
            valueNode.Align = Label.AlignEnum.Right;
            row.AddChild(valueNode);
        }
        
        public void RefreshAllTabs()
        {
            RefreshAvailableTab();
            RefreshActiveTab();
            RefreshCompletedTab();
            SetupStatsTab();
        }
        
        private void RefreshAvailableTab()
        {
            ClearContainer(_availableTab);
            
            var contracts = _system.Data.availableContracts;
            
            if (_currentDifficultyFilter.HasValue)
                contracts = contracts.FindAll(c => c.difficulty == _currentDifficultyFilter.Value);
            
            if (_currentTypeFilter.HasValue)
                contracts = contracts.FindAll(c => c.type == _currentTypeFilter.Value);
            
            foreach (var contract in contracts)
            {
                var item = CreateContractItem(contract);
                _availableTab.AddChild(item);
            }
            
            if (contracts.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "没有可用的合同";
                emptyLabel.Align = Label.AlignEnum.Center;
                _availableTab.AddChild(emptyLabel);
            }
        }
        
        private void RefreshActiveTab()
        {
            ClearContainer(_activeTab);
            
            var contracts = _system.Data.activeContracts;
            
            foreach (var contract in contracts)
            {
                var item = CreateContractItem(contract);
                _activeTab.AddChild(item);
            }
            
            if (contracts.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "没有进行中的合同";
                emptyLabel.Align = Label.AlignEnum.Center;
                _activeTab.AddChild(emptyLabel);
            }
        }
        
        private void RefreshCompletedTab()
        {
            ClearContainer(_completedTab);
            
            var contracts = _system.Data.completedContracts;
            
            foreach (var contract in contracts)
            {
                var item = CreateContractItem(contract);
                _completedTab.AddChild(item);
            }
            
            if (contracts.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "没有已完成的合同";
                emptyLabel.Align = Label.AlignEnum.Center;
                _completedTab.AddChild(emptyLabel);
            }
        }
        
        private Control CreateContractItem(Contract contract)
        {
            var container = new VBoxContainer();
            container.SetMeta("contract_id", contract.contractId);
            
            // Header
            var header = new HBoxContainer();
            container.AddChild(header);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = contract.title;
            titleLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            header.AddChild(titleLabel);
            
            // Difficulty badge
            var difficultyLabel = new Label();
            difficultyLabel.Text = $"[{_system.GetDifficultyName(contract.difficulty)}]";
            difficultyLabel.Modulate = ColorFromHex(_system.GetDifficultyColor(contract.difficulty));
            header.AddChild(difficultyLabel);
            
            // Type
            var typeLabel = new Label();
            typeLabel.Text = _system.GetTypeName(contract.type);
            container.AddChild(typeLabel);
            
            // Progress (for active contracts)
            if (contract.status == ContractStatus.Active)
            {
                var progressLabel = new Label();
                var progress = _system.GetProgress(contract.contractId);
                progressLabel.Text = $"进度: {contract.target.currentKills}/{contract.target.requiredKills} ({progress:P0})";
                container.AddChild(progressLabel);
            }
            
            // Reward
            var rewardLabel = new Label();
            rewardLabel.Text = $"奖励: {contract.reward.gold}金币 {contract.reward.experience}经验";
            container.AddChild(rewardLabel);
            
            // Status
            var statusLabel = new Label();
            statusLabel.Text = $"状态: {_system.GetContractStatusText(contract)}";
            container.AddChild(statusLabel);
            
            // Buttons
            var buttonContainer = new HBoxContainer();
            container.AddChild(buttonContainer);
            
            if (contract.status == ContractStatus.Available)
            {
                var acceptBtn = new Button();
                acceptBtn.Text = "接受";
                acceptBtn.Pressed += () => _on_accept_pressed(contract.contractId);
                buttonContainer.AddChild(acceptBtn);
            }
            else if (contract.status == ContractStatus.Active)
            {
                var detailsBtn = new Button();
                detailsBtn.Text = "详情";
                detailsBtn.Pressed += () => _on_details_pressed(contract.contractId);
                buttonContainer.AddChild(detailsBtn);
                
                var abandonBtn = new Button();
                abandonBtn.Text = "放弃";
                abandonBtn.Pressed += () => _on_abandon_pressed(contract.contractId);
                buttonContainer.AddChild(abandonBtn);
            }
            
            // Separator
            var separator = new HSeparator();
            container.AddChild(separator);
            
            return container;
        }
        
        private void ClearContainer(Control container)
        {
            foreach (var child in container.GetChildren())
            {
                child.QueueFree();
            }
        }
        
        private Color ColorFromHex(string hex)
        {
            var color = new Color(hex);
            return color;
        }
        
        private void ShowContractDetails(Contract contract)
        {
            var detailsContent = GetNodeOrNull<VBoxContainer>("/root/Main/HBoxContainer/RightPanel/DetailsContent");
            if (detailsContent == null) return;
            
            ClearContainer(detailsContent);
            
            // Title
            var title = new Label();
            title.Text = contract.title;
            title.Align = Label.AlignEnum.Center;
            title.FontSize = 24;
            detailsContent.AddChild(title);
            
            // Client
            var client = new Label();
            client.Text = $"委托者: {contract.clientName}";
            detailsContent.AddChild(client);
            
            // Description
            var descLabel = new Label();
            descLabel.Text = contract.description;
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            detailsContent.AddChild(descLabel);
            
            // Target info
            var targetLabel = new Label();
            targetLabel.Text = $"\n目标: {contract.target.targetName}\n" +
                              $"描述: {contract.target.targetDescription}\n" +
                              $"等级: {contract.target.level}\n" +
                              $"需要击杀: {contract.target.requiredKills}";
            detailsContent.AddChild(targetLabel);
            
            // Location
            var locationLabel = new Label();
            locationLabel.Text = $"\n位置: {contract.location}";
            detailsContent.AddChild(locationLabel);
            
            // Tips
            var tipsLabel = new Label();
            tipsLabel.Text = $"\n提示: {contract.tips}";
            tipsLabel.Modulate = new Color(1, 1, 0); // Yellow
            detailsContent.AddChild(tipsLabel);
            
            // Rewards
            var rewardLabel = new Label();
            rewardLabel.Text = $"\n=== 奖励 ===\n" +
                              $"金币: {contract.reward.gold}\n" +
                              $"经验: {contract.reward.experience}\n" +
                              $"声望: +{contract.reward.reputation}";
            detailsContent.AddChild(rewardLabel);
            
            // Status
            var statusLabel = new Label();
            statusLabel.Text = $"\n状态: {_system.GetContractStatusText(contract)}";
            detailsContent.AddChild(statusLabel);
            
            // Progress bar (if active)
            if (contract.status == ContractStatus.Active)
            {
                var progressBar = new ProgressBar();
                var progress = _system.GetProgress(contract.contractId);
                progressBar.Value = progress * 100;
                progressBar.MaxValue = 100;
                progressBar.CustomMinimumSize = new Vector2(200, 20);
                detailsContent.AddChild(progressBar);
                
                var progressLabel = new Label();
                progressLabel.Text = $"{contract.target.currentKills} / {contract.target.requiredKills}";
                progressLabel.Align = Label.AlignEnum.Center;
                detailsContent.AddChild(progressLabel);
            }
        }
        
        // Signal handlers
        private void _on_difficulty_selected(int index)
        {
            _currentDifficultyFilter = index == 0 ? null : (ContractDifficulty)(index - 1);
            RefreshAvailableTab();
        }
        
        private void _on_type_selected(int index)
        {
            _currentTypeFilter = index == 0 ? null : (ContractType)(index - 1);
            RefreshAvailableTab();
        }
        
        private void _on_refresh_pressed()
        {
            _system.RefreshAvailableContracts();
            RefreshAllTabs();
        }
        
        private void _on_accept_pressed(string contractId)
        {
            if (_system.AcceptContract(contractId))
            {
                RefreshAllTabs();
            }
        }
        
        private void _on_abandon_pressed(string contractId)
        {
            if (_system.AbandonContract(contractId))
            {
                RefreshAllTabs();
            }
        }
        
        private void _on_details_pressed(string contractId)
        {
            var contract = _system.Data.activeContracts.Find(c => c.contractId == contractId);
            if (contract != null)
            {
                ShowContractDetails(contract);
            }
        }
        
        private void OnContractAccepted(Contract contract)
        {
            RefreshAllTabs();
        }
        
        private void OnContractCompleted(Contract contract)
        {
            RefreshAllTabs();
        }
        
        private void OnContractFailed(Contract contract)
        {
            RefreshAllTabs();
        }
        
        public override void _Process(double delta)
        {
            // Update active contract timers
            if (_system.Data.activeContracts.Count > 0)
            {
                RefreshActiveTab();
            }
        }
        
        public override void _Input(InputEvent evt)
        {
            if (evt.IsActionPressed("ui_cancel"))
            {
                Visible = !Visible;
            }
        }
    }
}
