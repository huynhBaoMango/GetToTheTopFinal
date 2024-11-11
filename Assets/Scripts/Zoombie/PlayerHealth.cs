using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SyncVar(OnChange = nameof(OnHealthChange))] private float currentHealth;

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentHealth = maxHealth;
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0f)
        {
            Despawn();
        }
    }

    [ServerRpc]
    private void Despawn()
    {
        if (IsServer) ServerManager.Despawn(gameObject);
    }

    public void TakeDamage(float damage)
    {
        TakeDamageServerRpc(damage);
    }
    private void OnHealthChange(float oldValue, float newValue, bool asServer)
    {
        if (newValue <= 0f)
        {

            Debug.Log($"Player died with new health: {newValue} / old value: {oldValue} ");

            if (IsServer)
            {

                Despawn();

            }




        }
    }





}