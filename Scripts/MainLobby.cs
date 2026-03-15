using Godot;
using System;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainLobby - Handles lobby and multiplayer room management
    /// </summary>
    public partial class MainLobby : Node
    {
        private Main _main;
        
        public MainLobby()
        {
        }
        
        public void Initialize(Main main)
        {
            _main = main;
        }
        
        /// <summary>
        /// Open multiplayer lobby
        /// </summary>
        public void OpenLobby()
        {
            var lobbyUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MultiplayerLobbyUI");
            if (lobbyUI != null)
            {
                lobbyUI.Visible = true;
                GD.Print("Multiplayer lobby opened");
            }
        }
        
        /// <summary>
        /// Close multiplayer lobby
        /// </summary>
        public void CloseLobby()
        {
            var lobbyUI = _main?.GetNodeOrNull<Control>("CanvasLayer/MultiplayerLobbyUI");
            if (lobbyUI != null)
            {
                lobbyUI.Visible = false;
                GD.Print("Multiplayer lobby closed");
            }
        }
        
        /// <summary>
        /// Create a new room
        /// </summary>
        public void CreateRoom(string roomName, int maxPlayers)
        {
            GD.Print("Creating room: " + roomName + " with max players: " + maxPlayers);
            // Room creation logic would go here
        }
        
        /// <summary>
        /// Join an existing room
        /// </summary>
        public void JoinRoom(string roomId)
        {
            GD.Print("Joining room: " + roomId);
            // Room joining logic would go here
        }
        
        /// <summary>
        /// Leave current room
        /// </summary>
        public void LeaveRoom()
        {
            GD.Print("Leaving room");
            // Room leaving logic would go here
        }
        
        /// <summary>
        /// Toggle party UI
        /// </summary>
        public void ToggleParty()
        {
            var partyUI = _main?.GetNodeOrNull<Control>("CanvasLayer/PartyUI");
            if (partyUI != null)
            {
                partyUI.Visible = !partyUI.Visible;
                GD.Print("Party UI toggled");
            }
        }
        
        /// <summary>
        /// Toggle coop session UI
        /// </summary>
        public void ToggleCoopSession()
        {
            var coopUI = _main?.GetNodeOrNull<Control>("CanvasLayer/CoopSessionUI");
            if (coopUI != null)
            {
                coopUI.Visible = !coopUI.Visible;
                GD.Print("Coop Session UI toggled");
            }
        }
    }
}
