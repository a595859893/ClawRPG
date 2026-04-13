using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetBattleDamage
{
    /// <summary>
    /// 宠物战损外观系统
    /// REQ-186: 宠物受到 >30% HP 单次伤害时，在外观上留下可见痕迹
    /// 
    /// 视觉痕迹类型：
    /// - Bandage (绷带): 30-60% HP 单次伤害
    /// - Cut (缺口): 60-90% HP 单次伤害
    /// - Scar (疤痕): 90%+ HP 或多次累积
    /// </summary>
    public partial class PetBattleDamageSystem : BaseSystem
    {
        private static PetBattleDamageSystem _instance;
        public static PetBattleDamageSystem Instance => _instance;

        private PetBattleDamageDatabase _database;

        /// <summary>
        /// 触发战损的伤害阈值（相对于最大HP）
        /// </summary>
        private const float DAMAGE_THRESHOLD_RATIO = 0.30f;

        // Signals
        public delegate void DamageMarkAddedEventHandler(int petId, DamageMarkType markType);
        public delegate void DamageMarksClearedEventHandler(int petId);
        public delegate void VisualDamageLevelChangedEventHandler(int petId, DamageMarkType newLevel);

        public event DamageMarkAddedEventHandler OnDamageMarkAdded;
        public event DamageMarksClearedEventHandler OnDamageMarksCleared;
        public event VisualDamageLevelChangedEventHandler OnVisualDamageLevelChanged;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _database = new PetBattleDamageDatabase();
            SubscribeToSignals();
            GD.Print("[PetBattleDamage] Initialized");
        }

        /// <summary>
        /// 订阅相关信号
        /// </summary>
        private void SubscribeToSignals()
        {
            // 尝试订阅宠物受伤信号（来源: CombatSystem 或 PetCombatCompanionSystem）
            TryConnectToSignal("/root/CombatSystem", "PetDamaged", Callable.From<int, float, float, string>(OnPetDamagedHandler));
            TryConnectToSignal("/root/PetCombatCompanion", "PetDamaged", Callable.From<int, float, float, string>(OnPetDamagedHandler));

            // 订阅宠物死亡信号 → 清除痕迹
            TryConnectToSignal("/root/PetCombatCompanion", "PetDied", Callable.From<Godot.Collections.Dictionary>(OnPetDiedHandler));

            // 订阅治疗信号 → 清除痕迹
            TryConnectToSignal("/root/PetCombatCompanion", "PetHealed", Callable.From<int>(OnPetHealedHandler));
        }

        /// <summary>
        /// 安全连接信号（如果节点或信号不存在则静默跳过）
        /// </summary>
        private void TryConnectToSignal(string nodePath, string signalName, Callable callable)
        {
            var node = GetNodeOrNull<Godot.Node>(nodePath);
            if (node != null && node.HasSignal(signalName))
            {
                if (!node.IsConnected(signalName, callable))
                {
                    node.Connect(signalName, callable);
                }
            }
        }

        /// <summary>
        /// 处理宠物受伤事件
        /// </summary>
        private void OnPetDamagedHandler(int petId, float damage, float maxHp, string battleId)
        {
            // 检测是否超过阈值
            if (maxHp <= 0f)
                return;

            float damageRatio = damage / maxHp;

            if (damageRatio < DAMAGE_THRESHOLD_RATIO)
                return;

            // 确定视觉痕迹类型
            DamageMarkType markType = DetermineMarkType(damageRatio);

            // 添加痕迹
            AddDamageMark(petId, markType, battleId, damageRatio);
        }

        /// <summary>
        /// 根据伤害比例确定视觉痕迹类型
        /// </summary>
        private DamageMarkType DetermineMarkType(float damageRatio)
        {
            if (damageRatio >= 0.90f)
                return DamageMarkType.Scar;
            if (damageRatio >= 0.60f)
                return DamageMarkType.Cut;
            return DamageMarkType.Bandage;
        }

        /// <summary>
        /// 添加战损痕迹
        /// </summary>
        public void AddDamageMark(int petId, DamageMarkType markType, string battleId = "", float damagePercent = 0f)
        {
            var entry = new DamageMarkEntry(petId, markType, battleId, damagePercent);
            _database.AddDamageMark(petId, entry);

            GD.Print($"[PetBattleDamage] Added {markType} mark for pet {petId} (damage: {damagePercent:P0})");

            OnDamageMarkAdded?.Invoke(petId, markType);

            var newLevel = _database.GetVisualDamageLevel(petId);
            OnVisualDamageLevelChanged?.Invoke(petId, newLevel);
        }

        /// <summary>
        /// 清除宠物所有战损痕迹（治疗时调用）
        /// </summary>
        public void ClearDamageMarks(int petId)
        {
            if (_database.GetVisualDamageLevel(petId) == DamageMarkType.None)
                return;

            _database.ClearDamageMarks(petId);
            GD.Print($"[PetBattleDamage] Cleared all damage marks for pet {petId}");

            OnDamageMarksCleared?.Invoke(petId);
            OnVisualDamageLevelChanged?.Invoke(petId, DamageMarkType.None);
        }

        /// <summary>
        /// 处理宠物死亡 → 清除痕迹
        /// </summary>
        private void OnPetDiedHandler(Godot.Collections.Dictionary petData)
        {
            int petId = petData.ContainsKey("pet_id") ? Convert.ToInt32(petData["pet_id"]) : 0;
            if (petId > 0)
            {
                _database.ClearDamageMarks(petId);
                OnDamageMarksCleared?.Invoke(petId);
                OnVisualDamageLevelChanged?.Invoke(petId, DamageMarkType.None);
            }
        }

        /// <summary>
        /// 处理宠物治疗 → 清除痕迹
        /// </summary>
        private void OnPetHealedHandler(int petId)
        {
            ClearDamageMarks(petId);
        }

        /// <summary>
        /// 获取宠物所有战损记录
        /// </summary>
        public List<DamageMarkEntry> GetDamageMarks(int petId)
        {
            return _database.GetDamageMarks(petId);
        }

        /// <summary>
        /// 获取宠物当前视觉战损等级（供宠物精灵图渲染系统调用）
        /// </summary>
        public DamageMarkType GetVisualDamageLevel(int petId)
        {
            return _database.GetVisualDamageLevel(petId);
        }

        /// <summary>
        /// 判断宠物是否有战损痕迹
        /// </summary>
        public bool HasDamageMarks(int petId)
        {
            return _database.GetVisualDamageLevel(petId) != DamageMarkType.None;
        }

        // ══════════════════════════════════════════════════════════════════════
        // 持久化接口
        // ══════════════════════════════════════════════════════════════════════

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = base.ExportSaveData();
            data["PetBattleDamage"] = _database.ExportSaveData();
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            base.ImportSaveData(data);

            if (data.ContainsKey("PetBattleDamage"))
            {
                _database.ImportSaveData((Dictionary<string, object>)data["PetBattleDamage"]);
                GD.Print($"[PetBattleDamage] Loaded damage marks for pets");
            }
        }
    }
}
