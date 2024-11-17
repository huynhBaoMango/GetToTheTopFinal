//using FishNet.Object;
//using FishNet.Object.Synchronizing;
//using NaughtyAttributes;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class PlayerLobby : NetworkBehaviour
//{
//    public static PlayerLobby instance3;

//    [SerializeField]
//    GameObject lobbyPlayerCard;

//    [SerializeField]
//    TextMeshProUGUI playerNameText;

//    [SerializeField]
//    TextMeshProUGUI playerReadyText;


//    [SyncVar]
//    public bool IsReady;

//    [SyncVar]
//    public string playerName;

//    [field: SerializeField]
//    [field: SyncVar(OnChange = nameof(OnChangePlayerReady))]
//    public bool IsReady { get; private set; }

//    [field: SerializeField]
//    [field: SyncVar(OnChange = nameof(OnChangePlayerName))]
//    public string playerName { get; private set; }

//    private void OnChangePlayerReady(bool oldValue, bool newValue, bool isServer)
//    {
//        if (!isServer)
//        {
//            playerNameText.text = playerName;
//            if (IsReady)
//                playerReadyText.text = "READY";
//            else playerReadyText.text = "NOT READY";
//        }

//    }

//    private void OnChangePlayerName(bool oldValue, bool newValue, bool isServer)
//    {
//        if (!isServer)
//        {
//            playerNameText.text = playerName;
//            if (IsReady)
//                playerReadyText.text = "READY";
//            else playerReadyText.text = "NOT READY";
//        }
//    }

//    //Cap nhat khi playerReady thay doi
//    private void UpdatePlayerReadyText()
//    {
//        if (IsReady)
//            playerReadyText.text = "READY";
//        else
//            playerReadyText.text = "NOT READY";
//    }

//    // Cap nhat khi playerName thay doi
//    private void UpdatePlayerNameText()
//    {
//        playerNameText.text = playerName;
//    }

//    public override void OnStartClient()
//    {
//        base.OnStartClient();

//        GameObject playersCardPanel = GameObject.Find("Panel_Players");
//        if (!playersCardPanel)
//            Debug.LogError("UI lobby panel for players not found");
//        else
//        {
//            GameObject playerCard = Instantiate(lobbyPlayerCard, playersCardPanel.transform);
//            playerNameText = playerCard.GetComponentInChildren<TextMeshProUGUI>();
//            playerReadyText = playerCard.transform.Find("txtPlayerReady")?.GetComponent<TextMeshProUGUI>();
//            if (!playerNameText)
//                Debug.LogError("playerNameText not found");
//        }

//        if (!IsOwner)
//            return;

//        instance3 = this;
//        ChangePlayerName("Player: " + OwnerId);
//    }

//    public override void OnStartServer()
//    {
//        base.OnStartServer();
//        GameplayManager.instance2.players.Add(this);
//    }

//    [ServerRpc]
//    public void ToggleReadyState()
//    {
//        IsReady = !IsReady;
//        UpdatePlayerReadyText(); // Cap nhat trang thai "READY" hay "NOT READY"
//    }

//    [ServerRpc]
//    public void ChangePlayerName(string name)
//    {
//        name = Common.instance.currentUser.username;
//        playerName = name;
//        UpdatePlayerNameText(); // Cap nhat ten nguoi choi
//    }
//}
