using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 多人游戏聊天系统
/// 支持大厅、队伍、私聊消息
/// </summary>
public class MultiplayerChat : BaseSystem
{
    public static MultiplayerChat Instance { get; private set; }

    // 消息类型
    public enum ChatType
    {
        Lobby,      // 大厅聊天
        Team,       // 队伍聊天
        Whisper,    // 私聊
        System      // 系统消息
    }

    // 聊天消息
    public class ChatMessage
    {
        public ChatType Type;
        public int SenderId;
        public string SenderName;
        public string Content;
        public float Timestamp;
        public int TargetPlayerId;  // for whisper

        public ChatMessage(ChatType type, int senderId, string senderName, string content)
        {
            Type = type;
            SenderId = senderId;
            SenderName = senderName;
            Content = content;
            Timestamp = (float)OS.GetSystemTimeSecs();
        }
    }

    // 信号
    public delegate void MessageReceivedEvent(ChatMessage message);
    public event MessageReceivedEvent OnMessageReceived;

    // 状态
    private List<ChatMessage> _messageHistory = new List<ChatMessage>();
    private int _maxHistory = 100;
    private bool _isOpen = false; 
    private ChatType _currentChannel = ChatType.Lobby;

    public bool IsOpen => _isOpen;
    public ChatType CurrentChannel => _currentChannel;
    public List<ChatMessage> MessageHistory => _messageHistory;

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// 发送聊天消息
    /// </summary>
    public void SendMessage(string content, ChatType? type = null)
    {
        if (string.IsNullOrWhiteSpace(content)) return;

        var chatType = type ?? _currentChannel;
        var playerId = MultiplayerManager.Instance.LocalPlayerId;
        var playerName = MultiplayerManager.Instance.PlayerName;

        // 本地显示
        var message = new ChatMessage(chatType, playerId, playerName, content);
        AddMessage(message);

        // 发送到服务器
        if (MultiplayerManager.Instance.IsInRoom)
        {
            var data = new Dictionary<string, object>
            {
                { "type", "chat_message" },
                { "chat_type", (int)chatType },
                { "sender_id", playerId },
                { "sender_name", playerName },
                { "content", content }
            };

            // 私聊需要目标ID
            if (chatType == ChatType.Whisper)
            {
                data["target_id"] = message.TargetPlayerId;
            }

            NetworkClient.Instance.SendJson(data);
        }
    }

    /// <summary>
    /// 发送私聊
    /// </summary>
    public void SendWhisper(int targetPlayerId, string content)
    {
        var message = new ChatMessage(ChatType.Whisper, 
            MultiplayerManager.Instance.LocalPlayerId, 
            MultiplayerManager.Instance.PlayerName, 
            content);
        message.TargetPlayerId = targetPlayerId;
        
        AddMessage(message);

        // 发送到服务器
        if (MultiplayerManager.Instance.IsInRoom)
        {
            var data = new Dictionary<string, object>
            {
                { "type", "chat_message" },
                { "chat_type", (int)ChatType.Whisper },
                { "sender_id", message.SenderId },
                { "sender_name", message.SenderName },
                { "target_id", targetPlayerId },
                { "content", content }
            };
            NetworkClient.Instance.SendJson(data);
        }
    }

    /// <summary>
    /// 添加消息到历史
    /// </summary>
    public void AddMessage(ChatMessage message)
    {
        _messageHistory.Add(message);
        
        // 限制历史长度
        while (_messageHistory.Count > _maxHistory)
        {
            _messageHistory.RemoveAt(0);
        }

        OnMessageReceived?.Invoke(message);
    }

    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    public void HandleMessage(Dictionary<string, object> data)
    {
        if (!data.ContainsKey("content")) return;

        var chatType = ChatType.Lobby;
        if (data.ContainsKey("chat_type"))
            chatType = (ChatType)(int)data["chat_type"];

        var senderId = data.ContainsKey("sender_id") ? (int)data["sender_id"] : -1;
        var senderName = data.ContainsKey("sender_name") ? data["sender_name"].ToString() : "Unknown";
        var content = data["content"].ToString();

        var message = new ChatMessage(chatType, senderId, senderName, content);
        
        if (data.ContainsKey("target_id"))
            message.TargetPlayerId = (int)data["target_id"];

        AddMessage(message);
    }

    /// <summary>
    /// 切换聊天频道
    /// </summary>
    public void SwitchChannel(ChatType channel)
    {
        _currentChannel = channel;
    }

    /// <summary>
    /// 打开/关闭聊天界面
    /// </summary>
    public void Toggle()
    {
        _isOpen = !_isOpen;
    }

    /// <summary>
    /// 打开聊天界面
    /// </summary>
    public void Open()
    {
        _isOpen = true;
    }

    /// <summary>
    /// 关闭聊天界面
    /// </summary>
    public void Close()
    {
        _isOpen = false; 
    }

    /// <summary>
    /// 清除聊天历史
    /// </summary>
    public void ClearHistory()
    {
        _messageHistory.Clear();
    }

    /// <summary>
    /// 获取当前频道的名称
    /// </summary>
    public string GetChannelName(ChatType type)
    {
        return type switch
        {
            ChatType.Lobby => "大厅",
            ChatType.Team => "队伍",
            ChatType.Whisper => "私聊",
            ChatType.System => "系统",
            _ => "未知"
        };
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        // No persistent data needed for chat system
    }
}
