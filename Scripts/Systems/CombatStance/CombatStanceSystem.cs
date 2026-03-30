using Godot;
using System;
using System.Collections.Generic;

public partial class CombatStanceSystem : BaseSystem
{
    public static CombatStanceSystem Instance { get; private set; }
    
    // 战斗姿态类型
    public enum StanceType
    {
        Balanced,    // 平衡姿态
        Aggressive,  // 攻击姿态
        Defensive,   // 防御姿态
        Swift,       // 迅捷姿态
        Berserker,   // 狂热姿态
        Guardian     // 守护姿态
    }
    
    // 姿态配置
    private Dictionary<StanceType, StanceConfig> stanceConfigs = new Dictionary<StanceType, StanceConfig>();
    
    // 当前姿态
    private StanceType currentStance = StanceType.Balanced;
    private int stanceLevel = 1;
    
    // 姿态持续时间相关
    private float currentStanceDuration = 0f;
    private float maxStanceDuration = 0f;
    private bool isStanceActive = false;
    
    // 信号
    public delegate void StanceChangedEventHandler(StanceType newStance, StanceType oldStance);
    public delegate void StanceExpiredEventHandler();
    public delegate void StanceLevelUpEventHandler(int newLevel);
    
    public override void _Ready()
    {
        Instance = this;
        InitializeStanceConfigs();
    }
    
    public override void _Process(double delta)
    {
        if (isStanceActive && maxStanceDuration > 0)
        {
            currentStanceDuration -= (float)delta;
            if (currentStanceDuration <= 0)
            {
                currentStanceDuration = 0;
                isStanceActive = false;
                EmitSignal(SignalName.StanceExpired);
            }
        }
    }
    
    private void InitializeStanceConfigs()
    {
        // 平衡姿态 - 均衡加成
        stanceConfigs[StanceType.Balanced] = new StanceConfig
        {
            name = "平衡姿态",
            description = "攻守平衡，各项属性小幅提升",
            attackBonus = 1.1f,
            defenseBonus = 1.1f,
            speedBonus = 1.05f,
            critRateBonus = 0.05f,
            dodgeBonus = 0.05f,
            lifestealBonus = 0.05f,
            maxDuration = 0, // 0 表示无限持续
            staminaCost = 0,
            iconColor = new Color(1f, 1f, 1f)
        };
        
        // 攻击姿态 - 高攻击低防御
        stanceConfigs[StanceType.Aggressive] = new StanceConfig
        {
            name = "攻击姿态",
            description = "牺牲防御换取强大攻击力",
            attackBonus = 1.5f,
            defenseBonus = 0.7f,
            speedBonus = 1.1f,
            critRateBonus = 0.15f,
            critDamageBonus = 0.3f,
            dodgeBonus = 0f,
            maxDuration = 30f,
            staminaCost = 10f,
            iconColor = new Color(1f, 0.3f, 0.3f)
        };
        
        // 防御姿态 - 高防御
        stanceConfigs[StanceType.Defensive] = new StanceConfig
        {
            name = "防御姿态",
            description = "最大化防御，减少受到的伤害",
            attackBonus = 0.7f,
            defenseBonus = 2.0f,
            speedBonus = 0.8f,
            blockBonus = 0.3f,
            damageReduction = 0.25f,
            maxDuration = 45f,
            staminaCost = 8f,
            iconColor = new Color(0.3f, 0.3f, 1f)
        };
        
        // 迅捷姿态 - 高闪避高速度
        stanceConfigs[StanceType.Swift] = new StanceConfig
        {
            name = "迅捷姿态",
            description = "以闪避和速度为优势",
            attackBonus = 0.9f,
            defenseBonus = 0.8f,
            speedBonus = 1.5f,
            dodgeBonus = 0.25f,
            critRateBonus = 0.1f,
            maxDuration = 35f,
            staminaCost = 12f,
            iconColor = new Color(0.3f, 1f, 0.3f)
        };
        
        // 狂热姿态 - 高风险高回报
        stanceConfigs[StanceType.Berserker] = new StanceConfig
        {
            name = "狂热姿态",
            description = "极度危险：攻击力和暴击大幅提升，但受到的伤害增加",
            attackBonus = 2.0f,
            defenseBonus = 0.5f,
            speedBonus = 1.2f,
            critRateBonus = 0.25f,
            critDamageBonus = 0.5f,
            incomingDamageBonus = 1.5f,
            maxDuration = 20f,
            staminaCost = 15f,
            iconColor = new Color(1f, 0.5f, 0f)
        };
        
        // 守护姿态 - 保护队友（如果有的话）
        stanceConfigs[StanceType.Guardian] = new StanceConfig
        {
            name = "守护姿态",
            description = "为队友提供保护，减免伤害",
            attackBonus = 0.6f,
            defenseBonus = 1.8f,
            shieldBonus = 0.5f,
            allyDamageReduction = 0.2f,
            maxDuration = 40f,
            staminaCost = 12f,
            iconColor = new Color(1f, 0.8f, 0.2f)
        };
    }
    
