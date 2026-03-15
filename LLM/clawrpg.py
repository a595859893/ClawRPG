#!/usr/bin/env python3
"""
ClawRPG - LLM 可玩版本 (简化版)

本版本是 Godot 版本的简化映射，用于：
1. LLM 可以"玩"游戏进行测试
2. design_review 阶段验证游戏设计
3. 确保 LLM 体验 ≈ 玩家体验

核心逻辑与 Godot 版本保持一致：
- 战斗系统 → Scripts/Systems/Combat/
- 物品系统 → Scripts/Systems/Items/
- 任务系统 → Scripts/Systems/Quests/
- 升级系统 → 经验/等级机制

注意：本版本是简化版，完整功能请参考 Godot 版本
"""

import json
import random
import time
from dataclasses import dataclass, asdict
from typing import List, Optional

@dataclass
class Player:
    name: str = "Hero"
    hp: int = 100
    max_hp: int = 100
    attack: int = 10
    defense: int = 5
    gold: int = 100
    level: int = 1
    exp: int = 0
    inventory: List[str] = None
    
    def __post_init__(self):
        if self.inventory is None:
            self.inventory = ["potion"]

@dataclass
class Enemy:
    name: str
    hp: int
    max_hp: int
    attack: int
    defense: int
    exp_reward: int
    gold_reward: int

class GameState:
    def __init__(self):
        self.player = Player()
        self.location = "town"
        self.in_combat = False
        self.current_enemy: Optional[Enemy] = None
        self.turn = 1
        self.message_log: List[str] = []
        self.available_actions = ["explore", "status", "inventory", "shop", "rest", "help"]
    
    def add_message(self, msg: str):
        self.message_log.append(f"[{self.turn}] {msg}")
        if len(self.message_log) > 20:
            self.message_log = self.message_log[-20:]
    
    def to_dict(self) -> dict:
        state = {
            "player": asdict(self.player),
            "location": self.location,
            "in_combat": self.in_combat,
            "turn": self.turn,
            "available_actions": self.available_actions,
            "recent_messages": self.message_log[-5:]
        }
        if self.in_combat and self.current_enemy:
            state["enemy"] = asdict(self.current_enemy)
        return state

# 敌人数据库
ENEMIES = [
    Enemy("Goblin", hp=30, max_hp=30, attack=5, defense=2, exp_reward=10, gold_reward=15),
    Enemy("Wolf", hp=40, max_hp=40, attack=8, defense=3, exp_reward=15, gold_reward=20),
    Enemy("Skeleton", hp=50, max_hp=50, attack=10, defense=5, exp_reward=25, gold_reward=30),
    Enemy("Orc", hp=80, max_hp=80, attack=15, defense=8, exp_reward=50, gold_reward=60),
    Enemy("Dragon", hp=200, max_hp=200, attack=25, defense=15, exp_reward=200, gold_reward=500),
]

LOCATIONS = {
    "town": {
        "description": "你身处繁华的小镇。周围是商店和冒险者。",
        "actions": ["explore", "shop", "rest", "status", "inventory"]
    },
    "forest": {
        "description": "茂密的森林，偶尔传来奇怪的声音。",
        "actions": ["explore", "attack", "flee", "status"]
    },
    "dungeon": {
        "description": "阴暗的地牢，空气中弥漫着危险的气息。",
        "actions": ["explore", "attack", "flee", "status"]
    }
}

