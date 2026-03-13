using Godot;
using System;
using System.Collections.Generic;

public class PetFusionUI : Control {
    private PetFusionSystem _system;
    private Control _mainContainer;
    private Label _titleLabel;
    private OptionButton _pet1Select;
    private OptionButton _pet2Select;
    private Label _pet1TypeLabel;
    private Label _pet2TypeLabel;
    private Label _pet1LevelLabel;
    private Label _pet2LevelLabel;
    private Label _costLabel;
    private Label _successRateLabel;
    private Label _resultLabel;
    private Button _previewButton;
    private Button _fusionButton;
    private Button _closeButton;
    private VBoxContainer _historyContainer;
    private HBoxContainer _statsContainer;
    
    // 模拟宠物数据
    private List<Dictionary<string, object>> _availablePets = new List<Dictionary<string, object>>();
    
    public override void _Ready() {
        _system = new PetFusionSystem();
        SetupUI();
        SetupPetData();
        RefreshUI();
    }
    
    private void SetupUI() {
        // 主容器
        _mainContainer = new Control();
        _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainContainer.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainContainer);
        
        // 背景面板
        var bgPanel = new Panel();
        bgPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        bgPanel.Modulate = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        _mainContainer.AddChild(bgPanel);
        
        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "🔮 宠物融合系统";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _titleLabel.Position = new Vector2(0, 20);
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(_titleLabel);
        
        // 宠物1选择
        var pet1Label = new Label();
        pet1Label.Text = "宠物 1:";
        pet1Label.Position = new Vector2(50, 80);
        _mainContainer.AddChild(pet1Label);
        
        _pet1Select = new OptionButton();
        _pet1Select.Position = new Vector2(130, 75);
        _pet1Select.Size = new Vector2(150, 30);
        _pet1Select.ItemSelected += OnPet1Selected;
        _mainContainer.AddChild(_pet1Select);
        
        _pet1TypeLabel = new Label();
        _pet1TypeLabel.Text = "类型: -";
        _pet1TypeLabel.Position = new Vector2(300, 80);
        _mainContainer.AddChild(_pet1TypeLabel);
        
        _pet1LevelLabel = new Label();
        _pet1LevelLabel.Text = "等级: -";
        _pet1LevelLabel.Position = new Vector2(450, 80);
        _mainContainer.AddChild(_pet1LevelLabel);
        
        // 宠物2选择
        var pet2Label = new Label();
        pet2Label.Text = "宠物 2:";
        pet2Label.Position = new Vector2(50, 130);
        _mainContainer.AddChild(pet2Label);
        
        _pet2Select = new OptionButton();
        _pet2Select.Position = new Vector2(130, 125);
        _pet2Select.Size = new Vector2(150, 30);
        _pet2Select.ItemSelected += OnPet2Selected;
        _mainContainer.AddChild(_pet2Select);
        
        _pet2TypeLabel = new Label();
        _pet2TypeLabel.Text = "类型: -";
        _pet2TypeLabel.Position = new Vector2(300, 130);
        _mainContainer.AddChild(_pet2TypeLabel);
        
        _pet2LevelLabel = new Label();
        _pet2LevelLabel.Text = "等级: -";
        _pet2LevelLabel.Position = new Vector2(450, 130);
        _mainContainer.AddChild(_pet2LevelLabel);
        
        // 分隔线
        var separator = new HSeparator();
        separator.Position = new Vector2(30, 170);
        separator.Size = new Vector2(540, 5);
        _mainContainer.AddChild(separator);
        
        // 预览信息
        var infoLabel = new Label();
        infoLabel.Text = "融合预览:";
        infoLabel.Position = new Vector2(50, 190);
        _mainContainer.AddChild(infoLabel);
        
        _costLabel = new Label();
        _costLabel.Text = "预计费用: -";
        _costLabel.Position = new Vector2(50, 220);
        _mainContainer.AddChild(_costLabel);
        
        _successRateLabel = new Label();
        _successRateLabel.Text = "成功率: -";
        _successRateLabel.Position = new Vector2(300, 220);
        _mainContainer.AddChild(_successRateLabel);
        
