using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ClawRPG.Scripts.Systems.CoopSession {
    /// <summary>
    /// 战斗操作序列化器 - 负责序列化和反序列化战斗操作
    /// </summary>
    public partial class BattleActionSerializer : BaseSystem {
        
        /// <summary>
        /// 操作类型
        /// </summary>
        public enum ActionType {
            Attack,
            Skill,
            Item,
            Move,
            Defend,
            Escape
        }
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 序列化战斗操作
        /// </summary>
        public string SerializeAction(Dictionary action) {
            try {
                var json = JsonSerializer.Serialize(action);
                return json;
            }
            catch (Exception e) {
                GD.PrintErr($"[BattleActionSerializer] Failed to serialize: {e.Message}");
                return "";
            }
        }
        
        /// <summary>
        /// 反序列化战斗操作
        /// </summary>
        public Dictionary DeserializeAction(string json) {
            try {
                var action = JsonSerializer.Deserialize<Dictionary>(json);
                return action;
            }
            catch (Exception e) {
                GD.PrintErr($"[BattleActionSerializer] Failed to deserialize: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 创建攻击操作
        /// </summary>
        public Dictionary CreateAttackAction(int attackerId, int targetId, int damage) {
            return new Dictionary {
                { "type", (int)ActionType.Attack },
                { "attackerId", attackerId },
                { "targetId", targetId },
                { "damage", damage },
                { "timestamp", Time.GetUnixTimeFromSystem() }
            };
        }
        
        /// <summary>
        /// 创建技能操作
        /// </summary>
        public Dictionary CreateSkillAction(int casterId, string skillId, int targetId) {
            return new Dictionary {
                { "type", (int)ActionType.Skill },
                { "casterId", casterId },
                { "skillId", skillId },
                { "targetId", targetId },
                { "timestamp", Time.GetUnixTimeFromSystem() }
            };
        }
        
        /// <summary>
        /// 创建物品操作
        /// </summary>
        public Dictionary CreateItemAction(int playerId, string itemId, int targetId) {
            return new Dictionary {
                { "type", (int)ActionType.Item },
                { "playerId", playerId },
                { "itemId", itemId },
                { "targetId", targetId },
                { "timestamp", Time.GetUnixTimeFromSystem() }
            };
        }
        
        /// <summary>
        /// 创建移动操作
        /// </summary>
        public Dictionary CreateMoveAction(int entityId, Vector2 position) {
            return new Dictionary {
                { "type", (int)ActionType.Move },
                { "entityId", entityId },
                { "position", new Dictionary { { "x", position.X }, { "y", position.Y } } },
                { "timestamp", Time.GetUnixTimeFromSystem() }
            };
        }
        
        /// <summary>
        /// 批量序列化操作
        /// </summary>
        public string SerializeActions(List<Dictionary> actions) {
            try {
                var json = JsonSerializer.Serialize(actions);
                return json;
            }
            catch (Exception e) {
                GD.PrintErr($"[BattleActionSerializer] Failed to serialize batch: {e.Message}");
                return "[]";
            }
        }
        
        /// <summary>
        /// 批量反序列化操作
        /// </summary>
        public List<Dictionary> DeserializeActions(string json) {
            try {
                var actions = JsonSerializer.Deserialize<List<Dictionary>>(json);
                return actions;
            }
            catch (Exception e) {
                GD.PrintErr($"[BattleActionSerializer] Failed to deserialize batch: {e.Message}");
                return new List<Dictionary>();
            }
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
