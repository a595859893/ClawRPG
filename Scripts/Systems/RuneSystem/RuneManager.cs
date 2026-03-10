using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 玩家符文数据（用于存档）
    /// </summary>
    [System.Serializable]
    public class PlayerRuneData {
        public List<string> OwnedRuneIds { get; set; }  // 拥有的符文ID列表
        public int[] EquippedRuneSlotIndexes { get; set; }  // 装备的符文（槽位索引 -> 符文ID索引）
        public int[] UnlockedSlots { get; set; }  // 已解锁的槽位

        public PlayerRuneData() {
            OwnedRuneIds = new List<string>();
            EquippedRuneSlotIndexes = new int[5];
            UnlockedSlots = new int[5];
            
            // 默认解锁第一个槽位
            for (int i = 0; i < 5; i++) {
                EquippedRuneSlotIndexes[i] = -1;
                UnlockedSlots[i] = i == 0 ? 1 : 0;
            }
        }
    }

    /// <summary>
    /// 符文管理器 - 管理玩家符文
    /// </summary>
    public class RuneManager {
        private static RuneManager _instance;
        public static RuneManager Instance {
            get {
                if (_instance == null) {
                    _instance = new RuneManager();
                }
                return _instance;
            }
        }

        // 玩家拥有的符文
        private List<Rune> _ownedRunes;
        
        // 装备的符文槽位（5个槽位）
        private EquipmentRuneSlot[] _equipmentSlots;
        
        // 符文背包容量
        private const int MAX_RUNE_INVENTORY = 50;

        // 信号
        public delegate void RuneEvent();
        public event RuneEvent OnRunesUpdated;
        public event RuneEvent OnRuneEquipped;
        public event RuneEvent OnSlotUnlocked;

        public RuneManager() {
            _ownedRunes = new List<Rune>();
            _equipmentSlots = new EquipmentRuneSlot[5];
            
            for (int i = 0; i < 5; i++) {
                _equipmentSlots[i] = new EquipmentRuneSlot(i);
            }
        }

        /// <summary>
        /// 初始化（从存档加载）
        /// </summary>
        public void Initialize(PlayerRuneData data) {
            if (data == null) return;

            _ownedRunes.Clear();
            
            // 加载拥有的符文
            foreach (string runeId in data.OwnedRuneIds) {
                Rune rune = RuneDatabase.Instance.GetRune(runeId);
                if (rune != null) {
                    _ownedRunes.Add(rune);
                }
            }

            // 加载装备槽位
            for (int i = 0; i < 5; i++) {
                _equipmentSlots[i] = new EquipmentRuneSlot(i);
                _equipmentSlots[i].IsUnlocked = data.UnlockedSlots[i] == 1;
                
                if (data.EquippedRuneSlotIndexes[i] >= 0 && 
                    data.EquippedRuneSlotIndexes[i] < _ownedRunes.Count) {
                    _equipmentSlots[i].EquippedRune = _ownedRunes[data.EquippedRuneSlotIndexes[i]];
                }
            }
        }

        /// <summary>
        /// 获取存档数据
        /// </summary>
        public PlayerRuneData GetSaveData() {
            PlayerRuneData data = new PlayerRuneData();
            
            // 保存拥有的符文
            foreach (Rune rune in _ownedRunes) {
                data.OwnedRuneIds.Add(rune.Id);
            }

            // 保存装备槽位
            for (int i = 0; i < 5; i++) {
                data.UnlockedSlots[i] = _equipmentSlots[i].IsUnlocked ? 1 : 0;
                
                if (_equipmentSlots[i].EquippedRune != null) {
                    int runeIndex = _ownedRunes.IndexOf(_equipmentSlots[i].EquippedRune);
                    data.EquippedRuneSlotIndexes[i] = runeIndex;
                } else {
                    data.EquippedRuneSlotIndexes[i] = -1;
                }
            }

            return data;
        }

        /// <summary>
        /// 添加符文到背包
        /// </summary>
        public bool AddRune(Rune rune) {
            if (_ownedRunes.Count >= MAX_RUNE_INVENTORY) {
                GD.Print("符文背包已满！");
                return false;
            }

            if (rune == null) return false;

            _ownedRunes.Add(rune);
            OnRunesUpdated?.Invoke();
            return true;
        }

        /// <summary>
        /// 移除符文
        /// </summary>
        public bool RemoveRune(Rune rune) {
            if (rune == null) return false;

            // 如果装备中，先卸下
            for (int i = 0; i < 5; i++) {
                if (_equipmentSlots[i].EquippedRune == rune) {
                    UnequipRune(i);
                }
            }

            bool removed = _ownedRunes.Remove(rune);
            if (removed) {
                OnRunesUpdated?.Invoke();
            }
            return removed;
        }

        /// <summary>
        /// 装备符文到槽位
        /// </summary>
        public bool EquipRune(Rune rune, int slotIndex) {
            if (slotIndex < 0 || slotIndex >= 5) return false;
            if (!_equipmentSlots[slotIndex].IsUnlocked) return false;

            // 如果该槽位已有符文，先卸下
            if (_equipmentSlots[slotIndex].EquippedRune != null) {
                UnequipRune(slotIndex);
            }

            // 检查玩家是否拥有该符文
            if (!_ownedRunes.Contains(rune)) return false;

            _equipmentSlots[slotIndex].EquippedRune = rune;
            OnRuneEquipped?.Invoke();
            return true;
        }

        /// <summary>
        /// 卸下槽位符文
        /// </summary>
        public Rune UnequipRune(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= 5) return null;
            
            Rune unequipped = _equipmentSlots[slotIndex].EquippedRune;
            _equipmentSlots[slotIndex].EquippedRune = null;
            OnRuneEquipped?.Invoke();
            return unequipped;
        }

        /// <summary>
        /// 解锁槽位
        /// </summary>
        public bool UnlockSlot(int slotIndex, int playerGold) {
            if (slotIndex < 0 || slotIndex >= 5) return false;
            if (_equipmentSlots[slotIndex].IsUnlocked) return false;

            int cost = _equipmentSlots[slotIndex].UnlockCost;
            if (playerGold < cost) return false;

            _equipmentSlots[slotIndex].IsUnlocked = true;
            OnSlotUnlocked?.Invoke();
            return true;
        }

        /// <summary>
        /// 获取槽位解锁费用
        /// </summary>
        public int GetSlotUnlockCost(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= 5) return 0;
            return _equipmentSlots[slotIndex].UnlockCost;
        }

        /// <summary>
        /// 槽位是否已解锁
        /// </summary>
        public bool IsSlotUnlocked(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= 5) return false;
            return _equipmentSlots[slotIndex].IsUnlocked;
        }

        /// <summary>
        /// 获取装备的符文
        /// </summary>
        public Rune GetEquippedRune(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= 5) return null;
            return _equipmentSlots[slotIndex].EquippedRune;
        }

        /// <summary>
        /// 获取所有装备的符文
        /// </summary>
        public List<Rune> GetAllEquippedRunes() {
            List<Rune> equipped = new List<Rune>();
            for (int i = 0; i < 5; i++) {
                if (_equipmentSlots[i].EquippedRune != null) {
                    equipped.Add(_equipmentSlots[i].EquippedRune);
                }
            }
            return equipped;
        }

        /// <summary>
        /// 获取拥有的符文列表
        /// </summary>
        public List<Rune> GetOwnedRunes() {
            return new List<Rune>(_ownedRunes);
        }

        /// <summary>
        /// 计算符文提供的总属性加成
        /// </summary>
        public Dictionary<RuneAttribute, float> CalculateTotalAttributes() {
            Dictionary<RuneAttribute, float> total = new Dictionary<RuneAttribute, float>();

            foreach (Rune rune in GetAllEquippedRunes()) {
                foreach (var attr in rune.Attributes) {
                    if (total.ContainsKey(attr.Key)) {
                        total[attr.Key] += attr.Value;
                    } else {
                        total[attr.Key] = attr.Value;
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// 获取装备槽位数组
        /// </summary>
        public EquipmentRuneSlot[] GetEquipmentSlots() {
            return _equipmentSlots;
        }

        /// <summary>
        /// 获取符文背包容量
        /// </summary>
        public int GetInventoryCapacity() {
            return MAX_RUNE_INVENTORY;
        }

        /// <summary>
        /// 获取当前符文数量
        /// </summary>
        public int GetRuneCount() {
            return _ownedRunes.Count;
        }

        /// <summary>
        /// 检查是否拥有特定符文
        /// </summary>
        public bool HasRune(string runeId) {
            foreach (Rune rune in _ownedRunes) {
                if (rune.Id == runeId) return true;
            }
            return false;
        }
    }
}
