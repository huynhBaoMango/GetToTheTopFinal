using FishNet.Object;
using UnityEngine;

public class GasTank : NetworkBehaviour
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 100f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private int explosionDamage = 50;


    [ServerRpc]
    public void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit.CompareTag("Zombie"))
            {
                ZombieHealth health = hit.GetComponent<ZombieHealth>();
                if (health != null)
                {
                    //health.TakeDamage(explosionDamage, true);
                }


                Rigidbody rb = hit.GetComponent<Rigidbody>();


                if (rb != null)


                {


                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1, ForceMode.Impulse);



                }

            }
        }




        SpawnExplosionEffectRpc(transform.position);
        Despawn(gameObject);



    }

    [ObserversRpc(BufferLast = true)]
    private void SpawnExplosionEffectRpc(Vector3 position)
    {


        Instantiate(explosionEffect, position, Quaternion.identity);



    }



}