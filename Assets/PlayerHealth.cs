using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int _currnetHealt;

    private void wake()
    {
        _currnetHealt = maxHealth;
    }
    public void OnStartClient()
    {
        base.OnStartClient();
        if(!IsOwner)
        {
            enabled = false;
            return;
        }    
    }
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int damage)
    {
        _currnetHealt -= damage;
        Debug.Log($"New player health: {_currnetHealt}");
        if (_currnetHealt <= 0)
            Die();
     }
    private void Die()
    {
        Debug.Log("player is dead");
    }    
}
