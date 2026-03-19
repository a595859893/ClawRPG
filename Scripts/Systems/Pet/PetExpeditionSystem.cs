using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSystems
{
    public class PetExpeditionSystem : BaseSystem
    {
        private static PetExpeditionSystem _instance;
        public static PetExpeditionSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("PetExpeditionSystem not initialized!");
                }
                return _instance;
            }
        }
        
        public PlayerExpeditionData PlayerData { get; private set; } = new PlayerExpeditionData();
        
        // 信号系统
        public delegate void ExpeditionStartedDelegate(string expeditionId, string zoneId);
        public delegate void ExpeditionCompletedDelegate(ExpeditionResult result);
        public delegate void ExpeditionFailedDelegate(string expeditionId, string reason);
        
        public event ExpeditionStartedDelegate OnExpeditionStarted;
        public event ExpeditionCompletedDelegate OnExpeditionCompleted;
        public event ExpeditionFailedDelegate OnExpeditionFailed;
        
        private Random _random = new Random();
        private PetManager _petManager;
        private InventoryManager _inventoryManager;
        
        public override void _Ready()
        {
            _instance = this;
            _petManager = GetNode<PetManager>("/root/Main/PetManager");
            _inventoryManager = GetNode<InventoryManager>("/root/Main/InventoryManager");
            
            // 设置进程更新
            SetProcess(true);
            GD.Print("Pet Expedition System initialized");
        }
        
        public override void _Process(float delta)
        {
            // 检查远征是否完成
            CheckExpeditions();
        }
        
        public void Initialize()
        {
            _instance = this;
            _petManager = GetNode<PetManager>("/root/Main/PetManager");
            _inventoryManager = GetNode<InventoryManager>("/root/Main/InventoryManager");
            SetProcess(true);
            
            // Load saved data
            var saveSystem = GetNode<SaveSystem>("/root/Main/SaveSystem");
            if (saveSystem != null)
            {
                var data = saveSystem.LoadPetExpeditionData();
                if (data != null && data.Count > 0)
                {
                    LoadSaveData(data);
                }
            }
            
            GD.Print("Pet Expedition System initialized");
        }
        
        /// <summary>
        /// 开始宠物远征
        /// </summary>
        public bool StartExpedition(string zoneId, string petId)
        {
            var zone = PetExpeditionDatabase.Instance.GetZone(zoneId);
            if (zone == null)
            {
                GD.PrintErr("Invalid zone: " + zoneId);
                return false;
            }
            
            // 检查宠物是否存在
            if (_petManager == null || !_petManager.HasPet(petId))
            {
                GD.PrintErr("Invalid pet: " + petId);
                return false;
            }
            
            // 检查宠物是否已在远征中
            if (PlayerData.ActiveExpeditions.Any(e => e.PetId == petId && !e.Completed))
            {
                GD.PrintErr("Pet already on expedition: " + petId);
                return false;
            }
            
            // 检查宠物战斗力是否足够
            var pet = _petManager.GetPet(petId);
            int petPower = pet != null ? pet.Attack + pet.Defense + pet.Health / 10 : 0;
            if (petPower < zone.RequiredPower)
            {
                GD.PrintErr("Pet power not enough for this expedition");
                return false;
            }
            
            // 创建远征
            var expedition = new ActiveExpedition
            {
                ExpeditionId = Guid.NewGuid().ToString(),
                ZoneId = zoneId,
                PetId = petId,
                StartTime = DateTime.Now,
                DurationMinutes = zone.DurationMinutes,
                Completed = false
            };
            
            PlayerData.ActiveExpeditions.Add(expedition);
            
            OnExpeditionStarted?.Invoke(expedition.ExpeditionId, zoneId);
            GD.Print($"Expedition started: {zone.Name} with pet {petId}");
            
            SaveData();
            return true;
        }
        
        /// <summary>
        /// 检查远征完成状态
        /// </summary>
        private void CheckExpeditions()
        {
            var completed = new List<ActiveExpedition>();
            
            foreach (var expedition in PlayerData.ActiveExpeditions)
            {
                if (expedition.Completed) continue;
                
                var elapsed = DateTime.Now - expedition.StartTime;
                if (elapsed.TotalMinutes >= expedition.DurationMinutes)
                {
                    // 远征完成，计算奖励
                    var result = CalculateExpeditionResult(expedition);
                    expedition.Completed = true;
                    expedition.Result = result;
                    
                    // 发放奖励
                    GrantReward(result);
                    
                    // 更新统计
                    PlayerData.TotalExpeditions++;
                    PlayerData.TotalGoldEarned += result.GoldEarned;
                    PlayerData.TotalExpEarned += result.ExpEarned;
                    
                    if (!PlayerData.ZoneCompletions.ContainsKey(expedition.ZoneId))
                        PlayerData.ZoneCompletions[expedition.ZoneId] = 0;
                    PlayerData.ZoneCompletions[expedition.ZoneId]++;
                    
                    // 添加到历史
                    PlayerData.History.Insert(0, result);
                    if (PlayerData.History.Count > 50)
                        PlayerData.History.RemoveAt(PlayerData.History.Count - 1);
                    
                    OnExpeditionCompleted?.Invoke(result);
                    completed.Add(expedition);
                    
                    GD.Print($"Expedition completed: {result.ZoneId}, Gold: {result.GoldEarned}, Exp: {result.ExpEarned}");
                }
            }
            
            if (completed.Count > 0)
                SaveData();
        }
        
        /// <summary>
        /// 计算远征结果
        /// </summary>
        private ExpeditionResult CalculateExpeditionResult(ActiveExpedition expedition)
        {
            var zone = PetExpeditionDatabase.Instance.GetZone(expedition.ZoneId);
            var pet = _petManager?.GetPet(expedition.PetId);
            
            int petPower = pet != null ? pet.Attack + pet.Defense + pet.Health / 10 : 0;
            
            // 计算成功率 (基础 70% + 功率加成)
            float successChance = 0.7f + (float)petPower / (zone.RequiredPower * 2);
            successChance = Mathf.Clamp(successChance, 0.3f, 0.95f);
            
            bool success = _random.NextDouble() < successChance;
            
            var result = new ExpeditionResult
            {
                ZoneId = expedition.ZoneId,
                PetId = expedition.PetId,
                Success = success
            };
            
            if (success)
            {
                // 计算金币奖励 (80%-120% 基础范围)
                float goldMultiplier = 0.8f + (float)_random.NextDouble() * 0.4f;
                result.GoldEarned = (int)(zone.MinGoldReward + (zone.MaxGoldReward - zone.MinGoldReward) * goldMultiplier);
                
                // 计算经验奖励
                float expMultiplier = 0.8f + (float)_random.NextDouble() * 0.4f;
                result.ExpEarned = (int)(zone.MinExpReward + (zone.MaxExpReward - zone.MinExpReward) * expMultiplier);
                
                // 物品掉落
                if (_random.NextDouble() < zone.ItemDropChance)
                {
                    string itemId = zone.PossibleItems[_random.Next(zone.PossibleItems.Count)];
                    result.ItemsEarned.Add(itemId);
                }
            }
            else
            {
                // 失败也有少量安慰奖励
                result.GoldEarned = zone.MinGoldReward / 5;
                result.ExpEarned = zone.MinExpReward / 5;
            }
            
            return result;
        }
        
        /// <summary>
        /// 发放远征奖励
        /// </summary>
        private void GrantReward(ExpeditionResult result)
        {
            var player = GetNode<Player>("/root/Main/Player");
            if (player == null) return;
            
            // 发放金币
            player.Gold += result.GoldEarned;
            
            // 发放经验
            player.AddExperience(result.ExpEarned);
            
            // 发放物品
            foreach (var itemId in result.ItemsEarned)
            {
                if (_inventoryManager != null)
                {
                    _inventoryManager.AddItem(itemId, 1);
                }
            }
        }
        
        /// <summary>
        /// 取消远征
        /// </summary>
        public bool CancelExpedition(string expeditionId)
        {
            var expedition = PlayerData.ActiveExpeditions.FirstOrDefault(e => e.ExpeditionId == expeditionId);
            if (expedition == null || expedition.Completed)
            {
                return false;
            }
            
            // 计算返还比例
            var elapsed = DateTime.Now - expedition.StartTime;
            float returnRatio = 1.0f - (float)(elapsed.TotalMinutes / expedition.DurationMinutes);
            returnRatio = Mathf.Max(returnRatio, 0.1f);
            
            var zone = PetExpeditionDatabase.Instance.GetZone(expedition.ZoneId);
            
            // 返还部分金币
            var player = GetNode<Player>("/root/Main/Player");
            if (player != null)
            {
                int returnGold = (int)(zone.MinGoldReward / 5 * returnRatio);
                player.Gold += returnGold;
            }
            
            PlayerData.ActiveExpeditions.Remove(expedition);
            SaveData();
            
            GD.Print($"Expedition cancelled: {expeditionId}");
            return true;
        }
        
        /// <summary>
        /// 获取活跃远征数量
        /// </summary>
        public int GetActiveExpeditionCount()
        {
            return PlayerData.ActiveExpeditions.Count(e => !e.Completed);
        }
        
        /// <summary>
        /// 获取宠物远征状态
        /// </summary>
        public ActiveExpedition GetPetExpedition(string petId)
        {
            return PlayerData.ActiveExpeditions.FirstOrDefault(e => e.PetId == petId && !e.Completed);
        }
        
        /// <summary>
        /// 获取远征进度 (0.0 - 1.0)
        /// </summary>
        public float GetExpeditionProgress(string expeditionId)
        {
            var expedition = PlayerData.ActiveExpeditions.FirstOrDefault(e => e.ExpeditionId == expeditionId);
            if (expedition == null || expedition.Completed)
                return 1.0f;
            
            var elapsed = DateTime.Now - expedition.StartTime;
            return Mathf.Clamp((float)(elapsed.TotalMinutes / expedition.DurationMinutes), 0.0f, 1.0f);
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "total_expeditions", PlayerData.TotalExpeditions },
                { "total_gold_earned", PlayerData.TotalGoldEarned },
                { "total_exp_earned", PlayerData.TotalExpEarned },
                { "active_count", GetActiveExpeditionCount() },
                { "history_count", PlayerData.History.Count }
            };
        }
        
        /// <summary>
        /// 存档
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            var data = new Dictionary<string, object>();
            
            var activeList = new List<Dictionary<string, object>>();
            foreach (var exp in PlayerData.ActiveExpeditions)
            {
                activeList.Add(new Dictionary<string, object>
                {
                    { "expedition_id", exp.ExpeditionId },
                    { "zone_id", exp.ZoneId },
                    { "pet_id", exp.PetId },
                    { "start_time", exp.StartTime.ToString("o") },
                    { "duration_minutes", exp.DurationMinutes },
                    { "completed", exp.Completed }
                });
            }
            data["active_expeditions"] = activeList;
            
            var historyList = new List<Dictionary<string, object>>();
            foreach (var result in PlayerData.History)
            {
                historyList.Add(new Dictionary<string, object>
                {
                    { "zone_id", result.ZoneId },
                    { "success", result.Success },
                    { "gold_earned", result.GoldEarned },
                    { "exp_earned", result.ExpEarned },
                    { "items_earned", string.Join(",", result.ItemsEarned) },
                    { "pet_id", result.PetId }
                });
            }
            data["history"] = historyList;
            
            data["total_expeditions"] = PlayerData.TotalExpeditions;
            data["total_gold_earned"] = PlayerData.TotalGoldEarned;
            data["total_exp_earned"] = PlayerData.TotalExpEarned;
            
            var zoneCompletions = new Dictionary<string, int>();
            foreach (var kvp in PlayerData.ZoneCompletions)
            {
                zoneCompletions[kvp.Key] = kvp.Value;
            }
            data["zone_completions"] = zoneCompletions;
            
            return data;
        }
        
        /// <summary>
        /// 读档
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            PlayerData = new PlayerExpeditionData();
            
            if (data.ContainsKey("total_expeditions"))
                PlayerData.TotalExpeditions = Convert.ToInt32(data["total_expeditions"]);
            if (data.ContainsKey("total_gold_earned"))
                PlayerData.TotalGoldEarned = Convert.ToInt32(data["total_gold_earned"]);
            if (data.ContainsKey("total_exp_earned"))
                PlayerData.TotalExpEarned = Convert.ToInt32(data["total_exp_earned"]);
            
            if (data.ContainsKey("active_expeditions"))
            {
                foreach (Dictionary<string, object> expData in (Array)data["active_expeditions"])
                {
                    var exp = new ActiveExpedition
                    {
                        ExpeditionId = expData["expedition_id"].ToString(),
                        ZoneId = expData["zone_id"].ToString(),
                        PetId = expData["pet_id"].ToString(),
                        StartTime = DateTime.Parse(expData["start_time"].ToString()),
                        DurationMinutes = Convert.ToInt32(expData["duration_minutes"]),
                        Completed = Convert.ToBoolean(expData["completed"])
                    };
                    PlayerData.ActiveExpeditions.Add(exp);
                }
            }
            
            if (data.ContainsKey("history"))
            {
                foreach (Dictionary<string, object> histData in (Array)data["history"])
                {
                    var result = new ExpeditionResult
                    {
                        ZoneId = histData["zone_id"].ToString(),
                        Success = Convert.ToBoolean(histData["success"]),
                        GoldEarned = Convert.ToInt32(histData["gold_earned"]),
                        ExpEarned = Convert.ToInt32(histData["exp_earned"]),
                        PetId = histData["pet_id"].ToString()
                    };
                    
                    string itemsStr = histData["items_earned"].ToString();
                    if (!string.IsNullOrEmpty(itemsStr))
                        result.ItemsEarned = itemsStr.Split(',').ToList();
                    
                    PlayerData.History.Add(result);
                }
            }
            
            if (data.ContainsKey("zone_completions"))
            {
                foreach (var kvp in (Dictionary<string, object>)data["zone_completions"])
                {
                    PlayerData.ZoneCompletions[kvp.Key] = Convert.ToInt32(kvp.Value);
                }
            }
            
            GD.Print("Pet Expedition data loaded");
        }
        
        private void SaveData()
        {
            var saveSystem = GetNode<SaveSystem>("/root/Main/SaveSystem");
            if (saveSystem != null)
            {
                saveSystem.SavePetExpeditionData(GetSaveData());
            }
        }

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            if (PlayerData == null) return data;

            // 保存活跃远征
            var activeExpeditions = new List<Dictionary<string, Variant>>();
            foreach (var exp in PlayerData.ActiveExpeditions)
            {
                activeExpeditions.Add(new Dictionary<string, Variant>
                {
                    ["expedition_id"] = exp.ExpeditionId ?? "",
                    ["zone_id"] = exp.ZoneId ?? "",
                    ["pet_id"] = exp.PetId ?? "",
                    ["start_time"] = exp.StartTime.ToString("o"),
                    ["duration_minutes"] = exp.DurationMinutes,
                    ["completed"] = exp.Completed
                });
            }
            data["active_expeditions"] = activeExpeditions;

            // 保存远征历史
            var history = new List<Dictionary<string, Variant>>();
            foreach (var result in PlayerData.History)
            {
                var resultDict = new Dictionary<string, Variant>
                {
                    ["zone_id"] = result.ZoneId ?? "",
                    ["success"] = result.Success,
                    ["gold_earned"] = result.GoldEarned,
                    ["exp_earned"] = result.ExpEarned,
                    ["pet_id"] = result.PetId ?? ""
                };
                if (result.ItemsEarned != null)
                    resultDict["items_earned"] = new List<string>(result.ItemsEarned);
                history.Add(resultDict);
            }
            data["history"] = history;

            // 保存统计数据
            data["total_expeditions"] = PlayerData.TotalExpeditions;
            data["total_gold_earned"] = PlayerData.TotalGoldEarned;
            data["total_exp_earned"] = PlayerData.TotalExpEarned;

            // 保存区域完成次数
            var zoneCompletions = new Dictionary<string, int>();
            foreach (var kvp in PlayerData.ZoneCompletions)
            {
                zoneCompletions[kvp.Key] = kvp.Value;
            }
            data["zone_completions"] = zoneCompletions;

            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null || PlayerData == null) return;

            // 加载活跃远征
            if (data.TryGetValue("active_expeditions", out var activeData))
            {
                PlayerData.ActiveExpeditions = new List<ActiveExpedition>();
                var activeList = (List<Variant>)activeData;
                foreach (var expVar in activeList)
                {
                    var expDict = (Dictionary<string, Variant>)expVar;
                    var exp = new ActiveExpedition();

                    if (expDict.TryGetValue("expedition_id", out var expId))
                        exp.ExpeditionId = (string)expId;
                    if (expDict.TryGetValue("zone_id", out var zoneId))
                        exp.ZoneId = (string)zoneId;
                    if (expDict.TryGetValue("pet_id", out var petId))
                        exp.PetId = (string)petId;
                    if (expDict.TryGetValue("start_time", out var startTime) && DateTime.TryParse((string)startTime, out var parsed))
                        exp.StartTime = parsed;
                    if (expDict.TryGetValue("duration_minutes", out var duration))
                        exp.DurationMinutes = (int)duration;
                    if (expDict.TryGetValue("completed", out var completed))
                        exp.Completed = (bool)completed;

                    PlayerData.ActiveExpeditions.Add(exp);
                }
            }

            // 加载远征历史
            if (data.TryGetValue("history", out var historyData))
            {
                PlayerData.History = new List<ExpeditionResult>();
                var historyList = (List<Variant>)historyData;
                foreach (var resultVar in historyList)
                {
                    var resultDict = (Dictionary<string, Variant>)resultVar;
                    var result = new ExpeditionResult();

                    if (resultDict.TryGetValue("zone_id", out var zoneId))
                        result.ZoneId = (string)zoneId;
                    if (resultDict.TryGetValue("success", out var success))
                        result.Success = (bool)success;
                    if (resultDict.TryGetValue("gold_earned", out var gold))
                        result.GoldEarned = (int)gold;
                    if (resultDict.TryGetValue("exp_earned", out var exp))
                        result.ExpEarned = (int)exp;
                    if (resultDict.TryGetValue("pet_id", out var petId))
                        result.PetId = (string)petId;
                    if (resultDict.TryGetValue("items_earned", out var items))
                        result.ItemsEarned = new List<string>((List<string>)items);

                    PlayerData.History.Add(result);
                }
            }

            // 加载统计数据
            if (data.TryGetValue("total_expeditions", out var totalExp))
                PlayerData.TotalExpeditions = (int)totalExp;
            if (data.TryGetValue("total_gold_earned", out var totalGold))
                PlayerData.TotalGoldEarned = (int)totalGold;
            if (data.TryGetValue("total_exp_earned", out var totalExpEarned))
                PlayerData.TotalExpEarned = (int)totalExpEarned;

            // 加载区域完成次数
            if (data.TryGetValue("zone_completions", out var zoneData))
            {
                PlayerData.ZoneCompletions = new Dictionary<string, int>();
                var zoneDict = (Dictionary<string, Variant>)zoneData;
                foreach (var kvp in zoneDict)
                {
                    PlayerData.ZoneCompletions[kvp.Key] = (int)kvp.Value;
                }
            }
        }
    }
