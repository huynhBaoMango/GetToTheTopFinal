using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMoney : NetworkBehaviour
{
    private float currentMoney;
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Awake()
    {
        currentMoney = 300;
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
        moneyText.text = currentMoney.ToString();
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
}
