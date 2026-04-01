using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyWeaknessUI : Control
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
        var mainContainer = VBoxContainernew;
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(600, 500);
        AddChild(mainContainer);

        // 标题
        _titleLabel = Labelnew;
        _titleLabel.Text = "🎯 Enemy Weakness System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);

        // Tab 容器
        _tabContainer = TabContainernew;
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);

        // 创建标签页
        CreateWeaknessListTab();
        CreateEnemyInfoTab();
        CreateStatisticsTab();

        // 测试按钮
        var testButton = Buttonnew;
        testButton.Text = "🧪 Test Weakness System";
        testButton.Pressed += () => OnTestButtonPressed();
        mainContainer.AddChild(testButton);

        // 关闭提示
        var hintLabel = Labelnew;
        hintLabel.Text = "Press ESC to close";
        hintLabel.Align = Label.AlignEnum.Center;
        hintLabel.AddThemeFontSizeOverride("font_size", 14);
        mainContainer.AddChild(hintLabel);
    }

    private void CreateWeaknessListTab()
    {
        var tab = VBoxContainernew;
        tab.Name = "Weakness List";
        _tabContainer.AddChild(tab);

        var title = Labelnew;
        title.Text = "📋 All Weakness Types";
        title.AddThemeFontSizeOverride("font_size", 18);
        title.Align = Label.AlignEnum.Center;
        tab.AddChild(title);

        var scroll = ScrollContainernew;
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tab.AddChild(scroll);

        var list = VBoxContainernew;
        scroll.AddChild(list);

        // 显示所有弱点类型
        if (_database != null)
        {
            foreach (var weakness in _database.AllWeaknesses.Values)
            {
                var item = Labelnew;
                item.Text = $"[{weakness.Type}] {weakness.Element}: {weakness.Description}";
                list.AddChild(item);
            }
        }
    }

    private void CreateEnemyInfoTab()
    {
        var tab = VBoxContainernew;
        tab.Name = "Enemy Info";
        _tabContainer.AddChild(tab);

        var title = Labelnew;
        title.Text = "👹 Enemy Weakness Info";
        title.AddThemeFontSizeOverride("font_size", 18);
        title.Align = Label.AlignEnum.Center;
        tab.AddChild(title);

        // 敌人选择
        var enemySelectLabel = Labelnew;
        enemySelectLabel.Text = "Select Enemy:";
        tab.AddChild(enemySelectLabel);

        var enemyOptionButton = OptionButtonnew;
        enemyOptionButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        foreach (var enemy in _testEnemies)
        {
            enemyOptionButton.AddItem(enemy);
        }
        enemyOptionButton.ItemSelected += (index) => OnEnemySelected(index);
        tab.AddChild(enemyOptionButton);

        // 弱点信息显示
        _enemyList = VBoxContainernew;
        _enemyList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tab.AddChild(_enemyList);
    }

    private void CreateStatisticsTab()
    {
        var tab = VBoxContainernew;
        tab.Name = "Statistics";
        _tabContainer.AddChild(tab);

        var title = Labelnew;
        title.Text = "📊 Weakness Statistics";
        title.AddThemeFontSizeOverride("font_size", 18);
        title.Align = Label.AlignEnum.Center;
        tab.AddChild(title);

        _statisticsPanel = VBoxContainernew;
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
        var weaknessTitle = Labelnew;
        weaknessTitle.Text = "⚔️ Weaknesses:";
        weaknessTitle.AddThemeFontSizeOverride("font_size", 16);
        _enemyList.AddChild(weaknessTitle);

        var weaknesses = _database.GetEnemyWeaknesses(enemyType);
        if (weaknesses.Count == 0)
        {
            var noWeakness = Labelnew;
            noWeakness.Text = "  No known weaknesses";
            _enemyList.AddChild(noWeakness);
        }
        else
        {
            foreach (var weakness in weaknesses)
            {
                var item = Labelnew;
                item.Text = $"  • {weakness.Element}: {weakness.Description}";
                _enemyList.AddChild(item);
            }
        }

        // 显示抗性
        var resistanceTitle = Labelnew;
        resistanceTitle.Text = "🛡️ Resistances:";
        resistanceTitle.AddThemeFontSizeOverride("font_size", 16);
        _enemyList.AddChild(resistanceTitle);

        var config = _database.GetEnemyWeaknessConfig(enemyType);
        if (config == null || config.ResistanceIDs.Count == 0)
        {
            var noResistance = Labelnew;
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
                    var item = Labelnew;
                    item.Text = $"  • {resistance.Element}: {resistance.Description}";
                    _enemyList.AddChild(item);
                }
            }
        }

        // 显示提示
        var hint = _system.GetWeaknessHint(enemyType);
        if (!string.IsNullOrEmpty(hint))
        {
            var hintLabel = Labelnew;
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
            var noData = Labelnew;
            noData.Text = "No statistics available";
            _statisticsPanel.AddChild(noData);
            return;
        }

        // 显示统计
        var totalActivations = Labelnew;
        totalActivations.Text = $"Total Weakness Activations: {_data.TotalWeaknessActivations}";
        _statisticsPanel.AddChild(totalActivations);

        var totalBonus = Labelnew;
        totalBonus.Text = $"Total Bonus Damage: {_data.TotalBonusDamage}";
        _statisticsPanel.AddChild(totalBonus);

        // 弱点类型使用统计
        if (_data.WeaknessTypeUsage.Count > 0)
        {
            var typeTitle = Labelnew;
            typeTitle.Text = "By Weakness Type:";
            typeTitle.AddThemeFontSizeOverride("font_size", 14);
            _statisticsPanel.AddChild(typeTitle);

            foreach (var kvp in _data.WeaknessTypeUsage)
            {
                var item = Labelnew;
                item.Text = $"  {kvp.Key}: {kvp.Value}";
                _statisticsPanel.AddChild(item);
            }
        }

        // 元素使用统计
        if (_data.ElementUsage.Count > 0)
        {
            var elementTitle = Labelnew;
            elementTitle.Text = "By Element:";
            elementTitle.AddThemeFontSizeOverride("font_size", 14);
            _statisticsPanel.AddChild(elementTitle);

            foreach (var kvp in _data.ElementUsage)
            {
                var item = Labelnew;
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
