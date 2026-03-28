using System;
using System.Collections.Generic;

namespace Framework
{
    /// <summary>
    /// 单个 Combo 的遗忘状态数据
    /// </summary>
    public class ComboForgetEntry
    {
        /// <summary>
        /// Combo ID
        /// </summary>
        public string comboId;
        
        /// <summary>
        /// 距离上次使用经历的游戏数（每局游戏结束时 +1）
        /// </summary>
        public int gamesSinceLastUse = 0;
        
        /// <summary>
        /// 累计使用次数
        /// </summary>
        public int totalUseCount = 0;
        
        /// <summary>
        /// 是否被玩家锁定（锁定后永不休眠）
        /// </summary>
        public bool isLocked = false;
        
        /// <summary>
        /// 是否处于休眠状态（被遗忘）
        /// </summary>
        public bool isDormant = false;
        
        /// <summary>
        /// 是否曾经被发现过（用于区分"从未发现"和"已遗忘"）
        /// </summary>
        public bool wasEverDiscovered = false;
        
        /// <summary>
        /// 重置每局游戏的运行时状态（gamesSinceLastUse 会递增）
        /// </summary>
        public void OnGameEnded()
        {
            if (!isLocked)
            {
                gamesSinceLastUse++;
            }
            // locked combos 不增加计数器
        }
        
        /// <summary>
        /// 使用该 combo 后调用，重置计数器并唤醒
        /// </summary>
        public void OnComboUsed()
        {
            gamesSinceLastUse = 0;
            totalUseCount++;
            if (isDormant)
            {
                isDormant = false;
            }
        }
    }
    
    /// <summary>
    /// Combo 遗忘数据管理器 — 单例
    /// 追踪所有已发现 combo 的遗忘状态
    /// </summary>
    public class ComboForgetData
    {
        public static ComboForgetData Instance { get; private set; }
        
        /// <summary>
        /// 每个 combo 的遗忘状态
        /// </summary>
        private Dictionary<string, ComboForgetEntry> _comboStates = new Dictionary<string, ComboForgetEntry>();
        
        // ========== 平衡参数 ==========
        /// <summary>
        /// 连续不使用 N 局后进入休眠
        /// </summary>
        public const int DORMANT_AFTER_GAMES = 3;
        
        /// <summary>
        /// 最多可锁定的 combo 数量
        /// </summary>
        public const int MAX_LOCKED_COMBOS = 3;
        
        // ========== 信号 ==========
        public static Action<string, bool> ComboForgetStateChanged; // comboId, isNowDormant
        public static Action<string> ComboRediscovered; // comboId (first rediscover after dormant)
        public static Action<string> ComboLocked; // comboId
        public static Action<string> ComboUnlocked; // comboId
        
        public ComboForgetData()
        {
            Instance = this;
        }
        
        /// <summary>
        /// 初始化/注册一个 combo 的遗忘状态（发现时调用）
        /// </summary>
        public void RegisterCombo(string comboId)
        {
            if (!_comboStates.ContainsKey(comboId))
            {
                var entry = new ComboForgetEntry { comboId = comboId, wasEverDiscovered = true };
                _comboStates[comboId] = entry;
            }
            else
            {
                // 已存在但未被发现过？现在标记为已发现
                _comboStates[comboId].wasEverDiscovered = true;
            }
        }
        
        /// <summary>
        /// 每局游戏结束时调用 — 对所有 combo 递增计数器并检查休眠
        /// </summary>
        public void OnRunEnded()
        {
            bool anyChanged = false;
            foreach (var kvp in _comboStates)
            {
                var entry = kvp.Value;
                if (entry.isLocked || entry.isDormant) continue;
                
                entry.OnGameEnded();
                
                if (entry.gamesSinceLastUse >= DORMANT_AFTER_GAMES && entry.wasEverDiscovered)
                {
                    entry.isDormant = true;
                    anyChanged = true;
                    GD.Print($"[ComboForget] Combo '{kvp.Key}' went dormant after {entry.gamesSinceLastUse} games without use.");
                    ComboForgetStateChanged?.Invoke(kvp.Key, true);
                }
            }
        }
        
        /// <summary>
        /// 当玩家执行某个 combo 时调用 — 唤醒并记录使用
        /// </summary>
        public void RecordComboUsage(string comboId)
        {
            if (!_comboStates.TryGetValue(comboId, out var entry)) return;
            
            bool wasDormant = entry.isDormant;
            entry.OnComboUsed();
            
            if (wasDormant)
            {
                GD.Print($"[ComboForget] Combo '{comboId}' rediscovered! (used {entry.totalUseCount} times total)");
                ComboRediscovered?.Invoke(comboId);
            }
            
            ComboForgetStateChanged?.Invoke(comboId, false);
        }
        
