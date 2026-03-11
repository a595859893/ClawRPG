using Godot;
using System;
using System.Collections.Generic;
using Game.EquipmentSetDataSpace;

namespace Game
{
    /// <summary>
    /// 装备套装系统管理器
    /// </summary>
    public class EquipmentSetSystem : Node
    {
        private static EquipmentSetSystem _instance;
        public static EquipmentSetSystem Instance
        {
            get { return _instance; }
        }

        // 玩家套装数据
        private PlayerSetData _playerSetData = new PlayerSetData();

        // 信号定义
        [Signal]
        public delegate void SetItemAcquired(string setId, string itemId);

        [Signal]
        public delegate void SetCompleted(string setId);

        [Signal]
        public delegate void BonusActivated(string setId, int pieceCount);

        public override void _Ready()
        {
            _instance = this;
        }

        /// <summary>
        /// 获取玩家套装数据
        /// </summary>
        public PlayerSetData GetPlayerSetData()
        {
            return _playerSetData;
        }

        /// <summary>
        /// 加载玩家套装数据
        /// </summary>
        public void LoadPlayerSetData(PlayerSetData data)
        {
            if (data != null)
            {
                _playerSetData = data;
            }
        }

        /// <summary>
        /// 添加套装物品
        /// </summary>
        public void AddSetItem(string itemId)
        {
            var db = EquipmentSetDatabase.Instance;
            var setId = db.GetSetIdByItemId(itemId);
            
            if (setId == null)
                return;

            // 初始化套装数据
            if (!_playerSetData.OwnedItems.ContainsKey(setId))
            {
                _playerSetData.OwnedItems[setId] = new List<string>();
            }

            // 检查是否已拥有
            if (_playerSetData.OwnedItems[setId].Contains(itemId))
                return;

            // 添加物品
            _playerSetData.OwnedItems[setId].Add(itemId);

            // 检查套装完成
            var set = db.GetSet(setId);
            int pieceCount = _playerSetData.OwnedItems[setId].Count;

            // 发送信号
            EmitSignal(nameof(SetItemAcquired), setId, itemId);

            // 检查是否激活新效果
            CheckBonusActivation(setId, pieceCount);

            // 检查是否完成套装
            if (pieceCount >= set.Items.Count && !_playerSetData.ActivatedBonuses.ContainsKey(setId + "_complete"))
            {
                _playerSetData.ActivatedBonuses[setId + "_complete"] = set.Items.Count;
                EmitSignal(nameof(SetCompleted), setId);
            }
        }

        /// <summary>
        /// 检查并激活套装效果
        /// </summary>
        private void CheckBonusActivation(string setId, int pieceCount)
        {
            var set = EquipmentSetDatabase.Instance.GetSet(setId);
            if (set == null)
                return;

            foreach (var bonus in set.Bonuses)
            {
                if (pieceCount >= bonus.PieceCount)
                {
                    string key = setId + "_" + bonus.PieceCount;
                    if (!_playerSetData.ActivatedBonuses.ContainsKey(key))
                    {
                        _playerSetData.ActivatedBonuses[key] = bonus.PieceCount;
                        EmitSignal(nameof(BonusActivated), setId, bonus.PieceCount);
                    }
                }
            }
        }

        /// <summary>
        /// 检查玩家是否拥有指定套装物品
        /// </summary>
        public bool HasSetItem(string setId, string itemId)
        {
            if (!_playerSetData.OwnedItems.ContainsKey(setId))
                return false;
            return _playerSetData.OwnedItems[setId].Contains(itemId);
        }

        /// <summary>
        /// 获取指定套装的物品数量
        /// </summary>
        public int GetSetPieceCount(string setId)
        {
            if (!_playerSetData.OwnedItems.ContainsKey(setId))
                return 0;
            return _playerSetData.OwnedItems[setId].Count;
        }

