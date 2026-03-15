using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 地牢事件数据库 - 存储所有事件配置
    /// </summary>
    public partial class DungeonEventDatabase : BaseSystem {
        
        private Dictionary<string, Dictionary> _eventConfigs = new();
        
        public override void _Ready() {
            base._Ready();
            LoadEventDatabase();
        }
        
        /// <summary>
        /// 获取事件配置
        /// </summary>
        public Dictionary GetEventConfig(string eventId) {
            if (_eventConfigs.TryGetValue(eventId, out var config)) {
                return config;
            }
            return null;
        }
        
        /// <summary>
        /// 获取所有事件ID
        /// </summary>
        public List<string> GetAllEventIds() {
            return new List<string>(_eventConfigs.Keys);
        }
        
        /// <summary>
        /// 检查事件是否存在
        /// </summary>
        public bool HasEvent(string eventId) {
            return _eventConfigs.ContainsKey(eventId);
        }
        
        /// <summary>
        /// 加载事件数据库
        /// </summary>
        private void LoadEventDatabase() {
            // 战斗事件
            _eventConfigs["combat_encounter"] = new Dictionary {
                { "name", "Combat Encounter" },
                { "type", "combat" },
                { "weight", 1.0f }
            };
            
            // 宝藏事件
            _eventConfigs["treasure_chest"] = new Dictionary {
                { "name", "Treasure Chest" },
                { "type", "treasure" },
                { "weight", 0.5f }
            };
            
            // 祝福事件
            _eventConfigs["blessing_shrine"] = new Dictionary {
                { "name", "Blessing Shrine" },
                { "type", "blessing" },
                { "weight", 0.3f }
            };
            
            // 诅咒事件
            _eventConfigs["curse_trap"] = new Dictionary {
                { "name", "Curse Trap" },
                { "type", "curse" },
                { "weight", 0.4f }
            };
            
            // 商人事件
            _eventConfigs["merchant_encounter"] = new Dictionary {
                { "name", "Merchant Encounter" },
                { "type", "merchant" },
                { "weight", 0.2f }
            };
            
            // 休息区事件
            _eventConfigs["rest_zone"] = new Dictionary {
                { "name", "Rest Zone" },
                { "type", "rest" },
                { "weight", 0.3f }
            };
            
            GD.Print($"[DungeonEventDatabase] Loaded {_eventConfigs.Count} events");
        }
        
        /// <summary>
        /// 添加事件配置
        /// </summary>
        public void AddEventConfig(string eventId, Dictionary config) {
            _eventConfigs[eventId] = config;
        }
        
        /// <summary>
        /// 移除事件配置
        /// </summary>
        public void RemoveEventConfig(string eventId) {
            _eventConfigs.Remove(eventId);
        }
        
        public override Dictionary ExportSaveData() {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data) {
            // 加载数据
        }
    }
}
