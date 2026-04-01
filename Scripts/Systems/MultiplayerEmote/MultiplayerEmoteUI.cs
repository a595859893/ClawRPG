using Godot;
using System;
using System.Collections.Generic;
using MultiplayerEmoteSystem;

/// <summary>
/// 多人表情UI
/// 显示表情轮盘和使用统计
/// </summary>
public partial class MultiplayerEmoteUI : Control
{
    public static MultiplayerEmoteUI Instance { get; private set; }

    // UI组件
    private PanelContainer _mainPanel;
    private GridContainer _emoteGrid;
    private Label _comboLabel;
    private Label _statsLabel;
    private HSlider _categoryFilter;
    private CheckButton _showHotkey;

    // 表情按钮映射
    private Dictionary<EmoteType, TextureRect> _emoteButtons = new Dictionary<EmoteType, TextureRect>();

    // 当前选中的分类
    private EmoteCategory _currentCategory = EmoteCategory.Social;

    // 是否显示
    private bool _isVisible = false;

    // 热键
    private Key _toggleKey = Key.E;

    public override void _Ready()
    {
        Instance = this;
        SetupUI();
        Hide();
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == _toggleKey)
            {
                ToggleUI();
            }
            else if (keyEvent.Keycode >= Key.Key1 && keyEvent.Keycode <= Key.Key9)
            {
                // 数字键快速使用表情
                int index = keyEvent.Keycode - Key.Key1;
                UseEmoteByIndex(index);
            }
        }
    }

    /// <summary>
    /// 设置UI
    /// </summary>
    private void SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(400, 450);
        AddChild(_mainPanel);

        var mainVBox = new VBoxContainer();
        _mainPanel.AddChild(mainVBox);

        // 标题
        var titleLabel = new Label();
        titleLabel.Text = "🎭 多人表情";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(titleLabel);

        // 分类筛选
        var categoryLabel = new Label();
        categoryLabel.Text = "分类:";
        mainVBox.AddChild(categoryLabel);

        _categoryFilter = new HSlider();
        _categoryFilter.MinValue = 0;
        _categoryFilter.MaxValue = 4;
        _categoryFilter.Value = 0;
        _categoryFilter.ValueChanged += OnCategoryChanged;
        mainVBox.AddChild(_categoryFilter);

        var categoryNames = new Label();
        categoryNames.Text = "社交  情感  动作  战斗  庆祝";
        categoryNames.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(categoryNames);

        // 表情网格
        _emoteGrid = new GridContainer();
        _emoteGrid.Columns = 5;
        _emoteGrid.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(_emoteGrid);

        // 填充表情按钮
        PopulateEmoteGrid();

        // 连击显示
        _comboLabel = new Label();
        _comboLabel.Text = "连击: 0";
        _comboLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(_comboLabel);

        // 统计信息
        _statsLabel = new Label();
        _statsLabel.Text = "使用次数: 0";
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(_statsLabel);

        // 使用说明
        var helpLabel = new Label();
        helpLabel.Text = "快捷键: 1-9 快速使用 | E 打开/关闭";
        helpLabel.HorizontalAlignment = HorizontalAlignment.Center;
        helpLabel.AddThemeFontSizeOverride("font_size", 10);
        mainVBox.AddChild(helpLabel);
    }

    /// <summary>
    /// 填充表情网格
    /// </summary>
    private void PopulateEmoteGrid()
    {
        // 清除现有按钮
        foreach (Node child in _emoteGrid.GetChildren())
        {
            child.QueueFree();
        }
        _emoteButtons.Clear();

        // 获取当前分类的表情
        var emotes = MultiplayerEmoteDatabase.Instance.GetEmotesByCategory(_currentCategory);

        foreach (EmoteType emote in emotes)
        {
            var config = MultiplayerEmoteDatabase.Instance.GetEmoteConfig(emote);
            if (config == null) continue;

            // 创建表情按钮
            var button = new TextureRect();
            button.CustomMinimumSize = new Vector2(60, 60);
            
            // 设置占位图标
            var placeholder = new ColorRect();
            placeholder.Color = GetEmoteColor(emote);
            placeholder.CustomMinimumSize = new Vector2(50, 50);
            button.AddChild(placeholder);

            // 添加标签
            var label = new Label();
            label.Text = config.Name;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.AddThemeFontSizeOverride("font_size", 10);
            button.AddChild(label);

            // 按钮点击事件
            button.GuiInput += (InputEvent evt) =>
            {
                if (evt is InputEventMouseButton mouseEvt && mouseEvt.Pressed && mouseEvt.ButtonIndex == MouseButton.Left)
                {
                    UseEmote(emote);
                }
            };

            _emoteGrid.AddChild(button);
            _emoteButtons[emote] = button;
        }
    }

    /// <summary>
    /// 获取表情颜色
    /// </summary>
    private Color GetEmoteColor(EmoteType emote)
    {
        switch (emote)
        {
            case EmoteType.Wave: return new Color(0.4f, 0.7f, 1f);
            case EmoteType.Laugh: return new Color(1f, 0.8f, 0.2f);
            case EmoteType.Cry: return new Color(0.5f, 0.7f, 1f);
            case EmoteType.Dance: return new Color(1f, 0.4f, 0.7f);
            case EmoteType.Clap: return new Color(0.9f, 0.7f, 0.3f);
            case EmoteType.ThumbsUp: return new Color(0.3f, 0.9f, 0.5f);
            case EmoteType.Love: return new Color(1f, 0.3f, 0.5f);
            case EmoteType.Cheer: return new Color(1f, 0.6f, 0.2f);
            case EmoteType.Angry: return new Color(1f, 0.3f, 0.3f);
            default: return new Color(0.6f, 0.6f, 0.6f);
        }
    }

    /// <summary>
    /// 分类改变
    /// </summary>
    private void OnCategoryChanged(float value)
    {
        _currentCategory = (EmoteCategory)(int)value;
        PopulateEmoteGrid();
    }

    /// <summary>
    /// 切换UI显示
    /// </summary>
    public void ToggleUI()
    {
        if (_isVisible)
        {
            Hide();
        }
        else
        {
            Show();
            UpdateDisplay();
        }
        _isVisible = !_isVisible;
    }

    /// <summary>
    /// 更新显示
    /// </summary>
    public void UpdateDisplay()
    {
        if (MultiplayerEmoteSystem.Instance == null) return;

        // 更新连击
        int combo = MultiplayerEmoteSystem.Instance.CurrentCombo;
        _comboLabel.Text = $"🔥 连击: {combo}";

        // 更新统计
        var stats = MultiplayerEmoteSystem.Instance.Statistics;
        _statsLabel.Text = $"总使用次数: {stats.TotalEmotesUsed}";

        // 更新按钮状态
        int playerId = 1;
        foreach (var kvp in _emoteButtons)
        {
            bool unlocked = MultiplayerEmoteSystem.Instance.IsEmoteUnlocked(kvp.Key, playerId);
            kvp.Value.Modulate = unlocked ? Color.White : new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
    }

    /// <summary>
    /// 使用表情
    /// </summary>
    public void UseEmote(EmoteType emote)
    {
        if (MultiplayerEmoteSystem.Instance == null) return;

        var player = GetPlayerNode();
        Vector2 position = player != null ? player.Position : Vector2.Zero;

        MultiplayerEmoteSystem.Instance.UseEmote(emote, position);
        UpdateDisplay();
    }

    /// <summary>
    /// 按索引使用表情
    /// </summary>
    private void UseEmoteByIndex(int index)
    {
        var emotes = MultiplayerEmoteDatabase.Instance.GetEmotesByCategory(_currentCategory);
        if (index >= 0 && index < emotes.Count)
        {
            UseEmote(emotes[index]);
        }
    }

    private Node GetPlayerNode()
    {
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is Player player)
            {
                return player;
            }
        }
        return null;
    }

    public override void _ExitTree()
    {
        Instance = null;
    }
}
