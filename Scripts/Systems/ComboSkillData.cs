# Combo Skill Data - 连击技能系统数据结构
## 连击技能数据结构定义

extends Resource
class_name ComboSkillData

# 连击类型
enum ComboType {
	Sequential,    # 顺序释放 - 按顺序触发
	Parallel,      # 并行释放 - 同时触发
	Chain,         # 链式触发 - 前一个触发后一个
	Conditional    # 条件触发 - 满足条件触发
}

# 触发条件
enum TriggerCondition {
	OnHit,           # 命中时
	OnCritical,      # 暴击时
	OnKill,          # 击杀时
	OnDamageTaken,   # 受到伤害时
	OnHealthBelow,   # 生命低于X%
	OnManaBelow,     # 法力低于X%
	OnEnemyType,     # 针对特定敌人类型
	OnComboComplete, # 连击完成时
	Manual,          # 手动触发
	Cooldown         # 冷却触发
}

# 效果类型
enum EffectType {
	Damage,              # 伤害
	Heal,                # 治疗
	Shield,              # 护盾
	Buff,                # 增益
	Debuff,              # 减益
	Teleport,            # 传送
	Summon,              # 召唤
	Transform,           # 变形
	ClearDebuffs,        # 清除减益
	GrantInvulnerability # 无敌
}

# 技能效果
class_name ComboSkillEffect extends Resource
var effect_type: EffectType
var value: float
var duration: float = 0.0  # 持续时间，0为瞬发
var description: String
var target: String = "enemy"  # enemy/self/ally

# 连击步骤
class_name ComboStep extends Resource
var skill_id: String
var delay: float = 0.0  # 延迟（秒）
var condition: TriggerCondition = TriggerCondition.Manual
var condition_value: float = 0.0  # 条件参数
var effects: Array[ComboSkillEffect] = []

# 连击配置
class_name ComboSkill extends Resource
var id: String
var name: String
var description: String
var combo_type: ComboType
var steps: Array[ComboStep] = []
var total_time: float = 0.0  # 总释放时间
var cooldown: float = 0.0
var mana_cost: float = 0.0
var level_required: int = 1
var rarity: int = 0  # 0=Common, 1=Uncommon, 2=Rare, 3=Epic, 4=Legendary

# 玩家已解锁的连击
class_name PlayerComboSkill extends Resource
var combo_id: String
var is_equipped: bool = false
var current_cooldown: float = 0.0
var use_count: int = 0

# 连击执行状态
class_name ComboExecutionState
var combo_id: String
var current_step: int = 0
var is_executing: bool = false
var start_time: float = 0.0
var effects_applied: int = 0
