using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private List<APlayerWeapon> weapons = new List<APlayerWeapon>();
    [SerializeField] private APlayerWeapon currentWeapon;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
            return;
        }
    }
    public void InitializeWeapons(Transform parentOfWeapons)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].transform.parent = parentOfWeapons;
        }

        InitializeWeapon(0);
    }

    private void InitializeWeapon(int weaponIndex)
    {
        for(int i = 0;i < weapons.Count;i++)
        {
            weapons[i].gameObject.SetActive(false);
        }

        if (weapons.Count > weaponIndex)
        {
            currentWeapon = weapons[weaponIndex]; 
        }
    }

    private void FireWeapon()
    {
        currentWeapon.Fire();
    }
}
