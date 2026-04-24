using Godot;
using System;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainUI - UI协调者
    /// 持有UIPanelManager和UIController，统一管理UI面板的切换与协调
    /// </summary>
    public partial class MainUI : Node
    {
        private Main _main;

        // UI管理器
        private UI.UIPanelManager _panelManager;
        private UI.UIController _uiController;

        public override void _Ready()
        {
            InitializeManagers();
        }

        public void Initialize(Main main)
        {
            _main = main;
            InitializeManagers();
        }

        private void InitializeManagers()
        {
            // 获取或创建UIPanelManager
            _panelManager = GetNodeOrNull<UI.UIPanelManager>("UIPanelManager");
            if (_panelManager == null)
            {
                _panelManager = new UI.UIPanelManager();
                AddChild(_panelManager);
            }
            _panelManager.Initialize(_main);

            // 获取或创建UIController
            _uiController = GetNodeOrNull<UI.UIController>("UIController");
            if (_uiController == null)
            {
                _uiController = new UI.UIController();
                AddChild(_uiController);
            }
            _uiController.Initialize(_main, _panelManager);
        }

        #region 委托方法 - 转发到UIController

        // ========== MainMenuUI 委托 ==========

        public void ShowMainMenu()
        {
            _uiController?.ShowMainMenu();
        }

        public void HideMainMenu()
        {
            _uiController?.HideMainMenu();
        }

        public void ToggleMainMenu()
        {
            _uiController?.ToggleMainMenu();
        }

        // ========== HUDUI 委托 ==========

        public void ShowHUD()
        {
            _uiController?.ShowHUD();
        }

        public void HideHUD()
        {
            _uiController?.HideHUD();
        }

        public void ToggleHUD()
        {
            _uiController?.ToggleHUD();
        }

        public void RefreshHUD()
        {
            _uiController?.RefreshHUD();
        }

        // ========== InventoryUI 委托 ==========

        public void ShowInventory()
        {
            _uiController?.ShowInventory();
        }

        public void HideInventory()
        {
            _uiController?.HideInventory();
        }

        public void ToggleInventory()
        {
            _uiController?.ToggleInventory();
        }

        // ========== MiniMapUI 委托 ==========

        public void ShowMiniMap()
        {
            _uiController?.ShowMiniMap();
        }

        public void HideMiniMap()
        {
            _uiController?.HideMiniMap();
        }

        public void ToggleMiniMap()
        {
            _uiController?.ToggleMiniMap();
        }

        public void ToggleNPCMarkers()
        {
            _uiController?.ToggleNPCMarkers();
        }

        public void ToggleEnemyMarkers()
        {
            _uiController?.ToggleEnemyMarkers();
        }

        public void TogglePOIMarkers()
        {
            _uiController?.TogglePOIMarkers();
        }

        #endregion

        #region 独立面板Toggle方法 - 转发到UIPanelManager

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
        /// 导出所有UI状态数据
        /// </summary>
        public Godot.Collections.Dictionary ExportSaveData()
        {
            var data = new Godot.Collections.Dictionary();

            // 导出UIController数据
            if (_uiController != null)
                data["UIController"] = _uiController.ExportSaveData();

            // 导出UIPanelManager数据
            if (_panelManager != null)
                data["UIPanelManager"] = _panelManager.ExportSaveData();

            return data;
        }

        /// <summary>
        /// 导入UI状态数据
        /// </summary>
        public void ImportSaveData(Godot.Collections.Dictionary data)
        {
            if (data == null) return;

            if (data.ContainsKey("UIController") && _uiController != null)
                _uiController.ImportSaveData((Godot.Collections.Dictionary)data["UIController"]);

            if (data.ContainsKey("UIPanelManager") && _panelManager != null)
                _panelManager.ImportSaveData((Godot.Collections.Dictionary)data["UIPanelManager"]);
        }

        #endregion
    }
}
