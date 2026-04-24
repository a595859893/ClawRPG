using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetScout
{
    /// <summary>
    /// Pet Scout System - 宠物侦察系统
    /// 宠物感知玩家视角盲区敌人，通过发光/警告音提示
    /// 
    /// 功能：
    /// 1. 盲区检测：宠物维护一个扇形感知区域（与主人朝向互补的后方区域）
    /// 2. 敌意识别：当感知范围内出现敌人时，触发侦察事件
    /// 3. 提示方式：视觉（宠物发光特效+UI方向指示器）+ 听觉（警告音效）
    /// 4. 智能提示冷却：避免重复提示，同一敌人只在首次发现时提示
    /// 5. 模式切换：可关闭侦察模式（宠物只跟随，不提示）
    /// </summary>
    public partial class PetScoutSystem : BaseSystem
    {
        public static PetScoutSystem Instance { get; private set; }

        // 侦察数据
        private PetScoutData _scoutData = new PetScoutData();

        // 玩家引用
        private CharacterBody2D _player;

        // 宠物节点（用于显示特效）
        private Node2D _petNode;

        // 信号 (Godot 4 compatible)
        /// <summary>发现盲区敌人时触发 (enemyId, position, alertType)</summary>
        [Signal]
        public delegate void EnemyDetectedInBlindSpotDelegateEventHandlerEventHandler(string enemyId, Vector2 position, ScoutAlertType alertType);
        /// <summary>侦察模式切换时触发 (enabled)</summary>
        [Signal]
        public delegate void ScoutModeChangedDelegateEventHandlerEventHandler(bool enabled);
        /// <summary>宠物发光强度变化 (intensity 0-1)</summary>
        [Signal]
        public delegate void GlowIntensityChangedDelegateEventHandlerEventHandler(float intensity);
        /// <summary>触发警报音效时触发</summary>
        [Signal]
        public delegate void PlayedAlertSoundDelegateEventHandlerEventHandler();

        // 当前发光强度
        private float _currentGlowIntensity = 0f;
        private float _targetGlowIntensity = 0f;

        // 已检测但未警报的敌人（用于避免重复）
        private HashSet<string> _alertedEnemies = new HashSet<string>();

        // 敌人节点组名称
        private const string ENEMY_GROUP = "enemy";

        public override void _Ready()
        {
            Instance = this;
            base._Ready();
            InitializeScoutData();
        }

        public override void _Process(double delta)
        {
            if (!_scoutData.ScoutEnabled || _player == null)
                return;

            float deltaFloat = (float)delta;
            UpdateBlindSpotDetection(deltaFloat);
            UpdateGlowEffect(deltaFloat);
        }

        #region Initialization

        private void InitializeScoutData()
        {
            _scoutData.ScoutEnabled = true;
            _scoutData.BlindSpotAngle = 120f;
            _scoutData.PerceptionRadius = 300f;
            _scoutData.AlertCooldown = 3f;
            _scoutData.SoundEnabled = true;
        }

        /// <summary>
        /// 设置玩家引用
        /// </summary>
        public void SetPlayer(CharacterBody2D player)
        {
            _player = player;
        }

        /// <summary>
        /// 设置宠物节点引用（用于特效）
        /// </summary>
        public void SetPetNode(Node2D petNode)
        {
            _petNode = petNode;
        }

        #endregion

        #region Blind Spot Detection

        /// <summary>
        /// 更新盲区检测
        /// </summary>
        private void UpdateBlindSpotDetection(float delta)
        {
            if (_player == null) return;

            // 获取玩家朝向（基于速度方向）
            Vector2 playerFacing = GetPlayerFacingDirection();
            if (playerFacing == Vector2.Zero)
                return; // 玩家没有移动方向，使用上一次的朝向

            // 计算盲区中心方向（玩家朝向的反方向）
            Vector2 blindSpotCenter = -playerFacing;

            // 获取范围内的所有敌人
            var enemies = GetEnemiesInPerceptionRange();
            float currentTime = (float)Time.GetTicksMsec() / 1000f;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !IsInstanceValid(enemy))
                    continue;

                Vector2 enemyPos = enemy.GlobalPosition;
                Vector2 toEnemy = (enemyPos - _player.GlobalPosition);
                float distance = toEnemy.Length();

                // 检查是否在盲区内
                Vector2 toEnemyNorm = toEnemy.Normalized();
                float angleToEnemy = blindSpotCenter.AngleTo(toEnemyNorm);
                float angleDegrees = Mathf.RadToDeg(Mathf.Abs(angleToEnemy));

                if (angleDegrees <= _scoutData.BlindSpotAngle / 2f)
                {
                    // 在盲区内
                    string enemyId = enemy.Name;

                    // 检查是否在冷却中
                    if (_scoutData.LastAlertTime.TryGetValue(enemyId, out float lastAlert))
                    {
                        if (currentTime - lastAlert < _scoutData.AlertCooldown)
                            continue; // 还在冷却中
                    }

                    // 触发警报
                    TriggerAlert(enemy, enemyPos, distance, angleDegrees);
                    _scoutData.LastAlertTime[enemyId] = currentTime;
                }
            }
        }

        /// <summary>
        /// 获取玩家朝向方向（基于速度/输入方向）
        /// </summary>
        private Vector2 GetPlayerFacingDirection()
        {
            if (_player == null)
                return Vector2.Up; // 默认向上

            // 优先使用速度方向
            if (_player is CharacterBody2D body && body.Velocity != Vector2.Zero)
            {
                return body.Velocity.Normalized();
            }

            // 备选：从输入获取
            Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            if (input != Vector2.Zero)
                return input.Normalized();

            return Vector2.Up;
        }

        /// <summary>
        /// 获取感知范围内的所有敌人
        /// </summary>
        private List<Node2D> GetEnemiesInPerceptionRange()
        {
            var enemies = new List<Node2D>();

            // 从场景树获取所有敌人节点
            var tree = GetTree();
            if (tree == null) return enemies;

            var root = tree.Root;
            if (root == null) return enemies;

            // 遍历所有组
            foreach (var node in root.GetChildren(true))
            {
                if (node is CharacterBody2D enemy && IsEnemy(enemy))
                {
                    float dist = _player.GlobalPosition.DistanceTo(enemy.GlobalPosition);
                    if (dist <= _scoutData.PerceptionRadius)
                    {
                        enemies.Add(enemy);
                    }
                }
            }

            return enemies;
        }

        /// <summary>
        /// 判断是否为敌人（可扩展）
        /// </summary>
        private bool IsEnemy(Node2D node)
        {
            // 检查节点是否有 "Enemy" 标签或名称包含 "Enemy"
            if (node.Name.ContainsKey("Enemy") || node.Name.ContainsKey("Monster"))
                return true;

            // 检查是否在指定的敌人组
            if (node.IsInGroup(ENEMY_GROUP))
                return true;

            return false;
        }

        #endregion

        #region Alert System

        /// <summary>
        /// 触发警报
        /// </summary>
        private void TriggerAlert(Node2D enemy, Vector2 enemyPos, float distance, float angleFromBehind)
        {
            ScoutAlertType alertType = DetermineAlertType(distance);

            // 发送信号
            EnemyDetectedInBlindSpot.Emit(enemy.Name, enemyPos, alertType);

            // 记录已警报的敌人
            _alertedEnemies.Add(enemy.Name);

            // 触发发光效果
            _targetGlowIntensity = 1.0f;

            // 播放警告音效
            if (_scoutData.SoundEnabled)
            {
                PlayAlertSound();
            }

            GD.Print($"[PetScout] Enemy detected in blind spot: {enemy.Name}, type: {alertType}, dist: {distance:F1}");
        }

        /// <summary>
        /// 根据距离确定警报类型
        /// </summary>
        private ScoutAlertType DetermineAlertType(float distance)
        {
            if (distance < 100f)
                return ScoutAlertType.EnemyClose;
            if (distance < 200f)
                return ScoutAlertType.EnemyBehind;
            return ScoutAlertType.EnemyDetected;
        }

        /// <summary>
        /// 播放警告音效
        /// </summary>
        private void PlayAlertSound()
        {
            // 播放警告音效 - 通过信号通知其他系统处理实际音效播放
            PlayedAlertSound.Emit();
        }

        #endregion

        #region Glow Effect

        /// <summary>
        /// 更新发光效果
        /// </summary>
        private void UpdateGlowEffect(float delta)
        {
            if (Mathf.Abs(_currentGlowIntensity - _targetGlowIntensity) > 0.01f)
            {
                // 平滑过渡
                _currentGlowIntensity = Mathf.Lerp(_currentGlowIntensity, _targetGlowIntensity, delta * 5f);

                // 发送信号更新宠物外观
                GlowIntensityChanged.Emit(_currentGlowIntensity);

                // 衰减目标值
                if (_targetGlowIntensity > 0)
                {
                    _targetGlowIntensity = Mathf.Max(0, _targetGlowIntensity - delta * 0.5f);
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 切换侦察模式
        /// </summary>
        public void ToggleScoutMode()
        {
            SetScoutEnabled(!_scoutData.ScoutEnabled);
        }

        /// <summary>
        /// 设置侦察模式是否启用
        /// </summary>
        public void SetScoutEnabled(bool enabled)
        {
            _scoutData.ScoutEnabled = enabled;
            ScoutModeChanged.Emit(enabled);

            if (!enabled)
            {
                _targetGlowIntensity = 0f;
                _currentGlowIntensity = 0f;
            }

            GD.Print($"[PetScout] Scout mode: {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// 获取当前侦察数据
        /// </summary>
        public PetScoutData GetScoutData()
        {
            return _scoutData;
        }

        /// <summary>
        /// 设置感知参数
        /// </summary>
        public void Configure(float blindSpotAngle, float perceptionRadius, float alertCooldown)
        {
            _scoutData.BlindSpotAngle = blindSpotAngle;
            _scoutData.PerceptionRadius = perceptionRadius;
            _scoutData.AlertCooldown = alertCooldown;
        }

        /// <summary>
        /// 获取当前检测到的盲区敌人数量
        /// </summary>
        public int GetDetectedEnemyCount()
        {
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            int count = 0;

            foreach (var kvp in _scoutData.LastAlertTime)
            {
                if (currentTime - kvp.Value < _scoutData.AlertCooldown * 2)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// 清除警报冷却记录（当敌人死亡时调用）
        /// </summary>
        public void ClearAlertForEnemy(string enemyId)
        {
            _scoutData.LastAlertTime.Remove(enemyId);
            _alertedEnemies.Remove(enemyId);
        }

        #endregion
    }
}
