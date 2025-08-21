//using System.Diagnostics;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class HeadlessScript : MonoBehaviour
{
    void Start()
    {
        // Check if we're running with the -server argument
        bool isHeadlessServer = System.Environment.GetCommandLineArgs().Contains("-server");

        if (isHeadlessServer)
        {
            Debug.Log("[Headless] Starting server...");

            // Start the server
            NetworkManager.Singleton.StartServer();

            // Optional: Log successful start
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log("[Headless] Server started successfully!");
            }
            else
            {
                Debug.LogError("[Headless] Failed to start server!");
            }
        }
        else
        {
            // Not running as headless, destroy this component
            Destroy(this.gameObject);
        }
    }
}