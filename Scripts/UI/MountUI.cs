using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Mounts;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 坐骑界面 - O键打开
    /// </summary>
    public partial class MountUI : Control {
        private PanelContainer _mainPanel;
        private VBoxContainer _mainVBox;
        private Label _titleLabel;
        private HBoxContainer _mountListContainer;
        private ScrollContainer _scrollContainer;
        private VBoxContainer _mountListVBox;
        private Label _detailsLabel;
        private Label _noMountsLabel;
        private Button _closeButton;

        private Mount _selectedMount;
        private MountInstance _selectedInstance;

        public override void _Ready() {
            SetupUI();
            Visible = false; 
            
            // 连接信号
            if (MountManager.Instance != null) {
                MountManager.Instance.OnMountAdded += OnMountAdded;
                MountManager.Instance.OnMountRemoved += OnMountRemoved;
                MountManager.Instance.OnMountActivated += OnMountActivated;
                MountManager.Instance.OnMountDeactivated += OnMountDeactivated;
            }
        }

        private void SetupUI() {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 500);
            AddChild(_mainPanel);

            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderColor = new Color(0.6f, 0.5f, 0.3f, 1f);
            styleBox.CornerRadiusTopLeft = 8;
            styleBox.CornerRadiusTopRight = 8;
            styleBox.CornerRadiusBottomLeft = 8;
            styleBox.CornerRadiusBottomRight = 8;
            _mainPanel.AddThemeStyleboxOverride("panel", styleBox);

            // 主垂直容器
            _mainVBox = new VBoxContainer();
            _mainVBox.SetanchorsPreset(Control.LayoutPreset.FullRect);
            _mainPanel.AddChild(_mainVBox);

            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "🐴 坐 骑 系 统 🐴";
            _titleLabel.Align = Label.AlignEnum.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f, 1f));
            _mainVBox.AddChild(_titleLabel);

            // 横向容器
            _mountListContainer = new HBoxContainer();
            _mountListContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _mountListContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _mainVBox.AddChild(_mountListContainer);

            // 坐骑列表（左侧）
            _scrollContainer = new ScrollContainer();
            _scrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _scrollContainer.CustomMinimumSize = new Vector2(300, 0);
            _mountListContainer.AddChild(_scrollContainer);

            _mountListVBox = new VBoxContainer();
            _mountListVBox.SetanchorsPreset(Control.LayoutPreset.FullRect);
            _mountListVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _scrollContainer.AddChild(_mountListVBox);

            // 详情面板（右侧）
            var detailsPanel = new PanelContainer();
            detailsPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            detailsPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            detailsPanel.CustomMinimumSize = new Vector2(300, 0);
            _mountListContainer.AddChild(detailsPanel);

            var detailsStyle = new StyleBoxFlat();
            detailsStyle.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
            detailsStyle.CornerRadiusTopLeft = 4;
            detailsStyle.CornerRadiusTopRight = 4;
            detailsStyle.CornerRadiusBottomLeft = 4;
            detailsStyle.CornerRadiusBottomRight = 4;
            detailsPanel.AddThemeStyleboxOverride("panel", detailsStyle);

            var detailsVBox = new VBoxContainer();
            detailsVBox.SetanchorsPreset(Control.LayoutPreset.FullRect);
            detailsPanel.AddChild(detailsVBox);

            _detailsLabel = new Label();
            _detailsLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _detailsLabel.Align = Label.AlignEnum.Center;
            _detailsLabel.Text = "选择一个坐骑查看详情";
            _detailsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1f));
            detailsVBox.AddChild(_detailsLabel);

            // 无坐骑提示
            _noMountsLabel = new Label();
            _noMountsLabel.Text = "暂无坐骑\n\n可以通过商店购买或在\n探索中发现新的坐骑！";
            _noMountsLabel.Align = Label.AlignEnum.Center;
            _noMountsLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f, 1f));
            _noMountsLabel.Visible = false; 
            detailsVBox.AddChild(_noMountsLabel);

            // 底部按钮
            var buttonContainer = new HBoxContainer();
            buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
            buttonContainer.CustomMinimumSize = new Vector2(0, 50);
            _mainVBox.AddChild(buttonContainer);

            _closeButton = new Button();
            _closeButton.Text = "  关 闭 (O)  ";
            _closeButton.CustomMinimumSize = new Vector2(150, 40);
            _closeButton.Pressed += () => ToggleUI();
            buttonContainer.AddChild(_closeButton);

            // 刷新列表
            RefreshMountList();
        }

        private void RefreshMountList() {
            // 清空现有项
            foreach (Node child in _mountListVBox.GetChildren()) {
                child.QueueFree();
            }

            var ownedMounts = MountManager.Instance.GetOwnedMounts();

            if (ownedMounts.Count == 0) {
                _noMountsLabel.Visible = true;
                return;
            }

            _noMountsLabel.Visible = false; 

            foreach (var kvp in ownedMounts) {
                var mountData = MountDatabase.Instance.GetMount(kvp.Key);
                var instance = kvp.Value;

                if (mountData == null) continue;

                var itemPanel = CreateMountListItem(mountData, instance);
                _mountListVBox.AddChild(itemPanel);
            }
        }

        private PanelContainer CreateMountListItem(Mount mount, MountInstance instance) {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(0, 60);
            panel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            styleBox.BorderWidthBottom = 1;
            styleBox.BorderColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            panel.AddThemeStyleboxOverride("panel", styleBox);

            var hbox = new HBoxContainer();
            panel.AddChild(hbox);

            // 激活状态指示器
            var activeIndicator = new Label();
            activeIndicator.Text = instance.IsActive ? "★" : "  ";
            activeIndicator.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0f, 1f));
            activeIndicator.CustomMinimumSize = new Vector2(30, 0);
            hbox.AddChild(activeIndicator);

            // 坐骑图标/类型
            var iconLabel = new Label();
            iconLabel.Text = GetMountIcon(mount.Type);
            iconLabel.CustomMinimumSize = new Vector2(40, 0);
            hbox.AddChild(iconLabel);

            // 坐骑名称和等级
            var infoVBox = new VBoxContainer();
            infoVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hbox.AddChild(infoVBox);

            var nameLabel = new Label();
            nameLabel.Text = $"{mount.Name}  Lv.{instance.Level}";
            nameLabel.AddThemeColorOverride("font_color", GetRarityColor(mount.Rarity));
            infoVBox.AddChild(nameLabel);

            var typeLabel = new Label();
            typeLabel.Text = $"{GetMountTypeName(mount.Type)} · {GetRarityName(mount.Rarity)}";
            typeLabel.AddThemeFontSizeOverride("font_size", 12);
            typeLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f, 1f));
            infoVBox.AddChild(typeLabel);

            // 交互按钮
            var buttonContainer = new HBoxContainer();
            hbox.AddChild(buttonContainer);

            if (instance.IsActive) {
                var dismountButton = new Button();
                dismountButton.Text = "下马";
                dismountButton.Pressed += () => MountManager.Instance.DeactivateMount();
                buttonContainer.AddChild(dismountButton);
            } else {
                var rideButton = new Button();
                rideButton.Text = "骑乘";
                rideButton.Pressed += () => MountManager.Instance.ActivateMount(mount.MountId);
                buttonContainer.AddChild(rideButton);
            }

            // 点击显示详情
            panel.GuiInput += (inputEvent) => {
                if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
                    ShowMountDetails(mount, instance);
                }
            };

            return panel;
        }

        private void ShowMountDetails(Mount mount, MountInstance instance) {
            _selectedMount = mount;
            _selectedInstance = instance;

            string icon = mount.Type == MountType.Flying ? "🦅" : 
                         mount.Type == MountType.Aquatic ? "🐴" : 
                         mount.Type == MountType.Amphibian ? "🐢" : "🐴";

            string typeName = GetMountTypeName(mount.Type);
            string rarityName = GetRarityName(mount.Rarity);
            string rarityColor = GetRarityColorHex(mount.Rarity);

            int expProgress = instance.Experience;
            int expNeeded = instance.GetExpForNextLevel();
            float progressPercent = (float)expProgress / expNeeded * 100;

            string abilities = "";
            if (mount.CanFly) abilities += "🕊️ 飞行 ";
            if (mount.CanSwim) abilities += "🌊 游泳 ";

            _detailsLabel.Text = $@"
{icon} {mount.Name}

{rarityColor}{rarityName}{"#endregion"} {typeName}
等级: {instance.Level}

{mount.Description}

━━━ 属性加成 ━━━
🏃 速度: +{mount.SpeedBonus}
❤️ 生命: +{mount.HealthBonus}
🛡️ 防御: +{mount.DefenseBonus}
🎒 背包: +{mount.CarryCapacityBonus}

━━━ 特殊能力 ━━━
{(string.IsNullOrEmpty(abilities) ? "无" : abilities)}

━━━ 经验进度 ━━━
{expProgress} / {expNeeded} ({progressPercent:F1}%)
";
        }

        private string GetMountIcon(MountType type) {
            switch (type) {
                case MountType.Flying: return "🦅";
                case MountType.Aquatic: return "🐠";
                case MountType.Amphibian: return "🐢";
                default: return "🐴";
            }
        }

        private string GetMountTypeName(MountType type) {
            switch (type) {
                case MountType.Flying: return "飞行坐骑";
                case MountType.Aquatic: return "水生坐骑";
                case MountType.Amphibian: return "两栖坐骑";
                default: return "陆地坐骑";
            }
        }

        private string GetRarityName(MountRarity rarity) {
            switch (rarity) {
                case MountRarity.Common: return "普通";
                case MountRarity.Uncommon: return "优秀";
                case MountRarity.Rare: return "稀有";
                case MountRarity.Epic: return "史诗";
                case MountRarity.Legendary: return "传说";
                default: return "普通";
            }
        }

        private Color GetRarityColor(MountRarity rarity) {
            switch (rarity) {
                case MountRarity.Common: return new Color(0.7f, 0.7f, 0.7f, 1f);
                case MountRarity.Uncommon: return new Color(0.3f, 0.9f, 0.3f, 1f);
                case MountRarity.Rare: return new Color(0.3f, 0.5f, 1f, 1f);
                case MountRarity.Epic: return new Color(0.7f, 0.3f, 1f, 1f);
                case MountRarity.Legendary: return new Color(1f, 0.6f, 0f, 1f);
                default: return new Color(0.7f, 0.7f, 0.7f, 1f);
            }
        }

        private string GetRarityColorHex(MountRarity rarity) {
            switch (rarity) {
                case MountRarity.Common: return "[color=#b0b0b0]";
                case MountRarity.Uncommon: return "[color=#4ae84a]";
                case MountRarity.Rare: return "[color=#4a80e8]";
                case MountRarity.Epic: return "[color=#b04ae8]";
                case MountRarity.Legendary: return "[color=#ff9900]";
                default: return "[color=#b0b0b0]";
            }
        }

        public void ToggleUI() {
            Visible = !Visible;
            if (Visible) {
                RefreshMountList();
                UpdateInputMode();
            }
        }

        private void UpdateInputMode() {
            if (Visible) {
                Input.SetMouseMode(Input.MouseMode.Visible);
            }
        }

        public override void _Input(InputEvent eventData) {
            if (eventData is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo) {
                if (keyEvent.Keycode == Key.O || keyEvent.Keycode == Key.Escape) {
                    if (Visible) {
                        ToggleUI();
                    }
                }
            }
        }

        private void OnMountAdded(string mountId) {
            if (Visible) RefreshMountList();
        }

        private void OnMountRemoved(string mountId) {
            if (Visible) RefreshMountList();
        }

        private void OnMountActivated(string mountId) {
            if (Visible) RefreshMountList();
        }

        private void OnMountDeactivated() {
            if (Visible) RefreshMountList();
        }
    }
}
