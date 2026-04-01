using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Systems {
    /// <summary>
    /// 按键配置导出/导入格式
    /// </summary>
    public class KeybindingProfile
    {
        public int Version { get; set; } = 1;
        public string ProfileName { get; set; } = "Unnamed";
        public string CreatedAt { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "custom"; // custom, moba, arpg, shooter, minimal
        public Dictionary<string, string> Keybindings { get; set; } = new Dictionary<string, string>();

        public KeybindingProfile() { }

        public KeybindingProfile(string name, string category, string description = "")
        {
            ProfileName = name;
            Category = category;
            Description = description;
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Version = 1;
        }

        /// <summary>
        /// 从当前系统导出
        /// </summary>
        public static KeybindingProfile ExportFromSystem(KeybindingSystem system)
        {
            var profile = new KeybindingProfile
            {
                Version = 1,
                ProfileName = "My Config",
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Category = "custom"
            };

            var all = system.GetAllKeybindings();
            foreach (var kvp in all)
            {
                profile.Keybindings[kvp.Key] = ((int)kvp.Value.Key).ToString();
            }
            return profile;
        }

        /// <summary>
        /// 验证 schema 版本
        /// </summary>
        public bool Validate()
        {
            if (Version < 1 || Version > 2) return false;
            if (string.IsNullOrEmpty(ProfileName)) return false;
            return true;
        }

        /// <summary>
        /// 转换为 JSON 字符串
        /// </summary>
        public string ToJson()
        {
            return Godot.JSON.Stringify(this, "", false);
        }

        /// <summary>
        /// 从 JSON 解析
        /// </summary>
        public static KeybindingProfile FromJson(string json)
        {
            try
            {
                var result = Godot.JSON.Parse(json);
                if (result.Error != Error.Ok) return null;
                return FromDict(result.Result.AsDictionary());
            }
            catch
            {
                return null;
            }
        }

        private static KeybindingProfile FromDict(Dictionary dict)
        {
            var profile = new KeybindingProfile();
            if (dict.ContainsKey("version")) profile.Version = (int)(long)dict["version"];
            if (dict.ContainsKey("profile_name")) profile.ProfileName = (string)dict["profile_name"];
            if (dict.ContainsKey("created_at")) profile.CreatedAt = (string)dict["created_at"];
            if (dict.ContainsKey("description")) profile.Description = (string)dict["description"];
            if (dict.ContainsKey("category")) profile.Category = (string)dict["category"];

            if (dict.ContainsKey("keybindings"))
            {
                var kb = (Dictionary)dict["keybindings"];
                foreach (var kvp in kb)
                {
                    profile.Keybindings[kvp.Key.ToString()] = kvp.Value.ToString();
                }
            }
            return profile;
        }
    }

    /// <summary>
    /// 内置预设方案
    /// </summary>
    public static class KeybindingPresets
    {
        public static KeybindingProfile MobaStyle()
        {
            return new KeybindingProfile("MOBA风格", "moba", "适合MOBA玩家的按键布局：QWER技能，DF召宠")
            {
                Keybindings = BuildMobaKeybindings()
            };
        }

        public static KeybindingProfile ArpgStyle()
        {
            return new KeybindingProfile("ARPG风格", "arpg", "适合ARPG玩家的按键布局：左键普攻右键技能")
            {
                Keybindings = BuildArpgKeybindings()
            };
        }

        public static KeybindingProfile ShooterStyle()
        {
            return new KeybindingProfile("射击风格", "shooter", "适合射击游戏玩家的按键布局：WASD瞄准，空格跳跃")
            {
                Keybindings = BuildShooterKeybindings()
            };
        }

        public static KeybindingProfile MinimalStyle()
        {
            return new KeybindingProfile("简约风格", "minimal", "最小化按键，仅保留核心操作")
            {
                Keybindings = BuildMinimalKeybindings()
            };
        }

        public static KeybindingProfile DefaultStyle()
        {
            return new KeybindingProfile("默认风格", "default", "游戏默认按键布局")
            {
                Keybindings = BuildDefaultKeybindings()
            };
        }

        public static KeybindingProfile[] GetAllPresets()
        {
            return new[] { DefaultStyle(), MobaStyle(), ArpgStyle(), ShooterStyle(), MinimalStyle() };
        }

        private static Dictionary<string, string> BuildMobaKeybindings()
        {
            return new Dictionary<string, string>
            {
                ["move_up"] = ((int)Key.W).ToString(),
                ["move_down"] = ((int)Key.S).ToString(),
                ["move_left"] = ((int)Key.A).ToString(),
                ["move_right"] = ((int)Key.D).ToString(),
                ["attack"] = ((int)Key.A).ToString(),
                ["block"] = ((int)Key.D).ToString(),
                ["dodge"] = ((int)Key.Space).ToString(),
                ["skill_1"] = ((int)Key.Q).ToString(),
                ["skill_2"] = ((int)Key.W).ToString(),
                ["skill_3"] = ((int)Key.E).ToString(),
                ["skill_4"] = ((int)Key.R).ToString(),
                ["skill_5"] = ((int)Key.Digit1).ToString(),
                ["skill_6"] = ((int)Key.Digit2).ToString(),
                ["inventory"] = ((int)Key.I).ToString(),
                ["equipment"] = ((int)Key.E).ToString(),
                ["skills"] = ((int)Key.K).ToString(),
                ["quests"] = ((int)Key.J).ToString(),
                ["pause"] = ((int)Key.Escape).ToString(),
                ["interact"] = ((int)Key.F).ToString(),
                ["pet"] = ((int)Key.D).ToString(),
            };
        }

        private static Dictionary<string, string> BuildArpgKeybindings()
        {
            return new Dictionary<string, string>
            {
                ["move_up"] = ((int)Key.W).ToString(),
                ["move_down"] = ((int)Key.S).ToString(),
                ["move_left"] = ((int)Key.A).ToString(),
                ["move_right"] = ((int)Key.D).ToString(),
                ["attack"] = ((int)Key.Digit1).ToString(),
                ["block"] = ((int)Key.Digit2).ToString(),
                ["dodge"] = ((int)Key.Shift).ToString(),
                ["skill_1"] = ((int)Key.Digit3).ToString(),
                ["skill_2"] = ((int)Key.Digit4).ToString(),
                ["skill_3"] = ((int)Key.Digit5).ToString(),
                ["skill_4"] = ((int)Key.Digit6).ToString(),
                ["skill_5"] = ((int)Key.Q).ToString(),
                ["skill_6"] = ((int)Key.E).ToString(),
                ["inventory"] = ((int)Key.I).ToString(),
                ["equipment"] = ((int)Key.C).ToString(),
                ["pause"] = ((int)Key.Escape).ToString(),
                ["interact"] = ((int)Key.F).ToString(),
            };
        }

        private static Dictionary<string, string> BuildShooterKeybindings()
        {
            return new Dictionary<string, string>
            {
                ["move_up"] = ((int)Key.W).ToString(),
                ["move_down"] = ((int)Key.S).ToString(),
                ["move_left"] = ((int)Key.A).ToString(),
                ["move_right"] = ((int)Key.D).ToString(),
                ["attack"] = ((int)Key.Digit1).ToString(),
                ["block"] = ((int)Key.Digit2).ToString(),
                ["dodge"] = ((int)Key.Space).ToString(),
                ["skill_1"] = ((int)Key.Q).ToString(),
                ["skill_2"] = ((int)Key.E).ToString(),
                ["skill_3"] = ((int)Key.Digit3).ToString(),
                ["skill_4"] = ((int)Key.Digit4).ToString(),
                ["inventory"] = ((int)Key.Tab).ToString(),
                ["pause"] = ((int)Key.Escape).ToString(),
            };
        }

        private static Dictionary<string, string> BuildMinimalKeybindings()
        {
            return new Dictionary<string, string>
            {
                ["move_up"] = ((int)Key.W).ToString(),
                ["move_down"] = ((int)Key.S).ToString(),
                ["move_left"] = ((int)Key.A).ToString(),
                ["move_right"] = ((int)Key.D).ToString(),
                ["attack"] = ((int)Key.Digit1).ToString(),
                ["skill_1"] = ((int)Key.Q).ToString(),
                ["skill_2"] = ((int)Key.E).ToString(),
                ["dodge"] = ((int)Key.Space).ToString(),
                ["inventory"] = ((int)Key.I).ToString(),
                ["pause"] = ((int)Key.Escape).ToString(),
            };
        }

        private static Dictionary<string, string> BuildDefaultKeybindings()
        {
            // Return empty - will use system's default keybindings
            return new Dictionary<string, string>();
        }
    }
}
