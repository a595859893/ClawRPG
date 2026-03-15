// ============================================
// Relic Collection UI - 遗物收集系统界面
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClawRPG.Systems.Relics
{
    public class RelicCollectionUI
    {
        private RelicCollectionSystem _relicSystem;
        private bool _isVisible = false;
        
        // 当前选中的标签页
        private int _currentTab = 0;
        
        // 当前选中的遗物
        private string _selectedRelicId = null;
        
        // 筛选条件
        private RelicRarity? _filterRarity = null;
        private RelicType? _filterType = null;
        
        // 标签页名称
        private readonly string[] _tabNames = { "Collection", "Equipment", "Sets", "Statistics" };

        // 快捷键: R
        public const string ToggleKey = "R";

        public RelicCollectionUI(RelicCollectionSystem relicSystem)
        {
            _relicSystem = relicSystem;
            _relicSystem.OnRelicUnlocked += OnRelicUnlocked;
            _relicSystem.OnRelicEquipped += OnRelicEquipped;
            _relicSystem.OnRelicUnequipped += OnRelicUnequipped;
            _relicSystem.OnSetCompleted += OnSetCompleted;
        }

        // 切换显示
        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (_isVisible)
            {
                Render();
            }
        }

        // 显示
        public void Show()
        {
            _isVisible = true;
            Render();
        }

        // 隐藏
        public void Hide()
        {
            _isVisible = false;
        }

        // 处理输入
        public bool HandleInput(ConsoleKey key)
        {
            if (!_isVisible) return false;

            switch (key)
            {
                case ConsoleKey.Tab:
                    _currentTab = (_currentTab + 1) % _tabNames.Length;
                    _selectedRelicId = null;
                    Render();
                    return true;
                    
                case ConsoleKey.D1: case ConsoleKey.D2: case ConsoleKey.D3: case ConsoleKey.D4:
                    var tabIndex = key - ConsoleKey.D1;
                    if (tabIndex < _tabNames.Length)
                    {
                        _currentTab = tabIndex;
                        _selectedRelicId = null;
                        Render();
                    }
                    return true;
                    
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    NavigateRelics(-1);
                    return true;
                    
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    NavigateRelics(1);
                    return true;
                    
                case ConsoleKey.Enter:
                case ConsoleKey.E:
                    if (_selectedRelicId != null)
                    {
                        ToggleEquipRelic();
                    }
                    return true;
                    
                case ConsoleKey.U:
                    if (_selectedRelicId != null)
                    {
                        _relicSystem.UpgradeRelic(_selectedRelicId);
                        Render();
                    }
                    return true;
                    
                case ConsoleKey.Escape:
                case ConsoleKey.R:
                    Hide();
                    return true;
                    
                case ConsoleKey.F:
                    // 切换稀有度筛选
                    ToggleRarityFilter();
                    Render();
                    return true;
                    
                case ConsoleKey.T:
                    // 切换类型筛选
                    ToggleTypeFilter();
                    Render();
                    return true;
            }

            return false;
        }

        // 导航遗物列表
        private void NavigateRelics(int direction)
        {
            var collection = _relicSystem.GetPlayerCollection();
            var filteredRelics = GetFilteredRelics();
            
            if (filteredRelics.Count == 0) return;

            if (_selectedRelicId == null)
            {
                _selectedRelicId = filteredRelics[0].Id;
            }
            else
            {
                var currentIndex = filteredRelics.FindIndex(r => r.Id == _selectedRelicId);
                var newIndex = currentIndex + direction;
                
                if (newIndex < 0) newIndex = filteredRelics.Count - 1;
                if (newIndex >= filteredRelics.Count) newIndex = 0;
                
                _selectedRelicId = filteredRelics[newIndex].Id;
            }
            
            Render();
        }

        // 切换装备遗物
        private void ToggleEquipRelic()
        {
            var collection = _relicSystem.GetPlayerCollection();
            
            if (collection.Relics.TryGetValue(_selectedRelicId, out var relicData))
            {
                if (relicData.Equipped)
                {
                    _relicSystem.UnequipRelic(_selectedRelicId);
                }
                else
                {
                    _relicSystem.EquipRelic(_selectedRelicId);
                }
                Render();
            }
        }

        // 切换稀有度筛选
        private void ToggleRarityFilter()
        {
            var rarities = Enum.GetValues(typeof(RelicRarity)).Cast<RelicRarity>().ToList();
            var currentIndex = rarities.IndexOf(_filterRarity ?? RelicRarity.Common);
            var newIndex = (currentIndex + 1) % (rarities.Count + 1);
            
            if (newIndex >= rarities.Count)
                _filterRarity = null;
            else
                _filterRarity = rarities[newIndex];
        }

        // 切换类型筛选
        private void ToggleTypeFilter()
        {
            var types = Enum.GetValues(typeof(RelicType)).Cast<RelicType>().ToList();
            var currentIndex = types.IndexOf(_filterType ?? RelicType.Passive);
            var newIndex = (currentIndex + 1) % (types.Count + 1);
            
            if (newIndex >= types.Count)
                _filterType = null;
            else
                _filterType = types[newIndex];
        }

        // 获取筛选后的遗物列表
        private List<Relic> GetFilteredRelics()
        {
            var collection = _relicSystem.GetPlayerCollection();
            var allRelics = new List<Relic>();
            
            foreach (var relic in RelicCollectionDatabase.Relics.Values)
            {
                if (collection.Relics.TryGetValue(relic.Id, out var data) && data.Unlocked)
                {
                    if (_filterRarity.HasValue && relic.Rarity != _filterRarity.Value)
                        continue;
                    if (_filterType.HasValue && relic.Type != _filterType.Value)
                        continue;
                    allRelics.Add(relic);
                }
            }
            
            return allRelics;
        }

        // 渲染界面
        public void Render()
        {
            if (!_isVisible) return;

            Console.Clear();
            PrintHeader();
            PrintTabs();
            
            switch (_currentTab)
            {
                case 0:
                    RenderCollectionTab();
                    break;
                case 1:
                    RenderEquipmentTab();
                    break;
                case 2:
                    RenderSetsTab();
                    break;
                case 3:
                    RenderStatisticsTab();
                    break;
            }
            
            PrintFooter();
        }

        // 打印标题
        private void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           🎁 RELIC COLLECTION - 遗物收集系统              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        // 打印标签页
        private void PrintTabs()
        {
            Console.WriteLine("┌─ TABS ─────────────────────────────────────────────────────┐");
            for (int i = 0; i < _tabNames.Length; i++)
            {
                if (i == _currentTab)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($" ▶ [{i + 1}] {_tabNames[i]} ");
                }
                else
                {
                    Console.Write($"   [{i + 1}] {_tabNames[i]} ");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
            Console.WriteLine();
        }

        // 渲染收集标签页
        private void RenderCollectionTab()
        {
            var collection = _relicSystem.GetPlayerCollection();
            var filteredRelics = GetFilteredRelics();
            
            Console.WriteLine($"📦 Collection ({filteredRelics.Count} / {RelicCollectionDatabase.Relics.Count} unlocked)");
            Console.WriteLine($"   [F] Filter Rarity: {(_filterRarity?.ToString() ?? "All"))}");
            Console.WriteLine($"   [T] Filter Type: {(_filterType?.ToString() ?? "All"))}");
            Console.WriteLine();
            
            Console.WriteLine("┌─ RELICS ──────────────────────────────────────────────────┐");
            foreach (var relic in filteredRelics)
            {
                var relicData = collection.Relics[relic.Id];
                var isSelected = relic.Id == _selectedRelicId;
                var rarityColor = GetRarityColor(relic.Rarity);
                
                if (isSelected)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("▶ ");
                }
                else
                {
                    Console.Write("  ");
                }
                
                PrintRarityColor(relic.Rarity);
                Console.Write($"[{GetRaritySymbol(relic.Rarity)}] ");
                
                Console.Write($"{relic.Name}");
                
                if (relicData.Equipped)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" [EQUIPPED]");
                }
                
                if (relicData.CurrentLevel > 1)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($" +{relicData.CurrentLevel - 1}");
                }
                
                Console.ResetColor();
                Console.WriteLine();
                
                if (isSelected)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"   📖 {relic.Description}");
                    Console.WriteLine($"   🎯 {relic.PrimaryEffect}: +{relic.PrimaryEffectValue:P1}");
                    if (relic.SecondaryEffect.HasValue)
                    {
                        Console.WriteLine($"   ✨ {relic.SecondaryEffect}: +{relic.SecondaryEffectValue:P1}");
                    }
                    Console.ResetColor();
                }
            }
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        }

        // 渲染装备标签页
        private void RenderEquipmentTab()
        {
            var collection = _relicSystem.GetPlayerCollection();
            var bonuses = _relicSystem.GetEquippedRelicBonuses();
            
            Console.WriteLine("⚔️  Currently Equipped Relics");
            Console.WriteLine($"   Max Slots: 6 | Used: {collection.EquippedRelics.Count}");
            Console.WriteLine();
            
            Console.WriteLine("┌─ EQUIPPED RELICS ─────────────────────────────────────────┐");
            for (int i = 0; i < 6; i++)
            {
                if (i < collection.EquippedRelics.Count)
                {
                    var relicId = collection.EquippedRelics[i];
                    if (RelicCollectionDatabase.Relics.TryGetValue(relicId, out var relic))
                    {
                        PrintRarityColor(relic.Rarity);
                        Console.Write($"[{i + 1}] {relic.Name}");
                        Console.ResetColor();
                        
                        var relicData = collection.Relics[relicId];
                        if (relicData.CurrentLevel > 1)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write($" +{relicData.CurrentLevel - 1}");
                        }
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[{i + 1}] [Empty Slot]");
                }
            }
            Console.ResetColor();
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
            
            Console.WriteLine();
            Console.WriteLine("📊 Active Bonuses:");
            Console.WriteLine("┌─ BONUSES ─────────────────────────────────────────────────┐");
            foreach (var bonus in bonuses)
            {
                Console.WriteLine($"   {bonus.Key}: +{bonus.Value:P1}");
            }
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        }

        // 渲染套装标签页
        private void RenderSetsTab()
        {
            var collection = _relicSystem.GetPlayerCollection();
            
            Console.WriteLine("🎖️  Relic Sets");
            Console.WriteLine();
            
            Console.WriteLine("┌─ SETS ────────────────────────────────────────────────────┐");
            foreach (var set in RelicCollectionDatabase.RelicSets.Values)
            {
                var completion = collection.SetCompletions.TryGetValue(set.Id, out var c) ? c : 0;
                var isComplete = completion >= set.RequiredCount;
                
                if (isComplete)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("✅ ");
                }
                else
                {
                    Console.Write("🔒 ");
                }
                
                Console.Write($"{set.Name}");
                Console.ResetColor();
                Console.Write($" - {completion}/{set.RequiredCount} pieces");
                
                if (isComplete)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($" | {set.SetEffect}: +{set.SetEffectValue:P1}");
                }
                Console.ResetColor();
                Console.WriteLine();
                
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   {set.Description}");
                Console.ResetColor();
            }
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        }

        // 渲染统计标签页
        private void RenderStatisticsTab()
        {
            var stats = _relicSystem.GetStatistics();
            
            Console.WriteLine("📈 Relic Statistics");
            Console.WriteLine();
            
            Console.WriteLine("┌─ OVERVIEW ────────────────────────────────────────────────┐");
            Console.WriteLine($"   Total Unlocked: {stats.TotalRelicsUnlocked} / {RelicCollectionDatabase.Relics.Count}");
            Console.WriteLine($"   Currently Equipped: {stats.TotalRelicsEquipped} / 6");
            Console.WriteLine($"   Sets Completed: {stats.SetsCompleted} / {RelicCollectionDatabase.RelicSets.Count}");
            Console.WriteLine($"   Total Levels: {stats.TotalRelicLevels}");
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
            
            Console.WriteLine();
            Console.WriteLine("┌─ BY RARITY ───────────────────────────────────────────────┐");
            foreach (var rarity in stats.UnlockedByRarity)
            {
                PrintRarityColor(rarity.Key);
                Console.Write($"   {rarity.Key}: {rarity.Value}");
                Console.ResetColor();
                Console.WriteLine();
            }
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
            
            Console.WriteLine();
            Console.WriteLine("┌─ BY TYPE ─────────────────────────────────────────────────┐");
            foreach (var type in stats.UnlockedByType)
            {
                Console.WriteLine($"   {type.Key}: {type.Value}");
            }
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
        }

        // 打印页脚
        private void PrintFooter()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("┌─ CONTROLS ────────────────────────────────────────────────┐");
            Console.WriteLine("   [Tab/1-4] Switch Tabs  [↑/↓] Navigate  [Enter/E] Equip  [U] Upgrade");
            Console.WriteLine("   [F] Filter Rarity  [T] Filter Type  [R/Esc] Close");
            Console.WriteLine("└────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        // 获取稀有度符号
        private string GetRaritySymbol(RelicRarity rarity)
        {
            return rarity switch
            {
                RelicRarity.Common => "★",
                RelicRarity.Uncommon => "★★",
                RelicRarity.Rare => "★★★",
                RelicRarity.Epic => "★★★★",
                RelicRarity.Legendary => "★★★★★",
                RelicRarity.Mythic => "★★★★★★",
                _ => "★"
            };
        }

        // 打印稀有度颜色
        private void PrintRarityColor(RelicRarity rarity)
        {
            Console.ForegroundColor = rarity switch
            {
                RelicRarity.Common => ConsoleColor.White,
                RelicRarity.Uncommon => ConsoleColor.Green,
                RelicRarity.Rare => ConsoleColor.Blue,
                RelicRarity.Epic => ConsoleColor.Magenta,
                RelicRarity.Legendary => ConsoleColor.Yellow,
                RelicRarity.Mythic => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
        }

        // 事件处理
        private void OnRelicUnlocked(string relicId)
        {
            if (_isVisible) Render();
        }

        private void OnRelicEquipped(string relicId)
        {
            if (_isVisible) Render();
        }

        private void OnRelicUnequipped(string relicId)
        {
            if (_isVisible) Render();
        }

        private void OnSetCompleted(string setId)
        {
            if (_isVisible) Render();
        }
    }
}
