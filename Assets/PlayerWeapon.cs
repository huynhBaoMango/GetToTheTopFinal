using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private List<APlayerWeapon> weapons = new List<APlayerWeapon>();
    [SerializeField] private APlayerWeapon currentWeapon;
    private int currentIndexWeapon = 0;
    private readonly SyncVar<int> _currentWeaponIndex = new(-1);

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
            if(currentIndexWeapon == weapons.Count-1) currentIndexWeapon = 0;
            else currentIndexWeapon++;

            InitializeWeapon(currentIndexWeapon);
        }
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            FireWeapon();
        }    
    }

    private void OnCurrentWeaponIndexChange(int oldIndex, int newIndex, bool asServer)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }

        if (weapons.Count > newIndex)
        {
            currentWeapon = weapons[newIndex];
            currentWeapon.gameObject.SetActive(true);
        }
    }

    [ServerRpc] private void SetWeaponIndex (int weaponIndex) => _currentWeaponIndex.Value = weaponIndex;

    public void InitializeWeapons(Transform parentOfWeapons)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].transform.parent = parentOfWeapons;
        }

        InitializeWeapon(0);
    }

    public void InitializeWeapon(int weaponIndex)
    {
        SetWeaponIndex(weaponIndex);
    }

    private void FireWeapon()
    {
        currentWeapon.Fire();
    }
}
