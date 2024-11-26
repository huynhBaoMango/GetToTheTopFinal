using UnityEngine;
using FishNet.Object;

public class ObstacleHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 500f; 
    [SerializeField] private GameObject destructionEffectPrefab;

    private float currentHealth; 

    private void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"Obstacle initialized with Health: {currentHealth}");
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        Debug.Log($"TakeDamageServerRpc called. Damage: {damage}, Current Health Before: {currentHealth}");
        TakeDamage(damage);
        Debug.Log($"TakeDamageServerRpc finished. Current Health After: {currentHealth}");
    }

   
    private void TakeDamage(float damage)
    {
        if (!IsServer) return; 

        Debug.Log($"TakeDamage called. Damage: {damage}, Current Health Before: {currentHealth}");
        currentHealth -= damage; // Trừ máu của vật cản
        Debug.Log($"Obstacle took damage: {damage}, Current Health After: {currentHealth}");

        if (currentHealth <= 0) 
        {
            DestroyObstacle();
        }
    }

   
    [Server]
    private void DestroyObstacle()
    {
        Debug.Log("Destroying obstacle on server");

        SpawnDestructionEffect();

        if (IsSpawned) 
        {
            ServerManager.Despawn(gameObject); 
        }
    }

    
    [ObserversRpc]
    private void SpawnDestructionEffect()
    {
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
