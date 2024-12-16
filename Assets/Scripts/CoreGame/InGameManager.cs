using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class InGameManager : NetworkBehaviour
{
    private GameState _currentState;
    private int level;

    [Header("Heart Setup")]
    public GameObject heartPrefab;
    public Transform[] floors;


    [Header("Zombie Setup")]
    public NetworkObject[] zombiePrefabs;
    public List<Transform> zombieSpawnPosList;
    public ZombieSpawnController zombieSpawnController;


    [Header("Game Info")]
    private readonly SyncVar<float> Timer = new(0);
    private readonly SyncVar<string> BarName = new("");
    private readonly SyncVar<bool> endBool = new(false);
    private readonly SyncVar<int> floor = new(0);
    private readonly SyncVar<float> heartHealth = new(0);
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Slider heartHealthBar;
    [SerializeField] private Image SlideFill;
    [SerializeField] private TextMeshProUGUI progressNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject EndUI, WinUI;
    public GameObject[] players;
    public bool isCountdown, isShooting;


    [Header("Cutscene")]
    public GameObject cutsceneCam;
    public List<Transform> zombieCamList;
    public GameObject ExplodeFX;


    private void Start()
    {
        if (!base.IsServerInitialized)
            return;
        level = PlayerPrefs.GetInt("CurrentLevel", 1);
        floor.OnChange += OnChangeFloor;
        Timer.OnChange += OnChangeTimer;
        BarName.OnChange += OnChangeBarName;
        endBool.OnChange += OnChangeEndBool;
        heartHealth.OnChange += OnChangeHeartHealth;
        isCountdown = false;
        isShooting = false;

        floor.Value = level;
    }

    [ObserversRpc]
    private void OnChangeHeartHealth(float prev, float next, bool asServer)
    {
        heartHealthBar.value = next;
    }

    public void ChangeHeartHealthValue(float value)
    {
        heartHealth.Value = value;
    }

    [ObserversRpc]
    private void OnChangeFloor(int prev, int next, bool asServer)
    {
        levelText.text = "Floor " + next;
    }

    [ObserversRpc]
    private void OnChangeEndBool(bool prev, bool next, bool asServer)
    {
        EndUI.SetActive(next);
        Cursor.lockState = CursorLockMode.None;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject t in players)
        {
            t.GetComponent<PlayerControler>().canMove = false;
        }
        
        _currentState = GameState.End;
    }

    [ObserversRpc]
    private void OnChangeBarName(string prev, string next, bool asServer)
    {
        progressNameText.text = next;
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
        if (Input.GetKeyDown(KeyCode.U))
        {
            OnNextLevel();
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
        if (_currentState == GameState.Shooting && isShooting)
        {
            if (Timer.Value > 0)
            {
                Timer.Value -= Time.deltaTime;
            }
            else
            {
                ChangeState(GameState.End);
                GoToNextLevel();
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
                BarName.Value = "PREPARING...";
                break;
            case GameState.Shooting:
                StartShooting();
                BarName.Value = "SHOOTING!";
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
        level = PlayerPrefs.GetInt("CurrentLevel", 0);
        SpawnObject();
        SpawnTheHeart();
    }

    void StartPreparing()
    {
        UnableThis();
        Debug.Log("Preparing...");
        Timer.Value = 60;
        //SpawnZombieSpawns();
        //StartCoroutine(SpawnZombieSpawns());

        StartCutscene();
    }


    void StartShooting()
    {
        StartCountdownShooting();
        Debug.Log("Shooting...");
        InvokeTheSpawn();
    }


    void StartEnd()
    {
        Debug.Log("Ending...");
        CancelInvoke("SpawnZombie");

    }

    void StartRestart()
    {
        Debug.Log("Restarting...");
        ChangeState(GameState.Loading);
    }

    [ObserversRpc]
    void StartCountdownShooting()
    {
        SlideFill.color = Color.red;
        progressSlider.maxValue = 120 + (level * 0.1f * 60);
        Timer.Value = 120 + (level * 0.1f * 60);
        isShooting = true;
    }

    [ServerRpc]
    void UnableThis()
    {
        UnbleThisOb();
    }

    [ObserversRpc]
    void UnbleThisOb()
    {
        FindObjectOfType<DestroyColiObj>().enabled = false;
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
                ServerManager.Spawn(explode, null, gameObject.scene);
                //cho nay
                cutsceneCam.transform.DOShakePosition(1f, 0.5f);
                zombieSpawnController.EnableGivenSpawn(random);
            });

            yield return new WaitForSeconds(6f);
        }

        yield return new WaitForSeconds(1f);
        ServerManager.Despawn(cutsceneCam);
        isCountdown = true;
    }

    void GoToNextLevel()
    {
        //giet het zombie
        ZombieHealth[] allZombieHealths = FindObjectsOfType<ZombieHealth>();

        foreach (ZombieHealth zombieHealth in allZombieHealths)
        {
            zombieHealth.TakeDamage(500);
        }


      
        Cursor.lockState = CursorLockMode.None;
        WinUI.SetActive(true);
    }


    void SpawnTheHeart()
    {
        GameObject heart = Instantiate(heartPrefab, floors[UnityEngine.Random.Range(0, floors.Length-1)].position, Quaternion.identity);
        ServerManager.Spawn(heart, null, gameObject.scene);
    }

    void InvokeTheSpawn()
    {
        float spawnRate = 5f - (5f / 10 * level);
        InvokeRepeating("SpawnZombie", 1f, spawnRate);
    }

    void SpawnZombie()
    {
        Transform pos = zombieSpawnPosList[UnityEngine.Random.Range(0, zombieSpawnPosList.Count)];
        NetworkObject zombie = Instantiate(zombiePrefabs[Random.Range(0, zombiePrefabs.Length)], pos.position, Quaternion.identity);
        ServerManager.Spawn(zombie, null, gameObject.scene);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndGameTrigger()
    {
        endBool.Value = true;
    }

    public void OnNextLevel()
    {
        int level = PlayerPrefs.GetInt("CurrentLevel", 0);
        PlayerPrefs.SetInt("CurrentLevel", level + 1);
        foreach (GameObject go in players)
        {
            go.GetComponent<PlayerMoney>().SaveMoney();
        }
        string[] scenesToClose = new string[]
        {
            gameObject.scene.name
        };

        BootstrapNetworkManager.ChangeNetworkScene("Loading", scenesToClose);
    }

    public void OnBackToMenu()
    {
        BootstrapManager.BackToMenu();
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