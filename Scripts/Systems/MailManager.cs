using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// 邮件数据
    /// </summary>
    public class MailData {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Sender { get; set; } = "";
        public string Receiver { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public int Gold { get; set; } = 0;
        public List<string> AttachedItems { get; set; } = new List<string>();
        public DateTime SendTime { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public bool IsSystemMail { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// 邮件管理器 - 处理玩家邮件
    /// </summary>
    public partial class MailManager : Node {
        public static MailManager Instance { get; private set; }

        private Dictionary<string, List<MailData>> _mailBox = new Dictionary<string, List<MailData>>();
        
        // 信号系统
        public signal void MailReceived(string playerId, int unreadCount);
        public signal void MailDeleted(string playerId);
        public signal void MailSent(bool success, string message);

        public override void _Ready() {
            Instance = this;
        }

        /// <summary>
        /// 获取玩家邮箱
        /// </summary>
        public List<MailData> GetMailBox(string playerId) {
            if (!_mailBox.ContainsKey(playerId)) {
                _mailBox[playerId] = new List<MailData>();
            }
            return _mailBox[playerId];
        }

        /// <summary>
        /// 获取未读邮件数量
        /// </summary>
        public int GetUnreadCount(string playerId) {
            var mails = GetMailBox(playerId);
            int count = 0;
            foreach (var mail in mails) {
                if (!mail.IsRead && !mail.IsDeleted) count++;
            }
            return count;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public bool SendMail(string sender, string receiver, string title, string content, int gold = 0, List<string> items = null, bool isSystemMail = false) {
            if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(title)) {
                MailSent?.Invoke(false, "收件人或标题不能为空");
                return false;
            }

            var mail = new MailData {
                Sender = sender,
                Receiver = receiver,
                Title = title,
                Content = content,
                Gold = gold,
                AttachedItems = items ?? new List<string>(),
                IsSystemMail = isSystemMail,
                SendTime = DateTime.Now
            };

            if (!_mailBox.ContainsKey(receiver)) {
                _mailBox[receiver] = new List<MailData>();
            }
            _mailBox[receiver].Add(mail);

            MailReceived?.Invoke(receiver, GetUnreadCount(receiver));
            MailSent?.Invoke(true, "邮件发送成功");
            return true;
        }

        /// <summary>
        /// 读取邮件
        /// </summary>
        public bool MarkAsRead(string playerId, string mailId) {
            var mails = GetMailBox(playerId);
            foreach (var mail in mails) {
                if (mail.Id == mailId) {
                    mail.IsRead = true;
                    MailReceived?.Invoke(playerId, GetUnreadCount(playerId));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 领取邮件附件
        /// </summary>
        public (int gold, List<string> items) ClaimAttachments(string playerId, string mailId) {
            var mails = GetMailBox(playerId);
            foreach (var mail in mails) {
                if (mail.Id == mailId && !mail.IsDeleted) {
                    int gold = mail.Gold;
                    List<string> items = new List<string>(mail.AttachedItems);
                    
                    mail.Gold = 0;
                    mail.AttachedItems.Clear();
                    mail.IsRead = true;
                    
                    return (gold, items);
                }
            }
            return (0, new List<string>());
        }

        /// <summary>
        /// 删除邮件
        /// </summary>
        public bool DeleteMail(string playerId, string mailId) {
            var mails = GetMailBox(playerId);
            foreach (var mail in mails) {
                if (mail.Id == mailId) {
                    mail.IsDeleted = true;
                    MailDeleted?.Invoke(playerId);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 发送系统邮件
        /// </summary>
        public void SendSystemMail(string receiver, string title, string content, int gold = 0, List<string> items = null) {
            SendMail("系统", receiver, title, content, gold, items, true);
        }

        /// <summary>
        /// 广播邮件给所有玩家
        /// </summary>
        public void BroadcastMail(string title, string content, int gold = 0, List<string> items = null) {
            foreach (var playerId in _mailBox.Keys) {
                SendMail("系统", playerId, title, content, gold, items, true);
            }
        }

        /// <summary>
        /// 保存邮件数据
        /// </summary>
        public Dictionary<string, List<MailData>> SaveData() {
            return _mailBox;
        }

        /// <summary>
        /// 加载邮件数据
        /// </summary>
        public void LoadData(Dictionary<string, List<MailData>> data) {
            if (data != null) {
                _mailBox = data;
            }
        }
    }
}
