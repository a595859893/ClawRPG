using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetBattleMemory
{
    /// <summary>
    /// 宠物战斗记忆引导系统（REQ-190）
    /// 宠物记住玩家最常用的 combo 起手，在玩家长时间未触发 combo 时主动引导
    /// </summary>
    public partial class PetBattleMemorySystem : BaseSystem
    {
        private static PetBattleMemorySystem _instance;
        public static PetBattleMemorySystem Instance => _instance;

        // 引导参数
        private const float GUIDE_TIMEOUT_SECONDS = 5f;   // 玩家超过 5 秒未触发 combo 时触发引导
        private const float BASE_GUIDE_PROBABILITY = 0.3f; // 基础引导概率

        // 运行时状态
        private float _lastPlayerSkillTime = 0f;           // 上次玩家使用技能的时间
        private bool _guideTriggeredThisCombat = false;     // 本场战斗是否已触发过引导
        private bool _firstSkillRecordedThisCombat = false; // 本场战斗是否已记录第一个技能
        private string _lastGuidedSkill = "";              // 上次引导的技能（防止重复）

        // VFX 节点路径（运行时查找）
        private Node _guideVfxNode = null;
        private const string GUIDE_VFX_SCENE = "res://Scenes/Systems/PetBattleMemory/PetBattleMemoryGuideVFX.tscn";

        // Signals
        public delegate void GuideTriggeredEventHandler(string petId, string skillId, string skillName);
        public event GuideTriggeredEventHandler OnGuideTriggered;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _lastPlayerSkillTime = Time.GetTicksMsec() / 1000f;
            SubscribeToSignals();
        }

        public override void _Process(double delta)
        {
            // 检查是否需要触发引导（宠物在场 + 超过 5 秒未触发 combo + 未触发过）
            CheckGuidanceTimeout();
        }

        private void SubscribeToSignals()
        {
            // 订阅战斗开始/结束信号（来自 CombatManager）
            var combatManager = GetNodeOrNull<Godot.Node>("/root/CombatManager");
            if (combatManager != null)
            {
                if (combatManager.HasSignal("CombatStarted"))
                    combatManager.Connect("CombatStarted", Callable.From(OnCombatStarted), (uint)ConnectFlags.Oneshot);
                if (combatManager.HasSignal("CombatEnded"))
                    combatManager.Connect("CombatEnded", Callable.From(OnCombatEnded), (uint)ConnectFlags.Oneshot);
            }

            // 订阅 ComboStarted 信号（来自 SkillComboSystem）
            var comboSystem = GetNodeOrNull<Godot.Node>("/root/SkillComboSystem");
            if (comboSystem != null)
            {
                if (comboSystem.HasSignal("ComboStarted"))
                    comboSystem.Connect("ComboStarted", Callable.From<string>(OnComboStarted), (uint)ConnectFlags.Oneshot);
            }

            // 订阅宠物离队/归队信号（来自 PetCombatCompanion）
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petCompanion != null)
            {
                if (petCompanion.HasSignal("PetDied"))
                    petCompanion.Connect("PetDied", Callable.From<Godot.Collections.Dictionary>(OnPetDied), (uint)ConnectFlags.Oneshot);
            }
        }

        #region Signal Handlers

        private void OnCombatStarted()
        {
            // 重置本场战斗状态
            _guideTriggeredThisCombat = false;
            _firstSkillRecordedThisCombat = false;
            _lastGuidedSkill = "";
        }

        private void OnCombatEnded()
        {
            // 战斗结束，隐藏引导 VFX
            HideGuideVfx();
        }

        private void OnComboStarted(string comboId)
        {
            // 玩家触发 combo，重置引导冷却
            _guideTriggeredThisCombat = false;
            _lastGuidedSkill = "";
        }

        private void OnPetDied(Godot.Collections.Dictionary petData)
        {
            // 宠物死亡时清除引导 VFX
            HideGuideVfx();
        }

        #endregion

        #region Public API

        /// <summary>
        /// 记录玩家的技能使用（由 PetCombatCompanionSystem.RecordPlayerAttack 调用）
        /// 同时用于：(1) 记录第一技能 (2) 更新最后技能时间戳
        /// </summary>
        public void RecordPlayerSkillUse(string petId, string skillId, string comboId)
        {
            float currentTime = Time.GetTicksMsec() / 1000f;
            _lastPlayerSkillTime = currentTime;

            // 记录本场战斗的第一个技能
            if (!_firstSkillRecordedThisCombat)
            {
                _firstSkillRecordedThisCombat = true;
                PetBattleMemoryDatabase.RecordFirstSkillUsed(petId, skillId, comboId ?? "");
            }
        }

        /// <summary>
        /// 获取宠物当前最可能的引导技能
        /// </summary>
        public string GetGuideableSkillForPet(string petId)
        {
            var entry = PetBattleMemoryDatabase.GetMostFrequentFirstSkill(petId);
            return entry?.FirstSkillUsed ?? null;
        }

        /// <summary>
        /// 是否有可引导的记忆
        /// </summary>
        public bool HasGuidableMemory(string petId)
        {
            return PetBattleMemoryDatabase.HasGuidableMemory(petId);
        }

        #endregion

        #region Guidance Logic

        private void CheckGuidanceTimeout()
        {
            // 跳过已触发过、本场战斗无记忆、或宠物不在场的情况
            if (_guideTriggeredThisCombat)
                return;

            // 获取当前宠物 ID
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petCompanion == null || !petCompanion.HasMethod("GetActivePetId"))
                return;

            string petId = petCompanion.Call("GetActivePetId").AsString();
            if (string.IsNullOrEmpty(petId))
                return;

            // 检查是否有可引导的记忆
            if (!HasGuidableMemory(petId))
                return;

            // 检查是否超过引导超时
            float currentTime = Time.GetTicksMsec() / 1000f;
            float elapsed = currentTime - _lastPlayerSkillTime;

            if (elapsed < GUIDE_TIMEOUT_SECONDS)
                return;

            // 计算引导概率（TimesObserved 越多概率越高，≥10次=100%）
            var entry = PetBattleMemoryDatabase.GetMostFrequentFirstSkill(petId);
            if (entry == null)
                return;

            float probability = Mathf.Clamp(BASE_GUIDE_PROBABILITY + (entry.TimesObserved - 1) * 0.07f, 0.1f, 1.0f);

            if (GD.Randf() > probability)
                return;

            // 触发引导
            TriggerGuidance(petId, entry);
        }

        private void TriggerGuidance(string petId, PetBattleMemoryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.FirstSkillUsed))
                return;

            // 防止重复引导同一个技能
            if (_lastGuidedSkill == entry.FirstSkillUsed && _guideTriggeredThisCombat)
                return;

            _guideTriggeredThisCombat = true;
            _lastGuidedSkill = entry.FirstSkillUsed;

            // 获取宠物节点（用于定位 VFX）
            var petNode = FindPetNode(petId);
            Vector2 vfxPosition = Vector2.Zero;
            if (petNode != null)
                vfxPosition = petNode.GlobalPosition + new Vector2(0, -60); // 宠物头顶

            // 显示引导 VFX
            ShowGuideVfx(petId, entry.FirstSkillUsed, entry.AssociatedComboId, vfxPosition);

            // 触发信号
            OnGuideTriggered?.Invoke(petId, entry.FirstSkillUsed, GetSkillDisplayName(entry.FirstSkillUsed));
        }

        private Node FindPetNode(string petId)
        {
            // 尝试多种方式查找宠物节点
            var tree = GetTree();
            if (tree == null) return null;

            // 方法1: 通过组查找
            var petNodes = tree.GetNodesInGroup("pets");
            foreach (var node in petNodes)
            {
                if (node.HasMethod("GetPetId"))
                {
                    var id = node.Call("GetPetId").AsString();
                    if (id == petId)
                        return node;
                }
            }

            // 方法2: 直接查找命名的宠物节点
            var namedPet = tree.GetNodeOrNull<Node>("/root/Main/Pet");
            if (namedPet != null)
                return namedPet;

            return tree.GetNodeOrNull<Node>("/root/Pet");
        }

        private string GetSkillDisplayName(string skillId)
        {
            // 尝试从 SkillDatabase 获取技能显示名
            var skillDb = GetNodeOrNull<Godot.Node>("/root/SkillDatabase");
            if (skillDb != null && skillDb.HasMethod("GetSkillName"))
            {
                return skillDb.Call("GetSkillName", skillId).AsString();
            }
            // 降级：返回 skillId
            return skillId;
        }

        #endregion

        #region VFX

        private void ShowGuideVfx(string petId, string skillId, string comboId, Vector2 worldPosition)
        {
            // 尝试加载 VFX 场景
            var vfxScene = GD.Load<PackedScene>(GUIDE_VFX_SCENE);
            if (vfxScene == null)
            {
                // 场景不存在，尝试使用程序化 VFX
                ShowProceduralGuideVfx(petId, skillId, worldPosition);
                return;
            }

            // 实例化 VFX 场景
            var vfx = vfxScene.Instantiate();
            var root = GetTree()?.Root;
            if (root != null)
            {
                root.AddChild(vfx);
                vfx.GlobalPosition = worldPosition;

                // 通知 VFX 显示内容
                if (vfx.HasMethod("ShowGuide"))
                    vfx.Call("ShowGuide", skillId, comboId, 2f);
            }
            else
            {
                vfx.QueueFree();
            }
        }

        private void ShowProceduralGuideVfx(string petId, string skillId, Vector2 worldPosition)
        {
            // 程序化引导 VFX（无美术资源时的降级方案）
            // 在宠物头顶显示一个淡入淡出的文字标签
            var label = new Label3D
            {
                Text = GetSkillDisplayName(skillId),
                WorldSpace = true,
                PixelSize = 0.05f,
                Position = worldPosition,
                Modulate = new Color(1f, 0.85f, 0.4f, 1f), // 淡金色
            };

            var root = GetTree()?.Root;
            if (root != null)
            {
                root.AddChild(label);

                // Tween 动画：淡入 → 停留 → 淡出
                var tween = CreateTween();
                tween.TweenProperty(label, "modulate:a", 0f, 0.5f)
                    .From(1f)
                    .SetDelay(1.5f);
                tween.TweenCallback(Callable.From(() => label.QueueFree()));
            }
            else
            {
                label.QueueFree();
            }
        }

        private void HideGuideVfx()
        {
            if (_guideVfxNode != null && IsInstanceValid(_guideVfxNode))
            {
                _guideVfxNode.QueueFree();
                _guideVfxNode = null;
            }
        }

        #endregion

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = PetBattleMemoryDatabase.ExportSaveData();
            // 转换为 Godot 兼容的 Dictionary
            var dict = new Godot.Collections.Dictionary();
            var entriesList = new Godot.Collections.Array();
            foreach (var entry in data.Entries)
            {
                var entryDict = new Godot.Collections.Dictionary();
                entryDict["pet_id"] = entry.PetId;
                entryDict["first_skill"] = entry.FirstSkillUsed;
                entryDict["combo_id"] = entry.AssociatedComboId;
                entryDict["times_observed"] = entry.TimesObserved;
                entryDict["last_observed"] = entry.LastObservedTicks;
                entriesList.Add(entryDict);
            }
            dict["entries"] = entriesList;

            var reincarnateList = new Godot.Collections.Array();
            foreach (var kvp in data.ReincarnatedMemoryMap)
            {
                var kvpDict = new Godot.Collections.Dictionary();
                kvpDict["new_pet_id"] = kvp.Key;
                kvpDict["inherited_from"] = kvp.Value;
                reincarnateList.Add(kvpDict);
            }
            dict["reincarnated_map"] = reincarnateList;

            return dict;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            var saveData = new PetBattleMemorySaveData();

            if (data.Contains("entries"))
            {
                foreach (Godot.Collections.Dictionary entryDict in (Godot.Collections.Array)data["entries"])
                {
                    var entry = new PetBattleMemoryEntry();
                    entry.PetId = entryDict.Contains("pet_id") ? entryDict["pet_id"].AsString() : "";
                    entry.FirstSkillUsed = entryDict.Contains("first_skill") ? entryDict["first_skill"].AsString() : "";
                    entry.AssociatedComboId = entryDict.Contains("combo_id") ? entryDict["combo_id"].AsString() : "";
                    entry.TimesObserved = entryDict.Contains("times_observed") ? entryDict["times_observed"].AsInt32() : 1;
                    entry.LastObservedTicks = entryDict.Contains("last_observed") ? entryDict["last_observed"].AsInt64() : DateTime.Now.Ticks;
                    saveData.Entries.Add(entry);
                }
            }

            if (data.Contains("reincarnated_map"))
            {
                foreach (Godot.Collections.Dictionary kvpDict in (Godot.Collections.Array)data["reincarnated_map"])
                {
                    string newId = kvpDict.Contains("new_pet_id") ? kvpDict["new_pet_id"].AsString() : "";
                    string fromId = kvpDict.Contains("inherited_from") ? kvpDict["inherited_from"].AsString() : "";
                    if (!string.IsNullOrEmpty(newId) && !string.IsNullOrEmpty(fromId))
                        saveData.ReincarnatedMemoryMap[newId] = fromId;
                }
            }

            PetBattleMemoryDatabase.ImportSaveData(saveData);
        }

        #endregion
    }
}
