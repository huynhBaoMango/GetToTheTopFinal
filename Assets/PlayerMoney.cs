using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMoney : NetworkBehaviour
{
    [SerializeField] private float currentMoney = 3000;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject storeUI;
    [SerializeField] private Button healthBoostButton;
    [SerializeField] private Button ammoBoostButton;
    [SerializeField] private Button damageBoostButton;
    [SerializeField] private Button buyTrapButton; // Đổi tên thành buyTrapButton
    [SerializeField] private Button buyGasButton; // Thêm button mua gas
    [SerializeField] private GameObject prefabToBuy; // Prefab cần mua
    [SerializeField] private GameObject gasPrefab; // Prefab gas
    [SerializeField] private string healthBoostButtonTag = "HealthBoostButton";
    [SerializeField] private string ammoBoostButtonTag = "AmmoBoostButton";
    [SerializeField] private string damageBoostButtonTag = "DamageBoostButton";
    [SerializeField] private string buyTrapButtonTag = "BuyTrapButton"; // Đổi tên thành buyTrapButtonTag
    [SerializeField] private string buyGasButtonTag = "BuyGasButton"; // Tag của button mua gas

    private bool isStoreOpening;
    private PlayerWeapon playerWeaponManager;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // Khởi tạo UI
        moneyText = GameObject.FindWithTag("MoneyText").GetComponent<TextMeshProUGUI>();
        storeUI = GameObject.FindWithTag("StoreUI");
        healthBoostButton = GameObject.FindWithTag(healthBoostButtonTag)?.GetComponent<Button>();
        ammoBoostButton = GameObject.FindWithTag(ammoBoostButtonTag)?.GetComponent<Button>();
        damageBoostButton = GameObject.FindWithTag(damageBoostButtonTag)?.GetComponent<Button>();
        buyTrapButton = GameObject.FindWithTag(buyTrapButtonTag)?.GetComponent<Button>();
        buyGasButton = GameObject.FindWithTag(buyGasButtonTag)?.GetComponent<Button>();

        // Kiểm tra và lắng nghe sự kiện click cho các button
        if (healthBoostButton == null) Debug.LogError($"Không tìm thấy Health Boost Button! Tag: {healthBoostButtonTag}");
        else healthBoostButton.onClick.AddListener(OnHealthBoostButtonClicked);

        if (ammoBoostButton == null) Debug.LogError($"Không tìm thấy Ammo Boost Button! Tag: {ammoBoostButtonTag}");
        else ammoBoostButton.onClick.AddListener(OnAmmoBoostButtonClicked);

        if (damageBoostButton == null) Debug.LogError($"Không tìm thấy Damage Boost Button! Tag: {damageBoostButtonTag}");
        else damageBoostButton.onClick.AddListener(OnDamageBoostButtonClicked);

        if (buyTrapButton == null) Debug.LogError($"Không tìm thấy Buy Trap Button! Tag: {buyTrapButtonTag}");
        else buyTrapButton.onClick.AddListener(OnBuyTrapButtonClicked);

        if (buyGasButton == null) Debug.LogError($"Không tìm thấy Buy Gas Button! Tag: {buyGasButtonTag}");
        else buyGasButton.onClick.AddListener(OnBuyGasButtonClicked);

        playerWeaponManager = GetComponent<PlayerWeapon>();
        if (playerWeaponManager == null)
        {
            Debug.LogError("Không tìm thấy component PlayerWeapon!");
            return;
        }

        moneyText.text = currentMoney.ToString();
        storeUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && IsOwner)
        {
            OpenStore();
        }
    }

    public void OnHealthBoostButtonClicked()
    {
        if (currentMoney >= 100 && IsOwner)
        {
            CmdPurchaseHealthBoost();
        }
        else
        {
            Debug.Log("Không đủ tiền!");
        }
    }

    [ServerRpc]
    private void CmdPurchaseHealthBoost()
    {
        if (currentMoney >= 100)
        {
            ChangeCurrentMoney(-100);
            TargetApplyHealthBoost(Owner);
        }
    }

    [TargetRpc]
    private void TargetApplyHealthBoost(NetworkConnection target)
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.IncreaseHealth(0.3f);
        }
        else
        {
            Debug.LogError("Không tìm thấy component PlayerHealth!");
        }
    }

    public void OnAmmoBoostButtonClicked()
    {
        if (currentMoney >= 50 && playerWeaponManager != null && playerWeaponManager.currentWeapon != null && IsOwner)
        {
            CmdPurchaseAmmoBoost();
        }
        else
        {
            Debug.Log("Không đủ tiền hoặc không tìm thấy vũ khí!");
        }
    }

    [ServerRpc]
    private void CmdPurchaseAmmoBoost()
    {
        if (currentMoney >= 50 && playerWeaponManager != null && playerWeaponManager.currentWeapon != null)
        {
            ChangeCurrentMoney(-50);
            TargetApplyAmmoBoost(Owner);
        }
    }

    [TargetRpc]
    private void TargetApplyAmmoBoost(NetworkConnection target)
    {
        playerWeaponManager.currentWeapon.maxAmmo += 30;
    }

    public void OnDamageBoostButtonClicked()
    {
        if (currentMoney >= 150 && playerWeaponManager != null && playerWeaponManager.currentWeapon != null && IsOwner)
        {
            CmdPurchaseDamageBoost();
        }
        else
        {
            Debug.Log("Không đủ tiền hoặc không tìm thấy vũ khí!");
        }
    }

    [ServerRpc]
    private void CmdPurchaseDamageBoost()
    {
        if (currentMoney >= 150 && playerWeaponManager != null && playerWeaponManager.currentWeapon != null)
        {
            ChangeCurrentMoney(-150);
            TargetApplyDamageBoost(Owner);
        }
    }

    [TargetRpc]
    private void TargetApplyDamageBoost(NetworkConnection target)
    {
        playerWeaponManager.currentWeapon.damage += 5;
    }

    public void OnBuyTrapButtonClicked()
    {
        if (currentMoney >= 200 && IsOwner)
        {
            CmdPurchaseTrap();
        }
        else
        {
            Debug.Log("Không đủ tiền!");
        }
    }

    [ServerRpc]
    private void CmdPurchaseTrap()
    {
        if (currentMoney >= 200)
        {
            ChangeCurrentMoney(-200);
            Vector3 spawnPosition = transform.position + transform.forward * 2;
            GameObject instantiatedTrap = Instantiate(prefabToBuy, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = instantiatedTrap.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                Spawn(networkObject);
            }
            else
            {
                Debug.LogError("Trap prefab không có thành phần NetworkObject!");
            }
        }
    }

    public void OnBuyGasButtonClicked()
    {
        if (currentMoney >= 250 && IsOwner)
        {
            CmdPurchaseGas();
        }
        else
        {
            Debug.Log("Không đủ tiền!");
        }
    }

    [ServerRpc]
    private void CmdPurchaseGas()
    {
        if (currentMoney >= 250)
        {
            ChangeCurrentMoney(-250);
            Vector3 spawnPosition = transform.position + transform.forward * 2;
            GameObject instantiatedGas = Instantiate(gasPrefab, spawnPosition, Quaternion.identity);
            NetworkObject networkObject = instantiatedGas.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                Spawn(networkObject);
            }
            else
            {
                Debug.LogError("Gas prefab không có thành phần NetworkObject!");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeCurrentMoney(float value)
    {
        ChangeCurrentMoneyObserver(value);
    }

    [ObserversRpc]
    private void ChangeCurrentMoneyObserver(float value)
    {
        currentMoney += value;
        if (currentMoney < 0) currentMoney = 0;
        moneyText.text = currentMoney.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenStore()
    {
        OpenStoreObserver();
    }

    [ObserversRpc]
    private void OpenStoreObserver()
    {
        isStoreOpening = !isStoreOpening;
        storeUI.SetActive(isStoreOpening);

        if (IsOwner)
        {
            Cursor.lockState = isStoreOpening ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
