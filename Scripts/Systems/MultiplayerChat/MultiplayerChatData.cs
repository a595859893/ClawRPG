using Godot;
using System.Collections.Generic;

public class MultiplayerChatData : BaseSystem
{
    // Chat channels
    public enum ChatChannel
    {
        Global,
        Room,
        Party,
        Whisper,
        System
    }
    
    // Message structure
    public class ChatMessage
    {
        public string SenderName;
        public string Content;
        public ChatChannel Channel;
        public long Timestamp;
        public bool IsEmote;
    }
    
    // Channel settings
    public class ChannelSettings
    {
        public bool Enabled = true;
        public bool Muted = false;
    }
    
    // Data storage
    public Dictionary<ChatChannel, List<ChatMessage>> ChannelMessages = new Dictionary<ChatChannel, List<ChatMessage>>();
    public Dictionary<ChatChannel, ChannelSettings> ChannelSettings = new Dictionary<ChatChannel, ChannelSettings>();
    public List<ChatMessage> RecentMessages = new List<ChatMessage>();
    public string PlayerName = "Player";
    
    // Statistics
    public int TotalMessagesSent = 0;
    public int TotalEmotesUsed = 0;
    public Dictionary<ChatChannel, int> MessagesPerChannel = new Dictionary<ChatChannel, int>();
    
    // Ignore list
    public List<string> IgnoredPlayers = new List<string>();
    
    public override void _Ready()
    {
        // Initialize channel messages
        foreach (ChatChannel channel in System.Enum.GetValues(typeof(ChatChannel)))
        {
            ChannelMessages[channel] = new List<ChatMessage>();
            MessagesPerChannel[channel] = 0;
            ChannelSettings[channel] = new ChannelSettings();
        }
    }
    
    public void AddMessage(string sender, string content, ChatChannel channel, bool isEmote = false)
    {
        var message = new ChatMessage
        {
            SenderName = sender,
            Content = content,
            Channel = channel,
            Timestamp = OS.GetUnixTime(),
            IsEmote = isEmote
        };
        
        ChannelMessages[channel].Add(message);
        RecentMessages.Add(message);
        MessagesPerChannel[channel]++;
        TotalMessagesSent++;
        
        if (isEmote)
            TotalEmotesUsed++;
        
        // Keep only last 100 messages per channel
        if (ChannelMessages[channel].Count > 100)
            ChannelMessages[channel].RemoveAt(0);
        
        // Keep only last 200 recent messages
        if (RecentMessages.Count > 200)
            RecentMessages.RemoveAt(0);
    }
    
    public List<ChatMessage> GetMessages(ChatChannel channel)
    {
        return ChannelMessages[channel];
    }
    
    public List<ChatMessage> GetRecentMessages(int count = 50)
    {
        int start = Mathf.Max(0, RecentMessages.Count - count);
        return RecentMessages.GetRange(start, RecentMessages.Count - start);
    }
    
    public void SetChannelEnabled(ChatChannel channel, bool enabled)
    {
        if (ChannelSettings.ContainsKey(channel))
            ChannelSettings[channel].Enabled = enabled;
    }
    
    public void SetChannelMuted(ChatChannel channel, bool muted)
    {
        if (ChannelSettings.ContainsKey(channel))
            ChannelSettings[channel].Muted = muted;
    }
    
    public bool IsPlayerIgnored(string playerName)
    {
        return IgnoredPlayers.Contains(playerName);
    }
    
    public void IgnorePlayer(string playerName)
    {
        if (!IgnoredPlayers.Contains(playerName))
            IgnoredPlayers.Add(playerName);
    }
    
    public void UnignorePlayer(string playerName)
    {
        IgnoredPlayers.Remove(playerName);
    }
    
    public Dictionary<string, int> GetStatistics()
    {
        var stats = new Dictionary<string, int>();
        stats["TotalMessages"] = TotalMessagesSent;
        stats["TotalEmotes"] = TotalEmotesUsed;
        
        foreach (var kvp in MessagesPerChannel)
        {
            stats[kvp.Key.ToString() + "Messages"] = kvp.Value;
        }
        
        return stats;
    }
}
