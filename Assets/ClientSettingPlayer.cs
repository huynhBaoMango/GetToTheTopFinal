using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientSettingPlayer : NetworkBehaviour
{
    [SerializeField] public GameObject CamPlayer;
    [SerializeField] private PlayerInput playerInput;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            gameObject.GetComponent<ClientSettingPlayer>().enabled = false;
            CamPlayer.SetActive(false);
            //playerInput.enabled = false;
        }
    }
}
