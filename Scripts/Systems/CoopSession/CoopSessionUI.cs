using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.UI;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 合作会话UI - 管理合作冒险界面
    /// </summary>
    public class CoopSessionUI
    {
        private static CoopSessionUI _instance;
        public static CoopSessionUI Instance => _instance ??= new CoopSessionUI();

        private readonly CoopSessionSystem _system;
        private int _selectedTab;
        private string _selectedSessionId;
        private List<CoopSession> _availableSessions;
        private bool _isVisible;

        public event Action? OnToggle;

        public CoopSessionUI()
        {
            _system = CoopSessionSystem.Instance;
            _availableSessions = new List<CoopSession>();
            _selectedTab = 0;
            _selectedSessionId = "";
        }

        /// <summary>
        /// 切换UI显示
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (_isVisible)
            {
                RefreshSessions();
            }
            OnToggle?.Invoke();
        }

        /// <summary>
        /// 刷新可用会话
        /// </summary>
        public void RefreshSessions()
        {
            _availableSessions = _system.GetActiveSessions();
        }

        /// <summary>
        /// 渲染UI
        /// </summary>
        public void Render()
        {
            if (!_isVisible) return;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            🤝 合作冒险会话系统 (Coop Session)              ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.ResetColor();

            // Tab 选择
            Console.WriteLine("  [1] 可用会话    [2] 当前会话    [3] 队伍管理    [4] 历史统计");
            Console.WriteLine();

            // Tab 内容
            switch (_selectedTab)
            {
                case 0:
                    RenderAvailableSessions();
                    break;
                case 1:
                    RenderCurrentSession();
                    break;
                case 2:
                    RenderPartyManagement();
                    break;
                case 3:
                    RenderStatistics();
                    break;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  操作: [↑/↓] 选择  [1-4] 切换标签  [Enter] 确认  [ESC] 关闭");
            Console.ResetColor();
        }

        /// <summary>
        /// 渲染可用会话列表
        /// </summary>
        private void RenderAvailableSessions()
        {
            Console.WriteLine("  📋 可用会话列表:");
            Console.WriteLine();

            if (_availableSessions.Count == 0)
            {
                Console.WriteLine("  当前没有可用的合作会话。");
                Console.WriteLine();
                Console.WriteLine("  [C] 创建新会话");
                return;
            }

            for (int i = 0; i < _availableSessions.Count; i++)
            {
                var session = _availableSessions[i];
                var isSelected = session.SessionId == _selectedSessionId;
                var prefix = isSelected ? "► " : "  ";

                Console.ForegroundColor = isSelected ? ConsoleColor.Green : ConsoleColor.White;
                Console.WriteLine($"{prefix}[{i + 1}] {session.SessionName}");
                Console.ResetColor();
                Console.WriteLine($"      地下城: {session.DungeonName} | 玩家: {session.Party.Members.Count}/{session.MaxPlayers}");
                Console.WriteLine($"      模式: {GetAdventureTypeName(session.AdventureType)} | 状态: {GetStateName(session.State)}");
                Console.WriteLine();
            }

            Console.WriteLine("  [C] 创建新会话    [J] 加入选中会话    [R] 刷新");
        }

        /// <summary>
        /// 渲染当前会话
        /// </summary>
        private void RenderCurrentSession()
        {
            var current = _system.GetCurrentSession();

            if (current == null)
            {
                Console.WriteLine("  当前没有进行中的合作会话。");
                return;
            }

            Console.WriteLine($"  会话ID: {current.SessionId}");
            Console.WriteLine($"  会话名称: {current.SessionName}");
            Console.WriteLine($"  地下城: {current.DungeonName}");
            Console.WriteLine($"  当前楼层: {current.CurrentFloor}/{current.TotalFloors}");
            Console.WriteLine($"  状态: {GetStateName(current.State)}");
            Console.WriteLine();

            Console.WriteLine("  👥 队伍成员:");
            Console.WriteLine();

            foreach (var member in current.Party.Members)
            {
                Console.ForegroundColor = GetPlayerStateColor(member.State);
                Console.Write($"    [{GetPlayerStateIcon(member.State)}] ");
                Console.ResetColor();
                Console.WriteLine($"{member.PlayerName} (Lv.{member.Level})");

                if (member.State == CoopPlayerState.InDungeon)
                {
                    Console.WriteLine($"       生命: {member.HealthPercent:P0} | 房间: {member.CurrentRoomId}");
                    Console.WriteLine($"       贡献: 伤害 {member.DamageDealt} | 治疗 {member.HealingDone} | 击杀 {member.EnemiesKilled}");
                }
                Console.WriteLine();
            }

            Console.WriteLine($"  ⏱️  elapsed: {current.ElapsedTime:mm\\:ss} / {current.TimeLimitMinutes}:00");
            Console.WriteLine($"  📊 进度: 房间 {current.TotalRoomsCleared} | 击杀 {current.TotalEnemiesDefeated} | 宝藏 {current.TotalTreasuresFound}");
        }

        /// <summary>
        /// 渲染队伍管理
        /// </summary>
        private void RenderPartyManagement()
        {
            Console.WriteLine("  👥 队伍管理:");
            Console.WriteLine();

            var current = _system.GetCurrentSession();
            if (current == null)
            {
                Console.WriteLine("  当前没有进行中的会话。");
                Console.WriteLine();
                Console.WriteLine("  可用配置:");
                Console.WriteLine("  [1] 标准模式 (4人, 60分钟)");
                Console.WriteLine("  [2] 快速模式 (4人, 20分钟, 1.5x经验)");
                Console.WriteLine("  [3] 双人模式 (2人, 45分钟)");
                Console.WriteLine("  [4] 团队模式 (8人, 90分钟, 1.5x掉落)");
                Console.WriteLine();
                Console.WriteLine("  [C] 创建会话");
                return;
            }

            Console.WriteLine($"  队伍名称: {current.Party.PartyName}");
            Console.WriteLine($"  队长: {current.Party.LeaderId}");
            Console.WriteLine();

            // 队伍设置
            Console.WriteLine("  ⚙️ 队伍设置:");
            Console.WriteLine($"    经验倍率: {current.ExpMultiplier:P0}");
            Console.WriteLine($"    掉落倍率: {current.DropRateMultiplier:P0}");
            Console.WriteLine($"    快速模式: {(current.IsQuickMode ? "是" : "否")}");
            Console.WriteLine();

            Console.WriteLine("  [L] 转让队长    [K] 踢出玩家    [R] 准备    [S] 开始");
        }

        /// <summary>
        /// 渲染统计
        /// </summary>
        private void RenderStatistics()
        {
            Console.WriteLine("  📊 历史统计:");
            Console.WriteLine();

            // 这里应该从玩家数据中获取，简化显示
            Console.WriteLine("  你的合作冒险统计:");
            Console.WriteLine("  ─────────────────");
            Console.WriteLine("  总参与次数: -");
            Console.WriteLine("  完成次数: -");
            Console.WriteLine("  胜利次数: -");
            Console.WriteLine("  总获得经验: -");
            Console.WriteLine("  总获得金币: -");
            Console.WriteLine();

            Console.WriteLine("  [R] 刷新统计");
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        public bool HandleInput(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    _selectedTab = 0;
                    return true;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    _selectedTab = 1;
                    return true;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    _selectedTab = 2;
                    return true;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    _selectedTab = 3;
                    return true;
                case ConsoleKey.UpArrow:
                    MoveSelection(-1);
                    return true;
                case ConsoleKey.DownArrow:
                    MoveSelection(1);
                    return true;
                case ConsoleKey.R:
                    RefreshSessions();
                    return true;
                case ConsoleKey.C:
                    // 创建会话
                    return true;
                case ConsoleKey.Escape:
                    _isVisible = false;
                    OnToggle?.Invoke();
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 移动选择
        /// </summary>
        private void MoveSelection(int direction)
        {
            if (_availableSessions.Count == 0) return;

            int currentIndex = _selectedSessionId == "" ? -1 : 
                _availableSessions.FindIndex(s => s.SessionId == _selectedSessionId);

            int newIndex = currentIndex + direction;
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= _availableSessions.Count) newIndex = _availableSessions.Count - 1;

            _selectedSessionId = _availableSessions[newIndex].SessionId;
        }

        private string GetAdventureTypeName(CoopAdventureType type) => type switch
        {
            CoopAdventureType.Standard => "标准冒险",
            CoopAdventureType.Rush => "速通模式",
            CoopAdventureType.Challenge => "挑战模式",
            CoopAdventureType.Event => "活动模式",
            _ => "未知"
        };

        private string GetStateName(CoopSessionState state) => state switch
        {
            CoopSessionState.None => "无",
            CoopSessionState.Forming => "组队中",
            CoopSessionState.Starting => "即将开始",
            CoopSessionState.InProgress => "进行中",
            CoopSessionState.Paused => "暂停",
            CoopSessionState.Completed => "已完成",
            CoopSessionState.Failed => "失败",
            CoopSessionState.Cancelled => "已取消",
            _ => "未知"
        };

        private string GetPlayerStateIcon(CoopPlayerState state) => state switch
        {
            CoopPlayerState.Waiting => "⏳",
            CoopPlayerState.Ready => "✅",
            CoopPlayerState.InDungeon => "⚔️",
            CoopPlayerState.Dead => "💀",
            CoopPlayerState.Disconnected => "❌",
            _ => "❓"
        };

        private ConsoleColor GetPlayerStateColor(CoopPlayerState state) => state switch
        {
            CoopPlayerState.Waiting => ConsoleColor.Yellow,
            CoopPlayerState.Ready => ConsoleColor.Green,
            CoopPlayerState.InDungeon => ConsoleColor.Cyan,
            CoopPlayerState.Dead => ConsoleColor.Red,
            CoopPlayerState.Disconnected => ConsoleColor.DarkGray,
            _ => ConsoleColor.White
        };
    }
}
