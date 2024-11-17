using FishNet.Object;
using UnityEngine;

public class PlayerPickup : NetworkBehaviour
{
    [Header("Cài Đặt Nhặt Đồ Chung")]
    [SerializeField] private float pickUpRange = 4f;
    [SerializeField] private KeyCode pickUpKey = KeyCode.F;
    [SerializeField] private KeyCode dropButton = KeyCode.Q;
    [SerializeField] private KeyCode rotateButton = KeyCode.Z; // Key to switch rotation axis
    [SerializeField] private LayerMask pickUpLayers;

    [Header("Cài Đặt Nhặt Vật Phẩm")]
    [SerializeField] private float raycastDistance = 4f;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private Transform pickupPosition;

    private Transform cameraTransform;
    private PlayerWeapon _playerWeapon;
    private Camera cam;
    private bool hasObjectInHand;
    private GameObject objInHand;
    private Transform worldObjectHolder;
    private float rotationAmount = 0f;
    private enum RotationAxis { X, Y }
    private RotationAxis currentRotationAxis = RotationAxis.Y;

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
            // Toggle rotation axis
            currentRotationAxis = (currentRotationAxis == RotationAxis.Y) ? RotationAxis.X : RotationAxis.Y;
            rotationAmount = 0f; // Reset rotation amount when switching axes
            RotateObject(0f); // Apply the rotation change immediately
        }

        if (hasObjectInHand && Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            RotateObject(Input.GetAxis("Mouse ScrollWheel") * 90f);
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
                rotationAmount = 0; // Reset rotation amount here
            }
            else if (hasObjectInHand)
            {
                Drop();
                SetObjectInHandServer(hitObj.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hitObj.transform.gameObject;
                hasObjectInHand = true;
                rotationAmount = 0; // Reset rotation amount here as well
            }
        }
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
        else // RotationAxis.Y
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
    }

    void Drop()
    {
        if (!hasObjectInHand)
            return;

        DropObjectServer(objInHand, worldObjectHolder);
        hasObjectInHand = false;
        objInHand = null;
    }

    [ServerRpc(RequireOwnership = false)]
    void DropObjectServer(GameObject obj, Transform worldHolder)
    {
        DropObjectObserver(obj, worldHolder);
    }

    [ObserversRpc]
    void DropObjectObserver(GameObject obj, Transform worldHolder)
    {
        obj.transform.parent = worldHolder;

        if (obj.GetComponent<Rigidbody>() != null)
            obj.GetComponent<Rigidbody>().isKinematic = false;
    }
}