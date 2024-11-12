using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor.Rendering;
using UnityEngine;

public class ZombieSpawn : NetworkBehaviour
{
    [SerializeField] private NetworkObject zombiePrefab;
    [SerializeField] private Vector3 spawnSize;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private LayerMask spawnLayerMask;
    private float _lastSpawnTime;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsServerInitialized)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (_lastSpawnTime > Time.time - spawnInterval)
            return;
        _lastSpawnTime = Time.time;
        SpawnZombie();
    }

    private void SpawnZombie()
    {
        float x = UnityEngine.Random.Range(-spawnSize.x * 0.5f, spawnSize.x * 0.5f) + transform.position.x;
        float z = UnityEngine.Random.Range(-spawnSize.z * 0.5f, spawnSize.z * 0.5f) + transform.position.z;

        if (!Physics.Raycast(new Vector3(x, transform.position.y + spawnSize.y / 2, z), Vector3.down, out RaycastHit hit, spawnSize.y, spawnLayerMask))
            return;
        NetworkObject zombie = Instantiate(zombiePrefab, hit.point, Quaternion.identity); 
        ServerManager.Spawn(zombie);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnSize);
    }
}
