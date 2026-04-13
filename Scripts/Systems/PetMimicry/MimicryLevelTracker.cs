using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 宠物行为模仿等级追踪器 — 集中管理所有印记的 XP/等级/衰减
    /// 
    /// 职责：
    /// 1. _Process 定期检查印记衰减（长时间未触发 → 等级下降）
    /// 2. 提供 UI 友好的等级/XP 进度查询
    /// 3. 在等级变化时发射信号，通知 PetMimicrySkillSystem 刷新
    /// 4. 支持存档持久化
    /// </summary>
    public partial class MimicryLevelTracker : Node
    {
        public static MimicryLevelTracker Instance { get; private set; }

        // ── Config ────────────────────────────────────────────────────────
        /// <summary>衰减检查间隔（秒）</summary>
        private const float DECAY_CHECK_INTERVAL = 30f;

        /// <summary>超过此天数未触发则开始衰减</summary>
        private const float DECAY_GRACE_DAYS = 7f;

        /// <summary>每次衰减检查降低的XP量（线性）</summary>
        private const float XP_DECAY_AMOUNT = 5f;

        /// <summary>每 DECAY_GRACE_DAYS 天未触发，等级-1</summary>
        private const float DAYS_PER_LEVEL_DECAY = 14f;

        private float _decayAccumulator = 0f;

        // ── Signals ────────────────────────────────────────────────────────
public delegate void ImprintLevelChangedEventHandler(
            PlayerBehaviorType behavior,
            RoomEnvironmentType environment,
            int oldLevel,
            int newLevel);
public delegate void ImprintXpGainedEventHandler(
            PlayerBehaviorType behavior,
            RoomEnvironmentType environment,
            float xpGained,
            float xpTotal,
            float xpForNextLevel);

        // ── Dependencies ─────────────────────────────────────────────────
        private PetMimicryData _mimicryData;

        public override void _Ready()
        {
            Instance = this;
            _mimicryData = PetMimicryData.Instance;
            GD.Print("[MimicryLevelTracker] Initialized");
        }

        public override void _Process(double delta)
        {
            _decayAccumulator += (float)delta;
            if (_decayAccumulator >= DECAY_CHECK_INTERVAL)
            {
                _decayAccumulator = 0f;
                CheckImprintDecay();
            }
        }

        // ── Decay Logic ───────────────────────────────────────────────────

        /// <summary>
        /// 定期检查所有印记的衰减（REQ-143: 使用 BehaviorImprint.DecayTimer 实时跟踪）
        /// </summary>
        private void CheckImprintDecay()
        {
            if (_mimicryData == null) return;

            var allImprints = _mimicryData.GetAllImprints();
            float graceSeconds = DECAY_GRACE_DAYS * 86400f; // 7天转秒
            float decayIntervalSeconds = DAYS_PER_LEVEL_DECAY * 86400f; // 14天/级

            foreach (var imprint in allImprints)
            {
                if (imprint.ImprintLevel == 0) continue; // 未解锁的不衰减

                // REQ-143: 实时跟踪衰减计时器
                imprint.DecayTimer += DECAY_CHECK_INTERVAL;

                bool decayed = imprint.CheckDecay(graceSeconds, decayIntervalSeconds);
                if (decayed)
                {
                    int oldLevel = imprint.ImprintLevel + 1; // CheckDecay already decremented
                    GD.Print($"[MimicryLevelTracker] Decay: {imprint.BehaviorType} in {imprint.EnvironmentType} " +
                             $"dropped from Lv{oldLevel} → Lv{imprint.ImprintLevel} (idle {imprint.DecayTimer / 86400f:F1}d)");

                    EmitSignal(SignalName.ImprintLevelChanged,
                        imprint.BehaviorType, imprint.EnvironmentType, oldLevel, imprint.ImprintLevel);

                    NotifySkillSystemRefresh();
                }
            }
        }

        /// <summary>
        /// 通知 PetMimicrySkillSystem 刷新技能实例（等级变化后）
        /// </summary>
        private void NotifySkillSystemRefresh()
        {
            if (PetMimicrySkillSystem.Instance != null)
            {
                PetMimicrySkillSystem.Instance.RefreshSkillInstances();
            }
        }

        /// <summary>
        /// REQ-143: 刷新印记 — 宠物回到匹配的房间时调用，重置衰减计时器并提升等级
        /// </summary>
        public void RefreshImprint(RoomEnvironmentType envType)
        {
            if (_mimicryData == null) return;

            var imprints = _mimicryData.GetImprintsForEnvironment(envType);
            foreach (var imprint in imprints)
            {
                int oldLevel = imprint.ImprintLevel;

                // 重置衰减计时器
                imprint.ResetDecayTimer();

                // 等级+1（上限5）
                if (imprint.ImprintLevel < 5)
                {
                    imprint.ImprintLevel++;
                    imprint.Xp = 0f;
                    GD.Print($"[MimicryLevelTracker] Imprint refreshed: {imprint.BehaviorType} in {envType} Lv{oldLevel}→Lv{imprint.ImprintLevel}");

                    EmitSignal(SignalName.ImprintLevelChanged,
                        imprint.BehaviorType, envType, oldLevel, imprint.ImprintLevel);
                }
            }

            NotifySkillSystemRefresh();
        }

        // ── Level Up Handler ─────────────────────────────────────────────

        /// <summary>
        /// 由 PetBehaviorLogger 调用，在 XP 增加后检查是否升级
        /// </summary>
        public void OnImprintXpGained(BehaviorImprint imprint, float xpGained)
        {
            EmitSignal(SignalName.ImprintXpGained,
                imprint.BehaviorType,
                imprint.EnvironmentType,
                xpGained,
                imprint.Xp,
                imprint.GetXpForNextLevel());

            // 如果升级了，通知系统刷新
            NotifySkillSystemRefresh();
        }

        // ── UI Query APIs ─────────────────────────────────────────────────

        /// <summary>
        /// 获取指定行为的等级进度（0.0 ~ 1.0）
        /// </summary>
        public float GetLevelProgress(PlayerBehaviorType behavior, RoomEnvironmentType envType)
        {
            var imprint = _mimicryData?.GetImprint(envType, behavior);
            if (imprint == null || imprint.ImprintLevel >= 5) return 1f;
            if (imprint.ImprintLevel == 0 && imprint.Xp == 0) return 0f;

            float xpForNext = imprint.GetXpForNextLevel();
            return Mathf.Clamp(imprint.Xp / xpForNext, 0f, 1f);
        }

        /// <summary>
        /// 获取某行为在所有环境中的平均等级（用于个性卡）
        /// </summary>
        public float GetAverageLevel(PlayerBehaviorType behavior)
        {
            if (_mimicryData == null) return 0f;

            int totalLevel = 0;
            int count = 0;
            foreach (var imprint in _mimicryData.GetAllImprints())
            {
                if (imprint.BehaviorType == behavior && imprint.ImprintLevel > 0)
                {
                    totalLevel += imprint.ImprintLevel;
                    count++;
                }
            }
            return count > 0 ? (float)totalLevel / count : 0f;
        }

        /// <summary>
        /// 获取某行为所有印记的等级进度列表（用于UI展示）
        /// </summary>
        public List<ImprintLevelInfo> GetImprintLevelInfos(PlayerBehaviorType behavior)
        {
            var result = new List<ImprintLevelInfo>();
            if (_mimicryData == null) return result;

            foreach (var imprint in _mimicryData.GetAllImprints())
            {
                if (imprint.BehaviorType != behavior) continue;

                result.Add(new ImprintLevelInfo
                {
                    EnvironmentType = imprint.EnvironmentType,
                    EnvironmentName = RoomEnvironmentClassifier.GetDisplayName(imprint.EnvironmentType),
                    Level = imprint.ImprintLevel,
                    Xp = imprint.Xp,
                    XpForNextLevel = imprint.GetXpForNextLevel(),
                    Progress = imprint.ImprintLevel >= 5 ? 1f :
                               imprint.ImprintLevel == 0 ? 0f :
                               Mathf.Clamp(imprint.Xp / imprint.GetXpForNextLevel(), 0f, 1f),
                    DaysSinceLastRecord = (float)(DateTime.Now - imprint.LastRecordedAt).TotalDays,
                    TotalTriggers = imprint.TotalTriggers
                });
            }
            return result;
        }

        /// <summary>
        /// 获取所有已解锁（等级>0）的印记数量
        /// </summary>
        public int GetUnlockedImprintCount()
        {
            if (_mimicryData == null) return 0;
            int count = 0;
            foreach (var imprint in _mimicryData.GetAllImprints())
            {
                if (imprint.ImprintLevel > 0) count++;
            }
            return count;
        }

        /// <summary>
        /// 获取距离下次衰减检查的秒数
        /// </summary>
        public float GetSecondsUntilDecayCheck()
        {
            return DECAY_CHECK_INTERVAL - _decayAccumulator;
        }

        // ── Persistence ───────────────────────────────────────────────────

        /// <summary>
        /// 手动触发一次衰减检查（存档时调用）
        /// </summary>
        public void ForceDecayCheck()
        {
            _decayAccumulator = DECAY_CHECK_INTERVAL; // 强制下次 Process 触发衰减
        }
    }

    /// <summary>
    /// 印记等级信息（UI 展示用）
    /// </summary>
    public class ImprintLevelInfo
    {
        public RoomEnvironmentType EnvironmentType { get; set; }
        public string EnvironmentName { get; set; }
        public int Level { get; set; }
        public float Xp { get; set; }
        public float XpForNextLevel { get; set; }
        public float Progress { get; set; }
        public float DaysSinceLastRecord { get; set; }
        public int TotalTriggers { get; set; }

        /// <summary>等级颜色（UI 用）</summary>
        public Color GetLevelColor()
        {
            return Level switch
            {
                0 => new Color(0.5f, 0.5f, 0.5f),   // 灰：未解锁
                1 => new Color(0.3f, 0.8f, 0.3f),   // 绿
                2 => new Color(0.2f, 0.6f, 1f),      // 蓝
                3 => new Color(0.8f, 0.6f, 0.2f),   // 黄
                4 => new Color(0.9f, 0.4f, 0.1f),   // 橙
                5 => new Color(0.8f, 0.2f, 0.9f),   // 紫：满级
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }

        /// <summary>等级名称</summary>
        public string GetLevelName()
        {
            return Level switch
            {
                0 => "未解锁",
                1 => "初学",
                2 => "熟练",
                3 => "精通",
                4 => "大师",
                5 => "极致",
                _ => "?"
            };
        }
    }
}
