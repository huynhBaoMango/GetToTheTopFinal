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

    [SerializeField] private GameObject mainMenuScreen, lobbyScreen;
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI lobbyTitle, lobbtIDText;
    [SerializeField] private Button startGameButton;

    private void Awake() => instanceMenu = this;

    private void Start()
    {
        OpenMainMenu(); 
    }

    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
    }

    public void OpenMainMenu()
    {
        CloseAllScreens();
        mainMenuScreen.SetActive(true);
    }

    public void OpenLobby()
    {
        CloseAllScreens();
        lobbyScreen.SetActive(true);
    }

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instanceMenu.lobbyTitle.text = lobbyName;
        instanceMenu.startGameButton.gameObject.SetActive(isHost);
        instanceMenu.lobbtIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        instanceMenu.OpenLobby();
    }

    void CloseAllScreens()
    {
        mainMenuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
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

        BootstrapNetworkManager.ChangeNetworkScene("NewTest", scenesToClose);
    }
}
