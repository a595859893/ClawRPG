using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems;

/// <summary>
/// 控制台历史记录管理 - REQ-096-03
/// 支持内存历史 + 持久化 (ConfigFile) + FIFO 限制
/// </summary>
public class ConsoleHistory
{
    private const int MaxHistorySize = 50;
    private const string ConfigSection = "console";
    private const string HistoryKey = "command_history";

    private readonly List<string> _history = new List<string>();
    private int _navigationIndex = -1;
    private string _configPath => "user://clawrpg_console.cfg".ToGodotPath();

    // REQ-125: Search state — persists filtered navigation
    private List<string> _searchResults = new List<string>();
    private int _searchNavigationIndex = -1;

    /// <summary>
    /// 加载历史记录（从本地 ConfigFile）
    /// </summary>
    public void Load()
    {
        _history.Clear();
        _navigationIndex = -1;

        var cfg = new ConfigFile();
        Error err = cfg.Load(_configPath);
        if (err != Error.Ok)
            return;

        var historyStr = cfg.GetValue(ConfigSection, HistoryKey, "") as string;
        if (string.IsNullOrEmpty(historyStr))
            return;

        var parts = historyStr.Split('\x1F'); // ASCII RS 作为分隔符
        foreach (var cmd in parts)
        {
            if (!string.IsNullOrWhiteSpace(cmd))
                _history.Add(cmd);
        }
    }

    /// <summary>
    /// 保存历史记录（到本地 ConfigFile）
    /// </summary>
    public void Save()
    {
        var cfg = new ConfigFile();
        // 先加载现有配置（保留其他设置）
        cfg.Load(_configPath);

        string data = string.Join("\x1F", _history.Take(MaxHistorySize));
        cfg.SetValue(ConfigSection, HistoryKey, data);

        // 保存到用户目录（不加密，debug 专用）
        cfg.Save(_configPath);
    }

    /// <summary>
    /// 添加命令到历史（完整去重 — 扫描全部历史，REQ-125 Gap #2）
    /// </summary>
    public void Add(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        string trimmed = command.Trim();

        // 扫描全部历史去重，不只是最近一条
        if (_history.Contains(trimmed))
            _history.Remove(trimmed);

        _history.Add(trimmed);

        // FIFO 限制
        while (_history.Count > MaxHistorySize)
            _history.RemoveAt(0);

        _navigationIndex = -1;
        ClearSearch();
    }

    /// <summary>
    /// 获取历史列表（用于显示）
    /// </summary>
    public IReadOnlyList<string> All() => _history;

    /// <summary>
    /// 导航历史：direction=-1 往旧，direction=+1 往新
    /// 当有活跃搜索时，只在搜索结果中导航
    /// 返回 null 表示越界（回到当前输入）
    /// </summary>
    public string Navigate(int direction)
    {
        // Use search results if a search is active
        if (_searchResults.Count > 0)
            return NavigateSearch(direction);

        if (_history.Count == 0)
            return null;

        int newIndex = _navigationIndex + direction;

        // REQ-125 Gap #3: at oldest end (index = count-1) and pressing Down → return null (current input)
        if (newIndex >= _history.Count)
        {
            _navigationIndex = -1;
            return null;
        }
        if (newIndex < -1)
        {
            _navigationIndex = -1;
            return null;
        }

        _navigationIndex = newIndex;

        if (_navigationIndex == -1)
            return null; // 回到当前输入

        return _history[_history.Count - 1 - _navigationIndex];
    }

    private string NavigateSearch(int direction)
    {
        int newIndex = _searchNavigationIndex + direction;

        // At oldest end and pressing Down → return null (current input)
        if (newIndex >= _searchResults.Count)
        {
            _searchNavigationIndex = -1;
            return null;
        }
        if (newIndex < -1)
        {
            _searchNavigationIndex = -1;
            return null;
        }

        _searchNavigationIndex = newIndex;

        if (_searchNavigationIndex == -1)
            return null;

        return _searchResults[_searchResults.Count - 1 - _searchNavigationIndex];
    }

    /// <summary>
    /// 重置导航位置
    /// </summary>
    public void ResetNavigation()
    {
        _navigationIndex = -1;
        _searchNavigationIndex = -1;
    }

    /// <summary>
    /// REQ-125 Gap #1: 部分匹配搜索历史（大小写不敏感）
    /// 输入 "att" 能找到 "attack"、"defend" 等所有含 "att" 的历史
    /// </summary>
    public IReadOnlyList<string> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            ClearSearch();
            return _history.AsReadOnly();
        }

        string lower = query.ToLower();
        _searchResults = _history
            .Where(cmd => cmd.ToLower().Contains(lower))
            .ToList();
        _searchNavigationIndex = -1;
        return _searchResults.AsReadOnly();
    }

    /// <summary>
    /// 清除搜索状态，回到完整历史
    /// </summary>
    public void ClearSearch()
    {
        _searchResults.Clear();
        _searchNavigationIndex = -1;
    }

    /// <summary>
    /// 是否有活跃搜索
    /// </summary>
    public bool IsSearchActive => _searchResults.Count > 0;

    /// <summary>
    /// Tab 补全：返回匹配的命令名或 null
    /// </summary>
    public string[] GetCompletions(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return Array.Empty<string>();

        prefix = prefix.ToLower();
        var allCmds = CommandRegistry.Instance.All();
        return allCmds.Keys
            .Where(k => k.StartsWith(prefix))
            .OrderBy(k => k)
            .ToArray();
    }
}
