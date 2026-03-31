# Namespace 规范文档 (REQ-170-04)

> 建立日期: 2026-03-31
> 关联 REQ: REQ-170

## 现状

ClawRPG 代码库存在**四套并存的 namespace 前缀**：

| 前缀 | 使用场景 | 文件数量 |
|------|---------|---------|
| `ClawRPG.Scripts.*` | 主体代码 (默认) | 最多 |
| `ClawRPG.Scripts.Systems.*` | Systems 子系统 | 大量 |
| `ClawRPG.Core.*` | Framework 核心子集 | 少量 |
| `ClawRPG.Database` | 数据库类 | 少量 |
| `ClawRPG.Framework` | 框架层 | 少量 |
| `ClawRPG.Modules.*` | 独立模块 | 少量 |
| `ClawRPG.Data.*` | 数据模型 | 少量 |

## 规范

### 顶级划分

```
ClawRPG
├── Scripts/          # 游戏逻辑主体（默认）
├── Core/             # Framework 核心子集（Framework 层系统）
├── Database/         # 数据库访问层
├── Framework/        # 框架层（BaseSystem 等）
├── Modules/          # 独立功能模块
└── Data/             # 纯数据模型（不含逻辑）
```

### Scripts 子层规范

```
Scripts/
├── AI/               # AI 行为相关
├── Bosses/           # Boss 定义
├── Characters/       # 角色定义
├── Combat/           # 战斗相关
├── Crafting/         # 制作系统
├── Data/             # 数据结构（不含逻辑）
│   └── Enemy/
├── Database/         # 数据库访问
│   └── Loaders/
├── Fishing/          # 钓鱼系统
├── Framework/        # 框架层
├── Items/            # 物品相关
├── Leaderboard/      # 排行榜
├── Managers/         # 管理器
├── Mounts/           # 坐骑
├── Quests/           # 任务
├── Skills/           # 技能
│   └── Optimizer/
├── Systems/          # 游戏系统（推荐子文件夹）
│   ├── Achievement/
│   ├── Combat/
│   ├── PetMimicry/
│   └── ...
└── UI/               # UI 层
```

### Systems 子文件夹命名

每个系统使用**复数 PascalCase** 或 **System 结尾**：
- ✅ `ClawRPG.Scripts.Systems.PetMimicry`
- ✅ `ClawRPG.Scripts.Systems.Enchantment`
- ❌ `ClawRPG.Scripts.Systems.EnchantmentSystem`（避免 System 后缀重复）

### 文件夹 vs Namespace

**原则：namespace 应与文件夹路径一致。**

```
Scripts/Systems/PetMimicry/PetMimicryData.cs
→ namespace ClawRPG.Scripts.Systems.PetMimicry

Scripts/Database/ChoiceEventDatabase.cs
→ namespace ClawRPG.Scripts.Database
```

### 已知的无效 Namespace 模式

以下模式导致编译错误，**禁止使用**：

```csharp
// ❌ 错误: Scripts.Systems.Emote 应为 Systems.Emote
using ClawRPG.Scripts.Systems.Emote;  // 错误
using ClawRPG.Systems.Emote;          // 正确

// ❌ 错误: extension method 应为 static
private static SimEnemyState Clone(this SimEnemyState e)  // 错误
private static SimEnemyState Clone(SimEnemyState e)        // 正确
```

### Partial Class Namespace 一致性

所有 partial class 文件**必须使用相同 namespace**：

```csharp
// PetCombatCompanionUI.cs
namespace ClawRPG.Scripts.UI.Combat;

// PetCombatCompanionUI.Tabs.cs
namespace ClawRPG.Scripts.UI.Combat;  // 必须一致！
```

## 常见错误修复

| 错误模式 | 修复方法 |
|---------|---------|
| `The type or namespace name 'X' could not be found` | 检查 namespace 是否与文件夹路径匹配 |
| `CS0246: missing using directive` | 添加正确的 `using ClawRPG.*;` |
| `CS0103: Extension method must be defined in a non-generic static class` | 将 `Clone(this X)` 改为 `Clone(X)` |
| `partial class... different namespace` | 统一所有 partial 文件的 namespace |

## 工具

### 验证 Namespace 与文件夹一致性

```bash
# 检查 namespace 与文件路径不匹配的文件
cd /project/ClawRPG
for f in $(find Scripts -name "*.cs"); do
  ns=$(grep "^namespace " "$f" | head -1 | sed 's/namespace //' | tr -d ' ')
  # 从 namespace 推断期望路径
  expected=$(echo "$ns" | sed 's/\./\\//g' | sed 's/ClawRPG\\//')
  dir=$(dirname "$f" | sed 's|Scripts/|Scripts/|')
  if [ "$dir" != "$expected" ]; then
    echo "$f: ns=$ns expected=$expected"
  fi
done
```

## 变更历史

| 日期 | 变更 |
|------|------|
| 2026-03-31 | 初版创建 (REQ-170-04) |
