using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物训练界面
    /// </summary>
    public partial class PetTrainingUI : Control
    {
        private static PetTrainingUI _instance;
        public static PetTrainingUI Instance => _instance;

        // UI组件
        private PanelContainer _mainPanel;
        private VBoxContainer _contentBox;
        private TabContainer _tabContainer;
        
        // 训练界面
        private OptionButton _petSelectButton;
        private OptionButton _trainingTypeButton;
        private ItemList _projectList;
        private Label _projectInfoLabel;
        private Button _startTrainingButton;
        
        // 进行中界面
        private ItemList _activeList;
        private Label _progressLabel;
        
        // 历史界面
        private ItemList _historyList;
        
        // 统计界面
        private Label _statsLabel;
        
        // 数据
        private Pet _selectedPet;
        private PetTrainingData.TrainingType _selectedType = PetTrainingData.TrainingType.Attack;
        private List<Pet> _playerPets = new();

        public override void _Ready()
        {
            _instance = this;
            SetupUI();
            LoadPlayerPets();
            RefreshUI();
            
            // 连接到训练系统信号
            PetTrainingSystem.Instance.OnTrainingPointsChanged += _ => RefreshUI();
            PetTrainingSystem.Instance.OnTrainingStarted += _ => RefreshUI();
            PetTrainingSystem.Instance.OnTrainingCompleted += _ => RefreshUI();
            PetTrainingSystem.Instance.OnTrainingClaimed += _ => RefreshUI();
            
            GD.Print("宠物训练界面已初始化 (T键)");
        }

        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer
            {
                AnchorPreset = ControlPreset.Center,
                GrowDirection = GrowDirection.Both,
                CustomMinimumSize = new Vector2(600, 500)
            };
            AddChild(_mainPanel);
            _mainPanel.Hide();

            // 样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", style);

            // 内容容器
            _contentBox = new VBoxContainer { CustomMinimumSize = new Vector2(580, 480) };
            _mainPanel.AddChild(_contentBox);

            // 标题
            var title = new Label
            {
                Text = "🐾 宠物训练系统",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 24)
            };
            _contentBox.AddChild(title);

            // 关闭按钮
            var closeButton = new Button { Text = "✕ 关闭" };
            closeButton.Pressed += () => ToggleUI();
            _contentBox.AddChild(closeButton);

            // 标签页容器
            _tabContainer = new TabContainer { CustomMinimumSize = new Vector2(560, 400) };
            _contentBox.AddChild(_tabContainer);

            // 创建标签页
            CreateTrainingTab();
            CreateActiveTab();
            CreateHistoryTab();
            CreateStatsTab();
        }

        private void CreateTrainingTab()
        {
            var tab = new VBoxContainer();
            tab.SetMeta("name", "训练");
            _tabContainer.AddChild(tab);

            // 宠物选择
            var petLabel = new Label { Text = "选择宠物:" };
            tab.AddChild(petLabel);

            _petSelectButton = new OptionButton();
            _petSelectButton.ItemSelected += OnPetSelected;
            tab.AddChild(_petSelectButton);

            // 训练类型选择
            var typeLabel = new Label { Text = "训练类型:" };
            tab.AddChild(typeLabel);

            _trainingTypeButton = new OptionButton();
            _trainingTypeButton.AddItem("攻击训练", (int)PetTrainingData.TrainingType.Attack);
            _trainingTypeButton.AddItem("防御训练", (int)PetTrainingData.TrainingType.Defense);
            _trainingTypeButton.AddItem("速度训练", (int)PetTrainingData.TrainingType.Speed);
            _trainingTypeButton.AddItem("生命训练", (int)PetTrainingData.TrainingType.Health);
            _trainingTypeButton.AddItem("暴击训练", (int)PetTrainingData.TrainingType.Critical);
            _trainingTypeButton.AddItem("特殊训练", (int)PetTrainingData.TrainingType.Special);
            _trainingTypeButton.ItemSelected += OnTypeSelected;
            tab.AddChild(_trainingTypeButton);

            // 训练项目列表
            var projectLabel = new Label { Text = "训练项目:" };
            tab.AddChild(projectLabel);

            _projectList = new ItemList { CustomMinimumSize = new Vector2(540, 180) };
            _projectList.ItemSelected += OnProjectSelected;
            tab.AddChild(_projectList);

            // 项目信息
            _projectInfoLabel = new Label
            {
                Text = "选择一个训练项目查看详情",
                CustomMinimumSize = new Vector2(540, 60)
            };
            tab.AddChild(_projectInfoLabel);

            // 开始训练按钮
            _startTrainingButton = new Button
            {
                Text = "开始训练",
                CustomMinimumSize = new Vector2(540, 40)
            };
            _startTrainingButton.Pressed += OnStartTraining;
            tab.AddChild(_startTrainingButton);

            // 当前训练点数
            var pointsLabel = new Label { Text = "可用训练点数: 0" };
            pointsLabel.Name = "PointsLabel";
            tab.AddChild(pointsLabel);
        }

        private void CreateActiveTab()
        {
            var tab = new VBoxContainer();
            tab.SetMeta("name", "进行中");
            _tabContainer.AddChild(tab);

            var title = new Label { Text = "进行中的训练:" };
            tab.AddChild(title);

            _activeList = new ItemList { CustomMinimumSize = new Vector2(540, 250) };
            tab.AddChild(_activeList);

            _progressLabel = new Label { Text = "进度: 0%" };
            tab.AddChild(_progressLabel);

            var claimButton = new Button { Text = "领取奖励", CustomMinimumSize = new Vector2(540, 40) };
            claimButton.Pressed += OnClaimReward;
            tab.AddChild(claimButton);
        }

        private void CreateHistoryTab()
        {
            var tab = new VBoxContainer();
            tab.SetMeta("name", "历史");
            _tabContainer.AddChild(tab);

            var title = new Label { Text = "训练历史:" };
            tab.AddChild(title);

            _historyList = new ItemList { CustomMinimumSize = new Vector2(540, 300) };
            tab.AddChild(_historyList);

            var clearButton = new Button { Text = "清空历史", CustomMinimumSize = new Vector2(540, 40) };
            clearButton.Pressed += OnClearHistory;
            tab.AddChild(clearButton);
        }

        private void CreateStatsTab()
        {
            var tab = new VBoxContainer();
            tab.SetMeta("name", "统计");
            _tabContainer.AddChild(tab);

            var title = new Label { Text = "训练统计:" };
            tab.AddChild(title);

            _statsLabel = new Label
            {
                Text = "加载中...",
                CustomMinimumSize = new Vector2(540, 300)
            };
            tab.AddChild(_statsLabel);
        }

        private void LoadPlayerPets()
        {
            _playerPets.Clear();
            _petSelectButton.Clear();

            var petManager = PetManager.Instance;
            if (petManager != null)
            {
                var pets = petManager.GetAllPets();
                foreach (var pet in pets)
                {
                    _playerPets.Add(pet);
                    _petSelectButton.AddItem($"{pet.PetName} (Lv.{pet.Level})", _playerPets.Count - 1);
                }
            }

            if (_playerPets.Count > 0)
            {
                _selectedPet = _playerPets[0];
            }
        }

        private void RefreshUI()
        {
            // 刷新训练点数显示
            var pointsLabel = _contentBox.FindChild("PointsLabel", true, false) as Label;
            if (pointsLabel != null)
            {
                int points = PetTrainingSystem.Instance.GetTrainingPoints();
                pointsLabel.Text = $"可用训练点数: {points}";
            }

            // 刷新训练项目列表
            RefreshProjectList();

            // 刷新进行中列表
            RefreshActiveList();

            // 刷新历史列表
            RefreshHistoryList();

            // 刷新统计
            RefreshStats();
        }

        private void RefreshProjectList()
        {
            _projectList.Clear();
            var projects = PetTrainingDatabase.GetProjectsByType(_selectedType);
            foreach (var project in projects)
            {
                int level = PetTrainingSystem.Instance.GetProjectLevel(project.Id);
                string text = $"{project.Name} Lv.{project.Level}";
                if (level > 0) text += $" (已训练 {level} 次)";
                _projectList.AddItem(text);
            }
        }

        private void RefreshActiveList()
        {
            _activeList.Clear();
            var sessions = PetTrainingSystem.Instance.GetActiveSessions();
            foreach (var session in sessions)
            {
                var project = PetTrainingDatabase.GetProject(session.ProjectId);
                if (project != null)
                {
                    int remaining = PetTrainingSystem.Instance.GetRemainingTime(session.Id);
                    _activeList.AddItem($"{project.Name} - 剩余 {remaining} 秒");
                }
            }
        }

        private void RefreshHistoryList()
        {
            _historyList.Clear();
            var sessions = PetTrainingSystem.Instance.GetCompletedSessions();
            var reversed = sessions.OrderByDescending(s => s.StartTime).Take(50);
            foreach (var session in reversed)
            {
                var project = PetTrainingDatabase.GetProject(session.ProjectId);
                if (project != null)
                {
                    string status = session.Claimed ? "✓ 已领取" : "⏳ 待领取";
                    _historyList.AddItem($"{project.Name} - {session.StartTime:MM/dd HH:mm} - {status}");
                }
            }
        }

        private void RefreshStats()
        {
            var stats = PetTrainingSystem.Instance.GetStatistics();
            _statsLabel.Text = $"📊 训练统计\n\n" +
                $"累计获得训练点数: {stats["totalTrainingPoints"]}\n" +
                $"可用训练点数: {stats["availableTrainingPoints"]}\n" +
                $"总训练次数: {stats["totalTrainingCount"]}\n" +
                $"进行中训练: {stats["activeSessions"]}\n" +
                $"已完成训练: {stats["completedSessions"]}\n" +
                $"花费金币: {stats["goldSpent"]}";
        }

        private void OnPetSelected(long index)
        {
            if (index >= 0 && index < _playerPets.Count)
            {
                _selectedPet = _playerPets[(int)index];
            }
        }

        private void OnTypeSelected(long index)
        {
            _selectedType = (PetTrainingData.TrainingType)index;
            RefreshProjectList();
        }

        private void OnProjectSelected(long index)
        {
            var projects = PetTrainingDatabase.GetProjectsByType(_selectedType);
            if (index >= 0 && index < projects.Count)
            {
                var project = projects[(int)index];
                int level = PetTrainingSystem.Instance.GetProjectLevel(project.Id);
                _projectInfoLabel.Text = $"{project.Name}\n" +
                    $"{project.Description}\n" +
                    $"等级要求: {project.RequiredLevel}\n" +
                    $"金币费用: {project.GoldCost}\n" +
                    $"训练点数: {project.TrainingPoints}\n" +
                    $"持续时间: {project.Duration}秒\n" +
                    $"攻击+{project.AttackBonus} 防御+{project.DefenseBonus}\n" +
                    $"生命+{project.HealthBonus} 速度+{project.SpeedBonus}\n" +
                    $"暴击率+{project.CriticalRateBonus}% 暴击伤害+{project.CriticalDamageBonus}%\n" +
                    $"当前等级: {level}";
            }
        }

        private void OnStartTraining()
        {
            if (_selectedPet == null)
            {
                GD.Print("请选择宠物");
                return;
            }

            int selectedIndex = _projectList.GetSelectedItems()[0];
            var projects = PetTrainingDatabase.GetProjectsByType(_selectedType);
            if (selectedIndex >= 0 && selectedIndex < projects.Count)
            {
                var project = projects[selectedIndex];
                if (PetTrainingSystem.Instance.StartTraining(_selectedPet, project.Id))
                {
                    GD.Print($"开始训练: {project.Name}");
                    RefreshUI();
                }
            }
        }

        private void OnClaimReward()
        {
            var selected = _activeList.GetSelectedItems();
            if (selected.Length > 0)
            {
                // 查找选中的已完成会话
                var sessions = PetTrainingSystem.Instance.GetCompletedSessions();
                var unrevealed = sessions.Where(s => !s.Claimed).ToList();
                if (selected[0] < unrevealed.Count)
                {
                    var session = unrevealed[selected[0]];
                    PetTrainingSystem.Instance.ClaimTrainingReward(session.Id);
                    RefreshUI();
                }
            }
        }

        private void OnClearHistory()
        {
            // 清空历史记录
            GD.Print("清空历史记录");
            RefreshUI();
        }

        public void ToggleUI()
        {
            if (_mainPanel.Visible)
            {
                _mainPanel.Hide();
            }
            else
            {
                LoadPlayerPets();
                RefreshUI();
                _mainPanel.Show();
            }
        }

        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.T)
                {
                    ToggleUI();
                }
            }
        }
    }
}