        // 结果显示
        _resultLabel = new Label();
        _resultLabel.Text = "";
        _resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _resultLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _resultLabel.Position = new Vector2(0, 260);
        _resultLabel.AddThemeFontSizeOverride("font_size", 18);
        _mainContainer.AddChild(_resultLabel);
        
        // 按钮
        _previewButton = new Button();
        _previewButton.Text = "预览";
        _previewButton.Position = new Vector2(150, 310);
        _previewButton.Size = new Vector2(120, 40);
        _previewButton.Pressed += OnPreviewPressed;
        _mainContainer.AddChild(_previewButton);
        
        _fusionButton = new Button();
        _fusionButton.Text = "融合!";
        _fusionButton.Position = new Vector2(330, 310);
        _fusionButton.Size = new Vector2(120, 40);
        _fusionButton.Pressed += OnFusionPressed;
        _mainContainer.AddChild(_fusionButton);
        
        // 统计信息
        var statsLabel = new Label();
        statsLabel.Text = "统计:";
        statsLabel.Position = new Vector2(50, 370);
        _mainContainer.AddChild(statsLabel);
        
        var stats = _system.GetStatistics();
        var statsText = $"总融合: {stats["totalFusions"]} | 成功: {stats["successfulFusions"]} | 传说: {stats["legendaryFusions"]} | 花费: {stats["totalGoldSpent"]}G";
        
        var statsValueLabel = new Label();
        statsValueLabel.Text = statsText;
        statsValueLabel.Position = new Vector2(50, 400);
        _mainContainer.AddChild(statsValueLabel);
        
