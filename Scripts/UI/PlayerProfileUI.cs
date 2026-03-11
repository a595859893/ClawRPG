using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    
    /// <summary>
    /// 玩家资料卡界面 - 显示玩家详细信息和成就进度
    /// </summary>
    public class PlayerProfileUI : Control
    {
        // 节点引用
        private PanelContainer _mainPanel;
        private VBoxContainer _contentBox;
        private Label _titleLabel;
        private TextureRect _avatarFrame;
        private Label _playerNameLabel;
        private Label _levelLabel;
        private Label _classLabel;
        
        // 属性部分
        private Label _healthLabel;
        private Label _manaLabel;
        private Label _strengthLabel;
        private Label _agilityLabel;
        private Label _intelligenceLabel;
        private Label _attackLabel;
        private Label _defenseLabel;
        private Label _critRateLabel;
        private Label _critDamageLabel;
        private Label _dodgeLabel;
        private Label _blockLabel;
        
        // 统计部分
        private Label _killsLabel;
        private Label _deathsLabel;
        private Label _goldLabel;
        private Label _highestLevelLabel;
        private Label _highestComboLabel;
        private Label _playTimeLabel;
        
        // 成就部分
        private Label _achievementsLabel;
        private Label _titlesLabel;
        private Label _mountsLabel;
        private Label _petsLabel;
        
        // 装备外观
        private TextureRect _weaponSlot;
        private TextureRect _armorSlot;
        private TextureRect _helmetSlot;
        private TextureRect _bootsSlot;
        private TextureRect _accessorySlot;
        
        private Button _closeButton;
        private bool _isVisible = false; 
        
        public override void _Ready()
        {
            SetupUI();
            Visible = false; 
            GetTree().Root.SizeChanged += OnWindowSizeChanged;
        }
        
        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(500, 650);
            AddChild(_mainPanel);
            
            // 样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.8f, 0.6f, 0.2f, 1f);
            style.SetBorderWidthAll(3);
            style.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // 内容容器
            _contentBox = new VBoxContainer();
            _contentBox.SetCustomMinimumSize(new Vector2(480, 630));
            _contentBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(_contentBox);
            
            // 标题栏
            var titleBox = new HBoxContainer();
            _contentBox.AddChild(titleBox);
            
            _titleLabel = new Label();
            _titleLabel.Text = "  玩家资料";
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
            titleBox.AddChild(_titleLabel);
            
            titleBox.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlagsExpandFill });
            
            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _closeButton.Pressed += OnClosePressed;
            titleBox.AddChild(_closeButton);
            
            // 分隔线
            AddSeparator();
            
            // 玩家信息部分
            AddSectionTitle("玩家信息");
            
            var infoBox = new HBoxContainer();
            _contentBox.AddChild(infoBox);
            
            // 头像框
            _avatarFrame = new TextureRect();
            _avatarFrame.CustomMinimumSize = new Vector2(80, 80);
            _avatarFrame.Modulate = new Color(1f, 0.9f, 0.6f, 1f);
            infoBox.AddChild(_avatarFrame);
            
            // 绘制默认头像
            DrawDefaultAvatar();
            
            var infoStack = new VBoxContainer();
            infoStack.AddThemeConstantOverride("separation", 5);
            infoBox.AddChild(infoStack);
            
            _playerNameLabel = new Label();
            _playerNameLabel.Text = "冒险者";
            _playerNameLabel.AddThemeFontSizeOverride("font_size", 20);
            _playerNameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
            infoStack.AddChild(_playerNameLabel);
            
            _levelLabel = new Label();
            _levelLabel.Text = "等级 1";
            _levelLabel.AddThemeFontSizeOverride("font_size", 16);
            infoStack.AddChild(_levelLabel);
            
            _classLabel = new Label();
            _classLabel.Text = "战士";
            _classLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.9f, 1f));
            infoStack.AddChild(_classLabel);
            
            AddSeparator();
            
            // 属性部分
            AddSectionTitle("属性");
            
            var statsGrid = new GridContainer();
            statsGrid.Columns = 2;
            _contentBox.AddChild(statsGrid);
            
            _healthLabel = CreateStatLabel("生命: 100", statsGrid);
            _manaLabel = CreateStatLabel("魔法: 50", statsGrid);
            _strengthLabel = CreateStatLabel("力量: 10", statsGrid);
            _agilityLabel = CreateStatLabel("敏捷: 10", statsGrid);
            _intelligenceLabel = CreateStatLabel("智力: 10", statsGrid);
            _attackLabel = CreateStatLabel("攻击: 15", statsGrid);
            _defenseLabel = CreateStatLabel("防御: 5", statsGrid);
            _critRateLabel = CreateStatLabel("暴击率: 5%", statsGrid);
            _critDamageLabel = CreateStatLabel("暴击伤害: 150%", statsGrid);
            _dodgeLabel = CreateStatLabel("闪避: 5%", statsGrid);
            _blockLabel = CreateStatLabel("格挡: 10%", statsGrid);
            
            AddSeparator();
            
            // 统计部分
            AddSectionTitle("战斗统计");
            
            var combatStatsGrid = new GridContainer();
            combatStatsGrid.Columns = 2;
            _contentBox.AddChild(combatStatsGrid);
            
            _killsLabel = CreateStatLabel("击杀: 0", combatStatsGrid);
            _deathsLabel = CreateStatLabel("死亡: 0", combatStatsGrid);
            _goldLabel = CreateStatLabel("金币: 0", combatStatsGrid);
            _highestLevelLabel = CreateStatLabel("最高等级: 1", combatStatsGrid);
            _highestComboLabel = CreateStatLabel("最高连击: 0", combatStatsGrid);
            _playTimeLabel = CreateStatLabel("游戏时间: 0:00:00", combatStatsGrid);
            
            AddSeparator();
            
            // 收藏部分
            AddSectionTitle("收藏进度");
            
            var collectionGrid = new GridContainer();
            collectionGrid.Columns = 2;
            _contentBox.AddChild(collectionGrid);
            
            _achievementsLabel = CreateStatLabel("成就: 0/0", collectionGrid);
            _titlesLabel = CreateStatLabel("称号: 0", collectionGrid);
            _mountsLabel = CreateStatLabel("坐骑: 0", collectionGrid);
            _petsLabel = CreateStatLabel("宠物: 0", collectionGrid);
            
            CenterPanel();
        }
        
        private void DrawDefaultAvatar()
        {
            // 使用程序化方式绘制简单头像
            var viewport = new SubViewport();
            viewport.Size = new Vector2I(80, 80);
            viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
            
            var control = new Control();
            control.Size = new Vector2(80, 80);
            viewport.AddChild(control);
            
            var rect = new ColorRect();
            rect.Color = new Color(0.3f, 0.5f, 0.8f, 1f);
            rect.Size = new Vector2(40, 50);
            rect.Position = new Vector2(20, 10);
            control.AddChild(rect);
            
            var head = new ColorRect();
            head.Color = new Color(0.9f, 0.75f, 0.6f, 1f);
            head.Size = new Vector2(30, 30);
            head.Position = new Vector2(25, 5);
            control.AddChild(head);
            
            AddChild(viewport);
            
            // 创建纹理
            var image = viewport.GetTexture().GetImage();
            var texture = ImageTexture.CreateFromImage(image);
            _avatarFrame.Texture = texture;
            
            viewport.QueueFree();
        }
        
        private Label CreateStatLabel(string text, GridContainer parent)
        {
            var label = new Label();
            label.Text = text;
            label.AddThemeFontSizeOverride("font_size", 14);
            parent.AddChild(label);
            return label;
        }
        
        private void AddSectionTitle(string title)
        {
            var label = new Label();
            label.Text = "■ " + title;
            label.AddThemeFontSizeOverride("font_size", 16);
            label.AddThemeColorOverride("font_color", new Color(0.3f, 0.8f, 1f, 1f));
            _contentBox.AddChild(label);
        }
        
        private void AddSeparator()
        {
            var separator = new HSeparator();
            separator.AddThemeConstantOverride("separation", 5);
            _contentBox.AddChild(separator);
        }
        
        private void CenterPanel()
        {
            var viewportSize = GetViewportRect().Size;
            var panelSize = _mainPanel.CustomMinimumSize;
            _mainPanel.Position = (viewportSize - panelSize) / 2;
        }
        
        private void OnWindowSizeChanged()
        {
            CenterPanel();
        }
        
        private void OnClosePressed()
        {
            Toggle();
        }
        
        public void Toggle()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                RefreshData();
            }
        }
        
        public void RefreshData()
        {
            var player = GetPlayer();
            if (player == null) return;
            
            // 玩家基本信息
            _playerNameLabel.Text = "  " + (player.Name ?? "冒险者");
            _levelLabel.Text = "  等级 " + player.Level;
            _classLabel.Text = "  战士";
            
            // 属性
            var maxHealth = player.GetMaxHealth();
            var maxMana = player.GetMaxMana();
            
            _healthLabel.Text = $"  生命: {player.Health:F0}/{maxHealth:F0}";
            _manaLabel.Text = $"  魔法: {player.Mana:F0}/{maxMana:F0}";
            _strengthLabel.Text = $"  力量: {player.Strength}";
            _agilityLabel.Text = $"  敏捷: {player.Agility}";
            _intelligenceLabel.Text = $"  智力: {player.Intelligence}";
            
            var totalAttack = player.GetTotalAttack();
            var totalDefense = player.GetTotalDefense();
            
            _attackLabel.Text = $"  攻击: {totalAttack:F0}";
            _defenseLabel.Text = $"  防御: {totalDefense:F0}";
            _critRateLabel.Text = $"  暴击率: {player.CritRate:F1}%";
            _critDamageLabel.Text = $"  暴击伤害: {player.CritDamage:F0}%";
            _dodgeLabel.Text = $"  闪避: {player.DodgeRate:F1}%";
            _blockLabel.Text = $"  格挡: {player.BlockRate:F1}%";
            
            // 战斗统计
            if (StatisticsSystem.Instance != null)
            {
                var stats = StatisticsSystem.Instance.GetStatistics();
                _killsLabel.Text = $"  击杀: {stats.TotalKills}";
                _deathsLabel.Text = $"  死亡: {stats.TotalDeaths}";
                _goldLabel.Text = $"  金币: {player.Gold}";
                _highestLevelLabel.Text = $"  最高等级: {stats.HighestLevel}";
                _highestComboLabel.Text = $"  最高连击: {stats.HighestCombo}";
                
                var hours = stats.PlayTimeSeconds / 3600;
                var minutes = (stats.PlayTimeSeconds % 3600) / 60;
                var seconds = stats.PlayTimeSeconds % 60;
                _playTimeLabel.Text = $"  游戏时间: {hours}:{minutes:D2}:{seconds:D2}";
            }
            else
            {
                _killsLabel.Text = "  击杀: 0";
                _deathsLabel.Text = "  死亡: 0";
                _goldLabel.Text = $"  金币: {player.Gold}";
                _highestLevelLabel.Text = "  最高等级: 1";
                _highestComboLabel.Text = "  最高连击: 0";
                _playTimeLabel.Text = "  游戏时间: 0:00:00";
            }
            
            // 收藏进度
            if (AchievementManager.Instance != null)
            {
                var unlockedCount = 0;
                var totalCount = 0;
                foreach (var achievement in AchievementManager.Instance.GetAllAchievements())
                {
                    if (achievement.IsUnlocked) unlockedCount++;
                    totalCount++;
                }
                _achievementsLabel.Text = $"  成就: {unlockedCount}/{totalCount}";
            }
            else
            {
                _achievementsLabel.Text = "  成就: 0/0";
            }
            
            // 称号
            if (TitleSystem.Instance != null)
            {
                var titleCount = TitleSystem.Instance.GetUnlockedTitles().Count;
                _titlesLabel.Text = $"  称号: {titleCount}";
            }
            else
            {
                _titlesLabel.Text = "  称号: 0";
            }
            
            // 坐骑
            if (MountManager.Instance != null)
            {
                var mountCount = MountManager.Instance.GetOwnedMounts().Count;
                _mountsLabel.Text = $"  坐骑: {mountCount}";
            }
            else
            {
                _mountsLabel.Text = "  坐骑: 0";
            }
            
            // 宠物
            if (PetManager.Instance != null)
            {
                var petCount = PetManager.Instance.GetOwnedPets().Count;
                _petsLabel.Text = $"  宠物: {petCount}";
            }
            else
            {
                _petsLabel.Text = "  宠物: 0";
            }
        }
        
        private Player GetPlayer()
        {
            var main = GetTree().CurrentScene;
            if (main == null) return null;
            
            foreach (var child in main.GetChildren())
            {
                if (child is Player player)
                {
                    return player;
                }
            }
            return null;
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // F键切换资料卡
                if (keyEvent.Keycode == Key.F)
                {
                    Toggle();
                }
                // Escape 关闭
                else if (keyEvent.Keycode == Key.Escape && _isVisible)
                {
                    Toggle();
                }
            }
        }
    }
}
