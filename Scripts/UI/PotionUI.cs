using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 药水UI界面 - P键打开
    /// </summary>
    public partial class PotionUI : Control
    {
        private Control _container;
        private VBoxContainer _potionList;
        private Label _titleLabel;
        private Label _infoLabel;
        private Button _closeButton;
        
        // 筛选按钮
        private HBoxContainer _filterButtons;
        
        // 当前筛选类型
        private string _currentFilter = "all";
        
        // 当前选中的药水
        private Items.Potion _selectedPotion;
        private Label _selectedPotionName;
        private Label _selectedPotionDesc;
        private Label _selectedPotionEffect;
        private Button _useButton;
        private CheckButton _autoUseCheck;
        
        // 玩家引用
        private Node _player;
        
        // 资源颜色
        private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
        private Color _uncommonColor = new Color(0.2f, 0.8f, 0.2f);
        private Color _rareColor = new Color(0.2f, 0.5f, 1.0f);
        private Color _epicColor = new Color(0.6f, 0.3f, 0.9f);
        private Color _legendaryColor = new Color(1.0f, 0.6f, 0.0f);
        
        public override void _Ready()
        {
            SetupUI();
            Visible = false;
            
            // 连接信号
            Items.PotionManager.Instance.OnPotionAdded += OnPotionUpdated;
            Items.PotionManager.Instance.OnPotionRemoved += OnPotionUpdated;
            Items.PotionManager.Instance.OnPotionUsed += OnPotionUsed;
        }

        private void SetupUI()
        {
            // 主容器
            _container = new Control();
            _container.SetAnchorsPreset(Control.LayoutPreset.Center);
            _container.CustomMinimumimumSize = new Vector2(600, 500);
            AddChild(_container);
            
            // 背景面板
            var bgPanel = new Panel();
            bgPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            bgPanel.Modulate = new Color(0, 0, 0, 0.85f);
            _container.AddChild(bgPanel);
            
            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "  药水背包  ";
            _titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.Position = new Vector2(0, 10);
            _container.AddChild(_titleLabel);
            
            // 关闭按钮
            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.Position = new Vector2(560, 10);
            _closeButton.Size = new Vector2(30, 30);
            _closeButton.Pressed += () => ToggleUI();
            _container.AddChild(_closeButton);
            
            // 筛选按钮
            _filterButtons = new HBoxContainer();
            _filterButtons.Position = new Vector2(20, 50);
            _filterButtons.Spacing = 10;
            _container.AddChild(_filterButtons);
            
            CreateFilterButton("全部", "all");
            CreateFilterButton("生命", "health");
            CreateFilterButton("法力", "mana");
            CreateFilterButton("增益", "buffs");
            
            // 药水列表
            var scrollContainer = new ScrollContainer();
            scrollContainer.Position = new Vector2(20, 100);
            scrollContainer.Size = new Vector2(250, 350);
            _container.AddChild(scrollContainer);
            
            _potionList = new VBoxContainer();
            _potionList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _potionList.Spacing = 5;
            scrollContainer.AddChild(_potionList);
            
            // 选中药水信息面板
            var infoPanel = new Panel();
            infoPanel.Position = new Vector2(290, 100);
            infoPanel.Size = new Vector2(280, 200);
            _container.AddChild(infoPanel);
            
            _selectedPotionName = new Label();
            _selectedPotionName.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _selectedPotionName.AddThemeFontSizeOverride("font_size", 20);
            _selectedPotionName.Position = new Vector2(10, 10);
            infoPanel.AddChild(_selectedPotionName);
            
            _selectedPotionDesc = new Label();
            _selectedPotionDesc.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _selectedPotionDesc.Position = new Vector2(10, 45);
            _selectedPotionDesc.AutowrapMode = TextServer.AutowrapWord;
            infoPanel.AddChild(_selectedPotionDesc);
            
            _selectedPotionEffect = new Label();
            _selectedPotionEffect.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _selectedPotionEffect.Position = new Vector2(10, 100);
            _selectedPotionEffect.AutowrapMode = TextServer.AutowrapWord;
            infoPanel.AddChild(_selectedPotionEffect);
            
            // 使用按钮
            _useButton = new Button();
            _useButton.Text = "使用药水";
            _useButton.Position = new Vector2(290, 310);
            _useButton.Size = new Vector2(120, 40);
            _useButton.Pressed += OnUseButtonPressed;
            _container.AddChild(_useButton);
            
            // 自动使用复选框
            _autoUseCheck = new CheckButton();
            _autoUseCheck.Text = "自动使用";
            _autoUseCheck.Position = new Vector2(420, 310);
            _autoUseCheck.Size = new Vector2(150, 40);
            _autoUseCheck.Toggled += OnAutoUseToggled;
            _container.AddChild(_autoUseCheck);
            
            // 激活效果显示
            var activeLabel = new Label();
            activeLabel.Text = "激活效果:";
            activeLabel.Position = new Vector2(20, 420);
            activeLabel.AddThemeFontSizeOverride("font_size", 16);
            _container.AddChild(activeLabel);
            
            var activeScroll = new ScrollContainer();
            activeScroll.Position = new Vector2(20, 445);
            activeScroll.Size = new Vector2(550, 45);
            _container.AddChild(activeScroll);
            
            _infoLabel = new Label();
            _infoLabel.AutowrapMode = TextServer.AutowrapWord;
            activeScroll.AddChild(_infoLabel);
            
            RefreshPotionList();
        }

        private void CreateFilterButton(string text, string filter)
        {
            var btn = new Button();
            btn.Text = text;
            btn.Size = new Vector2(60, 30);
            btn.Pressed += () => OnFilterChanged(filter);
            _filterButtons.AddChild(btn);
        }

        private void OnFilterChanged(string filter)
        {
            _currentFilter = filter;
            RefreshPotionList();
        }

        private void RefreshPotionList()
        {
            // 清除现有列表
            foreach (var child in _potionList.GetChildren())
            {
                child.QueueFree();
            }

            List<Items.PotionInstance> filteredPotions = new List<Items.PotionInstance>();
            
            // 根据筛选类型过滤
            foreach (var potionInstance in Items.PotionManager.Instance.OwnedPotions)
            {
                if (potionInstance.Quantity <= 0) continue;
                
                var potion = Items.PotionDatabase.Instance.GetPotion(potionInstance.PotionId);
                if (potion == null) continue;
                
                bool shouldAdd = _currentFilter switch
                {
                    "health" => potion.Type == Items.PotionType.Health,
                    "mana" => potion.Type == Items.PotionType.Mana || potion.Type == Items.PotionType.Stamina,
                    "buffs" => potion.Duration > 0,
                    _ => true
                };
                
                if (shouldAdd)
                    filteredPotions.Add(potionInstance);
            }

            // 显示药水
            foreach (var potionInstance in filteredPotions)
            {
                var potion = Items.PotionDatabase.Instance.GetPotion(potionInstance.PotionId);
                if (potion == null) continue;

                var itemContainer = new HBoxContainer();
                itemContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                
                // 药水图标（用颜色方块代替）
                var icon = new ColorRect();
                icon.Size = new Vector2(30, 30);
                
                Color iconColor = potion.Rarity switch
                {
                    Items.PotionRarity.Common => _commonColor,
                    Items.PotionRarity.Uncommon => _uncommonColor,
                    Items.PotionRarity.Rare => _rareColor,
                    Items.PotionRarity.Epic => _epicColor,
                    Items.PotionRarity.Legendary => _legendaryColor,
                    _ => Colors.White
                };
                icon.Color = iconColor;
                itemContainer.AddChild(icon);
                
                // 药水名称和数量
                var nameLabel = new Label();
                nameLabel.Text = $"{potion.Name} x{potionInstance.Quantity}";
                nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                itemContainer.AddChild(nameLabel);
                
                // 选中事件
                itemContainer.GuiInput += (evt) =>
                {
                    if (evt is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
                    {
                        SelectPotion(potion);
                    }
                };
                
                _potionList.AddChild(itemContainer);
            }

            if (filteredPotions.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "背包中没有药水";
                emptyLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
                _potionList.AddChild(emptyLabel);
            }
        }

        private void SelectPotion(Items.Potion potion)
        {
            _selectedPotion = potion;
            
            var rarityColor = potion.GetRarityColor();
            _selectedPotionName.Text = potion.Name;
            _selectedPotionName.Modulate = rarityColor;
            
            _selectedPotionDesc.Text = potion.Description;
            
            // 显示效果
            string effects = "";
            if (potion.HealthRestore > 0) effects += $"恢复生命: {potion.HealthRestore}\n";
            if (potion.ManaRestore > 0) effects += $"恢复法力: {potion.ManaRestore}\n";
            if (potion.HealthRegen > 0) effects += $"生命再生: {potion.HealthRegen}/秒\n";
            if (potion.ManaRegen > 0) effects += $"法力再生: {potion.ManaRegen}/秒\n";
            if (potion.DamageBoost > 0) effects += $"攻击加成: +{potion.DamageBoost * 100}%\n";
            if (potion.DefenseBoost > 0) effects += $"防御加成: +{potion.DefenseBoost * 100}%\n";
            if (potion.SpeedBoost > 0) effects += $"速度加成: +{potion.SpeedBoost * 100}%\n";
            if (potion.CriticalBoost > 0) effects += $"暴击加成: +{potion.CriticalBoost * 100}%\n";
            if (potion.Duration > 0) effects += $"持续时间: {potion.Duration}秒\n";
            if (potion.Cooldown > 0) effects += $"冷却时间: {potion.Cooldown}秒";
            
            _selectedPotionEffect.Text = effects;
            
            // 检查是否有药水可用
            _useButton.Disabled = !Items.PotionManager.Instance.HasPotion(potion.Id);
            
            // 更新自动使用状态
            _autoUseCheck.ButtonPressed = Items.PotionManager.Instance.GetPotionQuantity(potion.Id) > 0 && 
                Items.PotionManager.Instance.OwnedPotions.Exists(p => p.PotionId == potion.Id && p.IsAutoUse);
        }

        private void OnUseButtonPressed()
        {
            if (_selectedPotion == null || _player == null) return;
            
            if (Items.PotionManager.Instance.UsePotion(_selectedPotion.Id, _player))
            {
                RefreshPotionList();
                SelectPotion(_selectedPotion);
            }
        }

        private void OnAutoUseToggled(bool toggled)
        {
            if (_selectedPotion == null) return;
            Items.PotionManager.Instance.SetAutoUse(_selectedPotion.Id, toggled);
        }

        private void OnPotionUpdated(Items.PotionInstance potion)
        {
            RefreshPotionList();
            UpdateActiveEffects();
        }

        private void OnPotionUsed(Items.Potion potion)
        {
            RefreshPotionList();
            UpdateActiveEffects();
        }

        private void UpdateActiveEffects()
        {
            var activeBuffs = Items.PotionManager.Instance.GetActiveBuffs();
            if (activeBuffs.Count == 0)
            {
                _infoLabel.Text = "无激活效果";
                return;
            }
            
            string text = "";
            foreach (var buff in activeBuffs)
            {
                float remaining = Items.PotionManager.Instance.GetBuffRemainingTime(buff.Id);
                text += $"{buff.Name}: {remaining:F1}秒 | ";
            }
            _infoLabel.Text = text;
        }

        public void ToggleUI()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshPotionList();
                UpdateActiveEffects();
                
                // 获取玩家节点
                if (_player == null)
                {
                    _player = GetTree().GetFirstNodeInGroup("player");
                }
            }
        }

        public override void _Process(double delta)
        {
            if (Visible)
            {
                UpdateActiveEffects();
            }
        }
    }
}
