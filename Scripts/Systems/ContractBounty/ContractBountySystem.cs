using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Scripts.Systems.ContractBounty
{
    /// <summary>
    /// Contract Bounty System - 委托赏金核心系统
    /// </summary>
    
    public class ContractBountySystem : BaseSystem
    {
        public static ContractBountySystem Instance { get; private set; }
        
        private ContractBountyData _data = new ContractBountyData();
        private ContractBountyDatabase _database;
        
        // 事件
        public event Action<Contract> OnContractAccepted;
        public event Action<Contract> OnContractCompleted;
        public event Action<Contract> OnContractFailed;
        public event Action<Contract, int> OnProgressUpdated;
        
        public ContractBountyData Data => _data;
        
        public override void _Ready()
        {
            Instance = this;
            _database = ContractBountyDatabase.Instance;
            InitializeContracts();
        }
        
        private void InitializeContracts()
        {
            // 生成初始可用合同
            RefreshAvailableContracts();
        }
        
        /// <summary>
        /// 刷新可用的合同列表
        /// </summary>
        public void RefreshAvailableContracts()
        {
            _data.availableContracts.Clear();
            
            // 随机生成5-8个合同
            int contractCount = (int)GD.RandRange(5, 9);
            
            for (int i = 0; i < contractCount; i++)
            {
                var template = _database.GetRandomTemplate();
                if (template != null)
                {
                    var contract = CreateContractFromTemplate(template);
                    _data.availableContracts.Add(contract);
                    
                    // 记录已发现的合同类型
                    _data.discoveredContracts.Add(template.templateId);
                }
            }
        }
        
        /// <summary>
        /// 从模板创建合同
        /// </summary>
        private Contract CreateContractFromTemplate(ContractTemplate template)
        {
            return new Contract
            {
                contractId = Guid.NewGuid().ToString("N").Substring(0, 8),
                title = template.title,
                description = template.description,
                clientName = template.clientName,
                type = template.type,
                difficulty = template.difficulty,
                status = ContractStatus.Available,
                target = new ContractTarget
                {
                    targetId = template.templateId,
                    targetName = template.targetName,
                    targetDescription = template.targetDescription,
                    requiredKills = template.requiredKills,
                    currentKills = 0,
                    level = template.baseLevel + (int)GD.RandRange(-2, 3),
                    difficulty = template.difficulty
                },
                reward = new ContractReward
                {
                    gold = template.goldReward,
                    experience = template.expReward,
                    items = new List<string>(),
                    reputation = template.reputationReward
                },
                timeLimit = template.timeLimit,
                location = template.location,
                tips = template.tips
            };
        }
        
        /// <summary>
        /// 接受合同
        /// </summary>
        public bool AcceptContract(string contractId)
        {
            var contract = _data.availableContracts.Find(c => c.contractId == contractId);
            if (contract == null || contract.status != ContractStatus.Available)
                return false;
            
            contract.status = ContractStatus.Active;
            contract.startTime = DateTime.Now;
            contract.expirationTime = contract.startTime.AddSeconds(contract.timeLimit);
            
            _data.activeContracts.Add(contract);
            _data.availableContracts.Remove(contract);
            
            OnContractAccepted?.Invoke(contract);
            return true;
        }
        
        /// <summary>
        /// 报告击杀进度
        /// </summary>
        public void ReportKill(string targetId, int killCount = 1)
        {
            foreach (var contract in _data.activeContracts)
            {
                if (contract.target.targetId == targetId && contract.status == ContractStatus.Active)
                {
                    contract.target.currentKills += killCount;
                    OnProgressUpdated?.Invoke(contract, contract.target.currentKills);
                    
                    // 检查是否完成
                    if (contract.target.currentKills >= contract.target.requiredKills)
                    {
                        CompleteContract(contract.contractId);
                    }
                }
            }
            
            // 检查超时合同
            CheckExpiredContracts();
        }
        
        /// <summary>
        /// 完成合同
        /// </summary>
        public bool CompleteContract(string contractId)
        {
            var contract = _data.activeContracts.Find(c => c.contractId == contractId);
            if (contract == null || contract.status != ContractStatus.Active)
                return false;
            
            contract.status = ContractStatus.Completed;
            
            // 移动到已完成列表
            _data.completedContracts.Add(contract);
            _data.activeContracts.Remove(contract);
            
            // 更新统计
            _data.totalCompleted++;
            _data.totalGoldEarned += contract.reward.gold;
            _data.totalExpEarned += contract.reward.experience;
            
            // 更新连续完成记录
            _data.currentStreak++;
            if (_data.currentStreak > _data.bestStreak)
                _data.bestStreak = _data.currentStreak;
            
            // 更新合同完成次数
            if (_data.contractCompletionCount.ContainsKey(contract.target.targetId))
                _data.contractCompletionCount[contract.target.targetId]++;
            else
                _data.contractCompletionCount[contract.target.targetId] = 1;
            
            // 补充新合同
            if (_data.availableContracts.Count < 5)
            {
                var template = _database.GetRandomTemplate();
                if (template != null)
                {
                    var newContract = CreateContractFromTemplate(template);
                    _data.availableContracts.Add(newContract);
                }
            }
            
            OnContractCompleted?.Invoke(contract);
            return true;
        }
        
        /// <summary>
        /// 放弃合同
        /// </summary>
        public bool AbandonContract(string contractId)
        {
            var contract = _data.activeContracts.Find(c => c.contractId == contractId);
            if (contract == null || contract.status != ContractStatus.Active)
                return false;
            
            contract.status = ContractStatus.Failed;
            
            _data.failedContracts.Add(contract);
            _data.activeContracts.Remove(contract);
            
            // 更新统计
            _data.totalFailed++;
            _data.currentStreak = 0;
            
            OnContractFailed?.Invoke(contract);
            return true;
        }
        
        /// <summary>
        /// 检查过期的合同
        /// </summary>
        public void CheckExpiredContracts()
        {
            var now = DateTime.Now;
            var expiredContracts = _data.activeContracts
                .Where(c => c.status == ContractStatus.Active && now > c.expirationTime)
                .ToList();
            
            foreach (var contract in expiredContracts)
            {
                contract.status = ContractStatus.Expired;
                _data.failedContracts.Add(contract);
                _data.activeContracts.Remove(contract);
                
                _data.totalFailed++;
                _data.currentStreak = 0;
                
                OnContractFailed?.Invoke(contract);
            }
        }
        
        /// <summary>
        /// 获取合同剩余时间（秒）
        /// </summary>
        public int GetRemainingTime(string contractId)
        {
            var contract = _data.activeContracts.Find(c => c.contractId == contractId);
            if (contract == null)
                return 0;
            
            var remaining = (contract.expirationTime - DateTime.Now).TotalSeconds;
            return Math.Max(0, (int)remaining);
        }
        
        /// <summary>
        /// 获取合同进度百分比
        /// </summary>
        public float GetProgress(string contractId)
        {
            var contract = _data.activeContracts.Find(c => c.contractId == contractId);
            if (contract == null || contract.target.requiredKills == 0)
                return 0;
            
            return (float)contract.target.currentKills / contract.target.requiredKills;
        }
        
        /// <summary>
        /// 获取合同状态描述
        /// </summary>
        public string GetContractStatusText(Contract contract)
        {
            switch (contract.status)
            {
                case ContractStatus.Available:
                    return "可接受";
                case ContractStatus.Active:
                    var remaining = GetRemainingTime(contract.contractId);
                    var minutes = remaining / 60;
                    var seconds = remaining % 60;
                    return $"进行中 ({minutes}:{seconds:D2})";
                case ContractStatus.Completed:
                    return "已完成";
                case ContractStatus.Failed:
                    return "已失败";
                case ContractStatus.Expired:
                    return "已过期";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 获取难度颜色
        /// </summary>
        public string GetDifficultyColor(ContractDifficulty difficulty)
        {
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    return "#4CAF50"; // 绿色
                case ContractDifficulty.Medium:
                    return "#FFC107"; // 黄色
                case ContractDifficulty.Hard:
                    return "#FF9800"; // 橙色
                case ContractDifficulty.Legendary:
                    return "#F44336"; // 红色
                default:
                    return "#FFFFFF";
            }
        }
        
        /// <summary>
        /// 获取难度名称
        /// </summary>
        public string GetDifficultyName(ContractDifficulty difficulty)
        {
            switch (difficulty)
            {
                case ContractDifficulty.Easy:
                    return "简单";
                case ContractDifficulty.Medium:
                    return "普通";
                case ContractDifficulty.Hard:
                    return "困难";
                case ContractDifficulty.Legendary:
                    return "传说";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 获取类型名称
        /// </summary>
        public string GetTypeName(ContractType type)
        {
            switch (type)
            {
                case ContractType.MonsterHunt:
                    return "怪物狩猎";
                case ContractType.Assassination:
                    return "暗杀";
                case ContractType.Rescue:
                    return "救援";
                case ContractType.Escort:
                    return "护送";
                case ContractType.Collection:
                    return "收集";
                case ContractType.Defense:
                    return "防御";
                default:
                    return "未知";
            }
        }
        
        /// <summary>
        /// 统计摘要
        /// </summary>
        public string GetStatisticsSummary()
        {
            return $"完成: {_data.totalCompleted} | 失败: {_data.totalFailed} | " +
                   $"金币: {_data.totalGoldEarned} | 经验: {_data.totalExpEarned} | " +
                   $"当前连胜: {_data.currentStreak} | 最高连胜: {_data.bestStreak}";
        }

        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            data["total_completed"] = _data.totalCompleted;
            data["total_failed"] = _data.totalFailed;
            data["total_gold_earned"] = _data.totalGoldEarned;
            data["total_exp_earned"] = _data.totalExpEarned;
            data["current_streak"] = _data.currentStreak;
            data["best_streak"] = _data.bestStreak;
            
            // Serialize completion counts
            var completionCounts = new Dictionary();
            foreach (var kvp in _data.contractCompletionCount)
            {
                completionCounts[kvp.Key] = kvp.Value;
            }
            data["contract_completion_count"] = completionCounts;

            // Serialize active contracts (ongoing contracts)
            var activeContractsData = new List<Dictionary>();
            foreach (var contract in _data.activeContracts)
            {
                var contractData = new Dictionary();
                contractData["contract_id"] = contract.contractId;
                contractData["title"] = contract.title;
                contractData["description"] = contract.description;
                contractData["client_name"] = contract.clientName;
                contractData["type"] = (int)contract.type;
                contractData["difficulty"] = (int)contract.difficulty;
                contractData["status"] = (int)contract.status;
                contractData["target_id"] = contract.target.targetId;
                contractData["target_name"] = contract.target.targetName;
                contractData["target_description"] = contract.target.targetDescription;
                contractData["required_kills"] = contract.target.requiredKills;
                contractData["current_kills"] = contract.target.currentKills;
                contractData["level"] = contract.target.level;
                contractData["target_difficulty"] = (int)contract.target.difficulty;
                contractData["gold"] = contract.reward.gold;
                contractData["experience"] = contract.reward.experience;
                contractData["reputation"] = contract.reward.reputation;
                contractData["time_limit"] = contract.timeLimit;
                contractData["start_time"] = contract.startTime.ToString("o");
                contractData["expiration_time"] = contract.expirationTime.ToString("o");
                contractData["location"] = contract.location;
                contractData["tips"] = contract.tips;
                activeContractsData.Add(contractData);
            }
            data["active_contracts"] = activeContractsData;
            
            return data;
        }

        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("total_completed"))
                _data.totalCompleted = Convert.ToInt32(data["total_completed"]);
            if (data.ContainsKey("total_failed"))
                _data.totalFailed = Convert.ToInt32(data["total_failed"]);
            if (data.ContainsKey("total_gold_earned"))
                _data.totalGoldEarned = Convert.ToInt32(data["total_gold_earned"]);
            if (data.ContainsKey("total_exp_earned"))
                _data.totalExpEarned = Convert.ToInt32(data["total_exp_earned"]);
            if (data.ContainsKey("current_streak"))
                _data.currentStreak = Convert.ToInt32(data["current_streak"]);
            if (data.ContainsKey("best_streak"))
                _data.bestStreak = Convert.ToInt32(data["best_streak"]);
            
            if (data.ContainsKey("contract_completion_count"))
            {
                var counts = (Dictionary)data["contract_completion_count"];
                _data.contractCompletionCount.Clear();
                foreach (var kvp in counts)
                {
                    _data.contractCompletionCount[kvp.Key.ToString()] = Convert.ToInt32(kvp.Value);
                }
            }

            // Deserialize active contracts
            if (data.ContainsKey("active_contracts"))
            {
                _data.activeContracts.Clear();
                var activeContractsData = (IList)data["active_contracts"];
                foreach (Dictionary contractData in activeContractsData)
                {
                    var contract = new Contract
                    {
                        contractId = contractData["contract_id"].ToString(),
                        title = contractData["title"].ToString(),
                        description = contractData["description"].ToString(),
                        clientName = contractData["client_name"].ToString(),
                        type = (ContractType)Convert.ToInt32(contractData["type"]),
                        difficulty = (ContractDifficulty)Convert.ToInt32(contractData["difficulty"]),
                        status = (ContractStatus)Convert.ToInt32(contractData["status"]),
                        target = new ContractTarget
                        {
                            targetId = contractData["target_id"].ToString(),
                            targetName = contractData["target_name"].ToString(),
                            targetDescription = contractData["target_description"].ToString(),
                            requiredKills = Convert.ToInt32(contractData["required_kills"]),
                            currentKills = Convert.ToInt32(contractData["current_kills"]),
                            level = Convert.ToInt32(contractData["level"]),
                            difficulty = (ContractDifficulty)Convert.ToInt32(contractData["target_difficulty"])
                        },
                        reward = new ContractReward
                        {
                            gold = Convert.ToInt32(contractData["gold"]),
                            experience = Convert.ToInt32(contractData["experience"]),
                            items = new List<string>(),
                            reputation = Convert.ToInt32(contractData["reputation"])
                        },
                        timeLimit = Convert.ToInt32(contractData["time_limit"]),
                        startTime = DateTime.Parse(contractData["start_time"].ToString()),
                        expirationTime = DateTime.Parse(contractData["expiration_time"].ToString()),
                        location = contractData["location"].ToString(),
                        tips = contractData["tips"].ToString()
                    };
                    _data.activeContracts.Add(contract);
                }
            }
        }
    }
}
