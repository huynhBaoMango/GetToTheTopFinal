using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AK12Weapon : APlayerWeapon
{

    public override void AnimateWeapon()
    {

    }

    public override void Fire()
    {
        AnimateWeapon();
        Vector3 startPosition = muzzleTransform.position;
        Vector3 direction = muzzleTransform.forward;
        SpawnBullet(startPosition, direction, TimeManager.Tick, damage, bulletPrefab);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBullet(Vector3 startPosition, Vector3 direction, uint startTick, int damage, GameObject bulletPrefab)
    {
        float timeDifference = (float)(TimeManager.Tick - startTick) / TimeManager.TickRate;
        //Vector3 spawnPosition = startPosition + direction * 10 * timeDifference;
        GameObject bullet = Instantiate(bulletPrefab, startPosition, Quaternion.identity);
        ServerManager.Spawn(bullet);
    }
}
