using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 药水UI界面
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
        private Button _btnAll;
        private Button _btnHealth;
        private Button _btnMana;
        private Button _btnBuffs;
        
        // 当前筛选类型
        private string _currentFilter = "all";
        
        // 当前选中的药水
        private Potion _selectedPotion;
        private Label _selectedPotionName;
        private Label _selectedPotionDesc;
        private Label _selectedPotionEffect;
        private Button _useButton;
        private CheckButton _autoUseCheck;
        
        // 玩家引用
        private Node _player;
        
        // 资源
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
            PotionManager.Instance.OnPotionAdded += OnPotionUpdated;
            PotionManager.Instance.OnPotionRemoved += OnPotionUpdated;
            PotionManager.Instance.OnPotionUsed += OnPotionUsed;
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
            
            _btnAll = CreateFilterButton("全部", "all");
            _btnHealth = CreateFilterButton("生命", "health");
            _btnMana = CreateFilterButton("法力", "mana");
            _btnBuffs = CreateFilterButton("增益", "buffs");
            
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

        private Button CreateFilterButton(string text, string filter)
        {
            var btn = new Button();
            btn.Text = text;
            btn.Size = new Vector2(60, 30);
            btn.Pressed += () => OnFilterChanged(filter);
            _filterButtons.AddChild(btn);
            return btn;
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

            List<PotionInstance> filteredPotions = new List<PotionInstance>();
            
            // 根据筛选类型过滤
            foreach (var potionInstance in PotionManager.Instance.OwnedPotions)
            {
                if (potionInstance.Quantity <= 0) continue;
                
                var potion = PotionDatabase.Instance.GetPotion(potionInstance.PotionId);
                if (potion == null) continue;
                
                bool shouldAdd = _currentFilter switch
                {
                    "health" => potion.Type == PotionType.Health,
                    "mana" => potion.Type == PotionType.Mana || potion.Type == PotionType.Stamina,
                    "buffs" => potion.Duration > 0,
                    _ => true
                };
                
                if (shouldAdd)
                    filteredPotions.Add(potionInstance);
            }

            // 显示药水
            foreach (var potionInstance in filteredPotions)
            {
                var potion = PotionDatabase.Instance.GetPotion(potionInstance.PotionId);
                if (potion == null) continue;

                var itemContainer = new HBoxContainer();
                itemContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                
                // 药水图标（用颜色方块代替）
                var icon = new ColorRect();
                icon.Size = new Vector2(30, 30);
                
                Color iconColor = potion.Rarity switch
                {
                    PotionRarity.Common => _commonColor,
                    PotionRarity.Uncommon => _uncommonColor,
                    PotionRarity.Rare => _rareColor,
                    PotionRarity.Epic => _epicColor,
                    PotionRarity.Legendary => _legendaryColor,
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

        private void SelectPotion(Potion potion)
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
            _useButton.Disabled = !PotionManager.Instance.HasPotion(potion.Id);
            
            // 更新自动使用状态
            _autoUseCheck.ButtonPressed = PotionManager.Instance.GetPotionQuantity(potion.Id) > 0 && 
                PotionManager.Instance.OwnedPotions.Exists(p => p.PotionId == potion.Id && p.IsAutoUse);
        }

        private void OnUseButtonPressed()
        {
            if (_selectedPotion == null || _player == null) return;
            
            if (PotionManager.Instance.UsePotion(_selectedPotion.Id, _player))
            {
                RefreshPotionList();
                SelectPotion(_selectedPotion); // 刷新状态
            }
        }

        private void OnAutoUseToggled(bool toggled)
        {
            if (_selectedPotion == null) return;
            PotionManager.Instance.SetAutoUse(_selectedPotion.Id, toggled);
        }

        private void OnPotionUpdated(PotionInstance potion)
        {
            RefreshPotionList();
            UpdateActiveEffects();
        }

        private void OnPotionUsed(Potion potion)
        {
            RefreshPotionList();
            UpdateActiveEffects();
        }

        private void UpdateActiveEffects()
        {
            var activeBuffs = PotionManager.Instance.GetActiveBuffs();
            if (activeBuffs.Count == 0)
            {
                _infoLabel.Text = "无激活效果";
                return;
            }
            
            string text = "";
            foreach (var buff in activeBuffs)
            {
                float remaining = PotionManager.Instance.GetBuffRemainingTime(buff.Id);
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
                    _player = GetTree().GetFirstNodeInGroup("Player");
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

        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey key && key.Pressed)
            {
                // 按P键打开药水UI
                if (key.Keycode == Key.P)
                {
                    ToggleUI();
                }
            }
        }
    }
}
