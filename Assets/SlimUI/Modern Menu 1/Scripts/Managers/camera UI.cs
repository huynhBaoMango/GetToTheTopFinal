using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
public class cameraUI : MonoBehaviour
{
    [SerializeField]
    private float duration;
    public void LookAt(Transform taget)
    {
        transform.DOLookAt(taget.position, duration);
    }
}
