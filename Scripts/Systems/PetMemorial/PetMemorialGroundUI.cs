using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetMemorial
{
    /// <summary>
    /// 宠物墓园 UI（REQ-201 子任务1）
    /// Safe House 深处专属墓园场景
    /// </summary>
    public partial class PetMemorialGroundUI : CanvasLayer
    {
        // ========== Constants ==========
        private const float MARKER_SPACING_X = 220f;
        private const float MARKER_SPACING_Y = 180f;
        private const float ROWS_PER_PAGE = 3;
        private const int MARKERS_PER_PAGE = 6;
        private const float DETAIL_PANEL_WIDTH = 400f;
        private const float DETAIL_PANEL_HEIGHT = 500f;

        // Tombstone style colors
        private static readonly Color NEW_STONE_COLOR = new Color(0.72f, 0.70f, 0.68f);
        private static readonly Color WEATHERED_STONE_COLOR = new Color(0.55f, 0.53f, 0.50f);
        private static readonly Color ANCIENT_STONE_COLOR = new Color(0.42f, 0.40f, 0.38f);

        // ========== Nodes ==========
        private Control _rootContainer;
        private Control _markerContainer;
        private Control _entrancePanel;
        private Control _collectiveMonument;
        private Control _detailPanel;
        private Control _background;
        private Label _titleLabel;
        private Label _emptyLabel;
        private Label _detailEpitaphLabel;
        private Label _detailStatsLabel;
        private Label _detailComboLabel;
        private int _currentPage = 0;
        private Dictionary<int, Control> _markerNodes = new Dictionary<int, Control>();
        private MemorialMarkerEntry _selectedMarker;

        public override void _Ready()
        {
            base._Ready();
            SetupBackground();
            SetupEntrancePanel();
            SetupTitleBar();
            SetupMarkerContainer();
            SetupCollectiveMonument();
            SetupDetailPanel();
            SetupEmptyState();
            SubscribeToSignals();

            // 初始状态：未解锁时只显示入口
            UpdateVisibility();
        }

        private void SetupBackground()
        {
            _background = new Control();
            _background.Name = "MemorialBackground";
            _background.SetAnchorsPreset(Control.LayoutPreset.Wide);
            _background.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _background.SizeFlagsVertical = Control.SizeFlags.Expand;

            // 墓园黄昏氛围背景（深蓝灰色渐变效果，用纯色模拟）
            var bgPanel = new Panel();
            bgPanel.Name = "BGPanel";
            bgPanel.SetAnchorsPreset(Control.LayoutPreset.Wide);
            bgPanel.Modulate = new Color(0.08f, 0.07f, 0.12f, 0.97f);
            _background.AddChild(bgPanel);

            AddChild(_background);

            // 萤火虫粒子容器（使用 Timer 模拟）
            var fireflyTimer = new Timer();
            fireflyTimer.Name = "FireflyTimer";
            fireflyTimer.WaitTime = 3f;
            fireflyTimer.OneShot = false;
            fireflyTimer.Timeout += SpawnFireflyParticle;
            AddChild(fireflyTimer);
            fireflyTimer.Start();
        }

        private void SpawnFireflyParticle()
        {
            if (!IsVisible()) return;

            // 随机在墓园范围内生成萤火虫光点（用 TextureRect 模拟）
            var firefly = new Control();
            firefly.Name = "Firefly";
            firefly.Position = new Vector2(GD.Randf() * 1200 + 100, GD.Randf() * 700 + 50);
            firefly.Size = new Vector2(6, 6);

            var dot = new ColorRect();
            dot.Name = "Dot";
            dot.Color = new Color(0.8f, 1f, 0.4f, 0.6f);
            dot.SetAnchorsPreset(Control.LayoutPreset.Center);
            firefly.AddChild(dot);

            _background.AddChild(firefly);

            // 淡入淡出动画
            var tween = CreateTween();
            tween.TweenProperty(dot, "modulate:a", 0f, 2.5f).From(0.8f);
            tween.TweenCallback(Callable.From(firefly.QueueFree));
        }

        private void SetupEntrancePanel()
        {
            _entrancePanel = new Control();
            _entrancePanel.Name = "EntrancePanel";
            _entrancePanel.Position = new Vector2(0, 0);
            _entrancePanel.Size = new Vector2(1920, 150);

            var panel = new Panel();
            panel.Name = "Panel";
            panel.SetAnchorsPreset(Control.LayoutPreset.Wide);
            panel.Modulate = new Color(0.10f, 0.08f, 0.14f, 0.90f);
            _entrancePanel.AddChild(panel);

            _titleLabel = new Label();
            _titleLabel.Name = "TitleLabel";
            _titleLabel.Text = "宠物墓园";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.VerticalAlignment = VerticalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 32);
            _titleLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.82f, 0.90f));
            _titleLabel.Position = new Vector2(0, 20);
            _titleLabel.Size = new Vector2(1920, 60);
            _entrancePanel.AddChild(_titleLabel);

            AddChild(_entrancePanel);
        }

        private void SetupTitleBar()
        {
            // 顶部信息栏（已包含在 entrancePanel 中）
        }

        private void SetupMarkerContainer()
        {
            _markerContainer = new Control();
            _markerContainer.Name = "MarkerContainer";
            _markerContainer.Position = new Vector2(0, 160);
            _markerContainer.Size = new Vector2(1920, 600);
            _markerContainer.SetAnchorsPreset(Control.LayoutPreset.Wide);
            AddChild(_markerContainer);
        }

        private void SetupCollectiveMonument()
        {
            _collectiveMonument = new Control();
            _collectiveMonument.Name = "CollectiveMonument";
            _collectiveMonument.Position = new Vector2(1920 / 2 - 100, 300);
            _collectiveMonument.Size = new Vector2(200, 250);
            _collectiveMonument.Visible = false;

            // 集体纪念碑主体
            var monument = new Panel();
            monument.Name = "MonumentPanel";
            monument.SetAnchorsPreset(Control.LayoutPreset.Center);
            monument.Modulate = new Color(0.50f, 0.48f, 0.55f);
            monument.Size = new Vector2(160, 200);
            _collectiveMonument.AddChild(monument);

            var label = new Label();
            label.Name = "Label";
            label.Text = "集体纪念碑";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.AddThemeFontSizeOverride("font_size", 16);
            label.AddThemeColorOverride("font_color", new Color(0.90f, 0.88f, 0.95f));
            label.Position = new Vector2(0, 70);
            label.Size = new Vector2(160, 60);
            monument.AddChild(label);

            var countLabel = new Label();
            countLabel.Name = "CountLabel";
            countLabel.Text = "已纪念 N 只宠物";
            countLabel.HorizontalAlignment = HorizontalAlignment.Center;
            countLabel.AddThemeFontSizeOverride("font_size", 12);
            countLabel.AddThemeColorOverride("font_color", new Color(0.70f, 0.68f, 0.75f));
            countLabel.Position = new Vector2(0, 120);
            countLabel.Size = new Vector2(160, 40);
            monument.AddChild(countLabel);

            AddChild(_collectiveMonument);
        }

        private void SetupEmptyState()
        {
            _emptyLabel = new Label();
            _emptyLabel.Name = "EmptyLabel";
            _emptyLabel.Text = "这里还没有墓碑。\n当你的宠物为你战死时，\n它们会在这里安息。";
            _emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _emptyLabel.VerticalAlignment = VerticalAlignment.Center;
            _emptyLabel.AddThemeFontSizeOverride("font_size", 20);
            _emptyLabel.AddThemeColorOverride("font_color", new Color(0.55f, 0.52f, 0.60f));
            _emptyLabel.Position = new Vector2(1920 / 2 - 200, 300);
            _emptyLabel.Size = new Vector2(400, 150);
            _emptyLabel.Visible = false;
            AddChild(_emptyLabel);
        }

        private void SetupDetailPanel()
        {
            _detailPanel = new Control();
            _detailPanel.Name = "DetailPanel";
            _detailPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _detailPanel.Position = new Vector2(1920 / 2 - DETAIL_PANEL_WIDTH / 2, 1080 / 2 - DETAIL_PANEL_HEIGHT / 2);
            _detailPanel.Size = new Vector2(DETAIL_PANEL_WIDTH, DETAIL_PANEL_HEIGHT);
            _detailPanel.Visible = false;

            // 背景
            var bg = new Panel();
            bg.Name = "BGPanel";
            bg.SetAnchorsPreset(Control.LayoutPreset.Wide);
            bg.Modulate = new Color(0.10f, 0.08f, 0.14f, 0.96f);
            _detailPanel.AddChild(bg);

            // 标题
            var nameLabel = new Label();
            nameLabel.Name = "NameLabel";
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 26);
            nameLabel.AddThemeColorOverride("font_color", new Color(0.92f, 0.90f, 0.97f));
            nameLabel.Position = new Vector2(20, 20);
            nameLabel.Size = new Vector2(DETAIL_PANEL_WIDTH - 40, 50);
            _detailPanel.AddChild(nameLabel);

            // 墓志铭
            _detailEpitaphLabel = new Label();
            _detailEpitaphLabel.Name = "EpitaphLabel";
            _detailEpitaphLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _detailEpitaphLabel.AddThemeFontSizeOverride("font_size", 16);
            _detailEpitaphLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.72f, 0.80f));
            _detailEpitaphLabel.Position = new Vector2(20, 75);
            _detailEpitaphLabel.Size = new Vector2(DETAIL_PANEL_WIDTH - 40, 40);
            _detailPanel.AddChild(_detailEpitaphLabel);

            // 统计数据
            _detailStatsLabel = new Label();
            _detailStatsLabel.Name = "StatsLabel";
            _detailStatsLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _detailStatsLabel.AddThemeFontSizeOverride("font_size", 15);
            _detailStatsLabel.AddThemeColorOverride("font_color", new Color(0.82f, 0.80f, 0.88f));
            _detailStatsLabel.Position = new Vector2(40, 130);
            _detailStatsLabel.Size = new Vector2(DETAIL_PANEL_WIDTH - 80, 150);
            _detailStatsLabel.Text = "";
            _detailPanel.AddChild(_detailStatsLabel);

            // Combo信息
            _detailComboLabel = new Label();
            _detailComboLabel.Name = "ComboLabel";
            _detailComboLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _detailComboLabel.AddThemeFontSizeOverride("font_size", 14);
            _detailComboLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.62f, 0.72f));
            _detailComboLabel.Position = new Vector2(40, 280);
            _detailComboLabel.Size = new Vector2(DETAIL_PANEL_WIDTH - 80, 40);
            _detailPanel.AddChild(_detailComboLabel);

            // 讣告文本
            var obituaryScroll = new ScrollContainer();
            obituaryScroll.Name = "ObituaryScroll";
            obituaryScroll.Position = new Vector2(40, 330);
            obituaryScroll.Size = new Vector2(DETAIL_PANEL_WIDTH - 80, 120);

            var obituaryLabel = new Label();
            obituaryLabel.Name = "ObituaryLabel";
            obituaryLabel.AddThemeFontSizeOverride("font_size", 13);
            obituaryLabel.AddThemeColorOverride("font_color", new Color(0.70f, 0.68f, 0.78f));
            obituaryLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            obituaryScroll.AddChild(obituaryLabel);

            _detailPanel.AddChild(obituaryScroll);

            // 关闭按钮
            var closeBtn = new Button();
            closeBtn.Name = "CloseButton";
            closeBtn.Text = "关闭";
            closeBtn.Position = new Vector2(DETAIL_PANEL_WIDTH / 2 - 50, DETAIL_PANEL_HEIGHT - 50);
            closeBtn.Size = new Vector2(100, 35);
            closeBtn.Pressed += () => HideDetailPanel();
            _detailPanel.AddChild(closeBtn);

            AddChild(_detailPanel);
        }

        private void SubscribeToSignals()
        {
            if (PetMemorialGroundSystem.Instance != null)
            {
                PetMemorialGroundSystem.Instance.OnMarkerAdded += OnMarkerAdded_System;
                PetMemorialGroundSystem.Instance.OnMemorialUnlocked += OnMemorialUnlocked_System;
                PetMemorialGroundSystem.Instance.OnCollectiveMonumentUnveiled += OnCollectiveMonument_System;
            }
        }

        // ========== Signal Handlers ==========

        private void OnMemorialUnlocked_System()
        {
            UpdateVisibility();
            RefreshAllMarkers();
            GD.Print("[PetMemorialUI] Memorial unlocked!");
        }

        private void OnMarkerAdded_System(MemorialMarkerEntry marker)
        {
            CreateMarkerNode(marker);
            UpdateVisibility();
        }

        private void OnCollectiveMonument_System()
        {
            if (_collectiveMonument != null)
            {
                _collectiveMonument.Visible = true;
                var countLabel = _collectiveMonument.GetNodeOrNull<Label>("MonumentPanel/CountLabel");
                if (countLabel != null)
                    countLabel.Text = $"已纪念 {PetMemorialGroundSystem.Instance.GetTotalDeaths()} 只宠物";
            }
        }

        // ========== Marker Node Creation ==========

        private void CreateMarkerNode(MemorialMarkerEntry marker)
        {
            var markers = PetMemorialGroundSystem.Instance.GetAllMarkers();
            int index = markers.IndexOf(marker);
            if (index < 0) return;

            int col = index % 3;
            int row = index / 3;
            float x = 1920 / 2 - (3 * MARKER_SPACING_X) / 2 + col * MARKER_SPACING_X;
            float y = 200 + row * MARKER_SPACING_Y;

            var markerNode = BuildMarkerVisual(marker);
            markerNode.Position = new Vector2(x, y);
            _markerContainer.AddChild(markerNode);
            _markerNodes[marker.PetId] = markerNode;
        }

        private Control BuildMarkerVisual(MemorialMarkerEntry marker)
        {
            var container = new Control();
            container.Name = $"Marker_{marker.PetId}";
            container.Size = new Vector2(180, 160);

            // 墓碑颜色（基于老化程度）
            Color stoneColor = marker.TombstoneStyle switch
            {
                0 => NEW_STONE_COLOR,
                1 => WEATHERED_STONE_COLOR,
                2 => ANCIENT_STONE_COLOR,
                _ => NEW_STONE_COLOR
            };

            // 墓碑本体
            var stone = new Panel();
            stone.Name = "Stone";
            stone.SetAnchorsPreset(Control.LayoutPreset.Center);
            stone.Position = new Vector2(-40, -60);
            stone.Size = new Vector2(80, 100);
            stone.Modulate = stoneColor;

            // 墓碑顶部（圆形）
            var stoneTop = new Panel();
            stoneTop.Name = "StoneTop";
            stoneTop.SetAnchorsPreset(Control.LayoutPreset.Center);
            stoneTop.Position = new Vector2(-35, -85);
            stoneTop.Size = new Vector2(70, 40);
            stoneTop.Modulate = stoneColor;
            container.AddChild(stoneTop);
            container.AddChild(stone);

            // 升华光环
            if (marker.IsTranscended)
            {
                var aura = new Panel();
                aura.Name = "TranscendedAura";
                aura.SetAnchorsPreset(Control.LayoutPreset.Center);
                aura.Position = new Vector2(-50, -90);
                aura.Size = new Vector2(100, 120);
                aura.Modulate = new Color(0.6f, 0.5f, 1f, 0.20f);
                container.AddChild(aura);
            }

            // 宠物名字
            var nameLabel = new Label();
            nameLabel.Name = "NameLabel";
            nameLabel.Text = marker.PetName;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            nameLabel.AddThemeColorOverride("font_color", new Color(0.90f, 0.88f, 0.95f));
            nameLabel.Position = new Vector2(-40, 45);
            nameLabel.Size = new Vector2(80, 25);
            container.AddChild(nameLabel);

            // 战斗次数
            var battleLabel = new Label();
            battleLabel.Name = "BattleLabel";
            battleLabel.Text = $"{marker.TotalBattles}战";
            battleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            battleLabel.AddThemeFontSizeOverride("font_size", 11);
            battleLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.63f, 0.70f));
            battleLabel.Position = new Vector2(-40, 68);
            battleLabel.Size = new Vector2(80, 20);
            container.AddChild(battleLabel);

            // 点击区域（更大的透明区域）
            var clickArea = new Control();
            clickArea.Name = "ClickArea";
            clickArea.Position = new Vector2(-50, -80);
            clickArea.Size = new Vector2(100, 160);
            clickArea.InputRayPickable = true;

            // 连接点击信号
            clickArea.GuiInput += (InputEvent @event) =>
            {
                if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                {
                    OnMarkerClicked(marker);
                }
            };

            container.AddChild(clickArea);

            return container;
        }

        private void OnMarkerClicked(MemorialMarkerEntry marker)
        {
            _selectedMarker = marker;
            ShowDetailPanel(marker);
            PetMemorialGroundSystem.Instance.OnMarkerClicked_UI(marker.PetId);
        }

        private void ShowDetailPanel(MemorialMarkerEntry marker)
        {
            _detailPanel.Visible = true;

            var nameLabel = _detailPanel.GetNodeOrNull<Label>("NameLabel");
            if (nameLabel != null) nameLabel.Text = marker.PetName;

            _detailEpitaphLabel.Text = $"「{marker.Epitaph}」";

            _detailStatsLabel.Text =
                $"累计战斗: {marker.TotalBattles} 次\n" +
                $"累计击杀: {marker.TotalEnemiesKilled} 只\n" +
                $"死亡日期: {marker.GetDeathDateString()}\n" +
                $"最后结局: {marker.GetLastBattleOutcome()}\n" +
                $"友谊等级: Lv.{marker.FriendshipLevel}";

            _detailComboLabel.Text = string.IsNullOrEmpty(marker.MostUsedCombo)
                ? ""
                : $"常用Combo: {marker.MostUsedCombo.Replace("→", " → ")}";

            var obituaryLabel = _detailPanel.GetNodeOrNull<Label>("ObituaryScroll/ObituaryLabel");
            if (obituaryLabel != null)
                obituaryLabel.Text = string.IsNullOrEmpty(marker.ObituaryText)
                    ? ""
                    : marker.ObituaryText.Replace("\\n", "\n");

            // 淡入动画
            var tween = CreateTween();
            tween.TweenProperty(_detailPanel, "modulate:a", 1f, 0.15f).From(0f);
        }

        private void HideDetailPanel()
        {
            var tween = CreateTween();
            tween.TweenProperty(_detailPanel, "modulate:a", 0f, 0.15f);
            tween.TweenCallback(Callable.From(() => _detailPanel.Visible = false));
        }

        private void RefreshAllMarkers()
        {
            // 清除旧节点
            foreach (var node in _markerNodes.Values)
                node.QueueFree();
            _markerNodes.Clear();

            if (!PetMemorialGroundSystem.Instance.IsMemorialUnlocked()) return;

            // 重建所有节点
            var markers = PetMemorialGroundSystem.Instance.GetAllMarkers();
            foreach (var marker in markers)
                CreateMarkerNode(marker);

            // 更新集体纪念碑
            if (PetMemorialGroundSystem.Instance.IsCollectiveMonumentUnveiled())
            {
                _collectiveMonument.Visible = true;
                var countLabel = _collectiveMonument.GetNodeOrNull<Label>("MonumentPanel/CountLabel");
                if (countLabel != null)
                    countLabel.Text = $"已纪念 {markers.Count} 只宠物";
            }
        }

        private void UpdateVisibility()
        {
            bool unlocked = PetMemorialGroundSystem.Instance?.IsMemorialUnlocked() ?? false;

            _markerContainer.Visible = unlocked;
            _emptyLabel.Visible = !unlocked || (PetMemorialGroundSystem.Instance?.GetTotalDeaths() ?? 0) == 0;
            _entrancePanel.Visible = true;
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            // ESC 关闭详情面板
            if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
            {
                if (_detailPanel.Visible)
                    HideDetailPanel();
            }
        }
    }
}
