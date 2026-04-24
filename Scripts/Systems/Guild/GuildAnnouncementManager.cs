using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Guild
{
    /// <summary>
    /// 公会公告管理器
    /// 负责公告的发布、查询、管理
    /// </summary>
    public partial class GuildAnnouncementManager
    {
        private readonly List<GuildAnnouncementData> _announcements = new List<GuildAnnouncementData>();
        private const int MaxAnnouncements = 20;

        /// <summary>
        /// 所有公告（最新在前）
        /// </summary>
        public List<GuildAnnouncementData> Announcements => _announcements;

        /// <summary>
        /// 发布新公告
        /// </summary>
        /// <param name="content">公告内容</param>
        /// <param name="authorName">作者名称</param>
        /// <returns>是否发布成功</returns>
        public bool PostAnnouncement(string content, string authorName)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            var announcement = new GuildAnnouncementData(
                "ann_" + GD.Hash(content + DateTime.Now.ToString()).ToString(),
                content,
                authorName
            );

            _announcements.Insert(0, announcement);

            // 超过最大数量时移除最旧的
            while (_announcements.Count > MaxAnnouncements)
                _announcements.RemoveAt(_announcements.Count - 1);

            return true;
        }

        /// <summary>
        /// 获取最新 N 条公告
        /// </summary>
        public List<GuildAnnouncementData> GetLatest(int count = 10)
        {
            var result = new List<GuildAnnouncementData>();
            for (int i = 0; i < Math.Min(count, _announcements.Count); i++)
                result.Add(_announcements[i]);
            return result;
        }

        /// <summary>
        /// 清空所有公告
        /// </summary>
        public void Clear()
        {
            _announcements.Clear();
        }

        /// <summary>
        /// 从保存数据恢复
        /// </summary>
        public void FromSaveData(List<Dictionary> data)
        {
            _announcements.Clear();
            if (data == null) return;

            foreach (var dict in data)
            {
                var a = new GuildAnnouncementData
                {
                    AnnouncementId = dict.ContainsKey("announcement_id") ? dict["announcement_id"].ToString() : "",
                    Content = dict.ContainsKey("content") ? dict["content"].ToString() : "",
                    AuthorName = dict.ContainsKey("author_name") ? dict["author_name"].ToString() : ""
                };
                if (dict.ContainsKey("post_time") && !string.IsNullOrEmpty(dict["post_time"].ToString()))
                {
                    try { a.PostTime = DateTime.Parse(dict["post_time"].ToString()); }
                    catch { a.PostTime = DateTime.Now; }
                }
                _announcements.Add(a);
            }
        }

        /// <summary>
        /// 导出为保存数据格式
        /// </summary>
        public List<Dictionary> ToSaveData()
        {
            var result = new List<Dictionary>();
            foreach (var a in _announcements)
            {
                result.Add(new Dictionary
                {
                    ["announcement_id"] = a.AnnouncementId,
                    ["content"] = a.Content,
                    ["author_name"] = a.AuthorName,
                    ["post_time"] = a.PostTime.ToString("o")
                });
            }
            return result;
        }
    }
}
