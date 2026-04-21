using Godot;
using System;
using System.Collections.Generic;
using Framework;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

namespace 悬疑RPG
{
    /// <summary>
    /// 每日登录奖励数据
    /// </summary>
    public class DailyLoginReward
    {
        public int Day { get; set; }                    // 第几天 (1-7)
        public int GoldRequired { get; set; }            // 需要的金币 (0表示免费)
        public List<string> ItemRewards { get; set; }    // 物品奖励
        public List<int> ItemCounts { get; set; }       // 物品数量
        public int ExpReward { get; set; }             // 经验奖励

        public DailyLoginReward()
        {
            ItemRewards = new List<string>();
            ItemCounts = new List<int>();
        }
    }

    /// <summary>
    /// 玩家每日登录数据
    /// </summary>
    public class PlayerDailyLoginData
    {
        public DateTime LastLoginDate { get; set; }
        public int TotalLoginDays { get; set; }        // 累计登录天数
        public int ConsecutiveLoginDays { get; set; }   // 连续登录天数
        public List<int> ClaimedDays { get; set; }     // 已领取的天数
        public DateTime LastClaimDate { get; set; }    // 最后领取日期

        public PlayerDailyLoginData()
        {
            LastLoginDate = DateTime.MinValue;
            LastClaimDate = DateTime.MinValue;
            ConsecutiveLoginDays = 0;
            TotalLoginDays = 0;
            ClaimedDays = new List<int>();
        }
    }

    /// <summary>
    /// 每日登录奖励系统
    /// </summary>
    public partial class DailyLoginRewardSystem : BaseSystem
    {
        public static DailyLoginRewardSystem Instance { get; private set; }

        // 7天登录奖励配置
        private List<DailyLoginReward> dailyRewards = new List<DailyLoginReward>();
        
        // 玩家数据
        private PlayerDailyLoginData playerData = new PlayerDailyLoginData();
        
        // 信号
        public delegate void LoginDaysUpdatedEventHandler(int consecutiveDays, int totalDays);
        public delegate void RewardClaimedEventHandler(int day, List<string> items, List<int> counts, int gold, int exp);
        public delegate void NewDayAvailableEventHandler();

        public override void _Ready()
        {
            Instance = this;
            InitializeRewards();
            LoadPlayerData();
            CheckLoginStatus();
        }

        /// <summary>
        /// 初始化奖励配置
        /// </summary>
        private void InitializeRewards()
        {
            // 第1天 - 登录礼包
            dailyRewards.Add(new DailyLoginReward
            {
                Day = 1,
                GoldRequired = 0,
                ItemRewards = new List<string> { "HealthPotion" },
                ItemCounts = new List<int> { 5 },
                ExpReward = 100
            });

            // 第2天 - 小额金币
            dailyRewards.Add(new DailyLoginReward
            {
                Day = 2,
                GoldRequired = 0,
                ItemRewards = new List<string> { "GoldCoin" },
                ItemCounts = new List<int> { 100 },
                ExpReward = 150
            });

            // 第3天 - 装备强化石
            dailyRewards.Add(new DailyLoginReward
            {
                Day = 3,
                GoldRequired = 0,
                ItemRewards = new List<string> { "EnhancementStone" },
                ItemCounts = new List<int> { 3 },
                ExpReward = 200
            });

            // 第4天 - 稀有药水
            dailyRewards.Add(new DailyLoginReward
            {
                Day = 4,
                GoldRequired = 0,
                ItemRewards = new List<string> { "ManaPotion", "SpeedPotion" },
                ItemCounts = new List<int> { 3, 2 },
                ExpReward = 250
            });

            // 第5天 - 金币+经验
            dailyRewards.Add(new DailyLoginReward
            {
                Day = 5,
                GoldRequired = 0,
                ItemRewards = new List<string> { "GoldCoin", "ExperienceScroll" },
                ItemCounts = new List<int> { 500, 2 },
                ExpReward = 300
            });

            // 第6天 - 高级强化石
            dailyRewards.Add(new DailyLoginReward
            {
                Day = 6,
                GoldRequired = 0,
                ItemRewards = new List<string> { "EnhancementStone", "RareEnhancementStone" },
                ItemCounts = new List<int> { 5, 2 },
                ExpReward = 400
            });

            // 第7天 - 传说奖励
            dailyRewards.Add(new DailyLoginReward
            {
                Day = 7,
                GoldRequired = 0,
                ItemRewards = new List<string> { "LegendaryChest", "GoldCoin" },
                ItemCounts = new List<int> { 1, 1000 },
                ExpReward = 1000
            });
        }

