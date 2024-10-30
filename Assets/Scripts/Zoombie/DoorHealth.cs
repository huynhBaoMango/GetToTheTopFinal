using UnityEngine;

public class DoorHealth : MonoBehaviour
{
    [SerializeField] private int _health = 10;
    public int CurrentHealth => _health; // Thu?c tính ??c s?c kh?e hi?n t?i

    public void TakeDamage()
    {
        _health--;
        Debug.Log("Obstacle Health: " + _health); // Thêm debug ?? ki?m tra
        if (_health <= 0)
        {
            Destroy(gameObject);
        }
    }
}

