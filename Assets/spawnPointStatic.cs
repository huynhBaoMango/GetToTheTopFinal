using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnPointStatic : MonoBehaviour
{
    public static spawnPointStatic instance;

    private void Awake()
    {
        instance = this;
    }
}
