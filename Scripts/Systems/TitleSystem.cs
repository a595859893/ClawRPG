using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 玩家称号系统 - 基于玩家行为解锁称号
    /// </summary>
    public class TitleSystem {
        public static TitleSystem Instance { get; private set; }
        
        // 称号类型
        public enum TitleType {
            Achievement,    // 成就相关
            Level,          // 等级相关
            Quest,          // 任务相关
            Combat,         // 战斗相关
            Collection,     // 收集相关
            Special         // 特殊称号
        }
        
        // 称号稀有度
        public enum TitleRarity {
            Common,     // 普通 (灰)
            Uncommon,   // 优秀 (绿)
            Rare,       // 稀有 (蓝)
            Epic,       // 史诗 (紫)
            Legendary   // 传说 (橙)
        }
        
        // 称号数据结构
        public class Title {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public TitleType Type { get; set; }
            public TitleRarity Rarity { get; set; }
            public int RequiredValue { get; set; }  // 解锁所需值
            public bool IsUnlocked { get; set; }
            public DateTime? UnlockedTime { get; set; }
            
            public Title(string id, string name, string desc, TitleType type, TitleRarity rarity, int required) {
                Id = id;
                Name = name;
                Description = desc;
                Type = type;
                Rarity = rarity;
                RequiredValue = required;
                IsUnlocked = false;
                UnlockedTime = null;
            }
        }
        
        // 玩家当前称号
        public string CurrentTitleId { get; set; }
        
        // 已解锁称号列表
        private List<string> unlockedTitleIds = new List<string>();
        
        // 称号数据库
        private Dictionary<string, Title> titleDatabase = new Dictionary<string, Title>();
        
        // Tutorial tracking
        private bool _hasTriggeredFirstTitle = false;
        
        public TitleSystem() {
            Instance = this;
            InitializeDatabase();
        }
        
        private void InitializeDatabase() {
            // 等级称号
            AddTitle(new Title("title_level_10", "初出茅庐", "达到10级", TitleType.Level, TitleRarity.Common, 10));
            AddTitle(new Title("title_level_20", "资深冒险者", "达到20级", TitleType.Level, TitleRarity.Uncommon, 20));
            AddTitle(new Title("title_level_30", "精英猎手", "达到30级", TitleType.Level, TitleRarity.Rare, 30));
            AddTitle(new Title("title_level_40", "传奇英雄", "达到40级", TitleType.Level, TitleRarity.Epic, 40));
            AddTitle(new Title("title_level_50", "神话存在", "达到50级", TitleType.Level, TitleRarity.Legendary, 50));
            
            // 成就称号
            AddTitle(new Title("title_kill_100", "怪物杀手", "击杀100只怪物", TitleType.Achievement, TitleRarity.Common, 100));
            AddTitle(new Title("title_kill_500", "战斗老手", "击杀500只怪物", TitleType.Achievement, TitleRarity.Uncommon, 500));
            AddTitle(new Title("title_kill_1000", "千人斩", "击杀1000只怪物", TitleType.Achievement, TitleRarity.Rare, 1000));
            AddTitle(new Title("title_kill_5000", "战争机器", "击杀5000只怪物", TitleType.Achievement, TitleRarity.Epic, 5000));
            AddTitle(new Title("title_kill_10000", "死神", "击杀10000只怪物", TitleType.Achievement, TitleRarity.Legendary, 10000));
            
            // Boss称号
            AddTitle(new Title("title_boss_1", "Boss猎人", "击败1只Boss", TitleType.Combat, TitleRarity.Uncommon, 1));
            AddTitle(new Title("title_boss_5", "Boss杀手", "击败5只Boss", TitleType.Combat, TitleRarity.Rare, 5));
            AddTitle(new Title("title_boss_10", "Boss克星", "击败10只Boss", TitleType.Combat, TitleRarity.Epic, 10));
            AddTitle(new Title("title_boss_25", "Boss毁灭者", "击败25只Boss", TitleType.Combat, TitleRarity.Legendary, 25));
            
            // 任务称号
            AddTitle(new Title("title_quest_5", "新手任务达人", "完成5个任务", TitleType.Quest, TitleRarity.Common, 5));
            AddTitle(new Title("title_quest_15", "任务猎人", "完成15个任务", TitleType.Quest, TitleRarity.Uncommon, 15));
            AddTitle(new Title("title_quest_30", "任务大师", "完成30个任务", TitleType.Quest, TitleRarity.Rare, 30));
            AddTitle(new Title("title_quest_50", "任务传奇", "完成50个任务", TitleType.Quest, TitleRarity.Epic, 50));
            
            // 金币称号
            AddTitle(new Title("title_gold_1000", "小有积蓄", "拥有1000金币", TitleType.Collection, TitleRarity.Common, 1000));
            AddTitle(new Title("title_gold_10000", "富甲一方", "拥有10000金币", TitleType.Collection, TitleRarity.Uncommon, 10000));
            AddTitle(new Title("title_gold_50000", "金币大亨", "拥有50000金币", TitleType.Collection, TitleRarity.Rare, 50000));
            AddTitle(new Title("title_gold_100000", "财富之王", "拥有100000金币", TitleType.Collection, TitleRarity.Epic, 100000));
            AddTitle(new Title("title_gold_500000", "世界首富", "拥有500000金币", TitleType.Collection, TitleRarity.Legendary, 500000));
            
            // 特殊称号
            AddTitle(new Title("title_perfect_block_100", "完美防御者", "完成100次完美格挡", TitleType.Combat, TitleRarity.Uncommon, 100));
            AddTitle(new Title("title_perfect_block_500", "钢铁壁垒", "完成500次完美格挡", TitleType.Combat, TitleRarity.Rare, 500));
            AddTitle(new Title("title_dodge_100", "灵活闪避", "闪避100次", TitleType.Combat, TitleRarity.Uncommon, 100));
            AddTitle(new Title("title_dodge_500", "幽灵步伐", "闪避500次", TitleType.Combat, TitleRarity.Rare, 500));
            AddTitle(new Title("title_craft_50", "新手匠人", "合成50件物品", TitleType.Collection, TitleRarity.Common, 50));
            AddTitle(new Title("title_craft_200", "熟练工匠", "合成200件物品", TitleType.Collection, TitleRarity.Uncommon, 200));
            AddTitle(new Title("title_craft_500", "大师级铁匠", "合成500件物品", TitleType.Collection, TitleRarity.Rare, 500));
            
            // 连击称号
            AddTitle(new Title("title_combo_10", "连击新手", "达成10连击", TitleType.Combat, TitleRarity.Common, 10));
            AddTitle(new Title("title_combo_30", "连击达人", "达成30连击", TitleType.Combat, TitleRarity.Uncommon, 30));
            AddTitle(new Title("title_combo_50", "连击王者", "达成50连击", TitleType.Combat, TitleRarity.Rare, 50));
            AddTitle(new Title("title_combo_100", "连击神话", "达成100连击", TitleType.Combat, TitleRarity.Epic, 100));
            
            // 探索称号
            AddTitle(new Title("title_region_3", "初探世界", "探索3个区域", TitleType.Special, TitleRarity.Common, 3));
            AddTitle(new Title("title_region_5", "世界探索者", "探索5个区域", TitleType.Special, TitleRarity.Uncommon, 5));
            AddTitle(new Title("title_region_7", "世界征服者", "探索全部7个区域", TitleType.Special, TitleRarity.Rare, 7));
        }
        
        private void AddTitle(Title title) {
            titleDatabase[title.Id] = title;
        }
        
        /// <summary>
        /// 检查并解锁称号
        /// </summary>
        public void CheckAndUnlockTitle(string titleType, int currentValue) {
            foreach (var title in titleDatabase.Values) {
                if (title.Type.ToString() == titleType && !title.IsUnlocked) {
                    if (currentValue >= title.RequiredValue) {
                        UnlockTitle(title.Id);
                    }
                }
            }
        }
        
        /// <summary>
        /// 解锁指定称号
        /// </summary>
        public bool UnlockTitle(string titleId) {
            if (titleDatabase.TryGetValue(titleId, out var title)) {
                if (!title.IsUnlocked) {
                    title.IsUnlocked = true;
                    title.UnlockedTime = DateTime.Now;
                    unlockedTitleIds.Add(titleId);
                    
                    // 触发称号解锁事件
                    OnTitleUnlocked?.Invoke(title);
                    
                    // Trigger tutorial for first title
                    if (!_hasTriggeredFirstTitle)
                    {
                        _hasTriggeredFirstTitle = true;
                        TutorialSystem.Trigger(TutorialTrigger.FirstTitle);
                    }
                    
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 设置当前显示的称号
        /// </summary>
        public void SetCurrentTitle(string titleId) {
            if (titleDatabase.ContainsKey(titleId) && titleDatabase[titleId].IsUnlocked) {
                CurrentTitleId = titleId;
                OnCurrentTitleChanged?.Invoke(titleId);
            }
        }
        
        /// <summary>
        /// 获取当前称号名称
        /// </summary>
        public string GetCurrentTitleName() {
            if (!string.IsNullOrEmpty(CurrentTitleId) && titleDatabase.TryGetValue(CurrentTitleId, out var title)) {
                return title.Name;
            }
            return "";
        }
        
        /// <summary>
        /// 获取当前称号稀有度颜色
        /// </summary>
        public Color GetCurrentTitleColor() {
            if (!string.IsNullOrEmpty(CurrentTitleId) && titleDatabase.TryGetValue(CurrentTitleId, out var title)) {
                return GetRarityColor(title.Rarity);
            }
            return Colors.White;
        }
        
        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        public Color GetRarityColor(TitleRarity rarity) {
            switch (rarity) {
                case TitleRarity.Common: return new Color(0.7f, 0.7f, 0.7f);
                case TitleRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f);
                case TitleRarity.Rare: return new Color(0.2f, 0.5f, 1.0f);
                case TitleRarity.Epic: return new Color(0.6f, 0.3f, 0.9f);
                case TitleRarity.Legendary: return new Color(1.0f, 0.6f, 0.0f);
                default: return Colors.White;
            }
        }
        
        /// <summary>
        /// 获取已解锁称号列表
        /// </summary>
        public List<Title> GetUnlockedTitles() {
            List<Title> unlocked = new List<Title>();
            foreach (var id in unlockedTitleIds) {
                if (titleDatabase.TryGetValue(id, out var title)) {
                    unlocked.Add(title);
                }
            }
            return unlocked;
        }
        
        /// <summary>
        /// 获取所有称号列表
        /// </summary>
        public List<Title> GetAllTitles() {
            return new List<Title>(titleDatabase.Values);
        }
        
        /// <summary>
        /// 获取指定类型的称号
        /// </summary>
        public List<Title> GetTitlesByType(TitleType type) {
            List<Title> result = new List<Title>();
            foreach (var title in titleDatabase.Values) {
                if (title.Type == type) {
                    result.Add(title);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 获取称号进度
        /// </summary>
        public float GetTitleProgress(string titleId, int currentValue) {
            if (titleDatabase.TryGetValue(titleId, out var title)) {
                if (title.IsUnlocked) return 1.0f;
                return Mathf.Clamp((float)currentValue / title.RequiredValue, 0f, 1f);
            }
            return 0f;
        }
        
        // 称号解锁事件
        public event Action<Title> OnTitleUnlocked;
        
        // 当前称号变更事件
        public event Action<string> OnCurrentTitleChanged;
        
        /// <summary>
        /// 获取当前显示的称号对象
        /// </summary>
        public Title GetCurrentTitle() {
            if (!string.IsNullOrEmpty(CurrentTitleId) && titleDatabase.TryGetValue(CurrentTitleId, out var title)) {
                return title;
            }
            return null;
        }
        
        /// <summary>
        /// 序列化存档数据
        /// </summary>
        public Dictionary<string, object> Serialize() {
            var data = new Dictionary<string, object>();
            data["currentTitleId"] = CurrentTitleId ?? "";
            data["unlockedTitleIds"] = unlockedTitleIds;
            
            // 序列化每个称号的解锁时间
            var unlockTimes = new Dictionary<string, string>();
            foreach (var id in unlockedTitleIds) {
                if (titleDatabase.TryGetValue(id, out var title) && title.UnlockedTime.HasValue) {
                    unlockTimes[id] = title.UnlockedTime.Value.ToString("o");
                }
            }
            data["unlockTimes"] = unlockTimes;
            
            return data;
        }
        
        /// <summary>
        /// 反序列化存档数据
        /// </summary>
        public void Deserialize(Dictionary<string, object> data) {
            if (data == null) return;
            
            if (data.ContainsKey("currentTitleId")) {
                CurrentTitleId = data["currentTitleId"] as string;
            }
            
            if (data.ContainsKey("unlockedTitleIds")) {
                unlockedTitleIds = new List<string>(data["unlockedTitleIds"] as System.Collections.IEnumerable);
                
                // 恢复称号解锁状态
                foreach (var id in unlockedTitleIds) {
                    if (titleDatabase.TryGetValue(id, out var title)) {
                        title.IsUnlocked = true;
                    }
                }
            }
            
            // 恢复解锁时间
            if (data.ContainsKey("unlockTimes")) {
                var unlockTimes = data["unlockTimes"] as Dictionary<string, object>;
                if (unlockTimes != null) {
                    foreach (var kvp in unlockTimes) {
                        if (titleDatabase.TryGetValue(kvp.Key, out var title) && DateTime.TryParse(kvp.Value as string, out var time)) {
                            title.UnlockedTime = time;
                        }
                    }
                }
            }
        }
    }
}
