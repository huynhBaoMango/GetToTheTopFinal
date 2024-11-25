using FishNet.Object;
using UnityEngine;

public class VendingInteraction : NetworkBehaviour
{
    private Camera playerCamera;
    [SerializeField] private float cameraYOffset = 0.7f;
    [SerializeField] private float interactionRange = 1f;
    [SerializeField] private GameObject vendingPanel;
    private PlayerWeapon playerWeapon;

    private bool isPanelActive = false;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            playerCamera = Camera.main;
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            playerCamera.transform.SetParent(transform);
            playerWeapon = FindObjectOfType<PlayerWeapon>();

        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (!isPanelActive && Input.GetMouseButtonDown(1))
        {
            ShowPanel();
        }

        if (isPanelActive && Input.GetKeyDown(KeyCode.Escape))
        {
            HidePanel();
        }
    }

    [ServerRpc]
    private void ShowPanelServerRpc()
    {
        ShowPanelClientRpc();
    }

    [ObserversRpc]
    private void ShowPanelClientRpc()
    {
        if (IsOwner)
        {
            vendingPanel.SetActive(true);
            isPanelActive = true;

            if (playerWeapon != null)
            {
                playerWeapon.SetCanFire(false);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    [ServerRpc]
    private void HidePanelServerRpc()
    {
        HidePanelClientRpc();
    }

    [ObserversRpc]
    private void HidePanelClientRpc()
    {
        if (IsOwner)
        {
            vendingPanel.SetActive(false);
            isPanelActive = false;

            if (playerWeapon != null)
            {
                playerWeapon.SetCanFire(true);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ShowPanel()
    {
        ShowPanelServerRpc();
    }

    private void HidePanel()
    {
        HidePanelServerRpc();
    }
}
