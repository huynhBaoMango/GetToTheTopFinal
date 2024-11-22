using UnityEngine;
using FishNet.Object;

public class GasTank : NetworkBehaviour
{
    [SerializeField] private int explosionDamage = 100;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private GameObject explosionEffect;

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
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        ServerManager.Despawn(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int damage)
    {
        Explode();
    }
}
