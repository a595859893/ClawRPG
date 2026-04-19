using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.Ripple;

namespace ClawRPG.Systems.PetObituary
{
    /// <summary>
    /// 宠物战斗讣告系统（REQ-191）
    /// 宠物死亡时，基于历史数据生成叙事讣告，显示在基地墓碑旁的讣告板上
    /// </summary>
    public partial class PetObituarySystem : BaseSystem
    {
        private static PetObituarySystem _instance;
        public static PetObituarySystem Instance => _instance;

        // 已记录的讣告列表（跨局次持久化）
        private List<ObituaryEntry> _obituaryEntries = new List<ObituaryEntry>();

        // Signals
        public delegate void ObituaryAddedEventHandler(ObituaryEntry entry);
        public event ObituaryAddedEventHandler OnObituaryAdded;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            SubscribeToSignals();
        }

        private void SubscribeToSignals()
        {
            // 订阅宠物死亡信号（来自 PetCombatCompanion）
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petCompanion != null && petCompanion.HasSignal("PetDied"))
            {
                petCompanion.Connect("PetDied", Callable.From<Godot.Collections.Dictionary>(OnPetDied));
            }

            // 订阅战斗开始/结束信号（来自 CombatManager）
            var combatManager = GetNodeOrNull<Godot.Node>("/root/CombatManager");
            if (combatManager != null)
            {
                if (combatManager.HasSignal("CombatStarted"))
                    combatManager.Connect("CombatStarted", Callable.From(OnCombatStarted));
                if (combatManager.HasSignal("CombatEnded"))
                    combatManager.Connect("CombatEnded", Callable.From<Godot.Collections.Dictionary>(OnCombatEnded));
            }
        }

        private void OnCombatStarted()
        {
            // 通知 PetCombatCompanionSystem 记录战斗开始
            var petSystem = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petSystem != null && petSystem.HasMethod("OnBattleStarted"))
            {
                petSystem.Call("OnBattleStarted");
            }
        }

        private void OnCombatEnded(Godot.Collections.Dictionary battleData)
        {
            // 通知 PetCombatCompanionSystem 记录战斗结束
            string firstCombo = battleData.ContainsKey("first_combo") 
                ? battleData["first_combo"].ToString() 
                : "basic_attack";
            var petSystem = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petSystem != null && petSystem.HasMethod("OnBattleEnded"))
            {
                petSystem.Call("OnBattleEnded", firstCombo);
            }
        }

        private void OnPetDied(Godot.Collections.Dictionary petData)
        {
            string petId = petData.ContainsKey("pet_id") ? petData["pet_id"].ToString() : "";
            string petName = petData.ContainsKey("pet_name") ? petData["pet_name"].ToString() : "Unknown Pet";
            string petColor = petData.ContainsKey("pet_color") ? petData["pet_color"].ToString() : "#FFFFFF";
            int friendshipLevel = petData.ContainsKey("friendship") ? Convert.ToInt32(petData["friendship"]) : 0;
            string battleId = petData.ContainsKey("battle_id") ? petData["battle_id"].ToString() : "";

            // REQ-210-06: 涟漪集成 — 宠物牺牲
            RippleIntegration.AddRipple(RippleType.Sacrifice, 1);

            GenerateAndRecordObituary(petId, petName, petColor, friendshipLevel, battleId);
        }

        /// <summary>
        /// 生成并记录讣告
        /// </summary>
        public void GenerateAndRecordObituary(string petId, string petName, string petColor, int friendshipLevel, string battleId)
        {
            // 从 PetCombatCompanionSystem 获取讣告数据
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            ClawRPG.Scripts.Data.PetObituaryData obituaryData = null;

            if (petCompanion != null && petCompanion.HasMethod("GetObituaryData"))
            {
                obituaryData = (ClawRPG.Scripts.Data.PetObituaryData)petCompanion.Call("GetObituaryData", petId);
            }

            obituaryData ??= new ClawRPG.Scripts.Data.PetObituaryData();

            string obituaryText = GenerateObituaryText(petName, obituaryData, friendshipLevel);

            var entry = new ObituaryEntry
            {
                PetId = petId,
                PetName = petName,
                PetColor = petColor,
                FriendshipLevel = friendshipLevel,
                ObituaryText = obituaryText,
                DeathTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                TotalBattles = obituaryData.TotalBattles,
                MostUsedCombo = obituaryData.MostUsedCombo,
                TotalEnemiesKilled = obituaryData.TotalEnemiesKilled
            };

            _obituaryEntries.Add(entry);
            OnObituaryAdded?.Invoke(entry);

            GD.Print($"[PetObituary] Obituary recorded for {petName} ({obituaryData.TotalBattles} battles, {obituaryData.TotalEnemiesKilled} kills)");
        }

        /// <summary>
        /// 生成讣告文本
        /// </summary>
        private string GenerateObituaryText(string petName, ClawRPG.Scripts.Data.PetObituaryData data, int friendshipLevel)
        {
            string comboStr = string.IsNullOrEmpty(data.MostUsedCombo) ? "basic attacks" : data.MostUsedCombo.Replace("→", " → ");
            string battleTimeStr = FormatBattleTime(data.TotalBattleTimeSeconds);

            string text = $"「{petName}」\\n\\n" +
                $"It fought in {data.TotalBattles} battles,\\n" +
                $"its signature combo was {comboStr},\\n" +
                $"and it assisted in {data.TotalEnemiesKilled} enemy defeats.\\n\\n";

            // 数据充足时的变体
            if (data.TotalBattles >= 5)
            {
                text += $"It fought for a total of {battleTimeStr}.\\n";
            }

            if (data.PeakStreak > 3)
            {
                text += $"Its finest hour was a ×{data.PeakStreak} winning streak.\\n";
            }

            if (data.FirstBattleTimestamp > 0 && data.LastBattleTimestamp > 0)
            {
                var firstDate = DateTimeOffset.FromUnixTimeSeconds(data.FirstBattleTimestamp).LocalDateTime;
                var lastDate = DateTimeOffset.FromUnixTimeSeconds(data.LastBattleTimestamp).LocalDateTime;
                string duration = (lastDate - firstDate).Days >= 1
                    ? $"{((lastDate - firstDate).Days)} days"
                    : "the same day";
                text += $"It lived for {duration}.\\n";
            }

            // 结束语
            text += $"\\nIt fought bravely until the end.\\nIts story lives on here.";

            return text;
        }

        private string FormatBattleTime(double totalSeconds)
        {
            int hours = (int)(totalSeconds / 3600);
            int minutes = (int)((totalSeconds % 3600) / 60);
            if (hours > 0)
                return $"{hours}h {minutes}m";
            if (minutes > 0)
                return $"{minutes}m";
            return $"{(int)totalSeconds}s";
        }

        /// <summary>
        /// 获取所有讣告记录
        /// </summary>
        public List<ObituaryEntry> GetAllObituaries()
        {
            return new List<ObituaryEntry>(_obituaryEntries);
        }

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            var entriesData = new List<Dictionary<string, object>>();

            foreach (var entry in _obituaryEntries)
            {
                entriesData.Add(new Dictionary<string, object>
                {
                    ["pet_id"] = entry.PetId,
                    ["pet_name"] = entry.PetName,
                    ["pet_color"] = entry.PetColor,
                    ["friendship_level"] = entry.FriendshipLevel,
                    ["obituary_text"] = entry.ObituaryText,
                    ["death_timestamp"] = entry.DeathTimestamp,
                    ["total_battles"] = entry.TotalBattles,
                    ["most_used_combo"] = entry.MostUsedCombo ?? "",
                    ["total_enemies_killed"] = entry.TotalEnemiesKilled
                });
            }

            data["obituary_entries"] = entriesData;
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null || !data.ContainsKey("obituary_entries")) return;

            try
            {
                var entriesData = (Godot.Collections.Array)data["obituary_entries"];
                _obituaryEntries.Clear();

                foreach (var item in entriesData)
                {
                    var dict = (Dictionary<string, object>)item;
                    _obituaryEntries.Add(new ObituaryEntry
                    {
                        PetId = dict.GetValueOrDefault("pet_id", "").ToString(),
                        PetName = dict.GetValueOrDefault("pet_name", "").ToString(),
                        PetColor = dict.GetValueOrDefault("pet_color", "#FFFFFF").ToString(),
                        FriendshipLevel = Convert.ToInt32(dict.GetValueOrDefault("friendship_level", 0)),
                        ObituaryText = dict.GetValueOrDefault("obituary_text", "").ToString(),
                        DeathTimestamp = Convert.ToInt64(dict.GetValueOrDefault("death_timestamp", 0L)),
                        TotalBattles = Convert.ToInt32(dict.GetValueOrDefault("total_battles", 0)),
                        MostUsedCombo = dict.GetValueOrDefault("most_used_combo", "").ToString(),
                        TotalEnemiesKilled = Convert.ToInt32(dict.GetValueOrDefault("total_enemies_killed", 0))
                    });
                }

                GD.Print($"[PetObituary] Loaded {_obituaryEntries.Count} obituary entries");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PetObituary] ImportSaveData failed: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// 单条讣告记录
    /// </summary>
    public class ObituaryEntry
    {
        public string PetId { get; set; }
        public string PetName { get; set; }
        public string PetColor { get; set; }
        public int FriendshipLevel { get; set; }
        public string ObituaryText { get; set; }
        public long DeathTimestamp { get; set; }
        public int TotalBattles { get; set; }
        public string MostUsedCombo { get; set; }
        public int TotalEnemiesKilled { get; set; }

        public string GetDeathDateString()
        {
            if (DeathTimestamp <= 0) return "Unknown";
            return DateTimeOffset.FromUnixTimeSeconds(DeathTimestamp).LocalDateTime.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
