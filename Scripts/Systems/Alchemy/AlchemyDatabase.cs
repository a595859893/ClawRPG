using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
{
    /// <summary>
    /// 炼金数据库主控制器 - 协调各子模块
    /// </summary>
    public partial class AlchemyDatabase : BaseSystem
    {
        private static AlchemyDatabase _instance;
        public static AlchemyDatabase Instance => _instance ??= new AlchemyDatabase();
        
        /// <summary>
        /// 静态初始化方法，用于在数据库节点添加到场景树前初始化数据
        /// </summary>
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new AlchemyDatabase();
            }
            if (!_instance.IsInitialized)
            {
                _instance._recipeDB = new AlchemyRecipeDB();
                _instance._crafting = new AlchemyCrafting();
                _instance._effects = new AlchemyEffects();
                _instance._inventory = new AlchemyInventorySystem();
                _instance._inventory.InitializeMaterials();
                _instance.IsInitialized = true;
            }
        }
        
        // 子系统
        private AlchemyRecipeDB _recipeDB;
        private AlchemyCrafting _crafting;
        private AlchemyEffects _effects;
        private AlchemyInventorySystem _inventory;
        
        // Signals
        [Signal] public delegate void CraftSuccessEventHandler(int recipeId, int resultItemId, int quantity);
        [Signal] public delegate void CraftFailedEventHandler(int recipeId, string reason);
        [Signal] public delegate void LevelUpEventHandler(int newLevel);
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            
            // 初始化子系统
            _recipeDB = new AlchemyRecipeDB();
            _crafting = new AlchemyCrafting();
            _effects = new AlchemyEffects();
            _inventory = new AlchemyInventorySystem();
            _inventory.InitializeMaterials();
        }
        
        protected override string SystemName => "AlchemyDatabase";
        
        #region Materials (Delegates)
        
        /// <summary>
        /// 添加材料
        /// </summary>
        private void AddMaterial(AlchemyMaterial material)
        {
            _inventory.AddMaterial(material);
        }
        
        /// <summary>
        /// 获取材料
        /// </summary>
        public AlchemyMaterial GetMaterial(int id)
        {
            return _inventory.GetMaterial(id);
        }
        
        /// <summary>
        /// 获取所有材料
        /// </summary>
        public List<AlchemyMaterial> GetAllMaterials()
        {
            return _inventory.GetAllMaterials();
        }
        
        /// <summary>
        /// 根据类型获取材料
        /// </summary>
        public List<AlchemyMaterial> GetMaterialsByType(AlchemyMaterialType type)
        {
            return _inventory.GetMaterialsByType(type);
        }
        
        /// <summary>
        /// 根据稀有度获取材料
        /// </summary>
        public List<AlchemyMaterial> GetMaterialsByRarity(AlchemyMaterialRarity rarity)
        {
            return _inventory.GetMaterialsByRarity(rarity);
        }
        
        /// <summary>
        /// 根据稀有度获取随机材料
        /// </summary>
        public AlchemyMaterial GetRandomMaterialByRarity(AlchemyMaterialRarity rarity)
        {
            return _inventory.GetRandomMaterialByRarity(rarity);
        }
        
        /// <summary>
        /// 获取随机材料
        /// </summary>
        public AlchemyMaterial GetRandomMaterial()
        {
            return _inventory.GetRandomMaterial();
        }
        
        #endregion
        
        #region Recipe Delegates
        
        /// <summary>
        /// 获取配方
        /// </summary>
        public AlchemyRecipe GetRecipe(int id)
        {
            return _recipeDB.GetRecipe(id);
        }
        
        /// <summary>
        /// 获取所有配方
        /// </summary>
        public List<AlchemyRecipe> GetAllRecipes()
        {
            return _recipeDB.GetAllRecipes();
        }
        
        /// <summary>
        /// 根据玩家等级获取配方
        /// </summary>
        public List<AlchemyRecipe> GetRecipesByLevel(int playerLevel)
        {
            return _recipeDB.GetRecipesByLevel(playerLevel);
        }
        
        #endregion
        
        #region Crafting Delegates
        
        /// <summary>
        /// 尝试合成
        /// </summary>
        public AlchemyCrafting.CraftResult TryCraft(int recipeId, Dictionary<int, int> availableMaterials, int playerLevel, int gold)
        {
            var result = _crafting.TryCraft(recipeId, availableMaterials, playerLevel, gold);
            
            if (result.Success)
            {
                EmitSignal(SignalName.CraftSuccess, recipeId, result.ResultItemId, result.ResultQuantity);
            }
            else
            {
                EmitSignal(SignalName.CraftFailed, recipeId, result.Message);
            }
            
            return result;
        }
        
        /// <summary>
        /// 检查是否可以合成
        /// </summary>
        public bool CanCraft(int recipeId, Dictionary<int, int> availableMaterials, int playerLevel, int gold)
        {
            return _crafting.CanCraft(recipeId, availableMaterials, playerLevel, gold);
        }
        
        /// <summary>
        /// 获取合成所需金币
        /// </summary>
        public int CalculateGoldCost(int recipeId)
        {
            return _crafting.CalculateGoldCost(recipeId);
        }
        
        /// <summary>
        /// 获取合成需求
        /// </summary>
        public List<AlchemyRecipeRequirement> GetRequirements(int recipeId)
        {
            return _crafting.GetRequirements(recipeId);
        }
        
        #endregion
        
        #region Effects Delegates
        
        /// <summary>
        /// 应用即时效果
        /// </summary>
        public void ApplyInstantEffect(string targetId, AlchemyEffects.EffectType type, float value)
        {
            _effects.ApplyInstantEffect(targetId, type, value);
        }
        
        /// <summary>
        /// 应用持续效果
        /// </summary>
        public void ApplyDurationEffect(string targetId, AlchemyEffects.EffectType type, float value, float duration)
        {
            _effects.ApplyDurationEffect(targetId, type, value, duration);
        }
        
        /// <summary>
        /// 移除效果
        /// </summary>
        public void RemoveEffect(string targetId, AlchemyEffects.EffectType type)
        {
            _effects.RemoveEffect(targetId, type);
        }
        
        /// <summary>
        /// 获取活跃效果
        /// </summary>
        public List<AlchemyEffects.ActiveEffect> GetActiveEffects(string targetId)
        {
            return _effects.GetActiveEffects(targetId);
        }
        
        /// <summary>
        /// 根据物品ID获取效果
        /// </summary>
        public List<AlchemyEffects.PotionEffect> GetEffectsFromItemId(int itemId)
        {
            return _effects.GetEffectsFromItemId(itemId);
        }
        
        #endregion
        
        #region Save/Load
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary
            {
                ["inventory"] = _inventory.ExportSaveData()
            };
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("inventory"))
            {
                _inventory.ImportSaveData((Dictionary)data["inventory"]);
            }
        }
        
        #endregion
    }
}
