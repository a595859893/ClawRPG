using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.PetSynergy;
using ClawRPG.Scripts.Systems.Pets.AI;

namespace ClawRPG.Scripts.Systems.Pets.VFX
{
    /// <summary>
    /// 宠物默契技能触发器 (REQ-163)
    /// 监听宠物攻击信号，检查友谊关系，触发配合动画
    /// 纯视觉系统，不影响数值/伤害计算
    /// </summary>
    public partial class PetSynergySkillTrigger : Node
    {
        public static PetSynergySkillTrigger Instance { get; private set; }

        /// <summary>场上宠物节点列表（由 PetCombatCompanionSystem 填充）</summary>
        private List<Node2D> _activePets = new();

        /// <summary>已注册的宠物节点 → 宠物ID映射</summary>
        private Dictionary<Node2D, int> _petNodeToId = new();

        /// <summary>最近攻击时间（防止重复触发）</summary>
        private float _lastTriggerTime = -999f;
        private const float TRIGGER_COOLDOWN = 0.5f;

        /// <summary>
        /// 配合动画触发信号 — 参数：(攻击宠物ID, 配合宠物ID, 友谊等级, 动画名称)
        /// </summary>
        [Signal]
        public delegate void SynergyAnimTriggeredDelegateEventHandlerEventHandler(int attackerId, int buddyId, int friendshipLevel, string animName);

        public override void _Ready()
        {
            Instance = this;
            ConnectToPetSignal();
            RegisterExistingPets();
        }

        private void ConnectToPetSignal()
        {
            CallDeferred(nameof(ConnectSignalDeferred));
        }

        private void ConnectSignalDeferred()
        {
            var petAI = PetCombatAI.Instance;
            if (petAI != null)
            {
                petAI.PetAttacked += OnPetAttacked;
                GD.Print("[PetSynergySkillTrigger] Connected to PetCombatAI.PetAttacked");
            }
            else
            {
                var timer = new Timer { OneShot = true, WaitTime = 1.0f };
                timer.Timeout += () => ConnectSignalDeferred();
                AddChild(timer);
                timer.Start();
            }
        }

        /// <summary>
        /// 注册场上已有的宠物节点
        /// </summary>
        private void RegisterExistingPets()
        {
            // 由 PetCombatCompanionSystem 在宠物生成时调用 RegisterPet()
        }

        /// <summary>
        /// 注册一只宠物到配合系统
        /// </summary>
        public void RegisterPet(Node2D petNode, int petId)
        {
            if (!_activePets.Contains(petNode))
            {
                _activePets.Add(petNode);
                _petNodeToId[petNode] = petId;
                GD.Print($"[PetSynergySkillTrigger] Registered pet {petId}");
            }
        }

        /// <summary>
        /// 注销一只宠物
        /// </summary>
        public void UnregisterPet(Node2D petNode)
        {
            if (_activePets.Remove(petNode))
            {
                _petNodeToId.Remove(petNode);
            }
        }

        private void OnPetAttacked(Node2D enemy, int damage)
        {
            // 防重复触发
            if (Time.GetSingleton().TotalElapsed < _lastTriggerTime + TRIGGER_COOLDOWN)
                return;
            _lastTriggerTime = Time.GetSingleton().TotalElapsed;

            // 获取攻击方宠物ID（从 PetCombatAI.Instance 获取当前宠物）
            var petAI = PetCombatAI.Instance;
            if (petAI == null) return;

            // 通过场上宠物列表找到当前攻击的宠物
            int attackerId = GetCurrentAttackerPetId();
            if (attackerId < 0) return;

            TryTriggerSynergy(attackerId, enemy);
        }

        private int GetCurrentAttackerPetId()
        {
            // PetCombatAI 正在处理哪只宠物我们需要从 pet node 映射
            // 优先查找当前活跃的宠物节点
            foreach (var kvp in _petNodeToId)
            {
                if (kvp.Key != null && kvp.Key.IsInsideTree())
                    return kvp.Value;
            }
            return -1;
        }

        private void TryTriggerSynergy(int attackerId, Node2D enemy)
        {
            if (_activePets.Count < 2) return;

            var friendshipSystem = PetFriendshipSystem.Instance;
            if (friendshipSystem == null) return;

            // 找攻击方宠物的好友
            foreach (var buddyNode in _activePets)
            {
                if (buddyNode == null || !buddyNode.IsInsideTree()) continue;

                int buddyId = _petNodeToId.TryGetValue(buddyNode, out var id) ? id : -1;
                if (buddyId < 0 || buddyId == attackerId) continue;

                var friendship = friendshipSystem.GetFriendship(attackerId, buddyId);
                if (friendship == null || friendship.FriendshipLevel < 1) continue;

                TriggerSynergyAnimation(attackerId, buddyId, friendship.FriendshipLevel, buddyNode);
                break; // 只触发一次
            }
        }

        private void TriggerSynergyAnimation(int attackerId, int buddyId, int friendshipLevel, Node2D buddyNode)
        {
            var db = PetSynergySkillDatabase.Instance;
            var skill = db.GetSkillForFriendship(friendshipLevel);
            if (skill == null) return;

            string animName;
            if (friendshipLevel >= 16)
                animName = skill.HighTierEffectScene ?? skill.MediumGestureAnim;
            else if (friendshipLevel >= 6)
                animName = skill.MediumGestureAnim;
            else
                animName = skill.SmallGestureAnim;

            // 延迟触发配合动画
            var timer = new Timer
            {
                OneShot = true,
                WaitTime = skill.TimingOffset
            };
            timer.Timeout += () => PlaySynergyAnim(buddyNode, animName, friendshipLevel);
            AddChild(timer);
            timer.Start();

            EmitSignal(SignalName.SynergyAnimTriggered, attackerId, buddyId, friendshipLevel, animName);

            GD.Print($"[PetSynergySkillTrigger] Synergy triggered: pet {attackerId} + pet {buddyId} (friendship {friendshipLevel}) → {animName}");
        }

        private void PlaySynergyAnim(Node2D petNode, string animName, int friendshipLevel)
        {
            if (petNode == null || !petNode.IsInsideTree()) return;

            // 尝试播放动画
            if (petNode.HasMethod("Play"))
            {
                petNode.CallDeferred("Play", animName);
            }

            // 高级友谊触发屏幕特效
            if (friendshipLevel >= 16 && animName.EndsWith(".tscn"))
            {
                ShowSynergyVFX(petNode);
            }
        }

        private void ShowSynergyVFX(Node2D petNode)
        {
            // 在宠物位置创建协同光效
            var vfxPosition = petNode.GlobalPosition;
            // 通知 UI 系统显示特效（由 PetSynergyNotificationUI 处理）
            var ui = GetNodeOrNull("/root/Main/PetCombatCompanionUI");
            if (ui != null && ui.HasMethod("ShowPetSynergyEffect"))
            {
                ui.CallDeferred("ShowPetSynergyEffect", vfxPosition);
            }
        }
    }
}
