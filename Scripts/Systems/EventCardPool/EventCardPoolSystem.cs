using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;

namespace ClawRPG.Scripts.Systems.EventCardPool
{
    /// <summary>
    /// 事件卡池系统 — 抽卡、效果应用、战斗集成
    /// </summary>
    public partial class EventCardPoolSystem : BaseSystem
    {
        public static EventCardPoolSystem Instance { get; private set; }

        // 效果应用回调（供其他系统注册）
        public Action<EventCardConfig, EventCardEffect> OnEffectTriggered;

        private EventCardPoolData _data;
        private bool _pendingCardActive = false;
        private bool _cardEffectsApplied = false;

        // 地形效果跟踪
        private Dictionary<string, float> _activeTerrainEffects = new Dictionary<string, float>();
        private Dictionary<string, float> _activeBuffs = new Dictionary<string, float>();
        private Dictionary<string, float> _activeDebuffs = new Dictionary<string, float>();

        // 临时盟友跟踪
        private string _tempAllyCardId = "";
        private float _tempAllyEndTime = 0f;

        public override void _Ready()
        {
            Instance = this;
            _data = EventCardPoolData.Instance;

            if (_data == null)
            {
                GD.PrintErr("[EventCardPoolSystem] EventCardPoolData singleton not found!");
                return;
            }

            _data.OnCardAccepted += HandleCardAccepted;

            SubscribeToEventBus();
            GD.Print("[EventCardPoolSystem] 初始化完成");
        }

        private EventBusManager _eventBus;
        private bool _combatStartedThisSession = false;

        public override void _Process(double delta)
        {
            // 检查临时盟友过期
            if (_tempAllyActive && _tempAllyEndTime > 0 && OS.GetUnixTime() > _tempAllyEndTime)
            {
                EndTempAlly();
            }
        }

        // ========== 公开 API ==========

        /// <summary>
        /// 抽一张事件卡
        /// </summary>
        public string DrawCard()
        {
            _pendingCardActive = true;
            _cardEffectsApplied = false;
            return _data.DrawCard();
        }

        /// <summary>
        /// 重新抽卡（需消耗资源）
        /// </summary>
        public string ReDrawCard(int rerollCost, out bool success)
        {
            if (!CanAffordReroll(rerollCost))
            {
                success = false;
                return "";
            }

            // 扣除资源
            ConsumeRerollCost(rerollCost);
            success = true;

            _cardEffectsApplied = false;
            return _data.ReDrawCard();
        }

        /// <summary>
        /// 接受当前事件卡
        /// </summary>
        public void AcceptCard()
        {
            _data.AcceptCurrentCard();
        }

        /// <summary>
        /// 获取当前抽中的卡
        /// </summary>
        public EventCardConfig GetCurrentCard() => _data.GetCurrentCard();

        /// <summary>
        /// 获取重抽费用
        /// </summary>
        public int GetRerollCost()
        {
            var card = GetCurrentCard();
            if (card?.AcceptOption != null)
            {
                // 基础费用 + 已重抽次数 * 增量
                return card.AcceptOption.RerollCost + _data.RerollCount * 10;
            }
            return 30;
        }

        /// <summary>
        /// 检查是否能支付重抽费用
        /// </summary>
        public bool CanAffordReroll(int cost)
        {
            // TODO: 检查玩家金币是否足够
            return true; // 临时放行
        }

        /// <summary>
        /// 消耗重抽资源
        /// </summary>
        private void ConsumeRerollCost(int cost)
        {
            // TODO: 从玩家金币扣除 cost
            GD.Print($"[EventCardPoolSystem] 消耗重抽费用: {cost} 金币");
        }

        // ========== 效果应用 ==========
        private void HandleCardAccepted(string cardId)
        {
            var card = _data.GetCurrentCard();
            if (card == null) return;

            ApplyAllEffects(card);
            _cardEffectsApplied = true;
            _pendingCardActive = false;

            // 根据触发时机决定是否立即完全执行
            if (card.TriggerTiming == EventCardTriggerTiming.OnDraw)
            {
                // 抽卡时触发：效果已在 ApplyAllEffects 中执行
            }

            GD.Print($"[EventCardPoolSystem] 事件卡已接受并应用: {card.Title}");
        }

