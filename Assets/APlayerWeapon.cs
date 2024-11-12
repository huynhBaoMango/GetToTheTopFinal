using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class APlayerWeapon : NetworkBehaviour
{
    public int damage;
    private LayerMask enemyLayer;
    private Transform _cameraTransform;
    public float maxRange = 20f;
    public LayerMask weaponHitLayers;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    public void Fire()
    {
        AnimateWeapon();
        if (!Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, maxRange, weaponHitLayers))
            return;

        if (hit.transform.TryGetComponent(out ZombieHealth zombieHealth))
        {
            Debug.Log($"Hit zombie at position: {hit.point}");
            zombieHealth.TakeDamage(damage);
        }
    }

    public abstract void AnimateWeapon();
}
