using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.Enchantment;
using ClawRPG.Systems.Enchantment;

namespace ClawRPG.Scripts.Systems.Enchantment
{
    /// <summary>
    /// 附魔系统UI界面
    /// </summary>
    public partial class EnchantmentUI : Control
    {
        // UI组件
        private PanelContainer _mainPanel;
        private VBoxContainer _contentBox;
        private TabContainer _tabContainer;
        
        // 附魔列表
        private ItemList _enchantmentList;
        
        // 附魔详情
        private Label _enchantmentName;
        private Label _enchantmentDescription;
        private Label _enchantmentStats;
        private Label _enchantmentCost;
        private Label _enchantmentSuccessRate;
        
        // 按钮
        private Button _enchantButton;
        private Button _closeButton;
        
        // 当前选中
        private string _selectedEnchantmentId;
        private EnchantmentType _currentFilterType = EnchantmentType.Universal;
        private EnchantmentTier? _currentFilterTier = null;
        
        // 玩家ID (假设为 "player")
        private string _playerId = "player";
        
        // 是否可见
        private bool _isVisible = false;
        
        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
            RefreshEnchantmentList();
        }
        
        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);
            
            // 内容容器
            _contentBox = new VBoxContainer();
            _contentBox.Setanchorspreset(Control.LayoutPreset.FullRect);
            _contentBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(_contentBox);
            
            // 标题栏
            var titleBar = new HBoxContainer();
            titleBar.AddThemeConstantOverride("separation", 10);
            _contentBox.AddChild(titleBar);
            
            var titleLabel = new Label();
            titleLabel.Text = "附魔系统";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(titleLabel);
            
            titleBar.AddChild(new Control()); // Spacer
            
            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            titleBar.AddChild(_closeButton);
            
            // 过滤器
            var filterBar = new HBoxContainer();
            filterBar.AddThemeConstantOverride("separation", 10);
            _contentBox.AddChild(filterBar);
            
            var typeLabel = new Label();
            typeLabel.Text = "类型:";
            filterBar.AddChild(typeLabel);
            
            var typeOptions = new OptionButton();
            typeOptions.AddItem("全部", 0);
            typeOptions.AddItem("武器", (int)EnchantmentType.Weapon);
            typeOptions.AddItem("护甲", (int)EnchantmentType.Armor);
            typeOptions.AddItem("饰品", (int)EnchantmentType.Accessory);
            typeOptions.AddItem("通用", (int)EnchantmentType.Universal);
            typeOptions.ItemSelected += OnTypeSelected;
            filterBar.AddChild(typeOptions);
            
            var tierLabel = new Label();
            tierLabel.Text = "  稀有度:";
            filterBar.AddChild(tierLabel);
            
            var tierOptions = new OptionButton();
            tierOptions.AddItem("全部", 0);
            tierOptions.AddItem("普通", (int)EnchantmentTier.Common);
            tierOptions.AddItem("优秀", (int)EnchantmentTier.Uncommon);
            tierOptions.AddItem("稀有", (int)EnchantmentTier.Rare);
            tierOptions.AddItem("史诗", (int)EnchantmentTier.Epic);
            tierOptions.AddItem("传说", (int)EnchantmentTier.Legendary);
            tierOptions.ItemSelected += OnTierSelected;
            filterBar.AddChild(tierOptions);
            
            // 标签页容器
            _tabContainer = new TabContainer();
            _tabContainer.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlagsFlags.ExpandFill);
            _contentBox.AddChild(_tabContainer);
            
            // 附魔列表页
            var listTab = new Control();
            listTab.Name = "附魔列表";
            _tabContainer.AddChild(listTab);
            
            var listScroll = new ScrollContainer();
            listScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            listTab.AddChild(listScroll);
            
            _enchantmentList = new ItemList();
            _enchantmentList.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _enchantmentList.ItemSelected += OnEnchantmentSelected;
            listScroll.AddChild(_enchantmentList);
            
            // 详情页
            var detailTab = new Control();
            detailTab.Name = "附魔详情";
            _tabContainer.AddChild(detailTab);
            
            var detailScroll = new ScrollContainer();
            detailScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            detailTab.AddChild(detailScroll);
            
            var detailBox = new VBoxContainer();
            detailBox.AddThemeConstantOverride("separation", 15);
            detailScroll.AddChild(detailBox);
            
            _enchantmentName = new Label();
            _enchantmentName.AddThemeFontSizeOverride("font_size", 20);
            detailBox.AddChild(_enchantmentName);
            
            _enchantmentDescription = new Label();
            detailBox.AddChild(_enchantmentDescription);
            
            var statsTitle = new Label();
            statsTitle.Text = "属性效果:";
            statsTitle.AddThemeFontSizeOverride("font_size", 16);
            detailBox.AddChild(statsTitle);
            
            _enchantmentStats = new Label();
            detailBox.AddChild(_enchantmentStats);
            
            var costTitle = new Label();
            costTitle.Text = "附魔费用:";
            costTitle.AddThemeFontSizeOverride("font_size", 16);
            detailBox.AddChild(costTitle);
            
            _enchantmentCost = new Label();
            detailBox.AddChild(_enchantmentCost);
            
            var rateTitle = new Label();
            rateTitle.Text = "成功率:";
            rateTitle.AddThemeFontSizeOverride("font_size", 16);
            detailBox.AddChild(rateTitle);
            
            _enchantmentSuccessRate = new Label();
            detailBox.AddChild(_enchantmentSuccessRate);
            
            // 统计页
            var statsTab = new Control();
            statsTab.Name = "统计";
            _tabContainer.AddChild(statsTab);
            
            var statsScroll = new ScrollContainer();
            statsScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            statsTab.AddChild(statsScroll);
            
            var statsBox = new VBoxContainer();
            statsBox.AddThemeConstantOverride("separation", 10);
            statsScroll.AddChild(statsBox);
            
            // 创建统计标签（稍后填充）
            statsBox.Name = "StatsBox";
            
            // 附魔按钮
            _enchantButton = new Button();
            _enchantButton.Text = "开始附魔";
            _enchantButton.CustomMinimumSize = new Vector2(200, 50);
            _enchantButton.Disabled = true;
            _contentBox.AddChild(_enchantButton);
            
            // 初始隐藏
            _mainPanel.Visible = false;
        }
        
        /// <summary>
        /// 连接信号
        /// </summary>
        private void ConnectSignals()
        {
            _closeButton.Pressed += OnClosePressed;
            _enchantButton.Pressed += OnEnchantPressed;
            
            // 连接到附魔系统信号
            if (EnchantmentSystem.Instance != null)
            {
                EnchantmentSystem.Instance.EnchantmentResult += OnEnchantmentResult;
            }
        }
        
        /// <summary>
        /// 刷新附魔列表
        /// </summary>
        public void RefreshEnchantmentList()
        {
            _enchantmentList.Clear();
            
            var enchantments = EnchantmentDatabase.Instance.GetAllEnchantments();
            
            foreach (var enchantment in enchantments)
            {
                // 应用过滤器
                if (_currentFilterType != EnchantmentType.Universal && enchantment.Type != _currentFilterType)
                    continue;
                
                if (_currentFilterTier.HasValue && enchantment.Tier != _currentFilterTier.Value)
                    continue;
                
                // 获取Tier颜色
                string tierColor = GetTierColor(enchantment.Tier);
                string displayText = $"[{tierColor}]{enchantment.Name}[/] - {enchantment.Description}";
                
                _enchantmentList.AddItem(displayText);
            }
            
            RefreshFocusPoints();
            RefreshStatistics();
        }
        
        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        private string GetTierColor(EnchantmentTier tier)
        {
            switch (tier)
            {
                case EnchantmentTier.Common:
                    return "white";
                case EnchantmentTier.Uncommon:
                    return "green";
                case EnchantmentTier.Rare:
                    return "blue";
                case EnchantmentTier.Epic:
                    return "purple";
                case EnchantmentTier.Legendary:
                    return "gold";
                default:
                    return "white";
            }
        }
        
        /// <summary>
        /// 类型选择回调
        /// </summary>
        private void OnTypeSelected(long index)
        {
            _currentFilterType = (EnchantmentType)index;
            RefreshEnchantmentList();
        }
        
        /// <summary>
        /// 稀有度选择回调
        /// </summary>
        private void OnTierSelected(long index)
        {
            if (index == 0)
                _currentFilterTier = null;
            else
                _currentFilterTier = (EnchantmentTier)(index - 1);
            
            RefreshEnchantmentList();
        }
        
        /// <summary>
        /// 附魔选择回调
        /// </summary>
        private void OnEnchantmentSelected(long index)
        {
            var items = _enchantmentList.GetItemText((int)index);
            
            // 查找对应的附魔
            var enchantments = EnchantmentDatabase.Instance.GetAllEnchantments();
            foreach (var enchantment in enchantments)
            {
                if (items.Contains(enchantment.Name))
                {
                    _selectedEnchantmentId = enchantment.Id;
                    ShowEnchantmentDetails(enchantment);
                    break;
                }
            }
        }
        
        /// <summary>
        /// 显示附魔详情
        /// </summary>
        private void ShowEnchantmentDetails(EnchantmentRecord enchantment)
        {
            _enchantmentName.Text = $"[color={GetTierColor(enchantment.Tier)}]{enchantment.Name}[/color]";
            _enchantmentDescription.Text = enchantment.Description;
            
            // 显示属性
            string statsText = $"{GetEffectName(enchantment.PrimaryEffect)}: +{enchantment.PrimaryEffectValue}";
            if (enchantment.SecondaryEffect.HasValue)
            {
                statsText += $"\n{GetEffectName(enchantment.SecondaryEffect.Value)}: +{enchantment.SecondaryEffectValue}";
            }
            _enchantmentStats.Text = statsText;
            
            // 显示费用
            int cost = enchantment.EnchantmentCost;
            _enchantmentCost.Text = $"{cost} 金币";
            
            // 显示成功率
            _enchantmentSuccessRate.Text = $"{enchantment.SuccessRate}% (基础)";
            
            // 启用附魔按钮
            _enchantButton.Disabled = false;
            
            // 切换到详情页
            _tabContainer.CurrentTab = 1;
        }
        
        /// <summary>
        /// 获取效果名称
        /// </summary>
        private string GetEffectName(EnchantmentEffect effect)
        {
            switch (effect)
            {
                case EnchantmentEffect.Damage: return "攻击力";
                case EnchantmentEffect.CriticalRate: return "暴击率";
                case EnchantmentEffect.CriticalDamage: return "暴击伤害";
                case EnchantmentEffect.AttackSpeed: return "攻击速度";
                case EnchantmentEffect.LifeSteal: return "生命偷取";
                case EnchantmentEffect.Defense: return "防御力";
                case EnchantmentEffect.Health: return "生命值";
                case EnchantmentEffect.Mana: return "法力值";
                case EnchantmentEffect.ManaRegen: return "法力回复";
                case EnchantmentEffect.Speed: return "移动速度";
                case EnchantmentEffect.Dodge: return "闪避率";
                case EnchantmentEffect.FireResistance: return "火焰抗性";
                case EnchantmentEffect.IceResistance: return "冰霜抗性";
                case EnchantmentEffect.LightningResistance: return "闪电抗性";
                case EnchantmentEffect.PoisonResistance: return "毒抗性";
                case EnchantmentEffect.AllAttributes: return "全属性";
                case EnchantmentEffect.Strength: return "力量";
                case EnchantmentEffect.Intelligence: return "智力";
                case EnchantmentEffect.Dexterity: return "敏捷";
                case EnchantmentEffect.Vitality: return "体力";
                case EnchantmentEffect.Luck: return "幸运";
                default: return effect.ToString();
            }
        }
        
        /// <summary>
        /// 刷新专注力显示（已移除：FocusPoints不在新API中）
        /// </summary>
        private void RefreshFocusPoints()
        {
            // FocusPoints 已从新 API 移除，此方法保留但无操作
        }
        
        /// <summary>
        /// 刷新统计信息
        /// </summary>
        private void RefreshStatistics()
        {
            var statsBox = _tabContainer.FindChild("统计", true, false)?
                .GetChild(0)?.GetChild(0) as VBoxContainer;
            
            if (statsBox == null) return;
            
            // 清除现有内容
            foreach (var child in statsBox.GetChildren())
            {
                child.QueueFree();
            }
            
            // 新 API：使用 GetInventory 获取库存信息
            var inventory = EnchantmentSystem.Instance.GetInventory();
            int totalOwned = 0;
            foreach (var kvp in inventory)
                totalOwned += kvp.Value;
            
            var totalLabel = new Label();
            totalLabel.Text = $"持有附魔卷轴: {totalOwned}";
            statsBox.AddChild(totalLabel);
            
            var unlockedLabel = new Label();
            unlockedLabel.Text = $"\n已解锁类型: {inventory.Count} / {EnchantmentDatabase.Instance.GetTotalCount()}";
            statsBox.AddChild(unlockedLabel);
            
            // 显示各卷轴库存
            var inventoryTitle = new Label();
            inventoryTitle.Text = "\n卷轴库存:";
            inventoryTitle.AddThemeFontSizeOverride("font_size", 14);
            statsBox.AddChild(inventoryTitle);
            
            foreach (var kvp in inventory)
            {
                var record = EnchantmentDatabase.Instance.GetEnchantment(kvp.Key);
                string name = record != null ? record.Name : kvp.Key;
                var itemLabel = new Label();
                itemLabel.Text = $"  {name}: {kvp.Value}个";
                statsBox.AddChild(itemLabel);
            }
        }
        
        /// <summary>
        /// 关闭按钮回调
        /// </summary>
        private void OnClosePressed()
        {
            Toggle();
        }
        
        /// <summary>
        /// 附魔按钮回调
        /// </summary>
        private void OnEnchantPressed()
        {
            if (string.IsNullOrEmpty(_selectedEnchantmentId)) return;
            
            // 获取玩家等级（如果存在PlayerSystem则获取真实等级，否则使用默认值1）
            int playerLevel = 1;
            var player = GetTree().CurrentScene?.FindChild("Player", true, false) as Godot.Node;
            if (player != null)
            {
                var levelProp = player.Get("Level");
                if (levelProp != null) playerLevel = (int)levelProp;
            }
            
            // 使用默认装备槽位0（武器），新API使用int而非string
            int equipmentSlot = 0;
            
            bool success = EnchantmentSystem.Instance.Enchant(_selectedEnchantmentId, playerLevel, equipmentSlot);
            
            // 显示结果（信号 EnchantmentResult 也会触发 OnEnchantmentResult）
            string resultText = success ? "附魔成功!" : "附魔失败!";
            GD.Print($"EnchantmentUI: {resultText}");
            
            // 刷新列表（信号会在适当时机触发，这里主动刷新确保UI同步）
            RefreshEnchantmentList();
        }
        
        /// <summary>
        /// 附魔结果回调（新API：EnchantmentResult信号）
        /// </summary>
        private void OnEnchantmentResult(bool success, string message)
        {
            RefreshEnchantmentList();
            GD.Print($"EnchantmentUI: {message}");
        }
        
        /// <summary>
        /// 切换可见性
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
            _mainPanel.Visible = _isVisible;
            
            if (_isVisible)
            {
                RefreshEnchantmentList();
            }
        }
        
        /// <summary>
        /// 显示界面
        /// </summary>
        public void Show()
        {
            _isVisible = true;
            _mainPanel.Visible = true;
            RefreshEnchantmentList();
        }
        
        /// <summary>
        /// 隐藏界面
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _mainPanel.Visible = false;
        }
        
        /// <summary>
        /// 处理输入
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // E键切换附魔UI
                if (keyEvent.Keycode == Key.E)
                {
                    Toggle();
                    GetTree().SetInputAsHandled();
                }
            }
        }
    }
}
