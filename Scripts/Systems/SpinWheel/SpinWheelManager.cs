using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SpinWheelManager : BaseSystem
{
    [Export] private PackedScene spinWheelScene;
    private Control spinWheelUI;
    private Button spinButton;
    private Label resultLabel;
    private Label timerLabel;
    private Label spinCountLabel;
    
    private bool isSpinning = false;
    private float currentRotation = 0f;
    private float targetRotation = 0f;
    private float spinSpeed = 0f;
    private float deceleration = 0.98f;
    
    private readonly string[] rewardTypes = { "Gold", "Exp", "Item", "Buff", "Drop", "Special" };
    private readonly int[] rewardAmounts = { 100, 200, 500, 1000, 2000, 5000 };
    private readonly string[] rewardItems = { "HealthPotion", "ManaPotion", "EnhancementStone", "Gem", "Rune", "Equipment" };
    private readonly string[] buffTypes = { "Attack", "Defense", "Speed", "Critical", "LifeSteal", "Experience" };
    
    private int dailySpinCount = 0;
    private int maxDailySpins = 3;
    private DateTime lastSpinDate;
    private List<int> selectedRewards = new List<int>();
    private Random rng = new Random();
    
    // Player data keys
    private const string SPIN_COUNT_KEY = "spin_wheel_count";
    private const string LAST_SPIN_DATE_KEY = "spin_wheel_last_date";
    private const string TOTAL_SPINS_KEY = "spin_wheel_total";
    private const string TOTAL_WINS_KEY = "spin_wheel_wins";
    private const string BIGGEST_WIN_KEY = "spin_wheel_biggest";
    
    public override void _Ready()
    {
        SetupSpinWheelUI();
        LoadSpinWheelData();
        UpdateUI();
    }
    
    private void SetupSpinWheelUI()
    {
        // Create spin wheel UI
        spinWheelUI = new Control();
        spinWheelUI.Name = "SpinWheelUI";
        spinWheelUI.SetAnchor(Control.LayoutPreset.Center);
        spinWheelUI.SetOffset(new Vector2(-300, -250), new Vector2(300, 250));
        spinWheelUI.Visible = false;
        AddChild(spinWheelUI);
        
        // Background panel
        Panel background = new Panel();
        background.SetAnchor(Control.LayoutPreset.FullRect);
        background.Modulate = new Color(0, 0, 0, 0.8f);
        spinWheelUI.AddChild(background);
        
        // Title
        Label titleLabel = new Label();
        titleLabel.Text = "🎡 Daily Lucky Wheel";
        titleLabel.SetAnchor(Control.LayoutPreset.TopWide);
        titleLabel.SetOffset(new Vector2(0, 10), new Vector2(0, 50));
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 28);
        spinWheelUI.AddChild(titleLabel);
        
        // Spin button
        spinButton = new Button();
        spinButton.Text = "SPIN!";
        spinButton.SetAnchor(Control.LayoutPreset.BottomWide);
        spinButton.SetOffset(new Vector2(-100, -80), new Vector2(100, -20));
        spinButton.Pressed += OnSpinButtonPressed;
        spinWheelUI.AddChild(spinButton);
        
        // Result label
        resultLabel = new Label();
        resultLabel.Text = "Spin the wheel to win prizes!";
        resultLabel.SetAnchor(Control.LayoutPreset.BottomWide);
        resultLabel.SetOffset(new Vector2(-200, -120), new Vector2(200, -85));
        resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
        resultLabel.AddThemeFontSizeOverride("font_size", 20);
        spinWheelUI.AddChild(resultLabel);
        
        // Timer label
        timerLabel = new Label();
        timerLabel.Text = "Next spin: --:--";
        timerLabel.SetAnchor(Control.LayoutPreset.TopWide);
        timerLabel.SetOffset(new Vector2(0, 60), new Vector2(200, 85));
        timerLabel.HorizontalAlignment = HorizontalAlignment.Center;
        spinWheelUI.AddChild(timerLabel);
        
        // Spin count label
        spinCountLabel = new Label();
        spinCountLabel.Text = "Spins today: 0/3";
        spinCountLabel.SetAnchor(Control.LayoutPreset.TopWide);
        spinCountLabel.SetOffset(new Vector2(-200, 90), new Vector2(0, 115));
        spinCountLabel.HorizontalAlignment = HorizontalAlignment.Center;
        spinWheelUI.AddChild(spinCountLabel);
        
        // Close button
        Button closeButton = new Button();
        closeButton.Text = "X";
        closeButton.SetAnchor(Control.LayoutPreset.TopRight);
        closeButton.SetOffset(new Vector2(-40, 10), new Vector2(-10, 40));
        closeButton.Pressed += () => spinWheelUI.Visible = false;
        spinWheelUI.AddChild(closeButton);
        
        // Initialize wheel segments
        InitializeWheelSegments();
    }
    
    private void InitializeWheelSegments()
    {
        // 12 segments for the wheel
        selectedRewards.Clear();
        for (int i = 0; i < 12; i++)
        {
            selectedRewards.Add(rng.Next(rewardTypes.Length));
        }
    }
    
    private void LoadSpinWheelData()
    {
        // Load player data
        if (PlayerData.HasKey(SPIN_COUNT_KEY))
        {
            dailySpinCount = PlayerData.GetInt(SPIN_COUNT_KEY, 0);
        }
        
        if (PlayerData.HasKey(LAST_SPIN_DATE_KEY))
        {
            string lastDateStr = PlayerData.GetString(LAST_SPIN_DATE_KEY, "");
            if (DateTime.TryParse(lastDateStr, out DateTime lastDate))
            {
                lastSpinDate = lastDate;
                
                // Reset daily spins if it's a new day
                if (lastSpinDate.Date < DateTime.Now.Date)
                {
                    dailySpinCount = 0;
                    lastSpinDate = DateTime.Now;
                    SaveSpinWheelData();
                }
            }
        }
        else
        {
            lastSpinDate = DateTime.Now;
        }
        
        UpdateSpinTimer();
    }
    
    private void SaveSpinWheelData()
    {
        PlayerData.SetInt(SPIN_COUNT_KEY, dailySpinCount);
        PlayerData.SetString(LAST_SPIN_DATE_KEY, lastSpinDate.ToString("yyyy-MM-dd"));
        PlayerData.SaveGame();
    }
    
    private void UpdateSpinTimer()
    {
        DateTime now = DateTime.Now;
        DateTime tomorrow = now.Date.AddDays(1);
        TimeSpan timeUntilReset = tomorrow - now;
        
        timerLabel.Text = $"Resets in: {timeUntilReset.Hours:D2}:{timeUntilReset.Minutes:D2}:{timeUntilReset.Seconds:D2}";
    }
    
    private void UpdateUI()
    {
        spinCountLabel.Text = $"Spins today: {dailySpinCount}/{maxDailySpins}";
        
        if (dailySpinCount >= maxDailySpins)
        {
            spinButton.Disabled = true;
            spinButton.Text = "No spins left";
        }
        else
        {
            spinButton.Disabled = false;
            spinButton.Text = "SPIN!";
        }
    }
    
    private void OnSpinButtonPressed()
    {
        if (isSpinning || dailySpinCount >= maxDailySpins)
            return;
        
        StartSpin();
    }
    
    private void StartSpin()
    {
        isSpinning = true;
        spinButton.Disabled = true;
        resultLabel.Text = "Spinning...";
        
        // Random spin parameters
        targetRotation = currentRotation + 1440f + (float)(rng.NextDouble() * 720f); // 4-6 full rotations
        spinSpeed = 30f + (float)(rng.NextDouble() * 20f);
        
        // Increment spin count
        dailySpinCount++;
        lastSpinDate = DateTime.Now;
        SaveSpinWheelData();
        
        // Track total spins
        int totalSpins = PlayerData.GetInt(TOTAL_SPINS_KEY, 0) + 1;
        PlayerData.SetInt(TOTAL_SPINS_KEY, totalSpins);
    }
    
    public override void _Process(float delta)
    {
        if (isSpinning)
        {
            // Apply rotation with deceleration
            currentRotation += spinSpeed * delta;
            spinSpeed *= deceleration;
            
            // Check if spin is complete
            if (currentRotation >= targetRotation || spinSpeed < 0.5f)
            {
                FinishSpin();
            }
            
            UpdateSpinTimer();
        }
    }
    
    private void FinishSpin()
    {
        isSpinning = false;
        currentRotation = targetRotation;
        
        // Calculate winning segment
        float segmentAngle = 360f / 12f;
        float normalizedRotation = currentRotation % 360f;
        int winningSegment = (int)((360f - normalizedRotation) / segmentAngle) % 12;
        int rewardType = selectedRewards[winningSegment];
        
        // Generate and display reward
        string rewardText = GenerateReward(rewardType);
        resultLabel.Text = rewardText;
        
        // Update UI
        UpdateUI();
        
        // Track wins
        int totalWins = PlayerData.GetInt(TOTAL_WINS_KEY, 0) + 1;
        PlayerData.SetInt(TOTAL_WINS_KEY, totalWins);
        
        // Show celebration effect
        ShowRewardEffect(rewardType);
    }
    
    private string GenerateReward(int rewardType)
    {
        switch (rewardType)
        {
            case 0: // Gold
                int goldAmount = rewardAmounts[rng.Next(rewardAmounts.Length)];
                PlayerData.AddGold(goldAmount);
                return $"🎉 Won {goldAmount} Gold!";
                
            case 1: // Exp
                int expAmount = rewardAmounts[rng.Next(rewardAmounts.Length)] * 10;
                PlayerData.AddExp(expAmount);
                return $"🎉 Won {expAmount} Experience!";
                
            case 2: // Item
                string item = rewardItems[rng.Next(rewardItems.Length)];
                PlayerData.AddItem(item, 1);
                return $"🎉 Won {item}!";
                
            case 3: // Buff
                string buff = buffTypes[rng.Next(buffTypes.Length)];
                int buffDuration = (rng.Next(5) + 1) * 60; // 5-30 minutes
                ActivateBuff(buff, buffDuration);
                return $"🎉 Won {buff} Buff ({buffDuration} min)!";
                
            case 4: // Drop bonus
                int dropBonus = (rng.Next(5) + 1) * 10;
                PlayerData.SetFloat("drop_bonus", PlayerData.GetFloat("drop_bonus", 0f) + dropBonus);
                return $"🎉 Won {dropBonus}% Drop Bonus!";
                
            case 5: // Special - Big prize
                string[] specialRewards = { "Legendary Chest", "Rare Gem", "Epic Equipment", "大量金币" };
                string special = specialRewards[rng.Next(specialRewards.Length)];
                
                if (special == "大量金币")
                {
                    int bigGold = 10000;
                    PlayerData.AddGold(bigGold);
                    
                    // Track biggest win
                    int biggestWin = PlayerData.GetInt(BIGGEST_WIN_KEY, 0);
                    if (bigGold > biggestWin)
                    {
                        PlayerData.SetInt(BIGGEST_WIN_KEY, bigGold);
                    }
                }
                else if (special == "Legendary Chest" || special == "Rare Gem" || special == "Epic Equipment")
                {
                    PlayerData.AddItem(special, 1);
                }
                
                return $"⭐⭐⭐ SPECIAL: {special}! ⭐⭐⭐";
                
            default:
                return "Try again!";
        }
    }
    
    private void ActivateBuff(string buffType, int durationMinutes)
    {
        float buffValue = 0f;
        
        switch (buffType)
        {
            case "Attack":
                buffValue = 0.1f;
                PlayerData.SetFloat("attack_buff", buffValue);
                break;
            case "Defense":
                buffValue = 0.1f;
                PlayerData.SetFloat("defense_buff", buffValue);
                break;
            case "Speed":
                buffValue = 0.15f;
                PlayerData.SetFloat("speed_buff", buffValue);
                break;
            case "Critical":
                buffValue = 0.05f;
                PlayerData.SetFloat("crit_buff", buffValue);
                break;
            case "LifeSteal":
                buffValue = 0.03f;
                PlayerData.SetFloat("lifesteal_buff", buffValue);
                break;
            case "Experience":
                PlayerData.SetFloat("exp_buff", 0.2f);
                break;
        }
        
        // Buff will expire after duration (simplified - would need timer in full implementation)
        GD.Print($"Activated {buffType} buff for {durationMinutes} minutes: +{buffValue * 100}%");
    }
    
    private void ShowRewardEffect(int rewardType)
    {
        // Visual feedback based on reward type
        Color flashColor;
        
        switch (rewardType)
        {
            case 5: // Special
                flashColor = new Color(1f, 0.84f, 0f); // Gold
                break;
            case 3: // Buff
                flashColor = new Color(0.5f, 1f, 0.5f); // Green
                break;
            case 2: // Item
                flashColor = new Color(0.5f, 0.5f, 1f); // Blue
                break;
            default:
                flashColor = new Color(1f, 1f, 1f); // White
                break;
        }
        
        // Flash effect would be applied here
        GD.Print($"Reward effect: {flashColor}");
    }
    
    public void ToggleSpinWheel()
    {
        spinWheelUI.Visible = !spinWheelUI.Visible;
        
        if (spinWheelUI.Visible)
        {
            LoadSpinWheelData();
            UpdateUI();
            UpdateSpinTimer();
        }
    }
    
    // Key binding: L for Lucky Wheel
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.L)
        {
            ToggleSpinWheel();
        }
    }
}
