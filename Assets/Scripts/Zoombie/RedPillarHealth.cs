using System.Collections;
using UnityEngine;
using FishNet.Object;

public class RedPillarHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private GameObject Fx;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    [ObserversRpc]
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        FindAnyObjectByType<InGameManager>().ChangeHeartHealthValue(currentHealth);
        if (currentHealth <= 0f)
        {
            
            StartCoroutine(DestroyPillar());
            FindAnyObjectByType<InGameManager>().EndGameTrigger();
        }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    private IEnumerator DestroyPillar()
    {
        ServerManager.Spawn(Instantiate(Fx, transform.position, Quaternion.identity));

        yield return new WaitForSeconds(3f);

        
    }
}
