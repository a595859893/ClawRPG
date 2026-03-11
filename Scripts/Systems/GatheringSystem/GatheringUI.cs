using Godot;
using Godot.Collections;
using System;

public partial class GatheringUI : Control
{
    private Panel mainPanel;
    private TabContainer tabContainer;
    private Label titleLabel;
    private Button closeButton;
    private VBoxContainer statsContainer;
    private GridContainer toolsGrid;
    private Label statsLabel;
    
    private bool isVisible = false;
    
    public override void _Ready()
    {
        base._Ready();
        SetupUI();
        Visible = false;
    }
    
    private void SetupUI()
    {
        // Main panel
        mainPanel = new Panel();
        mainPanel.SetSize(new Vector2(600, 500));
        mainPanel.Position = new Vector2(100, 50);
        AddChild(mainPanel);
        
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        panelStyle.SetBorderWidthAll(2);
        panelStyle.SetCornerRadiusAll(8);
        mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "🎣 Gathering System";
        titleLabel.Position = new Vector2(20, 15);
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainPanel.AddChild(titleLabel);
        
        // Close button
        closeButton = new Button();
        closeButton.Text = "✕";
        closeButton.Position = new Vector2(550, 10);
        closeButton.SetSize(new Vector2(40, 30));
        closeButton.Pressed += () => ToggleUI();
        mainPanel.AddChild(closeButton);
        
        // Tab container
        tabContainer = new TabContainer();
        tabContainer.Position = new Vector2(20, 60);
        tabContainer.SetSize(new Vector2(560, 420));
        mainPanel.AddChild(tabContainer);
        
        // Stats tab
        var statsTab = new Control();
        statsTab.Name = "Statistics";
        tabContainer.AddChild(statsTab);
        
        statsContainer = new VBoxContainer();
        statsContainer.Position = new Vector2(20, 20);
        statsContainer.SetSize(new Vector2(520, 370));
        statsContainer.AddThemeConstantOverride("separation", 10);
        statsTab.AddChild(statsContainer);
        
        // Tools tab
        var toolsTab = new Control();
        toolsTab.Name = "Tools";
        tabContainer.AddChild(toolsTab);
        
        var toolsScroll = new ScrollContainer();
        toolsScroll.Position = new Vector2(10, 10);
        toolsScroll.SetSize(new Vector2(540, 370));
        toolsTab.AddChild(toolsScroll);
        
        toolsGrid = new GridContainer();
        toolsGrid.Columns = 2;
        toolsGrid.AddThemeConstantOverride("h_separation", 10);
        toolsGrid.AddThemeConstantOverride("v_separation", 10);
        toolsScroll.AddChild(toolsGrid);
        
        // Resources tab
        var resourcesTab = new Control();
        resourcesTab.Name = "Resources";
        tabContainer.AddChild(resourcesTab);
        
        var resourcesInfo = new RichTextLabel();
        resourcesInfo.BbcodeEnabled = true;
        resourcesInfo.Position = new Vector2(20, 20);
        resourcesInfo.SetSize(new Vector2(520, 370));
        resourcesInfo.Text = @"[b]Resource Types:[/b]

[b]🌿 Herbs (Sickle)[/b]
- Common: Green Herb
- Uncommon: Silverleaf
- Rare: Moonpetal
- Epic: Starlight Bloom
- Legendary: Phoenix Feather

[b]⛏️ Ores (Pickaxe)[/b]
- Common: Iron Ore
- Uncommon: Copper Ore
- Rare: Gold Ore
- Epic: Mithril Ore
- Legendary: Adamantite

[b]🪵 Wood (Axe)[/b]
- Common: Oak Wood
- Uncommon: Pine Wood
- Rare: Ebony Wood
- Epic: Crystal Wood
- Legendary: Divine Wood

[b]🐟 Fish (Fishing Rod)[/b]
- Common: Small Fish
- Uncommon: River Trout
- Rare: Golden Koi
- Epic: Leviathan Scale
- Legendary: Dragon Fish

[b]🦋 Insects (Net)[/b]
- Common: Beetle
- Uncommon: Butterfly
- Rare: Scorpion
- Epic: Phoenix Moth
- Legendary: Soul Butterfly

[b]💎 Crystals (Pickaxe)[/b]
- Common: Amethyst
- Uncommon: Emerald
- Rare: Ruby
- Epic: Diamond
- Legendary: Prismatic Gem

[b]🍄 Mushrooms (Sickle)[/b]
- Common: Red Cap
- Uncommon: Glow Shroom
- Rare: Spirit Mold
- Epic: Dream Mushroom
- Legendary: Immortal Fungus

[b]🍎 Fruits (Sickle)[/b]
- Common: Apple
- Uncommon: Golden Fruit
- Rare: Spirit Fruit
- Epic: Phoenix Fruit
- Legendary: World Tree Fruit";
        resourcesTab.AddChild(resourcesInfo);
    }
    
    public override void _Process(double delta)
    {
        if (Visible)
        {
            UpdateStats();
            UpdateTools();
        }
    }
    
