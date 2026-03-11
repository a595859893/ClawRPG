using Godot;
using System;
using System.Collections.Generic;

public class PetMorphDatabase
{
    private static Dictionary<string, PetMorph> _morphs = new Dictionary<string, PetMorph>();
    
    public static void Initialize()
    {
        // Normal 形态 - 基础形态
        AddMorph(new PetMorph
        {
            MorphId = "normal_basic",
            MorphName = "普通形态",
            MorphType = PetMorphType.Normal,
            Description = "宠物的基础形态",
            RequiredAffectionLevel = 1,
            UnlockCost = 0,
            AttackBonus = 0f,
            DefenseBonus = 0f,
            HealthBonus = 0f,
            SpeedBonus = 0f,
            CritRateBonus = 0f,
            CritDamageBonus = 0f,
            LifeStealBonus = 0f,
            SpecialEffect = "none",
            EffectValue = 0f,
            VisualEffect = "none",
            GlowColor = new Color(1f, 1f, 1f, 0.3f)
        });
        
        // Battle 形态 - 战斗形态
        AddMorph(new PetMorph
        {
            MorphId = "battle_warrior",
            MorphName = "战士形态",
            MorphType = PetMorphType.Battle,
            Description = "增强攻击力的战斗形态",
            RequiredAffectionLevel = 3,
            UnlockCost = 500,
            AttackBonus = 25f,
            DefenseBonus = 10f,
            HealthBonus = 50f,
            SpeedBonus = 0f,
            CritRateBonus = 5f,
            CritDamageBonus = 10f,
            LifeStealBonus = 5f,
            SpecialEffect = "attack_boost",
            EffectValue = 1.25f,
            VisualEffect = "red_aura",
            GlowColor = new Color(1f, 0.3f, 0.3f, 0.5f)
        });
        
        AddMorph(new PetMorph
        {
            MorphId = "battle_berserker",
            MorphName = "狂战士形态",
            MorphType = PetMorphType.Battle,
            Description = "极端攻击力的狂暴形态",
            RequiredAffectionLevel = 6,
            UnlockCost = 2000,
            AttackBonus = 40f,
            DefenseBonus = -10f,
            HealthBonus = 0f,
            SpeedBonus = 5f,
            CritRateBonus = 10f,
            CritDamageBonus = 20f,
            LifeStealBonus = 10f,
            SpecialEffect = "berserk",
            EffectValue = 1.5f,
            VisualEffect = "red_flames",
            GlowColor = new Color(1f, 0.1f, 0.1f, 0.7f)
        });
        
        // Speed 形态 - 速度形态
        AddMorph(new PetMorph
        {
            MorphId = "speed_swift",
            MorphName = "迅捷形态",
            MorphType = PetMorphType.Speed,
            Description = "提升速度的敏捷形态",
            RequiredAffectionLevel = 3,
            UnlockCost = 500,
            AttackBonus = 5f,
            DefenseBonus = 0f,
            HealthBonus = 0f,
            SpeedBonus = 30f,
            CritRateBonus = 10f,
            CritDamageBonus = 0f,
            LifeStealBonus = 0f,
            SpecialEffect = "dodge_boost",
            EffectValue = 15f,
            VisualEffect = "blue_swirl",
            GlowColor = new Color(0.3f, 0.5f, 1f, 0.5f)
        });
        
        AddMorph(new PetMorph
        {
            MorphId = "speed_flash",
            MorphName = "闪电形态",
            MorphType = PetMorphType.Speed,
            Description = "极致速度的闪电形态",
            RequiredAffectionLevel = 7,
            UnlockCost = 3000,
            AttackBonus = 15f,
            DefenseBonus = 0f,
            HealthBonus = 0f,
            SpeedBonus = 50f,
            CritRateBonus = 15f,
            CritDamageBonus = 5f,
            LifeStealBonus = 0f,
            SpecialEffect = "lightning_dodge",
            EffectValue = 25f,
            VisualEffect = "lightning_particles",
            GlowColor = new Color(0.8f, 0.9f, 1f, 0.7f)
        });
        
        // Tank 形态 - 防御形态
        AddMorph(new PetMorph
        {
            MorphId = "tank_guardian",
            MorphName = "守护形态",
            MorphType = PetMorphType.Tank,
            Description = "高防御的保护形态",
            RequiredAffectionLevel = 4,
            UnlockCost = 800,
            AttackBonus = 0f,
            DefenseBonus = 30f,
            HealthBonus = 100f,
            SpeedBonus = -10f,
            CritRateBonus = 0f,
            CritDamageBonus = 0f,
            LifeStealBonus = 10f,
            SpecialEffect = "damage_reduction",
            EffectValue = 0.8f,
            VisualEffect = "blue_shield",
            GlowColor = new Color(0.2f, 0.4f, 0.8f, 0.5f)
        });
        
        AddMorph(new PetMorph
        {
            MorphId = "tank_titan",
            MorphName = "泰坦形态",
            MorphType = PetMorphType.Tank,
            Description = "极高防御力的巨兽形态",
            RequiredAffectionLevel = 8,
            UnlockCost = 4000,
            AttackBonus = 10f,
            DefenseBonus = 50f,
            HealthBonus = 200f,
            SpeedBonus = -20f,
            CritRateBonus = 0f,
            CritDamageBonus = 0f,
            LifeStealBonus = 15f,
            SpecialEffect = "iron_skin",
            EffectValue = 0.6f,
            VisualEffect = "stone_armor",
            GlowColor = new Color(0.4f, 0.4f, 0.5f, 0.7f)
        });
        
        // Magic 形态 - 魔法形态
        AddMorph(new PetMorph
        {
            MorphId = "magic_arcane",
            MorphName = "奥术形态",
            MorphType = PetMorphType.Magic,
            Description = "增强魔法攻击的形态",
            RequiredAffectionLevel = 5,
            UnlockCost = 1200,
            AttackBonus = 20f,
            DefenseBonus = 5f,
            HealthBonus = 0f,
            SpeedBonus = 5f,
            CritRateBonus = 15f,
            CritDamageBonus = 25f,
            LifeStealBonus = 0f,
            SpecialEffect = "magic_boost",
            EffectValue = 1.3f,
            VisualEffect = "purple_magic",
            GlowColor = new Color(0.6f, 0.3f, 0.8f, 0.5f)
        });
        
        AddMorph(new PetMorph
        {
            MorphId = "magic_elemental",
            MorphName = "元素形态",
            MorphType = PetMorphType.Magic,
            Description = "掌控元素力量的形态",
            RequiredAffectionLevel = 9,
            UnlockCost = 5000,
            AttackBonus = 35f,
            DefenseBonus = 10f,
            HealthBonus = 50f,
            SpeedBonus = 10f,
            CritRateBonus = 20f,
            CritDamageBonus = 35f,
            LifeStealBonus = 5f,
            SpecialEffect = "elemental_mastery",
            EffectValue = 1.5f,
            VisualEffect = "elemental_rainbow",
            GlowColor = new Color(0.8f, 0.4f, 0.9f, 0.7f)
        });
        
        // Elite 形态
        AddMorph(new PetMorph
        {
            MorphId = "elite_dragon",
            MorphName = "龙形态",
            MorphType = PetMorphType.Elite,
            Description = "获得龙之力量的精英形态",
            RequiredAffectionLevel = 6,
            UnlockCost = 3000,
            AttackBonus = 30f,
            DefenseBonus = 20f,
            HealthBonus = 80f,
            SpeedBonus = 15f,
            CritRateBonus = 10f,
            CritDamageBonus = 20f,
            LifeStealBonus = 10f,
            SpecialEffect = "dragon_breath",
            EffectValue = 1.4f,
            VisualEffect = "dragon_glow",
            GlowColor = new Color(1f, 0.6f, 0.2f, 0.6f)
        });
        
        // Legendary 形态
        AddMorph(new PetMorph
        {
            MorphId = "legendary_phoenix",
            MorphName = "凤凰形态",
            MorphType = PetMorphType.Legendary,
            Description = "浴火重生的传奇形态",
            RequiredAffectionLevel = 8,
            UnlockCost = 8000,
            AttackBonus = 40f,
            DefenseBonus = 25f,
            HealthBonus = 120f,
            SpeedBonus = 20f,
            CritRateBonus = 15f,
            CritDamageBonus = 30f,
            LifeStealBonus = 20f,
            SpecialEffect = "reborn",
            EffectValue = 0.5f,
            VisualEffect = "phoenix_flames",
            GlowColor = new Color(1f, 0.5f, 0.1f, 0.8f)
        });
        
        // Mythical 形态
        AddMorph(new PetMorph
        {
            MorphId = "mythical_god",
            MorphName = "神形态",
            MorphType = PetMorphType.Mythical,
            Description = "超越凡间的神级形态",
            RequiredAffectionLevel = 10,
            UnlockCost = 20000,
            AttackBonus = 60f,
            DefenseBonus = 40f,
            HealthBonus = 200f,
            SpeedBonus = 30f,
            CritRateBonus = 25f,
            CritDamageBonus = 50f,
            LifeStealBonus = 30f,
            SpecialEffect = "god_blessing",
            EffectValue = 2.0f,
            VisualEffect = "divine_aura",
            GlowColor = new Color(1f, 0.9f, 0.5f, 1f)
        });
    }
    
    private static void AddMorph(PetMorph morph)
    {
        _morphs[morph.MorphId] = morph;
    }
    
    public static PetMorph GetMorph(string morphId)
    {
        if (_morphs.ContainsKey(morphId))
            return _morphs[morphId];
        return null;
    }
    
    public static List<PetMorph> GetAllMorphs()
    {
        return new List<PetMorph>(_morphs.Values);
    }
    
    public static List<PetMorph> GetMorphsByType(PetMorphType type)
    {
        List<PetMorph> result = new List<PetMorph>();
        foreach (var morph in _morphs.Values)
        {
            if (morph.MorphType == type)
                result.Add(morph);
        }
        return result;
    }
    
    public static List<PetMorph> GetAvailableMorphs(int affectionLevel)
    {
        List<PetMorph> result = new List<PetMorph>();
        foreach (var morph in _morphs.Values)
        {
            if (morph.RequiredAffectionLevel <= affectionLevel)
                result.Add(morph);
        }
        return result;
    }
    
    public static Color GetMorphGlowColor(string morphId)
    {
        var morph = GetMorph(morphId);
        return morph != null ? morph.GlowColor : new Color(1f, 1f, 1f, 0.3f);
    }
}
