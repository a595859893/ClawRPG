using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.UI;
using ClawRPG.Scripts.Managers;

/// <summary>
/// 控制台命令实现 - REQ-096-02
/// 提供所有基础调试命令的实现
/// </summary>
namespace ClawRPG.Scripts.Systems.Commands
{
    /// <summary>
    /// God Mode 状态管理
    /// </summary>
    public static class GodModeManager
    {
        public static bool IsEnabled { get; set; } = false;

        public static void Toggle()
        {
            IsEnabled = !IsEnabled;
            if (DebugConsoleUI.Instance != null)
            {
                var msg = IsEnabled ? "[color=#4CAF50]God Mode ENABLED[/color]" : "[color=#FF5722]God Mode DISABLED[/color]";
                DebugConsoleUI.Instance.PrintLine(msg);
            }
            MainDebug.InfoPrint($"God Mode toggled: {IsEnabled}");
        }

        public static bool IsGodModeActive() => IsEnabled;
    }

    /// <summary>
    /// 控制台命令注册器 - 将所有 REQ-096-02 命令注册到 CommandRegistry
    /// </summary>
    public static class ConsoleCommandRegistrar
    {
        public static void RegisterAll()
        {
            RegisterSpawnCommand();
            RegisterGodModeCommand();
            RegisterGoldCommand();
            RegisterWarpCommand();
            RegisterKillCommand();
            RegisterSpeedCommand();
            RegisterHealCommand();
        }

        private static void Log(string msg)
        {
            MainDebug.DebugPrint($"[ConsoleCommands] {msg}");
        }

        private static void Print(string msg, Color? color = null)
        {
            if (DebugConsoleUI.Instance != null)
            {
                DebugConsoleUI.Instance.PrintLine(msg, color ?? new Color(0.85f, 0.85f, 0.9f));
            }
        }

        private static void PrintError(string msg)
        {
            if (DebugConsoleUI.Instance != null)
            {
                DebugConsoleUI.Instance.PrintLine($"[color=#FF5252]Error: {msg}[/color]");
            }
        }

        private static void PrintSuccess(string msg)
        {
            if (DebugConsoleUI.Instance != null)
            {
                DebugConsoleUI.Instance.PrintLine($"[color=#4CAF50]{msg}[/color]");
            }
        }

        // /spawn <entity_type> [count]
        private static void RegisterSpawnCommand()
        {
            CommandRegistry.Instance.Register("spawn", new MainDebug.ConsoleCommand(
                "spawn",
                "生成敌人: /spawn [type] [count]",
                (args) =>
                {
                    if (args.Length == 0)
                    {
                        // 无参数：使用默认敌人生成一个
                        var esm = EnemySpawnManager.Instance;
                        if (esm != null)
                        {
                            esm.SpawnEnemy();
                            Print("生成 1 个默认敌人");
                        }
                        else
                        {
                            PrintError("EnemySpawnManager 不可用");
                        }
                        return;
                    }

                    string enemyType = args[0];
                    int count = 1;

                    // 解析数量（如果提供了）
                    if (args.Length >= 2 && int.TryParse(args[1], out int parsedCount))
                    {
                        count = Mathf.Max(1, parsedCount);
                    }

                    var spawnManager = EnemySpawnManager.Instance;
                    if (spawnManager == null)
                    {
                        PrintError("EnemySpawnManager 不可用");
                        return;
                    }

                    if (count == 1)
                    {
                        var enemy = spawnManager.SpawnEnemy(enemyType);
                        if (enemy != null)
                        {
                            PrintSuccess($"生成敌人: {enemyType}");
                        }
                        else
                        {
                            PrintError($"无法生成敌人类型: {enemyType}");
                        }
                    }
                    else
                    {
                        var enemies = spawnManager.SpawnEnemies(count, enemyType);
                        PrintSuccess($"生成 {enemies.Count} 个 {enemyType}");
                    }
                }
            ));
        }

        // /godmode
        private static void RegisterGodModeCommand()
        {
            CommandRegistry.Instance.Register("godmode", new MainDebug.ConsoleCommand(
                "godmode",
                "开关无敌模式",
                (args) =>
                {
                    GodModeManager.Toggle();
                }
            ));
        }

        // /gold <amount>
        private static void RegisterGoldCommand()
        {
            CommandRegistry.Instance.Register("gold", new MainDebug.ConsoleCommand(
                "gold",
                "添加金币: /gold <amount>",
                (args) =>
                {
                    if (args.Length < 1)
                    {
                        PrintError("用法: /gold <数量>");
                        return;
                    }

                    if (!int.TryParse(args[0], out int amount))
                    {
                        PrintError($"无效的金币数量: {args[0]}，请输入整数");
                        return;
                    }

                    var player = PlayerSpawnManager.Instance?.GetPlayer();
                    if (player == null)
                    {
                        PrintError("玩家不存在");
                        return;
                    }

                    // Player 是否有 Gold 属性？
                    // 使用反射安全获取
                    var goldProp = player.GetType().GetProperty("Gold");
                    if (goldProp != null && goldProp.CanWrite)
                    {
                        int currentGold = (int)(goldProp.GetValue(player) ?? 0);
                        goldProp.SetValue(player, Mathf.Max(0, currentGold + amount));
                        PrintSuccess($"金币 +{amount} (当前: {currentGold + amount})");
                    }
                    else
                    {
                        PrintError("无法访问玩家金币 (Player.Gold 属性不存在)");
                    }
                }
            ));
        }

