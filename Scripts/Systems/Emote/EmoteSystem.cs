using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Systems.Emote {
    public partial class EmoteSystem : BaseSystem {
        public static EmoteSystem Instance { get; private set; }

        private PlayerEmoteData playerData = new PlayerEmoteData();
        
        // Signals
        public static Signal<string> EmoteUnlocked { get; } = new Signal<string>();
        public static Signal<string> EmoteUsed { get; } = new Signal<string>();
        public static Signal<string> FavoriteEmoteAdded { get; } = new Signal<string>();
        public static Signal<string> FavoriteEmoteRemoved { get; } = new Signal<string>();

        public override void _Ready() {
            Instance = this;
            EmoteDatabase.Initialize();
            UnlockDefaultEmotes();
        }

        private void UnlockDefaultEmotes() {
            var defaultEmotes = EmoteDatabase.GetDefaultEmotes();
            foreach (var emote in defaultEmotes) {
                if (!playerData.UnlockedEmotes.Contains(emote.Id)) {
                    playerData.UnlockedEmotes.Add(emote.Id);
                }
            }
        }

        public bool UnlockEmote(string emoteId, bool free = false) {
            var emote = EmoteDatabase.GetEmote(emoteId);
            if (emote == null) return false;
            
            if (playerData.UnlockedEmotes.Contains(emoteId)) return true;
            
            if (!free) {
                var player = GetTree().GetFirstNodeInGroup("player");
                if (player == null) return false;
                
                // Need gold to purchase
                // Assuming player has GetGold() method
                // if (!player.Call("GetGold", emote.Cost)) return false;
                // player.Call("AddGold", -emote.Cost);
            }
            
            playerData.UnlockedEmotes.Add(emoteId);
            EmoteUnlocked(emoteId);
            return true;
        }

        public bool UseEmote(string emoteId) {
            if (!playerData.UnlockedEmotes.Contains(emoteId)) return false;
            
            var emote = EmoteDatabase.GetEmote(emoteId);
            if (emote == null) return false;
            
            // Update usage count
            if (!playerData.EmoteUsageCount.ContainsKey(emoteId)) {
                playerData.EmoteUsageCount[emoteId] = 0;
            }
            playerData.EmoteUsageCount[emoteId]++;
            playerData.LastUsedEmote = emoteId;
            
            EmoteUsed(emoteId);
            
            // Broadcast to nearby players (multiplayer)
            // RpcId method would go here for multiplayer
            DisplayEmote(emote);
            
            return true;
        }

        private void DisplayEmote(Emote emote) {
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player == null) return;
            
            // Create visual emote effect above player
            // This would create a Label3D or Sprite3D with the emote
            // For now, just print to console
            GD.Print($"[Emote] Player used: {emote.Name} - {emote.Description}");
            
            // Show floating text
            // var floatingLabel = CreateFloatingLabel(emote.Name, player.GlobalPosition + new Vector3(0, 2, 0));
        }

        public bool AddFavorite(string emoteId) {
            if (!playerData.UnlockedEmotes.Contains(emoteId)) return false;
            if (playerData.FavoriteEmotes.Contains(emoteId)) return false;
            
            playerData.FavoriteEmotes.Add(emoteId);
            FavoriteEmoteAdded(emoteId);
            return true;
        }

        public bool RemoveFavorite(string emoteId) {
            if (!playerData.FavoriteEmotes.Contains(emoteId)) return false;
            
            playerData.FavoriteEmotes.Remove(emoteId);
            FavoriteEmoteRemoved(emoteId);
            return true;
        }

        public List<Emote> GetUnlockedEmotes() {
            return playerData.UnlockedEmotes
                .Select(id => EmoteDatabase.GetEmote(id))
                .Where(e => e != null)
                .ToList();
        }

        public List<Emote> GetFavoriteEmotes() {
            return playerData.FavoriteEmotes
                .Select(id => EmoteDatabase.GetEmote(id))
                .Where(e => e != null)
                .ToList();
        }

        public List<Emote> GetShopEmotes() {
            return EmoteDatabase.GetShopEmotes()
                .Where(e => !playerData.UnlockedEmotes.Contains(e.Id))
                .ToList();
        }

        public Dictionary<string, int> GetUsageStatistics() {
            return new Dictionary<string, int>(playerData.EmoteUsageCount);
        }

        public string GetMostUsedEmote() {
            if (playerData.EmoteUsageCount.Count == 0) return null;
            
            return playerData.EmoteUsageCount
                .OrderByDescending(kvp => kvp.Value)
                .First().Key;
        }

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData() {
            var data = new Dictionary();
            data["emote_unlocked"] = new Godot.Array(playerData.UnlockedEmotes);
            data["emote_favorites"] = new Godot.Array(playerData.FavoriteEmotes);
            
            var usageDict = new Dictionary();
            foreach (var kvp in playerData.EmoteUsageCount) {
                usageDict[kvp.Key] = kvp.Value;
            }
            data["emote_usage"] = usageDict;
            data["emote_last_used"] = playerData.LastUsedEmote;
            
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data) {
            if (data == null) return;
            
            if (data.Contains("emote_unlocked")) {
                playerData.UnlockedEmotes = ((Godot.Array)data["emote_unlocked"])
                    .Select(v => (string)v).ToList();
            }
            if (data.Contains("emote_favorites")) {
                playerData.FavoriteEmotes = ((Godot.Array)data["emote_favorites"])
                    .Select(v => (string)v).ToList();
            }
            if (data.Contains("emote_usage")) {
                var usageDict = (Dictionary)data["emote_usage"];
                playerData.EmoteUsageCount = new Dictionary<string, int>();
                foreach (var kvp in usageDict) {
                    playerData.EmoteUsageCount[kvp.Key] = (int)(long)kvp.Value;
                }
            }
            if (data.Contains("emote_last_used")) {
                playerData.LastUsedEmote = (string)data["emote_last_used"];
            }
        }

        public void SaveData(Dictionary<string, object> data) {
            data["emote_unlocked"] = playerData.UnlockedEmotes;
            data["emote_favorites"] = playerData.FavoriteEmotes;
            data["emote_usage"] = playerData.EmoteUsageCount;
            data["emote_last_used"] = playerData.LastUsedEmote;
        }

        public void LoadData(Dictionary<string, object> data) {
            if (data.ContainsKey("emote_unlocked")) {
                playerData.UnlockedEmotes = ((Godot.Collections.Array)data["emote_unlocked"])
                    .Select(v => (string)v).ToList();
            }
            if (data.ContainsKey("emote_favorites")) {
                playerData.FavoriteEmotes = ((Godot.Collections.Array)data["emote_favorites"])
                    .Select(v => (string)v).ToList();
            }
            if (data.ContainsKey("emote_usage")) {
                playerData.EmoteUsageCount = ((Godot.Collections.Dictionary)data["emote_usage"])
                    .ToDictionary(kvp => (string)kvp.Key, kvp => (int)(long)kvp.Value);
            }
            if (data.ContainsKey("emote_last_used")) {
                playerData.LastUsedEmote = (string)data["emote_last_used"];
            }
        }
    }
}
