using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using FishNet.Object.Synchronizing;
using System;

public class PlayerHealth : NetworkBehaviour
{
    private float maxHealth = 100;
    private readonly SyncVar<float> currentHealth = new(100);
    [SerializeField] private GameObject bloodSplatterUI; // Thêm biến cho UI Image của hiệu ứng máu bắn tung tóe
    private Slider healthBar;
    

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            healthBar = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
            healthBar.value = currentHealth.Value;
            if (bloodSplatterUI != null)
            {
                bloodSplatterUI.SetActive(false); // Đảm bảo hiệu ứng máu ban đầu bị ẩn
            }
            currentHealth.OnChange += currentHealthOnChange;
        }

        
    }
    
    public void ChangeHealthPlayer(float value)
    {
        currentHealth.Value += value;
    }

    private void currentHealthOnChange(float prev, float next, bool asServer)
    {
        healthBar.value = next;
        if (next <= 0)
        {
            //trigger death
            Debug.Log("Chet");

        }

        if (next > prev)
        {
            //hiệu ứng heal
            Debug.Log("Heal " + next);

        }

        if(next < prev)
        {
            //nhận damage
            Debug.Log("Nhan damage " + next);

        }
    }

    private void Update()
    {
        if (!IsOwner) return; // Chỉ kiểm tra phím bấm với chủ sở hữu

        // Nhấn phím O để trừ 10 máu
        if (Input.GetKeyDown(KeyCode.O))
        {
            currentHealth.Value -= 10;
            Debug.Log($"Player took 20 damage. Current health: {currentHealth}");
        }

        // Nhấn phím P để test hồi máu 10
        if (Input.GetKeyDown(KeyCode.P))
        {
            currentHealth.Value += 10;
            Debug.Log($"Player healed by 10. Current health: {currentHealth}");
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
