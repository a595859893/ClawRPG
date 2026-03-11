using Godot;
using Godot.Collections;
using System;
using System.Linq;

namespace ClawRPG.Scripts.Systems.GemSystem {
    /// <summary>
    /// 宝石合成界面
    /// </summary>
    public class GemFusionUI : Control {
        private Control _mainContainer;
        private VBoxContainer _recipeList;
        private Label _goldLabel;
        private Label _statsLabel;
        private Label _resultLabel;
        private Button _closeButton;
        
        private GemFusionSystem _fusionSystem;
        private GemSystem _gemSystem;
        private Player _player;
        
        private GemFusionRecipe _selectedRecipe;
        private string _selectedSourceGemId;
        
        public override void _Ready() {
            _fusionSystem = GemFusionSystem.Instance;
            _gemSystem = GemSystem.Instance;
            
            var main = GetTree().CurrentScene;
            if (main != null) {
                _player = main.GetNode<Player>("Player");
            }
            
            _CreateUI();
            _ConnectSignals();
            
            // 初始隐藏
            Hide();
        }
        
        private void _CreateUI() {
            // 主容器
            _mainContainer = new Control();
            _mainContainer.SetAnchorsPreset(Control.Preset.FullRect);
            _mainContainer.MouseFilter = Control.MouseFilterEnum.Stop;
            AddChild(_mainContainer);
            
            // 背景
            var bg = new ColorRect();
            bg.Color = new Color(0, 0, 0, 0.7f);
            bg.SetAnchorsPreset(Control.Preset.FullRect);
            _mainContainer.AddChild(bg);
            
            // 面板
            var panel = new PanelContainer();
            panel.SetAnchorsPreset(Control.Preset.Center);
            panel.CustomMinimumSize = new Vector2(800, 600);
            _mainContainer.AddChild(panel);
            
            var panelMargin = new MarginContainer();
            panelMargin.SetAnchorsPreset(Control.Preset.FullRect);
            panelMargin.AddConstantOverride("margin_left", 20);
            panelMargin.AddConstantOverride("margin_right", 20);
            panelMargin.AddConstantOverride("margin_top", 20);
            panelMargin.AddConstantOverride("margin_bottom", 20);
            panel.AddChild(panelMargin);
            
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(Control.Preset.FullRect);
            panelMargin.AddChild(vbox);
            
            // 标题栏
            var titleBar = new HBoxContainer();
            vbox.AddChild(titleBar);
            
            var title = new Label();
            title.Text = "  宝石合成";
            title.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(title);
            
            titleBar.AddChild(new Control()); // Spacer
            
            _closeButton = new Button();
            _closeButton.Text = "X";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _closeButton.Pressed += () => Hide();
            titleBar.AddChild(_closeButton);
            
            // 金币和统计
            var infoBar = new HBoxContainer();
            vbox.AddChild(infoBar);
            
            _goldLabel = new Label();
            _goldLabel.Text = "金币: 0";
            infoBar.AddChild(_goldLabel);
            
            infoBar.AddChild(new Control()); // Spacer
            
            _statsLabel = new Label();
            _statsLabel.Text = "合成: 0次 | 成功率: 0%";
            infoBar.AddChild(_statsLabel);
            
            // 分割线
            var hsep = new HSeparator();
            vbox.AddChild(hsep);
            
            // 内容区域
            var content = new HBoxContainer();
            content.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            vbox.AddChild(content);
            
            // 左侧：配方列表
            var leftPanel = new PanelContainer();
            leftPanel.CustomMinimumSize = new Vector2(350, 0);
            content.AddChild(leftPanel);
            
            var leftScroll = new ScrollContainer();
            leftPanel.AddChild(leftScroll);
            
            _recipeList = new VBoxContainer();
            leftScroll.AddChild(_recipeList);
            
            // 右侧：详情和结果
            var rightPanel = new VBoxContainer();
            rightPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            content.AddChild(rightPanel);
            
            // 配方详情
            var detailPanel = new PanelContainer();
            rightPanel.AddChild(detailPanel);
            
            var detailVBox = new VBoxContainer();
            detailPanel.AddChild(detailVBox);
            
            var detailTitle = new Label();
            detailTitle.Text = "合成详情";
            detailTitle.AddThemeFontSizeOverride("font_size", 18);
            detailVBox.AddChild(detailTitle);
            
            _resultLabel = new Label();
            _resultLabel.Text = "选择配方";
            _resultLabel.CustomMinimumSize = new Vector2(0, 100);
            _resultLabel.AutowrapMode = TextServer.AwrapMode.Word;
            detailVBox.AddChild(_resultLabel);
            
            // 合成按钮
            var fuseButton = new Button();
            fuseButton.Text = "开始合成";
            fuseButton.CustomMinimumSize = new Vector2(0, 50);
            fuseButton.Pressed += _OnFuseButtonPressed;
            rightPanel.AddChild(fuseButton);
            
            // 刷新列表
            _RefreshRecipeList();
        }
        
        private void _ConnectSignals() {
            if (_fusionSystem != null) {
                _fusionSystem.Connect(nameof(GemFusionSystem.FusionCompleted), 
                    this, nameof(_OnFusionCompleted));
            }
        }
        
