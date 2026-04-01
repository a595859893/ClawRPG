using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 装备外观管理器 - 允许玩家自定义装备外观
    /// </summary>
    public partial class EquipmentVisuals : BaseSystem {
        public static EquipmentVisuals Instance { get; private set; }

        // 武器外观
        private string _weaponVisualId = "default_sword";
        // 防具外观
        private string _armorVisualId = "default_armor";
        // 饰品外观
        private string _accessoryVisualId = "default_accessory";
        
        // 已解锁的外观（slot -> list of visualId）
        private Dictionary<string, HashSet<string>> _unlockedVisuals = new Dictionary<string, HashSet<string>>() {
            { "weapon", new HashSet<string>() { "default_sword" } },
            { "armor", new HashSet<string>() { "default_armor" } },
            { "accessory", new HashSet<string>() { "default_accessory" } }
        };

        // 武器外观数据库
        private Dictionary<string, WeaponVisual> _weaponVisuals = new Dictionary<string, WeaponVisual>();
        // 防具外观数据库
        private Dictionary<string, ArmorVisual> _armorVisuals = new Dictionary<string, ArmorVisual>();
        // 饰品外观数据库
        private Dictionary<string, AccessoryVisual> _accessoryVisuals = new Dictionary<string, AccessoryVisual>();

        public delegate void OnVisualChanged(string slot, string visualId);

        public override void _Ready() {
            Instance = this;
            InitializeVisuals();
        }

        /// <summary>
        /// 初始化外观数据库
        /// </summary>
        private void InitializeVisuals() {
            // 武器外观
            _weaponVisuals["default_sword"] = new WeaponVisual {
                Id = "default_sword",
                Name = "默认剑",
                Description = "默认武器外观",
                ModelPath = "res://Models/Weapons/sword.tscn",
                TexturePath = "",
                Rarity = "common",
                UnlockRequirement = ""
            };
            _weaponVisuals["flame_sword"] = new WeaponVisual {
                Id = "flame_sword",
                Name = "火焰之剑",
                Description = "燃烧着烈火的剑",
                ModelPath = "res://Models/Weapons/flame_sword.tscn",
                TexturePath = "res://Textures/Weapons/flame.png",
                ParticleEffect = "fire",
                Rarity = "rare",
                UnlockRequirement = "craft_flame_sword"
            };
            _weaponVisuals["ice_sword"] = new WeaponVisual {
                Id = "ice_sword",
                Name = "冰霜之剑",
                Description = "冰冷刺骨的剑",
                ModelPath = "res://Models/Weapons/ice_sword.tscn",
                TexturePath = "res://Textures/Weapons/ice.png",
                ParticleEffect = "ice",
                Rarity = "rare",
                UnlockRequirement = "craft_ice_sword"
            };
            _weaponVisuals["lightning_sword"] = new WeaponVisual {
                Id = "lightning_sword",
                Name = "雷神之剑",
                Description = "雷电环绕的剑",
                ModelPath = "res://Models/Weapons/lightning_sword.tscn",
                TexturePath = "res://Textures/Weapons/lightning.png",
                ParticleEffect = "lightning",
                Rarity = "epic",
                UnlockRequirement = "craft_lightning_sword"
            };
            _weaponVisuals["legendary_sword"] = new WeaponVisual {
                Id = "legendary_sword",
                Name = "传奇之刃",
                Description = "传说中的神兵利器",
                ModelPath = "res://Models/Weapons/legendary_sword.tscn",
                TexturePath = "res://Textures/Weapons/legendary.png",
                ParticleEffect = "legendary",
                Rarity = "legendary",
                UnlockRequirement = "defeat_boss_10"
            };

            // 防具外观
            _armorVisuals["default_armor"] = new ArmorVisual {
                Id = "default_armor",
                Name = "默认盔甲",
                Description = "默认防具外观",
                ModelPath = "res://Models/Armor/basic.tscn",
                TexturePath = "",
                Rarity = "common",
                UnlockRequirement = ""
            };
            _armorVisuals["iron_armor"] = new ArmorVisual {
                Id = "iron_armor",
                Name = "铁甲",
                Description = "坚固的铁制盔甲",
                ModelPath = "res://Models/Armor/iron.tscn",
                TexturePath = "res://Textures/Armor/iron.png",
                Rarity = "uncommon",
                UnlockRequirement = "reach_level_5"
            };
            _armorVisuals["dragon_scale"] = new ArmorVisual {
                Id = "dragon_scale",
                Name = "龙鳞甲",
                Description = "由巨龙的鳞片制成的盔甲",
                ModelPath = "res://Models/Armor/dragon.tscn",
                TexturePath = "res://Textures/Armor/dragon.png",
                SpecialEffect = "dragon_aura",
                Rarity = "epic",
                UnlockRequirement = "defeat_boss_5"
            };
            _armorVisuals["golden_armor"] = new ArmorVisual {
                Id = "golden_armor",
                Name = "黄金圣甲",
                Description = "神圣的黄金盔甲",
                ModelPath = "res://Models/Armor/golden.tscn",
                TexturePath = "res://Textures/Armor/golden.png",
                SpecialEffect = "holy_glow",
                Rarity = "legendary",
                UnlockRequirement = "collect_all_gold"
            };

            // 饰品外观
            _accessoryVisuals["default_accessory"] = new AccessoryVisual {
                Id = "default_accessory",
                Name = "默认饰品",
                Description = "默认饰品外观",
                ModelPath = "res://Models/Accessory/basic.tscn",
                Rarity = "common",
                UnlockRequirement = ""
            };
            _accessoryVisuals["ruby_amulet"] = new AccessoryVisual {
                Id = "ruby_amulet",
                Name = "红宝石项链",
                Description = "镶嵌红宝石的项链",
                ModelPath = "res://Models/Accessory/ruby.tscn",
                TexturePath = "res://Textures/Accessory/ruby.png",
                GlowColor = "#ff0000",
                Rarity = "rare",
                UnlockRequirement = "collect_ruby"
            };
            _accessoryVisuals["sapphire_ring"] = new AccessoryVisual {
                Id = "sapphire_ring",
                Name = "蓝宝石戒指",
                Description = "散发神秘光芒的戒指",
                ModelPath = "res://Models/Accessory/sapphire.tscn",
                TexturePath = "res://Textures/Accessory/sapphire.png",
                GlowColor = "#0066ff",
                Rarity = "epic",
                UnlockRequirement = "defeat_boss_shadow"
            };
            _accessoryVisuals["legendary_crown"] = new AccessoryVisual {
                Id = "legendary_crown",
                Name = "传奇王冠",
                Description = "王者专属的王冠",
                ModelPath = "res://Models/Accessory/crown.tscn",
                TexturePath = "res://Textures/Accessory/crown.png",
                GlowColor = "#ffd700",
                ParticleEffect = "stars",
                Rarity = "legendary",
                UnlockRequirement = "become_king"
            };
        }

        /// <summary>
        /// 设置武器外观
        /// </summary>
        public void SetWeaponVisual(string visualId) {
            if (_weaponVisuals.ContainsKey(visualId)) {
                _weaponVisualId = visualId;
                EmitSignal(nameof(OnVisualChanged), "weapon", visualId);
            }
        }

        /// <summary>
        /// 设置防具外观
        /// </summary>
        public void SetArmorVisual(string visualId) {
            if (_armorVisuals.ContainsKey(visualId)) {
                _armorVisualId = visualId;
                EmitSignal(nameof(OnVisualChanged), "armor", visualId);
            }
        }

        /// <summary>
        /// 设置饰品外观
        /// </summary>
        public void SetAccessoryVisual(string visualId) {
            if (_accessoryVisuals.ContainsKey(visualId)) {
                _accessoryVisualId = visualId;
                EmitSignal(nameof(OnVisualChanged), "accessory", visualId);
            }
        }

        /// <summary>
        /// 获取当前武器外观ID
        /// </summary>
        public string GetWeaponVisualId() => _weaponVisualId;

        /// <summary>
        /// 获取当前防具外观ID
        /// </summary>
        public string GetArmorVisualId() => _armorVisualId;

        /// <summary>
        /// 获取当前饰品外观ID
        /// </summary>
        public string GetAccessoryVisualId() => _accessoryVisualId;

        /// <summary>
        /// 获取武器外观数据
        /// </summary>
        public WeaponVisual GetWeaponVisual() {
            return _weaponVisuals.ContainsKey(_weaponVisualId) ? _weaponVisuals[_weaponVisualId] : null;
        }

        /// <summary>
        /// 获取防具外观数据
        /// </summary>
        public ArmorVisual GetArmorVisual() {
            return _armorVisuals.ContainsKey(_armorVisualId) ? _armorVisuals[_armorVisualId] : null;
        }

        /// <summary>
        /// 获取饰品外观数据
        /// </summary>
        public AccessoryVisual GetAccessoryVisual() {
            return _accessoryVisuals.ContainsKey(_accessoryVisualId) ? _accessoryVisuals[_accessoryVisualId] : null;
        }

        /// <summary>
        /// 获取所有武器外观
        /// </summary>
        public Dictionary<string, WeaponVisual> GetAllWeaponVisuals() => _weaponVisuals;

        /// <summary>
        /// 获取所有防具外观
        /// </summary>
        public Dictionary<string, ArmorVisual> GetAllArmorVisuals() => _armorVisuals;

        /// <summary>
        /// 获取所有饰品外观
        /// </summary>
        public Dictionary<string, AccessoryVisual> GetAllAccessoryVisuals() => _accessoryVisuals;

        /// <summary>
        /// 检查外观是否已解锁
        /// </summary>
        public bool IsVisualUnlocked(string slot, string visualId) {
            // 默认外观总是解锁
            if (visualId == "default_sword" || visualId == "default_armor" || visualId == "default_accessory") {
                return true;
            }

            // 从已解锁数据中检查
            if (_unlockedVisuals.TryGetValue(slot, out var unlocked)) {
                return unlocked.Contains(visualId);
            }
            return false;
        }

        /// <summary>
        /// 解锁外观
        /// </summary>
        public void UnlockVisual(string slot, string visualId) {
            if (!_unlockedVisuals.ContainsKey(slot)) {
                _unlockedVisuals[slot] = new HashSet<string>();
            }
            _unlockedVisuals[slot].Add(visualId);
            GD.Print($"外观已解锁: {slot} - {visualId}");
        }

        /// <summary>
        /// 序列化 - 保存数据
        /// </summary>
        public Dictionary<string, string> Serialize() {
            return new Dictionary<string, string> {
                { "weaponVisualId", _weaponVisualId },
                { "armorVisualId", _armorVisualId },
                { "accessoryVisualId", _accessoryVisualId }
            };
        }

        /// <summary>
        /// 反序列化 - 加载数据
        /// </summary>
        public void Deserialize(Dictionary<string, string> data) {
            if (data == null) return;

            if (data.ContainsKey("weaponVisualId")) {
                _weaponVisualId = data["weaponVisualId"];
            }
            if (data.ContainsKey("armorVisualId")) {
                _armorVisualId = data["armorVisualId"];
            }
            if (data.ContainsKey("accessoryVisualId")) {
                _accessoryVisualId = data["accessoryVisualId"];
            }
        }
        
        /// <summary>
        /// 获取已解锁外观数据（用于存档）
        /// </summary>
        public Dictionary<string, string[]> GetUnlockedVisualsData() {
            var result = new Dictionary<string, string[]>();
            foreach (var kvp in _unlockedVisuals) {
                result[kvp.Key] = new List<string>(kvp.Value).ToArray();
            }
            return result;
        }
        
        /// <summary>
        /// 加载已解锁外观数据（从存档）
        /// </summary>
        public void LoadUnlockedVisualsData(Dictionary<string, string[]> data) {
            if (data == null) return;
            
            foreach (var kvp in data) {
                if (_unlockedVisuals.ContainsKey(kvp.Key)) {
                    _unlockedVisuals[kvp.Key].Clear();
                    foreach (var visualId in kvp.Value) {
                        _unlockedVisuals[kvp.Key].Add(visualId);
                    }
                } else {
                    _unlockedVisuals[kvp.Key] = new HashSet<string>(kvp.Value);
                }
            }
        }

        #region BaseSystem Persistence

        public Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            // 保存当前装备的外观
            data["weapon_visual_id"] = _weaponVisualId ?? "default_sword";
            data["armor_visual_id"] = _armorVisualId ?? "default_armor";
            data["accessory_visual_id"] = _accessoryVisualId ?? "default_accessory";

            // 保存已解锁的外观
            var unlockedVisualsData = new Dictionary<string, List<string>>();
            foreach (var slotKvp in _unlockedVisuals)
            {
                unlockedVisualsData[slotKvp.Key] = new List<string>(slotKvp.Value);
            }
            data["unlocked_visuals"] = unlockedVisualsData;

            return data;
        }

        public void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            // 加载当前装备的外观
            if (data.TryGetValue("weapon_visual_id", out var weaponId))
                _weaponVisualId = (string)weaponId;
            if (data.TryGetValue("armor_visual_id", out var armorId))
                _armorVisualId = (string)armorId;
            if (data.TryGetValue("accessory_visual_id", out var accessoryId))
                _accessoryVisualId = (string)accessoryId;

            // 加载已解锁的外观
            if (data.TryGetValue("unlocked_visuals", out var unlockedData))
            {
                _unlockedVisuals = new Dictionary<string, HashSet<string>>();
                var unlockedDict = (Dictionary<string, Variant>)unlockedData;
                foreach (var slotKvp in unlockedDict)
                {
                    _unlockedVisuals[slotKvp.Key] = new HashSet<string>((IEnumerable<string>)slotKvp.Value);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 武器外观数据
    /// </summary>
    public class WeaponVisual {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ModelPath { get; set; }
        public string TexturePath { get; set; }
        public string ParticleEffect { get; set; }
        public string Rarity { get; set; }
        public string UnlockRequirement { get; set; }
    }

    /// <summary>
    /// 防具外观数据
    /// </summary>
    public class ArmorVisual {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ModelPath { get; set; }
        public string TexturePath { get; set; }
        public string SpecialEffect { get; set; }
        public string Rarity { get; set; }
        public string UnlockRequirement { get; set; }
    }

    /// <summary>
    /// 饰品外观数据
    /// </summary>
    public class AccessoryVisual {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ModelPath { get; set; }
        public string TexturePath { get; set; }
        public string GlowColor { get; set; }
        public string ParticleEffect { get; set; }
        public string Rarity { get; set; }
        public string UnlockRequirement { get; set; }
    }
}
