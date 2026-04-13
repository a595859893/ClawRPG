using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetLegacy
{
    /// <summary>
    /// 宠物死亡遗产系统
    /// 宠物战死后变成墓碑/灵魂留在场上，继续提供微弱被动效果
    /// </summary>
    public partial class PetLegacySystem : BaseSystem
    {
        private static PetLegacySystem _instance;
        public static PetLegacySystem Instance => _instance;

        private PetLegacyDatabase _database;

        // 被动效果配置
        private const float LEGACY_DAMAGE_BONUS = 0.05f;  // +5%基础伤害
        private const int MAX_ACTIVE_MARKERS = 3;          // 最多3个激活标记

        // Signals
        public delegate void LegacyMarkerAddedEventHandler(PetLegacyMarkerData marker);
        public delegate void LegacyMarkerClickedEventHandler(int petId);
        public delegate void LegacyBonusChangedEventHandler(int activeCount, float bonus);

        public event LegacyMarkerAddedEventHandler OnLegacyMarkerAdded;
        public event LegacyMarkerClickedEventHandler OnLegacyMarkerClicked;
        public event LegacyBonusChangedEventHandler OnLegacyBonusChanged;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _database = new PetLegacyDatabase();
            SubscribeToSignals();
        }

        /// <summary>
        /// 订阅相关信号
        /// </summary>
        private void SubscribeToSignals()
        {
            // 订阅宠物死亡信号
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petCompanion != null && petCompanion.HasSignal("PetDied"))
            {
                petCompanion.Connect("PetDied", Callable.From<Godot.Collections.Dictionary>(OnPetDiedHandler));
            }

            // 订阅战斗开始信号，用于激活标记增益
            var combatManager = GetNodeOrNull<Godot.Node>("/root/CombatManager");
            if (combatManager != null && combatManager.HasSignal("CombatStarted"))
            {
                combatManager.Connect("CombatStarted", Callable.From(OnCombatStartedHandler));
            }
        }

        /// <summary>
        /// 处理宠物死亡事件
        /// </summary>
        private void OnPetDiedHandler(Godot.Collections.Dictionary petData)
        {
            int petId = petData.ContainsKey("pet_id") ? Convert.ToInt32(petData["pet_id"]) : 0;
            string petName = petData.ContainsKey("pet_name") ? petData["pet_name"].ToString() : "Unknown";
            string petColor = petData.ContainsKey("pet_color") ? petData["pet_color"].ToString() : "#FFFFFF";
            int friendshipLevel = petData.ContainsKey("friendship") ? Convert.ToInt32(petData["friendship"]) : 0;
            int totalBattles = petData.ContainsKey("total_battles") ? Convert.ToInt32(petData["total_battles"]) : 0;
            string battleId = petData.ContainsKey("battle_id") ? petData["battle_id"].ToString() : "";

            // 确定标记类型：高友谊 = 灵魂光球
            LegacyType markerType = friendshipLevel >= 15 ? LegacyType.Soul : LegacyType.Tombstone;

            var marker = new PetLegacyMarkerData(
                petId, petName, petColor, battleId,
                (float)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                markerType, friendshipLevel, totalBattles
            );

            AddLegacyMarker(marker);
        }

        /// <summary>
        /// 添加遗产标记
        /// </summary>
        public void AddLegacyMarker(PetLegacyMarkerData marker)
        {
            _database.AddMarker(marker);
            OnLegacyMarkerAdded?.Invoke(marker);
            UpdateLegacyBonus();
            GD.Print($"[PetLegacy] Added marker for {marker.PetName} (Type: {marker.MarkerType})");
        }

        /// <summary>
        /// 战斗开始时激活增益
        /// </summary>
        private void OnCombatStartedHandler()
        {
            UpdateLegacyBonus();
        }

        /// <summary>
        /// 更新遗产增益并通知战斗系统
        /// </summary>
        private void UpdateLegacyBonus()
        {
            var activeMarkers = _database.GetActiveMarkers();
            int count = activeMarkers.Count;
            float bonus = count * LEGACY_DAMAGE_BONUS;  // 每个标记 +5%

            OnLegacyBonusChanged?.Invoke(count, bonus);

            // 通知伤害计算系统应用增益
            var combatSys = GetNodeOrNull<Godot.Node>("/root/CombatSystem");
            if (combatSys != null)
            {
                combatSys.Set("LegacyDamageBonus", bonus);
            }
        }

        /// <summary>
        /// 获取当前激活标记数量
        /// </summary>
        public int GetActiveMarkerCount() => _database.ActiveMarkerIds.Count;

        /// <summary>
        /// 获取当前激活的标记列表
        /// </summary>
        public List<PetLegacyMarkerData> GetActiveMarkers() => _database.GetActiveMarkers();

        /// <summary>
        /// 点击遗产标记，显示宠物小传
        /// </summary>
        public void OnMarkerClicked(int petId)
        {
            OnLegacyMarkerClicked?.Invoke(petId);
        }

        /// <summary>
        /// 获取指定宠物的遗产标记
        /// </summary>
        public PetLegacyMarkerData GetMarker(int petId) => _database.GetMarkerByPetId(petId);

        /// <summary>
        /// 触发祭拜特效（无实际加成，仅视觉）
        /// </summary>
        public void OfferIncenseToMarker(int petId)
        {
            var marker = _database.GetMarkerByPetId(petId);
            if (marker != null)
            {
                GD.Print($"[PetLegacy] Offering incense to {marker.PetName}");
                // 祭拜特效由UI层处理，此处只记录
            }
        }

        /// <summary>
        /// 获取所有休眠标记
        /// </summary>
        public List<PetLegacyMarkerData> GetDormantMarkers()
        {
            var result = new List<PetLegacyMarkerData>();
            foreach (var id in _database.DormantMarkerIds)
            {
                var marker = _database.GetMarkerByPetId(id);
                if (marker != null)
                    result.Add(marker);
            }
            return result;
        }

        // ========== Persistence ==========

        public override Dictionary<string, object> ExportSaveData()
        {
            var saveData = new PetLegacySaveData
            {
                Markers = _database.Markers,
                ActiveMarkerIds = _database.ActiveMarkerIds,
                DormantMarkerIds = _database.DormantMarkerIds
            };

            return new Dictionary<string, object>
            {
                { "markers", saveData.Markers },
                { "active_ids", saveData.ActiveMarkerIds },
                { "dormant_ids", saveData.DormantMarkerIds }
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null || !data.ContainsKey("markers")) return;

            try
            {
                _database = new PetLegacyDatabase();

                // 恢复标记
                var markers = (List<object>)data["markers"];
                foreach (var m in markers)
                {
                    var markerDict = (Dictionary<string, object>)m;
                    var marker = new PetLegacyMarkerData
                    {
                        PetId = Convert.ToInt32(markerDict.GetValueOrDefault("PetId", 0)),
                        PetName = markerDict.GetValueOrDefault("PetName", "").ToString(),
                        PetColor = markerDict.GetValueOrDefault("PetColor", "#FFFFFF").ToString(),
                        DeathBattleId = markerDict.GetValueOrDefault("DeathBattleId", "").ToString(),
                        DeathTimestamp = Convert.ToSingle(markerDict.GetValueOrDefault("DeathTimestamp", 0f)),
                        MarkerType = (LegacyType)Convert.ToInt32(markerDict.GetValueOrDefault("MarkerType", 0)),
                        FriendshipLevel = Convert.ToInt32(markerDict.GetValueOrDefault("FriendshipLevel", 0)),
                        TotalBattles = Convert.ToInt32(markerDict.GetValueOrDefault("TotalBattles", 0)),
                        IsDormant = Convert.ToBoolean(markerDict.GetValueOrDefault("IsDormant", false))
                    };
                    _database.Markers.Add(marker);
                }

                // 恢复激活/休眠列表
                if (data.ContainsKey("active_ids"))
                    _database.ActiveMarkerIds = ((List<object>)data["active_ids"])
                        .ConvertAll(x => Convert.ToInt32(x));
                if (data.ContainsKey("dormant_ids"))
                    _database.DormantMarkerIds = ((List<object>)data["dormant_ids"])
                        .ConvertAll(x => Convert.ToInt32(x));

                GD.Print($"[PetLegacy] Loaded {_database.Markers.Count} markers, {_database.ActiveMarkerIds.Count} active");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PetLegacy] ImportSaveData failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 清除所有遗产数据（用于新游戏）
        /// </summary>
        public void ResetLegacyData()
        {
            _database = new PetLegacyDatabase();
            GD.Print("[PetLegacy] Legacy data reset");
        }
    }
}
