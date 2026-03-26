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
    /// 添加命令到历史（不重复最近的命令）
    /// </summary>
    public void Add(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        string trimmed = command.Trim();

        // 不重复最近一条
        if (_history.Count > 0 && _history[^1] == trimmed)
            return;

        _history.Add(trimmed);

        // FIFO 限制
        while (_history.Count > MaxHistorySize)
            _history.RemoveAt(0);

        _navigationIndex = -1;
    }

    /// <summary>
    /// 获取历史列表（用于显示）
    /// </summary>
    public IReadOnlyList<string> All() => _history;

    /// <summary>
    /// 导航历史：direction=-1 往旧，direction=+1 往新
    /// 返回 null 表示越界（回到当前输入）
    /// </summary>
    public string Navigate(int direction)
    {
        if (_history.Count == 0)
            return null;

        int newIndex = _navigationIndex + direction;

        if (newIndex < -1)
        {
            _navigationIndex = -1;
            return null;
        }
        if (newIndex >= _history.Count)
        {
            _navigationIndex = _history.Count - 1;
            return _history[^1];
        }

        _navigationIndex = newIndex;

        if (_navigationIndex == -1)
            return null; // 回到当前输入

        return _history[_history.Count - 1 - _navigationIndex];
    }

    /// <summary>
    /// 重置导航位置
    /// </summary>
    public void ResetNavigation()
    {
        _navigationIndex = -1;
    }

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
