using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems;

namespace ClawRPG.UI {
    /// <summary>
    /// 按键绑定界面 - 允许玩家自定义快捷键
    /// </summary>
    public class KeybindingUI : Control
    {
        private VBoxContainer _mainContainer;
        private ScrollContainer _scrollContainer;
        private VBoxContainer _keybindingList;
        private Label _titleLabel;
        private Button _resetButton;
        private Button _closeButton;
        private Label _instructionsLabel;
        
        // 按键绑定项
        private Dictionary<string, KeybindingItem> _items = new Dictionary<string, KeybindingItem>();
        
        // 当前正在绑定的项
        private KeybindingItem _bindingItem = null;
        private Label _bindingPrompt;
        
        // 分类
        private OptionButton _categoryOption;
        private string _currentCategory = "all";
        
        private Dictionary<string, List<string>> _categories = new Dictionary<string, List<string>>
        {
            { "all", new List<string>() },
            { "movement", new List<string> { "move_up", "move_down", "move_left", "move_right" } },
            { "combat", new List<string> { "attack", "block", "dodge", "skill_1", "skill_2", "skill_3", "skill_4", "skill_5", "skill_6" } },
            { "inventory", new List<string> { "inventory", "equipment", "equipment_set", "quickslot_1", "quickslot_2", "quickslot_3", "quickslot_4", "quickslot_5", "quickslot_6", "quickslot_7", "quickslot_8", "auto_potion" } },
            { "crafting", new List<string> { "crafting", "enhancement", "enchant", "runes" } },
            { "system", new List<string> { "skills", "quests", "achievements", "titles", "statistics", "pets", "mounts", "region_map", "world_events", "bounty", "daily_challenge", "bookmarks", "auto_bookmark", "hotkey_help", "pause", "multiplayer", "settings" } },
            { "interaction", new List<string> { "interact", "quest_tracker", "quest_guide", "player_profile", "story", "shop", "weather" } }
        };

        public override void _Ready()
        {
            Visible = false; 
            SetupUI();
            ConnectSignals();
            PopulateKeybindings();
        }

        private void SetupUI()
        {
            // 背景面板
            var bgPanel = new Panel
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Color = new Color(0, 0, 0, 0.7f)
            };
            AddChild(bgPanel);

            // 主容器
            _mainContainer = new VBoxContainer
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                OffsetLeft = 100,
                OffsetTop = 50,
                OffsetRight = -100,
                OffsetBottom = -50
            };
            AddChild(_mainContainer);

            // 标题
            _titleLabel = new Label
            {
                Text = "🔧 按键绑定设置",
                Align = Label.AlignEnum.Center,
                FontSize = 28
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _mainContainer.AddChild(_titleLabel);

            // 说明文字
            _instructionsLabel = new Label
            {
                Text = "点击右侧按钮修改按键 | 点击[重置]恢复默认 | Esc关闭",
                Align = Label.AlignEnum.Center,
                Modulate = new Color(0.7f, 0.7f, 0.7f)
            };
            _mainContainer.AddChild(_instructionsLabel);

            // 分类选择
            var categoryContainer = new HBoxContainer
            {
                Alignment = BoxContainer.AlignMode.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            _mainContainer.AddChild(categoryContainer);

            var categoryLabel = new Label { Text = "分类: " };
            categoryContainer.AddChild(categoryLabel);

            _categoryOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(150, 30)
            };
            _categoryOption.AddItem("全部", 0);
            _categoryOption.AddItem("移动", 1);
            _categoryOption.AddItem("战斗", 2);
            _categoryOption.AddItem("背包", 3);
            _categoryOption.AddItem("合成", 4);
            _categoryOption.AddItem("系统", 5);
            _categoryOption.AddItem("交互", 6);
            _categoryOption.Selected = 0;
            _categoryOption.ItemSelected += OnCategorySelected;
            categoryContainer.AddChild(_categoryOption);

            // 重置按钮
            _resetButton = new Button
            {
                Text = "重置全部",
                CustomMinimumSize = new Vector2(120, 35)
            };
            _resetButton.Pressed += OnResetPressed;
            categoryContainer.AddChild(_resetButton);

            // 滚动容器
            _scrollContainer = new ScrollContainer
            {
                VerticalScrollFeedback = true,
                CustomMinimumSize = new Vector2(0, 400)
            };
            _mainContainer.AddChild(_scrollContainer);

            // 按键绑定列表
            _keybindingList = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(600, 0)
            };
            _scrollContainer.AddChild(_keybindingList);

