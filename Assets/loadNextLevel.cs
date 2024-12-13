using FishNet.Object;
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

    [ObserversRpc]
    void waitSecObserver()
    {
        Cursor.lockState = CursorLockMode.None;
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

        PlayerPrefs.SetInt("currentMoney", 3000);
        PlayerPrefs.SetString("sceneToLoad", sceneToLoad);
        BootstrapNetworkManager.ChangeNetworkScene(sceneToLoad, scenesToClose);
    }
}
