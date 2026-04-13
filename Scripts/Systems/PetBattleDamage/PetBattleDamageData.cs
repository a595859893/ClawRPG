using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetBattleDamage
{
    /// <summary>
    /// 宠物战损外观数据类型
    /// REQ-186: 宠物每次在战斗中受到超过 30% 最大生命值的单次伤害时，
    /// 在外观上留下可见痕迹（绷带、缺口、疤痕）
    /// </summary>
    public enum DamageMarkType
    {
        None = 0,
        /// <summary>轻伤：绷带包裹</summary>
        Bandage = 1,
        /// <summary>中伤：可见缺口</summary>
        Cut = 2,
        /// <summary>重伤：疤痕（多次累积）</summary>
        Scar = 3
    }

    /// <summary>
    /// 单条战损记录
    /// </summary>
    public class DamageMarkEntry
    {
        public int PetId { get; set; }
        public DamageMarkType MarkType { get; set; }
        public long RecordTimestamp { get; set; }
        public string SourceBattleId { get; set; }
        public float DamagePercent { get; set; }  // 伤害占最大HP的百分比

        public DamageMarkEntry(int petId, DamageMarkType markType, string battleId, float damagePercent)
        {
            PetId = petId;
            MarkType = markType;
            RecordTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SourceBattleId = battleId;
            DamagePercent = damagePercent;
        }
    }

    /// <summary>
    /// 宠物战损数据库 — 全局单例，存储所有宠物的战损记录
    /// 跨游戏持久化：存档时保存，进游戏时加载
    /// </summary>
    public class PetBattleDamageDatabase
    {
        public static PetBattleDamageDatabase Instance { get; private set; }

        /// <summary>
        /// 每只宠物的战损记录列表
        /// </summary>
        private Dictionary<int, List<DamageMarkEntry>> _petDamageMarks = new Dictionary<int, List<DamageMarkEntry>>();

        /// <summary>
        /// 每只宠物的最高战损等级（缓存，用于快速查询）
        /// </summary>
        private Dictionary<int, DamageMarkType> _petMaxMarkLevel = new Dictionary<int, DamageMarkType>();

        private const int MAX_MARKS_PER_PET = 5;

        public PetBattleDamageDatabase()
        {
            Instance = this;
        }

        /// <summary>
        /// 添加一条战损记录
        /// </summary>
        public void AddDamageMark(int petId, DamageMarkEntry entry)
        {
            if (!_petDamageMarks.ContainsKey(petId))
            {
                _petDamageMarks[petId] = new List<DamageMarkEntry>();
            }

            var marks = _petDamageMarks[petId];
            marks.Add(entry);

            // 超出最大数量时移除最旧的
            while (marks.Count > MAX_MARKS_PER_PET)
            {
                marks.RemoveAt(0);
            }

            // 更新最高等级缓存
            UpdateMaxLevel(petId);
        }

        /// <summary>
        /// 获取宠物所有战损记录
        /// </summary>
        public List<DamageMarkEntry> GetDamageMarks(int petId)
        {
            if (!_petDamageMarks.ContainsKey(petId))
                return new List<DamageMarkEntry>();
            return new List<DamageMarkEntry>(_petDamageMarks[petId]);
        }

        /// <summary>
        /// 获取宠物最高战损等级
        /// </summary>
        public DamageMarkType GetVisualDamageLevel(int petId)
        {
            return _petMaxMarkLevel.TryGetValue(petId, out var level) ? level : DamageMarkType.None;
        }

        /// <summary>
        /// 清除宠物所有战损记录（死亡或治疗时调用）
        /// </summary>
        public void ClearDamageMarks(int petId)
        {
            _petDamageMarks.Remove(petId);
            _petMaxMarkLevel.Remove(petId);
        }

        /// <summary>
        /// 更新宠物最高战损等级缓存
        /// </summary>
        private void UpdateMaxLevel(int petId)
        {
            if (!_petDamageMarks.ContainsKey(petId) || _petDamageMarks[petId].Count == 0)
            {
                _petMaxMarkLevel[petId] = DamageMarkType.None;
                return;
            }

            DamageMarkType maxType = DamageMarkType.None;
            foreach (var mark in _petDamageMarks[petId])
            {
                if (mark.MarkType > maxType)
                    maxType = mark.MarkType;
            }
            _petMaxMarkLevel[petId] = maxType;
        }

        /// <summary>
        /// 持久化：导出数据
        /// </summary>
        public Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            var petMarks = new List<Dictionary<string, object>>();

            foreach (var kvp in _petDamageMarks)
            {
                var petEntry = new Dictionary<string, object>
                {
                    { "petId", kvp.Key },
                    { "marks", new List<Dictionary<string, object>>() }
                };

                var marksList = (List<Dictionary<string, object>>)petEntry["marks"];
                foreach (var mark in kvp.Value)
                {
                    marksList.Add(new Dictionary<string, object>
                    {
                        { "markType", (int)mark.MarkType },
                        { "timestamp", mark.RecordTimestamp },
                        { "battleId", mark.SourceBattleId },
                        { "damagePercent", mark.DamagePercent }
                    });
                }

                petMarks.Add(petEntry);
            }

            data["petDamageMarks"] = petMarks;
            data["version"] = 1;
            return data;
        }

        /// <summary>
        /// 持久化：导入数据
        /// </summary>
        public void ImportSaveData(Dictionary<string, object> data)
        {
            _petDamageMarks.Clear();
            _petMaxMarkLevel.Clear();

            if (!data.ContainsKey("petDamageMarks"))
                return;

            var petMarks = (List<object>)data["petDamageMarks"];
            foreach (var petEntryObj in petMarks)
            {
                var petEntry = (Dictionary<string, object>)petEntryObj;
                int petId = Convert.ToInt32(petEntry["petId"]);
                var marksList = new List<DamageMarkEntry>();

                if (petEntry.ContainsKey("marks"))
                {
                    foreach (var markObj in (List<object>)petEntry["marks"])
                    {
                        var markData = (Dictionary<string, object>)markObj;
                        var entry = new DamageMarkEntry(
                            petId,
                            (DamageMarkType)Convert.ToInt32(markData["markType"]),
                            markData.ContainsKey("battleId") ? markData["battleId"].ToString() : "",
                            markData.ContainsKey("damagePercent") ? Convert.ToSingle(markData["damagePercent"]) : 0f
                        );
                        entry.RecordTimestamp = Convert.ToInt64(markData["timestamp"]);
                        marksList.Add(entry);
                    }
                }

                _petDamageMarks[petId] = marksList;
                UpdateMaxLevel(petId);
            }
        }
    }
}
