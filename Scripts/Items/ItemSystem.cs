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
            
            // Armor
            AddItem(new Armor { Id = 101, Name = "布袍", Description = "基础法师袍", Value = 20, Defense = 2, HealthBonus = 10 });
            AddItem(new Armor { Id = 102, Name = "皮甲", Description = "基础皮甲", Value = 30, Defense = 5 });
            AddItem(new Armor { Id = 103, Name = "锁甲", Description = "锁子甲", Value = 100, Defense = 10, HealthBonus = 20 });
            AddItem(new Armor { Id = 104, Name = "铁甲", Description = "重装铁甲", Value = 300, Defense = 20, HealthBonus = 50 });
            AddItem(new Armor { Id = 105, Name = "龙鳞甲", Description = "龙鳞制成的护甲", Value = 1000, Defense = 35, HealthBonus = 100 });
            AddItem(new Armor { Id = 106, Name = "金甲", Description = "华丽金甲", Value = 2000, Defense = 45, HealthBonus = 150 });
            AddItem(new Armor { Id = 107, Name = "神话战甲", Description = "神话级护甲", Value = 5000, Defense = 60, HealthBonus = 250 });
            
            // Consumables
            AddItem(new Consumable { Id = 201, Name = "小生命药水", Description = "恢复50生命", Value = 10, HealthRestore = 50 });
            AddItem(new Consumable { Id = 202, Name = "中生命药水", Description = "恢复150生命", Value = 30, HealthRestore = 150 });
            AddItem(new Consumable { Id = 203, Name = "大生命药水", Description = "恢复300生命", Value = 80, HealthRestore = 300 });
            AddItem(new Consumable { Id = 204, Name = "法力药水", Description = "恢复50法力", Value = 20, ManaRestore = 50 });
            AddItem(new Consumable { Id = 205, Name = "生命精华", Description = "恢复全部生命", Value = 200, HealthRestore = 9999 });
            AddItem(new Consumable { Id = 206, Name = "法力精华", Description = "恢复全部法力", Value = 200, ManaRestore = 9999 });
            AddItem(new Consumable { Id = 207, Name = "力量药水", Description = "临时提升攻击力", Value = 50, Duration = 60 });
            AddItem(new Consumable { Id = 208, Name = "防御药水", Description = "临时提升防御力", Value = 50, Duration = 60 });
            
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
