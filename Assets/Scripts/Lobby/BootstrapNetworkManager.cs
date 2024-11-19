using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Linq;
using UnityEngine;

public class BootstrapNetworkManager : NetworkBehaviour
{
    private static BootstrapNetworkManager instanceBootNet;

    private void Awake() => instanceBootNet = this;

    public static void ChangeNetworkScene(string sceneName, string[] scenesToClose)
    {
        instanceBootNet.CloseScenes(scenesToClose);
        
        SceneLoadData sld = new SceneLoadData(sceneName);
        NetworkConnection[] conns = instanceBootNet.ServerManager.Clients.Values.ToArray();
        instanceBootNet.SceneManager.LoadConnectionScenes(conns, sld);
    }


    [ServerRpc(RequireOwnership = false)]
    void CloseScenes(string[] scenesToClose)
    {
        CloseScenesObserver(scenesToClose);
    }

    [ObserversRpc]
    void CloseScenesObserver(string[] scenesToClose)
    {
        foreach (var sceneName in scenesToClose)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
