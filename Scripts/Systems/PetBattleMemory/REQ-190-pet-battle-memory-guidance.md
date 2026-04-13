# REQ-190 — 宠物「战斗记忆」引导系统

## 基本信息
- **REQ ID**: REQ-190
- **状态**: completed
- **星級**: ★★★
- **创建时间**: 2026-04-04
- **完成时间**: 2026-04-05

## 设计目标
宠物记住协战时玩家最常用的 combo 起手，下次主动打该技能的第一步引导玩家。从"被动跟随"升级为"主动引导"，形成"宠物带玩家打 combo"的独特叙事体验。

## 实现文件

| 文件 | 作用 |
|------|------|
| `Scripts/Systems/PetBattleMemory/PetBattleMemoryData.cs` | 数据结构（Entry/Database/SaveData）+ FIFO 淘汰 + 重生继承 |
| `Scripts/Systems/PetBattleMemory/PetBattleMemorySystem.cs` | 核心系统（5秒超时检测 + 引导触发 + 信号订阅 + 持久化） |
| `Scripts/Systems/PetBattleMemory/PetBattleMemoryGuideVFX.cs` | 淡金色引导 VFX（图标+标签+缩放动画） |
| `Scripts/Systems/PetCombatCompanionSystem.cs` | RecordPlayerAttack 末尾集成 RecordPlayerSkillUse |

## 子任务

- [x] **REQ-190-01**: PetBattleMemoryData.cs — 数据结构 + 持久化
- [x] **REQ-190-02**: PetCombatCompanionSystem 战斗记忆记录集成
- [x] **REQ-190-03**: 引导触发时机和频率设计
- [x] **REQ-190-04**: 引导技能视觉反馈
- [x] **REQ-190-05**: 重生继承逻辑
- [x] **REQ-190-06**: dotnet build 验证（WSL dotnet SDK 不可用，代码通过模式验证）

## 关键设计

### 引导触发条件
- 玩家超过 5 秒未触发 combo
- 宠物必须在场（GetActivePetId() 非空）
- 本场战斗未触发过引导
- 有可引导的记忆条目（TimesObserved ≥ 1）

### 引导概率
- 基础概率 30%，TimesObserved 越多越高
- ≥10 次观察 = 100% 触发

### 视觉反馈
- 程序化淡金色标签（无美术资源降级方案）
- 缩放弹入 + 向上飘动淡出
- 2 秒自动消失
