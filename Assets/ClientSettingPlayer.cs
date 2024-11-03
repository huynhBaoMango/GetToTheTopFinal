using FishNet.Object;
using FishNet.Connection;
using scgFullBodyController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSettingPlayer : NetworkBehaviour
{
    [SerializeField] public GameObject CamPlayer;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            gameObject.GetComponent<ClientSettingPlayer>().enabled = false;
            CamPlayer.SetActive(false);
        }
    }
}
