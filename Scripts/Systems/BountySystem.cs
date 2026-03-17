using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 赏金任务数据类
    /// </summary>
    public class Bounty
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public BountyType Type { get; set; }
        public BountyDifficulty Difficulty { get; set; }
        public int TargetId { get; set; }         // 敌人ID或物品ID
        public int TargetCount { get; set; }      // 需要击杀/收集的数量
        public int CurrentProgress { get; set; }  // 当前进度
        public int GoldReward { get; set; }
        public int XPReward { get; set; }
        public int ItemRewardId { get; set; }     // 奖励物品ID (0=无)
        public bool IsCompleted { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public float ProgressPercent => TargetCount > 0 ? (float)CurrentProgress / TargetCount : 0;
    }

    public enum BountyType
    {
        KillEnemy,        // 击杀敌人
        CollectItem,      // 收集物品
        BossChallenge,    // Boss挑战
        Survival,         // 生存挑战
        ComboChallenge    // 连击挑战
    }

    public enum BountyDifficulty
    {
        Easy,       // 简单
        Normal,     // 普通
        Hard,       // 困难
        Elite,      // 精英
        Legendary   // 传奇
    }

    /// <summary>
    /// 赏金任务数据库
    /// </summary>
    public class BountyDatabase
    {
        private static BountyDatabase _instance;
        public static BountyDatabase Instance => _instance ??= new BountyDatabase();

        private List<Bounty> _bountyTemplates;

        public BountyDatabase()
        {
            _bountyTemplates = new List<Bounty>();
            InitializeTemplates();
        }

        private void InitializeTemplates()
        {
            // 击杀敌人赏金
            _bountyTemplates.Add(new Bounty { Id = 1, Title = "哥布林清除", Description = "击杀10只哥布林", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Easy, TargetId = 1, TargetCount = 10, GoldReward = 100, XPReward = 50 });
            _bountyTemplates.Add(new Bounty { Id = 2, Title = "狼群威胁", Description = "击杀15只森林狼", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Easy, TargetId = 2, TargetCount = 15, GoldReward = 150, XPReward = 75 });
            _bountyTemplates.Add(new Bounty { Id = 3, Title = "骷髅大军", Description = "击杀20只骷髅", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Normal, TargetId = 6, TargetCount = 20, GoldReward = 300, XPReward = 150 });
            _bountyTemplates.Add(new Bounty { Id = 4, Title = "蜘蛛恐惧", Description = "击杀25只洞穴蜘蛛", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Normal, TargetId = 4, TargetCount = 25, GoldReward = 350, XPReward = 175 });
            _bountyTemplates.Add(new Bounty { Id = 5, Title = "火焰元素", Description = "击杀12只火焰元素", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Hard, TargetId = 8, TargetCount = 12, GoldReward = 500, XPReward = 300 });
            _bountyTemplates.Add(new Bounty { Id = 6, Title = "暗影杀手", Description = "击杀15只暗影精灵", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Hard, TargetId = 10, TargetCount = 15, GoldReward = 600, XPReward = 350 });
            _bountyTemplates.Add(new Bounty { Id = 7, Title = "冰霜亡魂", Description = "击杀18只冰霜亡灵", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Hard, TargetId = 9, TargetCount = 18, GoldReward = 650, XPReward = 375 });
            _bountyTemplates.Add(new Bounty { Id = 8, Title = "岩石傀儡", Description = "击杀8只岩石傀儡", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Elite, TargetId = 7, TargetCount = 8, GoldReward = 800, XPReward = 500 });
            _bountyTemplates.Add(new Bounty { Id = 9, Title = "精英怪物", Description = "击杀5只精英怪物", Type = BountyType.KillEnemy, Difficulty = BountyDifficulty.Elite, TargetId = 11, TargetCount = 5, GoldReward = 1000, XPReward = 600 });

            // Boss挑战赏金
            _bountyTemplates.Add(new Bounty { Id = 10, Title = "树精讨伐", Description = "击败古老树精", Type = BountyType.BossChallenge, Difficulty = BountyDifficulty.Normal, TargetId = 101, TargetCount = 1, GoldReward = 500, XPReward = 300, ItemRewardId = 521 });
            _bountyTemplates.Add(new Bounty { Id = 11, Title = "水晶粉碎", Description = "击败水晶傀儡", Type = BountyType.BossChallenge, Difficulty = BountyDifficulty.Hard, TargetId = 102, TargetCount = 1, GoldReward = 800, XPReward = 500, ItemRewardId = 522 });
            _bountyTemplates.Add(new Bounty { Id = 12, Title = "巨龙之战", Description = "击败炼狱巨龙", Type = BountyType.BossChallenge, Difficulty = BountyDifficulty.Elite, TargetId = 103, TargetCount = 1, GoldReward = 1500, XPReward = 1000, ItemRewardId = 523 });
            _bountyTemplates.Add(new Bounty { Id = 13, Title = "暗影刺客", Description = "击败暗夜刺客", Type = BountyType.BossChallenge, Difficulty = BountyDifficulty.Elite, TargetId = 104, TargetCount = 1, GoldReward = 1200, XPReward = 800, ItemRewardId = 524 });
            _bountyTemplates.Add(new Bounty { Id = 14, Title = "恶魔领主", Description = "击败恶魔领主", Type = BountyType.BossChallenge, Difficulty = BountyDifficulty.Legendary, TargetId = 106, TargetCount = 1, GoldReward = 3000, XPReward = 2000, ItemRewardId = 525 });

            // 收集物品赏金
            _bountyTemplates.Add(new Bounty { Id = 15, Title = "材料收集", Description = "收集10个怪物精华", Type = BountyType.CollectItem, Difficulty = BountyDifficulty.Easy, TargetId = 301, TargetCount = 10, GoldReward = 100, XPReward = 50 });
            _bountyTemplates.Add(new Bounty { Id = 16, Title = "龙鳞收集", Description = "收集5个龙鳞", Type = BountyType.CollectItem, Difficulty = BountyDifficulty.Normal, TargetId = 302, TargetCount = 5, GoldReward = 300, XPReward = 150 });
            _bountyTemplates.Add(new Bounty { Id = 17, Title = "凤凰羽毛", Description = "收集3个凤凰羽毛", Type = BountyType.CollectItem, Difficulty = BountyDifficulty.Hard, TargetId = 303, TargetCount = 3, GoldReward = 500, XPReward = 300 });
            _bountyTemplates.Add(new Bounty { Id = 18, Title = "暗影水晶", Description = "收集8个暗影水晶", Type = BountyType.CollectItem, Difficulty = BountyDifficulty.Normal, TargetId = 304, TargetCount = 8, GoldReward = 400, XPReward = 200 });
            _bountyTemplates.Add(new Bounty { Id = 19, Title = "神圣宝珠", Description = "收集5个神圣宝珠", Type = BountyType.CollectItem, Difficulty = BountyDifficulty.Hard, TargetId = 305, TargetCount = 5, GoldReward = 600, XPReward = 350 });

            // 生存挑战赏金
            _bountyTemplates.Add(new Bounty { Id = 20, Title = "生存考验", Description = "在敌人攻击下生存60秒", Type = BountyType.Survival, Difficulty = BountyDifficulty.Easy, TargetId = 0, TargetCount = 60, GoldReward = 200, XPReward = 100 });
            _bountyTemplates.Add(new Bounty { Id = 21, Title = "坚持到底", Description = "在敌人攻击下生存120秒", Type = BountyType.Survival, Difficulty = BountyDifficulty.Normal, TargetId = 0, TargetCount = 120, GoldReward = 400, XPReward = 200 });
            _bountyTemplates.Add(new Bounty { Id = 22, Title = "生存大师", Description = "在敌人攻击下生存180秒", Type = BountyType.Survival, Difficulty = BountyDifficulty.Hard, TargetId = 0, TargetCount = 180, GoldReward = 700, XPReward = 400 });

            // 连击挑战赏金
            _bountyTemplates.Add(new Bounty { Id = 23, Title = "连击新手", Description = "达成20连击", Type = BountyType.ComboChallenge, Difficulty = BountyDifficulty.Easy, TargetId = 0, TargetCount = 20, GoldReward = 150, XPReward = 75 });
            _bountyTemplates.Add(new Bounty { Id = 24, Title = "连击达人", Description = "达成50连击", Type = BountyType.ComboChallenge, Difficulty = BountyDifficulty.Normal, TargetId = 0, TargetCount = 50, GoldReward = 350, XPReward = 175 });
            _bountyTemplates.Add(new Bounty { Id = 25, Title = "连击王者", Description = "达成100连击", Type = BountyType.ComboChallenge, Difficulty = BountyDifficulty.Hard, TargetId = 0, TargetCount = 100, GoldReward = 800, XPReward = 450 });
        }

        public List<Bounty> GetBountiesByType(BountyType type)
        {
            return _bountyTemplates.Where(b => b.Type == type).ToList();
        }

        public List<Bounty> GetBountiesByDifficulty(BountyDifficulty difficulty)
        {
            return _bountyTemplates.Where(b => b.Difficulty == difficulty).ToList();
        }

        public Bounty GetRandomBounty(BountyType type, BountyDifficulty difficulty)
        {
            var bounties = _bountyTemplates.Where(b => b.Type == type && b.Difficulty == difficulty).ToList();
            if (bounties.Count == 0) return null;
            return bounties[GD.RandI() % bounties.Count];
        }

        public List<Bounty> GetDailyBounties(int count = 3)
        {
            var result = new List<Bounty>();
            var random = new Random();
            
            // 每天生成不同类型的赏金任务
            var types = Enum.GetValues(typeof(BountyType)).Cast<BountyType>().ToList();
            var difficulties = Enum.GetValues(typeof(BountyDifficulty)).Cast<BountyDifficulty>().ToList();
            
            for (int i = 0; i < count; i++)
            {
                var type = types[random.Next(types.Count)];
                var difficulty = difficulties[random.Next(difficulties.Count)];
                var bounty = GetRandomBounty(type, difficulty);
                if (bounty != null && !result.Any(r => r.Id == bounty.Id))
                {
                    result.Add(bounty);
                }
            }
            
            return result;
        }

        public Bounty CreateBountyInstance(Bounty template)
        {
            return new Bounty
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                Type = template.Type,
                Difficulty = template.Difficulty,
                TargetId = template.TargetId,
                TargetCount = template.TargetCount,
                CurrentProgress = 0,
                GoldReward = template.GoldReward,
                XPReward = template.XPReward,
                ItemRewardId = template.ItemRewardId,
                IsCompleted = false,
                IsClaimed = false,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(24) // 24小时有效期
            };
        }

        public string GetDifficultyColor(BountyDifficulty difficulty)
        {
            return difficulty switch
            {
                BountyDifficulty.Easy => "#808080",       // 灰色
                BountyDifficulty.Normal => "#00FF00",     // 绿色
                BountyDifficulty.Hard => "#0000FF",       // 蓝色
                BountyDifficulty.Elite => "#800080",      // 紫色
                BountyDifficulty.Legendary => "#FFA500", // 橙色
                _ => "#FFFFFF"
            };
        }
    }

    /// <summary>
    /// 赏金任务管理器
    /// </summary>
    public class BountyManager : BaseSystem
    {
        private static BountyManager _instance;
        public static BountyManager Instance => _instance ??= new BountyManager();

        public List<Bounty> ActiveBounties { get; private set; } = new List<Bounty>();
        
        // 信号系统
        public Signal1<Bounty> OnBountyAccepted { get; } = new Signal1<Bounty>();
        public Signal1<Bounty> OnBountyProgressUpdated { get; } = new Signal1<Bounty>();
        public Signal1<Bounty> OnBountyCompleted { get; } = new Signal1<Bounty>();
        public Signal1<Bounty> OnBountyClaimed { get; } = new Signal1<Bounty>();
        public Signal1<Bounty> OnBountyExpired { get; } = new Signal1<Bounty>();
        public Signal OnBountiesRefreshed { get; } = new Signal();

        private int _maxActiveBounties = 3;
        private DateTime _lastRefreshTime;

        public void Initialize()
        {
            ActiveBounties = new List<Bounty>();
            _lastRefreshTime = DateTime.Now;
            GenerateDailyBounties();
        }

        public void GenerateDailyBounties()
        {
            if (ActiveBounties.Count > 0 && (DateTime.Now - _lastRefreshTime).TotalHours < 24)
            {
                return; // 一天只刷新一次
            }

            ActiveBounties.Clear();
            var dailyBounties = BountyDatabase.Instance.GetDailyBounties(_maxActiveBounties);
            
            foreach (var template in dailyBounties)
            {
                var bounty = BountyDatabase.Instance.CreateBountyInstance(template);
                ActiveBounties.Add(bounty);
            }
            
            _lastRefreshTime = DateTime.Now;
            OnBountiesRefreshed.Emit();
        }

        public bool AcceptBounty(Bounty bounty)
        {
            if (ActiveBounties.Count >= _maxActiveBounties)
            {
                return false; // 已满
            }

            if (ActiveBounties.Any(b => b.Id == bounty.Id))
            {
                return false; // 已存在
            }

            ActiveBounties.Add(bounty);
            OnBountyAccepted.Emit(bounty);
            return true;
        }

        public void UpdateKillProgress(int enemyId, int count = 1)
        {
            foreach (var bounty in ActiveBounties)
            {
                if (bounty.Type == BountyType.KillEnemy && !bounty.IsCompleted && bounty.TargetId == enemyId)
                {
                    bounty.CurrentProgress = Mathf.Min(bounty.CurrentProgress + count, bounty.TargetCount);
                    OnBountyProgressUpdated.Emit(bounty);
                    
                    if (bounty.CurrentProgress >= bounty.TargetCount && !bounty.IsCompleted)
                    {
                        bounty.IsCompleted = true;
                        OnBountyCompleted.Emit(bounty);
                    }
                }
            }
        }

        public void UpdateBossKillProgress(int bossId)
        {
            foreach (var bounty in ActiveBounties)
            {
                if (bounty.Type == BountyType.BossChallenge && !bounty.IsCompleted && bounty.TargetId == bossId)
                {
                    bounty.CurrentProgress = 1;
                    bounty.IsCompleted = true;
                    OnBountyCompleted.Emit(bounty);
                }
            }
        }

        public void UpdateCollectProgress(int itemId, int count = 1)
        {
            foreach (var bounty in ActiveBounties)
            {
                if (bounty.Type == BountyType.CollectItem && !bounty.IsCompleted && bounty.TargetId == itemId)
                {
                    bounty.CurrentProgress = Mathf.Min(bounty.CurrentProgress + count, bounty.TargetCount);
                    OnBountyProgressUpdated.Emit(bounty);
                    
                    if (bounty.CurrentProgress >= bounty.TargetCount && !bounty.IsCompleted)
                    {
                        bounty.IsCompleted = true;
                        OnBountyCompleted.Emit(bounty);
                    }
                }
            }
        }

        public void UpdateSurvivalProgress(int seconds)
        {
            foreach (var bounty in ActiveBounties)
            {
                if (bounty.Type == BountyType.Survival && !bounty.IsCompleted)
                {
                    bounty.CurrentProgress = Mathf.Min(bounty.CurrentProgress + seconds, bounty.TargetCount);
                    OnBountyProgressUpdated.Emit(bounty);
                    
                    if (bounty.CurrentProgress >= bounty.TargetCount && !bounty.IsCompleted)
                    {
                        bounty.IsCompleted = true;
                        OnBountyCompleted.Emit(bounty);
                    }
                }
            }
        }

        public void UpdateComboProgress(int comboCount)
        {
            foreach (var bounty in ActiveBounties)
            {
                if (bounty.Type == BountyType.ComboChallenge && !bounty.IsCompleted)
                {
                    if (comboCount >= bounty.TargetCount && bounty.CurrentProgress < bounty.TargetCount)
                    {
                        bounty.CurrentProgress = comboCount;
                        bounty.IsCompleted = true;
                        OnBountyCompleted.Emit(bounty);
                    }
                }
            }
        }

        public bool ClaimBountyReward(Bounty bounty)
        {
            if (!bounty.IsCompleted || bounty.IsClaimed)
            {
                return false;
            }

            var player = GetPlayer();
            if (player == null) return false;

            // 发放奖励
            player.AddGold(bounty.GoldReward);
            player.AddExperience(bounty.XPReward);
            
            if (bounty.ItemRewardId > 0)
            {
                // 添加物品到背包
                var itemSystem = ItemSystem.Instance;
                if (itemSystem != null)
                {
                    itemSystem.AddItem(bounty.ItemRewardId, 1);
                }
            }

            bounty.IsClaimed = true;
            ActiveBounties.Remove(bounty);
            OnBountyClaimed.Emit(bounty);
            
            return true;
        }

        public void CheckExpiredBounties()
        {
            var expiredBounties = ActiveBounties.Where(b => DateTime.Now > b.ExpiresAt).ToList();
            foreach (var bounty in expiredBounties)
            {
                ActiveBounties.Remove(bounty);
                OnBountyExpired.Emit(bounty);
            }
        }

        private Player GetPlayer()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null) return null;
            return tree.Root.GetNode<Player>("Main/Player");
        }

        // 存档支持
        public Dictionary<string, object> Serialize()
        {
            var data = new Dictionary<string, object>();
            data["activeBounties"] = ActiveBounties.Select(b => new Dictionary<string, object>
            {
                ["id"] = b.Id,
                ["title"] = b.Title,
                ["description"] = b.Description,
                ["type"] = (int)b.Type,
                ["difficulty"] = (int)b.Difficulty,
                ["targetId"] = b.TargetId,
                ["targetCount"] = b.TargetCount,
                ["currentProgress"] = b.CurrentProgress,
                ["goldReward"] = b.GoldReward,
                ["xpReward"] = b.XPReward,
                ["itemRewardId"] = b.ItemRewardId,
                ["isCompleted"] = b.IsCompleted,
                ["isClaimed"] = b.IsClaimed,
                ["createdAt"] = b.CreatedAt.ToString("o"),
                ["expiresAt"] = b.ExpiresAt.ToString("o")
            }).ToList();
            data["lastRefreshTime"] = _lastRefreshTime.ToString("o");
            return data;
        }

        public void Deserialize(Dictionary<string, object> data)
        {
            if (data == null) return;

            ActiveBounties.Clear();
            if (data.TryGetValue("activeBounties", out var bountiesObj))
            {
                var bounties = bountiesObj as System.Collections.IEnumerable;
                foreach (var bObj in bounties)
                {
                    var bData = bObj as Dictionary<string, object>;
                    if (bData == null) continue;

                    var bounty = new Bounty
                    {
                        Id = Convert.ToInt32(bData["id"]),
                        Title = bData["title"].ToString(),
                        Description = bData["description"].ToString(),
                        Type = (BountyType)Convert.ToInt32(bData["type"]),
                        Difficulty = (BountyDifficulty)Convert.ToInt32(bData["difficulty"]),
                        TargetId = Convert.ToInt32(bData["targetId"]),
                        TargetCount = Convert.ToInt32(bData["targetCount"]),
                        CurrentProgress = Convert.ToInt32(bData["currentProgress"]),
                        GoldReward = Convert.ToInt32(bData["goldReward"]),
                        XPReward = Convert.ToInt32(bData["xpReward"]),
                        ItemRewardId = Convert.ToInt32(bData["itemRewardId"]),
                        IsCompleted = Convert.ToBoolean(bData["isCompleted"]),
                        IsClaimed = Convert.ToBoolean(bData["isClaimed"]),
                        CreatedAt = DateTime.Parse(bData["createdAt"].ToString()),
                        ExpiresAt = DateTime.Parse(bData["expiresAt"].ToString())
                    };
                    ActiveBounties.Add(bounty);
                }
            }

            if (data.TryGetValue("lastRefreshTime", out var refreshObj))
            {
                _lastRefreshTime = DateTime.Parse(refreshObj.ToString());
            }
        }

        /// <summary>
        /// Export save data for persistence - implements BaseSystem
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            data["activeBounties"] = ActiveBounties.Select(b => new Dictionary
            {
                ["id"] = b.Id,
                ["title"] = b.Title,
                ["description"] = b.Description,
                ["type"] = (int)b.Type,
                ["difficulty"] = (int)b.Difficulty,
                ["targetId"] = b.TargetId,
                ["targetCount"] = b.TargetCount,
                ["currentProgress"] = b.CurrentProgress,
                ["goldReward"] = b.GoldReward,
                ["xpReward"] = b.XPReward,
                ["itemRewardId"] = b.ItemRewardId,
                ["isCompleted"] = b.IsCompleted,
                ["isClaimed"] = b.IsClaimed,
                ["createdAt"] = b.CreatedAt.ToString("o"),
                ["expiresAt"] = b.ExpiresAt.ToString("o")
            }).ToList();
            data["lastRefreshTime"] = _lastRefreshTime.ToString("o");
            return data;
        }

        /// <summary>
        /// Import save data from persistence - implements BaseSystem
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            ActiveBounties.Clear();
            if (data.Contains("activeBounties"))
            {
                var bountiesObj = data["activeBounties"];
                if (bountiesObj is IEnumerable bounties)
                {
                    foreach (var bObj in bounties)
                    {
                        if (bObj is not Dictionary bData) continue;

                        var bounty = new Bounty
                        {
                            Id = Convert.ToInt32(bData["id"]),
                            Title = bData["title"].ToString(),
                            Description = bData["description"].ToString(),
                            Type = (BountyType)Convert.ToInt32(bData["type"]),
                            Difficulty = (BountyDifficulty)Convert.ToInt32(bData["difficulty"]),
                            TargetId = Convert.ToInt32(bData["targetId"]),
                            TargetCount = Convert.ToInt32(bData["targetCount"]),
                            CurrentProgress = Convert.ToInt32(bData["currentProgress"]),
                            GoldReward = Convert.ToInt32(bData["goldReward"]),
                            XPReward = Convert.ToInt32(bData["xpReward"]),
                            ItemRewardId = Convert.ToInt32(bData["itemRewardId"]),
                            IsCompleted = Convert.ToBoolean(bData["isCompleted"]),
                            IsClaimed = Convert.ToBoolean(bData["isClaimed"]),
                            CreatedAt = DateTime.Parse(bData["createdAt"].ToString()),
                            ExpiresAt = DateTime.Parse(bData["expiresAt"].ToString())
                        };
                        ActiveBounties.Add(bounty);
                    }
                }
            }

            if (data.Contains("lastRefreshTime"))
            {
                _lastRefreshTime = DateTime.Parse(data["lastRefreshTime"].ToString());
            }
        }
    }
}
