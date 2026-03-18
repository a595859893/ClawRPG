using Godot;
using System;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainUI - UI协调者
    /// 持有各UI模块引用，统一管理UI面板的切换与协调
    /// </summary>
    public partial class MainUI : Node
    {
        private Main _main;

        // UI 模块引用
        private UI.MainMenuUI _mainMenuUI;
        private UI.HUDUI _hudUI;
        private UI.InventoryUI _inventoryUI;
        private UI.MiniMapUI _miniMapUI;

        public override void _Ready()
        {
            InitializeModules();
        }

        public void Initialize(Main main)
        {
            _main = main;
            InitializeModules();
        }

        private void InitializeModules()
        {
            // 尝试获取已存在的UI模块实例
            _mainMenuUI = UI.MainMenuUI.Instance;
            _hudUI = UI.HUDUI.Instance;
            _inventoryUI = UI.InventoryUI.Instance;
            _miniMapUI = UI.MiniMapUI.Instance;

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

        #region 委托方法

        // ========== MainMenuUI 委托 ==========

        public void ShowMainMenu()
        {
            _mainMenuUI?.Show();
        }

        public void HideMainMenu()
        {
            _mainMenuUI?.Hide();
        }

        public void ToggleMainMenu()
        {
            _mainMenuUI?.Toggle();
        }

        // ========== HUDUI 委托 ==========

        public void ShowHUD()
        {
            _hudUI?.Show();
        }

        public void HideHUD()
        {
            _hudUI?.Hide();
        }

        public void ToggleHUD()
        {
            _hudUI?.Toggle();
        }

        public void RefreshHUD()
        {
            _hudUI?.Refresh();
        }

        // ========== InventoryUI 委托 ==========

        public void ShowInventory()
        {
            _inventoryUI?.Show();
        }

        public void HideInventory()
        {
            _inventoryUI?.Hide();
        }

        public void ToggleInventory()
        {
            _inventoryUI?.Toggle();
        }

        // ========== MiniMapUI 委托 ==========

        public void ShowMiniMap()
        {
            _miniMapUI?.Show();
        }

        public void HideMiniMap()
        {
            _miniMapUI?.Hide();
        }

        public void ToggleMiniMap()
        {
            _miniMapUI?.Toggle();
        }

        public void ToggleNPCMarkers()
        {
            _miniMapUI?.ToggleNPCMarkers();
        }

        public void ToggleEnemyMarkers()
        {
            _miniMapUI?.ToggleEnemyMarkers();
        }

        public void TogglePOIMarkers()
        {
            _miniMapUI?.TogglePOIMarkers();
        }

        #endregion

        #region 独立面板Toggle方法
        // 以下方法保留原有的独立面板切换逻辑

        public void ToggleRunesUI()
        {
            ToggleNode("CanvasLayer/RunesUI");
        }

        public void ToggleMeditationUI()
        {
            ToggleNode("CanvasLayer/MeditationUI");
        }

        public void ToggleQuestTracker()
        {
            ToggleNode("CanvasLayer/QuestTracker");
        }

        public void ToggleQuestGuide()
        {
            ToggleNode("CanvasLayer/QuestGuideUI");
        }

        public void ToggleMultiplayerUI()
        {
            ToggleNode("CanvasLayer/MultiplayerLobbyUI");
        }

        public void ToggleCombatRatingUI()
        {
            ToggleNode("CanvasLayer/CombatRatingUI");
        }

        public void ToggleWeaponMasteryUI()
        {
            ToggleNode("CanvasLayer/WeaponMasteryUI");
        }

        public void ToggleCounterAttackUI()
        {
            ToggleNode("CanvasLayer/CounterAttackUI");
        }

        public void ToggleMountUI()
        {
            ToggleNode("CanvasLayer/MountUI");
        }

        public void ToggleMountTrainingUI()
        {
            ToggleNode("CanvasLayer/MountTrainingUI");
        }

        public void ToggleSkillCooldownUI()
        {
            ToggleNode("CanvasLayer/SkillCooldownUI");
        }

        public void ToggleSkillSynergyUI()
        {
            ToggleNode("CanvasLayer/SkillSynergyUI");
        }

        public void ToggleSkillTreeResetUI()
        {
            ToggleNode("CanvasLayer/SkillTreeResetUI");
        }

        public void ToggleSkillMasteryUI()
        {
            ToggleNode("CanvasLayer/SkillMasteryUI");
        }

        public void ToggleConstellationUI()
        {
            ToggleNode("CanvasLayer/ConstellationUI");
        }

        public void ToggleProceduralStoryUI()
        {
            ToggleNode("CanvasLayer/ProceduralStoryUI");
        }

        public void ToggleMomentumUI()
        {
            ToggleNode("CanvasLayer/MomentumUI");
        }

        public void ToggleEnemyScalingUI()
        {
            ToggleNode("CanvasLayer/EnemyScalingUI");
        }

        public void ToggleWeatherUI()
        {
            ToggleNode("CanvasLayer/WeatherUI");
        }

        public void ToggleChoiceEventUI()
        {
            ToggleNode("CanvasLayer/ChoiceEventUI");
        }

        public void ToggleMusicCollectionUI()
        {
            ToggleNode("CanvasLayer/MusicCollectionUI");
        }

        public void ToggleGatheringUI()
        {
            ToggleNode("CanvasLayer/GatheringUI");
        }

        public void ToggleMonsterTamingUI()
        {
            ToggleNode("CanvasLayer/MonsterTamingUI");
        }

        public void ToggleDailyPuzzleUI()
        {
            ToggleNode("CanvasLayer/DailyPuzzleUI");
        }

        public void TogglePrestigeUI()
        {
            ToggleNode("CanvasLayer/PrestigeUI");
        }

        public void ToggleIdentificationUI()
        {
            ToggleNode("CanvasLayer/IdentificationUI");
        }

        public void ToggleTitleUI()
        {
            ToggleNode("CanvasLayer/TitleUI");
        }

        public void ToggleTitleCollectionUI()
        {
            ToggleNode("CanvasLayer/TitleCollectionUI");
        }

        public void ToggleBookmarkUI()
        {
            ToggleNode("CanvasLayer/BookmarkUI");
        }

        public void ToggleAutoBookmarkUI()
        {
            ToggleNode("CanvasLayer/AutoBookmarkUI");
        }

        public void ToggleEnhancementUI()
        {
            ToggleNode("CanvasLayer/EnhancementUI");
        }

        public void ToggleAutoPotionUI()
        {
            ToggleNode("CanvasLayer/AutoPotionUI");
        }

        public void ToggleEnchantmentUI()
        {
            ToggleNode("CanvasLayer/EnchantmentUI");
        }

        public void ToggleBossMechanicsUI()
        {
            ToggleNode("CanvasLayer/BossMechanicsUI");
        }

        public void ToggleCombatUI()
        {
            ToggleNode("CanvasLayer/CombatUI");
        }

        public void ToggleProceduralDungeonUI()
        {
            ToggleNode("CanvasLayer/ProceduralDungeonUI");
        }

        public void ToggleMythicPlusDungeonUI()
        {
            ToggleNode("CanvasLayer/MythicPlusDungeonUI");
        }

        public void ToggleArenaTournamentUI()
        {
            ToggleNode("CanvasLayer/ArenaTournamentUI");
        }

        public void ToggleFactionUI()
        {
            ToggleNode("CanvasLayer/FactionUI");
        }

        public void ToggleFishingUI()
        {
            ToggleNode("CanvasLayer/FishingUI");
        }

        public void ToggleAlchemyUI()
        {
            ToggleNode("CanvasLayer/AlchemyUI");
        }

        public void ToggleCookingUI()
        {
            ToggleNode("CanvasLayer/CookingUI");
        }

        public void ToggleMountCombatUI()
        {
            ToggleNode("CanvasLayer/MountCombatUI");
        }

        public void ToggleMountEvolutionUI()
        {
            ToggleNode("CanvasLayer/MountEvolutionUI");
        }

        public void ToggleMountEquipmentUI()
        {
            ToggleNode("CanvasLayer/MountEquipmentUI");
        }

        public void ToggleWorldEventUI()
        {
            ToggleNode("CanvasLayer/WorldEventUI");
        }

        public void ToggleGemUI()
        {
            ToggleNode("CanvasLayer/GemUI");
        }

        public void ToggleGemFusionUI()
        {
            ToggleNode("CanvasLayer/GemFusionUI");
        }

        public void ToggleCollectibleUI()
        {
            ToggleNode("CanvasLayer/CollectibleUI");
        }

        public void ToggleCostumeUI()
        {
            ToggleNode("CanvasLayer/CostumeUI");
        }

        public void TogglePetEquipmentUI()
        {
            ToggleNode("CanvasLayer/PetEquipmentUI");
        }

        public void TogglePetEquipmentEnhancementUI()
        {
            ToggleNode("CanvasLayer/PetEquipmentEnhancementUI");
        }

        public void ToggleRelicUI()
        {
            ToggleNode("CanvasLayer/RelicUI");
        }

        public void ToggleArenaColosseumUI()
        {
            ToggleNode("CanvasLayer/ArenaColosseumUI");
        }

        public void TogglePartyUI()
        {
            ToggleNode("CanvasLayer/PartyUI");
        }

        public void ToggleCoopSessionUI()
        {
            ToggleNode("CanvasLayer/CoopSessionUI");
        }

        public void ToggleEquipmentEnhancementUI()
        {
            ToggleNode("CanvasLayer/EquipmentEnhancementUI");
        }

        public void TogglePetEvolutionUI()
        {
            ToggleNode("CanvasLayer/PetEvolutionUI");
        }

        public void TogglePetTalentUI()
        {
            ToggleNode("CanvasLayer/PetTalentUI");
        }

        public void TogglePetAffectionUI()
        {
            ToggleNode("CanvasLayer/PetAffectionUI");
        }

        public void TogglePetInteractionUI()
        {
            ToggleNode("CanvasLayer/PetInteractionUI");
        }

        #endregion

        #region 辅助方法

        private void ToggleNode(string path)
        {
            var node = _main?.GetNodeOrNull<Control>(path);
            if (node != null)
            {
                node.Visible = !node.Visible;
                GD.Print($"{path} toggled: {node.Visible}");
            }
        }

        #endregion

        #region 数据持久化

        /// <summary>
        /// 导出所有UI状态数据
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
