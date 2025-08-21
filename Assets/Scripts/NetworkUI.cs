using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Debug = UnityEngine.Debug; // Ensure we use Unity's Debug

public class NetworkUI : NetworkBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button startButton;

    private bool isHostPlayer = false;

    private void Start()
    {
        hostButton.onClick.AddListener(() => {
            // Launch headless server
            LaunchHeadlessServer();
            
            // Start as client in THIS process
            NetworkManager.Singleton.StartClient();
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
            // Only the player who clicked host and is client should see this
            if (isHostPlayer && NetworkManager.Singleton.IsClient)
            {
     
                // Send a ServerRpc to the headless server to start the game
                GameManager.Instance.StartGame();
            }
        });

        SetStartButtonVisibility();
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
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Host Player] Failed to launch headless server: {e.Message}");
            Debug.LogError("Make sure you have built the server executable to the correct path!");
        }
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

    private void SetStartButtonVisibility()
    {
        // Only show the start button if this instance is the host player (the one who clicked host)
        startButton.gameObject.SetActive(isHostPlayer && NetworkManager.Singleton.IsClient);
    }
}
