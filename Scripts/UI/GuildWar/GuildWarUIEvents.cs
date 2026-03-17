using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Core.Systems.GuildWar
{
    public partial class GuildWarUI
    {
        private void OnWarStarted(string warId, string warName)
        {
            RefreshActiveWars();
        }

        private void OnWarEnded(string warId, string winnerId, List<GuildWarParticipant> rankings)
        {
            RefreshActiveWars();
            RefreshHistory();
        }

        private void OnTerritoryCaptured(string territoryId, string guildId, string guildName)
        {
            RefreshTerritories();
        }
    }
}
