using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawnController : NetworkBehaviour
{
    public List<GameObject> zombieSpawns = new List<GameObject>();

    private void Awake()
    {
        foreach (Transform t in transform)
        {
            zombieSpawns.Add(t.gameObject);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {

        }
        else
        {
            enabled = false;
        }
    }


    public void DisableAllZombieSpawns()
    {
        UpdateZombieSpawnRenderObserver();
    }

    [ObserversRpc]
    public void UpdateZombieSpawnRenderObserver()
    {
        foreach (GameObject go in zombieSpawns)
        {
            go.SetActive(false);
            Debug.Log("AAAAA");
        }
    }

    public void EnableGivenSpawn(int i)
    {
        EnableGivenSpawnObserver(i);
    }

    [ObserversRpc]
    void EnableGivenSpawnObserver(int i)
    {
        zombieSpawns[i].SetActive(true);
    }


}
