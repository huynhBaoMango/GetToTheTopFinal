using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;

public class loadNextLevel : NetworkBehaviour
{
    public void Awake()
    {
        if (!base.IsHost)
        {
            Invoke("waitSec", 3f);
        }
    }


    void waitSec()
    {
        waitSecObserver();
    }

    void waitSecObserver()
    {
        string[] mapsName =
        {
            "map0",
            "map1",
            "map2",
            "map3"
        };
        string sceneToLoad = mapsName[Random.Range(0, mapsName.Length)];

        string[] scenesToClose = new string[]
        {
            "Loading"
        };


        PlayerPrefs.SetString("sceneToLoad", sceneToLoad);
        BootstrapNetworkManager.ChangeNetworkScene(sceneToLoad, scenesToClose);
    }
}
