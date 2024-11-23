using FishNet.Example.ColliderRollbacks;
using FishNet.Example.Scened;
using FishNet.Object;
using UnityEngine;

public class VendingInteraction : NetworkBehaviour
{
    private Camera playerCamera; // Camera của nhân vật
    [SerializeField] private float cameraYOffset = 0.7f;
    [SerializeField] private float interactionRange = 1f; // Khoảng cách để tương tác
    [SerializeField] private GameObject vendingPanel; // Panel cần hiển thị
    private PlayerWeapon playerWeapon;

    private bool isPanelActive = false; // Kiểm tra trạng thái panel

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            playerCamera = Camera.main;
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            playerCamera.transform.SetParent(transform);
        }
        else
        {
            GetComponent<VendingInteraction>().enabled = false;
        }
    }
    void Update()
    {
        if (!isPanelActive) // Chỉ xử lý input khi panel chưa mở
        {
            // Kiểm tra khi nhấn phím E
            if (Input.GetKeyDown(KeyCode.E))
            {
                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                RaycastHit hit;

                // Nếu raycast trúng một đối tượng
                if (Physics.Raycast(ray, out hit, interactionRange))
                {
                    // Kiểm tra nếu đối tượng là Vending
                    if (hit.collider.CompareTag("Vending"))
                    {
                        Debug.Log("Open Panel");
                        ShowPanel(); // Hiển thị panel
                    }
                }
            }
        }

        // Ẩn panel nếu nhấn phím ESC
        if (isPanelActive && Input.GetKeyDown(KeyCode.Escape))
        {
            HidePanel(); // Tắt panel
        }
    }

    private void ShowPanel()
    {
        vendingPanel.SetActive(true);
        isPanelActive = true;

        // Vô hiệu hóa script bắn súng
        if (playerWeapon != null)
        {
            playerWeapon.SetCanFire(false);
        }

        // Khóa chuột (nếu cần)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HidePanel()
    {
        vendingPanel.SetActive(false);
        isPanelActive = false;

        // Bật lại script bắn súng
        if (playerWeapon != null)
        {
            playerWeapon.SetCanFire(true);
        }

        // Ẩn chuột (nếu cần)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
