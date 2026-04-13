using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetSoul
{
    /// <summary>
    /// 守护灵系统（REQ-195）
    /// 管理升华后守护灵的被动效果 — 所有宠物技能冷却速度 +3% per 守护灵
    /// </summary>
    public partial class GuardianSpiritSystem : BaseSystem
    {
        private static GuardianSpiritSystem _instance;
        public static GuardianSpiritSystem Instance => _instance;

        // Signals
        public delegate void GuardianCountChangedEventHandler(int count, float totalBonus);
        public event GuardianCountChangedEventHandler OnGuardianCountChanged;

        /// <summary>当前守护灵数量</summary>
        private int _guardianCount = 0;

        /// <summary>总冷却加速加成</summary>
        private float _totalCooldownBonus = 0f;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            SubscribeToSignals();
            LoadInitialState();
        }

        private void SubscribeToSignals()
        {
            // 订阅 PetSoulGhostSystem 的升华信号
            if (PetSoulGhostSystem.Instance != null)
            {
                PetSoulGhostSystem.Instance.OnSoulTranscended += OnSoulTranscended;
                PetSoulGhostSystem.Instance.OnGuardianSpiritBonusChanged += OnGuardianBonusChanged;
            }
        }

        private void LoadInitialState()
        {
            if (PetSoulGhostSystem.Instance != null)
            {
                var transcended = PetSoulGhostSystem.Instance.GetTranscendedGhosts();
                _guardianCount = transcended.Count;
                _totalCooldownBonus = PetSoulGhostSystem.Instance.GetGuardianCooldownBonus();
            }
        }

        private void OnSoulTranscended(int petId)
        {
            _guardianCount++;
            UpdateBonus();
            GD.Print($"[GuardianSpirit] Guardian count: {_guardianCount}, Bonus: {_totalCooldownBonus:P0}");
        }

        private void OnGuardianBonusChanged(float bonusPercent)
        {
            _totalCooldownBonus = bonusPercent;
            // 如果有 PetSkillSystem，直接应用
            ApplyBonusToPetSkills();
        }

        private void UpdateBonus()
        {
            if (PetSoulGhostSystem.Instance != null)
            {
                _totalCooldownBonus = PetSoulGhostSystem.Instance.GetGuardianCooldownBonus();
            }
            OnGuardianCountChanged?.Invoke(_guardianCount, _totalCooldownBonus);
            ApplyBonusToPetSkills();
        }

        /// <summary>
        /// 将加成应用到宠物技能系统
        /// </summary>
        private void ApplyBonusToPetSkills()
        {
            var petSkillSystem = GetNodeOrNull<Godot.Node>("/root/PetSkillSystem");
            if (petSkillSystem != null)
            {
                petSkillSystem.Set("GuardianCooldownBonus", _totalCooldownBonus);
            }

            // 也通知 PetCombatCompanionSystem
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petCompanion != null)
            {
                petCompanion.Set("GuardianCooldownBonus", _totalCooldownBonus);
            }
        }

        // ========== Public API ==========

        /// <summary>
        /// 获取当前守护灵数量
        /// </summary>
        public int GetGuardianCount() => _guardianCount;

        /// <summary>
        /// 获取总冷却加速加成
        /// </summary>
        public float GetTotalCooldownBonus() => _totalCooldownBonus;

        /// <summary>
        /// 获取格式化加成描述
        /// </summary>
        public string GetBonusDescription()
        {
            if (_guardianCount == 0)
                return "No guardian spirits";
            return $"{_guardianCount} guardian spirit(s) — {_totalCooldownBonus:P0} pet skill cooldown speed";
        }

        // ========== Persistence ==========

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>
            {
                { "guardian_count", _guardianCount },
                { "total_cooldown_bonus", _totalCooldownBonus }
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("guardian_count"))
                _guardianCount = Convert.ToInt32(data["guardian_count"]);
            if (data.ContainsKey("total_cooldown_bonus"))
                _totalCooldownBonus = Convert.ToSingle(data["total_cooldown_bonus"]);
            ApplyBonusToPetSkills();
        }
    }
}
