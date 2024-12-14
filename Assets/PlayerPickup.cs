using FishNet.Object;
using System.Globalization;
using Unity.VisualScripting;
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

        cameraTransform = Camera.main.transform;
        cam = Camera.main;
    }

    void Update()
    {
        if (IsOwner)
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
                if (!IsOwner) return;
                currentRotationAxis = (currentRotationAxis == RotationAxis.Y) ? RotationAxis.X : RotationAxis.Y;
            }

            if (hasObjectInHand)
            {
                UpdateObjectPosition();
            }
        }

        
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickUp()
    {
        PickUpOb();
    }

    [ObserversRpc]
    void PickUpOb()
    {
        if (hasObjectInHand)
        {
            Drop();
        }


        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        if (Physics.Raycast(ray, out RaycastHit hitObj, raycastDistance, pickupLayer))
        {
            SetObjectInHand(hitObj.transform.gameObject);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void Drop()
    {
        DropOb();
    }

    [ObserversRpc]
    void DropOb()
    {
        if (!hasObjectInHand)
            return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        RestoreObjectProperties(objInHand);

        
    }

    [ServerRpc(RequireOwnership = false)]
    void RestoreObjectProperties(GameObject objInHand)
    {
        RestoreObjectPropertiesOb(objInHand);
    }

    [ObserversRpc]
    private void RestoreObjectPropertiesOb(GameObject objInHand)
    {
        if (objInHand == null)
            return;

        if(objInHand.TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material = originalMaterial; 
        }

        if (objInHand.TryGetComponent<Rigidbody>(out Rigidbody rg))
            rg.isKinematic = false;

        if (objInHand.TryGetComponent<Collider>(out Collider cl))
        {
            cl.isTrigger = false;
        }

        objInHand = null;
        hasObjectInHand = false;
    }

    private void SetObjectInHand(GameObject obj)
    {
        objInHand = obj;
        hasObjectInHand = true;
        SetObjectProperties(objInHand);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetObjectProperties(GameObject objInHand)
    {
        SetPropertiesOb(objInHand);
    }

    [ObserversRpc]
    void SetPropertiesOb(GameObject objInHand)
    {
        originalMaterial = objInHand.GetComponent<Renderer>().material;
        objInHand.GetComponent<Renderer>().material = transparentMaterial;

        if (objInHand.TryGetComponent<Rigidbody>(out Rigidbody rg))
            rg.isKinematic = true;

        if (objInHand.TryGetComponent<Collider>(out Collider cl))
        {
            cl.isTrigger = true;
        }

        objInHand.transform.localPosition = Vector3.zero;
        objInHand.transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void RotateObject(float amount)
    {
        if (!hasObjectInHand)
            return;

        rotationAmount += amount;
        RotateObjectServer(objInHand, rotationAmount, currentRotationAxis);
    }

    [ServerRpc(RequireOwnership =false)]
    private void UpdateObjectPosition()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 6f, groundLayer))
        {
            UpdatePosObj(objInHand, hit.point);
        }
    }

    void UpdatePosObj(GameObject objInHand, Vector3 hit)
    {
        objInHand.transform.position = hit + new Vector3(0, objInHand.GetComponent<Collider>().bounds.size.y, 0);
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
}
