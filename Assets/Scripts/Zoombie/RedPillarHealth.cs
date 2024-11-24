using System.Collections;
using UnityEngine;
using FishNet.Object;

public class RedPillarHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 500f;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            DestroyPillar();
        }
    }

    [Server]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    private void DestroyPillar()
    {
        ServerManager.Despawn(gameObject);
    }
}
