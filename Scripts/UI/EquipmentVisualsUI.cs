using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 装备外观 UI - 允许玩家自定义装备外观
    /// </summary>
    public class EquipmentVisualsUI : Control {
        private Control _mainPanel;
        private Control _weaponPanel;
        private Control _armorPanel;
        private Control _accessoryPanel;
        
        private Label _titleLabel;
        private Label _goldLabel;
        
        private ItemList _weaponList;
        private ItemList _armorList;
        private ItemList _accessoryList;
        
        private Label _weaponDescription;
        private Label _armorDescription;
        private Label _accessoryDescription;
        
        private Button _equipWeaponButton;
        private Button _equipArmorButton;
        private Button _equipAccessoryButton;
        
        private TabContainer _tabContainer;
        
        private string _selectedWeaponId = "";
        private string _selectedArmorId = "";
        private string _selectedAccessoryId = "";
        
        // 信号
        public delegate void OnVisualEquipped(string slot, string visualId);

        public override void _Ready() {
            SetupUI();
            ConnectSignals();
            RefreshVisuals();
        }

        private void SetupUI() {
            // 主容器
            _mainPanel = new Control();
            _mainPanel.SetAnchor(AnchorPresets.FullRect);
            AddChild(_mainPanel);

            // 背景面板
            Panel background = new Panel();
            background.SetAnchor(AnchorPresets.FullRect);
            background.Modulate = new Color(0, 0, 0, 0.7f);
            _mainPanel.AddChild(background);

            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "装备外观";
            _titleLabel.SetAnchor(AnchorPresets.TopWide);
            _titleLabel.AddThemeFontSizeOverride("font_size", 32);
            _titleLabel.Position = new Vector2(0, 20);
            _titleLabel.Align = Label.AlignEnum.Center;
            _mainPanel.AddChild(_titleLabel);

            // 金币显示
            _goldLabel = new Label();
            _goldLabel.SetAnchor(AnchorPresets.TopRight);
            _goldLabel.Position = new Vector2(-200, 20);
            _goldLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainPanel.AddChild(_goldLabel);

            // 关闭按钮
            Button closeButton = new Button();
            closeButton.Text = "×";
            closeButton.SetAnchor(AnchorPresets.TopRight);
            closeButton.Position = new Vector2(-50, 15);
            closeButton.Size = new Vector2(40, 40);
            closeButton.Pressed += () => Hide();
            _mainPanel.AddChild(closeButton);

            // TabContainer
            _tabContainer = new TabContainer();
            _tabContainer.SetAnchor(AnchorPresets.FullRect);
            _tabContainer.Position = new Vector2(50, 80);
            _tabContainer.Size = new Vector2(700, 450);
            _mainPanel.AddChild(_tabContainer);

            // 武器外观面板
            _weaponPanel = CreateVisualPanel("weapon");
            _weaponList = _weaponPanel.GetNode<ItemList>("VisualList");
            _weaponDescription = _weaponPanel.GetNode<Label>("Description");
            _equipWeaponButton = _weaponPanel.GetNode<Button>("EquipButton");
            _tabContainer.AddChild(_weaponPanel);
            _tabContainer.SetTabTitle(0, "武器外观");

            // 防具外观面板
            _armorPanel = CreateVisualPanel("armor");
            _armorList = _armorPanel.GetNode<ItemList>("VisualList");
            _armorDescription = _armorPanel.GetNode<Label>("Description");
            _equipArmorButton = _armorPanel.GetNode<Button>("EquipButton");
            _tabContainer.AddChild(_armorPanel);
            _tabContainer.SetTabTitle(1, "防具外观");

            // 饰品外观面板
            _accessoryPanel = CreateVisualPanel("accessory");
            _accessoryList = _accessoryPanel.GetNode<ItemList>("VisualList");
            _accessoryDescription = _accessoryPanel.GetNode<Label>("Description");
            _equipAccessoryButton = _accessoryPanel.GetNode<Button>("EquipButton");
            _tabContainer.AddChild(_accessoryPanel);
            _tabContainer.SetTabTitle(2, "饰品外观");

            // 初始隐藏
            Hide();
        }

        private Control CreateVisualPanel(string type) {
            Control panel = new Control();
            panel.Name = type + "_panel";

            // 外观列表
            ItemList list = new ItemList();
            list.Name = "VisualList";
            list.SetAnchor(AnchorPresets.LeftWide);
            list.Position = new Vector2(20, 20);
            list.Size = new Vector2(300, 350);
            list.ItemSelected += (index) => OnVisualSelected(type, index);
            panel.AddChild(list);

            // 描述区域
            Label descLabel = new Label();
            descLabel.Name = "Description";
            descLabel.SetAnchor(AnchorPresets.RightWide);
            descLabel.Position = new Vector2(350, 20);
            descLabel.Size = new Vector2(300, 200);
            descLabel.Text = "选择一个外观查看详情";
            panel.AddChild(descLabel);

            // 装备按钮
            Button equipBtn = new Button();
            equipBtn.Name = "EquipButton";
            equipBtn.Text = "装备";
            equipBtn.SetAnchor(AnchorPresets.BottomWide);
            equipBtn.Position = new Vector2(350, -80);
            equipBtn.Size = new Vector2(300, 50);
            equipBtn.Disabled = true;
            panel.AddChild(equipBtn);

            return panel;
        }

        private void ConnectSignals() {
            _equipWeaponButton.Pressed += () => EquipVisual("weapon", _selectedWeaponId);
            _equipArmorButton.Pressed += () => EquipVisual("armor", _selectedArmorId);
            _equipAccessoryButton.Pressed += () => EquipVisual("accessory", _selectedAccessoryId);
        }

        private void RefreshVisuals() {
            // 刷新武器外观列表
            _weaponList.Clear();
            var weaponVisuals = EquipmentVisuals.Instance.GetAllWeaponVisuals();
            foreach (var kvp in weaponVisuals) {
                string displayText = kvp.Value.Name;
                if (kvp.Value.Rarity == "legendary") {
                    displayText = "★ " + displayText;
                } else if (kvp.Value.Rarity == "epic") {
                    displayText = "☆ " + displayText;
                }
                _weaponList.AddItem(displayText);
            }

            // 刷新防具外观列表
            _armorList.Clear();
            var armorVisuals = EquipmentVisuals.Instance.GetAllArmorVisuals();
            foreach (var kvp in armorVisuals) {
                string displayText = kvp.Value.Name;
                if (kvp.Value.Rarity == "legendary") {
                    displayText = "★ " + displayText;
                } else if (kvp.Value.Rarity == "epic") {
                    displayText = "☆ " + displayText;
                }
                _armorList.AddItem(displayText);
            }

            // 刷新饰品外观列表
            _accessoryList.Clear();
            var accessoryVisuals = EquipmentVisuals.Instance.GetAllAccessoryVisuals();
            foreach (var kvp in accessoryVisuals) {
                string displayText = kvp.Value.Name;
                if (kvp.Value.Rarity == "legendary") {
                    displayText = "★ " + displayText;
                } else if (kvp.Value.Rarity == "epic") {
                    displayText = "☆ " + displayText;
                }
                _accessoryList.AddItem(displayText);
            }

            // 更新金币显示
            UpdateGoldDisplay();
        }

        private void UpdateGoldDisplay() {
            var player = GetTree().CurrentScene.GetNodeOrNull<Player>("../Player");
            if (player != null) {
                _goldLabel.Text = $"金币: {player.Gold}";
            }
        }

        private void OnVisualSelected(string type, int index) {
            if (type == "weapon") {
                var visuals = EquipmentVisuals.Instance.GetAllWeaponVisuals();
                int i = 0;
                foreach (var kvp in visuals) {
                    if (i == index) {
                        _selectedWeaponId = kvp.Key;
                        var visual = kvp.Value;
                        _weaponDescription.Text = $"{visual.Name}\n\n品质: {GetRarityText(visual.Rarity)}\n\n{visual.Description}\n\n解锁条件: {GetUnlockText(visual.UnlockRequirement)}";
                        _equipWeaponButton.Disabled = false; 
                        break;
                    }
                    i++;
                }
            } else if (type == "armor") {
                var visuals = EquipmentVisuals.Instance.GetAllArmorVisuals();
                int i = 0;
                foreach (var kvp in visuals) {
                    if (i == index) {
                        _selectedArmorId = kvp.Key;
                        var visual = kvp.Value;
                        _armorDescription.Text = $"{visual.Name}\n\n品质: {GetRarityText(visual.Rarity)}\n\n{visual.Description}\n\n解锁条件: {GetUnlockText(visual.UnlockRequirement)}";
                        _equipArmorButton.Disabled = false; 
                        break;
                    }
                    i++;
                }
            } else if (type == "accessory") {
                var visuals = EquipmentVisuals.Instance.GetAllAccessoryVisuals();
                int i = 0;
                foreach (var kvp in visuals) {
                    if (i == index) {
                        _selectedAccessoryId = kvp.Key;
                        var visual = kvp.Value;
                        _accessoryDescription.Text = $"{visual.Name}\n\n品质: {GetRarityText(visual.Rarity)}\n\n{visual.Description}\n\n解锁条件: {GetUnlockText(visual.UnlockRequirement)}";
                        _equipAccessoryButton.Disabled = false; 
                        break;
                    }
                    i++;
                }
            }
        }

        private string GetRarityText(string rarity) {
            switch (rarity) {
                case "common": return "普通";
                case "uncommon": return "优秀";
                case "rare": return "稀有";
                case "epic": return "史诗";
                case "legendary": return "传说";
                default: return rarity;
            }
        }

        private string GetUnlockText(string requirement) {
            if (string.IsNullOrEmpty(requirement)) return "已解锁";
            return requirement;
        }

        private void EquipVisual(string slot, string visualId) {
            if (string.IsNullOrEmpty(visualId)) return;

            // 检查是否已解锁
            if (!EquipmentVisuals.Instance.IsVisualUnlocked(slot, visualId)) {
                GD.Print("该外观尚未解锁");
                return;
            }

            // 装备外观
            if (slot == "weapon") {
                EquipmentVisuals.Instance.SetWeaponVisual(visualId);
            } else if (slot == "armor") {
                EquipmentVisuals.Instance.SetArmorVisual(visualId);
            } else if (slot == "accessory") {
                EquipmentVisuals.Instance.SetAccessoryVisual(visualId);
            }

            EmitSignal(nameof(OnVisualEquipped), slot, visualId);
            GD.Print($"已装备外观: {slot} - {visualId}");
        }

        public void Toggle() {
            if (Visible) {
                Hide();
            } else {
                Show();
                RefreshVisuals();
            }
        }

        public override void _Input(InputEvent @event) {
            if (@event.IsActionPressed("ui_cancel")) {
                Hide();
            }
        }
    }
}
