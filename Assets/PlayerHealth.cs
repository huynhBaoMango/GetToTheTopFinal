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
    public float currentHealth;
    [SerializeField] private GameObject bloodSplatterUI; // Thêm biến cho UI Image của hiệu ứng máu bắn tung tóe
    [SerializeField] private Slider healthBar;

    private void Awake()
    {
        
        bloodSplatterUI.SetActive(false);
        currentHealth = 100;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        healthBar = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeCurrentHealth(float value)
    {
        ChangeCurrentHealthObserver(value);
    }

    [ObserversRpc]
    public void ChangeCurrentHealthObserver(float value)
    {
        currentHealth += value;
        healthBar.value = currentHealth;
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            //trigger death

        }

        if(value < 0)
        {
            //trigger damage
            bloodSplatterUI.SetActive(true);
            HideBloodSplatter();
        }

        if(value > 0)
        {
            //trigger heal
        }
    }

    private void Update()
    {
        if (!IsOwner) return; // Chỉ kiểm tra phím bấm với chủ sở hữu

        // Nhấn phím O để trừ 10 máu
        if (Input.GetKeyDown(KeyCode.O))
        {
            ChangeCurrentHealth(-10);
        }

        // Nhấn phím P để test hồi máu 10
        if (Input.GetKeyDown(KeyCode.P))
        {
            ChangeCurrentHealth(10);
        }
    }

    private IEnumerator HideBloodSplatter()
    {
        yield return new WaitForSeconds(0.5f); // Thời gian hiển thị hiệu ứng máu (ví dụ: 0.5 giây)
        if (bloodSplatterUI != null)
        {
            bloodSplatterUI.SetActive(false);
        }
    }
}
