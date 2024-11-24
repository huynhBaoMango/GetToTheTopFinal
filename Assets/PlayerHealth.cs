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

    private void Update()
    {
        if (!IsOwner) return; // Chỉ kiểm tra phím bấm với chủ sở hữu

        // Nhấn phím O để trừ 10 máu
        if (Input.GetKeyDown(KeyCode.O))
        {
            TakeDamage(20f);
            Debug.Log($"Player took 20 damage. Current health: {currentHealth}");
        }

        // Nhấn phím P để test hồi máu 10
        if (Input.GetKeyDown(KeyCode.P))
        {
            Heal(10f);
            Debug.Log($"Player healed by 10. Current health: {currentHealth}");
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

    [ServerRpc(RequireOwnership = false)]
    public void Heal(float healAmount)
    {
        if (!IsServerInitialized)
        {
            Debug.LogError("HealServerRpc can only be called on the server.");
            return;
        }
        // Tăng máu
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Cập nhật máu lên client
        UpdateHealthClientRpc(base.Owner, currentHealth);

        Debug.Log($"Player healed by {healAmount}. Current health: {currentHealth}");
    }


    private void UpdateHealthUI()
    {
        healthBar.value = currentHealth;
    }
}