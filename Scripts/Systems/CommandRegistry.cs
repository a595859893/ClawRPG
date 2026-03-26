using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems;

/// <summary>
/// 控制台命令注册表
/// 管理所有可用的控制台命令
/// </summary>
public class CommandRegistry
{
    public static CommandRegistry Instance { get; private set; } = new CommandRegistry();

    private Dictionary<string, MainDebug.ConsoleCommand> _commands = new Dictionary<string, MainDebug.ConsoleCommand>();

    private CommandRegistry() { }

    /// <summary>
    /// 注册命令
    /// </summary>
    public void Register(string name, MainDebug.ConsoleCommand command)
    {
        string key = name.ToLower();
        if (_commands.ContainsKey(key))
        {
            MainDebug.WarningPrint($"Command '{name}' already registered, overwriting.");
        }
        _commands[key] = command;
    }

    /// <summary>
    /// 获取命令
    /// </summary>
    public MainDebug.ConsoleCommand Get(string name)
    {
        string key = name.ToLower();
        if (_commands.TryGetValue(key, out var cmd))
        {
            return cmd;
        }
        return null;
    }

    /// <summary>
    /// 获取所有命令
    /// </summary>
    public Dictionary<string, MainDebug.ConsoleCommand> All()
    {
        return new Dictionary<string, MainDebug.ConsoleCommand>(_commands);
    }

    /// <summary>
    /// 命令是否存在
    /// </summary>
    public bool Exists(string name)
    {
        return _commands.ContainsKey(name.ToLower());
    }
}
