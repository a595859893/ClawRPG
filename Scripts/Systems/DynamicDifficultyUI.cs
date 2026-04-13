using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 动态难度用户界面。显示和管理动态难度设置面板。
/// </summary>
public partial class DynamicDifficultyUI : Control
{
    // 单例
    private static DynamicDifficultyUI _instance;
    public static DynamicDifficultyUI Instance
    {
        get
        {
            if (_instance == null)
            {
                GD.PrintErr("DynamicDifficultyUI not initialized!");
            }
            return _instance;
        }
    }

    // UI组件
    private PanelContainer _mainPanel;
    private VBoxContainer _mainVBox;
    private Label _titleLabel;
    private HBoxContainer _difficultyDisplayBox;
    private Label _currentDifficultyLabel;
    private Label _recommendedDifficultyLabel;
    private Label _skillScoreLabel;
    private Label _playerGroupLabel;
    private CheckButton _autoAdjustCheck;
    private ButtonContainer _difficultyButtons;
    private Label _statsTitleLabel;
    private Label _sessionStatsLabel;
    private Label _skillProfileTitleLabel;
    private Label _skillProfileLabel;
    private Button _closeButton;

    // 主题颜色
    private Color _titleColor = new Color(1f, 0.84f, 0f);
    private Color _bgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    private Color _panelColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
    private Color _textColor = new Color(0.9f, 0.9f, 0.9f);
    private Color _highlightColor = new Color(0.3f, 0.6f, 1f);

    // 动画
    private Tween _tween;
    private bool _isVisible = false; 

    public override void _Ready()
    {
        _instance = this;
        _tween = CreateTween();
        
        SetupUI();
        ConnectSignals();
        
        // 初始隐藏
        Visible = false; 
    }

    private void SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(500, 600);
        AddChild(_mainPanel);

        // 样式
        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = _bgColor;
        style.CornerRadiusTopLeft = 10;
        style.CornerRadiusTopRight = 10;
        style.CornerRadiusBottomLeft = 10;
        style.CornerRadiusBottomRight = 10;
        style.BorderWidthLeft = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = _titleColor;
        _mainPanel.AddThemeStyleboxOverride("panel", style);

        // 主容器
        _mainVBox = new VBoxContainer();
        _mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _mainPanel.AddChild(_mainVBox);

        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ 动态难度系统";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _titleLabel.AddThemeColorOverride("font_color", _titleColor);
        _mainVBox.AddChild(_titleLabel);

        // 分隔
        AddSeparator();

        // 当前难度显示
        _difficultyDisplayBox = new HBoxContainer();
        _mainVBox.AddChild(_difficultyDisplayBox);

        _currentDifficultyLabel = new Label();
        _currentDifficultyLabel.Text = "当前难度: 普通";
        _currentDifficultyLabel.AddThemeFontSizeOverride("font_size", 18);
        _currentDifficultyLabel.AddThemeColorOverride("font_color", _highlightColor);
        _difficultyDisplayBox.AddChild(_currentDifficultyLabel);

        AddSeparator();

        // 建议难度
        _recommendedDifficultyLabel = new Label();
        _recommendedDifficultyLabel.Text = "建议难度: 普通";
        _recommendedDifficultyLabel.Align = Label.AlignEnum.Center;
        _mainVBox.AddChild(_recommendedDifficultyLabel);

        // 技能评分
        _skillScoreLabel = new Label();
        _skillScoreLabel.Text = "技能评分: 0.50";
        _skillScoreLabel.Align = Label.AlignEnum.Center;
        _mainVBox.AddChild(_skillScoreLabel);

        // 玩家分组
        _playerGroupLabel = new Label();
        _playerGroupLabel.Text = "玩家分组: 普通";
        _playerGroupLabel.Align = Label.AlignEnum.Center;
        _playerGroupLabel.AddThemeFontSizeOverride("font_size", 16);
        _mainVBox.AddChild(_playerGroupLabel);

        AddSeparator();

        // 自动调整开关
        _autoAdjustCheck = new CheckButton();
        _autoAdjustCheck.Text = "自动调整难度";
        _autoAdjustCheck.ButtonPressed = true;
        _mainVBox.AddChild(_autoAdjustCheck);

        // 难度按钮
        _difficultyButtons = new ButtonContainer();
        _mainVBox.AddChild(_difficultyButtons);

        AddSeparator();

        // 会话统计标题
        _statsTitleLabel = new Label();
        _statsTitleLabel.Text = "📊 当前会话统计";
        _statsTitleLabel.AddThemeFontSizeOverride("font_size", 16);
        _statsTitleLabel.AddThemeColorOverride("font_color", _titleColor);
        _mainVBox.AddChild(_statsTitleLabel);

