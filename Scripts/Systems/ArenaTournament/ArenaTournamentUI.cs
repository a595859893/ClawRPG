using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 竞技场锦标赛 UI
    /// </summary>
    public partial class ArenaTournamentUI : Control
    {
        // 主容器
        private VBoxContainer _mainContainer;
        private TabContainer _tabContainer;
        
        // 标签页
        private Control _availableTab;
        private Control _activeTab;
        private Control _myTournamentsTab;
        private Control _statisticsTab;
        
        // 当前选中
        private Tournament _selectedTournament;
        
        public override void _Ready()
        {
            SetupUI();
            RefreshData();
        }

        private void SetupUI()
        {
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorPreset(ControlPreset.FullRect);
            _mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(_mainContainer);
            
            // 标题
            var title = new Label();
            title.Text = "🏆 竞技场锦标赛";
            title.AddThemeFontSizeOverride("font_size", 24);
            title.HorizontalAlignment = HorizontalAlignment.Center;
            _mainContainer.AddChild(title);
            
            // 分隔线
            var hsep = new HSeparator();
            _mainContainer.AddChild(hsep);
            
            // Tab容器
            _tabContainer = new TabContainer();
            _tabContainer.SetVExpand(ExpandMode.Expand);
            _mainContainer.AddChild(_tabContainer);
            
            // 创建标签页
            _availableTab = CreateAvailableTab();
            _availableTab.Name = "Available";
            _tabContainer.AddChild(_availableTab);
            
            _activeTab = CreateActiveTab();
            _activeTab.Name = "Active";
            _tabContainer.AddChild(_activeTab);
            
            _myTournamentsTab = CreateMyTournamentsTab();
            _myTournamentsTab.Name = "MyTournaments";
            _tabContainer.AddChild(_myTournamentsTab);
            
            _statisticsTab = CreateStatisticsTab();
            _statisticsTab.Name = "Statistics";
            _tabContainer.AddChild(_statisticsTab);
            
            // 设置Tab标题
            _tabContainer.SetTabTitle(0, "📋 可报名");
            _tabContainer.SetTabTitle(1, "⚔️ 进行中");
            _tabContainer.SetTabTitle(2, "🎯 我的比赛");
            _tabContainer.SetTabTitle(3, "📊 统计");
        }

        private Control CreateAvailableTab()
        {
            var scroll = new ScrollContainer();
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            scroll.AddChild(vbox);
            
            var title = new Label();
            title.Text = "可报名的锦标赛";
            title.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(title);
            
            var list = new VBoxContainer();
            list.Name = "TournamentList";
            list.AddThemeConstantOverride("separation", 8);
            vbox.AddChild(list);
            
            return scroll;
        }

        private Control CreateActiveTab()
        {
            var scroll = new ScrollContainer();
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            scroll.AddChild(vbox);
            
            var title = new Label();
            title.Text = "正在进行中的锦标赛";
            title.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(title);
            
            var list = new VBoxContainer();
            list.Name = "ActiveList";
            list.AddThemeConstantOverride("separation", 8);
            vbox.AddChild(list);
            
            return scroll;
        }

        private Control CreateMyTournamentsTab()
        {
            var scroll = new ScrollContainer();
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            scroll.AddChild(vbox);
            
            var title = new Label();
            title.Text = "我的锦标赛记录";
            title.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(title);
            
            var list = new VBoxContainer();
            list.Name = "MyTournamentsList";
            list.AddThemeConstantOverride("separation", 8);
            vbox.AddChild(list);
            
            return scroll;
        }

        private Control CreateStatisticsTab()
        {
            var scroll = new ScrollContainer();
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            scroll.AddChild(vbox);
            
            var title = new Label();
            title.Text = "锦标赛统计";
            title.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(title);
            
            // 统计信息容器
            var statsContainer = new VBoxContainer();
            statsContainer.Name = "StatsContainer";
            statsContainer.AddThemeConstantOverride("separation", 5);
            vbox.AddChild(statsContainer);
            
            return scroll;
        }

        private void RefreshData()
        {
            RefreshAvailableTournaments();
            RefreshActiveTournaments();
            RefreshMyTournaments();
            RefreshStatistics();
        }

        private void RefreshAvailableTournaments()
        {
            var list = _availableTab.GetNode<VBoxContainer>("TournamentList");
            
            // 清除旧内容
            foreach (var child in list.GetChildren())
            {
                child.QueueFree();
            }
            
            var tournaments = ArenaTournamentSystem.Instance.GetAvailableTournaments();
            
            if (tournaments.Count == 0)
            {
                var empty = new Label();
                empty.Text = "暂无可报名的锦标赛";
                empty.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                list.AddChild(empty);
                return;
            }
            
            foreach (var t in tournaments)
            {
                var card = CreateTournamentCard(t, true);
                list.AddChild(card);
            }
        }

        private void RefreshActiveTournaments()
        {
            var list = _activeTab.GetNode<VBoxContainer>("ActiveList");
            
            foreach (var child in list.GetChildren())
            {
                child.QueueFree();
            }
            
            var tournaments = ArenaTournamentSystem.Instance.GetActiveTournaments();
            
            if (tournaments.Count == 0)
            {
                var empty = new Label();
                empty.Text = "暂无进行中的锦标赛";
                empty.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                list.AddChild(empty);
                return;
            }
            
            foreach (var t in tournaments)
            {
                var card = CreateTournamentCard(t, false);
                list.AddChild(card);
            }
        }

        private void RefreshMyTournaments()
        {
            var list = _myTournamentsTab.GetNode<VBoxContainer>("MyTournamentsList");
            
            foreach (var child in list.GetChildren())
            {
                child.QueueFree();
            }
            
            // 获取玩家已参加的锦标赛
            // 这里需要实际玩家ID
            var empty = new Label();
            empty.Text = "我的比赛记录";
            list.AddChild(empty);
        }

        private void RefreshStatistics()
        {
            var container = _statisticsTab.GetNode<VBoxContainer>("StatsContainer");
            
            foreach (var child in container.GetChildren())
            {
                child.QueueFree();
            }
            
            var statsLabel = new Label();
            statsLabel.Text = "个人锦标赛统计";
            statsLabel.AddThemeFontSizeOverride("font_size", 16);
            container.AddChild(statsLabel);
            
            var infoLabel = new Label();
            infoLabel.Text = "参加锦标赛以积累统计数据";
            infoLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            container.AddChild(infoLabel);
        }

        private Control CreateTournamentCard(Tournament tournament, bool showRegister)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(0, 120);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 5);
            panel.AddChild(vbox);
            
            // 标题行
            var header = new HBoxContainer();
            vbox.AddChild(header);
            
            var nameLabel = new Label();
            nameLabel.Text = $"🏆 {tournament.tournamentName}";
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            header.AddChild(nameLabel);
            
            header.AddChild(new Control() { SetHExpand(ExpandMode.Expand) });
            
            var formatLabel = new Label();
            formatLabel.Text = GetFormatName(tournament.format);
            formatLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.2f));
            header.AddChild(formatLabel);
            
            // 描述
            var descLabel = new Label();
            descLabel.Text = tournament.description;
            descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            vbox.AddChild(descLabel);
            
            // 信息行
            var infoRow = new HBoxContainer();
            vbox.AddChild(infoRow);
            
            var playersLabel = new Label();
            playersLabel.Text = $"👥 {tournament.currentPlayerCount}/{tournament.maxPlayers}";
            infoRow.AddChild(playersLabel);
            
            infoRow.AddChild(new Control() { SetHExpand(ExpandMode.Expand) });
            
            var prizeLabel = new Label();
            prizeLabel.Text = $"💰 奖池: {tournament.prizePool}";
            infoRow.AddChild(prizeLabel);
            
            var feeLabel = new Label();
            feeLabel.Text = $"🎫 报名费: {tournament.entryFee}";
            infoRow.AddChild(feeLabel);
            
            // 状态/报名按钮
            var actionRow = new HBoxContainer();
            vbox.AddChild(actionRow);
            
            var statusLabel = new Label();
            if (tournament.status == TournamentStatus.Pending)
            {
                var timeLeft = tournament.registrationEnd - DateTime.Now;
                statusLabel.Text = $"⏰ 报名截止: {timeLeft.Minutes}分 {timeLeft.Seconds}秒";
            }
            else if (tournament.status == TournamentStatus.Active)
            {
                statusLabel.Text = $"🔥 进行中 - 第{tournament.currentRound}轮";
            }
            actionRow.AddChild(statusLabel);
            
            actionRow.AddChild(new Control() { SetHExpand(ExpandMode.Expand) });
            
            if (showRegister && tournament.status == TournamentStatus.Pending)
            {
                var registerBtn = new Button();
                registerBtn.Text = "📝 报名";
                registerBtn.Pressed += () => OnRegisterPressed(tournament);
                actionRow.AddChild(registerBtn);
            }
            
            var viewBtn = new Button();
            viewBtn.Text = "👁️ 查看详情";
            viewBtn.Pressed += () => OnViewDetailsPressed(tournament);
            actionRow.AddChild(viewBtn);
            
            return panel;
        }

        private string GetFormatName(TournamentFormat format)
        {
            return format switch
            {
                TournamentFormat.SingleElimination => "单败淘汰",
                TournamentFormat.DoubleElimination => "双败淘汰",
                TournamentFormat.RoundRobin => "循环赛",
                TournamentFormat.SwissSystem => "瑞士制",
                _ => format.ToString()
            };
        }

        private void OnRegisterPressed(Tournament tournament)
        {
            // 实际实现需要玩家ID
            // ArenaTournamentSystem.Instance.RegisterPlayer(tournament.tournamentId, playerId, playerName);
            GD.Print($"[ArenaTournamentUI] 报名锦标赛: {tournament.tournamentName}");
            RefreshData();
        }

        private void OnViewDetailsPressed(Tournament tournament)
        {
            _selectedTournament = tournament;
            GD.Print($"[ArenaTournamentUI] 查看锦标赛详情: {tournament.tournamentName}");
        }

        public void Toggle()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                RefreshData();
            }
        }
    }
}
