using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 符文系统UI
    /// </summary>
    public class RuneUI : Control {
        // UI组件
        private PanelContainer _mainPanel;
        private VBoxContainer _contentVBox;
        private HBoxContainer _runeSlotsContainer;
        private GridContainer _runeInventoryGrid;
        private Label _titleLabel;
        private Label _goldLabel;
        private Button _closeButton;
        
        // 符文槽位显示
        private RuneSlotDisplay[] _slotDisplays;
        
        // 当前选中的符文
        private Rune _selectedRune;
        private int _selectedSlotIndex = -1;
        
        // 玩家引用
        private Node _player;
        
        // 是否可见
        private bool _isVisible = false;

        public override void _Ready() {
            SetupUI();
            ConnectSignals();
            
            // 初始隐藏
            Hide();
        }

        private void SetupUI() {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(900, 600);
            AddChild(_mainPanel);

            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", style);

            // 内容容器
            _contentVBox = new VBoxContainer();
            _contentVBox.Setanchorspreset(Control.LayoutPreset.FullRect);
            _contentVBox.AddThemeConstantOverride("separation", 15);
            _mainPanel.AddChild(_contentVBox);

            // 标题栏
            var titleBar = new HBoxContainer();
            titleBar.AddThemeConstantOverride("separation", 10);
            _contentVBox.AddChild(titleBar);

            _titleLabel = new Label();
            _titleLabel.Text = "  🔮 符文系统";
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            titleBar.AddChild(_titleLabel);

            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            _goldLabel = new Label();
            _goldLabel.Text = "💰 0";
            _goldLabel.AddThemeFontSizeOverride("font_size", 18);
            _goldLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
            titleBar.AddChild(_goldLabel);

            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _closeButton.AddThemeFontSizeOverride("font_size", 18);
            titleBar.AddChild(_closeButton);

            // 符文槽位区域
            var slotsLabel = new Label();
            slotsLabel.Text = "装备槽位";
            slotsLabel.AddThemeFontSizeOverride("font_size", 16);
            slotsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
            _contentVBox.AddChild(slotsLabel);

            _runeSlotsContainer = new HBoxContainer();
            _runeSlotsContainer.Alignment = BoxContainer.AlignmentMode.Center;
            _runeSlotsContainer.AddThemeConstantOverride("separation", 15);
            _contentVBox.AddChild(_runeSlotsContainer);

            // 创建5个装备槽位
            _slotDisplays = new RuneSlotDisplay[5];
            for (int i = 0; i < 5; i++) {
                _slotDisplays[i] = new RuneSlotDisplay(i);
                _slotDisplays[i].Connect("slot_clicked", this, nameof(OnSlotClicked));
                _slotDisplays[i].Connect("slot_unlock_requested", this, nameof(OnSlotUnlockRequested));
                _runeSlotsContainer.AddChild(_slotDisplays[i]);
            }

            // 分割线
            var hsep = new HSeparator();
            hsep.AddThemeColorOverride("separator", new Color(0.4f, 0.4f, 0.5f));
            _contentVBox.AddChild(hsep);

            // 符文背包
            var inventoryLabel = new Label();
            inventoryLabel.Text = "符文背包";
            inventoryLabel.AddThemeFontSizeOverride("font_size", 16);
            inventoryLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
            _contentVBox.AddChild(inventoryLabel);

            _runeInventoryGrid = new GridContainer();
            _runeInventoryGrid.Columns = 8;
            _runeInventoryGrid.AddThemeConstantOverride("h_separation", 8);
            _runeInventoryGrid.AddThemeConstantOverride("v_separation", 8);
            _contentVBox.AddChild(_runeInventoryGrid);

            // 填充空格子
            RefreshInventoryGrid();
        }

        private void ConnectSignals() {
            _closeButton.Connect("pressed", this, nameof(OnClosePressed));
            
            if (RuneManager.Instance != null) {
                RuneManager.Instance.OnRunesUpdated += RefreshInventoryGrid;
                RuneManager.Instance.OnRuneEquipped += RefreshSlots;
                RuneManager.Instance.OnSlotUnlocked += RefreshSlots;
            }
        }

        private void RefreshInventoryGrid() {
            // 清空
            foreach (Node child in _runeInventoryGrid.GetChildren()) {
                child.QueueFree();
            }

            var runes = RuneManager.Instance.GetOwnedRunes();
            int inventorySize = RuneManager.Instance.GetInventoryCapacity();

            // 显示拥有的符文
            foreach (Rune rune in runes) {
                var runeButton = CreateRuneButton(rune);
                _runeInventoryGrid.AddChild(runeButton);
            }

            // 添加空格子
            int emptySlots = inventorySize - runes.Count;
            for (int i = 0; i < emptySlots; i++) {
                var emptySlot = new PanelContainer();
                emptySlot.CustomMinimumSize = new Vector2(50, 50);
                
                var emptyStyle = new StyleBoxFlat();
                emptyStyle.BgColor = new Color(0.2f, 0.2f, 0.25f);
                emptyStyle.BorderColor = new Color(0.3f, 0.3f, 0.35f);
                emptyStyle.SetBorderWidthAll(1);
                emptySlot.AddThemeStyleboxOverride("panel", emptyStyle);
                
                _runeInventoryGrid.AddChild(emptySlot);
            }
        }

        private Button CreateRuneButton(Rune rune) {
            var button = new Button();
            button.CustomMinimumSize = new Vector2(50, 50);
            button.TooltipText = $"{rune.Name}\n{rune.Description}\n\n类型: {rune.Type}\n稀有度: {rune.Rarity}";

            // 背景样式
            var style = new StyleBoxFlat();
            style.BgColor = rune.GetRarityColor() * new Color(0.3f, 0.3f, 0.3f);
            style.BorderColor = rune.GetRarityColor();
            style.SetBorderWidthAll(2);
            button.AddThemeStyleboxOverride("normal", style);

            // 选中样式
            var hoverStyle = style.Duplicate() as StyleBoxFlat;
            hoverStyle.BgColor = rune.GetRarityColor() * new Color(0.5f, 0.5f, 0.5f);
            button.AddThemeStyleboxOverride("hover", hoverStyle);

            // 标签
            var label = new Label();
            label.Text = GetRuneTypeIcon(rune.Type);
            label.SetAnchorsPreset(Control.LayoutPreset.Center);
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.AddThemeFontSizeOverride("font_size", 20);
            button.AddChild(label);

            button.Connect("pressed", this, nameof(OnRuneButtonPressed), new Godot.Collections.Array { rune });

            return button;
        }

        private string GetRuneTypeIcon(RuneType type) {
            return type switch {
                RuneType.Attack => "⚔️",
                RuneType.Defense => "🛡️",
                RuneType.Magic => "🔮",
                RuneType.Utility => "✨",
                RuneType.Legendary => "⭐",
                _ => "💎"
            };
        }

        private void RefreshSlots() {
            var slots = RuneManager.Instance.GetEquipmentSlots();
            for (int i = 0; i < 5; i++) {
                _slotDisplays[i].UpdateSlot(slots[i]);
            }
            
            UpdateGoldDisplay();
        }

        private void UpdateGoldDisplay() {
            if (_player != null) {
                var playerScript = _player as Characters.Player;
                if (playerScript != null) {
                    _goldLabel.Text = $"💰 {playerScript.Gold}";
                }
            }
        }

        public void Toggle() {
            if (_isVisible) {
                Hide();
            } else {
                Show();
            }
        }

        public override void _Input(InputEvent @event) {
            if (@event.IsActionPressed("ui_cancel")) {
                if (_isVisible) {
                    Hide();
                }
            }
        }

        private void OnClosePressed() {
            Hide();
        }

        private void Show() {
            Visible = true;
            _isVisible = true;
            RefreshInventoryGrid();
            RefreshSlots();
            
            // 获取玩家引用
            _player = GetTree().CurrentScene.GetNodeOrNull("Player");
        }

        private void Hide() {
            Visible = false;
            _isVisible = false;
            _selectedRune = null;
            _selectedSlotIndex = -1;
        }

        private void OnSlotClicked(int slotIndex) {
            _selectedSlotIndex = slotIndex;
            var equippedRune = RuneManager.Instance.GetEquippedRune(slotIndex);
            
            if (equippedRune != null) {
                // 卸下符文
                RuneManager.Instance.UnequipRune(slotIndex);
            }
        }

        private void OnSlotUnlockRequested(int slotIndex) {
            int cost = RuneManager.Instance.GetSlotUnlockCost(slotIndex);
            
            if (_player != null) {
                var playerScript = _player as Characters.Player;
                if (playerScript != null) {
                    if (playerScript.Gold >= cost) {
                        if (RuneManager.Instance.UnlockSlot(slotIndex, playerScript.Gold)) {
                            playerScript.Gold -= cost;
                            UpdateGoldDisplay();
                        }
                    }
                }
            }
        }

        private void OnRuneButtonPressed(Rune rune) {
            if (_selectedSlotIndex >= 0 && RuneManager.Instance.IsSlotUnlocked(_selectedSlotIndex)) {
                // 装备到选中的槽位
                RuneManager.Instance.EquipRune(rune, _selectedSlotIndex);
                _selectedSlotIndex = -1;
            }
        }
    }

    /// <summary>
    /// 符文槽位显示组件
    /// </summary>
    public class RuneSlotDisplay : Control {
        public int SlotIndex { get; private set; }
        
        private PanelContainer _slotPanel;
        private Label _iconLabel;
        private Label _indexLabel;
        private Label _unlockLabel;
        private Button _slotButton;
        
        public delegate void SlotEvent(int index);
        public event SlotEvent SlotClicked;
        public event SlotEvent SlotUnlockRequested;

        public RuneSlotDisplay(int index) {
            SlotIndex = index;
            CustomMinimumSize = new Vector2(80, 100);
            SetupUI();
        }

        private void SetupUI() {
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 5);
            AddChild(vbox);

            // 槽位按钮
            _slotButton = new Button();
            _slotButton.CustomMinimumSize = new Vector2(70, 70);
            _slotButton.Connect("pressed", this, nameof(OnSlotPressed));
            vbox.AddChild(_slotButton);

            // 槽位背景
            var slotStyle = new StyleBoxFlat();
            slotStyle.BgColor = new Color(0.15f, 0.15f, 0.2f);
            slotStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
            slotStyle.SetBorderWidthAll(2);
            _slotButton.AddThemeStyleboxOverride("normal", slotStyle);

            var slotHoverStyle = slotStyle.Duplicate() as StyleBoxFlat;
            slotHoverStyle.BgColor = new Color(0.25f, 0.25f, 0.3f);
            _slotButton.AddThemeStyleboxOverride("hover", slotHoverStyle);

            // 图标
            _iconLabel = new Label();
            _iconLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _iconLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _iconLabel.VerticalAlignment = VerticalAlignment.Center;
            _iconLabel.AddThemeFontSizeOverride("font_size", 28);
            _slotButton.AddChild(_iconLabel);

            // 索引标签
            _indexLabel = new Label();
            _indexLabel.Text = $"槽{SlotIndex + 1}";
            _indexLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _indexLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _indexLabel.AddThemeFontSizeOverride("font_size", 12);
            _indexLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
            vbox.AddChild(_indexLabel);

            // 解锁标签
            _unlockLabel = new Label();
            _unlockLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _unlockLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _unlockLabel.AddThemeFontSizeOverride("font_size", 11);
            vbox.AddChild(_unlockLabel);
        }

        public void UpdateSlot(EquipmentRuneSlot slot) {
            if (slot.IsUnlocked) {
                if (slot.EquippedRune != null) {
                    _iconLabel.Text = GetRuneTypeIcon(slot.EquippedRune.Type);
                    _iconLabel.Modulate = slot.EquippedRune.GetRarityColor();
                    _unlockLabel.Text = "";
                } else {
                    _iconLabel.Text = "💎";
                    _iconLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
                    _unlockLabel.Text = "点击装备";
                    _unlockLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
                }
            } else {
                _iconLabel.Text = "🔒";
                _iconLabel.Modulate = new Color(0.4f, 0.4f, 0.4f);
                _unlockLabel.Text = $"解锁: {slot.UnlockCost}💰";
                _unlockLabel.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.3f));
                
                // 点击解锁
                _slotButton.Disconnect("pressed", this, nameof(OnSlotPressed));
                _slotButton.Connect("pressed", this, nameof(OnUnlockPressed));
            }
        }

        private string GetRuneTypeIcon(RuneType type) {
            return type switch {
                RuneType.Attack => "⚔️",
                RuneType.Defense => "🛡️",
                RuneType.Magic => "🔮",
                RuneType.Utility => "✨",
                RuneType.Legendary => "⭐",
                _ => "💎"
            };
        }

        private void OnSlotPressed() {
            SlotClicked?.Invoke(SlotIndex);
        }

        private void OnUnlockPressed() {
            SlotUnlockRequested?.Invoke(SlotIndex);
        }
    }
}
