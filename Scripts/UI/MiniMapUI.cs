using Godot;
using System;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// MiniMapUI - 小地图管理
    /// 处理小地图的显示、玩家位置标记、NPC/敌人标记等功能
    /// REQ-101: Alt+Drag to move, Alt+Scroll to resize, Alt+DoubleClick to reset
    /// </summary>
    public partial class MiniMapUI : BaseUI
    {
        public static new MiniMapUI Instance { get; protected set; }
        private const string ELEMENT_ID = "MiniMapUI";

        // 场景引用
        private Main _main;
        private Player _player;

        // 地图节点
        private TextureRect _mapTexture;
        private Control _playerMarker;
        private Control _npcMarkers;
        private Control _enemyMarkers;
        private Control _poiMarkers;
        private Label _areaNameLabel;
        private Label _compassLabel;

        // 地图配置
        private float _mapScale = 1.0f;
        private bool _showNPCMarkers = true;
        private bool _showEnemyMarkers = true;
        private bool _showPOIMarkers = true;
        private bool _followPlayer = true;

        // 刷新间隔
        private float _updateInterval = 0.2f;
        private float _updateTimer = 0f;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            LoadNodes();

            // REQ-101: Register with HUD layout manager
            RegisterForDrag();
        }

        private void RegisterForDrag()
        {
            if (HUDLayoutManager.Instance != null)
            {
                HUDLayoutManager.Instance.RegisterHUD(ELEMENT_ID, this);
            }
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb)
            {
                // Double-click to reset position (Alt held)
                if (mb.ButtonIndex == MouseButton.Left && mb.DoubleClick && Input.IsKeyPressed(Key.Alt))
                {
                    HUDLayoutManager.Instance?.OnElementDoubleClicked(ELEMENT_ID);
                    AcceptEvent();
                    return;
                }

                // Start drag
                if (mb.ButtonIndex == MouseButton.Left && mb.Pressed && Input.IsKeyPressed(Key.Alt))
                {
                    var started = HUDLayoutManager.Instance?.TryStartDrag(this, ELEMENT_ID, mb.GlobalPosition) ?? false;
                    if (started) AcceptEvent();
                }
            }

            // Update drag position while dragging
            if (@event is InputEventMouseMotion mm && Input.IsKeyPressed(Key.Alt))
            {
                HUDLayoutManager.Instance?.UpdateDragPosition(mm.GlobalPosition);
            }

            // Let base handle other events
            base._GuiInput(@event);
        }

        private void LoadNodes()
        {
            var canvasLayer = GetTree()?.CurrentScene?.GetNodeOrNull<CanvasLayer>("CanvasLayer");
            if (canvasLayer != null)
            {
                var minimap = canvasLayer.GetNodeOrNull<Control>("MiniMapUI");
                if (minimap != null)
                {
                    _mapTexture = minimap.GetNodeOrNull<TextureRect>("MapTexture");
                    _playerMarker = minimap.GetNodeOrNull<Control>("PlayerMarker");
                    _npcMarkers = minimap.GetNodeOrNull<Control>("NPCMarkers");
                    _enemyMarkers = minimap.GetNodeOrNull<Control>("EnemyMarkers");
                    _poiMarkers = minimap.GetNodeOrNull<Control>("POIMarkers");
                    _areaNameLabel = minimap.GetNodeOrNull<Label>("AreaNameLabel");
                    _compassLabel = minimap.GetNodeOrNull<Label>("CompassLabel");
                }
            }

            // 降级查找
            if (_mapTexture == null)
                _mapTexture = GetNodeOrNull<TextureRect>("MapTexture");
            if (_playerMarker == null)
                _playerMarker = GetNodeOrNull<Control>("PlayerMarker");
            if (_npcMarkers == null)
                _npcMarkers = GetNodeOrNull<Control>("NPCMarkers");
            if (_enemyMarkers == null)
                _enemyMarkers = GetNodeOrNull<Control>("EnemyMarkers");
            if (_poiMarkers == null)
                _poiMarkers = GetNodeOrNull<Control>("POIMarkers");
            if (_areaNameLabel == null)
                _areaNameLabel = GetNodeOrNull<Label>("AreaNameLabel");
            if (_compassLabel == null)
                _compassLabel = GetNodeOrNull<Label>("CompassLabel");
        }

        public void Initialize(Main main)
        {
            _main = main;
            _player = GetTree()?.GetFirstNodeInGroup("player") as Player;
            UpdateCompass();
        }

        public override void _Process(double delta)
        {
            if (!IsVisible) return;

            _updateTimer += (float)delta;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;
                Refresh();
            }
        }

        protected override void OnShow()
        {
            GD.Print("[MiniMapUI] Minimap shown");
            Refresh();
        }

        protected override void OnHide()
        {
            GD.Print("[MiniMapUI] Minimap hidden");
        }

        protected override void OnRefresh()
        {
            UpdatePlayerPosition();
            UpdateMarkers();
            UpdateCompass();
            UpdateAreaName();
        }

        private void UpdatePlayerPosition()
        {
            if (_player == null || _playerMarker == null) return;

            if (_followPlayer && _player.HasMethod("GetGlobalPosition"))
            {
                Vector2 playerPos = (Vector2)_player.Get("GetGlobalPosition").DynamicInvoke();

                // 计算相对位置
                Vector2 mapCenter = _mapTexture?.Size / 2 ?? new Vector2(100, 100);
                Vector2 relativePos = playerPos * _mapScale;

                // 限制在地图范围内
                if (_mapTexture != null)
                {
                    relativePos.X = Mathf.Clamp(relativePos.X, 0, _mapTexture.Size.X);
                    relativePos.Y = Mathf.Clamp(relativePos.Y, 0, _mapTexture.Size.Y);
                }

                _playerMarker.Position = relativePos - (_playerMarker.Size / 2);

                // 旋转标记指向玩家朝向
                if (_player.HasMethod("GetRotation"))
                {
                    float rotation = (float)_player.Get("GetRotation").DynamicInvoke();
                    _playerMarker.Rotation = rotation;
                }
            }
        }

        private void UpdateMarkers()
        {
            // 更新 NPC 标记
            if (_showNPCMarkers && _npcMarkers != null)
            {
                UpdateMarkerGroup(_npcMarkers, "npc", Colors.Yellow);
            }

            // 更新敌人标记
            if (_showEnemyMarkers && _enemyMarkers != null)
            {
                UpdateMarkerGroup(_enemyMarkers, "enemy", Colors.Red);
            }

            // 更新兴趣点标记
            if (_showPOIMarkers && _poiMarkers != null)
            {
                UpdateMarkerGroup(_poiMarkers, "poi", Colors.Cyan);
            }
        }

        private void UpdateMarkerGroup(Control container, string group, Color color)
        {
            var nodes = GetTree()?.GetNodesInGroup(group);
            if (nodes == null) return;

            // 清空现有标记（简单实现：重新生成）
            foreach (Node child in container.GetChildren())
            {
                child.QueueFree();
            }

            foreach (Node node in nodes)
            {
                if (node is Node2D npc && npc.HasMethod("GetGlobalPosition"))
                {
                    Vector2 pos = (Vector2)npc.Get("GetGlobalPosition").DynamicInvoke();
                    AddMarker(container, pos, color, 4);
                }
            }
        }

        private void AddMarker(Control container, Vector2 worldPos, Color color, float size)
        {
            var marker = new ColorRect
            {
                Color = color,
                CustomMinimumSize = new Vector2(size, size),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };

            // 计算地图相对位置
            if (_mapTexture != null)
            {
                Vector2 mapCenter = _mapTexture.Size / 2;
                Vector2 relativePos = worldPos * _mapScale;
                marker.Position = relativePos - new Vector2(size / 2, size / 2);
            }

            container.AddChild(marker);
        }

        private void UpdateCompass()
        {
            if (_compassLabel == null || _player == null) return;

            if (_player.HasMethod("GetRotation"))
            {
                float rotation = (float)_player.Get("GetRotation").DynamicInvoke();
                float degrees = Mathf.RadToDeg(rotation) % 360;
                if (degrees < 0) degrees += 360;

                string direction;
                if (degrees >= 315 || degrees < 45)
                    direction = "N";
                else if (degrees >= 45 && degrees < 135)
                    direction = "E";
                else if (degrees >= 135 && degrees < 225)
                    direction = "S";
                else
                    direction = "W";

                _compassLabel.Text = direction;
            }
        }

        private void UpdateAreaName()
        {
            if (_areaNameLabel == null) return;

            // 从场景树或 Player 获取当前区域名
            if (_main != null && _main.HasMethod("GetCurrentAreaName"))
            {
                _areaNameLabel.Text = (string)_main.Get("GetCurrentAreaName").DynamicInvoke();
            }
            else
            {
                _areaNameLabel.Text = "Unknown Area";
            }
        }

        public void SetFollowPlayer(bool follow)
        {
            _followPlayer = follow;
        }

        public void SetMapScale(float scale)
        {
            _mapScale = Mathf.Clamp(scale, 0.1f, 10.0f);
        }

        public void ToggleNPCMarkers()
        {
            _showNPCMarkers = !_showNPCMarkers;
            if (_npcMarkers != null)
                _npcMarkers.Visible = _showNPCMarkers;
        }

        public void ToggleEnemyMarkers()
        {
            _showEnemyMarkers = !_showEnemyMarkers;
            if (_enemyMarkers != null)
                _enemyMarkers.Visible = _showEnemyMarkers;
        }

        public void TogglePOIMarkers()
        {
            _showPOIMarkers = !_showPOIMarkers;
            if (_poiMarkers != null)
                _poiMarkers.Visible = _showPOIMarkers;
        }

        public void FocusOnPoint(Vector2 worldPosition)
        {
            if (_mapTexture == null) return;

            // 计算将指定世界坐标置于地图中心需要的偏移
            Vector2 targetPos = worldPosition * _mapScale;
            Vector2 mapCenter = _mapTexture.Size / 2;
            Vector2 offset = mapCenter - targetPos;

            // 应用到所有标记容器
            if (_npcMarkers != null) _npcMarkers.Position = offset;
            if (_enemyMarkers != null) _enemyMarkers.Position = offset;
            if (_poiMarkers != null) _poiMarkers.Position = offset;
        }

        public Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                ["UIName"] = UIName,
                ["ShowNPCMarkers"] = _showNPCMarkers,
                ["ShowEnemyMarkers"] = _showEnemyMarkers,
                ["ShowPOIMarkers"] = _showPOIMarkers,
                ["MapScale"] = _mapScale
            };
        }

        public void ImportSaveData(Dictionary<string, object> data)
        {
            base.ImportSaveData(data);

            if (data.ContainsKey("ShowNPCMarkers"))
                _showNPCMarkers = Convert.ToBoolean(data["ShowNPCMarkers"]);
            if (data.ContainsKey("ShowEnemyMarkers"))
                _showEnemyMarkers = Convert.ToBoolean(data["ShowEnemyMarkers"]);
            if (data.ContainsKey("ShowPOIMarkers"))
                _showPOIMarkers = Convert.ToBoolean(data["ShowPOIMarkers"]);
            if (data.ContainsKey("MapScale"))
                _mapScale = Convert.ToSingle(data["MapScale"]);
        }

        public override void _ExitTree()
        {
            Instance = null;
        }
    }
}
