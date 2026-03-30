using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Quests;
using ClawRPG.Scripts.Characters;
using ClawRPG.Scripts.Systems;

/// <summary>
/// 2D integer vector struct (Godot 4.2 compatibility - Vector2i introduced in Godot 4.3)
/// </summary>
public struct Vector2i {
    public int X { get; set; }
    public int Y { get; set; }
    public Vector2i(int x, int y) { X = x; Y = y; }
    public static implicit operator Vector2(Vector2i v) => new Vector2(v.X, v.Y);
    public static Vector2i operator -(Vector2i a, Vector2i b) => new Vector2i(a.X - b.X, a.Y - b.Y);
    public static Vector2i operator +(Vector2i a, Vector2i b) => new Vector2i(a.X + b.X, a.Y + b.Y);
    public static bool operator ==(Vector2i a, Vector2i b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vector2i a, Vector2i b) => !(a == b);
    public override bool Equals(object obj) => obj is Vector2i v && v.X == X && v.Y == Y;
    public override int GetHashCode() => HashCode.Combine(X, Y);
}

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 任务指引箭头 - 在屏幕上显示指向任务目标的箭头
    /// 应用数据驱动设计模式：从 insights.md 学习
    /// </summary>
    public partial class QuestGuideArrow : Control
    {
        // 箭头显示节点
        private TextureRect arrowSprite;
        private Label distanceLabel;
        private Label targetNameLabel;
        
        // 配置数据
        private float arrowDistance = 80f;        // 箭头距离屏幕边缘的距离
        private float rotationSmoothness = 5f;    // 旋转平滑度
        private float updateInterval = 0.1f;      // 更新间隔
        
        // 状态
        private float timer = 0f;
        private Node2D currentTarget;            // 当前目标节点
        private Vector2 targetWorldPosition;       // 目标世界坐标
        private bool hasTarget = false; 
        
        // 目标类型
        private enum TargetType
        {
            None,
            NPC,
            Position,
            Enemy,
            Item
        }
        private TargetType currentTargetType = TargetType.None;
        
        // 信号系统
        public static event Action OnTargetChanged;
        
        public override void _Ready()
        {
            SetupUI();
            Hide();
        }
        
        private void SetupUI()
        {
            // 箭头精灵
            arrowSprite = new TextureRect();
            arrowSprite.CustomMinimumSize = new Vector2(40, 40);
            arrowSprite.Position = new Vector2(-20, -20);
            AddChild(arrowSprite);
            
            // 加载默认箭头纹理
            LoadArrowTexture();
            
            // 距离标签
            distanceLabel = new Label();
            distanceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            distanceLabel.AddThemeColorOverride("font_color", Colors.White);
            distanceLabel.AddThemeFontSizeOverride("font_size", 14);
            distanceLabel.Position = new Vector2(-30, 35);
            AddChild(distanceLabel);
            
            // 目标名称标签
            targetNameLabel = new Label();
            targetNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            targetNameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
            targetNameLabel.AddThemeFontSizeOverride("font_size", 12);
            targetNameLabel.Position = new Vector2(-50, -45);
            targetNameLabel.CustomMinimumSize = new Vector2(100, 0);
            AddChild(targetNameLabel);
            
            // 设置锚点为居中
            SetAnchorsPreset(Control.LayoutPreset.Center);
        }
        
        private void LoadArrowTexture()
        {
            // 使用程序化纹理绘制箭头
            var arrowImage = new Image(40, 40, Image.Format.Rgba8);
            arrowImage.Fill(new Color(0, 0, 0, 0));
            
            // 绘制向上箭头
            DrawArrowShape(arrowImage);
            
            var arrowTexture = ImageTexture.CreateFromImage(arrowImage);
            arrowSprite.Texture = arrowTexture;
        }
        
        private void DrawArrowShape(Image img)
        {
            // 绘制向上的箭头三角形
            Vector2i top = new Vector2i(20, 2);
            Vector2i leftBottom = new Vector2i(2, 38);
            Vector2i rightBottom = new Vector2i(38, 38);
            
            DrawTriangle(img, top, leftBottom, rightBottom, new Color(1f, 0.84f, 0f, 0.9f));
        }
        
        private void DrawTriangle(Image img, Vector2i p1, Vector2i p2, Vector2i p3, Color color)
        {
            int minX = Mathf.Min(p1.X, Mathf.Min(p2.X, p3.X));
            int maxX = Mathf.Max(p1.X, Mathf.Max(p2.X, p3.X));
            int minY = Mathf.Min(p1.Y, Mathf.Min(p2.Y, p3.Y));
            int maxY = Mathf.Max(p1.Y, Mathf.Max(p2.Y, p3.Y));
            
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (IsPointInTriangle(new Vector2(x, y), p1, p2, p3))
                    {
                        img.SetPixel(x, y, color);
                    }
                }
            }
        }
        
        private bool IsPointInTriangle(Vector2 p, Vector2i p1, Vector2i p2, Vector2i p3)
        {
            Vector2 v0 = p2 - p1;
            Vector2 v1 = p3 - p1;
            Vector2 v2 = p - p1;
            
            float dot00 = v0.Dot(v0);
            float dot01 = v0.Dot(v1);
            float dot02 = v0.Dot(v2);
            float dot11 = v1.Dot(v1);
            float dot12 = v1.Dot(v2);
            
            float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }
        
        public override void _Process(double delta)
        {
            timer += delta;
            if (timer >= updateInterval)
            {
                timer = 0;
                UpdateTarget();
            }
            
            if (hasTarget)
            {
                UpdateArrowPosition(delta);
                UpdateArrowRotation(delta);
                UpdateDistanceLabel();
            }
        }
        
        private void UpdateTarget()
        {
            // 获取当前任务系统
            var questSystem = QuestSystem.Instance;
            if (questSystem == null)
            {
                ClearTarget();
                return;
            }
            
            // 获取主线任务
            var activeQuest = questSystem.GetCurrentMainQuest();
            if (activeQuest == null)
            {
                // 没有主线任务，尝试获取支线任务
                var sideQuests = questSystem.GetActiveQuests();
                if (sideQuests.Count > 0)
                {
                    activeQuest = sideQuests[0];
                }
                else
                {
                    ClearTarget();
                    return;
                }
            }
            
            // 获取任务目标信息
            var targetPos = Vector2.Zero;
            var targetName = "";
            
            // 从任务目标中获取信息
            if (activeQuest.Objectives != null && activeQuest.Objectives.Count > 0)
            {
                var objective = activeQuest.Objectives[0];
                targetName = objective.Description;
                
                switch (objective.Type)
                {
                    case QuestObjective.ObjectiveType.Talk:
                        // 查找 NPC
                        currentTargetType = TargetType.NPC;
                        var npcNode = FindTargetNode(objective.TargetId, "npc");
                        if (npcNode != null)
                        {
                            targetPos = npcNode.GlobalPosition;
                        }
                        else
                        {
                            // 如果找不到 NPC，使用默认位置（需要根据实际地图设置）
                            targetPos = new Vector2(640, 360);
                        }
                        break;
                        
                    case QuestObjective.ObjectiveType.Kill:
                        // 查找敌人
                        currentTargetType = TargetType.Enemy;
                        var enemyNode = FindTargetNode(objective.TargetId, "enemy");
                        if (enemyNode != null)
                        {
                            targetPos = enemyNode.GlobalPosition;
                        }
                        else
                        {
                            targetPos = new Vector2(640, 360);
                        }
                        break;
                        
                    case QuestObjective.ObjectiveType.Collect:
                        // 查找物品
                        currentTargetType = TargetType.Item;
                        var itemNode = FindTargetNode(objective.TargetId, "item");
                        if (itemNode != null)
                        {
                            targetPos = itemNode.GlobalPosition;
                        }
                        else
                        {
                            targetPos = new Vector2(640, 360);
                        }
                        break;
                        
                    case QuestObjective.ObjectiveType.Reach:
                        // 到达位置 - 使用默认位置
                        currentTargetType = TargetType.Position;
                        targetPos = new Vector2(640, 360);
                        break;
                        
                    default:
                        ClearTarget();
                        return;
                }
            }
            else
            {
                ClearTarget();
                return;
            }
            
            // 检查目标是否改变
            if (!hasTarget || targetPos != targetWorldPosition)
            {
                targetWorldPosition = targetPos;
                targetNameLabel.Text = targetName;
                hasTarget = true;
                Show();
                OnTargetChanged?.Invoke();
            }
        }
        
        private Node2D FindTargetNode(string targetId, string nodeType)
        {
            var main = GetTree().CurrentScene;
            if (main == null) return null;
            
            // 根据类型查找对应节点
            switch (nodeType)
            {
                case "npc":
                    var npcs = main.GetTree().GetNodesInGroup("npcs");
                    foreach (Node node in npcs)
                    {
                        if (node.Name == targetId || node.Name.Contains(targetId))
                        {
                            return node as Node2D;
                        }
                    }
                    break;
                    
                case "enemy":
                    var enemies = main.GetTree().GetNodesInGroup("enemies");
                    foreach (Node node in enemies)
                    {
                        if (node.Name == targetId || node.Name.Contains(targetId))
                        {
                            return node as Node2D;
                        }
                    }
                    break;
                    
                case "item":
                    var items = main.GetTree().GetNodesInGroup("items");
                    foreach (Node node in items)
                    {
                        if (node.Name == targetId || node.Name.Contains(targetId))
                        {
                            return node as Node2D;
                        }
                    }
                    break;
            }
            
            return null;
        }
        
        private void ClearTarget()
        {
            if (hasTarget)
            {
                hasTarget = false; 
                currentTarget = null;
                currentTargetType = TargetType.None;
                Hide();
                OnTargetChanged?.Invoke();
            }
        }
        
        private void UpdateArrowPosition(float delta)
        {
            var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (player == null) return;
            
            var playerPos = player.GlobalPosition;
            var direction = (targetWorldPosition - playerPos).Normalized();
            
            var viewportSize = GetViewportRect().Size;
            var center = viewportSize / 2;
            
            var screenPos = center + direction * arrowDistance;
            
            float margin = 50f;
            screenPos.X = Mathf.Clamp(screenPos.X, margin, viewportSize.X - margin);
            screenPos.Y = Mathf.Clamp(screenPos.Y, margin, viewportSize.Y - margin);
            
            Position = Position.Lerp(screenPos, rotationSmoothness * delta);
        }
        
        private void UpdateArrowRotation(float delta)
        {
            if (!hasTarget) return;
            
            var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (player == null) return;
            
            var playerPos = player.GlobalPosition;
            var direction = (targetWorldPosition - playerPos).Normalized();
            float targetAngle = direction.Angle();
            
            float targetDegrees = Mathf.RadToDeg(targetAngle) + 90f;
            
            float currentDegrees = Mathf.RadToDeg(arrowSprite.Rotation);
            float newDegrees = Mathf.LerpAngle(currentDegrees, Mathf.DegToRad(targetDegrees), rotationSmoothness * delta);
            arrowSprite.Rotation = newDegrees;
            
            // 根据目标类型改变颜色
            switch (currentTargetType)
            {
                case TargetType.NPC:
                    arrowSprite.Modulate = new Color(1f, 0.84f, 0f, 1f); // 金色
                    break;
                case TargetType.Position:
                    arrowSprite.Modulate = new Color(0.3f, 0.7f, 1f, 1f); // 蓝色
                    break;
                case TargetType.Enemy:
                    arrowSprite.Modulate = new Color(1f, 0.3f, 0.3f, 1f); // 红色
                    break;
                case TargetType.Item:
                    arrowSprite.Modulate = new Color(0.5f, 1f, 0.5f, 1f); // 绿色
                    break;
            }
        }
        
        private void UpdateDistanceLabel()
        {
            if (!hasTarget) return;
            
            var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (player == null) return;
            
            float distance = player.GlobalPosition.DistanceTo(targetWorldPosition);
            
            if (distance >= 1000f)
            {
                distanceLabel.Text = $"{distance / 1000f:F1}km";
            }
            else if (distance >= 100f)
            {
                distanceLabel.Text = $"{distance / 100f:F1}00m";
            }
            else
            {
                distanceLabel.Text = $"{distance:F0}m";
            }
        }
        
        /// <summary>
        /// 手动设置目标位置（用于某些特殊任务）
        /// </summary>
        public void SetManualTarget(Vector2 worldPosition, string name, TargetType type)
        {
            targetWorldPosition = worldPosition;
            targetNameLabel.Text = name;
            currentTargetType = type;
            hasTarget = true;
            Show();
        }
        
        /// <summary>
        /// 清除手动目标
        /// </summary>
        public void ClearManualTarget()
        {
            ClearTarget();
        }
        
        /// <summary>
        /// 显示/隐藏指引箭头
        /// </summary>
        public void Toggle()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
        
        public override void _ExitTree()
        {
            OnTargetChanged = null;
        }
    }
}
