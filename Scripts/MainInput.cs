using Godot;
using System;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainInput - Handles all input processing and keyboard shortcuts
    /// </summary>
    public partial class MainInput : Node
    {
        private Main _main;
        
        public MainInput()
        {
        }
        
        public void Initialize(Main main)
        {
            _main = main;
        }
        
        public void ProcessInput(float delta)
        {
            // Handle all keyboard shortcuts
            // These are delegated to Main for actual functionality
            
            // Handle runes UI toggle (U key)
            if (Input.IsActionJustPressed("runes"))
            {
                _main?.ToggleRunesUI();
            }

            // Handle quest tracker toggle (T key)
            if (Input.IsActionJustPressed("quest_tracker"))
            {
                _main?.ToggleQuestTracker();
            }

            // Handle quest guide toggle (G key)
            if (Input.IsActionJustPressed("quest_guide"))
            {
                _main?.ToggleQuestGuide();
            }

            // Handle multiplayer UI toggle (M key)
            if (Input.IsActionJustPressed("multiplayer"))
            {
                _main?.ToggleMultiplayerUI();
            }

            // Handle combat rating UI toggle (R key)
            if (Input.IsActionJustPressed("combat_rating"))
            {
                _main?.ToggleCombatRatingUI();
            }

            // Handle weapon mastery UI toggle (W key)
            if (Input.IsActionJustPressed("weapon_mastery"))
            {
                _main?.ToggleWeaponMasteryUI();
            }

            // Handle counter attack UI toggle (Shift+C key)
            if (Input.IsActionJustPressed("counter_attack"))
            {
                _main?.ToggleCounterAttackUI();
            }

            // Handle mount UI toggle (O key)
            if (Input.IsActionJustPressed("mounts"))
            {
                _main?.ToggleMountUI();
            }

            // Handle mount training UI toggle (Ctrl+T key)
            if (Input.IsKeyPressed(Key.T) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleMountTrainingUI();
            }

            // Handle skill cooldown UI toggle (K key)
            if (Input.IsKeyPressed(Key.K))
            {
                _main?.ToggleSkillCooldownUI();
            }

            // Handle skill synergy UI toggle (Shift+K key)
            if (Input.IsKeyPressed(Key.K) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleSkillSynergyUI();
            }

            // Handle skill tree reset UI toggle (Ctrl+Shift+R key)
            if (Input.IsKeyPressed(Key.R) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleSkillTreeResetUI();
            }

            // Handle constellation UI toggle (K key - when not in combat)
            if (Input.IsKeyPressed(Key.K))
            {
                _main?.ToggleConstellationUI();
            }

            // Handle procedural story UI toggle (Ctrl+Shift+S key)
            if (Input.IsKeyPressed(Key.S) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleProceduralStoryUI();
            }

            // Handle momentum UI toggle (M key)
            if (Input.IsKeyPressed(Key.M))
            {
                _main?.ToggleMomentumUI();
            }

            // Handle weather UI toggle (W key)
            if (Input.IsKeyPressed(Key.W))
            {
                _main?.ToggleWeatherUI();
            }

            // Handle choice event UI toggle (C key)
            if (Input.IsKeyPressed(Key.C))
            {
                _main?.ToggleChoiceEventUI();
            }

            // Handle music collection UI toggle (Ctrl+Shift+M key)
            if (Input.IsKeyPressed(Key.M) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleMusicCollectionUI();
            }

            // Handle gathering UI toggle (G key)
            if (Input.IsKeyPressed(Key.G))
            {
                _main?.ToggleGatheringUI();
            }

            // Handle monster taming UI toggle (Ctrl+Shift+T key)
            if (Input.IsKeyPressed(Key.T) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleMonsterTamingUI();
            }

            // Handle daily puzzle UI toggle (D key)
            if (Input.IsKeyPressed(Key.D))
            {
                _main?.ToggleDailyPuzzleUI();
            }

            // Handle prestige UI toggle (P key)
            if (Input.IsKeyPressed(Key.P))
            {
                _main?.TogglePrestigeUI();
            }

            // Handle identification UI toggle (I key)
            if (Input.IsKeyPressed(Key.I))
            {
                _main?.ToggleIdentificationUI();
            }

            // Handle title UI toggle (Ctrl+T key)
            if (Input.IsKeyPressed(Key.T) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleTitleUI();
            }

            // Handle title collection UI toggle (Ctrl+Shift+T key)
            if (Input.IsKeyPressed(Key.T) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleTitleCollectionUI();
            }

            // Handle bookmark UI toggle (B key)
            if (Input.IsKeyPressed(Key.B))
            {
                _main?.ToggleBookmarkUI();
            }

            // Handle auto bookmark UI toggle (Ctrl+Shift+B key)
            if (Input.IsKeyPressed(Key.B) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleAutoBookmarkUI();
            }

            // Handle enhancement UI toggle (E key)
            if (Input.IsKeyPressed(Key.E))
            {
                _main?.ToggleEnhancementUI();
            }

            // Handle auto potion UI toggle (Ctrl+P key)
            if (Input.IsKeyPressed(Key.P) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleAutoPotionUI();
            }

            // Handle enchantment UI toggle (Ctrl+E key)
            if (Input.IsKeyPressed(Key.E) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleEnchantmentUI();
            }

            // Handle boss mechanics UI toggle (Ctrl+B key)
            if (Input.IsKeyPressed(Key.B) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleBossMechanicsUI();
            }

            // Handle combat UI toggle (Tab key)
            if (Input.IsKeyPressed(Key.Tab))
            {
                _main?.ToggleCombatUI();
            }

            // Handle procedural dungeon UI toggle (Ctrl+D key)
            if (Input.IsKeyPressed(Key.D) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleProceduralDungeonUI();
            }

            // Handle mythic+ dungeon UI toggle (Ctrl+M key)
            if (Input.IsKeyPressed(Key.M) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleMythicPlusDungeonUI();
            }

            // Handle arena tournament UI toggle (Ctrl+A key)
            if (Input.IsKeyPressed(Key.A) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleArenaTournamentUI();
            }

            // Handle faction UI toggle (F key)
            if (Input.IsKeyPressed(Key.F))
            {
                _main?.ToggleFactionUI();
            }

            // Handle fishing UI toggle (Shift+F key)
            if (Input.IsKeyPressed(Key.F) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleFishingUI();
            }

            // Handle alchemy UI toggle (A key)
            if (Input.IsKeyPressed(Key.A))
            {
                _main?.ToggleAlchemyUI();
            }

            // Handle cooking UI toggle (C key)
            if (Input.IsKeyPressed(Key.C))
            {
                _main?.ToggleCookingUI();
            }

            // Handle mount combat UI toggle (Ctrl+Shift+M key)
            if (Input.IsKeyPressed(Key.M) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleMountCombatUI();
            }

            // Handle mount evolution UI toggle (E key)
            if (Input.IsKeyPressed(Key.E))
            {
                _main?.ToggleMountEvolutionUI();
            }

            // Handle mount equipment UI toggle (Q key)
            if (Input.IsKeyPressed(Key.Q))
            {
                _main?.ToggleMountEquipmentUI();
            }

            // Handle world event UI toggle (W key)
            if (Input.IsKeyPressed(Key.W))
            {
                _main?.ToggleWorldEventUI();
            }

            // Handle gem UI toggle (G key)
            if (Input.IsKeyPressed(Key.G))
            {
                _main?.ToggleGemUI();
            }

            // Handle gem fusion UI toggle (Ctrl+G key)
            if (Input.IsKeyPressed(Key.G) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleGemFusionUI();
            }

            // Handle collectible UI toggle (Ctrl+Shift+C key)
            if (Input.IsKeyPressed(Key.C) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleCollectibleUI();
            }

            // Handle costume UI toggle (Ctrl+Shift+K key)
            if (Input.IsKeyPressed(Key.K) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleCostumeUI();
            }

            // Handle pet equipment UI toggle (P key)
            if (Input.IsKeyPressed(Key.P))
            {
                _main?.TogglePetEquipmentUI();
            }

            // Handle pet equipment enhancement UI toggle (Ctrl+Shift+P key)
            if (Input.IsKeyPressed(Key.P) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.TogglePetEquipmentEnhancementUI();
            }

            // Handle relic UI toggle (R key)
            if (Input.IsKeyPressed(Key.R))
            {
                _main?.ToggleRelicUI();
            }

            // Handle arena tournament UI toggle (Ctrl+Shift+A key)
            if (Input.IsKeyPressed(Key.A) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleArenaTournamentUI();
            }

            // Handle arena colosseum UI toggle (A key)
            if (Input.IsKeyPressed(Key.A))
            {
                _main?.ToggleArenaColosseumUI();
            }

            // Handle party UI toggle (P key)
            if (Input.IsKeyPressed(Key.P))
            {
                _main?.TogglePartyUI();
            }

            // Handle coop session UI toggle (Ctrl+O key)
            if (Input.IsKeyPressed(Key.O) && Input.IsKeyPressed(Key.Ctrl))
            {
                _main?.ToggleCoopSessionUI();
            }

            // Handle equipment enhancement UI toggle (E key)
            if (Input.IsKeyPressed(Key.E))
            {
                _main?.ToggleEquipmentEnhancementUI();
            }

            // Handle pet evolution UI toggle (V key)
            if (Input.IsKeyPressed(Key.V))
            {
                _main?.TogglePetEvolutionUI();
            }

            // Handle pet talent UI toggle (T key)
            if (Input.IsKeyPressed(Key.T))
            {
                _main?.TogglePetTalentUI();
            }

            // Handle pet affection UI toggle (A key)
            if (Input.IsKeyPressed(Key.A))
            {
                _main?.TogglePetAffectionUI();
            }

            // Handle pet interaction UI toggle (I key)
            if (Input.IsKeyPressed(Key.I))
            {
                _main?.TogglePetInteractionUI();
            }

            // Handle skill mastery UI toggle (S key)
            if (Input.IsKeyPressed(Key.S))
            {
                _main?.ToggleSkillMasteryUI();
            }

            // Handle meditation UI toggle (Y key)
            if (Input.IsKeyPressed(Key.Y))
            {
                _main?.ToggleMeditationUI();
            }

            // Handle enemy scaling UI toggle (Ctrl+Shift+E key)
            if (Input.IsKeyPressed(Key.E) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _main?.ToggleEnemyScalingUI();
            }
        }
    }
}
