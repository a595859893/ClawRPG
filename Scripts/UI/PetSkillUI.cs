using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 宠物技能界面
    /// </summary>
    public partial class PetSkillUI : Control
    {
        [Export] private Control _skillListContainer;
        [Export] private Label _skillPointsLabel;
        [Export] private Button _closeButton;
        
        // 技能详情面板
        [Export] private Label _skillNameLabel;
        [Export] private Label _skillDescriptionLabel;
        [Export] private Label _skillTypeLabel;
        [Export] private Label _skillRarityLabel;
        [Export] private Label _skillCooldownLabel;
        [Export] private Label _skillEffectLabel;
        [Export] private Label _skillCostLabel;
        [Export] private Button _learnButton;
        
        // 宠物选择
        [Export] private OptionButton _petSelector;
        
        private ClawRPG.Scripts.Systems.Pets.PetSkillSystem _skillSystem;
        private ClawRPG.Scripts.Systems.Pets.PetManager _petManager;
        private ClawRPG.Scripts.Systems.Pets.Pet _selectedPet;
        private List<ClawRPG.Scripts.Systems.Pets.PetSkill> _displayedSkills = new List<ClawRPG.Scripts.Systems.Pets.PetSkill>();
        private bool _isVisible = false; 

        public override void _Ready()
        {
            _skillSystem = ClawRPG.Scripts.Systems.Pets.PetSkillSystem.Instance;
            _petManager = ClawRPG.Scripts.Systems.Pets.PetManager.Instance;
            
            // 连接信号
            if (_closeButton != null)
                _closeButton.Pressed += OnClosePressed;
            
            if (_learnButton != null)
                _learnButton.Pressed += OnLearnPressed;
            
            if (_petSelector != null)
                _petSelector.ItemSelected += OnPetSelected;
            
            // 初始加载
            RefreshPetList();
            RefreshSkillList();
            
            // 初始隐藏
            if (Visible)
                Visible = false; 
                
            GD.Print("宠物技能界面已加载");
        }

        public override void _Process(double delta)
        {
            // 更新冷却显示
            if (_selectedPet != null && _skillSystem != null)
            {
                _skillSystem.UpdateCooldowns(delta, _selectedPet.PetId);
            }
        }

        private void RefreshPetList()
        {
            if (_petSelector == null) return;
            
            _petSelector.Clear();
            var pets = _petManager.GetAllPets();
            
            for (int i = 0; i < pets.Count; i++)
            {
                var pet = pets[i];
                _petSelector.AddItem($"{pet.PetName} (Lv.{pet.Level})", i);
            }
            
            if (pets.Count > 0)
            {
                _selectedPet = pets[0];
                RefreshSkillList();
            }
        }

        private void RefreshSkillList()
        {
            if (_skillListContainer == null || _selectedPet == null) return;
            
            // 清除旧技能列表
            foreach (Node child in _skillListContainer.GetChildren())
            {
                child.QueueFree();
            }
            _displayedSkills.Clear();
            
            // 获取可学习技能
            var availableSkills = PetSkillDatabase.GetAvailableSkills(_selectedPet.Level);
            var learnedSkills = _skillSystem.GetLearnedSkills(_selectedPet.PetId);
            
            // 显示所有可用技能
            foreach (var skill in availableSkills)
            {
                _displayedSkills.Add(skill);
                CreateSkillButton(skill, learnedSkills.Exists(s => s.SkillId == skill.SkillId));
            }
            
            // 更新技能点显示
            UpdateSkillPointsDisplay();
        }

        private void CreateSkillButton(ClawRPG.Scripts.Systems.Pets.PetSkill skill, bool isLearned)
        {
            if (_skillListContainer == null) return;
            
            // 创建技能按钮
            var button = new Button();
            button.Text = skill.SkillName;
            button.CustomMinimumSize = new Vector2(0, 40);
            
            // 设置稀有度颜色
            var color = GetRarityColor(skill.Rarity);
            button.Modulate = color;
            
            // 禁用已学习的技能显示
            if (isLearned)
            {
                button.Text += " ✓";
                button.Disabled = true;
            }
            else
            {
                button.Text += $" ({skill.SkillPointCost}点)";
            }
            
            button.Pressed += () => OnSkillButtonPressed(skill);
            
            _skillListContainer.AddChild(button);
        }

        private void OnSkillButtonPressed(ClawRPG.Scripts.Systems.Pets.PetSkill skill)
        {
            if (_skillNameLabel != null)
                _skillNameLabel.Text = skill.SkillName;
            
            if (_skillDescriptionLabel != null)
                _skillDescriptionLabel.Text = skill.Description;
            
            if (_skillTypeLabel != null)
                _skillTypeLabel.Text = $"类型: {GetSkillTypeName(skill.Type)}";
            
            if (_skillRarityLabel != null)
                _skillRarityLabel.Text = $"稀有度: {GetRarityName(skill.Rarity)}";
            
            if (_skillCooldownLabel != null)
                _skillCooldownLabel.Text = $"冷却: {skill.Cooldown}秒";
            
            if (_skillEffectLabel != null)
                _skillEffectLabel.Text = GetSkillEffectText(skill);
            
            if (_skillCostLabel != null)
                _skillCostLabel.Text = $"消耗: {skill.SkillPointCost}技能点 (需{skill.RequiredLevel}级)";
            
            // 更新学习按钮
            if (_learnButton != null)
            {
                if (_skillSystem.IsSkillLearned(_selectedPet.PetId, skill.SkillId))
                {
                    _learnButton.Text = "已学习";
                    _learnButton.Disabled = true;
                }
                else if (_skillSystem.CanLearnSkill(_selectedPet.PetId, skill.SkillId, _selectedPet.Level))
                {
                    _learnButton.Text = "学习";
                    _learnButton.Disabled = false; 
                }
                else
                {
                    _learnButton.Text = "无法学习";
                    _learnButton.Disabled = true;
                }
            }
        }

        private void UpdateSkillPointsDisplay()
        {
            if (_skillPointsLabel == null || _selectedPet == null) return;
            
            var points = _skillSystem.GetSkillPoints(_selectedPet.PetId);
            _skillPointsLabel.Text = $"可用技能点: {points}";
        }

        private void OnLearnPressed()
        {
            if (_selectedPet == null) return;
            
            // 获取当前选中的技能
            var availableSkills = PetSkillDatabase.GetAvailableSkills(_selectedPet.Level);
            // 这里简化处理，实际应该记录选中技能
            // 从列表中找到第一个可学习的技能
            foreach (var skill in availableSkills)
            {
                if (!_skillSystem.IsSkillLearned(_selectedPet.PetId, skill.SkillId))
                {
                    if (_skillSystem.LearnSkill(_selectedPet.PetId, skill.SkillId, _selectedPet.Level))
                    {
                        GD.Print($"学习了技能: {skill.SkillName}");
                        RefreshSkillList();
                        return;
                    }
                }
            }
        }

        private void OnPetSelected(int index)
        {
            var pets = _petManager.GetAllPets();
            if (index >= 0 && index < pets.Count)
            {
                _selectedPet = pets[index];
                RefreshSkillList();
            }
        }

        private void OnClosePressed()
        {
            ToggleUI(false);
        }

        public void ToggleUI(bool show)
        {
            _isVisible = show;
            Visible = show;
            
            if (show)
            {
                RefreshPetList();
                RefreshSkillList();
            }
        }

        public void ToggleUI()
        {
            ToggleUI(!_isVisible);
        }

        #region 辅助方法

        private string GetSkillTypeName(PetSkillType type)
        {
            return type switch
            {
                PetSkillType.Attack => "攻击",
                PetSkillType.Defense => "防御",
                PetSkillType.Support => "辅助",
                PetSkillType.Heal => "治疗",
                PetSkillType.Debuff => "减益",
                _ => "未知"
            };
        }

        private string GetRarityName(PetSkillRarity rarity)
        {
            return rarity switch
            {
                PetSkillRarity.Common => "普通",
                PetSkillRarity.Uncommon => "优秀",
                PetSkillRarity.Rare => "稀有",
                PetSkillRarity.Epic => "史诗",
                PetSkillRarity.Legendary => "传说",
                _ => "未知"
            };
        }

        private Color GetRarityColor(PetSkillRarity rarity)
        {
            return rarity switch
            {
                PetSkillRarity.Common => Colors.White,
                PetSkillRarity.Uncommon => Colors.Green,
                PetSkillRarity.Rare => Colors.Cyan,
                PetSkillRarity.Epic => new Color(0.6f, 0.3f, 0.8f), // Purple
                PetSkillRarity.Legendary => new Color(1f, 0.6f, 0f), // Orange
                _ => Colors.White
            };
        }

        private string GetSkillEffectText(ClawRPG.Scripts.Systems.Pets.PetSkill skill)
        {
            var effects = new List<string>();
            
            if (skill.Damage > 0)
                effects.Add($"伤害: {skill.Damage}");
            if (skill.DamageMultiplier > 0)
                effects.Add($"伤害倍率: {skill.DamageMultiplier * 100}%");
            if (skill.HealAmount > 0)
                effects.Add($"治疗: {skill.HealAmount}");
            if (skill.HealPercent > 0)
                effects.Add($"治疗%: {skill.HealPercent * 100}%");
            if (skill.ShieldAmount > 0)
                effects.Add($"护盾: {skill.ShieldAmount}");
            if (skill.SlowAmount != 0)
                effects.Add($"减速: {Mathf.Abs(skill.SlowAmount) * 100}%");
            if (skill.StunDuration > 0)
                effects.Add($"眩晕: {skill.StunDuration}秒");
            if (skill.FreezeDuration > 0)
                effects.Add($"冰冻: {skill.FreezeDuration}秒");
            if (skill.BurnDamage > 0)
                effects.Add($"燃烧: {skill.BurnDamage}/秒");
            
            return effects.Count > 0 ? string.Join("\n", effects) : "无特殊效果";
        }

        #endregion
    }
}
