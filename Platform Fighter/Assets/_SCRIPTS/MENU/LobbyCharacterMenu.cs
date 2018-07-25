﻿using System;
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
        
        protected override void SwitchToThis()
        { 
            Client.Instance.Lobby.Create(Lobby.Type.Public, 2);
            
            Client.Instance.Lobby.OnLobbyCreated = success =>
            {
                if (!success) return;

                _playerProfilerPanel.AddPlayerProfile(Client.Instance.SteamId);
                
                Debug.Log("lobby created: " + Client.Instance.Lobby.CurrentLobby);
                Debug.Log($"Owner: {Client.Instance.Lobby.Owner}");
                Debug.Log($"Max Members: {Client.Instance.Lobby.MaxMembers}");
                Debug.Log($"Num Members: {Client.Instance.Lobby.NumMembers}");
            };
            
            Client.Instance.Lobby.OnLobbyJoined = delegate(bool success)
            {
                if (!success) return;
                
                _playerProfilerPanel.AddPlayerProfile(Client.Instance.SteamId);
            };
            
            Client.Instance.Lobby.OnLobbyStateChanged = delegate(Lobby.MemberStateChange change, ulong initiator, ulong affectee)
            {
                Debug.Log("yallreadu know");
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
            };
        }
        
        public void GoBack()
        {
            _playerProfilerPanel.RemovePlayerProfile(Client.Instance.SteamId);
            Client.Instance.Lobby.Leave();
            MenuManager.Instance.SwitchToPreviousMenu();
        }
    }
}