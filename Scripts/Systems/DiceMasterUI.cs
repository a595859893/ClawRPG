using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 骰子大师UI - 骰子游戏界面
/// </summary>
public partial class DiceMasterUI : Control
{
    private Label titleLabel;
    private Label diamondsLabel;
    private Label statsLabel;
    private Label buffLabel;
    private Label historyLabel;
    private VBoxContainer diceButtons;
    private Button rollButton;
    private Button closeButton;
    private Label resultLabel;
    private ProgressBar buffProgressBar;
    
    private bool isVisible = false;
    
    public override void _Ready()
    {
        // Create UI
        CreateUI();
        
        // Connect signals
        if (DiceMasterSystem.Instance != null)
        {
            DiceMasterSystem.Instance.DiceRolled += OnDiceRolled;
        }
        
        // Hide initially
        Hide();
    }
    
    private void CreateUI()
    {
        // Main panel
        Panel panel = new Panel();
        panel.RectMinSize = new Vector2(500, 600);
        panel.SetAnchorsPreset(Control.LayoutPreset.Center);
        AddChild(panel);
        
        VBoxContainer mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(mainVBox);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "🎲 Dice Master";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);
        
        // Diamonds
        HBoxContainer diamondsBox = new HBoxContainer();
        mainVBox.AddChild(diamondsBox);
        
        Label diamondsTitle = new Label();
        diamondsTitle.Text = "💎 Diamonds: ";
        diamondsBox.AddChild(diamondsTitle);
        
        diamondsLabel = new Label();
        diamondsLabel.Text = "10";
        diamondsBox.AddChild(diamondsLabel);
        
        Button addDiamondBtn = new Button();
        addDiamondBtn.Text = "+";
        addDiamondBtn.RectMinSize = new Vector2(30, 30);
        addDiamondBtn.Pressed += OnAddDiamondPressed;
        diamondsBox.AddChild(addDiamondBtn);
        
        // Result display
        resultLabel = new Label();
        resultLabel.Text = "Roll a dice!";
        resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
        resultLabel.AddThemeFontSizeOverride("font_size", 32);
        mainVBox.AddChild(resultLabel);
        
        // Dice buttons
        diceButtons = new VBoxContainer();
        diceButtons.AddThemeConstantOverride("separation", 5);
        mainVBox.AddChild(diceButtons);
        
        CreateDiceButton("D4", DiceMasterSystem.DiceType.D4);
        CreateDiceButton("D6", DiceMasterSystem.DiceType.D6);
        CreateDiceButton("D8", DiceMasterSystem.DiceType.D8);
        CreateDiceButton("D10", DiceMasterSystem.DiceType.D10);
        CreateDiceButton("D12", DiceMasterSystem.DiceType.D12);
        CreateDiceButton("D20", DiceMasterSystem.DiceType.D20);
        CreateDiceButton("D100", DiceMasterSystem.DiceType.D100);
        
        // Buff progress bar
        Label buffTitle = new Label();
        buffTitle.Text = "⏱️ Buff Duration:";
        mainVBox.AddChild(buffTitle);
        
        buffProgressBar = new ProgressBar();
        buffProgressBar.RectMinSize = new Vector2(450, 20);
        mainVBox.AddChild(buffProgressBar);
        
        // Buff info
        buffLabel = new Label();
        buffLabel.Text = "No active buffs";
        buffLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(buffLabel);
        
        // Stats
        statsLabel = new Label();
        statsLabel.Text = "Stats: Rolls: 0 | Wins: 0 | Best: 0";
        statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(statsLabel);
        
        // History
        historyLabel = new Label();
        historyLabel.Text = "History: ";
        historyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        historyLabel.AddThemeFontSizeOverride("font_size", 12);
        mainVBox.AddChild(historyLabel);
        
