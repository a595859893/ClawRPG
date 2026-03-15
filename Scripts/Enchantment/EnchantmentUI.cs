// ============================================
// Enchantment UI - 附魔系统界面
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Enchantment
{
    public class EnchantmentUI
    {
        private static EnchantmentUI _instance;
        public static EnchantmentUI Instance => _instance ?? (_instance = new EnchantmentUI());
        
        private bool _isVisible;
        private int _selectedTab;
        private int _selectedEnchantmentIndex;
        private List<EnchantmentConfig> _displayedEnchantments;
        
        // 标签页
        private readonly string[] _tabNames = { "附魔库", "我的附魔", "统计" };
        
        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (_isVisible)
            {
                RefreshDisplay();
            }
        }
        
        public bool IsVisible() => _isVisible;
        
        public void Hide()
        {
            _isVisible = false;
        }
        
        private void RefreshDisplay()
        {
            switch (_selectedTab)
            {
                case 0:
                    _displayedEnchantments = EnchantmentDatabase.GetAllEnchantments();
                    break;
                case 1:
                    _displayedEnchantments = EnchantmentSystem.Instance.GetUnlockedEnchantments();
                    break;
                case 2:
                    _displayedEnchantments = new List<EnchantmentConfig>();
                    break;
            }
            _selectedEnchantmentIndex = 0;
        }
        
        public void HandleInput(ConsoleKey key)
        {
            if (!_isVisible) return;
            
            switch (key)
            {
                case ConsoleKey.Tab:
                    _selectedTab = (_selectedTab + 1) % 3;
                    RefreshDisplay();
                    break;
                    
                case ConsoleKey.UpArrow:
                    if (_displayedEnchantments != null && _displayedEnchantments.Count > 0)
                    {
                        _selectedEnchantmentIndex = Math.Max(0, _selectedEnchantmentIndex - 1);
                    }
                    break;
                    
                case ConsoleKey.DownArrow:
                    if (_displayedEnchantments != null && _displayedEnchantments.Count > 0)
                    {
                        _selectedEnchantmentIndex = Math.Min(_displayedEnchantments.Count - 1, _selectedEnchantmentIndex + 1);
                    }
                    break;
                    
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                    _selectedTab = key - ConsoleKey.D1;
                    RefreshDisplay();
                    break;
                    
                case ConsoleKey.Escape:
                    Hide();
                    break;
            }
        }
        
        public void Render()
        {
            if (!_isVisible) return;
            
            Console.Clear();
            RenderHeader();
            RenderTabs();
            
            switch (_selectedTab)
            {
                case 0:
                    RenderEnchantmentLibrary();
                    break;
                case 1:
                    RenderMyEnchantments();
                    break;
                case 2:
                    RenderStatistics();
                    break;
            }
            
            RenderFooter();
        }
        
        private void RenderHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    【附魔系统】 Enchantment System              ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }
        
        private void RenderTabs()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  ");
            for (int i = 0; i < _tabNames.Length; i++)
            {
                if (i == _selectedTab)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"[{i + 1}.{_tabNames[i]}] ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" {i + 1}.{_tabNames[i]}  ");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }
        
        private void RenderEnchantmentLibrary()
        {
            var enchantments = EnchantmentDatabase.GetAllEnchantments();
            
            // 按类型分组显示
            var types = new[] { EnchantmentType.Weapon, EnchantmentType.Armor, EnchantmentType.Accessory, EnchantmentType.Universal };
            
            foreach (var type in types)
            {
                var typeEnchantments = enchantments.Where(e => e.Type == type).ToList();
                if (typeEnchantments.Count == 0) continue;
                
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"  【{EnchantmentDatabase.TypeNames[type]}】");
                Console.ResetColor();
                
                foreach (var enchant in typeEnchantments)
                {
                    RenderEnchantmentItem(enchant, false);
                }
                Console.WriteLine();
            }
        }
        
        private void RenderMyEnchantments()
        {
            var unlocked = EnchantmentSystem.Instance.GetUnlockedEnchantments();
            
            if (unlocked.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("  暂无已解锁的附魔。");
                Console.ResetColor();
                return;
            }
            
            foreach (var enchant in unlocked)
            {
                RenderEnchantmentItem(enchant, true);
            }
        }
        
        private void RenderEnchantmentItem(EnchantmentConfig enchant, bool isUnlocked)
        {
            var rarityColor = GetRarityColor(enchant.Rarity);
            Console.ForegroundColor = rarityColor;
            
            string status = isUnlocked ? "✓ 已解锁" : "✗ 未解锁";
            Console.Write($"  [{status}] ");
            
            Console.Write($"{enchant.Name}");
            Console.ResetColor();
            Console.Write($" - {enchant.Description}");
            
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($" (费用: {enchant.GoldCost}G, 成功率: {enchant.SuccessRate:P0})");
            Console.ResetColor();
            Console.WriteLine();
        }
        
        private void RenderStatistics()
        {
            var data = EnchantmentSystem.Instance.GetPlayerData();
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  【附魔统计】");
            Console.ResetColor();
            Console.WriteLine();
            
            Console.WriteLine($"  总附魔次数: {data.TotalEnchantmentsPerformed}");
            Console.WriteLine($"  成功次数: {data.SuccessfulEnchantments}");
            Console.WriteLine($"  失败次数: {data.FailedEnchantments}");
            
            if (data.TotalEnchantmentsPerformed > 0)
            {
                var successRate = (float)data.SuccessfulEnchantments / data.TotalEnchantmentsPerformed * 100;
                Console.WriteLine($"  总成功率: {successRate:F1}%");
            }
            
            Console.WriteLine();
            
            // 按稀有度统计
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  【稀有度分布】");
            Console.ResetColor();
            
            var unlocked = EnchantmentSystem.Instance.GetUnlockedEnchantments();
            var rarityGroups = unlocked.GroupBy(e => e.Rarity).OrderByDescending(g => g.Key);
            
            foreach (var group in rarityGroups)
            {
                var rarityName = EnchantmentDatabase.RarityNames[group.Key];
                var color = GetRarityColor(group.Key);
                Console.ForegroundColor = color;
                Console.WriteLine($"  {rarityName}: {group.Count()} 个");
            }
            
            Console.ResetColor();
        }
        
        private void RenderFooter()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  操作说明:");
            Console.WriteLine("    [1-3] 切换标签页  [↑/↓] 选择附魔  [ESC] 关闭");
            Console.ResetColor();
        }
        
        private ConsoleColor GetRarityColor(EnchantmentRarity rarity)
        {
            switch (rarity)
            {
                case EnchantmentRarity.Common: return ConsoleColor.Gray;
                case EnchantmentRarity.Uncommon: return ConsoleColor.Green;
                case EnchantmentRarity.Rare: return ConsoleColor.Blue;
                case EnchantmentRarity.Epic: return ConsoleColor.Magenta;
                case EnchantmentRarity.Legendary: return ConsoleColor.Yellow;
                default: return ConsoleColor.White;
            }
        }
    }
}
