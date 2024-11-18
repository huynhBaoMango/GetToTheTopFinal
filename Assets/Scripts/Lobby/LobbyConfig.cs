using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;


public class LobbyConfig : MonoBehaviour
{
    [SerializeField, Scene]
    private string gameScene;

    private NetworkManager networkManager;
    private bool startedAsHost = false;

    private LocalConnectionState clientState = LocalConnectionState.Stopped;
    private LocalConnectionState serverState = LocalConnectionState.Stopped;

    private void Awake()
    {
        InitializeOnce();
    }
    private void InitializeOnce()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found, HUD will not function.");
            return;
        }
        else
        {
            networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
            networkManager.SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
        }
    }

    private void OnDestroy()
    {
        if (networkManager == null)
            return;

        networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        networkManager.SceneManager.OnLoadEnd -= SceneManager_OnLoadEnd;
    }

    private void SceneManager_OnLoadEnd(SceneLoadEndEventArgs obj)
    {
        Button readyButton = GameObject.Find("Button_ReadyGame").GetComponent<Button>();
        Button startGameButton = GameObject.Find("Button_StartGame").GetComponent<Button>();
        if (networkManager.IsServerStarted)
        {
            readyButton.gameObject.SetActive(true);
            startGameButton.onClick.AddListener(() => { LoadGameScene(); });
            GameplayManager.instance2.StartGameButton = startGameButton.gameObject;

            if (startedAsHost)
            {
                readyButton.onClick.AddListener(() =>
                {
                    if (!PlayerLobby.instance3)
                        Debug.Log("Player not loaded yet");
                    else
                        PlayerLobby.instance3.ToggleReadyState();
                   
                    //startGameButton.;
                });
            }
            else
            {
                readyButton.gameObject.SetActive(false);
            }
        }
        if (networkManager.IsClientStarted)
        {
            readyButton.gameObject.SetActive(true);
            readyButton.onClick.AddListener(() =>
            {
                if (!PlayerLobby.instance3)
                    Debug.Log("Player not loaded yet");
                else
                    PlayerLobby.instance3.ToggleReadyState();
            });
            if (!networkManager.IsServerStarted)
            {
                startGameButton.gameObject.SetActive(false);
            }
        }
    }

    private void LoadGameScene()
    {
        SceneLoadData sld = new SceneLoadData(GetSceneName(gameScene));
        sld.ReplaceScenes = ReplaceOption.None;
        networkManager.SceneManager.LoadGlobalScenes(sld);
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        clientState = obj.ConnectionState;

        if (clientState == LocalConnectionState.Stopped)
        {
            if (!networkManager.IsServerStarted)
                Debug.Log("Khong thee");
        }
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        serverState = obj.ConnectionState;

        if (serverState == LocalConnectionState.Started)
        {
            if (!networkManager.ServerManager.OneServerStarted())
                return;

            //SceneLoadData sld = new SceneLoadData(GetSceneName(gameScene));
            //sld.ReplaceScenes = ReplaceOption.None;
            //networkManager.SceneManager.LoadGlobalScenes(sld);
        }
        else if (serverState == LocalConnectionState.Stopped)
        {
            Debug.Log("Khongg the");
        }
    }

    private string GetSceneName(string fullPath)
    {
        return Path.GetFileNameWithoutExtension(fullPath);
    }
    public void OnClick_Server()
    {
        startedAsHost = false;
        if (networkManager == null)
            return;

        if (serverState != LocalConnectionState.Stopped)
            networkManager.ServerManager.StopConnection(true);
        else
            networkManager.ServerManager.StartConnection();

    }


    public void OnClick_Client()
    {
        startedAsHost = false;
        if (networkManager == null)
            return;

        if (clientState != LocalConnectionState.Stopped)
            networkManager.ClientManager.StopConnection();
        else
            networkManager.ClientManager.StartConnection();
    }

    public void OnClick_Host()
    {
        OnClick_Server();
        OnClick_Client();
        startedAsHost = true;
    }
}
