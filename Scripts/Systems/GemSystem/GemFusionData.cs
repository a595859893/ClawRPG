using Godot;
using Godot.Collections;
using System;

namespace ClawRPG.Scripts.Systems.GemSystem {
    /// <summary>
    /// 宝石合成配方数据
    /// </summary>
    [System.Serializable]
    public class GemFusionRecipe {
        public string RecipeId;
        public string ResultGemId; // 合成结果宝石ID
        public string SourceGemId; // 源宝石ID (相同类型和等级)
        public int SourceGemCount; // 需要的源宝石数量
        public int GoldCost; // 金币费用
        public float SuccessRate; // 成功率 (0-1)
        public System.Collections.Generic.Dictionary<string, int> Materials; // 额外材料 (材料ID -> 数量)
        
        public GemFusionRecipe() {
            Materials = new System.Collections.Generic.Dictionary<string, int>();
        }
        
        public GemFusionRecipe(string recipeId, string resultGemId, string sourceGemId, 
            int sourceGemCount, int goldCost, float successRate) {
            RecipeId = recipeId;
            ResultGemId = resultGemId;
            SourceGemId = sourceGemId;
            SourceGemCount = sourceGemCount;
            GoldCost = goldCost;
            SuccessRate = Mathf.Clamp01(successRate);
            Materials = new System.Collections.Generic.Dictionary<string, int>();
        }
    }
    
    /// <summary>
    /// 玩家合成数据
    /// </summary>
    [System.Serializable]
    public class PlayerFusionData {
        public int TotalFusions; // 总合成次数
        public int SuccessfulFusions; // 成功次数
        public System.Collections.Generic.Dictionary<string, int> FusionCountByGem; // 按宝石ID统计合成次数
        
        public PlayerFusionData() {
            FusionCountByGem = new System.Collections.Generic.Dictionary<string, int>();
        }
        
        public float GetSuccessRate() {
            if (TotalFusions == 0) return 0f;
            return (float)SuccessfulFusions / TotalFusions;
        }
        
        public void RecordFusion(bool success) {
            TotalFusions++;
            if (success) {
                SuccessfulFusions++;
            }
        }
        
        public void RecordGemFusion(string gemId) {
            if (FusionCountByGem.ContainsKey(gemId)) {
                FusionCountByGem[gemId]++;
            } else {
                FusionCountByGem[gemId] = 1;
            }
        }
    }
}