        /// <summary>
        /// 检查某个 combo 是否处于休眠状态
        /// </summary>
        public bool IsDormant(string comboId)
        {
            if (!_comboStates.TryGetValue(comboId, out var entry)) return false;
            return entry.isDormant;
        }
        
        /// <summary>
        /// 检查某个 combo 是否被锁定
        /// </summary>
        public bool IsLocked(string comboId)
        {
            if (!_comboStates.TryGetValue(comboId, out var entry)) return false;
            return entry.isLocked;
        }
        
        /// <summary>
        /// 尝试锁定一个 combo（最多 MAX_LOCKED_COMBOS 个）
        /// </summary>
        public bool TryLockCombo(string comboId)
        {
            int lockedCount = GetLockedCount();
            if (lockedCount >= MAX_LOCKED_COMBOS) return false;
            
            if (!_comboStates.TryGetValue(comboId, out var entry))
            {
                entry = new ComboForgetEntry { comboId = comboId };
                _comboStates[comboId] = entry;
            }
            
            if (entry.isLocked) return false; // already locked
            
            entry.isLocked = true;
            // 锁定时重置计数器
            entry.gamesSinceLastUse = 0;
            if (entry.isDormant)
            {
                entry.isDormant = false;
                ComboForgetStateChanged?.Invoke(comboId, false);
            }
            
            GD.Print($"[ComboForget] Combo '{comboId}' locked. ({lockedCount + 1}/{MAX_LOCKED_COMBOS})");
            ComboLocked?.Invoke(comboId);
            return true;
        }
        
        /// <summary>
        /// 解锁一个 combo
        /// </summary>
        public void UnlockCombo(string comboId)
        {
            if (!_comboStates.TryGetValue(comboId, out var entry)) return;
            entry.isLocked = false;
            GD.Print($"[ComboForget] Combo '{comboId}' unlocked.");
            ComboUnlocked?.Invoke(comboId);
        }
        
        /// <summary>
        /// 获取当前锁定的 combo 数量
        /// </summary>
        public int GetLockedCount()
        {
            int count = 0;
            foreach (var kvp in _comboStates)
            {
                if (kvp.Value.isLocked) count++;
            }
            return count;
        }
        
        /// <summary>
        /// 获取某个 combo 的遗忘信息（用于 UI 显示）
        /// </summary>
        public (int gamesSinceLastUse, bool isLocked, bool isDormant, int totalUseCount) GetForgetInfo(string comboId)
        {
            if (!_comboStates.TryGetValue(comboId, out var entry))
                return (0, false, false, 0);
            return (entry.gamesSinceLastUse, entry.isLocked, entry.isDormant, entry.totalUseCount);
        }
        
        /// <summary>
        /// 获取所有锁定的 combo ID 列表
        /// </summary>
        public List<string> GetLockedComboIds()
        {
            var result = new List<string>();
            foreach (var kvp in _comboStates)
            {
                if (kvp.Value.isLocked) result.Add(kvp.Key);
            }
            return result;
        }
        
        // ========== 存档 ==========
        
        public Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            var entries = new List<Dictionary>();
            foreach (var kvp in _comboStates)
            {
                var e = new Dictionary
                {
                    ["comboId"] = kvp.Key,
                    ["gamesSinceLastUse"] = kvp.Value.gamesSinceLastUse,
                    ["totalUseCount"] = kvp.Value.totalUseCount,
                    ["isLocked"] = kvp.Value.isLocked,
                    ["isDormant"] = kvp.Value.isDormant,
                    ["wasEverDiscovered"] = kvp.Value.wasEverDiscovered
                };
                entries.Add(e);
            }
            data["entries"] = entries;
            return data;
        }
        
        public void ImportSaveData(Dictionary data)
        {
            _comboStates.Clear();
            if (data == null || !data.ContainsKey("entries")) return;
            
            var entries = (List<object>)data["entries"];
            foreach (Dictionary entryData in entries)
            {
                var entry = new ComboForgetEntry
                {
                    comboId = (string)entryData["comboId"],
                    gamesSinceLastUse = (int)entryData["gamesSinceLastUse"],
                    totalUseCount = (int)entryData["totalUseCount"],
                    isLocked = (bool)entryData["isLocked"],
                    isDormant = (bool)entryData["isDormant"],
                    wasEverDiscovered = (bool)entryData["wasEverDiscovered"]
                };
                _comboStates[entry.comboId] = entry;
            }
        }
    }
}
