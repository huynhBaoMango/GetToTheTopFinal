using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : NetworkBehaviour
{
    [SerializeField] private List<GameObject> players = new List<GameObject>();
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject canvasObject;


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            canvasObject.SetActive(false);
        }
    }

    public void SpawnSkin1()
    {
        characterSelectPanel.SetActive(false);
        SpawnPlayer(0, LocalConnection);
    }

    public void SpawnSkin2()
    {
        characterSelectPanel.SetActive(false);
        SpawnPlayer(1, LocalConnection);
    }

    public void SpawnSkin3()
    {
        characterSelectPanel.SetActive(false);
        SpawnPlayer(2, LocalConnection);
    }


    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayer(int id, NetworkConnection conn)
    {
        GameObject player = Instantiate(players[id], spawnPointStatic.instance.transform.position, Quaternion.identity);
        Spawn(player, conn, UnityEngine.SceneManagement.SceneManager.GetSceneByName(PlayerPrefs.GetString("sceneToLoad", gameObject.scene.name)));
    }
}
