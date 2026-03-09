#!/bin/bash
# ClawRPG 每日汇报脚本 - 每天早上9点执行

cd /project

echo "========================================" >> /project/memory/daily-report.log
echo "📅 每日汇报 - $(date '+%Y-%m-%d %H:%M')" >> /project/memory/daily-report.log
echo "========================================" >> /project/memory/daily-report.log

# 1. 今天的开发进度
echo "" >> /project/memory/daily-report.log
echo "📊 今日开发进度:" >> /project/memory/daily-report.log
echo "---" >> /project/memory/daily-report.log
cd /project/ClawRPG && git log --since="00:00" --oneline 2>/dev/null | head -10 >> /project/memory/daily-report.log

# 2. 从社区获取的灵感
echo "" >> /project/memory/daily-report.log
echo "💡 社区灵感:" >> /project/memory/daily-report.log
echo "---" >> /project/memory/daily-report.log
# 检查Moltbook动态
echo "- Moltbook: 检查最新讨论" >> /project/memory/daily-report.log
# 检查EvoMap
echo "- EvoMap: 检查最新技术趋势" >> /project/memory/daily-report.log

# 3. 接下来的计划
echo "" >> /project/memory/daily-report.log
echo "🎯 接下来的计划:" >> /project/memory/daily-report.log
echo "---" >> /project/memory/daily-report.log
echo "- 继续完善游戏功能" >> /project/memory/daily-report.log
echo "- 根据社区反馈优化开发方向" >> /project/memory/daily-report.log

echo "" >> /project/memory/daily-report.log
echo "========================================" >> /project/memory/daily-report.log
echo "汇报生成完成" >> /project/memory/daily-report.log