        private void ApplyAllEffects(EventCardConfig card)
        {
            foreach (var effect in card.Effects)
            {
                ApplyEffect(card, effect);
                OnEffectTriggered?.Invoke(card, effect);
            }
        }

        private void ApplyEffect(EventCardConfig card, EventCardEffect effect)
        {
            switch (effect.EffectType)
            {
                case EventCardEffectType.HealPlayer:
                    ApplyHealPlayer(effect);
                    break;
                case EventCardEffectType.DamagePlayer:
                    ApplyDamagePlayer(effect);
                    break;
                case EventCardEffectType.EnergyBoost:
                    ApplyEnergyBoost(effect);
                    break;
                case EventCardEffectType.GoldChange:
                    ApplyGoldChange(effect);
                    break;
                case EventCardEffectType.TempAlly:
                    ApplyTempAlly(effect);
                    break;
                case EventCardEffectType.TerrainEffect:
                    ApplyTerrainEffect(effect);
                    break;
                case EventCardEffectType.BuffEnemy:
                    ApplyBuffEnemy(effect);
                    break;
                case EventCardEffectType.BuffPlayer:
                    ApplyBuffPlayer(effect);
                    break;
                case EventCardEffectType.DebuffPlayer:
                    ApplyDebuffPlayer(effect);
                    break;
                case EventCardEffectType.ShieldPlayer:
                    ApplyShieldPlayer(effect);
                    break;
            }

            EmitEffectSignal(card, effect);
        }

        private void ApplyHealPlayer(EventCardEffect effect)
        {
            // TODO: 调用 PlayerStats.Heal((int)effect.Amount)
            GD.Print($"[EventCardPoolSystem] 治疗玩家: +{effect.Amount}");
        }

        private void ApplyDamagePlayer(EventCardEffect effect)
        {
            // TODO: 调用 PlayerStats.TakeDamage((int)effect.Amount)
            GD.Print($"[EventCardPoolSystem] 伤害玩家: -{effect.Amount}");
        }

        private void ApplyEnergyBoost(EventCardEffect effect)
        {
            // TODO: 调用 PlayerStats.AddEnergy((int)effect.Amount)
            GD.Print($"[EventCardPoolSystem] 能量提升: +{effect.Amount}");
        }

        private void ApplyGoldChange(EventCardEffect effect)
        {
            // TODO: 调用 PlayerStats.AddGold((int)effect.Amount)
            GD.Print($"[EventCardPoolSystem] 金币变化: {(effect.Amount >= 0 ? "+" : "")}{effect.Amount}");
        }

        private bool _tempAllyActive = false;

        private void ApplyTempAlly(EventCardEffect effect)
        {
            _tempAllyCardId = _data.GetCurrentCard()?.CardId ?? "";
            _tempAllyEndTime = OS.GetUnixTime() + (long)effect.Duration;
            _tempAllyActive = true;
            GD.Print($"[EventCardPoolSystem] 临时盟友加入，持续 {effect.Duration} 秒");
            // TODO: 生成临时盟友NPC加入战斗
        }

        private void EndTempAlly()
        {
            if (_tempAllyActive)
            {
                _tempAllyActive = false;
                _tempAllyCardId = "";
                _tempAllyEndTime = 0;
                GD.Print("[EventCardPoolSystem] 临时盟友离开");
                // TODO: 移除临时盟友NPC
            }
        }

        private void ApplyTerrainEffect(EventCardEffect effect)
        {
            string terrainKey = $"terrain_{_data.GetCurrentCard()?.CardId}";
            _activeTerrainEffects[terrainKey] = effect.Amount;
            GD.Print($"[EventCardPoolSystem] 地形效果激活: {effect.Description} (强度: {effect.Amount})");
            // TODO: 通知 BattlefieldVariantSystem 应用地形效果
        }

        private void ApplyBuffEnemy(EventCardEffect effect)
        {
            GD.Print($"[EventCardPoolSystem] 敌人强化: {(effect.Amount * 100):F0}% 攻击力提升");
            // TODO: 通知所有敌人应用 buff，或通过事件系统广播
        }

