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
            rotationAmount = 0f;
            RotateObject(0f);
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
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, pickUpRange, pickUpLayers))
        {
            if (hit.transform.TryGetComponent(out GroundWeapon weapon))
            {
                _playerWeapon.InitializeWeapon(weapon.PickUpWeapon());
                return;
            }
        }

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hitObj, raycastDistance, pickupLayer))
        {
            if (!hasObjectInHand)
            {
                SetObjectInHandServer(hitObj.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hitObj.transform.gameObject;
                hasObjectInHand = true;
                rotationAmount = 0;

                originalMaterial = objInHand.GetComponent<Renderer>().material;
                objInHand.GetComponent<Renderer>().material = transparentMaterial;

                // Bật isKinematic và isTrigger khi nhặt vật phẩm
                if (objInHand.GetComponent<Rigidbody>() != null)
                    objInHand.GetComponent<Rigidbody>().isKinematic = true;

                //if (objInHand.GetComponent<Collider>() != null)
                //    objInHand.GetComponent<Collider>().isTrigger = true;

                
                PositionObjectOnGround();
            }
            else
            {
                Drop();
                SetObjectInHandServer(hitObj.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hitObj.transform.gameObject;
                hasObjectInHand = true;
                rotationAmount = 0;

                originalMaterial = objInHand.GetComponent<Renderer>().material;
                objInHand.GetComponent<Renderer>().material = transparentMaterial;

                // Bật isKinematic và isTrigger khi nhặt vật phẩm
                if (objInHand.GetComponent<Rigidbody>() != null)
                    objInHand.GetComponent<Rigidbody>().isKinematic = true;

                if (objInHand.GetComponent<Collider>() != null)
                    objInHand.GetComponent<Collider>().isTrigger = true;

                // Đảm bảo vật phẩm nằm sát mặt đất khi nhặt
                PositionObjectOnGround();
            }
        }
    }

    private void UpdateObjectPosition()
    {
        if (Physics.Raycast(objInHand.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            objInHand.transform.position = hit.point + Vector3.up * groundOffset; // Thêm offset để đảm bảo vật phẩm không lún vào mặt đất
        }
    }

    private void RotateObject(float amount)
    {
        if (!hasObjectInHand)
            return;

        rotationAmount += amount;
        RotateObjectServer(objInHand, rotationAmount, currentRotationAxis);
    }

    void Drop()
    {
        if (!hasObjectInHand)
            return;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, dropDistance, groundLayer))
        {
            DropObjectServer(objInHand, hit.point + Vector3.up * groundOffset, worldObjectHolder); // Thêm offset khi thả vật phẩm
        }
        else
        {
            Vector3 dropPosition = cameraTransform.position + cameraTransform.forward * dropDistance;
            DropObjectServer(objInHand, dropPosition + Vector3.up * groundOffset, worldObjectHolder); // Thêm offset khi thả vật phẩm
        }

        objInHand.GetComponent<Renderer>().material = originalMaterial;

        // Tắt isKinematic và isTrigger khi thả vật phẩm
        if (objInHand.GetComponent<Rigidbody>() != null)
            objInHand.GetComponent<Rigidbody>().isKinematic = false;

        if (objInHand.GetComponent<Collider>() != null)
            objInHand.GetComponent<Collider>().isTrigger = false;

        hasObjectInHand = false;
        objInHand = null;
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
        obj.transform.parent = player.transform;

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
        obj.transform.parent = worldHolder;
        obj.transform.position = dropPosition;

        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = false;

        if (obj.GetComponent<Collider>() != null)
            obj.GetComponent<Collider>().isTrigger = false;
    }

    private void PositionObjectOnGround()
    {
        if (Physics.Raycast(objInHand.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            objInHand.transform.position = hit.point + Vector3.up * groundOffset; // Thêm offset để đảm bảo vật phẩm không lún vào mặt đất
        }
    }
}
