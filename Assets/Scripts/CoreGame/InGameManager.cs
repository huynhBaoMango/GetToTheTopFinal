using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.WebGL;

public class InGameManager : NetworkBehaviour
{
    private GameState _currentState;

    [Header("Heart Setup")]
    public GameObject heartPrefab;
    public Transform[] floors;

    public Transform CamPos;


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
        if (Input.GetKeyDown(KeyCode.P))
        {

        }

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
        GameObject heart = Instantiate(heartPrefab, floors[UnityEngine.Random.Range(0, floors.Length)].position, Quaternion.identity);
        ServerManager.Spawn(heart);
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