using FishNet.Object;
using UnityEngine;

public class PlayerPickup : NetworkBehaviour
{
    [Header("Cài Đặt Nhặt Đồ Chung")]
    [SerializeField] private float pickUpRange = 4f;
    [SerializeField] private KeyCode pickUpKey = KeyCode.F;
    [SerializeField] private KeyCode dropButton = KeyCode.Q;
    [SerializeField] private KeyCode rotateButton = KeyCode.Z;
    [SerializeField] private LayerMask pickUpLayers;

    [Header("Cài Đặt Nhặt Vật Phẩm")]
    [SerializeField] private float raycastDistance = 4f;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private Transform pickupPosition;

    [Header("Cài Đặt Thả Vật")]
    [SerializeField] private float dropDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Hiệu Ứng Hình Ảnh")]
    [SerializeField] private Material transparentMaterial;

    private Transform cameraTransform;
    private PlayerWeapon _playerWeapon;
    private Camera cam;
    private bool hasObjectInHand;
    private GameObject objInHand;
    private Transform worldObjectHolder;
    private float rotationAmount = 0f;
    private enum RotationAxis { X, Y }
    private RotationAxis currentRotationAxis = RotationAxis.Y;
    private Material originalMaterial;
    private bool originalIsTrigger;

    private float groundOffset = 1f; // Thêm một offset nhỏ để đảm bảo vật phẩm nằm trên mặt đất

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        cameraTransform = Camera.main.transform;
        cam = Camera.main;
        if (GameObject.FindGameObjectWithTag("WorldObjects") != null)
        {
            worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;
        }

        if (TryGetComponent(out PlayerWeapon plWeapon))
        {
            _playerWeapon = plWeapon;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(pickUpKey))
        {
            PickUp();
        }

        if (Input.GetKeyDown(dropButton))
        {
            Drop();
        }

        if (Input.GetKeyDown(rotateButton))
        {
            currentRotationAxis = (currentRotationAxis == RotationAxis.Y) ? RotationAxis.X : RotationAxis.Y;
        }

        if (hasObjectInHand && Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            RotateObject(Input.GetAxis("Mouse ScrollWheel") * 90f);
        }

        if (hasObjectInHand)
        {
            UpdateObjectPosition();
        }
    }

    private void PickUp()
    {
        // Kiểm tra nếu đã có vật phẩm trong tay thì thả trước khi nhặt mới
        if (hasObjectInHand)
        {
            Drop(); // Thả vật phẩm đang cầm trước
        }

        // Cast ray từ tâm màn hình thay vì từ vị trí người chơi
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hitObj, raycastDistance, pickupLayer))
        {
            SetObjectInHand(hitObj.transform.gameObject);
        }
    }

    private void Drop()
    {
        if (!hasObjectInHand)
            return;

        // Tách vật phẩm khỏi người chơi

        // Cast ray từ tâm màn hình để tìm vị trí thả trên sàn
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(screenCenter);
        Vector3 dropPosition;

        if (Physics.Raycast(ray, out RaycastHit hit, dropDistance, groundLayer))
        {
            dropPosition = hit.point + Vector3.up * groundOffset; // Offset để đảm bảo vật phẩm không bị lún
        }
        else
        {
            dropPosition = cameraTransform.position + cameraTransform.forward * dropDistance + Vector3.up * groundOffset;
        }

        // Gọi hàm thả trên server
        DropObjectServer(objInHand, dropPosition, worldObjectHolder);

        // Khôi phục thuộc tính ban đầu
        RestoreObjectProperties();

        // Xóa tham chiếu đến vật phẩm
        objInHand = null;
        hasObjectInHand = false;
    }

    private void RestoreObjectProperties()
    {
        if (objInHand == null)
            return;

        // Đặt lại parent về null trước khi khôi phục các thuộc tính khác

        objInHand.GetComponent<Renderer>().material = originalMaterial;

        if (objInHand.GetComponent<Rigidbody>() != null)
            objInHand.GetComponent<Rigidbody>().isKinematic = false;

        if (objInHand.GetComponent<Collider>() != null)
            objInHand.GetComponent<Collider>().isTrigger = originalIsTrigger;
    }

    private void SetObjectInHand(GameObject obj)
    {
        // Gán vật phẩm mới vào tay
        objInHand = obj;
        hasObjectInHand = true;

        // Lưu các thuộc tính ban đầu
        originalMaterial = objInHand.GetComponent<Renderer>().material;
        objInHand.GetComponent<Renderer>().material = transparentMaterial;

        if (objInHand.GetComponent<Rigidbody>() != null)
            objInHand.GetComponent<Rigidbody>().isKinematic = true;

        if (objInHand.GetComponent<Collider>() != null)
        {
            originalIsTrigger = objInHand.GetComponent<Collider>().isTrigger;
            objInHand.GetComponent<Collider>().isTrigger = true;
        }

        // Đặt vật phẩm tại vị trí tay
        objInHand.transform.localPosition = Vector3.zero; // Căn chỉnh về vị trí gốc
        objInHand.transform.rotation = Quaternion.Euler(Vector3.zero);
        // Đảm bảo vật phẩm không bị lún vào mặt đất
        PositionObjectOnGround();
    }

    private void RotateObject(float amount)
    {
        if (!hasObjectInHand)
            return;

        rotationAmount += amount;
        RotateObjectServer(objInHand, rotationAmount, currentRotationAxis);
    }

    [ObserversRpc]
    private void UpdateObjectPosition()
    {
        Vector3 dropPosition;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 6f, groundLayer))
        {
            objInHand.transform.position = hit.point + new Vector3(0, objInHand.GetComponent<Collider>().bounds.size.y, 0);
        }
        else
        {
            //objInHand.transform.position = cameraTransform.position + cameraTransform.forward * dropDistance + Vector3.up * groundOffset;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(cam.transform.position, cam.transform.forward);
    }

    [ServerRpc(RequireOwnership = false)]
    void RotateObjectServer(GameObject obj, float amount, RotationAxis axis)
    {
        RotateObjectObserver(obj, amount, axis);
    }

    [ObserversRpc]
    void RotateObjectObserver(GameObject obj, float amount, RotationAxis axis)
    {
        if (axis == RotationAxis.X)
        {
            obj.transform.localRotation = Quaternion.Euler(amount, 0f, 0f);
        }
        else
        {
            obj.transform.localRotation = Quaternion.Euler(0f, amount, 0f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetObjectInHandServer(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        SetObjectInHandObserver(obj, position, rotation, player);
    }

    [ObserversRpc]
    void SetObjectInHandObserver(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = true;

        if (obj.GetComponent<Collider>() != null)
            obj.GetComponent<Collider>().isTrigger = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void DropObjectServer(GameObject obj, Vector3 dropPosition, Transform worldHolder)
    {
        DropObjectObserver(obj, dropPosition, worldHolder);
    }

    [ObserversRpc]
    void DropObjectObserver(GameObject obj, Vector3 dropPosition, Transform worldHolder)
    {
        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = false;

        if (obj.GetComponent<Collider>() != null)
            obj.GetComponent<Collider>().isTrigger = originalIsTrigger; // Sửa đổi: Đặt lại trạng thái isTrigger ban đầu
    }

    private void PositionObjectOnGround()
    {
        /*
        if (Physics.Raycast(objInHand.transform.position, Vector3.down, out RaycastHit hit, pickUpRange, groundLayer))
        {
            objInHand.transform.position = hit.point + new Vector3(0, objInHand.GetComponent<Collider>().bounds.size.y, 0); // Offset để tránh lún
        }
        */
    }
}
