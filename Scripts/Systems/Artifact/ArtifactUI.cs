// ============================================
// Artifact UI - 神器系统界面
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClawRPG.Core.UI
{
    public class ArtifactUI
    {
        private ArtifactSystem _artifactSystem;
        private bool _isVisible;
        
        public ArtifactUI(ArtifactSystem artifactSystem)
        {
            _artifactSystem = artifactSystem;
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (_isVisible)
                ShowUI();
            else
                HideUI();
        }

        public void ShowUI()
        {
            _isVisible = true;
            Render();
        }

        public void HideUI()
        {
            _isVisible = false;
            Console.WriteLine("[Artifact] UI Hidden");
        }

        private void Render()
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ⚔ ARTIFACT SYSTEM ⚔                      ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            
            var equipped = _artifactSystem.GetEquippedArtifacts();
            var stats = _artifactSystem.CalculateTotalStats();
            var statistics = _artifactSystem.GetStatistics();

            // 已装备神器
            Console.WriteLine("║ 📦 EQUIPPED ARTIFACTS                                        ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            
            if (equipped.Any())
            {
                foreach (var artifact in equipped)
                {
                    var rarityColor = GetRarityColor(artifact.Rarity);
                    Console.WriteLine($"║  [{rarityColor}{artifact.Name}{GetReset()}] +{artifact.EnhancementLevel}                      ║");
                    foreach (var effect in artifact.Effects)
                    {
                        Console.WriteLine($"║    └ {effect.Description}                                   ║");
                    }
                }
            }
            else
            {
                Console.WriteLine("║  No artifacts equipped                                       ║");
            }

            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            
            // 当前属性加成
            Console.WriteLine("║ 📊 ACTIVE STATS                                              ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            
            if (stats.Any())
            {
                foreach (var stat in stats.OrderByDescending(s => s.Value).Take(10))
                {
                    Console.WriteLine($"║  {GetStatIcon(stat.Key)} {stat.Key}: +{stat.Value:F1}%                        ║");
                }
            }
            else
            {
                Console.WriteLine("║  No active stats                                             ║");
            }

            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            
            // 套装效果
            var setBonuses = _artifactSystem.GetActiveSetBonuses();
            if (setBonuses.Any())
            {
                Console.WriteLine("║ 🔥 SET BONUSES                                               ║");
                Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
                
                foreach (var bonus in setBonuses)
                {
                    Console.WriteLine($"║  {bonus.Description}                     ║");
                }
            }

            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            
            // 统计信息
            Console.WriteLine("║ 📈 COLLECTION STATISTICS                                     ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║  Total Collected: {statistics.TotalArtifacts,-20}                 ║");
            Console.WriteLine($"║  Rare: {statistics.RareArtifacts,-25}                               ║");
            Console.WriteLine($"║  Epic: {statistics.EpicArtifacts,-25}                               ║");
            Console.WriteLine($"║  Legendary: {statistics.LegendaryArtifacts,-22}                        ║");
            Console.WriteLine($"║  Mythic: {statistics.MythicArtifacts,-24}                             ║");
            Console.WriteLine($"║  Sets Completed: {statistics.SetsCompleted,-20}                      ║");
            
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ [1] View All  [2] Equip  [3] Forge  [4] Acquire  [Q] Quit ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        }

        public void HandleInput(string input)
        {
            switch (input)
            {
                case "1":
                    ShowAllArtifacts();
                    break;
                case "2":
                    ShowEquipMenu();
                    break;
                case "3":
                    ShowForgeMenu();
                    break;
                case "4":
                    AcquireNewArtifact();
                    break;
                case "q":
                case "Q":
                    HideUI();
                    break;
            }
        }

        private void ShowAllArtifacts()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("         📦 ALL ARTIFACTS                  ");
            Console.WriteLine("═══════════════════════════════════════════");
            
            var artifacts = _artifactSystem.GetOwnedArtifacts();
            
            if (!artifacts.Any())
            {
                Console.WriteLine("No artifacts collected yet!");
                return;
            }

            var grouped = artifacts.GroupBy(a => a.Rarity);
            
            foreach (var group in grouped.OrderByDescending(g => g.Key))
            {
                Console.WriteLine($"\n{GetRarityColor(group.Key)}{group.Key.ToString().ToUpper()}{GetReset()}:");
                
                foreach (var artifact in group)
                {
                    var equipped = artifact.IsEquipped ? "[EQUIPPED]" : "";
                    Console.WriteLine($"  [{artifact.Id}] {artifact.Name} +{artifact.EnhancementLevel} {equipped}");
                    Console.WriteLine($"    {artifact.Description}");
                    Console.WriteLine($"    Slot: {artifact.Slot}");
                }
            }
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            ShowUI();
        }

        private void ShowEquipMenu()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("         ⚔ EQUIP ARTIFACT                ");
            Console.WriteLine("═══════════════════════════════════════════");
            
            var artifacts = _artifactSystem.GetOwnedArtifacts()
                .Where(a => !a.IsEquipped)
                .ToList();
            
            if (!artifacts.Any())
            {
                Console.WriteLine("No artifacts available to equip!");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey(true);
                ShowUI();
                return;
            }

            for (int i = 0; i < artifacts.Count; i++)
            {
                var a = artifacts[i];
                Console.WriteLine($"[{i + 1}] {a.Name} (+{a.EnhancementLevel}) - {a.Slot}");
            }

            Console.Write("\nSelect artifact to equip: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= artifacts.Count)
            {
                var artifact = artifacts[choice - 1];
                if (_artifactSystem.EquipArtifact(artifact.Id))
                {
                    Console.WriteLine($"\n✓ Equipped {artifact.Name}!");
                }
            }
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            ShowUI();
        }

        private void ShowForgeMenu()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("         🔨 FORGE ARTIFACT                ");
            Console.WriteLine("═══════════════════════════════════════════");
            
            var equipped = _artifactSystem.GetEquippedArtifacts();
            
            if (!equipped.Any())
            {
                Console.WriteLine("No artifacts to forge!");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey(true);
                ShowUI();
                return;
            }

            for (int i = 0; i < equipped.Count; i++)
            {
                var a = equipped[i];
                var successRate = Core.Databases.ArtifactDatabase.GetForgeSuccessRate(a.EnhancementLevel + 1);
                var cost = Core.Databases.ArtifactDatabase.GetForgeGoldCost(a.EnhancementLevel + 1);
                
                Console.WriteLine($"[{i + 1}] {a.Name} (+{a.EnhancementLevel})");
                Console.WriteLine($"    Success Rate: {successRate:P0} | Cost: {cost:N0} gold");
            }

            Console.Write("\nSelect artifact to forge: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= equipped.Count)
            {
                var artifact = equipped[choice - 1];
                var result = _artifactSystem.ForgeArtifact(artifact.Id, artifact.EnhancementLevel + 1);
                
                Console.WriteLine($"\n{result.Message}");
            }
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            ShowUI();
        }

        private void AcquireNewArtifact()
        {
            Console.WriteLine("\nAcquiring new artifact...");
            var artifact = _artifactSystem.AcquireArtifact(50); // Default level
            
            Console.WriteLine($"\n★ You acquired: {artifact.Name}!");
            Console.WriteLine($"  Rarity: {artifact.Rarity}");
            Console.WriteLine($"  Type: {artifact.Type}");
            
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            ShowUI();
        }

        #region Helpers

        private string GetRarityColor(ArtifactRarity rarity)
        {
            return rarity switch
            {
                ArtifactRarity.Common => "§7",
                ArtifactRarity.Uncommon => "§a",
                ArtifactRarity.Rare => "§9",
                ArtifactRarity.Epic => "§5",
                ArtifactRarity.Legendary => "§6",
                ArtifactRarity.Mythic => "§d",
                _ => "§f"
            };
        }

        private string GetReset()
        {
            return "§r";
        }

        private string GetStatIcon(ArtifactEffectType type)
        {
            return type switch
            {
                ArtifactEffectType.DamageIncrease => "⚔",
                ArtifactEffectType.CriticalRate => "🎯",
                ArtifactEffectType.CriticalDamage => "💥",
                ArtifactEffectType.DefenseIncrease => "🛡",
                ArtifactEffectType.HealthMax => "❤️",
                ArtifactEffectType.ManaMax => "💙",
                ArtifactEffectType.HealthRegen => "💚",
                ArtifactEffectType.ManaRegen => "💜",
                ArtifactEffectType.MoveSpeed => "👟",
                ArtifactEffectType.AttackSpeed => "⏱",
                ArtifactEffectType.CooldownReduction => "⚡",
                _ => "✨"
            };
        }

        #endregion

        public bool IsVisible => _isVisible;
    }
}
