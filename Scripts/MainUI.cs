using Godot;
using System;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainUI - Handles all UI panel toggles and visibility management
    /// </summary>
    public partial class MainUI : Node
    {
        private Main _main;
        
        public MainUI()
        {
        }
        
        public void Initialize(Main main)
        {
            _main = main;
        }
        
        /// <summary>
        /// Toggle Runes UI
        /// </summary>
        public void ToggleRunesUI()
        {
            var runesUI = _main?.GetNodeOrNull<Control>("CanvasLayer/RunesUI");
            if (runesUI != null)
            {
                runesUI.Visible = !runesUI.Visible;
                GD.Print("Runes UI toggled: " + runesUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Meditation UI
        /// </summary>
        public void ToggleMeditationUI()
        {
            var meditationUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MeditationUI");
            if (meditationUI != null)
            {
                meditationUI.Visible = !meditationUI.Visible;
                GD.Print("Meditation UI toggled: " + meditationUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Quest Tracker
        /// </summary>
        public void ToggleQuestTracker()
        {
            var questTracker = _main?.GetNodeOrNull<Control>("CanvasLayer/QuestTracker");
            if (questTracker != null)
            {
                questTracker.Visible = !questTracker.Visible;
                GD.Print("Quest Tracker toggled: " + questTracker.Visible);
            }
        }

        /// <summary>
        /// Toggle Quest Guide
        /// </summary>
        public void ToggleQuestGuide()
        {
            var questGuide = _main?.GetNodeOrNull<Control>("CanvasLayer/QuestGuideUI");
            if (questGuide != null)
            {
                questGuide.Visible = !questGuide.Visible;
                GD.Print("Quest Guide toggled: " + questGuide.Visible);
            }
        }

        /// <summary>
        /// Toggle Multiplayer UI
        /// </summary>
        public void ToggleMultiplayerUI()
        {
            var multiplayerUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MultiplayerLobbyUI");
            if (multiplayerUI != null)
            {
                multiplayerUI.Visible = !multiplayerUI.Visible;
                GD.Print("Multiplayer UI toggled: " + multiplayerUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Combat Rating UI
        /// </summary>
        public void ToggleCombatRatingUI()
        {
            var combatRatingUI = _main?.GetNodeOrNull<Control>("CanvasLayer/CombatRatingUI");
            if (combatRatingUI != null)
            {
                combatRatingUI.Visible = !combatRatingUI.Visible;
                GD.Print("Combat Rating UI toggled: " + combatRatingUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Weapon Mastery UI
        /// </summary>
        public void ToggleWeaponMasteryUI()
        {
            var weaponMasteryUI = _main?.GetNodeOrNull<Control>("CanvasLayer/WeaponMasteryUI");
            if (weaponMasteryUI != null)
            {
                weaponMasteryUI.Visible = !weaponMasteryUI.Visible;
                GD.Print("Weapon Mastery UI toggled: " + weaponMasteryUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Counter Attack UI
        /// </summary>
        public void ToggleCounterAttackUI()
        {
            var counterAttackUI = _main?.GetNodeOrNull<Control>("CanvasLayer/CounterAttackUI");
            if (counterAttackUI != null)
            {
                counterAttackUI.Visible = !counterAttackUI.Visible;
                GD.Print("Counter Attack UI toggled: " + counterAttackUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Mount UI
        /// </summary>
        public void ToggleMountUI()
        {
            var mountUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MountUI");
            if (mountUI != null)
            {
                mountUI.Visible = !mountUI.Visible;
                GD.Print("Mount UI toggled: " + mountUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Mount Training UI
        /// </summary>
        public void ToggleMountTrainingUI()
        {
            var mountTrainingUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MountTrainingUI");
            if (mountTrainingUI != null)
            {
                mountTrainingUI.Visible = !mountTrainingUI.Visible;
                GD.Print("Mount Training UI toggled: " + mountTrainingUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Skill Cooldown UI
        /// </summary>
        public void ToggleSkillCooldownUI()
        {
            var skillCooldownUI = _main?.GetNodeOrNull<Control>("CanvasLayer/SkillCooldownUI");
            if (skillCooldownUI != null)
            {
                skillCooldownUI.Visible = !skillCooldownUI.Visible;
                GD.Print("Skill Cooldown UI toggled: " + skillCooldownUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Skill Synergy UI
        /// </summary>
        public void ToggleSkillSynergyUI()
        {
            var skillSynergyUI = _main?.GetNodeOrNull<Control>("CanvasLayer/SkillSynergyUI");
            if (skillSynergyUI != null)
            {
                skillSynergyUI.Visible = !skillSynergyUI.Visible;
                GD.Print("Skill Synergy UI toggled: " + skillSynergyUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Skill Tree Reset UI
        /// </summary>
        public void ToggleSkillTreeResetUI()
        {
            var skillTreeResetUI = _main?.GetNodeOrNull<Control>("CanvasLayer/SkillTreeResetUI");
            if (skillTreeResetUI != null)
            {
                skillTreeResetUI.Visible = !skillTreeResetUI.Visible;
                GD.Print("Skill Tree Reset UI toggled: " + skillTreeResetUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Skill Mastery UI
        /// </summary>
        public void ToggleSkillMasteryUI()
        {
            var skillMasteryUI = _main?.GetNodeOrNull<Control>("CanvasLayer/SkillMasteryUI");
            if (skillMasteryUI != null)
            {
                skillMasteryUI.Visible = !skillMasteryUI.Visible;
                GD.Print("Skill Mastery UI toggled: " + skillMasteryUI.Visible);
            }
            else
            {
                // Try to find alternate path
                var altUI = _main?.GetNodeOrNull<Control>("CanvasLayer/SkillMastery/SkillMasteryUI");
                if (altUI != null)
                {
                    altUI.Visible = !altUI.Visible;
                    GD.Print("Skill Mastery UI toggled: " + altUI.Visible);
                }
            }
        }

        /// <summary>
        /// Toggle Constellation UI
        /// </summary>
        public void ToggleConstellationUI()
        {
            var constellationUI = _main?.GetNodeOrNull<Control>("CanvasLayer/ConstellationUI");
            if (constellationUI != null)
            {
                constellationUI.Visible = !constellationUI.Visible;
                GD.Print("Constellation UI toggled: " + constellationUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Procedural Story UI
        /// </summary>
        public void ToggleProceduralStoryUI()
        {
            var proceduralStoryUI = _main?.GetNodeOrNull<Control>("CanvasLayer/ProceduralStoryUI");
            if (proceduralStoryUI != null)
            {
                proceduralStoryUI.Visible = !proceduralStoryUI.Visible;
                GD.Print("Procedural Story UI toggled: " + proceduralStoryUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Momentum UI
        /// </summary>
        public void ToggleMomentumUI()
        {
            var momentumUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MomentumUI");
            if (momentumUI != null)
            {
                momentumUI.Visible = !momentumUI.Visible;
                GD.Print("Momentum UI toggled: " + momentumUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Enemy Scaling UI
        /// </summary>
        public void ToggleEnemyScalingUI()
        {
            var enemyScalingUI = _main?.GetNodeOrNull<Control>("CanvasLayer/EnemyScalingUI");
            if (enemyScalingUI != null)
            {
                enemyScalingUI.Visible = !enemyScalingUI.Visible;
                GD.Print("Enemy Scaling UI toggled: " + enemyScalingUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Weather UI
        /// </summary>
        public void ToggleWeatherUI()
        {
            var weatherUI = _main?.GetNodeOrNull<Control>("CanvasLayer/WeatherUI");
            if (weatherUI != null)
            {
                weatherUI.Visible = !weatherUI.Visible;
                GD.Print("Weather UI toggled: " + weatherUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Choice Event UI
        /// </summary>
        public void ToggleChoiceEventUI()
        {
            var choiceEventUI = _main?.GetNodeOrNull<Control>("CanvasLayer/ChoiceEventUI");
            if (choiceEventUI != null)
            {
                choiceEventUI.Visible = !choiceEventUI.Visible;
                GD.Print("Choice Event UI toggled: " + choiceEventUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Music Collection UI
        /// </summary>
        public void ToggleMusicCollectionUI()
        {
            var musicCollectionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MusicCollectionUI");
            if (musicCollectionUI != null)
            {
                musicCollectionUI.Visible = !musicCollectionUI.Visible;
                GD.Print("Music Collection UI toggled: " + musicCollectionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Gathering UI
        /// </summary>
        public void ToggleGatheringUI()
        {
            var gatheringUI = _main?.GetNodeOrNull<Control>("CanvasLayer/GatheringUI");
            if (gatheringUI != null)
            {
                gatheringUI.Visible = !gatheringUI.Visible;
                GD.Print("Gathering UI toggled: " + gatheringUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Monster Taming UI
        /// </summary>
        public void ToggleMonsterTamingUI()
        {
            var monsterTamingUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MonsterTamingUI");
            if (monsterTamingUI != null)
            {
                monsterTamingUI.Visible = !monsterTamingUI.Visible;
                GD.Print("Monster Taming UI toggled: " + monsterTamingUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Daily Puzzle UI
        /// </summary>
        public void ToggleDailyPuzzleUI()
        {
            var dailyPuzzleUI = _main?.GetNodeOrNull<Control>("CanvasLayer/DailyPuzzleUI");
            if (dailyPuzzleUI != null)
            {
                dailyPuzzleUI.Visible = !dailyPuzzleUI.Visible;
                GD.Print("Daily Puzzle UI toggled: " + dailyPuzzleUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Prestige UI
        /// </summary>
        public void TogglePrestigeUI()
        {
            var prestigeUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PrestigeUI");
            if (prestigeUI != null)
            {
                prestigeUI.Visible = !prestigeUI.Visible;
                GD.Print("Prestige UI toggled: " + prestigeUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Identification UI
        /// </summary>
        public void ToggleIdentificationUI()
        {
            var identificationUI = _main?.GetNodeOrNull<Control>("CanvasLayer/IdentificationUI");
            if (identificationUI != null)
            {
                identificationUI.Visible = !identificationUI.Visible;
                GD.Print("Identification UI toggled: " + identificationUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Title UI
        /// </summary>
        public void ToggleTitleUI()
        {
            var titleUI = _main?.GetNodeOrNull<Control>("CanvasLayer/TitleUI");
            if (titleUI != null)
            {
                titleUI.Visible = !titleUI.Visible;
                GD.Print("Title UI toggled: " + titleUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Title Collection UI
        /// </summary>
        public void ToggleTitleCollectionUI()
        {
            var titleCollectionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/TitleCollectionUI");
            if (titleCollectionUI != null)
            {
                titleCollectionUI.Visible = !titleCollectionUI.Visible;
                GD.Print("Title Collection UI toggled: " + titleCollectionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Bookmark UI
        /// </summary>
        public void ToggleBookmarkUI()
        {
            var bookmarkUI = _main?.GetNodeOrNull<Control>("CanvasLayer/BookmarkUI");
            if (bookmarkUI != null)
            {
                bookmarkUI.Visible = !bookmarkUI.Visible;
                GD.Print("Bookmark UI toggled: " + bookmarkUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Auto Bookmark UI
        /// </summary>
        public void ToggleAutoBookmarkUI()
        {
            var autoBookmarkUI = _main?.GetNodeOrNull<Control>("CanvasLayer/AutoBookmarkUI");
            if (autoBookmarkUI != null)
            {
                autoBookmarkUI.Visible = !autoBookmarkUI.Visible;
                GD.Print("Auto Bookmark UI toggled: " + autoBookmarkUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Enhancement UI
        /// </summary>
        public void ToggleEnhancementUI()
        {
            var enhancementUI = _main?.GetNodeOrNull<Control>("CanvasLayer/EnhancementUI");
            if (enhancementUI != null)
            {
                enhancementUI.Visible = !enhancementUI.Visible;
                GD.Print("Enhancement UI toggled: " + enhancementUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Auto Potion UI
        /// </summary>
        public void ToggleAutoPotionUI()
        {
            var autoPotionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/AutoPotionUI");
            if (autoPotionUI != null)
            {
                autoPotionUI.Visible = !autoPotionUI.Visible;
                GD.Print("Auto Potion UI toggled: " + autoPotionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Enchantment UI
        /// </summary>
        public void ToggleEnchantmentUI()
        {
            var enchantmentUI = _main?.GetNodeOrNull<Control>("CanvasLayer/EnchantmentUI");
            if (enchantmentUI != null)
            {
                enchantmentUI.Visible = !enchantmentUI.Visible;
                GD.Print("Enchantment UI toggled: " + enchantmentUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Boss Mechanics UI
        /// </summary>
        public void ToggleBossMechanicsUI()
        {
            var bossMechanicsUI = _main?.GetNodeOrNull<Control>("CanvasLayer/BossMechanicsUI");
            if (bossMechanicsUI != null)
            {
                bossMechanicsUI.Visible = !bossMechanicsUI.Visible;
                GD.Print("Boss Mechanics UI toggled: " + bossMechanicsUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Combat UI
        /// </summary>
        public void ToggleCombatUI()
        {
            var combatUI = _main?.GetNodeOrNull<Control>("CanvasLayer/CombatUI");
            if (combatUI != null)
            {
                combatUI.Visible = !combatUI.Visible;
                GD.Print("Combat UI toggled: " + combatUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Procedural Dungeon UI
        /// </summary>
        public void ToggleProceduralDungeonUI()
        {
            var proceduralDungeonUI = _main?.GetNodeOrNull<Control>("CanvasLayer/ProceduralDungeonUI");
            if (proceduralDungeonUI != null)
            {
                proceduralDungeonUI.Visible = !proceduralDungeonUI.Visible;
                GD.Print("Procedural Dungeon UI toggled: " + proceduralDungeonUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Mythic+ Dungeon UI
        /// </summary>
        public void ToggleMythicPlusDungeonUI()
        {
            var mythicPlusDungeonUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MythicPlusDungeonUI");
            if (mythicPlusDungeonUI != null)
            {
                mythicPlusDungeonUI.Visible = !mythicPlusDungeonUI.Visible;
                GD.Print("Mythic+ Dungeon UI toggled: " + mythicPlusDungeonUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Arena Tournament UI
        /// </summary>
        public void ToggleArenaTournamentUI()
        {
            var arenaTournamentUI = _main?.GetNodeOrNull<Control>("CanvasLayer/ArenaTournamentUI");
            if (arenaTournamentUI != null)
            {
                arenaTournamentUI.Visible = !arenaTournamentUI.Visible;
                GD.Print("Arena Tournament UI toggled: " + arenaTournamentUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Faction UI
        /// </summary>
        public void ToggleFactionUI()
        {
            var factionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/FactionUI");
            if (factionUI != null)
            {
                factionUI.Visible = !factionUI.Visible;
                GD.Print("Faction UI toggled: " + factionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Fishing UI
        /// </summary>
        public void ToggleFishingUI()
        {
            var fishingUI = _main?.GetNodeOrNull<Control>("CanvasLayer/FishingUI");
            if (fishingUI != null)
            {
                fishingUI.Visible = !fishingUI.Visible;
                GD.Print("Fishing UI toggled: " + fishingUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Alchemy UI
        /// </summary>
        public void ToggleAlchemyUI()
        {
            var alchemyUI = _main?.GetNodeOrNull<Control>("CanvasLayer/AlchemyUI");
            if (alchemyUI != null)
            {
                alchemyUI.Visible = !alchemyUI.Visible;
                GD.Print("Alchemy UI toggled: " + alchemyUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Cooking UI
        /// </summary>
        public void ToggleCookingUI()
        {
            var cookingUI = _main?.GetNodeOrNull<Control>("CanvasLayer/CookingUI");
            if (cookingUI != null)
            {
                cookingUI.Visible = !cookingUI.Visible;
                GD.Print("Cooking UI toggled: " + cookingUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Mount Combat UI
        /// </summary>
        public void ToggleMountCombatUI()
        {
            var mountCombatUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MountCombatUI");
            if (mountCombatUI != null)
            {
                mountCombatUI.Visible = !mountCombatUI.Visible;
                GD.Print("Mount Combat UI toggled: " + mountCombatUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Mount Evolution UI
        /// </summary>
        public void ToggleMountEvolutionUI()
        {
            var mountEvolutionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MountEvolutionUI");
            if (mountEvolutionUI != null)
            {
                mountEvolutionUI.Visible = !mountEvolutionUI.Visible;
                GD.Print("Mount Evolution UI toggled: " + mountEvolutionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Mount Equipment UI
        /// </summary>
        public void ToggleMountEquipmentUI()
        {
            var mountEquipmentUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MountEquipmentUI");
            if (mountEquipmentUI != null)
            {
                mountEquipmentUI.Visible = !mountEquipmentUI.Visible;
                GD.Print("Mount Equipment UI toggled: " + mountEquipmentUI.Visible);
            }
        }

        /// <summary>
        /// Toggle World Event UI
        /// </summary>
        public void ToggleWorldEventUI()
        {
            var worldEventUI = _main?.GetNodeOrNull<Control>("CanvasLayer/WorldEventUI");
            if (worldEventUI != null)
            {
                worldEventUI.Visible = !worldEventUI.Visible;
                GD.Print("World Event UI toggled: " + worldEventUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Gem UI
        /// </summary>
        public void ToggleGemUI()
        {
            var gemUI = _main?.GetNodeOrNull<Control>("CanvasLayer/GemUI");
            if (gemUI != null)
            {
                gemUI.Visible = !gemUI.Visible;
                GD.Print("Gem UI toggled: " + gemUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Gem Fusion UI
        /// </summary>
        public void ToggleGemFusionUI()
        {
            var gemFusionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/GemFusionUI");
            if (gemFusionUI != null)
            {
                gemFusionUI.Visible = !gemFusionUI.Visible;
                GD.Print("Gem Fusion UI toggled: " + gemFusionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Collectible UI
        /// </summary>
        public void ToggleCollectibleUI()
        {
            var collectibleUI = _main?.GetNodeOrNull<Control>("CanvasLayer/CollectibleUI");
            if (collectibleUI != null)
            {
                collectibleUI.Visible = !collectibleUI.Visible;
                GD.Print("Collectible UI toggled: " + collectibleUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Costume UI
        /// </summary>
        public void ToggleCostumeUI()
        {
            var costumeUI = _main?.GetNodeOrNull<Control>("CanvasLayer/CostumeUI");
            if (costumeUI != null)
            {
                costumeUI.Visible = !costumeUI.Visible;
                GD.Print("Costume UI toggled: " + costumeUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Pet Equipment UI
        /// </summary>
        public void TogglePetEquipmentUI()
        {
            var petEquipmentUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PetEquipmentUI");
            if (petEquipmentUI != null)
            {
                petEquipmentUI.Visible = !petEquipmentUI.Visible;
                GD.Print("Pet Equipment UI toggled: " + petEquipmentUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Pet Equipment Enhancement UI
        /// </summary>
        public void TogglePetEquipmentEnhancementUI()
        {
            var petEquipmentEnhancementUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PetEquipmentEnhancementUI");
            if (petEquipmentEnhancementUI != null)
            {
                petEquipmentEnhancementUI.Visible = !petEquipmentEnhancementUI.Visible;
                GD.Print("Pet Equipment Enhancement UI toggled: " + petEquipmentEnhancementUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Relic UI
        /// </summary>
        public void ToggleRelicUI()
        {
            var relicUI = _main?.GetNodeOrNull<Control>("CanvasLayer/RelicUI");
            if (relicUI != null)
            {
                relicUI.Visible = !relicUI.Visible;
                GD.Print("Relic UI toggled: " + relicUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Arena Colosseum UI
        /// </summary>
        public void ToggleArenaColosseumUI()
        {
            var arenaColosseumUI = _main?.GetNodeOrNull<Control>("CanvasLayer/ArenaColosseumUI");
            if (arenaColosseumUI != null)
            {
                arenaColosseumUI.Visible = !arenaColosseumUI.Visible;
                GD.Print("Arena Colosseum UI toggled: " + arenaColosseumUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Party UI
        /// </summary>
        public void TogglePartyUI()
        {
            var partyUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PartyUI");
            if (partyUI != null)
            {
                partyUI.Visible = !partyUI.Visible;
                GD.Print("Party UI toggled: " + partyUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Coop Session UI
        /// </summary>
        public void ToggleCoopSessionUI()
        {
            var coopSessionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/CoopSessionUI");
            if (coopSessionUI != null)
            {
                coopSessionUI.Visible = !coopSessionUI.Visible;
                GD.Print("Coop Session UI toggled: " + coopSessionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Equipment Enhancement UI
        /// </summary>
        public void ToggleEquipmentEnhancementUI()
        {
            var equipmentEnhancementUI = _main?.GetNodeOrNull<Control>("CanvasLayer/EquipmentEnhancementUI");
            if (equipmentEnhancementUI != null)
            {
                equipmentEnhancementUI.Visible = !equipmentEnhancementUI.Visible;
                GD.Print("Equipment Enhancement UI toggled: " + equipmentEnhancementUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Pet Evolution UI
        /// </summary>
        public void TogglePetEvolutionUI()
        {
            var petEvolutionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PetEvolutionUI");
            if (petEvolutionUI != null)
            {
                petEvolutionUI.Visible = !petEvolutionUI.Visible;
                GD.Print("Pet Evolution UI toggled: " + petEvolutionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Pet Talent UI
        /// </summary>
        public void TogglePetTalentUI()
        {
            var petTalentUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PetTalentUI");
            if (petTalentUI != null)
            {
                petTalentUI.Visible = !petTalentUI.Visible;
                GD.Print("Pet Talent UI toggled: " + petTalentUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Pet Affection UI
        /// </summary>
        public void TogglePetAffectionUI()
        {
            var petAffectionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PetAffectionUI");
            if (petAffectionUI != null)
            {
                petAffectionUI.Visible = !petAffectionUI.Visible;
                GD.Print("Pet Affection UI toggled: " + petAffectionUI.Visible);
            }
        }

        /// <summary>
        /// Toggle Pet Interaction UI
        /// </summary>
        public void TogglePetInteractionUI()
        {
            var petInteractionUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PetInteractionUI");
            if (petInteractionUI != null)
            {
                petInteractionUI.Visible = !petInteractionUI.Visible;
                GD.Print("Pet Interaction UI toggled: " + petInteractionUI.Visible);
            }
        }
    }
}
