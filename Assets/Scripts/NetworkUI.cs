using System.Diagnostics;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Debug = UnityEngine.Debug;
using System.Runtime.CompilerServices; // Ensure we use Unity's Debug

public class NetworkUI : NetworkBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button startButton;

    private bool isHostPlayer = false;

    private void Start()
    {
        hostButton.onClick.AddListener(() =>
        {
            // Launch headless server
            LaunchHeadlessServer();

            // Start as client in THIS process
            StartCoroutine(ConnectAfterDelay());
            NetworkLogger.Log("[Host Player] Started as client, connecting to headless server");

            isHostPlayer = true;
            SetStartButtonVisibility();
        });


        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            NetworkLogger.Log("[Host] Starting Client");
            isHostPlayer = false;
            SetStartButtonVisibility();
        });
        startButton.onClick.AddListener(() =>
        {
            if (isHostPlayer && NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("[Host Player] Attempting to start game...");

                // Make sure this is a ServerRpc call
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGameServerRpc(); // Note the ServerRpc suffix
                }
                else
                {
                    Debug.LogError("[Host Player] GameManager instance is null!");
                }
            }
            else
            {
                Debug.LogWarning($"[Host Player] Cannot start game. IsHostPlayer: {isHostPlayer}, IsClient: {NetworkManager.Singleton.IsClient}, IsConnected: {NetworkManager.Singleton.IsConnectedClient}");
            }
        });
        SetStartButtonVisibility();
    }

    private IEnumerator ConnectAfterDelay()
    {
        NetworkLogger.Log("[Host Player] Waiting for server to initialize...");
        yield return new WaitForSeconds(3f);

        // Ensure we're connecting to the right address/port
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        transport.ConnectionData.Address = "127.0.0.1";
        transport.ConnectionData.Port = 7777; // Make sure this matches server

        NetworkManager.Singleton.StartClient();
        NetworkLogger.Log("[Host Player] Started as client, attempting connection to 127.0.0.1:7777");
    }
    private void LaunchHeadlessServer()
    {
#if UNITY_EDITOR
        // In editor, launch a new process of the game with command line arguments
        var editorPath = UnityEngine.Application.dataPath;
        var exePath = editorPath.Substring(0, editorPath.Length - 7) + "/Builds/HeadlessServer/Server.exe";
        
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "-batchmode -nographics -server",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            var process = Process.Start(processInfo);
            Debug.Log($"[Host Player] Launched headless server process: {process.Id}");
            StartCoroutine(CheckServerStatus(process));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Host Player] Failed to launch headless server: {e.Message}");
            Debug.LogError("Make sure you have built the server executable to the correct path!");
        }

    

        // Optional: Check if process is still running after a moment
        
#elif UNITY_STANDALONE
        // In built game, launch another copy of the game executable
        var process = new Process();
        process.StartInfo.FileName = Application.dataPath.Replace("_Data", ".exe");
        process.StartInfo.Arguments = "-batchmode -nographics -server";
        process.StartInfo.UseShellExecute = false;
        process.Start();
        Debug.Log($"[Host Player] Launched headless server process: {process.Id}");
#endif
    }
    private IEnumerator CheckServerStatus(Process serverProcess)
    {
        yield return new WaitForSeconds(2f);
        if (serverProcess != null && !serverProcess.HasExited)
        {
            Debug.Log("[Host Player] Server process is running");
        }
        else
        {
            Debug.LogError("[Host Player] Server process has exited!");
        }
    }
    private void SetStartButtonVisibility()
    {
        // Only show the start button if this instance is the host player (the one who clicked host)
        startButton.gameObject.SetActive(isHostPlayer && NetworkManager.Singleton.IsClient);
    }
}
