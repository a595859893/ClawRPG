using Godot;
using System;
using System.Collections.Generic;

public class PetBattleArenaUI : Control
{
    private PetBattleArenaSystem _system;
    
    // UI Elements
    private Label _titleLabel;
    private Label _statsLabel;
    private Button _battleButton;
    private Button _autoBattleButton;
    private Button _closeButton;
    
    // Arena list
    private VBoxContainer _arenaList;
    private ScrollContainer _arenaScroll;
    
    // Current battle display
    private Label _battleStatusLabel;
    private Label _playerPetLabel;
    private Label _enemyPetLabel;
    private Label _scoreLabel;
    private Label _roundLabel;
    private ProgressBar _playerHealthBar;
    private ProgressBar _enemyHealthBar;
    
    // Results display
    private Label _resultLabel;
    private Label _rewardLabel;
    private Button _claimRewardButton;
    
    // Stats panel
    private VBoxContainer _statsPanel;
    private Label _totalBattlesLabel;
    private Label _winsLabel;
    private Label _lossesLabel;
    private Label _winRateLabel;
    private Label _streakLabel;
    private Label _rankLabel;
    private Label _rankingPointsLabel;
    
    // Selected arena
    private int _selectedArenaIndex = 0;
    private PetBattleArenaSystem.ArenaType _selectedArena = PetBattleArenaSystem.ArenaType.TrainingGround;
    
    // Pet selection
    private OptionButton _petSelector;
    
    public override void _Ready()
    {
        _system = GetNode<PetBattleArenaSystem>("/root/Main/PetBattleArenaSystem");
        
        SetupUI();
        ConnectSignals();
        RefreshDisplay();
    }
    
