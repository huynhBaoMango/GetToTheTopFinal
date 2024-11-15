﻿using FishNet.Object;
using UnityEngine;

public abstract class APlayerWeapon : NetworkBehaviour
{
    public int damage;
    public Transform muzzleTransform;
    private Transform _cameraTransform;
    public float maxRange = 20f;
    public LayerMask weaponHitLayers;
    public GameObject bulletPrefab;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    public abstract void Fire();

    public abstract void AnimateWeapon();


}