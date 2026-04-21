using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using Array = System.Array;

/// <summary>
/// 好友系统 - 玩家好友关系管理
/// 支持添加、删除、拉黑、好友状态等功能
/// </summary>
public partial class FriendSystem : BaseSystem
{
    // 单例
    private static FriendSystem _instance;
    public static FriendSystem Instance => _instance;

    // 好友数据
    private System.Collections.Generic.Dictionary<string, FriendData> _friends = new System.Collections.Generic.Dictionary<string, FriendData>();
    private System.Collections.Generic.Dictionary<string, FriendRequest> _pendingRequests = new System.Collections.Generic.Dictionary<string, FriendRequest>();
    private Array<string> _blockedPlayers = new Array<string>();
    
    // 聊天记录
    private System.Collections.Generic.Dictionary<string, Godot.Collections.Array> _chatHistory = new System.Collections.Generic.Dictionary<string, Godot.Collections.Array>();

    // 信号
    public delegate void FriendListUpdated();
    public delegate void FriendRequestReceived(string fromPlayer, string message);
    public delegate void ChatMessageReceived(string fromPlayer, string message);
    public delegate void FriendStatusChanged(string playerName, bool isOnline);

    public override void _Ready()
    {
        _instance = this;
    }

    // 发送好友申请
    public bool SendFriendRequest(string playerName, string message = "")
    {
        if (playerName == GetPlayerName()) return false;
        if (_friends.ContainsKey(playerName)) return false;
        if (_blockedPlayers.Contains(playerName)) return false;

        // 创建好友申请
        var request = new FriendRequest
        {
            fromPlayer = GetPlayerName(),
            toPlayer = playerName,
            message = message,
            timestamp = OS.GetSystemTimeMsecs()
        };

        // 模拟接受（实际应该是网络请求）
        // 这里简化处理，直接添加到待处理列表
        AddPendingRequest(playerName, request);
        
        return true;
    }

    // 接受好友申请
    public bool AcceptFriendRequest(string playerName)
    {
        if (!_pendingRequests.ContainsKey(playerName)) return false;

        var request = _pendingRequests[playerName];
        AddFriend(playerName, new FriendData
        {
            playerName = playerName,
            addedTime = OS.GetSystemTimeMsecs(),
            isOnline = true, // 假设在线
            lastSeen = OS.GetSystemTimeMsecs(),
            friendshipLevel = 1
        });

        _pendingRequests.Remove(playerName);
        EmitSignal(nameof(FriendListUpdated));
        return true;
    }

    // 拒绝好友申请
    public bool DeclineFriendRequest(string playerName)
    {
        if (!_pendingRequests.ContainsKey(playerName)) return false;
        _pendingRequests.Remove(playerName);
        return true;
    }

    // 拉黑玩家
    public void BlockPlayer(string playerName)
    {
        if (!_blockedPlayers.Contains(playerName))
        {
            _blockedPlayers.Add(playerName);
        }
        // 同时删除好友关系
        if (_friends.ContainsKey(playerName))
        {
            _friends.Remove(playerName);
            EmitSignal(nameof(FriendListUpdated));
        }
    }

    // 解除拉黑
    public void UnblockPlayer(string playerName)
    {
        _blockedPlayers.Remove(playerName);
    }

    // 发送私信
    public bool SendMessage(string friendName, string message)
    {
        if (!_friends.ContainsKey(friendName)) return false;

        var chatMessage = new ChatMessage
        {
            fromPlayer = GetPlayerName(),
            toPlayer = friendName,
            message = message,
            timestamp = OS.GetSystemTimeMsecs(),
            isRead = true
        };

        AddChatMessage(friendName, chatMessage);
        
        // 模拟收到回复（实际应该是网络）
        var replyMessage = new ChatMessage
        {
            fromPlayer = friendName,
            toPlayer = GetPlayerName(),
            message = "消息已收到！",
            timestamp = OS.GetSystemTimeMsecs(),
            isRead = false
        };
        AddChatMessage(friendName, replyMessage);
        
        EmitSignal(nameof(ChatMessageReceived), friendName, message);
        return true;
    }

    // 获取好友列表
    public Godot.Collections.Array GetFriends()
    {
        var friends = new Godot.Collections.Array();
        foreach (var friend in _friends.Values)
        {
            friends.Add(new Godot.Collections.Dictionary
            {
                { "playerName", friend.playerName },
                { "addedTime", friend.addedTime },
                { "isOnline", friend.isOnline },
                { "lastSeen", friend.lastSeen },
                { "friendshipLevel", friend.friendshipLevel }
            });
        }
        return friends;
    }

