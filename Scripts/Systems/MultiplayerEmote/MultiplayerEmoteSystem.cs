using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 多人表情系统
/// 处理表情使用、同步和显示
/// </summary>
public class MultiplayerEmoteSystem : Node
{
    public static MultiplayerEmoteSystem Instance { get; private set; }

    // 信号事件
    public delegate void EmoteUsedEvent(int playerId, string playerName, EmoteType emote);
    public delegate void EmoteReceivedEvent(int playerId, string playerName, EmoteType emote, Vector2 position);
    public delegate void EmoteUnlockedEvent(EmoteType emote);
    public delegate void ComboChangedEvent(int combo);

    public event EmoteUsedEvent OnEmoteUsed;
    public event EmoteReceivedEvent OnEmoteReceived;
    public event EmoteUnlockedEvent OnEmoteUnlocked;
    public event ComboChangedEvent OnComboChanged;

    // 玩家数据
    private Dictionary<int, PlayerEmoteData> _playerEmoteData = new Dictionary<int, PlayerEmoteData>();

    // 统计数据
    private EmoteStatistics _statistics = new EmoteStatistics();

    // 表情冷却
    private float _emoteCooldown = 0.5f;
    private float _lastEmoteTime = 0;

    // 表情显示
    private List<EmoteRecord> _recentEmotes = new List<EmoteRecord>();
    private float _emoteDisplayDuration = 5.0f;

    // 连击系统
    private int _currentCombo = 0;
    private float _comboTimeWindow = 3.0f;
    private float _lastEmoteComboTime = 0;

    // 当前显示的表情
    private Dictionary<int, float> _activeEmoteTimers = new Dictionary<int, float>();

    public EmoteStatistics Statistics => _statistics;
    public int CurrentCombo => _currentCombo;

    public override void _Ready()
    {
        Instance = this;
        InitializeStatistics();
    }

    public override void _Process(float delta)
    {
        // 更新活跃表情计时器
        UpdateActiveEmotes(delta);

        // 清理过期表情显示
        CleanExpiredEmotes(delta);
    }

    /// <summary>
    /// 初始化统计数据
    /// </summary>
    private void InitializeStatistics()
    {
        _statistics = new EmoteStatistics();
        foreach (EmoteCategory category in Enum.GetValues(typeof(EmoteCategory)))
        {
            _statistics.CategoryUsage[category] = 0;
        }
        foreach (EmoteType emote in Enum.GetValues(typeof(EmoteType)))
        {
            _statistics.EmoteUsage[emote] = 0;
        }
    }

    /// <summary>
    /// 使用表情
    /// </summary>
    public void UseEmote(EmoteType emote, Vector2 position)
    {
        float currentTime = Time.GetUnixTimeFromSystem();

        // 冷却检查
        if (currentTime - _lastEmoteTime < _emoteCooldown)
        {
            return;
        }

        // 获取玩家ID
        int playerId = GetLocalPlayerId();
        string playerName = GetLocalPlayerName();

        // 检查是否解锁
        if (!IsEmoteUnlocked(emote, playerId))
        {
            return;
        }

        // 更新冷却
        _lastEmoteTime = currentTime;

        // 记录使用
        RecordEmoteUsage(playerId, emote);

        // 更新连击
        UpdateCombo(currentTime);

        // 创建显示记录
        var record = new EmoteRecord
        {
            PlayerId = playerId,
            PlayerName = playerName,
            Emote = emote,
            Position = position,
            Timestamp = currentTime
        };

        _recentEmotes.Add(record);
        _activeEmoteTimers[playerId] = MultiplayerEmoteDatabase.Instance.GetEmoteConfig(emote).Duration;

        // 发送网络消息
        SendEmoteToNetwork(emote, position);

        // 触发事件
        OnEmoteUsed?.Invoke(playerId, playerName, emote);

        GD.Print($"[MultiplayerEmote] Player {playerName} used emote: {emote}");
    }

    /// <summary>
    /// 从网络接收表情
    /// </summary>
    public void ReceiveEmoteFromNetwork(int playerId, string playerName, EmoteType emote, Vector2 position)
    {
        float currentTime = Time.GetUnixTimeFromSystem();

        // 记录使用
        RecordEmoteUsage(playerId, emote);

        // 创建显示记录
        var record = new EmoteRecord
        {
            PlayerId = playerId,
            PlayerName = playerName,
            Emote = emote,
            Position = position,
            Timestamp = currentTime
        };

        _recentEmotes.Add(record);
        _activeEmoteTimers[playerId] = MultiplayerEmoteDatabase.Instance.GetEmoteConfig(emote).Duration;

        // 触发事件
        OnEmoteReceived?.Invoke(playerId, playerName, emote, position);

        GD.Print($"[MultiplayerEmote] Received emote from {playerName}: {emote}");
    }

