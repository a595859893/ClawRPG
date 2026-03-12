using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSystems
{
    public class PetHabitatUI : Control
    {
        private static PetHabitatUI _instance;
        public static PetHabitatUI Instance => _instance;
        
        // UI 组件
        private PanelContainer _mainPanel;
        private VBoxContainer _contentBox;
        private TabContainer _tabContainer;
        
        // 栖息地标签页
        private GridContainer _habitatGrid;
        private Label _habitatNameLabel;
        private Label _habitatDescriptionLabel;
        private Label _comfortLabel;
        private Label _attractionLabel;
        private ProgressBar _comfortBar;
        private ProgressBar _attractionBar;
        
        // 装饰品标签页
        private GridContainer _decorationGrid;
        private ScrollContainer _decorationScroll;
        
        // 统计标签页
        private VBoxContainer _statsBox;
        
        // 当前选中
        private string _selectedDecoration = null;
        private int _selectedSlot = -1;
        
        // 数据
        private PlayerHabitatData _playerData;
        private List<HabitatConfig> _habitats;
        private List<DecorationConfig> _decorations;
        
        public override void _Ready()
        {
            _instance = this;
            _playerData = PetHabitatSystem.Instance.PlayerData;
            _habitats = PetHabitatDatabase.GetAllHabitats();
            _decorations = PetHabitatDatabase.GetAllDecorations();
            
            SetupUI();
            ConnectSignals();
            
            Hide();
            GD.Print("Pet Habitat UI initialized");
        }
        
        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(900, 650);
            AddChild(_mainPanel);
            
            // 标题栏
            var titleBar = new HBoxContainer();
            titleBar.SetHorizontalExpandFillOverride(_mainPanel);
            titleBar.GCustomMinimumSize = new Vector2(0, 50);
            
            var titleLabel = new Label();
            titleLabel.Text = "  🏠 宠物栖息地";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(titleLabel);
            
            var spacer = new Control();
            spacer.SetHorizontalExpandFillOverride(titleBar);
            titleBar.AddChild(spacer);
            
            var closeButton = new Button();
            closeButton.Text = "✕";
            closeButton.CustomMinimumSize = new Vector2(40, 40);
            closeButton.Pressed += () => Hide();
            titleBar.AddChild(closeButton);
            
            // 内容区域
            _contentBox = new VBoxContainer();
            _contentBox.SetHorizontalExpandFillOverride(_mainPanel);
            _contentBox.AddChild(titleBar);
            _mainPanel.AddChild(_contentBox);
            
            // 标签页容器
            _tabContainer = new TabContainer();
            _tabContainer.SetVerticalExpandFillOverride(_contentBox);
            _contentBox.AddChild(_tabContainer);
            
            // 栖息地标签页
            SetupHabitatTab();
            
            // 装饰品标签页
            SetupDecorationTab();
            
            // 统计标签页
            SetupStatsTab();
            
            // 更新显示
            RefreshDisplay();
        }
        
        private void SetupHabitatTab()
        {
            var habitatBox = new VBoxContainer();
            habitatBox.Name = "栖息地";
            _tabContainer.AddChild(habitatBox);
            
            // 当前栖息地信息
            var currentHabitatBox = new VBoxContainer();
            currentHabitatBox.CustomMinimumSize = new Vector2(0, 200);
            habitatBox.AddChild(currentHabitatBox);
            
            var currentTitle = new Label();
            currentTitle.Text = "当前栖息地";
            currentTitle.AddThemeFontSizeOverride("font_size", 18);
            currentHabitatBox.AddChild(currentTitle);
            
            _habitatNameLabel = new Label();
            _habitatNameLabel.Text = "草原栖息地";
            _habitatNameLabel.AddThemeFontSizeOverride("font_size", 20);
            currentHabitatBox.AddChild(_habitatNameLabel);
            
            _habitatDescriptionLabel = new Label();
            _habitatDescriptionLabel.Text = "";
            _habitatDescriptionLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            currentHabitatBox.AddChild(_habitatDescriptionLabel);
            
            // 舒适度
            var comfortBox = new HBoxContainer();
            currentHabitatBox.AddChild(comfortBox);
            
            var comfortTitle = new Label();
            comfortTitle.Text = "舒适度: ";
            comfortTitle.CustomMinimumSize = new Vector2(100, 0);
            comfortBox.AddChild(comfortTitle);
            
            _comfortBar = new ProgressBar();
            _comfortBar.CustomMinimumSize = new Vector2(300, 20);
            _comfortBar.MaxValue = 100;
            comfortBox.AddChild(_comfortBar);
            
            _comfortLabel = new Label();
            _comfortLabel.Text = "0";
            comfortBox.AddChild(_comfortLabel);
            
            // 吸引力
            var attractionBox = new HBoxContainer();
            currentHabitatBox.AddChild(attractionBox);
            
            var attractionTitle = new Label();
            attractionTitle.Text = "吸引力: ";
            attractionTitle.CustomMinimumSize = new Vector2(100, 0);
            attractionBox.AddChild(attractionTitle);
            
            _attractionBar = new ProgressBar();
            _attractionBar.CustomMinimumSize = new Vector2(300, 20);
            _attractionBar.MaxValue = 100;
            attractionBox.AddChild(_attractionBar);
            
            _attractionLabel = new Label();
            _attractionLabel.Text = "0";
            attractionBox.AddChild(_attractionLabel);
            
            // 访问按钮
            var visitButton = new Button();
            visitButton.Text = "访问栖息地 (获得奖励)";
            visitButton.CustomMinimumSize = new Vector2(200, 40);
            visitButton.Pressed += OnVisitHabitat;
            currentHabitatBox.AddChild(visitButton);
            
            // 栖息地选择
            var selectTitle = new Label();
            selectTitle.Text = "选择栖息地";
            selectTitle.AddThemeFontSizeOverride("font_size", 18);
            habitatBox.AddChild(selectTitle);
            
            _habitatGrid = new GridContainer();
            _habitatGrid.Columns = 4;
            _habitatGrid.SetVerticalExpandFillOverride(habitatBox);
            habitatBox.AddChild(_habitatGrid);
            
            // 添加栖息地按钮
            foreach (var habitat in _habitats)
            {
                var habitatButton = CreateHabitatButton(habitat);
                _habitatGrid.AddChild(habitatButton);
            }
        }
        
        private Button CreateHabitatButton(HabitatConfig habitat)
        {
            var button = new Button();
            button.CustomMinimumSize = new Vector2(180, 80);
            
            var vbox = new VBoxContainer();
            button.AddChild(vbox);
            
            var nameLabel = new Label();
            nameLabel.Text = habitat.Name;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(nameLabel);
            
            var slotsLabel = new Label();
            slotsLabel.Text = $"槽位: {habitat.MaxSlots}";
            slotsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            slotsLabel.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(slotsLabel);
            
            var bonusLabel = new Label();
            bonusLabel.Text = $"+{habitat.ComfortBonus} 舒适";
            bonusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            bonusLabel.AddThemeFontSizeOverride("font_size", 11);
            bonusLabel.Modulate = new Color(0.5f, 0.8f, 0.5f);
            vbox.AddChild(bonusLabel);
            
            if (habitat.UnlockCost > 0)
            {
                var costLabel = new Label();
                costLabel.Text = $"💰 {habitat.UnlockCost}";
                costLabel.HorizontalAlignment = HorizontalAlignment.Center;
                costLabel.AddThemeFontSizeOverride("font_size", 11);
                costLabel.Modulate = new Color(1f, 0.8f, 0.3f);
                vbox.AddChild(costLabel);
            }
            
            button.Pressed += () => OnSelectHabitat(habitat.Id);
            
            return button;
        }
        
        private void SetupDecorationTab()
        {
            var decorationBox = new VBoxContainer();
            decorationBox.Name = "装饰品";
            _tabContainer.AddChild(decorationBox);
            
            // 说明
            var hintLabel = new Label();
            hintLabel.Text = "点击装饰品，然后点击栖息地中的槽位进行放置";
            hintLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            decorationBox.AddChild(hintLabel);
            
            // 装饰品网格
            _decorationScroll = new ScrollContainer();
            _decorationScroll.SetVerticalExpandFillOverride(decorationBox);
            decorationBox.AddChild(_decorationScroll);
            
            _decorationGrid = new GridContainer();
            _decorationGrid.Columns = 5;
            _decorationScroll.AddChild(_decorationGrid);
            
            // 添加装饰品按钮
            foreach (var decoration in _decorations)
            {
                var decorationButton = CreateDecorationButton(decoration);
                _decorationGrid.AddChild(decorationButton);
            }
        }
        
        private Button CreateDecorationButton(DecorationConfig decoration)
        {
            var button = new Button();
            button.CustomMinimumSize = new Vector2(140, 100);
            
            var vbox = new VBoxContainer();
            button.AddChild(vbox);
            
            var iconLabel = new Label();
            iconLabel.Text = decoration.Icon;
            iconLabel.HorizontalAlignment = HorizontalAlignment.Center;
            iconLabel.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(iconLabel);
            
            var nameLabel = new Label();
            nameLabel.Text = decoration.Name;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(nameLabel);
            
            var statsLabel = new Label();
            statsLabel.Text = $"+{decoration.ComfortBonus} 舒适  +{decoration.AttractionBonus} 吸引";
            statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            statsLabel.AddThemeFontSizeOverride("font_size", 10);
            statsLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            vbox.AddChild(statsLabel);
            
            var costLabel = new Label();
            costLabel.Text = $"💰 {decoration.Cost}";
            costLabel.HorizontalAlignment = HorizontalAlignment.Center;
            costLabel.AddThemeFontSizeOverride("font_size", 11);
            costLabel.Modulate = new Color(1f, 0.8f, 0.3f);
            vbox.AddChild(costLabel);
            
            button.Pressed += () => OnSelectDecoration(decoration.Id);
            
            return button;
        }
        
        private void SetupStatsTab()
        {
            var statsBox = new VBoxContainer();
            statsBox.Name = "统计";
            _tabContainer.AddChild(statsBox);
            
            var titleLabel = new Label();
            titleLabel.Text = "栖息地统计";
            titleLabel.AddThemeFontSizeOverride("font_size", 20);
            statsBox.AddChild(titleLabel);
            
            var separator = new HSeparator();
            statsBox.AddChild(separator);
            
            _statsBox = new VBoxContainer();
            _statsBox.SetVerticalExpandFillOverride(statsBox);
            statsBox.AddChild(_statsBox);
            
            RefreshStats();
        }
        
        private void ConnectSignals()
        {
            if (PetHabitatSystem.Instance != null)
            {
                PetHabitatSystem.Instance.OnComfortChanged += (value) => RefreshDisplay();
                PetHabitatSystem.Instance.OnAttractionChanged += (value) => RefreshDisplay();
                PetHabitatSystem.Instance.OnHabitatChanged += (value) => RefreshDisplay();
                PetHabitatSystem.Instance.OnDecorationPlaced += (id, slot) => RefreshDisplay();
                PetHabitatSystem.Instance.OnDecorationRemoved += (id, slot) => RefreshDisplay();
            }
        }
        
        private void RefreshDisplay()
        {
            // 刷新栖息地信息
            var habitat = PetHabitatSystem.Instance.GetCurrentHabitat();
            if (habitat != null)
            {
                _habitatNameLabel.Text = habitat.Name;
                _habitatDescriptionLabel.Text = habitat.Description;
            }
            
            // 刷新舒适度和吸引力
            int comfort = PetHabitatSystem.Instance.PlayerData.TotalComfort;
            int attraction = PetHabitatSystem.Instance.PlayerData.TotalAttraction;
            
            _comfortLabel.Text = comfort.ToString();
            _attractionLabel.Text = attraction.ToString();
            
            _comfortBar.Value = PetHabitatSystem.Instance.GetComfortPercentage() * 100;
            _attractionBar.Value = PetHabitatSystem.Instance.GetAttractionPercentage() * 100;
            
            // 刷新统计
            RefreshStats();
        }
        
        private void RefreshStats()
        {
            if (_statsBox == null) return;
            
            // 清除现有内容
            foreach (var child in _statsBox.GetChildren())
            {
                child.QueueFree();
            }
            
            var data = PetHabitatSystem.Instance.PlayerData;
            
            AddStatLine("🏠 当前栖息地", data.CurrentHabitatId);
            AddStatLine("📊 装饰品数量", data.PlacedDecorations.Count.ToString());
            AddStatLine("✨ 舒适度", data.TotalComfort.ToString());
            AddStatLine("⭐ 吸引力", data.TotalAttraction.ToString());
            AddStatLine("🛒 购买装饰品", data.DecorationsPurchased.ToString());
            AddStatLine("💰 总花费", data.GoldSpentOnDecorations.ToString());
            AddStatLine("🚪 访问次数", data.HabitatVisits.ToString());
            AddStatLine("🐾 吸引宠物", data.PetsAttracted.ToString());
        }
        
        private void AddStatLine(string label, string value)
        {
            var line = new HBoxContainer();
            _statsBox.AddChild(line);
            
            var labelText = new Label();
            labelText.Text = label + ": ";
            labelText.CustomMinimumSize = new Vector2(150, 0);
            line.AddChild(labelText);
            
            var valueText = new Label();
            valueText.Text = value;
            valueText.Modulate = new Color(0.8f, 0.9f, 1f);
            line.AddChild(valueText);
        }
        
        private void OnSelectHabitat(string habitatId)
        {
            var player = GetNode<Player>("/root/Main/Player");
            var habitat = PetHabitatDatabase.GetHabitat(habitatId);
            
            if (habitat == null) return;
            
            if (habitat.UnlockCost > 0 && PetHabitatSystem.Instance.PlayerData.CurrentHabitatId != habitatId)
            {
                // 需要花费金币切换
                if (player.Gold >= habitat.UnlockCost)
                {
                    PetHabitatSystem.Instance.ChangeHabitat(habitatId);
                    GD.Print($"Changed to habitat: {habitatId}");
                }
                else
                {
                    GD.Print($"Not enough gold to change habitat: {habitat.UnlockCost} required");
                }
            }
            else
            {
                PetHabitatSystem.Instance.ChangeHabitat(habitatId);
                GD.Print($"Changed to habitat: {habitatId}");
            }
        }
        
        private void OnSelectDecoration(string decorationId)
        {
            _selectedDecoration = decorationId;
            var decoration = PetHabitatDatabase.GetDecoration(decorationId);
            
            if (decoration != null)
            {
                GD.Print($"Selected decoration: {decoration.Name}, cost: {decoration.Cost}");
            }
        }
        
        private void OnVisitHabitat()
        {
            var result = PetHabitatSystem.Instance.VisitHabitat();
            
            if (result.Success)
            {
                GD.Print($"Visited habitat: +{result.ComfortGained} comfort, +{result.AttractionGained} attraction, +{result.GoldEarned} gold");
                
                // 显示访问结果
                var message = $"访问成功!\n舒适度 +{result.ComfortGained}\n吸引力 +{result.AttractionGained}\n金币 +{result.GoldEarned}";
                
                if (result.AttractedPets.Count > 0)
                {
                    message += $"\n吸引宠物: {string.Join(", ", result.AttractedPets)}";
                }
                
                // 这里可以显示一个简单的消息
                RefreshDisplay();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                Hide();
            }
        }
    }
}
