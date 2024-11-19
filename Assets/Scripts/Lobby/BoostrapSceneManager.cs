using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Managing.Scened;

public class BoostrapSceneManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (!InstanceFinder.IsServerStarted)
        {
            return;
        }
        LoadScene("New Test");
        UnLoadScene("Menu");
    }

    public void OnCreateClick()
    {
        LoadScene("New Test");
        UnLoadScene("Menu");
    }

    void LoadScene(string sceneName)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            return;
        }   

        SceneLoadData sld = new SceneLoadData(sceneName);
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }
    void UnLoadScene(string sceneName)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            return;
        }   

        SceneUnloadData sld = new SceneUnloadData(sceneName);
        InstanceFinder.SceneManager.UnloadGlobalScenes(sld);
    }


}
