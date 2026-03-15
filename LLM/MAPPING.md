# LLM 版本与 Godot 版本映射

## 核心对应关系

| Godot 系统 | LLM 版本 | 说明 |
|-------------|----------|------|
| Scripts/Systems/Combat/ | combat.py | 战斗系统 |
| Scripts/Systems/Items/ | items.py | 物品系统 |
| Scripts/Systems/Quests/ | quests.py | 任务系统 |
| Scripts/Systems/Character/ | character.py | 角色系统 |

## 维护规则

### 添加新系统时

1. **Godot 实现**: 在 Scripts/Systems/ 添加新系统
2. **LLM 映射**: 在 LLM/ 添加简化版
3. **design_review**: 验证两者一致性

### 验证一致性

```bash
# 检查 Godot 系统
ls /project/ClawRPG/Scripts/Systems/

# 检查 LLM 对应
ls /project/ClawRPG/LLM/
```

## 数据模型映射

### Player
```
Godot: Scripts/Player.cs
LLM:   clawrpg.py::Player
```

### Enemy
```
Godot: Scripts/Enemy.cs
LLM:   clawrpg.py::Enemy
```

### Item
```
Godot: Scripts/Items/
LLM:   clawrpg.py::items
```

## design_review 流程

1. 阅读 Godot 实现
2. 确认 LLM 版本有对应
3. 运行 LLM 版本测试
4. 分析一致性
5. 记录差异到 design_feedback
