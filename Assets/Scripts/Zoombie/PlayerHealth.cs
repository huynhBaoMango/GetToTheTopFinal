using FishNet.Object;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            currentHealth = maxHealth;
        }
    }

    [ServerRpc]
    public void TakeDamageServerRpc(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        TakeDamageObserverRpc(damage); // Đồng bộ hóa cho các client khác

        if (currentHealth <= 0f)
        {
            Despawn(); // Hủy object trên server
        }
    }


    [ObserversRpc]
    private void TakeDamageObserverRpc(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);


    }

    public void TakeDamage(float damage)
    {


        TakeDamageServerRpc(damage);




    }



    // Hàm Despawn để hủy object
    [ServerRpc]
    private void Despawn()
    {

        if (IsServer) ServerManager.Despawn(gameObject);



    }
}