    // 获取待处理申请
    public Array<string> GetPendingRequests()
    {
        var requests = new Array<string>();
        foreach (var request in _pendingRequests.Keys)
        {
            requests.Add(request);
        }
        return requests;
    }

    // 获取聊天记录
    public Godot.Collections.Array GetChatHistory(string friendName)
    {
        if (_chatHistory.ContainsKey(friendName))
        {
            return _chatHistory[friendName];
        }
        return new Godot.Collections.Array();
    }

    // 获取好友数量
    public int GetFriendCount() => _friends.Count;

    // 获取好友等级
    public int GetFriendshipLevel(string friendName)
    {
        if (_friends.ContainsKey(friendName))
        {
            return _friends[friendName].friendshipLevel;
        }
        return 0;
    }

    // 增加好感度
    public void IncreaseFriendship(string friendName, int amount)
    {
        if (_friends.ContainsKey(friendName))
        {
            _friends[friendName].friendshipLevel = Mathf.Min(
                _friends[friendName].friendshipLevel + amount, 100);
            EmitSignal(nameof(FriendListUpdated));
        }
    }

    // 移除好友
    public bool RemoveFriend(string playerName)
    {
        if (!_friends.ContainsKey(playerName)) return false;
        _friends.Remove(playerName);
        EmitSignal(nameof(FriendListUpdated));
        return true;
    }

    // 是否是好友
    public bool IsFriend(string playerName)
    {
        return _friends.ContainsKey(playerName);
    }

    // 是否被拉黑
    public bool IsBlocked(string playerName)
    {
        return _blockedPlayers.Contains(playerName);
    }

    // 私有方法
    private void AddFriend(string playerName, FriendData data)
    {
        _friends[playerName] = data;
    }

    private void AddPendingRequest(string playerName, FriendRequest request)
    {
        _pendingRequests[playerName] = request;
        EmitSignal(nameof(FriendRequestReceived), playerName, request.message);
    }

    private void AddChatMessage(string friendName, ChatMessage message)
    {
        if (!_chatHistory.ContainsKey(friendName))
        {
            _chatHistory[friendName] = new Godot.Collections.Array();
        }
        _chatHistory[friendName].Add(new Godot.Collections.Dictionary
        {
            { "fromPlayer", message.fromPlayer },
            { "toPlayer", message.toPlayer },
            { "message", message.message },
            { "timestamp", message.timestamp },
            { "isRead", message.isRead }
        });
    }

    private string GetPlayerName()
    {
        // 从游戏数据获取玩家名称
        if (HasNode("/root/GameManager"))
        {
            var gameManager = GetNode("/root/GameManager");
            var playerName = gameManager.Get("player_name") as string;
            if (playerName != null) return playerName;
        }
        return "Player";
    }

    // 存档支持
    public Dictionary SaveData()
    {
        var data = new System.Collections.Generic.Dictionary<string, object>();
        
        var friendsList = new Array<Dictionary>();
        foreach (var friend in _friends.Values)
        {
            friendsList.Add(new Dictionary
            {
                { "playerName", friend.playerName },
                { "addedTime", friend.addedTime },
                { "isOnline", friend.isOnline },
                { "lastSeen", friend.lastSeen },
                { "friendshipLevel", friend.friendshipLevel }
            });
        }
        data["friends"] = friendsList;
        data["blockedPlayers"] = _blockedPlayers;
        
        return data;
    }
    
    #region Data Persistence
    
    public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
    {
        return SaveData();
    }
    
    public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
    {
        if (data == null) return;
        LoadData(data);
    }
    
    #endregion

    public void LoadData(Dictionary data)
    {
        if (data.Contains("friends"))
        {
            _friends.Clear();
            var friendsList = data["friends"] as Array;
            foreach (Dictionary friendData in friendsList)
            {
                var friend = new FriendData
                {
                    playerName = friendData["playerName"] as string,
                    addedTime = (long)friendData["addedTime"],
                    isOnline = (bool)friendData["isOnline"],
                    lastSeen = (long)friendData["lastSeen"],
                    friendshipLevel = (int)friendData["friendshipLevel"]
                };
                _friends[friend.playerName] = friend;
            }
        }
        
        if (data.Contains("blockedPlayers"))
        {
            _blockedPlayers = data["blockedPlayers"] as Array;
        }
    }
}

public class FriendData
{
    public string playerName;
    public long addedTime;
    public bool isOnline;
    public long lastSeen;
    public int friendshipLevel;
}

public class FriendRequest
{
    public string fromPlayer;
    public string toPlayer;
    public string message;
    public long timestamp;
}

public class ChatMessage
{
    public string fromPlayer;
    public string toPlayer;
    public string message;
    public long timestamp;
    public bool isRead;
}
