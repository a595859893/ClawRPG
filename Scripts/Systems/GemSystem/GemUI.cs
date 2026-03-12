using Godot;
using Godot.Collections;
using System;
using System.Linq;
using Tween = Godot.Tween;

namespace ClawRPG.Scripts.Systems.GemSystem {
    /// <summary>
    /// 宝石镶嵌界面 - UI界面和交互
    /// </summary>
    
    public class GemUI : Control {
        // 界面引用
        private Control _mainPanel;
        private VBoxContainer _gemListContainer;
        private VBoxContainer _equipmentSlotsContainer;
        private Label _goldLabel;
        private Label _titleLabel;
        
        // 宝石背包面板
        private Control _gemInventoryPanel;
        private GridContainer _gemInventoryGrid;
        
        // 装备选择面板
        private Control _equipmentPanel;
        private ItemList _equipmentList;
        
        // 当前选中的装备
        private string _selectedEquipmentId = "";
        private string _selectedEquipmentType = "";
        
        // 宝石数据库和系统
        private GemDatabase _gemDatabase;
        private GemSystem _gemSystem;
        
        // 动画
        private Tween _tween;
        
        // 筛选状态
        private GemType? _selectedGemType = null;
        private GemRarity? _selectedRarity = null;
        
        // 按键提示
        private Label _keyHintLabel;
        
        public override void _Ready() {
            _gemDatabase = GemDatabase.Instance;
            _gemSystem = GemSystem.Instance;
            
            SetupUI();
            SetupAnimations();
            RefreshUI();
        }
        
        private void SetupUI() {
            // 主容器
            var mainContainer = new HBoxContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One,
                OffsetLeft = 50,
                OffsetTop = 50,
                OffsetRight = -50,
                OffsetBottom = -50
            };
            AddChild(mainContainer);
            
            // ===== 左侧：宝石背包 =====
            var leftPanel = new PanelContainer {
                CustomMinimumSize = new Vector2(400, 0),
                MarginRight = 10
            };
            mainContainer.AddChild(leftPanel);
            
            var leftVBox = new VBoxContainer();
            leftPanel.AddChild(leftVBox);
            
