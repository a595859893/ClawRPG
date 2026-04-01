using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 宠物AI控制台界面
    /// </summary>
    public class PetAIUI
    {
        private PetAISystem _aiSystem;
        private int _selectedTab;
        private int _selectedBehaviorIndex;
        private string _selectedSummonId;
        private List<AIBehavior> _behaviors;
        private List<PetAIInstance> _activeAIs;

        // 快捷键提示
        private const string KeyHints = "[↑/↓]移动 [Enter]选择 [1-3]切换标签 [R]刷新 [ESC]退出";

        public PetAIUI()
        {
            _aiSystem = PetAISystem.Instance;
            _behaviors = PetAIDatabase.GetAllBehaviors();
            _activeAIs = new List<PetAIInstance>();
            _selectedTab = 0;
            _selectedBehaviorIndex = 0;
        }

        /// <summary>
        /// 显示宠物AI界面
        /// </summary>
        public void Show()
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                DrawHeader();
                DrawTabs();
                DrawContent();
                DrawFooter();

                var key = Console.ReadKey(true);
                running = HandleInput(key);
            }
        }

        private void DrawHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          🎯 宠物AI行为控制系统                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private void DrawTabs()
        {
            string[] tabs = { "行为配置", "活跃AI", "学习数据", "统计" };
            
            for (int i = 0; i < tabs.Length; i++)
            {
                if (i == _selectedTab)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($" ▶ {tabs[i]} ◀ ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"   {tabs[i]}   ");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine(new string('─', 70));
        }

        private void DrawContent()
        {
            switch (_selectedTab)
            {
                case 0:
                    DrawBehaviorList();
                    break;
                case 1:
                    DrawActiveAIList();
                    break;
                case 2:
                    DrawLearningData();
                    break;
                case 3:
                    DrawStatistics();
                    break;
            }
        }

        private void DrawBehaviorList()
        {
            Console.WriteLine("可用AI行为配置:");
            Console.WriteLine();

            var groupedBehaviors = _behaviors.GroupBy(b => b.Pattern);
            
            int index = 0;
            foreach (var group in groupedBehaviors)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"【{GetPatternName(group.Key)}】");
                Console.ResetColor();

                foreach (var behavior in group)
                {
                    bool isSelected = index == _selectedBehaviorIndex && _behaviors[index].Id == behavior.Id;
                    
                    if (isSelected)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("▶ ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    Console.WriteLine($"{behavior.Name}: {behavior.Description}");
                    
                    if (isSelected)
                    {
                        DrawBehaviorDetails(behavior);
                    }
                    
                    index++;
                }
                Console.WriteLine();
            }

            // 如果有活跃AI，显示分配按钮
            if (_activeAIs.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[Enter] 为选中的召唤物分配: {_behaviors[_selectedBehaviorIndex].Name}");
                Console.ResetColor();
            }
        }

        private void DrawBehaviorDetails(AIBehavior behavior)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   攻击范围: {behavior.AttackRange} | 撤退血线: {behavior.RetreatHealthPercent:P0}");
            Console.WriteLine($"   决策间隔: {behavior.DecisionInterval}ms | 攻击性: {behavior.aggressionLevel:P0}");
            Console.ResetColor();
        }

        private void DrawActiveAIList()
        {
            Console.WriteLine("活跃的宠物AI实例:");
            Console.WriteLine();

            if (_activeAIs.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  暂无活跃的宠物AI");
                Console.ResetColor();
                return;
            }

            for (int i = 0; i < _activeAIs.Count; i++)
            {
                var ai = _activeAIs[i];
                bool isSelected = _selectedBehaviorIndex == i;

                if (isSelected)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("▶ ");
                }
                else
                {
                    Console.Write("  ");
                }

                string behaviorName = ai.CurrentBehavior?.Name ?? "未分配";
                Console.WriteLine($"召唤物: {ai.SummonId} | 行为: {behaviorName} | 状态: {GetStateName(ai.State)}");
                
                if (isSelected && ai.CurrentDecision != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"   当前决策: {ai.CurrentDecision.Type} (置信度: {ai.CurrentDecision.Confidence:P0})");
                    Console.WriteLine($"   决策次数: {ai.DecisionsMade} | 正确决策: {ai.CorrectDecisions}");
                    Console.ResetColor();
                }
            }
        }

        private void DrawLearningData()
        {
            Console.WriteLine("宠物学习数据:");
            Console.WriteLine();

            var learningDatas = _aiSystem.ExportSaveData()?.LearningData;
            if (learningDatas == null || learningDatas.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  暂无学习数据");
                Console.ResetColor();
                return;
            }

            foreach (var kvp in learningDatas)
            {
                var learning = kvp.Value;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"召唤物: {kvp.Key}");
                Console.ResetColor();
                
                Console.WriteLine($"  总体适应等级: {learning.OverallAdaptation:P0}");
                Console.WriteLine($"  成功闪避: {learning.SuccessfulDodges} | 失败闪避: {learning.FailedDodges}");
                Console.WriteLine($"  聪明撤退: {learning.SmartRetreats} | 过度深入: {learning.Overextensions}");
                
                if (learning.EnemyTypeKills.Count > 0)
                {
                    Console.Write("  击杀类型: ");
                    foreach (var kill in learning.EnemyTypeKills.Take(5))
                    {
                        Console.Write($"{kill.Key}({kill.Value}) ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }

        private void DrawStatistics()
        {
            var stats = _aiSystem.GetStatistics();
            
            Console.WriteLine("宠物AI统计:");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"总决策次数: {stats.TotalDecisions}");
            Console.WriteLine($"成功决策: {stats.SuccessfulDecisions}");
            Console.WriteLine($"成功率: {stats.SuccessRate:P1}");
            Console.WriteLine($"平均适应等级: {stats.AverageAdaptationLevel:P1}");
            Console.ResetColor();
            Console.WriteLine();

            if (stats.BehaviorUsage.Count > 0)
            {
                Console.WriteLine("行为模式使用分布:");
                foreach (var kvp in stats.BehaviorUsage)
                {
                    Console.WriteLine($"  {GetPatternName(kvp.Key)}: {kvp.Value}次");
                }
                Console.WriteLine();
            }

            if (stats.DecisionDistribution.Count > 0)
            {
                Console.WriteLine("决策类型分布:");
                foreach (var kvp in stats.DecisionDistribution)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}次");
                }
            }
        }

        private void DrawFooter()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(KeyHints);
            Console.ResetColor();
        }

        private bool HandleInput(ConsoleKeyInfo key)
        {
            int maxIndex = 0;
            
            switch (_selectedTab)
            {
                case 0:
                    maxIndex = _behaviors.Count - 1;
                    break;
                case 1:
                    maxIndex = Math.Max(0, _activeAIs.Count - 1);
                    break;
                default:
                    return key.Key != ConsoleKey.Escape;
            }

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    _selectedBehaviorIndex = Math.Max(0, _selectedBehaviorIndex - 1);
                    break;
                case ConsoleKey.DownArrow:
                    _selectedBehaviorIndex = Math.Min(maxIndex, _selectedBehaviorIndex + 1);
                    break;
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    _selectedTab = 0;
                    _selectedBehaviorIndex = 0;
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    _selectedTab = 1;
                    _selectedBehaviorIndex = 0;
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    _selectedTab = 2;
                    _selectedBehaviorIndex = 0;
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    _selectedTab = 3;
                    _selectedBehaviorIndex = 0;
                    break;
                case ConsoleKey.R:
                    RefreshData();
                    break;
                case ConsoleKey.Enter:
                    if (_selectedTab == 0 && _activeAIs.Count > 0)
                    {
                        AssignBehaviorToSelectedSummon();
                    }
                    break;
                case ConsoleKey.Escape:
                    return false;
            }

            return true;
        }

        private void RefreshData()
        {
            var data = _aiSystem.ExportSaveData();
            if (data != null)
            {
                _activeAIs = data.ActivePetAIs;
            }
        }

        private void AssignBehaviorToSelectedSummon()
        {
            if (_activeAIs.Count == 0 || _selectedBehaviorIndex >= _activeAIs.Count)
                return;

            var summonId = _activeAIs[_selectedBehaviorIndex].SummonId;
            var behavior = _behaviors[_selectedBehaviorIndex];
            
            if (_aiSystem.AssignBehavior(summonId, behavior.Id))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n成功为 {summonId} 分配行为: {behavior.Name}");
                Console.ResetColor;
                System.Threading.Thread.Sleep(1000);
            }
        }

        private string GetPatternName(AIBehaviorPattern pattern)
        {
            switch (pattern)
            {
                case AIBehaviorPattern.Aggressive: return "主动攻击";
                case AIBehaviorPattern.Defensive: return "防守";
                case AIBehaviorPattern.Support: return "支援";
                case AIBehaviorPattern.Guerrilla: return "游击";
                case AIBehaviorPattern.Follow: return "跟随";
                case AIBehaviorPattern.Passive: return "被动";
                default: return pattern.ToString();
            }
        }

        private string GetStateName(ClawRPG.Scripts.Data.PetAIState state)
        {
            switch (state)
            {
                case PetAIState.Idle: return "空闲";
                case PetAIState.Patrolling: return "巡逻";
                case PetAIState.Engaging: return "战斗中";
                case PetAIState.Supporting: return "支援中";
                case PetAIState.Fleeing: return "撤退中";
                case PetAIState.Learning: return "学习中";
                default: return state.ToString();
            }
        }
    }
}
