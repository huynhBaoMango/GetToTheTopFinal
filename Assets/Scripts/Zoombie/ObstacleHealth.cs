using UnityEngine;
using FishNet.Object;

public class ObstacleHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 50f; // Máu t?i ?a c?a v?t c?n
    private float currentHealth; // Máu hi?n t?i c?a v?t c?n

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Ph??ng th?c ?? gây sát th??ng, có th? ???c g?i t? m?i phía
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    // X? lý sát th??ng
    private void TakeDamage(float damage)
    {
        if (!IsServer) return; // Ch? máy ch? m?i ???c th?c hi?n logic này

        Debug.Log($"TakeDamage called. Damage: {damage}, Current Health Before: {currentHealth}");
        currentHealth -= damage; // Tr? máu c?a v?t c?n
        Debug.Log($"Obstacle took damage: {damage}, Current Health After: {currentHealth}");

        if (currentHealth <= 0) // N?u máu <= 0, phá h?y v?t c?n
        {
            DestroyObstacle();
        }
    }

    // Ph??ng th?c phá h?y v?t c?n
    [Server]
    private void DestroyObstacle()
    {
        Debug.Log("Destroying obstacle on server");
        if (IsSpawned) // Ki?m tra xem ??i t??ng ?ã ???c spawn hay ch?a
        {
            ServerManager.Despawn(gameObject); // Phá h?y v?t c?n trên t?t c? client
        }
    }
}
