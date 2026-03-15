#!/usr/bin/env python3
"""
ClawRPG API Server
让 LLM 通过 HTTP API 交互
"""

from flask import Flask, request, jsonify
from clawrpg import get_game_state, get_game_prompt, play

app = Flask(__name__)

@app.route('/api/state', methods=['GET'])
def state():
    """获取当前游戏状态"""
    return jsonify(get_game_state())

@app.route('/api/prompt', methods=['GET'])
def prompt():
    """获取 LLM 用的 prompt"""
    return jsonify({"prompt": get_game_prompt()})

@app.route('/api/action', methods=['POST'])
def action():
    """执行动作"""
    data = request.json
    action = data.get('action', '')
    result = play(action)
    return jsonify({
        "result": result,
        "state": get_game_state()
    })

@app.route('/api/reset', methods=['POST'])
def reset():
    """重置游戏"""
    global game
    from clawrpg import ClawRPG
    game = ClawRPG()
    return jsonify({"message": "游戏已重置"})

if __name__ == '__main__':
    print("ClawRPG API Server 启动中...")
    print("访问 http://localhost:8080/api/prompt 开始游戏")
    app.run(host='0.0.0.0', port=8080)
