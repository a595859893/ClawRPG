using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 敌人变异UI
    /// 显示敌人变异信息和统计
    /// </summary>
    public partial class EnemyMutationUI : Control
    {
        private VBoxContainer _mainContainer;
        private Label _titleLabel;
        private Label _progressLabel;
        private GridContainer _mutationGrid;
        
        // 统计面板
        private Label _statsLabel;
        
        // 稀有度筛选
        private OptionButton _rarityFilter;
        
        // 变异类型筛选
        private OptionButton _typeFilter;
        
        private bool _isVisible = false;
        private List<EnemyMutationData.Mutation> _displayedMutations = new();

        public override void _Ready()
        {
            CreateUI();
            Visible = false;
            GD.Print("敌人变异UI已创建 - 按 M 键切换");
        }

        private void CreateUI()
        {
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorPreset(Control.LayoutPreset.Center);
            _mainContainer.CustomMinimumSize = new Vector2(600, 500);
            AddChild(_mainContainer);

            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "   敌人变异百科   ";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(_titleLabel);

            // 发现进度
            _progressLabel = new Label();
            _progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _mainContainer.AddChild(_progressLabel);

            // 筛选容器
            var filterContainer = new HBoxContainer();
            _mainContainer.AddChild(filterContainer);

            // 稀有度筛选
            var rarityLabel = new Label();
            rarityLabel.Text = "稀有度:";
            filterContainer.AddChild(rarityLabel);

            _rarityFilter = new OptionButton();
            _rarityFilter.AddItem("全部", 0);
            _rarityFilter.AddItem("普通", 1);
            _rarityFilter.AddItem("优秀", 2);
            _rarityFilter.AddItem("稀有", 3);
            _rarityFilter.AddItem("史诗", 4);
            _rarityFilter.AddItem("传说", 5);
            _rarityFilter.Selected = 0;
            _rarityFilter.ItemSelected += OnRarityFilterChanged;
            filterContainer.AddChild(_rarityFilter);

            // 类型筛选
            var typeLabel = new Label();
            typeLabel.Text = "  类型:";
            filterContainer.AddChild(typeLabel);

            _typeFilter = new OptionButton();
            _typeFilter.AddItem("全部", 0);
            _typeFilter.AddItem("装甲化", 1);
            _typeFilter.AddItem("迅捷化", 2);
            _typeFilter.AddItem("生命强化", 3);
            _typeFilter.AddItem("狂怒化", 4);
            _typeFilter.AddItem("再生", 5);
            _typeFilter.AddItem("护盾", 6);
            _typeFilter.AddItem("爆炸化", 7);
            _typeFilter.AddItem("吸血", 8);
            _typeFilter.AddItem("分裂", 9);
            _typeFilter.AddItem("愤怒", 10);
            _typeFilter.AddItem("反射", 11);
            _typeFilter.AddItem("毒化", 12);
            _typeFilter.AddItem("雷电", 13);
            _typeFilter.AddItem("冰霜", 14);
            _typeFilter.AddItem("燃烧", 15);
            _typeFilter.AddItem("伪装", 16);
            _typeFilter.Selected = 0;
            _typeFilter.ItemSelected += OnTypeFilterChanged;
            filterContainer.AddChild(_typeFilter);

            // 变异网格
            _mutationGrid = new GridContainer();
            _mutationGrid.Columns = 3;
            _mutationGrid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _mutationGrid.CustomMinimumSize = new Vector2(0, 300);
            _mainContainer.AddChild(_mutationGrid);

            // 统计面板
            var statsContainer = new VBoxContainer();
            _mainContainer.AddChild(statsContainer);

            var statsTitle = new Label();
            statsTitle.Text = "击杀统计:";
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            statsContainer.AddChild(statsTitle);

            _statsLabel = new Label();
            _statsLabel.Text = "击杀变异敌人: 0\n总遭遇变异: 0";
            statsContainer.AddChild(_statsLabel);

            // 更新显示
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // 清除现有显示
            foreach (Node child in _mutationGrid.GetChildren())
            {
                child.QueueFree();
            }
            _displayedMutations.Clear();

            var allMutations = EnemyMutationDatabase.GetAllMutations();
            var rarityFilter = _rarityFilter.Selected;
            var typeFilter = _typeFilter.Selected;

            var discovered = EnemyMutationSystem.Instance.GetDiscoveredMutations();

            foreach (var mutation in allMutations)
            {
                // 筛选检查
                bool rarityMatch = rarityFilter == 0 || (int)mutation.Rarity == rarityFilter - 1;
                bool typeMatch = typeFilter == 0 || (int)mutation.Type == typeFilter - 1;
                
                if (!rarityMatch || !typeMatch) continue;

                _displayedMutations.Add(mutation);

                // 创建变异卡片
                var card = CreateMutationCard(mutation, discovered);
                _mutationGrid.AddChild(card);
            }

            // 更新进度
            var progress = EnemyMutationSystem.Instance.GetDiscoveryProgress();
            _progressLabel.Text = $"发现进度: {progress * 100:F1}% ({discovered.Count}/{allMutations.Count})";

            // 更新统计
            var stats = EnemyMutationSystem.Instance.GetStatistics();
            _statsLabel.Text = $"击杀变异敌人: {stats.TotalMutationsKilled}\n总遭遇变异: {stats.TotalMutationsEncountered}";
        }

        private Control CreateMutationCard(EnemyMutationData.Mutation mutation, List<EnemyMutationData.DiscoveredMutation> discovered)
        {
            var card = new PanelContainer();
            card.CustomMinimumSize = new Vector2(180, 120);

            var vbox = new VBoxContainer();
            card.AddChild(vbox);

            // 名称
            var nameLabel = new Label();
            nameLabel.Text = mutation.Name;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(nameLabel);

            // 稀有度
            var rarityLabel = new Label();
            rarityLabel.Text = GetRarityText(mutation.Rarity);
            rarityLabel.HorizontalAlignment = HorizontalAlignment.Center;
            rarityLabel.Modulate = GetRarityColor(mutation.Rarity);
            vbox.AddChild(rarityLabel);

            // 描述
            var descLabel = new Label();
            descLabel.Text = mutation.Description;
            descLabel.HorizontalAlignment = HorizontalAlignment.Center;
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            descLabel.CustomMinimumSize = new Vector2(160, 40);
            vbox.AddChild(descLabel);

            // 属性加成
            var attrLabel = new Label();
            attrLabel.Text = GetAttributeText(mutation);
            attrLabel.HorizontalAlignment = HorizontalAlignment.Center;
            attrLabel.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(attrLabel);

            // 发现状态
            var found = discovered.Exists(d => d.Type == mutation.Type);
            if (found)
            {
                card.Modulate = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                card.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            }

            return card;
        }

        private string GetRarityText(EnemyMutationData.MutationRarity rarity)
        {
            return rarity switch
            {
                EnemyMutationData.MutationRarity.Common => "普通",
                EnemyMutationData.MutationRarity.Uncommon => "优秀",
                EnemyMutationData.MutationRarity.Rare => "稀有",
                EnemyMutationData.MutationRarity.Epic => "史诗",
                EnemyMutationData.MutationRarity.Legendary => "传说",
                _ => "未知"
            };
        }

        private Color GetRarityColor(EnemyMutationData.MutationRarity rarity)
        {
            return rarity switch
            {
                EnemyMutationData.MutationRarity.Common => Colors.Gray,
                EnemyMutationData.MutationRarity.Uncommon => Colors.Green,
                EnemyMutationData.MutationRarity.Rare => Colors.Blue,
                EnemyMutationData.MutationRarity.Epic => Colors.Purple,
                EnemyMutationData.MutationRarity.Legendary => Colors.Orange,
                _ => Colors.White
            };
        }

        private string GetAttributeText(EnemyMutationData.Mutation mutation)
        {
            var attrs = new List<string>();
            
            if (mutation.HealthMultiplier != 1f)
                attrs.Add($"生命 x{mutation.HealthMultiplier:F1}");
            if (mutation.AttackMultiplier != 1f)
                attrs.Add($"攻击 x{mutation.AttackMultiplier:F1}");
            if (mutation.DefenseMultiplier != 1f)
                attrs.Add($"防御 x{mutation.DefenseMultiplier:F1}");
            if (mutation.SpeedMultiplier != 1f)
                attrs.Add($"速度 x{mutation.SpeedMultiplier:F1}");
            if (mutation.RegenPerSecond > 0)
                attrs.Add($"+{mutation.RegenPerSecond:F1}/秒回血");
            if (mutation.LifeStealPercent > 0)
                attrs.Add($"+{mutation.LifeStealPercent * 100:F0}%吸血");
            if (mutation.ExplosionDamage > 0)
                attrs.Add($"爆炸 {mutation.ExplosionDamage}伤害");
            if (mutation.SplitCount > 0)
                attrs.Add($"分裂 {mutation.SplitCount}个");

            return string.Join("\n", attrs);
        }

        private void OnRarityFilterChanged(long index)
        {
            UpdateDisplay();
        }

        private void OnTypeFilterChanged(long index)
        {
            UpdateDisplay();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.M)
                {
                    ToggleVisibility();
                }
            }
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                UpdateDisplay();
            }
        }

        public void Open()
        {
            _isVisible = true;
            Visible = true;
            UpdateDisplay();
        }

        public void Close()
        {
            _isVisible = false;
            Visible = false;
        }
    }
}
