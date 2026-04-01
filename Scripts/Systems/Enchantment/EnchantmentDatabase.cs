using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems.Enchantment
{
    /// <summary>
    /// 附魔配置数据库
    /// </summary>
    public partial class EnchantmentDatabase : DatabaseBase
    {
        private static EnchantmentDatabase _instance;
        public static EnchantmentDatabase Instance => _instance ??= new EnchantmentDatabase();
        public override object Instance => _instance ??= new EnchantmentDatabase();

        // 附魔记录缓存
        private Dictionary<string, EnchantmentRecord> _enchantments;
        private Dictionary<EnchantmentTier, List<string>> _tierCache;
        private Dictionary<EnchantmentType, List<string>> _typeCache;

        /// <summary>
        /// 初始化附魔数据库
        /// </summary>
        public override void Initialize()
        {
            _enchantments = new Dictionary<string, EnchantmentRecord>();
            _tierCache = new Dictionary<EnchantmentTier, List<string>>();
            _typeCache = new Dictionary<EnchantmentType, List<string>>();

            InitializeTierCache();
            InitializeTypeCache();
            LoadDefaultEnchantments();
        }

        public bool ValidateData()
        {
            return _enchantments != null && _enchantments.Count > 0;
        }

        private void InitializeTierCache()
        {
            _tierCache[EnchantmentTier.Common] = new List<string>();
            _tierCache[EnchantmentTier.Uncommon] = new List<string>();
            _tierCache[EnchantmentTier.Rare] = new List<string>();
            _tierCache[EnchantmentTier.Epic] = new List<string>();
            _tierCache[EnchantmentTier.Legendary] = new List<string>();
        }

        private void InitializeTypeCache()
        {
            _typeCache[EnchantmentType.Weapon] = new List<string>();
            _typeCache[EnchantmentType.Armor] = new List<string>();
            _typeCache[EnchantmentType.Accessory] = new List<string>();
            _typeCache[EnchantmentType.Universal] = new List<string>();
        }

        /// <summary>
        /// 添加附魔配置
        /// </summary>
        private void AddEnchantment(EnchantmentRecord record)
        {
            _enchantments[record.Id] = record;
            _tierCache[record.Tier].Add(record.Id);
            _typeCache[record.Type].Add(record.Id);
        }

        /// <summary>
        /// 获取所有附魔
        /// </summary>
        public List<EnchantmentRecord> GetAllEnchantments()
        {
            return new List<EnchantmentRecord>(_enchantments.Values);
        }

        /// <summary>
        /// 根据ID获取附魔
        /// </summary>
        public EnchantmentRecord GetEnchantmentById(string id)
        {
            if (_enchantments.ContainsKey(id))
                return _enchantments[id];
            return null;
        }

        /// <summary>
        /// 根据类型获取附魔
        /// </summary>
        public List<EnchantmentRecord> GetEnchantmentsByType(EnchantmentType type)
        {
            List<EnchantmentRecord> result = new List<EnchantmentRecord>();
            if (_typeCache.ContainsKey(type))
            {
                foreach (var id in _typeCache[type])
                {
                    result.Add(_enchantments[id]);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据等级获取附魔
        /// </summary>
        public List<EnchantmentRecord> GetEnchantmentsByTier(EnchantmentTier tier)
        {
            List<EnchantmentRecord> result = new List<EnchantmentRecord>();
            if (_tierCache.ContainsKey(tier))
            {
                foreach (var id in _tierCache[tier])
                {
                    result.Add(_enchantments[id]);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据玩家等级获取可用附魔
        /// </summary>
        public List<EnchantmentRecord> GetAvailableEnchantments(int playerLevel)
        {
            List<EnchantmentRecord> result = new List<EnchantmentRecord>();
            foreach (var enchantment in _enchantments.Values)
            {
                if (enchantment.RequiredLevel <= playerLevel)
                {
                    result.Add(enchantment);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取附魔总数
        /// </summary>
        public int GetTotalCount()
        {
            return _enchantments.Count;
        }

        protected override void OnExportSaveData(Godot.Collections.Dictionary saveData)
        {
            // EnchantmentDatabase 是静态配置数据库，无玩家状态需持久化
        }

        protected override void OnImportSaveData(Godot.Collections.Dictionary saveData)
        {
            // EnchantmentDatabase 是静态配置数据库，无玩家状态需恢复
        }
    }
}
