using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Connection;
public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthBar;

    [SerializeField]
    private float currentHealth;

    public override void OnStartClient()
    {
        base.OnStartClient();
        healthBar.maxValue = maxHealth;

        if (IsOwner) // Chỉ owner mới gọi RPC để set health ban đầu
        {
            SetHealthServerRpc(maxHealth);
        }

    }



    [ServerRpc]
    private void SetHealthServerRpc(float health)
    {
        currentHealth = health;
        UpdateHealthClientRpc(base.Owner, currentHealth);

    }

    [TargetRpc]
    private void UpdateHealthClientRpc(NetworkConnection conn, float health)
    {


        currentHealth = health;
        UpdateHealthUI();
    }





    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthClientRpc(base.Owner, currentHealth);


        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
        }
    }

    private void UpdateHealthUI()
    {
        healthBar.value = currentHealth;
    }
}