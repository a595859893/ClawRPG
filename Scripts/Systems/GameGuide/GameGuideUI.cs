using System;
using System.Collections.Generic;
using Godot;

public class GameGuideUI : Control
{
    private VBoxContainer _mainContainer;
    private HBoxContainer _categoryList;
    private VBoxContainer _guideList;
    private RichTextLabel _guideContent;
    private Label _titleLabel;
    private Label _progressLabel;
    
    private string _currentCategory = "getting_started";
    
    public override void _Ready()
    {
        GameGuideSystem.Instance.Initialize();
        SetupUI();
        RefreshCategories();
        RefreshGuides();
    }
    
    private void SetupUI()
    {
        // 主容器
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(_mainContainer);
        
        // 标题栏
        var header = new HBoxContainer();
        header.AddThemeConstantOverride("separation", 20);
        _mainContainer.AddChild(header);
        
        var title = new Label();
        title.Text = " 📖 Game Guide";
        title.AddThemeFontSizeOverride("font_size", 24);
        header.AddChild(title);
        
        header.AddChild(new Control()); // Spacer
        
        _progressLabel = new Label();
        _progressLabel.Text = "0/0 Guides Read";
        header.AddChild(_progressLabel);
        
        // 分割容器
        var splitContainer = new HSplitContainer();
        splitContainer.SetHExpandFlags(Control.ExpandLayout.Fill);
        splitContainer.SplitOffset = 200;
        _mainContainer.AddChild(splitContainer);
        
        // 类别列表
        _categoryList = new VBoxContainer();
        _categoryList.AddThemeConstantOverride("separation", 5);
        splitContainer.AddChild(_categoryList);
        
        // 指南列表和内容区域
        var rightPanel = new VBoxContainer();
        rightPanel.AddThemeConstantOverride("separation", 10);
        splitContainer.AddChild(rightPanel);
        
        // 指南列表标题
        var guideListLabel = new Label();
        guideListLabel.Text = "Guides:";
        guideListLabel.AddThemeFontSizeOverride("font_size", 16);
        rightPanel.AddChild(guideListLabel);
        
        // 指南列表
        _guideList = new VBoxContainer();
        _guideList.AddThemeConstantOverride("separation", 5);
        _guideList.CustomMinimumSize = new Vector2(0, 150);
        rightPanel.AddChild(_guideList);
        
        // 指南内容区域
        var contentLabel = new Label();
        contentLabel.Text = "Guide Content:";
        contentLabel.AddThemeFontSizeOverride("font_size", 16);
        rightPanel.AddChild(contentLabel);
        
        _guideContent = new RichTextLabel();
        _guideContent.BbcodeEnabled = true;
        _guideContent.SetVExpandFlags(Control.ExpandLayout.Fill);
        _guideContent.Modulate = new Color(0.9f, 0.9f, 0.9f);
        rightPanel.AddChild(_guideContent);
        
        // 更新进度显示
        UpdateProgress();
    }
    
    private void RefreshCategories()
    {
        // 清除现有项
        foreach (var child in _categoryList.GetChildren())
        {
            child.QueueFree();
        }
        
        var categories = GameGuideSystem.Instance.GetUnlockedCategories();
        
        foreach (var category in categories)
        {
            var btn = new Button();
            btn.Text = $" {category.Name}";
            btn.TooltipText = category.Description;
            btn.Pressed += () => OnCategorySelected(category.Id);
            
            if (category.Id == _currentCategory)
            {
                btn.AddThemeColorOverride("font_color", new Color(0.3f, 0.7f, 1f));
            }
            
            _categoryList.AddChild(btn);
        }
    }
    
    private void RefreshGuides()
    {
        // 清除现有项
        foreach (var child in _guideList.GetChildren())
        {
            child.QueueFree();
        }
        
        var guides = GameGuideSystem.Instance.GetGuidesByCategory(_currentCategory);
        
        foreach (var guide in guides)
        {
            var btn = new Button();
            var readStatus = GameGuideSystem.Instance.IsGuideRead(guide.Id) ? "✓" : "○";
            btn.Text = $"{readStatus} {guide.Title}";
            btn.Pressed += () => OnGuideSelected(guide.Id);
            
            if (GameGuideSystem.Instance.IsGuideRead(guide.Id))
            {
                btn.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            }
            
            _guideList.AddChild(btn);
        }
        
        // 如果有未读的指南，自动显示第一个
        if (guides.Count > 0 && !GameGuideSystem.Instance.IsGuideRead(guides[0].Id))
        {
            OnGuideSelected(guides[0].Id);
        }
        else if (guides.Count > 0)
        {
            OnGuideSelected(guides[0].Id);
        }
    }
    
    private void OnCategorySelected(string categoryId)
    {
        _currentCategory = categoryId;
        RefreshCategories();
        RefreshGuides();
    }
    
    private void OnGuideSelected(string guideId)
    {
        var guide = GameGuideSystem.Instance.GetGuide(guideId);
        if (guide == null) return;
        
        // 标记为已读
        GameGuideSystem.Instance.ReadGuide(guideId);
        
        // 更新内容
        var content = $"[b]{guide.Title}[/b]\n\n";
        content += guide.Content;
        content += $"\n\n[color=gray](Reading time: ~{guide.ReadTime} seconds)[/color]";
        
        _guideContent.Text = content;
        
        // 刷新列表显示已读状态
        RefreshGuides();
        UpdateProgress();
    }
    
    private void UpdateProgress()
    {
        var read = GameGuideSystem.Instance.GetReadGuidesCount();
        var total = GameGuideSystem.Instance.GetTotalGuidesCount();
        _progressLabel.Text = $"{read}/{total} Guides Read";
    }
    
    public static void ToggleUI()
    {
        var ui = GetExistingUI();
        if (ui != null)
        {
            ui.QueueFree();
        }
        else
        {
            ShowUI();
        }
    }
    
    public static void ShowUI()
    {
        var ui = new GameGuideUI();
        ui.Name = "GameGuideUI";
        
        var canvas = GetTree().Root.GetNode<Control>("CanvasLayer");
        if (canvas == null)
        {
            canvas = new Control();
            canvas.Name = "CanvasLayer";
            GetTree().Root.AddChild(canvas);
        }
        
        canvas.AddChild(ui);
    }
    
    private static GameGuideUI GetExistingUI()
    {
        var canvas = GetTree().Root.GetNode<Control>("CanvasLayer");
        if (canvas == null) return null;
        return canvas.GetNode<GameGuideUI>("GameGuideUI");
    }
    
    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("ui_cancel"))
        {
            QueueFree();
        }
    }
}
