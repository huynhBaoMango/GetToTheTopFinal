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
    [ServerRpc]
    public void Takedamage(int damege)
    { 
       _currentHealth -= damege;
        if (_currentHealth <= 0)
            Die();

    }
    [Server]  
    
    private void Die()
    {
        ServerManager.Despawn(gameObject);
    }    
}
