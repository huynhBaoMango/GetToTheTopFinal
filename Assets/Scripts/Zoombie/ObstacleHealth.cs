using UnityEngine;
using FishNet.Object;

public class ObstacleHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 50;
    private int _currentHealth;
    private bool _isDead = false;

    public int CurrentHealth => _currentHealth; // Thu?c t�nh ?? l?y m�u hi?n t?i

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"Obstacle took {damage} damage. Current health: {_currentHealth}");

        if (_currentHealth <= 0 && !_isDead)
        {
            _isDead = true;
            Die();
        }
    }

    public bool IsDead()
    {
        return _isDead;
    }

    [Server]
    private void Die()
    {
        Debug.Log("Obstacle died and will be despawned.");
        // Hi?u ?ng ph� h?y c� th? th�m v�o ?�y
        ServerManager.Despawn(gameObject);
    }
}
