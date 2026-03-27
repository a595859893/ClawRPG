# REQ-135: 合并 ComboSystem 和 SkillComboSystem

## 状态: completed

## 背景

`ComboSystem`（旧，硬编码 combos + static Actions）和 `SkillComboSystem`（新，数据驱动）同时存在，造成：
- 重复维护两套 API
- UI 组件不知道自己该用哪个
- `ComboSystem.OnSkillUsed()` 从未被调用（死代码）
- `SkillComboSystem.RecordSkillUse()` 也未被调用

## 解决方案

以 `SkillComboSystem` 为单一数据源，`ComboSystem` 保留但不注册（废弃）。

### 改动清单

**SkillComboData.cs:**
- `SkillCombo` 新增: `OldComboType`, `Rarity`, `Description`, `EffectName`, `RequiredComboLevel`, `CooldownReduction`, `ComboPointReward`
- `ActiveCombo` 新增: `CurrentStep`, `TimeRemaining`, `IsActive`, `TimesExecuted`（兼容旧 `ComboProgress`）
- `PlayerComboData` 新增: `ComboPoints`, `ComboLevel`, `ActiveProgress`
- `SkillCombo.ToComboData()`: 转换为旧 `ComboData` 格式

**SkillComboDatabase.cs:**
- 新增 `GetCombosByOldType(ComboData.ComboType)`: 按旧类型过滤
- 补充 12 个旧 `ComboSystem` hardcoded combos（迁移到 `SkillComboDatabase`）

**SkillComboSystem.cs:**
- 新增 static Actions: `ComboPointsChanged`, `ComboLevelChanged`, `ComboProgressUpdated`（兼容旧 UI）
- 新增 `GetAllCombos()`, `GetUnlockedCombos()`, `GetCombosByType()`, `GetPlayerProgress()`, `GetComboPoints()`, `GetComboLevel()`
- `CompleteCombo()` → `OnComboCompletedInternal()` 触发 static Actions

**ComboUI.cs:**
- `_comboSystem` 类型从 `ComboSystem` 改为 `SkillComboSystem`
- 使用 `SkillComboSystem.Instance` 替代 `GetNode`
- 所有 `ComboData` 引用改为 `SkillCombo`

**CombatStatsPanel.cs:**
- 订阅 `SkillComboSystem.ComboProgressUpdated` / `ComboLevelChanged`

**MainSaveLoad.cs / GameInitializationManager.cs / SaveSerializer.cs:**
- 存档/读档改用 `SkillComboSystem.Instance`

**SystemInitializationManager.cs:**
- 移除 `RegisterSystemInGroup("combat", typeof(ComboSystem))`

**ComboDisplayUI.cs:**
- 注释掉无效的 `OnComboChanged` 等事件订阅（这些事件从未实现）

## 遗留问题

- `SkillComboSystem.RecordSkillUse()` 仍需从技能系统调用才能触发 combo 检测（集成点待修复，属于另一 REQ）
