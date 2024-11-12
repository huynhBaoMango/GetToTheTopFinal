using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundWeapon : NetworkBehaviour
{
    public int wpIndex = -1;

    public int PickUpWeapon()
    {
        DespawnWeapon();
        return wpIndex;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnWeapon()
    {
        ServerManager.Despawn(gameObject);
    }
}
