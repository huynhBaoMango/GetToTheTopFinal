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
        if (hasObjectInHand)
        {
            Drop();
        }

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
            SetObjectInHand(hitObj.transform.gameObject);
        }
    }

    private void Drop()
    {
        if (!hasObjectInHand)
            return;

        objInHand.transform.parent = null;

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

        DropObjectServerRpc(objInHand, dropPosition, worldObjectHolder);

        RestoreObjectProperties();

        objInHand = null;
        hasObjectInHand = false;
    }

    private void RestoreObjectProperties()
    {
        if (objInHand == null)
            return;

        objInHand.transform.parent = null;

        objInHand.GetComponent<Renderer>().material = originalMaterial;

        if (objInHand.GetComponent<Rigidbody>() != null)
            objInHand.GetComponent<Rigidbody>().isKinematic = false;

        if (objInHand.GetComponent<Collider>() != null)
            objInHand.GetComponent<Collider>().isTrigger = originalIsTrigger;
    }

    private void SetObjectInHand(GameObject obj)
    {
        if (objInHand != null)
        {
            RestoreObjectProperties();
        }

        objInHand = obj;
        hasObjectInHand = true;

        originalMaterial = objInHand.GetComponent<Renderer>().material;
        objInHand.GetComponent<Renderer>().material = transparentMaterial;

        if (objInHand.GetComponent<Rigidbody>() != null)
            objInHand.GetComponent<Rigidbody>().isKinematic = true;

        if (objInHand.GetComponent<Collider>() != null)
        {
            originalIsTrigger = objInHand.GetComponent<Collider>().isTrigger;
            objInHand.GetComponent<Collider>().isTrigger = true;
        }

        objInHand.transform.parent = pickupPosition;
        objInHand.transform.localPosition = Vector3.zero;
        objInHand.transform.localRotation = Quaternion.identity;

        PositionObjectOnGround();
    }

    private void RotateObject(float amount)
    {
        if (!hasObjectInHand)
            return;

        rotationAmount += amount;
        RotateObjectServerRpc(objInHand, rotationAmount, currentRotationAxis);
    }

    private void UpdateObjectPosition()
    {
        if (Physics.Raycast(objInHand.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            objInHand.transform.position = hit.point + Vector3.up * groundOffset;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RotateObjectServerRpc(GameObject obj, float amount, RotationAxis axis)
    {
        RotateObjectClientRpc(obj, amount, axis);
    }

    [ClientRpc]
    private void RotateObjectClientRpc(GameObject obj, float amount, RotationAxis axis)
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
    private void SetObjectInHandServerRpc(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
    {
        SetObjectInHandClientRpc(obj, position, rotation, player);
    }

    [ClientRpc]
    private void SetObjectInHandClientRpc(GameObject obj, Vector3 position, Quaternion rotation, GameObject player)
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
    private void DropObjectServerRpc(GameObject obj, Vector3 dropPosition, Transform worldHolder)
    {
        DropObjectClientRpc(obj, dropPosition, worldHolder);
    }

    [ClientRpc]
    private void DropObjectClientRpc(GameObject obj, Vector3 dropPosition, Transform worldHolder)
    {
        obj.transform.parent = worldHolder;

        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = false;

        if (obj.GetComponent<Collider>() != null)
            obj.GetComponent<Collider>().isTrigger = originalIsTrigger;
    }

    private void PositionObjectOnGround()
    {
        if (Physics.Raycast(objInHand.transform.position, Vector3.down, out RaycastHit hit, groundOffset * 2, groundLayer))
        {
            objInHand.transform.position = hit.point + Vector3.up * groundOffset;
        }
    }
}
