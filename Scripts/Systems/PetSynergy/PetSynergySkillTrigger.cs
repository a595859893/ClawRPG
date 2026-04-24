using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.PetSynergy;
using ClawRPG.Systems.PetFormation;

/// <summary>
/// 宠物默契技能触发器 (REQ-163-03 + REQ-176-05)
/// 监听宠物攻击信号，检查友谊关系，选择配合动画并显示通知。
/// REQ-176-05扩展：查询 PetFormationSystem 获取当前阵型，
/// 根据阵型调整配合动画的视觉风格（前锋=进攻型/铁桶=防御型等）。
/// </summary>
public partial class PetSynergySkillTrigger : BaseSystem
{
    private static PetSynergySkillTrigger _instance;
    public static PetSynergySkillTrigger Instance => _instance;

    private PetSynergySkillDatabase _database;
    private PetFormationSystem _formationSystem;

    // Animation variant overrides per formation
    private Dictionary<FormationType, Dictionary<string, string>> _formationAnimOverrides;

    public override void _Ready()
    {
        base._Ready();
        _instance = this;
        _database = PetSynergySkillDatabase.Instance;
        _formationSystem = PetFormationSystem.Instance;

        SubscribeToSignals();
        InitializeFormationAnimOverrides();
        GD.Print("[PetSynergySkillTrigger] 初始化完成");
    }

    protected override string SystemName => "PetSynergySkillTrigger";

