using Godot;
using System;
using System.Collections.Generic;

public class EnemyWeaknessUI : Control
{
    // 引用
    private EnemyWeaknessSystem _system;
    private EnemyWeaknessData _data;
    private EnemyWeaknessDatabase _database;

    // UI 元素
    private Label _titleLabel;
    private VBoxContainer _weaknessList;
    private VBoxContainer _enemyList;
    private VBoxContainer _statisticsPanel;
    private TabContainer _tabContainer;

    // 当前选中的敌人ID
    private int _selectedEnemyId = -1;

    // 敌人列表（模拟数据）
    private List<string> _testEnemies = new List<string>
    {
        "FireElemental",
        "IceElemental",
        "LightningElemental",
        "ShadowCreature",
        "HolyCreature",
        "Mechanical",
        "Undead",
        "Beast",
        "Armored",
        "Flying"
    };

    public override void _Ready()
    {
        _system = GetNode<EnemyWeaknessSystem>("/root/EnemyWeaknessSystem");
        _data = GetNode<EnemyWeaknessData>("/root/EnemyWeaknessData");
        _database = GetNode<EnemyWeaknessDatabase>("/root/EnemyWeaknessDatabase");

        SetupUI();
        SetupInput();
    }

    private void SetupUI()
    {
        // 主容器
        var mainContainer = VBoxContainer.new();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(600, 500);
        AddChild(mainContainer);

        // 标题
        _titleLabel = Label.new();
        _titleLabel.Text = "🎯 Enemy Weakness System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);

        // Tab 容器
        _tabContainer = TabContainer.new();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);

        // 创建标签页
        CreateWeaknessListTab();
        CreateEnemyInfoTab();
        CreateStatisticsTab();

        // 测试按钮
        var testButton = Button.new();
        testButton.Text = "🧪 Test Weakness System";
        testButton.Pressed += () => OnTestButtonPressed();
        mainContainer.AddChild(testButton);

