using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class HeadlessScript : MonoBehaviour
{
    [SerializeField] private ushort serverPort = 7777;
    [SerializeField] private string serverAddress = "127.0.0.1";

    void Start()
    {
        // Check if we're running with the -server argument
        bool isHeadlessServer = System.Environment.GetCommandLineArgs().Contains("-server");
        if (isHeadlessServer)
        {
            Debug.Log("[Headless] Starting server...");

            // IMPORTANT: Configure the transport BEFORE starting the server
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = serverAddress;
                transport.ConnectionData.Port = serverPort;
                Debug.Log($"[Headless] Configured server transport: {serverAddress}:{serverPort}");
            }
            else
            {
                Debug.LogError("[Headless] UnityTransport component not found!");
                return;
            }

            // Start the server
            bool serverStarted = NetworkManager.Singleton.StartServer();

            if (serverStarted && NetworkManager.Singleton.IsServer)
            {
                Debug.Log($"[Headless] Server started successfully on {serverAddress}:{serverPort}!");
            }
            else
            {
                Debug.LogError("[Headless] Failed to start server!");
            }
        }
        else
        {
            // Not running as headless, destroy this component
            Debug.Log("[Headless] Not running as server, destroying component");
            Destroy(this.gameObject);
        }
    }
}