    private void SubscribeToSignals()
    {
        // Subscribe to pet attack performed signal (graceful — only connect if signal exists)
        try
        {
            var petCombat = GetNodeOrNull("/root/Main/PetCombatCompanionSystem")
                         ?? GetNodeOrNull("/root/Main/CombatManager");
            if (petCombat != null)
            {
                if (petCombat.HasSignal("PetAttackPerformed"))
                    petCombat.Connect("PetAttackPerformed", Callable.From((string petId) => OnPetAttackPerformed(petId)));

                // Also listen to PetAttacked from PetCombatCompanionSystem (REQ-134)
                if (petCombat.HasSignal("PetAttacked"))
                    petCombat.Connect("PetAttacked", Callable.From((string petId, int damage) => OnPetAttacked(petId, damage)));
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[PetSynergySkillTrigger] Signal subscription failed: {ex.Message}");
        }
    }

    private void InitializeFormationAnimOverrides()
    {
        // REQ-176-05: Formation-specific animation variant mapping
        // Different formations produce different animation flavors for the same synergy skill
        _formationAnimOverrides = new Dictionary<FormationType, Dictionary<string, string>>();

        // AggressiveRush formation → faster, more intense animations
        _formationAnimOverrides[FormationType.AggressiveRush] = new Dictionary<string, string>
        {
            { "sync_attack", "sync_attack_rush" },
            { "guard_follow", "guard_follow_rush" },
            { "small_gesture", "small_gesture_aggressive" },
            { "medium_gesture", "medium_gesture_aggressive" }
        };

        // GuardFormation formation → defensive, protective animations
        _formationAnimOverrides[FormationType.GuardFormation] = new Dictionary<string, string>
        {
            { "sync_attack", "sync_attack_guard" },
            { "guard_follow", "guard_follow_deep" },
            { "small_gesture", "small_gesture_guard" },
            { "medium_gesture", "medium_gesture_guard" }
        };

        // Balanced formation → standard animations
        _formationAnimOverrides[FormationType.Balanced] = new Dictionary<string, string>
        {
            { "sync_attack", "sync_attack" },
            { "guard_follow", "guard_follow" },
            { "small_gesture", "small_gesture" },
            { "medium_gesture", "medium_gesture" }
        };

        // PincerSetup formation → flanking, coordinated animations
        _formationAnimOverrides[FormationType.PincerSetup] = new Dictionary<string, string>
        {
            { "sync_attack", "sync_attack_pincers" },
            { "guard_follow", "guard_follow_pincers" },
            { "small_gesture", "small_gesture_pincers" },
            { "medium_gesture", "medium_gesture_pincers" }
        };

        // FlexibleAssault formation → adaptive, fluid animations
        _formationAnimOverrides[FormationType.FlexibleAssault] = new Dictionary<string, string>
        {
            { "sync_attack", "sync_attack_flex" },
            { "guard_follow", "guard_follow_flex" },
            { "small_gesture", "small_gesture_flex" },
            { "medium_gesture", "medium_gesture_flex" }
        };

        // Solo formation → solo-friendly variants
        _formationAnimOverrides[FormationType.Solo] = new Dictionary<string, string>
        {
            { "sync_attack", "sync_attack_solo" },
            { "guard_follow", "guard_follow_solo" },
            { "small_gesture", "small_gesture_solo" },
            { "medium_gesture", "medium_gesture_solo" }
        };
    }

    private void OnPetAttackPerformed(string petId)
    {
        TryTriggerSynergy(petId);
    }

    private void OnPetAttacked(string petId, int damage)
    {
        // Optionally trigger defensive synergy animations
    }

    /// <summary>
    /// 尝试触发默契技能 (REQ-176-05: formation-aware)
    /// </summary>
    private void TryTriggerSynergy(string attackerPetId)
    {
        if (_database == null || _database.SynergySkills.Count == 0) return;

        // Get active formation (REQ-176-05 key extension)
        FormationType activeFormation = FormationType.None;
        if (_formationSystem != null)
        {
            activeFormation = _formationSystem.GetFormationType();
        }

        // Get friendship level for attacker
        int friendshipLevel = GetFriendshipLevel(attackerPetId);
        if (friendshipLevel <= 0) return;

        // Select best skill for friendship level
        var skillEntry = _database.GetSkillForFriendship(friendshipLevel);
        if (skillEntry == null) return;

        // Get formation-aware animation variant (REQ-176-05)
        string gestureAnim = GetFormationAwareGestureAnim(skillEntry, friendshipLevel, activeFormation);
        string attackAnim = GetFormationAwareAttackAnim(skillEntry, activeFormation);

        // Get second pet (the one doing the co-op animation)
        string partnerPetId = GetPartnerPetId(attackerPetId);
        if (string.IsNullOrEmpty(partnerPetId)) return;

        // Trigger co-op animation with delay
        float delay = skillEntry.TimingOffset;
        var timer = new Timer { OneShot = true, WaitTime = delay };
        timer.Timeout += () => OnSynergyTriggered(partnerPetId, gestureAnim, skillEntry.SkillId, activeFormation);
        AddChild(timer);
        timer.Start();
    }

    /// <summary>
    /// REQ-176-05: 根据当前阵型获取调整后的配合动画
    /// 不同阵型产生不同的动画风格变体
    /// </summary>
    private string GetFormationAwareGestureAnim(SynergySkillEntry entry, int friendshipLevel, FormationType formation)
    {
        // Determine base gesture tier
        string baseAnim;
        if (friendshipLevel >= 16)
            baseAnim = entry.HighTierGestureAnim ?? entry.MediumGestureAnim;
        else if (friendshipLevel >= 6)
            baseAnim = entry.MediumGestureAnim;
        else
            baseAnim = entry.SmallGestureAnim;

        // Map to formation-specific variant
        string tierKey = GetAnimTierKey(baseAnim);
        if (_formationAnimOverrides.TryGetValue(formation, out var overrideMap))
        {
            if (overrideMap.TryGetValue(tierKey, out var overrideAnim))
            {
                return overrideAnim;
            }
        }

        return baseAnim;
    }

    /// <summary>
    /// REQ-176-05: 根据阵型获取调整后的攻击动画
    /// </summary>
    private string GetFormationAwareAttackAnim(SynergySkillEntry entry, FormationType formation)
    {
        string baseAnim = entry.AnimationA;
        string tierKey = GetAnimTierKey(baseAnim);

        if (_formationAnimOverrides.TryGetValue(formation, out var overrideMap))
        {
            if (overrideMap.TryGetValue(tierKey, out var overrideAnim))
            {
                return overrideAnim;
            }
        }

        return baseAnim;
    }

    /// <summary>
    /// 将动画名称映射到覆盖层查找键
    /// </summary>
    private string GetAnimTierKey(string animName)
    {
        if (animName.Contains("small") || animName.Contains("nod") || animName.Contains("tail"))
            return "small_gesture";
        if (animName.Contains("medium") || animName.Contains("jump") || animName.Contains("spin"))
            return "medium_gesture";
        if (animName.Contains("sync_attack"))
            return "sync_attack";
        if (animName.Contains("guard_follow"))
            return "guard_follow";
        return animName;
    }

    private void OnSynergyTriggered(string partnerPetId, string gestureAnim, string skillId, FormationType formation)
    {
        // Emit signal for PetSynergyNotificationUI to display
        OnSynergyAnimationRequested?.Invoke(partnerPetId, gestureAnim, skillId, formation);
        GD.Print($"[PetSynergySkillTrigger] 默契触发: pet={partnerPetId} anim={gestureAnim} skill={skillId} formation={formation}");
    }

    public delegate void SynergyAnimationEventHandler(string petId, string animation, string skillId, FormationType formation);
    public event SynergyAnimationEventHandler OnSynergyAnimationRequested;

    private int GetFriendshipLevel(string petId)
    {
        // Query PetFriendshipSystem for friendship level via reflection (graceful degradation)
        // PetFriendshipSystem may not exist — return 0 (no synergy) rather than crashing
        try
        {
            var friendshipSystemType = Type.GetType("PetFriendshipSystem, Assembly-CSharp")
                              ?? Type.GetType("ClawRPG.Scripts.Systems.Pets.PetFriendshipSystem, Assembly-CSharp");
            if (friendshipSystemType == null) return 0;

            var instanceProp = friendshipSystemType.GetProperty("Instance",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var instance = instanceProp?.GetValue(null);
            if (instance == null) return 0;

            var method = friendshipSystemType.GetMethod("GetFriendshipLevel")
                      ?? friendshipSystemType.GetMethod("GetFriendship");
            if (method != null)
            {
                var result = method.Invoke(instance, new object[] { petId });
                if (result is int level) return level;
            }
        }
        catch (Exception)
        {
            // PetFriendshipSystem not available — no synergy animations
        }

        return 0;
    }

    private string GetPartnerPetId(string attackerPetId)
    {
        // Find another active pet that has friendship > 0 with attacker
        // This would query PetCombatCompanionSystem for active pets
        // For now, return null (no co-op animation if no partner found)
        // Integration point: PetCombatCompanionSystem.GetActivePetIds()
        var companionSystem = PetCombatCompanionSystem.Instance;
        if (companionSystem == null) return null;

        // Try to get other active pets
        var method = typeof(PetCombatCompanionSystem).GetMethod("GetActivePetIds");
        if (method != null)
        {
            var result = method.Invoke(companionSystem, null) as IEnumerable<string>;
            if (result != null)
            {
                foreach (var petId in result)
                {
                    if (petId != attackerPetId && GetFriendshipLevel(petId) > 0)
                        return petId;
                }
            }
        }

        return null;
    }
}
