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
        // Bắt đầu Coroutine để đợi 3 giây trước khi destroy
        StartCoroutine(DieAfterDelay(3f));
    }

    private IEnumerator DieAfterDelay(float delay)
    {
        EnableRagdoll(); // Kích hoạt ragdoll
        yield return new WaitForSeconds(delay);
        ServerManager.Despawn(gameObject);
    }

    private void EnableRagdoll()
    {
        ZombieControler zombieControler = GetComponent<ZombieControler>();
        if (zombieControler != null)
        {
            zombieControler.TriggerRagdoll(Vector3.zero, transform.position);
        }
    }

}
