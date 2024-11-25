using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private List<APlayerWeapon> weapons = new List<APlayerWeapon>();
    [SerializeField] public APlayerWeapon currentWeapon;
    private int currentIndexWeapon = 0;
    private readonly SyncVar<int> _currentWeaponIndex = new(-1);

    [SerializeField] private Transform rightHandTarget, leftHandTarget, rightHint, leftHint;

    // Biến kiểm tra trạng thái tắt/mở bắn súng
    private bool canFire = true;

    private void Awake()
    {
        _currentWeaponIndex.OnChange += OnCurrentWeaponIndexChange;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
            return;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentIndexWeapon = (currentIndexWeapon == weapons.Count - 1) ? 0 : currentIndexWeapon + 1;
            InitializeWeapon(currentIndexWeapon);
        }
        // Chỉ bắn nếu canFire là true
        if (Input.GetKey(KeyCode.Mouse0) && canFire)
        {
            currentWeapon.Fire();
        }

        if (Input.GetKey(KeyCode.R))
        {
            currentWeapon.Reload();
        }

        ProcessAnimation();
    }
    void ProcessAnimation()
    {
        ProcessAnimationServer();
        if (currentWeapon.RightHandIKTarget != null) rightHandTarget.SetPositionAndRotation(currentWeapon.RightHandIKTarget.position, currentWeapon.RightHandIKTarget.rotation);
        if (currentWeapon.LeftHandIKTarget != null) leftHandTarget.SetPositionAndRotation(currentWeapon.LeftHandIKTarget.position, currentWeapon.LeftHandIKTarget.rotation);
        if (currentWeapon.rightHintIK != null) rightHint.position = currentWeapon.rightHintIK.position;
        if (currentWeapon.leftHintIK != null) leftHint.position = currentWeapon.leftHintIK.position;
    }

    [ServerRpc(RequireOwnership = false)]
    void ProcessAnimationServer()
    {
        ProcessAnimationObserver();
    }

    [ObserversRpc(ExcludeOwner = true)]
    void ProcessAnimationObserver()
    {
        if (currentWeapon.RightHandIKTarget != null) rightHandTarget.SetPositionAndRotation(currentWeapon.RightHandIKTarget.position, currentWeapon.RightHandIKTarget.rotation);
        if (currentWeapon.LeftHandIKTarget != null) leftHandTarget.SetPositionAndRotation(currentWeapon.LeftHandIKTarget.position, currentWeapon.LeftHandIKTarget.rotation);
        if (currentWeapon.rightHintIK != null) rightHint.position = currentWeapon.rightHintIK.position;
        if (currentWeapon.leftHintIK != null) leftHint.position = currentWeapon.leftHintIK.position;
    }

    private void OnCurrentWeaponIndexChange(int oldIndex, int newIndex, bool asServer)
    {
        foreach (var weapon in weapons)
        {
            weapon.gameObject.SetActive(false);
        }

        if (newIndex < weapons.Count)
        {
            currentWeapon = weapons[newIndex];
            currentWeapon.gameObject.SetActive(true);
        }
        ProcessAnimation();
    }

    
    private void SetWeaponIndex(int weaponIndex) => _currentWeaponIndex.Value = weaponIndex;

    public void InitializeWeapons(Transform parentOfWeapons)
    {
        foreach (var weapon in weapons)
        {
            weapon.transform.parent = parentOfWeapons;
        }

        InitializeWeapon(0);
    }

    [ServerRpc]
    public void InitializeWeapon(int weaponIndex)
    {
        SetWeaponIndex(weaponIndex);
    }

    // Hàm để bật/tắt bắn
    public void SetCanFire(bool value)
    {
        canFire = value;
    }

}
