using Godot;
using Godot.Collections;
using System;

namespace ClawRPG.Scripts.Systems.GemSystem {
    /// <summary>
    /// 宝石合成系统管理器
    /// </summary>
    public partial class GemFusionSystem : BaseSystem {
        public static GemFusionSystem Instance { get; private set; }
        
        // 信号
        public delegate void FusionCompleted(string resultGemId, bool success);
        public delegate void FusionStarted(string sourceGemId);
        
        private PlayerFusionData _playerFusionData;
        private GemSystem _gemSystem;
        private Player _player;
        
        public override void _Ready() {
            Instance = this;
            GemFusionDatabase.Initialize();
            _playerFusionData = new PlayerFusionData();
            
            // 获取GemSystem实例
            _gemSystem = GemSystem.Instance;
            
            // 获取Player实例
            var main = GetTree().CurrentScene;
            if (main != null) {
                _player = main.GetNode<Player>("Player");
            }
            
            AddToGroup("GemFusionSystem");
        }
        
        public void Initialize() {
            if (Instance == null) {
                Instance = this;
                GemFusionDatabase.Initialize();
                _playerFusionData = new PlayerFusionData();
                
                var main = GetTree().CurrentScene;
                if (main != null) {
                    _gemSystem = main.GetNode<GemSystem>("Systems/GemSystem");
                    _player = main.GetNode<Player>("Player");
                }
            }
        }
        
        /// <summary>
        /// 尝试合成宝石
        /// </summary>
        /// <param name="sourceGemId">源宝石ID</param>
        /// <returns>合成结果 (成功返回结果宝石ID，失败返回null)</returns>
        public string TryFusion(string sourceGemId) {
            // 检查是否有合成配方
            var recipe = GemFusionDatabase.GetRecipeByGems(sourceGemId);
            if (recipe == null) {
                GD.PrintErr($"[GemFusion] No fusion recipe found for gem: {sourceGemId}");
                return null;
            }
            
            // 检查玩家是否有足够的源宝石
            int gemCount = _gemSystem.GetGemCount(sourceGemId);
            if (gemCount < recipe.SourceGemCount) {
                GD.PrintErr($"[GemFusion] Not enough gems: {sourceGemId}, need {recipe.SourceGemCount}, have {gemCount}");
                return null;
            }
            
            // 检查金币是否足够
            if (_player != null) {
                if (_player.Gold < recipe.GoldCost) {
                    GD.PrintErr($"[GemFusion] Not enough gold: need {recipe.GoldCost}, have {_player.Gold}");
                    return null;
                }
            }
            
            // 检查额外材料
            foreach (var material in recipe.Materials) {
                // 简化检查，假设有材料系统
                // 这里暂时跳过材料检查
            }
            
            // 开始合成
            EmitSignal(nameof(FusionStarted), sourceGemId);
            
            // 扣除资源
            _gemSystem.RemoveGem(sourceGemId, recipe.SourceGemCount);
            if (_player != null) {
                _player.Gold -= recipe.GoldCost;
            }
            
            // 记录合成
            _playerFusionData.RecordFusion(true); // 先假设成功，后面根据结果调整
            _playerFusionData.RecordGemFusion(sourceGemId);
            
            // 计算合成结果
            bool success = _RollSuccess(recipe.SuccessRate);
            
            string resultGemId = null;
            if (success) {
                resultGemId = recipe.ResultGemId;
                _gemSystem.AddGem(resultGemId, 1);
                GD.Print($"[GemFusion] Fusion successful: {sourceGemId} -> {resultGemId}");
            } else {
                GD.Print($"[GemFusion] Fusion failed: {sourceGemId}");
            }
            
            EmitSignal(nameof(FusionCompleted), resultGemId ?? "", success);
            
            return resultGemId;
        }
        
        /// <summary>
        /// Roll success check
        /// </summary>
        private bool _RollSuccess(float successRate) {
            var random = new RandomNumberGenerator();
            random.Randomize();
            return random.randf() < successRate;
        }
        
