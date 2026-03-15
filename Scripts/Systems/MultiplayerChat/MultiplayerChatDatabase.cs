using Godot;
using System.Collections.Generic;

public class MultiplayerChatDatabase : BaseSystem
{
    // Emote definitions
    public class EmoteDefinition
    {
        public string Id;
        public string Name;
        public string DisplayText; // e.g., "waves" for /me waves
        public string Icon;
        
        public EmoteDefinition(string id, string name, string displayText, string icon = "")
        {
            Id = id;
            Name = name;
            DisplayText = displayText;
            Icon = icon;
        }
    }
    
    // Channel configuration
    public class ChannelConfig
    {
        public string Name;
        public string Color;
        public bool DefaultEnabled;
        public int MaxLength;
        
        public ChannelConfig(string name, string color, bool defaultEnabled = true, int maxLength = 200)
        {
            Name = name;
            Color = color;
            DefaultEnabled = defaultEnabled;
            MaxLength = maxLength;
        }
    }
    
    // Message color presets
    public Dictionary<string, string> MessageColors = new Dictionary<string, string>
    {
        { "Global", "#FFFFFF" },
        { "Room", "#00FF00" },
        { "Party", "#00FFFF" },
        { "Whisper", "#FF00FF" },
        { "System", "#FFFF00" },
        { "Emote", "#FFAA00" }
    };
    
    // Channel configurations
    public Dictionary<MultiplayerChatData.ChatChannel, ChannelConfig> Channels = new Dictionary<MultiplayerChatData.ChatChannel, ChannelConfig>
    {
        { MultiplayerChatData.ChatChannel.Global, new ChannelConfig("Global", "#FFFFFF", true, 200) },
        { MultiplayerChatData.ChatChannel.Room, new ChannelConfig("Room", "#00FF00", true, 300) },
        { MultiplayerChatData.ChatChannel.Party, new ChannelConfig("Party", "#00FFFF", true, 300) },
        { MultiplayerChatData.ChatChannel.Whisper, new ChannelConfig("Whisper", "#FF00FF", true, 500) },
        { MultiplayerChatData.ChatChannel.System, new ChannelConfig("System", "#FFFF00", true, 500) }
    };
    
    // Available emotes
    public List<EmoteDefinition> Emotes = new List<EmoteDefinition>
    {
        new EmoteDefinition("wave", "Wave", "waves"),
        new EmoteDefinition("laugh", "Laugh", "laughs"),
        new EmoteDefinition("cry", "Cry", "cries"),
        new EmoteDefinition("dance", "Dance", "dances"),
        new EmoteDefinition("attack", "Attack", "attacks"),
        new EmoteDefinition("defend", "Defend", "defends"),
        new EmoteDefinition("heal", "Heal", "heals"),
        new EmoteDefinition("buff", "Buff", "buffs"),
        new EmoteDefinition("cheer", "Cheer", "cheers"),
        new EmoteDefinition("thumbsup", "Thumbs Up", "gives a thumbs up"),
        new EmoteDefinition("clap", "Clap", "claps"),
        new EmoteDefinition("nod", "Nod", "nods"),
        new EmoteDefinition("shake", "Shake Head", "shakes their head"),
        new EmoteDefinition("hug", "Hug", "hugs"),
        new EmoteDefinition("poke", "Poke", "pokes"),
        new EmoteDefinition("bow", "Bow", "bows"),
        new EmoteDefinition("salute", "Salute", "salutes"),
        new EmoteDefinition("facepalm", "Facepalm", "facepalms"),
        new EmoteDefinition("shrug", "Shrug", "shrugs"),
        new EmoteDefinition("point", "Point", "points")
    };
    
    // Filter words (profanity filter placeholder)
    public List<string> FilteredWords = new List<string>
    {
        // Placeholder - can be expanded
    };
    
    // Chat commands
    public Dictionary<string, string> Commands = new Dictionary<string, string>
    {
        { "/help", "Show available commands" },
        { "/me", "Send emote (use /me wave)" },
        { "/w", "Send whisper (use /w player message)" },
        { "/ignore", "Ignore player (use /ignore player)" },
        { "/unignore", "Unignore player (use /unignore player)" },
        { "/clear", "Clear chat history" },
        { "/channel", "Switch channel (use /channel global/room/party)" }
    };
    
    public override void _Ready()
    {
        GD.Print("MultiplayerChatDatabase initialized");
    }
    
    public string GetChannelName(MultiplayerChatData.ChatChannel channel)
    {
        if (Channels.ContainsKey(channel))
            return Channels[channel].Name;
        return channel.ToString();
    }
    
    public string GetChannelColor(MultiplayerChatData.ChatChannel channel)
    {
        if (Channels.ContainsKey(channel))
            return Channels[channel].Color;
        return "#FFFFFF";
    }
    
    public int GetChannelMaxLength(MultiplayerChatData.ChatChannel channel)
    {
        if (Channels.ContainsKey(channel))
            return Channels[channel].MaxLength;
        return 200;
    }
    
    public EmoteDefinition GetEmote(string emoteId)
    {
        foreach (var emote in Emotes)
        {
            if (emote.Id == emoteId)
                return emote;
        }
        return null;
    }
    
    public List<EmoteDefinition> GetAllEmotes()
    {
        return Emotes;
    }
    
    public bool IsFiltered(string message)
    {
        string lowerMessage = message.ToLower();
        foreach (var word in FilteredWords)
        {
            if (lowerMessage.Contains(word.ToLower()))
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        // MultiplayerChatDatabase 是静态配置数据，不需要持久化
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        // MultiplayerChatDatabase 是静态配置数据，不需要持久化
    }
}
