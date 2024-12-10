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
        Spawn(Instantiate(CharacterPrefab, spawnPointStatic.instance.transform.position, Quaternion.identity), connection, UnityEngine.SceneManagement.SceneManager.GetSceneByName(PlayerPrefs.GetString("sceneToLoad", gameObject.scene.name)));
    }

    private void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players)
        {
            player.transform.position = spawnPointStatic.instance.transform.position;
        }
    }
}

