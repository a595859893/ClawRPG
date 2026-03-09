# 自我提升日志

**日期**: 2026-03-10
**时间**: 03:25
**任务**: improve_code_quality - 思考如何提升 C# 代码质量

---

## 代码分析总结

### 当前架构
- **Player.cs**: 玩家角色，控制移动、攻击、技能、升级
- **Skill.cs**: 技能定义 (Resource)
- **SkillManager.cs**: 技能系统核心，单例模式
- **Enemy.cs**: 敌人基类

---

## 发现的代码质量问题

### 1. 魔数 (Magic Numbers) 问题
```csharp
// Player.cs 中
if (distance < 50)  // 50 是什么？
_attackCooldown = 0.5f;  // 冷却时间硬编码
```

**建议**: 使用常量或导出属性
```csharp
[Export] public float AttackRange { get; private set; } = 50f;
[Export] public float AttackCooldown { get; private set; } = 0.5f;
```

### 2. 单例滥用风险
```csharp
SkillManager.Instance?.UseSkill(...)  // 可能为 null
```
**建议**: 依赖注入或使用 `RequireComponent` 确保初始化

### 3. 单一职责原则 (SRP) 违反
`Player.cs` 负责:
- 输入处理
- 移动物理
- 动画状态
- 技能使用
- 升级逻辑
- 装备加成

**建议**: 拆分
- `PlayerController` - 输入/移动
- `PlayerCombat` - 战斗/技能
- `PlayerStats` - 属性/升级
- `PlayerEquipment` - 装备系统

### 4. 空引用风险
```csharp
var player = user as Player;
if (player != null) { ... }
```
**建议**: 使用 pattern matching 或 Null Object Pattern

### 5. Skill 系统的问题
- `Skill` 继承 `Resource` 但用 `Clone()` 手动复制
- 没有使用 Godot 的信号系统通知技能冷却变化

### 6. 枚举使用可以改进
```csharp
// 当前
public enum SkillType { Attack, Heal, Buff, Debuff }

// 建议: 按位运算支持多类型
[Flags]
public enum SkillType 
{ 
    None = 0, 
    Attack = 1 << 0, 
    Heal = 1 << 1, 
    Buff = 1 << 2,
    Debuff = 1 << 3 
}
```

---

## 改进建议优先级

### P0 - 高优先级
1. **提取常量**: 消除魔数
2. **空安全**: 使用 null 条件运算符和 pattern matching
3. **单一职责**: 拆分 Player 类

### P1 - 中优先级
1. **事件系统**: 用 Godot 信号替代直接调用
2. **配置分离**: 技能数据外部化 (JSON/CSV)
3. **接口抽象**: 定义 `ICharacter`, `ISkillUser` 接口

### P2 - 低优先级
1. **Record 类型**: 用于不可变数据 (如 Skill 快照)
2. **Source Generators**: 减少样板代码
3. **单元测试**: 添加测试框架

---

## 实际改进示例

### 示例 1: 提取 Player 常量
```csharp
public partial class Player : CharacterBody2D
{
    // 导出的配置常量
    [ExportGroup("Combat Settings")]
    [Export] public float AttackRange { get; private set; } = 50f;
    [Export] public float AttackCooldown { get; private set; } = 0.5f;
    
    [ExportGroup("Level Up Settings")]
    [Export] public int HpPerLevel { get; private set; } = 10;
    [Export] public int MpPerLevel { get; private set; } = 5;
    [Export] public int AttackPerLevel { get; private set; } = 2;
    
    // 使用时
    private void HandleAttackCooldown(double delta)
    {
        if (_attackCooldown > 0)
            _attackCooldown -= (float)delta;
        
        if (Input.IsActionPressed("attack") && _attackCooldown <= 0 && !_isAttacking)
            PerformAttack();
    }
}
```

### 示例 2: Pattern Matching 改进
```csharp
// Before
var player = user as Player;
if (player != null && player.CurrentMp >= skill.MpCost)

// After (C# 9+)
if (user is Player player && player.CurrentMp >= skill.MpCost)
```

### 示例 3: 使用信号系统
```csharp
// Skill.cs
public partial class Skill : Resource
{
    [Signal] public delegate void CooldownChangedEventHandler(float remaining);
    [Signal] public delegate void SkillUsedEventHandler(string skillName);
}

// SkillManager.cs - 通知 UI 更新
skill.ResetCooldown();
skill.EmitSignal(SignalName.SkillUsed, skill.SkillName);
```

---

## 下一步行动

1. **立即可做**: 创建 `Constants.cs` 或使用 `[Export]` 属性提取魔数
2. **短期**: 拆分 Player.cs 为多个专注类
3. **中期**: 引入事件/信号系统解耦
4. **长期**: 考虑使用 Godot 4.6 的 source generators 减少样板

---

## 反思

当前 ClawRPG 代码处于原型阶段，核心功能可用。代码质量改进应该循序渐进：
- 优先解决影响游戏逻辑的 bug
- 逐步重构而非一次性重写
- 保持向后兼容

> "Premature optimization is the root of all evil" - Donald Knuth
> "Premature abstraction is equally dangerous" - 经验之谈

---

*本次自我提升用时: ~20分钟*
