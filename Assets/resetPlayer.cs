using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class resetPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshPro highScoreText;

    void Start()
    {
        PlayerPrefs.SetInt("prevCharacterId", -1);
        PlayerPrefs.SetInt("currentMoney", 1000);

        highScoreText.text = "highscore: " + PlayerPrefs.GetInt("HighScore", 0);
    }
}
