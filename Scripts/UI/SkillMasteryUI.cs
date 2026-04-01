using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Skills;
using SkillMasterySystem = ClawRPG.Scripts.Skills.SkillMasterySystem;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Skill Mastery and Combo UI
    /// Displays skill mastery levels, combos, and rune management
    /// </summary>
    public partial class SkillMasteryUI : Control
    {
        private TabContainer _tabContainer;
        private VBoxContainer _masteryTab;
        private VBoxContainer _comboTab;
        private VBoxContainer _runeTab;
        
        // Mastery UI elements
        private ItemList _skillMasteryList;
        private Label _selectedSkillLabel;
        private Label _masteryLevelLabel;
        private Label _masteryXPLabel;
        private Label _masteryRankLabel;
        private ProgressBar _xpProgressBar;
        
        // Combo UI elements
        private ItemList _comboList;
        private Label _comboDetailLabel;
        private Label _comboProgressLabel;
        
        // Rune UI elements
        private ItemList _runeList;
        private OptionButton _runeSlotSelect;
        private Button _equipRuneButton;
        private Button _unequipRuneButton;
        
        private SkillMasterySystem _masterySystem;
        
        public override void _Ready()
        {
            _masterySystem = SkillMasterySystem.Instance;
            
            SetupUI();
            RefreshMasteryList();
            RefreshComboList();
            RefreshRuneList();
        }
        
        private void SetupUI()
        {
            // Main container
            var mainContainer = new HBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainContainer.AddThemeConstantOverride("separation", 20);
            AddChild(mainContainer);
            
            // Left panel - lists
            var leftPanel = new VBoxContainer();
            leftPanel.CustomMinimumSize = new Vector2(300, 0);
            mainContainer.AddChild(leftPanel);
            
            // Tab container
            _tabContainer = new TabContainer();
            _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
            mainContainer.AddChild(_tabContainer);
            
            // ===== MASTERY TAB =====
            _masteryTab = new VBoxContainer();
            _masteryTab.Name = "精通";
            _tabContainer.AddChild(_masteryTab);
            
            var masteryHeader = new Label();
            masteryHeader.Text = "技能精通";
            masteryHeader.AddThemeFontSizeOverride("font_size", 20);
            _masteryTab.AddChild(masteryHeader);
            
            _skillMasteryList = new ItemList();
            _skillMasteryList.SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
            _skillMasteryList.ItemSelected += OnMasterySkillSelected;
            _masteryTab.AddChild(_skillMasteryList);
            
            var masteryInfoPanel = new VBoxContainer();
            _masteryTab.AddChild(masteryInfoPanel);
            
            _selectedSkillLabel = new Label();
            _selectedSkillLabel.Text = "选择技能查看精通";
            masteryInfoPanel.AddChild(_selectedSkillLabel);
            
            _masteryLevelLabel = new Label();
            masteryInfoPanel.AddChild(_masteryLevelLabel);
            
            _masteryXPLabel = new Label();
            masteryInfoPanel.AddChild(_masteryXPLabel);
            
            _masteryRankLabel = new Label();
            masteryInfoPanel.AddChild(_masteryRankLabel);
            
            _xpProgressBar = new ProgressBar();
            _xpProgressBar.CustomMinimumSize = new Vector2(0, 20);
            masteryInfoPanel.AddChild(_xpProgressBar);
            
            // ===== COMBO TAB =====
            _comboTab = new VBoxContainer();
            _comboTab.Name = "连招";
            _tabContainer.AddChild(_comboTab);
            
            var comboHeader = new Label();
            comboHeader.Text = "技能连招";
            comboHeader.AddThemeFontSizeOverride("font_size", 20);
            _comboTab.AddChild(comboHeader);
            
            _comboList = new ItemList();
            _comboList.SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
            _comboList.ItemSelected += OnComboSelected;
            _comboTab.AddChild(_comboList);
            
            _comboDetailLabel = new Label();
            _comboDetailLabel.Text = "选择连招查看详情";
            _comboTab.AddChild(_comboDetailLabel);
            
            _comboProgressLabel = new Label();
            _comboTab.AddChild(_comboProgressLabel);
            
            // ===== RUNE TAB =====
            _runeTab = new VBoxContainer();
            _runeTab.Name = "符文";
            _tabContainer.AddChild(_runeTab);
            
            var runeHeader = new Label();
            runeHeader.Text = "技能符文";
            runeHeader.AddThemeFontSizeOverride("font_size", 20);
            _runeTab.AddChild(runeHeader);
            
            _runeList = new ItemList();
            _runeList.SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
            _runeList.ItemSelected += OnRuneSelected;
            _runeTab.AddChild(_runeList);
            
            var runeControls = new HBoxContainer();
            _runeTab.AddChild(runeControls);
            
            var slotLabel = new Label();
            slotLabel.Text = "插槽:";
            runeControls.AddChild(slotLabel);
            
            _runeSlotSelect = new OptionButton();
            _runeSlotSelect.AddItem("插槽 1", 0);
            _runeSlotSelect.AddItem("插槽 2", 1);
            _runeSlotSelect.AddItem("插槽 3", 2);
            runeControls.AddChild(_runeSlotSelect);
            
            _equipRuneButton = new Button();
            _equipRuneButton.Text = "装备";
            _equipRuneButton.Pressed += OnEquipRune;
            runeControls.AddChild(_equipRuneButton);
            
            _unequipRuneButton = new Button();
            _unequipRuneButton.Text = "卸下";
            _unequipRuneButton.Pressed += OnUnequipRune;
            runeControls.AddChild(_unequipRuneButton);
        }
        
        private void RefreshMasteryList()
        {
            _skillMasteryList.Clear();
            
            var skills = SkillDatabase.Instance.GetAllSkills();
            foreach (var skill in skills)
            {
                var mastery = _masterySystem.GetMastery(skill.Id);
                string displayText = $"{skill.Name} [Lv.{mastery.CurrentLevel} {mastery.Rank}]";
                _skillMasteryList.AddItem(displayText);
            }
        }
        
        private void RefreshComboList()
        {
            _comboList.Clear();
            
            var combos = _masterySystem.GetAllCombos();
            foreach (var combo in combos)
            {
                string status = _masterySystem.IsComboOnCooldown(combo.Id) ? " (冷却中)" : "";
                string displayText = $"{combo.Name}{status}";
                _comboList.AddItem(displayText);
            }
        }
        
        private void RefreshRuneList()
        {
            _runeList.Clear();
            
            // Group runes by type
            var types = new[] { RuneSlotType.Damage, RuneSlotType.Cooldown, RuneSlotType.Range, 
                               RuneSlotType.Duration, RuneSlotType.Cost };
            
            foreach (var type in types)
            {
                var runes = _masterySystem.GetRunesByType(type);
                foreach (var rune in runes)
                {
                    string rarityStars = new string('★', rune.Rarity);
                    string displayText = $"{rune.Name} {rarityStars} - {rune.Description}";
                    _runeList.AddItem(displayText);
                }
            }
        }
        
        private void OnMasterySkillSelected(long index)
        {
            var skills = SkillDatabase.Instance.GetAllSkills();
            if (index < 0 || index >= skills.Count) return;
            
            var skill = skills[(int)index];
            var mastery = _masterySystem.GetMastery(skill.Id);
            var bonuses = _masterySystem.GetTotalBonuses(skill.Id);
            
            _selectedSkillLabel.Text = $"技能: {skill.Name}";
            _masteryLevelLabel.Text = $"精通等级: {mastery.CurrentLevel}/10";
            _masteryXPLabel.Text = $"经验值: {mastery.CurrentXP} / {GetXPForNextLevel(mastery.CurrentLevel)}";
            _masteryRankLabel.Text = $"阶级: {mastery.Rank}";
            
            int[] xpThresholds = { 100, 500, 1500, 5000 };
            int maxXP = xpThresholds[Math.Min(mastery.CurrentLevel - 1, 3)];
            _xpProgressBar.MaxValue = maxXP;
            _xpProgressBar.Value = mastery.CurrentXP;
            
            // Show bonuses
            GD.Print($"技能 {skill.Name} 精通加成:");
            GD.Print($"  伤害: +{bonuses.damage*100:F1}%");
            GD.Print($"  冷却: -{bonuses.cdr*100:F1}%");
            GD.Print($"  范围: +{bonuses.range*100:F1}%");
            GD.Print($"  消耗: -{bonuses.cost*100:F1}%");
        }
        
        private int GetXPForNextLevel(int currentLevel)
        {
            int[] thresholds = { 100, 500, 1500, 5000 };
            int index = Math.Min(currentLevel - 1, 3);
            return thresholds[index];
        }
        
        private void OnComboSelected(long index)
        {
            var combos = _masterySystem.GetAllCombos();
            if (index < 0 || index >= combos.Count) return;
            
            var combo = combos[(int)index];
            
            string skillNames = "";
            foreach (var skillId in combo.SkillSequence)
            {
                var skill = SkillDatabase.Instance.GetSkill(skillId);
                if (skill != null)
                {
                    skillNames += skill.Name + " → ";
                }
            }
            skillNames = skillNames.TrimEnd(' ', '→');
            
            _comboDetailLabel.Text = $"连招: {combo.Name}\n" +
                $"类型: {combo.Type}\n" +
                $"技能: {skillNames}\n" +
                $"伤害倍率: {combo.DamageMultiplier}x\n" +
                $"冷却: {combo.Cooldown}秒\n" +
                $"需求等级: {combo.RequiredMasteryLevel}";
            
            int usageCount = SkillMasterySystem.Instance.GetSaveData().ComboUsages.ContainsKey(combo.Id) 
                ? SkillMasterySystem.Instance.GetSaveData().ComboUsages[combo.Id] 
                : 0;
            _comboProgressLabel.Text = $"使用次数: {usageCount}";
        }
        
        private void OnRuneSelected(long index)
        {
            // Rune selection for equipping
        }
        
        private void OnEquipRune()
        {
            if (_skillMasteryList.GetSelectedItems().Length == 0)
            {
                GD.Print("请先在精通页面选择技能");
                return;
            }
            
            var skills = SkillDatabase.Instance.GetAllSkills();
            int skillIndex = (int)_skillMasteryList.GetSelectedItems()[0];
            if (skillIndex >= skills.Count) return;
            
            int skillId = skills[skillIndex].Id;
            int slot = _runeSlotSelect.Selected;
            
            // Get selected rune from list
            var runes = _masterySystem.GetAllCombos(); // This is wrong, need to fix
            // For now, just show message
            GD.Print($"尝试装备符文到技能 {skillId} 插槽 {slot}");
        }
        
        private void OnUnequipRune()
        {
            if (_skillMasteryList.GetSelectedItems().Length == 0)
            {
                GD.Print("请先在精通页面选择技能");
                return;
            }
            
            var skills = SkillDatabase.Instance.GetAllSkills();
            int skillIndex = (int)_skillMasteryList.GetSelectedItems()[0];
            if (skillIndex >= skills.Count) return;
            
            int skillId = skills[skillIndex].Id;
            int slot = _runeSlotSelect.Selected;
            
            if (_masterySystem.UnequipRune(skillId, slot))
            {
                GD.Print($"已卸下技能 {skillId} 插槽 {slot} 的符文");
            }
        }
        
        public override void _Process(double delta)
        {
            // Update combo progress
            var activeCombo = _masterySystem.GetActiveCombo();
            if (activeCombo != null)
            {
                var combo = _masterySystem.GetCombo(activeCombo.ComboId);
                _comboProgressLabel.Text = $"连招进行中: {combo.Name}\n" +
                    $"进度: {activeCombo.CurrentStep + 1}/{combo.SkillSequence.Count}\n" +
                    $"时间: {activeCombo.TimeRemaining:F1}秒";
            }
            
            // Refresh cooldown status
            RefreshComboList();
        }
        
        /// <summary>
        /// Toggle UI visibility
        /// </summary>
        public void Toggle()
        {
            if (Visible)
                Hide();
            else
                Show();
        }
    }
}
