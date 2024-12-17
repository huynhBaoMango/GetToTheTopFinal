using FishNet.Object;
using SlimUI.ModernMenu;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuUI;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button backtomenuButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private string resumeButtonTag = "resumeButton";
    [SerializeField] private string settingButtonTag = "settingButton";
    [SerializeField] private string backtomenuButtonTag = "backtomenuButton";
    [SerializeField] private string exitButtonTag = "exitButton";

    private bool isMenuOpening;

    public void Start()
    {
        //Khởi tạo UI
        menuUI = GameObject.FindWithTag("menuUI");
        resumeButton = GameObject.FindWithTag(resumeButtonTag)?.GetComponent<Button>();
        settingButton = GameObject.FindWithTag(settingButtonTag)?.GetComponent<Button>();
        backtomenuButton = GameObject.FindWithTag(backtomenuButtonTag)?.GetComponent<Button>();
        exitButton = GameObject.FindWithTag(exitButtonTag)?.GetComponent<Button>();

        // Kiểm tra và lắng nghe sự kiện click cho các button
        if (resumeButton == null) Debug.LogError($"Không tìm thấy Resume Button! Tag: {resumeButtonTag}");
        else resumeButton.onClick.AddListener(OnResumeButton);

        if (settingButton == null) Debug.LogError($"Không tìm thấy Setting Button! Tag: {settingButtonTag}");
        else settingButton.onClick.AddListener(OnSettingButton);

        if (backtomenuButton == null) Debug.LogError($"Không tìm thấy BacktoMenu Button! Tag: {backtomenuButtonTag}");
        else backtomenuButton.onClick.AddListener(OnBackButton);

        if (exitButton == null) Debug.LogError($"Không tìm thấy Exit Button! Tag: {exitButtonTag}");
        else exitButton.onClick.AddListener(OnExitButton);

        menuUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Menu is opened");
            OpenMenu();
        }
    }
    public void OpenMenu()
    {
        isMenuOpening = !isMenuOpening;
        menuUI.SetActive(isMenuOpening);
        Cursor.lockState = isMenuOpening ? CursorLockMode.None : CursorLockMode.Locked;
        //GetComponent<PlayerWeapon>().canFire = false;
        //GetComponent<PlayerControler>().canLook = false;
    }

    public void OnResumeButton()
    {
        isMenuOpening = false;
        menuUI.SetActive(isMenuOpening);
    }

    public void OnSettingButton()
    {

    }

    public void OnBackButton()
    {
        InGameManager ingameManager = GetComponent<InGameManager>();
        ingameManager.OnBackToMenu();
    }
    public void OnExitButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
		    Application.Quit();
        #endif
    }
}
