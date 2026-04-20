using Godot;
using System;
using System.Collections.Generic;

public partial class GuildLevelUI : Control
{
    private Control _mainContainer;
    private Label _levelLabel;
    private ProgressBar _expProgressBar;
    private Label _expLabel;
    private Label _maxMembersLabel;
    private Label _goldBonusLabel;
    private Label _expBonusLabel;
    private Label _warBonusLabel;
    private Label _questDiscountLabel;
    private Label _techDiscountLabel;
    private Label _bankDiscountLabel;
    private Label _lootBonusLabel;
    private VBoxContainer _perksContainer;
    private Label _statsLabel;
    
    private GuildLevelData _guildData;
    private int _guildId = 1;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        SetupUI();
        Visible = false;
        
        // Connect signals
        GuildLevelSystem.Instance.GuildLevelUp += _on_guild_level_up;
        GuildLevelSystem.Instance.GuildExperienceGained += _on_experience_gained;
        GuildLevelSystem.Instance.PerkUnlocked += _on_perk_unlocked;
        
        // Input handling
        SetProcessInput(true);
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new Control();
        _mainContainer.SetAnchorsPreset(Control.AnchorsPreset.Center);
        _mainContainer.RectMinSize = new Vector2(600, 500);
        AddChild(_mainContainer);
        
        // Background panel
        Panel background = new Panel();
        background.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        background.Modulate = new Color(0, 0, 0, 0.85f);
        _mainContainer.AddChild(background);
        