        // 关闭提示
        var hintLabel = Label.new();
        hintLabel.Text = "Press ESC to close";
        hintLabel.Align = Label.AlignEnum.Center;
        hintLabel.AddThemeFontSizeOverride("font_size", 14);
        mainContainer.AddChild(hintLabel);
    }

    private void CreateWeaknessListTab()
    {
        var tab = VBoxContainer.new();
        tab.Name = "Weakness List";
        _tabContainer.AddChild(tab);

        var title = Label.new();
        title.Text = "📋 All Weakness Types";
        title.AddThemeFontSizeOverride("font_size", 18);
        title.Align = Label.AlignEnum.Center;
        tab.AddChild(title);

        var scroll = ScrollContainer.new();
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tab.AddChild(scroll);

        var list = VBoxContainer.new();
        scroll.AddChild(list);

        // 显示所有弱点类型
        if (_database != null)
        {
            foreach (var weakness in _database.AllWeaknesses.Values)
            {
                var item = Label.new();
                item.Text = $"[{weakness.Type}] {weakness.Element}: {weakness.Description}";
                list.AddChild(item);
            }
        }
    }

    private void CreateEnemyInfoTab()
    {
        var tab = VBoxContainer.new();
        tab.Name = "Enemy Info";
        _tabContainer.AddChild(tab);

        var title = Label.new();
        title.Text = "👹 Enemy Weakness Info";
        title.AddThemeFontSizeOverride("font_size", 18);
        title.Align = Label.AlignEnum.Center;
        tab.AddChild(title);

        // 敌人选择
        var enemySelectLabel = Label.new();
        enemySelectLabel.Text = "Select Enemy:";
        tab.AddChild(enemySelectLabel);

        var enemyOptionButton = OptionButton.new();
        enemyOptionButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        foreach (var enemy in _testEnemies)
        {
            enemyOptionButton.AddItem(enemy);
        }
        enemyOptionButton.ItemSelected += (index) => OnEnemySelected(index);
        tab.AddChild(enemyOptionButton);

        // 弱点信息显示
        _enemyList = VBoxContainer.new();
        _enemyList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tab.AddChild(_enemyList);
    }

    private void CreateStatisticsTab()
    {
        var tab = VBoxContainer.new();
        tab.Name = "Statistics";
        _tabContainer.AddChild(tab);

        var title = Label.new();
        title.Text = "📊 Weakness Statistics";
        title.AddThemeFontSizeOverride("font_size", 18);
        title.Align = Label.AlignEnum.Center;
        tab.AddChild(title);

        _statisticsPanel = VBoxContainer.new();
        _statisticsPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tab.AddChild(_statisticsPanel);

        UpdateStatistics();
    }

    private void OnEnemySelected(int index)
    {
        if (index < 0 || index >= _testEnemies.Count)
            return;

        string enemyType = _testEnemies[index];

        // 清除旧内容
        foreach (var child in _enemyList.GetChildren())
        {
            child.QueueFree();
        }

        // 显示弱点
        var weaknessTitle = Label.new();
        weaknessTitle.Text = "⚔️ Weaknesses:";
        weaknessTitle.AddThemeFontSizeOverride("font_size", 16);
        _enemyList.AddChild(weaknessTitle);

        var weaknesses = _database.GetEnemyWeaknesses(enemyType);
        if (weaknesses.Count == 0)
        {
            var noWeakness = Label.new();
            noWeakness.Text = "  No known weaknesses";
            _enemyList.AddChild(noWeakness);
        }
        else
        {
            foreach (var weakness in weaknesses)
            {
                var item = Label.new();
                item.Text = $"  • {weakness.Element}: {weakness.Description}";
                _enemyList.AddChild(item);
            }
        }

        // 显示抗性
        var resistanceTitle = Label.new();
        resistanceTitle.Text = "🛡️ Resistances:";
        resistanceTitle.AddThemeFontSizeOverride("font_size", 16);
        _enemyList.AddChild(resistanceTitle);

        var config = _database.GetEnemyWeaknessConfig(enemyType);
        if (config == null || config.ResistanceIDs.Count == 0)
        {
            var noResistance = Label.new();
            noResistance.Text = "  No known resistances";
            _enemyList.AddChild(noResistance);
        }
        else
        {
            foreach (var resistanceId in config.ResistanceIDs)
            {
                var resistance = _database.GetWeaknessConfig(resistanceId);
                if (resistance != null)
                {
                    var item = Label.new();
                    item.Text = $"  • {resistance.Element}: {resistance.Description}";
                    _enemyList.AddChild(item);
                }
            }
        }

        // 显示提示
        var hint = _system.GetWeaknessHint(enemyType);
        if (!string.IsNullOrEmpty(hint))
        {
            var hintLabel = Label.new();
            hintLabel.Text = $"💡 Hint: {hint}";
            hintLabel.AddThemeFontSizeOverride("font_size", 14);
            _enemyList.AddChild(hintLabel);
        }
    }

    private void UpdateStatistics()
    {
        foreach (var child in _statisticsPanel.GetChildren())
        {
            child.QueueFree();
        }

        if (_data == null)
        {
            var noData = Label.new();
            noData.Text = "No statistics available";
            _statisticsPanel.AddChild(noData);
            return;
        }

        // 显示统计
        var totalActivations = Label.new();
        totalActivations.Text = $"Total Weakness Activations: {_data.TotalWeaknessActivations}";
        _statisticsPanel.AddChild(totalActivations);

        var totalBonus = Label.new();
        totalBonus.Text = $"Total Bonus Damage: {_data.TotalBonusDamage}";
        _statisticsPanel.AddChild(totalBonus);

        // 弱点类型使用统计
        if (_data.WeaknessTypeUsage.Count > 0)
        {
            var typeTitle = Label.new();
            typeTitle.Text = "By Weakness Type:";
            typeTitle.AddThemeFontSizeOverride("font_size", 14);
            _statisticsPanel.AddChild(typeTitle);

            foreach (var kvp in _data.WeaknessTypeUsage)
            {
                var item = Label.new();
                item.Text = $"  {kvp.Key}: {kvp.Value}";
                _statisticsPanel.AddChild(item);
            }
        }

        // 元素使用统计
        if (_data.ElementUsage.Count > 0)
        {
            var elementTitle = Label.new();
            elementTitle.Text = "By Element:";
            elementTitle.AddThemeFontSizeOverride("font_size", 14);
            _statisticsPanel.AddChild(elementTitle);

            foreach (var kvp in _data.ElementUsage)
            {
                var item = Label.new();
                item.Text = $"  {kvp.Key}: {kvp.Value}";
                _statisticsPanel.AddChild(item);
            }
        }
    }

    private void OnTestButtonPressed()
    {
        if (_system != null)
        {
            _system.TestWeaknessSystem();
            UpdateStatistics();
        }
    }

    private void SetupInput()
    {
        // ESC 关闭
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
            GetTree().SetInputAsHandled();
        }
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
            UpdateStatistics();
        }
    }
}
