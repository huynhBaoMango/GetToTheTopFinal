using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Connection;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject bloodSplatterUI; // Thêm biến cho UI Image của hiệu ứng máu bắn tung tóe

    private float currentHealth;

    public override void OnStartClient()
    {
        base.OnStartClient();
        healthBar.maxValue = maxHealth;

        if (IsOwner) // Chỉ owner mới gọi RPC để set health ban đầu
        {
            SetHealthServerRpc(maxHealth);
        }

        if (bloodSplatterUI != null)
        {
            bloodSplatterUI.SetActive(false); // Đảm bảo hiệu ứng máu ban đầu bị ẩn
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
        UpdateHealthObserversRpc(currentHealth);
    }

    [ObserversRpc]
    private void UpdateHealthObserversRpc(float health)
    {
        currentHealth = health;
        UpdateHealthUI();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Gọi hiệu ứng máu bắn tung tóe
        SpawnBloodSplatterObserversRpc();

        UpdateHealthObserversRpc(currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
            FindObjectOfType<InGameManager>().EndGameTrigger();
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
        UpdateHealthObserversRpc(currentHealth);

        Debug.Log($"Player healed by {healAmount}. Current health: {currentHealth}");
    }

    private void UpdateHealthUI()
    {
        healthBar.value = currentHealth;
    }

    [ObserversRpc]
    private void SpawnBloodSplatterObserversRpc()
    {
        if (bloodSplatterUI != null)
        {
            bloodSplatterUI.SetActive(true); // Hiển thị UI Image của hiệu ứng máu
            StartCoroutine(HideBloodSplatter()); // Ẩn hiệu ứng sau một khoảng thời gian
        }
    }

    private IEnumerator HideBloodSplatter()
    {
        yield return new WaitForSeconds(0.5f); // Thời gian hiển thị hiệu ứng máu (ví dụ: 0.5 giây)
        if (bloodSplatterUI != null)
        {
            bloodSplatterUI.SetActive(false); // Ẩn UI Image của hiệu ứng máu
        }
    }
}
