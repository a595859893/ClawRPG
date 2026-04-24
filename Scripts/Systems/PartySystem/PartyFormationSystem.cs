using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 队伍编成系统 - 负责成员管理、位置分配、队伍配置等
    /// </summary>
    public partial class PartyFormationSystem : BaseSystem
    {
        private static PartyFormationSystem _instance;
        public static PartyFormationSystem Instance => _instance;
        
        // 队伍位置配置 (每个队伍最多6个位置)
        private Dictionary<string, PartyPositionConfig> _positionConfigs = new Dictionary<string, PartyPositionConfig>();
        
        // 队伍成员配置
        private Dictionary<string, MemberConfig> _memberConfigs = new Dictionary<string, MemberConfig>();
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "PartyFormation";
        
        #region Position Management
        
        /// <summary>
        /// 设置队伍位置配置
        /// </summary>
        public void SetPositionConfig(string partyId, int position, string memberId, string role)
        {
            if (!_positionConfigs.ContainsKey(partyId))
            {
                _positionConfigs[partyId] = new PartyPositionConfig();
            }
            
            _positionConfigs[partyId].SetPosition(position, memberId, role);
        }
        
        /// <summary>
        /// 获取队伍位置配置
        /// </summary>
        public PartyPositionConfig GetPositionConfig(string partyId)
        {
            return _positionConfigs.ContainsKey(partyId) ? _positionConfigs[partyId] : null;
        }
        
        /// <summary>
        /// 交换两个位置
        /// </summary>
        public bool SwapPositions(string partyId, int pos1, int pos2)
        {
            if (!_positionConfigs.ContainsKey(partyId))
                return false;
            
            return _positionConfigs[partyId].Swap(pos1, pos2);
        }
        
        /// <summary>
        /// 清空位置
        /// </summary>
        public void ClearPosition(string partyId, int position)
        {
            if (_positionConfigs.ContainsKey(partyId))
            {
                _positionConfigs[partyId].ClearPosition(position);
            }
        }
        
        #endregion
        
        #region Member Configuration
        
        /// <summary>
        /// 设置成员配置
        /// </summary>
        public void SetMemberConfig(string partyId, string memberId, MemberConfig config)
        {
            var key = $"{partyId}_{memberId}";
            _memberConfigs[key] = config;
        }
        
        /// <summary>
        /// 获取成员配置
        /// </summary>
        public MemberConfig GetMemberConfig(string partyId, string memberId)
        {
            var key = $"{partyId}_{memberId}";
            return _memberConfigs.ContainsKey(key) ? _memberConfigs[key] : null;
        }
        
        /// <summary>
        /// 移除成员配置
        /// </summary>
        public void RemoveMemberConfig(string partyId, string memberId)
        {
            var key = $"{partyId}_{memberId}";
            if (_memberConfigs.ContainsKey(key))
            {
                _memberConfigs.Remove(key);
            }
        }
        
        #endregion
        
        #region Formation Validation
        
        /// <summary>
        /// 验证队伍编成是否有效
        /// </summary>
        public bool ValidateFormation(string partyId, int maxSize)
        {
            if (!_positionConfigs.ContainsKey(partyId))
                return true; // No config is valid
            
            return _positionConfigs[partyId].Validate(maxSize);
        }
        
        /// <summary>
        /// 获取队伍当前编成
        /// </summary>
        public Dictionary<int, string> GetFormation(string partyId)
        {
            if (!_positionConfigs.ContainsKey(partyId))
                return new Dictionary<int, string>();
            
            return _positionConfigs[partyId].GetFormation();
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // Export position configs
            var positionsArray = new Godot.Collections.Array();
            foreach (var kvp in _positionConfigs)
            {
                var entry = new Dictionary
                {
                    ["partyId"] = kvp.Key,
                    ["config"] = JsonSerializer.Serialize(kvp.Value)
                };
                positionsArray.Add(entry);
            }
            data["positionConfigs"] = positionsArray;
            
            // Export member configs
            var membersArray = new Godot.Collections.Array();
            foreach (var kvp in _memberConfigs)
            {
                var entry = new Dictionary
                {
                    ["key"] = kvp.Key,
                    ["config"] = JsonSerializer.Serialize(kvp.Value)
                };
                membersArray.Add(entry);
            }
            data["memberConfigs"] = membersArray;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _positionConfigs.Clear();
            _memberConfigs.Clear();
            
            // Import position configs
            if (data.ContainsKey("positionConfigs"))
            {
                var positionsArray = (Array)data["positionConfigs"];
                foreach (Dictionary entry in positionsArray)
                {
                    var partyId = entry["partyId"].ToString();
                    var config = JsonSerializer.Deserialize<PartyPositionConfig>(entry["config"].ToString());
                    if (config != null)
                    {
                        _positionConfigs[partyId] = config;
                    }
                }
            }
            
            // Import member configs
            if (data.ContainsKey("memberConfigs"))
            {
                var membersArray = (Array)data["memberConfigs"];
                foreach (Dictionary entry in membersArray)
                {
                    var key = entry["key"].ToString();
                    var config = JsonSerializer.Deserialize<MemberConfig>(entry["config"].ToString());
                    if (config != null)
                    {
                        _memberConfigs[key] = config;
                    }
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 队伍位置配置
    /// </summary>
    public class PartyPositionConfig
    {
        public Dictionary<int, string> Positions { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> Roles { get; set; } = new Dictionary<int, string>();
        
        public void SetPosition(int position, string memberId, string role)
        {
            Positions[position] = memberId;
            Roles[position] = role;
        }
        
        public void ClearPosition(int position)
        {
            Positions.Remove(position);
            Roles.Remove(position);
        }
        
        public bool Swap(int pos1, int pos2)
        {
            if (!Positions.ContainsKey(pos1) && !Positions.ContainsKey(pos2))
                return false;
            
            var tempMember = Positions.ContainsKey(pos1) ? Positions[pos1] : "";
            var tempRole = Roles.ContainsKey(pos1) ? Roles[pos1] : "";
            
            if (Positions.ContainsKey(pos2))
            {
                Positions[pos1] = Positions[pos2];
                Roles[pos1] = Roles[pos2];
            }
            else
            {
                Positions.Remove(pos1);
                Roles.Remove(pos1);
            }
            
            if (!string.IsNullOrEmpty(tempMember))
            {
                Positions[pos2] = tempMember;
                Roles[pos2] = tempRole;
            }
            else
            {
                Positions.Remove(pos2);
                Roles.Remove(pos2);
            }
            
            return true;
        }
        
        public bool Validate(int maxSize)
        {
            return Positions.Keys.All(p => p >= 0 && p < maxSize);
        }
        
        public Dictionary<int, string> GetFormation()
        {
            return new Dictionary<int, string>(Positions);
        }
    }
    
    /// <summary>
    /// 成员配置
    /// </summary>
    public class MemberConfig
    {
        public string MemberId { get; set; }
        public string BuildTemplate { get; set; }
        public List<string> AutoSkills { get; set; } = new List<string>();
        public bool AutoRevive { get; set; }
        public int Priority { get; set; }
    }
}
