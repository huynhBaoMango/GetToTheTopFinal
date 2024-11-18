using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager instance2;
    public GameObject StartGameButton;

    public readonly SyncList<PlayerLobby> players = new SyncList<PlayerLobby>();

    private void Awake()
    {
        instance2 = this;
    }

    private void Update()
    {
        if (!IsServerInitialized)
            return;

        bool allPlayersReady = players.Count > 0;
        foreach (PlayerLobby player in players)
        {
            if (!player.IsReady)
                allPlayersReady = false;
        }

        if (allPlayersReady && !StartGameButton.activeSelf)
            StartGameButton.SetActive(true);
        else if (!allPlayersReady && StartGameButton.activeSelf)
            StartGameButton.SetActive(false);
    }
}
