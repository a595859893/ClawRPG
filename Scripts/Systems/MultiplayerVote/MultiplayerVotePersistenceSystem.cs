using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 多人投票持久化系统 - 负责所有投票和队伍数据的持久化
    /// </summary>
    public partial class MultiplayerVotePersistenceSystem : BaseSystem
    {
        private static MultiplayerVotePersistenceSystem _instance;
        public static MultiplayerVotePersistenceSystem Instance => _instance;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "MultiplayerVotePersistence";
        
        /// <summary>
        /// 导出所有持久化数据
        /// </summary>
        public Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // Export from PartyManagementSystem
            if (PartyManagementSystem.Instance != null)
            {
                var partyData = PartyManagementSystem.Instance.ExportSaveData();
                foreach (var key in partyData.Keys)
                {
                    data[key] = partyData[key];
                }
            }
            
            // Export from VoteProcessingSystem
            if (VoteProcessingSystem.Instance != null)
            {
                var voteData = VoteProcessingSystem.Instance.ExportSaveData();
                foreach (var key in voteData.Keys)
                {
                    data["vote_" + key] = voteData[key];
                }
            }
            
            return data;
        }
        
        /// <summary>
        /// 导入所有持久化数据
        /// </summary>
        public void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // Import to PartyManagementSystem
            if (PartyManagementSystem.Instance != null)
            {
                var partyData = new Dictionary();
                foreach (var key in data.Keys)
                {
                    if (!key.ToString().StartsWith("vote_"))
                    {
                        partyData[key] = data[key];
                    }
                }
                PartyManagementSystem.Instance.ImportSaveData(partyData);
            }
            
            // Import to VoteProcessingSystem
            if (VoteProcessingSystem.Instance != null)
            {
                var voteData = new Dictionary();
                foreach (var key in data.Keys)
                {
                    if (key.ToString().StartsWith("vote_"))
                    {
                        var newKey = key.ToString().Substring(5);
                        voteData[newKey] = data[key];
                    }
                }
                VoteProcessingSystem.Instance.ImportSaveData(voteData);
            }
        }
        
        /// <summary>
        /// 保存数据到文件
        /// </summary>
        public bool SaveToFile(string filePath)
        {
            try
            {
                var data = ExportSaveData();
                var json = JsonSerializer.Serialize(data);
                FileAccess.File(filePath, FileAccess.ModeFlags.Write);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to save MultiplayerVote data: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 从文件加载数据
        /// </summary>
        public bool LoadFromFile(string filePath)
        {
            try
            {
                if (!FileAccess.FileExists(filePath))
                    return false;
                
                var file = FileAccess.File(filePath, FileAccess.ModeFlags.Read);
                var json = file.GetAsText();
                file.Close();
                
                var data = JsonSerializer.Deserialize<Dictionary>(json);
                ImportSaveData(data);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load MultiplayerVote data: {e.Message}");
                return false;
            }
        }
    }
}
