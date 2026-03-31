using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetSynergy
{
    /// <summary>
    /// 宠物默契技能数据库 — 预定义配合动作配置
    /// </summary>
    public class PetSynergySkillDatabase
    {
        private static PetSynergySkillDatabase _instance;
        public static PetSynergySkillDatabase Instance => _instance ??= new PetSynergySkillDatabase();

        public List<SynergySkillEntry> SynergySkills { get; private set; } = new();

        private PetSynergySkillDatabase()
        {
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            SynergySkills = new List<SynergySkillEntry>
            {
                // sync_attack — 宠物B横向移动配合
                new SynergySkillEntry
                {
                    SkillId = "sync_attack",
                    AnimationA = "attack",
                    AnimationB = "dash_side",
                    MinFriendshipLevel = 1,
                    TimingOffset = 0.3f,
                    SmallGestureAnim = "nod",
                    MediumGestureAnim = "jump",
                    HighTierGestureAnim = "special_sync",
                    HighTierEffectScene = "res://Effects/PetSynergy/PetSynergyVFX.tscn"
                },

                // guard_follow — 宠物B做保护动作
                new SynergySkillEntry
                {
                    SkillId = "guard_follow",
                    AnimationA = "attack",
                    AnimationB = "guard",
                    MinFriendshipLevel = 1,
                    TimingOffset = 0.35f,
                    SmallGestureAnim = "tail_wag",
                    MediumGestureAnim = "spin",
                    HighTierGestureAnim = "special_guard",
                    HighTierEffectScene = "res://Effects/PetSynergy/PetSynergyVFX.tscn"
                },

                // element_reaction — 元素反应配合
                new SynergySkillEntry
                {
                    SkillId = "element_reaction",
                    AnimationA = "skill_fire",
                    AnimationB = "skill_ice",
                    MinFriendshipLevel = 6,
                    TimingOffset = 0.4f,
                    SmallGestureAnim = "roar",
                    MediumGestureAnim = "element_burst",
                    HighTierGestureAnim = "special_element",
                    HighTierEffectScene = "res://Effects/PetSynergy/PetSynergyVFX.tscn"
                },

                // victory_pose — 击杀后同步摆pose
                new SynergySkillEntry
                {
                    SkillId = "victory_pose",
                    AnimationA = "attack",
                    AnimationB = "victory",
                    MinFriendshipLevel = 11,
                    TimingOffset = 0.5f,
                    SmallGestureAnim = "sit",
                    MediumGestureAnim = "dance",
                    HighTierGestureAnim = "special_victory",
                    HighTierEffectScene = "res://Effects/PetSynergy/PetSynergyVFX.tscn"
                },

                // high_five — 最高友谊特殊动作
                new SynergySkillEntry
                {
                    SkillId = "high_five",
                    AnimationA = "attack",
                    AnimationB = "high_five",
                    MinFriendshipLevel = 16,
                    TimingOffset = 0.25f,
                    SmallGestureAnim = "paw",
                    MediumGestureAnim = "flip",
                    HighTierGestureAnim = "special_highfive",
                    HighTierEffectScene = "res://Effects/PetSynergy/PetSynergyVFX.tscn"
                }
            };
        }

        /// <summary>
        /// 根据友谊等级获取合适的配合动画
        /// </summary>
        public string GetGestureAnimForFriendship(string skillId, int friendshipLevel)
        {
            var entry = SynergySkills.Find(s => s.SkillId == skillId);
            if (entry == null) return null;

            if (friendshipLevel >= 16) return entry.HighTierGestureAnim ?? entry.MediumGestureAnim;
            if (friendshipLevel >= 6) return entry.MediumGestureAnim;
            return entry.SmallGestureAnim;
        }

        /// <summary>
        /// 获取适合友谊等级的配合技能
        /// </summary>
        public SynergySkillEntry GetSkillForFriendship(int friendshipLevel)
        {
            // 选择最高等级的满足条件的技能
            SynergySkillEntry best = null;
            foreach (var skill in SynergySkills)
            {
                if (skill.MinFriendshipLevel <= friendshipLevel)
                {
                    if (best == null || skill.MinFriendshipLevel > best.MinFriendshipLevel)
                        best = skill;
                }
            }
            if (SynergySkills.Count == 0) return null;
            return best ?? SynergySkills[0];
        }

        /// <summary>
        /// 获取所有技能
        /// </summary>
        public IReadOnlyList<SynergySkillEntry> GetAllSkills() => SynergySkills.AsReadOnly();
    }
}
