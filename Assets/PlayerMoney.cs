using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMoney : NetworkBehaviour
{
    private float currentMoney;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject storeUI;
    private bool isStoreOpening;

    private void Awake()
    {
        currentMoney = 300;
        isStoreOpening = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            OpenStore();
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
        moneyText = GameObject.FindWithTag("MoneyText").GetComponent<TextMeshProUGUI>();
        storeUI = GameObject.FindWithTag("StoreUI");
        moneyText.text = currentMoney.ToString();
        storeUI.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeCurrentMoney(float value)
    {
        ChangeCurrentMoneyObserver(value);
    }

    [ObserversRpc]
    public void ChangeCurrentMoneyObserver(float value)
    {
        float temp = currentMoney + value;
        if (temp < 0) return;
        
        currentMoney = temp;
        moneyText.text = temp.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenStore()
    {
        OpenStoreObserver();
    }

    [ObserversRpc]
    void OpenStoreObserver()
    {
        isStoreOpening = !isStoreOpening;
        storeUI.SetActive(isStoreOpening);
        if (isStoreOpening) Cursor.lockState = CursorLockMode.None;
        else Cursor.lockState = CursorLockMode.Locked;
    }
}
