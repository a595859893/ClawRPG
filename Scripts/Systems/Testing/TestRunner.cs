using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ClawRPG.Scripts.Systems.Testing
{
    /// <summary>
    /// 测试运行器 - 支持控制台输出、JSON 导出、CI 集成（退出码）
    /// </summary>
    public class TestRunner : Node
    {
        private const string VERSION = "1.0.0";

        public override void _Ready()
        {
            GD.Print($"=== ClawRPG Combat Test Runner v{VERSION} ===");
            RunAllTests(verbose: true);
        }

        /// <summary>
        /// 运行所有测试并输出报告
        /// </summary>
        public static TestSummary RunAllTests(bool verbose = true)
        {
            var summary = new TestSummary();
            var suite = BuildStandardSuite();
            var sw = Stopwatch.StartNew();

            // ── 基础数值测试 ──
            GD.Print("\n=== Category: Basic Numerical Tests ===");
            summary.BasicPassed = RunBasicCategory(verbose, summary);

            // ── 缩放测试 ──
            GD.Print("\n=== Category: Enemy Scaling Tests ===");
            summary.ScalingPassed = RunScalingCategory(verbose, summary);

            // ── 技能组合测试 ──
            GD.Print("\n=== Category: Skill Combo Tests ===");
            summary.SkillPassed = RunSkillCategory(verbose, summary);

            // ── DPS 基准测试 ──
            GD.Print("\n=== Category: DPS Benchmark ===");
            RunDpsBenchmarks();

            // ── 概率验证 ──
            GD.Print("\n=== Category: Probability Validation ===");
            RunProbabilityTests();

            sw.Stop();
            summary.TotalMs = sw.ElapsedMilliseconds;
            summary.TotalPassed = summary.BasicPassed + summary.ScalingPassed + summary.SkillPassed;
            summary.TotalTests = 3 + 3 + 3; // basic + scaling + skill

            PrintSummary(summary);

            // JSON 导出
            string json = ExportJson(summary);
            string jsonPath = GetJsonOutputPath();
            File.WriteAllText(jsonPath, json);
            GD.Print($"\n📄 JSON report saved: {jsonPath}");

            return summary;
        }

        /// <summary>
        /// CI 入口：返回 false 时进程以退出码 1 退出（测试失败）
        /// </summary>
        public static bool RunForCI()
        {
            var summary = RunAllTests(verbose: true);
            bool success = summary.TotalPassed == summary.TotalTests;

            if (!success)
            {
                GD.Print($"\n❌ CI FAILED: {summary.TotalPassed}/{summary.TotalTests} tests passed");
            }
            else
            {
                GD.Print($"\n✅ CI PASSED: All {summary.TotalTests} tests passed");
            }

            // 注意：在 Godot 中进程退出码通过 GD.Exit() 设置
            // 在外部 CI 脚本中通过检查 stdout/进程返回码判断
            return success;
        }

        // ─── Category runners ──────────────────────────────────────────────

        private static int RunBasicCategory(bool verbose, TestSummary summary)
        {
            int passed = 0;

            // Test 1: 玩家应击败哥布林
            {
                var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 };
                var enemies = new List<SimEnemyState>
                {
                    new SimEnemyState { Id = "goblin", Name = "Goblin", MaxHealth = 30, CurrentHealth = 30, Attack = 8, Defense = 2 }
                };
                var result = CombatSimulator.Run(player, enemies);
                bool ok = result.Victory && result.TotalDamageDealt >= 10;
                if (verbose) CombatSimulator.PrintResult("basic_goblin_fight", result, ok);
                if (ok) passed++;
                else summary.Failures.Add("basic_goblin_fight");
            }

            // Test 2: 高防敌人减少玩家伤害
            {
                var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 };
                var enemies = new List<SimEnemyState>
                {
                    new SimEnemyState { Id = "armored", Name = "Armored", MaxHealth = 50, CurrentHealth = 50, Attack = 5, Defense = 10 }
                };
                var result = CombatSimulator.Run(player, enemies);
                bool ok = result.Victory && result.TotalDamageTaken < 30;
                if (verbose) CombatSimulator.PrintResult("basic_armored_enemy", result, ok);
                if (ok) passed++;
                else summary.Failures.Add("basic_armored_enemy");
            }

            // Test 3: 玩家被围攻时应该受伤
            {
                var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 15, Defense = 3 };
                var enemies = new List<SimEnemyState>
                {
                    new SimEnemyState { Id = "goblin1", Name = "Goblin1", MaxHealth = 20, CurrentHealth = 20, Attack = 8, Defense = 1 },
                    new SimEnemyState { Id = "goblin2", Name = "Goblin2", MaxHealth = 20, CurrentHealth = 20, Attack = 8, Defense = 1 }
                };
                var result = CombatSimulator.Run(player, enemies);
                bool ok = result.Victory && result.TotalDamageTaken > 0;
                if (verbose) CombatSimulator.PrintResult("basic_multi_enemy", result, ok);
                if (ok) passed++;
                else summary.Failures.Add("basic_multi_enemy");
            }

            return passed;
        }

        private static int RunScalingCategory(bool verbose, TestSummary summary)
        {
            int passed = 0;

            // Test 1: Elite 敌人需要更多回合
            {
                var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 };
                var normal = new SimEnemyState { Id = "goblin", Name = "Goblin", MaxHealth = 30, CurrentHealth = 30, Attack = 8, Defense = 2 };
                var elite = new SimEnemyState { Id = "goblin_elite", Name = "Goblin Elite", MaxHealth = 60, CurrentHealth = 60, Attack = 16, Defense = 4, EliteMultiplier = 2.0f };

                var r1 = CombatSimulator.Run(new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 },
                    new List<SimEnemyState> { normal.Clone() });
                var r2 = CombatSimulator.Run(new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 },
                    new List<SimEnemyState> { elite });

                bool ok = r1.TotalRounds < r2.TotalRounds;
                if (verbose)
                {
                    GD.Print($"  Scaling comparison: Normal={r1.TotalRounds}rounds vs Elite={r2.TotalRounds}rounds. " +
                              $"Elite takes longer: {(ok ? "✅" : "❌")}");
                }
                if (ok) passed++;
                else summary.Failures.Add("scaling_elite_slowdown");
            }

            // Test 2: Boss 缩放 3x
            {
                var player = new SimPlayerState { MaxHealth = 200, CurrentHealth = 200, Attack = 20, Defense = 10 };
                var boss = new SimEnemyState { Id = "boss", Name = "Orc Warlord", MaxHealth = 300, CurrentHealth = 300, Attack = 30, Defense = 10, EliteMultiplier = 3.0f };
                var result = CombatSimulator.Run(player, new List<SimEnemyState> { boss }, null, 200);
                bool ok = result.Victory;
                if (verbose) CombatSimulator.PrintResult("scaling_boss_3x", result, ok);
                if (ok) passed++;
                else summary.Failures.Add("scaling_boss_3x");
            }

            // Test 3: 玩家等级缩放验证
            {
                var lowLevel = new SimPlayerState { MaxHealth = 80, CurrentHealth = 80, Attack = 8, Defense = 3 };
                var highLevel = new SimPlayerState { MaxHealth = 150, CurrentHealth = 150, Attack = 18, Defense = 8 };
                var enemy = new SimEnemyState { Id = "wolf", Name = "Wolf", MaxHealth = 40, CurrentHealth = 40, Attack = 10, Defense = 3 };

                var r1 = CombatSimulator.Run(lowLevel, new List<SimEnemyState> { enemy.Clone() });
                var r2 = CombatSimulator.Run(highLevel, new List<SimEnemyState> { enemy.Clone() });

                bool ok = r2.TotalDamageDealt > r1.TotalDamageDealt;
                if (verbose)
                {
                    GD.Print($"  Level scaling: LowLv dealt={r1.TotalDamageDealt}, HighLv dealt={r2.TotalDamageDealt}. " +
                              $"Higher level does more: {(ok ? "✅" : "❌")}");
                }
                if (ok) passed++;
                else summary.Failures.Add("scaling_player_level");
            }

            return passed;
        }

        private static int RunSkillCategory(bool verbose, TestSummary summary)
        {
            int passed = 0;

            // Test 1: 技能增加总伤害
            {
                var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 };
                var enemies = new List<SimEnemyState>
                {
                    new SimEnemyState { Id = "goblin", Name = "Goblin", MaxHealth = 50, CurrentHealth = 50, Attack = 5, Defense = 0 }
                };
                var skills = new List<SimSkill>
                {
                    new SimSkill { Id = "strike", Name = "Strike", BaseDamage = 15, DamageType = "physical", Cooldown = 0f }
                };
                var result = CombatSimulator.Run(player, enemies, skills);
                bool ok = result.Victory && result.SkillUsage.ContainsKey("strike") && result.SkillUsage["strike"] > 0;
                if (verbose) CombatSimulator.PrintResult("skill_strike_used", result, ok);
                if (ok) passed++;
                else summary.Failures.Add("skill_strike_used");
            }

            // Test 2: 技能协同测试（Combo 效果）
            {
                var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 12, Defense = 5 };
                var enemies = new List<SimEnemyState>
                {
                    new SimEnemyState { Id = "slime", Name = "Slime", MaxHealth = 40, CurrentHealth = 40, Attack = 6, Defense = 1 }
                };
                var skills = new List<SimSkill>
                {
                    new SimSkill { Id = "fire", Name = "Fire", BaseDamage = 10, DamageType = "magic", Cooldown = 0f },
                    new SimSkill { Id = "fire2", Name = "Fire", BaseDamage = 10, DamageType = "magic", Cooldown = 0f }
                };
                var result = CombatSimulator.Run(player, enemies, skills);
                bool ok = result.Victory;
                if (verbose) CombatSimulator.PrintResult("skill_magic_combo", result, ok);
                if (ok) passed++;
                else summary.Failures.Add("skill_magic_combo");
            }

            // Test 3: 暴击验证
            {
                var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 20, Defense = 5, CritChance = 1.0f, CritMultiplier = 2.0f };
                var enemies = new List<SimEnemyState>
                {
                    new SimEnemyState { Id = "target", Name = "Target", MaxHealth = 1000, CurrentHealth = 1000, Attack = 1, Defense = 0 }
                };
                var result = CombatSimulator.Run(player, enemies, null, 20);
                bool ok = result.CriticalHitsDealt >= 5;
                if (verbose) CombatSimulator.PrintResult("skill_crit_guaranteed", result, ok,
                    !ok ? new List<string> { $"Expected >=5 crits, got {result.CriticalHitsDealt}" } : null);
                if (ok) passed++;
                else summary.Failures.Add("skill_crit_guaranteed");
            }

            return passed;
        }

        private static void RunDpsBenchmarks()
        {
            GD.Print("Running DPS benchmarks (10k iterations)...");
            for (int i = 0; i < 10; i++)
            {
                CombatSimulator.RunDpsBenchmark();
            }
        }

        private static void RunProbabilityTests()
        {
            GD.Print("Running probability validation (10k iterations each)...");
            CombatSimulator.RunProbabilityTest(10000);
        }

        // ─── Report helpers ────────────────────────────────────────────────

        private static void PrintSummary(TestSummary s)
        {
            GD.Print($"\n{'='.ToString()[0]}==========================================");
            GD.Print($"  COMBAT TEST SUITE SUMMARY");
            GD.Print($"  ==========================================");
            GD.Print($"  Basic Tests:       {s.BasicPassed}/3 passed");
            GD.Print($"  Scaling Tests:     {s.ScalingPassed}/3 passed");
            GD.Print($"  Skill Tests:       {s.SkillPassed}/3 passed");
            GD.Print($"  ───────────────────────────────────────");
            GD.Print($"  TOTAL:             {s.TotalPassed}/{s.TotalTests} passed");
            GD.Print($"  Time:              {s.TotalMs}ms");
            GD.Print($"  ===========================================");
            if (s.Failures.Count > 0)
            {
                GD.Print($"  FAILED: {string.Join(", ", s.Failures)}");
            }
        }

        private static string ExportJson(TestSummary s)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"version\": \"{VERSION}\",");
            sb.AppendLine($"  \"timestamp\": \"{DateTime.UtcNow:O}\",");
            sb.AppendLine($"  \"total_tests\": {s.TotalTests},");
            sb.AppendLine($"  \"total_passed\": {s.TotalPassed},");
            sb.AppendLine($"  \"total_failed\": {s.TotalTests - s.TotalPassed},");
            sb.AppendLine($"  \"basic_passed\": {s.BasicPassed},");
            sb.AppendLine($"  \"scaling_passed\": {s.ScalingPassed},");
            sb.AppendLine($"  \"skill_passed\": {s.SkillPassed},");
            sb.AppendLine($"  \"elapsed_ms\": {s.TotalMs},");
            sb.AppendLine($"  \"failures\": [");
            for (int i = 0; i < s.Failures.Count; i++)
            {
                sb.AppendLine($"    \"{s.Failures[i]}\"{(i < s.Failures.Count - 1 ? "," : "")}");
            }
            sb.AppendLine("  ],");
            sb.AppendLine($"  \"exit_code\": {(s.TotalPassed == s.TotalTests ? 0 : 1)}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GetJsonOutputPath()
        {
            return "user://combat_test_report.json";
        }

        private static CombatTestSuite BuildStandardSuite()
        {
            // 预留：未来可从 Resource 文件加载测试套件
            return new CombatTestSuite { SuiteId = "standard", Description = "Standard combat regression tests" };
        }

        /// <summary>
        /// 扩展 SimEnemyState 以支持 ICloneable 模式
        /// </summary>
        private static SimEnemyState Clone(this SimEnemyState e)
        {
            return new SimEnemyState
            {
                Id = e.Id,
                Name = e.Name,
                MaxHealth = e.MaxHealth,
                CurrentHealth = e.CurrentHealth,
                Attack = e.Attack,
                Defense = e.Defense,
                CritChance = e.CritChance,
                CritMultiplier = e.CritMultiplier,
                DodgeChance = e.DodgeChance,
                Speed = e.Speed,
                EliteMultiplier = e.EliteMultiplier
            };
        }
    }

    public class TestSummary
    {
        public int TotalTests { get; set; }
        public int TotalPassed { get; set; }
        public int BasicPassed { get; set; }
        public int ScalingPassed { get; set; }
        public int SkillPassed { get; set; }
        public long TotalMs { get; set; }
        public List<string> Failures { get; set; } = new();
    }
}
