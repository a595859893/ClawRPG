using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.PetMemorial
{
    /// <summary>
    /// 单个墓碑记录（墓园展示用）
    /// </summary>
    public class MemorialMarkerEntry
    {
        /// <summary>宠物ID</summary>
        public int PetId;
        /// <summary>宠物名称</summary>
        public string PetName;
        /// <summary>宠物类型</summary>
        public string PetType;
        /// <summary>宠物颜色（十六进制）</summary>
        public string PetColor;
        /// <summary>累计战斗次数</summary>
        public int TotalBattles;
        /// <summary>最爱技能/Combo</summary>
        public string MostUsedCombo;
        /// <summary>友谊等级（死亡时）</summary>
        public int FriendshipLevel;
        /// <summary>累计击杀数</summary>
        public int TotalEnemiesKilled;
        /// <summary>最后一次战斗的HP百分比（0-100）</summary>
        public int LastBattleHpPercent;
        /// <summary>是否为牺牲（救主人）</summary>
        public bool IsSacrificeDeath;
        /// <summary>死亡时间戳</summary>
        public long DeathTimestamp;
        /// <summary>墓志铭</summary>
        public string Epitaph;
        /// <summary>墓碑风格（0=新，1=旧，2=古老）</summary>
        public int TombstoneStyle;
        /// <summary>讣告原文（来自 PetObituarySystem）</summary>
        public string ObituaryText;
        /// <summary>灵魂是否已升华</summary>
        public bool IsTranscended;
        /// <summary>升华时间戳</summary>
        public long TranscendedTimestamp;

        /// <summary>
        /// 获取死亡日期字符串
        /// </summary>
        public string GetDeathDateString()
        {
            if (DeathTimestamp <= 0) return "Unknown";
            return DateTimeOffset.FromUnixTimeSeconds(DeathTimestamp).LocalDateTime.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 获取墓碑老化程度描述
        /// </summary>
        public string GetWeatheringDesc()
        {
            return TombstoneStyle switch
            {
                0 => "新立",
                1 => "略有风化",
                2 => "古老斑驳",
                _ => "新立"
            };
        }

        /// <summary>
        /// 获取最后结局描述
        /// </summary>
        public string GetLastBattleOutcome()
        {
            if (IsSacrificeDeath) return "为救你而倒下";
            if (LastBattleHpPercent >= 90) return "满血退役";
            if (LastBattleHpPercent >= 50) return $"血量剩余{LastBattleHpPercent}%";
            if (LastBattleHpPercent > 0) return $"重伤倒下";
            return "战死";
        }
    }

    /// <summary>
    /// 墓志铭生成器 — 基于规则（无LLM）
    /// </summary>
    public static class EpitaphGenerator
    {
        public static string Generate(MemorialMarkerEntry marker)
        {
            // 规则1：牺牲
            if (marker.IsSacrificeDeath)
                return "它为保护你而倒下。";

            // 规则2：满血退役
            if (marker.LastBattleHpPercent >= 90)
                return "它在最后一刻仍然意气风发。";

            // 规则3：老兵
            if (marker.TotalBattles >= 50)
                return $"它参与了{marker.TotalBattles}场战斗，是真正的老兵。";

            // 规则4：高度参与
            if (marker.TotalBattles >= 20)
                return $"它陪伴你走过了{marker.TotalBattles}场战斗。";

            // 规则5：英勇
            if (marker.TotalEnemiesKilled >= 30)
                return $"它协助击败了{marker.TotalEnemiesKilled}个敌人。";

            // 规则6：高度友谊
            if (marker.FriendshipLevel >= 15)
                return "它是你最忠诚的伙伴。";

            // 规则7：升华
            if (marker.IsTranscended)
                return "它的灵魂已化为守护之光。";

            // 默认
            return "它陪伴你走完了这段路。";
        }

        /// <summary>
        /// 获取墓碑风格（基于死亡时间）
        /// 0=新立(0-7天)，1=略有风化(8-30天)，2=古老斑驳(30天+)
        /// </summary>
        public static int GetTombstoneStyle(long deathTimestamp)
        {
            if (deathTimestamp <= 0) return 0;
            var deathDate = DateTimeOffset.FromUnixTimeSeconds(deathTimestamp).LocalDateTime;
            var daysSinceDeath = (DateTime.Now - deathDate).Days;

            if (daysSinceDeath <= 7) return 0;      // 新立
            if (daysSinceDeath <= 30) return 1;     // 略有风化
            return 2;                                // 古老斑驳
        }
    }

    /// <summary>
    /// 墓园数据库 — 聚合所有宠物死亡数据
    /// </summary>
    public class PetMemorialDatabase
    {
        /// <summary>所有墓碑条目（PetId -> Entry）</summary>
        public Dictionary<int, MemorialMarkerEntry> Markers = new Dictionary<int, MemorialMarkerEntry>();

        /// <summary>墓园解锁标志</summary>
        public bool IsUnlocked { get; set; }

        /// <summary>
        /// 添加或更新墓碑条目
        /// </summary>
        public void AddOrUpdateMarker(MemorialMarkerEntry marker)
        {
            marker.TombstoneStyle = EpitaphGenerator.GetTombstoneStyle(marker.DeathTimestamp);
            marker.Epitaph = EpitaphGenerator.Generate(marker);
            Markers[marker.PetId] = marker;
        }

        /// <summary>
        /// 获取所有墓碑（按死亡时间倒序，最近的在前面）
        /// </summary>
        public List<MemorialMarkerEntry> GetAllMarkers()
        {
            var list = new List<MemorialMarkerEntry>(Markers.Values);
            list.Sort((a, b) => b.DeathTimestamp.CompareTo(a.DeathTimestamp));
            return list;
        }

        /// <summary>
        /// 获取所有已升华的宠物
        /// </summary>
        public List<MemorialMarkerEntry> GetTranscendedMarkers()
        {
            var list = new List<MemorialMarkerEntry>();
            foreach (var m in Markers.Values)
            {
                if (m.IsTranscended) list.Add(m);
            }
            return list;
        }

        /// <summary>
        /// 获取累计战死宠物数量
        /// </summary>
        public int GetDeathCount()
        {
            return Markers.Count;
        }
    }
}
