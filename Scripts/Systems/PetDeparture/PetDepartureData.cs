using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetDeparture
{
    /// <summary>
    /// 宠物离队档案记录
    /// REQ-189: 宠物离队时生成档案卡，归队时激活 +5% 协同伤害
    /// </summary>
    public class DepartureRecord
    {
        /// <summary>宠物唯一ID</summary>
        public string PetId { get; set; } = "";

        /// <summary>宠物显示名</summary>
        public string PetName { get; set; } = "";

        /// <summary>最后并肩作战的时间戳</summary>
        public long LastBattleTimestamp { get; set; }

        /// <summary>该宠物参与的总战斗场次</summary>
        public int TotalBattles { get; set; }

        /// <summary>宠物最常用的技能ID</summary>
        public string MostUsedSkill { get; set; } = "";

        /// <summary>是否已归队</summary>
        public bool IsReturned { get; set; }

        /// <summary>归队时间戳（0 表示未归队）</summary>
        public long ReturnTimestamp { get; set; }

        /// <summary>协同加成是否激活</summary>
        public bool SynergyBonusActive { get; set; }
    }

    /// <summary>
    /// 档案数据库 — 保存所有曾经并肩过的宠物记录
    /// </summary>
    public class PetDepartureDatabase
    {
        // petId → DepartureRecord
        private Dictionary<string, DepartureRecord> _records = new Dictionary<string, DepartureRecord>();

        public const float SYNERGY_BONUS = 0.05f; // +5%

        /// <summary>
        /// 创建或更新一条离队记录
        /// </summary>
        public void RecordDeparture(string petId, string petName, int totalBattles, string mostUsedSkill)
        {
            if (!_records.ContainsKey(petId))
            {
                _records[petId] = new DepartureRecord
                {
                    PetId = petId,
                    PetName = petName,
                    TotalBattles = totalBattles,
                    MostUsedSkill = mostUsedSkill,
                    LastBattleTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    IsReturned = false,
                    SynergyBonusActive = false
                };
            }
            else
            {
                var rec = _records[petId];
                rec.TotalBattles = totalBattles;
                rec.MostUsedSkill = mostUsedSkill;
                rec.LastBattleTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                rec.IsReturned = false;
                rec.SynergyBonusActive = false;
            }
        }

        /// <summary>
        /// 标记宠物已归队
        /// </summary>
        public void RecordReturn(string petId)
        {
            if (_records.TryGetValue(petId, out var rec))
            {
                rec.IsReturned = true;
                rec.ReturnTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                rec.SynergyBonusActive = true;
            }
            else
            {
                // No prior departure record — create a minimal one
                _records[petId] = new DepartureRecord
                {
                    PetId = petId,
                    IsReturned = true,
                    ReturnTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    SynergyBonusActive = true
                };
            }
        }

        /// <summary>
        /// 宠物再次离队时，清除协同加成
        /// </summary>
        public void ClearSynergyBonus(string petId)
        {
            if (_records.TryGetValue(petId, out var rec))
            {
                rec.SynergyBonusActive = false;
                rec.IsReturned = false;
            }
        }

        /// <summary>
        /// 获取所有离队记录（含已归队）
        /// </summary>
        public Dictionary<string, DepartureRecord> GetAllRecords()
        {
            return new Dictionary<string, DepartureRecord>(_records);
        }

        /// <summary>
        /// 获取特定宠物的离队记录
        /// </summary>
        public DepartureRecord GetRecord(string petId)
        {
            return _records.TryGetValue(petId, out var rec) ? rec : null;
        }

        /// <summary>
        /// 获取当前有协同加成的宠物列表
        /// </summary>
        public List<string> GetPetsWithSynergyBonus()
        {
            var result = new List<string>();
            foreach (var kvp in _records)
            {
                if (kvp.Value.SynergyBonusActive)
                    result.Add(kvp.Key);
            }
            return result;
        }

        /// <summary>
        /// 检查某宠物是否处于"归队"状态（有协同加成）
        /// </summary>
        public bool HasActiveSynergyBonus(string petId)
        {
            return _records.TryGetValue(petId, out var rec) && rec.SynergyBonusActive;
        }

        #region Persistence

        public Dictionary<string, object> ExportSaveData()
        {
            var recordsData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var kvp in _records)
            {
                recordsData[kvp.Key] = new Dictionary<string, object>
                {
                    ["pet_id"] = kvp.Value.PetId,
                    ["pet_name"] = kvp.Value.PetName,
                    ["last_battle_timestamp"] = kvp.Value.LastBattleTimestamp,
                    ["total_battles"] = kvp.Value.TotalBattles,
                    ["most_used_skill"] = kvp.Value.MostUsedSkill,
                    ["is_returned"] = kvp.Value.IsReturned,
                    ["return_timestamp"] = kvp.Value.ReturnTimestamp,
                    ["synergy_bonus_active"] = kvp.Value.SynergyBonusActive
                };
            }
            return new Dictionary<string, object> { ["records"] = recordsData };
        }

        public void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null || !data.ContainsKey("records")) return;

            var recordsData = data["records"] as Dictionary<string, Dictionary<string, object>>;
            if (recordsData == null) return;

            _records.Clear();
            foreach (var kvp in recordsData)
            {
                var rec = new DepartureRecord();
                var d = kvp.Value;
                rec.PetId = d.TryGetValue("pet_id", out var v) ? v?.ToString() ?? "" : "";
                rec.PetName = d.TryGetValue("pet_name", out var vn) ? vn?.ToString() ?? "" : "";
                rec.LastBattleTimestamp = d.TryGetValue("last_battle_timestamp", out var vt) ? Convert.ToInt64(vt) : 0;
                rec.TotalBattles = d.TryGetValue("total_battles", out var tb) ? Convert.ToInt32(tb) : 0;
                rec.MostUsedSkill = d.TryGetValue("most_used_skill", out var ms) ? ms?.ToString() ?? "" : "";
                rec.IsReturned = d.TryGetValue("is_returned", out var ir) && ir is true;
                rec.ReturnTimestamp = d.TryGetValue("return_timestamp", out var rts) ? Convert.ToInt64(rts) : 0;
                rec.SynergyBonusActive = d.TryGetValue("synergy_bonus_active", out var sba) && sba is true;
                _records[kvp.Key] = rec;
            }
        }

        #endregion
    }
}