        // 会话统计
        _sessionStatsLabel = new Label();
        _sessionStatsLabel.Text = "击杀: 0 | 死亡: 0 | 用时: 0分钟";
        _sessionStatsLabel.Align = Label.AlignEnum.Center;
        _mainVBox.AddChild(_sessionStatsLabel);

        AddSeparator();

        // 技能档案标题
        _skillProfileTitleLabel = new Label();
        _skillProfileTitleLabel.Text = "📈 技能档案";
        _skillProfileTitleLabel.AddThemeFontSizeOverride("font_size", 16);
        _skillProfileTitleLabel.AddThemeColorOverride("font_color", _titleColor);
        _mainVBox.AddChild(_skillProfileTitleLabel);

        // 技能档案
        _skillProfileLabel = new Label();
        _skillProfileLabel.Text = "总会话: 0 | 胜利: 0 | 失败: 0";
        _skillProfileLabel.Align = Label.AlignEnum.Center;
        _mainVBox.AddChild(_skillProfileLabel);

        AddSeparator();

        // 关闭按钮
        _closeButton = new Button();
        _closeButton.Text = " 关闭 ";
        _closeButton.CustomMinimumSize = new Vector2(120, 40);
        _mainVBox.AddChild(_closeButton);
    }

    private void AddSeparator()
    {
        HSeparator sep = new HSeparator();
        sep.AddThemeColorOverride("separator", new Color(0.3f, 0.3f, 0.3f));
        _mainVBox.AddChild(sep);
    }

    private void ConnectSignals()
    {
        _closeButton.Pressed += OnClosePressed;
        _autoAdjustCheck.Toggled += OnAutoAdjustToggled;
        
        // 订阅系统信号
        if (DynamicDifficultySystem.Instance != null)
        {
            DynamicDifficultySystem.Instance.DifficultyChanged += OnDifficultyChanged;
            DynamicDifficultySystem.Instance.SkillProfileUpdated += OnSkillProfileUpdated;
            DynamicDifficultySystem.Instance.RecommendationChanged += OnRecommendationChanged;
        }
    }

    #region 信号处理

    private void OnDifficultyChanged(DynamicDifficultyData.DifficultyLevel newDifficulty, 
        DynamicDifficultyData.DifficultyLevel oldDifficulty)
    {
        UpdateDisplay();
    }

    private void OnSkillProfileUpdated(DynamicDifficultyData.PlayerSkillProfile profile)
    {
        UpdateDisplay();
    }

    private void OnRecommendationChanged(DynamicDifficultyData.DifficultyLevel recommended)
    {
        UpdateDisplay();
    }

    private void OnClosePressed()
    {
        ToggleUI();
    }

    private void OnAutoAdjustToggled(bool toggledOn)
    {
        if (DynamicDifficultySystem.Instance != null)
        {
            DynamicDifficultySystem.Instance.SetAutoAdjustment(toggledOn);
        }
    }

    private void OnDifficultyButtonPressed(DynamicDifficultyData.DifficultyLevel level)
    {
        if (DynamicDifficultySystem.Instance != null)
        {
            DynamicDifficultySystem.Instance.SetDifficulty(level);
        }
    }

    #endregion

    #region 显示更新

    public void UpdateDisplay()
    {
        if (DynamicDifficultySystem.Instance == null) return;

        var system = DynamicDifficultySystem.Instance;

        // 当前难度
        DynamicDifficultyData.DifficultyLevel current = system.GetCurrentDifficulty();
        string currentName = DynamicDifficultyDatabase.GetDifficultyName(current);
        Color currentColor = DynamicDifficultyDatabase.GetDifficultyColor(current);
        _currentDifficultyLabel.Text = $"当前难度: {currentName}";
        _currentDifficultyLabel.AddThemeColorOverride("font_color", currentColor);

        // 建议难度
        DynamicDifficultyData.DifficultyLevel recommended = system.GetRecommendedDifficulty();
        string recommendedName = DynamicDifficultyDatabase.GetDifficultyName(recommended);
        Color recommendedColor = DynamicDifficultyDatabase.GetDifficultyColor(recommended);
        _recommendedDifficultyLabel.Text = $"建议难度: {recommendedName}";
        _recommendedDifficultyLabel.AddThemeColorOverride("font_color", recommendedColor);

        // 技能评分
        var profile = system.GetSkillProfile();
        _skillScoreLabel.Text = $"技能评分: {profile.OverallScore:F2}";

        // 玩家分组
        string group = DynamicDifficultyDatabase.GetPlayerGroup(profile.OverallScore);
        _playerGroupLabel.Text = $"玩家分组: {group}";

        // 自动调整
        _autoAdjustCheck.ButtonPressed = system.IsAutoAdjustment();

        // 会话统计
        var session = system.GetCurrentSessionStats();
        float minutes = session.SessionTime / 60f;
        _sessionStatsLabel.Text = $"击杀: {session.EnemiesKilled} | Boss: {session.BossesDefeated} | 死亡: {session.TimesDied} | 用时: {minutes:F1}分钟";

        // 技能档案
        _skillProfileLabel.Text = $"总会话: {profile.TotalSessions} | 胜利: {profile.Wins} | 失败: {profile.Losses} | 胜率: {profile.WinRate:P0}";

        // 更新难度按钮
        _difficultyButtons.UpdateButtons(current);
    }

    #endregion

    #region 动画

    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        
        if (_isVisible)
        {
            Show();
            UpdateDisplay();
            PlayOpenAnimation();
        }
        else
        {
            PlayCloseAnimation();
        }
    }

    private void PlayOpenAnimation()
    {
        _tween.Kill();
        _tween = CreateTween();
        
        // 淡入
        Modulate = new Color(1, 1, 1, 0);
        _tween.TweenProperty(this, "modulate:a", 1f, 0.3f);
        
        // 缩放
        Scale = new Vector2(0.9f, 0.9f);
        _tween.TweenProperty(this, "scale", Vector2.One, 0.3f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseOut);
    }

    private void PlayCloseAnimation()
    {
        _tween.Kill();
        _tween = CreateTween();
        
        // 淡出
        _tween.TweenProperty(this, "modulate:a", 0f, 0.2f);
        
        // 缩放
        _tween.TweenProperty(this, "scale", new Vector2(0.95f, 0.95f), 0.2f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseIn);
        
        _tween.TweenCallback(this, "hide");
    }

    #endregion

    // 难度按钮容器类
    private partial class ButtonContainer : HBoxContainer
    {
        private List<Button> _buttons = new List<Button>();
        private DynamicDifficultyData.DifficultyLevel _currentLevel;

        public ButtonContainer()
        {
            Alignment = BoxContainer.AlignmentMode.Center;
            Spacing = 10;
            
            // 创建5个难度按钮
            for (int i = 0; i < 5; i++)
            {
                Button btn = new Button();
                btn.Text = DynamicDifficultyDatabase.GetDifficultyName((DynamicDifficultyData.DifficultyLevel)i);
                btn.CustomMinimumSize = new Vector2(80, 35);
                
                int level = i;
                btn.Pressed += () => OnButtonPressed((DynamicDifficultyData.DifficultyLevel)level);
                
                AddChild(btn);
                _buttons.Add(btn);
            }
            
            UpdateButtons(DynamicDifficultyData.DifficultyLevel.Normal);
        }

        private void OnButtonPressed(DynamicDifficultyData.DifficultyLevel level)
        {
            if (DynamicDifficultySystem.Instance != null)
            {
                DynamicDifficultySystem.Instance.SetDifficulty(level);
            }
        }

        public void UpdateButtons(DynamicDifficultyData.DifficultyLevel current)
        {
            _currentLevel = current;
            
            for (int i = 0; i < _buttons.Count; i++)
            {
                Color bgColor = DynamicDifficultyDatabase.GetDifficultyColor((DynamicDifficultyData.DifficultyLevel)i);
                
                StyleBoxFlat style = new StyleBoxFlat();
                
                if (i == (int)current)
                {
                    style.BgColor = bgColor;
                    style.CornerRadiusTopLeft = 5;
                    style.CornerRadiusTopRight = 5;
                    style.CornerRadiusBottomLeft = 5;
                    style.CornerRadiusBottomRight = 5;
                }
                else
                {
                    style.BgColor = new Color(0.2f, 0.2f, 0.25f);
                    style.CornerRadiusTopLeft = 5;
                    style.CornerRadiusTopRight = 5;
                    style.CornerRadiusBottomLeft = 5;
                    style.CornerRadiusBottomRight = 5;
                }
                
                _buttons[i].AddThemeStyleboxOverride("normal", style);
                
                // 选中状态
                StyleBoxFlat pressedStyle = style.Duplicate() as StyleBoxFlat;
                pressedStyle.BgColor = bgColor * 0.8f;
                _buttons[i].AddThemeStyleboxOverride("pressed", pressedStyle);
                
                // 悬停状态
                StyleBoxFlat hoverStyle = style.Duplicate() as StyleBoxFlat;
                hoverStyle.BgColor = bgColor * 1.2f;
                _buttons[i].AddThemeStyleboxOverride("hover", hoverStyle);
            }
        }
    }
}
