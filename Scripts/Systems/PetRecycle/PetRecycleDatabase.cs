using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetRecycle {
    /// <summary>
    /// 宠物回收配置数据库
    /// </summary>
    public class PetRecycleDatabase : BaseSystem
    {
        // 宠物类型配置
        public Dictionary<string, PetTypeConfig> PetTypes { get; private set; } = new Dictionary<string, PetTypeConfig>();
        
        // 稀有度配置
        public Dictionary<string, RarityConfig> Rarities { get; private set; } = new Dictionary<string, RarityConfig>();
        
        // 材料配置
        public Dictionary<string, MaterialConfig> Materials { get; private set; } = new Dictionary<string, MaterialConfig>();
        
        // 等级加成配置
        public Dictionary<int, float> LevelBonus { get; private set; } = new Dictionary<int, float>();
        
        public override void _Ready()
        {
            base._Ready();
            InitializePetTypes();
            InitializeRarities();
            InitializeMaterials();
            InitializeLevelBonus();
            GD.Print("[PetRecycleDatabase] Database initialized");
        }
        
        private void InitializePetTypes()
        {
            // 8种宠物类型配置
            PetTypes["Dog"] = new PetTypeConfig
            {
                Id = "Dog",
                DisplayName = "Dog",
                BaseMaterials = new List<string> { "pet_essence", "bone_fragment", "fur_sample" },
                BonusMaterials = new List<string> { "loyalty_token" }
            };
            
            PetTypes["Cat"] = new PetTypeConfig
            {
                Id = "Cat",
                DisplayName = "Cat",
                BaseMaterials = new List<string> { "pet_essence", "whisker", "fur_sample" },
                BonusMaterials = new List<string> { "agility_crystal" }
            };
            
            PetTypes["Bird"] = new PetTypeConfig
            {
                Id = "Bird",
                DisplayName = "Bird",
                BaseMaterials = new List<string> { "pet_essence", "feather", "hollow_bone" },
                BonusMaterials = new List<string> { "wind_essence" }
            };
            
            PetTypes["Rabbit"] = new PetTypeConfig
            {
                Id = "Rabbit",
                DisplayName = "Rabbit",
                BaseMaterials = new List<string> { "pet_essence", "fluffy_tail", "carrot_essence" },
                BonusMaterials = new List<string> { "luck_charm" }
            };
            
            PetTypes["Dragon"] = new PetTypeConfig
            {
                Id = "Dragon",
                DisplayName = "Dragon",
                BaseMaterials = new List<string> { "dragon_scale", "fire_essence", "ancient_ore" },
                BonusMaterials = new List<string> { "dragon_heart", "flame_orb" }
            };
            
            PetTypes["Slime"] = new PetTypeConfig
            {
                Id = "Slime",
                DisplayName = "Slime",
                BaseMaterials = new List<string> { "slime_gel", "jelly_core", "gelatinous_orb" },
                BonusMaterials = new List<string> { "sticky_resin" }
            };
            
            PetTypes["Skeleton"] = new PetTypeConfig
            {
                Id = "Skeleton",
                DisplayName = "Skeleton",
                BaseMaterials = new List<string> { "bone_dust", "skull_shard", "dark_essence" },
                BonusMaterials = new List<string> { "soul_fragment" }
            };
            
            PetTypes["Elemental"] = new PetTypeConfig
            {
                Id = "Elemental",
                DisplayName = "Elemental",
                BaseMaterials = new List<string> { "elemental_core", "pure_essence", "magic_crystal" },
                BonusMaterials = new List<string> { "prismatic_shard" }
            };
            
            GD.Print($"[PetRecycleDatabase] Initialized {PetTypes.Count} pet types");
        }
        
        private void InitializeRarities()
        {
            Rarities["Common"] = new RarityConfig
            {
                Id = "Common",
                DisplayName = "Common",
                ColorHex = "#FFFFFF",
                MaterialMultiplier = 1.0f,
                ExperienceMultiplier = 1.0f,
                SpecialDropChance = 0.0f
            };
            
            Rarities["Uncommon"] = new RarityConfig
            {
                Id = "Uncommon",
                DisplayName = "Uncommon",
                ColorHex = "#00FF00",
                MaterialMultiplier = 1.5f,
                ExperienceMultiplier = 1.5f,
                SpecialDropChance = 0.05f
            };
            
            Rarities["Rare"] = new RarityConfig
            {
                Id = "Rare",
                DisplayName = "Rare",
                ColorHex = "#0080FF",
                MaterialMultiplier = 2.0f,
                ExperienceMultiplier = 2.0f,
                SpecialDropChance = 0.10f
            };
            
            Rarities["Epic"] = new RarityConfig
            {
                Id = "Epic",
                DisplayName = "Epic",
                ColorHex = "#8000FF",
                MaterialMultiplier = 3.0f,
                ExperienceMultiplier = 3.0f,
                SpecialDropChance = 0.15f
            };
            
            Rarities["Legendary"] = new RarityConfig
            {
                Id = "Legendary",
                DisplayName = "Legendary",
                ColorHex = "#FF8000",
                MaterialMultiplier = 5.0f,
                ExperienceMultiplier = 5.0f,
                SpecialDropChance = 0.25f
            };
            
            GD.Print($"[PetRecycleDatabase] Initialized {Rarities.Count} rarity types");
        }
        
        private void InitializeMaterials()
        {
            // 基础材料
            Materials["pet_essence"] = new MaterialConfig
            {
                Id = "pet_essence",
                DisplayName = "Pet Essence",
                Description = "Basic essence extracted from pets",
                Category = "Basic",
                BaseValue = 10,
                Rarity = "Common"
            };
            
            Materials["bone_fragment"] = new MaterialConfig
            {
                Id = "bone_fragment",
                DisplayName = "Bone Fragment",
                Description = "Fragments of bone from skeletal pets",
                Category = "Material",
                BaseValue = 15,
                Rarity = "Common"
            };
            
            Materials["fur_sample"] = new MaterialConfig
            {
                Id = "fur_sample",
                DisplayName = "Fur Sample",
                Description = "Fur sample from furry pets",
                Category = "Material",
                BaseValue = 12,
                Rarity = "Common"
            };
            
            Materials["feather"] = new MaterialConfig
            {
                Id = "feather",
                DisplayName = "Feather",
                Description = "Beautiful feather from avian pets",
                Category = "Material",
                BaseValue = 18,
                Rarity = "Common"
            };
            
            Materials["dragon_scale"] = new MaterialConfig
            {
                Id = "dragon_scale",
                DisplayName = "Dragon Scale",
                Description = "Rare scale from dragon pets",
                Category = "Rare",
                BaseValue = 100,
                Rarity = "Rare"
            };
            
            Materials["fire_essence"] = new MaterialConfig
            {
                Id = "fire_essence",
                DisplayName = "Fire Essence",
                Description = "Concentrated fire energy",
                Category = "Elemental",
                BaseValue = 80,
                Rarity = "Rare"
            };
            
            Materials["slime_gel"] = new MaterialConfig
            {
                Id = "slime_gel",
                DisplayName = "Slime Gel",
                Description = "Gelatinous substance from slime pets",
                Category = "Material",
                BaseValue = 25,
                Rarity = "Common"
            };
            
            Materials["elemental_core"] = new MaterialConfig
            {
                Id = "elemental_core",
                DisplayName = "Elemental Core",
                Description = "Core essence of elemental pets",
                Category = "Elemental",
                BaseValue = 150,
                Rarity = "Epic"
            };
            
            // 特殊材料
            Materials["loyalty_token"] = new MaterialConfig
            {
                Id = "loyalty_token",
                DisplayName = "Loyalty Token",
                Description = "Symbol of a dog's unwavering loyalty",
                Category = "Special",
                BaseValue = 50,
                Rarity = "Uncommon"
            };
            
            Materials["agility_crystal"] = new MaterialConfig
            {
                Id = "agility_crystal",
                DisplayName = "Agility Crystal",
                Description = "Crystal containing feline agility",
                Category = "Special",
                BaseValue = 50,
                Rarity = "Uncommon"
            };
            
            Materials["wind_essence"] = new MaterialConfig
            {
                Id = "wind_essence",
                DisplayName = "Wind Essence",
                Description = "Essence of the wind from bird pets",
                Category = "Elemental",
                BaseValue = 60,
                Rarity = "Uncommon"
            };
            
            Materials["luck_charm"] = new MaterialConfig
            {
                Id = "luck_charm",
                DisplayName = "Luck Charm",
                Description = "Charm bringing fortune from rabbit pets",
                Category = "Special",
                BaseValue = 75,
                Rarity = "Uncommon"
            };
            
            Materials["dragon_heart"] = new MaterialConfig
            {
                Id = "dragon_heart",
                DisplayName = "Dragon Heart",
                Description = "Heart of a dragon, pulsating with power",
                Category = "Legendary",
                BaseValue = 500,
                Rarity = "Legendary"
            };
            
            Materials["flame_orb"] = new MaterialConfig
            {
                Id = "flame_orb",
                DisplayName = "Flame Orb",
                Description = "Orb containing primordial flame",
                Category = "Legendary",
                BaseValue = 400,
                Rarity = "Epic"
            };
            
            Materials["soul_fragment"] = new MaterialConfig
            {
                Id = "soul_fragment",
                DisplayName = "Soul Fragment",
                Description = "Fragment of a soul from undead pets",
                Category = "Special",
                BaseValue = 200,
                Rarity = "Epic"
            };
            
            Materials["prismatic_shard"] = new MaterialConfig
            {
                Id = "prismatic_shard",
                DisplayName = "Prismatic Shard",
                Description = "Shard containing rainbow light",
                Category = "Legendary",
                BaseValue = 600,
                Rarity = "Legendary"
            };
            
            GD.Print($"[PetRecycleDatabase] Initialized {Materials.Count} materials");
        }
        
        private void InitializeLevelBonus()
        {
            // 每级增加5%材料产出
            for (int level = 1; level <= 100; level++)
            {
                LevelBonus[level] = 1.0f + (level - 1) * 0.05f;
            }
            GD.Print($"[PetRecycleDatabase] Initialized level bonus for {LevelBonus.Count} levels");
        }
        
        /// <summary>
        /// 获取宠物类型配置
        /// </summary>
        public PetTypeConfig GetPetTypeConfig(string petType)
        {
            if (PetTypes.ContainsKey(petType))
                return PetTypes[petType];
            return PetTypes["Dog"]; // 默认
        }
        
        /// <summary>
        /// 获取稀有度配置
        /// </summary>
        public RarityConfig GetRarityConfig(string rarity)
        {
            if (Rarities.ContainsKey(rarity))
                return Rarities[rarity];
            return Rarities["Common"]; // 默认
        }
        
        /// <summary>
        /// 获取材料配置
        /// </summary>
        public MaterialConfig GetMaterialConfig(string materialId)
        {
            if (Materials.ContainsKey(materialId))
                return Materials[materialId];
            return null;
        }
        
        /// <summary>
        /// 获取等级加成
        /// </summary>
        public float GetLevelBonus(int level)
        {
            if (LevelBonus.ContainsKey(level))
                return LevelBonus[level];
            return 1.0f;
        }
    }
    
    /// <summary>
    /// 宠物类型配置
    /// </summary>
    public class PetTypeConfig
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public List<string> BaseMaterials { get; set; } = new List<string>();
        public List<string> BonusMaterials { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// 稀有度配置
    /// </summary>
    public class RarityConfig
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string ColorHex { get; set; } = "#FFFFFF";
        public float MaterialMultiplier { get; set; } = 1.0f;
        public float ExperienceMultiplier { get; set; } = 1.0f;
        public float SpecialDropChance { get; set; } = 0.0f;
    }
    
    /// <summary>
    /// 材料配置
    /// </summary>
    public class MaterialConfig
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public int BaseValue { get; set; } = 0;
        public string Rarity { get; set; } = "Common";
    }

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }
}
