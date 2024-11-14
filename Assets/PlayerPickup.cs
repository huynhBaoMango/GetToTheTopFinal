using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : NetworkBehaviour
{
    [Header("Cài ??t Nh?t ?? Chung")]
    [SerializeField] private float pickUpRange = 4f;
    [SerializeField] private KeyCode pickUpKey = KeyCode.F;
    [SerializeField] private KeyCode dropButton = KeyCode.Q;
    [SerializeField] private LayerMask pickUpLayers;

    [Header("Cài ??t Nh?t V?t Ph?m")]
    [SerializeField] private float raycastDistance = 4f;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private Transform pickupPosition;

    private Transform cameraTransform;
    private PlayerWeapon _playerWeapon;
    private Camera cam;
    private bool hasObjectInHand;
    private GameObject objInHand;
    private Transform worldObjectHolder;

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
        worldObjectHolder = GameObject.FindGameObjectWithTag("WorldObjects").transform;

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
    }

    private void PickUp()
    {
        // Ki?m tra n?u có th? nh?t v? khí
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, pickUpRange, pickUpLayers))
        {
            if (hit.transform.TryGetComponent(out GroundWeapon weapon))
            {
                _playerWeapon.InitializeWeapon(weapon.PickUpWeapon());
                return;
            }
        }

        // Ki?m tra n?u có th? nh?t các v?t ph?m khác
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hitObj, raycastDistance, pickupLayer))
        {
            if (!hasObjectInHand)
            {
                SetObjectInHandServer(hitObj.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hitObj.transform.gameObject;
                hasObjectInHand = true;
            }
            else if (hasObjectInHand)
            {
                Drop();
                SetObjectInHandServer(hitObj.transform.gameObject, pickupPosition.position, pickupPosition.rotation, gameObject);
                objInHand = hitObj.transform.gameObject;
                hasObjectInHand = true;
            }
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
