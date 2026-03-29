using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.UI;

/// <summary>
/// 宠物变异系统 UI
/// </summary>
public class PetMutationUI : Control
{
    private Label _titleLabel;
    private Label _statsLabel;
    private PetMutationTree _mutationTree;
    private PetMutationDetails _detailsPanel;
    private VBoxContainer _petListContainer;
    private OptionButton _petSelector;
    private Button _rerollButton;
    private Button _closeButton;
    
    private int _selectedPetId = -1;
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        SetupUI();
        SetupShortcuts();
        Hide();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainPanel = new PanelContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 100,
            OffsetTop = 50,
            OffsetRight = -100,
            OffsetBottom = -50
        };
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer { RectMinSize = new Vector2(0, 40) };
        mainPanel.AddChild(mainVBox);
        
        // 标题栏
        var titleBar = new HBoxContainer { RectMinSize = new Vector2(0, 50) };
        mainVBox.AddChild(titleBar);
        
        _titleLabel = new Label
        {
            Text = "  宠物变异系统",
            RectMinSize = new Vector2(200, 0),
            Align = Label.AlignEnum.Left
        };
        titleBar.AddChild(_titleLabel);
        
        titleBar.AddChild(new Control { HBoxExpand = true });
        
        _closeButton = new Button { Text = "✕ 关闭" };
        _closeButton.Pressed += OnClosePressed;
        titleBar.AddChild(_closeButton);
        
        // 主内容区
        var contentHBox = new HBoxContainer { SizeFlagsVertical = SizeFlags.Expand };
        mainVBox.AddChild(contentHBox);
        
        // 左侧 - 宠物选择和变异列表
        var leftPanel = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.Expand, SizeFlagsVertical = SizeFlags.Expand };
        contentHBox.AddChild(leftPanel);
        
        // 宠物选择器
        var selectorContainer = new HBoxContainer { RectMinSize = new Vector2(0, 40) };
        leftPanel.AddChild(selectorContainer);
        
        var selectorLabel = new Label { Text = "选择宠物:", RectMinSize = new Vector2(80, 0) };
        selectorContainer.AddChild(selectorLabel);
        
        _petSelector = new OptionButton();
        _petSelector.ItemSelected += OnPetSelected;
        selectorContainer.AddChild(_petSelector);
        
        // 变异树
        _mutationTree = new PetMutationTree { SizeFlagsHorizontal = SizeFlags.Expand, SizeFlagsVertical = SizeFlags.Expand };
        leftPanel.AddChild(_mutationTree);
        
        // 操作按钮
        var buttonContainer = new HBoxContainer { RectMinSize = new Vector2(0, 40) };
        leftPanel.AddChild(buttonContainer);
        
        var mutateButton = new Button { Text = "🔄 尝试变异 (100金币)" };
        mutateButton.Pressed += OnMutatePressed;
        buttonContainer.AddChild(mutateButton);
        
        _rerollButton = new Button { Text = "🎲 重新随机 (200金币)", Disabled = true };
        _rerollButton.Pressed += OnRerollPressed;
        buttonContainer.AddChild(_rerollButton);
        
        // 右侧 - 详情面板和统计
        var rightPanel = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.Expand, SizeFlagsVertical = SizeFlags.Expand, RectMinSize = new Vector2(300, 0) };
        contentHBox.AddChild(rightPanel);
        
        // 详情面板
        _detailsPanel = new PetMutationDetails { SizeFlagsVertical = SizeFlags.Expand };
        rightPanel.AddChild(_detailsPanel);
        
        // 统计面板
        var statsPanel = new PanelContainer { RectMinSize = new Vector2(0, 120) };
        rightPanel.AddChild(statsPanel);
        
        var statsVBox = new VBoxContainer();
        statsPanel.AddChild(statsVBox);
        
        var statsTitle = new Label { Text = "变异统计" };
        statsVBox.AddChild(statsTitle);
        
        _statsLabel = new Label { Text = "" };
        statsVBox.AddChild(_statsLabel);
        
        // 底部说明
        var tipLabel = new Label
        {
            Text = "提示: 变异概率随宠物等级提升，传说变异极为稀有",
            RectMinSize = new Vector2(0, 30),
            Align = Label.AlignEnum.Center
        };
        mainVBox.AddChild(tipLabel);
    }
    
    private void SetupShortcuts()
    {
        // 快捷键将在外层设置
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Scancode == KeyList.Escape)
            {
                HidePanel();
            }
        }
    }
    
    public void TogglePanel()
    {
        if (_isVisible)
            HidePanel();
        else
            ShowPanel();
    }
    
    public void ShowPanel()
    {
        RefreshPetList();
        RefreshStatistics();
        Show();
        _isVisible = true;
    }
    
    public void HidePanel()
    {
        Hide();
        _isVisible = false;
    }
    
    private void RefreshPetList()
    {
        _petSelector.Clear();
        
        // 从宠物系统获取宠物列表
        var petSystem = PetSystem.Instance;
        if (petSystem != null)
        {
            var pets = petSystem.GetAllPets();
            int index = 0;
            foreach (var pet in pets)
            {
                _petSelector.AddItem($"{pet.Name} (Lv.{pet.Level})", index);
                index++;
            }
        }
        
        if (_petSelector.GetItemCount() > 0)
        {
            _petSelector.Selected = 0;
            OnPetSelected(0);
        }
    }
    
    private void OnPetSelected(int index)
    {
        var petSystem = PetSystem.Instance;
        if (petSystem == null) return;
        
        var pets = petSystem.GetAllPets();
        if (index < 0 || index >= pets.Count) return;
        
        _selectedPetId = pets[index].Id;
        RefreshMutationList();
    }
    
    private void RefreshMutationList()
    {
        _mutationTree.Clear();
        
        if (_selectedPetId < 0) return;
        
        var mutations = PetMutationSystem.Instance.GetPetMutations(_selectedPetId);
        var mutationData = PetMutationSystem.Instance.GetPetMutationData(_selectedPetId);
        
        if (mutations.Count == 0)
        {
            _mutationTree.AddEmptyText("该宠物尚未发生变异");
            _detailsPanel.ClearDetails();
            _rerollButton.Disabled = true;
            return;
        }
        
        // 按稀有度分组显示
        var rarityOrder = new[] { "Legendary", "Epic", "Rare", "Uncommon", "Common" };
        
        foreach (var rarity in rarityOrder)
        {
            var rarityMutations = mutations.FindAll(m => m.Rarity == rarity);
            if (rarityMutations.Count == 0) continue;
            
            var groupItem = _mutationTree.AddGroup($"{rarity} ({rarityMutations.Count})");
            
            foreach (var mutation in rarityMutations)
            {
                var icon = GetRarityIcon(mutation.Rarity);
                var item = _mutationTree.AddMutationItem(groupItem, mutation.Name, icon);
                item.SetMetadata(0, mutation);
            }
        }
        
        _rerollButton.Disabled = mutations.Count == 0;
        
        // 显示属性加成
        var bonuses = PetMutationSystem.Instance.CalculateMutationBonuses(_selectedPetId);
        _detailsPanel.ShowBonuses(bonuses);
    }
    
    private string GetRarityIcon(string rarity)
    {
        return rarity switch
        {
            "Legendary" => "⭐",
            "Epic" => "💜",
            "Rare" => "💙",
            "Uncommon" => "💚",
            _ => "🤍"
        };
    }
    
    private void RefreshStatistics()
    {
        var stats = PetMutationSystem.Instance.GetStatistics();
        
        _statsLabel.Text = $"总变异尝试: {stats["total_attempts"]}\n" +
            $"成功变异: {stats["successful_mutations"]}\n" +
            $"成功率: {stats["success_rate"]:F1}%\n" +
            $"重新随机: {stats["rerolls_used"]}";
    }
    
    private void OnMutatePressed()
    {
        if (_selectedPetId < 0) return;
        
        var player = GetTree().CurrentScene.FindNode("Player") as Node2D;
        if (player == null) return;
        
        // 检查金币
        int cost = 100;
        // 这里应该从玩家数据获取金币
        // 简化处理，直接尝试变异
        
        var petSystem = PetSystem.Instance;
        var pets = petSystem.GetAllPets();
        var pet = pets.Find(p => p.Id == _selectedPetId);
        
        if (pet != null)
        {
            bool success = PetMutationSystem.Instance.TryMutatePet(pet.Id, pet.Level);
            
            if (success)
            {
                ShowNotification("变异成功！");
            }
            else
            {
                ShowNotification("变异失败，再接再厉！");
            }
            
            RefreshMutationList();
            RefreshStatistics();
        }
    }
    
    private void OnRerollPressed()
    {
        if (_selectedPetId < 0) return;
        
        // 选择第一个变异进行重新随机
        var mutations = PetMutationSystem.Instance.GetPetMutations(_selectedPetId);
        if (mutations.Count == 0) return;
        
        bool success = PetMutationSystem.Instance.RerollMutation(_selectedPetId, 0);
        
        if (success)
        {
            ShowNotification("重新随机成功！");
            RefreshMutationList();
            RefreshStatistics();
        }
    }
    
    private void OnClosePressed()
    {
        HidePanel();
    }
    
    private void ShowNotification(string text)
    {
        var notification = new Label
        {
            Text = text,
            RectMinSize = new Vector2(200, 40),
            Align = Label.AlignEnum.Center,
            Modulate = new Color(1, 1, 0)
        };
        AddChild(notification);
        
        var tween = CreateTween();
        tween.TweenProperty(notification, "rect_position:y", 100, 0.5f);
        tween.TweenInterval(1f);
        tween.TweenProperty(notification, "modulate:a", 0f, 0.5f);
        tween.TweenCallback(notification, "queue_free");
    }
}

