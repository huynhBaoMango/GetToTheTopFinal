using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FALWeapon : APlayerWeapon
{
    public override void AnimateWeapon()
    {
        Debug.Log("FAL Fire");
    }

    public override void Fire()
    {
        throw new System.NotImplementedException();
    }
}