            // 标题
            var leftTitle = new Label {
                Text = "宝石背包",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            leftVBox.AddChild(leftTitle);
            
            // 筛选按钮
            var filterContainer = new HBoxContainer();
            leftVBox.AddChild(filterContainer);
            
            // 类型筛选
            var typeOption = new OptionButton {
                CustomMinimumSize = new Vector2(120, 30)
            };
            typeOption.AddItem("全部类型", 0);
            typeOption.AddItem("红宝石(攻击)", (int)GemType.Ruby + 1);
            typeOption.AddItem("蓝宝石(防御)", (int)GemType.Sapphire + 1);
            typeOption.AddItem("绿宝石(生命)", (int)GemType.Emerald + 1);
            typeOption.AddItem("钻石(暴击)", (int)GemType.Diamond + 1);
            typeOption.AddItem("黄宝石(速度)", (int)GemType.Topaz + 1);
            typeOption.AddItem("紫宝石(魔法)", (int)GemType.Amethyst + 1);
            typeOption.AddItem("黑曜石(韧性)", (int)GemType.Onyx + 1);
            typeOption.AddItem("珍珠(幸运)", (int)GemType.Pearl + 1);
            typeOption.ItemSelected += OnTypeFilterChanged;
            filterContainer.AddChild(typeOption);
            
            // 稀有度筛选
            var rarityOption = new OptionButton {
                CustomMinimumSize = new Vector2(100, 30)
            };
            rarityOption.AddItem("全部稀有度", 0);
            rarityOption.AddItem("普通", (int)GemRarity.Common + 1);
            rarityOption.AddItem("优秀", (int)GemRarity.Uncommon + 1);
            rarityOption.AddItem("稀有", (int)GemRarity.Rare + 1);
            rarityOption.AddItem("史诗", (int)GemRarity.Epic + 1);
            rarityOption.AddItem("传说", (int)GemRarity.Legendary + 1);
            rarityOption.ItemSelected += OnRarityFilterChanged;
            filterContainer.AddChild(rarityOption);
            
            // 宝石列表
            _gemInventoryGrid = new GridContainer {
                Columns = 4,
                CustomMinimumSize = new Vector2(0, 400),
                SizeFlagsVertical = SizeFlags.Expand
            };
            leftVBox.AddChild(_gemInventoryGrid);
            
            // ===== 中间：装备槽位 =====
            var centerPanel = new PanelContainer {
                CustomMinimumSize = new Vector2(350, 0),
                MarginLeft = 10,
                MarginRight = 10
            };
            mainContainer.AddChild(centerPanel);
            
            var centerVBox = new VBoxContainer();
            centerPanel.AddChild(centerVBox);
            
            // 标题
            var centerTitle = new Label {
                Text = "装备镶嵌",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            centerVBox.AddChild(centerTitle);
            
            // 装备选择列表
            _equipmentList = new ItemList {
                CustomMinimumSize = new Vector2(0, 300),
                SizeFlagsVertical = SizeFlags.Expand
            };
            _equipmentList.ItemSelected += OnEquipmentSelected;
            centerVBox.AddChild(_equipmentList);
            
            // 宝石槽位显示
            _equipmentSlotsContainer = new VBoxContainer();
            centerVBox.AddChild(_equipmentSlotsContainer);
            
            // ===== 右侧：详情面板 =====
            var rightPanel = new PanelContainer {
                CustomMinimumSize = new Vector2(300, 0),
                MarginLeft = 10
            };
            mainContainer.AddChild(rightPanel);
            
            var rightVBox = new VBoxContainer();
            rightPanel.AddChild(rightVBox);
            
            // 标题
            var rightTitle = new Label {
                Text = "宝石详情",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            rightVBox.AddChild(rightTitle);
            
            // 宝石详情
            var detailContainer = new VBoxContainer {
                CustomMinimumSize = new Vector2(0, 300),
                SizeFlagsVertical = SizeFlags.Expand
            };
            rightVBox.AddChild(detailContainer);
            
            // 按键提示
            _keyHintLabel = new Label {
                Text = "按 G 键关闭",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = new Color(0.7f, 0.7f, 0.7f)
            };
            rightVBox.AddChild(_keyHintLabel);
            
            // 保存引用
            _gemInventoryPanel = leftPanel;
            _equipmentPanel = centerPanel;
            _mainPanel = mainContainer;
        }
        
        private void SetupAnimations() {
            // 面板打开动画：淡入 + 缩放
            _tween = CreateTween();
            _tween.SetParallel(true);
            _tween.SetTrans(Tween.TransitionType.Back);
            _tween.SetEasing(Tween.EasingFunction.EaseOut);
            
            _mainPanel.Modulate = new Color(1, 1, 1, 0);
            _mainPanel.Scale = new Vector2(0.9f, 0.9f);
            
            _tween.TweenProperty(_mainPanel, "modulate:a", 1.0f, 0.3f);
            _tween.TweenProperty(_mainPanel, "scale", Vector2.One, 0.3f);
        }
        
        /// <summary>
        /// 刷新UI
        /// </summary>
        public void RefreshUI() {
            RefreshGemInventory();
            RefreshEquipmentList();
            RefreshEquipmentSlots();
        }
        
        /// <summary>
        /// 刷新宝石背包
        /// </summary>
        private void RefreshGemInventory() {
            // 清除现有宝石
            foreach (var child in _gemInventoryGrid.GetChildren()) {
                child.QueueFree();
            }
            
            // 获取玩家拥有的宝石
            var ownedGems = _gemSystem.GetOwnedGems();
            
            // 根据筛选获取宝石列表
            List<GemData> gemsToShow;
            if (_selectedGemType.HasValue && _selectedRarity.HasValue) {
                gemsToShow = _gemDatabase.GetGemsByTypeAndRarity(_selectedGemType.Value, _selectedRarity.Value);
            } else if (_selectedGemType.HasValue) {
                gemsToShow = _gemDatabase.GetGemsByType(_selectedGemType.Value);
            } else if (_selectedRarity.HasValue) {
                gemsToShow = _gemDatabase.GetGemsByRarity(_selectedRarity.Value);
            } else {
                gemsToShow = _gemDatabase.GetAllGems();
            }
            
            // 显示宝石
            foreach (var gem in gemsToShow) {
                int count = ownedGems.TryGetValue(gem.GemId, out int c) ? c : 0;
                
                var gemButton = CreateGemButton(gem, count);
                _gemInventoryGrid.AddChild(gemButton);
            }
        }
        
        /// <summary>
        /// 创建宝石按钮
        /// </summary>
        private Control CreateGemButton(GemData gem, int count) {
            var container = new VBoxContainer {
                CustomMinimumSize = new Vector2(80, 80),
                SizeFlagsHorizontal = SizeFlags.Expand
            };
            
            // 宝石图标（使用颜色方块代替）
            var icon = new ColorRect {
                Color = GetGemTypeColor(gem.Type),
                CustomMinimumSize = new Vector2(50, 50),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter
            };
            container.AddChild(icon);
            
            // 数量标签
            var countLabel = new Label {
                Text = count > 0 ? $"x{count}" : "",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = count > 0 ? new Color(1, 1, 0) : new Color(0.5f, 0.5f, 0.5f)
            };
            container.AddChild(countLabel);
            
            // 稀有度颜色
            var rarityLabel = new Label {
                Text = GemData.GetRarityName(gem.Rarity),
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = GemData.GetRarityColor(gem.Rarity),
                CustomMinimumSize = new Vector2(0, 20)
            };
            container.AddChild(rarityLabel);
            
            // 点击事件
            if (count > 0 && !string.IsNullOrEmpty(_selectedEquipmentId)) {
                icon.GuiInput += (inputEvent) => {
                    if (inputEvent is InputEventMouseButton mouseEvent && 
                        mouseEvent.ButtonIndex == MouseButton.Left && 
                        mouseEvent.Pressed) {
                        OnGemClicked(gem);
                    }
                };
                
                // 悬停动画
                icon.MouseEntered += () => {
                    var t = CreateTween();
                    t.TweenProperty(icon, "scale", new Vector2(1.1f, 1.1f), 0.15f)
                     .SetTrans(Tween.TransitionType.Cubic)
                     .SetEasing(Tween.EasingFunction.EaseOut);
                };
                
                icon.MouseExited += () => {
                    var t = CreateTween();
                    t.TweenProperty(icon, "scale", Vector2.One, 0.15f)
                     .SetTrans(Tween.TransitionType.Cubic)
                     .SetEasing(Tween.EasingFunction.EaseOut);
                };
            }
            
            return container;
        }
        
        private Color GetGemTypeColor(GemType type) {
            return type switch {
                GemType.Ruby => new Color(0.9f, 0.2f, 0.2f),
                GemType.Sapphire => new Color(0.2f, 0.4f, 0.9f),
                GemType.Emerald => new Color(0.2f, 0.8f, 0.2f),
                GemType.Diamond => new Color(0.8f, 0.9f, 1f),
                GemType.Topaz => new Color(1f, 0.8f, 0.2f),
                GemType.Amethyst => new Color(0.6f, 0.3f, 0.8f),
                GemType.Onyx => new Color(0.2f, 0.2f, 0.2f),
                GemType.Pearl => new Color(1f, 0.9f, 0.8f),
                _ => Color.White
            };
        }
        
        /// <summary>
        /// 刷新装备列表
        /// </summary>
        private void RefreshEquipmentList() {
            _equipmentList.Clear();
            
            // 从 GemSystem 获取已装备的物品
            var equippedIds = _gemSystem.GetEquippedEquipmentIds();
            
            if (equippedIds.Count > 0) {
                // 显示已装备的物品
                int index = 0;
                foreach (var equipmentId in equippedIds) {
                    // 从装备ID中提取类型
                    string equipmentType = "weapon";
                    if (equipmentId.Contains("armor")) equipmentType = "armor";
                    else if (equipmentId.Contains("helmet")) equipmentType = "helmet";
                    else if (equipmentId.Contains("boots")) equipmentType = "boots";
                    else if (equipmentId.Contains("gloves")) equipmentType = "gloves";
                    else if (equipmentId.Contains("accessory")) equipmentType = "accessory";
                    
                    string displayName = equipmentId.Replace("_", " ");
                    displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);
                    
                    _equipmentList.AddItem($"{displayName}", index);
                    _equipmentList.SetItemMetadata(index, new Dictionary { { "id", equipmentId }, { "type", equipmentType } });
                    index++;
                }
            } else {
                // 如果没有已装备的物品，显示默认装备（首次使用）
                _equipmentList.AddItem("武器 - 烈焰之剑", 0);
                _equipmentList.SetItemMetadata(0, new Dictionary { { "id", "weapon_001" }, { "type", "weapon" } });
                
                _equipmentList.AddItem("护甲 - 鳞甲", 1);
                _equipmentList.SetItemMetadata(1, new Dictionary { { "id", "armor_001" }, { "type", "armor" } });
                
                _equipmentList.AddItem("头盔 - 铁盔", 2);
                _equipmentList.SetItemMetadata(2, new Dictionary { { "id", "helmet_001" }, { "type", "helmet" } });
                
                _equipmentList.AddItem("靴子 - 轻靴", 3);
                _equipmentList.SetItemMetadata(3, new Dictionary { { "id", "boots_001" }, { "type", "boots" } });
                
                _equipmentList.AddItem("手套 - 皮手套", 4);
                _equipmentList.SetItemMetadata(4, new Dictionary { { "id", "gloves_001" }, { "type", "gloves" } });
                
                _equipmentList.AddItem("饰品 - 魔法戒指", 5);
                _equipmentList.SetItemMetadata(5, new Dictionary { { "id", "accessory_001" }, { "type", "accessory" } });
            }
        }
        
        /// <summary>
        /// 刷新装备槽位显示
        /// </summary>
        private void RefreshEquipmentSlots() {
            // 清除现有槽位
            foreach (var child in _equipmentSlotsContainer.GetChildren()) {
                child.QueueFree();
            }
            
            if (string.IsNullOrEmpty(_selectedEquipmentId)) {
                var hint = new Label {
                    Text = "请选择装备",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = new Color(0.5f, 0.5f, 0.5f)
                };
                _equipmentSlotsContainer.AddChild(hint);
                return;
            }
            
            // 获取槽位
            var slots = _gemSystem.GetEquipmentSlots(_selectedEquipmentId, _selectedEquipmentType);
            
            for (int i = 0; i < slots.Count; i++) {
                var slot = slots[i];
                var slotContainer = CreateSlotDisplay(i, slot);
                _equipmentSlotsContainer.AddChild(slotContainer);
            }
        }
        
        /// <summary>
        /// 创建槽位显示
        /// </summary>
        private Control CreateSlotDisplay(int index, EquipmentGemSlot slot) {
            var container = new HBoxContainer {
                CustomMinimumSize = new Vector2(0, 50)
            };
            
            // 槽位标签
            var label = new Label {
                Text = $"槽位 {index + 1}: ",
                CustomMinimumSize = new Vector2(80, 0)
            };
            container.AddChild(label);
            
            // 槽位状态
            if (!slot.IsUnlocked) {
                var unlockButton = new Button {
                    Text = "解锁",
                    CustomMinimumSize = new Vector2(80, 0)
                };
                unlockButton.Pressed += () => OnUnlockSlotClicked(index);
                container.AddChild(unlockButton);
            } else if (slot.HasGem) {
                var gem = _gemSystem.GetGem(slot.GemId);
                if (gem != null) {
                    var gemLabel = new Label {
                        Text = $"{gem.Name} (+{gem.Attributes.FirstOrDefault().Value})",
                        Modulate = GemData.GetRarityColor(gem.Rarity)
                    };
                    container.AddChild(gemLabel);
                    
                    var removeButton = new Button {
                        Text = "取下",
                        CustomMinimumSize = new Vector2(60, 0)
                    };
                    removeButton.Pressed += () => OnRemoveGemClicked(index);
                    container.AddChild(removeButton);
                }
            } else {
                var emptyLabel = new Label {
                    Text = "空",
                    Modulate = new Color(0.5f, 0.5f, 0.5f)
                };
                container.AddChild(emptyLabel);
            }
            
            return container;
        }
        
        private void OnEquipmentSelected(int index) {
            var metadata = _equipmentList.GetItemMetadata(index) as Dictionary;
            if (metadata != null) {
                _selectedEquipmentId = metadata["id"].ToString();
                _selectedEquipmentType = metadata["type"].ToString();
                RefreshEquipmentSlots();
            }
        }
        
        private void OnGemClicked(GemData gem) {
            if (string.IsNullOrEmpty(_selectedEquipmentId)) return;
            
            // 找到第一个空槽位
            var slots = _gemSystem.GetEquipmentSlots(_selectedEquipmentId, _selectedEquipmentType);
            for (int i = 0; i < slots.Count; i++) {
                if (slots[i].IsUnlocked && !slots[i].HasGem) {
                    bool success = _gemSystem.InsertGem(_selectedEquipmentId, _selectedEquipmentType, i, gem.GemId);
                    if (success) {
                        RefreshUI();
                        PlaySuccessAnimation();
                    }
                    return;
                }
            }
            
            GD.Print("[GemUI] No empty slots available");
        }
        
        private void OnRemoveGemClicked(int slotIndex) {
            if (string.IsNullOrEmpty(_selectedEquipmentId)) return;
            
            bool success = _gemSystem.RemoveGem(_selectedEquipmentId, slotIndex);
            if (success) {
                RefreshUI();
            }
        }
        
        private void OnUnlockSlotClicked(int slotIndex) {
            if (string.IsNullOrEmpty(_selectedEquipmentId)) return;
            
            bool success = _gemSystem.UnlockSlot(_selectedEquipmentId, _selectedEquipmentType, slotIndex);
            if (success) {
                RefreshUI();
                PlaySuccessAnimation();
            }
        }
        
        private void OnTypeFilterChanged(long index) {
            if (index == 0) {
                _selectedGemType = null;
            } else {
                _selectedGemType = (GemType)(index - 1);
            }
            RefreshGemInventory();
        }
        
        private void OnRarityFilterChanged(long index) {
            if (index == 0) {
                _selectedRarity = null;
            } else {
                _selectedRarity = (GemRarity)(index - 1);
            }
            RefreshGemInventory();
        }
        
        /// <summary>
        /// 成功动画
        /// </summary>
        private void PlaySuccessAnimation() {
            var t = CreateTween();
            t.TweenProperty(_equipmentPanel, "modulate", new Color(0, 1, 0, 1), 0.1f);
            t.TweenProperty(_equipmentPanel, "modulate", Color.White, 0.2f);
        }
        
        /// <summary>
        /// 关闭界面
        /// </summary>
        public void Close() {
            var t = CreateTween();
            t.SetParallel(true);
            t.SetTrans(Tween.TransitionType.Back);
            t.SetEasing(Tween.EasingFunction.EaseIn);
            
            t.TweenProperty(_mainPanel, "modulate:a", 0.0f, 0.2f);
            t.TweenProperty(_mainPanel, "scale", new Vector2(0.95f, 0.95f), 0.2f);
            
            t.TweenCallback(QueueFree);
        }
        
        public override void _Input(InputEvent inputEvent) {
            if (inputEvent.IsActionPressed("ui_cancel") || inputEvent.IsActionPressed("ui_gem")) {
                Close();
            }
        }
    }
}
