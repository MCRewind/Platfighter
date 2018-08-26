﻿using Facepunch.Steamworks;
using TOOLS;
using UnityEngine;

namespace NETWORKING
{
    public class Lobby : MonoBehaviour
    {
        [SerializeField] private GameObject _serverList, _serverButtonPrefab;

        public void JoinLobby(ulong id)
        {
            Client.Instance.Lobby.Join(id);
            Client.Instance.Lobby.OnLobbyJoined = delegate
            {
                NLog.Log(NLog.LogType.Message, "lobby joined: " + Client.Instance.Lobby.CurrentLobbyData);
            };
        }
    }
}