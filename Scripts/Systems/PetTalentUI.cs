using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物天赋界面 - 显示和管理宠物天赋的UI
/// </summary>
public class PetTalentUI : Control
{
    private PetSystem _petSystem;
    private Pet _selectedPet;
    
    private VBoxContainer _mainContainer;
    private HBoxContainer _petListContainer;
    private ItemList _petList;
    private VBoxContainer _talentContainer;
    private Label _talentPointsLabel;
    private Label _petInfoLabel;
    private Button _rerollButton;
    private Label _statsLabel;

    private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
    private Color _uncommonColor = new Color(0.2f, 0.8f, 0.2f);
    private Color _rareColor = new Color(0.2f, 0.5f, 1f);
    private Color _epicColor = new Color(0.6f, 0.3f, 0.9f);
    private Color _legendaryColor = new Color(1f, 0.6f, 0.1f);

    public override void _Ready()
    {
        Visible = false; 
        SetupUI();
        
        _petSystem = GetNode<PetSystem>("/root/Main/PetSystem");
    }

    private void SetupUI()
    {
        // 主容器
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(_mainContainer);

        // 标题
        Label title = new Label();
        title.Text = "  宠物天赋系统  ";
        title.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(title);

        // 宠物列表容器
        _petListContainer = new HBoxContainer();
        _petListContainer.AddThemeConstantOverride("separation", 20);
        _mainContainer.AddChild(_petListContainer);

        // 宠物列表
        _petList = new ItemList();
        _petList.CustomMinimumSize = new Vector2(200, 300);
        _petList.Connect("item_selected", this, nameof(_OnPetSelected));
        _petListContainer.AddChild(_petList);

        // 天赋容器
        _talentContainer = new VBoxContainer();
        _talentContainer.CustomMinimumSize = new Vector2(400, 300);
        _petListContainer.AddChild(_talentContainer);

        // 宠物信息
        _petInfoLabel = new Label();
        _petInfoLabel.Text = "选择一个宠物查看天赋";
        _talentContainer.AddChild(_petInfoLabel);

        // 天赋点数
        _talentPointsLabel = new Label();
        _talentPointsLabel.Text = "天赋点数: 0";
        _talentPointsLabel.AddThemeFontSizeOverride("font_size", 18);
        _talentContainer.AddChild(_talentPointsLabel);

        // 重置按钮
        _rerollButton = new Button();
        _rerollButton.Text = "重置天赋 (消耗1点)";
        _rerollButton.Connect("pressed", this, nameof(_OnRerollPressed));
        _talentContainer.AddChild(_rerollButton);

        // 属性加成显示
        Label statsTitle = new Label();
        statsTitle.Text = "属性加成:";
        statsTitle.AddThemeFontSizeOverride("font_size", 16);
        _talentContainer.AddChild(statsTitle);

        _statsLabel = new Label();
        _statsLabel.Text = "无";
        _statsLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        _talentContainer.AddChild(_statsLabel);

        // 天赋列表标题
        Label talentTitle = new Label();
        talentTitle.Text = "天赋列表:";
        talentTitle.AddThemeFontSizeOverride("font_size", 16);
        _talentContainer.AddChild(talentTitle);

        // 底部关闭按钮
        Button closeButton = new Button();
        closeButton.Text = "关闭 (P)";
        closeButton.Connect("pressed", this, nameof(_OnClosePressed));
        _mainContainer.AddChild(closeButton);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_cancel") || Input.IsActionJustPressed("ui_pet_talent"))
        {
            ToggleUI();
        }
    }

