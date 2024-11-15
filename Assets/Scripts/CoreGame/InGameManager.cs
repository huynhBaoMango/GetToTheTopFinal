using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem.WebGL;

public class InGameManager : NetworkBehaviour
{
    private GameState _currentState;

    [Header("Heart Setup")]
    public GameObject heartPrefab;
    public Transform spawnHeartPoint;
    public float spawnRange;
    public LayerMask groundLayer;


    private void Start()
    {
        if (!base.IsServerInitialized)
            return;

        ChangeState(GameState.Loading);
    }

    private void Update()
    {
        if (!base.IsServerInitialized)
            return;

        if (Input.GetKeyDown(KeyCode.F))
            ChangeState(_currentState + 1);
    }

    void ChangeState(GameState state)
    {
        _currentState = state;

        switch (state)
        {
            case GameState.None:
                break;
            case GameState.Loading:
                StartLoading();
                break;
            case GameState.Running:
                StartRunning();
                break;
            case GameState.End:
                StartEnd();
                break;
            case GameState.Restart:
                StartRestart();
                break;
            default:
                break;
        }
    }

    void StartLoading()
    {
        Debug.Log("Loading...");
        SpawnTheHeart();
    }

    void StartRunning()
    {
        Debug.Log("Running...");

    }

    void StartEnd()
    {
        Debug.Log("Ending...");

    }

    void StartRestart()
    {
        Debug.Log("Restarting...");
        ChangeState(GameState.Loading);
    }


    void SpawnTheHeart()
    {
        bool spawned = false; // Biến kiểm tra xem đã spawn được đối tượng hay chưa
        int attempts = 0;      // Số lần thử

        while (!spawned && attempts < 10000)
        {
            attempts++;

            // Tạo một vị trí ngẫu nhiên trong phạm vi đã chỉ định
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(spawnHeartPoint.position.x - spawnRange, spawnHeartPoint.position.x + spawnRange),
                10f,
                UnityEngine.Random.Range(spawnHeartPoint.position.z - spawnRange, spawnHeartPoint.position.z + spawnRange)
            );

            RaycastHit hit;


            if (Physics.Raycast(randomPosition, Vector3.down, out hit, Mathf.Infinity, groundLayer))
            {
                GameObject heart = Instantiate(heartPrefab, hit.point, Quaternion.identity);
                ServerManager.Spawn(heart);
                spawned = true; // Đánh dấu là đã spawn
            }
        }
    }

    enum GameState
    {
        None = 0,
        Loading = 1,
        Running = 2,
        End = 3,
        Restart = 4
    }
}