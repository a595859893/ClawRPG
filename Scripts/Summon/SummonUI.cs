using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 召唤系统 UI
    /// </summary>
    public class SummonUI
    {
        private SummonSystem _summonSystem;
        private bool _isVisible;
        private int _currentTab;
        private string _selectedSummonId;
        private int _playerLevel;

        public event Action OnToggle;
        public event Action<string, string> OnNotification;

        public SummonUI(SummonSystem summonSystem, int playerLevel = 1)
        {
            _summonSystem = summonSystem;
            _playerLevel = playerLevel;
            _currentTab = 0;
        }

        public void SetPlayerLevel(int level)
        {
            _playerLevel = level;
        }

        /// <summary>
        /// 切换 UI 显示
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
            if (_isVisible)
            {
                Render();
            }
            OnToggle?.Invoke();
        }

        /// <summary>
        /// 显示 UI
        /// </summary>
        public void Show()
        {
            _isVisible = true;
            Render();
        }

        /// <summary>
        /// 隐藏 UI
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
        }

        /// <summary>
        /// 渲染界面
        /// </summary>
        public void Render()
        {
            if (!_isVisible) return;

            Console.Clear();
            PrintHeader();
            PrintTabs();
            
            switch (_currentTab)
            {
                case 0:
                    PrintSummonsList();
                    break;
                case 1:
                    PrintActiveSummons();
                    break;
                case 2:
                    PrintStatistics();
                    break;
                case 3:
                    PrintDetails();
                    break;
            }

            PrintFooter();
        }

        private void PrintHeader()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    ✨ 召唤系统 ✨                           ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        }

        private void PrintTabs()
        {
            var tabs = new[] { "📜 召唤物", "⚔️ 活跃", "📊 统计", "🔍 详情" };
            Console.Write("║ ");
            for (int i = 0; i < tabs.Length; i++)
            {
                if (i == _currentTab)
                    Console.Write($"[{tabs[i]}] ");
                else
                    Console.Write($" {tabs[i]}  ");
            }
            Console.WriteLine(new string(' ', 40));
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        }

        private void PrintSummonsList()
        {
            Console.WriteLine("║ 可用召唤物:                                                  ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            
            var available = _summonSystem.GetAvailableSummons(_playerLevel);
            var unlocked = _summonSystem.GetUnlockedSummons();
            var unlockedIds = unlocked.Select(s => s.Id).ToHashSet();

            int count = 0;
            foreach (var summon in available)
            {
                count++;
                var isUnlocked = unlockedIds.Contains(summon.Id);
                var icon = summon.Icon ?? "❓";
                var rarityColor = SummonDatabase.RarityColors.ContainsKey(summon.Rarity) 
                    ? SummonDatabase.RarityColors[summon.Rarity] 
                    : "#FFFFFF";
                
                Console.Write($"║ {icon} {summon.Name,-15} ");
                Console.Write($"[{(int)summon.Rarity}] ");
                Console.Write($"{summon.Type,-12} ");
                Console.Write($"Lv.{summon.LevelRequirement,2} ");
                
                if (isUnlocked)
                    Console.Write("✅ ");
                else
                    Console.Write("🔒 ");

                if (_selectedSummonId == summon.Id)
                    Console.Write(" ←");
                
                Console.WriteLine();
            }

            if (count == 0)
            {
                Console.WriteLine("║ 暂无可用召唤物                                               ║");
            }

            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ 操作说明:                                                    ║");
            Console.WriteLine("║   [数字] 选择召唤物  [A] 激活  [D] 解散  [←/→] 切换标签   ║");
        }

        private void PrintActiveSummons()
        {
            Console.WriteLine("║ 活跃召唤物:                                                  ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");

            var active = _summonSystem.GetActiveSummons();
            
            if (active.Count == 0)
            {
                Console.WriteLine("║ 当前没有活跃的召唤物                                          ║");
            }
            else
            {
                foreach (var summon in active)
                {
                    var isActive = _summonSystem.HasActiveSummon(summon.Id);
                    var icon = summon.Icon ?? "❓";
                    
                    Console.Write($"║ {icon} {summon.Name,-15} ");
                    Console.Write($"HP: {GetActiveHealth(summon.Id),4} ");
                    Console.Write($"ATK: {summon.BaseStats.Attack,-4} ");
                    Console.Write($"SPD: {summon.BaseStats.Speed,-3} ");
                    Console.WriteLine(isActive ? "⚔️" : "");
                }
            }

            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ 最大同时召唤: {_summonSystem.PlayerData.MaxActiveSummons} / 6                              ║");
            Console.WriteLine("║   [D] 解散选中的召唤物  [←/→] 切换标签                     ║");
        }

        private int GetActiveHealth(string summonId)
        {
            var activeList = _summonSystem.PlayerData.ActiveSummons;
            var active = activeList.FirstOrDefault(a => a.SummonId == summonId && a.State == SummonState.Active);
            return active?.CurrentHealth ?? 0;
        }

        private void PrintStatistics()
        {
            var stats = _summonSystem.GetStatistics();
            
            Console.WriteLine("║ 召唤统计:                                                    ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ 总召唤次数:     {stats.TotalSummons,-10}                              ║");
            Console.WriteLine($"║ 总伤害输出:     {stats.TotalDamageDealt,-10}                              ║");
            Console.WriteLine($"║ 最高单次伤害:   {stats.HighestDamage,-10}                              ║");
            Console.WriteLine($"║ 总击杀数:       {stats.TotalKills,-10}                              ║");
            Console.WriteLine($"║ 总活跃时间:     {stats.TotalActiveTime,-10}                              ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ 按类型统计:                                                    ║");
            
            foreach (var kvp in stats.SummonsByType)
            {
                Console.WriteLine($"║   {kvp.Key,-15}: {kvp.Value,-5}                                   ║");
            }

            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ 按稀有度统计:                                                ║");
            
            foreach (var kvp in stats.SummonsByRarity)
            {
                Console.WriteLine($"║   {kvp.Key,-15}: {kvp.Value,-5}                                   ║");
            }

            if (!string.IsNullOrEmpty(stats.MostUsedSummonId))
            {
                var mostUsed = SummonDatabase.GetSummon(stats.MostUsedSummonId);
                if (mostUsed != null)
                {
                    Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
                    Console.WriteLine($"║ 最常用: {mostUsed.Name,-20}                        ║");
                }
            }
        }

        private void PrintDetails()
        {
            if (string.IsNullOrEmpty(_selectedSummonId))
            {
                Console.WriteLine("║ 请先选择一个召唤物                                           ║");
                Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
                return;
            }

            var summon = SummonDatabase.GetSummon(_selectedSummonId);
            if (summon == null)
            {
                Console.WriteLine("║ 未找到该召唤物                                               ║");
                return;
            }

            var isUnlocked = _summonSystem.GetUnlockedSummons().Any(s => s.Id == summon.Id);
            
            Console.WriteLine($"║ {summon.Icon ?? "❓"} {summon.Name} [{(int)summon.Rarity}]                          ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ 类型: {summon.Type,-50}║");
            Console.WriteLine($"║ 稀有度: {summon.Rarity,-47}║");
            Console.WriteLine($"║ 等级需求: {summon.LevelRequirement,-43}║");
            Console.WriteLine($"║ 魔法消耗: {summon.ManaCost,-45}║");
            Console.WriteLine($"║ 持续时间: {summon.Duration}秒{-46}║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║ 基础属性:                                                    ║");
            Console.WriteLine($"║   生命: {summon.BaseStats.Health,-48}║");
            Console.WriteLine($"║   攻击: {summon.BaseStats.Attack,-48}║");
            Console.WriteLine($"║   防御: {summon.BaseStats.Defense,-48}║");
            Console.WriteLine($"║   魔法: {summon.BaseStats.Magic,-48}║");
            Console.WriteLine($"║   速度: {summon.BaseStats.Speed,-48}║");
            Console.WriteLine($"║   暴击率: {(summon.BaseStats.CriticalRate * 100):F1}%-{43}║");
            Console.WriteLine($"║   暴击伤害: {(summon.BaseStats.CriticalDamage * 100):F0}%-{43}║");
            
            if (summon.BaseStats.LifeSteal > 0)
                Console.WriteLine($"║   生命偷取: {summon.BaseStats.LifeSteal}%-{46}║");
            if (summon.BaseStats.DodgeRate > 0)
                Console.WriteLine($"║   闪避率: {(summon.BaseStats.DodgeRate * 100):F1}%-{44}║");
            if (summon.BaseStats.BlockRate > 0)
                Console.WriteLine($"║   格挡率: {(summon.BaseStats.BlockRate * 100):F1}%-{44}║");
                
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            
            if (summon.Skills != null && summon.Skills.Count > 0)
            {
                Console.WriteLine("║ 技能:                                                       ║");
                foreach (var skill in summon.Skills)
                {
                    Console.WriteLine($"║   [{skill.Name}] {skill.Description,-40}║");
                    Console.WriteLine($"║     冷却: {skill.Cooldown}秒  消耗: {skill.ManaCost}  伤害倍率: {skill.DamageMultiplier:F1}x    ║");
                }
            }
            
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ 状态: {(isUnlocked ? "已解锁 ✅" : "未解锁 🔒"),-50}║");
            
            if (isUnlocked)
            {
                var unlocked = _summonSystem.PlayerData.UnlockedSummons
                    .FirstOrDefault(u => u.SummonId == summon.Id);
                if (unlocked != null)
                {
                    Console.WriteLine($"║ 使用次数: {unlocked.UseCount,-45}║");
                    Console.WriteLine($"║ 总伤害: {unlocked.TotalDamage,-47}║");
                    Console.WriteLine($"║ 总击杀: {unlocked.TotalKills,-48}║");
                }
            }
        }

        private void PrintFooter()
        {
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine("  [ESC] 关闭  [←/→] 切换标签  [数字] 选择  [A] 激活  [D] 解散");
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        public bool HandleInput(string input)
        {
            switch (input.ToLower())
            {
                case "escape":
                case "esc":
                    Hide();
                    return true;
                case "left":
                case "a":
                    _currentTab = (_currentTab - 1 + 4) % 4;
                    Render();
                    return true;
                case "right":
                case "d":
                    _currentTab = (_currentTab + 1) % 4;
                    Render();
                    return true;
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                    SelectByNumber(int.Parse(input));
                    return true;
                case "activate":
                case "act":
                    ActivateSelected();
                    return true;
                case "dismiss":
                case "dis":
                    DismissSelected();
                    return true;
            }
            return false;
        }

        private void SelectByNumber(int number)
        {
            var available = _summonSystem.GetAvailableSummons(_playerLevel);
            if (number > 0 && number <= available.Count)
            {
                _selectedSummonId = available[number - 1].Id;
                Render();
            }
        }

        private void ActivateSelected()
        {
            if (string.IsNullOrEmpty(_selectedSummonId))
            {
                OnNotification?.Invoke("warning", "请先选择一个召唤物");
                return;
            }

            if (_summonSystem.ActivateSummon(_selectedSummonId, _playerLevel))
            {
                OnNotification?.Invoke("success", "召唤成功！");
                Render();
            }
            else
            {
                OnNotification?.Invoke("error", "召唤失败！可能已在冷却中或达到上限");
            }
        }

        private void DismissSelected()
        {
            if (string.IsNullOrEmpty(_selectedSummonId))
            {
                OnNotification?.Invoke("warning", "请先选择一个召唤物");
                return;
            }

            if (_summonSystem.HasActiveSummon(_selectedSummonId))
            {
                _summonSystem.DismissSummon(_selectedSummonId);
                OnNotification?.Invoke("info", "召唤物已解散");
                Render();
            }
            else
            {
                OnNotification?.Invoke("warning", "该召唤物不在活跃状态");
            }
        }
    }
}
