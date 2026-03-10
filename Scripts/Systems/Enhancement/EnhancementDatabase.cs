using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Enhancement {
    /// <summary>
    /// 强化石数据库
    /// </summary>
    public class EnhancementStoneData {
        public string Id;
        public string Name;
        public string Description;
        public int Value;
        public float SuccessBonus;
        public EnhancementType Type;
        
        public EnhancementStoneData(string id, string name, string desc, int value, float bonus, EnhancementType type) {
            Id = id;
            Name = name;
            Description = desc;
            Value = value;
            SuccessBonus = bonus;
            Type = type;
        }
    }
    
    public class EnhancementDatabase : Node {
        public static EnhancementDatabase Instance { get; private set; }
        
        private Dictionary<string, EnhancementStoneData> _stones = new();
        
        public override void _Ready() {
            Instance = this;
            InitializeStones();
        }
        
        private void InitializeStones() {
            // 普通强化石 - ID 401
            _stones["401"] = new EnhancementStoneData(
                "401",
                "普通强化石",
                "用于装备强化的基础材料，可提高强化成功率",
                100,
                0f,
                EnhancementType.Weapon
            );
            
            // 优秀强化石 - ID 402
            _stones["402"] = new EnhancementStoneData(
                "402",
                "优秀强化石",
                "高品质强化材料，可提高5%强化成功率",
                500,
                0.05f,
                EnhancementType.Weapon
            );
            
            // 稀有强化石 - ID 403
            _stones["403"] = new EnhancementStoneData(
                "403",
                "稀有强化石",
                "稀有强化材料，可提高10%强化成功率",
                2000,
                0.10f,
                EnhancementType.Armor
            );
            
            // 史诗强化石 - ID 404
            _stones["404"] = new EnhancementStoneData(
                "404",
                "史诗强化石",
                "史诗级强化材料，可提高15%强化成功率",
                10000,
                0.15f,
                EnhancementType.Armor
            );
            
            // 传说强化石 - ID 405
            _stones["405"] = new EnhancementStoneData(
                "405",
                "传说强化石",
                "传说级强化材料，可提高25%强化成功率",
                50000,
                0.25f,
                EnhancementType.Accessory
            );
        }
        
        public EnhancementStoneData GetStoneData(string stoneId) {
            if (_stones.ContainsKey(stoneId)) {
                return _stones[stoneId];
            }
            return null;
        }
        
        public Dictionary<string, EnhancementStoneData> GetAllStones() {
            return _stones;
        }
        
        public string[] GetStoneIds() {
            string[] ids = new string[_stones.Count];
            _stones.Keys.CopyTo(ids, 0);
            return ids;
        }
    }
}
