using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Costume database - contains all available costumes
    /// </summary>
    public class CostumeDatabase
    {
        private static CostumeDatabase _instance;
        public static CostumeDatabase Instance => _instance ??= new CostumeDatabase();
        
        private Dictionary<string, CostumeData> _costumes = new();
        
        public CostumeDatabase()
        {
            InitializeCostumes();
        }
        
        private void InitializeCostumes()
        {
            // Outfit costumes (服装)
            AddCostume(new CostumeData {
                Id = "outfit_default",
                Name = "默认服装",
                Description = "冒险者标准服装",
                Category = CostumeCategory.Outfit,
                Cost = 0,
                IsDefault = true,
                IsPurchased = true,
                IconPath = "res://Icons/costume_default.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "outfit_knight",
                Name = "骑士铠甲",
                Description = "王国骑士的标准铠甲",
                Category = CostumeCategory.Outfit,
                Cost = 500,
                IsDefault = false,
                IconPath = "res://Icons/costume_knight.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "outfit_mage",
                Name = "法师长袍",
                Description = "法师议会的高级长袍",
                Category = CostumeCategory.Outfit,
                Cost = 500,
                IsDefault = false,
                IconPath = "res://Icons/costume_mage.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "outfit_rogue",
                Name = "盗贼皮甲",
                Description = "盗贼公会特制的皮甲",
                Category = CostumeCategory.Outfit,
                Cost = 450,
                IsDefault = false,
                IconPath = "res://Icons/costume_rogue.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "outfit_dragon",
                Name = "龙鳞战甲",
                Description = "用巨龙鳞片打造的传奇战甲",
                Category = CostumeCategory.Outfit,
                Cost = 2000,
                IsDefault = false,
                IconPath = "res://Icons/costume_dragon.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "outfit_phoenix",
                Name = "凤凰羽衣",
                Description = "蕴含凤凰神力的羽衣",
                Category = CostumeCategory.Outfit,
                Cost = 2500,
                IsDefault = false,
                IconPath = "res://Icons/costume_phoenix.png",
                ResourcePath = ""
            });
            
            // Hat costumes (帽子)
            AddCostume(new CostumeData {
                Id = "hat_none",
                Name = "无帽子",
                Description = "不佩戴帽子",
                Category = CostumeCategory.Hat,
                Cost = 0,
                IsDefault = true,
                IsPurchased = true,
                IconPath = "res://Icons/hat_none.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "hat_helmet",
                Name = "头盔",
                Description = "标准防护头盔",
                Category = CostumeCategory.Hat,
                Cost = 150,
                IsDefault = false,
                IconPath = "res://Icons/hat_helmet.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "hat_wizard",
                Name = "巫师帽",
                Description = "法师议会认证的巫师帽",
                Category = CostumeCategory.Hat,
                Cost = 200,
                IsDefault = false,
                IconPath = "res://Icons/hat_wizard.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "hat_crown",
                Name = "王冠",
                Description = "王者象征的王冠",
                Category = CostumeCategory.Hat,
                Cost = 1000,
                IsDefault = false,
                IconPath = "res://Icons/hat_crown.png",
                ResourcePath = ""
            });
            
            // Weapon skins (武器外观)
            AddCostume(new CostumeData {
                Id = "weapon_default",
                Name = "默认外观",
                Description = "武器默认外观",
                Category = CostumeCategory.WeaponSkin,
                Cost = 0,
                IsDefault = true,
                IsPurchased = true,
                IconPath = "res://Icons/weapon_default.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "weapon_gold",
                Name = "黄金外观",
                Description = "金色光芒的武器外观",
                Category = CostumeCategory.WeaponSkin,
                Cost = 300,
                IsDefault = false,
                IconPath = "res://Icons/weapon_gold.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "weapon_ice",
                Name = "冰霜外观",
                Description = "寒冷冰霜包裹的武器外观",
                Category = CostumeCategory.WeaponSkin,
                Cost = 400,
                IsDefault = false,
                IconPath = "res://Icons/weapon_ice.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "weapon_fire",
                Name = "烈焰外观",
                Description = "燃烧着烈火的武器外观",
                Category = CostumeCategory.WeaponSkin,
                Cost = 400,
                IsDefault = false,
                IconPath = "res://Icons/weapon_fire.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "weapon_legendary",
                Name = "传奇外观",
                Description = "传说级武器外观",
                Category = CostumeCategory.WeaponSkin,
                Cost = 1500,
                IsDefault = false,
                IconPath = "res://Icons/weapon_legendary.png",
                ResourcePath = ""
            });
            
            // Effects (特效)
            AddCostume(new CostumeData {
                Id = "effect_none",
                Name = "无特效",
                Description = "无特殊特效",
                Category = CostumeCategory.Effect,
                Cost = 0,
                IsDefault = true,
                IsPurchased = true,
                IconPath = "res://Icons/effect_none.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "effect_sparkle",
                Name = "星光特效",
                Description = "角色周围环绕星光",
                Category = CostumeCategory.Effect,
                Cost = 250,
                IsDefault = false,
                IconPath = "res://Icons/effect_sparkle.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "effect_flame",
                Name = "火焰特效",
                Description = "角色被火焰环绕",
                Category = CostumeCategory.Effect,
                Cost = 350,
                IsDefault = false,
                IconPath = "res://Icons/effect_flame.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "effect_frost",
                Name = "冰霜特效",
                Description = "角色被冰霜环绕",
                Category = CostumeCategory.Effect,
                Cost = 350,
                IsDefault = false,
                IconPath = "res://Icons/effect_frost.png",
                ResourcePath = ""
            });
            
            // Trails (拖尾效果)
            AddCostume(new CostumeData {
                Id = "trail_none",
                Name = "无拖尾",
                Description = "无拖尾效果",
                Category = CostumeCategory.Trail,
                Cost = 0,
                IsDefault = true,
                IsPurchased = true,
                IconPath = "res://Icons/trail_none.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "trail_dust",
                Name = "尘土拖尾",
                Description = "移动时留下尘土",
                Category = CostumeCategory.Trail,
                Cost = 150,
                IsDefault = false,
                IconPath = "res://Icons/trail_dust.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "trail_light",
                Name = "光影拖尾",
                Description = "移动时留下光影",
                Category = CostumeCategory.Trail,
                Cost = 300,
                IsDefault = false,
                IconPath = "res://Icons/trail_light.png",
                ResourcePath = ""
            });
            
            AddCostume(new CostumeData {
                Id = "trail_rainbow",
                Name = "彩虹拖尾",
                Description = "移动时留下彩虹光芒",
                Category = CostumeCategory.Trail,
                Cost = 800,
                IsDefault = false,
                IconPath = "res://Icons/trail_rainbow.png",
                ResourcePath = ""
            });
        }
        
        private void AddCostume(CostumeData costume)
        {
            _costumes[costume.Id] = costume;
        }
        
        public CostumeData GetCostume(string id)
        {
            return _costumes.ContainsKey(id) ? _costumes[id] : null;
        }
        
        public List<CostumeData> GetCostumesByCategory(CostumeCategory category)
        {
            List<CostumeData> result = new();
            foreach (var costume in _costumes.Values)
            {
                if (costume.Category == category)
                    result.Add(costume);
            }
            return result;
        }
        
        public List<CostumeData> GetAllCostumes()
        {
            return new List<CostumeData>(_costumes.Values);
        }
        
        public List<CostumeData> GetPurchasableCostumes()
        {
            List<CostumeData> result = new();
            foreach (var costume in _costumes.Values)
            {
                if (!costume.IsPurchased)
                    result.Add(costume);
            }
            return result;
        }
    }
}
