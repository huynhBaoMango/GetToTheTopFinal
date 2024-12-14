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

    private float groundOffset = 1f;

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
            //Gọi RotateObjectServer thay vì RotateObject
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

        if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, pickUpLayers))
        {
            if (hit.transform.TryGetComponent(out GroundWeapon weapon))
            {
                _playerWeapon.InitializeWeapon(weapon.PickUpWeapon());
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit hitObj, raycastDistance, pickupLayer))
        {
            PickupObjectServer(hitObj.transform.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PickupObjectServer(GameObject obj)
    {

        SetObjectInHandObserver(obj, pickupPosition.position, pickupPosition.rotation, gameObject);

        // Store initial properties on server
        originalMaterial = obj.GetComponent<Renderer>().material;
        originalIsTrigger = obj.GetComponent<Collider>() != null ? obj.GetComponent<Collider>().isTrigger : false;

        // Make changes for the visuals on pickup
        SetPickupVisualsObserver(obj);
    }

    [ObserversRpc]
    private void SetPickupVisualsObserver(GameObject obj)
    {
        obj.GetComponent<Renderer>().material = transparentMaterial;

        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = true;

        if (obj.GetComponent<Collider>() != null)
        {
            obj.GetComponent<Collider>().isTrigger = true;
        }
    }

    private void Drop()
    {
        if (!hasObjectInHand)
            return;

        // Cast ray từ tâm màn hình để tìm vị trí thả trên sàn
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(screenCenter);
        Vector3 dropPosition;

        if (Physics.Raycast(ray, out RaycastHit hit, dropDistance, groundLayer))
        {
            dropPosition = hit.point + Vector3.up * groundOffset;
        }
        else
        {
            dropPosition = cameraTransform.position + cameraTransform.forward * dropDistance + Vector3.up * groundOffset;
        }

        // Gọi hàm thả trên server
        DropObjectServer(objInHand, dropPosition, worldObjectHolder);

        // Reset local properties
        objInHand = null;
        hasObjectInHand = false;
    }

    private void RestoreObjectProperties(GameObject obj)
    {
        if (obj == null)
            return;

        // Revert changes made during pickup
        obj.GetComponent<Renderer>().material = originalMaterial;

        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = false;

        if (obj.GetComponent<Collider>() != null)
            obj.GetComponent<Collider>().isTrigger = originalIsTrigger;
    }
    private void RotateObject(float amount)
    {
        if (!hasObjectInHand)
            return;
        rotationAmount += amount;
        RotateObjectServer(objInHand, rotationAmount, currentRotationAxis);
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

    [ObserversRpc]
    void SetObjectInHandObserver(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        if (obj == null) return;

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.parent = player.transform;

        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = true;

        if (obj.GetComponent<Collider>() != null)
            obj.GetComponent<Collider>().isTrigger = true;

        objInHand = obj;
        hasObjectInHand = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void DropObjectServer(GameObject obj, Vector3 dropPosition, Transform worldHolder)
    {
        DropObjectObserver(obj, dropPosition, worldHolder);
    }

    [ObserversRpc]
    void DropObjectObserver(GameObject obj, Vector3 dropPosition, Transform worldHolder)
    {
        if (obj == null) return;

        obj.transform.parent = worldHolder;
        obj.transform.position = dropPosition;

        // Restore object properties on drop
        RestoreObjectProperties(obj);
    }

    private void UpdateObjectPosition()
    {
        if (objInHand != null && Physics.Raycast(objInHand.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            objInHand.transform.position = hit.point + Vector3.up * groundOffset; // Thêm offset để đảm bảo vật phẩm không lún vào mặt đất
        }
    }
}