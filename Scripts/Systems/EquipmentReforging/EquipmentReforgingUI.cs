using Godot;
using System;
using System.Collections.Generic;
using Game.Scripts.Systems.EquipmentReforging;

namespace Game.Scripts.UI
{
    /// <summary>
    /// 装备洗练界面
    /// </summary>
    public class EquipmentReforgingUI : Control
    {
        private Control _panel;
        private VBoxContainer _mainContainer;
        private Label _titleLabel;
        private Label _goldLabel;
        
        // 装备选择
        private Label _equipmentLabel;
        private OptionButton _equipmentOption;
        
        // 洗练类型选择
        private Label _typeLabel;
        private OptionButton _typeOption;
        
        // 稀有度选择(高级洗练)
        private Label _rarityLabel;
        private OptionButton _rarityOption;
        
        // 配方信息
        private Label _recipeLabel;
        private RichTextLabel _recipeInfo;
        
        // 统计
        private Label _statsLabel;
        private RichTextLabel _statsInfo;
        
        // 按钮
        private Button _reforgeButton;
        private Button _closeButton;
        
        // 当前选中
        private string _selectedEquipmentId = "";
        private ReforgeType _selectedType = ReforgeType.Basic;
        private ReforgeRarity _selectedRarity = ReforgeRarity.Common;

        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
            RefreshEquipmentList();
            UpdateRecipeInfo();
            UpdateStats();
            Hide();
        }

        private void SetupUI()
        {
            // 主面板
            _panel = new PanelContainer
            {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 400,
                OffsetTop = 150,
                OffsetRight = -400,
                OffsetBottom = -150
            };
            AddChild(_panel);

            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderColor = new Color(0.8f, 0.6f, 0.2f, 1f);
            styleBox.CornerRadiusTopLeft = 8;
            styleBox.CornerRadiusTopRight = 8;
            styleBox.CornerRadiusBottomLeft = 8;
            styleBox.CornerRadiusBottomRight = 8;
            _panel.AddThemeStyleboxOverride("panel", styleBox);

            _mainContainer = new VBoxContainer
            {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 20,
                OffsetTop = 20,
                OffsetRight = -20,
                OffsetBottom = -20
            };
            _panel.AddChild(_mainContainer);

            // 标题
            _titleLabel = new Label
            {
                Text = "⚒️ 装备洗练系统",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f, 1f));
            _mainContainer.AddChild(_titleLabel);

