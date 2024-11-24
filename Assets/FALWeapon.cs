using FishNet.Object;
using TMPro;
using UnityEngine;

public class FALWeapon : APlayerWeapon
{
    public int currentAmmo = 30;
    public int magazineAmmo = 30;
    public TextMeshProUGUI ammoText;

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

                // Hi?u ?ng b?n trúng (ví d?: sparks) t?i v? trí va ch?m
                SpawnImpactEffect(hit.point, hit.normal);
            }
            Debug.Log("Current Damager: " + damage);
            Debug.Log("Current Ammo: " + currentAmmo);
        }
    }

    public override void Reload()
    {
        //throw new System.NotImplementedException();
        if (currentAmmo < magazineAmmo)
        {
            currentAmmo = magazineAmmo; // Refill current ammo
            maxAmmo -= magazineAmmo; // Reduce max ammo by clip size
        }
        Debug.Log("IsReloading");
        //UpdateAmmoDisplay();
    }

    [ObserversRpc]
    private void SpawnImpactEffect(Vector3 hitPoint, Vector3 hitNormal)
    {
        
    }

    
}