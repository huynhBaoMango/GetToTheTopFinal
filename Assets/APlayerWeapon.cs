using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class APlayerWeapon : NetworkBehaviour
{
    public abstract void Fire();
}
