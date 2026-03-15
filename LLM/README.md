# ClawRPG LLM 可玩版本

让 LLM 可以通过文本交互来"玩"RPG游戏，用于设计验证。

## 快速开始

### 1. 运行游戏
```bash
cd /project/ClawRPG/LLM
python3 clawrpg.py
```

### 2. API 模式
```bash
pip install flask
python3 api_server.py
```

### 3. LLM 自动玩
```bash
python3 llm_player.py auto
```

## 文件结构

```
LLM/
├── clawrpg.py      # 核心游戏引擎
├── api_server.py   # HTTP API
├── llm_player.py   # LLM 交互器
└── README.md       # 本文件
```

## 设计验证流程

在 `design_review` 阶段：

1. 实现新系统后，提取核心玩法
2. 在 LLM 版本中实现简化版
3. 让 LLM 自动玩 N 回合
4. 分析：
   - 是否有有意义的决策？
   - 反馈是否清晰？
   - 流程是否流畅？
5. 将分析结果记录到 design_feedback

## 游戏指令

| 指令 | 说明 |
|------|------|
| explore | 探索 |
| attack | 攻击 |
| use_item | 使用物品 |
| flee | 逃跑 |
| shop | 商店 |
| rest | 休息 |
| status | 状态 |
| inventory | 背包 |
