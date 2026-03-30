using Godot;
using System;

namespace ClawRPG.Scripts.Systems.Guild
{
    /// <summary>
    /// 公会公告数据
    /// </summary>
    public class GuildAnnouncementData
    {
        public string AnnouncementId { get; set; } = "";
        public string Content { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public DateTime PostTime { get; set; } = DateTime.Now;

        public GuildAnnouncementData() { }

        public GuildAnnouncementData(string id, string content, string authorName)
        {
            AnnouncementId = id;
            Content = content;
            AuthorName = authorName;
            PostTime = DateTime.Now;
        }
    }
}
