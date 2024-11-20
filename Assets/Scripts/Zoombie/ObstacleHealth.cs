using UnityEngine;
using FishNet.Object;

public class ObstacleHealth : NetworkBehaviour
{
    [SerializeField] public int maxHealth = 5;
    private int _currentHealth;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _currentHealth = maxHealth;
    }

    [ServerRpc]
    public void TakeDamageRpc(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            Despawn();
        }

        
        ObserversTakeDamage(_currentHealth);
    }

    [ObserversRpc(ExcludeOwner = true)] 
    private void ObserversTakeDamage(int health)
    {

        _currentHealth = health;



        if (_currentHealth <= 0)
        {



           
        }
    }

    private void Despawn()
    {
        

        NetworkObject.Despawn();
    }
}