        /// <summary>
        /// 获取合成配方信息
        /// </summary>
        public GemFusionRecipe GetRecipe(string sourceGemId) {
            return GemFusionDatabase.GetRecipeByGems(sourceGemId);
        }
        
        /// <summary>
        /// 检查是否可以合成
        /// </summary>
        public bool CanFuse(string sourceGemId) {
            var recipe = GemFusionDatabase.GetRecipeByGems(sourceGemId);
            if (recipe == null) return false;
            
            // 检查宝石数量
            int gemCount = _gemSystem.GetGemCount(sourceGemId);
            if (gemCount < recipe.SourceGemCount) return false;
            
            // 检查金币
            if (_player != null && _player.Gold < recipe.GoldCost) return false;
            
            return true;
        }
        
        /// <summary>
        /// 获取玩家的合成数据
        /// </summary>
        public PlayerFusionData GetPlayerFusionData() {
            return _playerFusionData;
        }
        
        /// <summary>
        /// 获取合成统计
        /// </summary>
        public Dictionary GetFusionStats() {
            return new Dictionary {
                { "total_fusions", _playerFusionData.TotalFusions },
                { "successful_fusions", _playerFusionData.SuccessfulFusions },
                { "success_rate", _playerFusionData.GetSuccessRate() }
            };
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        public Dictionary Save() {
            return new Dictionary {
                { "player_fusion_data", new Dictionary {
                    { "total_fusions", _playerFusionData.TotalFusions },
                    { "successful_fusions", _playerFusionData.SuccessfulFusions },
                    { "fusion_count_by_gem", _playerFusionData.FusionCountByGem }
                }}
            };
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        public void Load(Dictionary data) {
            if (data == null) return;
            
            if (data.Contains("player_fusion_data")) {
                var fusionData = (Dictionary)data["player_fusion_data"];
                _playerFusionData.TotalFusions = fusionData.Contains("total_fusions") ? 
                    Convert.ToInt32(fusionData["total_fusions"]) : 0;
                _playerFusionData.SuccessfulFusions = fusionData.Contains("successful_fusions") ? 
                    Convert.ToInt32(fusionData["successful_fusions"]) : 0;
                    
                if (fusionData.Contains("fusion_count_by_gem")) {
                    var countData = (Dictionary)fusionData["fusion_count_by_gem"];
                    _playerFusionData.FusionCountByGem = new System.Collections.Generic.Dictionary<string, int>();
                    foreach (var key in countData.Keys) {
                        _playerFusionData.FusionCountByGem[key.ToString()] = Convert.ToInt32(countData[key]);
                    }
                }
            }
        }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
    {
        var data = new System.Collections.Generic.Dictionary<string, object>();
        
        // 玩家合成数据
        data["total_fusions"] = _playerFusionData.TotalFusions;
        data["successful_fusions"] = _playerFusionData.SuccessfulFusions;
        
        // 按宝石类型的合成次数
        var fusionCountData = new System.Collections.Generic.Dictionary<string, object>();
        foreach (var kvp in _playerFusionData.FusionCountByGem)
        {
            fusionCountData[kvp.Key] = kvp.Value;
        }
        data["fusion_count_by_gem"] = fusionCountData;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("total_fusions")) _playerFusionData.TotalFusions = (int)data["total_fusions"];
        if (data.Contains("successful_fusions")) _playerFusionData.SuccessfulFusions = (int)data["successful_fusions"];
        
        _playerFusionData.FusionCountByGem.Clear();
        if (data.Contains("fusion_count_by_gem"))
        {
            var fusionCountData = (Dictionary)data["fusion_count_by_gem"];
            foreach (var kvp in fusionCountData)
            {
                _playerFusionData.FusionCountByGem[kvp.Key] = (int)kvp.Value;
            }
        }
    }
}
}
