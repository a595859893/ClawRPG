using Godot;
using System;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// UIController - UI交互逻辑控制器
    /// 处理UI事件、用户交互、面板间数据同步
    /// </summary>
    public partial class UIController : Node
    {
        private Main _main;
        
        // UI 模块引用
        private MainMenuUI _mainMenuUI;
        private HUDUI _hudUI;
        private InventoryUI _inventoryUI;
        private MiniMapUI _miniMapUI;
        
        // 面板管理器引用
        private UIPanelManager _panelManager;

        public override void _Ready()
        {
            // 默认不显示
        }

        /// <summary>
        /// 初始化UI控制器
        /// </summary>
        public void Initialize(Main main, UIPanelManager panelManager)
        {
            _main = main;
            _panelManager = panelManager;
            InitializeModules();
        }

        private void InitializeModules()
        {
            // 尝试获取已存在的UI模块实例
            _mainMenuUI = MainMenuUI.Instance;
            _hudUI = HUDUI.Instance;
            _inventoryUI = InventoryUI.Instance;
            _miniMapUI = MiniMapUI.Instance;

            // 如果场景中已有这些节点，初始化它们
            if (_mainMenuUI != null && _main != null)
                _mainMenuUI.Initialize(_main);

            if (_hudUI != null && _main != null)
                _hudUI.Initialize(_main);

            if (_inventoryUI != null && _main != null)
                _inventoryUI.Initialize(_main);

            if (_miniMapUI != null && _main != null)
                _miniMapUI.Initialize(_main);
        }

        #region 模块显示/隐藏接口

        /// <summary>
        /// 显示主菜单
        /// </summary>
        public void ShowMainMenu()
        {
            _mainMenuUI?.Show();
        }

        /// <summary>
        /// 隐藏主菜单
        /// </summary>
        public void HideMainMenu()
        {
            _mainMenuUI?.Hide();
        }

        /// <summary>
        /// 切换主菜单显示状态
        /// </summary>
        public void ToggleMainMenu()
        {
            _mainMenuUI?.Toggle();
        }

        /// <summary>
        /// 显示HUD
        /// </summary>
        public void ShowHUD()
        {
            _hudUI?.Show();
        }

        /// <summary>
        /// 隐藏HUD
        /// </summary>
        public void HideHUD()
        {
            _hudUI?.Hide();
        }

        /// <summary>
        /// 切换HUD显示状态
        /// </summary>
        public void ToggleHUD()
        {
            _hudUI?.Toggle();
        }

        /// <summary>
        /// 刷新HUD显示
        /// </summary>
        public void RefreshHUD()
        {
            _hudUI?.Refresh();
        }

        /// <summary>
        /// 显示背包
        /// </summary>
        public void ShowInventory()
        {
            _inventoryUI?.Show();
        }

        /// <summary>
        /// 隐藏背包
        /// </summary>
        public void HideInventory()
        {
            _inventoryUI?.Hide();
        }

        /// <summary>
        /// 切换背包显示状态
        /// </summary>
        public void ToggleInventory()
        {
            _inventoryUI?.Toggle();
        }

        /// <summary>
        /// 显示小地图
        /// </summary>
        public void ShowMiniMap()
        {
            _miniMapUI?.Show();
        }

        /// <summary>
        /// 隐藏小地图
        /// </summary>
        public void HideMiniMap()
        {
            _miniMapUI?.Hide();
        }

        /// <summary>
        /// 切换小地图显示状态
        /// </summary>
        public void ToggleMiniMap()
        {
            _miniMapUI?.Toggle();
        }

        /// <summary>
        /// 切换NPC标记显示
        /// </summary>
        public void ToggleNPCMarkers()
        {
            _miniMapUI?.ToggleNPCMarkers();
        }

        /// <summary>
        /// 切换敌人标记显示
        /// </summary>
        public void ToggleEnemyMarkers()
        {
            _miniMapUI?.ToggleEnemyMarkers();
        }

        /// <summary>
        /// 切换兴趣点标记显示
        /// </summary>
        public void TogglePOIMarkers()
        {
            _miniMapUI?.TogglePOIMarkers();
        }

        #endregion

        #region 面板协调逻辑

        /// <summary>
        /// 当进入战斗时调用
        /// </summary>
        public void OnCombatStart()
        {
            // 战斗时隐藏某些UI
            _miniMapUI?.Hide();
        }

        /// <summary>
        /// 当结束战斗时调用
        /// </summary>
        public void OnCombatEnd()
        {
            // 战斗结束后恢复小地图
            _miniMapUI?.Show();
        }

        /// <summary>
        /// 当打开背包时调用
        /// </summary>
        public void OnInventoryOpened()
        {
            // 打开背包时可以暂停游戏或做其他协调
            GD.Print("Inventory opened - UI coordinated");
        }

        /// <summary>
        /// 当关闭背包时调用
        /// </summary>
        public void OnInventoryClosed()
        {
            GD.Print("Inventory closed - UI coordinated");
        }

        /// <summary>
        /// 当打开主菜单时调用
        /// </summary>
        public void OnMainMenuOpened()
        {
            // 主菜单打开时可能需要暂停游戏
            GD.Print("Main menu opened - game may pause");
        }

        /// <summary>
        /// 当关闭主菜单时调用
        /// </summary>
        public void OnMainMenuClosed()
        {
            GD.Print("Main menu closed - game may resume");
        }

        #endregion

        #region 跨面板数据同步

        /// <summary>
        /// 当玩家数据更新时，刷新所有相关UI
        /// </summary>
        public void OnPlayerDataChanged()
        {
            _hudUI?.Refresh();
            _inventoryUI?.Refresh();
            _miniMapUI?.Refresh();
        }

        /// <summary>
        /// 当物品变化时通知相关面板
        /// </summary>
        public void OnItemChanged()
        {
            _hudUI?.Refresh();
            _inventoryUI?.Refresh();
        }

        /// <summary>
        /// 当技能变化时通知相关面板
        /// </summary>
        public void OnSkillChanged()
        {
            _hudUI?.Refresh();
        }

        #endregion

        #region 面板Toggle转发

        // 转发所有面板Toggle到UIPanelManager
        public void ToggleRunesUI() => _panelManager?.ToggleRunesUI();
        public void ToggleMeditationUI() => _panelManager?.ToggleMeditationUI();
        public void ToggleQuestTracker() => _panelManager?.ToggleQuestTracker();
        public void ToggleQuestGuide() => _panelManager?.ToggleQuestGuide();
        public void ToggleMultiplayerUI() => _panelManager?.ToggleMultiplayerUI();
        public void ToggleCombatRatingUI() => _panelManager?.ToggleCombatRatingUI();
        public void ToggleWeaponMasteryUI() => _panelManager?.ToggleWeaponMasteryUI();
        public void ToggleCounterAttackUI() => _panelManager?.ToggleCounterAttackUI();
        public void ToggleMountUI() => _panelManager?.ToggleMountUI();
        public void ToggleMountTrainingUI() => _panelManager?.ToggleMountTrainingUI();
        public void ToggleSkillCooldownUI() => _panelManager?.ToggleSkillCooldownUI();
        public void ToggleSkillSynergyUI() => _panelManager?.ToggleSkillSynergyUI();
        public void ToggleSkillTreeResetUI() => _panelManager?.ToggleSkillTreeResetUI();
        public void ToggleSkillMasteryUI() => _panelManager?.ToggleSkillMasteryUI();
        public void ToggleConstellationUI() => _panelManager?.ToggleConstellationUI();
        public void ToggleProceduralStoryUI() => _panelManager?.ToggleProceduralStoryUI();
        public void ToggleMomentumUI() => _panelManager?.ToggleMomentumUI();
        public void ToggleEnemyScalingUI() => _panelManager?.ToggleEnemyScalingUI();
        public void ToggleWeatherUI() => _panelManager?.ToggleWeatherUI();
        public void ToggleChoiceEventUI() => _panelManager?.ToggleChoiceEventUI();
        public void ToggleMusicCollectionUI() => _panelManager?.ToggleMusicCollectionUI();
        public void ToggleGatheringUI() => _panelManager?.ToggleGatheringUI();
        public void ToggleMonsterTamingUI() => _panelManager?.ToggleMonsterTamingUI();
        public void ToggleDailyPuzzleUI() => _panelManager?.ToggleDailyPuzzleUI();
        public void TogglePrestigeUI() => _panelManager?.TogglePrestigeUI();
        public void ToggleIdentificationUI() => _panelManager?.ToggleIdentificationUI();
        public void ToggleTitleUI() => _panelManager?.ToggleTitleUI();
        public void ToggleTitleCollectionUI() => _panelManager?.ToggleTitleCollectionUI();
        public void ToggleBookmarkUI() => _panelManager?.ToggleBookmarkUI();
        public void ToggleAutoBookmarkUI() => _panelManager?.ToggleAutoBookmarkUI();
        public void ToggleEnhancementUI() => _panelManager?.ToggleEnhancementUI();
        public void ToggleAutoPotionUI() => _panelManager?.ToggleAutoPotionUI();
        public void ToggleEnchantmentUI() => _panelManager?.ToggleEnchantmentUI();
        public void ToggleBossMechanicsUI() => _panelManager?.ToggleBossMechanicsUI();
        public void ToggleCombatUI() => _panelManager?.ToggleCombatUI();
        public void ToggleProceduralDungeonUI() => _panelManager?.ToggleProceduralDungeonUI();
        public void ToggleMythicPlusDungeonUI() => _panelManager?.ToggleMythicPlusDungeonUI();
        public void ToggleArenaTournamentUI() => _panelManager?.ToggleArenaTournamentUI();
        public void ToggleFactionUI() => _panelManager?.ToggleFactionUI();
        public void ToggleFishingUI() => _panelManager?.ToggleFishingUI();
        public void ToggleAlchemyUI() => _panelManager?.ToggleAlchemyUI();
        public void ToggleCookingUI() => _panelManager?.ToggleCookingUI();
        public void ToggleMountCombatUI() => _panelManager?.ToggleMountCombatUI();
        public void ToggleMountEvolutionUI() => _panelManager?.ToggleMountEvolutionUI();
        public void ToggleMountEquipmentUI() => _panelManager?.ToggleMountEquipmentUI();
        public void ToggleWorldEventUI() => _panelManager?.ToggleWorldEventUI();
        public void ToggleGemUI() => _panelManager?.ToggleGemUI();
        public void ToggleGemFusionUI() => _panelManager?.ToggleGemFusionUI();
        public void ToggleCollectibleUI() => _panelManager?.ToggleCollectibleUI();
        public void ToggleCostumeUI() => _panelManager?.ToggleCostumeUI();
        public void TogglePetEquipmentUI() => _panelManager?.TogglePetEquipmentUI();
        public void TogglePetEquipmentEnhancementUI() => _panelManager?.TogglePetEquipmentEnhancementUI();
        public void ToggleRelicUI() => _panelManager?.ToggleRelicUI();
        public void ToggleArenaColosseumUI() => _panelManager?.ToggleArenaColosseumUI();
        public void TogglePartyUI() => _panelManager?.TogglePartyUI();
        public void ToggleCoopSessionUI() => _panelManager?.ToggleCoopSessionUI();
        public void ToggleEquipmentEnhancementUI() => _panelManager?.ToggleEquipmentEnhancementUI();
        public void TogglePetEvolutionUI() => _panelManager?.TogglePetEvolutionUI();
        public void TogglePetTalentUI() => _panelManager?.TogglePetTalentUI();
        public void TogglePetAffectionUI() => _panelManager?.TogglePetAffectionUI();
        public void TogglePetInteractionUI() => _panelManager?.TogglePetInteractionUI();

        #endregion

        #region 数据持久化

        /// <summary>
        /// 导出UI状态数据
        /// </summary>
        public Godot.Collections.Dictionary ExportSaveData()
        {
            var data = new Godot.Collections.Dictionary();

            if (_mainMenuUI != null)
                data["MainMenuUI"] = _mainMenuUI.ExportSaveData();
            if (_hudUI != null)
                data["HUDUI"] = _hudUI.ExportSaveData();
            if (_inventoryUI != null)
                data["InventoryUI"] = _inventoryUI.ExportSaveData();
            if (_miniMapUI != null)
                data["MiniMapUI"] = _miniMapUI.ExportSaveData();

            return data;
        }

        /// <summary>
        /// 导入UI状态数据
        /// </summary>
        public void ImportSaveData(Godot.Collections.Dictionary data)
        {
            if (data == null) return;

            if (data.Contains("MainMenuUI") && _mainMenuUI != null)
                _mainMenuUI.ImportSaveData((Godot.Collections.Dictionary)data["MainMenuUI"]);
            if (data.Contains("HUDUI") && _hudUI != null)
                _hudUI.ImportSaveData((Godot.Collections.Dictionary)data["HUDUI"]);
            if (data.Contains("InventoryUI") && _inventoryUI != null)
                _inventoryUI.ImportSaveData((Godot.Collections.Dictionary)data["InventoryUI"]);
            if (data.Contains("MiniMapUI") && _miniMapUI != null)
                _miniMapUI.ImportSaveData((Godot.Collections.Dictionary)data["MiniMapUI"]);
        }

        #endregion
    }
}
