using FishNet.Object;
using UnityEngine;

public abstract class APlayerWeapon : NetworkBehaviour
{
    public int damage;
    public Transform muzzleTransform;
    private Transform _cameraTransform;
    public float maxRange = 20f;
    public LayerMask weaponHitLayers;
    private Bullet bullet;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        bullet = GetComponent<Bullet>();
    }

    public void Fire()
    {
        AnimateWeapon();
        Vector3 startPosition = muzzleTransform.position; 
        Vector3 direction = muzzleTransform.forward; 
        bullet.Shoot(startPosition, direction, damage);
    }

    public abstract void AnimateWeapon();
}