    private void SetupUI()
    {
        // Main container
        var mainContainer = new HBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainContainer.MarginLeft = 50;
        mainContainer.MarginTop = 50;
        mainContainer.MarginRight = -50;
        mainContainer.MarginBottom = -50;
        AddChild(mainContainer);
        
        // Left panel - Arena selection and stats
        var leftPanel = new VBoxContainer();
        leftPanel.CustomMinimumSize = new Vector2(300, 0);
        mainContainer.AddChild(leftPanel);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🐾 Pet Battle Arena";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.Modulate = new Color(1, 0.85f, 0);
        leftPanel.AddChild(_titleLabel);
        
        // Arena selection
        var arenaSection = new VBoxContainer();
        leftPanel.AddChild(arenaSection);
        
        var arenaTitle = new Label();
        arenaTitle.Text = "Select Arena:";
        arenaTitle.Modulate = new Color(0.7f, 0.9f, 1f);
        arenaSection.AddChild(arenaTitle);
        
        _arenaScroll = new ScrollContainer();
        _arenaScroll.CustomMinimumSize = new Vector2(280, 150);
        arenaSection.AddChild(_arenaScroll);
        
        _arenaList = new VBoxContainer();
        _arenaScroll.AddChild(_arenaList);
        
        // Pet selector
        var petSection = new VBoxContainer();
        leftPanel.AddChild(petSection);
        
        var petTitle = new Label();
        petTitle.Text = "Select Pet:";
        petTitle.Modulate = new Color(0.7f, 0.9f, 1f);
        petSection.AddChild(petTitle);
        
        _petSelector = new OptionButton();
        _petSelector.CustomMinimumSize = new Vector2(280, 30);
        petSection.AddChild(_petSelector);
        
        // Battle buttons
        var buttonSection = new HBoxContainer();
        leftPanel.AddChild(buttonSection);
        
        _battleButton = new Button();
        _battleButton.Text = "⚔️ Battle";
        _battleButton.CustomMinimumSize = new Vector2(130, 40);
        _battleButton.Connect("pressed", this, nameof(OnBattlePressed));
        buttonSection.AddChild(_battleButton);
        
        _autoBattleButton = new Button();
        _autoBattleButton.Text = "⚡ Auto";
        _autoBattleButton.CustomMinimumSize = new Vector2(130, 40);
        _autoBattleButton.Connect("pressed", this, nameof(OnAutoBattlePressed));
        buttonSection.AddChild(_autoBattleButton);
        
        // Stats panel
        var statsSection = new VBoxContainer();
        leftPanel.AddChild(statsSection);
        
        var statsTitle = new Label();
        statsTitle.Text = "📊 Battle Statistics";
        statsTitle.Modulate = new Color(0.7f, 0.9f, 1f);
        statsSection.AddChild(statsTitle);
        
        _totalBattlesLabel = new Label();
        _totalBattlesLabel.Text = "Total Battles: 0";
        statsSection.AddChild(_totalBattlesLabel);
        
        _winsLabel = new Label();
        _winsLabel.Text = "Wins: 0";
        _winsLabel.Modulate = new Color(0.3f, 0.9f, 0.3f);
        statsSection.AddChild(_winsLabel);
        
        _lossesLabel = new Label();
        _lossesLabel.Text = "Losses: 0";
        _lossesLabel.Modulate = new Color(0.9f, 0.3f, 0.3f);
        statsSection.AddChild(_lossesLabel);
        
        _winRateLabel = new Label();
        _winRateLabel.Text = "Win Rate: 0%";
        statsSection.AddChild(_winRateLabel);
        
        _streakLabel = new Label();
        _streakLabel.Text = "Current Streak: 0";
        _streakLabel.Modulate = new Color(1f, 0.85f, 0);
        statsSection.AddChild(_streakLabel);
        
        _rankLabel = new Label();
        _rankLabel.Text = "Rank: 500";
        _rankLabel.Modulate = new Color(0.7f, 0.9f, 1f);
        statsSection.AddChild(_rankLabel);
        
        _rankingPointsLabel = new Label();
        _rankingPointsLabel.Text = "Points: 0";
        statsSection.AddChild(_rankingPointsLabel);
        
        // Right panel - Battle display
        var rightPanel = new VBoxContainer();
        mainContainer.AddChild(rightPanel);
        
        // Battle status
        var battleTitle = new Label();
        battleTitle.Text = "⚔️ Battle Arena";
        battleTitle.Align = Label.AlignEnum.Center;
        battleTitle.Modulate = new Color(1, 0.85f, 0);
        rightPanel.AddChild(battleTitle);
        
        // Battle field
        var battleField = new HBoxContainer();
        rightPanel.AddChild(battleField);
        
        // Player side
        var playerSide = new VBoxContainer();
        playerSide.Alignment = BoxContainer.AlignMode.Center;
        battleField.AddChild(playerSide);
        
        var playerTitle = new Label();
        playerTitle.Text = "🐾 Your Pet";
        playerTitle.Align = Label.AlignEnum.Center;
        playerTitle.Modulate = new Color(0.3f, 0.9f, 0.3f);
        playerSide.AddChild(playerTitle);
        
        _playerPetLabel = new Label();
        _playerPetLabel.Text = "No Pet Selected";
        _playerPetLabel.Align = Label.AlignEnum.Center;
        playerSide.AddChild(_playerPetLabel);
        
        _playerHealthBar = new ProgressBar();
        _playerHealthBar.CustomMinimumSize = new Vector2(150, 20);
        _playerHealthBar.MaxValue = 100;
        _playerHealthBar.Value = 100;
        playerSide.AddChild(_playerHealthBar);
        
        // VS label
        var vsLabel = new Label();
        vsLabel.Text = "VS";
        vsLabel.Align = Label.AlignEnum.Center;
        vsLabel.Modulate = new Color(1, 0.5f, 0);
        vsLabel.CustomMinimumSize = new Vector2(50, 0);
        battleField.AddChild(vsLabel);
        
        // Enemy side
        var enemySide = new VBoxContainer();
        enemySide.Alignment = BoxContainer.AlignMode.Center;
        battleField.AddChild(enemySide);
        
        var enemyTitle = new Label();
        enemyTitle.Text = "👹 Enemy";
        enemyTitle.Align = Label.AlignEnum.Center;
        enemyTitle.Modulate = new Color(0.9f, 0.3f, 0.3f);
        enemySide.AddChild(enemyTitle);
        
        _enemyPetLabel = new Label();
        _enemyPetLabel.Text = "Waiting...";
        _enemyPetLabel.Align = Label.AlignEnum.Center;
        enemySide.AddChild(_enemyPetLabel);
        
        _enemyHealthBar = new ProgressBar();
        _enemyHealthBar.CustomMinimumSize = new Vector2(150, 20);
        _enemyHealthBar.MaxValue = 100;
        _enemyHealthBar.Value = 100;
        enemySide.AddChild(_enemyHealthBar);
        
        // Score display
        var scoreContainer = new HBoxContainer();
        scoreContainer.Alignment = BoxContainer.AlignMode.Center;
        rightPanel.AddChild(scoreContainer);
        
        _scoreLabel = new Label();
        _scoreLabel.Text = "0 - 0";
        _scoreLabel.Align = Label.AlignEnum.Center;
        _scoreLabel.Modulate = new Color(1, 0.85f, 0);
        _scoreLabel.CustomMinimumSize = new Vector2(200, 0);
        scoreContainer.AddChild(_scoreLabel);
        
        _roundLabel = new Label();
        _roundLabel.Text = "Round: 0/5";
        _roundLabel.Align = Label.AlignEnum.Center;
        scoreContainer.AddChild(_roundLabel);
        
        // Result display
        _resultLabel = new Label();
        _resultLabel.Text = "";
        _resultLabel.Align = Label.AlignEnum.Center;
        _resultLabel.Modulate = new Color(1, 0.85f, 0);
        _resultLabel.CustomMinimumSize = new Vector2(400, 30);
        rightPanel.AddChild(_resultLabel);
        
        _rewardLabel = new Label();
        _rewardLabel.Text = "";
        _rewardLabel.Align = Label.AlignEnum.Center;
        rightPanel.AddChild(_rewardLabel);
        
        // Claim reward button
        _claimRewardButton = new Button();
        _claimRewardButton.Text = "🎁 Claim Reward";
        _claimRewardButton.Visible = false;
        _claimRewardButton.Connect("pressed", this, nameof(OnClaimRewardPressed));
        rightPanel.AddChild(_claimRewardButton);
        
        // Close button
        _closeButton = new Button();
        _closeButton.Text = "✖ Close";
        _closeButton.CustomMinimumSize = new Vector2(100, 40);
        _closeButton.Connect("pressed", this, nameof(OnClosePressed));
        rightPanel.AddChild(_closeButton);
        
        RefreshArenaList();
        RefreshPetSelector();
    }
    
