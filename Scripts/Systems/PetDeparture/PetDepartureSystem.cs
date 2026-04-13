using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Systems.PetDeparture
{
    /// <summary>
    /// 宠物离队感叙事系统（REQ-189）
    ///
    /// 宠物从队伍中移除时，Safe House 生成「档案卡」记录战斗历史。
    /// 宠物重新归队时，激活 +5% 协同伤害（团队默契残留）。
    /// 加成在宠物再次离队后消失。
    ///
    /// 数据来源：PetActionStatsDatabase（最常用技能）、PetCombatCompanionData（战斗场次）
    /// UI 输出：PetDeparturePanel（Safe House 档案陈列室）
    /// </summary>
    public partial class PetDepartureSystem : BaseSystem
    {
        private static PetDepartureSystem _instance;
        public static PetDepartureSystem Instance => _instance;

        private PetDepartureDatabase _database = new PetDepartureDatabase();
        private PetActionStatsDatabase _actionStats = new PetActionStatsDatabase();

        // 上一次记录的 ActivePetId（用于检测变更）
        private string _lastActivePetId = "";
        private bool _initialized = false;

        // Signals
        public delegate void DepartureRecordedEventHandler(string petId, DepartureRecord record);
        public delegate void PetReturnedEventHandler(string petId, DepartureRecord record);
        public delegate void SynergyBonusChangedEventHandler(string petId, bool active, float bonus);
        public delegate void RecordsUpdatedEventHandler();

        public event DepartureRecordedEventHandler OnDepartureRecorded;
        public event PetReturnedEventHandler OnPetReturned;
        public event SynergyBonusChangedEventHandler OnSynergyBonusChanged;
        public event RecordsUpdatedEventHandler OnRecordsUpdated;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            GD.Print("[PetDeparture] Initialized");
        }

        public override void _Process(double delta)
        {
            // 在 _Process 中检测 ActivePetId 变更（无信号可用时轮询）
            if (!PetCombatCompanionSystem.InstanceExists)
                return;

            var companion = PetCombatCompanionSystem.Instance;
            string currentActiveId = companion.GetActivePetId() ?? "";

            if (!_initialized)
            {
                _lastActivePetId = currentActiveId;
                _initialized = true;
                return;
            }

            // 检测变更
            if (currentActiveId != _lastActivePetId)
            {
                HandleActivePetChanged(_lastActivePetId, currentActiveId);
                _lastActivePetId = currentActiveId;
            }
        }

        /// <summary>
        /// 处理活跃宠物变更
        /// </summary>
        private void HandleActivePetChanged(string oldId, string newId)
        {
            // 新宠物的战斗记录
            if (!string.IsNullOrEmpty(newId))
            {
                RecordBattleForPet(newId);
            }

            // 离队：oldId 不是空，且 oldId != newId
            if (!string.IsNullOrEmpty(oldId) && oldId != newId)
            {
                HandlePetDeparture(oldId);
            }

            // 归队：oldId 不是空，newId 不是空，且 oldId != newId
            if (!string.IsNullOrEmpty(oldId) && !string.IsNullOrEmpty(newId) && oldId != newId)
            {
                // 检查 newId 是否之前有离队记录
                var existingRecord = _database.GetRecord(newId);
                if (existingRecord != null && !existingRecord.IsReturned)
                {
                    HandlePetReturn(newId);
                }
            }
        }

        /// <summary>
        /// 每场战斗结束时，记录宠物的战斗数据
        /// </summary>
        public void RecordBattleForPet(string petId)
        {
            _actionStats.RecordBattle(petId);

            // 监听战斗结束信号
            if (CombatSystem.InstanceExists)
            {
                CombatSystem.Instance.OnCombatEnded += OnCombatEnded;
            }
        }

        private void OnCombatEnded()
        {
            // 从 PetCombatCompanionSystem 获取当前活跃宠物并记录战斗
            if (!PetCombatCompanionSystem.InstanceExists)
                return;

            var companion = PetCombatCompanionSystem.Instance;
            string activeId = companion.GetActivePetId();
            if (string.IsNullOrEmpty(activeId))
                return;

            _actionStats.RecordBattle(activeId);
        }

        /// <summary>
        /// 宠物离队：生成档案卡
        /// </summary>
        private void HandlePetDeparture(string petId)
        {
            var companion = PetCombatCompanionSystem.Instance;
            string petName = GetPetDisplayName(petId);
            int totalBattles = _actionStats.GetBattleCount(petId);
            string mostUsedSkill = _actionStats.GetMostUsedSkill(petId);

            _database.RecordDeparture(petId, petName, totalBattles, mostUsedSkill);
            _database.ClearSynergyBonus(petId);

            var record = _database.GetRecord(petId);
            OnDepartureRecorded?.Invoke(petId, record);
            OnSynergyBonusChanged?.Invoke(petId, false, 0f);
            OnRecordsUpdated?.Invoke();

            GD.Print($"[PetDeparture] Pet departed: {petId} (battles={totalBattles}, skill={mostUsedSkill})");
        }

        /// <summary>
        /// 宠物归队：激活协同加成
        /// </summary>
        private void HandlePetReturn(string petId)
        {
            _database.RecordReturn(petId);
            var record = _database.GetRecord(petId);

            if (record != null && record.SynergyBonusActive)
            {
                OnPetReturned?.Invoke(petId, record);
                OnSynergyBonusChanged?.Invoke(petId, true, PetDepartureDatabase.SYNERGY_BONUS);
                OnRecordsUpdated?.Invoke();

                GD.Print($"[PetDeparture] Pet returned: {petId}, synergy bonus +{PetDepartureDatabase.SYNERGY_BONUS:P0} active");
            }
        }

        /// <summary>
        /// 获取宠物的显示名称（从 PetDatabase 或其他来源）
        /// </summary>
        private string GetPetDisplayName(string petId)
        {
            // 尝试从 PetDatabase 获取宠物名
            // 如果没有，返回 petId 作为后备
            return petId;
        }

        #region Public API

        /// <summary>
        /// 记录宠物使用了一个技能（用于统计最常用技能）
        /// </summary>
        public void RecordSkillUsed(string petId, string skillId)
        {
            _actionStats.RecordSkillUsage(petId, skillId);
        }

        /// <summary>
        /// 获取所有离队档案记录
        /// </summary>
        public Dictionary<string, DepartureRecord> GetAllRecords()
        {
            return _database.GetAllRecords();
        }

        /// <summary>
        /// 获取特定宠物的档案记录
        /// </summary>
        public DepartureRecord GetRecord(string petId)
        {
            return _database.GetRecord(petId);
        }

        /// <summary>
        /// 获取当前有协同加成的宠物列表
        /// </summary>
        public List<string> GetPetsWithSynergyBonus()
        {
            return _database.GetPetsWithSynergyBonus();
        }

        /// <summary>
        /// 检查某宠物当前是否有协同加成
        /// </summary>
        public bool HasActiveSynergyBonus(string petId)
        {
            return _database.HasActiveSynergyBonus(petId);
        }

        /// <summary>
        /// 获取协同加成数值
        /// </summary>
        public float GetSynergyBonus(string petId)
        {
            return _database.HasActiveSynergyBonus(petId) ? PetDepartureDatabase.SYNERGY_BONUS : 0f;
        }

        /// <summary>
        /// 获取档案总数（含已归队）
        /// </summary>
        public int GetRecordCount()
        {
            return _database.GetAllRecords().Count;
        }

        #endregion

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["departure_database"] = _database.ExportSaveData();
            data["action_stats"] = _actionStats.ExportSaveData();
            data["last_active_pet_id"] = _lastActivePetId ?? "";
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.TryGetValue("departure_database", out var dbObj) && dbObj is Dictionary<string, object> dbData)
            {
                _database.ImportSaveData(dbData);
            }

            if (data.TryGetValue("action_stats", out var statsObj) && statsObj is Dictionary<string, object> statsData)
            {
                _actionStats.ImportSaveData(statsData);
            }

            if (data.TryGetValue("last_active_pet_id", out var lastId))
            {
                _lastActivePetId = lastId?.ToString() ?? "";
            }

            _initialized = false; // 等待下一帧重新同步
        }

        #endregion
    }
}
