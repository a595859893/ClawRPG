# 开发需求池

自我提升需要重点关注的方向（由开发循环更新）

## 2026-03-10
- **商店UI集成**: ✅ 已完成！添加 ShopUI 初始化到 GameManager，添加 O 键打开商店快捷键，更新 ControlsLabel 显示商店快捷键
- **任务UI系统**: ✅ 已完成！QuestUI.tscn、QuestManager.cs、QuestUI.cs 全部实现，Q键打开任务面板，支持接受/放弃任务
- **保存系统**: 已完成，实现了3个存档槽位，支持快速保存(Ctrl+F5)和快速加载(Ctrl+F9)
- **UI实时更新**: 已完成：添加PlayerUI.cs实时刷新玩家状态，添加HP/MP/经验值进度条，修复快捷键bug
- **商店系统**: 已完成：实现Shop.cs、ShopDatabase.cs(4个商店)、ShopManager.cs(购买/卖出)、ShopUI.cs，集成到GameManager，支持金币存档
- **战斗系统改进**: 添加状态机模式 - PlayerStateMachine.cs（5个状态：Idle/Walk/Attack/Hurt/Die）、EnemyStateMachine.cs（5个状态：Idle/Chase/Attack/Hurt/Dead），代码更模块化易维护
- **技能学习系统**: 已完成：添加SkillLearnUI.cs和SkillLearnUI.tscn，K键打开技能学习界面，显示所有可用技能，支持点击学习，与存档系统集成
- **UI优化**: ✅ 已完成！添加暂停菜单(ESC键)，修复GameManager输入处理使用_Input()代替_Process()

## 2026-03-10 新方向（待开发）
- **装备系统**: ✅ 已完成！创建EquipmentUI.cs，E键打开装备面板，显示当前武器/防具，支持卸下装备，实时显示属性加成
- **敌人AI**: ✅ 已完成！添加EnemyAI.cs（巡逻/逃跑/多行为类型）、扩展EnemyStateMachine.cs（新增Flee/Retreat状态）、更新Enemy.cs集成AI控制器
- **音效系统**: ✅ 已完成！创建AudioManager.cs，背景音乐播放/淡入淡出、音效播放器池、战斗音效（攻击/受伤/升级/死亡等）、环境音乐切换，集成到GameManager和Player/Enemy
- **地图系统**: ✅ 已完成！区域切换，大地图(M键)，小地图导航
- **UI优化**: ✅ 已完成！添加暂停菜单(ESC键)，修复GameManager输入处理使用_Input()代替_Process()
- **按键绑定修复**: ✅ 已完成！修复I键背包和Q键任务绑定，添加H键帮助面板
- **通知系统**: ✅ 已完成！创建 NotificationUI.cs，顶部通知消息队列系统，支持淡入淡出动画，地图切换时自动显示通知
- **WorldMapUI 修复**: ✅ 已完成！修复 CreateMapNode 方法中的代码问题
- **polish_ui - 升级特效**: ✅ 已完成！创建 LevelUpEffect.cs，添加升级动画通知，显示属性提升数值（HP/MP/攻击/防御），带金色边框和呼吸效果
- **polish_ui - 屏幕震动**: ✅ 已完成！创建 ScreenShake.cs，支持5种预设震动强度（Light/Medium/Heavy/Extreme/Quake），攻击/暴击/升级时触发
- **polish_ui - 伤害数字**: ✅ 已完成！创建 DamagePopup.cs，显示伤害数字/治疗/经验值/金币，带渐出动画
- **polish_ui - 暴击特效**: ✅ 已完成！创建 CriticalEffect.cs，暴击时显示 "💥 CRITICAL! 💥" 特效，带缩放和闪烁动画
- **Enemy 暴击系统**: ✅ 已完成！Enemy.cs 添加暴击率和暴击伤害倍率，暴击时触发特效和屏幕震动
- **polish_ui - 连击系统**: ✅ 已完成！创建 ComboSystem.cs，追踪连续命中并显示连击数，支持多重连击（5/10/20+），带动画效果
- **polish_ui - 受伤闪烁**: ✅ 已完成！创建 HitFlashEffect.cs，敌人受伤时闪烁，支持普通/暴击/魔法/中毒不同颜色闪烁
- **polish_ui - 攻击拖尾**: ✅ 已完成！创建 AttackTrailEffect.cs，攻击时产生视觉拖尾效果
- **polish_ui - 粒子效果**: ✅ 已完成！创建 EffectParticle.cs，支持击中火花/暴击火花/升级粒子效果
- **Player 暴击系统**: ✅ 已完成！Player.cs 添加暴击率和暴击伤害倍率，攻击拖尾效果，集成粒子系统
- **设置系统**: ✅ 已完成！创建SettingsData.cs(设置数据结构)、SettingsManager.cs(单例管理器)、SettingsUI.cs(设置界面)，支持音量/画面/游戏设置，保存到user://settings.json
- **宠物系统**: ✅ 已完成！创建Pet.cs(数据类)、PetDatabase.cs(10种宠物)、PetManager.cs(管理器)、PetUI.cs(P键打开)，支持宠物解锁/召唤/升级/属性加成，完全集成存档系统