        private void ApplyBuffPlayer(EventCardEffect effect)
        {
            string buffKey = $"buff_{_data.GetCurrentCard()?.CardId}";
            _activeBuffs[buffKey] = effect.Duration;
            GD.Print($"[EventCardPoolSystem] 玩家增益: {(effect.Amount * 100):F0}% 增伤 (持续 {effect.Duration}s)");
            // TODO: 通过 BuffSystem 应用玩家增益
        }

        private void ApplyDebuffPlayer(EventCardEffect effect)
        {
            string debuffKey = $"debuff_{_data.GetCurrentCard()?.CardId}";
            _activeDebuffs[debuffKey] = effect.Duration;
            GD.Print($"[EventCardPoolSystem] 玩家debuff: {effect.Description} (持续 {effect.Duration}s)");
            // TODO: 通过 BuffSystem 应用玩家debuff
        }

        private void ApplyShieldPlayer(EventCardEffect effect)
        {
            GD.Print($"[EventCardPoolSystem] 玩家护盾: +{effect.Amount}");
            // TODO: 通过护盾系统应用临时护盾
        }

        // ========== 信号发射（通过 Action 事件）==========
        // C# Action event for effect triggers (UI can subscribe)
        public Action<string, EventCardEffectType, float> OnEffectTriggeredEvent;

        private void EmitEffectSignal(EventCardConfig card, EventCardEffect effect)
        {
            OnEffectTriggeredEvent?.Invoke(card.CardId, effect.EffectType, effect.Amount);
        }

        // ========== 事件总线订阅 ==========
        private void SubscribeToEventBus()
        {
            _eventBus = EventBusManager.Instance;
            if (_eventBus != null)
            {
                _eventBus.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
                _eventBus.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
                _eventBus.Subscribe(EventBusManager.Events.CombatEventCardDraw, OnCombatEventCardDrawRequested);
                GD.Print("[EventCardPoolSystem] 已订阅 EventBusManager");
            }
        }

        private void OnCombatStarted(object data)
        {
            _combatStartedThisSession = true;
            CheckTimingTrigger(EventCardTriggerTiming.OnCombatStart);
        }

        private void OnCombatEnded(object data)
        {
            // 清理本场战斗的临时状态
            _activeTerrainEffects.Clear();
            _activeBuffs.Clear();
            _activeDebuffs.Clear();
            _combatStartedThisSession = false;
        }

        private void OnCombatEventCardDrawRequested(object data)
        {
            // 外部请求抽卡（来自战斗前预览流程）
            DrawCard();
        }

        // private void OnEnemySpawned(object data) { CheckTimingTrigger(EventCardTriggerTiming.OnEnemySpawn); }
        // private void OnPlayerHurt(object data) { CheckTimingTrigger(EventCardTriggerTiming.OnPlayerHurt); }

        private void CheckTimingTrigger(EventCardTriggerTiming timing)
        {
            var card = _data.GetCurrentCard();
            if (card == null || _cardEffectsApplied) return;

            if (card.TriggerTiming == timing)
            {
                ApplyAllEffects(card);
                _cardEffectsApplied = true;
            }
        }

        // ========== 持久化 ==========
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary {
                { "rerollCount", _data.RerollCount },
                { "tempAllyActive", _tempAllyActive },
                { "tempAllyEndTime", _tempAllyEndTime },
                { "tempAllyCardId", _tempAllyCardId }
            };
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.Contains("tempAllyActive"))
                _tempAllyActive = (bool)data["tempAllyActive"];
            if (data.Contains("tempAllyEndTime"))
                _tempAllyEndTime = (float)data["tempAllyEndTime"];
            if (data.Contains("tempAllyCardId"))
                _tempAllyCardId = (string)data["tempAllyCardId"];
        }

        public void ResetRunState()
        {
            _pendingCardActive = false;
            _cardEffectsApplied = false;
            _activeTerrainEffects.Clear();
            _activeBuffs.Clear();
            _activeDebuffs.Clear();
            _tempAllyActive = false;
            _tempAllyCardId = "";
            _tempAllyEndTime = 0f;
            _data.ResetRunState();
        }
    }
}
