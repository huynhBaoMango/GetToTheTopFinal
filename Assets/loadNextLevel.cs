using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loadNextLevel : NetworkBehaviour
{
    void Awake()
    {
        Invoke("waitSec", 3f);
    }

    void waitSec()
    {
        Debug.Log("AAAAAAAAAAAAA");
        string[] scenesToClose = new string[]
        {
            "Loading"
        };

        BootstrapNetworkManager.ChangeNetworkScene("NewTest", scenesToClose);
    }
}