        /// <summary>
        /// 获取套装已激活的效果数量
        /// </summary>
        public int GetActivatedBonusCount(string setId)
        {
            int count = 0;
            var set = EquipmentSetDatabase.Instance.GetSet(setId);
            if (set == null)
                return 0;

            foreach (var bonus in set.Bonuses)
            {
                string key = setId + "_" + bonus.PieceCount;
                if (_playerSetData.ActivatedBonuses.ContainsKey(key))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 获取套装收集进度
        /// </summary>
        public float GetSetProgress(string setId)
        {
            var set = EquipmentSetDatabase.Instance.GetSet(setId);
            if (set == null)
                return 0f;

            int owned = GetSetPieceCount(setId);
            return (float)owned / set.Items.Count;
        }

        /// <summary>
        /// 获取套装是否完成
        /// </summary>
        public bool IsSetComplete(string setId)
        {
            var set = EquipmentSetDatabase.Instance.GetSet(setId);
            if (set == null)
                return false;
            return GetSetPieceCount(setId) >= set.Items.Count;
        }

        /// <summary>
        /// 获取套装属性加成
        /// </summary>
        public Dictionary<string, float> GetSetBonuses()
        {
            var bonuses = new Dictionary<string, float>();
            bonuses["AttackBonus"] = 0f;
            bonuses["DefenseBonus"] = 0f;
            bonuses["HealthBonus"] = 0f;
            bonuses["MagicBonus"] = 0f;
            bonuses["SpeedBonus"] = 0f;
            bonuses["CritRateBonus"] = 0f;
            bonuses["CritDamageBonus"] = 0f;
            bonuses["LifeStealBonus"] = 0f;
            bonuses["DodgeBonus"] = 0f;
            bonuses["EXPBonus"] = 0f;
            bonuses["GoldBonus"] = 0f;

            foreach (var setKvp in _playerSetData.OwnedItems)
            {
                var set = EquipmentSetDatabase.Instance.GetSet(setKvp.Key);
                if (set == null)
                    continue;

                int pieceCount = setKvp.Value.Count;

                // 累加已激活的效果
                foreach (var bonus in set.Bonuses)
                {
                    if (pieceCount >= bonus.PieceCount)
                    {
                        bonuses["AttackBonus"] += bonus.AttackBonus;
                        bonuses["DefenseBonus"] += bonus.DefenseBonus;
                        bonuses["HealthBonus"] += bonus.HealthBonus;
                        bonuses["MagicBonus"] += bonus.MagicBonus;
                        bonuses["SpeedBonus"] += bonus.SpeedBonus;
                        bonuses["CritRateBonus"] += bonus.CritRateBonus;
                        bonuses["CritDamageBonus"] += bonus.CritDamageBonus;
                        bonuses["LifeStealBonus"] += bonus.LifeStealBonus;
                        bonuses["DodgeBonus"] += bonus.DodgeBonus;
                        bonuses["EXPBonus"] += bonus.EXPBonus;
                        bonuses["GoldBonus"] += bonus.GoldBonus;
                    }
                }
            }

            return bonuses;
        }

        /// <summary>
        /// 获取玩家套装统计
        /// </summary>
        public SetStatistics GetStatistics()
        {
            var stats = new SetStatistics();
            var db = EquipmentSetDatabase.Instance;
            var allSets = db.GetAllSets();

            stats.TotalSets = allSets.Count;
            stats.MaxPieceCount = 0;

            foreach (var set in allSets)
            {
                int pieceCount = GetSetPieceCount(set.SetId);
                stats.SetPieceCounts[set.SetId] = pieceCount;

                if (pieceCount >= set.Items.Count)
                    stats.CompletedSets++;

                if (pieceCount > stats.MaxPieceCount)
                    stats.MaxPieceCount = pieceCount;
            }

            return stats;
        }

        /// <summary>
        /// 获取套装列表
        /// </summary>
        public List<EquipmentSet> GetOwnedSets()
        {
            var result = new List<EquipmentSet>();
            foreach (var setId in _playerSetData.OwnedItems.Keys)
            {
                var set = EquipmentSetDatabase.Instance.GetSet(setId);
                if (set != null)
                    result.Add(set);
            }
            return result;
        }

        /// <summary>
        /// 获取所有套装（包括未拥有的）
        /// </summary>
        public List<EquipmentSet> GetAllSetsWithOwnership()
        {
            var result = new List<EquipmentSet>();
            var db = EquipmentSetDatabase.Instance;
            
            foreach (var set in db.GetAllSets())
            {
                // 标记物品拥有状态
                if (_playerSetData.OwnedItems.ContainsKey(set.SetId))
                {
                    result.Add(set);
                }
            }
            
            // 添加未拥有的套装
            foreach (var set in db.GetAllSets())
            {
                if (!_playerSetData.OwnedItems.ContainsKey(set.SetId))
                {
                    result.Add(set);
                }
            }
            
            return result;
        }

        /// <summary>
        /// 保存套装数据
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 保存已拥有的物品
            var ownedItems = new Dictionary<string, List<string>>();
            foreach (var kvp in _playerSetData.OwnedItems)
            {
                ownedItems[kvp.Key] = kvp.Value;
            }
            data["owned_items"] = ownedItems;
            
            // 保存已激活的效果
            var activatedBonuses = new Dictionary<string, int>();
            foreach (var kvp in _playerSetData.ActivatedBonuses)
            {
                activatedBonuses[kvp.Key] = kvp.Value;
            }
            data["activated_bonuses"] = activatedBonuses;
            
            return data;
        }

        /// <summary>
        /// 加载套装数据
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null)
                return;

            _playerSetData = new PlayerSetData();

            // 加载已拥有的物品
            if (data.ContainsKey("owned_items"))
            {
                var ownedItems = (Dictionary<string, object>)data["owned_items"];
                foreach (var kvp in ownedItems)
                {
                    var items = new List<string>();
                    var itemList = (Godot.Collections.Array)kvp.Value;
                    foreach (var item in itemList)
                    {
                        items.Add((string)item);
                    }
                    _playerSetData.OwnedItems[kvp.Key] = items;
                }
            }

            // 加载已激活的效果
            if (data.ContainsKey("activated_bonuses"))
            {
                var activatedBonuses = (Dictionary<string, object>)data["activated_bonuses"];
                foreach (var kvp in activatedBonuses)
                {
                    _playerSetData.ActivatedBonuses[kvp.Key] = (int)(long)kvp.Value;
                }
            }
        }
    }
}
