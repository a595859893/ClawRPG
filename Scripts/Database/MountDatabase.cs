using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑数据库
    /// </summary>
    public class MountDatabase : Node {
        public static MountDatabase Instance { get; private set; }

        private Dictionary<string, Mount> _mounts = new Dictionary<string, Mount>();

        public override void _Ready() {
            Instance = this;
            InitializeMounts();
        }

        private void InitializeMounts() {
            // 陆地坐骑 - 普通
            AddMount(new Mount {
                MountId = "horse",
                Name = "战马",
                Description = "基础陆地坐骑，提供稳定的移动速度加成",
                Type = MountType.Land,
                Rarity = MountRarity.Common,
                SpeedBonus = 50,
                HealthBonus = 50,
                DefenseBonus = 10,
                CarryCapacityBonus = 20,
                UnlockLevel = 1,
                Price = 0,
                CanFly = false,
                CanSwim = false
            });

            AddMount(new Mount {
                MountId = "dire_wolf",
                Name = "恐狼",
                Description = "凶猛的狼坐骑，奔跑速度快",
                Type = MountType.Land,
                Rarity = MountRarity.Uncommon,
                SpeedBonus = 80,
                HealthBonus = 80,
                DefenseBonus = 15,
                CarryCapacityBonus = 30,
                UnlockLevel = 5,
                Price = 500,
                CanFly = false,
                CanSwim = false
            });

            AddMount(new Mount {
                MountId = "armored_bear",
                Name = "装甲熊",
                Description = "体型巨大的熊坐骑，提供高额防御加成",
                Type = MountType.Land,
                Rarity = MountRarity.Rare,
                SpeedBonus = 60,
                HealthBonus = 150,
                DefenseBonus = 40,
                CarryCapacityBonus = 50,
                UnlockLevel = 10,
                Price = 2000,
                CanFly = false,
                CanSwim = false
            });

            // 飞行坐骑
            AddMount(new Mount {
                MountId = "giant_eagle",
                Name = "巨鹰",
                Description = "可以在空中飞翔的鸟类坐骑",
                Type = MountType.Flying,
                Rarity = MountRarity.Rare,
                SpeedBonus = 120,
                HealthBonus = 70,
                DefenseBonus = 20,
                CarryCapacityBonus = 25,
                UnlockLevel = 15,
                Price = 3000,
                CanFly = true,
                CanSwim = false
            });

            AddMount(new Mount {
                MountId = "gryphon",
                Name = "狮鹫",
                Description = "半狮半鹰的传说生物，飞行速度极快",
                Type = MountType.Flying,
                Rarity = MountRarity.Epic,
                SpeedBonus = 150,
                HealthBonus = 100,
                DefenseBonus = 30,
                CarryCapacityBonus = 35,
                UnlockLevel = 20,
                Price = 8000,
                CanFly = true,
                CanSwim = false
            });

            AddMount(new Mount {
                MountId = "dragon",
                Name = "巨龙",
                Description = "传说中的龙坐骑，提供最强大的属性加成",
                Type = MountType.Flying,
                Rarity = MountRarity.Legendary,
                SpeedBonus = 200,
                HealthBonus = 200,
                DefenseBonus = 50,
                CarryCapacityBonus = 50,
                UnlockLevel = 30,
                Price = 50000,
                CanFly = true,
                CanSwim = false
            });

            // 水生坐骑
            AddMount(new Mount {
                MountId = "sea_horse",
                Name = "海马",
                Description = "可以在水中自由游动的坐骑",
                Type = MountType.Aquatic,
                Rarity = MountRarity.Uncommon,
                SpeedBonus = 70,
                HealthBonus = 60,
                DefenseBonus = 15,
                CarryCapacityBonus = 25,
                UnlockLevel = 8,
                Price = 800,
                CanFly = false,
                CanSwim = true
            });

            AddMount(new Mount {
                MountId = "water_elemental",
                Name = "水元素",
                Description = "由水元素构成的坐骑，在水中如鱼得水",
                Type = MountType.Aquatic,
                Rarity = MountRarity.Epic,
                SpeedBonus = 130,
                HealthBonus = 120,
                DefenseBonus = 35,
                CarryCapacityBonus = 40,
                UnlockLevel = 22,
                Price = 10000,
                CanFly = false,
                CanSwim = true
            });

            // 两栖坐骑
            AddMount(new Mount {
                MountId = "swamp_turtle",
                Name = "沼泽龟",
                Description = "可以在陆地和水中使用的乌龟坐骑",
                Type = MountType.Amphibian,
                Rarity = MountRarity.Common,
                SpeedBonus = 40,
                HealthBonus = 100,
                DefenseBonus = 30,
                CarryCapacityBonus = 40,
                UnlockLevel = 3,
                Price = 300,
                CanFly = false,
                CanSwim = true
            });

            AddMount(new Mount {
                MountId = "magic_carpet",
                Name = "魔法飞毯",
                Description = "神秘的东方坐骑，可在水面和空中移动",
                Type = MountType.Amphibian,
                Rarity = MountRarity.Epic,
                SpeedBonus = 140,
                HealthBonus = 80,
                DefenseBonus = 25,
                CarryCapacityBonus = 30,
                UnlockLevel = 18,
                Price = 12000,
                CanFly = true,
                CanSwim = true
            });

            // 稀有坐骑
            AddMount(new Mount {
                MountId = "phantom_steed",
                Name = "幽灵骏马",
                Description = "由灵魂构成的幽灵战马，日行千里",
                Type = MountType.Land,
                Rarity = MountRarity.Epic,
                SpeedBonus = 180,
                HealthBonus = 90,
                DefenseBonus = 25,
                CarryCapacityBonus = 30,
                UnlockLevel = 25,
                Price = 15000,
                CanFly = false,
                CanSwim = false
            });

            AddMount(new Mount {
                MountId = "phoenix",
                Name = "凤凰",
                Description = "浴火重生的神鸟，永恒的象征",
                Type = MountType.Flying,
                Rarity = MountRarity.Legendary,
                SpeedBonus = 220,
                HealthBonus = 180,
                DefenseBonus = 45,
                CarryCapacityBonus = 45,
                UnlockLevel = 35,
                Price = 80000,
                CanFly = true,
                CanSwim = false
            });
        }

        private void AddMount(Mount mount) {
            _mounts[mount.MountId] = mount;
        }

        public Mount GetMount(string mountId) {
            return _mounts.ContainsKey(mountId) ? _mounts[mountId] : null;
        }

        public List<Mount> GetAllMounts() {
            return new List<Mount>(_mounts.Values);
        }

        public List<Mount> GetMountsByType(MountType type) {
            List<Mount> result = new List<Mount>();
            foreach (var mount in _mounts.Values) {
                if (mount.Type == type) result.Add(mount);
            }
            return result;
        }

        public List<Mount> GetMountsByRarity(MountRarity rarity) {
            List<Mount> result = new List<Mount>();
            foreach (var mount in _mounts.Values) {
                if (mount.Rarity == rarity) result.Add(mount);
            }
            return result;
        }

        public List<Mount> GetAvailableMounts(int playerLevel) {
            List<Mount> result = new List<Mount>();
            foreach (var mount in _mounts.Values) {
                if (mount.UnlockLevel <= playerLevel) result.Add(mount);
            }
            return result;
        }
    }
}