        // 关闭按钮
        _closeButton = new Button();
        _closeButton.Text = "关闭";
        _closeButton.Position = new Vector2(250, 450);
        _closeButton.Size = new Vector2(100, 35);
        _closeButton.Pressed += OnClosePressed;
        _mainContainer.AddChild(_closeButton);
    }
    
    private void SetupPetData() {
        // 模拟可用宠物
        _availablePets = new List<Dictionary<string, object>> {
            new Dictionary<string, object> { { "id", 1 }, { "name", "火焰狼" }, { "type", "Fire" }, { "level", 25 } },
            new Dictionary<string, object> { { "id", 2 }, { "name", "冰霜熊" }, { "type", "Ice" }, { "level", 30 } },
            new Dictionary<string, object> { { "id", 3 }, { "name", "雷电豹" }, { "type", "Lightning" }, { "level", 20 } },
            new Dictionary<string, object> { { "id", 4 }, { "name", "暗影狼" }, { "type", "Shadow" }, { "level", 35 } },
            new Dictionary<string, object> { { "id", 5 }, { "name", "光明狮" }, { "type", "Holy" }, { "level", 28 } },
            new Dictionary<string, object> { { "id", 6 }, { "name", "水元素" }, { "type", "Water" }, { "level", 22 } },
            new Dictionary<string, object> { { "id", 7 }, { "name", "龙宝宝" }, { "type", "Dragon" }, { "level", 40 } },
            new Dictionary<string, object> { { "id", 8 }, { "name", "史莱姆" }, { "type", "Slime" }, { "level", 10 } },
            new Dictionary<string, object> { { "id", 9 }, { "name", "骷髅兽" }, { "type", "Undead" }, { "level", 15 } },
            new Dictionary<string, object> { { "id", 10 }, { "name", "普通小狗" }, { "type", "Common" }, { "level", 5 } }
        };
        
        // 填充选项
        foreach (var pet in _availablePets) {
            string displayText = $"{pet["name"]} (Lv.{pet["level"]})";
            _pet1Select.AddItem(displayText);
            _pet2Select.AddItem(displayText);
        }
    }
    
    private void RefreshUI() {
        // 更新统计显示
    }
    
    private void OnPet1Selected(long index) {
        if (index >= 0 && index < _availablePets.Count) {
            var pet = _availablePets[(int)index];
            _pet1TypeLabel.Text = $"类型: {pet["type"]}";
            _pet1LevelLabel.Text = $"等级: {pet["level"]}";
        }
    }
    
    private void OnPet2Selected(long index) {
        if (index >= 0 && index < _availablePets.Count) {
            var pet = _availablePets[(int)index];
            _pet2TypeLabel.Text = $"类型: {pet["type"]}";
            _pet2LevelLabel.Text = $"等级: {pet["level"]}";
        }
    }
    
    private void OnPreviewPressed() {
        int pet1Index = _pet1Select.Selected;
        int pet2Index = _pet2Select.Selected;
        
        if (pet1Index < 0 || pet2Index < 0) {
            _resultLabel.Text = "请选择两只宠物!";
            _resultLabel.Modulate = new Color(1f, 0.5f, 0.5f);
            return;
        }
        
        if (pet1Index == pet2Index) {
            _resultLabel.Text = "请选择两只不同的宠物!";
            _resultLabel.Modulate = new Color(1f, 0.5f, 0.5f);
            return;
        }
        
        var pet1 = _availablePets[pet1Index];
        var pet2 = _availablePets[pet2Index];
        
        var preview = _system.PreviewFusion(
            (string)pet1["type"],
            (string)pet2["type"],
            (int)pet1["level"],
            (int)pet2["level"]
        );
        
        _costLabel.Text = $"预计费用: {preview["estimatedCost"]}G";
        _successRateLabel.Text = $"成功率: {preview["successRate"]:F1}%";
        
        _resultLabel.Text = $"预计结果: {preview["resultType"]} ({preview["estimatedRarity"]})";
        _resultLabel.Modulate = PetFusionDatabase.GetRarityColor((string)preview["estimatedRarity"]);
    }
    
    private void OnFusionPressed() {
        int pet1Index = _pet1Select.Selected;
        int pet2Index = _pet2Select.Selected;
        
        if (pet1Index < 0 || pet2Index < 0) {
            _resultLabel.Text = "请选择两只宠物!";
            _resultLabel.Modulate = new Color(1f, 0.5f, 0.5f);
            return;
        }
        
        if (pet1Index == pet2Index) {
            _resultLabel.Text = "请选择两只不同的宠物!";
            _resultLabel.Modulate = new Color(1f, 0.5f, 0.5f);
            return;
        }
        
        var pet1 = _availablePets[pet1Index];
        var pet2 = _availablePets[pet2Index];
        
        // 模拟玩家金币
        int playerGold = 10000;
        
        var result = _system.FusionPets(
            (string)pet1["type"],
            (string)pet2["type"],
            (int)pet1["level"],
            (int)pet2["level"],
            playerGold
        );
        
        switch (result) {
            case PetFusionResult.Legendary:
                _resultLabel.Text = "🎉 传说融合成功! 获得传说宠物!";
                _resultLabel.Modulate = new Color(1f, 0.6f, 0f);
                break;
            case PetFusionResult.Epic:
                _resultLabel.Text = "✨ 史诗融合成功! 获得史诗宠物!";
                _resultLabel.Modulate = new Color(0.6f, 0.3f, 0.8f);
                break;
            case PetFusionResult.Rare:
                _resultLabel.Text = "⭐ 稀有融合成功! 获得稀有宠物!";
                _resultLabel.Modulate = new Color(0.2f, 0.5f, 1f);
                break;
            case PetFusionResult.Uncommon:
                _resultLabel.Text = "✓ 优秀融合成功! 获得优秀宠物!";
                _resultLabel.Modulate = new Color(0.2f, 0.8f, 0.2f);
                break;
            case PetFusionResult.Common:
                _resultLabel.Text = "○ 普通融合成功! 获得普通宠物!";
                _resultLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
                break;
            case PetFusionResult.Failure:
                _resultLabel.Text = "💔 融合失败! 宠物消失了...";
                _resultLabel.Modulate = new Color(1f, 0.3f, 0.3f);
                break;
        }
        
        // 保存数据
        _system.SaveData();
        RefreshUI();
    }
    
    private void OnClosePressed() {
        QueueFree();
    }
}
