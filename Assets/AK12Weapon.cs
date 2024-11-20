using DG.Tweening;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AK12Weapon : APlayerWeapon
{
    float currentDelayBullet = 0;
    public override void AnimateWeapon()
    {
        
        transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y -0.01f, transform.localPosition.z - 0.07f), 0.001f).OnComplete(() =>
        {
            Instantiate(muzzleFlash, muzzleTransform.position, transform.rotation);
            transform.DOLocalMove(new Vector3(transform.localPosition.x, transform.localPosition.y + 0.01f, transform.localPosition.z + 0.07f), 0.1f).SetEase(Ease.OutBack);
        });
    }
    public override void Fire()
    {
        if (IsOwner)
        {
            currentDelayBullet -= Time.deltaTime;
            if (currentDelayBullet <= 0)
            {
                AnimateWeapon();
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Ray ray = Camera.main.ScreenPointToRay(screenCenter);

                if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
                {

                    if (hit.collider.TryGetComponent<ZombieHealth>(out ZombieHealth zombieHealth))
                    {
                        zombieHealth.TakeDamage(damage);
                        SpawnImpactEffect(hit.point, hit.normal, bloodImpactPref);
                    }
                    else
                    {
                        Debug.Log($"Hit: {hit.collider.gameObject.name}");
                        SpawnImpactEffect(hit.point, hit.normal, norImpactPref);
                    }
                    
                }
                currentDelayBullet = delayBulletTime;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnImpactEffect(Vector3 hitPoint, Vector3 hitNormal, GameObject impactEffectPrefab)
    {
        SpawnImpactEffectObserver(hitPoint, hitNormal, impactEffectPrefab);
    }

    [ObserversRpc]
    private void SpawnImpactEffectObserver(Vector3 hitPoint, Vector3 hitNormal, GameObject impactEffectPrefab)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impactEffect = Instantiate(impactEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
            ServerManager.Spawn(impactEffect);
            Destroy(impactEffect, 2f);
        }
    }
}
