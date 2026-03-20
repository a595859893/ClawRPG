using Godot;
using System;

namespace ClawRPG.Systems;

/// <summary>
/// 圣物稀有度
/// </summary>
public enum RelicRarity
{
    Common,      // 普通 - 白色
    Uncommon,    // 优秀 - 绿色
    Rare,       // 稀有 - 蓝色
    Epic,       // 史诗 - 紫色
    Legendary    // 传说 - 橙色
}

/// <summary>
/// 圣物槽位类型
/// </summary>
public enum RelicSlotType
{
    Head,       // 头部
    Chest,      // 胸部
    Weapon,     // 武器
    Accessory,  // 饰品
    Offhand,    // 副手
    Ring,       // 戒指
    Amulet,     // 护符
    Talisman    // 护身符
}
