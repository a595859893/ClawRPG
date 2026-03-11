namespace ClawRPG.Scripts.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ClawRPG.Scripts.Data;
    using ClawRPG.Scripts.Database;

    /// <summary>
    /// 符文系统管理器
    /// </summary>
    public class RuneSystem
    {
        public static RuneSystem Instance { get; private set; }

        // 信号系统
        public Action<Rune> OnRuneAdded;
        public Action<Rune> OnRuneRemoved;
        public Action<Rune> OnRuneEquipped;
        public Action<Rune> OnRuneUnequipped;
        public Action<RuneSet, int> OnSetBonusActivated;

        private PlayerRuneData _playerData = new PlayerRuneData();
        private readonly int _maxRuneSlots = 6;

        public RuneSystem()
        {
            Instance = this;
        }

        /// <summary>
        /// 初始化符文系统
        /// </summary>
        public void Initialize()
        {
            Instance = this;
        }

        /// <summary>
        /// 添加符文
        /// </summary>
        public RuneInstance AddRune(Rune rune)
        {
            if (rune == null) return null;

            var instance = new RuneInstance
            {
                UniqueId = Guid.NewGuid().ToString(),
                RuneId = rune.Id,
                SlotIndex = -1,
                IsLocked = false
            };

            _playerData.OwnedRunes.Add(instance);
            _playerData.TotalRunesFound++;
            _playerData.DiscoveredRunes.Add(rune.Id);

            OnRuneAdded?.Invoke(rune);
            return instance;
        }

        /// <summary>
        /// 随机添加符文
        /// </summary>
        public RuneInstance AddRandomRune()
        {
            var rune = RuneDatabase.GetRandomRune();
            return AddRune(rune);
        }

        /// <summary>
        /// 移除符文
        /// </summary>
        public bool RemoveRune(string uniqueId)
        {
            var instance = _playerData.OwnedRunes.FirstOrDefault(r => r.UniqueId == uniqueId);
            if (instance == null) return false;

            // 如果装备则卸下
            if (instance.SlotIndex >= 0)
            {
                UnequipRune(uniqueId);
            }

            var rune = GetRuneData(instance.RuneId);
            _playerData.OwnedRunes.Remove(instance);
            OnRuneRemoved?.Invoke(rune);
            return true;
        }

        /// <summary>
        /// 装备符文
        /// </summary>
        public bool EquipRune(string uniqueId, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _maxRuneSlots) return false;

            var instance = _playerData.OwnedRunes.FirstOrDefault(r => r.UniqueId == uniqueId);
            if (instance == null) return false;

            var rune = GetRuneData(instance.RuneId);
            if (rune == null) return false;

            // 检查槽位是否已有符文
            var existingInSlot = _playerData.OwnedRunes.FirstOrDefault(r => r.SlotIndex == slotIndex);
            if (existingInSlot != null)
            {
                // 卸下现有符文
                UnequipRune(existingInSlot.UniqueId);
            }

            instance.SlotIndex = slotIndex;
            rune.IsEquipped = true;
            OnRuneEquipped?.Invoke(rune);

            // 检查套装效果
            CheckSetBonus();

            return true;
        }

        /// <summary>
        /// 卸下符文
        /// </summary>
        public bool UnequipRune(string uniqueId)
        {
            var instance = _playerData.OwnedRunes.FirstOrDefault(r => r.UniqueId == uniqueId);
            if (instance == null || instance.SlotIndex < 0) return false;

            var rune = GetRuneData(instance.RuneId);
            instance.SlotIndex = -1;
            if (rune != null) rune.IsEquipped = false;

            OnRuneUnequipped?.Invoke(rune);
            return true;
        }

        /// <summary>
        /// 获取已装备的符文
        /// </summary>
        public List<Rune> GetEquippedRunes()
        {
            var equipped = new List<Rune>();
            foreach (var instance in _playerData.OwnedRunes)
            {
                if (instance.SlotIndex >= 0)
                {
                    var rune = GetRuneData(instance.RuneId);
                    if (rune != null) equipped.Add(rune);
                }
            }
            return equipped;
        }

        /// <summary>
        /// 获取玩家拥有的所有符文
        /// </summary>
        public List<Rune> GetAllOwnedRunes()
        {
            var runes = new List<Rune>();
            foreach (var instance in _playerData.OwnedRunes)
            {
                var rune = GetRuneData(instance.RuneId);
                if (rune != null)
                {
                    rune.IsEquipped = instance.SlotIndex >= 0;
                    runes.Add(rune);
                }
            }
            return runes;
        }

        /// <summary>
        /// 获取符文数据
        /// </summary>
        public Rune GetRuneData(string runeId)
        {
            return RuneDatabase.Runes.ContainsKey(runeId) ? RuneDatabase.Runes[runeId] : null;
        }

        /// <summary>
        /// 获取符文实例
        /// </summary>
        public RuneInstance GetRuneInstance(string uniqueId)
        {
            return _playerData.OwnedRunes.FirstOrDefault(r => r.UniqueId == uniqueId);
        }

        /// <summary>
        /// 获取属性加成
        /// </summary>
        public Dictionary<RuneType, float> GetAttributeBonuses()
        {
            var bonuses = new Dictionary<RuneType, float>();
            foreach (RuneType type in Enum.GetValues(typeof(RuneType)))
            {
                bonuses[type] = 0f;
            }

            foreach (var instance in _playerData.OwnedRunes)
            {
                if (instance.SlotIndex >= 0)
                {
                    var rune = GetRuneData(instance.RuneId);
                    if (rune != null && bonuses.ContainsKey(rune.Type))
                    {
                        bonuses[rune.Type] += rune.AttributeValue;
                    }
                }
            }

            return bonuses;
        }

        /// <summary>
        /// 获取总攻击加成
        /// </summary>
        public float GetTotalAttackBonus()
        {
            var bonuses = GetAttributeBonuses();
            return bonuses[RuneType.Attack] + bonuses[RuneType.Magic];
        }

        /// <summary>
        /// 获取总防御加成
        /// </summary>
        public float GetTotalDefenseBonus()
        {
            return GetAttributeBonuses()[RuneType.Defense];
        }

        /// <summary>
        /// 获取总生命加成
        /// </summary>
        public float GetTotalHealthBonus()
        {
            return GetAttributeBonuses()[RuneType.Health];
        }

        /// <summary>
        /// 获取总速度加成
        /// </summary>
        public float GetTotalSpeedBonus()
        {
            return GetAttributeBonuses()[RuneType.Speed];
        }

        /// <summary>
        /// 获取总暴击加成
        /// </summary>
        public float GetTotalCriticalBonus()
        {
            return GetAttributeBonuses()[RuneType.Critical];
        }

        /// <summary>
        /// 获取总生命偷取加成
        /// </summary>
        public float GetTotalLifeStealBonus()
        {
            return GetAttributeBonuses()[RuneType.LifeSteal];
        }

        /// <summary>
        /// 获取总闪避加成
        /// </summary>
        public float GetTotalDodgeBonus()
        {
            return GetAttributeBonuses()[RuneType.Dodge];
        }

        /// <summary>
        /// 检查套装效果
        /// </summary>
        private void CheckSetBonus()
        {
            var equipped = GetEquippedRunes();
            var typeCounts = new Dictionary<RuneType, int>();
            foreach (RuneType type in Enum.GetValues(typeof(RuneType)))
            {
                typeCounts[type] = 0;
            }

            foreach (var rune in equipped)
            {
                typeCounts[rune.Type]++;
            }

            // 检查每个套装
            foreach (var set in RuneDatabase.RuneSets.Values)
            {
                int matchCount = 0;
                for (int i = 0; i < 8; i++)
                {
                    var type = (RuneType)i;
                    matchCount += Math.Min(typeCounts[type], set.RuneTypeCounts[i]);
                }

                int setLevel = 0;
                if (matchCount >= 6) setLevel = 3;
                else if (matchCount >= 4) setLevel = 2;
                else if (matchCount >= 2) setLevel = 1;

                if (setLevel > 0)
                {
                    OnSetBonusActivated?.Invoke(set, setLevel);
                }
            }
        }

        /// <summary>
        /// 获取套装加成
        /// </summary>
        public (string SetName, int Level, float Bonus) GetActiveSetBonus()
        {
            var equipped = GetEquippedRunes();
            var typeCounts = new Dictionary<RuneType, int>();
            foreach (RuneType type in Enum.GetValues(typeof(RuneType)))
            {
                typeCounts[type] = 0;
            }

            foreach (var rune in equipped)
            {
                typeCounts[rune.Type]++;
            }

            RuneSet bestSet = null;
            int bestLevel = 0;

            foreach (var set in RuneDatabase.RuneSets.Values)
            {
                int matchCount = 0;
                for (int i = 0; i < 8; i++)
                {
                    var type = (RuneType)i;
                    matchCount += Math.Min(typeCounts[type], set.RuneTypeCounts[i]);
                }

                int setLevel = 0;
                if (matchCount >= 6) setLevel = 3;
                else if (matchCount >= 4) setLevel = 2;
                else if (matchCount >= 2) setLevel = 1;

                if (setLevel > bestLevel)
                {
                    bestLevel = setLevel;
                    bestSet = set;
                }
            }

            if (bestSet != null && bestLevel > 0)
            {
                return (bestSet.Name, bestLevel, bestSet.BonusAttributes[bestLevel - 1]);
            }

            return ("无", 0, 0f);
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "TotalRunesFound", _playerData.TotalRunesFound },
                { "TotalRuneUpgrades", _playerData.TotalRuneUpgrades },
                { "OwnedRunesCount", _playerData.OwnedRunes.Count },
                { "EquippedRunesCount", _playerData.OwnedRunes.Count(r => r.SlotIndex >= 0) },
                { "DiscoveredRunesCount", _playerData.DiscoveredRunes.Count },
                { "TotalRuneTypes", RuneDatabase.Runes.Count }
            };
        }

        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public PlayerRuneData GetPlayerData()
        {
            return _playerData;
        }

        /// <summary>
        /// 加载玩家数据
        /// </summary>
        public void LoadData(PlayerRuneData data)
        {
            _playerData = data ?? new PlayerRuneData();
        }

        /// <summary>
        /// 保存玩家数据
        /// </summary>
        public PlayerRuneData SaveData()
        {
            return _playerData;
        }
    }
}
