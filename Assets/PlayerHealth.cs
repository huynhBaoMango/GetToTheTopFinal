using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentHealth;
    [SerializeField] private GameObject bloodSplatterUI;
    [SerializeField] private Slider healthBar;

    [Header("Sounds")]
    public AudioClip heat;
    public AudioClip dead;

    private void Awake()
    {
        bloodSplatterUI.SetActive(false);
        currentHealth = maxHealth;
    }
    private void Update()
    {
        if (!IsOwner) return; // Chỉ thực hiện trên máy chủ sở hữu player

        if (Input.GetKeyDown(KeyCode.O))
        {
            ChangeCurrentHealth(-10);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ChangeCurrentHealth(10);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        healthBar = GameObject.FindWithTag("HealthBar").GetComponent<Slider>(); // Make sure "HealthBar" tag is set
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    [ServerRpc]
    public void ChangeCurrentHealth(float value)
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(heat);
        ChangeCurrentHealthObserver(value);
    }

    [ObserversRpc]
    public void ChangeCurrentHealthObserver(float value)
    {
        currentHealth += value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            // Xử lý khi chết
            FindAnyObjectByType<InGameManager>().EndGameTrigger();
            gameObject.GetComponent<AudioSource>().PlayOneShot(dead);
        }

        if (value < 0)
        {
            bloodSplatterUI.SetActive(true);
            StartCoroutine(HideBloodSplatter());
        }
    }

    public void IncreaseHealth(float percentageIncrease)
    {
        maxHealth *= (1 + percentageIncrease);
        currentHealth = Mathf.Min(currentHealth + (maxHealth * percentageIncrease), maxHealth);

        
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;

        ChangeCurrentHealth(0); 
    }


    private IEnumerator HideBloodSplatter()
    {
        yield return new WaitForSeconds(0.5f);
        bloodSplatterUI.SetActive(false);
    }
}