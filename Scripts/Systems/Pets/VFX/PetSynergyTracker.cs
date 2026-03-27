using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets;

namespace ClawRPG.Scripts.Systems.Pets.VFX {
    /// <summary>
    /// 宠物协同增益追踪器
    /// 记录宠物攻击次数，每达到阈值（5次）触发协同增益
    /// </summary>
    public partial class PetSynergyTracker : Node {
        public static PetSynergyTracker Instance { get; private set; }

        // 配置
        [Export] private int synergyThreshold = 5;
        [Export] private float synergyDamageBonus = 0.10f;  // +10%
        [Export] private float synergyDuration = 30f;        // 30秒

        // 状态
        private int _attackCount = 0;
        private bool _synergyActive = false;
        private float _synergyRemainingTime = 0f;
        private Timer _decayTimer;

        // 信号
        [Signal] public delegate void SynergyCountUpdatedEventHandler(int count, int threshold);
        [Signal] public delegate void SynergyTriggeredEventHandler(float damageBonus, float duration);
        [Signal] public delegate void SynergyExpiredEventHandler();
        [Signal] public delegate void SynergyAccumulatedEventHandler(int count);

        public override void _Ready() {
            Instance = this;
            SetupDecayTimer();
            ConnectToPetSignal();
            UpdateSynergyUI();
        }

        private void SetupDecayTimer() {
            _decayTimer = new Timer {
                OneShot = false,
                WaitTime = 1f,
                Autostart = true
            };
            _decayTimer.Timeout += OnDecayTick;
            AddChild(_decayTimer);
        }

        private void ConnectToPetSignal() {
            CallDeferred(nameof(ConnectSignalDeferred));
        }

        private void ConnectSignalDeferred() {
            var petAI = PetCombatAI.Instance;
            if (petAI != null) {
                petAI.Connect("PetAttacked", new Callable(this, nameof(OnPetAttacked)), (uint)ConnectFlags.Deferred);
                GD.Print("[PetSynergyTracker] Connected to PetCombatAI.PetAttacked");
            } else {
                var timer = new Timer { OneShot = true, WaitTime = 1.0f };
                timer.Timeout += () => ConnectSignalDeferred();
                AddChild(timer);
                timer.Start();
            }
        }

        private void OnPetAttacked(Node2D enemy, int damage) {
            RecordAttack();
        }

        /// <summary>
        /// 记录一次宠物攻击
        /// </summary>
        public void RecordAttack() {
            _attackCount++;
            EmitSignal(SignalName.SynergyCountUpdated, _attackCount, synergyThreshold);
            EmitSignal(SignalName.SynergyAccumulated, _attackCount);

            // 重置衰减计时器（如果增益激活，重置持续时间）
            if (_synergyActive) {
                _synergyRemainingTime = synergyDuration;
            }

            // 检查是否达到阈值
            if (_attackCount >= synergyThreshold && !_synergyActive) {
                TriggerSynergy();
            }

            UpdateSynergyUI();
        }

        private void TriggerSynergy() {
            _synergyActive = true;
            _synergyRemainingTime = synergyDuration;
            _attackCount = 0;  // 重置计数

            EmitSignal(SignalName.SynergyTriggered, synergyDamageBonus, synergyDuration);

            // 显示特效
            ShowSynergyVFX();

            GD.Print($"[PetSynergyTracker] Synergy triggered! +{synergyDamageBonus * 100}% damage for {synergyDuration}s");
            UpdateSynergyUI();
        }
            }
        }

        private void ShowSynergyVFX() {
            // 通知 UI 显示金色爆发特效
            var ui = GetNodeOrNull("/root/Main/PetCombatCompanionUI");
            if (ui != null && ui.HasMethod("ShowSynergyBurst")) {
                ui.CallDeferred("ShowSynergyBurst");
            }
        }

        private void OnDecayTick() {
            if (_synergyActive) {
                _synergyRemainingTime -= 1f;
                if (_synergyRemainingTime <= 0) {
                    ExpireSynergy();
                }
            }

            // 可选：长时间无攻击则衰减计数
            // if (_attackCount > 0 && !_synergyActive) { _attackCount--; }
        }

        private void ExpireSynergy() {
            _synergyActive = false;
            _synergyRemainingTime = 0f;
            _attackCount = 0;

            EmitSignal(SignalName.SynergyExpired);
            UpdateSynergyUI();

            GD.Print("[PetSynergyTracker] Synergy expired");
        }

        private void UpdateSynergyUI() {
            var ui = GetNodeOrNull("/root/Main/PetCombatCompanionUI");
            if (ui != null && ui.HasMethod("UpdateSynergyCounter")) {
                ui.CallDeferred("UpdateSynergyCounter", _attackCount, synergyThreshold, _synergyActive, _synergyRemainingTime);
            }
        }

        /// <summary>
        /// 获取当前伤害倍率（给 SkillModules 查询）
        /// </summary>
        public float GetCurrentDamageMultiplier() {
            return _synergyActive ? (1f + synergyDamageBonus) : 1f;
        }

        /// <summary>
        /// 获取协同增益信息
        /// </summary>
        public (int count, int threshold, bool active, float remaining) GetSynergyInfo() {
            return (_attackCount, synergyThreshold, _synergyActive, _synergyRemainingTime);
        }

        /// <summary>
        /// 手动触发协同（供测试/调试）
        /// </summary>
        public void ForceTriggerSynergy() {
            if (!_synergyActive) {
                TriggerSynergy();
            }
        }

        /// <summary>
        /// 重置追踪状态
        /// </summary>
        public void Reset() {
            _attackCount = 0;
            _synergyActive = false;
            _synergyRemainingTime = 0f;
            EmitSignal(SignalName.SynergyCountUpdated, 0, synergyThreshold);
            UpdateSynergyUI();
        }
    }
}
