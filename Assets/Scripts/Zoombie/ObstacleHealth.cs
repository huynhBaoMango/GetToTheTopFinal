using UnityEngine;
using FishNet.Object;

public class ObstacleHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 50f; // M�u t?i ?a c?a v?t c?n
    private float currentHealth; // M�u hi?n t?i c?a v?t c?n

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Ph??ng th?c ?? g�y s�t th??ng, c� th? ???c g?i t? m?i ph�a
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    // X? l� s�t th??ng
    private void TakeDamage(float damage)
    {
        if (!IsServer) return; // Ch? m�y ch? m?i ???c th?c hi?n logic n�y

        Debug.Log($"TakeDamage called. Damage: {damage}, Current Health Before: {currentHealth}");
        currentHealth -= damage; // Tr? m�u c?a v?t c?n
        Debug.Log($"Obstacle took damage: {damage}, Current Health After: {currentHealth}");

        if (currentHealth <= 0) // N?u m�u <= 0, ph� h?y v?t c?n
        {
            DestroyObstacle();
        }
    }

    // Ph??ng th?c ph� h?y v?t c?n
    [Server]
    private void DestroyObstacle()
    {
        Debug.Log("Destroying obstacle on server");
        if (IsSpawned) // Ki?m tra xem ??i t??ng ?� ???c spawn hay ch?a
        {
            ServerManager.Despawn(gameObject); // Ph� h?y v?t c?n tr�n t?t c? client
        }
    }
}
