using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace ClawRPG.Scripts.Systems.GuildTournamentBracket {
    /// <summary>
    /// 公会锦标赛赛程 UI
    /// </summary>
    public partial class GuildTournamentBracketUI : Control {
        // UI 组件
        private Label _titleLabel;
        private TabContainer _tabContainer;
        
        // Overview 标签页
        private VBoxContainer _overviewTab;
        private Label _tournamentStatusLabel;
        private Label _currentRoundLabel;
        private Label _participatingGuildsLabel;
        private Label _championLabel;
        private Button _startTournamentButton;
        private Button _resetButton;
        
        // Bracket 标签页
        private VBoxContainer _bracketTab;
        private ScrollContainer _bracketScroll;
        private VBoxContainer _bracketContainer;
        
        // Rankings 标签页
        private VBoxContainer _rankingsTab;
        private VBoxContainer _rankingsContainer;
        
        // Statistics 标签页
        private VBoxContainer _statisticsTab;
        private Label _statsLabel;
        
        // 系统引用
        private GuildTournamentBracketSystem _bracketSystem;
        
        // 样式颜色
        private Color _primaryColor = new Color(0.2f, 0.6f, 1.0f);
        private Color _secondaryColor = new Color(1.0f, 0.8f, 0.2f);
        private Color _successColor = new Color(0.2f, 0.8f, 0.4f);
        private Color _dangerColor = new Color(0.9f, 0.3f, 0.3f);
        
        public override void _Ready() {
            _bracketSystem = GuildTournamentBracketSystem.Instance;
            
            SetupUI();
            CreateSampleTournament();
            RefreshUI();
        }
        
        private void SetupUI() {
            // 背景
            var bg = new ColorRect {
                Color = new Color(0.1f, 0.1f, 0.15f, 0.95f)
            };
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(bg);
            
            // 标题
            _titleLabel = new Label {
                Text = "🏆 Guild Tournament Bracket",
                Align = Label.AlignEnum.Center,
                Modulate = _primaryColor
            };
            _titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _titleLabel.Position = new Vector2(0, 10);
            _titleLabel.Size = new Vector2(1152, 40);
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            AddChild(_titleLabel);
            
            // 关闭按钮
            var closeBtn = new Button {
                Text = "✕",
                Size = new Vector2(40, 40)
            };
            closeBtn.Position = new Vector2(1100, 10);
            closeBtn.Pressed += () => Hide();
            AddChild(closeBtn);
            
            // TabContainer
            _tabContainer = new TabContainer {
                Position = new Vector2(50, 70),
                Size = new Vector2(1052, 550)
            };
            AddChild(_tabContainer);
            
            // 创建标签页
            CreateOverviewTab();
            CreateBracketTab();
            CreateRankingsTab();
            CreateStatisticsTab();
        }
        
        private void CreateOverviewTab() {
            _overviewTab = new VBoxContainer {
                Name = "Overview"
            };
            _overviewTab.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _overviewTab.AddThemeConstantOverride("separation", 20);
            _tabContainer.AddChild(_overviewTab);
            
            // 锦标赛状态
            _tournamentStatusLabel = new Label {
                Text = "Tournament Status: Registration",
                Align = Label.AlignEnum.Center
            };
            _tournamentStatusLabel.AddThemeFontSizeOverride("font_size", 24);
            _tournamentStatusLabel.Modulate = _primaryColor;
            _overviewTab.AddChild(_tournamentStatusLabel);
            
            // 当前轮次
            _currentRoundLabel = new Label {
                Text = "Current Round: 0 / 0",
                Align = Label.AlignEnum.Center
            };
            _currentRoundLabel.AddThemeFontSizeOverride("font_size", 20);
            _overviewTab.AddChild(_currentRoundLabel);
            
            // 参赛公会数
            _participatingGuildsLabel = new Label {
                Text = "Participating Guilds: 0",
                Align = Label.AlignEnum.Center
            };
            _participatingGuildsLabel.AddThemeFontSizeOverride("font_size", 18);
            _overviewTab.AddChild(_participatingGuildsLabel);
            
            // 冠军
            _championLabel = new Label {
                Text = "Champion: TBD",
                Align = Label.AlignEnum.Center
            };
            _championLabel.AddThemeFontSizeOverride("font_size", 22);
            _championLabel.Modulate = _secondaryColor;
            _overviewTab.AddChild(_championLabel);
            
            // 按钮容器
            var buttonContainer = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            buttonContainer.AddThemeConstantOverride("separation", 30);
            _overviewTab.AddChild(buttonContainer);
            
            // 开始锦标赛按钮
            _startTournamentButton = new Button {
                Text = "🚀 Start Tournament",
                Size = new Vector2(200, 50)
            };
            _startTournamentButton.Pressed += OnStartTournamentPressed;
            buttonContainer.AddChild(_startTournamentButton);
            
            // 重置按钮
            _resetButton = new Button {
                Text = "🔄 Reset Tournament",
                Size = new Vector2(200, 50)
            };
            _resetButton.Pressed += OnResetPressed;
            buttonContainer.AddChild(_resetButton);
            
            // 说明文本
            var helpLabel = new Label {
                Text = "Guild Tournament Bracket System\n" +
                       "• Register guilds during registration phase\n" +
                       "• Click 'Start Tournament' to generate bracket\n" +
                       "• Matches advance automatically when completed\n" +
                       "• Top teams win gold, experience, and tournament points!",
                Align = Label.AlignEnum.Center,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            helpLabel.AddThemeFontSizeOverride("font_size", 16);
            helpLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
            _overviewTab.AddChild(helpLabel);
        }
        
        private void CreateBracketTab() {
            _bracketTab = new VBoxContainer {
                Name = "Bracket"
            };
            _bracketTab.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _bracketTab.AddThemeConstantOverride("separation", 10);
            _tabContainer.AddChild(_bracketTab);
            
            var bracketTitle = new Label {
                Text = "Tournament Bracket",
                Align = Label.AlignEnum.Center
            };
            bracketTitle.AddThemeFontSizeOverride("font_size", 22);
            bracketTitle.Modulate = _primaryColor;
            _bracketTab.AddChild(bracketTitle);
            
            _bracketScroll = new ScrollContainer {
                Size = new Vector2(1052, 480)
            };
            _bracketScroll.SetHorizontalScrollMode(ScrollContainer.ScrollMode.Enabled);
            _bracketTab.AddChild(_bracketScroll);
            
            _bracketContainer = new VBoxContainer {
                Name = "BracketContainer"
            };
            _bracketContainer.AddThemeConstantOverride("separation", 5);
            _bracketScroll.AddChild(_bracketContainer);
        }
        
        private void CreateRankingsTab() {
            _rankingsTab = new VBoxContainer {
                Name = "Rankings"
            };
            _rankingsTab.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _rankingsTab.AddThemeConstantOverride("separation", 15);
            _tabContainer.AddChild(_rankingsTab);
            
            var rankingsTitle = new Label {
                Text = "Guild Rankings",
                Align = Label.AlignEnum.Center
            };
            rankingsTitle.AddThemeFontSizeOverride("font_size", 22);
            rankingsTitle.Modulate = _primaryColor;
            _rankingsTab.AddChild(rankingsTitle);
            
            // 表头
            var headerContainer = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            headerContainer.AddThemeConstantOverride("separation", 50);
            _rankingsTab.AddChild(headerContainer);
            
            var rankHeader = new Label { Text = "Rank", Size = new Vector2(80, 30) };
            var guildHeader = new Label { Text = "Guild", Size = new Vector2(300, 30) };
            var winsHeader = new Label { Text = "Wins", Size = new Vector2(100, 30) };
            var lossesHeader = new Label { Text = "Losses", Size = new Vector2(100, 30) };
            var pointsHeader = new Label { Text = "Points", Size = new Vector2(100, 30) };
            
            headerContainer.AddChild(rankHeader);
            headerContainer.AddChild(guildHeader);
            headerContainer.AddChild(winsHeader);
            headerContainer.AddChild(lossesHeader);
            headerContainer.AddChild(pointsHeader);
            
            _rankingsContainer = new VBoxContainer {
                Name = "RankingsContainer"
            };
            _rankingsTab.AddChild(_rankingsContainer);
        }
        
        private void CreateStatisticsTab() {
            _statisticsTab = new VBoxContainer {
                Name = "Statistics"
            };
            _statisticsTab.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _statisticsTab.AddThemeConstantOverride("separation", 20);
            _tabContainer.AddChild(_statisticsTab);
            
            var statsTitle = new Label {
                Text = "Tournament Statistics",
                Align = Label.AlignEnum.Center
            };
            statsTitle.AddThemeFontSizeOverride("font_size", 22);
            statsTitle.Modulate = _primaryColor;
            _statisticsTab.AddChild(statsTitle);
            
            _statsLabel = new Label {
                Text = "No statistics available",
                Align = Label.AlignEnum.Center,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            _statsLabel.AddThemeFontSizeOverride("font_size", 18);
            _statisticsTab.AddChild(_statsLabel);
        }
        
        private void CreateSampleTournament() {
            _bracketSystem.CreateTournament("sample_tournament_001", GuildTournamentBracketDatabase.TournamentFormat.SingleElimination);
            
            // 添加示例公会
            var sampleGuilds = new List<string> {
                "Dragon Knights",
                "Shadow Legion",
                "Phoenix Rising",
                "Iron Vanguard",
                "Storm Guardians",
                "Dark Eclipse",
                "Silver Wolves",
                "Golden Phoenix"
            };
            
            foreach (var guild in sampleGuilds) {
                _bracketSystem.RegisterGuild(guild, guild);
            }
        }
        
        private void OnStartTournamentPressed() {
            _bracketSystem.StartSeeding();
            RefreshUI();
        }
        
        private void OnResetPressed() {
            _bracketSystem.ResetTournament();
            CreateSampleTournament();
            RefreshUI();
        }
        
        private void RefreshUI() {
            var data = _bracketSystem.GetTournamentData();
            
            // 更新 Overview
            _tournamentStatusLabel.Text = $"Tournament Status: {data.CurrentPhase}";
            _currentRoundLabel.Text = $"Current Round: {data.CurrentRound} / {data.TotalRounds}";
            _participatingGuildsLabel.Text = $"Participating Guilds: {data.ParticipatingGuilds.Count}";
            _championLabel.Text = string.IsNullOrEmpty(data.ChampionGuildId) 
                ? "Champion: TBD" 
                : $"Champion: {data.ChampionGuildId}";
            
            // 更新 Bracket
            RefreshBracket();
            
            // 更新 Rankings
            RefreshRankings();
            
            // 更新 Statistics
            RefreshStatistics();
        }
        
        private void RefreshBracket() {
            // 清除现有内容
            foreach (var child in _bracketContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var matches = _bracketSystem.GetAllMatches();
            var currentRound = _bracketSystem.GetTournamentData().CurrentRound;
            
            // 按轮次分组显示
            int currentRoundNum = 1;
            while (true) {
                var roundMatches = matches.FindAll(m => m.Round == currentRoundNum);
                if (roundMatches.Count == 0) break;
                
                var roundLabel = new Label {
                    Text = GetRoundName(currentRoundNum),
                    Align = Label.AlignEnum.Center
                };
                roundLabel.AddThemeFontSizeOverride("font_size", 18);
                roundLabel.Modulate = currentRoundNum == currentRound ? _primaryColor : new Color(0.6f, 0.6f, 0.6f);
                _bracketContainer.AddChild(roundLabel);
                
                foreach (var match in roundMatches) {
                    var matchContainer = CreateMatchCard(match);
                    _bracketContainer.AddChild(matchContainer);
                }
                
                currentRoundNum++;
            }
        }
        
        private string GetRoundName(int round) {
            var totalRounds = _bracketSystem.GetTournamentData().TotalRounds;
            if (round == totalRounds) return "🏆 Finals";
            if (round == totalRounds - 1) return "🥈 Semi-Finals";
            if (round == totalRounds - 2) return "🥉 Quarter-Finals";
            return $"Round {round}";
        }
        
        private Control CreateMatchCard(BracketMatch match) {
            var container = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            container.AddThemeConstantOverride("separation", 10);
            
            // 左侧公会
            var guild1Label = new Label {
                Text = string.IsNullOrEmpty(match.Guild1Id) ? "TBD" : match.Guild1Id,
                Size = new Vector2(200, 30)
            };
            guild1Label.Modulate = match.Status == MatchStatus.Completed && match.WinnerId == match.Guild1Id 
                ? _successColor 
                : Colors.White;
            container.AddChild(guild1Label);
            
            // VS
            var vsLabel = new Label {
                Text = match.Status == MatchStatus.Pending ? "vs" : $"{match.Guild1Score} - {match.Guild2Score}",
                Size = new Vector2(80, 30),
                Align = Label.AlignEnum.Center
            };
            vsLabel.Modulate = _secondaryColor;
            container.AddChild(vsLabel);
            
            // 右侧公会
            var guild2Label = new Label {
                Text = string.IsNullOrEmpty(match.Guild2Id) ? "TBD" : match.Guild2Id,
                Size = new Vector2(200, 30)
            };
            guild2Label.Modulate = match.Status == MatchStatus.Completed && match.WinnerId == match.Guild2Id 
                ? _successColor 
                : Colors.White;
            container.AddChild(guild2Label);
            
            // 状态
            var statusLabel = new Label {
                Text = GetMatchStatusText(match.Status),
                Size = new Vector2(100, 30)
            };
            statusLabel.Modulate = GetMatchStatusColor(match.Status);
            container.AddChild(statusLabel);
            
            return container;
        }
        
        private string GetMatchStatusText(MatchStatus status) {
            switch (status) {
                case MatchStatus.Pending: return "⏳ Pending";
                case MatchStatus.Ready: return "⚔️ Ready";
                case MatchStatus.InProgress: return "🔥 In Progress";
                case MatchStatus.Completed: return "✅ Completed";
                case MatchStatus.Cancelled: return "❌ Cancelled";
                default: return "Unknown";
            }
        }
        
        private Color GetMatchStatusColor(MatchStatus status) {
            switch (status) {
                case MatchStatus.Pending: return new Color(0.6f, 0.6f, 0.6f);
                case MatchStatus.Ready: return _primaryColor;
                case MatchStatus.InProgress: return _dangerColor;
                case MatchStatus.Completed: return _successColor;
                case MatchStatus.Cancelled: return _dangerColor;
                default: return Colors.White;
            }
        }
        
        private void RefreshRankings() {
            // 清除现有内容
            foreach (var child in _rankingsContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var rankings = _bracketSystem.GetGuildRankings();
            
            int rank = 1;
            foreach (var guild in rankings) {
                var row = new HBoxContainer {
                    Alignment = BoxContainer.AlignmentMode.Center
                };
                row.AddThemeConstantOverride("separation", 50);
                
                var rankLabel = new Label {
                    Text = $"#{rank}",
                    Size = new Vector2(80, 30)
                };
                rankLabel.Modulate = GetRankColor(rank);
                
                var guildLabel = new Label {
                    Text = guild.GuildId,
                    Size = new Vector2(300, 30)
                };
                
                var winsLabel = new Label {
                    Text = guild.Wins.ToString(),
                    Size = new Vector2(100, 30)
                };
                winsLabel.Modulate = _successColor;
                
                var lossesLabel = new Label {
                    Text = guild.Losses.ToString(),
                    Size = new Vector2(100, 30)
                };
                lossesLabel.Modulate = _dangerColor;
                
                var pointsLabel = new Label {
                    Text = guild.Points.ToString(),
                    Size = new Vector2(100, 30)
                };
                pointsLabel.Modulate = _secondaryColor;
                
                row.AddChild(rankLabel);
                row.AddChild(guildLabel);
                row.AddChild(winsLabel);
                row.AddChild(lossesLabel);
                row.AddChild(pointsLabel);
                
                _rankingsContainer.AddChild(row);
                rank++;
            }
        }
        
        private Color GetRankColor(int rank) {
            switch (rank) {
                case 1: return _secondaryColor;
                case 2: return new Color(0.8f, 0.8f, 0.8f);
                case 3: return new Color(0.8f, 0.5f, 0.3f);
                default: return Colors.White;
            }
        }
        
        private void RefreshStatistics() {
            var stats = _bracketSystem.GetStatistics();
            
            var statsText = "📊 Tournament Statistics\n\n";
            statsText += $"Total Matches: {stats["TotalMatches"]}\n";
            statsText += $"Completed Matches: {stats["CompletedMatches"]}\n";
            statsText += $"Current Round: {stats["CurrentRound"]} / {stats["TotalRounds"]}\n";
            statsText += $"Participating Guilds: {stats["ParticipatingGuilds"]}\n";
            statsText += $"\n🏆 Champion: {stats["Champion"]}";
            
            _statsLabel.Text = statsText;
        }
        
        /// <summary>
        /// 切换显示
        /// </summary>
        public static void Toggle() {
            var ui = GetOrCreateUI();
            if (ui.Visible) {
                ui.Hide();
            } else {
                ui.Show();
            }
        }
        
        private static GuildTournamentBracketUI GetOrCreateUI() {
            var sceneTree = Engine.GetMainLoop();
            if (sceneTree == null) return null;
            
            var root = sceneTree.GetRoot();
            var existing = root.GetNodeOrNull<GuildTournamentBracketUI>("GuildTournamentBracketUI");
            
            if (existing != null) {
                return existing;
            }
            
            var ui = new GuildTournamentBracketUI {
                Name = "GuildTournamentBracketUI"
            };
            root.AddChild(ui);
            ui.Hide();
            return ui;
        }
    }
}