        // Close button
        closeButton = new Button();
        closeButton.Text = "Close (Esc)";
        closeButton.Pressed += OnClosePressed;
        mainVBox.AddChild(closeButton);
    }
    
    private void CreateDiceButton(string text, DiceMasterSystem.DiceType diceType)
    {
        Button btn = new Button();
        btn.Text = text;
        btn.RectMinSize = new Vector2(450, 40);
        
        // Store dice type in metadata
        btn.SetMeta("dice_type", (int)diceType);
        btn.Pressed += OnDiceButtonPressed;
        
        diceButtons.AddChild(btn);
    }
    
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
        
        // Update UI
        if (DiceMasterSystem.Instance != null)
        {
            diamondsLabel.Text = DiceMasterSystem.Instance.Diamonds.ToString();
            
            var stats = DiceMasterSystem.Instance.GetStatistics();
            statsLabel.Text = $"Stats: Rolls: {stats["total_rolls"]} | Wins: {stats["total_wins"]} | Best: {stats["highest_roll"]} | Streak: {stats["lucky_streak"]}";
            
            // Update buff display
            var buffs = DiceMasterSystem.Instance.GetActiveBuffsInfo();
            if (buffs.Count > 0)
            {
                string buffText = "Active Buffs: ";
                foreach (var kvp in buffs)
                {
                    string sign = kvp.Value > 0 ? "+" : "";
                    buffText += $"{kvp.Key}{sign}{kvp.Value * 100:F0}% ";
                }
                buffLabel.Text = buffText;
            }
            else
            {
                buffLabel.Text = "No active buffs";
            }
            
            // Update progress bar
            if (DiceMasterSystem.Instance.HasActiveBuffs())
            {
                buffProgressBar.Show();
            }
            else
            {
                buffProgressBar.Hide();
            }
            
            // Update history
            var history = DiceMasterSystem.Instance.GetRollHistory(5);
            string historyText = "History: ";
            foreach (var roll in history)
            {
                historyText += $"{roll.roll}/{roll.max} ";
            }
            historyLabel.Text = historyText;
        }
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt.IsActionPressed("ui_cancel"))
        {
            OnClosePressed();
        }
    }
    
    private void OnDiceButtonPressed()
    {
        Button btn = GetSenderButton();
        if (btn == null) return;
        
        int diceTypeInt = btn.GetMeta("dice_type", 0);
        DiceMasterSystem.DiceType diceType = (DiceMasterSystem.DiceType)diceTypeInt;
        
        if (DiceMasterSystem.Instance != null)
        {
            int roll = DiceMasterSystem.Instance.Roll(diceType);
            int max = DiceMasterSystem.Instance.GetMaxValue(diceType);
            
            // Show result
            string result = "";
            float ratio = (float)roll / max;
            
            if (ratio >= 0.9f)
                result = $"🎉 CRITICAL! {roll}/{max}";
            else if (ratio >= 0.7f)
                result = $"✨ Great! {roll}/{max}";
            else if (ratio >= 0.5f)
                result = $"👍 Good: {roll}/{max}";
            else if (ratio <= 0.2f)
                result = $"😱 Curse! {roll}/{max}";
            else
                result = $"🎲 {roll}/{max}";
            
            resultLabel.Text = result;
        }
    }
    
    private void OnDiceRolled(int roll, int max)
    {
        // Update handled in _Process
    }
    
    private void OnAddDiamondPressed()
    {
        // For demo, add diamond
        if (DiceMasterSystem.Instance != null)
        {
            DiceMasterSystem.Instance.Diamonds += 1;
        }
    }
    
    private void OnClosePressed()
    {
        Hide();
        isVisible = false;
    }
    
    private Button GetSenderButton()
    {
        foreach (Node child in diceButtons.GetChildren())
        {
            if (child is Button btn)
            {
                return btn;
            }
        }
        return null;
    }
    
    public void Toggle()
    {
        if (isVisible)
        {
            Hide();
            isVisible = false;
        }
        else
        {
            Show();
            isVisible = true;
        }
    }
}
