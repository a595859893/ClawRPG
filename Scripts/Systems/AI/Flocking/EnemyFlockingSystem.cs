using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.AI.Flocking
{
    /// <summary>
    /// Enemy Flocking System - Boids 算法实现
    /// 应用 Advanced Game AI Patterns 学习成果
    /// 实现分离(Seperation)、对齐(Alignment)、凝聚(Cohesion)三种行为
    /// </summary>
    public partial class EnemyFlockingSystem : BaseSystem
    {
        // 单例
        private static EnemyFlockingSystem _instance;
        public static EnemyFlockingSystem Instance => _instance;

        // 群体参数
        [Export] private float _separationWeight = 1.5f;
        [Export] private float _alignmentWeight = 1.0f;
        [Export] private float _cohesionWeight = 1.0f;
        [Export] private float _perceptionRadius = 100.0f;
        [Export] private float _maxSpeed = 50.0f;
        [Export] private float _maxForce = 10.0f;
        
        // 群体管理
        private Dictionary<int, FlockMember> _flocks = new Dictionary<int, FlockMember>();
        private int _nextFlockId = 0;
        
        // 统计
        private int _totalFlockMembers = 0;
        private int _flockUpdates = 0;
        
        public override void _Ready()
        {
            _instance = this;
        }
        
        /// <summary>
        /// 添加一个成员到群体系统
        /// </summary>
        public int AddFlockMember(Node2D member, string enemyType = "Default")
        {
            int id = _nextFlockId++;
            _flocks[id] = new FlockMember
            {
                Id = id,
                Member = member,
                EnemyType = enemyType,
                Velocity = Vector2.Zero,
                IsActive = true,
                JoinedTime = Time.GetTicksMsec()
            };
            _totalFlockMembers++;
            return id;
        }
        
        /// <summary>
        /// 移除成员
        /// </summary>
        public void RemoveFlockMember(int id)
        {
            if (_flocks.ContainsKey(id))
            {
                _flocks.Remove(id);
                _totalFlockMembers--;
            }
        }
        
        /// <summary>
        /// 更新群体行为
        /// </summary>
        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            _flockUpdates++;
            
            var activeFlocks = _flocks.Values.Where(f => f.IsActive && IsInstanceValid(f.Member)).ToList();
            
            foreach (var flock in activeFlocks)
            {
                if (!IsInstanceValid(flock.Member)) continue;
                
                // 计算三种行为力
                Vector2 separation = CalculateSeparation(flock, activeFlocks) * _separationWeight;
                Vector2 alignment = CalculateAlignment(flock, activeFlocks) * _alignmentWeight;
                Vector2 cohesion = CalculateCohesion(flock, activeFlocks) * _cohesionWeight;
                
                // 应用力量
                Vector2 acceleration = separation + alignment + cohesion;
                flock.Velocity += acceleration * dt;
                
                // 限制速度
                if (flock.Velocity.Length() > _maxSpeed)
                {
                    flock.Velocity = flock.Velocity.Normalized() * _maxSpeed;
                }
                
                // 更新位置
                flock.Member.Position += flock.Velocity * dt;
            }
        }
        
        /// <summary>
        /// 分离行为 - 避免与其他成员碰撞
        /// </summary>
        private Vector2 CalculateSeparation(FlockMember member, List<FlockMember> allFlocks)
        {
            Vector2 steering = Vector2.Zero;
            int count = 0;
            
            foreach (var other in allFlocks)
            {
                if (other.Id == member.Id) continue;
                
                float dist = member.Member.Position.DistanceTo(other.Member.Position);
                if (dist < _perceptionRadius && dist > 0)
                {
                    Vector2 diff = member.Member.Position - other.Member.Position;
                    diff = diff.Normalized() / dist; // 越近权重越大
                    steering += diff;
                    count++;
                }
            }
            
            if (count > 0)
            {
                steering = steering / count;
                steering = steering.Normalized() * _maxSpeed - member.Velocity;
                steering = steering.LimitLength(_maxForce);
            }
            
            return steering;
        }
        
        /// <summary>
        /// 对齐行为 - 与邻居方向一致
        /// </summary>
        private Vector2 CalculateAlignment(FlockMember member, List<FlockMember> allFlocks)
        {
            Vector2 avgVelocity = Vector2.Zero;
            int count = 0;
            
            foreach (var other in allFlocks)
            {
                if (other.Id == member.Id) continue;
                
                float dist = member.Member.Position.DistanceTo(other.Member.Position);
                if (dist < _perceptionRadius)
                {
                    avgVelocity += other.Velocity;
                    count++;
                }
            }
            
            if (count > 0)
            {
                avgVelocity = avgVelocity / count;
                avgVelocity = avgVelocity.Normalized() * _maxSpeed;
                Vector2 steering = avgVelocity - member.Velocity;
                steering = steering.LimitLength(_maxForce);
                return steering;
            }
            
            return Vector2.Zero;
        }
        
        /// <summary>
        /// 凝聚行为 - 向邻居中心移动
        /// </summary>
        private Vector2 CalculateCohesion(FlockMember member, List<FlockMember> allFlocks)
        {
            Vector2 centerOfMass = Vector2.Zero;
            int count = 0;
            
            foreach (var other in allFlocks)
            {
                if (other.Id == member.Id) continue;
                
                float dist = member.Member.Position.DistanceTo(other.Member.Position);
                if (dist < _perceptionRadius)
                {
                    centerOfMass += other.Member.Position;
                    count++;
                }
            }
            
            if (count > 0)
            {
                centerOfMass = centerOfMass / count;
                return Seek(member, centerOfMass);
            }
            
            return Vector2.Zero;
        }
        
        /// <summary>
        /// 寻求目标位置
        /// </summary>
        private Vector2 Seek(FlockMember member, Vector2 target)
        {
            Vector2 desired = target - member.Member.Position;
            desired = desired.Normalized() * _maxSpeed;
            Vector2 steering = desired - member.Velocity;
            steering = steering.LimitLength(_maxForce);
            return steering;
        }
        
        /// <summary>
        /// 设置群体参数
        /// </summary>
        public void SetFlockingParameters(float separation, float alignment, float cohesion)
        {
            _separationWeight = separation;
            _alignmentWeight = alignment;
            _cohesionWeight = cohesion;
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "total_flock_members", _totalFlockMembers },
                { "active_flocks", _flocks.Values.Count(f => f.IsActive) },
                { "flock_updates", _flockUpdates },
                { "perception_radius", _perceptionRadius },
                { "max_speed", _maxSpeed },
                { "separation_weight", _separationWeight },
                { "alignment_weight", _alignmentWeight },
                { "cohesion_weight", _cohesionWeight }
            };
        }
        
        /// <summary>
        /// 群体成员类
        /// </summary>
        private class FlockMember
        {
            public int Id { get; set; }
            public Node2D Member { get; set; }
            public string EnemyType { get; set; }
            public Vector2 Velocity { get; set; }
            public bool IsActive { get; set; }
            public long JoinedTime { get; set; }
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>()
            {
                ["separation_weight"] = _separationWeight,
                ["alignment_weight"] = _alignmentWeight,
                ["cohesion_weight"] = _cohesionWeight,
                ["perception_radius"] = _perceptionRadius,
                ["max_speed"] = _maxSpeed,
                ["max_force"] = _maxForce,
                ["next_flock_id"] = _nextFlockId
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("separation_weight")) _separationWeight = Convert.ToSingle(data["separation_weight"]);
            if (data.ContainsKey("alignment_weight")) _alignmentWeight = Convert.ToSingle(data["alignment_weight"]);
            if (data.ContainsKey("cohesion_weight")) _cohesionWeight = Convert.ToSingle(data["cohesion_weight"]);
            if (data.ContainsKey("perception_radius")) _perceptionRadius = Convert.ToSingle(data["perception_radius"]);
            if (data.ContainsKey("max_speed")) _maxSpeed = Convert.ToSingle(data["max_speed"]);
            if (data.ContainsKey("max_force")) _maxForce = Convert.ToSingle(data["max_force"]);
            if (data.ContainsKey("next_flock_id")) _nextFlockId = Convert.ToInt32(data["next_flock_id"]);
        }
    }
}
