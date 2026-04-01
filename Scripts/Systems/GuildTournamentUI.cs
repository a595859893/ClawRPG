using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Guild Tournament UI - Display and manage guild tournaments
    /// </summary>
    public partial class GuildTournamentUI : Control
    {
        private static GuildTournamentUI _instance;
        public static GuildTournamentUI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GuildTournamentUI();
                return _instance;
            }
        }

        // UI Elements
        private Control _mainContainer;
        private Label _titleLabel;
        private Label _statusLabel;
        private Label _timerLabel;
        private TabContainer _tabContainer;
        
        // Tournament info
        private Label _tournamentNameLabel;
        private Label _tournamentTypeLabel;
        private Label _registeredCountLabel;
        private Label _prizePoolLabel;
        
        // Leaderboard
        private VBoxContainer _leaderboardContainer;
        
        // History
        private VBoxContainer _historyContainer;
        
        // Buttons
        private Button _registerButton;
        private Button _startTournamentButton;
        private Button _closeButton;
        
        // Tournament selection
        private OptionButton _tournamentTypeOption;
        private LineEdit _tournamentNameEdit;
        
        // State
        private bool _isVisible = false;
        
        public GuildTournamentUI()
        {
            _instance = this;
        }
        
        /// <summary>
        /// Initialize the UI
        /// </summary>
        public void Initialize()
        {
            if (_mainContainer != null)
                return;
                
            // Create main container
            _mainContainer = new Control();
            _mainContainer.Name = "GuildTournamentUI";
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainContainer.CustomMinimumSize = new Vector2(800, 600);
            _mainContainer.Visible = false;
            
            // Create background
            Panel background = new Panel();
            background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            background.Modulate = new Color(0, 0, 0, 0.85f);
            _mainContainer.AddChild(background);
            
            // Create title
            _titleLabel = new Label();
            _titleLabel.Text = "⚔️ 公会锦标赛";
            _titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f)); // Gold
            _titleLabel.Position = new Vector2(0, 20);
            _mainContainer.AddChild(_titleLabel);
            
            // Status label
            _statusLabel = new Label();
            _statusLabel.Text = "等待开始...";
            _statusLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _statusLabel.AddThemeFontSizeOverride("font_size", 18);
            _statusLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _statusLabel.Position = new Vector2(0, 60);
            _mainContainer.AddChild(_statusLabel);
            
            // Timer label
            _timerLabel = new Label();
            _timerLabel.Text = "";
            _timerLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _timerLabel.AddThemeFontSizeOverride("font_size", 24);
            _timerLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.3f));
            _timerLabel.Position = new Vector2(0, 90);
            _mainContainer.AddChild(_timerLabel);
            
            // Create tab container
            _tabContainer = new TabContainer();
            _tabContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _tabContainer.Position = new Vector2(20, 140);
            _tabContainer.CustomMinimumSize = new Vector2(760, 380);
            _mainContainer.AddChild(_tabContainer);
            
            // Create tabs
            CreateTournamentTab();
            CreateLeaderboardTab();
            CreateHistoryTab();
            
            // Create buttons
            CreateButtons();
            
            // Add to tree
            GetTree().Root.AddChild(_mainContainer);
            
            // Connect signals
            GuildTournamentSystem.Instance.OnStateChanged += OnStateChanged;
            GuildTournamentSystem.Instance.OnScoreUpdated += OnScoreUpdated;
            GuildTournamentSystem.Instance.OnTournamentComplete += OnTournamentComplete;
            
            GD.Print("[GuildTournamentUI] Initialized");
        }
        
        /// <summary>
        /// Create tournament setup tab
        /// </summary>
        private void CreateTournamentTab()
        {
            Control tab = new Control();
            tab.Name = "Tournament";
            
            // Tournament name input
            Label nameLabel = new Label();
            nameLabel.Text = "锦标赛名称:";
            nameLabel.Position = new Vector2(50, 30);
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            tab.AddChild(nameLabel);
            
            _tournamentNameEdit = new LineEdit();
            _tournamentNameEdit.Text = "公会争霸赛";
            _tournamentNameEdit.Position = new Vector2(180, 30);
            _tournamentNameEdit.CustomMinimumSize = new Vector2(300, 30);
            tab.AddChild(_tournamentNameEdit);
            
            // Tournament type selection
            Label typeLabel = new Label();
            typeLabel.Text = "比赛类型:";
            typeLabel.Position = new Vector2(50, 80);
            typeLabel.AddThemeFontSizeOverride("font_size", 16);
            tab.AddChild(typeLabel);
            
            _tournamentTypeOption = new OptionButton();
            _tournamentTypeOption.Position = new Vector2(180, 80);
            _tournamentTypeOption.CustomMinimumSize = new Vector2(200, 30);
            
            // Add tournament types
            string[] typeNames = { "死亡竞赛", "夺旗战", "生存赛", "Bossrush", "寻宝赛", "解谜挑战" };
            for (int i = 0; i < typeNames.Length; i++)
            {
                _tournamentTypeOption.AddItem(typeNames[i], i);
            }
            tab.AddChild(_tournamentTypeOption);
            
            // Start button
            _startTournamentButton = new Button();
            _startTournamentButton.Text = "开始锦标赛";
            _startTournamentButton.Position = new Vector2(420, 80);
            _startTournamentButton.CustomMinimumSize = new Vector2(150, 30);
            _startTournamentButton.Pressed += OnStartTournamentPressed;
            tab.AddChild(_startTournamentButton);
            
            // Tournament info
            _tournamentNameLabel = new Label();
            _tournamentNameLabel.Text = "当前锦标赛: -";
            _tournamentNameLabel.Position = new Vector2(50, 140);
            _tournamentNameLabel.AddThemeFontSizeOverride("font_size", 18);
            tab.AddChild(_tournamentNameLabel);
            
            _tournamentTypeLabel = new Label();
            _tournamentTypeLabel.Text = "类型: -";
            _tournamentTypeLabel.Position = new Vector2(50, 170);
            _tournamentTypeLabel.AddThemeFontSizeOverride("font_size", 16);
            tab.AddChild(_tournamentTypeLabel);
            
            _registeredCountLabel = new Label();
            _registeredCountLabel.Text = "已报名公会: 0/16";
            _registeredCountLabel.Position = new Vector2(50, 200);
            _registeredCountLabel.AddThemeFontSizeOverride("font_size", 16);
            tab.AddChild(_registeredCountLabel);
            
            _prizePoolLabel = new Label();
            _prizePoolLabel.Text = "奖金池: 0 金币";
            _prizePoolLabel.Position = new Vector2(50, 230);
            _prizePoolLabel.AddThemeFontSizeOverride("font_size", 16);
            _prizePoolLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
            tab.AddChild(_prizePoolLabel);
            
            // Register button
            _registerButton = new Button();
            _registerButton.Text = "报名参加";
            _registerButton.Position = new Vector2(50, 280);
            _registerButton.CustomMinimumSize = new Vector2(200, 40);
            _registerButton.Pressed += OnRegisterPressed;
            tab.AddChild(_registerButton);
            
            _tabContainer.AddChild(tab);
        }
        
        /// <summary>
        /// Create leaderboard tab
        /// </summary>
        private void CreateLeaderboardTab()
        {
            Control tab = new Control();
            tab.Name = "Leaderboard";
            
            // Header
            HBoxContainer header = new HBoxContainer();
            header.Position = new Vector2(20, 20);
            header.CustomMinimumSize = new Vector2(700, 30);
            
            Label rankHeader = new Label();
            rankHeader.Text = "排名";
            rankHeader.CustomMinimumSize = new Vector2(80, 30);
            rankHeader.AddThemeFontSizeOverride("font_size", 16);
            rankHeader.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
            header.AddChild(rankHeader);
            
            Label guildHeader = new Label();
            guildHeader.Text = "公会名称";
            guildHeader.CustomMinimumSize = new Vector2(300, 30);
            guildHeader.AddThemeFontSizeOverride("font_size", 16);
            guildHeader.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
            header.AddChild(guildHeader);
            
            Label scoreHeader = new Label();
            scoreHeader.Text = "得分";
            scoreHeader.CustomMinimumSize = new Vector2(150, 30);
            scoreHeader.AddThemeFontSizeOverride("font_size", 16);
            scoreHeader.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
            header.AddChild(scoreHeader);
            
            Label killsHeader = new Label();
            killsHeader.Text = "击杀/死亡";
            killsHeader.CustomMinimumSize = new Vector2(150, 30);
            killsHeader.AddThemeFontSizeOverride("font_size", 16);
            killsHeader.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
            header.AddChild(killsHeader);
            
            tab.AddChild(header);
            
            // Scroll container for leaderboard
            ScrollContainer scroll = new ScrollContainer();
            scroll.Position = new Vector2(20, 60);
            scroll.CustomMinimumSize = new Vector2(700, 280);
            tab.AddChild(scroll);
            
            _leaderboardContainer = new VBoxContainer();
            _leaderboardContainer.CustomMinimumSize = new Vector2(700, 280);
            scroll.AddChild(_leaderboardContainer);
            
            _tabContainer.AddChild(tab);
        }
        
        /// <summary>
        /// Create history tab
        /// </summary>
        private void CreateHistoryTab()
        {
            Control tab = new Control();
            tab.Name = "History";
            
            // Scroll container
            ScrollContainer scroll = new ScrollContainer();
            scroll.Position = new Vector2(20, 20);
            scroll.CustomMinimumSize = new Vector2(700, 320);
            tab.AddChild(scroll);
            
            _historyContainer = new VBoxContainer();
            _historyContainer.CustomMinimumSize = new Vector2(700, 320);
            scroll.AddChild(_historyContainer);
            
            _tabContainer.AddChild(tab);
        }
        
        /// <summary>
        /// Create buttons
        /// </summary>
        private void CreateButtons()
        {
            // Close button
            _closeButton = new Button();
            _closeButton.Text = "关闭 (ESC)";
            _closeButton.Position = new Vector2(600, 540);
            _closeButton.CustomMinimumSize = new Vector2(150, 40);
            _closeButton.Pressed += OnClosePressed;
            _mainContainer.AddChild(_closeButton);
        }
        
        /// <summary>
        /// Toggle UI visibility
        /// </summary>
        public void Toggle()
        {
            if (_mainContainer == null)
                Initialize();
                
            _isVisible = !_isVisible;
            _mainContainer.Visible = _isVisible;
            
            if (_isVisible)
            {
                UpdateUI();
            }
        }
        
        /// <summary>
        /// Show the UI
        /// </summary>
        public void Show()
        {
            if (_mainContainer == null)
                Initialize();
                
            _isVisible = true;
            _mainContainer.Visible = true;
            UpdateUI();
        }
        
        /// <summary>
        /// Hide the UI
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            if (_mainContainer != null)
                _mainContainer.Visible = false;
        }
        
        /// <summary>
        /// Update UI elements
        /// </summary>
        public void UpdateUI()
        {
            var tournament = GuildTournamentSystem.Instance.GetCurrentTournament();
            if (tournament == null)
                return;
                
            // Update status
            string[] stateNames = { "报名中", "准备中", "进行中", "已结束" };
            _statusLabel.Text = $"状态: {stateNames[(int)tournament.State]}";
            
            // Update timer
            float timeRemaining = GuildTournamentSystem.Instance.GetTimeRemaining();
            if (tournament.State == GuildTournamentSystem.TournamentState.InProgress ||
                tournament.State == GuildTournamentSystem.TournamentState.Registration ||
                tournament.State == GuildTournamentSystem.TournamentState.Preparation)
            {
                int minutes = (int)(timeRemaining / 60);
                int seconds = (int)(timeRemaining % 60);
                _timerLabel.Text = $"剩余时间: {minutes}:{seconds:D2}";
            }
            else
            {
                _timerLabel.Text = "";
            }
            
            // Update tournament info
            _tournamentNameLabel.Text = $"当前锦标赛: {tournament.Name}";
            
            string[] typeNames = { "死亡竞赛", "夺旗战", "生存赛", "Bossrush", "寻宝赛", "解谜挑战" };
            _tournamentTypeLabel.Text = $"类型: {typeNames[(int)tournament.Type]}";
            _registeredCountLabel.Text = $"已报名公会: {tournament.RegisteredGuilds.Count}/16";
            _prizePoolLabel.Text = $"奖金池: {tournament.RegisteredGuilds.Count * 1000} 金币";
            
            // Update button states
            bool canRegister = tournament.State == GuildTournamentSystem.TournamentState.Registration;
            _registerButton.Disabled = !canRegister;
            _startTournamentButton.Disabled = tournament.State != GuildTournamentSystem.TournamentState.Completed;
            
            // Update leaderboard
            UpdateLeaderboard();
            
            // Update history
            UpdateHistory();
        }
        
        /// <summary>
        /// Update leaderboard display
        /// </summary>
        private void UpdateLeaderboard()
        {
            // Clear existing
            foreach (Node child in _leaderboardContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var leaderboard = GuildTournamentSystem.Instance.GetLeaderboard();
            
            if (leaderboard.Count == 0)
            {
                Label empty = new Label();
                empty.Text = "暂无排名数据";
                empty.AddThemeFontSizeOverride("font_size", 18);
                empty.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                _leaderboardContainer.AddChild(empty);
                return;
            }
            
            foreach (var score in leaderboard)
            {
                HBoxContainer row = new HBoxContainer();
                row.CustomMinimumSize = new Vector2(700, 40);
                
                // Rank
                Label rank = new Label();
                rank.Text = $"#{score.Rank}";
                rank.CustomMinimumSize = new Vector2(80, 40);
                rank.AddThemeFontSizeOverride("font_size", 18);
                
                // Color by rank
                if (score.Rank == 1)
                    rank.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f)); // Gold
                else if (score.Rank == 2)
                    rank.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f)); // Silver
                else if (score.Rank == 3)
                    rank.AddThemeColorOverride("font_color", new Color(0.8f, 0.5f, 0.2f)); // Bronze
                else
                    rank.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
                    
                row.AddChild(rank);
                
                // Guild name
                Label guildName = new Label();
                guildName.Text = score.GuildName;
                guildName.CustomMinimumSize = new Vector2(300, 40);
                guildName.AddThemeFontSizeOverride("font_size", 16);
                row.AddChild(guildName);
                
                // Score
                Label scoreLabel = new Label();
                scoreLabel.Text = score.Score.ToString();
                scoreLabel.CustomMinimumSize = new Vector2(150, 40);
                scoreLabel.AddThemeFontSizeOverride("font_size", 16);
                scoreLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.3f));
                row.AddChild(scoreLabel);
                
                // Kills/Deaths
                Label kd = new Label();
                kd.Text = $"{score.Kills}/{score.Deaths}";
                kd.CustomMinimumSize = new Vector2(150, 40);
                kd.AddThemeFontSizeOverride("font_size", 16);
                row.AddChild(kd);
                
                _leaderboardContainer.AddChild(row);
            }
        }
        
        /// <summary>
        /// Update history display
        /// </summary>
        private void UpdateHistory()
        {
            // Clear existing
            foreach (Node child in _historyContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var history = GuildTournamentSystem.Instance.GetHistory();
            
            if (history.Count == 0)
            {
                Label empty = new Label();
                empty.Text = "暂无历史记录";
                empty.AddThemeFontSizeOverride("font_size", 18);
                empty.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                _historyContainer.AddChild(empty);
                return;
            }
            
            foreach (var tournament in history)
            {
                HBoxContainer row = new HBoxContainer();
                row.CustomMinimumSize = new Vector2(700, 35);
                
                // Name
                Label name = new Label();
                name.Text = tournament.Name;
                name.CustomMinimumSize = new Vector2(300, 35);
                name.AddThemeFontSizeOverride("font_size", 14);
                row.AddChild(name);
                
                // Type
                string[] typeNames = { "死亡竞赛", "夺旗战", "生存赛", "Bossrush", "寻宝赛", "解谜挑战" };
                Label type = new Label();
                type.Text = typeNames[(int)tournament.Type];
                type.CustomMinimumSize = new Vector2(150, 35);
                type.AddThemeFontSizeOverride("font_size", 14);
                row.AddChild(type);
                
                // Winner
                Label winner = new Label();
                winner.Text = tournament.WinnerGuildName;
                winner.CustomMinimumSize = new Vector2(200, 35);
                winner.AddThemeFontSizeOverride("font_size", 14);
                winner.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
                row.AddChild(winner);
                
                _historyContainer.AddChild(row);
            }
        }
        
        /// <summary>
        /// Handle state change
        /// </summary>
        private void OnStateChanged(GuildTournamentSystem.TournamentState state)
        {
            UpdateUI();
        }
        
        /// <summary>
        /// Handle score update
        /// </summary>
        private void OnScoreUpdated(int guildId, GuildTournamentScore score)
        {
            UpdateUI();
        }
        
        /// <summary>
        /// Handle tournament complete
        /// </summary>
        private void OnTournamentComplete(TournamentData tournament)
        {
            UpdateUI();
        }
        
        /// <summary>
        /// Handle start tournament button pressed
        /// </summary>
        private void OnStartTournamentPressed()
        {
            string name = _tournamentNameEdit.Text;
            int typeIndex = _tournamentTypeOption.GetSelectedId();
            
            GuildTournamentSystem.TournamentType type = (GuildTournamentSystem.TournamentType)typeIndex;
            GuildTournamentSystem.Instance.StartTournament(type, name);
            
            GD.Print($"[GuildTournamentUI] Started tournament: {name}, Type: {type}");
        }
        
        /// <summary>
        /// Handle register button pressed
        /// </summary>
        private void OnRegisterPressed()
        {
            // Get current player guild (placeholder - would need to integrate with guild system)
            int guildId = 1; // Default guild
            string guildName = "玩家公会";
            
            bool success = GuildTournamentSystem.Instance.RegisterGuild(guildId, guildName);
            if (success)
            {
                GD.Print($"[GuildTournamentUI] Registered guild: {guildName}");
            }
        }
        
        /// <summary>
        /// Handle close button pressed
        /// </summary>
        private void OnClosePressed()
        {
            Hide();
        }
        
        /// <summary>
        /// Handle input
        /// </summary>
        public void _Input(InputEvent event_)
        {
            if (event_ is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Escape)
                {
                    Hide();
                }
                else if (keyEvent.Keycode == Key.T && !keyEvent.Echo)
                {
                    // Ctrl+T to toggle tournament UI
                    Toggle();
                }
            }
        }
    }
}