/// <summary>
/// 变异树形视图
/// </summary>
public class PetMutationTree : VBoxContainer
{
    private Tree _tree;
    
    public PetMutationTree()
    {
        _tree = new Tree { SizeFlagsHorizontal = SizeFlags.Expand, SizeFlagsVertical = SizeFlags.Expand };
        AddChild(_tree);
    }
    
    public void Clear()
    {
        _tree.Clear();
    }
    
    public TreeItem AddGroup(string text)
    {
        var root = _tree.CreateItem();
        root.SetText(0, text);
        root.SetSelectable(0, false);
        return root;
    }
    
    public TreeItem AddEmptyText(string text)
    {
        var item = _tree.CreateItem();
        item.SetText(0, text);
        item.SetSelectable(0, false);
        return item;
    }
    
    public TreeItem AddMutationItem(TreeItem parent, string text, string icon)
    {
        var item = _tree.CreateItem(parent);
        item.SetText(0, $"{icon} {text}");
        return item;
    }
}

/// <summary>
/// 变异详情面板
/// </summary>
public class PetMutationDetails : PanelContainer
{
    private VBoxContainer _content;
    private Label _bonusesLabel;
    
    public PetMutationDetails()
    {
        var scroll = new ScrollContainer { SizeFlagsHorizontal = SizeFlags.Expand, SizeFlagsVertical = SizeFlags.Expand };
        AddChild(scroll);
        
        _content = new VBoxContainer();
        scroll.AddChild(_content);
        
        var title = new Label { Text = "属性加成" };
        _content.AddChild(title);
        
        _bonusesLabel = new Label { Text = "无变异加成" };
        _content.AddChild(_bonusesLabel);
    }
    
    public void ClearDetails()
    {
        _bonusesLabel.Text = "无变异加成";
    }
    
    public void ShowBonuses(Dictionary<string, float> bonuses)
    {
        if (bonuses.Count == 0)
        {
            _bonusesLabel.Text = "无变异加成";
            return;
        }
        
        var text = "";
        foreach (var kvp in bonuses)
        {
            var sign = kvp.Value >= 0 ? "+" : "";
            text += $"{kvp.Key}: {sign}{kvp.Value:F1}\n";
        }
        
        _bonusesLabel.Text = text;
    }
}
