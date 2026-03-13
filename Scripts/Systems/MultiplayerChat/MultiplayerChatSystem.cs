using Godot;
using System.Collections.Generic;

public class MultiplayerChatSystem : Node
{
    private MultiplayerChatData _data;
    private MultiplayerChatDatabase _database;
    
    // Current channel
    private MultiplayerChatData.ChatChannel _currentChannel = MultiplayerChatData.ChatChannel.Global;
    
    // Settings
    private bool _showTimestamps = true;
    private bool _emoteEnabled = true;
    private bool _profanityFilter = false;
    
    public override void _Ready()
    {
        _data = GetNode<MultiplayerChatData>("MultiplayerChatData");
        _database = GetNode<MultiplayerChatDatabase>("MultiplayerChatDatabase");
        
        GD.Print("MultiplayerChatSystem initialized");
    }
    
    // Send a message to a channel
    public void SendMessage(string message, MultiplayerChatData.ChatChannel? channel = null)
    {
        var targetChannel = channel ?? _currentChannel;
        
        if (!_database.Channels.ContainsKey(targetChannel))
            return;
            
        var channelConfig = _database.Channels[targetChannel];
        if (!channelConfig.DefaultEnabled)
            return;
        
        // Check for commands
        if (message.StartsWith("/"))
        {
            ProcessCommand(message);
            return;
        }
        
        // Apply profanity filter if enabled
        if (_profanityFilter && _database.IsFiltered(message))
        {
            message = FilterMessage(message);
        }
        
        // Truncate if too long
        int maxLength = _database.GetChannelMaxLength(targetChannel);
        if (message.Length > maxLength)
            message = message.Substring(0, maxLength);
        
        // Add message
        _data.AddMessage(_data.PlayerName, message, targetChannel);
        
        // Emit signal for UI update
        EmitSignal(nameof(MessageReceived), message, targetChannel);
    }
    
    // Process chat commands
    private void ProcessCommand(string command)
    {
        string[] parts = command.Split(" ", true, 2);
        string cmd = parts[0].ToLower();
        string arg = parts.Length > 1 ? parts[1] : "";
        
        switch (cmd)
        {
            case "/help":
                ShowHelp();
                break;
            case "/me":
                SendEmote(arg);
                break;
            case "/w":
                SendWhisper(arg);
                break;
            case "/ignore":
                IgnorePlayer(arg);
                break;
            case "/unignore":
                UnignorePlayer(arg);
                break;
            case "/clear":
                // Handled in UI
                EmitSignal(nameof(ClearRequested));
                break;
            case "/channel":
                SwitchChannel(arg);
                break;
            default:
                SendSystemMessage("Unknown command. Use /help for available commands.");
                break;
        }
    }
    
    // Send emote (/me command)
    public void SendEmote(string emoteText)
    {
        if (!_emoteEnabled)
        {
            SendSystemMessage("Emotes are disabled.");
            return;
        }
        
        if (string.IsNullOrEmpty(emoteText))
        {
            SendSystemMessage("Usage: /me [emote] or /me text");
            return;
        }
        
        // Check if it's a known emote
        string[] words = emoteText.Split(" ");
        var emote = _database.GetEmote(words[0].ToLower());
        
        if (emote != null)
        {
            string emoteMessage = $"{_data.PlayerName} {emote.DisplayText}!";
            _data.AddMessage(_data.PlayerName, emoteMessage, _currentChannel, true);
            EmitSignal(nameof(MessageReceived), emoteMessage, _currentChannel, true);
        }
        else
        {
            // Custom emote text
            string emoteMessage = $"{_data.PlayerName} {emoteText}";
            _data.AddMessage(_data.PlayerName, emoteMessage, _currentChannel, true);
            EmitSignal(nameof(MessageReceived), emoteMessage, _currentChannel, true);
        }
    }
    
