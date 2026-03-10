using Godot;
using Godot.Collections;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 装备强化界面
    /// </summary>
    public class EnhancementUI : Control {
        private Control _mainPanel;
        private VBoxContainer _mainContainer;
        private Label _titleLabel;
        private GridContainer _equipmentGrid;
        private VBoxContainer _detailPanel;
        private Label _itemNameLabel;
        private Label _itemStatsLabel;
        private Label _enhancementLevelLabel;
        private Label _successRateLabel;
        private Label _materialsLabel;
        private Button _enhanceButton;
        private Button _closeButton;
        
        // 强化石选择
        private OptionButton _stoneSelector;
        private Label _stoneInfoLabel;
        
        // 玩家装备数据
        private Array<string> _equipSlots = new();
        private string _selectedItemId = "";
        private int _selectedSlotIndex = -1;
        
        // 引用
        private Player _player;
        private EnhancementSystem _enhancementSystem;
        
        // 颜色
        private Color ColorCommon = new Color(0.7f, 0.7f, 0.7f);
        private Color ColorUncommon = new Color(0.2f, 0.8f, 0.2f);
        private Color ColorRare = new Color(0.3f, 0.5f, 1.0f);
        private Color ColorEpic = new Color(0.6f, 0.3f, 0.8f);
        private Color ColorLegendary = new Color(1.0f, 0.6f, 0.0f);
        
        public override void _Ready() {
            SetupUI();
            ConnectSignals();
            
            _enhancementSystem = EquipmentEnhancement.Instance;
            _player = GetNodeOrNull<Player>("/root/Main/Player");
            
            if (_enhancementSystem != null) {
                _enhancementSystem.OnEnhancementComplete += OnEnhancementComplete;
            }
            
            Visible = false;
        }
        
        private void SetupUI() {
            // 主面板
            _mainPanel = new Control();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(900, 600);
            AddChild(_mainPanel);
            
            // 背景
            var bg = new Panel();
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            bg.Modulate = new Color(0, 0, 0, 0.85f);
            _mainPanel.AddChild(bg);
            
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainContainer.AddThemeConstantOverride("separation", 20);
            _mainPanel.AddChild(_mainContainer);
            
            // 标题栏
            var titleBar = new HBoxContainer();
            _mainContainer.AddChild(titleBar);
            
            _titleLabel = new Label();
            _titleLabel.Text = "  装备强化";
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            titleBar.AddChild(_titleLabel);
            
            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlagsExpandFill });
            
            _closeButton = new Button();
            _closeButton.Text = "X";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _closeButton.Pressed += () => Hide();
            titleBar.AddChild(_closeButton);
            
            // 内容区域
            var content = new HBoxContainer();
            content.AddThemeConstantOverride("separation", 20);
            _mainContainer.AddChild(content);
            
            // 左侧装备列表
            var leftPanel = new VBoxContainer();
            leftPanel.CustomMinimumSize = new Vector2(350, 0);
            content.AddChild(leftPanel);
            
            var equipLabel = new Label();
            equipLabel.Text = "可强化装备";
            equipLabel.AddThemeFontSizeOverride("font_size", 18);
            leftPanel.AddChild(equipLabel);
            
            _equipmentGrid = new GridContainer();
            _equipmentGrid.Columns = 2;
            _equipmentGrid.AddThemeConstantOverride("h_separation", 10);
            _equipmentGrid.AddThemeConstantOverride("v_separation", 10);
            leftPanel.AddChild(_equipmentGrid);
            
            // 右侧详情面板
            _detailPanel = new VBoxContainer();
            _detailPanel.CustomMinimumSize = new Vector2(400, 0);
            content.AddChild(_detailPanel);
            
            _itemNameLabel = new Label();
            _itemNameLabel.Text = "选择一件装备";
            _itemNameLabel.AddThemeFontSizeOverride("font_size", 24);
            _detailPanel.AddChild(_itemNameLabel);
            
            _itemStatsLabel = new Label();
            _itemStatsLabel.Text = "";
            _itemStatsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _detailPanel.AddChild(_itemStatsLabel);
            
            _enhancementLevelLabel = new Label();
            _enhancementLevelLabel.Text = "强化等级: 0";
            _enhancementLevelLabel.AddThemeFontSizeOverride("font_size", 20);
            _detailPanel.AddChild(_enhancementLevelLabel);
            
            var separator = new HSeparator();
            _detailPanel.AddChild(separator);
            
            // 强化石选择
            var stoneLabel = new Label();
            stoneLabel.Text = "选择强化石:";
            stoneLabel.AddThemeFontSizeOverride("font_size", 18);
            _detailPanel.AddChild(stoneLabel);
            
            _stoneSelector = new OptionButton();
            _stoneSelector.CustomMinimumSize = new Vector2(300, 40);
            _stoneSelector.ItemSelected += OnStoneSelected;
            _detailPanel.AddChild(_stoneSelector);
            
            _stoneInfoLabel = new Label();
            _stoneInfoLabel.Text = "";
            _detailPanel.AddChild(_stoneInfoLabel);
            
            // 成功率
            _successRateLabel = new Label();
            _successRateLabel.Text = "成功率: --%";
            _successRateLabel.AddThemeFontSizeOverride("font_size", 20);
            _successRateLabel.Modulate = Color.Yellow;
            _detailPanel.AddChild(_successRateLabel);
            
            // 材料需求
            _materialsLabel = new Label();
            _materialsLabel.Text = "所需材料: --";
            _materialsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _detailPanel.AddChild(_materialsLabel);
            
            _detailPanel.AddChild(new Control() { SizeFlagsVertical = Control.SizeFlagsExpandFill });
            
            // 强化按钮
            _enhanceButton = new Button();
            _enhanceButton.Text = "开始强化";
            _enhanceButton.CustomMinimumSize = new Vector2(300, 60);
            _enhanceButton.AddThemeFontSizeOverride("font_size", 24);
            _enhanceButton.Pressed += OnEnhancePressed;
            _enhanceButton.Disabled = true;
            _detailPanel.AddChild(_enhanceButton);
            
            // 初始化强化石选项
            InitializeStoneOptions();
        }
        
        private void ConnectSignals() {
            if (Input.IsActionPressed("ui_cancel")) {
                // 未来可以绑定ESC关闭
            }
        }
        
        private void InitializeStoneOptions() {
            _stoneSelector.Clear();
            
            var stones = new string[] {
                "普通强化石",
                "优秀强化石",
                "稀有强化石",
                "史诗强化石",
                "传说强化石"
            };
            
            var stoneIds = new string[] {
                "401",
                "402",
                "403",
                "404",
                "405"
            };
            
            for (int i = 0; i < stones.Length; i++) {
                _stoneSelector.AddItem(stones[i], i);
            }
        }
        
        public void Show() {
            Visible = true;
            RefreshEquipmentList();
            _selectedItemId = "";
            _selectedSlotIndex = -1;
            UpdateDetailPanel();
        }
        
        public void Hide() {
            Visible = false;
        }
        
        private void RefreshEquipmentList() {
            foreach (Node child in _equipmentGrid.GetChildren()) {
                child.QueueFree();
            }
            
            _equipSlots.Clear();
            
            if (_player == null) return;
            
            var inventory = _player.GetInventory();
            if (inventory == null) return;
            
            // 获取所有装备槽物品
            var equipTypes = new string[] { "weapon", "armor", "accessory1", "accessory2" };
            var slotNames = new string[] { "武器", "防具", "饰品1", "饰品2" };
            
            for (int i = 0; i < equipTypes.Length; i++) {
                string equipSlot = equipTypes[i];
                string itemId = GetEquippedItem(equipSlot);
                
                if (!string.IsNullOrEmpty(itemId)) {
                    _equipSlots.Add(itemId);
                    
                    var slotBtn = CreateEquipmentSlot(itemId, slotNames[i], i);
                    _equipmentGrid.AddChild(slotBtn);
                }
            }
            
            // 如果没有装备，显示提示
            if (_equipSlots.Count == 0) {
                var emptyLabel = new Label();
                emptyLabel.Text = "没有可强化的装备";
                emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
                _equipmentGrid.AddChild(emptyLabel);
            }
        }
        
        private string GetEquippedItem(string slot) {
            // 从Player获取装备
            if (_player == null) return "";
            
            // 使用反射或直接访问
            var weapon = _player.Get("EquippedWeapon")?.ToString();
            var armor = _player.Get("EquippedArmor")?.ToString();
            
            switch (slot) {
                case "weapon": return weapon ?? "";
                case "armor": return armor ?? "";
                default: return "";
            }
        }
        
        private Control CreateEquipmentSlot(string itemId, string slotName, int index) {
            var container = new VBoxContainer();
            container.CustomMinimumSize = new Vector2(150, 120);
            
            var btn = new Button();
            btn.Text = slotName + "\n" + itemId;
            btn.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
            btn.Pressed += () => OnEquipmentSelected(itemId, index);
            container.AddChild(btn);
            
            // 显示强化等级
            int enhLevel = 0;
            if (_enhancementSystem != null) {
                enhLevel = _enhancementSystem.GetEnhancementLevel(itemId);
            }
            
            var levelLabel = new Label();
            levelLabel.Text = "强化: +" + enhLevel;
            levelLabel.HorizontalAlignment = HorizontalAlignment.Center;
            if (enhLevel >= 7) {
                levelLabel.Modulate = ColorLegendary;
            } else if (enhLevel >= 5) {
                levelLabel.Modulate = ColorEpic;
            } else if (enhLevel >= 3) {
                levelLabel.Modulate = ColorRare;
            } else if (enhLevel >= 1) {
                levelLabel.Modulate = ColorUncommon;
            }
            container.AddChild(levelLabel);
            
            return container;
        }
        
        private void OnEquipmentSelected(string itemId, int index) {
            _selectedItemId = itemId;
            _selectedSlotIndex = index;
            UpdateDetailPanel();
        }
        
        private void OnStoneSelected(long index) {
            UpdateDetailPanel();
        }
        
        private void UpdateDetailPanel() {
            if (string.IsNullOrEmpty(_selectedItemId)) {
                _itemNameLabel.Text = "选择一件装备";
                _itemStatsLabel.Text = "";
                _enhancementLevelLabel.Text = "强化等级: 0";
                _successRateLabel.Text = "成功率: --%";
                _materialsLabel.Text = "所需材料: --";
                _enhanceButton.Disabled = true;
                _stoneInfoLabel.Text = "";
                return;
            }
            
            // 获取物品数据
            var itemSystem = ItemSystem.Instance;
            var itemData = itemSystem?.GetItemData(_selectedItemId);
            
            if (itemData == null) {
                _itemNameLabel.Text = _selectedItemId;
                _itemStatsLabel.Text = "物品数据未找到";
            } else {
                _itemNameLabel.Text = itemData.Name;
                _itemStatsLabel.Text = itemData.Description;
            }
            
            // 强化等级
            int enhLevel = 0;
            EnhancementType type = EnhancementType.Weapon;
            
            if (_enhancementSystem != null) {
                enhLevel = _enhancementSystem.GetEnhancementLevel(_selectedItemId);
            }
            
            // 判断装备类型
            if (_selectedSlotIndex == 0) {
                type = EnhancementType.Weapon;
            } else if (_selectedSlotIndex == 1) {
                type = EnhancementType.Armor;
            } else {
                type = EnhancementType.Accessory;
            }
            
            _enhancementLevelLabel.Text = "强化等级: +" + enhLevel;
            
            // 更新强化等级颜色
            if (enhLevel >= 7) {
                _enhancementLevelLabel.Modulate = ColorLegendary;
            } else if (enhLevel >= 5) {
                _enhancementLevelLabel.Modulate = ColorEpic;
            } else if (enhLevel >= 3) {
                _enhancementLevelLabel.Modulate = ColorRare;
            } else if (enhLevel >= 1) {
                _enhancementLevelLabel.Modulate = ColorUncommon;
            } else {
                _enhancementLevelLabel.Modulate = ColorWhite;
            }
            
            // 成功率
            string[] stoneIds = {
                "401",
                "402",
                "403",
                "404",
                "405"
            };
            
            int stoneIndex = _stoneSelector.Selected;
            string selectedStone = stoneIds[Math.Min(stoneIndex, stoneIds.Length - 1)];
            
            float successRate = 0f;
            if (_enhancementSystem != null && enhLevel < 10) {
                successRate = _enhancementSystem.GetSuccessRate(enhLevel, selectedStone);
            }
            
            _successRateLabel.Text = "成功率: " + (successRate * 100).ToString("F1") + "%";
            
            // 强化石信息
            var enhDb = EnhancementDatabase.Instance;
            var stoneData = enhDb?.GetStoneData(selectedStone);
            if (stoneData != null) {
                _stoneInfoLabel.Text = stoneData.Description;
            }
            
            // 材料需求
            if (_enhancementSystem != null) {
                var materials = _enhancementSystem.GetRequiredMaterials(enhLevel, type);
                var materialText = "所需材料:\n";
                
                bool hasMaterials = true;
                var inventory = _player?.GetInventory();
                
                foreach (var mat in materials) {
                    var matName = GetMaterialName(mat.Key);
                    int playerCount = 0;
                    if (inventory != null && inventory.ContainsKey(mat.Key)) {
                        playerCount = inventory[mat.Key];
                    }
                    
                    bool has = playerCount >= mat.Value;
                    if (!has) hasMaterials = false;
                    
                    materialText += $"{matName}: {playerCount}/{mat.Value}";
                    if (!has) materialText += " ❌";
                    materialText += "\n";
                }
                
                if (enhLevel >= 10) {
                    materialText = "已达到最大强化等级!";
                    hasMaterials = false;
                }
                
                _materialsLabel.Text = materialText;
                _enhanceButton.Disabled = !hasMaterials || enhLevel >= 10;
                
                if (enhLevel >= 10) {
                    _enhanceButton.Text = "已满级";
                } else if (hasMaterials) {
                    _enhanceButton.Text = "开始强化";
                } else {
                    _enhanceButton.Text = "材料不足";
                }
            }
        }
        
        private Color ColorWhite = Colors.White;
        
        private string GetMaterialName(string itemId) {
            switch (itemId) {
                case "401": return "普通强化石";
                case "402": return "优秀强化石";
                case "403": return "稀有强化石";
                case "404": return "史诗强化石";
                case "405": return "传说强化石";
                default: return itemId;
            }
        }
        
        private void OnEnhancePressed() {
            if (_enhancementSystem == null || string.IsNullOrEmpty(_selectedItemId)) return;
            
            string[] stoneIds = {
                "401",
                "402",
                "403",
                "404",
                "405"
            };
            
            int stoneIndex = _stoneSelector.Selected;
            string selectedStone = stoneIds[Math.Min(stoneIndex, stoneIds.Length - 1)];
            
            EnhancementType type = EnhancementType.Weapon;
            if (_selectedSlotIndex == 1) type = EnhancementType.Armor;
            else if (_selectedSlotIndex >= 2) type = EnhancementType.Accessory;
            
            int currentLevel = _enhancementSystem.GetEnhancementLevel(_selectedItemId);
            
            var result = _enhancementSystem.EnhanceItem(_selectedItemId, currentLevel, type, selectedStone);
            
            // 播放音效或显示消息
            ShowEnhancementResult(result, currentLevel);
            
            UpdateDetailPanel();
            RefreshEquipmentList();
        }
        
        private void ShowEnhancementResult(EnhancementResult result, int oldLevel) {
            var msgSystem = GameMessageSystem.Instance;
            
            switch (result) {
                case EnhancementResult.Success:
                    msgSystem?.ShowPositive("强化成功! 装备强化+" + (oldLevel + 1));
                    break;
                case EnhancementResult.Failed:
                    msgSystem?.ShowWarning("强化失败... 装备降级至+" + Math.Max(0, oldLevel - 1));
                    break;
                case EnhancementResult.MaxLevel:
                    msgSystem?.ShowWarning("装备已达到最大强化等级!");
                    break;
            }
        }
        
        private void OnEnhancementComplete(string itemId, int level, EnhancementResult result) {
            if (itemId == _selectedItemId) {
                UpdateDetailPanel();
                RefreshEquipmentList();
            }
        }
        
        public override void _Input(InputEvent @event) {
            if (@event.IsActionPressed("enhancement")) {
                if (Visible) {
                    Hide();
                } else {
                    Show();
                }
                GetTree().SetInputAsHandled();
            }
        }
        
        public override void _ExitTree() {
            if (_enhancementSystem != null) {
                _enhancementSystem.OnEnhancementComplete -= OnEnhancementComplete;
            }
        }
    }
}
