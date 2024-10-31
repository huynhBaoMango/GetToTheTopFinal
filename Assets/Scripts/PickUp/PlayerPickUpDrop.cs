using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickUpDrop : MonoBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private LayerMask pickUpLayerMask;
    [SerializeField] private GameObject defaultCrosshair; 
    [SerializeField] private GameObject interactionCrosshair; 
    private ObjectGrabbable objectGrabbable;

    private void Start()
    {
        interactionCrosshair.SetActive(false); 
    }

    private void Update()
    {
        float pickUpDistance = 4f;
        Ray ray = new Ray(playerCameraTransform.position, playerCameraTransform.forward);
        RaycastHit raycastHit;

        if (objectGrabbable == null) 
        {
            if (Physics.Raycast(ray, out raycastHit, pickUpDistance, pickUpLayerMask))
            {
                if (raycastHit.transform.TryGetComponent(out ObjectGrabbable objectGrabbableTemp))
                {
                    interactionCrosshair.SetActive(true);
                    defaultCrosshair.SetActive(false); 

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        objectGrabbableTemp.Grab(objectGrabPointTransform);
                        objectGrabbable = objectGrabbableTemp;
                        defaultCrosshair.SetActive(true); 
                        interactionCrosshair.SetActive(false); 
                    }
                    return; 
                }
            }
            interactionCrosshair.SetActive(false); 
            defaultCrosshair.SetActive(true); 
        }
        else 
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                objectGrabbable.Drop();
                objectGrabbable = null;
                defaultCrosshair.SetActive(true); 
                interactionCrosshair.SetActive(false); 
            }
        }
    }
}