    // 切换姿态
    public bool SwitchStance(StanceType newStance, bool ignoreDuration = false)
    {
        if (!stanceConfigs.ContainsKey(newStance))
            return false;
        
        var config = stanceConfigs[newStance];
        
        // 检查耐力是否足够
        if (Player.Instance != null)
        {
            float currentStamina = Player.Instance.GetNode<PlayerStats>(PlayerStats.Instance.Path).CurrentStamina;
            if (currentStamina < config.staminaCost)
                return false;
            
            // 消耗耐力
            Player.Instance.GetNode<PlayerStats>(PlayerStats.Instance.Path).ModifyStamina(-config.staminaCost);
        }
        
        var oldStance = currentStance;
        currentStance = newStance;
        
        // 设置持续时间
        if (config.maxDuration > 0)
        {
            isStanceActive = true;
            currentStanceDuration = config.maxDuration;
            maxStanceDuration = config.maxDuration;
        }
        else
        {
            isStanceActive = false;
            currentStanceDuration = 0;
            maxStanceDuration = 0;
        }
        
        EmitSignal(SignalName.StanceChanged, newStance, oldStance);
        
        GD.Print($"[CombatStance] Switched from {oldStance} to {newStance}");
        return true;
    }
    
    // 获取当前姿态配置
    public StanceConfig GetCurrentStanceConfig()
    {
        if (stanceConfigs.ContainsKey(currentStance))
            return stanceConfigs[currentStance];
        return null;
    }
    
    // 获取当前姿态
    public StanceType GetCurrentStance() => currentStance;
    
    // 获取姿态持续时间比例 (0-1)
    public float GetStanceDurationRatio()
    {
        if (maxStanceDuration <= 0) return 1f;
        return currentStanceDuration / maxStanceDuration;
    }
    
    // 获取姿态等级
    public int GetStanceLevel() => stanceLevel;
    
    // 提升姿态等级
    public void LevelUpStance()
    {
        stanceLevel++;
        EmitSignal(SignalName.StanceLevelUp, stanceLevel);
        GD.Print($"[CombatStance] Stance level up to {stanceLevel}");
    }
    
    // 获取攻击加成
    public float GetAttackBonus()
    {
        var config = GetCurrentStanceConfig();
        return config != null ? config.attackBonus : 1f;
    }
    
    // 获取防御加成
    public float GetDefenseBonus()
    {
        var config = GetCurrentStanceConfig();
        return config != null ? config.defenseBonus : 1f;
    }
    
    // 获取速度加成
    public float GetSpeedBonus()
    {
        var config = GetCurrentStanceConfig();
        float baseBonus = config != null ? config.speedBonus : 1f;
        
        // 等级加成：每级+2%
        float levelBonus = 1f + (stanceLevel - 1) * 0.02f;
        return baseBonus * levelBonus;
    }
    
