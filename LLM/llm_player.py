#!/usr/bin/env python3
"""
ClawRPG LLM 交互器
让 LLM 通过命令行玩 RPG
"""

import subprocess
import json
import sys
from clawrpg import get_game_state, get_game_prompt, play

def call_llm(prompt: str, model: str = "minimax") -> str:
    """调用 LLM 生成回复"""
    # 这里可以替换为实际的 LLM API 调用
    # 示例使用 curl 调用本地 API
    cmd = [
        "curl", "-s", "-X", "POST",
        "http://localhost:11434/api/generate",
        "-d", json.dumps({
            "model": "llama3",
            "prompt": prompt,
            "stream": False
        })
    ]
    
    try:
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=30)
        data = json.loads(result.stdout)
        return data.get("response", "")
    except Exception as e:
        print(f"LLM 调用失败: {e}")
        return None

def interactive_loop():
    """交互循环"""
    print("=" * 50)
    print("ClawRPG - LLM 玩版本")
    print("=" * 50)
    print()
    
    # 第一次获取 prompt
    print(get_game_prompt())
    print()
    
    while True:
        # 获取 LLM 的决策
        prompt = get_game_prompt()
        
        print("\n[等待 LLM 决策...]")
        # 在这里 LLM 会分析状态并给出动作
        # 实际使用时替换为真正的 LLM 调用
        
        action = input("\n输入动作 (或 'quit' 退出): ")
        
        if action in ["quit", "exit", "退出"]:
            break
        
        if not action:
            continue
        
        # 执行动作
        result = play(action)
        print("\n" + result)
        print("\n" + "=" * 50)

def auto_play_loop(llm_api=None):
    """自动玩模式（LLM 驱动）"""
    print("=" * 50)
    print("ClawRPG - LLM 自动玩模式")
    print("=" * 50)
    
    turns = 0
    max_turns = 20
    
    while turns < max_turns:
        turns += 1
        print(f"\n=== 第 {turns} 回合 ===")
        
        # 获取当前状态
        prompt = get_game_prompt()
        
        # 调用 LLM
        if llm_api:
            action = llm_api(prompt)
        else:
            # 演示：随机动作
            import random
            actions = ["explore", "attack", "rest", "status"]
            action = random.choice(actions)
            print(f"[演示模式] 随机动作: {action}")
        
        # 执行
        result = play(action)
        print(f"\n执行: {action}")
        print(result)
        
        # 检查游戏结束
        state = get_game_state()
        if state["player"]["hp"] <= 0:
            print("\n游戏结束！")
            break
    
    print(f"\n游戏结束！共进行了 {turns} 回合")

if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "auto":
        auto_play_loop()
    else:
        interactive_loop()
