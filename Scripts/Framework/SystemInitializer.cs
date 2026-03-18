using Godot;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 系统初始化器 - 负责管理所有游戏系统的初始化
/// 减少 Main.cs 中的初始化代码
/// </summary>
public class SystemInitializer : BaseSystem
{
    /// <summary>
    /// 初始化所有系统
    /// </summary>
    public void InitializeAllSystems(Node parent)
    {
        GD.Print("=== Initializing All Systems ===");
        
        // 基础战斗系统
        InitializeCombatSystems(parent);
        
        // 角色系统
        InitializeCharacterSystems(parent);
        
        // 装备系统
        InitializeEquipmentSystems(parent);
        
        // 内容系统
        InitializeContentSystems(parent);
        
        // 社交系统
        InitializeSocialSystems(parent);
        
        // 活动系统
        InitializeEventSystems(parent);
        
        GD.Print("=== All Systems Initialized ===");
    }
    
    // ==================== BaseSystem 持久化接口 ====================
    public override Dictionary<string, object> ExportSaveData()
    {
        // SystemInitializer 是初始化器，无持久化状态
        return new Dictionary<string, object>();
    }

    public override bool ImportSaveData(Dictionary<string, object> data)
    {
        // SystemInitializer 是初始化器，无持久化状态
        return true;
    }
    // ==================== 持久化接口结束 ====================
    
    private void InitializeCombatSystems(Node parent)
    {
        // 武器专精系统
        AddSystem<WeaponMasterySystem>(parent, "WeaponMasterySystem");
        
        // 反击系统
        var counterAttackSystem = AddSystem<CounterAttackSystem>(parent, "CounterAttackSystem");
        
        // Bounty系统
        var bountyManager = BountyManager.Instance;
        bountyManager.Initialize();
    }
    
    private void InitializeCharacterSystems(Node parent)
    {
        // 称号系统
        AddSystem<TitleSystem>(parent, "TitleSystem");
        
        // 称号收集系统
        AddSystem<TitleCollectionSystem>(parent, "TitleCollectionSystem");
        
        // 宠物战斗AI
        var petCombatAI = AddSystem<PetCombatAI>(parent, "PetCombatAI");
        petCombatAI.Initialize();
        
        // 玩家档案系统
        AddSystem<PlayerProfileSystem>(parent, "PlayerProfileSystem");
        
        // 冥想系统
        AddSystem<MeditationSystem>(parent, "MeditationSystem");
    }
    
    private void InitializeEquipmentSystems(Node parent)
    {
        // 强化系统
        AddSystem<EnhancementSystem>(parent, "EnhancementSystem");
        
        // 装备套装系统
        AddSystem<EquipmentSetSystem>(parent, "EquipmentSetSystem");
        
        // 附魔系统
        var enchantmentSystem = AddSystem<Enchantment.EnchantmentSystem>(parent, "EnchantmentSystem");
    }
    
    private void InitializeContentSystems(Node parent)
    {
        // 神器系统
        AddSystem<ArtifactSystem>(parent, "ArtifactSystem");
        
        // 坐骑训练系统
        AddSystem<MountTrainingSystem>(parent, "MountTrainingSystem");
        
        // 神器融合系统
        AddSystem<ArtifactFusionSystem>(parent, "ArtifactFusionSystem");
        
        // 声望系统
        AddSystem<FactionSystem>(parent, "FactionSystem");
        
        // 天气系统
        AddSystem<WeatherSystem>(parent, "WeatherSystem");
    }
    
    private void InitializeSocialSystems(Node parent)
    {
        // 邮件系统
        AddSystem<MailManager>(parent, "MailManager");
    }
    
    private void InitializeEventSystems(Node parent)
    {
        // 每日登录奖励系统
        AddSystem<DailyLoginRewardSystem>(parent, "DailyLoginRewardSystem");
        
        // 成就徽章系统
        AddSystem<AchievementBadgeSystem>(parent, "AchievementBadgeSystem");
        
        // 隐藏成就系统
        AddSystem<SecretAchievementSystem>(parent, "SecretAchievementSystem");
    }
    
    /// <summary>
    /// 添加系统到父节点
    /// </summary>
    private T AddSystem<T>(Node parent, string name) where T : Node, new()
    {
        var system = new T();
        system.Name = name;
        parent.AddChild(system);
        
        if (system is BaseSystem baseSystem)
        {
            GameManager.Instance?.RegisterSystem(baseSystem);
        }
        
        GD.Print($"[SystemInitializer] Initialized: {name}");
        return system;
    }
}
