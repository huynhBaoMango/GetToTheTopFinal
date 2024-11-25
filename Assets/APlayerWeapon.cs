using FishNet.Object;
using TMPro;
using UnityEngine;

public abstract class APlayerWeapon : NetworkBehaviour
{
    public int damage;
    public float delayBulletTime;
    public int maxAmmo;
    public int initialMaxAmmo = 90;
    public Transform muzzleTransform;
    private Transform _cameraTransform;
    public float maxRange = 20f;
    public LayerMask weaponHitLayers;

    public GameObject muzzleFlash;
    public GameObject bloodImpactPref, norImpactPref;
    public Transform magHoldPos, reloadPos;
    public Transform tempLeftHandIK;

    public Transform MagPos;
    public GameObject MagPref;
    public TextMeshProUGUI ammoText;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        maxAmmo = initialMaxAmmo;
    }

    public Transform RightHandIKTarget, LeftHandIKTarget, rightHintIK, leftHintIK;

    public abstract void Fire();

    public abstract void Reload();

    public abstract void AnimateWeapon();
    
}
