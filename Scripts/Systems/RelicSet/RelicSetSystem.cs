using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 遗物套装系统 - 提供套装加成功能
    /// </summary>
    public class RelicSetSystem : BaseSystem
    {
        private static RelicSetSystem _instance;
        public static RelicSetSystem Instance => _instance ??= new RelicSetSystem();

        private RelicSetData _data = new RelicSetData();
        
        // 套装加成缓存
        private Dictionary<string, float> _cachedSetBonuses = new Dictionary<string, float>();
        
        // 统计追踪
        private int _totalSetsCompleted = 0;
        private int _totalPiecesEquipped = 0;
        
        public event Action OnSetBonusChanged;

        protected override void Initialize()
        {
            base.Initialize();
            _instance = this;
            LoadData();
        }

        public RelicSetSystem()
        {
            // 实际初始化在 Initialize() 中进行
        }

        #region Data Management

        private void LoadData()
        {
            // 从存档加载数据
            if (SaveSystem.Exists("relic_set_data"))
            {
                _data = SaveSystem.Load<RelicSetData>("relic_set_data");
            }
            RecalculateBonuses();
        }

        public void SaveData()
        {
            SaveSystem.Save(_data, "relic_set_data");
        }

        #endregion

        #region Core Mechanics

        /// <summary>
        /// 装备遗物
        /// </summary>
        public bool EquipRelic(string relicId)
        {
            if (string.IsNullOrEmpty(relicId) || _data.EquippedRelicIds.Contains(relicId))
                return false;

            _data.EquippedRelicIds.Add(relicId);
            _totalPiecesEquipped++;
            
            // 检查是否解锁新套装
            CheckSetUnlock(relicId);
            
            RecalculateBonuses();
            SaveData();
            
            GD.Print($"[RelicSetSystem] Equipped relic: {relicId}");
            return true;
        }

        /// <summary>
        /// 卸下遗物
        /// </summary>
        public bool UnequipRelic(string relicId)
        {
            if (!_data.EquippedRelicIds.Contains(relicId))
                return false;

            _data.EquippedRelicIds.Remove(relicId);
            
            RecalculateBonuses();
            SaveData();
            
            GD.Print($"[RelicSetSystem] Unequipped relic: {relicId}");
            return true;
        }

        /// <summary>
        /// 检查套装解锁
        /// </summary>
        private void CheckSetUnlock(string relicId)
        {
            var db = RelicSetDatabase.Instance;
            
            foreach (var set in db.GetAllSets())
            {
                if (set.RelicIds.Contains(relicId) && !_data.UnlockedSetIds.Contains(set.Id))
                {
                    // 检查是否已有该套装的全部遗物
                    int ownedCount = 0;
                    foreach (var rid in set.RelicIds)
                    {
                        if (_data.EquippedRelicIds.Contains(rid))
                            ownedCount++;
                    }
                    
                    if (ownedCount >= 2)  // 至少2件才能解锁套装
                    {
                        _data.UnlockedSetIds.Add(set.Id);
                        GD.Print($"[RelicSetSystem] Unlocked new set: {set.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// 重新计算套装加成
        /// </summary>
        private void RecalculateBonuses()
        {
            _cachedSetBonuses.Clear();
            
            var db = RelicSetDatabase.Instance;
            
            foreach (var setId in _data.UnlockedSetIds)
            {
                var set = db.GetSet(setId);
                if (set == null) continue;
                
                // 计算当前装备的套装件数
                int equippedCount = 0;
                foreach (var relicId in set.RelicIds)
                {
                    if (_data.EquippedRelicIds.Contains(relicId))
                        equippedCount++;
                }
                
                // 应用套装加成
                if (equippedCount >= 2 && set.SetBonuses.ContainsKey("2"))
                {
                    _cachedSetBonuses["attack"] = _cachedSetBonuses.GetValueOrDefault("attack", 0f) + set.SetBonuses["2"];
                }
                if (equippedCount >= 3 && set.SetBonuses.ContainsKey("3"))
                {
                    _cachedSetBonuses["defense"] = _cachedSetBonuses.GetValueOrDefault("defense", 0f) + set.SetBonuses["3"];
                }
                if (equippedCount >= 4 && set.SetBonuses.ContainsKey("4"))
                {
                    _cachedSetBonuses["health"] = _cachedSetBonuses.GetValueOrDefault("health", 0f) + set.SetBonuses["4"];
                }
                
                // 更新套装完成统计
                if (equippedCount >= set.PieceCount)
                {
                    if (!_data.SetCompletionCounts.ContainsKey(setId))
                        _data.SetCompletionCounts[setId] = 0;
                    _data.SetCompletionCounts[setId]++;
                    _totalSetsCompleted++;
                }
            }
            
            OnSetBonusChanged?.Invoke();
        }

        /// <summary>
        /// 获取当前套装加成
        /// </summary>
        public float GetSetBonus(string bonusType)
        {
            return _cachedSetBonuses.GetValueOrDefault(bonusType, 0f);
        }

        /// <summary>
        /// 获取所有套装加成
        /// </summary>
        public Dictionary<string, float> GetAllSetBonuses()
        {
            return new Dictionary<string, float>(_cachedSetBonuses);
        }

        /// <summary>
        /// 获取套装完成信息
        /// </summary>
        public Dictionary<string, int> GetSetCompletionInfo()
        {
            var result = new Dictionary<string, int>();
            var db = RelicSetDatabase.Instance;
            
            foreach (var setId in _data.UnlockedSetIds)
            {
                var set = db.GetSet(setId);
                if (set == null) continue;
                
                int equippedCount = 0;
                foreach (var relicId in set.RelicIds)
                {
                    if (_data.EquippedRelicIds.Contains(relicId))
                        equippedCount++;
                }
                
                result[setId] = equippedCount;
            }
            
            return result;
        }

        #endregion

        #region Statistics

        public int GetTotalSetsCompleted() => _totalSetsCompleted;
        public int GetTotalPiecesEquipped() => _totalPiecesEquipped;
        public List<string> GetUnlockedSets() => new List<string>(_data.UnlockedSetIds);
        public List<string> GetEquippedRelics() => new List<string>(_data.EquippedRelicIds);

        #endregion

        #region Utility

        /// <summary>
        /// 获取指定套装的当前装备件数
        /// </summary>
        public int GetEquippedCount(string setId)
        {
            var set = RelicSetDatabase.Instance.GetSet(setId);
            if (set == null) return 0;
            
            int count = 0;
            foreach (var relicId in set.RelicIds)
            {
                if (_data.EquippedRelicIds.Contains(relicId))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 获取套装加成描述
        /// </summary>
        public string GetSetBonusDescription(string setId)
        {
            var set = RelicSetDatabase.Instance.GetSet(setId);
            if (set == null) return "";
            
            int equippedCount = GetEquippedCount(setId);
            var description = new System.Text.StringBuilder();
            description.AppendLine(set.Description);
            description.AppendLine($"\n当前装备: {equippedCount}/{set.PieceCount}");
            
            if (equippedCount >= 2 && set.SetBonuses.ContainsKey("2"))
            {
                description.AppendLine($"2件套效果: +{set.SetBonuses["2"]*100:F0}% 攻击力");
            }
            if (equippedCount >= 3 && set.SetBonuses.ContainsKey("3"))
            {
                var bonus3 = set.SetBonuses["3"];
                if (bonus3 < 1f)
                    description.AppendLine($"3件套效果: +{bonus3*100:F0}% 防御力");
                else
                    description.AppendLine($"3件套效果: +{bonus3*100:F0}% 经验获取");
            }
            if (equippedCount >= 4 && set.SetBonuses.ContainsKey("4"))
            {
                var bonus4 = set.SetBonuses["4"];
                if (bonus4 < 1f)
                    description.AppendLine($"4件套效果: +{bonus4*100:F0}% 生命值");
                else
                    description.AppendLine($"4件套效果: +{bonus4*100:F0}% 技能冷却");
            }
            
            return description.ToString();
        }

        #endregion

        #region Save System
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Godot.Dictionary();
            
            // 保存已装备的遗物ID列表
            var equippedRelics = new Godot.Array();
            foreach (var relicId in _data.EquippedRelicIds)
            {
                equippedRelics.Add(relicId);
            }
            data["equipped_relics"] = equippedRelics;
            
            // 保存已解锁的套装ID列表
            var unlockedSets = new Godot.Array();
            foreach (var setId in _data.UnlockedSetIds)
            {
                unlockedSets.Add(setId);
            }
            data["unlocked_sets"] = unlockedSets;
            
            // 保存套装完成次数
            var setCompletionCounts = new Godot.Dictionary();
            foreach (var kvp in _data.SetCompletionCounts)
            {
                setCompletionCounts[kvp.Key] = kvp.Value;
            }
            data["set_completion_counts"] = setCompletionCounts;
            
            // 保存统计信息
            data["total_sets_completed"] = _totalSetsCompleted;
            data["total_pieces_equipped"] = _totalPiecesEquipped;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 加载已装备的遗物
            if (data.Contains("equipped_relics"))
            {
                _data.EquippedRelicIds.Clear();
                var equippedRelics = (Godot.Array)data["equipped_relics"];
                foreach (string relicId in equippedRelics)
                {
                    _data.EquippedRelicIds.Add(relicId);
                }
            }
            
            // 加载已解锁的套装
            if (data.Contains("unlocked_sets"))
            {
                _data.UnlockedSetIds.Clear();
                var unlockedSets = (Godot.Array)data["unlocked_sets"];
                foreach (string setId in unlockedSets)
                {
                    _data.UnlockedSetIds.Add(setId);
                }
            }
            
            // 加载套装完成次数
            if (data.Contains("set_completion_counts"))
            {
                _data.SetCompletionCounts.Clear();
                var setCompletionCounts = (Godot.Dictionary)data["set_completion_counts"];
                foreach (string setId in setCompletionCounts.Keys)
                {
                    _data.SetCompletionCounts[setId] = (int)setCompletionCounts[setId];
                }
            }
            
            // 加载统计信息
            if (data.Contains("total_sets_completed"))
                _totalSetsCompleted = (int)data["total_sets_completed"];
            if (data.Contains("total_pieces_equipped"))
                _totalPiecesEquipped = (int)data["total_pieces_equipped"];
            
            RecalculateBonuses();
            GD.Print($"[RelicSet] Loaded: {_data.EquippedRelicIds.Count} relics equipped, {_data.UnlockedSetIds.Count} sets unlocked");
        }
        
        #endregion
    }
}
