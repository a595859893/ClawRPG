using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑进化系统管理器
    /// </summary>
    public class MountEvolutionSystem : BaseSystem {
        public static MountEvolutionSystem Instance { get; private set; }

        private PlayerMountEvolutionData _playerData = new PlayerMountEvolutionData();

        // 信号系统
        [Signal] public delegate void OnEvolutionStarted(string mountId, MountEvolutionStage newStage);
        [Signal] public delegate void OnEvolutionCompleted(string mountId, MountEvolutionStage newStage, MountEvolutionType newType);
        [Signal] public delegate void OnEvolutionFailed(string mountId, EvolutionResult reason);
        [Signal] public delegate void OnStageChanged(string mountId, MountEvolutionStage newStage);
        [Signal] public delegate void OnTypeChanged(string mountId, MountEvolutionType newType);
        [Signal] public delegate void OnBattleExpGained(string mountId, int exp);

        public override void _Ready() {
            Instance = this;
            GD.Print("[MountEvolutionSystem] Initialized");
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "MountEvolution";
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return GetSaveData();
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data != null)
                LoadSaveData(data);
        }

        /// <summary>
        /// 初始化坐骑进化数据
        /// </summary>
        public void InitializeMountEvolution(string mountId, MountEvolutionChain chain) {
            if (!_playerData.MountEvolutions.ContainsKey(mountId)) {
                var evolution = new MountEvolutionInstance {
                    MountId = mountId,
                    CurrentStage = MountEvolutionStage.Basic,
                    CurrentType = MountEvolutionType.Nature,
                    EvolutionChain = chain,
                    TotalEvolutions = 0,
                    BattleExp = 0
                };
                _playerData.MountEvolutions[mountId] = evolution;
                GD.Print($"[MountEvolutionSystem] Initialized evolution for mount: {mountId}");
            }
        }

        /// <summary>
        /// 尝试进化坐骑
        /// </summary>
        public EvolutionResult TryEvolveMount(string mountId, MountEvolutionType targetType) {
            if (!_playerData.MountEvolutions.ContainsKey(mountId)) {
                GD.Warning($"[MountEvolutionSystem] Mount {mountId} not found in evolution data");
                return EvolutionResult.Failed;
            }

            var evolution = _playerData.MountEvolutions[mountId];
            var currentStageConfig = MountEvolutionDatabase.GetStageConfig(evolution.CurrentStage);
            var nextStage = MountEvolutionDatabase.GetNextStage(evolution.CurrentStage);
            var nextStageConfig = MountEvolutionDatabase.GetStageConfig(nextStage);

            // 检查是否已达最大阶段
            if (evolution.CurrentStage == MountEvolutionStage.Legendary) {
                EmitSignal(nameof(OnEvolutionFailed), mountId, EvolutionResult.MaxStage);
                return EvolutionResult.MaxStage;
            }

            // 检查等级要求
            var mountManager = MountManager.Instance;
            if (mountManager == null) {
                GD.Warning("[MountEvolutionSystem] MountManager not found");
                return EvolutionResult.Failed;
            }

            // 检查经验要求
            if (evolution.BattleExp < nextStageConfig.RequiredExp) {
                EmitSignal(nameof(OnEvolutionFailed), mountId, EvolutionResult.InsufficientExp);
                return EvolutionResult.InsufficientExp;
            }

            // 检查材料
            var materialName = MountEvolutionDatabase.GetEvolutionMaterialName(nextStage);
            var inventoryManager = InventoryManager.Instance;
            if (inventoryManager != null) {
                var hasMaterial = inventoryManager.HasItem(materialName, nextStageConfig.RequiredItems);
                if (!hasMaterial) {
                    EmitSignal(nameof(OnEvolutionFailed), mountId, EvolutionResult.InsufficientItems);
                    return EvolutionResult.InsufficientItems;
                }
                // 消耗材料
                inventoryManager.RemoveItem(materialName, nextStageConfig.RequiredItems);
            }

            // 检查金币
            var goldCost = MountEvolutionDatabase.GetEvolutionGoldCost(nextStage);
            if (goldCost > 0) {
                var player = GetTree().CurrentScene.GetNode<CharacterBody2D>("../Player");
                if (player != null) {
                    // 假设 Player 有 Gold 属性
                    var playerScript = player.GetScript();
                    if (playerScript != null) {
                        var goldField = playerScript.GetType().GetField("Gold");
                        if (goldField != null) {
                            var currentGold = (int)goldField.GetValue(player);
                            if (currentGold < goldCost) {
                                EmitSignal(nameof(OnEvolutionFailed), mountId, EvolutionResult.InsufficientItems);
                                return EvolutionResult.InsufficientItems;
                            }
                            goldField.SetValue(player, currentGold - goldCost);
                        }
                    }
                }
            }

            // 执行进化
            EmitSignal(nameof(OnEvolutionStarted), mountId, nextStage);

            evolution.CurrentStage = nextStage;
            evolution.CurrentType = targetType;
            evolution.TotalEvolutions++;

            // 应用属性加成
            ApplyStageBonuses(evolution);

            // 更新统计
            _playerData.TotalEvolutions++;
            if (_playerData.StageCount.ContainsKey(nextStage)) {
                _playerData.StageCount[nextStage]++;
            } else {
                _playerData.StageCount[nextStage] = 1;
            }

            if (_playerData.TypeCount.ContainsKey(targetType)) {
                _playerData.TypeCount[targetType]++;
            } else {
                _playerData.TypeCount[targetType] = 1;
            }

            EmitSignal(nameof(OnEvolutionCompleted), mountId, nextStage, targetType);
            EmitSignal(nameof(OnStageChanged), mountId, nextStage);
            EmitSignal(nameof(OnTypeChanged), mountId, targetType);

            // 自动保存
            SaveEvolutionData();

            GD.Print($"[MountEvolutionSystem] Mount {mountId} evolved to {nextStage} ({targetType})");
            return EvolutionResult.Success;
        }

        /// <summary>
        /// 应用阶段属性加成
        /// </summary>
        private void ApplyStageBonuses(MountEvolutionInstance evolution) {
            // 重置加成
            evolution.TotalHealthBonus = 0;
            evolution.TotalAttackBonus = 0;
            evolution.TotalDefenseBonus = 0;
            evolution.TotalSpeedBonus = 0;
            evolution.TotalCritRateBonus = 0;
            evolution.TotalCritDamageBonus = 0;

            // 计算所有已解锁阶段的加成
            var stages = new List<MountEvolutionStage> {
                MountEvolutionStage.Basic,
                MountEvolutionStage.Advanced,
                MountEvolutionStage.Elite,
                MountEvolutionStage.Epic,
                MountEvolutionStage.Legendary
            };

            foreach (var stage in stages) {
                if ((int)stage <= (int)evolution.CurrentStage) {
                    var config = MountEvolutionDatabase.GetStageConfig(stage);
                    if (config != null) {
                        evolution.TotalHealthBonus += config.HealthBonus;
                        evolution.TotalAttackBonus += config.AttackBonus;
                        evolution.TotalDefenseBonus += config.DefenseBonus;
                        evolution.TotalSpeedBonus += config.SpeedBonus;
                        evolution.TotalCritRateBonus += config.CritRateBonus;
                        evolution.TotalCritDamageBonus += config.CritDamageBonus;
                    }
                }
            }
        }

        /// <summary>
        /// 获取坐骑进化信息
        /// </summary>
        public MountEvolutionInstance GetMountEvolution(string mountId) {
            return _playerData.MountEvolutions.ContainsKey(mountId) ? _playerData.MountEvolutions[mountId] : null;
        }

        /// <summary>
        /// 获取坐骑进化属性加成
        /// </summary>
        public Dictionary<string, float> GetMountEvolutionBonuses(string mountId) {
            var evolution = GetMountEvolution(mountId);
            if (evolution == null) return new Dictionary<string, float>();

            return new Dictionary<string, float> {
                { "HealthBonus", evolution.TotalHealthBonus },
                { "AttackBonus", evolution.TotalAttackBonus },
                { "DefenseBonus", evolution.TotalDefenseBonus },
                { "SpeedBonus", evolution.TotalSpeedBonus },
                { "CritRateBonus", evolution.TotalCritRateBonus },
                { "CritDamageBonus", evolution.TotalCritDamageBonus }
            };
        }

        /// <summary>
        /// 添加战斗经验
        /// </summary>
        public void AddBattleExp(string mountId, int exp) {
            if (!_playerData.MountEvolutions.ContainsKey(mountId)) return;

            var evolution = _playerData.MountEvolutions[mountId];
            evolution.BattleExp += exp;
            _playerData.TotalBattleExp += exp;

            EmitSignal(nameof(OnBattleExpGained), mountId, exp);

            // 检查是否可以自动进化（如果满足条件）
            var nextStage = MountEvolutionDatabase.GetNextStage(evolution.CurrentStage);
            var nextStageConfig = MountEvolutionDatabase.GetStageConfig(nextStage);
            if (nextStageConfig != null && evolution.BattleExp >= nextStageConfig.RequiredExp) {
                GD.Print($"[MountEvolutionSystem] Mount {mountId} is ready to evolve!");
            }
        }

        /// <summary>
        /// 检查是否可以进化
        /// </summary>
        public bool CanEvolve(string mountId) {
            var evolution = GetMountEvolution(mountId);
            if (evolution == null || evolution.CurrentStage == MountEvolutionStage.Legendary) {
                return false;
            }

            var nextStage = MountEvolutionDatabase.GetNextStage(evolution.CurrentStage);
            var nextStageConfig = MountEvolutionDatabase.GetStageConfig(nextStage);
            return evolution.BattleExp >= nextStageConfig.RequiredExp;
        }

        /// <summary>
        /// 获取进化进度 (0.0 - 1.0)
        /// </summary>
        public float GetEvolutionProgress(string mountId) {
            var evolution = GetMountEvolution(mountId);
            if (evolution == null) return 0f;

            if (evolution.CurrentStage == MountEvolutionStage.Legendary) return 1f;

            var nextStage = MountEvolutionDatabase.GetNextStage(evolution.CurrentStage);
            var nextStageConfig = MountEvolutionDatabase.GetStageConfig(nextStage);
            if (nextStageConfig == null || nextStageConfig.RequiredExp == 0) return 1f;

            return Mathf.Clamp((float)evolution.BattleExp / nextStageConfig.RequiredExp, 0f, 1f);
        }

        /// <summary>
        /// 获取进化统计
        /// </summary>
        public PlayerMountEvolutionData GetStatistics() {
            return _playerData;
        }

        /// <summary>
        /// 获取进化阶段名称
        /// </summary>
        public string GetStageName(MountEvolutionStage stage) {
            var config = MountEvolutionDatabase.GetStageConfig(stage);
            return config?.StageName ?? "未知";
        }

        /// <summary>
        /// 获取进化类型名称
        /// </summary>
        public string GetTypeName(MountEvolutionType type) {
            var config = MountEvolutionDatabase.GetTypeConfig(type);
            return config?.TypeName ?? "未知";
        }

        /// <summary>
        /// 保存进化数据
        /// </summary>
        public Dictionary<string, object> GetSaveData() {
            var data = new Dictionary<string, object> {
                { "MountEvolutions", new List<Dictionary<string, object>>() },
                { "TotalEvolutions", _playerData.TotalEvolutions },
                { "TotalBattleExp", _playerData.TotalBattleExp }
            };

            foreach (var kvp in _playerData.MountEvolutions) {
                var evolution = kvp.Value;
                var evolutionData = new Dictionary<string, object> {
                    { "MountId", evolution.MountId },
                    { "CurrentStage", (int)evolution.CurrentStage },
                    { "CurrentType", (int)evolution.CurrentType },
                    { "EvolutionChain", (int)evolution.EvolutionChain },
                    { "TotalEvolutions", evolution.TotalEvolutions },
                    { "BattleExp", evolution.BattleExp },
                    { "TotalHealthBonus", evolution.TotalHealthBonus },
                    { "TotalAttackBonus", evolution.TotalAttackBonus },
                    { "TotalDefenseBonus", evolution.TotalDefenseBonus },
                    { "TotalSpeedBonus", evolution.TotalSpeedBonus },
                    { "TotalCritRateBonus", evolution.TotalCritRateBonus },
                    { "TotalCritDamageBonus", evolution.TotalCritDamageBonus }
                };
                ((List<Dictionary<string, object>>)data["MountEvolutions"]).Add(evolutionData);
            }

            return data;
        }

        /// <summary>
        /// 加载进化数据
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> data) {
            if (data == null) return;

            _playerData = new PlayerMountEvolutionData();

            if (data.ContainsKey("TotalEvolutions")) {
                _playerData.TotalEvolutions = Convert.ToInt32(data["TotalEvolutions"]);
            }
            if (data.ContainsKey("TotalBattleExp")) {
                _playerData.TotalBattleExp = Convert.ToInt32(data["TotalBattleExp"]);
            }

            if (data.ContainsKey("MountEvolutions")) {
                var evolutions = (List<object>)data["MountEvolutions"];
                foreach (var evolutionData in evolutions) {
                    var dict = (Dictionary<string, object>)evolutionData;
                    var evolution = new MountEvolutionInstance {
                        MountId = dict["MountId"].ToString(),
                        CurrentStage = (MountEvolutionStage)Convert.ToInt32(dict["CurrentStage"]),
                        CurrentType = (MountEvolutionType)Convert.ToInt32(dict["CurrentType"]),
                        EvolutionChain = (MountEvolutionChain)Convert.ToInt32(dict["EvolutionChain"]),
                        TotalEvolutions = Convert.ToInt32(dict["TotalEvolutions"]),
                        BattleExp = Convert.ToInt32(dict["BattleExp"]),
                        TotalHealthBonus = (float)Convert.ToDouble(dict["TotalHealthBonus"]),
                        TotalAttackBonus = (float)Convert.ToDouble(dict["TotalAttackBonus"]),
                        TotalDefenseBonus = (float)Convert.ToDouble(dict["TotalDefenseBonus"]),
                        TotalSpeedBonus = (float)Convert.ToDouble(dict["TotalSpeedBonus"]),
                        TotalCritRateBonus = (float)Convert.ToDouble(dict["TotalCritRateBonus"]),
                        TotalCritDamageBonus = (float)Convert.ToDouble(dict["TotalCritDamageBonus"])
                    };
                    _playerData.MountEvolutions[evolution.MountId] = evolution;
                }
            }

            GD.Print($"[MountEvolutionSystem] Loaded {_playerData.MountEvolutions.Count} mount evolutions");
        }
    }
}
