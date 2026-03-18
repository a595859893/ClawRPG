using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
{
    /// <summary>
    /// 炼金数据库主控制器 - 协调各子模块，管理炼金系统
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
                _instance._recipeStore = new AlchemyRecipeStore();
                _instance._effectCalc = new AlchemyEffectCalc();
                _instance._inventory = new AlchemyInventorySystem();
                _instance._inventory.InitializeMaterials();
                _instance.IsInitialized = true;
            }
        }
        
        // 子系统引用
        private AlchemyRecipeStore _recipeStore;
        private AlchemyEffectCalc _effectCalc;
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
            _recipeStore = GetNodeOrNull<AlchemyRecipeStore>("AlchemyRecipeStore");
            _effectCalc = GetNodeOrNull<AlchemyEffectCalc>("AlchemyEffectCalc");
            _inventory = GetNodeOrNull<AlchemyInventorySystem>("AlchemyInventorySystem");
            
            // 如果节点不存在，创建默认实现
            if (_recipeStore == null)
            {
                _recipeStore = new AlchemyRecipeStore();
                AddChild(_recipeStore);
            }
            
            if (_effectCalc == null)
            {
                _effectCalc = new AlchemyEffectCalc();
                AddChild(_effectCalc);
            }
            
            if (_inventory == null)
            {
                _inventory = new AlchemyInventorySystem();
                AddChild(_inventory);
            }
            
            _inventory.InitializeMaterials();
            
            // 连接信号
            if (_recipeStore != null)
            {
                _recipeStore.Connect(SignalName.CraftSuccess, Callable.From((int rId, int itemId, int qty) => 
                    EmitSignal(SignalName.CraftSuccess, rId, itemId, qty)));
                _recipeStore.Connect(SignalName.CraftFailed, Callable.From((int rId, string reason) => 
                    EmitSignal(SignalName.CraftFailed, rId, reason)));
            }
        }
        
        protected override string SystemName => "AlchemyDatabase";
        
        #region Materials (Delegates)
        
        /// <summary>
        /// 添加材料
        /// </summary>
        private void AddMaterial(AlchemyMaterial material)
        {
            _inventory?.AddMaterial(material);
        }
        
        /// <summary>
        /// 获取材料
        /// </summary>
        public AlchemyMaterial GetMaterial(int id)
        {
            return _inventory?.GetMaterial(id);
        }
        
        /// <summary>
        /// 获取所有材料
        /// </summary>
        public List<AlchemyMaterial> GetAllMaterials()
        {
            return _inventory?.GetAllMaterials() ?? new List<AlchemyMaterial>();
        }
        
        /// <summary>
        /// 根据类型获取材料
        /// </summary>
        public List<AlchemyMaterial> GetMaterialsByType(AlchemyMaterialType type)
        {
            return _inventory?.GetMaterialsByType(type) ?? new List<AlchemyMaterial>();
        }
        
        /// <summary>
        /// 根据稀有度获取材料
        /// </summary>
        public List<AlchemyMaterial> GetMaterialsByRarity(AlchemyMaterialRarity rarity)
        {
            return _inventory?.GetMaterialsByRarity(rarity) ?? new List<AlchemyMaterial>();
        }
        
        /// <summary>
        /// 根据稀有度获取随机材料
        /// </summary>
        public AlchemyMaterial GetRandomMaterialByRarity(AlchemyMaterialRarity rarity)
        {
            return _inventory?.GetRandomMaterialByRarity(rarity);
        }
        
        /// <summary>
        /// 获取随机材料
        /// </summary>
        public AlchemyMaterial GetRandomMaterial()
        {
            return _inventory?.GetRandomMaterial();
        }
        
        #endregion
        
        #region Recipe Delegates
        
        /// <summary>
        /// 获取配方
        /// </summary>
        public AlchemyRecipe GetRecipe(int id)
        {
            return _recipeStore?.GetRecipe(id);
        }
        
        /// <summary>
        /// 获取所有配方
        /// </summary>
        public List<AlchemyRecipe> GetAllRecipes()
        {
            return _recipeStore?.GetAllRecipes() ?? new List<AlchemyRecipe>();
        }
        
        /// <summary>
        /// 根据玩家等级获取配方
        /// </summary>
        public List<AlchemyRecipe> GetRecipesByLevel(int playerLevel)
        {
            return _recipeStore?.GetRecipesByLevel(playerLevel) ?? new List<AlchemyRecipe>();
        }
        
        #endregion
        
        #region Crafting Delegates
        
        /// <summary>
        /// 尝试合成
        /// </summary>
        public AlchemyRecipeStore.CraftResult TryCraft(int recipeId, Dictionary<int, int> availableMaterials, int playerLevel, int gold)
        {
            return _recipeStore?.TryCraft(recipeId, availableMaterials, playerLevel, gold) 
                ?? new AlchemyRecipeStore.CraftResult { Success = false, Message = "Recipe store not available" };
        }
        
        /// <summary>
        /// 检查是否可以合成
        /// </summary>
        public bool CanCraft(int recipeId, Dictionary<int, int> availableMaterials, int playerLevel, int gold)
        {
            return _recipeStore?.CanCraft(recipeId, availableMaterials, playerLevel, gold) ?? false;
        }
        
        /// <summary>
        /// 获取合成所需金币
        /// </summary>
        public int CalculateGoldCost(int recipeId)
        {
            return _recipeStore?.CalculateGoldCost(recipeId) ?? 0;
        }
        
        /// <summary>
        /// 获取合成需求
        /// </summary>
        public List<AlchemyRecipeRequirement> GetRequirements(int recipeId)
        {
            return _recipeStore?.GetRequirements(recipeId) ?? new List<AlchemyRecipeRequirement>();
        }
        
        #endregion
        
        #region Effects Delegates
        
        /// <summary>
        /// 应用即时效果
        /// </summary>
        public void ApplyInstantEffect(string targetId, AlchemyEffectCalc.EffectType type, float value)
        {
            _effectCalc?.ApplyInstantEffect(targetId, type, value);
        }
        
        /// <summary>
        /// 应用持续效果
        /// </summary>
        public void ApplyDurationEffect(string targetId, AlchemyEffectCalc.EffectType type, float value, float duration)
        {
            _effectCalc?.ApplyDurationEffect(targetId, type, value, duration);
        }
        
        /// <summary>
        /// 移除效果
        /// </summary>
        public void RemoveEffect(string targetId, AlchemyEffectCalc.EffectType type)
        {
            _effectCalc?.RemoveEffect(targetId, type);
        }
        
        /// <summary>
        /// 获取活跃效果
        /// </summary>
        public List<AlchemyEffectCalc.ActiveEffect> GetActiveEffects(string targetId)
        {
            return _effectCalc?.GetActiveEffects(targetId) ?? new List<AlchemyEffectCalc.ActiveEffect>();
        }
        
        /// <summary>
        /// 根据物品ID获取效果
        /// </summary>
        public List<AlchemyEffectCalc.PotionEffect> GetEffectsFromItemId(int itemId)
        {
            return _effectCalc?.GetEffectsFromItemId(itemId) ?? new List<AlchemyEffectCalc.PotionEffect>();
        }
        
        /// <summary>
        /// 使用物品
        /// </summary>
        public void UseItem(int itemId, string targetId)
        {
            _effectCalc?.UseItem(itemId, targetId);
        }
        
        /// <summary>
        /// 是否有特定效果
        /// </summary>
        public bool HasEffect(string targetId, AlchemyEffectCalc.EffectType type)
        {
            return _effectCalc?.HasEffect(targetId, type) ?? false;
        }
        
        #endregion
        
        #region Save/Load
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            if (_inventory != null)
            {
                data["inventory"] = _inventory.ExportSaveData();
            }
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("inventory") && _inventory != null)
            {
                _inventory.ImportSaveData((Dictionary)data["inventory"]);
            }
        }
        
        #endregion
        
        #region System Info
        
        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        public string GetSystemStatus()
        {
            return $"[AlchemyDatabase] Materials: {_inventory?.GetAllMaterials().Count ?? 0}, " +
                   $"Recipes: {_recipeStore?.GetAllRecipes().Count ?? 0}";
        }
        
        #endregion
    }
}
