using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ArenaTournament {
    /// <summary>
    /// 竞技场奖励计算 - 计算比赛奖励
    /// </summary>
    public partial class ArenaRewards : BaseSystem {
        
        /// <summary>
        /// 奖励类型
        /// </summary>
        public enum RewardType {
            Gold,
            Experience,
            Rating,
            Item,
            Title
        }
        
        /// <summary>
        /// 奖励配置
        /// </summary>
        public class RewardConfig {
            public RewardType Type;
            public int Amount;
            public string ItemId;
            public string TitleId;
        }
        
        // 基础奖励
        private int _baseGold = 100;
        private int _baseExp = 50;
        
        // 排名奖励倍数
        private Dictionary<int, float> _rankingMultipliers = new() {
            { 1, 2.0f },
            { 2, 1.5f },
            { 3, 1.2f },
            { 4, 1.0f }
        };
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 计算比赛奖励
        /// </summary>
        public List<RewardConfig> CalculateRewards(int ranking, int playerRating) {
            var rewards = new List<RewardConfig>();
            
            // 金币奖励
            var goldMultiplier = _rankingMultipliers.GetValueOrDefault(ranking, 0.5f);
            var gold = (int)(_baseGold * goldMultiplier * (playerRating / 1000f));
            rewards.Add(new RewardConfig {
                Type = RewardType.Gold,
                Amount = gold
            });
            
            // 经验奖励
            var exp = (int)(_baseExp * goldMultiplier);
            rewards.Add(new RewardConfig {
                Type = RewardType.Experience,
                Amount = exp
            });
            
            // 排名奖励
            var ratingChange = CalculateRatingChange(ranking);
            rewards.Add(new RewardConfig {
                Type = RewardType.Rating,
                Amount = ratingChange
            });
            
            // 特殊奖励
            if (ranking == 1) {
                rewards.Add(new RewardConfig {
                    Type = RewardType.Title,
                    TitleId = "arena_champion"
                });
            } else if (ranking <= 3) {
                rewards.Add(new RewardConfig {
                    Type = RewardType.Title,
                    TitleId = $"arena_top_{ranking}"
                });
            }
            
            return rewards;
        }
        
        /// <summary>
        /// 计算Rating变化
        /// </summary>
        public int CalculateRatingChange(int ranking) {
            return ranking switch {
                1 => 25,
                2 => 15,
                3 => 10,
                4 => 5,
                5 => 0,
                6 => -5,
                7 => -10,
                8 => -15,
                _ => -20
            };
        }
        
        /// <summary>
        /// 计算赛季奖励
        /// </summary>
        public List<RewardConfig> CalculateSeasonRewards(int seasonRanking, int totalPlayers) {
            var rewards = new List<RewardConfig>();
            
            // 前10%获得传奇奖励
            if (seasonRanking <= totalPlayers * 0.1) {
                rewards.Add(new RewardConfig {
                    Type = RewardType.Item,
                    ItemId = "legendary_arena_token"
                });
                rewards.Add(new RewardConfig {
                    Type = RewardType.Title,
                    TitleId = "legendary_gladiator"
                });
            }
            // 前25%获得史诗奖励
            else if (seasonRanking <= totalPlayers * 0.25) {
                rewards.Add(new RewardConfig {
                    Type = RewardType.Item,
                    ItemId = "epic_arena_token"
                });
                rewards.Add(new RewardConfig {
                    Type = RewardType.Title,
                    TitleId = "epic_gladiator"
                });
            }
            
            return rewards;
        }
        
        /// <summary>
        /// 设置基础奖励
        /// </summary>
        public void SetBaseRewards(int gold, int exp) {
            _baseGold = Math.Max(0, gold);
            _baseExp = Math.Max(0, exp);
        }
        
        /// <summary>
        /// 设置排名倍数
        /// </summary>
        public void SetRankingMultiplier(int ranking, float multiplier) {
            _rankingMultipliers[ranking] = multiplier;
        }
        
        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();
            data["baseGold"] = _baseGold;
            data["baseExp"] = _baseExp;
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data) {
            if (data.Contains("baseGold")) {
                _baseGold = (int)data["baseGold"];
            }
            if (data.Contains("baseExp")) {
                _baseExp = (int)data["baseExp"];
            }
        }
    }
}
