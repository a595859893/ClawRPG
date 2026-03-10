using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems;

namespace ClawRPG.UI {
    /// <summary>
    /// 邮件UI - 显示和管理玩家邮件
    /// </summary>
    public partial class MailUI : Control {
        private VBoxContainer _mailListContainer;
        private VBoxContainer _mailContentContainer;
        private Label _titleLabel;
        private Label _senderLabel;
        private Label _timeLabel;
        private RichTextLabel _contentLabel;
        private Label _goldLabel;
        private Button _claimButton;
        private Button _deleteButton;
        private Button _closeButton;
        private Label _unreadLabel;
        
        private string _currentPlayerId;
        private List<MailData> _currentMails = new List<MailData>();
        private MailData _selectedMail;

        public override void _Ready() {
            SetupUI();
            Visible = false;
        }

        private void SetupUI() {
            // 主容器
            var mainContainer = new HBoxContainer {
                AnchorRight = new Vector2(1, 1),
                AnchorBottom = new Vector2(1, 1),
                OffsetLeft = 100,
                OffsetTop = 50,
                OffsetRight = -100,
                OffsetBottom = -50
            };
            AddChild(mainContainer);

            // 左侧邮件列表
            var listPanel = new PanelContainer {
                CustomMinimumSize = new Vector2(300, 0),
                SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.ShrinkEnd
            };
            mainContainer.AddChild(listPanel);

            var listVBox = new VBoxContainer {
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            listPanel.AddChild(listVBox);

            // 标题和未读数
            var headerHBox = new HBoxContainer();
            listVBox.AddChild(headerHBox);

            var headerLabel = new Label {
                Text = "📧 邮件",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            headerLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            headerHBox.AddChild(headerLabel);

            _unreadLabel = new Label {
                Text = "(0未读)",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            _unreadLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
            headerHBox.AddChild(_unreadLabel);

            // 邮件列表容器
            _mailListContainer = new VBoxContainer {
                SizeFlagsVertical = Control.SizeFlags.Expand
            };
            listVBox.AddChild(_mailListContainer);

            // 右侧邮件内容
            var contentPanel = new PanelContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            mainContainer.AddChild(contentPanel);

            _mailContentContainer = new VBoxContainer {
                OffsetLeft = 15,
                OffsetTop = 15,
                OffsetRight = -15,
                OffsetBottom = -15
            };
            contentPanel.AddChild(_mailContentContainer);

            // 邮件标题
            _titleLabel = new Label {
                Text = "选择一封邮件",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mailContentContainer.AddChild(_titleLabel);

            // 发件人和时间
            var metaHBox = new HBoxContainer {};
            _mailContentContainer.AddChild(metaHBox);

            _senderLabel = new Label { Text = "发件人: -" };
            _senderLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            metaHBox.AddChild(_senderLabel);

            _timeLabel = new Label { Text = "时间: -" };
            _timeLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
            metaHBox.AddChild(_timeLabel);

            // 邮件内容
            _contentLabel = new RichTextLabel {
                BbcodeEnabled = true,
                SizeFlagsVertical = Control.SizeFlags.Expand
            };
            _mailContentContainer.AddChild(_contentLabel);

            // 金币/附件信息
            _goldLabel = new Label { Text = "" };
            _mailContentContainer.AddChild(_goldLabel);

            // 按钮容器
            var buttonHBox = new HBoxContainer {
                CustomMinimumSize = new Vector2(0, 50)
            };
            _mailContentContainer.AddChild(buttonHBox);

            // 领取按钮
            _claimButton = new Button {
                Text = "📦 领取附件",
                CustomMinimumSize = new Vector2(120, 40)
            };
            _claimButton.Pressed += OnClaimPressed;
            buttonHBox.AddChild(_claimButton);

            buttonHBox.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            // 删除按钮
            _deleteButton = new Button {
                Text = "🗑️ 删除",
                CustomMinimumSize = new Vector2(100, 40)
            };
            _deleteButton.Pressed += OnDeletePressed;
            buttonHBox.AddChild(_deleteButton);

            // 关闭按钮
            var closeContainer = new HBoxContainer {};
            _mailContentContainer.AddChild(closeContainer);
            
            closeContainer.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            _closeButton = new Button {
                Text = "关闭",
                CustomMinimumSize = new Vector2(100, 40)
            };
            _closeButton.Pressed += OnClosePressed;
            closeContainer.AddChild(_closeButton);

            // 添加样式
            AddThemeStyleboxOverride("panel", CreatePanelStyle());
        }

        private StyleBoxFlat CreatePanelStyle() {
            var style = new StyleBoxFlat {
                BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                BorderWidthLeft = 2,
                BorderWidthTop = 2,
                BorderWidthRight = 2,
                BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.3f, 0.4f),
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            return style;
        }

        /// <summary>
        /// 打开邮件界面
        /// </summary>
        public void Open(string playerId) {
            _currentPlayerId = playerId;
            RefreshMailList();
            Visible = true;
        }

        /// <summary>
        /// 刷新邮件列表
        /// </summary>
        public void RefreshMailList() {
            // 清除现有列表
            foreach (var child in _mailListContainer.GetChildren()) {
                child.QueueFree();
            }

            _currentMails = MailManager.Instance.GetMailBox(_currentPlayerId);
            int unreadCount = 0;

            // 添加邮件项
            foreach (var mail in _currentMails) {
                if (mail.IsDeleted) continue;

                if (!mail.IsRead) unreadCount++;

                var mailButton = CreateMailButton(mail);
                _mailListContainer.AddChild(mailButton);
            }

            _unreadLabel.Text = $"({unreadCount}未读)";

            // 重置选中状态
            _selectedMail = null;
            UpdateMailContent();
        }

        private Button CreateMailButton(MailData mail) {
            var button = new Button {
                Text = $"{(mail.IsRead ? "  " : "🔴 ")}{mail.Title}",
                CustomMinimumSize = new Vector2(0, 50),
                TextAlignment = HorizontalAlignment.Left
            };
            
            if (!mail.IsRead) {
                button.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.5f));
            }
            
            if (mail.IsSystemMail) {
                button.Text = "⚙️ " + button.Text.TrimStart();
            }

            button.Pressed += () => OnMailSelected(mail);
            return button;
        }

        private void OnMailSelected(MailData mail) {
            _selectedMail = mail;
            MailManager.Instance.MarkAsRead(_currentPlayerId, mail.Id);
            RefreshMailList();
            UpdateMailContent();
        }

        private void UpdateMailContent() {
            if (_selectedMail == null) {
                _titleLabel.Text = "选择一封邮件";
                _senderLabel.Text = "发件人: -";
                _timeLabel.Text = "时间: -";
                _contentLabel.Text = "";
                _goldLabel.Text = "";
                _claimButton.Disabled = true;
                _deleteButton.Disabled = true;
                return;
            }

            _titleLabel.Text = _selectedMail.Title;
            _senderLabel.Text = $"发件人: {_selectedMail.Sender}";
            _timeLabel.Text = $"时间: {_selectedMail.SendTime:yyyy-MM-dd HH:mm}";
            _contentLabel.Text = _selectedMail.Content;

            // 显示附件信息
            string attachments = "";
            if (_selectedMail.Gold > 0) {
                attachments += $"💰 金币: {_selectedMail.Gold} ";
            }
            if (_selectedMail.AttachedItems.Count > 0) {
                attachments += $"📦 物品: {_selectedMail.AttachedItems.Count}件";
            }
            _goldLabel.Text = attachments;
            _goldLabel.Visible = !string.IsNullOrEmpty(attachments);

            _claimButton.Disabled = _selectedMail.Gold == 0 && _selectedMail.AttachedItems.Count == 0;
            _deleteButton.Disabled = false;
        }

        private void OnClaimPressed() {
            if (_selectedMail == null) return;

            var (gold, items) = MailManager.Instance.ClaimAttachments(_currentPlayerId, _selectedMail.Id);
            
            // 发放金币 - 集成到 EconomySystem
            if (gold > 0) {
                // Gold will be added via EconomySystem when integrated
                GD.Print($"Claimed {gold} gold from mail");
            }

            // 发放物品 - 集成到 InventorySystem
            foreach (var itemId in items) {
                // Items will be added via InventorySystem when integrated
                GD.Print($"Claimed item: {itemId}");
            }

            RefreshMailList();
        }

        private void OnDeletePressed() {
            if (_selectedMail == null) return;

            MailManager.Instance.DeleteMail(_currentPlayerId, _selectedMail.Id);
            RefreshMailList();
        }

        private void OnClosePressed() {
            Visible = false;
        }

        public override void _Input(InputEvent e) {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
                Visible = false;
            }
        }
    }
}
