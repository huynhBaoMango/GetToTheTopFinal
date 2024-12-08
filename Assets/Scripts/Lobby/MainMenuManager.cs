using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Transporting;
using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instanceMenu;

    [SerializeField] private GameObject joinScreen, mainMenuScreen, lobbyScreen;
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI lobbyTitle, lobbyIDText;
    [SerializeField] private Button startGameButton;

    private enum ScreenState { MainMenu, JoinMenu, Lobby, None }
    private ScreenState currentScreenState = ScreenState.None;

    private void Awake() => instanceMenu = this;

    private void Start()
    {
        switch (currentScreenState)
        {
            case ScreenState.MainMenu:
                OpenMainMenu();
                break;
            case ScreenState.JoinMenu:
                OpenJoinMenu();
                break;
            default:
                OpenMainMenu(); // Mặc định quay về MainMenu
                break;
        }
    }

    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
    }

    public void OpenMainMenu()
    {
        CloseAllScreens();
        mainMenuScreen.SetActive(true);
        currentScreenState = ScreenState.MainMenu;
    }

    public void OpenLobby()
    {
        CloseAllScreens();
        lobbyScreen.SetActive(true);
        currentScreenState = ScreenState.Lobby;
    }
    public void OpenJoinMenu()
    {
        CloseAllScreens();
        joinScreen.SetActive(true);
        currentScreenState = ScreenState.JoinMenu;
    }
    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instanceMenu.lobbyTitle.text = lobbyName;
        instanceMenu.startGameButton.gameObject.SetActive(isHost);
        instanceMenu.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        instanceMenu.OpenLobby();
    }

    void CloseAllScreens()
    {
        mainMenuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        joinScreen.SetActive(false);
        currentScreenState = ScreenState.None;
    }

    public void JoinLobby()
    {
        CSteamID steamID = new CSteamID(Convert.ToUInt64(lobbyInput.text));
        BootstrapManager.JoinByID(steamID);
    }
    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
        OpenMainMenu();

    }

    public void StartGame()
    {
        string[] scenesToClose = new string[]
        {
            "Menu"
        };

        BootstrapNetworkManager.ChangeNetworkScene("Loading", scenesToClose);
    }
}
