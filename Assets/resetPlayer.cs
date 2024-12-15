using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resetPlayer : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.SetInt("prevCharacterId", -1);
        PlayerPrefs.SetInt("currentMoney", 3000);
    }
}
