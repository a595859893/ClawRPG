using Godot;
/// <summary>
/// 元素共鸣用户界面。
/// </summary>
using System;
using System.Collections.Generic;

public partial class ElementalResonanceUI : Control
{
    private Control container;
    private Label titleLabel;
    private Label infoLabel;
    private VBoxContainer resonanceContainer;
    private Button closeButton;

    private bool isVisible = false;

    public override void _Ready()
    {
        Visible = false;
        SetupUI();
    }

    private void SetupUI()
    {
        // Background panel
        ColorRect bg = new ColorRect();
        bg.Color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        bg.SetAnchorPreset(LayoutPreset.Center);
        bg.CustomMinimumSize = new Vector2(600, 500);
        AddChild(bg);

        // Title
        titleLabel = new Label();
        titleLabel.Text = "⚡ 元素共鸣系统 ⚡";
        titleLabel.SetAnchorPreset(LayoutPreset.TopWide);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleLabel.Position = new Vector2(0, 20);
        bg.AddChild(titleLabel);

        // Info label
        infoLabel = new Label();
        infoLabel.Text = "当多个元素同时作用于敌人时触发共鸣效果";
        infoLabel.SetAnchorPreset(LayoutPreset.TopWide);
        infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        infoLabel.AddThemeFontSizeOverride("font_size", 14);
        infoLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        infoLabel.Position = new Vector2(0, 60);
        bg.AddChild(infoLabel);

        // Resonance container
        resonanceContainer = new VBoxContainer();
        resonanceContainer.SetAnchorPreset(LayoutPreset.FullRect);
        resonanceContainer.Position = new Vector2(20, 100);
        resonanceContainer.CustomMinimumSize = new Vector2(560, 340);
        bg.AddChild(resonanceContainer);

        // Add resonance info
        AddResonanceInfo();

        // Close button
        closeButton = new Button();
        closeButton.Text = "关闭 (Esc)";
        closeButton.SetAnchorPreset(LayoutPreset.BottomWide);
        closeButton.Position = new Vector2(0, -50);
        closeButton.CustomMinimumSize = new Vector2(0, 40);
        closeButton.Pressed += () => ToggleUI();
        bg.AddChild(closeButton);
    }

    private void AddResonanceInfo()
    {
        var resonances = new[]
        {
            ("烈焰爆发", "火+火", "2.0x 伤害", new Color(1f, 0.3f, 0f)),
            ("蒸汽爆炸", "火+水", "1.8x 伤害", new Color(0.8f, 0.8f, 0.9f)),
            ("融化", "火+冰", "1.7x 伤害", new Color(1f, 0.5f, 0f)),
            ("熔岩", "火+土", "2.1x 伤害", new Color(0.9f, 0.2f, 0f)),
            ("冰冻", "冰+水", "1.8x 伤害", new Color(0.5f, 0.8f, 1f)),
            ("暴风雪", "冰+雷/风", "1.9x 伤害", new Color(0.7f, 0.9f, 1f)),
            ("电击", "雷+水", "1.7x 伤害", new Color(1f, 1f, 0.2f)),
            ("雷暴", "雷+风", "2.0x 伤害", new Color(0.7f, 0.7f, 1f)),
            ("虚空", "暗+火", "2.2x 伤害", new Color(0.3f, 0f, 0.5f)),
            ("审判", "暗+圣/火+圣", "2.3x 伤害", new Color(1f, 0.9f, 0.3f)),
            ("圣光", "圣+水", "1.6x 伤害", new Color(1f, 1f, 0.8f)),
            ("自然", "土+水", "1.5x 伤害", new Color(0.2f, 0.8f, 0.2f)),
            ("剧毒", "毒+土/风", "1.8x 伤害", new Color(0.4f, 0.8f, 0.2f)),
            ("腐蚀", "暗+毒", "1.7x 伤害", new Color(0.5f, 0f, 0.5f)),
            ("混沌", "三元素", "2.5x 伤害", new Color(0.8f, 0f, 0.8f)),
        };

        foreach (var (name, combo, damage, color) in resonances)
        {
            HBoxContainer row = new HBoxContainer();
            row.CustomMinimumSize = new Vector2(0, 35);
            resonanceContainer.AddChild(row);

            Label nameLabel = new Label();
            nameLabel.Text = name;
            nameLabel.Modulate = color;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.CustomMinimumSize = new Vector2(120, 0);
            row.AddChild(nameLabel);

            Label comboLabel = new Label();
            comboLabel.Text = combo;
            comboLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
            comboLabel.AddThemeFontSizeOverride("font_size", 14);
            comboLabel.CustomMinimumSize = new Vector2(100, 0);
            row.AddChild(comboLabel);

            Label damageLabel = new Label();
            damageLabel.Text = damage;
            damageLabel.Modulate = new Color(1f, 0.8f, 0.3f);
            damageLabel.AddThemeFontSizeOverride("font_size", 14);
            row.AddChild(damageLabel);
        }
    }

    public void ToggleUI()
    {
        isVisible = !isVisible;
        Visible = isVisible;
        if (isVisible)
        {
            GetTree().CurrentScene.GetNode<CanvasLayer>("CanvasLayer").Show();
        }
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        {
            if (isVisible)
            {
                ToggleUI();
            }
        }
    }
}
