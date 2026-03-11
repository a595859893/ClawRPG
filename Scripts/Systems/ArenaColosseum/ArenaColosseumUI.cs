using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 角斗场UI界面
    /// </summary>
    public class ArenaColosseumUI : Control
    {
        private Control _mainPanel;
        private VBoxContainer _colosseumList;
        private VBoxContainer _myStatsPanel;
        private VBoxContainer _historyPanel;
        private TabContainer _tabContainer;
        
        // 当前选中的角斗场
        private ArenaColosseumData.Colosseum _selectedColosseum;
        
        // 信号
        public event Action OnClose;

        public override void _Ready()
        {
            SetupUI();
            RefreshData();
        }

        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);

            var mainVBox = new VBoxContainer();
            mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainVBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(mainVBox);

            // 标题栏
            var titleLabel = new Label();
            titleLabel.Text = "  🏟️ 角斗场竞技";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainVBox.AddChild(titleLabel);

            // Tab 容器
            _tabContainer = new TabContainer();
            _tabContainer.SetHExpand(Control.ExpandMode.ExpandAndFill);
            _tabContainer.SetVExpand(Control.ExpandMode.ExpandAndFill);
            _tabContainer.CustomMinimumSize = new Vector2(0, 500);
            mainVBox.AddChild(_tabContainer);

            // 角斗场列表
            var listScroll = new ScrollContainer();
            listScroll.Name = "角斗场列表";
            _colosseumList = new VBoxContainer();
            _colosseumList.SetHExpand(Control.ExpandMode.ExpandAndFill);
            listScroll.AddChild(_colosseumList);
            _tabContainer.AddChild(listScroll);

            // 我的统计
            var statsScroll = new ScrollContainer();
            statsScroll.Name = "我的统计";
            _myStatsPanel = new VBoxContainer();
            _myStatsPanel.SetHExpand(Control.ExpandMode.ExpandAndFill);
            statsScroll.AddChild(_myStatsPanel);
            _tabContainer.AddChild(statsScroll);

            // 历史记录
            var historyScroll = new ScrollContainer();
            historyScroll.Name = "对战历史";
            _historyPanel = new VBoxContainer();
            _historyPanel.SetHExpand(Control.ExpandMode.ExpandAndFill);
            historyScroll.AddChild(_historyPanel);
            _tabContainer.AddChild(historyScroll);

            // 关闭按钮
            var closeButton = new Button();
            closeButton.Text = "关闭 (ESC)";
            closeButton.Pressed += () => OnClose?.Invoke();
            mainVBox.AddChild(closeButton);

            // 输入处理
            SetProcessInput(true);
        }

        private void RefreshData()
        {
            RefreshColosseumList();
            RefreshStats();
            RefreshHistory();
        }

        private void RefreshColosseumList()
        {
            // 清理现有项
            foreach (var child in _colosseumList.GetChildren())
            {
                child.QueueFree();
            }

            var colosseums = ArenaColosseumSystem.Instance.GetColosseumList();
            
            // 按类型分组
            var typeGroups = new Dictionary<ArenaColosseumData.ColosseumType, List<ArenaColosseumData.Colosseum>>();
            foreach (var c in colosseums)
            {
                if (!typeGroups.ContainsKey(c.Type))
                    typeGroups[c.Type] = new List<ArenaColosseumData.Colosseum>();
                typeGroups[c.Type].Add(c);
            }

            foreach (var group in typeGroups)
            {
                // 类型标题
                var typeLabel = new Label();
                typeLabel.Text = $"=== {GetTypeName(group.Key)} ===";
                typeLabel.AddThemeFontSizeOverride("font_size", 18);
                typeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
                _colosseumList.AddChild(typeLabel);

                // 每个角斗场
                foreach (var colosseum in group.Value)
                {
                    var itemPanel = CreateColosseumItem(colosseum);
                    _colosseumList.AddChild(itemPanel);
                }
            }
        }

        private Control CreateColosseumItem(ArenaColosseumData.Colosseum colosseum)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(700, 80);
            panel.SetHExpand(Control.ExpandMode.ExpandAndFill);

            var hBox = new HBoxContainer();
            hBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            hBox.AddThemeConstantOverride("separation", 20);
            panel.AddChild(hBox);

            // 左侧信息
            var infoVBox = new VBoxContainer();
            infoVBox.SetVExpand(Control.ExpandMode.ExpandAndFill);
            hBox.AddChild(infoVBox);

            var nameLabel = new Label();
            nameLabel.Text = colosseum.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            infoVBox.AddChild(nameLabel);

            var descLabel = new Label();
            descLabel.Text = colosseum.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            infoVBox.AddChild(descLabel);

            var statsLabel = new Label();
            statsLabel.Text = $"等级: {colosseum.MinLevel}+ | 人数: {colosseum.MaxPlayers} | 时长: {colosseum.Duration:F0}秒";
            statsLabel.AddThemeFontSizeOverride("font_size", 12);
            statsLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            infoVBox.AddChild(statsLabel);

            // 中间奖励
            var rewardVBox = new VBoxContainer();
            rewardVBox.SetHExpand(Control.ExpandMode.ExpandAndFill);
            rewardVBox.Alignment = BoxContainer.AlignmentMode.Center;
            hBox.AddChild(rewardVBox);

            var feeLabel = new Label();
            feeLabel.Text = $"参赛费: {colosseum.EntryFee} 金";
            feeLabel.AddThemeFontSizeOverride("font_size", 14);
            feeLabel.Modulate = colosseum.EntryFee > 0 ? new Color(1f, 0.5f, 0.5f) : new Color(0.5f, 1f, 0.5f);
            rewardVBox.AddChild(feeLabel);

            var prizeLabel = new Label();
            prizeLabel.Text = $"胜利奖励: {colosseum.WinnerReward} 金";
            prizeLabel.AddThemeFontSizeOverride("font_size", 14);
            prizeLabel.Modulate = new Color(1f, 0.84f, 0f);
            rewardVBox.AddChild(prizeLabel);

            // 右侧按钮
            var buttonVBox = new VBoxContainer();
            buttonVBox.Alignment = BoxContainer.AlignmentMode.Center;
            hBox.AddChild(buttonVBox);

            var joinButton = new Button();
            joinButton.Text = "加入";
            joinButton.Pressed += () => OnJoinPressed(colosseum);
            buttonVBox.AddChild(joinButton);

            return panel;
        }

        private void RefreshStats()
        {
            // 清理现有项
            foreach (var child in _myStatsPanel.GetChildren())
            {
                child.QueueFree();
            }

            var playerData = ArenaColosseumSystem.Instance.GetPlayerData(0); // 玩家ID需要从Player获取

            var statsLabel = new Label();
            statsLabel.Text = $"总场次: {playerData.TotalMatches} | 胜: {playerData.Wins} | 负: {playerData.Losses}";
            statsLabel.AddThemeFontSizeOverride("font_size", 16);
            _myStatsPanel.AddChild(statsLabel);

            var winRate = playerData.TotalMatches > 0 ? (float)playerData.Wins / playerData.TotalMatches * 100 : 0;
            var winRateLabel = new Label();
            winRateLabel.Text = $"胜率: {winRate:F1}%";
            winRateLabel.AddThemeFontSizeOverride("font_size", 14);
            _myStatsPanel.AddChild(winRateLabel);

            var streakLabel = new Label();
            streakLabel.Text = $"最高连胜: {playerData.HighestStreak} | 当前连胜: {playerData.CurrentStreak}";
            streakLabel.AddThemeFontSizeOverride("font_size", 14);
            _myStatsPanel.AddChild(streakLabel);

            var prizeLabel = new Label();
            prizeLabel.Text = $"总收益: {playerData.TotalPrizeEarned} 金 | 总支出: {playerData.TotalEntryFees} 金";
            prizeLabel.AddThemeFontSizeOverride("font_size", 14);
            _myStatsPanel.AddChild(prizeLabel);

            var ratingLabel = new Label();
            ratingLabel.Text = $"竞技积分: {playerData.Rating}";
            ratingLabel.AddThemeFontSizeOverride("font_size", 18);
            ratingLabel.Modulate = new Color(1f, 0.84f, 0f);
            _myStatsPanel.AddChild(ratingLabel);

            var killsLabel = new Label();
            killsLabel.Text = $"总击杀: {playerData.TotalKills} | 最高伤害: {playerData.HighestDamage}";
            killsLabel.AddThemeFontSizeOverride("font_size", 14);
            _myStatsPanel.AddChild(killsLabel);
        }

        private void RefreshHistory()
        {
            // 清理现有项
            foreach (var child in _historyPanel.GetChildren())
            {
                child.QueueFree();
            }

            var playerData = ArenaColosseumSystem.Instance.GetPlayerData(0);
            var history = playerData.History;

            if (history.Count == 0)
            {
                var noDataLabel = new Label();
                noDataLabel.Text = "暂无对战记录";
                noDataLabel.AddThemeFontSizeOverride("font_size", 14);
                noDataLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
                _historyPanel.AddChild(noDataLabel);
                return;
            }

            // 倒序显示最近10条
            int count = 0;
            for (int i = history.Count - 1; i >= 0 && count < 10; i--)
            {
                var record = history[i];
                var recordPanel = new PanelContainer();
                recordPanel.CustomMinimumSize = new Vector2(0, 50);
                recordPanel.SetHExpand(Control.ExpandMode.ExpandAndFill);

                var hBox = new HBoxContainer();
                hBox.AddThemeConstantOverride("separation", 15);
                recordPanel.AddChild(hBox);

                var resultLabel = new Label();
                resultLabel.Text = record.IsWinner ? "✅ 胜利" : "❌ 失败";
                resultLabel.Modulate = record.IsWinner ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f);
                hBox.AddChild(resultLabel);

                var typeLabel = new Label();
                typeLabel.Text = GetTypeName(record.Type);
                hBox.AddChild(typeLabel);

                var damageLabel = new Label();
                damageLabel.Text = $"伤害: {record.DamageDealt}";
                hBox.AddChild(damageLabel);

                var prizeLabel = new Label();
                prizeLabel.Text = $"奖励: {record.PrizeEarned}";
                prizeLabel.Modulate = new Color(1f, 0.84f, 0f);
                hBox.AddChild(prizeLabel);

                _historyPanel.AddChild(recordPanel);
                count++;
            }
        }

        private void OnJoinPressed(ArenaColosseumData.Colosseum colosseum)
        {
            // TODO: 从Player获取实际数据
            int playerId = 0;
            string playerName = "Player";
            int level = 1;
            int health = 100;
            int damage = 10;
            int wins = 0;
            int losses = 0;

            bool success = ArenaColosseumSystem.Instance.JoinColosseum(
                playerId, colosseum.Id, playerName, level, health, damage, wins, losses);

            if (success)
            {
                GD.Print($"[ArenaColosseumUI] Joined colosseum: {colosseum.Name}");
                
                // 刷新UI
                RefreshData();
            }
            else
            {
                GD.PrintErr($"[ArenaColosseumUI] Failed to join colosseum: {colosseum.Name}");
            }
        }

        private string GetTypeName(ArenaColosseumData.ColosseumType type)
        {
            switch (type)
            {
                case ArenaColosseumData.ColosseumType.SoloDuel:
                    return "⚔️ 单挑";
                case ArenaColosseumData.ColosseumType.TeamArena:
                    return "👥 团队战";
                case ArenaColosseumData.ColosseumType.FreeForAll:
                    return "🔥 大乱斗";
                case ArenaColosseumData.ColosseumType.MountCombat:
                    return "🐎 坐骑战";
                case ArenaColosseumData.ColosseumType.PetBattle:
                    return "🐾 宠物战";
                default:
                    return type.ToString();
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Escape)
                {
                    OnClose?.Invoke();
                }
            }
        }

        public void Show()
        {
            Visible = true;
            RefreshData();
        }

        public void Hide()
        {
            Visible = false;
        }
    }
}
