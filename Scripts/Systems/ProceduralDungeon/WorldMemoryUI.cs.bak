using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 世界记忆UI — 显示各房间类型的记忆状态
    ///
    /// 集成点：订阅 RoomMemorySystem.RoomMemoryUpdated 信号
    /// </summary>
    public partial class WorldMemoryUI : Control
    {
        private VBoxContainer _memoryContainer;
        private System.Collections.Generic.Dictionary<RoomType, MemoryEntryRow> _entryRows = new();
        private Label _titleLabel;
        private bool _isVisible = false;
        private bool _enabled = true;

        [Export] public bool Enabled { get => _enabled; set => _enabled = value; }

        /// <summary>
        /// 单个房间类型的记忆行
        /// </summary>
        private class MemoryEntryRow
        {
            public HBoxContainer Container;
            public Label TypeLabel;
            public TextureRect IconRect;
            public ProgressBar WeightBar;
            public Label WeightLabel;
            public Label CountLabel;
            public Tween AnimTween;
        }

        public override void _Ready()
        {
            // 初始化UI结构
            SetupUI();

            // 订阅信号
            if (RoomMemorySystem.Instance != null)
            {
                RoomMemorySystem.Instance.RoomMemoryUpdated += OnMemoryUpdated;
                RoomMemorySystem.Instance.RoomMemoryDecayed += OnMemoryDecayed;
            }

            // 初始隐藏
            Visible = false;
        }

        public override void _ExitTree()
        {
            if (RoomMemorySystem.Instance != null)
            {
                RoomMemorySystem.Instance.RoomMemoryUpdated -= OnMemoryUpdated;
                RoomMemorySystem.Instance.RoomMemoryDecayed -= OnMemoryDecayed;
            }

            foreach (var row in _entryRows.Values)
            {
                row.AnimTween?.Kill();
            }
        }

        private void SetupUI()
        {
            // 标题
            _titleLabel = new Label { Text = "World Memory", HorizontalAlignment = HorizontalAlignment.Center };
            AddChild(_titleLabel);

            // 主容器
            _memoryContainer = new VBoxContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            AddChild(_memoryContainer);

            // 初始化所有房间类型的行
            foreach (RoomType rt in Enum.GetValues(typeof(RoomType)))
            {
                if (rt == RoomType.Entrance || rt == RoomType.Corridor) continue;
                CreateRow(rt);
            }

            // 初始填充数据
            RefreshAllRows();
        }

        private void CreateRow(RoomType roomType)
        {
            var row = new MemoryEntryRow();

            // 容器
            row.Container = new HBoxContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            _memoryContainer.AddChild(row.Container);

            // 类型名称
            row.TypeLabel = new Label
            {
                Text = roomType.ToString(),
                SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
                CustomMinimumSize = new Vector2(80, 0)
            };
            row.Container.AddChild(row.TypeLabel);

            // 记忆条
            row.WeightBar = new ProgressBar
            {
                MinValue = 0,
                MaxValue = RoomMemoryConstants.MAX_WEIGHT,
                Value = 0,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                Step = 1
            };
            row.Container.AddChild(row.WeightBar);

            // 权重数值
            row.WeightLabel = new Label
            {
                Text = "0",
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
                CustomMinimumSize = new Vector2(30, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            row.Container.AddChild(row.WeightLabel);

            // 进入次数
            row.CountLabel = new Label
            {
                Text = "(0)",
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
                CustomMinimumSize = new Vector2(50, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            row.Container.AddChild(row.CountLabel);

            _entryRows[roomType] = row;

            // 根据房间类型设置条颜色
            UpdateRowColor(roomType, 0);
        }

        private void RefreshAllRows()
        {
            if (RoomMemorySystem.Instance == null) return;

            var allEntries = RoomMemorySystem.Instance.GetAllMemoryEntries();
            foreach (var rt in _entryRows.Keys)
            {
                int weight = RoomMemorySystem.Instance.GetMemoryWeight(rt);
                int count = allEntries.TryGetValue(rt, out var e) ? e.EntryCount : 0;
                UpdateRow(rt, weight, count);
            }
        }

        private void UpdateRow(RoomType roomType, int weight, int entryCount)
        {
            if (!_entryRows.TryGetValue(roomType, out var row)) return;

            // 更新数值
            row.WeightBar.Value = weight;
            row.WeightLabel.Text = $"{weight}";
            row.CountLabel.Text = $"({entryCount})";

            // 颜色渐变
            UpdateRowColor(roomType, weight);

            // 动画
            AnimateRowChange(row);
        }

        private void UpdateRowColor(RoomType roomType, int weight)
        {
            if (!_entryRows.TryGetValue(roomType, out var row)) return;

            // 颜色语义：
            // 0 = 蓝色（全新/遗忘）
            // 1-4 = 绿色（轻度熟悉）
            // 5-7 = 黄色（中等熟悉）
            // 8-10 = 红色/橙色（高度熟悉）
            Color barColor;
            if (weight == 0)
                barColor = new Color(0.4f, 0.6f, 1f);   // 蓝色
            else if (weight <= 4)
                barColor = new Color(0.3f, 0.8f, 0.4f);   // 绿色
            else if (weight <= 7)
                barColor = new Color(1f, 0.8f, 0.2f);     // 黄色
            else
                barColor = new Color(1f, 0.4f, 0.2f);     // 橙色/红色

            row.WeightBar.Modulate = barColor;
        }

        private void AnimateRowChange(MemoryEntryRow row)
        {
            row.AnimTween?.Kill();
            row.AnimTween = CreateTween();
            row.AnimTween.TweenProperty(row.Container, "modulate:a", 0.5f, 0.1f);
            row.AnimTween.TweenProperty(row.Container, "modulate:a", 1f, 0.2f);
        }

        private void OnMemoryUpdated(RoomType roomType, int newWeight)
        {
            if (!_enabled) return;
            if (!_entryRows.ContainsKey(roomType)) return;

            // 从数据库直接获取完整条目
            var allEntries = RoomMemorySystem.Instance.GetAllMemoryEntries();
            int count = allEntries.TryGetValue(roomType, out var e) ? e.EntryCount : 0;

            UpdateRow(roomType, newWeight, count);
        }

        private void OnMemoryDecayed(RoomType roomType, int newWeight)
        {
            if (!_enabled) return;
            if (!_entryRows.ContainsKey(roomType)) return;

            var allEntries = RoomMemorySystem.Instance.GetAllMemoryEntries();
            int count = allEntries.TryGetValue(roomType, out var e) ? e.EntryCount : 0;

            UpdateRow(roomType, newWeight, count);

            // 遗忘时闪一下灰
            if (_entryRows.TryGetValue(roomType, out var row))
            {
                var originalColor = row.WeightBar.Modulate;
                var tween = CreateTween();
                tween.TweenProperty(row.WeightBar, "modulate", new Color(0.5f, 0.5f, 0.5f), 0.3f);
                tween.TweenProperty(row.WeightBar, "modulate", originalColor, 0.5f);
            }
        }

        /// <summary>
        /// 切换显示/隐藏
        /// </summary>
        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public void ShowMemory()
        {
            Visible = true;
            _isVisible = true;
        }

        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void HideMemory()
        {
            Visible = false;
            _isVisible = false;
        }

        public override void _Input(InputEvent @event)
        {
            // 按 M 键切换记忆UI
            if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.M)
            {
                ToggleVisibility();
            }
        }
    }
}
