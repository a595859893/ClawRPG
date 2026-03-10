using Godot;
using System;

/// <summary>
/// 游戏数学和随机数工具类
/// 提供常用的数学计算和随机数生成方法
/// </summary>
public static class GameMath
{
    // 全局随机数生成器 - 确保随机数质量
    private static Random _globalRandom = new Random();
    
    // 随机数生成器的锁对象 - 线程安全
    private static readonly object _randomLock = new object();
    
    /// <summary>
    /// 获取全局随机数生成器
    /// </summary>
    public static Random Random
    {
        get
        {
            lock (_randomLock)
            {
                return _globalRandom;
            }
        }
    }
    
    /// <summary>
    /// 生成指定范围内的随机整数 [min, max)
    /// </summary>
    public static int Range(int min, int max)
    {
        if (min >= max) return min;
        lock (_randomLock)
        {
            return _globalRandom.Next(min, max);
        }
    }
    
    /// <summary>
    /// 生成指定范围内的随机浮点数 [min, max)
    /// </summary>
    public static float Range(float min, float max)
    {
        if (Math.Abs(min - max) < 0.0001f) return min;
        lock (_randomLock)
        {
            return (float)(_globalRandom.NextDouble() * (max - min) + min);
        }
    }
    
    /// <summary>
    /// 计算百分比概率 - 返回 true 的概率为 percent%
    /// </summary>
    public static bool Chance(float percent)
    {
        if (percent >= 100f) return true;
        if (percent <= 0f) return false;
        lock (_randomLock)
        {
            return _globalRandom.NextDouble() * 100f < percent;
        }
    }
    
    /// <summary>
    /// 计算百分比概率 - 返回 true 的概率为 0-1 之间的小数
    /// </summary>
    public static bool Probability(float probability)
    {
        if (probability >= 1f) return true;
        if (probability <= 0f) return false;
        lock (_randomLock)
        {
            return _globalRandom.NextDouble() < probability;
        }
    }
    
    /// <summary>
    /// 根据权重随机选择 - 返回选中项的索引
    /// </summary>
    public static int WeightedChoice(float[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;
        
        float total = 0f;
        foreach (float w in weights)
        {
            total += w;
        }
        
        if (total <= 0f) return Range(0, weights.Length);
        
        float randomValue = Range(0f, total);
        float cumulative = 0f;
        
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (randomValue <= cumulative)
            {
                return i;
            }
        }
        
        return weights.Length - 1;
    }
    
    /// <summary>
    /// 根据权重随机选择 - 返回选中项的索引
    /// </summary>
    public static int WeightedChoice(System.Collections.Generic.List<float> weights)
    {
        if (weights == null || weights.Count == 0) return -1;
        return WeightedChoice(weights.ToArray());
    }
    
    /// <summary>
    /// 从数组中随机选择一个元素
    /// </summary>
    public static T RandomElement<T>(T[] array)
    {
        if (array == null || array.Length == 0) return default(T);
        return array[Range(0, array.Length)];
    }
    
    /// <summary>
    /// 从列表中随机选择一个元素
    /// </summary>
    public static T RandomElement<T>(System.Collections.Generic.List<T> list)
    {
        if (list == null || list.Count == 0) return default(T);
        return list[Range(0, list.Count)];
    }
    
    /// <summary>
    /// 随机打乱数组
    /// </summary>
    public static void Shuffle<T>(T[] array)
    {
        if (array == null || array.Length <= 1) return;
        
        lock (_randomLock)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = _globalRandom.Next(i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
    
    /// <summary>
    /// 随机打乱列表
    /// </summary>
    public static void Shuffle<T>(System.Collections.Generic.List<T> list)
    {
        if (list == null || list.Count <= 1) return;
        
        lock (_randomLock)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _globalRandom.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
    
    /// <summary>
    /// 计算两点之间的距离
    /// </summary>
    public static float Distance(Vector2 a, Vector2 b)
    {
        return a.DistanceTo(b);
    }
    
    /// <summary>
    /// 计算两点之间的距离（3D）
    /// </summary>
    public static float Distance3D(Vector3 a, Vector3 b)
    {
        return a.DistanceTo(b);
    }
    
    /// <summary>
    /// 将角度规范化到 [0, 360) 范围
    /// </summary>
    public static float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }
    
    /// <summary>
    /// 将角度规范化到 [-180, 180) 范围
    /// </summary>
    public static float NormalizeAngle180(float angle)
    {
        angle = angle % 360f;
        if (angle > 180f) angle -= 360f;
        else if (angle < -180f) angle += 360f;
        return angle;
    }
    
    /// <summary>
    /// 计算线性插值
    /// </summary>
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    
    /// <summary>
    /// 计算角度插值
    /// </summary>
    public static float LerpAngle(float a, float b, float t)
    {
        float diff = NormalizeAngle180(b - a);
        return a + diff * t;
    }
    
    /// <summary>
    /// 将值限制在指定范围内
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        return Mathf.Clamp(value, min, max);
    }
    
    /// <summary>
    /// 将值限制在指定范围内
    /// </summary>
    public static int Clamp(int value, int min, int max)
    {
        return Mathf.Clamp(value, min, max);
    }
    
    /// <summary>
    /// 平滑过渡 - 使用平滑函数
    /// </summary>
    public static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
    
    /// <summary>
    /// 更平滑的过渡
    /// </summary>
    public static float SmootherStep(float edge0, float edge1, float x)
    {
        float t = Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }
    
    /// <summary>
    /// 计算伤害波动 - 返回基础伤害的波动值
    /// </summary>
    public static float DamageVariance(float baseDamage, float variancePercent)
    {
        if (variancePercent <= 0f) return baseDamage;
        float variation = Range(1f - variancePercent / 100f, 1f + variancePercent / 100f);
        return baseDamage * variation;
    }
    
    /// <summary>
    /// 计算暴击伤害
    /// </summary>
    public static float CalculateCritDamage(float damage, float critMultiplier)
    {
        return damage * critMultiplier;
    }
    
    /// <summary>
    /// 计算最终伤害（考虑各种修正）
    /// </summary>
    public static float CalculateFinalDamage(
        float baseDamage,
        float attackBonus = 0f,
        float defenseReduction = 0f,
        float elementalBonus = 0f,
        float critMultiplier = 1f,
        bool isCrit = false,
        float variancePercent = 0f)
    {
        // 应用攻击加成
        float damage = baseDamage * (1f + attackBonus / 100f);
        
        // 应用防御减免
        damage *= (1f - defenseReduction / 100f);
        
        // 应用元素加成
        damage *= (1f + elementalBonus / 100f);
        
        // 应用伤害波动
        if (variancePercent > 0f)
        {
            damage = DamageVariance(damage, variancePercent);
        }
        
        // 应用暴击
        if (isCrit)
        {
            damage = CalculateCritDamage(damage, critMultiplier);
        }
        
        return Mathf.Max(0f, damage);
    }
    
    /// <summary>
    /// 计算经验值需求（等级相关）
    /// </summary>
    public static float ExpRequirement(int level, float baseExp = 100f, float multiplier = 1.5f)
    {
        return baseExp * Mathf.Pow(level, multiplier);
    }
    
    /// <summary>
    /// 计算掉落概率 - 基于等级差
    /// </summary>
    public static float DropRateByLevelDiff(int playerLevel, int enemyLevel, float baseRate = 1f)
    {
        int diff = enemyLevel - playerLevel;
        if (diff <= 0) return baseRate;
        
        // 等级越高，掉落越容易
        return baseRate * (1f + diff * 0.1f);
    }
}
