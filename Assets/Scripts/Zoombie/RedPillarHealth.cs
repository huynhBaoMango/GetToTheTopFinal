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
            StartCoroutine(DestroyPillar());
        }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    private IEnumerator DestroyPillar()
    {
        ServerManager.Despawn(gameObject);
        yield return new WaitForSeconds(2f);
        FindAnyObjectByType<InGameManager>().EndGameTrigger();
    }
}