    /// <summary>
    /// 记录表情使用
    /// </summary>
    private void RecordEmoteUsage(int playerId, EmoteType emote)
    {
        // 更新全局统计
        _statistics.TotalEmotesUsed++;

        var config = MultiplayerEmoteDatabase.Instance.GetEmoteConfig(emote);
        if (config != null)
        {
            if (_statistics.CategoryUsage.ContainsKey(config.Category))
                _statistics.CategoryUsage[config.Category]++;
            else
                _statistics.CategoryUsage[config.Category] = 1;
        }

        if (_statistics.EmoteUsage.ContainsKey(emote))
            _statistics.EmoteUsage[emote]++;
        else
            _statistics.EmoteUsage[emote] = 1;

        // 更新最常用表情
        UpdateMostUsedEmote();

        // 更新玩家数据
        if (!_playerEmoteData.ContainsKey(playerId))
        {
            _playerEmoteData[playerId] = new PlayerEmoteData { PlayerId = playerId };
        }

        var playerData = _playerEmoteData[playerId];
        if (playerData.EmoteUsageCount.ContainsKey(emote))
            playerData.EmoteUsageCount[emote]++;
        else
            playerData.EmoteUsageCount[emote] = 1;

        playerData.LastEmote = emote;
        playerData.LastEmoteTime = Time.GetUnixTimeFromSystem();
    }

    /// <summary>
    /// 更新连击
    /// </summary>
    private void UpdateCombo(float currentTime)
    {
        if (currentTime - _lastEmoteComboTime <= _comboTimeWindow)
        {
            _currentCombo++;
        }
        else
        {
            _currentCombo = 1;
        }

        _lastEmoteComboTime = currentTime;

        if (_currentCombo > _statistics.MaxComboEmotes)
        {
            _statistics.MaxComboEmotes = _currentCombo;
        }

        OnComboChanged?.Invoke(_currentCombo);
    }

