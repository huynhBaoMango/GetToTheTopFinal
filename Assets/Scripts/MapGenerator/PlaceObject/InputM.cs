using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputM : MonoBehaviour
{
    [SerializeField] Camera sceneCamera;
    private Vector3 lastPosition;

    [SerializeField] LayerMask PlacementlayerMask;

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100, PlacementlayerMask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }
}
