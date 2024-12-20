﻿using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace FishNet.Example
{
    public class NetworkHudCanvases : MonoBehaviour
    {
        #region Types.
        private enum AutoStartType
        {
            Disabled,
            Host,
            Server,
            Client
        }
        #endregion

        #region Serialized.
        [Tooltip("What connections to automatically start on play.")]
        [SerializeField]
        private AutoStartType _autoStartType = AutoStartType.Disabled;

        [Tooltip("Color when socket is stopped.")]
        [SerializeField]
        private Color _stoppedColor;

        [Tooltip("Color when socket is changing.")]
        [SerializeField]
        private Color _changingColor;

        [Tooltip("Color when socket is started.")]
        [SerializeField]
        private Color _startedColor;

        [Header("Indicators")]
        [Tooltip("Indicator for server state.")]
        [SerializeField]
        private Image _serverIndicator;

        [Tooltip("Indicator for client state.")]
        [SerializeField]
        private Image _clientIndicator;
        #endregion

        #region Private.
        private NetworkManager _networkManager;
        private LocalConnectionState _clientState = LocalConnectionState.Stopped;
        private LocalConnectionState _serverState = LocalConnectionState.Stopped;

#if !ENABLE_INPUT_SYSTEM
        private EventSystem _eventSystem;
#endif
        #endregion

        void OnGUI()
        {
#if ENABLE_INPUT_SYSTEM
            string GetNextStateText(LocalConnectionState state)
            {
                if (state == LocalConnectionState.Stopped)
                    return "Start";
                else if (state == LocalConnectionState.Starting)
                    return "Starting";
                else if (state == LocalConnectionState.Stopping)
                    return "Stopping";
                else if (state == LocalConnectionState.Started)
                    return "Stop";
                else
                    return "Invalid";
            }

            GUILayout.BeginArea(new Rect(4, 110, 256, 9000));
            Vector2 defaultResolution = new Vector2(1920f, 1080f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / defaultResolution.x, Screen.height / defaultResolution.y, 1));

            GUIStyle style = GUI.skin.GetStyle("button");
            int originalFontSize = style.fontSize;

            Vector2 buttonSize = new Vector2(165f, 42f);
            style.fontSize = 26;

            // Server button
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (GUILayout.Button($"{GetNextStateText(_serverState)} Server", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
                    OnClick_Server();
                GUILayout.Space(10f);
            }

            // Client button
            if (GUILayout.Button($"{GetNextStateText(_clientState)} Client", GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
                OnClick_Client();

            style.fontSize = originalFontSize;

            GUILayout.EndArea();
#endif
        }

        private void Start()
        {
#if !ENABLE_INPUT_SYSTEM
            SetEventSystem();
            BaseInputModule inputModule = FindObjectOfType<BaseInputModule>();
            if (inputModule == null)
                gameObject.AddComponent<StandaloneInputModule>();
#else
            _serverIndicator.transform.gameObject.SetActive(false);
            _clientIndicator.transform.gameObject.SetActive(false);
#endif

            _networkManager = FindObjectOfType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found, HUD will not function.");
                return;
            }
            else
            {
                UpdateColor(LocalConnectionState.Stopped, ref _serverIndicator);
                UpdateColor(LocalConnectionState.Stopped, ref _clientIndicator);
                _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
                _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
            }

            if (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Server)
                OnClick_Server();
            if (!Application.isBatchMode && (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Client))
                OnClick_Client();
        }

        private void OnDestroy()
        {
            if (_networkManager == null)
                return;

            _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        }

        private void UpdateColor(LocalConnectionState state, ref Image img)
        {
            Color c;
            if (state == LocalConnectionState.Started)
                c = _startedColor;
            else if (state == LocalConnectionState.Stopped)
                c = _stoppedColor;
            else
                c = _changingColor;

            img.color = c;
        }

        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            _clientState = obj.ConnectionState;
            UpdateColor(obj.ConnectionState, ref _clientIndicator);

            if (obj.ConnectionState == LocalConnectionState.Started)
            {
                
            }
        }

        private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
        {
            _serverState = obj.ConnectionState;
            UpdateColor(obj.ConnectionState, ref _serverIndicator);
        }

        public void OnClick_Server()
        {
            if (_networkManager == null)
                return;

            if (_serverState != LocalConnectionState.Stopped)
                _networkManager.ServerManager.StopConnection(true);
            else
                _networkManager.ServerManager.StartConnection();

            DeselectButtons();
        }

        public void OnClick_Client()
        {
            if (_networkManager == null)
                return;

            if (_clientState != LocalConnectionState.Stopped)
            {
                _networkManager.ClientManager.StopConnection();
            }
            else
            {
                _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
                _networkManager.ClientManager.StartConnection();
            }

            DeselectButtons();
        }

 
        private void SetEventSystem()
        {
#if !ENABLE_INPUT_SYSTEM
            if (_eventSystem != null)
                return;
            _eventSystem = FindObjectOfType<EventSystem>();
            if (_eventSystem == null)
                _eventSystem = gameObject.AddComponent<EventSystem>();
#endif
        }

        private void DeselectButtons()
        {
#if !ENABLE_INPUT_SYSTEM
            SetEventSystem();
            _eventSystem?.SetSelectedGameObject(null);
#endif
        }
    }
}
