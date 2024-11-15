using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using IO.Swagger.Model;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.WebGL;

public class InGameManager : NetworkBehaviour
{
    private GameState _currentState;
    private int level;

    [Header("Heart Setup")]
    public GameObject heartPrefab;
    public Transform[] floors;


    [Header("Zombie Setup")]
    public NetworkObject zombiePrefab;
    public List<GameObject> zombieSpawns;
    public List<Transform> zombieSpawnPosList;



    private void Start()
    {
        if (!base.IsServerInitialized)
            return;
        level = PlayerPrefs.GetInt("CurrentLevel", 1);
        ChangeState(GameState.Loading);
    }

    private void Update()
    {
        if (!base.IsServerInitialized)
            return;

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

    [Client]
    void UpdateZombieSpawnRender()
    {
        foreach (GameObject go in zombieSpawns)
        {
            foreach (Transform t in go.transform.GetChild(1).transform)
            {
               t.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    void StartLoading()
    {
        Debug.Log("Loading...");
        UpdateZombieSpawnRender();
        SpawnTheHeart();
        CreateZombieSpawns();
        
    }

    void StartRunning()
    {
        Debug.Log("Running...");
        InvokeTheSpawn();

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

    void InvokeTheSpawn()
    {
        float spawnRate = 5f;
        InvokeRepeating("SpawnZombie", 1f, spawnRate);
    }

    void SpawnZombie()
    {
        Transform pos = zombieSpawnPosList[UnityEngine.Random.Range(0, zombieSpawnPosList.Count)];
        NetworkObject zombie = Instantiate(zombiePrefab, pos.position, Quaternion.identity);
        ServerManager.Spawn(zombie);
    }

    void CreateZombieSpawns()
    {
        zombieSpawnPosList.Clear();
        if(level > 2 && level % 3 == 0)
        {
            int spawnCount = (level / 3);
            if(spawnCount > zombieSpawns.Count) spawnCount = zombieSpawns.Count;
            for(int i = 0; i < spawnCount; i++)
            {
                int random = UnityEngine.Random.Range(0, zombieSpawns.Count);
                zombieSpawns[random].SetActive(true);
                zombieSpawnPosList.Add(zombieSpawns[random].transform.GetChild(0));
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