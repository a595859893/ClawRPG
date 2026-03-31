using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClawRPG.Scripts.Systems.Testing
{
    /// <summary>
    /// 战斗模拟器 - 输入战斗状态，模拟完整战斗流程，输出统计报告
    /// </summary>
    public static class CombatSimulator
    {
        private static readonly Random _rng = new();

        /// <summary>
        /// 运行单次战斗模拟
        /// </summary>
        public static CombatResult Run(
            SimPlayerState player,
            List<SimEnemyState> enemies,
            List<SimSkill> skills = null,
            int maxRounds = 100)
        {
            var sw = Stopwatch.StartNew();
            var result = new CombatResult { TotalRounds = maxRounds };
            var log = result.CombatLog;

            // 克隆状态（不修改原始数据）
            var p = ClonePlayer(player);
            var enemyList = CloneEnemies(enemies);
            var skillList = skills != null ? CloneSkills(skills) : DefaultSkills();
            var skillCooldowns = new Dictionary<string, float>();

            log.Add($"=== Combat Start: Player({p.CurrentHealth}HP) vs {enemyList.Count} enemy(ies) ===");

            for (int round = 1; round <= maxRounds; round++)
            {
                result.TotalRounds = round;
                log.Add($"\n--- Round {round} ---");

                // 玩家回合
                PlayerTurn(p, enemyList, skillList, skillCooldowns, result, log);

                // 检查是否全部击杀
                if (enemyList.TrueForAll(e => e.CurrentHealth <= 0))
                {
                    result.Victory = true;
                    result.RoundsWon = round;
                    log.Add("All enemies defeated! Victory!");
                    break;
                }

                // 敌人回合
                EnemyTurn(p, enemyList, result, log);

                // 检查玩家是否死亡
                if (p.CurrentHealth <= 0)
                {
                    result.Victory = false;
                    result.RoundsWon = round - 1;
                    log.Add("Player defeated!");
                    break;
                }
            }

            if (result.TotalRounds >= maxRounds && !result.Victory && p.CurrentHealth > 0)
            {
                // 超时视为胜利（玩家存活）
                result.Victory = true;
                result.RoundsWon = maxRounds;
                log.Add("Max rounds reached, player survives. Victory!");
            }

            sw.Stop();
            result.ElapsedMs = sw.ElapsedMilliseconds;
            result.PlayerFinalHealth = Math.Max(0, p.CurrentHealth);
            if (enemyList.Count > 0)
                result.EnemyFinalHealth = Math.Max(0, enemyList[0].CurrentHealth);

            return result;
        }

        /// <summary>
        /// 运行单个测试用例
        /// </summary>
        public static bool RunTestCase(CombatTestCase testCase, bool verbose = false)
        {
            var result = Run(testCase.Player, testCase.Enemies, testCase.AvailableSkills, testCase.MaxRounds);

            bool passed = true;
            var issues = new List<string>();

            if (testCase.ExpectedVictory.HasValue && result.Victory != testCase.ExpectedVictory.Value)
            {
                issues.Add($"Victory mismatch: expected {testCase.ExpectedVictory}, got {result.Victory}");
                passed = false;
            }

            if (testCase.MinDamage.HasValue && result.TotalDamageDealt < testCase.MinDamage.Value)
            {
                issues.Add($"Damage too low: {result.TotalDamageDealt} < {testCase.MinDamage}");
                passed = false;
            }

            if (testCase.MaxDamage.HasValue && result.TotalDamageDealt > testCase.MaxDamage.Value)
            {
                issues.Add($"Damage too high: {result.TotalDamageDealt} > {testCase.MaxDamage}");
                passed = false;
            }

            if (verbose)
            {
                PrintResult(testCase.TestId, result, passed, issues);
            }

            return passed;
        }

        /// <summary>
        /// 运行测试套件
        /// </summary>
        public static (int passed, int failed, List<string> failures) RunSuite(CombatTestSuite suite, bool verbose = true)
        {
            var failures = new List<string>();
            int passed = 0, failed = 0;

            GD.Print($"\n=== Running Suite: {suite.SuiteId} ({suite.Cases.Count} cases) ===");

            foreach (var testCase in suite.Cases)
            {
                bool ok = RunTestCase(testCase, verbose);
                if (ok) passed++;
                else
                {
                    failed++;
                    failures.Add($"{testCase.TestId}: {testCase.Description}");
                }
            }

            GD.Print($"\n=== Suite Results: {passed}/{suite.Cases.Count} passed ===");
            return (passed, failed, failures);
        }

        /// <summary>
        /// 基础数值测试（玩家 vs 单个敌人）
        /// </summary>
        public static CombatResult RunBasicTest()
        {
            var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 };
            var enemies = new List<SimEnemyState>
            {
                new SimEnemyState { Id = "goblin", Name = "Goblin", MaxHealth = 30, CurrentHealth = 30, Attack = 8, Defense = 2 }
            };
            return Run(player, enemies);
        }

        /// <summary>
        /// 精英/Boss 属性缩放测试
        /// </summary>
        public static CombatResult RunEliteTest(float eliteMultiplier = 2.0f)
        {
            var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 };
            var enemies = new List<SimEnemyState>
            {
                new SimEnemyState { Id = "goblin_elite", Name = "Goblin Elite", MaxHealth = 60, CurrentHealth = 60, Attack = 16, Defense = 4, EliteMultiplier = eliteMultiplier }
            };
            return Run(player, enemies);
        }

        /// <summary>
        /// 技能组合测试
        /// </summary>
        public static CombatResult RunSkillTest()
        {
            var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5 };
            var enemies = new List<SimEnemyState>
            {
                new SimEnemyState { Id = "goblin", Name = "Goblin", MaxHealth = 30, CurrentHealth = 30, Attack = 8, Defense = 2 }
            };
            var skills = new List<SimSkill>
            {
                new SimSkill { Id = "strike", Name = "Strike", BaseDamage = 12, DamageType = "physical", Cooldown = 0f },
                new SimSkill { Id = "defend", Name = "Defend", BaseDamage = 0, DamageType = "physical", Cooldown = 0f }
            };
            return Run(player, enemies, skills);
        }

        /// <summary>
        /// 暴击/元素反应概率验证（Monte Carlo）
        /// </summary>
        public static void RunProbabilityTest(int iterations = 10000)
        {
            int critHits = 0;
            for (int i = 0; i < iterations; i++)
            {
                if (_rng.NextDouble() < 0.1) critHits++;
            }
            GD.Print($"Crit test ({iterations} iter): observed={((float)critHits/iterations*100):F2}%, expected=10%");
        }

        /// <summary>
        /// DPS 基准测试
        /// </summary>
        public static void RunDpsBenchmark()
        {
            var player = new SimPlayerState { MaxHealth = 100, CurrentHealth = 100, Attack = 10, Defense = 5, CritChance = 0.25f, CritMultiplier = 2.0f };
            var enemies = new List<SimEnemyState>
            {
                new SimEnemyState { Id = "training_dummy", Name = "Training Dummy", MaxHealth = 1000, CurrentHealth = 1000, Attack = 0, Defense = 0 }
            };
            var sw = Stopwatch.StartNew();
            var result = Run(player, enemies, null, 200);
            sw.Stop();
            float dps = result.TotalDamageDealt / (float)Math.Max(1, result.TotalRounds);
            GD.Print($"DPS Benchmark: {dps:F2} damage/round, {result.TotalRounds} rounds, {sw.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// 打印结果到控制台
        /// </summary>
        public static void PrintResult(string testId, CombatResult result, bool passed, List<string> issues = null)
        {
            string icon = passed ? "✅" : "❌";
            GD.Print($"\n{icon} [{testId}] Victory={result.Victory} | DamageDealt={result.TotalDamageDealt} | " +
                      $"DamageTaken={result.TotalDamageTaken} | Rounds={result.TotalRounds} | " +
                      $"PlayerHP={result.PlayerFinalHealth} | {result.ElapsedMs}ms");

            if (issues != null && issues.Count > 0)
            {
                foreach (var issue in issues)
                    GD.Print($"   ⚠️  {issue}");
            }

            if (result.SkillUsage.Count > 0)
            {
                GD.Print($"   Skills used: {string.Join(", ", result.SkillUsage)}");
            }
        }

        // ─── Private helpers ───────────────────────────────────────────────

        private static void PlayerTurn(
            SimPlayerState player,
            List<SimEnemyState> enemies,
            List<SimSkill> skills,
            Dictionary<string, float> cooldowns,
            CombatResult result,
            List<string> log)
        {
            // 更新冷却
            foreach (var key in cooldowns.Keys)
                cooldowns[key] = Math.Max(0, cooldowns[key] - 1f);

            // 选择存活的敌人（优先最高威胁，或第一个）
            var alive = enemies.FindAll(e => e.CurrentHealth > 0);
            if (alive.Count == 0) return;
            var target = alive[0];

            // 优先使用有伤害的技能
            SimSkill chosenSkill = null;
            foreach (var skill in skills)
            {
                if (skill.BaseDamage > 0 && (!cooldowns.TryGetValue(skill.Id, out var cd) || cd <= 0))
                {
                    chosenSkill = skill;
                    break;
                }
            }

            if (chosenSkill != null)
            {
                // 计算伤害
                float rawDamage = chosenSkill.BaseDamage + player.Attack * 0.5f;
                bool isCrit = _rng.NextDouble() < player.CritChance;
                if (isCrit) rawDamage *= player.CritMultiplier;
                float finalDamage = Math.Max(1, rawDamage - target.Defense);

                target.CurrentHealth -= (int)finalDamage;
                result.TotalDamageDealt += (int)finalDamage;
                if (isCrit) result.CriticalHitsDealt++;
                result.SkillUsage[chosenSkill.Id] = result.SkillUsage.GetValueOrDefault(chosenSkill.Id, 0) + 1;
                chosenSkill.TimesUsed++;
                cooldowns[chosenSkill.Id] = chosenSkill.Cooldown + 1;

                string critStr = isCrit ? " CRIT!" : "";
                log.Add($"  Player uses {chosenSkill.Name} on {target.Name}: {(int)finalDamage}{critStr} damage. " +
                        $"{Math.Max(0, target.CurrentHealth)}HP remaining.");
            }
            else
            {
                // 普通攻击
                float rawDamage = player.Attack;
                bool isCrit = _rng.NextDouble() < player.CritChance;
                if (isCrit) rawDamage *= player.CritMultiplier;
                float finalDamage = Math.Max(1, rawDamage - target.Defense);

                target.CurrentHealth -= (int)finalDamage;
                result.TotalDamageDealt += (int)finalDamage;
                if (isCrit) result.CriticalHitsDealt++;

                string critStr = isCrit ? " CRIT!" : "";
                log.Add($"  Player attacks {target.Name}: {(int)finalDamage}{critStr} damage. " +
                        $"{Math.Max(0, target.CurrentHealth)}HP remaining.");
            }
        }

        private static void EnemyTurn(
            SimPlayerState player,
            List<SimEnemyState> enemies,
            CombatResult result,
            List<string> log)
        {
            foreach (var enemy in enemies)
            {
                if (enemy.CurrentHealth <= 0) continue;

                // 敌人攻击
                bool playerDodges = _rng.NextDouble() < player.DodgeChance;
                if (playerDodges)
                {
                    result.DodgesPerformed++;
                    log.Add($"  {enemy.Name} attacks but {player.CurrentHealth}HP player dodges!");
                }
                else
                {
                    float rawDamage = enemy.Attack;
                    bool isCrit = _rng.NextDouble() < enemy.CritChance;
                    if (isCrit) rawDamage *= enemy.CritMultiplier;
                    float finalDamage = Math.Max(1, rawDamage - player.Defense);

                    player.CurrentHealth -= (int)finalDamage;
                    result.TotalDamageTaken += (int)finalDamage;
                    if (isCrit) result.CriticalHitsTaken++;

                    string critStr = isCrit ? " CRIT!" : "";
                    log.Add($"  {enemy.Name} attacks player: {(int)finalDamage}{critStr} damage. " +
                            $"{Math.Max(0, player.CurrentHealth)}HP remaining.");
                }
            }
        }

        private static List<SimSkill> DefaultSkills()
        {
            return new List<SimSkill>
            {
                new SimSkill { Id = "strike", Name = "Strike", BaseDamage = 12, DamageType = "physical", Cooldown = 0f }
            };
        }

        private static SimPlayerState ClonePlayer(SimPlayerState p)
        {
            return new SimPlayerState
            {
                MaxHealth = p.MaxHealth,
                CurrentHealth = p.CurrentHealth,
                Attack = p.Attack,
                Defense = p.Defense,
                CritChance = p.CritChance,
                CritMultiplier = p.CritMultiplier,
                DodgeChance = p.DodgeChance,
                ActiveSkills = new List<string>(p.ActiveSkills)
            };
        }

        private static List<SimEnemyState> CloneEnemies(List<SimEnemyState> enemies)
        {
            var list = new List<SimEnemyState>();
            foreach (var e in enemies)
            {
                list.Add(new SimEnemyState
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
                });
            }
            return list;
        }

        private static List<SimSkill> CloneSkills(List<SimSkill> skills)
        {
            var list = new List<SimSkill>();
            foreach (var s in skills)
            {
                list.Add(new SimSkill
                {
                    Id = s.Id,
                    Name = s.Name,
                    BaseDamage = s.BaseDamage,
                    DamageType = s.DamageType,
                    ManaCost = s.ManaCost,
                    Cooldown = s.Cooldown
                });
            }
            return list;
        }
    }
}
