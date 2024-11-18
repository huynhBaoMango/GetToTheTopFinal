using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawnController : NetworkBehaviour
{
    public List<GameObject> zombieSpawns;

    private void Awake()
    {
        zombieSpawns = new List<GameObject>();
        foreach (Transform t in transform)
        {
            zombieSpawns.Add(t.gameObject);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsServerInitialized)
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


    public void UpdateZombieSpawnRenderObserver()
    {
        foreach (GameObject go in zombieSpawns)
        {
            go.GetComponent<Renderer>().enabled = false;
            Debug.Log("AAAAA");
        }
    }


    public void EnableGivenSpawn(int i)
    {
        if (zombieSpawns[i].TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.enabled = true;
        }
        EnableGivenSpawnObserver(i);
    }

    [ObserversRpc]
    void EnableGivenSpawnObserver(int i)
    {
        if (zombieSpawns[i].TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.enabled = true;
        }
    }


}
