using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 坐骑战斗UI - 显示坐骑技能和战斗状态
    /// </summary>
    public class MountCombatUI : Control {
        public static MountCombatUI Instance { get; private set; }

        private PanelContainer _mainPanel;
        private VBoxContainer _skillsContainer;
        private Label _mountNameLabel;
        private Label _combatStatsLabel;
        private HBoxContainer _skillButtonsContainer;
        
        private bool _isVisible = false; 
        private string _currentMountId = null;
        private List<MountCombatData.MountCombatSkill> _currentSkills = new List<MountCombatData.MountCombatSkill>();
        private List<TextureRect> _skillIcons = new List<TextureRect>();
        private List<Label> _cooldownLabels = new List<Label>();

        public override void _Ready() {
            Instance = this;
            SetupUI();
            Visible = false; 
            
            // 连接到坐骑管理器信号
            if (MountManager.Instance != null) {
                MountManager.Instance.Connect(nameof(MountManager.OnMountActivated), this, nameof(_OnMountActivated));
                MountManager.Instance.Connect(nameof(MountManager.OnMountDeactivated), this, nameof(_OnMountDeactivated));
            }
            
            // 连接到战斗系统信号
            if (MountCombatSystem.Instance != null) {
                MountCombatSystem.Instance.Connect(nameof(MountCombatSystem.OnMountSkillUsed), this, nameof(_OnSkillUsed));
                MountCombatSystem.Instance.Connect(nameof(MountCombatSystem.OnMountSkillReady), this, nameof(_OnSkillReady));
                MountCombatSystem.Instance.Connect(nameof(MountCombatSystem.OnMountCombatStart), this, nameof(_OnCombatStart));
                MountCombatSystem.Instance.Connect(nameof(MountCombatSystem.OnMountCombatEnd), this, nameof(_OnCombatEnd));
            }
            
            GD.Print("[MountCombatUI] Initialized");
        }

        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI() {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchor(AnchorPresets.BottomRight);
            _mainPanel.MarginLeft = -320;
            _mainPanel.MarginTop = -220;
            _mainPanel.MarginRight = -20;
            _mainPanel.MarginBottom = -20;
            AddChild(_mainPanel);

            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            panelStyle.CornerRadiusTopLeft = 8;
            panelStyle.CornerRadiusTopRight = 8;
            panelStyle.CornerRadiusBottomLeft = 8;
            panelStyle.CornerRadiusBottomRight = 8;
            panelStyle.BorderWidthLeft = 2;
            panelStyle.BorderWidthTop = 2;
            panelStyle.BorderWidthRight = 2;
            panelStyle.BorderWidthBottom = 2;
            panelStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
            _mainPanel.AddStyleboxOverride("panel", panelStyle);

            var mainVBox = new VBoxContainer();
            mainVBox.AddConstantOverride("separation", 8);
            _mainPanel.AddChild(mainVBox);

            // 坐骑名称标签
            _mountNameLabel = new Label();
            _mountNameLabel.Text = "坐骑战斗";
            _mountNameLabel.Align = Label.AlignEnum.Center;
            _mountNameLabel.AddColorOverride("font_color", new Color(0.9f, 0.8f, 0.4f, 1.0f));
            mainVBox.AddChild(_mountNameLabel);

            // 战斗统计标签
            _combatStatsLabel = new Label();
            _combatStatsLabel.Text = "准备就绪";
            _combatStatsLabel.Align = Label.AlignEnum.Center;
            _combatStatsLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1.0f));
            _combatStatsLabel.RectMinHeight = 30;
            mainVBox.AddChild(_combatStatsLabel);

            // 技能按钮容器
            _skillButtonsContainer = new HBoxContainer();
            _skillButtonsContainer.Alignment = BoxContainer.AlignmentMode.Center;
            _skillButtonsContainer.AddConstantOverride("separation", 10);
            mainVBox.AddChild(_skillButtonsContainer);

            // 提示标签
            var hintLabel = new Label();
            hintLabel.Text = "点击技能按钮或按 1-9 释放技能";
            hintLabel.Align = Label.AlignEnum.Center;
            hintLabel.AddColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f, 1.0f));
            hintLabel.RectMinHeight = 20;
            mainVBox.AddChild(hintLabel);
        }

        /// <summary>
        /// 创建技能按钮
        /// </summary>
        private void CreateSkillButtons(List<MountCombatData.MountCombatSkill> skills) {
            // 清理旧按钮
            foreach (var icon in _skillIcons) {
                icon.QueueFree();
            }
            _skillIcons.Clear();
            
            foreach (var label in _cooldownLabels) {
                label.QueueFree();
            }
            _cooldownLabels.Clear();
            
            // 创建新按钮
            for (int i = 0; i < skills.Count; i++) {
                var skill = skills[i];
                
                var buttonContainer = new Control();
                buttonContainer.RectMinSize = new Vector2(60, 60);
                _skillButtonsContainer.AddChild(buttonContainer);
                
                // 技能图标背景
                var iconBg = new TextureRect();
                iconBg.RectMinSize = new Vector2(56, 56);
                iconBg.Modulate = GetSkillTypeColor(skill.SkillType);
                buttonContainer.AddChild(iconBg);
                
                var bgStyle = new StyleBoxFlat();
                bgStyle.BgColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
                bgStyle.CornerRadiusTopLeft = 8;
                bgStyle.CornerRadiusTopRight = 8;
                bgStyle.CornerRadiusBottomLeft = 8;
                bgStyle.CornerRadiusBottomRight = 8;
                bgStyle.BorderWidthLeft = 2;
                bgStyle.BorderWidthTop = 2;
                bgStyle.BorderWidthRight = 2;
                bgStyle.BorderWidthBottom = 2;
                bgStyle.BorderColor = GetSkillTypeColor(skill.SkillType);
                iconBg.AddStyleboxOverride("normal", bgStyle);
                
                // 技能名称
                var skillLabel = new Label();
                skillLabel.Text = skill.Name;
                skillLabel.Align = Label.AlignEnum.Center;
                skillLabel.AddColorOverride("font_color", Colors.White);
                skillLabel.RectMinSize = new Vector2(56, 20);
                skillLabel.RectPosition = new Vector2(0, 18);
                iconBg.AddChild(skillLabel);
                
                // 快捷键提示
                var keyLabel = new Label();
                keyLabel.Text = (i + 1).ToString();
                keyLabel.Align = Label.AlignEnum.Center;
                keyLabel.AddColorOverride("font_color", new Color(1f, 1f, 0.5f, 1f));
                keyLabel.RectMinSize = new Vector2(56, 16);
                keyLabel.RectPosition = new Vector2(0, 40);
                iconBg.AddChild(keyLabel);
                
                // 冷却标签
                var cooldownLabel = new Label();
                cooldownLabel.Text = "";
                cooldownLabel.Align = Label.AlignEnum.Center;
                cooldownLabel.AddColorOverride("font_color", new Color(1f, 0.3f, 0.3f, 1f));
                cooldownLabel.RectMinSize = new Vector2(56, 16);
                cooldownLabel.RectPosition = new Vector2(0, 0);
                iconBg.AddChild(cooldownLabel);
                _cooldownLabels.Add(cooldownLabel);
                
                // 保存引用
                _skillIcons.Add(iconBg);
                
                // 绑定点击事件
                iconBg.Connect("gui_input", this, nameof(_OnSkillButtonPressed), new Array { i });
            }
        }

        /// <summary>
        /// 获取技能类型颜色
        /// </summary>
        private Color GetSkillTypeColor(MountCombatData.MountSkillType skillType) {
            switch (skillType) {
                case MountCombatData.MountSkillType.Charge:
                    return new Color(1f, 0.6f, 0.2f, 1f); // 橙色
                case MountCombatData.MountSkillType.Slam:
                    return new Color(0.8f, 0.3f, 0.3f, 1f); // 红色
                case MountCombatData.MountSkillType.Sweep:
                    return new Color(0.9f, 0.4f, 0.1f, 1f); // 深橙
                case MountCombatData.MountSkillType.Trample:
                    return new Color(0.6f, 0.4f, 0.2f, 1f); // 棕色
                case MountCombatData.MountSkillType.Roar:
                    return new Color(0.5f, 0.5f, 1f, 1f); // 蓝色
                case MountCombatData.MountSkillType.Shield:
                    return new Color(0.3f, 0.8f, 0.3f, 1f); // 绿色
                case MountCombatData.MountSkillType.Dash:
                    return new Color(0.4f, 0.8f, 1f, 1f); // 浅蓝
                case MountCombatData.MountSkillType.Bleed:
                    return new Color(0.7f, 0.2f, 0.2f, 1f); // 深红
                case MountCombatData.MountSkillType.Burn:
                    return new Color(1f, 0.4f, 0.1f, 1f); // 火红
                case MountCombatData.MountSkillType.Freeze:
                    return new Color(0.6f, 0.9f, 1f, 1f); // 冰蓝
                default:
                    return new Color(0.7f, 0.7f, 0.7f, 1f); // 灰色
            }
        }

        /// <summary>
        /// 技能按钮点击
        /// </summary>
        private void _OnSkillButtonPressed(InputEvent @event, int skillIndex) {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == Button.Left) {
                UseSkill(skillIndex);
            }
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        public void UseSkill(int skillIndex) {
            if (skillIndex < 0 || skillIndex >= _currentSkills.Count) return;
            
            var skill = _currentSkills[skillIndex];
            var player = GetTree().CurrentScene.GetNodeOrNull<Player>("../Player");
            
            if (player == null) return;
            
            // 获取攻击方向
            Vector2 direction = player.GetAimDirection();
            
            // 使用技能
            MountCombatSystem.Instance.UseMountSkill(_currentMountId, skill.Id, player.GlobalPosition + direction * 100f, player);
        }

        /// <summary>
        /// 切换显示
        /// </summary>
        public void Toggle() {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible) {
                RefreshSkills();
            }
        }

        /// <summary>
        /// 刷新技能显示
        /// </summary>
        public void RefreshSkills() {
            _currentMountId = MountManager.Instance.GetActiveMountId();
            
            if (_currentMountId == null) {
                _mountNameLabel.Text = "未骑乘坐骑";
                _currentSkills.Clear();
                return;
            }
            
            // 获取坐骑名称
            var mountData = MountDatabase.Instance.GetMount(_currentMountId);
            _mountNameLabel.Text = mountData != null ? mountData.Name + " 战斗技能" : "坐骑战斗";
            
            // 获取技能
            _currentSkills = MountCombatSystem.Instance.GetUnlockedSkills(_currentMountId);
            
            // 创建技能按钮
            CreateSkillButtons(_currentSkills);
            
            // 更新冷却显示
            UpdateCooldownDisplay();
        }

        /// <summary>
        /// 更新冷却显示
        /// </summary>
        public void UpdateCooldownDisplay() {
            if (_currentMountId == null) return;
            
            for (int i = 0; i < _currentSkills.Count && i < _cooldownLabels.Count; i++) {
                var skill = _currentSkills[i];
                int cooldown = MountCombatSystem.Instance.GetSkillCooldown(_currentMountId, skill.Id);
                
                if (cooldown > 0) {
                    _cooldownLabels[i].Text = cooldown.ToString();
                    _skillIcons[i].Modulate = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                } else {
                    _cooldownLabels[i].Text = "";
                    _skillIcons[i].Modulate = Colors.White;
                }
            }
        }

        /// <summary>
        /// 更新战斗统计显示
        /// </summary>
        public void UpdateCombatStats() {
            if (_currentMountId == null) {
                _combatStatsLabel.Text = "准备就绪";
                return;
            }
            
            var instance = MountCombatSystem.Instance.GetMountCombatInstance(_currentMountId);
            if (instance == null || !instance.IsInCombat) {
                _combatStatsLabel.Text = "准备就绪";
                return;
            }
            
            _combatStatsLabel.Text = $"伤害: {instance.CombatDamageDealt} | 击杀: {instance.CombatKills}";
        }

        public override void _Process(float delta) {
            if (Visible && _currentMountId != null) {
                UpdateCooldownDisplay();
                UpdateCombatStats();
            }
        }

        /// <summary>
        /// 输入处理
        /// </summary>
        public override void _Input(InputEvent @event) {
            if (!Visible) return;
            
            // 数字键使用技能
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                int skillIndex = -1;
                
                if (keyEvent.Scancode == Godot.KeyList.Key1) skillIndex = 0;
                else if (keyEvent.Scancode == Godot.KeyList.Key2) skillIndex = 1;
                else if (keyEvent.Scancode == Godot.KeyList.Key3) skillIndex = 2;
                else if (keyEvent.Scancode == Godot.KeyList.Key4) skillIndex = 3;
                else if (keyEvent.Scancode == Godot.KeyList.Key5) skillIndex = 4;
                else if (keyEvent.Scancode == Godot.KeyList.Key6) skillIndex = 5;
                else if (keyEvent.Scancode == Godot.KeyList.Key7) skillIndex = 6;
                else if (keyEvent.Scancode == Godot.KeyList.Key8) skillIndex = 7;
                else if (keyEvent.Scancode == Godot.KeyList.Key9) skillIndex = 8;
                
                if (skillIndex >= 0) {
                    UseSkill(skillIndex);
                    GetTree().SetInputAsHandled();
                }
            }
        }

        /// <summary>
        /// 坐骑激活回调
        /// </summary>
        private void _OnMountActivated(string mountId) {
            if (MountCombatSystem.Instance != null) {
                var instance = MountCombatSystem.Instance.GetMountCombatInstance(mountId);
                if (instance == null) {
                    var mountInstance = MountManager.Instance.GetOwnedMounts()[mountId];
                    MountCombatSystem.Instance.InitializeMountCombat(mountId, mountInstance.Level);
                }
            }
            
            RefreshSkills();
            
            // 自动显示UI
            if (MountCombatSystem.Instance.HasCombatAbility()) {
                Visible = true;
                _isVisible = true;
            }
        }

        /// <summary>
        /// 坐骑取消激活回调
        /// </summary>
        private void _OnMountDeactivated() {
            Visible = false; 
            _isVisible = false; 
            _currentMountId = null;
            _currentSkills.Clear();
        }

        /// <summary>
        /// 技能使用回调
        /// </summary>
        private void _OnSkillUsed(string mountId, string skillId, Vector2 targetPosition) {
            // 播放音效
            if (SoundEffectSystem.Instance != null) {
                SoundEffectSystem.Instance.PlayCombatSound("ability");
            }
        }

        /// <summary>
        /// 技能冷却完成回调
        /// </summary>
        private void _OnSkillReady(string mountId, string skillId) {
            // 可以添加视觉提示
        }

        /// <summary>
        /// 战斗开始回调
        /// </summary>
        private void _OnCombatStart(string mountId) {
            UpdateCombatStats();
        }

        /// <summary>
        /// 战斗结束回调
        /// </summary>
        private void _OnCombatEnd(string mountId, int damageDealt, int damageTaken, int kills) {
            _combatStatsLabel.Text = $"战斗结束 - 伤害: {damageDealt} | 击杀: {kills}";
        }
    }
}
