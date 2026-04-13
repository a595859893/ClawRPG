using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetFormation
{
    /// <summary>
    /// 宠物战术阵型系统
    /// 玩家可拖动宠物到前锋/中线/后卫三个位置，不同阵型触发不同协同效果
    /// 位置本身就是决策变量
    /// </summary>
    public partial class PetFormationSystem : BaseSystem
    {
        private static PetFormationSystem _instance;
        public static PetFormationSystem Instance => _instance;

        private PetFormationDatabase _database;

        // 当前槽位分配
        private int? _frontPetId;
        private int? _midPetId;
        private int? _rearPetId;

        // 当前激活的阵型
        private FormationType _activeFormation = FormationType.None;
        private FormationEffect _activeEffect = FormationEffect.None;

        // Signals
        public delegate void FormationChangedEventHandler(FormationType formation, FormationEffect effect);
        public delegate void SlotAssignedEventHandler(int petId, PetFormationSlot slot);
        public delegate void SlotRemovedEventHandler(PetFormationSlot slot);

        public event FormationChangedEventHandler OnFormationChanged;
        public event SlotAssignedEventHandler OnSlotAssigned;
        public event SlotRemovedEventHandler OnSlotRemoved;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _database = PetFormationDatabase.Instance;

            SubscribeToGameEvents();
            GD.Print("[PetFormation] 系统初始化完成");
        }

        protected override string SystemName => "PetFormationSystem";

        #region Event Subscription

        private void SubscribeToGameEvents()
        {
            // 战斗开始时验证阵型分配
            var combat = GetNodeOrNull("/root/Main/CombatManager");
            if (combat != null)
            {
                // 注意：实际信号名称需根据 CombatManager 的具体信号确定
                // 这里使用占位符，实际集成时需要匹配真实信号名
            }

            // 宠物系统信号（预留集成点）
            // OnPetAssignedToSlot / OnPetRemovedFromSlot
        }

        #endregion

        #region Core API

        /// <summary>
        /// 获取当前激活的阵型类型
        /// </summary>
        public FormationType GetFormationType()
        {
            return _activeFormation;
        }

        /// <summary>
        /// 根据宠物ID计算当前阵型
        /// </summary>
        public FormationType GetFormationType(int? frontPetId, int? midPetId, int? rearPetId)
        {
            return _database.DetermineFormation(frontPetId, midPetId, rearPetId);
        }

        /// <summary>
        /// 获取指定阵型的效果
        /// </summary>
        public FormationEffect GetFormationEffect(FormationType formation)
        {
            return _database.GetEffect(formation);
        }

        /// <summary>
        /// 获取当前激活的阵型效果
        /// </summary>
        public FormationEffect GetActiveEffect()
        {
            return _activeEffect;
        }

        /// <summary>
        /// 获取当前激活的阵型类型和效果
        /// </summary>
        public (FormationType Type, FormationEffect Effect) GetActiveFormation()
        {
            return (_activeFormation, _activeEffect);
        }

        /// <summary>
        /// 获取所有可用的阵型配置
        /// </summary>
        public List<FormationConfigEntry> GetAllFormationConfigs()
        {
            return _database.GetAllConfigs();
        }

        #endregion

        #region Slot Assignment

        /// <summary>
        /// 将宠物分配到指定槽位
        /// 如果宠物已在其他槽位，先移除
        /// </summary>
        public void AssignPetToSlot(int petId, PetFormationSlot slot)
        {
            if (slot == PetFormationSlot.None)
            {
                GD.PrintErr($"[PetFormation] 无法分配到 None 槽位: petId={petId}");
                return;
            }

            // 先从所有槽位移除该宠物
            RemovePetFromAllSlots(petId);

            // 分配到新槽位
            switch (slot)
            {
                case PetFormationSlot.Front:
                    _frontPetId = petId;
                    break;
                case PetFormationSlot.Mid:
                    _midPetId = petId;
                    break;
                case PetFormationSlot.Rear:
                    _rearPetId = petId;
                    break;
            }

            UpdateActiveFormation();
            OnSlotAssigned?.Invoke(petId, slot);
            GD.Print($"[PetFormation] 宠物 {petId} 分配到 {slot}");
        }

        /// <summary>
        /// 从指定槽位移除宠物
        /// </summary>
        public void RemovePetFromSlot(PetFormationSlot slot)
        {
            switch (slot)
            {
                case PetFormationSlot.Front:
                    _frontPetId = null;
                    break;
                case PetFormationSlot.Mid:
                    _midPetId = null;
                    break;
                case PetFormationSlot.Rear:
                    _rearPetId = null;
                    break;
            }

            UpdateActiveFormation();
            OnSlotRemoved?.Invoke(slot);
            GD.Print($"[PetFormation] 槽位 {slot} 已清空");
        }

        /// <summary>
        /// 从所有槽位移除指定宠物
        /// </summary>
        public void RemovePetFromAllSlots(int petId)
        {
            bool removed = false;

            if (_frontPetId == petId)
            {
                _frontPetId = null;
                removed = true;
            }
            if (_midPetId == petId)
            {
                _midPetId = null;
                removed = true;
            }
            if (_rearPetId == petId)
            {
                _rearPetId = null;
                removed = true;
            }

            if (removed)
            {
                UpdateActiveFormation();
                OnSlotRemoved?.Invoke(PetFormationSlot.None);
            }
        }

        /// <summary>
        /// 交换两个槽位的宠物
        /// </summary>
        public void SwapSlots(PetFormationSlot slotA, PetFormationSlot slotB)
        {
            int? petA = GetPetIdInSlot(slotA);
            int? petB = GetPetIdInSlot(slotB);

            RemovePetFromSlot(slotA);
            RemovePetFromSlot(slotB);

            if (petA.HasValue && petA.Value > 0)
                AssignPetToSlot(petA.Value, slotB);
            if (petB.HasValue && petB.Value > 0)
                AssignPetToSlot(petB.Value, slotA);
        }

        /// <summary>
        /// 清空所有槽位
        /// </summary>
        public void ClearAllSlots()
        {
            _frontPetId = null;
            _midPetId = null;
            _rearPetId = null;
            UpdateActiveFormation();
            GD.Print("[PetFormation] 所有槽位已清空");
        }

        #endregion

        #region Slot Queries

        /// <summary>
        /// 获取指定槽位的宠物ID
        /// </summary>
        public int? GetPetIdInSlot(PetFormationSlot slot)
        {
            switch (slot)
            {
                case PetFormationSlot.Front:
                    return _frontPetId;
                case PetFormationSlot.Mid:
                    return _midPetId;
                case PetFormationSlot.Rear:
                    return _rearPetId;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取宠物当前所在的槽位
        /// </summary>
        public PetFormationSlot GetSlotForPet(int petId)
        {
            if (_frontPetId == petId) return PetFormationSlot.Front;
            if (_midPetId == petId) return PetFormationSlot.Mid;
            if (_rearPetId == petId) return PetFormationSlot.Rear;
            return PetFormationSlot.None;
        }

        /// <summary>
        /// 检查宠物是否在某槽位
        /// </summary>
        public bool IsPetInSlot(int petId, PetFormationSlot slot)
        {
            return GetSlotForPet(petId) == slot;
        }

        /// <summary>
        /// 检查槽位是否为空
        /// </summary>
        public bool IsSlotEmpty(PetFormationSlot slot)
        {
            int? petId = GetPetIdInSlot(slot);
            return !petId.HasValue || petId.Value <= 0;
        }

        /// <summary>
        /// 获取当前分配的宠物数量
        /// </summary>
        public int GetAssignedPetCount()
        {
            int count = 0;
            if (_frontPetId.HasValue && _frontPetId.Value > 0) count++;
            if (_midPetId.HasValue && _midPetId.Value > 0) count++;
            if (_rearPetId.HasValue && _rearPetId.Value > 0) count++;
            return count;
        }

        /// <summary>
        /// 获取前锋宠物ID
        /// </summary>
        public int? GetFrontPetId() => _frontPetId;

        /// <summary>
        /// 获取中线宠物ID
        /// </summary>
        public int? GetMidPetId() => _midPetId;

        /// <summary>
        /// 获取后卫宠物ID
        /// </summary>
        public int? GetRearPetId() => _rearPetId;

        #endregion

        #region Formation Logic

        /// <summary>
        /// 更新当前激活的阵型
        /// </summary>
        private void UpdateActiveFormation()
        {
            var newFormation = GetFormationType(_frontPetId, _midPetId, _rearPetId);

            if (newFormation != _activeFormation)
            {
                _activeFormation = newFormation;
                _activeEffect = _database.GetEffect(newFormation);

                OnFormationChanged?.Invoke(_activeFormation, _activeEffect);
                GD.Print($"[PetFormation] 阵型变更: {_activeFormation} — 伤害{_activeEffect.DamageMod:F0%} / 受到{_activeEffect.TakenMod:F0%} / {_activeEffect.SpecialEffect}");
            }
        }

        /// <summary>
        /// 获取当前阵型的显示名称
        /// </summary>
        public string GetFormationDisplayName()
        {
            var config = _database.GetConfig(_activeFormation);
            return config != null ? config.DisplayName : "无阵型";
        }

        /// <summary>
        /// 获取当前阵型的描述
        /// </summary>
        public string GetFormationDescription()
        {
            var config = _database.GetConfig(_activeFormation);
            return config != null ? config.Description : "未选择有效阵型";
        }

        #endregion

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["front_pet_id"] = _frontPetId ?? 0;
            data["mid_pet_id"] = _midPetId ?? 0;
            data["rear_pet_id"] = _rearPetId ?? 0;
            data["last_formation_type"] = (int)_activeFormation;
            data["formation_validated"] = false; // 新 run 重置
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 加载槽位分配
            if (data.TryGetValue("front_pet_id", out var fObj))
                _frontPetId = fObj is System.Int64 f && f > 0 ? (int)f : null;

            if (data.TryGetValue("mid_pet_id", out var mObj))
                _midPetId = mObj is System.Int64 m && m > 0 ? (int)m : null;

            if (data.TryGetValue("rear_pet_id", out var rObj))
                _rearPetId = rObj is System.Int64 r && r > 0 ? (int)r : null;

            // 恢复上次阵型（如果有的话）
            if (data.TryGetValue("last_formation_type", out var typeObj) && typeObj is System.Int64 typeInt)
            {
                var restoredType = (FormationType)(int)typeInt;
                // 不直接用存档的阵型，而是根据当前槽位重新计算
                GD.Print($"[PetFormation] 从存档加载槽位: 前={_frontPetId} 中={_midPetId} 后={_rearPetId}");
            }

            UpdateActiveFormation();
            GD.Print($"[PetFormation] 从存档加载完成 — 激活阵型: {_activeFormation}");
        }

        #endregion

        #region Combat Integration

        /// <summary>
        /// 战斗开始时调用 — 验证阵型分配是否仍然有效
        /// 如果有宠物已离队，清空对应槽位
        /// </summary>
        public void ValidateFormationForCombat()
        {
            // 这里预留集成点
            // 实际使用时，CombatSystem 或 PetCombatCompanionSystem 应在战斗开始时调用此方法
            // 以验证所有分配的宠物都还在队伍中
            GD.Print($"[PetFormation] 战斗前阵型验证: {_activeFormation}");
        }

        #endregion
    }
}
