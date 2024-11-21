using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> characters = new List<GameObject>();
    public static CustomPlayerSpawner Instance;

    private void Awake()
    {
        Instance = this;
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayer(int idcharacter, Vector3 spawnPos, NetworkConnection conn)
    {
        GameObject player = Instantiate(characters[idcharacter], spawnPointStatic.instance.transform.position, Quaternion.identity);
        Spawn(player, conn);
    }
}
