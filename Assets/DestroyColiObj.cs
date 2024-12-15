using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyColiObj : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision != null)
        {
            DestroyThis(collision.gameObject);
        }
    }

    [ServerRpc]
    void DestroyThis(GameObject g)
    {
        Despawn(g);
    }
}
