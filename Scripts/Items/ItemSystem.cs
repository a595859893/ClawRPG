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
