﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ATTRIBUTES;
using Facepunch.Steamworks;
using MANAGERS;
using MISC;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Types = DATA.Types;

namespace MENU
{
    [MenuType(Types.Menu.LobbyCharacterMenu)]
    public class LobbyCharacterMenu : Menu
    {
        [SerializeField] private PlayerProfilePanel _playerProfilerPanel;

        private int _playerReady;
        
        // true = create, false = join
        public bool InteractionType { get; set; }
        public ulong LobbyId { get; set; }

        protected override void SwitchToThis()
        {
            Client.Instance.Lobby.OnLobbyCreated           += OnCreated;
            Client.Instance.Lobby.OnLobbyJoined            += OnJoined;
            Client.Instance.Lobby.OnLobbyDataUpdated       += OnDataUpdated;
            Client.Instance.Lobby.OnLobbyMemberDataUpdated += OnMemberDataUpdated;
            Client.Instance.Lobby.OnLobbyStateChanged      += OnStateChange;

            if (InteractionType)
                Client.Instance.Lobby.Create(Lobby.Type.Public, 2);
            else
                Client.Instance.Lobby.Join(LobbyId);
        }

        void OnCreated(bool success)
        {
            if (!success) return;

            _playerProfilerPanel.ClearPlayerProfiles();
            _playerProfilerPanel.AddPlayerProfile(Client.Instance.SteamId);

            Debug.Log("lobby created: " + Client.Instance.Lobby.CurrentLobby);
            Debug.Log($"Owner: {Client.Instance.Lobby.Owner}");
            Debug.Log($"Max Members: {Client.Instance.Lobby.MaxMembers}");
            Debug.Log($"Num Members: {Client.Instance.Lobby.NumMembers}");
        }

        void OnJoined(bool success)
        {
            Debug.Log("OnLobbyJoined");
            if (!success) return;
        }

        void OnDataUpdated()
        {
            Debug.Log("OnLobbyDataUpdated");
            _playerProfilerPanel.ClearPlayerProfiles();
            foreach (var member in Client.Instance.Lobby.GetMemberIDs())
            {
                _playerProfilerPanel.AddPlayerProfile(member);
            }
        }

        void OnMemberDataUpdated(ulong member)
        {
            Debug.Log("OnLobbyMemberDataUpdated");
            if (Client.Instance.Lobby.GetMemberData(member, "ready").Equals("true"))
            {
                ++_playerReady;
                if (_playerReady >= 1)
                    MenuManager.Instance.MenuState = Types.Menu.GameStartMenu;
            }
        }

        void OnStateChange(Lobby.MemberStateChange change, ulong initiator, ulong affectee)
        {
            Debug.Log("OnLobbyStateChanged");
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

        private void Update()
        {
            Client.Instance.Lobby.SetMemberData("ready", "true");
        }

        public void ReadyToPlay()
        {
            Client.Instance.Lobby.SetMemberData("ready", "true");
            //MenuManager.Instance.MenuState = Types.Menu.GameStartMenu;
            Debug.Log("wedy 2 pway");
            _playerProfilerPanel.ReadyPlayerProfile(Client.Instance.SteamId);
        }
        
        public void GoBack()
        {
            _playerProfilerPanel.ClearPlayerProfiles();
            Client.Instance.Lobby.Leave();
            MenuManager.Instance.SwitchToPreviousMenu();
        }
    }
}