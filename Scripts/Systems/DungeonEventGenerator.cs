using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 地牢事件生成器 - 负责生成随机事件
    /// </summary>
    public partial class DungeonEventGenerator : BaseSystem {
        
        private Random _random = new Random();
        private List<string> _availableEvents = new();
        
        public override void _Ready() {
            base._Ready();
            InitializeEventPool();
        }
        
        /// <summary>
        /// 生成随机事件
        /// </summary>
        public string GenerateRandomEvent() {
            if (_availableEvents.Count == 0) {
                return "default_event";
            }
            
            var index = _random.Next(_availableEvents.Count);
            return _availableEvents[index];
        }
        
        /// <summary>
        /// 根据类别生成事件
        /// </summary>
        public string GenerateEventByCategory(string category) {
            // 根据类别过滤事件
            return "default_event";
        }
        
        /// <summary>
        /// 初始化事件池
        /// </summary>
        private void InitializeEventPool() {
            _availableEvents.Add("combat_encounter");
            _availableEvents.Add("treasure_chest");
            _availableEvents.Add("blessing_shrine");
            _availableEvents.Add("curse_trap");
            _availableEvents.Add("merchant_encounter");
            _availableEvents.Add("rest_zone");
            
            GD.Print($"[DungeonEventGenerator] Initialized with {_availableEvents.Count} events");
        }
        
        /// <summary>
        /// 计算事件权重
        /// </summary>
        public float CalculateEventWeight(string eventId, int playerFloor) {
            var baseWeight = 1.0f;
            
            // 根据层数调整权重
            if (playerFloor > 10) {
                baseWeight *= 1.5f; // 更高楼层更多战斗事件
            }
            
            return baseWeight;
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
