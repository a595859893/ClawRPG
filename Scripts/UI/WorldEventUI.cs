// World Event UI
// UI for managing and viewing world events

using System;
using System.Collections.Generic;
using ClawRPG.Core.Systems;
using WorldEventType = ClawRPG.Core.Systems.WorldEventType;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Core.UI
{
    /// <summary>
    /// World Event UI - Displays active and historical world events
    /// </summary>
    public class WorldEventUI
    {
        private WorldEventSystem _worldEventSystem;
        private bool _isVisible;
        private int _selectedTab; // 0: Active, 1: History, 2: Stats
        
        // UI dimensions
        private int _windowWidth = 800;
        private int _windowHeight = 600;
        
        public WorldEventUI(WorldEventSystem worldEventSystem)
        {
            _worldEventSystem = worldEventSystem;
            _isVisible = false;
            _selectedTab = 0;
        }
        
        /// <summary>
        /// Toggle UI visibility
        /// </summary>
        public void Toggle()
        {
            _isVisible = !_isVisible;
        }
        
        /// <summary>
        /// Show UI
        /// </summary>
        public void Show()
        {
            _isVisible = true;
        }
        
        /// <summary>
        /// Hide UI
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
        }
        
        /// <summary>
        /// Check if UI is visible
        /// </summary>
        public bool IsVisible()
        {
            return _isVisible;
        }
        
        /// <summary>
        /// Handle tab selection
        /// </summary>
        public void SelectTab(int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex <= 2)
            {
                _selectedTab = tabIndex;
            }
        }
        
        /// <summary>
        /// Render the UI
        /// </summary>
        public void Render()
        {
            if (!_isVisible) return;
            
            // Header
            DrawHeader();
            
            // Tabs
            DrawTabs();
            
            // Content based on selected tab
            switch (_selectedTab)
            {
                case 0:
                    RenderActiveEvents();
                    break;
                case 1:
                    RenderEventHistory();
                    break;
                case 2:
                    RenderStatistics();
                    break;
            }
            
            // Footer
            DrawFooter();
        }
        
        private void DrawHeader()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              🌎 WORLD EVENT SYSTEM 🌎                           ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
        }
        
        private void DrawTabs()
        {
            Console.WriteLine("║  [1] Active Events  [2] History  [3] Statistics               ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
        }
        
        private void RenderActiveEvents()
        {
            var activeEvents = _worldEventSystem.GetActiveEvents();
            
            if (activeEvents.Count == 0)
            {
                Console.WriteLine("║                                                                  ║");
                Console.WriteLine("║           No active events at the moment.                       ║");
                Console.WriteLine("║           Check back later for new events!                      ║");
                Console.WriteLine("║                                                                  ║");
                return;
            }
            
            foreach (var evt in activeEvents)
            {
                var config = _worldEventSystem.GetEventConfig(evt.ConfigId);
                if (config == null) continue;
                
                string rarityColor = GetRarityColor(evt.Rarity);
                string typeIcon = GetEventTypeIcon(evt.Type);
                
                Console.WriteLine($"║  {typeIcon} {config.Name,-20} [{rarityColor}{evt.Rarity,-8}§r]                    ║");
                Console.WriteLine($"║     {config.Description,-55}              ║");
                Console.WriteLine($"║     ⏱ Time: {evt.RemainingSeconds}s | Progress: {evt.CurrentProgress}/{evt.RequiredProgress}              ║");
                Console.WriteLine($"║     📍 Location: {evt.LocationName,-40}              ║");
                
                if (evt.State == Systems.WorldEventState.Pending)
                {
                    Console.WriteLine($"║     [Click to Participate]                                      ║");
                }
                else if (evt.State == Systems.WorldEventState.Active)
                {
                    Console.WriteLine($"║     [████████{new string('░', (int)(evt.ProgressPercent / 10))}] {evt.ProgressPercent:F0}%                       ║");
                }
                
                Console.WriteLine("║ ---------------------------------------------------------------- ║");
            }
        }
        
        private void RenderEventHistory()
        {
            var playerData = _worldEventSystem.GetPlayerData();
            var history = playerData.EventHistory;
            
            if (history.Count == 0)
            {
                Console.WriteLine("║                                                                  ║");
                Console.WriteLine("║           No event history yet.                                 ║");
                Console.WriteLine("║           Participate in world events to build history!         ║");
                Console.WriteLine("║                                                                  ║");
                return;
            }
            
            // Show last 10 events
            int count = 0;
            for (int i = history.Count - 1; i >= 0 && count < 10; i--)
            {
                var evt = history[i];
                var config = _worldEventSystem.GetEventConfig(evt.ConfigId);
                if (config == null) continue;
                
                string statusIcon = evt.State == Systems.WorldEventState.Completed ? "✅" : "❌";
                string rarityColor = GetRarityColor(evt.Rarity);
                
                Console.WriteLine($"║  {statusIcon} {config.Name,-20} [{rarityColor}{evt.Rarity,-8}§r]     {evt.StartTime:yyyy-MM-dd HH:mm}    ║");
                count++;
            }
        }
        
        private void RenderStatistics()
        {
            var playerData = _worldEventSystem.GetPlayerData();
            
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine($"║  📊 OVERALL STATISTICS                                         ║");
            Console.WriteLine("║ ---------------------------------------------------------------- ║");
            Console.WriteLine($"║  Total Events:     {playerData.TotalEventsParticipated,-10}                        ║");
            Console.WriteLine($"║  Completed:        {playerData.EventsCompleted,-10}                        ║");
            Console.WriteLine($"║  Failed:           {playerData.EventsFailed,-10}                        ║");
            Console.WriteLine($"║  Success Rate:     {(playerData.TotalEventsParticipated > 0 ? (float)playerData.EventsCompleted / playerData.TotalEventsParticipated * 100 : 0),-10:F1}%                        ║");
            Console.WriteLine("║                                                                  ║");
            Console.WriteLine($"║  💰 Total Gold Earned:    {playerData.GoldEarned,-15}                ║");
            Console.WriteLine($"║  ⭐ Total Experience:     {playerData.ExperienceEarned,-15}                ║");
            Console.WriteLine("║                                                                  ║");
            
            // Events by type
            Console.WriteLine("║  📋 EVENTS BY TYPE                                              ║");
            Console.WriteLine("║ ---------------------------------------------------------------- ║");
            foreach (var kvp in playerData.EventsByType)
            {
                string typeIcon = GetEventTypeIcon(kvp.Key);
                Console.WriteLine($"║  {typeIcon} {kvp.Key,-20}: {kvp.Value,-5} events                      ║");
            }
            
            Console.WriteLine("║                                                                  ║");
            
            // Events by rarity
            Console.WriteLine("║  💎 EVENTS BY RARITY                                            ║");
            Console.WriteLine("║ ---------------------------------------------------------------- ║");
            foreach (var kvp in playerData.EventsByRarity)
            {
                string rarityColor = GetRarityColor(kvp.Key);
                Console.WriteLine($"║  [{rarityColor}{kvp.Key,-8}§r] {kvp.Value,-5} events                       ║");
            }
        }
        
        private void DrawFooter()
        {
            Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  [W/↑] Previous Tab  [S/↓] Next Tab  [Enter] Participate      ║");
            Console.WriteLine("║  [E] Toggle Events  [ESC] Close                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
        }
        
        private string GetEventTypeIcon(WorldEventType type)
        {
            return type switch
            {
                WorldEventType.TreasureSpawn => "💎",
                WorldEventType.MonsterSurge => "👹",
                WorldEventType.MerchantVisit => "🛒",
                WorldEventType.WeatherChange => "🌤️",
                WorldEventType.Blessing => "✨",
                WorldEventType.Curse => "💀",
                WorldEventType.RareSpawn => "🐉",
                WorldEventType.ResourceBurst => "🌿",
                WorldEventType.Portal => "🌀",
                WorldEventType.NPCrescue => "👤",
                _ => "❓"
            };
        }
        
        private string GetRarityColor(WorldEventRarity rarity)
        {
            return rarity switch
            {
                WorldEventRarity.Common => "§f",
                WorldEventRarity.Uncommon => "§a",
                WorldEventRarity.Rare => "§9",
                WorldEventRarity.Epic => "§5",
                WorldEventRarity.Legendary => "§6",
                _ => "§f"
            };
        }
        
        /// <summary>
        /// Handle keyboard input
        /// </summary>
        public void HandleInput(string key)
        {
            switch (key)
            {
                case "1":
                    SelectTab(0);
                    break;
                case "2":
                    SelectTab(1);
                    break;
                case "3":
                    SelectTab(2);
                    break;
                case "w":
                case "W":
                case "up":
                    SelectTab(Math.Max(0, _selectedTab - 1));
                    break;
                case "s":
                case "S":
                case "down":
                    SelectTab(Math.Min(2, _selectedTab + 1));
                    break;
                case "e":
                case "E":
                    // Participate in first active event
                    var activeEvents = _worldEventSystem.GetActiveEvents();
                    if (activeEvents.Count > 0)
                    {
                        _worldEventSystem.ParticipateInEvent(activeEvents[0].EventId);
                    }
                    break;
                case "escape":
                    Hide();
                    break;
            }
        }
    }
}
