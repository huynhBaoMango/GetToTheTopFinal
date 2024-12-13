using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        Debug.Log($"Zombie current health: {_currentHealth}");
        if (_currentHealth <= 0)
        {
            Debug.Log("Zombie is dead");
            Die();
        }
    }

    [Server]
    private void Die()
    {
        Debug.Log("Zombie is dead");
        // Gọi Rpc để kích hoạt ragdoll trên client
        RpcEnableRagdoll();
        // Bắt đầu Coroutine để đợi 3 giây trước khi destroy
        StartCoroutine(DieAfterDelay(3f));
    }

    private IEnumerator DieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ServerManager.Despawn(gameObject);
    }

    [ObserversRpc]
    private void RpcEnableRagdoll()
    {
        EnableRagdoll();
    }

    private void EnableRagdoll()
    {
        if (TryGetComponent<FastZombieController>(out var fastZombie))
        {
            fastZombie.TriggerRagdoll(Vector3.zero, transform.position);
        }
        else if (TryGetComponent<ZombieControler>(out var zombieController))
        {
            zombieController.TriggerRagdoll(Vector3.zero, transform.position);
        }
        else if(TryGetComponent<ZombieTank>(out var ZombieTank))
        {
            ZombieTank.TriggerRagdoll(Vector3.zero, transform.position);
        }    
    }
}