        // /warp <scene_id>
        private static void RegisterWarpCommand()
        {
            CommandRegistry.Instance.Register("warp", new MainDebug.ConsoleCommand(
                "warp",
                "切换场景: /warp <scene_id>",
                (args) =>
                {
                    if (args.Length < 1)
                    {
                        PrintError("用法: /warp <scene_id>");
                        return;
                    }

                    string sceneId = args[0];
                    var sceneManager = SceneManager.Instance;

                    if (sceneManager == null)
                    {
                        PrintError("SceneManager 不可用");
                        return;
                    }

                    // 尝试多种路径格式
                    string[] paths = {
                        $"res://Scenes/{sceneId}.tscn",
                        $"res://{sceneId}.tscn",
                        sceneId
                    };

                    string validPath = null;
                    foreach (var path in paths)
                    {
                        if (ResourceLoader.Exists(path))
                        {
                            validPath = path;
                            break;
                        }
                    }

                    if (validPath != null)
                    {
                        Print($"传送中: {validPath}");
                        sceneManager.ChangeScene(validPath);
                        PrintSuccess($"已传送到: {sceneId}");
                    }
                    else
                    {
                        PrintError($"场景不存在: {sceneId}");
                        Print($"尝试的路径: {string.Join(", ", paths)}");
                    }
                }
            ));
        }

        // /kill_all
        private static void RegisterKillCommand()
        {
            CommandRegistry.Instance.Register("kill_all", new MainDebug.ConsoleCommand(
                "kill_all",
                "击杀所有敌人",
                (args) =>
                {
                    var spawnManager = EnemySpawnManager.Instance;
                    if (spawnManager == null)
                    {
                        PrintError("EnemySpawnManager 不可用");
                        return;
                    }

                    int count = spawnManager.GetEnemyCount();
                    if (count == 0)
                    {
                        Print("没有敌人需要击杀");
                        return;
                    }

                    var container = spawnManager.GetEnemyContainer();
                    if (container != null)
                    {
                        // 遍历并移除所有敌人子节点
                        int removed = 0;
                        foreach (Node child in container.GetChildren())
                        {
                            if (child is CharacterBody2D || child is Enemy)
                            {
                                child.QueueFree();
                                removed++;
                            }
                        }
                        PrintSuccess($"已击杀 {removed} 个敌人");
                    }
                    else
                    {
                        PrintError("无法获取敌人容器");
                    }
                }
            ));
        }

        // /speed <1-10>
        private static void RegisterSpeedCommand()
        {
            CommandRegistry.Instance.Register("speed", new MainDebug.ConsoleCommand(
                "speed",
                "设置玩家速度: /speed <1-10>",
                (args) =>
                {
                    if (args.Length < 1)
                    {
                        PrintError("用法: /speed <1-10>");
                        return;
                    }

                    if (!float.TryParse(args[0], out float speed))
                    {
                        PrintError($"无效的速度值: {args[0]}，请输入数字");
                        return;
                    }

                    // 限制速度范围 50-1000
                    speed = Mathf.Clamp(speed, 50f, 1000f);

                    var player = PlayerSpawnManager.Instance?.GetPlayer();
                    if (player == null)
                    {
                        PrintError("玩家不存在");
                        return;
                    }

                    // 找到 Speed 属性并设置
                    var speedProp = player.GetType().GetProperty("Speed");
                    if (speedProp != null && speedProp.CanWrite)
                    {
                        speedProp.SetValue(player, speed);
                        PrintSuccess($"玩家速度设置为: {speed}");
                    }
                    else
                    {
                        // 尝试直接字段访问
                        PrintError("无法设置玩家速度");
                    }
                }
            ));
        }

        // /heal
        private static void RegisterHealCommand()
        {
            CommandRegistry.Instance.Register("heal", new MainDebug.ConsoleCommand(
                "heal",
                "治愈玩家: /heal [amount]",
                (args) =>
                {
                    float amount = 100f;
                    if (args.Length >= 1 && float.TryParse(args[0], out float parsed))
                    {
                        amount = parsed;
                    }

                    var player = PlayerSpawnManager.Instance?.GetPlayer();
                    if (player == null)
                    {
                        PrintError("玩家不存在");
                        return;
                    }

                    // 通过反射查找 Heal 方法或 CurrentHealth/MaxHealth
                    var healMethod = player.GetType().GetMethod("Heal");
                    if (healMethod != null)
                    {
                        healMethod.Invoke(player, new object[] { amount });
                        PrintSuccess($"治愈 {amount} HP");
                    }
                    else
                    {
                        // 尝试修改 CurrentHealth
                        var healthProp = player.GetType().GetProperty("CurrentHealth");
                        var maxHealthProp = player.GetType().GetProperty("MaxHealth");
                        if (healthProp != null && maxHealthProp != null)
                        {
                            float current = Convert.ToSingle(healthProp.GetValue(player) ?? 0);
                            float max = Convert.ToSingle(maxHealthProp.GetValue(player) ?? 100);
                            float newHealth = Mathf.Min(current + amount, max);
                            healthProp.SetValue(player, newHealth);
                            PrintSuccess($"HP {current} → {newHealth}");
                        }
                        else
                        {
                            PrintError("无法治愈玩家 (找不到生命值属性)");
                        }
                    }
                }
            ));
        }
    }
}