    private void UpdateStats()
    {
        statsContainer.GetChildren().ForEach(c => c.QueueFree());
        
        if (GatheringSystem.Instance == null) return;
        
        var stats = GatheringSystem.Instance.GetGatheringStats();
        
        // Title
        var title = new Label();
        title.Text = "📊 Gathering Statistics";
        title.AddThemeFontSizeOverride("font_size", 20);
        statsContainer.AddChild(title);
        
        // Overview
        var overview = new Label();
        overview.Text = $"Total Gathers: {stats.GetValueOrDefault("total_gathers", 0)}";
        statsContainer.AddChild(overview);
        
        var totalRes = new Label();
        totalRes.Text = $"Total Resources: {stats.GetValueOrDefault("total_resources", 0)}";
        statsContainer.AddChild(totalRes);
        
        // By type
        var typeTitle = new Label();
        typeTitle.Text = "\n[b]Resources by Type:[/b]";
        typeTitle.BbcodeEnabled = true;
        statsContainer.AddChild(typeTitle);
        
        var herbs = new Label();
        herbs.Text = $"🌿 Herbs: {stats.GetValueOrDefault("herbs_gathered", 0)}";
        statsContainer.AddChild(herbs);
        
        var ores = new Label();
        ores.Text = $"⛏️ Ores: {stats.GetValueOrDefault("ores_gathered", 0)}";
        statsContainer.AddChild(ores);
        
        var wood = new Label();
        wood.Text = $"🪵 Wood: {stats.GetValueOrDefault("wood_gathered", 0)}";
        statsContainer.AddChild(wood);
        
        var fish = new Label();
        fish.Text = $"🐟 Fish: {stats.GetValueOrDefault("fish_caught", 0)}";
        statsContainer.AddChild(fish);
        
        var insects = new Label();
        insects.Text = $"🦋 Insects: {stats.GetValueOrDefault("insects_caught", 0)}";
        statsContainer.AddChild(insects);
        
        var crystals = new Label();
        crystals.Text = $"💎 Crystals: {stats.GetValueOrDefault("crystals_gathered", 0)}";
        statsContainer.AddChild(crystals);
        
        var mushrooms = new Label();
        mushrooms.Text = $"🍄 Mushrooms: {stats.GetValueOrDefault("mushrooms_gathered", 0)}";
        statsContainer.AddChild(mushrooms);
        
        var fruits = new Label();
        fruits.Text = $"🍎 Fruits: {stats.GetValueOrDefault("fruits_gathered", 0)}";
        statsContainer.AddChild(fruits);
        
        // Rarity finds
        var rarityTitle = new Label();
        rarityTitle.Text = "\n[b]Rare Finds:[/b]";
        rarityTitle.BbcodeEnabled = true;
        statsContainer.AddChild(rarityTitle);
        
        var rare = new Label();
        rare.Text = $"⭐ Rare: {stats.GetValueOrDefault("rare_finds", 0)}";
        statsContainer.AddChild(rare);
        
        var epic = new Label();
        epic.Text = $"🌟 Epic: {stats.GetValueOrDefault("epic_finds", 0)}";
        statsContainer.AddChild(epic);
        
        var legendary = new Label();
        legendary.Text = $"💫 Legendary: {stats.GetValueOrDefault("legendary_finds", 0)}";
        statsContainer.AddChild(legendary);
    }
    
    private void UpdateTools()
    {
        toolsGrid.GetChildren().ForEach(c => c.QueueFree());
        
        if (GatheringSystem.Instance == null) return;
        
        var allTools = GatheringSystem.Instance.GetAllTools();
        
        foreach (Dictionary tool in allTools)
        {
            var toolPanel = new PanelContainer();
            toolPanel.SetSize(new Vector2(250, 80));
            
            var toolStyle = new StyleBoxFlat();
            toolStyle.BgColor = new Color(0.15f, 0.15f, 0.2f);
            toolStyle.SetCornerRadiusAll(5);
            toolPanel.AddThemeStyleboxOverride("panel", toolStyle);
            
            var toolVBox = new VBoxContainer();
            toolVBox.AddThemeConstantOverride("separation", 5);
            toolPanel.AddChild(toolVBox);
            
            var nameLabel = new Label();
            nameLabel.Text = $"[b]{tool.GetValueOrDefault("tool_name", "")}[/b]";
            nameLabel.BbcodeEnabled = true;
            toolVBox.AddChild(nameLabel);
            
            var typeLabel = new Label();
            typeLabel.Text = $"Type: {tool.GetValueOrDefault("tool_type", "")}";
            toolVBox.AddChild(typeLabel);
            
            var levelLabel = new Label();
            levelLabel.Text = $"Level Req: {tool.GetValueOrDefault("level_required", 1)}";
            toolVBox.AddChild(levelLabel);
            
            var effLabel = new Label();
            effLabel.Text = $"Efficiency: {tool.GetValueOrDefault("efficiency", 1.0f)}x";
            toolVBox.AddChild(effLabel);
            
            toolsGrid.AddChild(toolPanel);
        }
    }
    
    public void ToggleUI()
    {
        isVisible = !isVisible;
        Visible = isVisible;
        
        if (isVisible)
        {
            UpdateStats();
            UpdateTools();
        }
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.G)
        {
            if (keyEvent.ShiftPressed)
            {
                ToggleUI();
            }
        }
    }
}
