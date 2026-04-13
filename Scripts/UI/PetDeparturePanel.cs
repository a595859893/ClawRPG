using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Systems.PetDeparture;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 宠物离队档案陈列室面板 — Safe House 内显示所有曾经并肩过的宠物档案
    /// REQ-189: 宠物离队时 Safe House 显示档案卡（宠物名、时间、常用技能）
    /// </summary>
    public partial class PetDeparturePanel : CanvasLayer
    {
        private PetDepartureSystem _system;
        private Panel _mainPanel;
        private VBoxContainer _content;
        private Label _titleLabel;
        private Label _emptyLabel;
        private ScrollContainer _scroll;
        private VBoxContainer _recordList;
        private Button _closeButton;
        private bool _visible = false;

        public override void _Ready()
        {
            _system = PetDepartureSystem.Instance;
            if (_system != null)
            {
                _system.OnRecordsUpdated += RefreshDisplay;
                _system.OnDepartureRecorded += OnDepartureRecorded;
                _system.OnPetReturned += OnPetReturned;
            }
            BuildUI();
            RefreshDisplay();
        }

        public override void _ExitTree()
        {
            if (_system != null)
            {
                _system.OnRecordsUpdated -= RefreshDisplay;
                _system.OnDepartureRecorded -= OnDepartureRecorded;
                _system.OnPetReturned -= OnPetReturned;
            }
        }

        private void BuildUI()
        {
            // 主面板
            _mainPanel = new Panel
            {
                Name = "DepartureBoard",
                Size = new Vector2(300, 400)
            };
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _mainPanel.OffsetLeft = -320;
            _mainPanel.OffsetTop = 60;
            _mainPanel.OffsetRight = -20;
            _mainPanel.OffsetBottom = 480;
            _mainPanel.Visible = false;

            // 背景样式：深紫色主题
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.08f, 0.06f, 0.14f, 0.95f);
            style.BorderColorLeft = new Color(0.5f, 0.35f, 0.7f, 0.7f);
            style.BorderColorRight = new Color(0.5f, 0.35f, 0.7f, 0.7f);
            style.BorderColorTop = new Color(0.5f, 0.35f, 0.7f, 0.7f);
            style.BorderColorBottom = new Color(0.5f, 0.35f, 0.7f, 0.7f);
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthBottom = 2;
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            _mainPanel.AddThemeStyleboxOverride("panel", style);

            AddChild(_mainPanel);

            // 标题栏
            var header = new HBoxContainer
            {
                Name = "Header",
                SizeFlagsHorizontal = SizeFlags.Fill
            };
            header.CustomMinimumSize = new Vector2(0, 36);
            _mainPanel.AddChild(header);

            _titleLabel = new Label
            {
                Text = "档案陈列室",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _titleLabel.VerticalAlignment = VerticalAlignment.Center;
            _titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 1f, 1f));
            header.AddChild(_titleLabel);

            _closeButton = new Button
            {
                Text = "×",
                CustomMinimumSize = new Vector2(28, 28)
            };
            _closeButton.Pressed += () => HidePanel();
            header.AddChild(_closeButton);

            // 滚动区域
            _scroll = new ScrollContainer
            {
                Name = "Scroll",
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            _scroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _scroll.OffsetLeft = 0;
            _scroll.OffsetTop = 36;
            _scroll.OffsetRight = 0;
            _scroll.OffsetBottom = 0;
            _mainPanel.AddChild(_scroll);

            _recordList = new VBoxContainer
            {
                Name = "RecordList",
                CustomMinimumSize = new Vector2(0, 0)
            };
            _recordList.SizeFlagsHorizontal = SizeFlags.Fill;
            _scroll.AddChild(_recordList);

            // 空状态标签
            _emptyLabel = new Label
            {
                Text = "暂无档案记录",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visible = false
            };
            _emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.4f, 0.6f, 0.8f));
            _recordList.AddChild(_emptyLabel);
        }

        private void RefreshDisplay()
        {
            if (_system == null) return;

            // 清除现有记录
            foreach (var child in _recordList.GetChildren())
            {
                if (child != _emptyLabel)
                    child.QueueFree();
            }

            var records = _system.GetAllRecords();
            bool hasRecords = records.Count > 0;
            _emptyLabel.Visible = !hasRecords;

            if (!hasRecords)
            {
                if (!_emptyLabel.IsInsideTree()) _recordList.AddChild(_emptyLabel);
                return;
            }

            foreach (var kvp in records)
            {
                var rec = kvp.Value;
                var card = BuildRecordCard(rec);
                _recordList.AddChild(card);
            }
        }

        private Control BuildRecordCard(DepartureRecord rec)
        {
            var container = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 80),
                SizeFlagsHorizontal = SizeFlags.Fill
            };

            // 卡片背景
            var cardStyle = new StyleBoxFlat();
            cardStyle.BgColor = new Color(0.12f, 0.08f, 0.20f, 0.9f);
            cardStyle.BorderColorLeft = new Color(0.45f, 0.3f, 0.65f, 0.5f);
            cardStyle.BorderWidthLeft = 3;
            cardStyle.CornerRadiusTopLeft = 6;
            cardStyle.CornerRadiusTopRight = 6;
            cardStyle.CornerRadiusBottomLeft = 6;
            cardStyle.CornerRadiusBottomRight = 6;
            cardStyle.ContentMarginLeft = 10;
            cardStyle.ContentMarginTop = 8;
            cardStyle.ContentMarginRight = 10;
            cardStyle.ContentMarginBottom = 8;
            container.AddThemeStyleboxOverride("panel", cardStyle);

            // 状态条（左边框颜色表示状态）
            Color borderColor;
            string statusText;
            if (rec.SynergyBonusActive)
            {
                borderColor = new Color(0.3f, 0.9f, 0.5f, 1f); // 绿色 = 归队
                statusText = "已归队  +5%";
            }
            else if (rec.IsReturned)
            {
                borderColor = new Color(0.5f, 0.5f, 0.5f, 0.8f); // 灰色 = 曾归队但已离队
                statusText = "曾归队";
            }
            else
            {
                borderColor = new Color(0.9f, 0.5f, 0.3f, 1f); // 橙色 = 当前离队
                statusText = "已离队";
            }

            cardStyle.BorderColorLeft = borderColor;
            cardStyle.BorderWidthLeft = 3;

            var vbox = new VBoxContainer();
            container.AddChild(vbox);

            // 第一行：宠物名 + 状态
            var row1 = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            vbox.AddChild(row1);

            var nameLabel = new Label
            {
                Text = string.IsNullOrEmpty(rec.PetName) ? rec.PetId : rec.PetName,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            nameLabel.HorizontalAlignment = HorizontalAlignment.Left;
            nameLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.9f, 1f, 1f));
            row1.AddChild(nameLabel);

            var statusLabel = new Label
            {
                Text = statusText,
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd
            };
            statusLabel.HorizontalAlignment = HorizontalAlignment.Right;
            statusLabel.AddThemeColorOverride("font_color", borderColor);
            row1.AddChild(statusLabel);

            // 第二行：战斗场次
            var battleLabel = new Label
            {
                Text = $"并肩战斗: {rec.TotalBattles} 场",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            battleLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.55f, 0.7f, 1f));
            battleLabel.CustomMinimumSize = new Vector2(0, 18);
            vbox.AddChild(battleLabel);

            // 第三行：最常用技能
            if (!string.IsNullOrEmpty(rec.MostUsedSkill))
            {
                var skillLabel = new Label
                {
                    Text = $"常用技能: {rec.MostUsedSkill}",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                skillLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.8f, 1f));
                skillLabel.CustomMinimumSize = new Vector2(0, 18);
                vbox.AddChild(skillLabel);
            }

            // 第四行：最后战斗时间
            if (rec.LastBattleTimestamp > 0)
            {
                var dt = DateTimeOffset.FromUnixTimeMilliseconds(rec.LastBattleTimestamp);
                var timeAgo = GetTimeAgo(dt);
                var timeLabel = new Label
                {
                    Text = $"最后并肩: {timeAgo}",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                timeLabel.AddThemeColorOverride("font_color", new Color(0.45f, 0.4f, 0.6f, 0.9f));
                timeLabel.CustomMinimumSize = new Vector2(0, 18);
                vbox.AddChild(timeLabel);
            }

            return container;
        }

        private string GetTimeAgo(DateTimeOffset dt)
        {
            var span = DateTimeOffset.Now - dt;
            if (span.TotalDays >= 30)
                return $"{(int)(span.TotalDays / 30)}个月前";
            if (span.TotalDays >= 1)
                return $"{(int)span.TotalDays}天前";
            if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours}小时前";
            if (span.TotalMinutes >= 1)
                return $"{(int)span.TotalMinutes}分钟前";
            return "刚刚";
        }

        private void OnDepartureRecorded(string petId, DepartureRecord record)
        {
            // 新档案出现时高亮动画
            RefreshDisplay();
        }

        private void OnPetReturned(string petId, DepartureRecord record)
        {
            // 归队时动画
            RefreshDisplay();
        }

        #region Show/Hide API

        public void ShowPanel()
        {
            _visible = true;
            _mainPanel.Visible = true;
            RefreshDisplay();
        }

        public void HidePanel()
        {
            _visible = false;
            _mainPanel.Visible = false;
        }

        public void TogglePanel()
        {
            if (_visible)
                HidePanel();
            else
                ShowPanel();
        }

        public bool IsVisible() => _visible;

        #endregion
    }
}
