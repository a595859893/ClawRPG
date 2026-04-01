using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// 按键绑定系统 - 管理游戏快捷键配置
    /// </summary>
    public class KeybindingSystem : BaseSystem
    {
        public static KeybindingSystem Instance { get; private set; }

        // 按键绑定数据
        private Dictionary<string, KeybindingAction> _keybindings = new Dictionary<string, KeybindingAction>();
        
        // 事件信号
public delegate void KeybindingChanged(string actionName, Key oldKey, Key newKey);
public delegate void KeybindingsReset();

        public override void _Ready()
        {
            Instance = this;
            InitializeDefaultKeybindings();
        }

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            var keybindings = new Dictionary<string, int>();
            
            foreach (var kvp in _keybindings)
            {
                keybindings[kvp.Key] = (int)kvp.Value.Key;
            }
            
            data["keybindings"] = keybindings;
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null || !data.Contains("keybindings")) return;
            
            var keybindings = (Dictionary)data["keybindings"];
            foreach (var kvp in keybindings)
            {
                if (_keybindings.ContainsKey(kvp.Key))
                {
                    _keybindings[kvp.Key].Key = (Key)kvp.Value;
                }
            }
        }

        private void InitializeDefaultKeybindings()
        {
            // 移动
            AddKeybinding("move_up", Key.W, "移动向上");
            AddKeybinding("move_down", Key.S, "移动向下");
            AddKeybinding("move_left", Key.A, "移动向左");
            AddKeybinding("move_right", Key.D, "移动向右");
            
            // 战斗
            AddKeybinding("attack", Key.J, "攻击");
            AddKeybinding("block", Key.K, "格挡");
            AddKeybinding("dodge", Key.L, "闪避");
            AddKeybinding("skill_1", Key.Digit1, "技能1");
            AddKeybinding("skill_2", Key.Digit2, "技能2");
            AddKeybinding("skill_3", Key.Digit3, "技能3");
            AddKeybinding("skill_4", Key.Digit4, "技能4");
            AddKeybinding("skill_5", Key.Digit5, "技能5");
            AddKeybinding("skill_6", Key.Digit6, "技能6");
            
            // 背包与物品
            AddKeybinding("inventory", Key.I, "背包");
            AddKeybinding("equipment", Key.E, "装备");
            AddKeybinding("quickslot_1", Key.Digit1, "快捷槽1");
            AddKeybinding("quickslot_2", Key.Digit2, "快捷槽2");
            AddKeybinding("quickslot_3", Key.Digit3, "快捷槽3");
            AddKeybinding("quickslot_4", Key.Digit4, "快捷槽4");
            AddKeybinding("quickslot_5", Key.Digit5, "快捷槽5");
            AddKeybinding("quickslot_6", Key.Digit6, "快捷槽6");
            AddKeybinding("quickslot_7", Key.Digit7, "快捷槽7");
            AddKeybinding("quickslot_8", Key.Digit8, "快捷槽8");
            
            // 合成与强化
            AddKeybinding("crafting", Key.C, "合成");
            AddKeybinding("enhancement", Key.X, "强化");
            AddKeybinding("enchant", Key.E, "附魔");
            AddKeybinding("runes", Key.U, "符文");
            
            // 系统
            AddKeybinding("skills", Key.K, "技能树");
            AddKeybinding("quests", Key.Q, "任务");
            AddKeybinding("achievements", Key.L, "成就");
            AddKeybinding("titles", Key.Y, "称号");
            AddKeybinding("statistics", Key.Z, "统计");
            AddKeybinding("pets", Key.P, "宠物");
            AddKeybinding("mounts", Key.O, "坐骑");
            AddKeybinding("region_map", Key.R, "区域地图");
            AddKeybinding("world_events", Key.W, "世界事件");
            AddKeybinding("bounty", Key.B, "赏金任务");
            AddKeybinding("daily_challenge", Key.J, "每日挑战");
            AddKeybinding("weekly_challenge", Key.Shift + Key.W, "每周挑战");
            AddKeybinding("bookmarks", Key.N, "收藏点");
            AddKeybinding("settings", Key.F10, "设置");
            AddKeybinding("hotkey_help", Key.H, "快捷键帮助");
            AddKeybinding("pause", Key.Escape, "暂停");
            AddKeybinding("multiplayer", Key.M, "多人游戏");
            
            // 交互
            AddKeybinding("interact", Key.F, "交互");
            AddKeybinding("quest_tracker", Key.T, "任务追踪");
            AddKeybinding("quest_guide", Key.G, "任务指引");
            AddKeybinding("player_profile", Key.F, "玩家资料");
            AddKeybinding("story", Key.K, "故事");
            
            // 特效
            AddKeybinding("weather", Key.V, "天气");
            AddKeybinding("auto_potion", Key.Shift + Key.X, "自动药水");
            AddKeybinding("auto_bookmark", Key.Shift + Key.N, "自动收藏");
            AddKeybinding("equipment_set", Key.Shift + Key.E, "装备套装");
            
            // 商店
            AddKeybinding("shop", Key.S, "商店");
            
            // 事件连锁系统
            AddKeybinding("event_chain", Key.L, "事件连锁");
        }

        private void AddKeybinding(string actionName, Key key, string description)
        {
            _keybindings[actionName] = new KeybindingAction
            {
                ActionName = actionName,
                Key = key,
                DefaultKey = key,
                Description = description
            };
        }

        /// <summary>
        /// 获取按键绑定
        /// </summary>
        public KeybindingAction GetKeybinding(string actionName)
        {
            if (_keybindings.TryGetValue(actionName, out var binding))
                return binding;
            return null;
        }

        /// <summary>
        /// 获取所有按键绑定
        /// </summary>
        public Dictionary<string, KeybindingAction> GetAllKeybindings()
        {
            return new Dictionary<string, KeybindingAction>(_keybindings);
        }

        /// <summary>
        /// 修改按键绑定
        /// </summary>
        public bool ChangeKeybinding(string actionName, Key newKey)
        {
            if (!_keybindings.ContainsKey(actionName))
                return false;

            var oldBinding = _keybindings[actionName];
            var oldKey = oldBinding.Key;

            // 检查是否与其他绑定冲突
            foreach (var kvp in _keybindings)
            {
                if (kvp.Value.Key == newKey && kvp.Key != actionName)
                {
                    // 交换按键
                    var tempKey = oldKey;
                    kvp.Value.Key = oldKey;
                    EmitSignal(nameof(KeybindingChanged), kvp.Key, tempKey, oldKey);
                }
            }

            oldBinding.Key = newKey;
            EmitSignal(nameof(KeybindingChanged), actionName, oldKey, newKey);
            return true;
        }

        /// <summary>
        /// 重置所有按键为默认值
        /// </summary>
        public void ResetAllKeybindings()
        {
            foreach (var kvp in _keybindings)
            {
                var binding = kvp.Value;
                binding.Key = binding.DefaultKey;
            }
            EmitSignal(nameof(KeybindingsReset));
        }

        /// <summary>
        /// 重置单个按键为默认值
        /// </summary>
        public bool ResetKeybinding(string actionName)
        {
            if (!_keybindings.ContainsKey(actionName))
                return false;

            var binding = _keybindings[actionName];
            var oldKey = binding.Key;
            binding.Key = binding.DefaultKey;
            EmitSignal(nameof(KeybindingChanged), actionName, oldKey, binding.DefaultKey);
            return true;
        }

        /// <summary>
        /// 检查按键是否被按下
        /// </summary>
        public bool IsActionPressed(string actionName)
        {
            if (!_keybindings.TryGetValue(actionName, out var binding))
                return false;
            return Input.IsKeyPressed(binding.Key);
        }

        /// <summary>
        /// 检查按键是否刚刚按下
        /// </summary>
        public bool IsActionJustPressed(string actionName)
        {
            if (!_keybindings.TryGetValue(actionName, out var binding))
                return false;
            return Input.IsKeyJustPressed(binding.Key);
        }

        /// <summary>
        /// 获取按键名称
        /// </summary>
        public static string GetKeyName(Key key)
        {
            if (key == Key.None) return "None";
            
            // 处理组合键
            if ((key & Key.MaskShift) != 0)
                return "Shift+" + GetKeyName(key & ~Key.MaskShift);
            if ((key & Key.MaskCtrl) != 0)
                return "Ctrl+" + GetKeyName(key & ~Key.MaskCtrl);
            if ((key & Key.MaskAlt) != 0)
                return "Alt+" + GetKeyName(key & ~Key.MaskAlt);

            // 数字键
            if (key >= Key.Digit0 && key <= Key.Digit9)
                return key.ToString().Replace("Digit", "");
            
            // 功能键
            if (key >= Key.F1 && key <= Key.F12)
                return key.ToString();
            
            // 方向键
            if (key == Key.Up) return "↑";
            if (key == Key.Down) return "↓";
            if (key == Key.Left) return "←";
            if (key == Key.Right) return "→";
            
            // 其他特殊键
            switch (key)
            {
                case Key.Space: return "Space";
                case Key.Enter: return "Enter";
                case Key.Tab: return "Tab";
                case Key.Backspace: return "Backspace";
                case Key.Delete: return "Delete";
                case Key.Insert: return "Insert";
                case Key.Escape: return "Esc";
                case Key.Home: return "Home";
                case Key.End: return "End";
                case Key.PageUp: return "PgUp";
                case Key.PageDown: return "PgDown";
                default: return key.ToString();
            }
        }

        /// <summary>
        /// 序列化按键绑定数据
        /// </summary>
        public Dictionary<string, int> Serialize()
        {
            var data = new Dictionary<string, int>();
            foreach (var kvp in _keybindings)
            {
                data[kvp.Key] = (int)kvp.Value.Key;
            }
            return data;
        }

        /// <summary>
        /// 反序列化按键绑定数据
        /// </summary>
        public void Deserialize(Dictionary<string, int> data)
        {
            if (data == null) return;
            
            foreach (var kvp in data)
            {
                if (_keybindings.ContainsKey(kvp.Key))
                {
                    _keybindings[kvp.Key].Key = (Key)kvp.Value;
                }
            }
        }

        /// <summary>
        /// 导出为可分享的 JSON 字符串
        /// </summary>
        public string ExportToJson(string profileName = "My Config")
        {
            var profile = new KeybindingProfile(profileName, "custom");
            foreach (var kvp in _keybindings)
            {
                profile.Keybindings[kvp.Key] = ((int)kvp.Value.Key).ToString();
            }
            return profile.ToJson();
        }

        /// <summary>
        /// 从 JSON 字符串导入按键配置
        /// </summary>
        public bool ImportFromJson(string json, bool showErrors = true)
        {
            var profile = KeybindingProfile.FromJson(json);
            if (profile == null || !profile.Validate())
            {
                if (showErrors) GD.PrintErr("[KeybindingSystem] Invalid profile JSON");
                return false;
            }

            int applied = 0;
            int errors = 0;
            foreach (var kvp in profile.Keybindings)
            {
                if (!_keybindings.ContainsKey(kvp.Key))
                {
                    errors++;
                    continue;
                }

                if (int.TryParse(kvp.Value, out int keyCode))
                {
                    _keybindings[kvp.Key].Key = (Key)keyCode;
                    applied++;
                }
                else
                {
                    errors++;
                }
            }

            if (errors > 0 && showErrors)
            {
                GD.PrintErr($"[KeybindingSystem] Import: {errors} bindings could not be applied");
            }

            EmitSignal(nameof(KeybindingsReset));
            return applied > 0;
        }

        /// <summary>
        /// 应用预设方案
        /// </summary>
        public bool ApplyPreset(string category)
        {
            KeybindingProfile preset = null;
            switch (category.ToLower())
            {
                case "moba": preset = KeybindingPresets.MobaStyle(); break;
                case "arpg": preset = KeybindingPresets.ArpgStyle(); break;
                case "shooter": preset = KeybindingPresets.ShooterStyle(); break;
                case "minimal": preset = KeybindingPresets.MinimalStyle(); break;
                case "default": preset = KeybindingPresets.DefaultStyle(); break;
                default: return false;
            }

            if (preset.Keybindings.Count == 0)
            {
                ResetAllKeybindings();
                return true;
            }

            int applied = 0;
            foreach (var kvp in preset.Keybindings)
            {
                if (_keybindings.ContainsKey(kvp.Key) && int.TryParse(kvp.Value, out int keyCode))
                {
                    _keybindings[kvp.Key].Key = (Key)keyCode;
                    applied++;
                }
            }

            EmitSignal(nameof(KeybindingsReset));
            return applied > 0;
        }

        /// <summary>
        /// 同步到云端（挂载 CloudSaveSystem）
        /// </summary>
        public void SyncToCloud()
        {
            if (CloudSaveSystem.Instance != null)
            {
                var json = ExportToJson("Cloud Backup");
                CloudSaveSystem.Instance.SyncSlotToCloud("keybindings", json);
            }
        }

        /// <summary>
        /// 从云端恢复
        /// </summary>
        public void RestoreFromCloud()
        {
            if (CloudSaveSystem.Instance != null)
            {
                var json = CloudSaveSystem.Instance.SyncSlotFromCloud("keybindings");
                if (!string.IsNullOrEmpty(json))
                {
                    ImportFromJson(json);
                }
            }
        }
    }

    /// <summary>
    /// 按键绑定数据类
    /// </summary>
    public class KeybindingAction
    {
        public string ActionName { get; set; }
        public Key Key { get; set; }
        public Key DefaultKey { get; set; }
        public string Description { get; set; }

        public string KeyName => KeybindingSystem.GetKeyName(Key);
        public string DefaultKeyName => KeybindingSystem.GetKeyName(DefaultKey);
        public bool IsModified => Key != DefaultKey;
    }
}
