using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    public bool isCountdown;


    [Header("Cutscene")]
    public GameObject cutsceneCam;
    public List<Transform> zombieCamList;
    public GameObject ExplodeFX;


    private void Start()
    {
        if (!base.IsServerInitialized)
            return;
        level = PlayerPrefs.GetInt("CurrentLevel", 1);
        Timer.OnChange += OnChangeTimer;
        isCountdown = false;
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

        if (_currentState == GameState.Prepare && isCountdown)
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
        //SpawnZombieSpawns();
        //StartCoroutine(SpawnZombieSpawns());

        StartCutscene();
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

    [ObserversRpc]
    void StartCutscene()
    {
        cutsceneCam.GetComponent<Camera>().enabled = true;
        cutsceneCam.transform.position = players[0].transform.position + Vector3.up;
        StartCoroutine(SpawnZombieSpawns());
    }

    IEnumerator SpawnZombieSpawns()
    {
        level = PlayerPrefs.GetInt("CurrentLevel", 0);
        cutsceneCam.GetComponent<Camera>().enabled = true;
        cutsceneCam.transform.position = players[0].transform.position + Vector3.up;

        int maxCount = ((level / 3) + 1) * 2;
        for (int i = 0; i < maxCount; i++)
        {
            int random = Random.Range(0, zombieSpawnController.zombieSpawns.Count);
            zombieSpawnPosList.Add(zombieSpawnController.zombieSpawns[random].transform.GetChild(0).transform);
            zombieCamList.Add(zombieSpawnController.zombieSpawns[random].transform.GetChild(1).transform);


            cutsceneCam.transform.DORotate(zombieCamList[i].rotation.eulerAngles, 1f);
            cutsceneCam.transform.DOMove(zombieCamList[i].position, 3f).OnComplete(() =>
            {
                GameObject explode = Instantiate(ExplodeFX, zombieSpawnPosList[i].position + new Vector3(0, 1.5f, 0), Quaternion.identity);
                ServerManager.Spawn(explode);
                cutsceneCam.transform.DOShakePosition(1f, 0.5f);
                zombieSpawnController.EnableGivenSpawn(random);
            });

            yield return new WaitForSeconds(6f);
        }

        yield return new WaitForSeconds(1f);
        ServerManager.Despawn(cutsceneCam);
        isCountdown = true;
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