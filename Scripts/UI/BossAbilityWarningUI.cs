using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Boss技能预警UI系统 - 显示Boss即将释放的技能预警
    /// </summary>
    public partial class BossAbilityWarningUI : Control
    {
        // 技能预警数据结构
        private class AbilityWarning
        {
            public string AbilityId { get; set; }
            public string AbilityName { get; set; }
            public string Description { get; set; }
            public float WarningTime { get; set; }
            public float RemainingTime { get; set; }
            public Vector2 Position { get; set; }
            public bool IsAoE { get; set; }
            public float AoERadius { get; set; }
            public AbilityWarningStatus Status { get; set; }
        }

        private enum AbilityWarningStatus
        {
            Warning,
            Active,
            Expired
        }

        // UI Components
        private VBoxContainer _warningContainer;
        private Dictionary<string, Control> _activeWarnings;

        // State
        private int _maxWarnings = 3;
        private float _defaultWarningTime = 2f;
        private bool _isEnabled = true;

        // Colors
        private Color _warningBorderColor = new Color(1f, 0.6f, 0f, 1f);
        private Color _warningBgColor = new Color(0.1f, 0.05f, 0f, 0.85f);
        private Color _activeBorderColor = new Color(1f, 0.2f, 0.2f, 1f);
        private Color _activeBgColor = new Color(0.2f, 0.05f, 0.05f, 0.9f);
        private Color _aoeIndicatorColor = new Color(1f, 0.3f, 0.3f, 0.8f);

        // Localization
        private Dictionary<string, string> _abilityNameCN = new Dictionary<string, string>
        {
            { "fire_breath", "🔥 火焰吐息" },
            { "lightning_chain", "⚡ 闪电链" },
            { "poison_cloud", "☠️ 毒云" },
            { "ice_lance", "❄️ 寒冰长矛" },
            { "shadow_bolt", "🔮 暗影箭" },
            { "ground_slam", "💥 地震猛击" },
            { "fear_roar", "😱 恐惧咆哮" },
            { "blood_ripple", "🩸 鲜血波纹" },
            { "arcane_missile", "✨ 奥术飞弹" },
            { "self_heal", "💚 自我治疗" },
            { "teleport", "🌀 闪现" },
            { "summon_minions", "👹 召唤小怪" }
        };

        private Dictionary<string, string> _abilityDescCN = new Dictionary<string, string>
        {
            { "fire_breath", "危险！前方扇形区域将受到火焰伤害" },
            { "lightning_chain", "危险！闪电将连锁攻击多个目标" },
            { "poison_cloud", "警告！毒云将持续造成伤害" },
            { "ice_lance", "危险！寒冰攻击即将到来" },
            { "shadow_bolt", "警告！暗影能量正在聚集" },
            { "ground_slam", "危险！地面攻击即将触发" },
            { "fear_roar", "警告！Boss即将发出恐惧咆哮" },
            { "blood_ripple", "危险！鲜血能量正在爆发" },
            { "arcane_missile", "警告！奥术飞弹正在瞄准" },
            { "self_heal", "注意！Boss正在治疗自身" },
            { "teleport", "警告！Boss即将闪现" },
            { "summon_minions", "警告！Boss正在召唤援军" }
        };

        public override void _Ready()
        {
            _activeWarnings = new Dictionary<string, Control>();
            SetupUI();
        }

        private void SetupUI()
        {
            // Main warning container - top right corner
            _warningContainer = new VBoxContainer();
            _warningContainer.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _warningContainer.Position = new Vector2(-220, 100);
            _warningContainer.Size = new Vector2(200, 300);
            _warningContainer.AddThemeConstantOverride("separation", 8);
            AddChild(_warningContainer);
        }

        /// <summary>
        /// 显示技能预警
        /// </summary>
        public void ShowAbilityWarning(string abilityId, Vector2 position, float warningTime = 2f, bool isAoE = false, float aoeRadius = 0f)
        {
            if (!_isEnabled) return;
            if (_activeWarnings.ContainsKey(abilityId)) return;
            if (_activeWarnings.Count >= _maxWarnings) return;

            string abilityName = _abilityNameCN.GetValueOrDefault(abilityId, abilityId);
            string description = _abilityDescCN.GetValueOrDefault(abilityId, "危险技能即将释放");

            var warning = new AbilityWarning
            {
                AbilityId = abilityId,
                AbilityName = abilityName,
                Description = description,
                WarningTime = warningTime,
                RemainingTime = warningTime,
                Position = position,
                IsAoE = isAoE,
                AoERadius = aoeRadius,
                Status = AbilityWarningStatus.Warning
            };

            CreateWarningUI(warning);
        }

        /// <summary>
        /// 创建预警UI
        /// </summary>
        private void CreateWarningUI(AbilityWarning warning)
        {
            var panel = new PanelContainer();
            panel.SetAnchorsPreset(Control.LayoutPreset.Wide);
            panel.CustomMinimumSize = new Vector2(0, 60);

            // StyleBox
            var style = new StyleBoxFlat();
            style.BgColor = _warningBgColor;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = _warningBorderColor;
            style.CornerRadiusTopLeft = 6;
            style.CornerRadiusTopRight = 6;
            style.CornerRadiusBottomLeft = 6;
            style.CornerRadiusBottomRight = 6;
            panel.AddThemeStyleboxOverride("panel", style);

            // VBox content
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 4);
            panel.AddChild(vbox);

            // Ability name
            var nameLabel = new Label();
            nameLabel.Text = warning.AbilityName;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.4f, 1f));
            vbox.AddChild(nameLabel);

            // Description
            var descLabel = new Label();
            descLabel.Text = warning.Description;
            descLabel.HorizontalAlignment = HorizontalAlignment.Center;
            descLabel.AutowrapMode = TextServer.AwrapMode.Word;
            descLabel.CustomMinimumSize = new Vector2(180, 0);
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 0.9f));
            vbox.AddChild(descLabel);

            // Timer bar
            var timerBar = new ProgressBar();
            timerBar.CustomMinimumSize = new Vector2(180, 8);
            timerBar.Value = 100;
            timerBar.MaxValue = 100;
            
            var timerStyle = new StyleBoxFlat();
            timerStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            timerStyle.CornerRadiusTopLeft = 4;
            timerStyle.CornerRadiusTopRight = 4;
            timerStyle.CornerRadiusBottomLeft = 4;
            timerStyle.CornerRadiusBottomRight = 4;
            timerBar.AddThemeStyleboxOverride("background", timerStyle);

            var fillStyle = new StyleBoxFlat();
            fillStyle.BgColor = _warningBorderColor;
            fillStyle.CornerRadiusTopLeft = 4;
            fillStyle.CornerRadiusTopRight = 4;
            fillStyle.CornerRadiusBottomLeft = 4;
            fillStyle.CornerRadiusBottomRight = 4;
            timerBar.AddThemeStyleboxOverride("fill", fillStyle);
            
            vbox.AddChild(timerBar);

            // AoE indicator
            if (warning.IsAoE && warning.AoERadius > 0)
            {
                var aoeLabel = new Label();
                aoeLabel.Text = $"⚠️ 范围: {warning.AoERadius:F0}px";
                aoeLabel.HorizontalAlignment = HorizontalAlignment.Center;
                aoeLabel.AddThemeFontSizeOverride("font_size", 11);
                aoeLabel.AddThemeColorOverride("font_color", _aoeIndicatorColor);
                vbox.AddChild(aoeLabel);
            }

            // Store reference
            panel.SetMeta("warning", warning);
            panel.SetMeta("timer_bar", timerBar);
            panel.SetMeta("name_label", nameLabel);
            panel.SetMeta("style", style);

            _warningContainer.AddChild(panel);
            _activeWarnings[warning.AbilityId] = panel;

            // Start countdown
            StartWarningCountdown(warning, panel, timerBar, style, nameLabel);
        }

        /// <summary>
        /// 开始预警倒计时
        /// </summary>
        private async void StartWarningCountdown(AbilityWarning warning, Control panel, ProgressBar timerBar, StyleBoxFlat style, Label nameLabel)
        {
            float delta = 0.1f;
            
            while (warning.RemainingTime > 0 && warning.Status != AbilityWarningStatus.Expired)
            {
                await ToSignal(GetTree().CreateTimer(delta), "timeout");
                
                if (!_isEnabled || !IsInstanceValid(panel)) return;
                
                warning.RemainingTime -= delta;
                
                // Update timer bar
                float percent = (warning.RemainingTime / warning.WarningTime) * 100;
                timerBar.Value = percent;

                // Change style when about to expire (last 0.5s)
                if (warning.RemainingTime <= 0.5f && warning.Status == AbilityWarningStatus.Warning)
                {
                    warning.Status = AbilityWarningStatus.Active;
                    style.BorderColor = _activeBorderColor;
                    style.BgColor = _activeBgColor;
                    nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.4f, 1f));
                }
            }

            // Remove warning
            if (IsInstanceValid(panel))
            {
                panel.QueueFree();
            }
            
            _activeWarnings.Remove(warning.AbilityId);
        }

        /// <summary>
        /// 清除所有预警
        /// </summary>
        public void ClearAllWarnings()
        {
            foreach (var kvp in _activeWarnings)
            {
                if (IsInstanceValid(kvp.Value))
                {
                    kvp.Value.QueueFree();
                }
            }
            _activeWarnings.Clear();
        }

        /// <summary>
        /// 设置是否启用
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
            Visible = enabled;
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled() => _isEnabled;

        /// <summary>
        /// 设置预警时间
        /// </summary>
        public void SetDefaultWarningTime(float time)
        {
            _defaultWarningTime = Mathf.Max(0.5f, time);
        }

        /// <summary>
        /// 获取当前预警数量
        /// </summary>
        public int GetWarningCount() => _activeWarnings.Count;
    }
}