    private void ConnectSignals()
    {
        if (_system != null)
        {
            _system.Connect(nameof(PetBattleArenaSystem.BattleStarted), this, nameof(OnBattleStarted));
            _system.Connect(nameof(PetBattleArenaSystem.BattleRoundComplete), this, nameof(OnBattleRoundComplete));
            _system.Connect(nameof(PetBattleArenaSystem.BattleCompleted), this, nameof(OnBattleCompleted));
            _system.Connect(nameof(PetBattleArenaSystem.RankUpdated), this, nameof(OnRankUpdated));
        }
    }
    
    private void RefreshArenaList()
    {
        // Clear existing
        foreach (Node child in _arenaList.GetChildren())
        {
            child.QueueFree();
        }
        
        var arenas = _system.GetArenaTypes();
        
        foreach (var arena in arenas)
        {
            var arenaButton = new Button();
            arenaButton.Text = $"{( (bool)arena["unlocked"] ? "✅" : "🔒" )} {(string)arena["name"]} (Lv.{arena["difficulty"]})";
            arenaButton.CustomMinimumSize = new Vector2(260, 35);
            
            if ((bool)arena["unlocked"])
            {
                arenaButton.Modulate = new Color(0.9f, 0.9f, 0.9f);
            }
            else
            {
                arenaButton.Modulate = new Color(0.5f, 0.5f, 0.5f);
            }
            
            int arenaIdx = (int)arena["index"];
            arenaButton.Connect("pressed", this, nameof(OnArenaSelected), new Godot.Collections.Array { arenaIdx });
            
            _arenaList.AddChild(arenaButton);
        }
    }
    
    private void RefreshPetSelector()
    {
        _petSelector.Clear();
        
        // Get available pets from PetSystem
        var petSystem = GetNode<PetSystem>("/root/Main/PetSystem");
        
        if (petSystem != null)
        {
            var pets = petSystem.GetOwnedPets();
            
            if (pets != null && pets.Count > 0)
            {
                int index = 0;
                foreach (var pet in pets)
                {
                    _petSelector.AddItem($"{pet.Name} (Lv.{pet.Level})", index);
                    index++;
                }
            }
            else
            {
                _petSelector.AddItem("No Pets Available", 0);
            }
        }
        else
        {
            _petSelector.AddItem("No Pets Available", 0);
        }
    }
    
    private void RefreshDisplay()
    {
        var info = _system.GetArenaInfo();
        
        _totalBattlesLabel.Text = $"Total Battles: {info["totalBattles"]}";
        _winsLabel.Text = $"Wins: {info["wins"]}";
        _lossesLabel.Text = $"Losses: {info["losses"]}";
        _winRateLabel.Text = $"Win Rate: {info["winRate"]:F1}%";
        _streakLabel.Text = $"Current Streak: {info["currentStreak"]}";
        _rankLabel.Text = $"Rank: {info["rank"]}";
        _rankingPointsLabel.Text = $"Points: {info["rankingPoints"]}";
        
        var battle = _system.GetCurrentBattle();
        _scoreLabel.Text = $"{battle["playerScore"]} - {battle["enemyScore"]}";
        _roundLabel.Text = $"Round: {battle["roundsPlayed"]}/{battle["maxRounds"]}";
        
        if (battle["playerPet"].ToString() != "None")
        {
            _playerPetLabel.Text = battle["playerPet"].ToString();
        }
        
        if (battle["enemyPet"].ToString() != "None")
        {
            _enemyPetLabel.Text = battle["enemyPet"].ToString();
        }
    }
    