            // 绑定提示
            _bindingPrompt = new Label
            {
                Text = "请按下新按键...",
                Align = Label.AlignEnum.Center,
                FontSize = 24,
                Visible = false,
                Modulate = new Color(1f, 0.8f, 0.2f)
            };
            _bindingPrompt.AddThemeFontSizeOverride("font_size", 24);
            _bindingPrompt.SetAnchorsPreset(Control.LayoutPreset.Center);
            _bindingPrompt.Position = new Vector2(300, 200);
            AddChild(_bindingPrompt);

            // 关闭按钮
            _closeButton = new Button
            {
                Text = "关闭 (Esc)",
                CustomMinimumSize = new Vector2(200, 40)
            };
            _closeButton.Pressed += OnClosePressed;
            _mainContainer.AddChild(_closeButton);

            // 初始化分类
            foreach (var kvp in _categories)
            {
                if (kvp.Key != "all")
                {
                    _categories["all"].AddRange(kvp.Value);
                }
            }
        }

        private void ConnectSignals()
        {
            if (KeybindingSystem.Instance != null)
            {
                KeybindingSystem.Instance.Connect(nameof(KeybindingSystem.KeybindingChanged), this, nameof(OnKeybindingChanged));
                KeybindingSystem.Instance.Connect(nameof(KeybindingSystem.KeybindingsReset), this, nameof(OnKeybindingsReset));
            }
        }

        private void PopulateKeybindings()
        {
            // 清除现有项
            foreach (var child in _keybindingList.GetChildren())
            {
                child.QueueFree();
            }
            _items.Clear();

            var bindings = KeybindingSystem.Instance.GetAllKeybindings();
            var categoryActions = _categories[_currentCategory];

            foreach (var kvp in bindings)
            {
                if (_currentCategory != "all" && !categoryActions.Contains(kvp.Key))
                    continue;

                var item = new KeybindingItem(kvp.Value);
                _keybindingList.AddChild(item);
                _items[kvp.Key] = item;

                item.OnBindRequested += () => StartBinding(kvp.Key);
                item.OnResetRequested += () => ResetKeybinding(kvp.Key);
            }
        }

        private void StartBinding(string actionName)
        {
            if (_bindingItem != null) return;
            
            if (_items.TryGetValue(actionName, out var item))
            {
                _bindingItem = item;
                _bindingItem.SetBinding(true);
                _bindingPrompt.Visible = true;
                
                // 设置输入捕获
                GetTree().SetInputAsHandled();
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (_bindingItem == null) return;

            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                var newKey = keyEvent.Keycode;
                
                // 忽略Escape键
                if (newKey == Key.Escape)
                {
                    CancelBinding();
                    return;
                }

                // 应用新按键
                if (KeybindingSystem.Instance.ChangeKeybinding(_bindingItem.ActionName, newKey))
                {
                    _bindingItem.SetBinding(false);
                    _bindingItem.UpdateKey(newKey);
                    _bindingPrompt.Visible = false; 
                    _bindingItem = null;
                }
                
                GetTree().SetInputAsHandled();
            }
        }

        private void CancelBinding()
        {
            if (_bindingItem != null)
            {
                _bindingItem.SetBinding(false);
                _bindingItem = null;
                _bindingPrompt.Visible = false; 
            }
        }

        private void ResetKeybinding(string actionName)
        {
            KeybindingSystem.Instance.ResetKeybinding(actionName);
        }

        private void OnCategorySelected(int index)
        {
            string[] categories = { "all", "movement", "combat", "inventory", "crafting", "system", "interaction" };
            _currentCategory = categories[index];
            PopulateKeybindings();
        }

        private void OnResetPressed()
        {
            var confirmDialog = new AcceptDialog
            {
                WindowTitle = "确认重置",
                DialogText = "确定要重置所有按键绑定为默认值吗？"
            };
            confirmDialog.Ok += () => KeybindingSystem.Instance.ResetAllKeybindings();
            AddChild(confirmDialog);
            confirmDialog.PopupCentered();
        }

        private void OnClosePressed()
        {
            HideKeybindingUI();
        }

        private void OnKeybindingChanged(string actionName, Key oldKey, Key newKey)
        {
            if (_items.TryGetValue(actionName, out var item))
            {
                item.UpdateKey(newKey);
            }
        }

        private void OnKeybindingsReset()
        {
            PopulateKeybindings();
        }

        public void ShowKeybindingUI()
        {
            Visible = true;
            PopulateKeybindings();
        }

        public void HideKeybindingUI()
        {
            Visible = false; 
            CancelBinding();
        }

        public void ToggleKeybindingUI()
        {
            if (Visible)
                HideKeybindingUI();
            else
                ShowKeybindingUI();
        }
    }

    /// <summary>
    /// 单个按键绑定项
    /// </summary>
    public class KeybindingItem : HBoxContainer
    {
        public string ActionName { get; }
        
        private Label _nameLabel;
        private Label _keyLabel;
        private Button _bindButton;
        private Button _resetButton;
        
        public event Action OnBindRequested;
        public event Action OnResetRequested;

        public KeybindingItem(KeybindingAction binding)
        {
            ActionName = binding.ActionName;
            SetupUI(binding);
        }

        private void SetupUI(KeybindingAction binding)
        {
            CustomMinimumSize = new Vector2(600, 50);
            Alignment = BoxContainer.AlignMode.Center;

            // 操作名称
            _nameLabel = new Label
            {
                Text = binding.Description,
                CustomMinimumSize = new Vector2(200, 0),
                Modulate = new Color(0.9f, 0.9f, 0.9f)
            };
            AddChild(_nameLabel);

            // 添加间隔
            AddChild(new Control { CustomMinimumSize = new Vector2(50, 0) });

            // 当前按键显示
            _keyLabel = new Label
            {
                Text = binding.KeyName,
                CustomMinimumSize = new Vector2(120, 0),
                Align = Label.AlignEnum.Center,
                Modulate = binding.IsModified ? new Color(1f, 0.7f, 0.3f) : new Color(0.5f, 0.9f, 0.5f)
            };
            AddChild(_keyLabel);

            // 修改按钮
            _bindButton = new Button
            {
                Text = "修改",
                CustomMinimumSize = new Vector2(80, 30)
            };
            _bindButton.Pressed += () => OnBindRequested?.Invoke();
            AddChild(_bindButton);

            // 重置按钮
            _resetButton = new Button
            {
                Text = "重置",
                CustomMinimumSize = new Vector2(70, 30),
                Disabled = !binding.IsModified
            };
            _resetButton.Pressed += () => OnResetRequested?.Invoke();
            AddChild(_resetButton);

            // 背景样式
            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0.15f, 0.15f, 0.2f, 0.8f),
                CornerRadiusTopLeft = 5,
                CornerRadiusTopRight = 5,
                CornerRadiusBottomLeft = 5,
                CornerRadiusBottomRight = 5,
                BorderWidthBottom = 1,
                BorderWidthTop = 1,
                BorderWidthLeft = 1,
                BorderWidthRight = 1,
                BorderColor = new Color(0.3f, 0.3f, 0.4f)
            };
            AddThemeStyleboxOverride("panel", styleBox);
        }

        public void UpdateKey(Key key)
        {
            _keyLabel.Text = KeybindingSystem.GetKeyName(key);
            _keyLabel.Modulate = new Color(1f, 0.7f, 0.3f);
        }

        public void SetBinding(bool binding)
        {
            if (binding)
            {
                _bindButton.Text = "等待...";
                _bindButton.Disabled = true;
                _keyLabel.Modulate = new Color(1f, 0.8f, 0.2f);
            }
            else
            {
                _bindButton.Text = "修改";
                _bindButton.Disabled = false; 
            }
        }
    }
}
