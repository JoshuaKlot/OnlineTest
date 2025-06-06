using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class NetworkLogger : MonoBehaviour
{
    public static NetworkLogger Instance;

    [SerializeField] private bool logToConsole = true;
    [SerializeField] private UnityEngine.UI.Text logTextUI; // Optional: assign in inspector
    private List<string> logEntries = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        logTextUI.text = "Starting the game....\nPress the Start Host or Client Buttons";
        Debug.Log(logTextUI.text);
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        string msg = $"Client Connected: ID = {clientId} {(clientId == NetworkManager.Singleton.LocalClientId ? "(Local)" : "")}";
        AddLog(msg);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        string msg = $"Client Disconnected: ID = {clientId}";
        AddLog(msg);
    }

    public void AddLog(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string fullMessage = $"[{timestamp}] {message}";
        logEntries.Add(fullMessage);

        if (logToConsole)
            Debug.Log(fullMessage);

        if (logTextUI != null)
            logTextUI.text = string.Join("\n", logEntries);
    }

    // Optionally call this from other scripts to log custom info
    public static void Log(string message)
    {
        if (Instance != null)
            Instance.AddLog(message);
    }
}
