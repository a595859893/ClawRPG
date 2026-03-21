using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.SecretBoss {
    /// <summary>
    /// Secret Boss UI - 隐藏Boss系统UI显示
    /// </summary>
    public class SecretBossUI : Control {
        // UI元素
        private Label _titleLabel;
        private TabContainer _tabContainer;
        
        // 标签页
        private VBoxContainer _bossListTab;
        private VBoxContainer _activeBossTab;
        private VBoxContainer _statisticsTab;
        
        // Boss列表容器
        private ScrollContainer _bossListContainer;
        private VBoxContainer _bossListContent;
        
        // 活跃Boss容器
        private VBoxContainer _activeBossContent;
        
        // 统计容器
        private VBoxContainer _statisticsContent;
        
        // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
        public event Action<int> OnRarityFilterSelectedUI;
        
        // 按键提示
        private Label _hintLabel;
        
        // 过滤器
        private OptionButton _rarityFilter;
        
        // 状态
        private bool _isVisible = false;
        
        public override void _Ready() {
            base._Ready();
            SetupUI();
            SetupInput();
            GD.Print("[SecretBossUI] 隐藏Boss UI已初始化");
        }
        
        private void SetupUI() {
            // 主容器
            RectMinSize = new Vector2(800, 600);
            
            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "🔮 隐藏Boss系统";
            _titleLabel.RectPosition = new Vector2(20, 20);
            _titleLabel.AddColorOverride("font_color", new Color(1f, 0.84f, 0f));
            AddChild(_titleLabel);
            
            // 标签容器
            _tabContainer = new TabContainer();
            _tabContainer.RectPosition = new Vector2(20, 60);
            _tabContainer.RectSize = new Vector2(760, 480);
            AddChild(_tabContainer);
            
            // 创建标签页
            SetupBossListTab();
            SetupActiveBossTab();
            SetupStatisticsTab();
            
            // 稀有度过滤
            _rarityFilter = new OptionButton();
            _rarityFilter.RectPosition = new Vector2(600, 20);
            _rarityFilter.RectSize = new Vector2(180, 30);
            _rarityFilter.AddItem("全部稀有度", 0);
            _rarityFilter.AddItem("普通 (Common)", 1);
            _rarityFilter.AddItem("优秀 (Uncommon)", 2);
            _rarityFilter.AddItem("稀有 (Rare)", 3);
            _rarityFilter.AddItem("史诗 (Epic)", 4);
            _rarityFilter.AddItem("传说 (Legendary)", 5);
            // REQ-058-11: migrated from Godot 3 .Connect() to C# event +=
            _rarityFilter.ItemSelected += _OnRarityFilterSelected; // NEW
            _rarityFilter.Connect("item_selected", this, nameof(_OnRarityFilterSelected)); // TODO: Remove after migration
            AddChild(_rarityFilter);
            
            // 提示
            _hintLabel = new Label();
            _hintLabel.Text = "按 B 键关闭 | 按 Tab 切换标签";
            _hintLabel.RectPosition = new Vector2(20, 550);
            _hintLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            AddChild(_hintLabel);
            
            // 初始隐藏
            Visible = false;
        }
        
        private void SetupBossListTab() {
            _bossListTab = new VBoxContainer();
            _bossListTab.Name = "Boss图鉴";
            _tabContainer.AddChild(_bossListTab);
            
            _bossListContainer = new ScrollContainer();
            _bossListContainer.RectSize = new Vector2(760, 480);
            _bossListTab.AddChild(_bossListContainer);
            
            _bossListContent = new VBoxContainer();
            _bossListContent.RectMinSize = new Vector2(740, 0);
            _bossListContainer.AddChild(_bossListContent);
            
            RefreshBossList();
        }
        
        private void SetupActiveBossTab() {
            _activeBossTab = new VBoxContainer();
            _activeBossTab.Name = "当前活跃";
            _tabContainer.AddChild(_activeBossTab);
            
            _activeBossContent = new VBoxContainer();
            _activeBossContent.RectMinSize = new Vector2(740, 460);
            _activeBossTab.AddChild(_activeBossContent);
            
            RefreshActiveBossList();
        }
        
        private void SetupStatisticsTab() {
            _statisticsTab = new VBoxContainer();
            _statisticsTab.Name = "统计";
            _tabContainer.AddChild(_statisticsTab);
            
            _statisticsContent = new VBoxContainer();
            _statisticsContent.RectMinSize = new Vector2(740, 460);
            _statisticsTab.AddChild(_statisticsContent);
            
            RefreshStatistics();
        }
        
        private void SetupInput() {
            // 连接到输入系统
        }
        
        /// <summary>
        /// 刷新Boss列表
        /// </summary>
        public void RefreshBossList() {
            // 清除现有内容
            foreach (Node child in _bossListContent.GetChildren()) {
                child.QueueFree();
            }
            
            // 获取Boss列表
            var bosses = SecretBossDatabase.GetAllBosses();
            
            foreach (var boss in bosses) {
                // 应用过滤器
                int filterIndex = _rarityFilter.Selected;
                if (filterIndex > 0) {
                    Rarity filterRarity = (Rarity)(filterIndex - 1);
                    if (boss.Rarity != filterRarity) continue;
                }
                
                var bossCard = CreateBossCard(boss);
                _bossListContent.AddChild(bossCard);
            }
        }
        
        /// <summary>
        /// 创建Boss卡片
        /// </summary>
        private Control CreateBossCard(SecretBossData boss) {
            var container = new VBoxContainer();
            container.RectMinSize = new Vector2(700, 120);
            container.AddStyleboxOverride("panel", GetStyleBox("panel", "Card"));
            
            // 标题栏
            var titleBar = new HBoxContainer();
            container.AddChild(titleBar);
            
            // Boss名称和图标
            var nameLabel = new Label();
            nameLabel.Text = $"{(boss.IsDiscovered ? "🔓" : "🔒")} {boss.BossName}";
            nameLabel.AddColorOverride("font_color", GetRarityColor(boss.Rarity));
            titleBar.AddChild(nameLabel);
            
            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            // 稀有度标签
            var rarityLabel = new Label();
            rarityLabel.Text = $"[{boss.Rarity}]";
            rarityLabel.AddColorOverride("font_color", GetRarityColor(boss.Rarity));
            titleBar.AddChild(rarityLabel);
            
            // 类型标签
            var typeLabel = new Label();
            typeLabel.Text = $" {boss.Type}";
            typeLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            titleBar.AddChild(typeLabel);
            
            // 描述
            var descLabel = new Label();
            descLabel.Text = boss.Description;
            descLabel.AddColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
            container.AddChild(descLabel);
            
            // 出现条件
            var conditionLabel = new Label();
            conditionLabel.Text = $"出现条件: {GetConditionText(boss.Condition)}";
            conditionLabel.AddColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
            container.AddChild(conditionLabel);
            
            // 状态
            var statusLabel = new Label();
            string status = "";
            if (boss.IsDefeated) {
                status = $"✓ 已击败 ({boss.DefeatCount}次)";
            } else if (boss.IsDiscovered) {
                status = "👁 已发现";
            } else {
                status = "❓ 未知";
            }
            statusLabel.Text = status;
            statusLabel.AddColorOverride("font_color", boss.IsDefeated ? new Color(0.3f, 1f, 0.3f) : new Color(0.8f, 0.8f, 0.8f));
            container.AddChild(statusLabel);
            
            // 分割线
            var hSeparator = new HSeparator();
            container.AddChild(hSeparator);
            
            return container;
        }
        
        /// <summary>
        /// 刷新活跃Boss列表
        /// </summary>
        public void RefreshActiveBossList() {
            // 清除现有内容
            foreach (Node child in _activeBossContent.GetChildren()) {
                child.QueueFree();
            }
            
            if (SecretBossSystem.Instance == null) {
                var noDataLabel = new Label();
                noDataLabel.Text = "系统未初始化";
                noDataLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                _activeBossContent.AddChild(noDataLabel);
                return;
            }
            
            var activeBosses = SecretBossSystem.Instance.GetActiveBosses();
            
            if (activeBosses.Count == 0) {
                var noDataLabel = new Label();
                noDataLabel.Text = "当前没有活跃的隐藏Boss";
                noDataLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                _activeBossContent.AddChild(noDataLabel);
                return;
            }
            
            foreach (var spawnInfo in activeBosses) {
                var boss = SecretBossDatabase.GetBoss(spawnInfo.BossId);
                if (boss == null) continue;
                
                var bossCard = CreateActiveBossCard(boss, spawnInfo);
                _activeBossContent.AddChild(bossCard);
            }
        }
        
        /// <summary>
        /// 创建活跃Boss卡片
        /// </summary>
        private Control CreateActiveBossCard(SecretBossData boss, SecretBossSpawnInfo spawnInfo) {
            var container = new VBoxContainer();
            container.RectMinSize = new Vector2(700, 100);
            
            var nameLabel = new Label();
            nameLabel.Text = $"⚔ {boss.BossName} [{boss.Rarity}]";
            nameLabel.AddColorOverride("font_color", GetRarityColor(boss.Rarity));
            container.AddChild(nameLabel);
            
            var infoLabel = new Label();
            float elapsed = OS.GetTicksMsec() / 1000f - spawnInfo.SpawnTime;
            float remaining = spawnInfo.Duration - elapsed;
            infoLabel.Text = $"剩余时间: {remaining:F1}秒 | 位置: {spawnInfo.Position}";
            infoLabel.AddColorOverride("font_color", new Color(1f, 0.5f, 0.5f));
            container.AddChild(infoLabel);
            
            var hSeparator = new HSeparator();
            container.AddChild(hSeparator);
            
            return container;
        }
        
        /// <summary>
        /// 刷新统计
        /// </summary>
        public void RefreshStatistics() {
            // 清除现有内容
            foreach (Node child in _statisticsContent.GetChildren()) {
                child.QueueFree();
            }
            
            if (SecretBossSystem.Instance == null) {
                var noDataLabel = new Label();
                noDataLabel.Text = "系统未初始化";
                noDataLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                _statisticsContent.AddChild(noDataLabel);
                return;
            }
            
            var stats = SecretBossSystem.Instance.GetStatistics();
            
            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "📊 隐藏Boss统计";
            titleLabel.AddColorOverride("font_color", new Color(1f, 0.84f, 0f));
            _statisticsContent.AddChild(titleLabel);
            
            AddStatRow("总计Boss数量", stats["TotalBosses"]);
            AddStatRow("已发现", stats["Discovered"]);
            AddStatRow("已击败", stats["Defeated"]);
            AddStatRow("Boss生成次数", stats["TotalSpawns"]);
            AddStatRow("Boss击败次数", stats["TotalDefeats"]);
            AddStatRow("掉落物品总数", stats["TotalDrops"]);
        }
        
        /// <summary>
        /// 添加统计行
        /// </summary>
        private void AddStatRow(string label, int value) {
            var row = new HBoxContainer();
            
            var labelWidget = new Label();
            labelWidget.Text = $"{label}: ";
            labelWidget.AddColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            row.AddChild(labelWidget);
            
            var valueWidget = new Label();
            valueWidget.Text = value.ToString();
            valueWidget.AddColorOverride("font_color", new Color(1f, 0.84f, 0f));
            row.AddChild(valueWidget);
            
            _statisticsContent.AddChild(row);
        }
        
        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        private Color GetRarityColor(Rarity rarity) {
            switch (rarity) {
                case Rarity.Common: return new Color(0.7f, 0.7f, 0.7f);
                case Rarity.Uncommon: return new Color(0.3f, 0.9f, 0.3f);
                case Rarity.Rare: return new Color(0.3f, 0.6f, 1f);
                case Rarity.Epic: return new Color(0.7f, 0.3f, 1f);
                case Rarity.Legendary: return new Color(1f, 0.65f, 0f);
                default: return new Color(1f, 1f, 1f);
            }
        }
        
        /// <summary>
        /// 获取条件文本
        /// </summary>
        private string GetConditionText(SecretBossCondition condition) {
            switch (condition.Type) {
                case ConditionType.TimeOfDay:
                    return $"{condition.RequiredHourStart}:00 - {condition.RequiredHourEnd}:00";
                case ConditionType.Weather:
                    return $"{condition.RequiredWeather}天气";
                case ConditionType.KillCount:
                    return $"击杀{condition.RequiredKillAmount}个{condition.RequiredKillCount}";
                case ConditionType.PlayerLevel:
                    return $"玩家等级{condition.RequiredPlayerLevel}";
                case ConditionType.Luck:
                    return $"幸运值{condition.RequiredLuck}";
                case ConditionType.MoonPhase:
                    return $"月相{condition.RequiredValue}";
                case ConditionType.ComboCount:
                    return $"{condition.RequiredValue}连击";
                case ConditionType.BossDefeated:
                    return $"击败{condition.RequiredBossDefeated}";
                case ConditionType.Location:
                    return condition.RequiredArea;
                default:
                    return "未知条件";
            }
        }
        
        /// <summary>
        /// 获取样式
        /// </summary>
        private StyleBox GetStyleBox(string styleName, string styleType) {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
            style.BorderWidthBottom = 2;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            return style;
        }
        
        /// <summary>
        /// 稀有度过滤选择
        /// </summary>
        private void _OnRarityFilterSelected(int index) {
            // REQ-058-11: Invoke new event
            OnRarityFilterSelectedUI?.Invoke(index);
            RefreshBossList();
        }
        
        /// <summary>
        /// 切换显示
        /// </summary>
        public void Toggle() {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible) {
                RefreshBossList();
                RefreshActiveBossList();
                RefreshStatistics();
            }
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        public override void _UnhandledInput(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                switch (keyEvent.Scancode) {
                    case KeyList.B:
                    case KeyList.Escape:
                        Toggle();
                        break;
                    case KeyList.Tab:
                        // 切换标签页
                        int currentTab = _tabContainer.CurrentTab;
                        int tabCount = _tabContainer.GetChildCount();
                        _tabContainer.CurrentTab = (currentTab + 1) % tabCount;
                        break;
                }
            }
        }
        
        /// <summary>
        /// 设置快捷键绑定
        /// </summary>
        public void SetupKeyBinding() {
            // 在 Godot 项目中配置
            // InputMap.add_action("toggle_secret_boss_ui")
            // InputMap.action_add_awesome("toggle_secret_boss_ui", KeyList.B)
        }
    }
}
