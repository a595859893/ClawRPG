using Godot;
using System;

namespace ClawRPG.Scripts.Systems.PetSynergy
{
    /// <summary>
    /// 默契技能条目 — 配置宠物配合动作
    /// </summary>
    [System.Serializable]
    public class SynergySkillEntry
    {
        /// <summary>e.g. "sync_attack"</summary>
        public string SkillId;

        /// <summary>宠物A的攻击动画名称</summary>
        public string AnimationA;

        /// <summary>宠物B的配合动画名称</summary>
        public string AnimationB;

        /// <summary>触发配合所需的最低友谊等级</summary>
        public int MinFriendshipLevel;

        /// <summary>B动画相对A的延迟（秒）</summary>
        public float TimingOffset;

        /// <summary>友谊等级 1-5 触发的小动作</summary>
        public string SmallGestureAnim;

        /// <summary>友谊等级 6-15 触发的中动作</summary>
        public string MediumGestureAnim;

        /// <summary>友谊等级 16-20 触发的华丽特效</summary>
        public string HighTierEffectScene;
    }
}
