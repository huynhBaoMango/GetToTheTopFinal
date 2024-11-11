using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Xử lý khi người chơi chết
        Debug.Log("Player died!");
        // Có thể thêm hiệu ứng chết, respawn, hoặc kết thúc trò chơi tại đây.
    }
}