class ClawRPG:
    def __init__(self):
        self.state = GameState()
        self.state.add_message("欢迎来到 ClawRPG！")
        self.state.add_message("你是一名冒险者，来到了这座小镇。")
        self.state.add_message("输入 'help' 查看可用动作。")
    
    def get_state(self) -> dict:
        """获取当前游戏状态"""
        return self.state.to_dict()
    
    def get_prompt(self) -> str:
        """获取 LLM 用的 prompt"""
        state = self.state
        prompt = f"""## 当前状态

### 位置
{state.location}

### 玩家
- HP: {state.player.hp}/{state.player.max_hp}
- 等级: {state.player.level}
- 经验: {state.player.exp}/100
- 金币: {state.player.gold}
- 背包: {', '.join(state.player.inventory)}

### 可用动作
{', '.join(state.available_actions)}

"""
        if state.in_combat and state.current_enemy:
            enemy = state.current_enemy
            prompt += f"""### 战斗！
敌人: {enemy.name}
敌人HP: {enemy.hp}/{enemy.max_hp}

战斗动作: attack, use_item, flee
"""
        else:
            prompt += f"""### 环境
{LOCATIONS[state.location]['description']}
"""
        
        prompt += f"""
### 最近消息
{chr(10).join(state.message_log[-3:])}

---

请描述你的行动。只输出动作，不要输出其他内容。"""
        return prompt
    
    def execute(self, action: str) -> str:
        """执行动作"""
        action = action.lower().strip()
        self.state.turn += 1
        
        if action in ["help", "帮助"]:
            return self._help()
        
        if action in ["status", "状态"]:
            return self._status()
        
        if action in ["inventory", "背包", "背包"]:
            return self._inventory()
        
        if action in ["explore", "探索"]:
            return self._explore()
        
        if action in ["shop", "商店"]:
            return self._shop()
        
        if action in ["rest", "休息"]:
            return self._rest()
        
        if self.state.in_combat:
            if action in ["attack", "攻击"]:
                return self._combat_attack()
            elif action in ["flee", "逃跑"]:
                return self._combat_flee()
            elif action.startswith("use "):
                return self._use_item(action[4:])
        
        return f"无效动作: {action}。输入 'help' 查看可用动作。"
    
    def _help(self) -> str:
        return """
## 可用动作

- **explore**: 探索当前区域
- **attack**: 攻击敌人
- **use_item <物品>**: 使用物品
- **flee**: 逃跑
- **shop**: 进入商店
- **rest**: 休息恢复
- **status**: 查看状态
- **inventory**: 查看背包
"""
    
    def _status(self) -> str:
        p = self.state.player
        return f"""
## 状态

- 名字: {p.name}
- HP: {p.hp}/{p.max_hp}
- 攻击: {p.attack}
- 防御: {p.defense}
- 等级: {p.level} (经验: {p.exp}/100)
- 金币: {p.gold}
- 位置: {self.state.location}
"""
    
    def _inventory(self) -> str:
        return f"背包: {', '.join(self.state.player.inventory)}"
    
    def _explore(self) -> str:
        if self.state.location == "town":
            dest = random.choice(["forest", "dungeon"])
            self.state.location = dest
            self.state.available_actions = LOCATIONS[dest]["actions"]
            self.state.add_message(f"你离开了小镇，来到了{dest}。")
            return f"你离开了小镇，来到了{random.choice(['森林', '地下城'])}。"
        
        # 探索遇敌
        if random.random() < 0.5:
            enemy = random.choice(ENEMIES[:3])
            self.state.in_combat = True
            self.state.current_enemy = enemy
            self.state.available_actions = ["attack", "use_item", "flee"]
            return f"突然，一个 {enemy.name} 出现了！\nHP: {enemy.hp}/{enemy.max_hp}\n攻击: {enemy.attack}"
        
        gold_found = random.randint(5, 20)
        self.state.player.gold += gold_found
        return f"你探索了一会儿，发现了 {gold_found} 金币！"
    
    def _shop(self) -> str:
        if self.state.location != "town":
            return "这里没有商店。"
        
        return """
## 商店

- potion (50g): 恢复50HP
- sword (100g): +5攻击
- armor (100g): +5防御

输入 'buy <物品>' 购买。
"""
    
    def _rest(self) -> str:
        if self.state.location != "town":
            return "这里不能休息。"
        
        self.state.player.hp = min(self.state.player.max_hp, self.state.player.hp + 30)
        return f"你休息了一会儿，HP恢复到 {self.state.player.hp}/{self.state.player.max_hp}"
    
    def _combat_attack(self) -> str:
        enemy = self.state.current_enemy
        player = self.state.player
        
        # 玩家攻击
        damage = max(1, player.attack - enemy.defense + random.randint(-2, 2))
        enemy.hp -= damage
        
        # 敌人反击
        enemy_damage = max(1, enemy.attack - player.defense + random.randint(-2, 2))
        player.hp -= enemy_damage
        
        result = f"你攻击了 {enemy.name}，造成 {damage} 伤害！\n"
        result += f"{enemy.name} 反击，造成 {enemy_damage} 伤害！\n"
        
        # 检查结果
        if enemy.hp <= 0:
            result += f"\n你击败了 {enemy.name}！\n"
            result += f"获得 {enemy.exp_reward} 经验，{enemy.gold_reward} 金币！\n"
            player.exp += enemy.exp_reward
            player.gold += enemy.gold_reward
            self.state.in_combat = False
            self.state.current_enemy = None
            self.state.available_actions = LOCATIONS[self.state.location]["actions"]
            
            # 升级
            if player.exp >= 100:
                player.level += 1
                player.exp -= 100
                player.max_hp += 10
                player.hp = player.max_hp
                player.attack += 2
                player.defense += 1
                result += f"\n恭喜升级！现在你是 {player.level} 级了！"
        elif player.hp <= 0:
            result += "\n你被击败了！"
            player.hp = 50
            self.state.location = "town"
            self.state.in_combat = False
            self.state.available_actions = LOCATIONS["town"]["actions"]
        
        self.state.add_message(result)
        return result
    
    def _combat_flee(self) -> str:
        if random.random() < 0.5:
            self.state.in_combat = False
            self.state.current_enemy = None
            self.state.available_actions = LOCATIONS[self.state.location]["actions"]
            self.state.add_message("你成功逃跑了！")
            return "逃跑成功！"
        else:
            damage = self.state.current_enemy.attack
            self.state.player.hp -= damage
            self.state.add_message(f"逃跑失败，受到 {damage} 伤害！")
            return f"逃跑失败！受到 {damage} 伤害！"
    
    def _use_item(self, item: str) -> str:
        if item not in self.state.player.inventory:
            return f"你没有 {item}。"
        
        if item == "potion":
            heal = 50
            self.state.player.hp = min(self.state.player.max_hp, self.state.player.hp + heal)
            return f"使用了药水，恢复 {heal} HP！"
        
        return f"现在不能使用 {item}。"


# 全局游戏实例
game = ClawRPG()

def get_game_state():
    return game.get_state()

def get_game_prompt():
    return game.get_prompt()

def play(action: str):
    result = game.execute(action)
    return result

if __name__ == "__main__":
    print("=" * 50)
    print("ClawRPG - LLM 可玩版本")
    print("=" * 50)
    print()
    
    while True:
        print(game.get_prompt())
        print()
        action = input("> ")
        if action in ["quit", "exit", "退出"]:
            break
        print(game.execute(action))
        print()
