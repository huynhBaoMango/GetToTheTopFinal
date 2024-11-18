using FishNet.Object;
using FishNet.Connection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet.Managing.Scened;
using FishNet;
using FishNet.Managing;

public class LobbyManager : NetworkBehaviour
{
    public TMP_InputField lobbyNameInput;
    public TMP_InputField joinCodeInput;
    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI lobbyCodeText;
    public TextMeshProUGUI playersListText;

    private List<NetworkConnection> playersInLobby = new List<NetworkConnection>();
    private string lobbyCode;

    void Update()
    {
        lobbyNameText.text = "Lobby name: " + lobbyNameInput.text;
        lobbyCodeText.text = "Lobby code: " + lobbyCode;
    }

    // Khi nhấn nút tạo lobby
    public void OnCreateLobbyClicked()
    {
        Debug.Log($"IsServer: {IsServerInitialized}");  // In ra trạng thái server
        Debug.Log($"IsClient: {IsClientInitialized}");  // In ra trạng thái client

        if (!IsServerInitialized)
        {
            Debug.LogError("Chỉ server mới có thể tạo lobby!");
            return;
        }

        lobbyCode = GenerateLobbyCode();
        playersInLobby.Clear();
        Debug.Log($"Lobby được tạo với mã: {lobbyCode}");
    }

    // Khi nhấn nút tham gia lobby
    public void OnJoinLobbyClicked()
    {
        if (!IsClientInitialized)
        {
            Debug.LogError("Chỉ client mới có thể tham gia lobby!");
            return;
        }

        string code = joinCodeInput.text;
        ServerRequestJoinLobby(code); // Gọi hàm phía server
    }

    // Server: Xử lý yêu cầu tham gia lobby
    [Server]
    private void ServerRequestJoinLobby(string code, NetworkConnection conn = null)
    {
        if (code != lobbyCode)
        {
            conn.Disconnect(false); // Đóng kết nối nếu sai mã
            Debug.LogWarning("Sai mã lobby!");
            return;
        }

        playersInLobby.Add(conn);
        ClientUpdatePlayerList(); // Gọi client-side để cập nhật danh sách
    }

    // Client: Cập nhật danh sách người chơi
    [Client]
    private void ClientUpdatePlayerList()
    {
        playersListText.text = "Người chơi trong lobby:\n";
        foreach (var player in playersInLobby)
        {
            playersListText.text += $"{player.ClientId}\n";
        }
    }

    // Tạo mã lobby ngẫu nhiên
    private string GenerateLobbyCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] code = new char[6];
        System.Random random = new System.Random();

        for (int i = 0; i < code.Length; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }

        return new string(code);
    }

    // Khi nhấn nút Start Game
    public void OnStartGameClicked()
    {
        if (!IsServerInitialized)
        {
            Debug.LogError("Chỉ server mới có thể bắt đầu game!");
            return;
        }

        // Chuyển scene cho tất cả người chơi
        ChangeSceneForAllPlayers("NewTest");
    }

    [Server]
    private void ChangeSceneForAllPlayers(string sceneName)
    {
        // Tạo dữ liệu scene
        SceneLoadData sceneLoadData = new SceneLoadData(sceneName)
        {
            ReplaceScenes = ReplaceOption.All, // Thay thế tất cả scene hiện tại
        };

        // Tải scene cho tất cả người chơi
        InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
    }
}