        // Title
        Label title = new Label();
        title.Text = "🏰 Guild Level";
        title.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.RectMinSize = new Vector2(0, 60);
        title.RectPosition = new Vector2(0, 10);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f)); // Gold
        title.AddFontOverride("font_size", 28);
        _mainContainer.AddChild(title);
        
        // Close button
        Button closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.RectMinSize = new Vector2(40, 40);
        closeBtn.RectPosition = new Vector2(550, 15);
        closeBtn.Pressed += ToggleUI;
        _mainContainer.AddChild(closeBtn);
        
        // Level display
        _levelLabel = new Label();
        _levelLabel.Text = "Level 1";
        _levelLabel.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        _levelLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _levelLabel.RectMinSize = new Vector2(0, 50);
        _levelLabel.RectPosition = new Vector2(0, 60);
        _levelLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
        _levelLabel.AddFontOverride("font_size", 32);
        _mainContainer.AddChild(_levelLabel);
        
        // Experience progress bar
        _expProgressBar = new ProgressBar();
        _expProgressBar.RectMinSize = new Vector2(500, 30);
        _expProgressBar.RectPosition = new Vector2(50, 110);
        _expProgressBar.Step = 1;
        _expProgressBar.PercentVisible = false;
        _mainContainer.AddChild(_expProgressBar);
        
        // Progress background
        StyleBoxFlat progressBg = new StyleBoxFlat();
        progressBg.BgColor = new Color(0.2f, 0.2f, 0.2f);
        progressBg.CornerRadiusTopLeft = 5;
        progressBg.CornerRadiusTopRight = 5;
        progressBg.CornerRadiusBottomLeft = 5;
        progressBg.CornerRadiusBottomRight = 5;
        _expProgressBar.AddThemeStyleboxOverride("background", progressBg);
        
        // Progress fill
        StyleBoxFlat progressFill = new StyleBoxFlat();
        progressFill.BgColor = new Color(0.3f, 0.7f, 1f);
        progressFill.CornerRadiusTopLeft = 5;
        progressFill.CornerRadiusTopRight = 5;
        progressFill.CornerRadiusBottomLeft = 5;
        progressFill.CornerRadiusBottomRight = 5;
        _expProgressBar.AddThemeStyleboxOverride("fill", progressFill);
        
        // Experience label
        _expLabel = new Label();
        _expLabel.Text = "0 / 1000 XP";
        _expLabel.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        _expLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _expLabel.RectMinSize = new Vector2(0, 30);
        _expLabel.RectPosition = new Vector2(0, 145);
        _expLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        _mainContainer.AddChild(_expLabel);
        
        // Stats container
        VBoxContainer statsContainer = new VBoxContainer();
        statsContainer.RectMinSize = new Vector2(250, 200);
        statsContainer.RectPosition = new Vector2(30, 180);
        _mainContainer.AddChild(statsContainer);
        
        // Stats title
        Label statsTitle = new Label();
        statsTitle.Text = "📊 Guild Stats";
        statsTitle.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 1f));
        statsTitle.AddFontOverride("font_size", 18);
        statsContainer.AddChild(statsTitle);
        
        _statsLabel = new Label();
        _statsLabel.Text = "Loading...";
        _statsLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        _statsLabel.AddFontOverride("font_size", 14);
        statsContainer.AddChild(_statsLabel);
        
        // Bonuses container
        VBoxContainer bonusesContainer = new VBoxContainer();
        bonusesContainer.RectMinSize = new Vector2(250, 200);
        bonusesContainer.RectPosition = new Vector2(320, 180);
        _mainContainer.AddChild(bonusesContainer);
        
        // Bonuses title
        Label bonusesTitle = new Label();
        bonusesTitle.Text = "⚡ Active Bonuses";
        bonusesTitle.AddThemeColorOverride("font_color", new Color(0.7f, 1f, 0.7f));
        bonusesTitle.AddFontOverride("font_size", 18);
        bonusesContainer.AddChild(bonusesTitle);
        
        _goldBonusLabel = new Label();
        _goldBonusLabel.Text = "Gold Bonus: +0%";
        _goldBonusLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
        bonusesContainer.AddChild(_goldBonusLabel);
        
        _expBonusLabel = new Label();
        _expBonusLabel.Text = "EXP Bonus: +0%";
        _expBonusLabel.AddThemeColorOverride("font_color", new Color(0.5f, 1f, 0.5f));
        bonusesContainer.AddChild(_expBonusLabel);
        
        _warBonusLabel = new Label();
        _warBonusLabel.Text = "War Score: +0%";
        _warBonusLabel.AddThemeColorOverride("font_color", new Color(1f, 0.5f, 0.5f));
        bonusesContainer.AddChild(_warBonusLabel);
        
        _questDiscountLabel = new Label();
        _questDiscountLabel.Text = "Quest Discount: -0%";
        _questDiscountLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.8f, 1f));
        bonusesContainer.AddChild(_questDiscountLabel);
        
        _techDiscountLabel = new Label();
        _techDiscountLabel.Text = "Tech Discount: -0%";
        _techDiscountLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.5f, 1f));
        bonusesContainer.AddChild(_techDiscountLabel);
        
        _bankDiscountLabel = new Label();
        _bankDiscountLabel.Text = "Bank Fee: -0%";
        _bankDiscountLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.5f));
        bonusesContainer.AddChild(_bankDiscountLabel);
        
        _lootBonusLabel = new Label();
        _lootBonusLabel.Text = "Loot Bonus: +0%";
        _lootBonusLabel.AddThemeColorOverride("font_color", new Color(1f, 0.5f, 1f));
        bonusesContainer.AddChild(_lootBonusLabel);
        
        // Perks section
        Label perksTitle = new Label();
        perksTitle.Text = "🎁 Unlocked Perks";
        perksTitle.RectPosition = new Vector2(30, 390);
        perksTitle.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.5f));
        perksTitle.AddFontOverride("font_size", 18);
        _mainContainer.AddChild(perksTitle);
        
        // Scroll container for perks
        ScrollContainer perksScroll = new ScrollContainer();
        perksScroll.RectMinSize = new Vector2(540, 80);
        perksScroll.RectPosition = new Vector2(30, 420);
        _mainContainer.AddChild(perksScroll);
        
        _perksContainer = new VBoxContainer();
        _perksContainer.RectMinSize = new Vector2(520, 80);
        perksScroll.AddChild(_perksContainer);
        
        // Max members label
        _maxMembersLabel = new Label();
        _maxMembersLabel.Text = "Max Members: 10";
        _maxMembersLabel.RectPosition = new Vector2(450, 60);
        _maxMembersLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        _mainContainer.AddChild(_maxMembersLabel);
        
        // Update button
        Button refreshBtn = new Button();
        refreshBtn.Text = "🔄 Refresh";
        refreshBtn.RectMinSize = new Vector2(100, 30);
        refreshBtn.RectPosition = new Vector2(480, 460);
        refreshBtn.Pressed += RefreshData;
        _mainContainer.AddChild(refreshBtn);
    }
    
    public void ToggleUI()
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
        _guildData = GuildLevelSystem.Instance.GetOrCreateGuildLevel(_guildId);
        
        // Update level display
        _levelLabel.Text = $"Level {_guildData.Level}";
        
        // Update progress bar
        float progress = GuildLevelSystem.Instance.GetLevelProgress(_guildId);
        _expProgressBar.Value = progress * 100;
        
        // Update experience label
        int currentExp = _guildData.Experience;
        int nextLevelExp = GuildLevelDatabase.GetExperienceForLevel(_guildData.Level + 1);
        if (nextLevelExp == 0)
        {
            _expLabel.Text = $"{currentExp} / {currentExp} XP (MAX)";
        }
        else
        {
            _expLabel.Text = $"{currentExp} / {nextLevelExp} XP";
        }
        
        // Update max members
        int maxMembers = GuildLevelSystem.Instance.GetMaxMembers(_guildId);
        _maxMembersLabel.Text = $"Max Members: {maxMembers}";
        
        // Update bonuses
        float goldBonus = GuildLevelSystem.Instance.GetGoldBonus(_guildId);
        float expBonus = GuildLevelSystem.Instance.GetExpBonus(_guildId);
        float warBonus = GuildLevelSystem.Instance.GetWarBonus(_guildId);
        float questDiscount = GuildLevelSystem.Instance.GetQuestDiscount(_guildId);
        float techDiscount = GuildLevelSystem.Instance.GetTechDiscount(_guildId);
        float bankDiscount = GuildLevelSystem.Instance.GetBankDiscount(_guildId);
        float lootBonus = GuildLevelSystem.Instance.GetLootBonus(_guildId);
        
        _goldBonusLabel.Text = $"Gold Bonus: +{(goldBonus * 100):F0}%";
        _expBonusLabel.Text = $"EXP Bonus: +{(expBonus * 100):F0}%";
        _warBonusLabel.Text = $"War Score: +{(warBonus * 100):F0}%";
        _questDiscountLabel.Text = $"Quest Discount: -{(questDiscount * 100):F0}%";
        _techDiscountLabel.Text = $"Tech Discount: -{(techDiscount * 100):F0}%";
        _bankDiscountLabel.Text = $"Bank Fee: -{(bankDiscount * 100):F0}%";
        _lootBonusLabel.Text = $"Loot Bonus: +{(lootBonus * 100):F0}%";
        
        // Update stats
        var stats = GuildLevelSystem.Instance.GetGuildStats(_guildId);
        _statsLabel.Text = $"Total XP: {stats["total_experience"]}\n" +
                          $"Quests: {stats["total_quests"]}\n" +
                          $"Wars Won: {stats["wars_won"]} | Lost: {stats["wars_lost"]}\n" +
                          $"Tech Researched: {stats["tech_researched"]}\n" +
                          $"Daily Contrib: {stats["daily_contributions"]}";
        
        // Update perks list
        UpdatePerksList();
    }
    
    private void UpdatePerksList()
    {
        // Clear existing perks
        foreach (Node child in _perksContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var perks = GuildLevelSystem.Instance.GetUnlockedPerks(_guildId);
        
        if (perks.Count == 0)
        {
            Label noPerks = new Label();
            noPerks.Text = "No perks unlocked yet. Level up to unlock!";
            noPerks.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            _perksContainer.AddChild(noPerks);
        }
        else
        {
            foreach (string perkId in perks)
            {
                var perkInfo = GuildLevelSystem.Instance.GetPerkInfo(perkId);
                if (perkInfo != null)
                {
                    Label perkLabel = new Label();
                    perkLabel.Text = $"✓ {perkInfo["name"]}: {perkInfo["description"]}";
                    perkLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
                    _perksContainer.AddChild(perkLabel);
                }
            }
        }
    }
    
    public override void _Input(InputEvent eventData)
    {
        if (eventData.IsActionPressed("ui_cancel") && _isVisible)
        {
            ToggleUI();
        }
    }
    
    private void _on_guild_level_up(int guildId, int newLevel)
    {
        if (guildId == _guildId && _isVisible)
        {
            RefreshData();
        }
    }
    
    private void _on_experience_gained(int guildId, int amount)
    {
        if (guildId == _guildId && _isVisible)
        {
            RefreshData();
        }
    }
    
    private void _on_perk_unlocked(int guildId, string perkId)
    {
        if (guildId == _guildId && _isVisible)
        {
            RefreshData();
        }
    }
}
