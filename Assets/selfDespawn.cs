using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class selfDespawn : NetworkBehaviour
{
    [SerializeField]
    private float m_Delay = 1f;
    [SerializeField]
    private GameObject fx;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(this.m_Delay);
        DestroyFX(); 
    }


    [ObserversRpc]
    void DestroyFX()
    {
        Instantiate(fx, gameObject.transform.position, Quaternion.identity);
        Despawn(gameObject);
    }
}
