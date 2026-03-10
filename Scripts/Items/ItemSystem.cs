using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Items {
    /// <summary>
    /// Item quality levels
    /// </summary>
    public enum ItemQuality
    {
        Common = 0,      // Gray
        Uncommon = 1,    // Green
        Rare = 2,        // Blue
        Epic = 3,        // Purple
        Legendary = 4    // Orange
    }

    /// <summary>
    /// Base Item class
    /// </summary>
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public ItemType Type { get; set; }
        public ItemQuality Quality { get; set; } = ItemQuality.Common;
        public int Value { get; set; }
        public int MaxStack { get; set; } = 99;
        public string IconPath { get; set; }
        
        public enum ItemType { 
            Weapon, Armor, Accessory, 
            Consumable, Material, QuestItem 
        }
    }
    
    /// <summary>
    /// Weapon item with combat stats
    /// </summary>
    public class Weapon : Item
    {
        public float Damage { get; set; }
        public float AttackSpeed { get; set; }
        public float CriticalChance { get; set; }
        public WeaponType WeaponType { get; set; }
        
        public enum WeaponType { Sword, Axe, Bow, Staff, Dagger }
        
        public Weapon()
        {
            Type = ItemType.Weapon;
        }
    }
    
    /// <summary>
    /// Armor item with defense stats
    /// </summary>
    public class Armor : Item
    {
        public float Defense { get; set; }
        public float HealthBonus { get; set; }
        public ArmorType ArmorType { get; set; }
        
        public enum ArmorType { Helmet, Chest, Legs, Shield }
        
        public Armor()
        {
            Type = ItemType.Armor;
        }
    }
    
    /// <summary>
    /// Consumable item (potions, food)
    /// </summary>
    public class Consumable : Item
    {
        public int HealthRestore { get; set; }
        public int ManaRestore { get; set; }
        public int StaminaRestore { get; set; }
        public float Duration { get; set; } // For buffs
        
        public Consumable()
        {
            Type = ItemType.Consumable;
            MaxStack = 10;
        }
    }
    
    /// <summary>
    /// Item database - contains all game items
    /// </summary>
    public class ItemDatabase
    {
        private static ItemDatabase _instance;
        public static ItemDatabase Instance => _instance ??= new ItemDatabase();
        
        private Dictionary<int, Item> _items = new();
        
        public ItemDatabase()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // Weapons
            AddItem(new Weapon { Id = 1, Name = "木剑", Description = "基础的木质剑", Value = 10, Damage = 5, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Weapon { Id = 2, Name = "铁剑", Description = "精炼铁制剑", Value = 50, Damage = 12, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Weapon { Id = 3, Name = "钢剑", Description = "优质钢材剑", Value = 150, Damage = 20, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Weapon { Id = 4, Name = "银剑", Description = "附魔银剑", Value = 500, Damage = 30, CriticalChance = 0.1f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Weapon { Id = 5, Name = "火焰之剑", Description = "火焰附魔剑", Value = 1000, Damage = 40, CriticalChance = 0.15f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Weapon { Id = 6, Name = "冰霜之剑", Description = "冰霜附魔剑", Value = 1000, Damage = 40, CriticalChance = 0.15f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Weapon { Id = 7, Name = "雷神之锤", Description = "雷电之力", Value = 1500, Damage = 50, AttackSpeed = 0.8f, WeaponType = Weapon.WeaponType.Axe });
            AddItem(new Weapon { Id = 8, Name = "传奇之刃", Description = "传说中的武器", Value = 5000, Damage = 80, CriticalChance = 0.25f, WeaponType = Weapon.WeaponType.Sword });
            
            // Extended weapons
            AddItem(new Weapon { Id = 9, Name = "暗影之刃", Description = "暗影附魔剑", Value = 2000, Damage = 55, CriticalChance = 0.2f, WeaponType = Weapon.WeaponType.Dagger });
            AddItem(new Weapon { Id = 10, Name = "神圣之剑", Description = "神圣附魔剑", Value = 2500, Damage = 60, CriticalChance = 0.18f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Weapon { Id = 11, Name = "龙息之剑", Description = "龙火淬炼的剑", Value = 3500, Damage = 70, CriticalChance = 0.22f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Weapon { Id = 12, Name = "精灵长弓", Description = "精灵族的长弓", Value = 1800, Damage = 45, AttackSpeed = 1.2f, WeaponType = Weapon.WeaponType.Bow });
            AddItem(new Weapon { Id = 13, Name = "猎龙弓", Description = "专门猎龙的强弓", Value = 4000, Damage = 65, AttackSpeed = 1.0f, WeaponType = Weapon.WeaponType.Bow });
            AddItem(new Weapon { Id = 14, Name = "奥术法杖", Description = "蕴含奥术能量", Value = 2200, Damage = 50, WeaponType = Weapon.WeaponType.Staff });
            AddItem(new Weapon { Id = 15, Name = "元素法杖", Description = "元素魔法杖", Value = 3500, Damage = 70, WeaponType = Weapon.WeaponType.Staff });
            AddItem(new Weapon { Id = 16, Name = "月影之刃", Description = "月光淬炼的刺客武器", Value = 4500, Damage = 75, CriticalChance = 0.3f, WeaponType = Weapon.WeaponType.Dagger });
            AddItem(new Weapon { Id = 17, Name = "泰坦战斧", Description = "泰坦之力", Value = 6000, Damage = 90, AttackSpeed = 0.7f, WeaponType = Weapon.WeaponType.Axe });
            
            // Armor
            AddItem(new Armor { Id = 101, Name = "布袍", Description = "基础法师袍", Value = 20, Defense = 2, HealthBonus = 10 });
            AddItem(new Armor { Id = 102, Name = "皮甲", Description = "基础皮甲", Value = 30, Defense = 5 });
            AddItem(new Armor { Id = 103, Name = "锁甲", Description = "锁子甲", Value = 100, Defense = 10, HealthBonus = 20 });
            AddItem(new Armor { Id = 104, Name = "铁甲", Description = "重装铁甲", Value = 300, Defense = 20, HealthBonus = 50 });
            AddItem(new Armor { Id = 105, Name = "龙鳞甲", Description = "龙鳞制成的护甲", Value = 1000, Defense = 35, HealthBonus = 100 });
            AddItem(new Armor { Id = 106, Name = "金甲", Description = "华丽金甲", Value = 2000, Defense = 45, HealthBonus = 150 });
            AddItem(new Armor { Id = 107, Name = "神话战甲", Description = "神话级护甲", Value = 5000, Defense = 60, HealthBonus = 250 });
            
            // Extended armor
            AddItem(new Armor { Id = 108, Name = "暗影皮甲", Description = "暗影刺客皮甲", Value = 1500, Defense = 25, HealthBonus = 80 });
            AddItem(new Armor { Id = 109, Name = "火焰锁甲", Description = "火焰附魔锁甲", Value = 1800, Defense = 28, HealthBonus = 90, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 110, Name = "冰霜铁甲", Description = "冰霜附魔铁甲", Value = 2000, Defense = 30, HealthBonus = 100 });
            AddItem(new Armor { Id = 111, Name = "神圣长袍", Description = "神圣法师长袍", Value = 2500, Defense = 15, HealthBonus = 120 });
            AddItem(new Armor { Id = 112, Name = "龙鳞战甲", Description = "强化龙鳞甲", Value = 3500, Defense = 50, HealthBonus = 180 });
            AddItem(new Armor { Id = 113, Name = "精灵皮甲", Description = "精灵族皮甲", Value = 2200, Defense = 32, HealthBonus = 110 });
            AddItem(new Armor { Id = 114, Name = "泰坦重甲", Description = "泰坦之力重甲", Value = 6000, Defense = 70, HealthBonus = 300 });
            AddItem(new Armor { Id = 115, Name = "月影斗篷", Description = "月光守护斗篷", Value = 4000, Defense = 40, HealthBonus = 200 });
            AddItem(new Armor { Id = 116, Name = "神圣战甲", Description = "神圣之力战甲", Value = 5500, Defense = 55, HealthBonus = 250 });
            AddItem(new Armor { Id = 117, Name = "元素法袍", Description = "元素魔法长袍", Value = 4500, Defense = 22, HealthBonus = 180 });
            
            // === Equipment Sets ===
            // Set 1: Warrior's Might (战士之力) - ID 1001-1005
            AddItem(new Weapon { Id = 1001, Name = "战士之剑", Description = "战士之力套装 - 武器", Value = 3000, Damage = 45, CriticalChance = 0.15f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Armor { Id = 1002, Name = "战士头盔", Description = "战士之力套装 - 头盔", Value = 2000, Defense = 25, HealthBonus = 100, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1003, Name = "战士胸甲", Description = "战士之力套装 - 胸甲", Value = 2500, Defense = 35, HealthBonus = 150, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1004, Name = "战士护腿", Description = "战士之力套装 - 护腿", Value = 1800, Defense = 20, HealthBonus = 80, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1005, Name = "战士盾牌", Description = "战士之力套装 - 盾牌", Value = 2200, Defense = 30, HealthBonus = 100, ArmorType = Armor.ArmorType.Shield });
            
            // Set 2: Mage's Robes (法师长袍) - ID 1011-1015
            AddItem(new Weapon { Id = 1011, Name = "奥术法杖", Description = "法师长袍套装 - 法杖", Value = 3200, Damage = 55, WeaponType = Weapon.WeaponType.Staff });
            AddItem(new Armor { Id = 1012, Name = "法师帽子", Description = "法师长袍套装 - 帽子", Value = 2100, Defense = 15, ManaBonus = 100, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1013, Name = "法师长袍", Description = "法师长袍套装 - 长袍", Value = 2600, Defense = 18, ManaBonus = 150, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1014, Name = "法师护腿", Description = "法师长袍套装 - 护腿", Value = 1900, Defense = 12, ManaBonus = 80, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1015, Name = "法师手套", Description = "法师长袍套装 - 手套", Value = 1600, Defense = 10, ManaBonus = 60, ArmorType = Armor.ArmorType.Helmet });
            
            // Set 3: Assassin's Shadow (刺客阴影) - ID 1021-1025
            AddItem(new Weapon { Id = 1021, Name = "暗影匕首", Description = "刺客阴影套装 - 匕首", Value = 3500, Damage = 50, CriticalChance = 0.25f, WeaponType = Weapon.WeaponType.Dagger });
            AddItem(new Armor { Id = 1022, Name = "刺客头巾", Description = "刺客阴影套装 - 头巾", Value = 2000, Defense = 12, HealthBonus = 50, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1023, Name = "刺客皮甲", Description = "刺客阴影套装 - 皮甲", Value = 2400, Defense = 18, HealthBonus = 80, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1024, Name = "刺客护腿", Description = "刺客阴影套装 - 护腿", Value = 1700, Defense = 10, HealthBonus = 50, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1025, Name = "刺客靴子", Description = "刺客阴影套装 - 靴子", Value = 1500, Defense = 8, HealthBonus = 40, ArmorType = Armor.ArmorType.Legs });
            
            // Set 4: Dragon Scale Armor (龙鳞护甲) - ID 1031-1035
            AddItem(new Weapon { Id = 1031, Name = "龙鳞剑", Description = "龙鳞护甲套装 - 剑", Value = 5000, Damage = 65, CriticalChance = 0.2f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Armor { Id = 1032, Name = "龙鳞头盔", Description = "龙鳞护甲套装 - 头盔", Value = 3500, Defense = 40, HealthBonus = 200, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1033, Name = "龙鳞胸甲", Description = "龙鳞护甲套装 - 胸甲", Value = 4500, Defense = 55, HealthBonus = 300, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1034, Name = "龙鳞护腿", Description = "龙鳞护甲套装 - 护腿", Value = 3000, Defense = 35, HealthBonus = 180, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1035, Name = "龙鳞盾牌", Description = "龙鳞护甲套装 - 盾牌", Value = 3800, Defense = 45, HealthBonus = 220, ArmorType = Armor.ArmorType.Shield });
            
            // Set 5: Holy Light (神圣之光) - ID 1041-1045
            AddItem(new Weapon { Id = 1041, Name = "圣光法杖", Description = "神圣之光套装 - 法杖", Value = 4500, Damage = 60, WeaponType = Weapon.WeaponType.Staff });
            AddItem(new Armor { Id = 1042, Name = "圣光头盔", Description = "神圣之光套装 - 头盔", Value = 3200, Defense = 25, HealthBonus = 180, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1043, Name = "圣光胸甲", Description = "神圣之光套装 - 胸甲", Value = 4000, Defense = 35, HealthBonus = 250, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1044, Name = "圣光护腿", Description = "神圣之光套装 - 护腿", Value = 2800, Defense = 22, HealthBonus = 150, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1045, Name = "圣光护符", Description = "神圣之光套装 - 饰品", Value = 3500, Defense = 15, HealthBonus = 120, ArmorType = Armor.ArmorType.Helmet });
            
            // Set 6: Elemental Master (元素大师) - ID 1051-1055
            AddItem(new Weapon { Id = 1051, Name = "元素法杖", Description = "元素大师套装 - 法杖", Value = 4800, Damage = 70, WeaponType = Weapon.WeaponType.Staff });
            AddItem(new Armor { Id = 1052, Name = "元素头盔", Description = "元素大师套装 - 头盔", Value = 3400, Defense = 22, ManaBonus = 120, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1053, Name = "元素长袍", Description = "元素大师套装 - 长袍", Value = 4200, Defense = 28, ManaBonus = 180, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1054, Name = "元素护腿", Description = "元素大师套装 - 护腿", Value = 3000, Defense = 18, ManaBonus = 100, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1055, Name = "元素戒指", Description = "元素大师套装 - 饰品", Value = 3600, Defense = 12, ManaBonus = 150, ArmorType = Armor.ArmorType.Helmet });
            
            // Set 7: Shadow Lord (暗影王者) - ID 1061-1065
            AddItem(new Weapon { Id = 1061, Name = "暗影之刃", Description = "暗影王者套装 - 武器", Value = 5200, Damage = 75, CriticalChance = 0.25f, WeaponType = Weapon.WeaponType.Dagger });
            AddItem(new Armor { Id = 1062, Name = "暗影头盔", Description = "暗影王者套装 - 头盔", Value = 3600, Defense = 28, HealthBonus = 150, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1063, Name = "暗影胸甲", Description = "暗影王者套装 - 胸甲", Value = 4400, Defense = 38, HealthBonus = 200, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1064, Name = "暗影护腿", Description = "暗影王者套装 - 护腿", Value = 3200, Defense = 25, HealthBonus = 120, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1065, Name = "暗影披风", Description = "暗影王者套装 - 披风", Value = 3800, Defense = 20, HealthBonus = 180, ArmorType = Armor.ArmorType.Chest });
            
            // Set 8: Fire Lord (火焰领主) - ID 1071-1075
            AddItem(new Weapon { Id = 1071, Name = "火焰剑", Description = "火焰领主套装 - 剑", Value = 5500, Damage = 80, CriticalChance = 0.22f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Armor { Id = 1072, Name = "火焰头盔", Description = "火焰领主套装 - 头盔", Value = 3800, Defense = 30, HealthBonus = 160, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1073, Name = "火焰胸甲", Description = "火焰领主套装 - 胸甲", Value = 4600, Defense = 42, HealthBonus = 220, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1074, Name = "火焰护腿", Description = "火焰领主套装 - 护腿", Value = 3400, Defense = 28, HealthBonus = 140, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1075, Name = "火焰盾牌", Description = "火焰领主套装 - 盾牌", Value = 4000, Defense = 48, HealthBonus = 180, ArmorType = Armor.ArmorType.Shield });
            
            // Set 9: Frost Heart (冰霜之心) - ID 1081-1085
            AddItem(new Weapon { Id = 1081, Name = "冰霜剑", Description = "冰霜之心套装 - 剑", Value = 5500, Damage = 75, CriticalChance = 0.2f, WeaponType = Weapon.WeaponType.Sword });
            AddItem(new Armor { Id = 1082, Name = "冰霜头盔", Description = "冰霜之心套装 - 头盔", Value = 3700, Defense = 28, HealthBonus = 150, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1083, Name = "冰霜胸甲", Description = "冰霜之心套装 - 胸甲", Value = 4500, Defense = 40, HealthBonus = 200, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1084, Name = "冰霜护腿", Description = "冰霜之心套装 - 护腿", Value = 3300, Defense = 26, HealthBonus = 130, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1085, Name = "冰霜戒指", Description = "冰霜之心套装 - 饰品", Value = 3900, Defense = 15, HealthBonus = 160, ArmorType = Armor.ArmorType.Helmet });
            
            // Set 10: Lightning Messenger (闪电使者) - ID 1091-1095
            AddItem(new Weapon { Id = 1091, Name = "雷神之杖", Description = "闪电使者套装 - 法杖", Value = 5800, Damage = 85, AttackSpeed = 1.3f, WeaponType = Weapon.WeaponType.Staff });
            AddItem(new Armor { Id = 1092, Name = "闪电头盔", Description = "闪电使者套装 - 头盔", Value = 3900, Defense = 26, HealthBonus = 140, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1093, Name = "闪电胸甲", Description = "闪电使者套装 - 胸甲", Value = 4700, Defense = 36, HealthBonus = 180, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1094, Name = "闪电护腿", Description = "闪电使者套装 - 护腿", Value = 3500, Defense = 24, HealthBonus = 120, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1095, Name = "闪电靴子", Description = "闪电使者套装 - 靴子", Value = 3200, Defense = 18, HealthBonus = 100, ArmorType = Armor.ArmorType.Legs });
            
            // Set 11: Elven Grace (精灵套装) - ID 1101-1105
            AddItem(new Weapon { Id = 1101, Name = "精灵弓", Description = "精灵套装 - 弓", Value = 5400, Damage = 70, AttackSpeed = 1.4f, WeaponType = Weapon.WeaponType.Bow });
            AddItem(new Armor { Id = 1102, Name = "精灵头盔", Description = "精灵套装 - 头盔", Value = 3600, Defense = 24, HealthBonus = 180, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1103, Name = "精灵皮甲", Description = "精灵套装 - 皮甲", Value = 4400, Defense = 32, HealthBonus = 220, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1104, Name = "精灵护腿", Description = "精灵套装 - 护腿", Value = 3200, Defense = 22, HealthBonus = 140, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1105, Name = "精灵斗篷", Description = "精灵套装 - 斗篷", Value = 3800, Defense = 18, HealthBonus = 160, ArmorType = Armor.ArmorType.Chest });
            
            // Set 12: Titan's Power (泰坦之力) - ID 1111-1115
            AddItem(new Weapon { Id = 1111, Name = "泰坦战斧", Description = "泰坦之力套装 - 战斧", Value = 6500, Damage = 100, AttackSpeed = 0.75f, WeaponType = Weapon.WeaponType.Axe });
            AddItem(new Armor { Id = 1112, Name = "泰坦头盔", Description = "泰坦之力套装 - 头盔", Value = 4500, Defense = 45, HealthBonus = 300, ArmorType = Armor.ArmorType.Helmet });
            AddItem(new Armor { Id = 1113, Name = "泰坦胸甲", Description = "泰坦之力套装 - 胸甲", Value = 5500, Defense = 60, HealthBonus = 400, ArmorType = Armor.ArmorType.Chest });
            AddItem(new Armor { Id = 1114, Name = "泰坦护腿", Description = "泰坦之力套装 - 护腿", Value = 4000, Defense = 40, HealthBonus = 250, ArmorType = Armor.ArmorType.Legs });
            AddItem(new Armor { Id = 1115, Name = "泰坦盾牌", Description = "泰坦之力套装 - 盾牌", Value = 4800, Defense = 55, HealthBonus = 300, ArmorType = Armor.ArmorType.Shield });
            
            // Consumables
            AddItem(new Consumable { Id = 201, Name = "小生命药水", Description = "恢复50生命", Value = 10, HealthRestore = 50 });
            AddItem(new Consumable { Id = 202, Name = "中生命药水", Description = "恢复150生命", Value = 30, HealthRestore = 150 });
            AddItem(new Consumable { Id = 203, Name = "大生命药水", Description = "恢复300生命", Value = 80, HealthRestore = 300 });
            AddItem(new Consumable { Id = 204, Name = "法力药水", Description = "恢复50法力", Value = 20, ManaRestore = 50 });
            AddItem(new Consumable { Id = 205, Name = "生命精华", Description = "恢复全部生命", Value = 200, HealthRestore = 9999 });
            AddItem(new Consumable { Id = 206, Name = "法力精华", Description = "恢复全部法力", Value = 200, ManaRestore = 9999 });
            AddItem(new Consumable { Id = 207, Name = "力量药水", Description = "临时提升攻击力", Value = 50, Duration = 60 });
            AddItem(new Consumable { Id = 208, Name = "防御药水", Description = "临时提升防御力", Value = 50, Duration = 60 });
            
            // Extended consumables
            AddItem(new Consumable { Id = 209, Name = "体力药水", Description = "恢复50体力", Value = 15, StaminaRestore = 50 });
            AddItem(new Consumable { Id = 210, Name = "全恢复药水", Description = "恢复全部生命和法力", Value = 300, HealthRestore = 9999, ManaRestore = 9999 });
            AddItem(new Consumable { Id = 211, Name = "速度药水", Description = "临时提升移动速度", Value = 60, Duration = 90 });
            AddItem(new Consumable { Id = 212, Name = "暴击药水", Description = "临时提升暴击率", Value = 80, Duration = 60 });
            AddItem(new Consumable { Id = 213, Name = "魔法药水", Description = "临时提升魔法伤害", Value = 70, Duration = 60 });
            AddItem(new Consumable { Id = 214, Name = "采集药水", Description = "增加掉落率", Value = 100, Duration = 300 });
            AddItem(new Consumable { Id = 215, Name = "经验药水", Description = "增加经验获取", Value = 150, Duration = 180 });
            AddItem(new Consumable { Id = 216, Name = "复活药水", Description = "死亡时自动复活并恢复50%生命", Value = 500 });
            
            // Materials
            AddItem(new Item { Id = 301, Name = "怪物精华", Description = "怪物掉落的精华", Value = 5, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 302, Name = "龙鳞", Description = "巨龙的鳞片", Value = 100, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 303, Name = "凤凰羽毛", Description = "凤凰的羽毛", Value = 150, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 304, Name = "暗影水晶", Description = "蕴含暗影能量的水晶", Value = 80, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 305, Name = "神圣宝珠", Description = "蕴含神圣能量的宝珠", Value = 120, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 306, Name = "古钱币", Description = "古代流通的钱币", Value = 1, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 307, Name = "哥布林耳朵", Description = "哥布林的战利品", Value = 2, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 308, Name = "骷髅骨头", Description = "骷髅的骨头", Value = 3, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 309, Name = "史莱姆凝胶", Description = "史莱姆的凝胶", Value = 1, Type = Item.ItemType.Material });
            
            // New materials from elite enemies
            AddItem(new Item { Id = 310, Name = "狼皮", Description = "森林狼的毛皮", Value = 8, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 311, Name = "蜘蛛丝", Description = "蜘蛛吐出的丝线", Value = 6, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 312, Name = "傀儡核心", Description = "岩石傀儡的核心", Value = 25, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 313, Name = "火焰精华", Description = "火焰元素的精华", Value = 30, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 314, Name = "冰霜精华", Description = "冰霜元素的精华", Value = 30, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 315, Name = "暗影精华", Description = "暗影能量的精华", Value = 35, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 316, Name = "神圣精华", Description = "神圣能量的精华", Value = 35, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 317, Name = "岩浆核心", Description = "岩浆傀儡的核心", Value = 40, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 318, Name = "冰晶", Description = "冰霜地牢的结晶", Value = 28, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 319, Name = "幽魂精华", Description = "幽灵的精华", Value = 32, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 320, Name = "熊皮", Description = "森林熊的毛皮", Value = 15, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 321, Name = "鹿角", Description = " mystic 鹿的角", Value = 12, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 322, Name = "蘑菇", Description = "毒蘑菇", Value = 5, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 323, Name = "毒精华", Description = "毒素的精华", Value = 18, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 324, Name = "蜈蚣腿", Description = "巨型蜈蚣的腿", Value = 10, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 325, Name = "巨魔肉", Description = "巨魔的肉", Value = 20, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 326, Name = "熔岩鳞片", Description = "熔岩鳗的鳞片", Value = 22, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 327, Name = "凤凰羽毛", Description = "火凤凰的羽毛", Value = 50, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 328, Name = "雪怪毛皮", Description = "冰霜雪怪的毛皮", Value = 28, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 329, Name = "冰霜核心", Description = "冰霜傀儡的核心", Value = 35, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 330, Name = "吸血鬼獠牙", Description = "吸血鬼的獠牙", Value = 40, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 331, Name = "血液精华", Description = "蕴含生命力的精华", Value = 38, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 332, Name = "龙晶", Description = "龙嗣的晶核", Value = 80, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 333, Name = "龙血", Description = "龙血结晶", Value = 100, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 334, Name = "神圣水晶", Description = "神圣殿堂的水晶", Value = 45, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 335, Name = "神圣羽毛", Description = "神圣天使的羽毛", Value = 60, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 336, Name = "树精之心", Description = "古老树精的生命核心", Value = 200, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 337, Name = "恶魔之冠", Description = "恶魔领主的冠冕", Value = 500, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 338, Name = "恶魔之心", Description = "恶魔领主的核心", Value = 500, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 339, Name = "自然精华", Description = "自然能量精华", Value = 25, Type = Item.ItemType.Material });
            
            // === New materials for Dragon's Lair ===
            AddItem(new Item { Id = 340, Name = "龙鳞", Description = "巨龙的鳞片", Value = 100, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 341, Name = "龙血结晶", Description = "龙血凝固形成的结晶", Value = 150, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 342, Name = "龙晶", Description = "巨龙的生命晶核", Value = 200, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 343, Name = "老龙鳞", Description = "远古巨龙的鳞片", Value = 300, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 344, Name = "老龙血", Description = "远古巨龙的血液精华", Value = 400, Type = Item.ItemType.Material });
            
            // === New materials for Holy Temple ===
            AddItem(new Item { Id = 345, Name = "神圣羽毛", Description = "神圣护卫的羽毛", Value = 80, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 346, Name = "天使羽毛", Description = "堕落天使的羽毛", Value = 120, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 347, Name = "天堂水晶", Description = "天堂能量的结晶", Value = 100, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 348, Name = "神光精华", Description = "神圣光芒的精华", Value = 150, Type = Item.ItemType.Material });
            
            // === New materials for Swamp ===
            AddItem(new Item { Id = 349, Name = "腐肉", Description = "沼泽僵尸的腐肉", Value = 5, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 350, Name = "鳄鱼鳞片", Description = "鳄鱼坚硬鳞片", Value = 25, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 351, Name = "蚊刺", Description = "蚊群的刺针", Value = 3, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 352, Name = "女巫之帽", Description = "沼泽女巫的魔法帽子", Value = 60, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 353, Name = "毒素精华", Description = "剧毒精华", Value = 30, Type = Item.ItemType.Material });
            
            // === New materials for Abyss ===
            AddItem(new Item { Id = 354, Name = "虚空碎片", Description = "来自虚空维度的碎片", Value = 100, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 355, Name = "深渊精华", Description = "深渊恶魔的能量精华", Value = 80, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 356, Name = "暗能核心", Description = "暗影能量的核心", Value = 90, Type = Item.ItemType.Material });
            
            // === New materials for Mini-bosses ===
            AddItem(new Item { Id = 357, Name = "哥布林首领之牙", Description = "哥布林首领的獠牙", Value = 40, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 358, Name = "骷髅头骨", Description = "骷髅领主的头骨", Value = 50, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 359, Name = "女巫扫帚", Description = "女巫的魔法扫帚", Value = 45, Type = Item.ItemType.Material });
            
            // Enhancement stones
            AddItem(new Item { Id = 401, Name = "普通强化石", Description = "用于装备强化的基础材料", Value = 100, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 402, Name = "优秀强化石", Description = "高品质强化材料，可提高5%强化成功率", Value = 500, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 403, Name = "稀有强化石", Description = "稀有强化材料，可提高10%强化成功率", Value = 2000, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 404, Name = "史诗强化石", Description = "史诗级强化材料，可提高15%强化成功率", Value = 10000, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 405, Name = "传说强化石", Description = "传说级强化材料，可提高25%强化成功率", Value = 50000, Type = Item.ItemType.Material });

            // === Enchantment Scrolls - Attack Type ===
            AddItem(new Item { Id = 501, Name = "锋利卷轴", Description = "附魔攻击+5", Value = 100, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 502, Name = "锐利卷轴", Description = "附魔攻击+10", Value = 300, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 503, Name = "嗜血卷轴", Description = "附魔暴击率+5%", Value = 500, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 504, Name = "致命卷轴", Description = "附魔暴击伤害+15%", Value = 800, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 505, Name = "闪电卷轴", Description = "附魔攻击速度+10%", Value = 600, Type = Item.ItemType.Material });

            // === Enchantment Scrolls - Defense Type ===
            AddItem(new Item { Id = 506, Name = "坚固卷轴", Description = "附魔防御+5", Value = 100, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 507, Name = "铁壁卷轴", Description = "附魔防御+10", Value = 300, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 508, Name = "生命卷轴", Description = "附魔生命+50", Value = 400, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 509, Name = "重生卷轴", Description = "附魔生命+100", Value = 700, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 510, Name = "恢复卷轴", Description = "附魔生命+200", Value = 1200, Type = Item.ItemType.Material });

            // === Enchantment Scrolls - Magic Type ===
            AddItem(new Item { Id = 511, Name = "魔法卷轴", Description = "附魔法力+30", Value = 150, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 512, Name = "奥术卷轴", Description = "附魔法力+60", Value = 400, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 513, Name = "智慧卷轴", Description = "附魔法力+100", Value = 800, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 514, Name = "冰霜抗性卷轴", Description = "附魔冰霜抗性+10%", Value = 250, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 515, Name = "火焰抗性卷轴", Description = "附魔火焰抗性+10%", Value = 250, Type = Item.ItemType.Material });

            // === Enchantment Scrolls - Utility Type ===
            AddItem(new Item { Id = 516, Name = "敏捷卷轴", Description = "附魔移动速度+5%", Value = 200, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 517, Name = "疾风卷轴", Description = "附魔移动速度+10%", Value = 500, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 518, Name = "全抗性卷轴", Description = "附魔所有抗性+5%", Value = 600, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 519, Name = "雷电抗性卷轴", Description = "附魔雷电抗性+10%", Value = 250, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 520, Name = "毒液抗性卷轴", Description = "附魔毒抗性+10%", Value = 250, Type = Item.ItemType.Material });

            // === Enchantment Scrolls - Legendary Type ===
            AddItem(new Item { Id = 521, Name = "传奇力量卷轴", Description = "附魔全属性+5%", Value = 5000, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 522, Name = "传奇守护卷轴", Description = "附魔防御+30生命+200", Value = 8000, Type = Item.ItemType.Material });
            AddItem(new Item { Id = 523, Name = "传奇攻击卷轴", Description = "附魔攻击+20暴击率+10%", Value = 10000, Type = Item.ItemType.Material });
        }
        
        private void AddItem(Item item)
        {
            _items[item.Id] = item;
        }
        
        public Item GetItem(int id)
        {
            return _items.ContainsKey(id) ? _items[id] : null;
        }
        
        public List<Item> GetAllItems()
        {
            return new List<Item>(_items.Values);
        }
        
        public List<Item> GetItemsByType(Item.ItemType type)
        {
            var result = new List<Item>();
            foreach (var item in _items.Values)
            {
                if (item.Type == type)
                    result.Add(item);
            }
            return result;
        }
    }
    
    /// <summary>
    /// Player inventory system
    /// </summary>
    public class Inventory
    {
        public const int MaxSlots = 30;
        
        public class InventorySlot
        {
            public Item Item { get; set; }
            public int Quantity { get; set; }
        }
        
        private InventorySlot[] _slots = new InventorySlot[MaxSlots];
        
        public Inventory()
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                _slots[i] = new InventorySlot();
            }
        }
        
        public bool AddItem(int itemId, int quantity = 1)
        {
            var item = ItemDatabase.Instance.GetItem(itemId);
            if (item == null) return false;
            
            // Try to stack
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i].Item != null && _slots[i].Item.Id == itemId && _slots[i].Quantity < item.MaxStack)
                {
                    int canAdd = item.MaxStack - _slots[i].Quantity;
                    int toAdd = Math.Min(canAdd, quantity);
                    
                    _slots[i].Quantity += toAdd;
                    quantity -= toAdd;
                    
                    if (quantity <= 0) return true;
                }
            }
            
            // Find empty slot
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i].Item == null)
                {
                    _slots[i].Item = item;
                    _slots[i].Quantity = Math.Min(quantity, item.MaxStack);
                    return true;
                }
            }
            
            return false; // Inventory full
        }
        
        public bool RemoveItem(int itemId, int quantity = 1)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i].Item != null && _slots[i].Item.Id == itemId)
                {
                    if (_slots[i].Quantity >= quantity)
                    {
                        _slots[i].Quantity -= quantity;
                        if (_slots[i].Quantity <= 0)
                        {
                            _slots[i].Item = null;
                            _slots[i].Quantity = 0;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        
        public InventorySlot GetSlot(int index)
        {
            if (index >= 0 && index < MaxSlots)
                return _slots[index];
            return null;
        }
        
        public int GetItemCount(int itemId)
        {
            int count = 0;
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i].Item != null && _slots[i].Item.Id == itemId)
                {
                    count += _slots[i].Quantity;
                }
            }
            return count;
        }
    }
    
    /// <summary>
    /// Equipment slots
    /// </summary>
    public class Equipment
    {
        public Item Weapon { get; set; }
        public Item Armor { get; set; }
        public Item Accessory1 { get; set; }
        public Item Accessory2 { get; set; }
        
        public Item GetEquipped(int slot)
        {
            return slot switch
            {
                0 => Weapon,
                1 => Armor,
                2 => Accessory1,
                3 => Accessory2,
                _ => null
            };
        }
        
        public bool Equip(Item item)
        {
            if (item == null) return false;
            
            switch (item.Type)
            {
                case Item.ItemType.Weapon:
                    Weapon = item;
                    break;
                case Item.ItemType.Armor:
                    Armor = item;
                    break;
                case Item.ItemType.Accessory:
                    if (Accessory1 == null) Accessory1 = item;
                    else if (Accessory2 == null) Accessory2 = item;
                    else return false;
                    break;
                default:
                    return false;
            }
            return true;
        }
        
        public Item Unequip(int slot)
        {
            Item item = GetEquipped(slot);
            switch (slot)
            {
                case 0: Weapon = null; break;
                case 1: Armor = null; break;
                case 2: Accessory1 = null; break;
                case 3: Accessory2 = null; break;
            }
            return item;
        }
    }
}