    // 获取暴击率加成
    public float GetCritRateBonus()
    {
        var config = GetCurrentStanceConfig();
        return config != null ? config.critRateBonus : 0f;
    }
    
    // 获取暴击伤害加成
    public float GetCritDamageBonus()
    {
        var config = GetCurrentStanceConfig();
        float baseBonus = config != null ? config.critDamageBonus : 0f;
        
        // 等级加成
        float levelBonus = (stanceLevel - 1) * 0.05f;
        return baseBonus + levelBonus;
    }
    
    // 获取闪避加成
    public float GetDodgeBonus()
    {
        var config = GetCurrentStanceConfig();
        return config != null ? config.dodgeBonus : 0f;
    }
    
    // 获取受到的伤害加成（狂热姿态）
    public float GetIncomingDamageBonus()
    {
        var config = GetCurrentStanceConfig();
        return config != null ? config.incomingDamageBonus : 1f;
    }
    
    // 获取伤害减免
    public float GetDamageReduction()
    {
        var config = GetCurrentStanceConfig();
        float baseReduction = config != null ? config.damageReduction : 0f;
        
        // 等级加成
        float levelBonus = (stanceLevel - 1) * 0.02f;
        return baseReduction + levelBonus;
    }
    
    // 获取所有姿态类型
    public StanceType[] GetAllStanceTypes()
    {
        return (StanceType[])Enum.GetValues(typeof(StanceType));
    }
    
    // 获取姿态名称
    public string GetStanceName(StanceType stance)
    {
        if (stanceConfigs.ContainsKey(stance))
            return stanceConfigs[stance].name;
        return stance.ToString();
    }
    
    // 获取姿态描述
    public string GetStanceDescription(StanceType stance)
    {
        if (stanceConfigs.ContainsKey(stance))
            return stanceConfigs[stance].description;
        return "";
    }
    
    // 获取姿态图标颜色
    public Color GetStanceIconColor(StanceType stance)
    {
        if (stanceConfigs.ContainsKey(stance))
            return stanceConfigs[stance].iconColor;
        return Colors.White;
    }
    
    // 保存数据
    public Dictionary<string, Variant> SaveData()
    {
        return new Dictionary<string, Variant>
        {
            { "currentStance", (int)currentStance },
            { "stanceLevel", stanceLevel }
        };
    }
    
    // 加载数据
    public void LoadData(Dictionary<string, Variant> data)
    {
        if (data.ContainsKey("currentStance"))
            currentStance = (StanceType)(int)data["currentStance"];
        if (data.ContainsKey("stanceLevel"))
            stanceLevel = (int)data["stanceLevel"];
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return SaveData();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("currentStance"))
            currentStance = (StanceType)(int)data["currentStance"];
        if (data.Contains("stanceLevel"))
            stanceLevel = (int)data["stanceLevel"];
    }
}

// 姿态配置类
public class StanceConfig
{
    public string name { get; set; } = "";
    public string description { get; set; } = "";
    public float attackBonus { get; set; } = 1f;
    public float defenseBonus { get; set; } = 1f;
    public float speedBonus { get; set; } = 1f;
    public float critRateBonus { get; set; } = 0f;
    public float critDamageBonus { get; set; } = 0f;
    public float dodgeBonus { get; set; } = 0f;
    public float blockBonus { get; set; } = 0f;
    public float lifestealBonus { get; set; } = 0f;
    public float shieldBonus { get; set; } = 0f;
    public float damageReduction { get; set; } = 0f;
    public float incomingDamageBonus { get; set; } = 1f;
    public float allyDamageReduction { get; set; } = 0f;
    public float maxDuration { get; set; } = 0f;
    public float staminaCost { get; set; } = 0f;
    public Color iconColor { get; set; } = Colors.White;
}
