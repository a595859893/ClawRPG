using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
{
    /// <summary>
    /// 炼金材料库存管理系统
    /// 负责材料的存储、查询和管理
    /// </summary>
    public partial class AlchemyInventorySystem : BaseSystem
    {
        private Dictionary<int, AlchemyMaterial> _materials = new Dictionary<int, AlchemyMaterial>();
        private int _nextMaterialId = 1001;
        
        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }
        
        protected override string SystemName => "AlchemyInventorySystem";
        
        /// <summary>
        /// 初始化材料数据
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            InitializeMaterials();
        }
        
        /// <summary>
        /// 初始化材料数据
        /// </summary>
        public void InitializeMaterials()
        {
            // 草药类 (1001-1015)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1001,
                Name = "红叶草",
                Description = "常见的红色草药",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 5
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1002,
                Name = "蓝莲花",
                Description = "具有魔法能量的莲花",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 15
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1003,
                Name = "银叶草",
                Description = "月光照耀的银色草药",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 30
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1004,
                Name = "火焰花",
                Description = "生长在火山附近的炽热花朵",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 45
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1005,
                Name = "冰晶草",
                Description = "极寒之地生长的晶体草",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 45
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1006,
                Name = "月光草",
                Description = "只在满月时绽放的奇迹草药",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 100
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1007,
                Name = "龙心草",
                Description = "传说中巨龙栖息地的神圣草药",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Legendary,
                Value = 500
            });

            // 矿物类 (1011-1018)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1011,
                Name = "铜矿石",
                Description = "常见的金属矿石",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 3
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1012,
                Name = "银矿石",
                Description = "稀有的银色金属",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 20
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1013,
                Name = "金矿石",
                Description = "珍贵的金色金属",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 50
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1014,
                Name = "秘银",
                Description = "传说中的魔法金属",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 200
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1015,
                Name = "龙晶",
                Description = "蕴含巨龙力量的晶体",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Legendary,
                Value = 800
            });

            // 怪物素材类 (1021-1030)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1021,
                Name = "史莱姆凝胶",
                Description = "史莱姆的凝胶状物质",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 8
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1022,
                Name = "哥布林耳朵",
                Description = "哥布林的战利品",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 10
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1023,
                Name = "狼牙",
                Description = "野狼的锋利獠牙",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 25
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1024,
                Name = "骷髅碎片",
                Description = "亡灵生物的遗骸",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 20
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1025,
                Name = "龙鳞",
                Description = "巨龙的鳞片",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 300
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1026,
                Name = "凤凰羽毛",
                Description = "浴火凤凰的羽毛",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Legendary,
                Value = 1000
            });

            // 水晶类 (1031-1040)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1031,
                Name = "红水晶",
                Description = "蕴含火焰能量的水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 30
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1032,
                Name = "蓝水晶",
                Description = "蕴含冰霜能量的水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 30
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1033,
                Name = "绿水晶",
                Description = "蕴含自然能量的水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 30
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1034,
                Name = "紫水晶",
                Description = "蕴含暗影能量的水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 60
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1035,
                Name = "彩虹水晶",
                Description = "蕴含多元能量的稀有水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 250
            });
        }
        
        /// <summary>
        /// 添加材料
        /// </summary>
        public void AddMaterial(AlchemyMaterial material)
        {
            if (material.Id == 0)
            {
                material.Id = _nextMaterialId++;
            }
            _materials[material.Id] = material;
        }
        
        /// <summary>
        /// 获取材料
        /// </summary>
        public AlchemyMaterial GetMaterial(int id)
        {
            return _materials.ContainsKey(id) ? _materials[id] : null;
        }
        
        /// <summary>
        /// 获取所有材料
        /// </summary>
        public List<AlchemyMaterial> GetAllMaterials()
        {
            return new List<AlchemyMaterial>(_materials.Values);
        }
        
        /// <summary>
        /// 根据类型获取材料
        /// </summary>
        public List<AlchemyMaterial> GetMaterialsByType(AlchemyMaterialType type)
        {
            List<AlchemyMaterial> result = new List<AlchemyMaterial>();
            foreach (var material in _materials.Values)
            {
                if (material.Type == type)
                    result.Add(material);
            }
            return result;
        }
        
        /// <summary>
        /// 根据稀有度获取材料
        /// </summary>
        public List<AlchemyMaterial> GetMaterialsByRarity(AlchemyMaterialRarity rarity)
        {
            List<AlchemyMaterial> result = new List<AlchemyMaterial>();
            foreach (var material in _materials.Values)
            {
                if (material.Rarity == rarity)
                    result.Add(material);
            }
            return result;
        }
        
        /// <summary>
        /// 根据稀有度获取随机材料
        /// </summary>
        public AlchemyMaterial GetRandomMaterialByRarity(AlchemyMaterialRarity rarity)
        {
            var materials = GetMaterialsByRarity(rarity);
            if (materials.Count == 0) return null;
            
            var random = new Random();
            return materials[random.Next(materials.Count)];
        }
        
        /// <summary>
        /// 获取随机材料
        /// </summary>
        public AlchemyMaterial GetRandomMaterial()
        {
            var random = new Random();
            var materialList = new List<AlchemyMaterial>(_materials.Values);
            return materialList[random.Next(materialList.Count)];
        }
        
        /// <summary>
        /// 重置系统数据
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            _materials.Clear();
            _nextMaterialId = 1001;
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary
            {
                ["nextMaterialId"] = _nextMaterialId,
                ["materials"] = new Dictionary<string, object>()
            };
            
            var materialsData = (Dictionary)data["materials"];
            foreach (var kvp in _materials)
            {
                materialsData[kvp.Key] = kvp.Value;
            }
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("nextMaterialId"))
            {
                _nextMaterialId = (int)data["nextMaterialId"];
            }
            
            if (data.ContainsKey("materials"))
            {
                var materialsData = (Dictionary)data["materials"];
                _materials.Clear();
                foreach (var key in materialsData.Keys)
                {
                    var material = (AlchemyMaterial)materialsData[key];
                    _materials[(int)key] = material;
                }
            }
        }
    }
}
