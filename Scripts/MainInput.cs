using Godot;
using System;
using ClawRPG.Scripts.UI;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainInput - Handles all input processing and keyboard shortcuts
    /// </summary>
    public partial class MainInput : Node
    {
        private Main _main;
        private UIManager _uiManager;
        
        public MainInput()
        {
        }
        
        public void Initialize(Main main)
        {
            _main = main;
            _uiManager = UIManager.Instance;
        }
        
        public void ProcessInput(float delta)
        {
            // Handle all keyboard shortcuts
            // These are delegated to UIManager for actual functionality
            
            // Handle runes UI toggle (U key)
            if (Input.IsActionJustPressed("runes"))
            {
                _uiManager?.ToggleUI("RunesUI");
            }

            // Handle quest tracker toggle (T key)
            if (Input.IsActionJustPressed("quest_tracker"))
            {
                _uiManager?.ToggleUI("QuestTrackerUI");
            }

            // Handle quest guide toggle (G key)
            if (Input.IsActionJustPressed("quest_guide"))
            {
                _uiManager?.ToggleUI("QuestGuideUI");
            }

            // Handle multiplayer UI toggle (M key)
            if (Input.IsActionJustPressed("multiplayer"))
            {
                _uiManager?.ToggleUI("MultiplayerUI");
            }

            // Handle combat rating UI toggle (R key)
            if (Input.IsActionJustPressed("combat_rating"))
            {
                _uiManager?.ToggleUI("CombatRatingUI");
            }

            // Handle weapon mastery UI toggle (W key)
            if (Input.IsActionJustPressed("weapon_mastery"))
            {
                _uiManager?.ToggleUI("WeaponMasteryUI");
            }

            // Handle counter attack UI toggle (Shift+C key)
            if (Input.IsActionJustPressed("counter_attack"))
            {
                _uiManager?.ToggleUI("CounterAttackUI");
            }

            // Handle mount UI toggle (O key)
            if (Input.IsActionJustPressed("mounts"))
            {
                _uiManager?.ToggleUI("MountUI");
            }

            // Handle mount training UI toggle (Ctrl+T key)
            if (Input.IsKeyPressed(Key.T) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("MountTrainingUI");
            }

            // Handle skill cooldown UI toggle (K key)
            if (Input.IsKeyPressed(Key.K))
            {
                _uiManager?.ToggleUI("SkillCooldownUI");
            }

            // Handle skill synergy UI toggle (Shift+K key)
            if (Input.IsKeyPressed(Key.K) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("SkillSynergyUI");
            }

            // Handle skill tree reset UI toggle (Ctrl+Shift+R key)
            if (Input.IsKeyPressed(Key.R) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("SkillTreeResetUI");
            }

            // Handle constellation UI toggle (K key - when not in combat)
            if (Input.IsKeyPressed(Key.K))
            {
                _uiManager?.ToggleUI("ConstellationUI");
            }

            // Handle procedural story UI toggle (Ctrl+Shift+S key)
            if (Input.IsKeyPressed(Key.S) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("ProceduralStoryUI");
            }

            // Handle momentum UI toggle (M key)
            if (Input.IsKeyPressed(Key.M))
            {
                _uiManager?.ToggleUI("MomentumUI");
            }

            // Handle weather UI toggle (W key)
            if (Input.IsKeyPressed(Key.W))
            {
                _uiManager?.ToggleUI("WeatherUI");
            }

            // Handle choice event UI toggle (C key)
            if (Input.IsKeyPressed(Key.C))
            {
                _uiManager?.ToggleUI("ChoiceEventUI");
            }

            // Handle music collection UI toggle (Ctrl+Shift+M key)
            if (Input.IsKeyPressed(Key.M) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("MusicCollectionUI");
            }

            // Handle gathering UI toggle (G key)
            if (Input.IsKeyPressed(Key.G))
            {
                _uiManager?.ToggleUI("GatheringUI");
            }

            // Handle monster taming UI toggle (Ctrl+Shift+T key)
            if (Input.IsKeyPressed(Key.T) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("MonsterTamingUI");
            }

            // Handle daily puzzle UI toggle (D key)
            if (Input.IsKeyPressed(Key.D))
            {
                _uiManager?.ToggleUI("DailyPuzzleUI");
            }

            // Handle prestige UI toggle (P key)
            if (Input.IsKeyPressed(Key.P))
            {
                _uiManager?.ToggleUI("PrestigeUI");
            }

            // Handle identification UI toggle (I key)
            if (Input.IsKeyPressed(Key.I))
            {
                _uiManager?.ToggleUI("IdentificationUI");
            }

            // Handle title UI toggle (Ctrl+T key)
            if (Input.IsKeyPressed(Key.T) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("TitleUI");
            }

            // Handle title collection UI toggle (Ctrl+Shift+T key)
            if (Input.IsKeyPressed(Key.T) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("TitleCollectionUI");
            }

            // Handle bookmark UI toggle (B key)
            if (Input.IsKeyPressed(Key.B))
            {
                _uiManager?.ToggleUI("BookmarkUI");
            }

            // Handle auto bookmark UI toggle (Ctrl+Shift+B key)
            if (Input.IsKeyPressed(Key.B) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("AutoBookmarkUI");
            }

            // Handle enhancement UI toggle (E key)
            if (Input.IsKeyPressed(Key.E))
            {
                _uiManager?.ToggleUI("EnhancementUI");
            }

            // Handle auto potion UI toggle (Ctrl+P key)
            if (Input.IsKeyPressed(Key.P) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("AutoPotionUI");
            }

            // Handle enchantment UI toggle (Ctrl+E key)
            if (Input.IsKeyPressed(Key.E) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("EnchantmentUI");
            }

            // Handle boss mechanics UI toggle (Ctrl+B key)
            if (Input.IsKeyPressed(Key.B) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("BossMechanicsUI");
            }

            // Handle combat UI toggle (Tab key)
            if (Input.IsKeyPressed(Key.Tab))
            {
                _uiManager?.ToggleUI("CombatUI");
            }

            // Handle procedural dungeon UI toggle (Ctrl+D key)
            if (Input.IsKeyPressed(Key.D) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("ProceduralDungeonUI");
            }

            // Handle mythic+ dungeon UI toggle (Ctrl+M key)
            if (Input.IsKeyPressed(Key.M) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("MythicPlusDungeonUI");
            }

            // Handle arena tournament UI toggle (Ctrl+A key)
            if (Input.IsKeyPressed(Key.A) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("ArenaTournamentUI");
            }

            // Handle faction UI toggle (F key)
            if (Input.IsKeyPressed(Key.F))
            {
                _uiManager?.ToggleUI("FactionUI");
            }

            // Handle fishing UI toggle (Shift+F key)
            if (Input.IsKeyPressed(Key.F) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("FishingUI");
            }

            // Handle alchemy UI toggle (A key)
            if (Input.IsKeyPressed(Key.A))
            {
                _uiManager?.ToggleUI("AlchemyUI");
            }

            // Handle cooking UI toggle (C key)
            if (Input.IsKeyPressed(Key.C))
            {
                _uiManager?.ToggleUI("CookingUI");
            }

            // Handle mount combat UI toggle (Ctrl+Shift+M key)
            if (Input.IsKeyPressed(Key.M) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("MountCombatUI");
            }

            // Handle mount evolution UI toggle (E key)
            if (Input.IsKeyPressed(Key.E))
            {
                _uiManager?.ToggleUI("MountEvolutionUI");
            }

            // Handle mount equipment UI toggle (Q key)
            if (Input.IsKeyPressed(Key.Q))
            {
                _uiManager?.ToggleUI("MountEquipmentUI");
            }

            // Handle world event UI toggle (W key)
            if (Input.IsKeyPressed(Key.W))
            {
                _uiManager?.ToggleUI("WorldEventUI");
            }

            // Handle gem UI toggle (G key)
            if (Input.IsKeyPressed(Key.G))
            {
                _uiManager?.ToggleUI("GemUI");
            }

            // Handle gem fusion UI toggle (Ctrl+G key)
            if (Input.IsKeyPressed(Key.G) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("GemFusionUI");
            }

            // Handle collectible UI toggle (Ctrl+Shift+C key)
            if (Input.IsKeyPressed(Key.C) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("CollectibleUI");
            }

            // Handle costume UI toggle (Ctrl+Shift+K key)
            if (Input.IsKeyPressed(Key.K) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("CostumeUI");
            }

            // Handle pet equipment UI toggle (P key)
            if (Input.IsKeyPressed(Key.P))
            {
                _uiManager?.ToggleUI("PetEquipmentUI");
            }

            // Handle pet equipment enhancement UI toggle (Ctrl+Shift+P key)
            if (Input.IsKeyPressed(Key.P) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("PetEquipmentEnhancementUI");
            }

            // Handle relic UI toggle (R key)
            if (Input.IsKeyPressed(Key.R))
            {
                _uiManager?.ToggleUI("RelicUI");
            }

            // Handle arena tournament UI toggle (Ctrl+Shift+A key)
            if (Input.IsKeyPressed(Key.A) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("ArenaTournamentUI");
            }

            // Handle arena colosseum UI toggle (A key)
            if (Input.IsKeyPressed(Key.A))
            {
                _uiManager?.ToggleUI("ArenaColosseumUI");
            }

            // Handle party UI toggle (P key)
            if (Input.IsKeyPressed(Key.P))
            {
                _uiManager?.ToggleUI("PartyUI");
            }

            // Handle coop session UI toggle (Ctrl+O key)
            if (Input.IsKeyPressed(Key.O) && Input.IsKeyPressed(Key.Ctrl))
            {
                _uiManager?.ToggleUI("CoopSessionUI");
            }

            // Handle equipment enhancement UI toggle (E key)
            if (Input.IsKeyPressed(Key.E))
            {
                _uiManager?.ToggleUI("EquipmentEnhancementUI");
            }

            // Handle pet evolution UI toggle (V key)
            if (Input.IsKeyPressed(Key.V))
            {
                _uiManager?.ToggleUI("PetEvolutionUI");
            }

            // Handle pet talent UI toggle (T key)
            if (Input.IsKeyPressed(Key.T))
            {
                _uiManager?.ToggleUI("PetTalentUI");
            }

            // Handle pet affection UI toggle (A key)
            if (Input.IsKeyPressed(Key.A))
            {
                _uiManager?.ToggleUI("PetAffectionUI");
            }

            // Handle pet interaction UI toggle (I key)
            if (Input.IsKeyPressed(Key.I))
            {
                _uiManager?.ToggleUI("PetInteractionUI");
            }

            // Handle skill mastery UI toggle (S key)
            if (Input.IsKeyPressed(Key.S))
            {
                _uiManager?.ToggleUI("SkillMasteryUI");
            }

            // Handle meditation UI toggle (Y key)
            if (Input.IsKeyPressed(Key.Y))
            {
                _uiManager?.ToggleUI("MeditationUI");
            }

            // Handle enemy scaling UI toggle (Ctrl+Shift+E key)
            if (Input.IsKeyPressed(Key.E) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                _uiManager?.ToggleUI("EnemyScalingUI");
            }
        }
    }
}