        private void _RefreshRecipeList() {
            // 清除现有列表
            foreach (var child in _recipeList.GetChildren()) {
                child.QueueFree();
            }
            
            // 获取所有配方
            var recipes = GemFusionDatabase.GetAllRecipes();
            
            foreach (var recipe in recipes) {
                // 检查玩家是否有足够的宝石
                bool canFuse = _gemSystem != null && _gemSystem.GetGemCount(recipe.SourceGemId) >= recipe.SourceGemCount;
                
                var btn = new Button();
                btn.Text = $"{_GetGemName(recipe.SourceGemId)} x{recipe.SourceGemCount} → {_GetGemName(recipe.ResultGemId)}";
                btn.CustomMinimumSize = new Vector2(0, 50);
                btn.Modulate = canFuse ? new Color(1, 1, 1) : new Color(0.5f, 0.5f, 0.5f);
                
                int gemCount = _gemSystem != null ? _gemSystem.GetGemCount(recipe.SourceGemId) : 0;
                btn.TooltipText = $"需要: {recipe.SourceGemCount}个 {GetGemTypeName(recipe.SourceGemId)}\n" +
                    $"金币: {recipe.GoldCost}\n" +
                    $"成功率: {recipe.SuccessRate * 100}%\n" +
                    $"当前拥有: {gemCount}个";
                
                btn.Pressed += () => _SelectRecipe(recipe);
                
                _recipeList.AddChild(btn);
            }
            
            _UpdateInfo();
        }
        
        private string _GetGemName(string gemId) {
            if (string.IsNullOrEmpty(gemId)) return "未知";
            
            var parts = gemId.Split('_');
            if (parts.Length < 2) return gemId;
            
            string type = parts[0];
            string rarity = parts[1];
            
            string typeName = type switch {
                "ruby" => "红宝石",
                "sapphire" => "蓝宝石",
                "emerald" => "绿宝石",
                "diamond" => "钻石",
                "topaz" => "黄宝石",
                "amethyst" => "紫宝石",
                "onyx" => "黑曜石",
                "pearl" => "珍珠",
                _ => type
            };
            
            string rarityName = rarity switch {
                "common" => "普通",
                "uncommon" => "优秀",
                "rare" => "稀有",
                "epic" => "史诗",
                "legendary" => "传说",
                _ => rarity
            };
            
            return $"{rarityName}{typeName}";
        }
        
        private string GetGemTypeName(string gemId) {
            return _GetGemName(gemId);
        }
        
        private void _SelectRecipe(GemFusionRecipe recipe) {
            _selectedRecipe = recipe;
            _selectedSourceGemId = recipe.SourceGemId;
            
            string result = $"合成配方:\n";
            result += $"源宝石: {GetGemTypeName(recipe.SourceGemId)} x{recipe.SourceGemCount}\n";
            result += $"结果: {GetGemTypeName(recipe.ResultGemId)}\n";
            result += $"金币: {recipe.GoldCost}\n";
            result += $"成功率: {recipe.SuccessRate * 100}%\n";
            
            int gemCount = _gemSystem != null ? _gemSystem.GetGemCount(recipe.SourceGemId) : 0;
            result += $"\n当前拥有: {gemCount}个";
            
            bool canFuse = gemCount >= recipe.SourceGemCount && 
                (_player != null && _player.Gold >= recipe.GoldCost);
            
            if (canFuse) {
                result += $"\n\n✓ 可以合成";
            } else {
                result += $"\n\n✗ 资源不足";
            }
            
            _resultLabel.Text = result;
        }
        
        private void _OnFuseButtonPressed() {
            if (_selectedRecipe == null || string.IsNullOrEmpty(_selectedSourceGemId)) {
                _resultLabel.Text = "请先选择配方";
                return;
            }
            
            var result = _fusionSystem.TryFusion(_selectedSourceGemId);
            
            if (result != null) {
                _resultLabel.Text = $"合成成功!\n获得: {GetGemTypeName(result)}";
                _resultLabel.Modulate = new Color(0, 1, 0);
            } else {
                _resultLabel.Text = "合成失败!\n资源不足或没有可用配方";
                _resultLabel.Modulate = new Color(1, 0, 0);
            }
            
            _RefreshRecipeList();
            
            // 延迟恢复颜色
            var timer = GetTree().CreateTimer(2.0f);
            timer.Connect("timeout", this, nameof(_ResetResultColor));
        }
        
        private void _ResetResultColor() {
            _resultLabel.Modulate = new Color(1, 1, 1);
        }
        
        private void _OnFusionCompleted(string resultGemId, bool success) {
            _RefreshRecipeList();
        }
        
        private void _UpdateInfo() {
            // 更新金币显示
            if (_player != null) {
                _goldLabel.Text = $"金币: {_player.Gold}";
            }
            
            // 更新统计
            if (_fusionSystem != null) {
                var stats = _fusionSystem.GetFusionStats();
                _statsLabel.Text = $"合成: {stats["total_fusions"]}次 | 成功率: {(float)stats["success_rate"] * 100:F1}%";
            }
        }
        
        public override void _Process(float delta) {
            if (Visible) {
                _UpdateInfo();
            }
        }
        
        public void Toggle() {
            if (Visible) {
                Hide();
            } else {
                Show();
                _RefreshRecipeList();
            }
        }
        
        public override void _Input(InputEvent evt) {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed) {
                // 按G键关闭
                if (keyEvent.Keycode == Key.G) {
                    Hide();
                }
            }
        }
    }
}
