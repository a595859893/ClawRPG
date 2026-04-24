using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// UIPanelManager - 面板生命周期管理器
    /// 统一管理所有UI面板的显示/隐藏状态，处理面板互斥逻辑
    /// </summary>
    public partial class UIPanelManager : BaseSystem
    {
        private Main _main;
        
        // 面板互斥组 - 某些面板不能同时显示
        private readonly HashSet<string> _exclusivePanelGroups = new HashSet<string>
        {
            "Inventory", "Character", "SkillTree", "Map"
        };
        
        // 当前显示的面板记录
        private readonly HashSet<string> _openPanels = new HashSet<string>();
        
        // 面板路径映射
        private readonly Dictionary<string, string> _panelPaths = new Dictionary<string, string>
        {
            { "RunesUI", "CanvasLayer/RunesUI" },
            { "MeditationUI", "CanvasLayer/MeditationUI" },
            { "QuestTracker", "CanvasLayer/QuestTracker" },
            { "QuestGuideUI", "CanvasLayer/QuestGuideUI" },
            { "MultiplayerLobbyUI", "CanvasLayer/MultiplayerLobbyUI" },
            { "CombatRatingUI", "CanvasLayer/CombatRatingUI" },
            { "WeaponMasteryUI", "CanvasLayer/WeaponMasteryUI" },
            { "CounterAttackUI", "CanvasLayer/CounterAttackUI" },
            { "MountUI", "CanvasLayer/MountUI" },
            { "MountTrainingUI", "CanvasLayer/MountTrainingUI" },
            { "SkillCooldownUI", "CanvasLayer/SkillCooldownUI" },
            { "SkillSynergyUI", "CanvasLayer/SkillSynergyUI" },
            { "SkillTreeResetUI", "CanvasLayer/SkillTreeResetUI" },
            { "SkillMasteryUI", "CanvasLayer/SkillMasteryUI" },
            { "ConstellationUI", "CanvasLayer/ConstellationUI" },
            { "ProceduralStoryUI", "CanvasLayer/ProceduralStoryUI" },
            { "MomentumUI", "CanvasLayer/MomentumUI" },
            { "EnemyScalingUI", "CanvasLayer/EnemyScalingUI" },
            { "WeatherUI", "CanvasLayer/WeatherUI" },
            { "ChoiceEventUI", "CanvasLayer/ChoiceEventUI" },
            { "MusicCollectionUI", "CanvasLayer/MusicCollectionUI" },
            { "GatheringUI", "CanvasLayer/GatheringUI" },
            { "MonsterTamingUI", "CanvasLayer/MonsterTamingUI" },
            { "DailyPuzzleUI", "CanvasLayer/DailyPuzzleUI" },
            { "PrestigeUI", "CanvasLayer/PrestigeUI" },
            { "IdentificationUI", "CanvasLayer/IdentificationUI" },
            { "TitleUI", "CanvasLayer/TitleUI" },
            { "TitleCollectionUI", "CanvasLayer/TitleCollectionUI" },
            { "BookmarkUI", "CanvasLayer/BookmarkUI" },
            { "AutoBookmarkUI", "CanvasLayer/AutoBookmarkUI" },
            { "EnhancementUI", "CanvasLayer/EnhancementUI" },
            { "AutoPotionUI", "CanvasLayer/AutoPotionUI" },
            { "EnchantmentUI", "CanvasLayer/EnchantmentUI" },
            { "BossMechanicsUI", "CanvasLayer/BossMechanicsUI" },
            { "CombatUI", "CanvasLayer/CombatUI" },
            { "ProceduralDungeonUI", "CanvasLayer/ProceduralDungeonUI" },
            { "MythicPlusDungeonUI", "CanvasLayer/MythicPlusDungeonUI" },
            { "ArenaTournamentUI", "CanvasLayer/ArenaTournamentUI" },
            { "FactionUI", "CanvasLayer/FactionUI" },
            { "FishingUI", "CanvasLayer/FishingUI" },
            { "AlchemyUI", "CanvasLayer/AlchemyUI" },
            { "CookingUI", "CanvasLayer/CookingUI" },
            { "MountCombatUI", "CanvasLayer/MountCombatUI" },
            { "MountEvolutionUI", "CanvasLayer/MountEvolutionUI" },
            { "MountEquipmentUI", "CanvasLayer/MountEquipmentUI" },
            { "WorldEventUI", "CanvasLayer/WorldEventUI" },
            { "GemUI", "CanvasLayer/GemUI" },
            { "GemFusionUI", "CanvasLayer/GemFusionUI" },
            { "CollectibleUI", "CanvasLayer/CollectibleUI" },
            { "CostumeUI", "CanvasLayer/CostumeUI" },
            { "PetEquipmentUI", "CanvasLayer/PetEquipmentUI" },
            { "PetEquipmentEnhancementUI", "CanvasLayer/PetEquipmentEnhancementUI" },
            { "RelicUI", "CanvasLayer/RelicUI" },
            { "ArenaColosseumUI", "CanvasLayer/ArenaColosseumUI" },
            { "PartyUI", "CanvasLayer/PartyUI" },
            { "CoopSessionUI", "CanvasLayer/CoopSessionUI" },
            { "EquipmentEnhancementUI", "CanvasLayer/EquipmentEnhancementUI" },
            { "PetEvolutionUI", "CanvasLayer/PetEvolutionUI" },
            { "PetTalentUI", "CanvasLayer/PetTalentUI" },
            { "PetAffectionUI", "CanvasLayer/PetAffectionUI" },
            { "PetInteractionUI", "CanvasLayer/PetInteractionUI" }
        };

        public override void _Ready()
        {
            // 默认不显示
        }

        /// <summary>
        /// 初始化面板管理器
        /// </summary>
        public void Initialize(Main main)
        {
            _main = main;
        }

        #region 面板切换接口

        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        public void TogglePanel(string panelName)
        {
            if (_panelPaths.TryGetValue(panelName, out string path))
            {
                ToggleNode(path);
                
                // 更新面板状态记录
                if (_openPanels.Contains(panelName))
                    _openPanels.Remove(panelName);
                else
                    _openPanels.Add(panelName);
            }
            else
            {
                // 对于未在映射中的面板，直接尝试切换
                ToggleNode($"CanvasLayer/{panelName}");
            }
        }

        /// <summary>
        /// 显示指定面板
        /// </summary>
        public void ShowPanel(string panelName)
        {
            if (_panelPaths.TryGetValue(panelName, out string path))
            {
                ShowNode(path);
                _openPanels.Add(panelName);
            }
            else
            {
                ShowNode($"CanvasLayer/{panelName}");
            }
        }

        /// <summary>
        /// 隐藏指定面板
        /// </summary>
        public void HidePanel(string panelName)
        {
            if (_panelPaths.TryGetValue(panelName, out string path))
            {
                HideNode(path);
                _openPanels.Remove(panelName);
            }
            else
            {
                HideNode($"CanvasLayer/{panelName}");
            }
        }

        /// <summary>
        /// 隐藏所有已打开的面板
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var panelName in _openPanels)
            {
                if (_panelPaths.TryGetValue(panelName, out string path))
                {
                    HideNode(path);
                }
                else
                {
                    HideNode($"CanvasLayer/{panelName}");
                }
            }
            _openPanels.Clear();
        }

        /// <summary>
        /// 检查面板是否正在显示
        /// </summary>
        public bool IsPanelOpen(string panelName)
        {
            return _openPanels.Contains(panelName);
        }

        /// <summary>
        /// 获取所有当前打开的面板
        /// </summary>
        public IReadOnlyCollection<string> GetOpenPanels()
        {
            return _openPanels;
        }

        #endregion

        #region 独立面板Toggle方法

        public void ToggleRunesUI() => TogglePanel("RunesUI");
        public void ToggleMeditationUI() => TogglePanel("MeditationUI");
        public void ToggleQuestTracker() => TogglePanel("QuestTracker");
        public void ToggleQuestGuide() => TogglePanel("QuestGuideUI");
        public void ToggleMultiplayerUI() => TogglePanel("MultiplayerLobbyUI");
        public void ToggleCombatRatingUI() => TogglePanel("CombatRatingUI");
        public void ToggleWeaponMasteryUI() => TogglePanel("WeaponMasteryUI");
        public void ToggleCounterAttackUI() => TogglePanel("CounterAttackUI");
        public void ToggleMountUI() => TogglePanel("MountUI");
        public void ToggleMountTrainingUI() => TogglePanel("MountTrainingUI");
        public void ToggleSkillCooldownUI() => TogglePanel("SkillCooldownUI");
        public void ToggleSkillSynergyUI() => TogglePanel("SkillSynergyUI");
        public void ToggleSkillTreeResetUI() => TogglePanel("SkillTreeResetUI");
        public void ToggleSkillMasteryUI() => TogglePanel("SkillMasteryUI");
        public void ToggleConstellationUI() => TogglePanel("ConstellationUI");
        public void ToggleProceduralStoryUI() => TogglePanel("ProceduralStoryUI");
        public void ToggleMomentumUI() => TogglePanel("MomentumUI");
        public void ToggleEnemyScalingUI() => TogglePanel("EnemyScalingUI");
        public void ToggleWeatherUI() => TogglePanel("WeatherUI");
        public void ToggleChoiceEventUI() => TogglePanel("ChoiceEventUI");
        public void ToggleMusicCollectionUI() => TogglePanel("MusicCollectionUI");
        public void ToggleGatheringUI() => TogglePanel("GatheringUI");
        public void ToggleMonsterTamingUI() => TogglePanel("MonsterTamingUI");
        public void ToggleDailyPuzzleUI() => TogglePanel("DailyPuzzleUI");
        public void TogglePrestigeUI() => TogglePanel("PrestigeUI");
        public void ToggleIdentificationUI() => TogglePanel("IdentificationUI");
        public void ToggleTitleUI() => TogglePanel("TitleUI");
        public void ToggleTitleCollectionUI() => TogglePanel("TitleCollectionUI");
        public void ToggleBookmarkUI() => TogglePanel("BookmarkUI");
        public void ToggleAutoBookmarkUI() => TogglePanel("AutoBookmarkUI");
        public void ToggleEnhancementUI() => TogglePanel("EnhancementUI");
        public void ToggleAutoPotionUI() => TogglePanel("AutoPotionUI");
        public void ToggleEnchantmentUI() => TogglePanel("EnchantmentUI");
        public void ToggleBossMechanicsUI() => TogglePanel("BossMechanicsUI");
        public void ToggleCombatUI() => TogglePanel("CombatUI");
        public void ToggleProceduralDungeonUI() => TogglePanel("ProceduralDungeonUI");
        public void ToggleMythicPlusDungeonUI() => TogglePanel("MythicPlusDungeonUI");
        public void ToggleArenaTournamentUI() => TogglePanel("ArenaTournamentUI");
        public void ToggleFactionUI() => TogglePanel("FactionUI");
        public void ToggleFishingUI() => TogglePanel("FishingUI");
        public void ToggleAlchemyUI() => TogglePanel("AlchemyUI");
        public void ToggleCookingUI() => TogglePanel("CookingUI");
        public void ToggleMountCombatUI() => TogglePanel("MountCombatUI");
        public void ToggleMountEvolutionUI() => TogglePanel("MountEvolutionUI");
        public void ToggleMountEquipmentUI() => TogglePanel("MountEquipmentUI");
        public void ToggleWorldEventUI() => TogglePanel("WorldEventUI");
        public void ToggleGemUI() => TogglePanel("GemUI");
        public void ToggleGemFusionUI() => TogglePanel("GemFusionUI");
        public void ToggleCollectibleUI() => TogglePanel("CollectibleUI");
        public void ToggleCostumeUI() => TogglePanel("CostumeUI");
        public void TogglePetEquipmentUI() => TogglePanel("PetEquipmentUI");
        public void TogglePetEquipmentEnhancementUI() => TogglePanel("PetEquipmentEnhancementUI");
        public void ToggleRelicUI() => TogglePanel("RelicUI");
        public void ToggleArenaColosseumUI() => TogglePanel("ArenaColosseumUI");
        public void TogglePartyUI() => TogglePanel("PartyUI");
        public void ToggleCoopSessionUI() => TogglePanel("CoopSessionUI");
        public void ToggleEquipmentEnhancementUI() => TogglePanel("EquipmentEnhancementUI");
        public void TogglePetEvolutionUI() => TogglePanel("PetEvolutionUI");
        public void TogglePetTalentUI() => TogglePanel("PetTalentUI");
        public void TogglePetAffectionUI() => TogglePanel("PetAffectionUI");
        public void TogglePetInteractionUI() => TogglePanel("PetInteractionUI");

        #endregion

        #region 底层节点操作

        private void ToggleNode(string path)
        {
            var node = _main?.GetNodeOrNull<Control>(path);
            if (node != null)
            {
                node.Visible = !node.Visible;
                GD.Print($"{path} toggled: {node.Visible}");
            }
        }

        private void ShowNode(string path)
        {
            var node = _main?.GetNodeOrNull<Control>(path);
            if (node != null)
            {
                node.Visible = true;
                GD.Print($"{path} shown");
            }
        }

        private void HideNode(string path)
        {
            var node = _main?.GetNodeOrNull<Control>(path);
            if (node != null)
            {
                node.Visible = false;
                GD.Print($"{path} hidden");
            }
        }

        #endregion

        #region 数据持久化

        /// <summary>
        /// 导出面板状态数据
        /// </summary>
        public Godot.Collections.Dictionary ExportSaveData()
        {
            var data = new Godot.Collections.Dictionary();
            var openPanelsList = new Godot.Collections.Array();
            
            foreach (var panel in _openPanels)
            {
                openPanelsList.Add(panel);
            }
            
            data["OpenPanels"] = openPanelsList;
            return data;
        }

        /// <summary>
        /// 导入面板状态数据
        /// </summary>
        public void ImportSaveData(Godot.Collections.Dictionary data)
        {
            if (data == null || !data.ContainsKey("OpenPanels")) return;

            // 先隐藏所有面板
            HideAllPanels();

            // 恢复保存的状态
            var openPanelsList = (Godot.Collections.Array)data["OpenPanels"];
            foreach (string panelName in openPanelsList)
            {
                ShowPanel(panelName);
            }
        }

        #endregion
    }
}
