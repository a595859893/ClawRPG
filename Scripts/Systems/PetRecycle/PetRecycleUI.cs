using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.PetRecycle;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 宠物回收系统 - UI显示
    /// </summary>
    public class PetRecycleUI : Control
    {
        private PetRecycleSystem _system;
        
        // UI组件
        private TabContainer _tabContainer;
        private Label _titleLabel;
        
        // 回收标签页
        private OptionButton _petTypeOption;
        private OptionButton _rarityOption;
        private SpinBox _levelSpinBox;
        private Button _previewButton;
        private Button _recycleButton;
        private VBoxContainer _previewContainer;
        private Label _previewLabel;
        
        // 历史标签页
        private VBoxContainer _historyContainer;
        
        // 统计标签页
        private Label _statsLabel;
        
        // 宠物类型列表
        private List<string> _petTypes;
        private List<string> _rarities;
        
        public override void _Ready()
        {
            base._Ready();
            
            // 设置UI
            SetanchorsPreset(Control.LayoutPreset.Center);
            CustomMinimumSize = new Vector2(800, 600);
            
            CreateUI();
            
            GD.Print("[PetRecycleUI] Initialized");
        }
        
        public void Initialize(PetRecycleSystem system)
        {
            _system = system;
            _petTypes = _system.GetPetTypeList();
            _rarities = _system.GetRarityList();
            
            // 填充选项
            foreach (var petType in _petTypes)
            {
                _petTypeOption.AddItem(petType);
            }
            
            foreach (var rarity in _rarities)
            {
                _rarityOption.AddItem(rarity);
            }
            
            UpdateStatistics();
            UpdateHistory();
        }
        
        private void CreateUI()
        {
            // 背景面板
            var bgPanel = new Panel
            {
                AnchorsPreset = Control.LayoutPreset.FullRect,
                Modulate = new Color(0.1f, 0.1f, 0.15f, 0.95f)
            };
            AddChild(bgPanel);
            
            // 标题
            _titleLabel = new Label
            {
                Text = "🐾 Pet Recycle System",
                AnchorsPreset = Control.LayoutPreset.TopWide,
                AnchorTop = 0.0f,
                AnchorBottom = 0.0f,
                OffsetTop = 20,
                OffsetBottom = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            AddChild(_titleLabel);
            
            // Tab容器
            _tabContainer = new TabContainer
            {
                AnchorsPreset = Control.LayoutPreset.FullRect,
                AnchorTop = 0.15f,
                AnchorBottom = 0.9f,
                OffsetLeft = 20,
                OffsetRight = -20
            };
            AddChild(_tabContainer);
            
            // 创建标签页
            CreateRecycleTab();
            CreateHistoryTab();
            CreateStatisticsTab();
        }
        
        private void CreateRecycleTab()
        {
            var recycleTab = new Control();
            _tabContainer.AddChild(recycleTab);
            _tabContainer.SetTabTitle(0, "♻️ Recycle");
            
            var vbox = new VBoxContainer
            {
                AnchorsPreset = Control.LayoutPreset.FullRect,
                OffsetLeft = 20,
                OffsetRight = -20,
                OffsetTop = 20,
                OffsetBottom = -20
            };
            recycleTab.AddChild(vbox);
            
            // 宠物类型选择
            var petTypeLabel = new Label { Text = "Pet Type:" };
            vbox.AddChild(petTypeLabel);
            
            _petTypeOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(300, 40)
            };
            vbox.AddChild(_petTypeOption);
            
            // 稀有度选择
            var rarityLabel = new Label { Text = "Rarity:" };
            vbox.AddChild(rarityLabel);
            
            _rarityOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(300, 40)
            };
            vbox.AddChild(_rarityOption);
            
            // 等级选择
            var levelLabel = new Label { Text = "Pet Level:" };
            vbox.AddChild(levelLabel);
            
            _levelSpinBox = new SpinBox
            {
                MinValue = 1,
                MaxValue = 100,
                Value = 1,
                CustomMinimumSize = new Vector2(300, 40)
            };
            vbox.AddChild(_levelSpinBox);
            
            // 按钮容器
            var buttonHbox = new HBoxContainer { CustomMinimumSize = new Vector2(0, 50) };
            vbox.AddChild(buttonHbox);
            
            _previewButton = new Button
            {
                Text = "👁 Preview",
                CustomMinimumSize = new Vector2(150, 40)
            };
            _previewButton.Pressed += OnPreviewPressed;
            buttonHbox.AddChild(_previewButton);
            
            _recycleButton = new Button
            {
                Text = "♻️ Recycle!",
                CustomMinimumSize = new Vector2(150, 40)
            };
            _recycleButton.Pressed += OnRecyclePressed;
            buttonHbox.AddChild(_recycleButton);
            
            // 预览区域
            _previewLabel = new Label
            {
                Text = "Click Preview to see materials",
                VerticalAlignment = VerticalAlignment.Top,
                CustomMinimumSize = new Vector2(0, 200)
            };
            vbox.AddChild(_previewLabel);
            
            _previewContainer = new VBoxContainer { CustomMinimumSize = new Vector2(0, 200) };
            vbox.AddChild(_previewContainer);
        }
        
        private void CreateHistoryTab()
        {
            var historyTab = new Control();
            _tabContainer.AddChild(historyTab);
            _tabContainer.SetTabTitle(1, "📜 History");
            
            var scroll = new ScrollContainer
            {
                AnchorsPreset = Control.LayoutPreset.FullRect,
                OffsetLeft = 20,
                OffsetRight = -20,
                OffsetTop = 20,
                OffsetBottom = -20
            };
            historyTab.AddChild(scroll);
            
            _historyContainer = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(700, 400)
            };
            scroll.AddChild(_historyContainer);
        }
        
        private void CreateStatisticsTab()
        {
            var statsTab = new Control();
            _tabContainer.AddChild(statsTab);
            _tabContainer.SetTabTitle(2, "📊 Statistics");
            
            _statsLabel = new Label
            {
                Text = "Loading statistics...",
                AnchorsPreset = Control.LayoutPreset.FullRect,
                OffsetLeft = 20,
                OffsetRight = -20,
                OffsetTop = 20,
                OffsetBottom = -20,
                VerticalAlignment = VerticalAlignment.Top
            };
            _statsLabel.AddThemeFontSizeOverride("font_size", 18);
            statsTab.AddChild(_statsLabel);
        }
        
        private void OnPreviewPressed()
        {
            if (_system == null) return;
            
            var petType = _petTypes[_petTypeOption.Selected];
            var rarity = _rarities[_rarityOption.Selected];
            var level = (int)_levelSpinBox.Value;
            
            var preview = _system.PreviewRecycle(petType, rarity, level);
            
            _previewContainer.GetChildren().ForEach(c => c.QueueFree());
            
            if (preview.Count == 0)
            {
                _previewLabel.Text = "No materials preview available";
                return;
            }
            
            _previewLabel.Text = $"📦 Preview ({preview.Count} materials):";
            
            foreach (var material in preview)
            {
                var materialLabel = new Label
                {
                    Text = $"  • {material.MaterialName} x{material.Quantity} (Value: {material.Value})"
                };
                _previewContainer.AddChild(materialLabel);
            }
        }
        
        private void OnRecyclePressed()
        {
            if (_system == null) return;
            
            var petType = _petTypes[_petTypeOption.Selected];
            var rarity = _rarities[_rarityOption.Selected];
            var level = (int)_levelSpinBox.Value;
            var petName = $"Pet {DateTime.Now.Ticks % 10000}";
            
            var record = _system.RecyclePet(petType, petName, rarity, level);
            
            // 显示结果
            string resultText = $"✅ Recycled {petName} ({rarity}, Lv.{level})!\n";
            resultText += $"📦 Materials: {record.Materials.Count}\n";
            resultText += $"✨ Experience: +{record.ExperienceGained}\n\n";
            resultText += "Materials received:\n";
            
            foreach (var material in record.Materials)
            {
                resultText += $"  • {material.MaterialName} x{material.Quantity}\n";
            }
            
            _previewLabel.Text = resultText;
            
            // 更新UI
            UpdateStatistics();
            UpdateHistory();
        }
        
        private void UpdateStatistics()
        {
            if (_system == null) return;
            
            var stats = _system.GetStatistics();
            
            _statsLabel.Text = "📊 Pet Recycle Statistics\n\n";
            _statsLabel.Text += $"Total Recycled: {stats["TotalRecycled"]}\n\n";
            _statsLabel.Text += "By Rarity:\n";
            _statsLabel.Text += $"  Common: {stats["CommonRecycled"]}\n";
            _statsLabel.Text += $"  Uncommon: {stats["UncommonRecycled"]}\n";
            _statsLabel.Text += $"  Rare: {stats["RareRecycled"]}\n";
            _statsLabel.Text += $"  Epic: {stats["EpicRecycled"]}\n";
            _statsLabel.Text += $"  Legendary: {stats["LegendaryRecycled"]}\n\n";
            _statsLabel.Text += $"Total Materials: {stats["TotalMaterials"]}\n";
            _statsLabel.Text += $"Total Experience: {stats["TotalExperience"]}";
        }
        
        private void UpdateHistory()
        {
            if (_system == null) return;
            
            _historyContainer.GetChildren().ForEach(c => c.QueueFree());
            
            var history = _system.GetRecycleHistory(20);
            
            if (history.Count == 0)
            {
                var emptyLabel = new Label { Text = "No recycle history yet" };
                _historyContainer.AddChild(emptyLabel);
                return;
            }
            
            foreach (var record in history)
            {
                var recordLabel = new Label
                {
                    Text = $"[{DateTime.FromUnixTimeSeconds(record.Timestamp):HH:mm:ss}] {record.PetName} ({record.Rarity}, Lv.{record.Level}) - {record.Materials.Count} materials, +{record.ExperienceGained} XP"
                };
                _historyContainer.AddChild(recordLabel);
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // ESC键关闭
                if (keyEvent.Keycode == Key.Escape)
                {
                    Hide();
                    GD.Print("[PetRecycleUI] Closed");
                }
            }
        }
    }
}
