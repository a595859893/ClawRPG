using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Combat;
using ClawRPG.Scripts.Crafting;
using ClawRPG.Scripts.Systems.PetMimicry;
using ClawRPG.Scripts.Systems.EventCardPool;
using Framework;
using CombatSkillCooldownSystem = global::CombatSkillCooldownSystem;
using SummonSystem = global::ClawRPG.Scripts.SummonSystem;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// MainSaveLoadSystem - 负责游戏数据的导入导出
    /// </summary>
    public partial class MainSaveLoadSystem : BaseSystem
    {
        private Main _main;
        private GameStateManager _gameStateManager;
        private SystemInitializationManager _systemInitializationManager;
        private UIManager _uiManager;

        // 战斗相关系统引用
        private CombatSkillCooldownSystem _combatSkillCooldownSystem;
        private CombatStatusSystem _combatStatusSystem;
        private CraftingSystem _craftingSystem;
        private SummonSystem _summonSystem;

        // Pet Mimicry system reference - REQ-142-07
        private PetMimicryData _petMimicryData;

        // Event Card Pool system reference - REQ-118
        private EventCardPoolData _eventCardPoolData;

        // Pet Performance system reference - REQ-148
        private PetPerformanceData _petPerformanceData;
        
        // Combo Forget system reference - REQ-154
        private ComboForgetData _comboForgetData;

        // Combat Log system reference - REQ-001
        private CombatLogSystem _combatLogSystem;

        public void Initialize(Main main)
        {
            _main = main;
        }

        public void SetManagers(GameStateManager gameStateManager, SystemInitializationManager systemInitManager, UIManager uiManager)
        {
            _gameStateManager = gameStateManager;
            _systemInitializationManager = systemInitManager;
            _uiManager = uiManager;
            
            // 获取战斗相关系统引用
            _combatSkillCooldownSystem = _main.GetNodeOrNull<CombatSkillCooldownSystem>("CombatSkillCooldownSystem");
            _combatStatusSystem = _main.GetNodeOrNull<CombatStatusSystem>("CombatStatusSystem");
            _craftingSystem = _main.GetNodeOrNull<CraftingSystem>("CraftingSystem");
            _summonSystem = _main.GetNodeOrNull<SummonSystem>("SummonSystem");

            // Pet Mimicry system - REQ-142-07
            _petMimicryData = PetMimicryData.Instance;

            // Event Card Pool system - REQ-118
            _eventCardPoolData = EventCardPoolData.Instance;

            // Pet Performance system - REQ-148
            _petPerformanceData = PetPerformanceData.Instance;
            
            // Combo Forget system - REQ-154
            _comboForgetData = ComboForgetData.Instance;

            // Combat Log system - REQ-001
            _combatLogSystem = CombatLogSystem.Instance;
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        public void LoadGameData()
        {
            var mainSaveLoad = _main.GetNodeOrNull<MainSaveLoad>("MainSaveLoad");
            mainSaveLoad?.LoadGameData();
        }

        /// <summary>
        /// 导出所有游戏数据（供存档使用）
        /// </summary>
        public Dictionary ExportAllData()
        {
            var allData = new Dictionary<string, object>();

            if (_gameStateManager != null)
            {
                allData["gameState"] = _gameStateManager.ExportSaveData();
            }

            if (_systemInitializationManager != null)
            {
                allData["systemInit"] = _systemInitializationManager.ExportSaveData();
            }

            if (_uiManager != null)
            {
                allData["ui"] = _uiManager.ExportSaveData();
            }

            var saveLoadManager = _main.GetNodeOrNull<SaveLoadManager>("SaveLoadManager");
            if (saveLoadManager != null)
            {
                allData["saveLoad"] = saveLoadManager.ExportSaveData();
            }

            // 战斗系统持久化
            if (_combatSkillCooldownSystem != null)
            {
                allData["combatSkillCooldown"] = _combatSkillCooldownSystem.ExportSaveData();
            }

            if (_combatStatusSystem != null)
            {
                allData["combatStatus"] = _combatStatusSystem.ExportSaveData();
            }

            if (_craftingSystem != null)
            {
                allData["crafting"] = _craftingSystem.ExportSaveData();
            }

            if (_summonSystem != null)
            {
                allData["summon"] = _summonSystem.ExportSaveData();
            }

            // Pet Mimicry persistence - REQ-142-07
            if (_petMimicryData != null)
            {
                allData["petMimicry"] = _petMimicryData.ExportSaveData();
            }

            // Event Card Pool persistence - REQ-118
            if (_eventCardPoolData != null)
            {
                allData["eventCardPool"] = _eventCardPoolData.ExportSaveData();
            }

            // Pet Performance persistence - REQ-148
            if (_petPerformanceData != null)
            {
                allData["petPerformance"] = _petPerformanceData.ExportSaveData();
            }

            // Combo Forget persistence - REQ-154
            if (_comboForgetData != null)
            {
                allData["comboForget"] = _comboForgetData.ExportSaveData();
            }

            // Combat Log persistence - REQ-001
            if (_combatLogSystem != null)
            {
                allData["combatLog"] = _combatLogSystem.ExportSaveData();
            }

            return allData;
        }

        /// <summary>
        /// 导入所有游戏数据（供读档使用）
        /// </summary>
        public void ImportAllData(Dictionary data)
        {
            if (data == null) return;

            if (data.Contains("gameState"))
            {
                _gameStateManager?.ImportSaveData(data["gameState"] as Dictionary);
            }

            if (data.Contains("systemInit"))
            {
                _systemInitializationManager?.ImportSaveData(data["systemInit"] as Dictionary);
            }

            if (data.Contains("ui"))
            {
                _uiManager?.ImportSaveData(data["ui"] as Dictionary);
            }

            if (data.Contains("saveLoad"))
            {
                var saveLoadManager = _main.GetNodeOrNull<SaveLoadManager>("SaveLoadManager");
                saveLoadManager?.ImportSaveData(data["saveLoad"] as Dictionary);
            }

            // 战斗系统持久化
            if (data.Contains("combatSkillCooldown"))
            {
                _combatSkillCooldownSystem?.ImportSaveData(data["combatSkillCooldown"] as Dictionary);
            }

            if (data.Contains("combatStatus"))
            {
                _combatStatusSystem?.ImportSaveData(data["combatStatus"] as Dictionary);
            }

            if (data.Contains("crafting"))
            {
                _craftingSystem?.ImportSaveData(data["crafting"] as Dictionary);
            }

            if (data.Contains("summon"))
            {
                _summonSystem?.ImportSaveData(data["summon"] as Dictionary);
            }

            // Pet Mimicry persistence - REQ-142-07
            if (data.Contains("petMimicry"))
            {
                _petMimicryData?.ImportSaveData(data["petMimicry"] as Dictionary);
            }

            // Event Card Pool persistence - REQ-118
            if (data.Contains("eventCardPool"))
            {
                _eventCardPoolData?.ImportSaveData(data["eventCardPool"] as Dictionary);
            }

            // Pet Performance persistence - REQ-148
            if (data.Contains("petPerformance"))
            {
                _petPerformanceData?.ImportSaveData(data["petPerformance"] as Dictionary);
            }

            // Combo Forget persistence - REQ-154
            if (data.Contains("comboForget"))
            {
                _comboForgetData?.ImportSaveData(data["comboForget"] as Dictionary);
            }

            // Combat Log persistence - REQ-001
            if (data.Contains("combatLog"))
            {
                _combatLogSystem?.ImportSaveData(data["combatLog"] as Dictionary);
            }
        }

        /// <summary>
        /// 重写基类的导出保存数据方法
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return ExportAllData();
        }

        /// <summary>
        /// 重写基类的导入保存数据方法
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            ImportAllData(data);
        }
    }
}
