using Godot;
using System;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainMenu - Handles title screen, start menu, and settings
    /// </summary>
    public partial class MainMenu : Node
    {
        private Main _main;
        
        public MainMenu()
        {
        }
        
        public void Initialize(Main main)
        {
            _main = main;
        }
        
        /// <summary>
        /// Show title screen
        /// </summary>
        public void ShowTitleScreen()
        {
            GD.Print("Showing title screen");
            // Title screen logic is typically handled by the scene
            // This method can be used for any title screen specific logic
        }
        
        /// <summary>
        /// Start new game
        /// </summary>
        public void StartNewGame()
        {
            GD.Print("Starting new game");
            _main?.SetGameState(Main.GameState.Playing);
        }
        
        /// <summary>
        /// Continue existing game
        /// </summary>
        public void ContinueGame()
        {
            GD.Print("Continuing game");
            _main?.LoadGame();
            _main?.SetGameState(Main.GameState.Playing);
        }
        
        /// <summary>
        /// Open settings
        /// </summary>
        public void OpenSettings()
        {
            _main?.OpenSettingsUI();
        }
        
        /// <summary>
        /// Show game over screen
        /// </summary>
        public void ShowGameOver()
        {
            GD.Print("Game Over");
            _main?.SetGameState(Main.GameState.GameOver);
        }
        
        /// <summary>
        /// Return to title screen
        /// </summary>
        public void ReturnToTitle()
        {
            GD.Print("Returning to title screen");
            _main?.SetGameState(Main.GameState.TitleScreen);
        }
    }
}
