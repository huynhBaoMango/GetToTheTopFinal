using FishNet.Managing;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BootstrapManager : MonoBehaviour
{
    private static BootstrapManager instanceBootstrap;

    private void Awake() => instanceBootstrap = this;

    [SerializeField] private string menuName = "Menu";
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;

    public static ulong CurrentLobbyID;


    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }
    public void GoToMenu()
    {
        SceneManager.LoadScene(menuName, LoadSceneMode.Additive);
    }

    public static void BackToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
        Cursor.lockState = CursorLockMode.None;

        LeaveLobby();
    }

    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        //Debug.Log("Starting lobby creation" + callback.m_eResult.ToString());
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        CurrentLobbyID = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s lobby ");
        fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        fishySteamworks.StartConnection(true);
        Debug.Log("Lobby creation was successful");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t callback) 
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        if (networkManager.IsServerStarted)
            MainMenuManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name"), networkManager.IsServerStarted);
            CurrentLobbyID = callback.m_ulSteamIDLobby;
            SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s lobby ");

        fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress"));
        fishySteamworks.StartConnection(false);
    }

    public static void JoinByID(CSteamID steamID)
    {
        Debug.Log("Attemping to join lobby with ID: " + steamID.m_SteamID);
        if(SteamMatchmaking.RequestLobbyData(steamID))
            SteamMatchmaking.JoinLobby(steamID);
        else
            Debug.Log("Failed to join lobby with ID: " + steamID.m_SteamID);

    }

    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;

        instanceBootstrap.fishySteamworks.StopConnection(false);
        if(instanceBootstrap.networkManager.IsServerStarted)
            instanceBootstrap.fishySteamworks.StopConnection(true);
    }
}
