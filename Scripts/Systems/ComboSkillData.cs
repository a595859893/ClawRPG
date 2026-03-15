// Combo Skill Data - 连击技能系统数据结构
// 连击技能数据结构定义

using Godot;

#pragma warning disable CS8618 // Non-nullable field is uninitialized

// 连击类型
public enum ComboType
{
	Sequential,    // 顺序释放 - 按顺序触发
	Parallel,      // 并行释放 - 同时触发
	Chain,         // 链式触发 - 前一个触发后一个
	Conditional    // 条件触发 - 满足条件触发
}

// 触发条件
public enum TriggerCondition
{
	OnHit,           // 命中时
	OnCritical,      // 暴击时
	OnKill,          // 击杀时
	OnDamageTaken,   // 受到伤害时
	OnHealthBelow,   // 生命低于X%
	OnManaBelow,     // 法力低于X%
	OnEnemyType,     // 针对特定敌人类型
	OnComboComplete, // 连击完成时
	Manual,          // 手动触发
	Cooldown         // 冷却触发
}

// 效果类型
public enum EffectType
{
	Damage,              // 伤害
	Heal,                // 治疗
	Shield,              // 护盾
	Buff,                // 增益
	Debuff,              // 减益
	Teleport,            // 传送
	Summon,              // 召唤
	Transform,           // 变形
	ClearDebuffs,        // 清除减益
	GrantInvulnerability // 无敌
}

// 技能效果
public class ComboSkillEffect : Resource
{
	public EffectType EffectType;
	public float Value;
	public float Duration = 0.0f;  // 持续时间，0为瞬发
	public string Description = "";
	public string Target = "enemy";  // enemy/self/ally
}

// 连击步骤
public class ComboStep : Resource
{
	public string SkillId = "";
	public float Delay = 0.0f;  // 延迟（秒）
	public TriggerCondition Condition = TriggerCondition.Manual;
	public float ConditionValue = 0.0f;  // 条件参数
	public Godot.Collections.Array<ComboSkillEffect> Effects = new Godot.Collections.Array<ComboSkillEffect>();
}

// 连击配置
public class ComboSkill : Resource
{
	public string Id = "";
	public string Name = "";
	public string Description = "";
	public ComboType ComboType;
	public Godot.Collections.Array<ComboStep> Steps = new Godot.Collections.Array<ComboStep>();
	public float TotalTime = 0.0f;  // 总释放时间
	public float Cooldown = 0.0f;
	public float ManaCost = 0.0f;
	public int LevelRequired = 1;
	public int Rarity = 0;  // 0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary
}

// 玩家已解锁的连击
public class PlayerComboSkill : Resource
{
	public string ComboId = "";
	public bool IsEquipped = false;
	public float CurrentCooldown = 0.0f;
	public int UseCount = 0;
}

// 连击执行状态
public class ComboExecutionState
{
	public string ComboId = "";
	public int CurrentStep = 0;
	public bool IsExecuting = false;
	public float StartTime = 0.0f;
	public int EffectsApplied = 0;
}
