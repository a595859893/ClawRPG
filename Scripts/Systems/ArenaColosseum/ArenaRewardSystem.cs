using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 竞技场奖励系统 - 处理奖励计算和发放
    /// </summary>
    public partial class ArenaRewardSystem : BaseSystem
    {
        private static ArenaRewardSystem _instance;
        public static ArenaRewardSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ArenaRewardSystem();
                return _instance;
            }
            private set { _instance = value; }
        }

        protected override string SystemName => "ArenaRewardSystem";

        // 信号系统 - 奖励相关
        public event Action<int, int> OnRewardDistributed; // playerId, rewardAmount
        public event Action<ArenaColosseumData.ActiveColosseum, int> OnColosseumEnded; // colosseum, winnerId

        public ArenaRewardSystem()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            GD.Print("[ArenaRewardSystem] Initialized");
        }

        #region 公开接口

        /// <summary>
        /// 计算玩家奖励
        /// </summary>
        public int CalculateReward(ArenaColosseumData.Colosseum colosseum, bool isWinner, int score, int kills)
        {
            if (colosseum == null) return 0;

            int baseReward = isWinner ? colosseum.WinnerReward : colosseum.LoserReward;

            // 胜利者额外奖励
            if (isWinner)
            {
                // 额外击杀奖励
                int killBonus = kills * 50;
                // 表现分奖励 (每100分额外10%)
                int scoreBonus = (int)(baseReward * (score / 100f) * 0.1f);

                return baseReward + killBonus + scoreBonus;
            }

            return baseReward;
        }

        /// <summary>
        /// 计算玩家rating变化
        /// </summary>
        public int CalculateRatingChange(int currentRating, bool isWinner, int opponentRating = 1000)
        {
            // 简单的Elo评分计算
            float expectedScore = 1.0f / (1.0f + (float)Math.Pow(10, (opponentRating - currentRating) / 400.0));
            int kFactor = GetKFactor(currentRating);

            if (isWinner)
            {
                return (int)(kFactor * (1 - expectedScore));
            }
            else
            {
                return -(int)(kFactor * expectedScore);
            }
        }

        /// <summary>
        /// 根据rating获取K因子
        /// </summary>
        private int GetKFactor(int rating)
        {
            if (rating >= 2000) return 16; // 大师+
            if (rating >= 1500) return 24; // 钻石
            if (rating >= 1000) return 32; // 黄金
            return 40; // 新手
        }

        /// <summary>
        /// 结束角斗场并发放奖励
        /// </summary>
        public void EndColosseum(ArenaColosseumData.ActiveColosseum ac, int winnerId, 
            ArenaColosseumData.Colosseum colosseum, Action<int, bool, int, int, int> onRewardCallback)
        {
            ac.State = ArenaColosseumData.ColosseumState.Completed;
            ac.WinnerId = winnerId;

            if (colosseum == null) return;

            // 发放奖励给所有参与者
            foreach (var p in ac.Participants)
            {
                bool isWinner = (p.PlayerId == winnerId);
                int prize = CalculateReward(colosseum, isWinner, p.Score, isWinner ? 1 : 0);

                // 回调更新玩家数据
                onRewardCallback?.Invoke(p.PlayerId, isWinner, p.Score, isWinner ? 1 : 0, prize);

                OnRewardDistributed?.Invoke(p.PlayerId, prize);
            }

            OnColosseumEnded?.Invoke(ac, winnerId);
        }

        /// <summary>
        /// 获取玩家连胜奖励加成
        /// </summary>
        public float GetStreakBonusMultiplier(int currentStreak)
        {
            if (currentStreak >= 10) return 1.5f;  // 10连+ 50%加成
            if (currentStreak >= 5) return 1.25f;  // 5连+ 25%加成
            if (currentStreak >= 3) return 1.1f;   // 3连+ 10%加成
            return 1.0f;
        }

        /// <summary>
        /// 计算每日签到奖励
        /// </summary>
        public int GetDailyLoginBonus(int totalWins)
        {
            // 基础100 + 胜利数*10
            return 100 + totalWins * 10;
        }

        /// <summary>
        /// 计算段位奖励
        /// </summary>
        public int GetTierReward(string tier)
        {
            switch (tier)
            {
                case "王者": return 10000;
                case "大师": return 5000;
                case "钻石": return 2000;
                case "铂金": return 1000;
                case "黄金": return 500;
                case "白银": return 200;
                case "青铜": return 100;
                default: return 0;
            }
        }

        /// <summary>
        /// 获取段位晋升所需的rating
        /// </summary>
        public int GetRatingRequiredForTier(string tier)
        {
            switch (tier)
            {
                case "王者": return 2500;
                case "大师": return 2000;
                case "钻石": return 1600;
                case "铂金": return 1300;
                case "黄金": return 1000;
                case "白银": return 700;
                default: return 0;
            }
        }

        /// <summary>
        /// 预览奖励（不实际发放）
        /// </summary>
        public Dictionary<string, int> PreviewRewards(ArenaColosseumData.Colosseum colosseum)
        {
            if (colosseum == null)
            {
                return new Dictionary<string, int>
                {
                    { "winnerReward", 0 },
                    { "loserReward", 0 },
                    { "killBonus", 0 }
                };
            }

            return new Dictionary<string, int>
            {
                { "winnerReward", colosseum.WinnerReward },
                { "loserReward", colosseum.LoserReward },
                { "killBonus", 50 },
                { "prizePool", colosseum.PrizePool }
            };
        }

        /// <summary>
        /// 获取赛季排名奖励
        /// </summary>
        public Dictionary<int, int> GetSeasonRankRewards()
        {
            return new Dictionary<int, int>
            {
                { 1, 50000 },   // 第1名
                { 2, 25000 },   // 第2名
                { 3, 10000 },   // 第3名
                { 4, 5000 },    // 第4名
                { 5, 2500 },    // 第5名
                { 10, 1000 },   // 第10名
                { 50, 500 },    // 第50名
                { 100, 200 }    // 第100名
            };
        }

        /// <summary>
        /// 获取赛季奖励
        /// </summary>
        public int GetSeasonReward(int rank)
        {
            var rewards = GetSeasonRankRewards();
            
            // 找到最高档位
            foreach (var kvp in rewards)
            {
                if (rank <= kvp.Key)
                    return kvp.Value;
            }
            
            return 50; // 参与奖
        }

        #endregion

        #region 存档支持

        public override Dictionary<string, object> ExportSaveData()
        {
            // 奖励系统不需要持久化运行时状态
            return new Dictionary<string, object>();
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 奖励系统不需要持久化运行时状态
        }

        #endregion
    }
}
