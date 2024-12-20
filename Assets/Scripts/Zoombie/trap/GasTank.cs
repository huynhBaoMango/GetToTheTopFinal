using UnityEngine;
using FishNet.Object;

public class GasTank : NetworkBehaviour
{
    [SerializeField] private int explosionDamage = 100;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;


    public void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.TryGetComponent<ZombieHealth>(out ZombieHealth zombieHealth))
            {
                zombieHealth.TakeDamage(explosionDamage);
            }
        }

        if (explosionEffect != null)
        {
            // G?i ObserversRpc ?? t?o hi?u ?ng n? tr�n t?t c? c�c client
            TriggerExplosionEffect();
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        ServerManager.Despawn(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int damage)
    {
        Explode();
    }

    [ObserversRpc]
    private void TriggerExplosionEffect()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }
    }
}
