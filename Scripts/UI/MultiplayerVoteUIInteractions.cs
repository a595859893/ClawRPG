using System.Linq;
using Godot;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// UI Interactions - MultiplayerVoteUI 交互逻辑
    /// </summary>
    public partial class MultiplayerVoteUI
    {
        #region Event Handlers

        private void OnCreatePartyPressed()
        {
            var partyName = $" {_currentPlayerName}'s Party";
            _system.CreateParty(_currentPlayerId, _currentPlayerName, partyName, true, "", "PvE", 4);
            RefreshAll();
        }

        private void OnLeavePartyPressed()
        {
            _system.LeaveParty(_currentPlayerId);
            RefreshAll();
        }

        private void OnReadyPressed()
        {
            var party = _system.GetPlayerParty(_currentPlayerId);
            if (party == null) return;

            var member = party.Members.FirstOrDefault(m => m.PlayerId == _currentPlayerId);
            if (member == null) return;

            _system.SetReady(_currentPlayerId, !member.IsReady);
            RefreshPartyTab();
        }

        private void OnInvitePressed()
        {
            var party = _system.GetPlayerParty(_currentPlayerId);
            if (party == null) return;

            GD.Print($"Invite link: clawrpg://party/{party.PartyId}");
        }

        private void OnSettingsPressed()
        {
            // Toggle party visibility
            var party = _system.GetPlayerParty(_currentPlayerId);
            if (party != null)
            {
                _system.UpdatePartySettings(_currentPlayerId, isPublic: !party.IsPublic);
                RefreshPartyTab();
            }
        }

        private void OnInitiateVotePressed()
        {
            var voteType = (VoteType)_voteTypeSelector.GetSelectedId();
            var targetId = _targetPlayerInput.Text;
            var reason = _reasonInput.Text;

            _system.InitiateVote(_currentPlayerId, voteType, targetId, "", reason);
            _targetPlayerInput.Text = "";
            _reasonInput.Text = "";
            RefreshVoteTab();
        }

        private void OnRefreshPressed()
        {
            RefreshBrowseTab();
        }

        private void OnJoinPartyPressed()
        {
            var partyId = _partyIdInput.Text;
            var password = _passwordInput.Text;

            if (!string.IsNullOrEmpty(partyId))
            {
                var joined = _system.JoinParty(_currentPlayerId, _currentPlayerName, 1, 100, partyId, password);
                if (joined)
                {
                    _partyIdInput.Text = "";
                    _passwordInput.Text = "";
                    RefreshAll();
                }
            }
        }

        #endregion
    }
}
