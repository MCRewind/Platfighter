using System;
using System.Threading;
using ATTRIBUTES;
using Facepunch.Steamworks;
using MANAGERS;
using TOOLS;
using UnityEngine;
using Types = DATA.Types;

namespace MENU
{
    [MenuType(Types.Menu.LobbyCharacterMenu)]
    public class LobbyCharacterMenu : Menu
    {
        [SerializeField] private PlayerProfilePanel _playerProfilerPanel;

        private int _playerReady;

        protected override void SwitchToThis(params string[] args)
        {
            Client.Instance.Lobby.OnLobbyCreated = OnCreated;
            Client.Instance.Lobby.OnLobbyJoined = OnJoined;
            Client.Instance.Lobby.OnLobbyDataUpdated = OnDataUpdated;
            Client.Instance.Lobby.OnLobbyMemberDataUpdated = OnMemberDataUpdated;
            Client.Instance.Lobby.OnLobbyStateChanged = OnStateChange;
            Client.Instance.Lobby.OnChatMessageRecieved = OnChatMessage;
            
            if (args.Length > 0)
            {
                if (args[0] == "create")
                    Client.Instance.Lobby.Create(Lobby.Type.Public, 2);
                else if (args[0] == "join") Client.Instance.Lobby.Join(ulong.Parse(args[1]));
            }
        }

        private void OnCreated(bool success)
        {
            if (!success) return;

            _playerProfilerPanel.ClearPlayerProfiles();
            _playerProfilerPanel.AddPlayerProfile(Client.Instance.SteamId);

             Debug.Log("lobby created: " + Client.Instance.Lobby.CurrentLobby);
             Debug.Log($"Owner: {Client.Instance.Lobby.Owner}");
             Debug.Log($"Max Members: {Client.Instance.Lobby.MaxMembers}");
             Debug.Log($"Num Members: {Client.Instance.Lobby.NumMembers}");
        }

        private void OnJoined(bool success)
        {
            NLog.Log(NLog.LogType.Message, "OnLobbyJoined");
            if (!success) return;
        }

        private void OnDataUpdated()
        {
            Debug.Log( "OnLobbyDataUpdated");
            _playerProfilerPanel.ClearPlayerProfiles();
            foreach (var member in Client.Instance.Lobby.GetMemberIDs()) _playerProfilerPanel.AddPlayerProfile(member);
        }

        private void OnMemberDataUpdated(ulong member)
        {
            Debug.Log( "OnLobbyMemberDataUpdated");
            if (Client.Instance.Lobby.GetMemberData(member, "ready").Equals("true"))
            {
                Debug.Log("ddddd");
                _playerProfilerPanel.ReadyPlayerProfile(member);
                ++_playerReady;
                if (_playerReady >= Client.Instance.Lobby.NumMembers)
                    MenuManager.Instance.MenuState = Types.Menu.GameStartMenu;
            }
        }

        private void OnChatMessage(ulong d, byte[] c, int s)
        {
            Debug.Log("OnChatMessage");
        }

        private void OnStateChange(Lobby.MemberStateChange change, ulong initiator, ulong affectee)
        {
            Debug.Log( "OnLobbyStateChanged");
            switch (change)
            {
                case Lobby.MemberStateChange.Entered:
                    _playerProfilerPanel.AddPlayerProfile(initiator);
                    break;
                case Lobby.MemberStateChange.Disconnected:
                    _playerProfilerPanel.RemovePlayerProfile(initiator);
                    break;
                case Lobby.MemberStateChange.Left:
                    _playerProfilerPanel.RemovePlayerProfile(initiator);
                    break;
                case Lobby.MemberStateChange.Kicked:
                    break;
                case Lobby.MemberStateChange.Banned:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
            }
        }

        public void ReadyToPlay()
        {
            Client.Instance.Lobby.SetMemberData("ready", "true");
            //Client.Instance.Lobby.OnLobbyMemberDataUpdated(Client.Instance.SteamId);
            Debug.Log("wedy 2 pway");
        }

        public void GoBack()
        {
            _playerProfilerPanel.ClearPlayerProfiles();
            Client.Instance.Lobby.Leave();
            MenuManager.Instance.SwitchToPreviousMenu();
        }
    }
}