            // 金币显示
            _goldLabel = new Label
            {
                Text = "金币: 0",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _goldLabel.AddThemeFontSizeOverride("font_size", 18);
            _goldLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.3f, 1f));
            _mainContainer.AddChild(_goldLabel);

            AddSeparator();

            // 装备选择
            _equipmentLabel = new Label { Text = "选择装备:" };
            _equipmentLabel.AddThemeFontSizeOverride("font_size", 16);
            _mainContainer.AddChild(_equipmentLabel);

            _equipmentOption = new OptionButton;
            _equipmentOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _mainContainer.AddChild(_equipmentOption);

            // 洗练类型
            _typeLabel = new Label { Text = "洗练类型:" };
            _typeLabel.AddThemeFontSizeOverride("font_size", 16);
            _mainContainer.AddChild(_typeLabel);

            _typeOption = new OptionButton;
            _typeOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _mainContainer.AddChild(_typeOption);

            // 稀有度选择
            _rarityLabel = new Label { Text = "目标稀有度:" };
            _rarityLabel.AddThemeFontSizeOverride("font_size", 16);
            _mainContainer.AddChild(_rarityLabel);

            _rarityOption = new OptionButton;
            _rarityOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _mainContainer.AddChild(_rarityOption);

            AddSeparator();

            // 配方信息
            _recipeLabel = new Label { Text = "洗练配方:" };
            _recipeLabel.AddThemeFontSizeOverride("font_size", 16);
            _mainContainer.AddChild(_recipeLabel);

            _recipeInfo = new RichTextLabel
            {
                BbcodeEnabled = true,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                FitContent = true
            };
            _recipeInfo.AddThemeColorOverride("default_color", new Color(0.9f, 0.9f, 0.9f, 1f));
            _mainContainer.AddChild(_recipeInfo);

            AddSeparator();

            // 统计
            _statsLabel = new Label { Text = "洗练统计:" };
            _statsLabel.AddThemeFontSizeOverride("font_size", 16);
            _mainContainer.AddChild(_statsLabel);

            _statsInfo = new RichTextLabel
            {
                BbcodeEnabled = true,
                FitContent = true
            };
            _statsInfo.AddThemeColorOverride("default_color", new Color(0.7f, 0.9f, 0.7f, 1f));
            _mainContainer.AddChild(_statsInfo);

            AddSeparator();

            // 按钮容器
            var buttonContainer = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _mainContainer.AddChild(buttonContainer);

            _reforgeButton = new Button
            {
                Text = "开始洗练",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _reforgeButton.Pressed += OnReforgePressed;
            buttonContainer.AddChild(_reforgeButton);

            _closeButton = new Button
            {
                Text = "关闭",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _closeButton.Pressed += OnClosePressed;
            buttonContainer.AddChild(_closeButton);

            // 初始化选项
            InitializeOptions();
        }

        private void AddSeparator()
        {
            var separator = new HSeparator;
            separator.AddThemeConstantOverride("separation", 10);
            _mainContainer.AddChild(separator);
        }

        private void InitializeOptions()
        {
            // 洗练类型
            _typeOption.Clear();
            _typeOption.AddItem("基础洗练 (2属性)", (int)ReforgeType.Basic);
            _typeOption.AddItem("高级洗练 (3属性)", (int)ReforgeType.Advanced);
            _typeOption.AddItem("传奇洗练 (5属性)", (int)ReforgeType.Legendary);
            _typeOption.Selected = 0;

            // 稀有度
            _rarityOption.Clear();
            _rarityOption.AddItem("普通", (int)ReforgeRarity.Common);
            _rarityOption.AddItem("优秀", (int)ReforgeRarity.Uncommon);
            _rarityOption.AddItem("稀有", (int)ReforgeRarity.Rare);
            _rarityOption.AddItem("史诗", (int)ReforgeRarity.Epic);
            _rarityOption.AddItem("传说", (int)ReforgeRarity.Legendary);
            _rarityOption.Selected = 0;
        }

        private void ConnectSignals()
        {
            _equipmentOption.ItemSelected += OnEquipmentSelected;
            _typeOption.ItemSelected += OnTypeSelected;
            _rarityOption.ItemSelected += OnRaritySelected;

            if (EquipmentReforgingSystem.Instance != null)
            {
                EquipmentReforgingSystem.Instance.Connect(nameof(EquipmentReforgingSystem.ReforgeCompleted), 
                    Callable.From<string, bool, Dictionary<string, float>>(OnReforgeCompleted));
            }
        }

        private void RefreshEquipmentList()
        {
            _equipmentOption.Clear();
            _equipmentOption.AddItem("请选择装备", 0);
            
            // 这里应该从背包系统获取可洗练的装备
            // 暂时添加示例
            _equipmentOption.AddItem("示例装备 - 铁剑", 1);
            _equipmentOption.AddItem("示例装备 - 皮甲", 2);
            
            _equipmentOption.Selected = 0;
        }

        private void UpdateRecipeInfo()
        {
            var recipe = EquipmentReforgingDatabase.GetRecipe(_selectedType, _selectedRarity);
            
            string info = $"[color=yellow]类型:[/color] {recipe.Type}\n";
            info += $"[color=yellow]金币:[/color] {recipe.GoldCost}\n";
            info += $"[color=yellow]成功率:[/color] {recipe.SuccessRate * 100}%\n";
            info += $"[color=yellow]材料:[/color]\n";
            
            foreach (var material in recipe.MaterialCosts)
            {
                int playerCount = GetMaterialCount(material.Key);
                string color = playerCount >= material.Value ? "green" : "red";
                info += $"  - {material.Key}: {playerCount}/{material.Value} [color={color}]({GetMaterialName(material.Key)})[/color]\n";
            }
            
            _recipeInfo.Text = info;
            
            // 更新金币显示
            var player = GetTree().CurrentScene.GetNodeOrNull<Player>("Player");
            if (player != null)
            {
                _goldLabel.Text = $"金币: {player.Gold:N0}";
            }
        }

        private string GetMaterialName(string materialId)
        {
            return materialId switch
            {
                "reforge_stone" => "洗练石",
                "reforge_crystal" => "洗练水晶",
                "reforge_orb" => "洗练宝珠",
                _ => materialId
            };
        }

        private int GetMaterialCount(string materialId)
        {
            var inventoryManager = GetTree().CurrentScene.GetNodeOrNull<InventoryManager>("CanvasLayer/UI/InventoryManager");
            if (inventoryManager != null)
            {
                var items = inventoryManager.GetInventoryItems();
                foreach (var item in items)
                {
                    if (item.Id == materialId)
                    {
                        return item.Quantity;
                    }
                }
            }
            return 0;
        }

        private void UpdateStats()
        {
            if (EquipmentReforgingSystem.Instance == null) return;
            
            var stats = EquipmentReforgingSystem.Instance.GetStatistics();
            
            string info = $"[color=yellow]总洗练次数:[/color] {stats["total_reforges"]}\n";
            info += $"[color=yellow]成功:[/color] {stats["successful_reforges"]} | [color=red]失败:[/color] {stats["failed_reforges"]}\n";
            info += $"[color=yellow]成功率:[/color] {stats["success_rate"]:.1f}%\n";
            
            _statsInfo.Text = info;
        }

        private void OnEquipmentSelected(long index)
        {
            // 处理装备选择
            UpdateRecipeInfo();
        }

        private void OnTypeSelected(long index)
        {
            _selectedType = (ReforgeType)index;
            UpdateRecipeInfo();
        }

        private void OnRaritySelected(long index)
        {
            _selectedRarity = (ReforgeRarity)index;
            UpdateRecipeInfo();
        }

        private void OnReforgePressed()
        {
            if (EquipmentReforgingSystem.Instance == null) return;
            
            // 根据选择执行洗练
            if (_typeOption.Selected == 0)
            {
                EquipmentReforgingSystem.Instance.TryReforgeEquipment(_selectedEquipmentId, ReforgeType.Basic);
            }
            else if (_typeOption.Selected == 1)
            {
                EquipmentReforgingSystem.Instance.TryAdvancedReforge(_selectedEquipmentId, _selectedRarity);
            }
            else
            {
                EquipmentReforgingSystem.Instance.TryReforgeEquipment(_selectedEquipmentId, ReforgeType.Legendary);
            }
        }

        private void OnReforgeCompleted(string equipmentId, bool success, Dictionary<string, float> newAttributes)
        {
            string title = success ? "[color=green]洗练成功![/color]" : "[color=red]洗练失败![/color]";
            string message = success 
                ? $"新属性: {string.Join(", ", newAttributes)}"
                : "很遗憾,洗练失败了";
            
            // 显示结果提示(可以通过弹出面板实现)
            GD.Print($"{title} {message}");
            
            UpdateRecipeInfo();
            UpdateStats();
        }

        private void OnClosePressed()
        {
            Hide();
        }

        public void Toggle()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                RefreshEquipmentList();
                UpdateRecipeInfo();
                UpdateStats();
            }
        }
    }
}
