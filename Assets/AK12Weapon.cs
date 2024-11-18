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

        if (IsOwner)
        {
            
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
            {
               
                if (hit.collider.TryGetComponent<ZombieHealth>(out ZombieHealth zombieHealth))
                {
                    zombieHealth.TakeDamage(damage);
                }
                else
                {
                  
                    Debug.Log($"Hit: {hit.collider.gameObject.name}");
                }

               
                SpawnImpactEffect(hit.point, hit.normal);
            }
        }
    }
    [ObserversRpc]
    private void SpawnImpactEffect(Vector3 hitPoint, Vector3 hitNormal)
    {
        
    }
}
