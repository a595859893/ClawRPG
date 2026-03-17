using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// UI for multiplayer voting and party management
    /// </summary>
    public partial class MultiplayerVoteUI : Control
    {
        private static MultiplayerVoteUI _instance;
        public static MultiplayerVoteUI Instance => _instance;

        private MultiplayerVoteSystem _system;
        
        private string _currentPlayerId = "player_1";  // Would come from game state
        private string _currentPlayerName = "Player";

        public override void _Ready()
        {
            _instance = this;
            _system = MultiplayerVoteSystem.Instance;
            
            SetupUI();
            RefreshAll();
        }

        public void SetCurrentPlayer(string playerId, string playerName)
        {
            _currentPlayerId = playerId;
            _currentPlayerName = playerName;
            RefreshAll();
        }

        public void Toggle()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshAll();
            }
        }
    }
}
