using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.UI
{
    /// <summary>
    /// Rune System UI - Console-based interface
    /// </summary>
    public class RuneUI
    {
        private RuneSystem _runeSystem;
        private bool _isVisible;
        private int _selectedTab;
        private string _selectedRuneId;
        private RuneSlotType _selectedSlot = RuneSlotType.Weapon;
        
        public RuneUI(RuneSystem runeSystem)
        {
            _runeSystem = runeSystem;
        }
        
        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (_isVisible) Render();
        }
        
        public bool IsVisible() => _isVisible;
        
        public void Render()
        {
            if (!_isVisible) return;
            
            Console.Clear();
            PrintHeader();
            PrintTabs();
            
            switch (_selectedTab)
            {
                case 0: RenderCollection(); break;
                case 1: RenderEquipment(); break;
                case 2: RenderCrafting(); break;
                case 3: RenderStatistics(); break;
            }
            
            PrintFooter();
        }
        
        private void PrintHeader()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ⚡ RUNE SYSTEM ⚡                          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }
        
        private void PrintTabs()
        {
            string[] tabs = { "[C]ollection", "[E]quipment", "[C]rafting", "[S]tatistics" };
            string[] tabNames = { "Collection", "Equipment", "Crafting", "Statistics" };
            
            for (int i = 0; i < tabs.Length; i++)
            {
                if (i == _selectedTab)
                    Console.Write($"▶ {tabNames[i]} ◀");
                else
                    Console.Write($"  {tabs[i]}  ");
            }
            Console.WriteLine();
            Console.WriteLine(new string('─', 70));
        }
        
        private void RenderCollection()
        {
            Console.WriteLine("📜 YOUR RUNE COLLECTION");
            Console.WriteLine();
            
            var runes = _runeSystem.GetOwnedRunes();
            if (!runes.Any())
            {
                Console.WriteLine("  No runes yet. Visit a merchant or craft some!");
                return;
            }
            
            // Group by type
            var grouped = runes.GroupBy(r => r.Type).OrderBy(g => g.Key);
            
            foreach (var group in grouped)
            {
                Console.WriteLine($"\n═══ {group.Key} ═══");
                foreach (var rune in group.OrderBy(r => r.Rarity))
                {
                    int count = _runeSystem.GetRuneCount(rune.Id);
                    string rarityColor = GetRarityColor(r.Rarity);
                    string equipped = _runeSystem.GetEquippedRunes().Any(e => e.Id == rune.Id) ? " [EQUIPPED]" : "";
                    Console.WriteLine($"  {rarityColor}{rune.Name}{GetRarityReset()} x{count}{equipped}");
                    Console.WriteLine($"      {rune.Description}");
                    Console.WriteLine($"      └─ {rune.Power} Power | Lv.{rune.LevelRequired}");
                }
            }
        }
        
        private void RenderEquipment()
        {
            Console.WriteLine("⚔️ EQUIPPED RUNES");
            Console.WriteLine();
            
            var equipped = _runeSystem.GetEquippedRunes();
            if (!equipped.Any())
            {
                Console.WriteLine("  No runes equipped. Go to Collection to equip!");
                return;
            }
            
            foreach (var rune in equipped)
            {
                string rarityColor = GetRarityColor(rune.Rarity);
                Console.WriteLine($"  {rarityColor}{rune.Name}{GetRarityReset()}");
                Console.WriteLine($"      {rune.Description}");
                
                // Print stats
                var stats = new List<string>();
                if (rune.DamageBonus > 0) stats.Add($"+{rune.DamageBonus}% Damage");
                if (rune.DefenseBonus > 0) stats.Add($"+{rune.DefenseBonus} Defense");
                if (rune.HealthBonus > 0) stats.Add($"+{rune.HealthBonus} Health");
                if (rune.ManaBonus > 0) stats.Add($"+{rune.ManaBonus} Mana");
                if (rune.SpeedBonus != 0) stats.Add($"{(rune.SpeedBonus > 0 ? "+" : "")}{rune.SpeedBonus}% Speed");
                if (rune.CritChance > 0) stats.Add($"+{rune.CritChance}% Crit");
                if (rune.CritDamage > 0) stats.Add($"+{rune.CritDamage}% Crit Damage");
                if (rune.LifeSteal > 0) stats.Add($"+{rune.LifeSteal}% Life Steal");
                if (rune.Regen > 0) stats.Add($"+{rune.Regen}/s Regen");
                
                if (stats.Any())
                {
                    Console.WriteLine($"      Stats: {string.Join(", ", stats)}");
                }
                
                // Print effects
                var effects = new List<string>();
                if (rune.OnHitEffect) effects.Add("On Hit");
                if (rune.OnKillEffect) effects.Add("On Kill");
                if (rune.OnDamagedEffect) effects.Add("On Damaged");
                if (rune.OnCriticalEffect) effects.Add("On Critical");
                
                if (effects.Any())
                {
                    Console.WriteLine($"      Effects: {string.Join(", ", effects)}");
                }
                
                Console.WriteLine();
            }
            
            // Print total bonuses
            Console.WriteLine("═══ TOTAL BONUSES ═══");
            Console.WriteLine($"  +{_runeSystem.GetTotalDamageBonus()}% Damage");
            Console.WriteLine($"  +{_runeSystem.GetTotalDefenseBonus()} Defense");
            Console.WriteLine($"  +{_runeSystem.GetTotalHealthBonus()} Health");
            Console.WriteLine($"  +{_runeSystem.GetTotalManaBonus()} Mana");
            Console.WriteLine($"  +{_runeSystem.GetTotalSpeedBonus()}% Speed");
            Console.WriteLine($"  +{_runeSystem.GetTotalCritChance()}% Crit Chance");
            Console.WriteLine($"  +{_runeSystem.GetTotalCritDamage()}% Crit Damage");
            Console.WriteLine($"  +{_runeSystem.GetTotalLifeSteal()}% Life Steal");
            Console.WriteLine($"  +{_runeSystem.GetTotalRegen()}/s Regen");
        }
        
        private void RenderCrafting()
        {
            Console.WriteLine("🔨 RUNE SYNTHESIS");
            Console.WriteLine();
            Console.WriteLine("Combine 3 runes of the same type and lower rarity to craft:");
            Console.WriteLine();
            
            var runeTypes = Enum.GetValues(typeof(RuneType)) as RuneType[];
            
            for (int i = 0; i < runeTypes.Length; i++)
            {
                var type = runeTypes[i];
                var runes = Systems.RuneDatabase.GetRunesByType(type);
                if (!runes.Any()) continue;
                
                Console.WriteLine($"\n═══ {type} ═══");
                for (int j = 1; j < runes.Count; j++)
                {
                    var rune = runes[j];
                    var prevRune = runes[j - 1];
                    
                    string canCraft = CanCraft(rune) ? "✓" : "✗";
                    string rarityColor = GetRarityColor(rune.Rarity);
                    
                    Console.WriteLine($"  {canCraft} {rarityColor}{rune.Name}{GetRarityReset()}");
                    Console.WriteLine($"      Requires: 3x {prevRune.Name}");
                    Console.WriteLine($"      Power: {rune.Power} (+{rune.Power - prevRune.Power})");
                }
            }
        }
        
        private void RenderStatistics()
        {
            Console.WriteLine("📊 RUNE STATISTICS");
            Console.WriteLine();
            
            var stats = _runeSystem.GetStatistics();
            
            Console.WriteLine($"  Total Runes Owned: {stats.TotalRunesOwned}");
            Console.WriteLine($"  Unique Rune Types: {stats.UniqueRunes}");
            Console.WriteLine($"  Total Crafted: {stats.TotalCrafted}");
            Console.WriteLine();
            
            Console.WriteLine("═══ BY RARITY ═══");
            foreach (var kvp in stats.RarityBreakdown.OrderBy(k => k.Key))
            {
                string rarityColor = GetRarityColor(kvp.Key);
                Console.WriteLine($"  {rarityColor}{kvp.Key}{GetRarityReset()}: {kvp.Value}");
            }
            
            Console.WriteLine();
            Console.WriteLine("═══ BY TYPE ═══");
            foreach (var kvp in stats.TypeBreakdown.OrderBy(k => k.Key))
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }
        }
        
        private bool CanCraft(Rune rune)
        {
            int rarityLevel = (int)rune.Rarity;
            if (rarityLevel <= 0) return false;
            
            var ingredientRarity = (RuneRarity)(rarityLevel - 1);
            return _runeSystem.GetOwnedRunes()
                .Any(r => r.Type == rune.Type && r.Rarity == ingredientRarity && _runeSystem.GetRuneCount(r.Id) >= 1);
        }
        
        private void PrintFooter()
        {
            Console.WriteLine();
            Console.WriteLine(new string('─', 70));
            Console.WriteLine("[1-4] Switch Tab  [E] Equip  [U] Unequip  [C] Craft  [Q] Quit  [R] Refresh");
        }
        
        private string GetRarityColor(RuneRarity rarity)
        {
            return rarity switch
            {
                RuneRarity.Common => "§7",
                RuneRarity.Uncommon => "§a",
                RuneRarity.Rare => "§9",
                RuneRarity.Epic => "§5",
                RuneRarity.Legendary => "§6",
                RuneRarity.Mythic => "§d",
                _ => "§f"
            };
        }
        
        private string GetRarityReset()
        {
            return "§r";
        }
        
        public void HandleInput(string input)
        {
            switch (input.ToLower())
            {
                case "1": _selectedTab = 0; break;
                case "2": _selectedTab = 1; break;
                case "3": _selectedTab = 2; break;
                case "4": _selectedTab = 3; break;
                case "c":
                    if (_selectedTab == 2) TryCraft();
                    break;
                case "r":
                case "refresh":
                    Render();
                    break;
                case "q":
                case "quit":
                    _isVisible = false;
                    break;
            }
            Render();
        }
        
        private void TryCraft()
        {
            // Simplified crafting - just show it works
            Console.WriteLine("\nEnter rune ID to craft (or 'back'): ");
            // In real implementation, would handle input
        }
    }
}
