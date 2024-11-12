using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : NetworkBehaviour
{
    [SerializeField] private float pickUpRange = 4f;
    [SerializeField] private KeyCode pickUpKey = KeyCode.F;

    [SerializeField] private LayerMask pickUpLayers;
    private Transform cameraTransform;
    private PlayerWeapon _playerWeapon;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(!IsOwner)
        {
            enabled = false;
            return;
        }
        cameraTransform = Camera.main.transform;
        if(TryGetComponent(out PlayerWeapon plWeapon))
        {
            _playerWeapon = plWeapon;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(pickUpKey))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        if(!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, pickUpRange, pickUpLayers))
        {
            return;
        }

        if(hit.transform.TryGetComponent(out GroundWeapon weapon))
        {
            _playerWeapon.InitializeWeapon(weapon.PickUpWeapon());
        }
    }
}
