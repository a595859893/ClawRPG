using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑管理器 - 管理玩家拥有的坐骑
    /// </summary>
    public class MountManager : BaseSystem {
        public static MountManager Instance { get; private set; }

        private Dictionary<string, MountInstance> _ownedMounts = new Dictionary<string, MountInstance>();
        private string _activeMountId = null;

        // 信号系统
        public delegate void OnMountAdded(string mountId);
        public delegate void OnMountRemoved(string mountId);
        public delegate void OnMountActivated(string mountId);
        public delegate void OnMountDeactivated();
        public delegate void OnMountLevelUp(string mountId, int newLevel);
        public delegate void OnMountExperienceGained(string mountId, int exp);
        public delegate void OnMountEffectTriggered(string mountId, Vector2 position);

        // Shader material for mount trail effect
        private ShaderMaterial _mountTrailMaterial;

        public override void _Ready() {
            Instance = this;
            InitializeMountTrailEffect();
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "Mount";
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            var mounts = new Array();
            foreach (var kvp in _ownedMounts)
            {
                mounts.Add(kvp.Key);
            }
            data["owned_mounts"] = mounts;
            data["active_mount_id"] = _activeMountId ?? "";
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 导入已拥有的坐骑
            if (data.Contains("owned_mounts"))
            {
                var mounts = data["owned_mounts"] as Array;
                if (mounts != null)
                {
                    foreach (string mountId in mounts)
                    {
                        if (!string.IsNullOrEmpty(mountId) && !_ownedMounts.ContainsKey(mountId))
                        {
                            _ownedMounts[mountId] = new MountInstance
                            {
                                MountId = mountId,
                                Level = 1,
                                Experience = 0,
                                IsActive = false,
                                ObtainedAt = DateTime.Now
                            };
                        }
                    }
                }
            }
            
            // 导入当前激活的坐骑
            if (data.Contains("active_mount_id"))
            {
                string activeMountId = data["active_mount_id"] as string;
                if (!string.IsNullOrEmpty(activeMountId) && _ownedMounts.ContainsKey(activeMountId))
                {
                    _activeMountId = activeMountId;
                    _ownedMounts[activeMountId].IsActive = true;
                    EmitSignal(nameof(OnMountActivated), activeMountId);
                }
            }
            
            GD.Print($"[MountManager] Imported {_ownedMounts.Count} mounts, active: {_activeMountId}");
        }

        /// <summary>
        /// 初始化坐骑拖尾特效
        /// </summary>
        private void InitializeMountTrailEffect() {
            var shader = GD.Load<Shader>("res://Shaders/mount_trail.gdshader");
            if (shader != null) {
                _mountTrailMaterial = new ShaderMaterial();
                _mountTrailMaterial.Shader = shader;
                // Set default values
                _mountTrailMaterial.SetShaderParameter("trail_intensity", 0.5f);
                _mountTrailMaterial.SetShaderParameter("pulse_speed", 2.0f);
                _mountTrailMaterial.SetShaderParameter("trail_color", new Color(0.3f, 0.7f, 1.0f, 0.8f));
                GD.Print("[MountManager] Trail shader initialized");
            } else {
                GD.Warning("[MountManager] Failed to load mount_trail.gdshader");
            }
        }

        /// <summary>
        /// 获取坐骑拖尾特效材质
        /// </summary>
        public ShaderMaterial GetMountTrailMaterial() {
            return _mountTrailMaterial;
        }

        /// <summary>
        /// 触发坐骑特效
        /// </summary>
        public void TriggerMountEffect(Vector2 position) {
            if (_activeMountId != null && _mountTrailMaterial != null) {
                EmitSignal(nameof(OnMountEffectTriggered), _activeMountId, position);
            }
        }

        /// <summary>
        /// 设置拖尾强度
        /// </summary>
        public void SetTrailIntensity(float intensity) {
            if (_mountTrailMaterial != null) {
                _mountTrailMaterial.SetShaderParameter("trail_intensity", Mathf.Clamp(intensity, 0.0f, 1.0f));
            }
        }

        /// <summary>
        /// 设置拖尾颜色
        /// </summary>
        public void SetTrailColor(Color color) {
            if (_mountTrailMaterial != null) {
                _mountTrailMaterial.SetShaderParameter("trail_color", color);
            }
        }

        /// <summary>
        /// 设置脉冲速度
        /// </summary>
        public void SetPulseSpeed(float speed) {
            if (_mountTrailMaterial != null) {
                _mountTrailMaterial.SetShaderParameter("pulse_speed", Mathf.Clamp(speed, 0.1f, 5.0f));
            }
        }

        public override void _Process(float delta) {
            // 可以在这里处理坐骑相关的实时更新
        }

        /// <summary>
        /// 购买坐骑
        /// </summary>
        public bool PurchaseMount(string mountId) {
            var database = MountDatabase.Instance;
            if (database == null) return false;

            var mount = database.GetMount(mountId);
            if (mount == null) return false;

            if (_ownedMounts.ContainsKey(mountId)) {
                GD.Print("已拥有该坐骑");
                return false;
            }

            var player = GetTree().CurrentScene.GetNodeOrNull<Player>("../Player");
            if (player == null) {
                GD.Print("找不到玩家节点");
                return false;
            }

            if (player.Level < mount.UnlockLevel) {
                GD.Print($"等级不足，需要 {mount.UnlockLevel} 级");
                return false;
            }

            if (player.Gold < mount.Price) {
                GD.Print($"金币不足，需要 {mount.Price}");
                return false;
            }

            // 扣除金币并添加坐骑
            player.AddGold(-mount.Price);
            AddMount(mountId);

            return true;
        }

        /// <summary>
        /// 添加坐骑
        /// </summary>
        public void AddMount(string mountId) {
            if (_ownedMounts.ContainsKey(mountId)) return;

            var instance = new MountInstance {
                MountId = mountId
            };
            _ownedMounts[mountId] = instance;

            GD.Print($"坐骑已添加: {mountId}");
            EmitSignal(nameof(OnMountAdded), mountId);
        }

        /// <summary>
        /// 移除坐骑
        /// </summary>
        public void RemoveMount(string mountId) {
            if (!_ownedMounts.ContainsKey(mountId)) return;

            if (_activeMountId == mountId) {
                DeactivateMount();
            }

            _ownedMounts.Remove(mountId);
            EmitSignal(nameof(OnMountRemoved), mountId);
        }

        /// <summary>
        /// 激活坐骑
        /// </summary>
        public void ActivateMount(string mountId) {
            if (!_ownedMounts.ContainsKey(mountId)) return;

            var instance = _ownedMounts[mountId];
            instance.IsActive = true;
            _activeMountId = mountId;

            // 应用坐骑属性加成
            ApplyMountBonuses(mountId, true);

            // 触发坐骑特效
            var player = GetTree().CurrentScene.GetNodeOrNull<Player>("../Player");
            if (player != null) {
                TriggerMountEffect(player.GlobalPosition);
            }

            GD.Print($"坐骑已激活: {mountId}");
            EmitSignal(nameof(OnMountActivated), mountId);
        }

        /// <summary>
        /// 取消激活坐骑
        /// </summary>
        public void DeactivateMount() {
            if (_activeMountId == null) return;

            ApplyMountBonuses(_activeMountId, false);

            var instance = _ownedMounts[_activeMountId];
            instance.IsActive = false; 

            GD.Print($"坐骑已取消: {_activeMountId}");
            EmitSignal(nameof(OnMountDeactivated));

            _activeMountId = null;
        }

        /// <summary>
        /// 切换坐骑
        /// </summary>
        public void ToggleMount(string mountId) {
            if (_activeMountId == mountId) {
                DeactivateMount();
            } else {
                if (_activeMountId != null) {
                    DeactivateMount();
                }
                ActivateMount(mountId);
            }
        }

        /// <summary>
        /// 应用/移除坐骑属性加成
        /// </summary>
        private void ApplyMountBonuses(string mountId, bool apply) {
            var player = GetTree().CurrentScene.GetNodeOrNull<Player>("../Player");
            if (player == null) return;

            var mount = MountDatabase.Instance.GetMount(mountId);
            if (mount == null) return;

            int multiplier = apply ? 1 : -1;

            // 应用属性加成
            player.BaseMaxHealth += mount.HealthBonus * multiplier;
            player.BaseDefense += mount.DefenseBonus * multiplier;
            
            // 注意：速度加成需要在Player中特殊处理
            if (apply) {
                player.MountSpeedBonus = mount.SpeedBonus;
                player.MountCarryCapacityBonus = mount.CarryCapacityBonus;
            } else {
                player.MountSpeedBonus = 0;
                player.MountCarryCapacityBonus = 0;
            }

            // 更新当前HP和MP
            if (apply && player.CurrentHealth < player.MaxHealth) {
                player.Heal(mount.HealthBonus);
            }
        }

        /// <summary>
        /// 为坐骑添加经验
        /// </summary>
        public void AddExperience(string mountId, int exp) {
            if (!_ownedMounts.ContainsKey(mountId)) return;

            var instance = _ownedMounts[mountId];
            int oldLevel = instance.Level;
            instance.AddExperience(exp);

            EmitSignal(nameof(OnMountExperienceGained), mountId, exp);

            if (instance.Level > oldLevel) {
                EmitSignal(nameof(OnMountLevelUp), mountId, instance.Level);
                
                // 重新应用属性加成（等级提升后属性增加）
                if (instance.IsActive) {
                    ApplyMountBonuses(mountId, false);
                    ApplyMountBonuses(mountId, true);
                }
            }
        }

        /// <summary>
        /// 获取已拥有的坐骑
        /// </summary>
        public Dictionary<string, MountInstance> GetOwnedMounts() {
            return _ownedMounts;
        }

        /// <summary>
        /// 获取已激活的坐骑ID
        /// </summary>
        public string GetActiveMountId() {
            return _activeMountId;
        }

        /// <summary>
        /// 获取已激活的坐骑实例
        /// </summary>
        public MountInstance GetActiveMount() {
            if (_activeMountId == null) return null;
            return _ownedMounts.ContainsKey(_activeMountId) ? _ownedMounts[_activeMountId] : null;
        }

        /// <summary>
        /// 获取已激活的坐骑数据
        /// </summary>
        public Mount GetActiveMountData() {
            if (_activeMountId == null) return null;
            return MountDatabase.Instance.GetMount(_activeMountId);
        }

        /// <summary>
        /// 是否拥有指定坐骑
        /// </summary>
        public bool HasMount(string mountId) {
            return _ownedMounts.ContainsKey(mountId);
        }

        /// <summary>
        /// 获取坐骑数量
        /// </summary>
        public int GetMountCount() {
            return _ownedMounts.Count;
        }

        /// <summary>
        /// 是否正在骑乘坐骑
        /// </summary>
        public bool IsRiding() {
            return _activeMountId != null;
        }

        /// <summary>
        /// 序列化 - 保存数据
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> Serialize() {
            Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>();

            foreach (var kvp in _ownedMounts) {
                Dictionary<string, object> mountData = new Dictionary<string, object>();
                mountData["mountId"] = kvp.Value.MountId;
                mountData["level"] = kvp.Value.Level;
                mountData["experience"] = kvp.Value.Experience;
                mountData["isActive"] = kvp.Value.IsActive;
                mountData["obtainedAt"] = kvp.Value.ObtainedAt.ToString("o");
                data[kvp.Key] = mountData;
            }

            return data;
        }

        /// <summary>
        /// 反序列化 - 加载数据
        /// </summary>
        public void Deserialize(Dictionary<string, Dictionary<string, object>> data) {
            if (data == null) return;

            _ownedMounts.Clear();

            foreach (var kvp in data) {
                var mountData = kvp.Value;
                var instance = new MountInstance();
                instance.MountId = mountData["mountId"].ToString();
                instance.Level = Convert.ToInt32(mountData["level"]);
                instance.Experience = Convert.ToInt32(mountData["experience"]);
                instance.IsActive = Convert.ToBoolean(mountData["isActive"]);
                
                if (mountData.ContainsKey("obtainedAt")) {
                    DateTime.TryParse(mountData["obtainedAt"].ToString(), out var obtainedAt);
                    instance.ObtainedAt = obtainedAt;
                }

                _ownedMounts[kvp.Key] = instance;

                // 如果坐骑处于激活状态，恢复属性加成
                if (instance.IsActive) {
                    _activeMountId = instance.MountId;
                    ApplyMountBonuses(instance.MountId, true);
                }
            }
        }
    }
}
