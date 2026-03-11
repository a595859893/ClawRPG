using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    public class ArtifactSystem : Node
    {
        public static ArtifactSystem Instance { get; private set; }

        private PlayerArtifactData playerData = new PlayerArtifactData();
        private List<ActiveArtifactBuff> activeBuffs = new List<ActiveArtifactBuff>();
        
        // Signals
        public static string SignalArtifactUnlocked = "artifact_unlocked";
        public static string SignalArtifactEquipped = "artifact_equipped";
        public static string SignalArtifactUnequipped = "artifact_unequipped";
        public static string SignalBuffActivated = "buff_activated";
        public static string SignalBuffExpired = "buff_expired";

        public override void _Ready()
        {
            Instance = this;
            ArtifactDatabase.Initialize();
            LoadData();
        }

        public void LoadData()
        {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
            if (saveSystem != null)
            {
                var data = saveSystem.GetCustomData("ArtifactSystem");
                if (data != null)
                {
                    playerData = JsonUtility.FromJson<PlayerArtifactData>(data);
                }
            }
        }

        public void SaveData()
        {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
            if (saveSystem != null)
            {
                saveSystem.SetCustomData("ArtifactSystem", JsonUtility.ToJson(playerData));
            }
        }

        // Unlock artifact
        public bool UnlockArtifact(string artifactId)
        {
            if (playerData.UnlockedArtifactIds.Contains(artifactId))
                return false;

            var artifact = ArtifactDatabase.GetArtifact(artifactId);
            if (artifact == null) return false;

            playerData.UnlockedArtifactIds.Add(artifactId);
            playerData.TotalArtifacts++;
            
            if (artifact.Rarity == ArtifactRarity.Legendary)
                playerData.LegendaryFound++;
            if (artifact.Rarity == ArtifactRarity.Mythical)
                playerData.MythicalFound++;

            if (!playerData.ArtifactCount.ContainsKey(artifactId))
                playerData.ArtifactCount[artifactId] = 0;
            playerData.ArtifactCount[artifactId]++;

            SaveData();
            
            EmitSignal(SignalArtifactUnlocked, artifactId);
            GD.Print($"[ArtifactSystem] Unlocked: {artifact.Name} ({artifact.Rarity})");
            return true;
        }

        // Equip artifact
        public bool EquipArtifact(string artifactId)
        {
            if (!playerData.UnlockedArtifactIds.Contains(artifactId))
                return false;

            if (playerData.EquippedArtifactIds.Contains(artifactId))
                return false;

            // Check type limit (max 3 of each type)
            var artifact = ArtifactDatabase.GetArtifact(artifactId);
            int typeCount = 0;
            foreach (var equippedId in playerData.EquippedArtifactIds)
            {
                var equipped = ArtifactDatabase.GetArtifact(equippedId);
                if (equipped != null && equipped.Type == artifact.Type)
                    typeCount++;
            }

            if (typeCount >= 3) return false;

            playerData.EquippedArtifactIds.Add(artifactId);
            SaveData();
            
            ApplyArtifactStats(artifactId, true);
            EmitSignal(SignalArtifactEquipped, artifactId);
            return true;
        }

        // Unequip artifact
        public bool UnequipArtifact(string artifactId)
        {
            if (!playerData.EquippedArtifactIds.Contains(artifactId))
                return false;

            playerData.EquippedArtifactIds.Remove(artifactId);
            SaveData();
            
            ApplyArtifactStats(artifactId, false);
            EmitSignal(SignalArtifactUnequipped, artifactId);
            return true;
        }

        // Apply/remove artifact stats
        private void ApplyArtifactStats(string artifactId, bool apply)
        {
            var artifact = ArtifactDatabase.GetArtifact(artifactId);
            if (artifact == null) return;

            var player = GetNode<Player>("/root/Player");
            if (player == null) return;

            float multiplier = apply ? 1.0f : -1.0f;

            foreach (var effect in artifact.Effects)
            {
                switch (effect.StatName)
                {
                    case "attack":
                        player.attackPower = (int)(player.attackPower + effect.Value * multiplier);
                        break;
                    case "defense":
                        player.defense = (int)(player.defense + effect.Value * multiplier);
                        break;
                    case "health":
                        player.maxHealth = (int)(player.maxHealth + effect.Value * multiplier);
                        player.health = Mathf.Min(player.health, player.maxHealth);
                        break;
                    case "magic":
                        // Player magic stat
                        break;
                    case "speed":
                        // Player speed stat
                        break;
                    case "crit_rate":
                        // Player crit rate
                        break;
                    case "crit_damage":
                        // Player crit damage
                        break;
                    case "lifesteal":
                        // Player lifesteal
                        break;
                    case "dodge":
                        // Player dodge
                        break;
                    case "mp_max":
                        // Player mp max
                        break;
                }
            }
        }

        // Generate random artifact (for rewards/drops)
        public string GenerateRandomArtifact(float playerLuck = 0)
        {
            var artifact = ArtifactDatabase.GenerateRandomArtifact(playerLuck);
            if (artifact != null)
            {
                UnlockArtifact(artifact.Id);
                return artifact.Id;
            }
            return null;
        }

        // Get set bonus
        public float GetSetBonus(string setId)
        {
            int count = 0;
            foreach (var equippedId in playerData.EquippedArtifactIds)
            {
                var artifact = ArtifactDatabase.GetArtifact(equippedId);
                if (artifact != null && artifact.SetId == setId)
                    count++;
            }
            return count * 0.1f; // 10% per artifact
        }

        // Get all set bonuses
        public Dictionary<string, float> GetAllSetBonuses()
        {
            var bonuses = new Dictionary<string, float>();
            var sets = ArtifactDatabase.GetAllSets();
            
            foreach (var set in sets.Keys)
            {
                float bonus = GetSetBonus(set);
                if (bonus > 0)
                    bonuses[set] = bonus;
            }
            return bonuses;
        }

        // Get player data
        public PlayerArtifactData GetPlayerData() => playerData;

        public List<Artifact> GetUnlockedArtifacts()
        {
            var result = new List<Artifact>();
            foreach (var id in playerData.UnlockedArtifactIds)
            {
                var artifact = ArtifactDatabase.GetArtifact(id);
                if (artifact != null)
                    result.Add(artifact);
            }
            return result;
        }

        public List<Artifact> GetEquippedArtifacts()
        {
            var result = new List<Artifact>();
            foreach (var id in playerData.EquippedArtifactIds)
            {
                var artifact = ArtifactDatabase.GetArtifact(id);
                if (artifact != null)
                    result.Add(artifact);
            }
            return result;
        }

        // Get statistics
        public Dictionary<string, int> GetStatistics()
        {
            var stats = new Dictionary<string, int>
            {
                ["total_unlocked"] = playerData.UnlockedArtifactIds.Count,
                ["total_equipped"] = playerData.EquippedArtifactIds.Count,
                ["common_count"] = 0,
                ["uncommon_count"] = 0,
                ["rare_count"] = 0,
                ["epic_count"] = 0,
                ["legendary_count"] = playerData.LegendaryFound,
                ["mythical_count"] = playerData.MythicalFound,
                ["weapon_count"] = 0,
                ["armor_count"] = 0,
                ["accessory_count"] = 0,
                ["relic_count"] = 0
            };

            foreach (var id in playerData.UnlockedArtifactIds)
            {
                var artifact = ArtifactDatabase.GetArtifact(id);
                if (artifact == null) continue;

                switch (artifact.Rarity)
                {
                    case ArtifactRarity.Common: stats["common_count"]++; break;
                    case ArtifactRarity.Uncommon: stats["uncommon_count"]++; break;
                    case ArtifactRarity.Rare: stats["rare_count"]++; break;
                    case ArtifactRarity.Epic: stats["epic_count"]++; break;
                }

                switch (artifact.Type)
                {
                    case ArtifactType.Weapon: stats["weapon_count"]++; break;
                    case ArtifactType.Armor: stats["armor_count"]++; break;
                    case ArtifactType.Accessory: stats["accessory_count"]++; break;
                    case ArtifactType.Relic: stats["relic_count"]++; break;
                }
            }

            return stats;
        }

        // Check if all artifacts of a rarity collected
        public bool IsRarityComplete(ArtifactRarity rarity)
        {
            var allRarity = ArtifactDatabase.GetArtifactsByRarity(rarity);
            int unlocked = 0;
            foreach (var a in allRarity)
            {
                if (playerData.UnlockedArtifactIds.Contains(a.Id))
                    unlocked++;
            }
            return unlocked >= allRarity.Count;
        }

        // Get collection progress
        public float GetCollectionProgress()
        {
            var all = ArtifactDatabase.GetAllArtifacts();
            if (all.Count == 0) return 0;
            return (float)playerData.UnlockedArtifactIds.Count / all.Count;
        }
    }
}
