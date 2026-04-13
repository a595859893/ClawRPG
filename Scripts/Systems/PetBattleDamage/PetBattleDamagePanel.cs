using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetBattleDamage
{
    /// <summary>
    /// 宠物战损面板 — 在宠物面板中显示战损痕迹列表
    /// REQ-186 视觉反馈层
    /// </summary>
    public partial class PetBattleDamagePanel : Control
    {
        private VBoxContainer _content;
        private Label _titleLabel;
        private ScrollContainer _scrollContainer;
        private VBoxContainer _marksContainer;
        private Label _emptyLabel;

        private int _currentPetId = -1;

        public override void _Ready()
        {
            // 设置面板基本属性
            CustomMinimumSize = new Vector2(300, 200);

            var theme = new Theme();
            var labelStyle = new StyleBoxFlat { BgColor = new Color(0.15f, 0.12f, 0.18f, 0.95f) };
            theme.SetStyleBox("normal", "Label", labelStyle);

            // 标题
            _titleLabel = new Label { Text = "战损记录", Theme = theme };
            _titleLabel.CustomMinimumSize = new Vector2(0, 24);
            AddChild(_titleLabel);

            // 滚动容器
            _scrollContainer = new ScrollContainer
            {
                SizeFlagsVertical = SizeFlags.Fill,
                SizeFlagsHorizontal = SizeFlags.Fill
            };
            _scrollContainer.CustomMinimumSize = new Vector2(0, 160);
            AddChild(_scrollContainer);

            _marksContainer = new VBoxContainer { Name = "MarksContainer" };
            _scrollContainer.AddChild(_marksContainer);

            // 空状态标签
            _emptyLabel = new Label
            {
                Text = "暂无战损记录",
                Theme = theme,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _emptyLabel.CustomMinimumSize = new Vector2(0, 80);
            _marksContainer.AddChild(_emptyLabel);

            // 订阅信号
            if (PetBattleDamageSystem.Instance != null)
            {
                PetBattleDamageSystem.Instance.OnDamageMarkAdded += OnDamageMarkAdded;
                PetBattleDamageSystem.Instance.OnDamageMarksCleared += OnDamageMarksCleared;
                PetBattleDamageSystem.Instance.OnVisualDamageLevelChanged += OnVisualLevelChanged;
            }

            Hide();
        }

        /// <summary>
        /// 显示指定宠物的战损面板
        /// </summary>
        public void ShowForPet(int petId)
        {
            _currentPetId = petId;
            RefreshDisplay();
            Show();
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void RefreshDisplay()
        {
            // 清除现有项
            foreach (var child in _marksContainer.GetChildren())
            {
                if (child is Label or ScrollContainer)
                    continue;
                child.QueueFree();
            }

            if (_currentPetId < 0)
            {
                _emptyLabel.Show();
                return;
            }

            var marks = PetBattleDamageSystem.Instance.GetDamageMarks(_currentPetId);

            if (marks.Count == 0)
            {
                _emptyLabel.Show();
                return;
            }

            _emptyLabel.Hide();

            // 显示所有痕迹
            foreach (var mark in marks)
            {
                var markLabel = CreateMarkLabel(mark);
                _marksContainer.AddChild(markLabel);
            }
        }

        private Label CreateMarkLabel(DamageMarkEntry mark)
        {
            string icon = GetMarkIcon(mark.MarkType);
            string timeStr = GetRelativeTime(mark.RecordTimestamp);
            string damageStr = mark.DamagePercent > 0 ? $" ({mark.DamagePercent:P0})" : "";

            var label = new Label
            {
                Text = $"{icon} {mark.MarkType}{damageStr}\n  {timeStr}",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            label.CustomMinimumSize = new Vector2(0, 40);
            return label;
        }

        private string GetMarkIcon(DamageMarkType type)
        {
            return type switch
            {
                DamageMarkType.Bandage => "🩹",
                DamageMarkType.Cut => "🩸",
                DamageMarkType.Scar => "⚔️",
                _ => "•"
            };
        }

        private string GetRelativeTime(long timestamp)
        {
            try
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                var span = DateTimeOffset.UtcNow - dt;

                if (span.TotalMinutes < 1)
                    return "刚刚";
                if (span.TotalHours < 1)
                    return $"{(int)span.TotalMinutes}分钟前";
                if (span.TotalDays < 1)
                    return $"{(int)span.TotalHours}小时前";
                if (span.TotalDays < 7)
                    return $"{(int)span.TotalDays}天前";
                return dt.ToString("yyyy-MM-dd");
            }
            catch
            {
                return "未知时间";
            }
        }

        private void OnDamageMarkAdded(int petId, DamageMarkType markType)
        {
            if (petId == _currentPetId)
                RefreshDisplay();
        }

        private void OnDamageMarksCleared(int petId)
        {
            if (petId == _currentPetId)
                RefreshDisplay();
        }

        private void OnVisualLevelChanged(int petId, DamageMarkType newLevel)
        {
            if (petId == _currentPetId)
                RefreshDisplay();
        }

        public override void _ExitTree()
        {
            if (PetBattleDamageSystem.Instance != null)
            {
                PetBattleDamageSystem.Instance.OnDamageMarkAdded -= OnDamageMarkAdded;
                PetBattleDamageSystem.Instance.OnDamageMarksCleared -= OnDamageMarksCleared;
                PetBattleDamageSystem.Instance.OnVisualDamageLevelChanged -= OnVisualLevelChanged;
            }
            base._ExitTree();
        }
    }
}
