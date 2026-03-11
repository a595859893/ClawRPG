using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Costume data structures
    /// </summary>
    
    public enum CostumeCategory {
        Outfit,      // 服装
        Hat,         // 帽子
        WeaponSkin,  // 武器外观
        Effect,      // 特效
        Trail        // 拖尾效果
    }
    
    [System.Serializable]
    public class CostumeData {
        public string Id;
        public string Name;
        public string Description;
        public CostumeCategory Category;
        public int Cost;           // 价格
        public bool IsDefault;     // 是否默认解锁
        public bool IsPurchased;  // 是否已购买
        public bool IsEquipped;    // 是否已装备
        public string IconPath;
        public string ResourcePath; // 资源路径
        
        public CostumeData() {
            IsDefault = false;
            IsPurchased = false;
            IsEquipped = false;
        }
    }
    
    [System.Serializable]
    public class PlayerCostumeData {
        public List<string> PurchasedCostumes = new();
        public string EquippedOutfit = "";
        public string EquippedHat = "";
        public string EquippedWeaponSkin = "";
        public string EquippedEffect = "";
        public string EquippedTrail = "";
        
        public PlayerCostumeData() {
            PurchasedCostumes = new List<string>();
        }
    }
}
