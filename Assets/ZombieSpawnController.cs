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
            foreach(GameObject t in transform)
            {
                zombieSpawns.Add(t);
            }
        }
        else
        {
            enabled = false;
        }
    }

    [ServerRpc]
    public void DisableAllZombieSpawns()
    {
        UpdateZombieSpawnRenderObserver();
    }


    public void UpdateZombieSpawnRenderObserver()
    {
        foreach (GameObject go in zombieSpawns)
        {
            go.SetActive(false);
            Debug.Log("AAAAA");
        }
    }

}
