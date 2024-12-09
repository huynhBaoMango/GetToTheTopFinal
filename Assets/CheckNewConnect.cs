using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CheckNewConnect : NetworkBehaviour
{
    [SerializeField] NetworkObject CharacterPrefab;

    public override void OnSpawnServer(NetworkConnection connection)
    {
        Spawn(Instantiate(CharacterPrefab, spawnPointStatic.instance.transform.position, Quaternion.identity), connection, gameObject.scene);
    }
}
