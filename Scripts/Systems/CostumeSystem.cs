using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Costume system manager - handles purchasing, equipping, and managing costumes
    /// </summary>
    public partial class CostumeSystem : BaseSystem
    {
        public static CostumeSystem Instance { get; private set; }
        
        private PlayerCostumeData _playerData = new();
        private CostumeDatabase _database;
        
        // Signals
        public static Signal CostumePurchased { get; } = new("costume_purchased");
        public static Signal CostumeEquipped { get; } = new("costume_equipped");
        public static Signal CostumeUnequipped { get; } = new("costume_unequipped");
        
        public CostumeSystem()
        {
            _database = CostumeDatabase.Instance;
        }
        
        public override void _Ready()
        {
            Instance = this;
            LoadCostumeData();
        }
        
        /// <summary>
        /// Purchase a costume
        /// </summary>
        public bool PurchaseCostume(string costumeId)
        {
            var costume = _database.GetCostume(costumeId);
            if (costume == null)
            {
                GD.PrintErr($"Costume not found: {costumeId}");
                return false;
            }
            
            if (costume.IsPurchased)
            {
                GD.Print($"Costume already purchased: {costume.Name}");
                return false;
            }
            
            // Check if player has enough gold
            var player = GetTree().CurrentScene?.GetNode<Player>("Player");
            if (player == null)
            {
                GD.PrintErr("Player not found");
                return false;
            }
            
            if (player.Gold < costume.Cost)
            {
                GD.Print($"Not enough gold to purchase {costume.Name}. Need {costume.Cost}, have {player.Gold}");
                return false;
            }
            
            // Deduct gold and purchase
            player.Gold -= costume.Cost;
            costume.IsPurchased = true;
            _playerData.PurchasedCostumes.Add(costumeId);
            
            SaveCostumeData();
            CostumePurchased.Emit(costumeId);
            
            GD.Print($"Purchased costume: {costume.Name} for {costume.Cost} gold");
            return true;
        }
        
        /// <summary>
        /// Equip a costume
        /// </summary>
        public bool EquipCostume(string costumeId)
        {
            var costume = _database.GetCostume(costumeId);
            if (costume == null)
            {
                GD.PrintErr($"Costume not found: {costumeId}");
                return false;
            }
            
            if (!costume.IsPurchased)
            {
                GD.Print($"Costume not purchased: {costume.Name}");
                return false;
            }
            
            // Unequip current costume of same category
            UnequipCostumeByCategory(costume.Category);
            
            // Equip new costume
            costume.IsEquipped = true;
            
            switch (costume.Category)
            {
                case CostumeCategory.Outfit:
                    _playerData.EquippedOutfit = costumeId;
                    break;
                case CostumeCategory.Hat:
                    _playerData.EquippedHat = costumeId;
                    break;
                case CostumeCategory.WeaponSkin:
                    _playerData.EquippedWeaponSkin = costumeId;
                    break;
                case CostumeCategory.Effect:
                    _playerData.EquippedEffect = costumeId;
                    break;
                case CostumeCategory.Trail:
                    _playerData.EquippedTrail = costumeId;
                    break;
            }
            
            SaveCostumeData();
            CostumeEquipped.Emit(costumeId);
            
            GD.Print($"Equipped costume: {costume.Name}");
            ApplyCostumeVisual(costume);
            return true;
        }
        
        /// <summary>
        /// Unequip a costume
        /// </summary>
        public bool UnequipCostume(string costumeId)
        {
            var costume = _database.GetCostume(costumeId);
            if (costume == null) return false;
            
            if (!costume.IsEquipped) return false;
            
            costume.IsEquipped = true;
            RemoveCostumeVisual(costume);
            
            // Reset category slot
            switch (costume.Category)
            {
                case CostumeCategory.Outfit:
                    _playerData.EquippedOutfit = "";
                    break;
                case CostumeCategory.Hat:
                    _playerData.EquippedHat = "";
                    break;
                case CostumeCategory.WeaponSkin:
                    _playerData.EquippedWeaponSkin = "";
                    break;
                case CostumeCategory.Effect:
                    _playerData.EquippedEffect = "";
                    break;
                case CostumeCategory.Trail:
                    _playerData.EquippedTrail = "";
                    break;
            }
            
            SaveCostumeData();
            CostumeUnequipped.Emit(costumeId);
            
            return true;
        }
        
        private void UnequipCostumeByCategory(CostumeCategory category)
        {
            string currentId = "";
            
            switch (category)
            {
                case CostumeCategory.Outfit:
                    currentId = _playerData.EquippedOutfit;
                    break;
                case CostumeCategory.Hat:
                    currentId = _playerData.EquippedHat;
                    break;
                case CostumeCategory.WeaponSkin:
                    currentId = _playerData.EquippedWeaponSkin;
                    break;
                case CostumeCategory.Effect:
                    currentId = _playerData.EquippedEffect;
                    break;
                case CostumeCategory.Trail:
                    currentId = _playerData.EquippedTrail;
                    break;
            }
            
            if (!string.IsNullOrEmpty(currentId))
            {
                var current = _database.GetCostume(currentId);
                if (current != null)
                {
                    current.IsEquipped = false; 
                    RemoveCostumeVisual(current);
                }
            }
        }
        
        /// <summary>
        /// Apply costume visual effects
        /// </summary>
        private void ApplyCostumeVisual(CostumeData costume)
        {
            var player = GetTree().CurrentScene?.GetNode<Player>("Player");
            if (player == null) return;
            
            // Apply visual changes based on costume category
            // This would modify player appearance based on the costume
            // Actual implementation depends on how player visuals are structured
        }
        
        /// <summary>
        /// Remove costume visual effects
        /// </summary>
        private void RemoveCostumeVisual(CostumeData costume)
        {
            var player = GetTree().CurrentScene?.GetNode<Player>("Player");
            if (player == null) return;
            
            // Remove visual changes
        }
        
        /// <summary>
        /// Get equipped costume for category
        /// </summary>
        public string GetEquippedCostume(CostumeCategory category)
        {
            switch (category)
            {
                case CostumeCategory.Outfit:
                    return _playerData.EquippedOutfit;
                case CostumeCategory.Hat:
                    return _playerData.EquippedHat;
                case CostumeCategory.WeaponSkin:
                    return _playerData.EquippedWeaponSkin;
                case CostumeCategory.Effect:
                    return _playerData.EquippedEffect;
                case CostumeCategory.Trail:
                    return _playerData.EquippedTrail;
            }
            return "";
        }
        
        /// <summary>
        /// Get all purchased costumes
        /// </summary>
        public List<CostumeData> GetPurchasedCostumes()
        {
            List<CostumeData> result = new();
            foreach (var id in _playerData.PurchasedCostumes)
            {
                var costume = _database.GetCostume(id);
                if (costume != null)
                    result.Add(costume);
            }
            return result;
        }
        
        /// <summary>
        /// Save costume data
        /// </summary>
        public void SaveCostumeData()
        {
            var saveSystem = GetTree().CurrentScene?.GetNode<Systems.SaveSystem>("SaveSystem");
            if (saveSystem != null)
            {
                var data = new Dictionary
                {
                    { "purchased_costumes", new Array(_playerData.PurchasedCostumes) },
                    { "equipped_outfit", _playerData.EquippedOutfit },
                    { "equipped_hat", _playerData.EquippedHat },
                    { "equipped_weapon_skin", _playerData.EquippedWeaponSkin },
                    { "equipped_effect", _playerData.EquippedEffect },
                    { "equipped_trail", _playerData.EquippedTrail }
                };
                saveSystem.SaveData("costume_system", data);
            }
        }
        
        /// <summary>
        /// Load costume data
        /// </summary>
        public void LoadCostumeData()
        {
            var saveSystem = GetTree().CurrentScene?.GetNode<Systems.SaveSystem>("SaveSystem");
            if (saveSystem != null)
            {
                var data = saveSystem.LoadData("costume_system") as Dictionary;
                if (data != null)
                {
                    if (data.Contains("purchased_costumes"))
                    {
                        var purchased = data["purchased_costumes"] as Array;
                        _playerData.PurchasedCostumes = new List<string>();
                        foreach (var item in purchased)
                        {
                            _playerData.PurchasedCostumes.Add((string)item);
                            var costume = _database.GetCostume((string)item);
                            if (costume != null)
                                costume.IsPurchased = true;
                        }
                    }
                    
                    if (data.Contains("equipped_outfit"))
                        _playerData.EquippedOutfit = (string)data["equipped_outfit"];
                    if (data.Contains("equipped_hat"))
                        _playerData.EquippedHat = (string)data["equipped_hat"];
                    if (data.Contains("equipped_weapon_skin"))
                        _playerData.EquippedWeaponSkin = (string)data["equipped_weapon_skin"];
                    if (data.Contains("equipped_effect"))
                        _playerData.EquippedEffect = (string)data["equipped_effect"];
                    if (data.Contains("equipped_trail"))
                        _playerData.EquippedTrail = (string)data["equipped_trail"];
                    
                    // Apply equipped status
                    UpdateEquippedStatus();
                }
            }
        }
        
        private void UpdateEquippedStatus()
        {
            foreach (var id in _playerData.PurchasedCostumes)
            {
                var costume = _database.GetCostume(id);
                if (costume != null)
                {
                    costume.IsEquipped = (
                        id == _playerData.EquippedOutfit ||
                        id == _playerData.EquippedHat ||
                        id == _playerData.EquippedWeaponSkin ||
                        id == _playerData.EquippedEffect ||
                        id == _playerData.EquippedTrail
                    );
                }
            }
        }
        
        /// <summary>
        /// Get costume count
        /// </summary>
        public int GetTotalCostumeCount() => _database.GetAllCostumes().Count;
        
        public int GetPurchasedCostumeCount() => _playerData.PurchasedCostumes.Count;
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "purchased_costumes", new Array(_playerData.PurchasedCostumes) },
                { "equipped_outfit", _playerData.EquippedOutfit },
                { "equipped_hat", _playerData.EquippedHat },
                { "equipped_weapon_skin", _playerData.EquippedWeaponSkin },
                { "equipped_effect", _playerData.EquippedEffect },
                { "equipped_trail", _playerData.EquippedTrail }
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.Contains("purchased_costumes"))
            {
                var purchased = data["purchased_costumes"] as Array;
                _playerData.PurchasedCostumes = new List<string>();
                foreach (var item in purchased)
                {
                    _playerData.PurchasedCostumes.Add((string)item);
                    var costume = _database.GetCostume((string)item);
                    if (costume != null)
                        costume.IsPurchased = true;
                }
            }
            
            if (data.Contains("equipped_outfit"))
                _playerData.EquippedOutfit = (string)data["equipped_outfit"];
            if (data.Contains("equipped_hat"))
                _playerData.EquippedHat = (string)data["equipped_hat"];
            if (data.Contains("equipped_weapon_skin"))
                _playerData.EquippedWeaponSkin = (string)data["equipped_weapon_skin"];
            if (data.Contains("equipped_effect"))
                _playerData.EquippedEffect = (string)data["equipped_effect"];
            if (data.Contains("equipped_trail"))
                _playerData.EquippedTrail = (string)data["equipped_trail"];
            
            UpdateEquippedStatus();
        }
    }
}
