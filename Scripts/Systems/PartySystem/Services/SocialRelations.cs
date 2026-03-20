using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PartySystem
{

/// <summary>
/// PartySystem 社交关系服务
/// 负责好友、黑名单、社交数据等逻辑
/// </summary>
public class SocialRelations
{
    private Dictionary<int, PartyData.PlayerPartyData> _playerData;

    // Signals for social events
    private Signal _friendAdded;
    private Signal _friendRemoved;
    private Signal _playerBlocked;
    private Signal _playerUnblocked;

    public SocialRelations(
        ref Dictionary<int, PartyData.PlayerPartyData> playerData,
        Signal friendAdded,
        Signal friendRemoved,
        Signal playerBlocked,
        Signal playerUnblocked)
    {
        _playerData = playerData;
        _friendAdded = friendAdded;
        _friendRemoved = friendRemoved;
        _playerBlocked = playerBlocked;
        _playerUnblocked = playerUnblocked;
    }

    private void EnsurePlayerData(int playerId)
    {
        if (!_playerData.ContainsKey(playerId))
        {
            _playerData[playerId] = new PartyData.PlayerPartyData();
        }
    }

    /// <summary>
    /// 添加好友
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="friendId">好友ID</param>
    /// <returns>是否添加成功</returns>
    public bool AddFriend(int playerId, int friendId)
    {
        if (playerId == friendId)
            return false;

        EnsurePlayerData(playerId);
        var data = _playerData[playerId];

        // Check if already friends
        if (data.Friends.Contains(friendId))
            return false;

        // Check if blocked
        if (data.Blacklist.Contains(friendId))
            return false;

        data.Friends.Add(friendId);
        _friendAdded.Emit(playerId, friendId);
        GD.Print($"[SocialRelations] Player {playerId} added friend {friendId}");

        return true;
    }

    /// <summary>
    /// 移除好友
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="friendId">好友ID</param>
    /// <returns>是否移除成功</returns>
    public bool RemoveFriend(int playerId, int friendId)
    {
        EnsurePlayerData(playerId);
        var data = _playerData[playerId];

        if (!data.Friends.Contains(friendId))
            return false;

        data.Friends.Remove(friendId);
        _friendRemoved.Emit(playerId, friendId);
        GD.Print($"[SocialRelations] Player {playerId} removed friend {friendId}");

        return true;
    }

    /// <summary>
    /// 检查是否是好友
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="otherId">对方ID</param>
    /// <returns>是否是好友</returns>
    public bool IsFriend(int playerId, int otherId)
    {
        if (!_playerData.ContainsKey(playerId))
            return false;

        return _playerData[playerId].Friends.Contains(otherId);
    }

    /// <summary>
    /// 获取所有好友列表
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <returns>好友ID列表</returns>
    public List<int> GetFriends(int playerId)
    {
        if (!_playerData.ContainsKey(playerId))
            return new List<int>();

        return new List<int>(_playerData[playerId].Friends);
    }

    /// <summary>
    /// 添加到黑名单
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="blockedId">被屏蔽的玩家ID</param>
    /// <returns>是否添加成功</returns>
    public bool BlockPlayer(int playerId, int blockedId)
    {
        if (playerId == blockedId)
            return false;

        EnsurePlayerData(playerId);
        var data = _playerData[playerId];

        // Check if already blocked
        if (data.Blacklist.Contains(blockedId))
            return false;

        data.Blacklist.Add(blockedId);

        // Also remove from friends if they were friends
        if (data.Friends.Contains(blockedId))
        {
            data.Friends.Remove(blockedId);
            _friendRemoved.Emit(playerId, blockedId);
        }

        _playerBlocked.Emit(playerId, blockedId);
        GD.Print($"[SocialRelations] Player {playerId} blocked player {blockedId}");

        return true;
    }

    /// <summary>
    /// 从黑名单移除
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="unblockedId">解除屏蔽的玩家ID</param>
    /// <returns>是否移除成功</returns>
    public bool UnblockPlayer(int playerId, int unblockedId)
    {
        EnsurePlayerData(playerId);
        var data = _playerData[playerId];

        if (!data.Blacklist.Contains(unblockedId))
            return false;

        data.Blacklist.Remove(unblockedId);
        _playerUnblocked.Emit(playerId, unblockedId);
        GD.Print($"[SocialRelations] Player {playerId} unblocked player {unblockedId}");

        return true;
    }

    /// <summary>
    /// 检查是否在黑名单中
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="otherId">对方ID</param>
    /// <returns>是否在黑名单中</returns>
    public bool IsBlocked(int playerId, int otherId)
    {
        if (!_playerData.ContainsKey(playerId))
            return false;

        return _playerData[playerId].Blacklist.Contains(otherId);
    }

    /// <summary>
    /// 获取黑名单列表
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <returns>黑名单ID列表</returns>
    public List<int> GetBlacklist(int playerId)
    {
        if (!_playerData.ContainsKey(playerId))
            return new List<int>();

        return new List<int>(_playerData[playerId].Blacklist);
    }

    /// <summary>
    /// 检查两个玩家之间的关系
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <param name="otherId">对方ID</param>
    /// <returns>关系类型: 0=无关系, 1=好友, 2=黑名单</returns>
    public int GetRelationStatus(int playerId, int otherId)
    {
        if (!_playerData.ContainsKey(playerId))
            return 0;

        var data = _playerData[playerId];

        if (data.Blacklist.Contains(otherId))
            return 2; // Blocked

        if (data.Friends.Contains(otherId))
            return 1; // Friend

        return 0; // No relation
    }

    /// <summary>
    /// 获取玩家社交数据
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <returns>玩家社交数据</returns>
    public PartyData.PlayerPartyData GetPlayerData(int playerId)
    {
        EnsurePlayerData(playerId);
        return _playerData[playerId];
    }

    /// <summary>
    /// 获取好友数量
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <returns>好友数量</returns>
    public int GetFriendCount(int playerId)
    {
        if (!_playerData.ContainsKey(playerId))
            return 0;

        return _playerData[playerId].Friends.Count;
    }

    /// <summary>
    /// 获取黑名单数量
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    /// <returns>黑名单数量</returns>
    public int GetBlacklistCount(int playerId)
    {
        if (!_playerData.ContainsKey(playerId))
            return 0;

        return _playerData[playerId].Blacklist.Count;
    }

    /// <summary>
    /// 清除所有好友和黑名单（用于重置）
    /// </summary>
    /// <param name="playerId">玩家ID</param>
    public void ClearSocialData(int playerId)
    {
        EnsurePlayerData(playerId);
        var data = _playerData[playerId];
        data.Friends.Clear();
        data.Blacklist.Clear();
        GD.Print($"[SocialRelations] Player {playerId} cleared all social data");
    }
}
}