        /// <summary>
        /// 加载玩家数据
        /// </summary>
        public void LoadPlayerData()
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem == null) return;

            // 从存档加载数据
            if (saveSystem.Data.ContainsKey("daily_login"))
            {
                var data = saveSystem.Data["daily_login"] as Dictionary<string, object>;
                if (data != null)
                {
                    if (data.ContainsKey("last_login_date") && data["last_login_date"] != null)
                        playerData.LastLoginDate = DateTime.Parse(data["last_login_date"].ToString());
                    if (data.ContainsKey("total_login_days"))
                        playerData.TotalLoginDays = Convert.ToInt32(data["total_login_days"]);
                    if (data.ContainsKey("consecutive_login_days"))
                        playerData.ConsecutiveLoginDays = Convert.ToInt32(data["consecutive_login_days"]);
                    
                    playerData.ClaimedDays = new List<int>();
                    if (data.ContainsKey("claimed_days") && data["claimed_days"] != null)
                    {
                        var claimed = data["claimed_days"] as List<object>;
                        if (claimed != null)
                        {
                            foreach (var item in claimed)
                            {
                                playerData.ClaimedDays.Add(Convert.ToInt32(item));
                            }
                        }
                    }

                    if (data.ContainsKey("last_claim_date") && data["last_claim_date"] != null)
                        playerData.LastClaimDate = DateTime.Parse(data["last_claim_date"].ToString());
                }
            }
            else
            {
                playerData = new PlayerDailyLoginData();
            }
        }

        /// <summary>
        /// 保存玩家数据
        /// </summary>
        public void SavePlayerData()
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem == null) return;

            var data = new Dictionary<string, object>
            {
                { "last_login_date", playerData.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss") },
                { "total_login_days", playerData.TotalLoginDays },
                { "consecutive_login_days", playerData.ConsecutiveLoginDays },
                { "claimed_days", playerData.ClaimedDays },
                { "last_claim_date", playerData.LastClaimDate.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            saveSystem.Data["daily_login"] = data;
        }

        /// <summary>
        /// 检查登录状态
        /// </summary>
        private void CheckLoginStatus()
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;

            // 首次登录
            if (playerData.LastLoginDate == DateTime.MinValue)
            {
                playerData.TotalLoginDays = 1;
                playerData.ConsecutiveLoginDays = 1;
                playerData.LastLoginDate = now;
                SavePlayerData();
                EmitSignal(SignalName.LoginDaysUpdated, playerData.ConsecutiveLoginDays, playerData.TotalLoginDays);
                return;
            }

            DateTime lastLoginDay = playerData.LastLoginDate.Date;

            if (lastLoginDay == today)
            {
                // 今天已经登录过，不处理
                EmitSignal(SignalName.LoginDaysUpdated, playerData.ConsecutiveLoginDays, playerData.TotalLoginDays);
            }
            else if (lastLoginDay == today.AddDays(-1))
            {
                // 昨天登录过，连续登录
                playerData.ConsecutiveLoginDays++;
                playerData.TotalLoginDays++;
                playerData.LastLoginDate = now;
                SavePlayerData();
                EmitSignal(SignalName.LoginDaysUpdated, playerData.ConsecutiveLoginDays, playerData.TotalLoginDays);
                
                // 检查是否有新的一天可以领取
                CheckNewDayAvailable();
            }
            else
            {
                // 断开连接，重置连续登录
                playerData.ConsecutiveLoginDays = 1;
                playerData.TotalLoginDays++;
                playerData.LastLoginDate = now;
                // 重置已领取记录（新月或断开连接后）
                playerData.ClaimedDays.Clear();
                SavePlayerData();
                EmitSignal(SignalName.LoginDaysUpdated, playerData.ConsecutiveLoginDays, playerData.TotalLoginDays);
            }
        }

        /// <summary>
        /// 检查是否有新的一天可以领取
        /// </summary>
        private void CheckNewDayAvailable()
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;
            DateTime lastClaimDay = playerData.LastClaimDate.Date;

            if (lastClaimDay != today)
            {
                EmitSignal(SignalName.NewDayAvailable);
            }
        }

        /// <summary>
        /// 获取连续登录天数
        /// </summary>
        public int GetConsecutiveLoginDays() => playerData.ConsecutiveLoginDays;

        /// <summary>
        /// 获取累计登录天数
        /// </summary>
        public int GetTotalLoginDays() => playerData.TotalLoginDays;

        /// <summary>
        /// 获取当前天的奖励
        /// </summary>
        public DailyLoginReward GetCurrentDayReward()
        {
            int day = ((playerData.ConsecutiveLoginDays - 1) % 7) + 1;
            return dailyRewards[day - 1];
        }

        /// <summary>
        /// 获取指定天的奖励
        /// </summary>
        public DailyLoginReward GetRewardForDay(int day)
        {
            if (day < 1 || day > 7) return null;
            return dailyRewards[day - 1];
        }

        /// <summary>
        /// 某天是否已领取
        /// </summary>
        public bool IsDayClaimed(int day)
        {
            return playerData.ClaimedDays.Contains(day);
        }

        /// <summary>
        /// 是否有未领取的奖励
        /// </summary>
        public bool HasUnclaimedReward()
        {
            int currentDay = ((playerData.ConsecutiveLoginDays - 1) % 7) + 1;
            
            // 检查当前天是否已领取
            if (!playerData.ClaimedDays.Contains(currentDay))
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        public bool ClaimReward(int day)
        {
            if (day < 1 || day > 7) return false;
            if (playerData.ClaimedDays.Contains(day)) return false;

            // 检查是否是当前可以领取的天
            int currentDay = ((playerData.ConsecutiveLoginDays - 1) % 7) + 1;
            if (day != currentDay) return false;

            // 发放奖励
            var reward = dailyRewards[day - 1];
            var player = GetTree().GetFirstNodeInGroup("Player") as Player;
            
            if (player != null)
            {
                // 发放物品
                for (int i = 0; i < reward.ItemRewards.Count; i++)
                {
                    ItemSystem.Instance.AddItem(reward.ItemRewards[i], reward.ItemCounts[i]);
                }

                // 发放经验
                if (reward.ExpReward > 0)
                {
                    player.AddExp(reward.ExpReward);
                }
            }

            // 记录已领取
            playerData.ClaimedDays.Add(day);
            playerData.LastClaimDate = DateTime.Now;
            SavePlayerData();

            // 发出信号
            EmitSignal(SignalName.RewardClaimed, day, reward.ItemRewards, reward.ItemCounts, 0, reward.ExpReward);

            return true;
        }

        /// <summary>
        /// 领取当前天奖励（快捷方法）
        /// </summary>
        public bool ClaimCurrentDayReward()
        {
            int currentDay = ((playerData.ConsecutiveLoginDays - 1) % 7) + 1;
            return ClaimReward(currentDay);
        }

        /// <summary>
        /// 获取本周期的天数 (1-7循环)
        /// </summary>
        public int GetCurrentCycleDay()
        {
            return ((playerData.ConsecutiveLoginDays - 1) % 7) + 1;
        }

        /// <summary>
        /// 获取所有奖励配置
        /// </summary>
        public List<DailyLoginReward> GetAllRewards()
        {
            return dailyRewards;
        }

        /// <summary>
        /// 获取下一个可领取的天数
        /// </summary>
        public int GetNextClaimableDay()
        {
            int currentDay = GetCurrentCycleDay();
            if (!playerData.ClaimedDays.Contains(currentDay))
            {
                return currentDay;
            }
            return -1; // 已全部领取
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "last_login_date", playerData.LastLoginDate.ToString("yyyy-MM-dd HH:mm:ss") },
                { "total_login_days", playerData.TotalLoginDays },
                { "consecutive_login_days", playerData.ConsecutiveLoginDays },
                { "claimed_days", new Godot.Collections.Array(playerData.ClaimedDays) },
                { "last_claim_date", playerData.LastClaimDate.ToString("yyyy-MM-dd HH:mm:ss") }
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("last_login_date") && data["last_login_date"] != null)
                playerData.LastLoginDate = DateTime.Parse(data["last_login_date"].ToString());
            if (data.ContainsKey("total_login_days"))
                playerData.TotalLoginDays = Convert.ToInt32(data["total_login_days"]);
            if (data.ContainsKey("consecutive_login_days"))
                playerData.ConsecutiveLoginDays = Convert.ToInt32(data["consecutive_login_days"]);
            
            playerData.ClaimedDays = new List<int>();
            if (data.ContainsKey("claimed_days") && data["claimed_days"] != null)
            {
                var claimed = data["claimed_days"] as Array;
                if (claimed != null)
                {
                    foreach (var item in claimed)
                    {
                        playerData.ClaimedDays.Add(Convert.ToInt32(item));
                    }
                }
            }

            if (data.ContainsKey("last_claim_date") && data["last_claim_date"] != null)
                playerData.LastClaimDate = DateTime.Parse(data["last_claim_date"].ToString());
        }
    }
}
