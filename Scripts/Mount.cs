using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑数据类
    /// </summary>
    public class Mount : Resource {
        [Export] public string MountId { get; set; }
        [Export] public string Name { get; set; }
        [Export] public string Description { get; set; }
        [Export] public MountType Type { get; set; }
        [Export] public MountRarity Rarity { get; set; }
        [Export] public int SpeedBonus { get; set; }        // 移动速度加成
        [Export] public int HealthBonus { get; set; }        // 生命值加成
        [Export] public int DefenseBonus { get; set; }       // 防御加成
        [Export] public int CarryCapacityBonus { get; set; } // 背包容量加成
        [Export] public int UnlockLevel { get; set; }         // 解锁所需等级
        [Export] public int Price { get; set; }              // 价格（金币）
        [Export] public bool CanFly { get; set; }            // 是否能飞行
        [Export] public bool CanSwim { get; set; }           // 是否能游泳
        [Export] public string TexturePath { get; set; }     // 图标路径
    }

    public enum MountType {
        Land,       // 陆地
        Flying,     // 飞行
        Aquatic,    // 水生
        Amphibian   // 两栖
    }

    public enum MountRarity {
        Common,     // 普通
        Uncommon,   // 优秀
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }

    /// <summary>
    /// 玩家坐骑实例
    /// </summary>
    public class MountInstance {
        public string MountId { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public bool IsActive { get; set; }
        public DateTime ObtainedAt { get; set; }

        public MountInstance() {
            Level = 1;
            Experience = 0;
            IsActive = false;
            ObtainedAt = DateTime.Now;
        }

        public int GetExpForNextLevel() {
            return Level * 100 + Level * Level * 10;
        }

        public bool CanLevelUp() {
            return Experience >= GetExpForNextLevel();
        }

        public void AddExperience(int exp) {
            Experience += exp;
            while (CanLevelUp()) {
                Experience -= GetExpForNextLevel();
                Level++;
            }
        }
    }
}
