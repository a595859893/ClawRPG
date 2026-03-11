namespace ClawRPG.Scripts.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ClawRPG.Scripts.Data;
    using ClawRPG.Scripts.Database;
    using ClawRPG.Scripts.Systems;

    /// <summary>
    /// 符文界面
    /// </summary>
    public partial class RuneUI : Control
    {
        private RuneSystem _runeSystem;
        private bool _isVisible = false;

        // UI 组件
        private Label _titleLabel;
        private HBoxContainer _runeSlotsContainer;
        private VBoxContainer _inventoryContainer;
        private VBoxContainer _statsContainer;
        private Label _setBonusLabel;
        private Button _closeButton;
        private Button _addRuneButton;

        // 符文槽位
        private RuneSlot[] _runeSlots = new RuneSlot[6];

        public override void _Ready()
        {
            _runeSystem = RuneSystem.Instance;
            if (_runeSystem == null)
            {
                _runeSystem = new RuneSystem();
                _runeSystem.Initialize();
            }

            SetupUI();
            Visible = false;
        }

        private void SetupUI()
        {
            // 主容器
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorPreset(ControlPreset.CenterAll);
            mainContainer.CustomMinimumSize = new Vector2(800, 600);
            mainContainer.Modulate = new Color(1, 1, 1, 0.95f);
            AddChild(mainContainer);

            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "符文系统";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainContainer.AddChild(_titleLabel);

            // 内容区域
            var contentContainer = new HBoxContainer();
            contentContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            contentContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            mainContainer.AddChild(contentContainer);

            // 符文槽位区域
            var slotsPanel = new VBoxContainer;
            slotsPanel.CustomMinimumSize = new Vector2(300, 0);
            contentContainer.AddChild(slotsPanel);

            var slotsLabel = new Label();
            slotsLabel.Text = "已装备符文";
            slotsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            slotsPanel.AddChild(slotsLabel);

            _runeSlotsContainer = new HBoxContainer();
            _runeSlotsContainer.Alignment = BoxContainer.Alignment.Center;
            slotsPanel.AddChild(_runeSlotsContainer);

            // 创建6个符文槽位
            for (int i = 0; i < 6; i++)
            {
                var slot = CreateRuneSlot(i);
                _runeSlots[i] = slot;
                _runeSlotsContainer.AddChild(slot.Container);
            }

            // 背包区域
            var inventoryPanel = new VBoxContainer();
            inventoryPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            contentContainer.AddChild(inventoryPanel);

            var inventoryLabel = new Label();
            inventoryLabel.Text = "符文背包";
            inventoryLabel.HorizontalAlignment = HorizontalAlignment.Center;
            inventoryPanel.AddChild(inventoryLabel);

            var scrollContainer = new ScrollContainer;
            scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            inventoryPanel.AddChild(scrollContainer);

            _inventoryContainer = new VBoxContainer();
            scrollContainer.AddChild(_inventoryContainer);

            // 按钮区域
            var buttonContainer = new HBoxContainer();
            buttonContainer.Alignment = BoxContainer.Alignment.Center;
            mainContainer.AddChild(buttonContainer);

            _addRuneButton = new Button();
            _addRuneButton.Text = "添加随机符文";
            _addRuneButton.Pressed += OnAddRunePressed;
            buttonContainer.AddChild(_addRuneButton);

            _closeButton = new Button();
            _closeButton.Text = "关闭";
            _closeButton.Pressed += OnClosePressed;
            buttonContainer.AddChild(_closeButton);

            // 统计和套装加成区域
            var statsPanel = new VBoxContainer();
            statsPanel.CustomMinimumSize = new Vector2(200, 0);
            contentContainer.AddChild(statsPanel);

            var statsLabel = new Label();
            statsLabel.Text = "属性加成";
            statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            statsPanel.AddChild(statsLabel);

            _statsContainer = new VBoxContainer();
            statsPanel.AddChild(_statsContainer);

            var setLabel = new Label();
            setLabel.Text = "套装效果";
            setLabel.HorizontalAlignment = HorizontalAlignment.Center;
            statsPanel.AddChild(setLabel);

            _setBonusLabel = new Label();
            _setBonusLabel.Text = "无";
            statsPanel.AddChild(_setBonusLabel);
        }

        private RuneSlot CreateRuneSlot(int index)
        {
            var slot = new RuneSlot();

            slot.Container = new PanelContainer;
            slot.Container.CustomMinimumSize = new Vector2(60, 60);
            slot.Container.Modulate = new Color(0.3f, 0.3f, 0.3f);

            slot.Icon = new TextureRect;
            slot.Icon.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            slot.Icon.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
            slot.Container.AddChild(slot.Icon);

            slot.IndexLabel = new Label;
            slot.IndexLabel.Text = (index + 1).ToString();
            slot.IndexLabel.HorizontalAlignment = HorizontalAlignment.Center;
            slot.IndexLabel.VerticalAlignment = VerticalAlignment.Center;
            slot.Container.AddChild(slot.IndexLabel);

            slot.Index = index;
            slot.Container.GuiInput += (InputEvent) => OnSlotInput(slot, InputEvent);

            return slot;
        }

        private void OnSlotInput(RuneSlot slot, InputEvent evt)
        {
            if (evt is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
            {
                var equipped = _runeSystem.GetEquippedRunes();
                var slotRune = equipped.FirstOrDefault(r =>
                {
                    var instance = _runeSystem.GetAllOwnedRunes().FirstOrDefault(x => x.Id == r.Id);
                    return instance != null && _runeSystem.GetRuneInstance(instance.Id)?.SlotIndex == slot.Index;
                });

                if (slotRune != null)
                {
                    // 卸下符文
                    var instance = _runeSystem.GetAllOwnedRunes().FirstOrDefault(x => x.Id == slotRune.Id);
                    if (instance != null)
                    {
                        var inst = _runeSystem.GetRuneInstance(instance.Id);
                        if (inst != null)
                        {
                            _runeSystem.UnequipRune(inst.UniqueId);
                        }
                    }
                }

                UpdateUI();
            }
        }

        public override void _Process(double delta)
        {
            // 实时更新
        }

        private void UpdateUI()
        {
            // 清空背包
            foreach (var child in _inventoryContainer.GetChildren())
            {
                child.QueueFree();
            }

            // 显示符文背包
            var runes = _runeSystem.GetAllOwnedRunes();
            foreach (var rune in runes)
            {
                var item = CreateRuneItem(rune);
                _inventoryContainer.AddChild(item);
            }

            // 更新槽位显示
            var equipped = _runeSystem.GetEquippedRunes();
            foreach (var slot in _runeSlots)
            {
                var runeInSlot = equipped.FirstOrDefault(r =>
                {
                    var instance = _runeSystem.GetAllOwnedRunes().FirstOrDefault(x => x.Id == r.Id);
                    return instance != null && _runeSystem.GetRuneInstance(instance.Id)?.SlotIndex == slot.Index;
                });

                if (runeInSlot != null)
                {
                    slot.Container.Modulate = RuneDatabase.RarityColors[runeInSlot.Rarity];
                    slot.IndexLabel.Text = runeInSlot.Name.Substring(0, 1);
                }
                else
                {
                    slot.Container.Modulate = new Color(0.3f, 0.3f, 0.3f);
                    slot.IndexLabel.Text = (slot.Index + 1).ToString();
                }
            }

            // 更新属性加成
            UpdateStats();

            // 更新套装加成
            var (setName, level, bonus) = _runeSystem.GetActiveSetBonus();
            if (level > 0)
            {
                _setBonusLabel.Text = $"{setName}\n{level}件效果\n+{bonus}%全属性";
            }
            else
            {
                _setBonusLabel.Text = "无";
            }
        }

        private Control CreateRuneItem(Rune rune)
        {
            var container = new PanelContainer;
            container.CustomMinimumSize = new Vector2(0, 50);

            var hbox = new HBoxContainer;
            container.AddChild(hbox);

            var colorBox = new ColorRect;
            colorBox.CustomMinimumSize = new Vector2(40, 40);
            colorBox.Color = RuneDatabase.RarityColors[rune.Rarity];
            hbox.AddChild(colorBox);

            var info = new VBoxContainer;
            info.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hbox.AddChild(info);

            var nameLabel = new Label;
            nameLabel.Text = $"{rune.Name} Lv.{rune.Level}";
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            info.AddChild(nameLabel);

            var typeLabel = new Label;
            typeLabel.Text = $"{RuneDatabase.GetRuneTypeName(rune.Type)} +{rune.AttributeValue}";
            typeLabel.AddThemeFontSizeOverride("font_size", 12);
            info.AddChild(typeLabel);

            var equipButton = new Button;
            equipButton.Text = rune.IsEquipped ? "卸下" : "装备";
            equipButton.Pressed += () =>
            {
                var instance = _runeSystem.GetAllOwnedRunes().FirstOrDefault(x => x.Id == rune.Id);
                if (instance != null)
                {
                    var inst = _runeSystem.GetRuneInstance(instance.Id);
                    if (inst != null)
                    {
                        if (rune.IsEquipped)
                        {
                            _runeSystem.UnequipRune(inst.UniqueId);
                        }
                        else
                        {
                            // 找到第一个空槽位
                            var equipped = _runeSystem.GetEquippedRunes();
                            int slot = -1;
                            for (int i = 0; i < 6; i++)
                            {
                                if (!equipped.Any(e =>
                                {
                                    var inst2 = _runeSystem.GetAllOwnedRunes().FirstOrDefault(x => x.Id == e.Id);
                                    return inst2 != null && _runeSystem.GetRuneInstance(inst2.Id)?.SlotIndex == i;
                                }))
                                {
                                    slot = i;
                                    break;
                                }
                            }
                            if (slot >= 0)
                            {
                                _runeSystem.EquipRune(inst.UniqueId, slot);
                            }
                        }
                    }
                }
                UpdateUI();
            };
            hbox.AddChild(equipButton);

            var deleteButton = new Button;
            deleteButton.Text = "删除";
            deleteButton.Pressed += () =>
            {
                var instance = _runeSystem.GetAllOwnedRunes().FirstOrDefault(x => x.Id == rune.Id);
                if (instance != null)
                {
                    var inst = _runeSystem.GetRuneInstance(instance.Id);
                    if (inst != null)
                    {
                        _runeSystem.RemoveRune(inst.UniqueId);
                    }
                }
                UpdateUI();
            };
            hbox.AddChild(deleteButton);

            return container;
        }

        private void UpdateStats()
        {
            foreach (var child in _statsContainer.GetChildren())
            {
                child.QueueFree();
            }

            var bonuses = _runeSystem.GetAttributeBonuses();

            AddStatLine("攻击", bonuses[RuneType.Attack]);
            AddStatLine("防御", bonuses[RuneType.Defense]);
            AddStatLine("生命", bonuses[RuneType.Health]);
            AddStatLine("速度", bonuses[RuneType.Speed]);
            AddStatLine("暴击", bonuses[RuneType.Critical]);
            AddStatLine("魔法", bonuses[RuneType.Magic]);
            AddStatLine("生命偷取", bonuses[RuneType.LifeSteal]);
            AddStatLine("闪避", bonuses[RuneType.Dodge]);
        }

        private void AddStatLine(string name, float value)
        {
            var label = new Label;
            label.Text = $"{name}: +{value:F1}";
            label.AddThemeFontSizeOverride("font_size", 12);
            _statsContainer.AddChild(label);
        }

        private void OnAddRunePressed()
        {
            _runeSystem.AddRandomRune();
            UpdateUI();
        }

        private void OnClosePressed()
        {
            ToggleUI();
        }

        public void ToggleUI()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;

            if (_isVisible)
            {
                UpdateUI();
            }
        }

        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey key && key.Pressed)
            {
                // R 键切换显示
                if (key.Keycode == Key.R)
                {
                    ToggleUI();
                }
                // ESC 关闭
                else if (key.Keycode == Key.Escape && _isVisible)
                {
                    ToggleUI();
                }
            }
        }

        private class RuneSlot
        {
            public PanelContainer Container;
            public TextureRect Icon;
            public Label IndexLabel;
            public int Index;
        }
    }
}
