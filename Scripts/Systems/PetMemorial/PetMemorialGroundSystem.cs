using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetMemorial
{
    /// <summary>
    /// 宠物墓园系统（REQ-201）
    /// 宠物叙事的终极终点站，整合 PetSoul/PetLegacy/PetObituary 等子系统的死亡数据
    /// </summary>
    public partial class PetMemorialGroundSystem : BaseSystem
    {
        private static PetMemorialGroundSystem _instance;
        public static PetMemorialGroundSystem Instance => _instance;

        private PetMemorialDatabase _database;

        // Signals
        public delegate void MemorialUnlockedEventHandler();
        public delegate void MarkerAddedEventHandler(MemorialMarkerEntry marker);
        public delegate void MarkerClickedEventHandler(int petId);
        public delegate void CollectiveMonumentUnveiledEventHandler();

        public event MemorialUnlockedEventHandler OnMemorialUnlocked;
        public event MarkerAddedEventHandler OnMarkerAdded;
        public event MarkerClickedEventHandler OnMarkerClicked;
        public event CollectiveMonumentUnveiledEventHandler OnCollectiveMonumentUnveiled;

        /// <summary>集体纪念碑解锁阈值（累计战死N只）</summary>
        private const int COLLECTIVE_MONUMENT_THRESHOLD = 5;

        private bool _collectiveMonumentUnveiled = false;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _database = new PetMemorialDatabase();
            SubscribeToSignals();
        }

        /// <summary>
        /// 订阅宠物死亡信号，整合各子系统数据
        /// </summary>
        private void SubscribeToSignals()
        {
            // 订阅 PetSoulGhostSystem 的灵魂升华信号（REQ-195）
            var soulSystem = GetNodeOrNull<Godot.Node>("/root/PetSoulGhostSystem");
            if (soulSystem != null)
            {
                if (soulSystem.HasSignal("SoulTranscended"))
                    soulSystem.Connect("SoulTranscended", Callable.From<int>(OnSoulTranscended), (uint)ConnectFlags.Deferred);
            }

            // 订阅宠物死亡信号（PetCombatCompanion）
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petCompanion != null && petCompanion.HasSignal("PetDied"))
            {
                petCompanion.Connect("PetDied", Callable.From<Godot.Collections.Dictionary>(OnPetDied), (uint)ConnectFlags.Deferred);
            }
        }

        /// <summary>
        /// 处理宠物死亡 — 聚合所有来源数据创建墓碑条目
        /// </summary>
        private void OnPetDied(Godot.Collections.Dictionary petData)
        {
            int petId = petData.ContainsKey("pet_id") ? Convert.ToInt32(petData["pet_id"]) : 0;
            if (petId == 0) return;

            string petName = petData.ContainsKey("pet_name") ? petData["pet_name"].ToString() : "Unknown";
            string petType = petData.ContainsKey("pet_type") ? petData["pet_type"].ToString() : "Default";
            string petColor = petData.ContainsKey("pet_color") ? petData["pet_color"].ToString() : "#FFFFFF";
            int friendshipLevel = petData.ContainsKey("friendship") ? Convert.ToInt32(petData["friendship"]) : 0;
            int totalBattles = petData.ContainsKey("total_battles") ? Convert.ToInt32(petData["total_battles"]) : 0;
            int enemiesKilled = petData.ContainsKey("enemies_killed") ? Convert.ToInt32(petData["enemies_killed"]) : 0;
            int lastBattleHp = petData.ContainsKey("last_battle_hp") ? Convert.ToInt32(petData["last_battle_hp"]) : 0;
            int lastBattleMaxHp = petData.ContainsKey("last_battle_max_hp") ? Convert.ToInt32(petData["last_battle_max_hp"]) : 100;
            bool sacrifice = petData.ContainsKey("sacrifice_death") && Convert.ToBoolean(petData["sacrifice_death"]);

            int hpPercent = lastBattleMaxHp > 0 ? (int)(lastBattleHp * 100f / lastBattleMaxHp) : 0;

            var marker = new MemorialMarkerEntry
            {
                PetId = petId,
                PetName = petName,
                PetType = petType,
                PetColor = petColor,
                TotalBattles = totalBattles,
                MostUsedCombo = GetMostUsedCombo(petId),
                FriendshipLevel = friendshipLevel,
                TotalEnemiesKilled = enemiesKilled,
                LastBattleHpPercent = hpPercent,
                IsSacrificeDeath = sacrifice,
                DeathTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // 补充 PetObituarySystem 数据
            var obituaryEntry = GetObituaryEntry(petId.ToString());
            if (obituaryEntry != null)
            {
                marker.ObituaryText = obituaryEntry.ObituaryText;
                if (string.IsNullOrEmpty(marker.MostUsedCombo) && !string.IsNullOrEmpty(obituaryEntry.MostUsedCombo))
                    marker.MostUsedCombo = obituaryEntry.MostUsedCombo;
                if (obituaryEntry.TotalBattles > marker.TotalBattles)
                    marker.TotalBattles = obituaryEntry.TotalBattles;
                if (obituaryEntry.TotalEnemiesKilled > marker.TotalEnemiesKilled)
                    marker.TotalEnemiesKilled = obituaryEntry.TotalEnemiesKilled;
            }

            // 补充 PetSoulGhostSystem 数据（升华状态）
            var ghostEntry = GetSoulGhostEntry(petId);
            if (ghostEntry != null)
            {
                marker.IsTranscended = ghostEntry.IsTranscended;
                marker.TranscendedTimestamp = (long)ghostEntry.TranscendedTimestamp;
            }

            _database.AddOrUpdateMarker(marker);

            // 第一次解锁墓园
            if (!_database.IsUnlocked)
            {
                _database.IsUnlocked = true;
                OnMemorialUnlocked?.Invoke();
                GD.Print($"[PetMemorial] Memorial Ground UNLOCKED by {petName}'s death");
            }

            // 检查集体纪念碑
            CheckCollectiveMonument();

            OnMarkerAdded?.Invoke(marker);
            GD.Print($"[PetMemorial] Marker added for {petName} ({totalBattles} battles, {enemiesKilled} kills, HP%={hpPercent})");
        }

        /// <summary>
        /// 灵魂升华回调
        /// </summary>
        private void OnSoulTranscended(int petId)
        {
            if (_database.Markers.TryGetValue(petId, out var marker))
            {
                marker.IsTranscended = true;
                marker.TranscendedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                marker.Epitaph = EpitaphGenerator.Generate(marker);
                GD.Print($"[PetMemorial] {marker.PetName} transcended, epitaph updated");
            }
        }

        /// <summary>
        /// 检查是否解锁集体纪念碑
        /// </summary>
        private void CheckCollectiveMonument()
        {
            int deathCount = _database.GetDeathCount();
            if (deathCount >= COLLECTIVE_MONUMENT_THRESHOLD && !_collectiveMonumentUnveiled)
            {
                _collectiveMonumentUnveiled = true;
                OnCollectiveMonumentUnveiled?.Invoke();
                GD.Print($"[PetMemorial] Collective monument unveiled! ({deathCount} pets have fallen)");
            }
        }

        /// <summary>
        /// 从 PetObituarySystem 获取讣告数据
        /// </summary>
        private ClawRPG.Systems.PetObituary.ObituaryEntry GetObituaryEntry(string petId)
        {
            var obituarySystem = GetNodeOrNull<Godot.Node>("/root/PetObituarySystem");
            if (obituarySystem != null && obituarySystem.HasMethod("GetAllObituaries"))
            {
                var obituaries = (List<ClawRPG.Systems.PetObituary.ObituaryEntry>)obituarySystem.Call("GetAllObituaries");
                foreach (var entry in obituaries)
                {
                    if (entry.PetId == petId) return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// 从 PetSoulGhostSystem 获取灵魂数据
        /// </summary>
        private ClawRPG.Systems.PetSoul.PetSoulGhostEntry GetSoulGhostEntry(int petId)
        {
            var soulSystem = GetNodeOrNull<Godot.Node>("/root/PetSoulGhostSystem");
            if (soulSystem != null && soulSystem.HasMethod("GetGhost"))
            {
                return (ClawRPG.Systems.PetSoul.PetSoulGhostEntry)soulSystem.Call("GetGhost", petId);
            }
            return null;
        }

        /// <summary>
        /// 从 PetCombatCompanion 获取最爱Combo
        /// </summary>
        private string GetMostUsedCombo(int petId)
        {
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petCompanion != null && petCompanion.HasMethod("GetPetMostUsedCombo"))
            {
                return petCompanion.Call("GetPetMostUsedCombo", petId)?.ToString() ?? "";
            }
            return "";
        }

        // ========== Public API ==========

        /// <summary>
        /// 检查墓园是否已解锁
        /// </summary>
        public bool IsMemorialUnlocked() => _database.IsUnlocked;

        /// <summary>
        /// 获取所有墓碑（按死亡时间倒序）
        /// </summary>
        public List<MemorialMarkerEntry> GetAllMarkers() => _database.GetAllMarkers();

        /// <summary>
        /// 获取指定墓碑
        /// </summary>
        public MemorialMarkerEntry GetMarker(int petId)
        {
            return _database.Markers.TryGetValue(petId, out var marker) ? marker : null;
        }

        /// <summary>
        /// 获取已升华的宠物墓碑
        /// </summary>
        public List<MemorialMarkerEntry> GetTranscendedMarkers() => _database.GetTranscendedMarkers();

        /// <summary>
        /// 获取累计战死数量
        /// </summary>
        public int GetTotalDeaths() => _database.GetDeathCount();

        /// <summary>
        /// 集体纪念碑是否已揭幕
        /// </summary>
        public bool IsCollectiveMonumentUnveiled() => _collectiveMonumentUnveiled;

        /// <summary>
        /// 点击墓碑（触发 MarkerClicked 信号，供 UI 调用）
        /// </summary>
        public void OnMarkerClicked_UI(int petId)
        {
            if (_database.Markers.ContainsKey(petId))
                OnMarkerClicked?.Invoke(petId);
        }

        // ========== Persistence ==========

        public override Dictionary<string, object> ExportSaveData()
        {
            var markersData = new List<Dictionary<string, object>>();
            foreach (var marker in _database.Markers.Values)
            {
                markersData.Add(new Dictionary<string, object>
                {
                    ["pet_id"] = marker.PetId,
                    ["pet_name"] = marker.PetName,
                    ["pet_type"] = marker.PetType,
                    ["pet_color"] = marker.PetColor,
                    ["total_battles"] = marker.TotalBattles,
                    ["most_used_combo"] = marker.MostUsedCombo ?? "",
                    ["friendship_level"] = marker.FriendshipLevel,
                    ["total_enemies_killed"] = marker.TotalEnemiesKilled,
                    ["last_battle_hp_percent"] = marker.LastBattleHpPercent,
                    ["is_sacrifice_death"] = marker.IsSacrificeDeath,
                    ["death_timestamp"] = marker.DeathTimestamp,
                    ["obituary_text"] = marker.ObituaryText ?? "",
                    ["is_transcended"] = marker.IsTranscended,
                    ["transcended_timestamp"] = marker.TranscendedTimestamp
                });
            }

            return new Dictionary<string, object>
            {
                ["is_unlocked"] = _database.IsUnlocked,
                ["markers"] = markersData,
                ["collective_monument_unveiled"] = _collectiveMonumentUnveiled
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            _database.IsUnlocked = data.ContainsKey("is_unlocked") && Convert.ToBoolean(data["is_unlocked"]);
            _collectiveMonumentUnveiled = data.ContainsKey("collective_monument_unveiled") && Convert.ToBoolean(data["collective_monument_unveiled"]);

            if (!data.ContainsKey("markers")) return;

            _database.Markers.Clear();
            var markersList = (List<object>)data["markers"];
            foreach (var item in markersList)
            {
                var dict = (Dictionary<string, object>)item;
                var marker = new MemorialMarkerEntry
                {
                    PetId = Convert.ToInt32(dict.GetValueOrDefault("pet_id", 0)),
                    PetName = dict.GetValueOrDefault("pet_name", "").ToString(),
                    PetType = dict.GetValueOrDefault("pet_type", "Default").ToString(),
                    PetColor = dict.GetValueOrDefault("pet_color", "#FFFFFF").ToString(),
                    TotalBattles = Convert.ToInt32(dict.GetValueOrDefault("total_battles", 0)),
                    MostUsedCombo = dict.GetValueOrDefault("most_used_combo", "").ToString(),
                    FriendshipLevel = Convert.ToInt32(dict.GetValueOrDefault("friendship_level", 0)),
                    TotalEnemiesKilled = Convert.ToInt32(dict.GetValueOrDefault("total_enemies_killed", 0)),
                    LastBattleHpPercent = Convert.ToInt32(dict.GetValueOrDefault("last_battle_hp_percent", 0)),
                    IsSacrificeDeath = Convert.ToBoolean(dict.GetValueOrDefault("is_sacrifice_death", false)),
                    DeathTimestamp = Convert.ToInt64(dict.GetValueOrDefault("death_timestamp", 0L)),
                    ObituaryText = dict.GetValueOrDefault("obituary_text", "").ToString(),
                    IsTranscended = Convert.ToBoolean(dict.GetValueOrDefault("is_transcended", false)),
                    TranscendedTimestamp = Convert.ToInt64(dict.GetValueOrDefault("transcended_timestamp", 0L))
                };
                marker.Epitaph = EpitaphGenerator.Generate(marker);
                marker.TombstoneStyle = EpitaphGenerator.GetTombstoneStyle(marker.DeathTimestamp);
                _database.Markers[marker.PetId] = marker;
            }

            GD.Print($"[PetMemorial] Loaded {_database.Markers.Count} memorial markers, unlocked={_database.IsUnlocked}");
        }
    }
}
