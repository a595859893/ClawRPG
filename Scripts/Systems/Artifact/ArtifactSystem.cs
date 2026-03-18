// ============================================
// Artifact System - 神器核心系统
// 功能：神器收集、强化、合成、套装效果
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Core.Systems
{
    public class ArtifactSystem : BaseSystem
    {
        private PlayerArtifactData _playerData;
        private ArtifactStatistics _statistics;
        
        public event EventHandler<ArtifactEventArgs> OnArtifactAcquired;
        public event EventHandler<ArtifactEventArgs> OnArtifactEquipped;
        public event EventHandler<ArtifactEventArgs> OnArtifactUnequipped;
        public event EventHandler<ForgeEventArgs> OnForgeCompleted;
        public event EventHandler<SetBonusEventArgs> OnSetBonusActivated;

        public ArtifactSystem()
        {
            _playerData = new PlayerArtifactData
            {
                OwnedArtifacts = new List<Artifact>(),
                UnlockedSets = new List<ArtifactSet>(),
                ArtifactStats = new Dictionary<string, int>(),
                TotalArtifactsCollected = 0,
                MythicArtifacts = 0
            };
            _statistics = new ArtifactStatistics();
        }

        #region Artifact Management

        public Artifact AcquireArtifact(int playerLevel)
        {
            var artifact = ArtifactDatabase.DropRandomArtifact(playerLevel);
            
            _playerData.OwnedArtifacts.Add(artifact);
            _playerData.TotalArtifactsCollected++;
            
            if (artifact.Rarity >= ArtifactRarity.Legendary)
            {
                _playerData.ArtifactStats[artifact.Rarity.ToString()] = 
                    _playerData.ArtifactStats.GetValueOrDefault(artifact.Rarity.ToString(), 0) + 1;
            }

            if (artifact.Rarity == ArtifactRarity.Mythic)
                _playerData.MythicArtifacts++;

            UpdateStatistics();
            OnArtifactAcquired?.Invoke(this, new ArtifactEventArgs(artifact));

            return artifact;
        }

        public bool EquipArtifact(string artifactId)
        {
            var artifact = _playerData.OwnedArtifacts.FirstOrDefault(a => a.Id == artifactId);
            if (artifact == null || artifact.IsEquipped) return false;

            // 检查是否有同槽位已装备
            var equipped = _playerData.OwnedArtifacts.FirstOrDefault(a => 
                a.IsEquipped && a.Slot == artifact.Slot);
            if (equipped != null)
            {
                equipped.IsEquipped = false;
                OnArtifactUnequipped?.Invoke(this, new ArtifactEventArgs(equipped));
            }

            artifact.IsEquipped = true;
            CheckSetBonus(artifact.SetId);
            OnArtifactEquipped?.Invoke(this, new ArtifactEventArgs(artifact));

            return true;
        }

        public bool UnequipArtifact(string artifactId)
        {
            var artifact = _playerData.OwnedArtifacts.FirstOrDefault(a => a.Id == artifactId);
            if (artifact == null || !artifact.IsEquipped) return false;

            artifact.IsEquipped = false;
            CheckSetBonus(artifact.SetId);
            OnArtifactUnequipped?.Invoke(this, new ArtifactEventArgs(artifact));

            return true;
        }

        public List<Artifact> GetEquippedArtifacts()
        {
            return _playerData.OwnedArtifacts.Where(a => a.IsEquipped).ToList();
        }

        public List<Artifact> GetOwnedArtifacts()
        {
            return _playerData.OwnedArtifacts;
        }

        public Artifact GetArtifactById(string artifactId)
        {
            return _playerData.OwnedArtifacts.FirstOrDefault(a => a.Id == artifactId);
        }

        #endregion

        #region Enhancement System

        public ForgeResult ForgeArtifact(string artifactId, int targetLevel)
        {
            var artifact = _playerData.OwnedArtifacts.FirstOrDefault(a => a.Id == artifactId);
            if (artifact == null)
                return new ForgeResult { Success = false, Message = "神器不存在" };

            if (targetLevel > 10)
                return new ForgeResult { Success = false, Message = "强化等级不能超过10" };

            if (targetLevel <= artifact.EnhancementLevel)
                return new ForgeResult { Success = false, Message = "目标等级必须高于当前等级" };

            var currentLevel = artifact.EnhancementLevel;
            var successRate = ArtifactDatabase.GetForgeSuccessRate(targetLevel);
            var totalCost = 0L;

            for (int i = currentLevel + 1; i <= targetLevel; i++)
            {
                totalCost += ArtifactDatabase.GetForgeGoldCost(i);
            }

            var random = new Random();
            var roll = random.NextDouble();

            var result = new ForgeResult
            {
                Success = roll <= successRate,
                ArtifactId = artifactId,
                TargetLevel = targetLevel,
                SuccessRate = successRate,
                GoldCost = totalCost
            };

            if (result.Success)
            {
                artifact.EnhancementLevel = targetLevel;
                
                // 强化成功增加效果
                var multiplier = 1.0f + (targetLevel * 0.1f);
                foreach (var effect in artifact.Effects)
                {
                    effect.Value *= multiplier;
                }

                _statistics.SuccessfulForges++;
                if (targetLevel > _statistics.BestForgeLevel)
                    _statistics.BestForgeLevel = targetLevel;

                result.Message = $"强化成功！神器等级提升至 {targetLevel}";
            }
            else
            {
                _statistics.FailedForges++;
                result.Message = "强化失败，神器等级未变化";
            }

            UpdateStatistics();
            OnForgeCompleted?.Invoke(this, new ForgeEventArgs(artifact, result));

            return result;
        }

        #endregion

        #region Set Bonus System

        private void CheckSetBonus(string setId)
        {
            if (string.IsNullOrEmpty(setId)) return;

            var setArtifacts = _playerData.OwnedArtifacts
                .Where(a => a.SetId == setId && a.IsEquipped)
                .ToList();

            var set = ArtifactDatabase.GetSetById(setId);
            if (set == null) return;

            var pieceCount = setArtifacts.Count;
            var unlockedSet = _playerData.UnlockedSets.FirstOrDefault(s => s.Id == setId);

            if (pieceCount >= 2 && unlockedSet == null)
            {
                unlockedSet = set;
                _playerData.UnlockedSets.Add(set);
                _statistics.SetsCompleted++;
                OnSetBonusActivated?.Invoke(this, new SetBonusEventArgs(setId, 2, true));
            }
        }

        public List<ArtifactEffect> GetActiveSetBonuses()
        {
            var bonuses = new List<ArtifactEffect>();
            var equippedSets = GetEquippedArtifacts()
                .Where(a => !string.IsNullOrEmpty(a.SetId))
                .GroupBy(a => a.SetId);

            foreach (var group in equippedSets)
            {
                var set = ArtifactDatabase.GetSetById(group.Key);
                if (set == null) continue;

                var count = group.Count();
                var bonus = new ArtifactEffect
                {
                    Type = ArtifactEffectType.AllAttributes,
                    Value = count * 10,
                    Description = $"{set.Name} 套装 ({count}件): 全属性 +{count * 10}%"
                };
                bonuses.Add(bonus);
            }

            return bonuses;
        }

        #endregion

        #region Stats Calculation

        public Dictionary<ArtifactEffectType, float> CalculateTotalStats()
        {
            var stats = new Dictionary<ArtifactEffectType, float>();
            var equipped = GetEquippedArtifacts();

            foreach (var artifact in equipped)
            {
                foreach (var effect in artifact.Effects)
                {
                    var current = stats.GetValueOrDefault(effect.Type, 0);
                    stats[effect.Type] = current + effect.Value;
                }
            }

            // 加上套装效果
            var setBonuses = GetActiveSetBonuses();
            foreach (var bonus in setBonuses)
            {
                var current = stats.GetValueOrDefault(bonus.Type, 0);
                stats[bonus.Type] = current + bonus.Value;
            }

            return stats;
        }

        public float GetStatValue(ArtifactEffectType statType)
        {
            var stats = CalculateTotalStats();
            return stats.GetValueOrDefault(statType, 0);
        }

        #endregion

        #region Statistics

        private void UpdateStatistics()
        {
            var artifacts = _playerData.OwnedArtifacts;
            _statistics.TotalArtifacts = artifacts.Count;
            _statistics.RareArtifacts = artifacts.Count(a => a.Rarity == ArtifactRarity.Rare);
            _statistics.EpicArtifacts = artifacts.Count(a => a.Rarity == ArtifactRarity.Epic);
            _statistics.LegendaryArtifacts = artifacts.Count(a => a.Rarity == ArtifactRarity.Legendary);
            _statistics.MythicArtifacts = artifacts.Count(a => a.Rarity == ArtifactRarity.Mythic);
            _statistics.SetsCompleted = _playerData.UnlockedSets.Count;
        }

        public ArtifactStatistics GetStatistics()
        {
            UpdateStatistics();
            return _statistics;
        }

        public PlayerArtifactData GetPlayerData()
        {
            return _playerData;
        }

        public void LoadData(PlayerArtifactData data)
        {
            _playerData = data ?? new PlayerArtifactData();
            UpdateStatistics();
        }

        #endregion

        #region Save/Load

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>
            {
                ["OwnedArtifacts"] = _playerData.OwnedArtifacts,
                ["UnlockedSets"] = _playerData.UnlockedSets,
                ["ArtifactStats"] = _playerData.ArtifactStats,
                ["TotalArtifactsCollected"] = _playerData.TotalArtifactsCollected,
                ["MythicArtifacts"] = _playerData.MythicArtifacts,
                ["Statistics"] = _statistics
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            _playerData.OwnedArtifacts = data.GetValueOrDefault("OwnedArtifacts") as List<Artifact> ?? new List<Artifact>();
            _playerData.UnlockedSets = data.GetValueOrDefault("UnlockedSets") as List<ArtifactSet> ?? new List<ArtifactSet>();
            _playerData.ArtifactStats = data.GetValueOrDefault("ArtifactStats") as Dictionary<string, int> ?? new Dictionary<string, int>();
            _playerData.TotalArtifactsCollected = (int)(data.GetValueOrDefault("TotalArtifactsCollected", 0));
            _playerData.MythicArtifacts = (int)(data.GetValueOrDefault("MythicArtifacts", 0));
            _statistics = data.GetValueOrDefault("Statistics") as ArtifactStatistics ?? new ArtifactStatistics();
        }

        #endregion
    }

    #region Event Args

    public class ArtifactEventArgs : EventArgs
    {
        public Artifact Artifact { get; }
        public ArtifactEventArgs(Artifact artifact) => Artifact = artifact;
    }

    public class ForgeEventArgs : EventArgs
    {
        public Artifact Artifact { get; }
        public ForgeResult Result { get; }
        public ForgeEventArgs(Artifact artifact, ForgeResult result)
        {
            Artifact = artifact;
            Result = result;
        }
    }

    public class SetBonusEventArgs : EventArgs
    {
        public string SetId { get; }
        public int PieceCount { get; }
        public bool IsActivated { get; }
        public SetBonusEventArgs(string setId, int pieceCount, bool isActivated)
        {
            SetId = setId;
            PieceCount = pieceCount;
            IsActivated = isActivated;
        }
    }

    #endregion

    #region Results

    public class ForgeResult
    {
        public bool Success { get; set; }
        public string ArtifactId { get; set; }
        public int TargetLevel { get; set; }
        public float SuccessRate { get; set; }
        public long GoldCost { get; set; }
        public string Message { get; set; }
    }

    #endregion
}
