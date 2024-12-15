using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CheckNewConnect : NetworkBehaviour
{
    [SerializeField] NetworkObject CharacterPrefab;
    [SerializeField] NetworkObject[] ListCharPrefab;
    public override void OnSpawnServer(NetworkConnection connection)
    {
        if(PlayerPrefs.GetInt("prevCharacterId", -1) == -1)
        {
            Spawn(Instantiate(CharacterPrefab, spawnPointStatic.instance.transform.position, Quaternion.identity), connection, UnityEngine.SceneManagement.SceneManager.GetSceneByName(PlayerPrefs.GetString("sceneToLoad", gameObject.scene.name)));
        }

        if (PlayerPrefs.GetInt("prevCharacterId", -1) == 0)
        {
            Spawn(Instantiate(ListCharPrefab[0], spawnPointStatic.instance.transform.position, Quaternion.identity), connection, UnityEngine.SceneManagement.SceneManager.GetSceneByName(PlayerPrefs.GetString("sceneToLoad", gameObject.scene.name)));
        }

        if (PlayerPrefs.GetInt("prevCharacterId", -1) == 1)
        {
            Spawn(Instantiate(ListCharPrefab[1], spawnPointStatic.instance.transform.position, Quaternion.identity), connection, UnityEngine.SceneManagement.SceneManager.GetSceneByName(PlayerPrefs.GetString("sceneToLoad", gameObject.scene.name)));
        }

        if (PlayerPrefs.GetInt("prevCharacterId", -1) == 2)
        {
            Spawn(Instantiate(ListCharPrefab[2], spawnPointStatic.instance.transform.position, Quaternion.identity), connection, UnityEngine.SceneManagement.SceneManager.GetSceneByName(PlayerPrefs.GetString("sceneToLoad", gameObject.scene.name)));
        }
    }
}

