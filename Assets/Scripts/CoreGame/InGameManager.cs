using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DG.Tweening;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using IO.Swagger.Model;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Random = UnityEngine.Random;

public class InGameManager : NetworkBehaviour
{
    private GameState _currentState;
    private int level;

    [Header("Heart Setup")]
    public GameObject heartPrefab;
    public Transform[] floors;


    [Header("Zombie Setup")]
    public NetworkObject zombiePrefab;
    public List<Transform> zombieSpawnPosList;
    public ZombieSpawnController zombieSpawnController;


    [Header("Game Info")]
    private readonly SyncVar<float> Timer = new(0);
    [SerializeField] private Slider progressSlider;
    public GameObject[] players;


    [Header("Cutscene")]
    public GameObject cutsceneCam;
    public List<Transform> zombieCamList;


    private void Start()
    {
        if (!base.IsServerInitialized)
            return;
        level = PlayerPrefs.GetInt("CurrentLevel", 1);
        Timer.OnChange += OnChangeTimer;
    }

    public void UpdatePlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    
    private void OnChangeTimer(float prev, float next, bool asServer)
    {
        if(next >= 0)
        {
            UpdateSliderTimer(next);
        }
    }

    [ObserversRpc]
    void UpdateSliderTimer(float next)
    {
        progressSlider.value = next;
    }

    private void Update()
    {
        if (!base.IsServerInitialized)
            return;
        if (Input.GetKeyDown(KeyCode.J))
        {
            ChangeState(_currentState+1);
        }

        if (_currentState == GameState.Prepare)
        {
            if(Timer.Value > 0)
            {
                Timer.Value -= Time.deltaTime;
            }
            else
            {
                ChangeState(_currentState + 1);
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ChangeState(GameState.Loading);
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
            case GameState.Prepare:
                StartPreparing();
                break;
            case GameState.Shooting:
                StartShooting();
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
        zombieSpawnController.DisableAllZombieSpawns();
        SpawnObject();
        SpawnTheHeart();
    }

    void StartPreparing()
    {
        Debug.Log("Preparing...");
        Timer.Value = 120;
        SpawnZombieSpawns();
 
       // StartCutsceneServer();
    }

    void StartCutsceneServer()
    {
        StartCutscene();
    }

    [ObserversRpc]
    void StartCutscene()
    {
        cutsceneCam.GetComponent<Camera>().enabled = true;
        cutsceneCam.transform.position = players[0].transform.position + Vector3.up;
        cutsceneCam.transform.DOMove(zombieSpawnController.zombieSpawns[0].transform.GetChild(1).transform.position, 3f);
        cutsceneCam.transform.DORotate(zombieSpawnController.zombieSpawns[0].transform.GetChild(1).transform.rotation.eulerAngles, 3f).OnComplete(() =>
        {
            //this just work on the host
            cutsceneCam.GetComponent<Camera>().enabled = false;
        });
        
    }

    void StartShooting()
    {
        Debug.Log("Shooting...");
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

    void SpawnObject()
    {
        foreach(Transform t in floors)
        {
            if(t.TryGetComponent<GridMaker>(out GridMaker grid1))
            {
                grid1.DotheSpawn();
            }
           
        }
        if (floors[0].TryGetComponent<GridMaker>(out GridMaker grid))
        {
            grid.BuidNavMesh();
        }
    }

    void SpawnZombieSpawns()
    {
        level = PlayerPrefs.GetInt("CurrentLevel", 0);

        cutsceneCam.GetComponent<Camera>().enabled = true;
        cutsceneCam.transform.position = players[0].transform.position + Vector3.up;


        if (level > 2 && level % 3 == 0)
        {
            int maxCount = level / 3;
            for(int i = 0; i < maxCount; i++)
            {
                int random = Random.Range(0, zombieSpawnController.zombieSpawns.Count);

                zombieSpawnController.EnableGivenSpawn(random);
                zombieSpawnPosList.Add(zombieSpawnController.zombieSpawns[random].transform.GetChild(0).transform);
                zombieCamList.Add(zombieSpawnController.zombieSpawns[random].transform.GetChild(1).transform);

                cutsceneCam.transform.DOMove(zombieCamList[i].position, 3f);
                cutsceneCam.transform.DORotate(zombieCamList[i].rotation.eulerAngles, 3f).OnComplete(() =>
                {
                    //this just work on the host
                    zombieSpawnController.EnableGivenSpawn(random);
                });

                
            }
        }
        else
        {
            int random = Random.Range(0, zombieSpawnController.zombieSpawns.Count);
            zombieSpawnPosList.Add(zombieSpawnController.zombieSpawns[random].transform.GetChild(0).transform);
            zombieCamList.Add(zombieSpawnController.zombieSpawns[random].transform.GetChild(1).transform);

            cutsceneCam.transform.DOMove(zombieCamList[0].position, 3f);
            cutsceneCam.transform.DORotate(zombieCamList[0].rotation.eulerAngles, 3f).OnComplete(() =>
            {
                //this just work on the host
                zombieSpawnController.EnableGivenSpawn(random);

                
            });
        }
        //cutsceneCam.GetComponent<Camera>().enabled = false;
    }

    void SpawnTheHeart()
    {
        GameObject heart = Instantiate(heartPrefab, floors[UnityEngine.Random.Range(0, floors.Length-1)].position, Quaternion.identity);
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

    

    enum GameState
    {
        None = 0,
        Loading = 1,
        Prepare = 2,
        Shooting = 3,
        End = 4,
        Restart = 5
    }
}