using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loadNextLevel : NetworkBehaviour
{
    public void Awake()
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

        string sceneToLoad = "NewTest";

        PlayerPrefs.SetString("sceneToLoad", sceneToLoad);
        BootstrapNetworkManager.ChangeNetworkScene(sceneToLoad, scenesToClose);
    }
}
