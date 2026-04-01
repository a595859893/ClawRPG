using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Skills;
using ClawRPG.Scripts.Systems.Pets;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Tooltip System - Displays detailed information on mouse hover
    /// </summary>
    public partial class TooltipSystem : Control
    {
        public static TooltipSystem Instance { get; private set; }

        [Export] private Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        [Export] private Color titleColor = new Color(1f, 0.84f, 0f, 1f);
        [Export] private Color textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [Export] private Color rarityCommon = new Color(0.6f, 0.6f, 0.6f, 1f);
        [Export] private Color rarityUncommon = new Color(0.2f, 0.8f, 0.2f, 1f);
        [Export] private Color rarityRare = new Color(0.2f, 0.5f, 1f, 1f);
        [Export] private Color rarityEpic = new Color(0.6f, 0.2f, 0.8f, 1f);
        [Export] private Color rarityLegendary = new Color(1f, 0.5f, 0f, 1f);

        private PanelContainer panel;
        private VBoxContainer contentBox;
        private Label titleLabel;
        private Label typeLabel;
        private Label descriptionLabel;
        private Label statsLabel;
        private Label rarityLabel;

        private Item currentItem;
        private Skill currentSkill;
        private bool isVisible = false; 
        private float showDelay = 0.3f;
        private float showTimer = 0f;

        public override void _Ready()
        {
            Instance = this;
            SetupUI();
            Hide();
        }

        private void SetupUI()
        {
            panel = new PanelContainer
            {
                Name = "TooltipPanel",
                ZIndex = 1000,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            var style = new StyleBoxFlat();
            style.BgColor = backgroundColor;
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f, 1f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            style.ContentMarginLeft = 15f;
            style.ContentMarginRight = 15f;
            style.ContentMarginTop = 12f;
            style.ContentMarginBottom = 12f;
            panel.AddThemeStyleboxOverride("panel", style);

            contentBox = new VBoxContainer();
            contentBox.AddThemeConstantOverride("separation", 5);

            titleLabel = new Label();
            titleLabel.AddThemeFontSizeOverride("font_size", 18);
            titleLabel.AddThemeColorOverride("font_color", titleColor);

            typeLabel = new Label();
            typeLabel.AddThemeFontSizeOverride("font_size", 12);
            typeLabel.AddThemeColorOverride("font_color", textColor);

            descriptionLabel = new Label();
            descriptionLabel.AddThemeFontSizeOverride("font_size", 14);
            descriptionLabel.AddThemeColorOverride("font_color", textColor);
            descriptionLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            descriptionLabel.CustomMinimumSize = new Vector2(250, 0);

            statsLabel = new Label();
            statsLabel.AddThemeFontSizeOverride("font_size", 13);
            statsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 0.7f, 1f));

            rarityLabel = new Label();
            rarityLabel.AddThemeFontSizeOverride("font_size", 11);
            rarityLabel.HorizontalAlignment = HorizontalAlignment.Right;

            contentBox.AddChild(titleLabel);
            contentBox.AddChild(typeLabel);
            contentBox.AddChild(new HSeparator());
            contentBox.AddChild(descriptionLabel);
            contentBox.AddChild(statsLabel);
            contentBox.AddChild(rarityLabel);

            panel.AddChild(contentBox);
            AddChild(panel);
        }

        public override void _Process(double delta)
        {
            if (!isVisible && (currentItem != null || currentSkill != null))
            {
                showTimer += (float)delta;
                if (showTimer >= showDelay)
                {
                    ShowTooltip();
                }
            }

            if (isVisible && Input.GetMouseButton_mask() == 0)
            {
                UpdatePosition();
            }
        }

        public void ShowItemTooltip(Item item)
        {
            ClearTooltip();
            currentItem = item;
            showTimer = 0f;
            Hide();
        }

        public void ShowSkillTooltip(Skill skill)
        {
            ClearTooltip();
            currentSkill = skill;
            showTimer = 0f;
            Hide();
        }

        public void ClearTooltip()
        {
            currentItem = null;
            currentSkill = null;
            showTimer = 0f;
            Hide();
            isVisible = false; 
        }

        private void ShowTooltip()
        {
            if (currentItem != null)
            {
                DisplayItemInfo();
            }
            else if (currentSkill != null)
            {
                DisplaySkillInfo();
            }

            UpdatePosition();
            Show();
            isVisible = true;
        }

        private void DisplayItemInfo()
        {
            titleLabel.Text = currentItem.Name;
            
            string typeStr = currentItem.Type.ToString();
            if (currentItem is Weapon w)
                typeStr += $" - {w.WeaponType}";
            else if (currentItem is Armor a)
                typeStr += $" - {a.ArmorSlot}";
            typeLabel.Text = typeStr;

            descriptionLabel.Text = currentItem.Description;

            // Display stats
            string stats = "";
            if (currentItem is Weapon weapon)
            {
                if (weapon.Damage > 0) stats += $"⚔️ 伤害: {weapon.Damage}\n";
                if (weapon.CritChance > 0) stats += $"🎯 暴击: {weapon.CritChance}%\n";
                if (weapon.AttackSpeed > 0) stats += $"⚡ 攻速: {weapon.AttackSpeed}\n";
            }
            else if (currentItem is Armor armor)
            {
                if (armor.Defense > 0) stats += $"🛡️ 防御: {armor.Defense}\n";
                if (armor.Health > 0) stats += $"❤️ 生命: {armor.Health}\n";
                if (armor.Stamina > 0) stats += $"💪 体力: {armor.Stamina}\n";
            }
            else if (currentItem is Consumable cons)
            {
                if (cons.HealthRestore > 0) stats += $"❤️ 恢复生命: {cons.HealthRestore}\n";
                if (cons.ManaRestore > 0) stats += $"💙 恢复法力: {cons.ManaRestore}\n";
            }
            statsLabel.Text = stats;

            // Rarity color
            rarityLabel.Text = currentItem.Rarity.ToString().ToUpper();
            rarityLabel.AddThemeColorOverride("font_color", GetRarityColor(currentItem.Rarity));
        }

        private void DisplaySkillInfo()
        {
            titleLabel.Text = currentSkill.Name;
            typeLabel.Text = $"技能 - {currentSkill.SkillType}";
            descriptionLabel.Text = currentSkill.Description;

            string stats = "";
            if (currentSkill.Damage > 0) stats += $"⚔️ 伤害: {currentSkill.Damage}\n";
            if (currentSkill.HealAmount > 0) stats += $"💚 治疗: {currentSkill.HealAmount}\n";
            if (currentSkill.ManaCost > 0) stats += $"💙 法力消耗: {currentSkill.ManaCost}\n";
            if (currentSkill.Cooldown > 0) stats += $"⏱️ 冷却: {currentSkill.Cooldown}秒\n";
            if (currentSkill.Range > 0) stats += $"📏 范围: {currentSkill.Range}\n";
            if (currentSkill.Duration > 0) stats += $"⏲️ 持续: {currentSkill.Duration}秒\n";
            statsLabel.Text = stats;

            rarityLabel.Text = currentSkill.SkillType.ToString().ToUpper();
            rarityLabel.AddThemeColorOverride("font_color", GetSkillTypeColor(currentSkill.SkillType));
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => rarityCommon,
                ItemRarity.Uncommon => rarityUncommon,
                ItemRarity.Rare => rarityRare,
                ItemRarity.Epic => rarityEpic,
                ItemRarity.Legendary => rarityLegendary,
                _ => textColor
            };
        }

        private Color GetSkillTypeColor(SkillType type)
        {
            return type switch
            {
                SkillType.Attack => new Color(1f, 0.3f, 0.3f, 1f),
                SkillType.Heal => new Color(0.3f, 1f, 0.3f, 1f),
                SkillType.Buff => new Color(0.3f, 0.7f, 1f, 1f),
                SkillType.Debuff => new Color(0.7f, 0.3f, 0.7f, 1f),
                _ => textColor
            };
        }

        private void UpdatePosition()
        {
            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector2 tooltipSize = panel.Size;
            
            Vector2 newPos = mousePos + new Vector2(20, 20);
            
            // Keep on screen
            var viewportSize = GetViewport().GetVisibleRect().Size;
            if (newPos.x + tooltipSize.x > viewportSize.x)
                newPos.x = mousePos.x - tooltipSize.x - 10;
            if (newPos.y + tooltipSize.y > viewportSize.y)
                newPos.y = mousePos.y - tooltipSize.y - 10;
            
            panel.Position = newPos;
        }
    }
}
