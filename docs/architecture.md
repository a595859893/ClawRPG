# ClawRPG 架构设计

## 核心原则

```
┌─────────────────────────────────────────────────────────────┐
│                      GameCore (核心逻辑)                      │
│  - 数据模型 (Player, Enemy, Item, Quest...)                │
│  - 游戏规则 (战斗、升级、掉落...)                           │
│  - 状态管理 (GameState)                                    │
└─────────────────────────────────────────────────────────────┘
          ↓                              ↓
┌─────────────────────┐      ┌─────────────────────┐
│   Godot UI (玩家)   │      │  Text/CLI (LLM)    │
│  - 图形界面          │      │  - 纯文本交互       │
│  - 鼠标/键盘操作     │      │  - API 接口         │
│  - 音效/动画        │      │  - LLM 驱动        │
└─────────────────────┘      └─────────────────────┘
```

## 共用模块

| 模块 | 说明 | 位置 |
|------|------|------|
| GameCore | 核心逻辑 | Scripts/GameCore/ |
| GameState | 状态管理 | Scripts/GameCore/State/ |
| Models | 数据模型 | Scripts/GameCore/Models/ |
| Systems | 游戏系统 | Scripts/Systems/ |

## LLM 专用模块

| 模块 | 说明 |
|------|------|
| TextInterface | 文本界面 |
| APIServer | HTTP API |
| LLMBridge | LLM 交互桥接 |

## 设计验证流程

```
实现新系统 (Godot)
       ↓
提取核心逻辑到 GameCore
       ↓
LLM 版本使用相同 GameCore
       ↓
LLM 体验 = 玩家体验
       ↓
design_review 分析结果可靠
```