    /// <summary>
    /// 更新最常用表情
    /// </summary>
    private void UpdateMostUsedEmote()
    {
        int maxCount = 0;
        EmoteType maxEmote = EmoteType.Wave;

        foreach (var kvp in _statistics.EmoteUsage)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                maxEmote = kvp.Key;
            }
        }

        _statistics.MostUsedEmote = maxEmote;
    }

    /// <summary>
    /// 更新活跃表情计时器
    /// </summary>
    private void UpdateActiveEmotes(float delta)
    {
        List<int> finishedPlayers = new List<int>();

        foreach (var kvp in _activeEmoteTimers)
        {
            float remaining = kvp.Value - delta;
            if (remaining <= 0)
            {
                finishedPlayers.Add(kvp.Key);
            }
            else
            {
                _activeEmoteTimers[kvp.Key] = remaining;
            }
        }

        foreach (int playerId in finishedPlayers)
        {
            _activeEmoteTimers.Remove(playerId);
        }
    }

    /// <summary>
    /// 清理过期表情显示
    /// </summary>
    private void CleanExpiredEmotes(float delta)
    {
        float currentTime = Time.GetUnixTimeFromSystem();
        _recentEmotes.RemoveAll(r => currentTime - r.Timestamp > _emoteDisplayDuration);
    }

    /// <summary>
    /// 发送表情到网络
    /// </summary>
    private void SendEmoteToNetwork(EmoteType emote, Vector2 position)
    {
        if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsInRoom)
            return;

        var message = new Dictionary<string, object>
        {
            { "type", "emote" },
            { "emote_type", (int)emote },
            { "position", new Dictionary<string, float> { { "x", position.X }, { "y", position.Y } } }
        };

        // NetworkClient.Instance.SendJson(message); // 需要网络客户端支持
    }

    /// <summary>
    /// 检查表情是否解锁
    /// </summary>
    public bool IsEmoteUnlocked(EmoteType emote, int playerId)
    {
        if (_playerEmoteData.ContainsKey(playerId))
        {
            return _playerEmoteData[playerId].UnlockedEmotes.Contains(emote);
        }

        // 默认解锁基于等级
        int playerLevel = GetPlayerLevel(playerId);
        var unlockedEmotes = MultiplayerEmoteDatabase.Instance.GetUnlockedEmotesByLevel(playerLevel);
        return unlockedEmotes.Contains(emote);
    }

    /// <summary>
    /// 解锁表情
    /// </summary>
    public void UnlockEmote(EmoteType emote, int playerId)
    {
        if (!_playerEmoteData.ContainsKey(playerId))
        {
            _playerEmoteData[playerId] = new PlayerEmoteData { PlayerId = playerId };
        }

        var playerData = _playerEmoteData[playerId];
        if (!playerData.UnlockedEmotes.Contains(emote))
        {
            playerData.UnlockedEmotes.Add(emote);
            OnEmoteUnlockedEvent?.Invoke(emote);
        }
    }

    /// <summary>
    /// 获取玩家可用的表情列表
    /// </summary>
    public List<EmoteType> GetAvailableEmotes(int playerId)
    {
        if (_playerEmoteData.ContainsKey(playerId))
        {
            return new List<EmoteType>(_playerEmoteData[playerId].UnlockedEmotes);
        }

        // 返回基于等级的默认解锁
        int playerLevel = GetPlayerLevel(playerId);
        return MultiplayerEmoteDatabase.Instance.GetUnlockedEmotesByLevel(playerLevel);
    }

    /// <summary>
    /// 获取最近的表情记录
    /// </summary>
    public List<EmoteRecord> GetRecentEmotes(int count = 10)
    {
        int startIndex = Math.Max(0, _recentEmotes.Count - count);
        return _recentEmotes.GetRange(startIndex, _recentEmotes.Count - startIndex);
    }

    /// <summary>
    /// 获取活跃表情
    /// </summary>
    public Dictionary<int, float> GetActiveEmotes()
    {
        return new Dictionary<int, float>(_activeEmoteTimers);
    }

    /// <summary>
    /// 检查玩家是否正在使用表情
    /// </summary>
    public bool IsPlayerUsingEmote(int playerId)
    {
        return _activeEmoteTimers.ContainsKey(playerId);
    }

    /// <summary>
    /// 获取本地玩家ID
    /// </summary>
    private int GetLocalPlayerId()
    {
        if (MultiplayerManager.Instance != null)
            return MultiplayerManager.Instance.LocalPlayerId;
        return 1;
    }

    /// <summary>
    /// 获取本地玩家名称
    /// </summary>
    private string GetLocalPlayerName()
    {
        if (MultiplayerManager.Instance != null)
            return MultiplayerManager.Instance.PlayerName;
        return "Player";
    }

    /// <summary>
    /// 获取玩家等级
    /// </summary>
    private int GetPlayerLevel(int playerId)
    {
        // 从玩家数据获取等级
        var player = GetPlayerNode();
        if (player != null)
        {
            return player.Level;
        }
        return 1;
    }

    private Node GetPlayerNode()
    {
        var root = GetTree().Root;
        foreach (Node child in root.GetChildren())
        {
            if (child is Player player)
            {
                return player;
            }
        }
        return null;
    }

    /// <summary>
    /// 导出存档数据
    /// </summary>
    public Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>
        {
            { "statistics", _statistics }
        };

        // 导出玩家数据
        var playerDataList = new List<Dictionary<string, object>>();
        foreach (var kvp in _playerEmoteData)
        {
            var pd = new Dictionary<string, object>
            {
                { "player_id", kvp.Key },
                { "unlocked_emotes", new List<int>() }
            };

            foreach (var emote in kvp.Value.UnlockedEmotes)
            {
                ((List<int>)pd["unlocked_emotes"]).Add((int)emote);
            }

            playerDataList.Add(pd);
        }
        data["player_data"] = playerDataList;

        return data;
    }

    /// <summary>
    /// 导入存档数据
    /// </summary>
    public void ImportSaveData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("statistics"))
        {
            var stats = data["statistics"] as Dictionary<string, object>;
            // 恢复统计数据
        }

        if (data.ContainsKey("player_data"))
        {
            var playerDataList = data["player_data"] as List<object>;
            foreach (var pd in playerDataList)
            {
                var pdata = pd as Dictionary<string, object>;
                int playerId = Convert.ToInt32(pdata["player_id"]);
                var unlockedEmotes = pdata["unlocked_emotes"] as List<object>;

                if (!_playerEmoteData.ContainsKey(playerId))
                {
                    _playerEmoteData[playerId] = new PlayerEmoteData { PlayerId = playerId };
                }

                foreach (var emote in unlockedEmotes)
                {
                    _playerEmoteData[playerId].UnlockedEmotes.Add((EmoteType)Convert.ToInt32(emote));
                }
            }
        }
    }

    public override void _ExitTree()
    {
        Instance = null;
    }
}
