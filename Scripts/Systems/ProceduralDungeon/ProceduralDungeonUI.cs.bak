using System;
using Godot;
using Godot.Collections;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 程序化地下城UI
    /// </summary>
    public partial class ProceduralDungeonUI : Control
    {
        private ProceduralDungeonSystem _dungeonSystem;
        
        // UI组件
        private Label _titleLabel;
        private Label _dungeonNameLabel;
        private Label _floorLabel;
        private Label _roomLabel;
        private Label _difficultyLabel;
        private Label _enemiesLabel;
        private Label _treasureLabel;
        private Label _statusLabel;
        
        private Button _generateButton;
        private Button _enterRoomButton;
        private Button _clearRoomButton;
        private Button _nextFloorButton;
        private Button _closeButton;
        
        private ItemList _roomList;
        private ItemList _connectedRoomsList;
        
        private VBoxContainer _roomInfoContainer;
        private HBoxContainer _dungeonInfoContainer;
        
        private bool _isVisible = false;
        
        public override void _Ready()
        {
            _dungeonSystem = ProceduralDungeonSystem.Instance;
            SetupUI();
            GD.Print("Procedural Dungeon UI initialized");
        }
        
        private void SetupUI()
        {
            // 主容器
            var mainContainer = new VBoxContainer
            {
                AnchorRight = Vector2.Right,
                AnchorBottom = Vector2.Bottom,
                OffsetRight = -20,
                OffsetBottom = -20
            };
            AddChild(mainContainer);
            
            // 标题
            _titleLabel = new Label
            {
                Text = "Procedural Dungeon",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainContainer.AddChild(_titleLabel);
            
            // 地下城信息
            _dungeonInfoContainer = new HBoxContainer();
            mainContainer.AddChild(_dungeonInfoContainer);
            
            _dungeonNameLabel = new Label { Text = "No dungeon" };
            _floorLabel = new Label { Text = "Floor: 0/0" };
            _roomLabel = new Label { Text = "Room: -" };
            _difficultyLabel = new Label { Text = "Difficulty: -" };
            
            _dungeonInfoContainer.AddChild(_dungeonNameLabel);
            _dungeonInfoContainer.AddChild(new VSeparator());
            _dungeonInfoContainer.AddChild(_floorLabel);
            _dungeonInfoContainer.AddChild(new VSeparator());
            _dungeonInfoContainer.AddChild(_roomLabel);
            _dungeonInfoContainer.AddChild(new VSeparator());
            _dungeonInfoContainer.AddChild(_difficultyLabel);
            
            // 分隔
            mainContainer.AddChild(new HSeparator());
            
            // 房间信息容器
            _roomInfoContainer = new VBoxContainer();
            mainContainer.AddChild(_roomInfoContainer);
            
            _enemiesLabel = new Label { Text = "Enemies: 0" };
            _treasureLabel = new Label { Text = "Treasure: None" };
            _statusLabel = new Label { Text = "Status: Not entered" };
            
            _roomInfoContainer.AddChild(_enemiesLabel);
            _roomInfoContainer.AddChild(_treasureLabel);
            _roomInfoContainer.AddChild(_statusLabel);
            
            // 分隔
            mainContainer.AddChild(new HSeparator());
            
            // 房间列表
            var roomListLabel = new Label { Text = "Current Floor Rooms:" };
            mainContainer.AddChild(roomListLabel);
            
            _roomList = new ItemList();
            _roomList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _roomList.CustomMinimumSize = new Vector2(0, 150);
            mainContainer.AddChild(_roomList);
            
            // 连接房间列表
            var connectedLabel = new Label { Text = "Connected Rooms:" };
            mainContainer.AddChild(connectedLabel);
            
            _connectedRoomsList = new ItemList();
            _connectedRoomsList.CustomMinimumSize = new Vector2(0, 100);
            mainContainer.AddChild(_connectedRoomsList);
            
            // 按钮容器
            var buttonContainer = new HBoxContainer();
            mainContainer.AddChild(buttonContainer);
            
            _generateButton = new Button { Text = "Generate" };
            _generateButton.Pressed += OnGeneratePressed;
            buttonContainer.AddChild(_generateButton);
            
            _enterRoomButton = new Button { Text = "Enter Room" };
            _enterRoomButton.Pressed += OnEnterRoomPressed;
            _enterRoomButton.Disabled = true;
            buttonContainer.AddChild(_enterRoomButton);
            
            _clearRoomButton = new Button { Text = "Clear Room" };
            _clearRoomButton.Pressed += OnClearRoomPressed;
            _clearRoomButton.Disabled = true;
            buttonContainer.AddChild(_clearRoomButton);
            
            _nextFloorButton = new Button { Text = "Next Floor" };
            _nextFloorButton.Pressed += OnNextFloorPressed;
            _nextFloorButton.Disabled = true;
            buttonContainer.AddChild(_nextFloorButton);
            
            _closeButton = new Button { Text = "Close" };
            _closeButton.Pressed += OnClosePressed;
            buttonContainer.AddChild(_closeButton);
            
            // 初始隐藏
            Visible = false;
        }
        
        private void UpdateUI()
        {
            if (_dungeonSystem?.CurrentDungeon == null)
            {
                _dungeonNameLabel.Text = "No dungeon";
                _floorLabel.Text = "Floor: 0/0";
                _roomLabel.Text = "Room: -";
                _difficultyLabel.Text = "Difficulty: -";
                _enemiesLabel.Text = "Enemies: 0";
                _treasureLabel.Text = "Treasure: None";
                _statusLabel.Text = "Status: Not in dungeon";
                
                _roomList.Clear();
                _connectedRoomsList.Clear();
                
                _enterRoomButton.Disabled = true;
                _clearRoomButton.Disabled = true;
                _nextFloorButton.Disabled = true;
                
                return;
            }
            
            var dungeon = _dungeonSystem.CurrentDungeon;
            _dungeonNameLabel.Text = dungeon.DungeonName;
            _floorLabel.Text = $"Floor: {dungeon.CurrentFloor}/{dungeon.TotalFloors}";
            
            var currentFloor = dungeon.Floors[dungeon.CurrentFloor - 1];
            
            // 更新房间列表
            _roomList.Clear();
            foreach (var room in currentFloor.Rooms)
            {
                string prefix = room.IsDiscovered ? (room.IsCleared ? "✓" : "●") : "○";
                string roomInfo = $"{prefix} {room.Type} ({room.Difficulty})";
                _roomList.AddItem(roomInfo);
            }
            
            // 更新当前房间信息
            if (dungeon.CurrentRoom != null)
            {
                var room = dungeon.CurrentRoom;
                _roomLabel.Text = $"Room: {room.Type}";
                _difficultyLabel.Text = $"Difficulty: {room.Difficulty}";
                _enemiesLabel.Text = $"Enemies: {room.Enemies.Count}";
                _treasureLabel.Text = room.TreasureId != null ? $"Treasure: {room.TreasureId}" : "Treasure: None";
                _statusLabel.Text = room.IsCleared ? "Status: Cleared" : "Status: Not cleared";
                
                // 更新连接房间列表
                _connectedRoomsList.Clear();
                var connectedRooms = _dungeonSystem.GetConnectedRooms();
                foreach (var connected in connectedRooms)
                {
                    string prefix = connected.IsDiscovered ? (connected.IsCleared ? "✓" : "●") : "○";
                    _connectedRoomsList.AddItem($"{prefix} {connected.Type}");
                }
                
                _enterRoomButton.Disabled = false;
                _clearRoomButton.Disabled = room.IsCleared || room.Enemies.Count == 0;
                _nextFloorButton.Disabled = dungeon.CurrentFloor >= dungeon.TotalFloors;
            }
            else
            {
                _roomLabel.Text = "Room: -";
                _difficultyLabel.Text = "Difficulty: -";
                _enemiesLabel.Text = "Enemies: 0";
                _treasureLabel.Text = "Treasure: None";
                _statusLabel.Text = "Status: Choose a room";
                
                _connectedRoomsList.Clear();
                _enterRoomButton.Disabled = true;
                _clearRoomButton.Disabled = true;
                _nextFloorButton.Disabled = true;
            }
        }
        
        private void OnGeneratePressed()
        {
            // 随机选择地下城类型
            var dungeonTypes = new[] { "AncientRuins", "DeepCavern", "ForgottenTemple", "AbandonedFortress", "EnchantedForest" };
            var selectedType = dungeonTypes[new Random().Next(dungeonTypes.Length)];
            
            _dungeonSystem.GenerateDungeon(selectedType);
            UpdateUI();
        }
        
        private void OnEnterRoomPressed()
        {
            var selected = _connectedRoomsList.GetSelectedItems();
            if (selected.Length > 0)
            {
                var connectedRooms = _dungeonSystem.GetConnectedRooms();
                if (selected[0] < connectedRooms.Count)
                {
                    var room = connectedRooms[selected[0]];
                    _dungeonSystem.EnterRoom(room.RoomId);
                    UpdateUI();
                }
            }
            else if (_roomList.GetItemCount() > 0)
            {
                // 进入选中的房间
                var currentFloor = _dungeonSystem.CurrentDungeon.Floors[_dungeonSystem.CurrentDungeon.CurrentFloor - 1];
                var selectedRooms = _roomList.GetSelectedItems();
                if (selectedRooms.Length > 0)
                {
                    var room = currentFloor.Rooms[selectedRooms[0]];
                    _dungeonSystem.EnterRoom(room.RoomId);
                    UpdateUI();
                }
            }
        }
        
        private void OnClearRoomPressed()
        {
            _dungeonSystem.ClearCurrentRoom();
            UpdateUI();
        }
        
        private void OnNextFloorPressed()
        {
            _dungeonSystem.CompleteFloor();
            UpdateUI();
        }
        
        private void OnClosePressed()
        {
            Toggle();
        }
        
        public void Toggle()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                UpdateUI();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Ctrl+Shift+D 切换地下城UI
                if (keyEvent.Ctrl && keyEvent.Shift && keyEvent.Keycode == Key.D)
                {
                    Toggle();
                }
            }
        }
    }
}
