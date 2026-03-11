using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems {
    // 坐骑装备UI
    public partial class MountEquipmentUI : Control {
        private Control container;
        private VBoxContainer mainVBox;
        
        // 标签页
        private TabContainer tabContainer;
        private Control shopTab;
        private Control inventoryTab;
        private Control equipTab;
        
        // 商店相关
        private GridContainer shopGrid;
        private Label goldLabel;
        
        // 背包相关
        private GridContainer inventoryGrid;
        
        // 装备相关
        private OptionButton mountSelector;
        private GridContainer equipSlotsGrid;
        
        // 当前选择
        private string selectedMountId = "";
        
        public override void _Ready() {
            SetupUI();
            Visible = false; 
            
            // 连接信号
            if (MountSystem.Instance != null) {
                MountSystem.Instance.MountListUpdated += RefreshMountSelector;
            }
        }

        private void SetupUI() {
            // 背景面板
            var bgPanel = new Panel {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                Modulate = new Color(0, 0, 0, 0.7f)
            };
            AddChild(bgPanel);
            
            // 主容器
            mainVBox = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 100,
                OffsetTop = 50,
                OffsetRight = -100,
                OffsetBottom = -50
            };
            AddChild(mainVBox);
            
            // 标题
            var titleLabel = new Label {
                Text = "  坐骑装备系统  ",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 32)
            };
            mainVBox.AddChild(titleLabel);
            
            // 标签页容器
            tabContainer = new TabContainer {
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            mainVBox.AddChild(tabContainer);
            
            // 商店标签页
            SetupShopTab();
            
            // 背包标签页
            SetupInventoryTab();
            
            // 装备标签页
            SetupEquipTab();
            
            // 底部按钮
            var buttonBox = new HBoxContainer {
                Alignment = BoxContainerAlignment.Center,
                CustomMinimumHeight = 50
            };
            mainVBox.AddChild(buttonBox);
            
            var closeButton = new Button {
                Text = "  关闭 (K)  ",
                CustomMinimumSize = new Vector2(150, 40)
            };
            closeButton.Pressed += () => ToggleUI();
            buttonBox.AddChild(closeButton);
            
            // 金币显示
            goldLabel = new Label {
                Text = "金币: 0",
                HorizontalAlignment = HorizontalAlignment.Right,
                AddThemeFontSizeOverride("font_size", 20)
            };
            buttonBox.AddChild(goldLabel);
            
            UpdateGoldDisplay();
        }

        private void SetupShopTab() {
            shopTab = new Control();
            shopTab.Name = "商店";
            tabContainer.AddChild(shopTab);
            
            var shopVBox = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            shopTab.AddChild(shopVBox);
            
            // 筛选按钮
            var filterBox = new HBoxContainer {
                CustomMinimumHeight = 40
            };
            shopVBox.AddChild(filterBox);
            
            var filterLabel = new Label { Text = "筛选: " };
            filterBox.AddChild(filterLabel);
            
            var typeFilter = new OptionButton;
            typeFilter.AddItem("全部类型");
            typeFilter.AddItem("马鞍");
            typeFilter.AddItem("马蹄铁");
            typeFilter.AddItem("缰绳");
            typeFilter.AddItem("护甲");
            typeFilter.AddItem("配饰");
            typeFilter.Selected = 0;
            typeFilter.ItemSelected += (index) => RefreshShopGrid((int)index - 1);
            filterBox.AddChild(typeFilter);
            
            var rarityFilter = new OptionButton;
            rarityFilter.AddItem("全部稀有度");
            rarityFilter.AddItem("普通");
            rarityFilter.AddItem("优秀");
            rarityFilter.AddItem("稀有");
            rarityFilter.AddItem("史诗");
            rarityFilter.AddItem("传说");
            rarityFilter.Selected = 0;
            rarityFilter.ItemSelected += (index) => RefreshShopGridRarity((int)index - 1);
            rarityFilter.CustomMinimumSize = new Vector2(100, 0);
            filterBox.AddChild(rarityFilter);
            
            // 商店网格
            shopGrid = new GridContainer {
                Columns = 5,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            shopVBox.AddChild(shopGrid);
            
            RefreshShopGrid(-1);
        }

        private void SetupInventoryTab() {
            inventoryTab = new Control();
            inventoryTab.Name = "背包";
            tabContainer.AddChild(inventoryTab);
            
            var inventoryVBox = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            inventoryTab.AddChild(inventoryVBox);
            
            var inventoryLabel = new Label {
                Text = "已拥有的坐骑装备",
                AddThemeFontSizeOverride("font_size", 24)
            };
            inventoryVBox.AddChild(inventoryLabel);
            
            inventoryGrid = new GridContainer {
                Columns = 4,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            inventoryVBox.AddChild(inventoryGrid);
            
            RefreshInventoryGrid();
        }

        private void SetupEquipTab() {
            equipTab = new Control();
            equipTab.Name = "装备";
            tabContainer.AddChild(equipTab);
            
            var equipVBox = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            equipTab.AddChild(equipVBox);
            
            // 坐骑选择
            var mountBox = new HBoxContainer {
                CustomMinimumHeight = 40
            };
            equipVBox.AddChild(mountBox);
            
            var mountLabel = new Label { Text = "选择坐骑: " };
            mountBox.AddChild(mountLabel);
            
            mountSelector = new OptionButton {
                CustomMinimumSize = new Vector2(200, 0)
            };
            mountSelector.ItemSelected += (index) => OnMountSelected(index);
            mountBox.AddChild(mountSelector);
            
            // 装备槽位
            var slotsLabel = new Label {
                Text = "装备槽位",
                AddThemeFontSizeOverride("font_size", 24)
            };
            equipVBox.AddChild(slotsLabel);
            
            equipSlotsGrid = new GridContainer {
                Columns = 3,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            equipVBox.AddChild(equipSlotsGrid);
            
            // 创建5个装备槽位
            CreateEquipSlots();
            
            // 属性加成显示
            var bonusLabel = new Label {
                Text = "装备加成",
                AddThemeFontSizeOverride("font_size", 24)
            };
            equipVBox.AddChild(bonusLabel);
            
            RefreshMountSelector();
        }

        private void CreateEquipSlots() {
            string[] slotNames = { "马鞍", "马蹄铁", "缰绳", "护甲", "配饰" };
            
            foreach (var slotName in slotNames) {
                var slotPanel = new PanelContainer {
                    CustomMinimumSize = new Vector2(200, 80)
                };
                
                var slotVBox = new VBoxContainer();
                slotPanel.AddChild(slotVBox);
                
                var slotTitle = new Label {
                    Text = slotName,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    AddThemeFontSizeOverride("font_size", 18)
                };
                slotVBox.AddChild(slotTitle);
                
                var slotItemLabel = new Label {
                    Text = "[未装备]",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = new Color(0.7f, 0.7f, 0.7f)
                };
                slotItemLabel.Name = "ItemLabel";
                slotVBox.AddChild(slotItemLabel);
                
                var equipButton = new Button {
                    Text = "穿戴",
                    CustomMinimumSize = new Vector2(80, 25)
                };
                equipButton.Name = "EquipButton";
                equipButton.Pressed += () => OnEquipButtonPressed(slotName);
                slotVBox.AddChild(equipButton);
                
                equipSlotsGrid.AddChild(slotPanel);
            }
        }

        private void RefreshShopGrid(int typeFilter = -1, int rarityFilter = -1) {
            foreach (var child in shopGrid.GetChildren()) {
                child.QueueFree();
            }
            
            var allEquipment = MountEquipmentSystem.Instance != null ? 
                MountEquipmentSystem.Instance.GetType().GetMethod("GetEquipmentData").Invoke(null, null) as Dictionary<string, MountEquipmentData> :
                null;
            
            if (MountEquipmentSystem.Instance == null) return;
            
            // 获取所有装备
            var shopItems = new List<MountEquipmentData>();
            var dbType = typeof(MountEquipmentSystem);
            
            foreach (var kvp in ((Dictionary<string, MountEquipmentData>)dbType.GetField("equipmentDatabase", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(MountEquipmentSystem.Instance))) {
                var equip = kvp.Value;
                
                // 筛选
                if (typeFilter >= 0 && (int)equip.Type != typeFilter) continue;
                if (rarityFilter >= 0 && (int)equip.Rarity != rarityFilter) continue;
                
                shopItems.Add(equip);
            }
            
            // 按稀有度排序
            shopItems.Sort((a, b) => ((int)a.Rarity).CompareTo((int)b.Rarity));
            
            // 显示
            foreach (var equip in shopItems) {
                var itemPanel = CreateShopItemPanel(equip);
                shopGrid.AddChild(itemPanel);
            }
        }

        private void RefreshShopGridRarity(int rarityFilter) {
            RefreshShopGrid(-1, rarityFilter);
        }

        private Control CreateShopItemPanel(MountEquipmentData equip) {
            var panel = new PanelContainer {
                CustomMinimumSize = new Vector2(180, 140)
            };
            
            var vbox = new VBoxContainer();
            panel.AddChild(vbox);
            
            // 稀有度颜色
            Color rarityColor = GetRarityColor(equip.Rarity);
            
            var nameLabel = new Label {
                Text = equip.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = rarityColor,
                AddThemeFontSizeOverride("font_size", 16)
            };
            vbox.AddChild(nameLabel);
            
            var typeLabel = new Label {
                Text = GetTypeName(equip.Type),
                HorizontalAlignment = HorizontalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 12)
            };
            vbox.AddChild(typeLabel);
            
            // 属性
            string stats = "";
            if (equip.AttackBonus > 0) stats += $"攻击+{equip.AttackBonus} ";
            if (equip.DefenseBonus > 0) stats += $"防御+{equip.DefenseBonus} ";
            if (equip.SpeedBonus > 0) stats += $"速度+{equip.SpeedBonus} ";
            if (equip.HealthBonus > 0) stats += $"生命+{equip.HealthBonus} ";
            if (equip.CriticalRateBonus > 0) stats += $"暴击+{equip.CriticalRateBonus}% ";
            if (equip.CriticalDamageBonus > 0) stats += $"暴伤+{equip.CriticalDamageBonus}% ";
            
            var statsLabel = new Label {
                Text = stats.Length > 0 ? stats : "无加成",
                HorizontalAlignment = HorizontalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 11),
                Modulate = new Color(0.8f, 0.8f, 0.8f)
            };
            vbox.AddChild(statsLabel);
            
            // 购买按钮
            var buyButton = new Button {
                Text = $"购买 ({equip.Price}金)",
                CustomMinimumSize = new Vector2(100, 30)
            };
            
            bool owned = MountEquipmentSystem.Instance.IsOwned(equip.Id);
            if (owned) {
                buyButton.Text = "已拥有";
                buyButton.Disabled = true;
            }
            
            buyButton.Pressed += () => {
                if (MountEquipmentSystem.Instance.PurchaseEquipment(equip.Id)) {
                    UpdateGoldDisplay();
                    RefreshShopGrid();
                    RefreshInventoryGrid();
                }
            };
            vbox.AddChild(buyButton);
            
            return panel;
        }

        private void RefreshInventoryGrid() {
            foreach (var child in inventoryGrid.GetChildren()) {
                child.QueueFree();
            }
            
            if (MountEquipmentSystem.Instance == null) return;
            
            var owned = MountEquipmentSystem.Instance.GetOwnedEquipment();
            
            if (owned.Count == 0) {
                var emptyLabel = new Label {
                    Text = "暂无已拥有的坐骑装备",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = new Color(0.6f, 0.6f, 0.6f)
                };
                inventoryGrid.AddChild(emptyLabel);
                return;
            }
            
            foreach (var equip in owned) {
                var itemPanel = CreateInventoryItemPanel(equip);
                inventoryGrid.AddChild(itemPanel);
            }
        }

        private Control CreateInventoryItemPanel(MountEquipmentData equip) {
            var panel = new PanelContainer {
                CustomMinimumSize = new Vector2(180, 100)
            };
            
            var vbox = new VBoxContainer();
            panel.AddChild(vbox);
            
            Color rarityColor = GetRarityColor(equip.Rarity);
            
            var nameLabel = new Label {
                Text = equip.Name,
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = rarityColor,
                AddThemeFontSizeOverride("font_size", 16)
            };
            vbox.AddChild(nameLabel);
            
            var typeLabel = new Label {
                Text = $"{GetTypeName(equip.Type)} - {GetRarityName(equip.Rarity)}",
                HorizontalAlignment = HorizontalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 12)
            };
            vbox.AddChild(typeLabel);
            
            return panel;
        }

        private void RefreshMountSelector() {
            mountSelector.Clear();
            
            if (MountSystem.Instance == null) {
                mountSelector.AddItem("无坐骑", 0);
                return;
            }
            
            var mounts = MountSystem.Instance.GetType().GetMethod("GetMounts").Invoke(MountSystem.Instance, null);
            if (mounts == null) {
                mountSelector.AddItem("无坐骑", 0);
                return;
            }
            
            var mountsList = mounts as List<object>;
            if (mountsList == null || mountsList.Count == 0) {
                mountSelector.AddItem("无坐骑", 0);
                return;
            }
            
            int index = 0;
            foreach (var mount in mountsList) {
                var idProp = mount.GetType().GetProperty("Id");
                var nameProp = mount.GetType().GetProperty("Name");
                if (idProp != null && nameProp != null) {
                    string id = idProp.GetValue(mount).ToString();
                    string name = nameProp.GetValue(mount).ToString();
                    mountSelector.AddItem(name, index);
                    index++;
                }
            }
            
            if (mountSelector.ItemCount > 0) {
                mountSelector.Selected = 0;
                OnMountSelected(0);
            }
        }

        private void OnMountSelected(long index) {
            if (MountSystem.Instance == null || index < 0) return;
            
            var mounts = MountSystem.Instance.GetType().GetMethod("GetMounts").Invoke(MountSystem.Instance, null) as List<object>;
            if (mounts == null || index >= mounts.Count) return;
            
            var mount = mounts[(int)index];
            var idProp = mount.GetType().GetProperty("Id");
            if (idProp != null) {
                selectedMountId = idProp.GetValue(mount).ToString();
                RefreshEquipSlots();
            }
        }

        private void RefreshEquipSlots() {
            if (MountEquipmentSystem.Instance == null || selectedMountId == "") return;
            
            var equipped = MountEquipmentSystem.Instance.GetEquippedItems(selectedMountId);
            
            // 更新每个槽位
            int slotIndex = 0;
            foreach (var child in equipSlotsGrid.GetChildren()) {
                var panel = child as PanelContainer;
                if (panel == null) continue;
                
                var vbox = panel.GetChild(0) as VBoxContainer;
                if (vbox == null || vbox.GetChildCount() < 3) continue;
                
                var itemLabel = vbox.GetChild(1) as Label;
                var equipButton = vbox.GetChild(2) as Button;
                
                if (itemLabel == null || equipButton == null) continue;
                
                // 查找对应类型的已装备物品
                MountEquipmentData equippedItem = null;
                foreach (var item in equipped) {
                    // 马鞍=0, 马蹄铁=1, 缰绳=2, 护甲=3, 配饰=4
                    int targetType = slotIndex;
                    if ((int)item.Type == targetType) {
                        equippedItem = item;
                        break;
                    }
                }
                
                if (equippedItem != null) {
                    Color rarityColor = GetRarityColor(equippedItem.Rarity);
                    itemLabel.Text = equippedItem.Name;
                    itemLabel.Modulate = rarityColor;
                    equipButton.Text = "卸下";
                } else {
                    itemLabel.Text = "[未装备]";
                    itemLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
                    equipButton.Text = "穿戴";
                }
                
                slotIndex++;
            }
        }

        private void OnEquipButtonPressed(string slotType) {
            if (selectedMountId == "" || MountEquipmentSystem.Instance == null) return;
            
            // 找到对应类型的未装备物品
            var owned = MountEquipmentSystem.Instance.GetOwnedEquipment();
            int targetType = slotType switch {
                "马鞍" => 0,
                "马蹄铁" => 1,
                "缰绳" => 2,
                "护甲" => 3,
                "配饰" => 4,
                _ => -1
            };
            
            if (targetType < 0) return;
            
            // 检查是否已装备该类型
            var equipped = MountEquipmentSystem.Instance.GetEquippedItems(selectedMountId);
            foreach (var item in equipped) {
                if ((int)item.Type == targetType) {
                    // 卸下
                    MountEquipmentSystem.Instance.UnequipFromMount(item.Id);
                    RefreshEquipSlots();
                    return;
                }
            }
            
            // 穿戴第一个该类型的未装备物品
            foreach (var item in owned) {
                if ((int)item.Type == targetType) {
                    MountEquipmentSystem.Instance.EquipToMount(item.Id, selectedMountId);
                    RefreshEquipSlots();
                    return;
                }
            }
            
            // 提示没有该类型装备
            GD.Print($"[MountEquipmentUI] No {slotType} type equipment in inventory");
        }

        private void UpdateGoldDisplay() {
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player != null) {
                var goldProperty = player.GetProperty("Gold");
                int gold = goldProperty != null ? (int)goldProperty : 0;
                goldLabel.Text = $"金币: {gold}";
            }
        }

        private Color GetRarityColor(MountEquipmentRarity rarity) {
            return rarity switch {
                MountEquipmentRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                MountEquipmentRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                MountEquipmentRarity.Rare => new Color(0.2f, 0.5f, 1f),
                MountEquipmentRarity.Epic => new Color(0.6f, 0.3f, 0.9f),
                MountEquipmentRarity.Legendary => new Color(1f, 0.6f, 0.1f),
                _ => Colors.White
            };
        }

        private string GetRarityName(MountEquipmentRarity rarity) {
            return rarity switch {
                MountEquipmentRarity.Common => "普通",
                MountEquipmentRarity.Uncommon => "优秀",
                MountEquipmentRarity.Rare => "稀有",
                MountEquipmentRarity.Epic => "史诗",
                MountEquipmentRarity.Legendary => "传说",
                _ => "未知"
            };
        }

        private string GetTypeName(MountEquipmentType type) {
            return type switch {
                MountEquipmentType.Saddle => "马鞍",
                MountEquipmentType.Horseshoe => "马蹄铁",
                MountEquipmentType.Bridle => "缰绳",
                MountEquipmentType.Armor => "护甲",
                MountEquipmentType.Accessory => "配饰",
                _ => "未知"
            };
        }

        public void ToggleUI() {
            Visible = !Visible;
            if (Visible) {
                UpdateGoldDisplay();
                RefreshShopGrid();
                RefreshInventoryGrid();
                RefreshMountSelector();
            }
        }

        public override void _Input(InputEvent evt) {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.K) {
                if (Visible) {
                    ToggleUI();
                }
            }
        }
    }
}