    public void ToggleUI()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshPetList();
        }
    }

    private void RefreshPetList()
    {
        _petList.Clear();
        
        if (_petSystem == null || _petSystem.PlayerPets == null)
            return;

        int index = 0;
        foreach (var pet in _petSystem.PlayerPets)
        {
            string displayName = $"{pet.PetName} (Lv.{pet.Level})";
            _petList.AddItem(displayName);
            index++;
        }

        if (_petList.GetItemCount() > 0)
        {
            _petList.Select(0);
            _OnPetSelected(0);
        }
    }

    private void _OnPetSelected(int index)
    {
        if (_petSystem == null || _petSystem.PlayerPets == null)
            return;

        if (index < 0 || index >= _petSystem.PlayerPets.Count)
            return;

        _selectedPet = _petSystem.PlayerPets[index];
        RefreshTalentDisplay();
    }

    private void RefreshTalentDisplay()
    {
        if (_selectedPet == null)
            return;

        string petId = _selectedPet.Id;
        
        // 更新宠物信息
        _petInfoLabel.Text = $"{_selectedPet.PetName} (Lv.{_selectedPet.Level}) - {_selectedPet.PetType}";

        // 更新天赋点数
        int points = PetTalentSystem.Instance.GetTalentPoints(petId);
        _talentPointsLabel.Text = $"天赋点数: {points}";

        // 更新属性加成
        var bonuses = PetTalentSystem.Instance.GetAllTalentBonuses(petId);
        string statsText = "";
        foreach (var kvp in bonuses)
        {
            if (kvp.Value > 0)
            {
                string statName = GetStatDisplayName(kvp.Key);
                statsText += $"{statName}: +{(kvp.Value * 100):F1}%\n";
            }
        }
        _statsLabel.Text = statsText.Length > 0 ? statsText : "无";

        // 更新天赋列表
        UpdateTalentList();
    }

    private void UpdateTalentList()
    {
        // 清除旧的天赋显示
        foreach (Node child in _talentContainer.GetChildren())
        {
            if (child is Label l && l.Name.StartsWith("Talent_"))
            {
                child.QueueFree();
            }
        }

        if (_selectedPet == null) return;

        var talents = PetTalentSystem.Instance.GetPetTalents(_selectedPet.Id);
        
        if (talents.Count == 0)
        {
            Label noTalents = new Label();
            noTalents.Name = "Talent_empty";
            noTalents.Text = "该宠物暂无天赋";
            noTalents.Modulate = new Color(0.5f, 0.5f, 0.5f);
            _talentContainer.AddChild(noTalents);
            return;
        }

        foreach (var talent in talents)
        {
            var talentData = PetTalentDatabase.Instance.GetTalent(talent.TalentId);
            if (talentData == null) continue;

            Label talentLabel = new Label();
            talentLabel.Name = $"Talent_{talent.TalentId}";
            
            string raritySymbol = GetRaritySymbol(talentData.Rarity);
            talentLabel.Text = $"{raritySymbol} {talentData.Name} Lv.{talent.Level}";
            talentLabel.Text += $"\n  {talentData.Description}";
            
            talentLabel.Modulate = GetRarityColor(talentData.Rarity);
            _talentContainer.AddChild(talentLabel);
        }
    }

    private string GetRaritySymbol(PetTalentData.TalentRarity rarity)
    {
        switch (rarity)
        {
            case PetTalentData.TalentRarity.Common: return "⚪";
            case PetTalentData.TalentRarity.Uncommon: return "🟢";
            case PetTalentData.TalentRarity.Rare: return "🔵";
            case PetTalentData.TalentRarity.Epic: return "🟣";
            case PetTalentData.TalentRarity.Legendary: return "🟠";
            default: return "⚪";
        }
    }

    private Color GetRarityColor(PetTalentData.TalentRarity rarity)
    {
        switch (rarity)
        {
            case PetTalentData.TalentRarity.Common: return _commonColor;
            case PetTalentData.TalentRarity.Uncommon: return _uncommonColor;
            case PetTalentData.TalentRarity.Rare: return _rareColor;
            case PetTalentData.TalentRarity.Epic: return _epicColor;
            case PetTalentData.TalentRarity.Legendary: return _legendaryColor;
            default: return _commonColor;
        }
    }

    private string GetStatDisplayName(string stat)
    {
        switch (stat)
        {
            case "attack": return "攻击力";
            case "defense": return "防御力";
            case "health": return "生命值";
            case "speed": return "移动速度";
            case "crit_rate": return "暴击率";
            case "crit_damage": return "暴击伤害";
            case "lifesteal": return "生命偷取";
            case "dodge": return "闪避率";
            case "tenacity": return "韧性";
            case "exp": return "经验获取";
            case "gold": return "金币获取";
            case "drop": return "物品掉落";
            default: return stat;
        }
    }

    private void _OnRerollPressed()
    {
        if (_selectedPet == null) return;

        string petId = _selectedPet.Id;
        int points = PetTalentSystem.Instance.GetTalentPoints(petId);
        
        if (points < 1)
        {
            GD.Print("[PetTalentUI] Not enough talent points!");
            return;
        }

        PetTalentSystem.Instance.RerollPetTalents(petId, 1);
        RefreshTalentDisplay();
    }

    private void _OnClosePressed()
    {
        Visible = false; 
    }
}
