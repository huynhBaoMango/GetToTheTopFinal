using UnityEngine;
using FishNet.Object;

public class ObstacleHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 50f; // Máu tối đa của vật cản
    [SerializeField] private GameObject destructionEffectPrefab; // Prefab của hiệu ứng phá hủy

    private float currentHealth; // Máu hiện tại của vật cản

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // Phương thức để gây sát thương, có thể được gọi từ mọi phía
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }

    // Xử lý sát thương
    private void TakeDamage(float damage)
    {
        if (!IsServer) return; // Chỉ máy chủ mới được thực hiện logic này

        Debug.Log($"TakeDamage called. Damage: {damage}, Current Health Before: {currentHealth}");
        currentHealth -= damage; // Trừ máu của vật cản
        Debug.Log($"Obstacle took damage: {damage}, Current Health After: {currentHealth}");

        if (currentHealth <= 0) // Nếu máu <= 0, phá hủy vật cản
        {
            DestroyObstacle();
        }
    }

    // Phương thức phá hủy vật cản
    [Server]
    private void DestroyObstacle()
    {
        Debug.Log("Destroying obstacle on server");

        // Tạo hiệu ứng phá hủy
        SpawnDestructionEffect();

        if (IsSpawned) // Kiểm tra xem đối tượng đã được spawn hay chưa
        {
            ServerManager.Despawn(gameObject); // Phá hủy vật cản trên tất cả client
        }
    }

    // Phương thức tạo hiệu ứng phá hủy
    [ObserversRpc]
    private void SpawnDestructionEffect()
    {
        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