    private void OnArenaSelected(int arenaIndex)
    {
        _selectedArenaIndex = arenaIndex;
        _selectedArena = (PetBattleArenaSystem.ArenaType)arenaIndex;
        RefreshArenaList();
    }
    
    private void OnBattlePressed()
    {
        if (_system == null)
            return;
            
        var petSystem = GetNode<PetSystem>("/root/Main/PetSystem");
        
        if (petSystem == null)
            return;
            
        var pets = petSystem.GetOwnedPets();
        
        if (pets == null || pets.Count == 0)
            return;
            
        int selectedIndex = _petSelector.GetSelectedId();
        
        if (selectedIndex >= 0 && selectedIndex < pets.Count)
        {
            var selectedPet = pets[selectedIndex];
            
            if (_system.StartBattle(selectedPet, _selectedArena))
            {
                // Play first round
                _system.PlayRound();
            }
        }
        
        RefreshDisplay();
    }
    
    private void OnAutoBattlePressed()
    {
        if (_system == null)
            return;
            
        var petSystem = GetNode<PetSystem>("/root/Main/PetSystem");
        
        if (petSystem == null)
            return;
            
        var pets = petSystem.GetOwnedPets();
        
        if (pets == null || pets.Count == 0)
            return;
            
        int selectedIndex = _petSelector.GetSelectedId();
        
        if (selectedIndex >= 0 && selectedIndex < pets.Count)
        {
            var selectedPet = pets[selectedIndex];
            
            if (_system.StartBattle(selectedPet, _selectedArena))
            {
                _system.AutoPlayBattle();
            }
        }
        
        RefreshDisplay();
    }
    
    private void OnClaimRewardPressed()
    {
        // Add rewards to player
        var economySystem = GetNode<EconomySystem>("/root/Main/EconomySystem");
        
        if (economySystem != null)
        {
            economySystem.AddGold(_system.GetArenaInfo()["wins"].GetHashCode()); // Placeholder
        }
        
        _system.ResetBattle();
        
        _resultLabel.Text = "";
        _rewardLabel.Text = "";
        _claimRewardButton.Visible = false;
        
        RefreshDisplay();
    }
    
    private void OnClosePressed()
    {
        _system?.ResetBattle();
        Visible = false;
    }
    
    private void OnBattleStarted(PetInstance playerPet, PetInstance enemyPet)
    {
        _playerPetLabel.Text = playerPet?.Name ?? "Unknown";
        _enemyPetLabel.Text = enemyPet?.Name ?? "Unknown";
        _resultLabel.Text = "";
        _rewardLabel.Text = "";
        _claimRewardButton.Visible = false;
    }
    
    private void OnBattleRoundComplete(int round, int playerScore, int enemyScore)
    {
        _scoreLabel.Text = $"{playerScore} - {enemyScore}";
        _roundLabel.Text = $"Round: {round}/5";
    }
    
    private void OnBattleCompleted(bool victory, int goldReward, int expReward)
    {
        if (victory)
        {
            _resultLabel.Text = "🎉 VICTORY!";
            _resultLabel.Modulate = new Color(0.3f, 0.9f, 0.3f);
            _rewardLabel.Text = $"+{goldReward} Gold | +{expReward} EXP";
            _rewardLabel.Modulate = new Color(1, 0.85f, 0);
        }
        else
        {
            _resultLabel.Text = "💀 DEFEAT";
            _resultLabel.Modulate = new Color(0.9f, 0.3f, 0.3f);
            _rewardLabel.Text = $"+{goldReward / 2} Gold | +{expReward / 2} EXP";
        }
        
        _claimRewardButton.Visible = true;
    }
    
    private void OnRankUpdated(int newRank)
    {
        _rankLabel.Text = $"Rank: {newRank}";
        _rankLabel.Modulate = new Color(0.3f, 0.9f, 0.9f);
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Scancode == (int)KeyList.Escape)
            {
                OnClosePressed();
            }
        }
    }
}
