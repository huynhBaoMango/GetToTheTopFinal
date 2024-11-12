using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : NetworkBehaviour
{
    [Header("Resource setup")]
    [SerializeField] private List<GameObject> resources = new List<GameObject>();
    [SerializeField] private float spacing = 0.3f;
    [SerializeField][Range(0, 100)] private float spawnChance = 10f;
    [SerializeField] private LayerMask worldLayer;
    [SerializeField] private float minHeight = 0;

    [Header("Area setup")]
    [SerializeField] private Vector3 centerOfSpawn;
    [SerializeField] private float spawnRadius;

    private int seed;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsServer)
        {
            seed = Random.Range(0, 999999);
            SpawnResources(LocalConnection, seed);
        }
        else
        {
            PlayerReady(LocalConnection);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerReady(NetworkConnection target)
    {
        SpawnResources(target, seed);
    }

    [TargetRpc]
    void SpawnResources(NetworkConnection conn, int seed)
    {
        Random.InitState(seed);

        for (float x = centerOfSpawn.x - spawnRadius; x < centerOfSpawn.x + spawnRadius; x += spacing)
        {
            for (float z = centerOfSpawn.z - spawnRadius; z < centerOfSpawn.z + spawnRadius; z += spacing)
            {
                Vector3 raycastPosition = new Vector3(x, centerOfSpawn.y + spawnRadius, z);
                if (Physics.Raycast(raycastPosition, Vector3.down, out RaycastHit hit, spawnRadius * 2, worldLayer))
                {
                    if (hit.point.y >= minHeight && Random.Range(0f, 100f) < spawnChance)
                    {
                        Instantiate(resources[Random.Range(0, resources.Count)], hit.point,
                            Quaternion.Euler(new Vector3(0, Random.Range(0f, 360f), 0)));
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(centerOfSpawn, new Vector3(spawnRadius, spawnRadius, spawnRadius));
    }
}