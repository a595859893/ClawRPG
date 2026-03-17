using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;

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

        public void Initialize(Main main)
        {
            _main = main;
        }

        public void SetManagers(GameStateManager gameStateManager, SystemInitializationManager systemInitManager, UIManager uiManager)
        {
            _gameStateManager = gameStateManager;
            _systemInitializationManager = systemInitManager;
            _uiManager = uiManager;
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
            var allData = new Dictionary();

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
        }

        /// <summary>
        /// 重写基类的导出保存数据方法
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return ExportAllData();
        }

        /// <summary>
        /// 重写基类的导入保存数据方法
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            ImportAllData(data);
        }
    }
}
