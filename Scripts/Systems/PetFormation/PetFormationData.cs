using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.PetFormation
{
    /// <summary>
    /// 宠物站位槽位
    /// </summary>
    public enum PetFormationSlot
    {
        None = 0,
        Front = 1,   // 前锋 — 坦克位，吸收伤害
        Mid = 2,     // 中线 — 平衡位
        Rear = 3     // 后卫 — 输出位
    }

    /// <summary>
    /// 阵型类型 — 根据宠物位置组合决定
    /// </summary>
    public enum FormationType
    {
        None = 0,           // 无有效阵型
        Solo = 1,           // 单宠物默认阵型
        AggressiveRush = 2, // 全力进攻：前+中+后全前压
        Balanced = 3,       // 攻守平衡：前坦后输出
        GuardFormation = 4, // 铁桶阵：前+后，中线保护
        PincerSetup = 5,   // 钳形攻势：前+后夹击
        FlexibleAssault = 6 // 灵活突击：中有坦克
    }

    /// <summary>
    /// 阵型效果
    /// </summary>
    [System.Serializable]
    public struct FormationEffect
    {
        /// <summary>对敌伤害倍率 (1.0 = 无加成)</summary>
        public float DamageMod;
        /// <summary>受到伤害倍率 (1.0 = 无减免)</summary>
        public float TakenMod;
        /// <summary>特殊效果描述</summary>
        public string SpecialEffect;
        /// <summary>是否启用协同技能触发</summary>
        public bool SynergyEnabled;

        public FormationEffect(float damageMod = 1.0f, float takenMod = 1.0f, string specialEffect = "", bool synergyEnabled = true)
        {
            DamageMod = damageMod;
            TakenMod = takenMod;
            SpecialEffect = specialEffect;
            SynergyEnabled = synergyEnabled;
        }

        public static FormationEffect None => new FormationEffect(1.0f, 1.0f, "", false);
        public static FormationEffect Solo => new FormationEffect(1.0f, 1.0f, "单宠物无阵型加成", false);
    }

    /// <summary>
    /// 宠物槽位分配记录
    /// </summary>
    [System.Serializable]
    public class PetFormationAssignment
    {
        public int PetId;
        public PetFormationSlot Slot;

        public PetFormationAssignment() { }

        public PetFormationAssignment(int petId, PetFormationSlot slot)
        {
            PetId = petId;
            Slot = slot;
        }
    }

    /// <summary>
    /// 阵型配置条目
    /// </summary>
    [System.Serializable]
    public class FormationConfigEntry
    {
        public FormationType Type;
        public string DisplayName;
        public string Description;
        public FormationEffect Effect;
        /// <summary>触发此阵型需要的宠物数量</summary>
        public int MinPets;
        /// <summary>需要的宠物位置掩码 (PetFormationSlot 位标志)</summary>
        public int RequiredSlots;
    }

    /// <summary>
    /// 宠物阵型数据库 — 预定义所有阵型配置
    /// </summary>
    public class PetFormationDatabase
    {
        private static PetFormationDatabase _instance;
        public static PetFormationDatabase Instance => _instance ??= new PetFormationDatabase();

        private Dictionary<FormationType, FormationConfigEntry> _configByType = new Dictionary<FormationType, FormationConfigEntry>();
        private List<FormationConfigEntry> _allConfigs = new List<FormationConfigEntry>();

        public PetFormationDatabase()
        {
            InitializeFormations();
        }

        private void InitializeFormations()
        {
            // Solo — 单宠物
            AddFormation(new FormationConfigEntry
            {
                Type = FormationType.Solo,
                DisplayName = "单独作战",
                Description = "单宠物默认位置，无阵型加成",
                Effect = FormationEffect.Solo,
                MinPets = 1,
                RequiredSlots = 0
            });

            // AggressiveRush — 全力进攻 (前+中+后全前压)
            AddFormation(new FormationConfigEntry
            {
                Type = FormationType.AggressiveRush,
                DisplayName = "全力突击",
                Description = "三路齐头并进，最大化输出但防御薄弱",
                Effect = new FormationEffect(1.35f, 1.25f, "全队+35%伤害，受到+25%伤害", true),
                MinPets = 3,
                RequiredSlots = (int)(PetFormationSlot.Front | PetFormationSlot.Mid | PetFormationSlot.Rear)
            });

            // Balanced — 攻守平衡
            AddFormation(new FormationConfigEntry
            {
                Type = FormationType.Balanced,
                DisplayName = "攻守平衡",
                Description = "前排吸收伤害，后排稳定输出",
                Effect = new FormationEffect(1.15f, 0.85f, "全队+15%伤害，受到-15%伤害", true),
                MinPets = 3,
                RequiredSlots = (int)(PetFormationSlot.Front | PetFormationSlot.Mid | PetFormationSlot.Rear)
            });

            // GuardFormation — 铁桶阵 (前坦后输出)
            AddFormation(new FormationConfigEntry
            {
                Type = FormationType.GuardFormation,
                DisplayName = "铁桶阵",
                Description = "中线保护后排，最大化生存能力",
                Effect = new FormationEffect(0.90f, 0.60f, "全队-10%伤害，受到-40%伤害", true),
                MinPets = 3,
                RequiredSlots = (int)(PetFormationSlot.Front | PetFormationSlot.Mid | PetFormationSlot.Rear)
            });

            // PincerSetup — 钳形攻势 (前+后，中间空)
            AddFormation(new FormationConfigEntry
            {
                Type = FormationType.PincerSetup,
                DisplayName = "钳形攻势",
                Description = "前后夹击，中间引诱敌人",
                Effect = new FormationEffect(1.25f, 1.10f, "全队+25%伤害，受到+10%伤害，前后排协同+20%", true),
                MinPets = 2,
                RequiredSlots = (int)(PetFormationSlot.Front | PetFormationSlot.Rear)
            });

            // FlexibleAssault — 灵活突击 (中有坦克)
            AddFormation(new FormationConfigEntry
            {
                Type = FormationType.FlexibleAssault,
                DisplayName = "灵活突击",
                Description = "中线坦克牵制，灵活应对变化",
                Effect = new FormationEffect(1.10f, 0.90f, "全队+10%伤害，受到-10%伤害，中线嘲讽+15%", true),
                MinPets = 2,
                RequiredSlots = (int)(PetFormationSlot.Mid)
            });
        }

        private void AddFormation(FormationConfigEntry entry)
        {
            _configByType[entry.Type] = entry;
            _allConfigs.Add(entry);
        }

        public FormationConfigEntry GetConfig(FormationType type)
        {
            return _configByType.TryGetValue(type, out var config) ? config : null;
        }

        public FormationEffect GetEffect(FormationType type)
        {
            var config = GetConfig(type);
            return config != null ? config.Effect : FormationEffect.None;
        }

        public List<FormationConfigEntry> GetAllConfigs() => new List<FormationConfigEntry>(_allConfigs);

        /// <summary>
        /// 根据槽位分配判断激活的阵型
        /// </summary>
        public FormationType DetermineFormation(int? frontPetId, int? midPetId, int? rearPetId)
        {
            int filledCount = 0;
            if (frontPetId.HasValue && frontPetId.Value > 0) filledCount++;
            if (midPetId.HasValue && midPetId.Value > 0) filledCount++;
            if (rearPetId.HasValue && rearPetId.Value > 0) filledCount++;

            if (filledCount == 0)
                return FormationType.None;

            if (filledCount == 1)
                return FormationType.Solo;

            // 有3个宠物
            if (filledCount == 3)
            {
                // 中间是坦克(前排宠物) → Balanced
                // 全进攻态势 → AggressiveRush
                // GuardFormation 有特殊处理
                // 这里简化为：中间有宠物=Balanced，否则=AggressiveRush
                if (midPetId.HasValue && midPetId.Value > 0)
                    return FormationType.Balanced;
                return FormationType.AggressiveRush;
            }

            // 2个宠物的情况
            bool hasFront = frontPetId.HasValue && frontPetId.Value > 0;
            bool hasMid = midPetId.HasValue && midPetId.Value > 0;
            bool hasRear = rearPetId.HasValue && rearPetId.Value > 0;

            if (hasFront && hasRear && !hasMid)
                return FormationType.PincerSetup; // 钳形：前+后
            if (hasMid)
                return FormationType.FlexibleAssault; // 中线为主
            if (hasFront && hasMid)
                return FormationType.Balanced; // 前+中 → 平衡
            if (hasMid && hasRear)
                return FormationType.Balanced; // 中+后 → 平衡

            return FormationType.Solo;
        }
    }
}
