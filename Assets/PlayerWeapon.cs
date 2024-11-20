using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private List<APlayerWeapon> weapons = new List<APlayerWeapon>();
    [SerializeField] private APlayerWeapon currentWeapon;
    private int currentIndexWeapon = 0;
    private readonly SyncVar<int> _currentWeaponIndex = new(-1);

    [SerializeField] private Transform rightHandTarget, leftHandTarget, rightHint, leftHint;

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
        if (Input.GetKey(KeyCode.Mouse0))
        {
            currentWeapon.Fire();
        }
        if(currentWeapon.RightHandIKTarget != null) rightHandTarget.SetPositionAndRotation(currentWeapon.RightHandIKTarget.position, currentWeapon.RightHandIKTarget.rotation);
        if(currentWeapon.LeftHandIKTarget != null) leftHandTarget.SetPositionAndRotation(currentWeapon.LeftHandIKTarget.position, currentWeapon.LeftHandIKTarget.rotation);
        if(currentWeapon.rightHintIK != null) rightHint.position = currentWeapon.rightHintIK.position;
        if(currentWeapon.leftHintIK != null) leftHint.position = currentWeapon.leftHintIK.position;

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
        if (currentWeapon.RightHandIKTarget != null) rightHandTarget.SetPositionAndRotation(currentWeapon.RightHandIKTarget.position, currentWeapon.RightHandIKTarget.rotation);
        if (currentWeapon.LeftHandIKTarget != null) leftHandTarget.SetPositionAndRotation(currentWeapon.LeftHandIKTarget.position, currentWeapon.LeftHandIKTarget.rotation);
        if (currentWeapon.rightHintIK != null) rightHint.position = currentWeapon.rightHintIK.position;
        if (currentWeapon.leftHintIK != null) leftHint.position = currentWeapon.leftHintIK.position;
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
}
