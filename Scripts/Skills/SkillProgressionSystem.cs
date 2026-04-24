using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Skills
{
    /// <summary>
    /// 技能进度系统 - 负责技能经验、等级、熟练度等进度管理
    /// </summary>
    public partial class SkillProgressionSystem : BaseSystem
    {
        private static SkillProgressionSystem _instance;
        public static SkillProgressionSystem Instance => _instance;
        
        // 技能进度存储: playerId -> skillId -> SkillProgress
        private Dictionary<string, Dictionary<string, SkillProgress>> _skillProgress = new Dictionary<string, Dictionary<string, SkillProgress>>();
        
        // 技能经验配置
        private Dictionary<MasteryRank, int> _masteryThresholds = new Dictionary<MasteryRank, int>
        {
            { MasteryRank.Novice, 0 },
            { MasteryRank.Apprentice, 100 },
            { MasteryRank.Expert, 500 },
            { MasteryRank.Master, 1500 },
            { MasteryRank.GrandMaster, 5000 }
        };
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "SkillProgression";
        
        #region Progress Management
        
        /// <summary>
        /// 获取技能进度
        /// </summary>
        public SkillProgress GetProgress(string playerId, string skillId)
        {
            if (!_skillProgress.ContainsKey(playerId))
                return null;
            
            return _skillProgress[playerId].ContainsKey(skillId) ? _skillProgress[playerId][skillId] : null;
        }
        
        /// <summary>
        /// 获取或创建技能进度
        /// </summary>
        public SkillProgress GetOrCreateProgress(string playerId, string skillId)
        {
            if (!_skillProgress.ContainsKey(playerId))
            {
                _skillProgress[playerId] = new Dictionary<string, SkillProgress>();
            }
            
            if (!_skillProgress[playerId].ContainsKey(skillId))
            {
                _skillProgress[playerId][skillId] = new SkillProgress
                {
                    SkillId = skillId,
                    CurrentXp = 0,
                    Level = 1,
                    MasteryRank = MasteryRank.Novice
                };
            }
            
            return _skillProgress[playerId][skillId];
        }
        
        /// <summary>
        /// 添加经验
        /// </summary>
        public void AddXp(string playerId, string skillId, int xp)
        {
            var progress = GetOrCreateProgress(playerId, skillId);
            progress.CurrentXp += xp;
            
            // Check for level up
            while (ShouldLevelUp(progress))
            {
                progress.Level++;
            }
            
            // Update mastery rank
            progress.MasteryRank = GetMasteryRank(progress.CurrentXp);
        }
        
        /// <summary>
        /// 获取玩家所有技能进度
        /// </summary>
        public Dictionary<string, SkillProgress> GetAllProgress(string playerId)
        {
            return _skillProgress.ContainsKey(playerId) ? new Dictionary<string, SkillProgress>(_skillProgress[playerId]) : new Dictionary<string, SkillProgress>();
        }
        
        #endregion
        
        #region Level Management
        
        /// <summary>
        /// 技能是否可升级
        /// </summary>
        public bool CanLevelUp(string playerId, string skillId)
        {
            var progress = GetProgress(playerId, skillId);
            if (progress == null)
                return false;
            
            return ShouldLevelUp(progress);
        }
        
        /// <summary>
        /// 升级技能
        /// </summary>
        public bool LevelUp(string playerId, string skillId)
        {
            var progress = GetProgress(playerId, skillId);
            if (progress == null || !ShouldLevelUp(progress))
                return false;
            
            progress.Level++;
            return true;
        }
        
        /// <summary>
        /// 重置技能进度
        /// </summary>
        public void ResetProgress(string playerId, string skillId)
        {
            if (_skillProgress.ContainsKey(playerId) && _skillProgress[playerId].ContainsKey(skillId))
            {
                _skillProgress[playerId][skillId] = new SkillProgress
                {
                    SkillId = skillId,
                    CurrentXp = 0,
                    Level = 1,
                    MasteryRank = MasteryRank.Novice
                };
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool ShouldLevelUp(SkillProgress progress)
        {
            var xpForNextLevel = progress.Level * 100;
            return progress.CurrentXp >= xpForNextLevel;
        }
        
        private MasteryRank GetMasteryRank(int xp)
        {
            if (xp >= 5000) return MasteryRank.GrandMaster;
            if (xp >= 1500) return MasteryRank.Master;
            if (xp >= 500) return MasteryRank.Expert;
            if (xp >= 100) return MasteryRank.Apprentice;
            return MasteryRank.Novice;
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            var progressArray = new Godot.Collections.Array();
            foreach (var playerKvp in _skillProgress)
            {
                foreach (var skillKvp in playerKvp.Value)
                {
                    var entry = new Dictionary
                    {
                        ["playerId"] = playerKvp.Key,
                        ["progress"] = JsonSerializer.Serialize(skillKvp.Value)
                    };
                    progressArray.Add(entry);
                }
            }
            data["skillProgress"] = progressArray;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _skillProgress.Clear();
            
            if (data.ContainsKey("skillProgress"))
            {
                var progressArray = (Array)data["skillProgress"];
                foreach (Dictionary entry in progressArray)
                {
                    var playerId = entry["playerId"].ToString();
                    var progress = JsonSerializer.Deserialize<SkillProgress>(entry["progress"].ToString());
                    
                    if (!_skillProgress.ContainsKey(playerId))
                    {
                        _skillProgress[playerId] = new Dictionary<string, SkillProgress>();
                    }
                    _skillProgress[playerId][progress.SkillId] = progress;
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 技能进度数据
    /// </summary>
    public class SkillProgress
    {
        public string SkillId { get; set; }
        public int CurrentXp { get; set; }
        public int Level { get; set; }
        public MasteryRank MasteryRank { get; set; }
        public int TotalUsageCount { get; set; }
    }
}
