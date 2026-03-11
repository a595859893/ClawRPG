using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 附魔UI - 附魔界面管理
    /// </summary>
    public class EnchantmentUI : Control {
        // 界面元素
        private PanelContainer _mainPanel;
        private VBoxContainer _mainVBox;
        
        // 附魔类型筛选
        private OptionButton _typeFilter;
        
        // 附魔列表
        private ItemList _enchantmentList;
        
        // 附魔详情
        private RichTextLabel _detailName;
        private RichTextLabel _detailDescription;
        private RichTextLabel _detailType;
        private RichTextLabel _detailRarity;
        private RichTextLabel _detailLevel;
        private RichTextLabel _detailCost;
        private RichTextLabel _detailAttributes;
        
        // 操作按钮
        private Button _applyButton;
        private Button _closeButton;
        
        // 当前选中的附魔
        private EnchantmentData _selectedEnchantment;
        private int _playerLevel = 1;

        // 玩家金币显示
        private Label _goldLabel;

        public override void _Ready() {
            SetupUI();
            RefreshEnchantmentList();
        }

        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI() {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);

            _mainVBox = new VBoxContainer();
            _mainVBox.SetSeparation(10);
            _mainPanel.AddChild(_mainVBox);

            // 标题
            Label title = new Label();
            title.Text = "附魔系统";
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 24);
            _mainVBox.AddChild(title);

            // 金币显示
            _goldLabel = new Label();
            _goldLabel.HorizontalAlignment = HorizontalAlignment.Right;
            UpdateGoldDisplay();
            _mainVBox.AddChild(_goldLabel);

            // 筛选器
            HBoxContainer filterContainer = new HBoxContainer();
            filterContainer.SetSeparation(10);
            
            Label filterLabel = new Label();
            filterLabel.Text = "类型筛选:";
            filterContainer.AddChild(filterLabel);

            _typeFilter = new OptionButton();
            _typeFilter.AddItem("全部", 0);
            _typeFilter.AddItem("武器", (int)EnchantmentType.Weapon);
            _typeFilter.AddItem("防具", (int)EnchantmentType.Armor);
            _typeFilter.AddItem("饰品", (int)EnchantmentType.Accessory);
            _typeFilter.AddItem("通用", (int)EnchantmentType.Universal);
            _typeFilter.Selected = 0;
            _typeFilter.ItemSelected += OnTypeFilterChanged;
            filterContainer.AddChild(_typeFilter);
            
            _mainVBox.AddChild(filterContainer);

            // 主内容区域
            HSplitContainer splitContainer = new HSplitContainer();
            splitContainer.SetSeparation(10);
            _mainVBox.AddChild(splitContainer);

            // 左侧 - 附魔列表
            VBoxContainer listContainer = new VBoxContainer();
            listContainer.SetSeparation(5);
            listContainer.CustomMinimumSize = new Vector2(250, 0);
            splitContainer.AddChild(listContainer);

            Label listLabel = new Label();
            listLabel.Text = "可用附魔:";
            listContainer.AddChild(listLabel);

            _enchantmentList = new ItemList();
            _enchantmentList.CustomMinimumSize = new Vector2(250, 400);
            _enchantmentList.ItemSelected += OnEnchantmentSelected;
            listContainer.AddChild(_enchantmentList);

            // 右侧 - 详情面板
            VBoxContainer detailContainer = new VBoxContainer();
            detailContainer.SetSeparation(10);
            detailContainer.CustomMinimumSize = new Vector2(450, 0);
            splitContainer.AddChild(detailContainer);

            Label detailLabel = new Label();
            detailLabel.Text = "附魔详情:";
            detailContainer.AddChild(detailLabel);

            // 详情内容
            PanelContainer detailPanel = new PanelContainer();
            detailPanel.CustomMinimumSize = new Vector2(450, 350);
            detailContainer.AddChild(detailPanel);

            VBoxContainer detailVBox = new VBoxContainer();
            detailVBox.SetSeparation(5);
            detailPanel.AddChild(detailVBox);

            _detailName = new RichTextLabel();
            _detailName.BbcodeEnabled = true;
            _detailName.CustomMinimumSize = new Vector2(0, 30);
            detailVBox.AddChild(_detailName);

            _detailDescription = new RichTextLabel();
            _detailDescription.BbcodeEnabled = true;
            _detailDescription.CustomMinimumSize = new Vector2(0, 40);
            detailVBox.AddChild(_detailDescription);

            _detailType = new RichTextLabel();
            _detailType.BbcodeEnabled = true;
            detailVBox.AddChild(_detailType);

            _detailRarity = new RichTextLabel();
            _detailRarity.BbcodeEnabled = true;
            detailVBox.AddChild(_detailRarity);

            _detailLevel = new RichTextLabel();
            _detailLevel.BbcodeEnabled = true;
            detailVBox.AddChild(_detailLevel);

            _detailCost = new RichTextLabel();
            _detailCost.BbcodeEnabled = true;
            detailVBox.AddChild(_detailCost);

            _detailAttributes = new RichTextLabel();
            _detailAttributes.BbcodeEnabled = true;
            _detailAttributes.CustomMinimumSize = new Vector2(0, 150);
            detailVBox.AddChild(_detailAttributes);

            // 按钮区域
            HBoxContainer buttonContainer = new HBoxContainer();
            buttonContainer.SetSeparation(20);
            buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
            _mainVBox.AddChild(buttonContainer);

            _applyButton = new Button();
            _applyButton.Text = "应用附魔";
            _applyButton.CustomMinimumSize = new Vector2(150, 40);
            _applyButton.Disabled = true;
            _applyButton.Pressed += OnApplyPressed;
            buttonContainer.AddChild(_applyButton);

            _closeButton = new Button();
            _closeButton.Text = "关闭 (ESC)";
            _closeButton.CustomMinimumSize = new Vector2(150, 40);
            _closeButton.Pressed += OnClosePressed;
            buttonContainer.AddChild(_closeButton);

            // 更新玩家等级
            UpdatePlayerLevel();
        }

        /// <summary>
        /// 更新玩家等级
        /// </summary>
        private void UpdatePlayerLevel() {
            var player = GameManager.Instance?.Player;
            if (player != null) {
                _playerLevel = player.Level;
            }
        }

        /// <summary>
        /// 更新金币显示
        /// </summary>
        private void UpdateGoldDisplay() {
            var player = GameManager.Instance?.Player;
            if (player != null) {
                _goldLabel.Text = $"金币: {player.Gold:N0}";
            } else {
                _goldLabel.Text = "金币: 0";
            }
        }

        /// <summary>
        /// 刷新附魔列表
        /// </summary>
        private void RefreshEnchantmentList() {
            _enchantmentList.Clear();
            
            var selectedType = (EnchantmentType)_typeFilter.GetSelectedId();
            List<EnchantmentData> enchantments;

            if (selectedType == 0) {
                enchantments = EnchantmentDatabase.Instance.GetAvailableEnchantments(_playerLevel);
            } else {
                enchantments = EnchantmentDatabase.Instance.GetEnchantmentsByType(selectedType);
                enchantments.RemoveAll(e => e.RequiredLevel > _playerLevel);
            }

            foreach (var enchant in enchantments) {
                string displayName = enchant.Name;
                Color rarityColor = enchant.GetRarityColor();
                
                // 添加解锁状态标记
                bool isUnlocked = EnchantmentSystem.Instance.IsEnchantmentUnlocked(enchant.Id);
                if (!isUnlocked) {
                    displayName += " 🔒";
                }
                
                int index = _enchantmentList.AddItem(displayName);
                _enchantmentList.SetItemCustomFgColor(index, rarityColor);
            }
        }

        /// <summary>
        /// 类型筛选改变
        /// </summary>
        private void OnTypeFilterChanged(long index) {
            RefreshEnchantmentList();
            ClearDetail();
        }

        /// <summary>
        /// 选择附魔
        /// </summary>
        private void OnEnchantmentSelected(long index) {
            var selectedType = (EnchantmentType)_typeFilter.GetSelectedId();
            List<EnchantmentData> enchantments;

            if (selectedType == 0) {
                enchantments = EnchantmentDatabase.Instance.GetAvailableEnchantments(_playerLevel);
            } else {
                enchantments = EnchantmentDatabase.Instance.GetEnchantmentsByType(selectedType);
                enchantments.RemoveAll(e => e.RequiredLevel > _playerLevel);
            }

            if (index >= 0 && index < enchantments.Count) {
                _selectedEnchantment = enchantments[(int)index];
                UpdateDetailDisplay();
            }
        }

        /// <summary>
        /// 更新详情显示
        /// </summary>
        private void UpdateDetailDisplay() {
            if (_selectedEnchantment == null) return;

            Color rarityColor = _selectedEnchantment.GetRarityColor();
            bool isUnlocked = EnchantmentSystem.Instance.IsEnchantmentUnlocked(_selectedEnchantment.Id);
            string lockStatus = isUnlocked ? " (已解锁)" : " (未解锁)";

            _detailName.Text = $"[color=#{rarityColor.ToHtml()}]{_selectedEnchantment.Name}[/color]{lockStatus}";
            _detailDescription.Text = _selectedEnchantment.Description;
            _detailType.Text = $"类型: {GetTypeName(_selectedEnchantment.Type)}";
            _detailRarity.Text = $"稀有度: [color=#{rarityColor.ToHtml()}]{_selectedEnchantment.GetRarityName()}[/color]";
            _detailLevel.Text = $"需求等级: {_selectedEnchantment.RequiredLevel} (当前: {_playerLevel})";
            _detailCost.Text = $"费用: {_selectedEnchantment.GoldCost} 金币";

            // 属性列表
            string attrs = "属性加成:\n";
            foreach (var attr in _selectedEnchantment.Attributes) {
                attrs += $"  • {GetAttributeName(attr.Key)}: +{attr.Value}\n";
            }
            _detailAttributes.Text = attrs;

            // 启用/禁用应用按钮
            bool canApply = isUnlocked && _selectedEnchantment.RequiredLevel <= _playerLevel;
            _applyButton.Disabled = !canApply;
        }

        /// <summary>
        /// 清除详情显示
        /// </summary>
        private void ClearDetail() {
            _detailName.Text = "";
            _detailDescription.Text = "";
            _detailType.Text = "";
            _detailRarity.Text = "";
            _detailLevel.Text = "";
            _detailCost.Text = "";
            _detailAttributes.Text = "";
            _applyButton.Disabled = true;
            _selectedEnchantment = null;
        }

        /// <summary>
        /// 应用按钮按下
        /// </summary>
        private void OnApplyPressed() {
            if (_selectedEnchantment == null) return;

            // 显示提示消息
            string message = $"请在背包中选择要附魔的装备\n然后使用附魔功能";
            ShowMessage(message);
        }

        /// <summary>
        /// 关闭按钮按下
        /// </summary>
        private void OnClosePressed() {
            Hide();
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        private void ShowMessage(string message) {
            var dialog = new AcceptDialog();
            dialog.Title = "附魔系统";
            dialog.DialogText = message;
            AddChild(dialog);
            dialog.PopupCentered(new Vector2(400, 200));
            dialog.CloseRequested += () => dialog.QueueFree();
        }

        /// <summary>
        /// 获取类型名称
        /// </summary>
        private string GetTypeName(EnchantmentType type) {
            switch (type) {
                case EnchantmentType.Weapon: return "武器";
                case EnchantmentType.Armor: return "防具";
                case EnchantmentType.Accessory: return "饰品";
                case EnchantmentType.Universal: return "通用";
                default: return "未知";
            }
        }

        /// <summary>
        /// 获取属性名称
        /// </summary>
        private string GetAttributeName(EnchantmentAttribute attr) {
            switch (attr) {
                case EnchantmentAttribute.Damage: return "伤害";
                case EnchantmentAttribute.AttackSpeed: return "攻击速度";
                case EnchantmentAttribute.CriticalRate: return "暴击率";
                case EnchantmentAttribute.CriticalDamage: return "暴击伤害";
                case EnchantmentAttribute.Defense: return "防御";
                case EnchantmentAttribute.Health: return "生命";
                case EnchantmentAttribute.Mana: return "法力";
                case EnchantmentAttribute.HealthRegen: return "生命恢复";
                case EnchantmentAttribute.ManaRegen: return "法力恢复";
                case EnchantmentAttribute.MoveSpeed: return "移动速度";
                case EnchantmentAttribute.FireResistance: return "火焰抗性";
                case EnchantmentAttribute.IceResistance: return "冰霜抗性";
                case EnchantmentAttribute.ThunderResistance: return "雷电抗性";
                case EnchantmentAttribute.DarkResistance: return "暗影抗性";
                case EnchantmentAttribute.LightResistance: return "光明抗性";
                default: return attr.ToString();
            }
        }

        public override void _Input(InputEvent evt) {
            if (evt.IsActionPressed("ui_cancel")) {
                Hide();
            }
        }

        /// <summary>
        /// 显示界面
        /// </summary>
        public new void Show() {
            UpdatePlayerLevel();
            UpdateGoldDisplay();
            RefreshEnchantmentList();
            ClearDetail();
            base.Show();
        }
    }
}
