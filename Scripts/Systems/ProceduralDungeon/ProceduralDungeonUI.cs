using Godot;
using System;
using System.Collections.Generic;

public class ProceduralDungeonUI : Control
{
    private ProceduralDungeonSystem dungeonSystem;
    private Label titleLabel;
    private Label floorLabel;
    private Label difficultyLabel;
    private Label shapeLabel;
    private Label statsLabel;
    private Button generateButton;
    private Button generateAllButton;
    private Button closeButton;
    private VBoxContainer roomListContainer;
    private TabContainer tabContainer;
    
    private int currentFloor = 1;
    private DungeonShape currentShape = DungeonShape.Branching;
    private DungeonDifficulty currentDifficulty = DungeonDifficulty.Normal;
    
    public override void _Ready()
    {
        dungeonSystem = GetNode<ProceduralDungeonSystem>("/root/Main/ProceduralDungeonSystem");
        if (dungeonSystem == null)
        {
            dungeonSystem = new ProceduralDungeonSystem();
        }
        
        SetupUI();
        Visible = false;
    }
    
    private void SetupUI()
    {
        // Main panel
        Panel mainPanel = new Panel();
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainPanel);
        
        VBoxContainer mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 10);
        mainPanel.AddChild(mainVBox);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "Procedural Dungeon Generator";
        titleLabel.Align = Label.AlignEnum.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);
        
        // Tab container
        tabContainer = new TabContainer();
        tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainVBox.AddChild(tabContainer);
        
        // Generate Tab
        VBoxContainer generateTab = new VBoxContainer();
        generateTab.Name = "Generate";
        tabContainer.AddChild(generateTab);
        
        // Floor selection
        HBoxContainer floorHBox = new HBoxContainer();
        floorHBox.AddThemeConstantOverride("separation", 10);
        generateTab.AddChild(floorHBox);
        
        floorHBox.AddChild(new Label { Text = "Floor: " });
        
        SpinBox floorSpin = new SpinBox();
        floorSpin.MinValue = 1;
        floorSpin.MaxValue = 100;
        floorSpin.Value = currentFloor;
        floorSpin.ValueChanged += (val) => currentFloor = (int)val;
        floorHBox.AddChild(floorSpin);
        
        // Shape selection
        HBoxContainer shapeHBox = new HBoxContainer();
        shapeHBox.AddThemeConstantOverride("separation", 10);
        generateTab.AddChild(shapeHBox);
        
        shapeHBox.AddChild(new Label { Text = "Shape: " });
        
        OptionButton shapeOption = new OptionButton();
        foreach (DungeonShape shape in Enum.GetValues(typeof(DungeonShape)))
        {
            shapeOption.AddItem(shape.ToString(), (int)shape);
        }
        shapeOption.Selected = (int)currentShape;
        shapeOption.ItemSelected += (index) => currentShape = (DungeonShape)index;
        shapeHBox.AddChild(shapeOption);
        
        // Difficulty selection
        HBoxContainer diffHBox = new HBoxContainer();
        diffHBox.AddThemeConstantOverride("separation", 10);
        generateTab.AddChild(diffHBox);
        
        diffHBox.AddChild(new Label { Text = "Difficulty: " });
        
        OptionButton diffOption = new OptionButton();
        foreach (DungeonDifficulty diff in Enum.GetValues(typeof(DungeonDifficulty)))
        {
            diffOption.AddItem(diff.ToString(), (int)diff);
        }
        diffOption.Selected = (int)currentDifficulty;
        diffOption.ItemSelected += (index) => currentDifficulty = (DungeonDifficulty)index;
        diffHBox.AddChild(diffOption);
        
        // Generate button
        generateButton = new Button();
        generateButton.Text = "Generate Dungeon";
        generateButton.Pressed += OnGeneratePressed;
        generateTab.AddChild(generateButton);
        
        // Generate all floors button
        generateAllButton = new Button();
        generateAllButton.Text = "Generate 5 Floors";
        generateAllButton.Pressed += OnGenerateAllPressed;
        generateTab.AddChild(generateAllButton);
        
        // Rooms list
        ScrollContainer roomScroll = new ScrollContainer();
        roomScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        generateTab.AddChild(roomScroll);
        
        roomListContainer = new VBoxContainer();
        roomListContainer.AddThemeConstantOverride("separation", 5);
        roomScroll.AddChild(roomListContainer);
        
        // Statistics Tab
        VBoxContainer statsTab = new VBoxContainer();
        statsTab.Name = "Statistics";
        tabContainer.AddChild(statsTab);
        
        statsLabel = new Label();
        statsLabel.Text = "Statistics:\n";
        statsTab.AddChild(statsLabel);
        
        // Info Tab
        VBoxContainer infoTab = new VBoxContainer();
        infoTab.Name = "Info";
        tabContainer.AddChild(infoTab);
        
        Label infoLabel = new Label();
        infoLabel.Text = "Procedural Dungeon Generator\n\n" +
            "This system generates random dungeons with various shapes and room types.\n\n" +
            "Shapes:\n" +
            "- Linear: Straight path from start to boss\n" +
            "- Branching: Main path with side branches\n" +
            "- Circular: Ring of rooms with center boss\n" +
            "- Hub and Spoke: Central hub with radiating paths\n" +
            "- Maze: Complex interconnected rooms\n\n" +
            "Room Types:\n" +
            "- Combat: Fight enemies\n" +
            "- Treasure: Find loot\n" +
            "- Boss: Challenge the boss\n" +
            "- Shop: Buy items\n" +
            "- Rest: Heal and save\n" +
            "- Event: Random events";
        infoTab.AddChild(infoLabel);
        
        // Close button
        closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += OnClosePressed;
        mainVBox.AddChild(closeButton);
    }
    
    private void OnGeneratePressed()
    {
        if (dungeonSystem != null)
        {
            var floor = dungeonSystem.GenerateDungeon(currentFloor, currentShape, currentDifficulty);
            UpdateRoomList(floor);
            UpdateStats();
        }
    }
    
    private void OnGenerateAllPressed()
    {
        if (dungeonSystem != null)
        {
            for (int i = 1; i <= 5; i++)
            {
                DungeonShape shape = (DungeonShape)(i % 5);
                dungeonSystem.GenerateDungeon(i, shape, currentDifficulty);
            }
            UpdateStats();
        }
    }
    
    private void OnClosePressed()
    {
        Visible = false;
    }
    
    private void UpdateRoomList(DungeonFloor floor)
    {
        foreach (Node child in roomListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        foreach (var room in floor.rooms)
        {
            Label roomLabel = new Label();
            roomLabel.Text = $"Room {room.id}: {room.type} ({room.x}, {room.y})";
            
            // Color by room type
            Color roomColor = Colors.White;
            switch (room.type)
            {
                case RoomType.Boss: roomColor = Colors.Red; break;
                case RoomType.Treasure: roomColor = Colors.Gold; break;
                case RoomType.Shop: roomColor = Colors.Green; break;
                case RoomType.Rest: roomColor = Colors.Cyan; break;
                case RoomType.Combat: roomColor = Colors.Orange; break;
            }
            roomLabel.AddThemeColorOverride("font_color", roomColor);
            
            roomListContainer.AddChild(roomLabel);
        }
    }
    
    private void UpdateStats()
    {
        if (dungeonSystem != null)
        {
            var stats = dungeonSystem.GetStatistics();
            statsLabel.Text = "Statistics:\n";
            foreach (var kvp in stats)
            {
                statsLabel.Text += $"{kvp.Key}: {kvp.Value}\n";
            }
        }
    }
    
    public override void _Input(InputEvent eventInput)
    {
        if (eventInput.IsActionPressed("ui_cancel"))
        {
            Visible = !Visible;
        }
    }
    
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            UpdateStats();
        }
    }
}