    // Send whisper to specific player
    public void SendWhisper(string args)
    {
        string[] parts = args.Split(" ", 2);
        if (parts.Length < 2)
        {
            SendSystemMessage("Usage: /w playerName message");
            return;
        }
        
        string targetPlayer = parts[0];
        string message = parts[1];
        
        _data.AddMessage(_data.PlayerName, $"To {targetPlayer}: {message}", MultiplayerChatData.ChatChannel.Whisper);
        EmitSignal(nameof(MessageReceived), $"To {targetPlayer}: {message}", MultiplayerChatData.ChatChannel.Whisper);
        
        // Also show in current channel that we whispered
        SendSystemMessage($"You whispered to {targetPlayer}: {message}");
    }
    
    // Ignore a player
    public void IgnorePlayer(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            SendSystemMessage("Usage: /ignore playerName");
            return;
        }
        
        _data.IgnorePlayer(playerName);
        SendSystemMessage($"Ignored {playerName}");
    }
    
    // Unignore a player
    public void UnignorePlayer(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            SendSystemMessage("Usage: /unignore playerName");
            return;
        }
        
        _data.UnignorePlayer(playerName);
        SendSystemMessage($"Unignored {playerName}");
    }
    
    // Switch chat channel
    public void SwitchChannel(string channelName)
    {
        channelName = channelName.ToLower().Trim();
        
        switch (channelName)
        {
            case "global":
                _currentChannel = MultiplayerChatData.ChatChannel.Global;
                SendSystemMessage("Switched to Global channel");
                break;
            case "room":
                _currentChannel = MultiplayerChatData.ChatChannel.Room;
                SendSystemMessage("Switched to Room channel");
                break;
            case "party":
                _currentChannel = MultiplayerChatData.ChatChannel.Party;
                SendSystemMessage("Switched to Party channel");
                break;
            default:
                SendSystemMessage("Available channels: global, room, party");
                break;
        }
        
        EmitSignal(nameof(ChannelChanged), _currentChannel);
    }
    
    // Show help
    private void ShowHelp()
    {
        string helpText = "=== Chat Commands ===\n";
        foreach (var kvp in _database.Commands)
        {
            helpText += $"{kvp.Key}: {kvp.Value}\n";
        }
        SendSystemMessage(helpText);
    }
    
    // Send system message
    public void SendSystemMessage(string message)
    {
        _data.AddMessage("System", message, MultiplayerChatData.ChatChannel.System);
        EmitSignal(nameof(MessageReceived), message, MultiplayerChatData.ChatChannel.System);
    }
    
    // Filter message (profanity filter)
    private string FilterMessage(string message)
    {
        // Replace filtered words with asterisks
        foreach (var word in _database.FilteredWords)
        {
            if (message.ToLower().Contains(word.ToLower()))
            {
                string replacement = "";
                for (int i = 0; i < word.Length; i++)
                    replacement += "*";
                message = message.Replace(word, replacement);
            }
        }
        return message;
    }
    
    // Get messages for a channel
    public List<MultiplayerChatData.ChatMessage> GetChannelMessages(MultiplayerChatData.ChatChannel channel)
    {
        return _data.GetMessages(channel);
    }
    
    // Get recent messages
    public List<MultiplayerChatData.ChatMessage> GetRecentMessages(int count = 50)
    {
        return _data.GetRecentMessages(count);
    }
    
    // Get current channel
    public MultiplayerChatData.ChatChannel GetCurrentChannel()
    {
        return _currentChannel;
    }
    
    // Get all emotes
    public List<MultiplayerChatDatabase.EmoteDefinition> GetEmotes()
    {
        return _database.GetAllEmotes();
    }
    
    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        return _data.GetStatistics();
    }
    
    // Settings
    public void SetShowTimestamps(bool show)
    {
        _showTimestamps = show;
    }
    
    public void SetEmoteEnabled(bool enabled)
    {
        _emoteEnabled = enabled;
    }
    
    public void SetProfanityFilter(bool enabled)
    {
        _profanityFilter = enabled;
    }
    
    public void SetPlayerName(string name)
    {
        _data.PlayerName = name;
    }
    
    // Signals
    [Signal]
    public delegate void MessageReceived(string message, MultiplayerChatData.ChatChannel channel, bool isEmote = false);
    
    [Signal]
    public delegate void ChannelChanged(MultiplayerChatData.ChatChannel channel);
    
    [Signal]
    public delegate void ClearRequested();
}
