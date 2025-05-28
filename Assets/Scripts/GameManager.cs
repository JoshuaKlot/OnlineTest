using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private Dictionary<ulong, bool> playerReadyStatus = new Dictionary<ulong, bool>();
    private bool playersSpawned = false;
    private Dictionary<ulong, NetworkObject> cursors = new Dictionary<ulong, NetworkObject>();

    public void RegisterCursor(ulong clientId, NetworkObject cursorObject)
    {
        if (!cursors.ContainsKey(clientId))
        {
            cursors.Add(clientId, cursorObject);
            Debug.Log("Cursor Registered");
        }
    }

    private void DespawnAllCursors()
    {
        foreach (var cursor in cursors.Values)
        {
            Debug.Log("Despawning Cursor "+cursor);
            if (cursor.IsSpawned)
                cursor.Despawn();
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterPlayer(ulong clientId)
    {
        if (!playerReadyStatus.ContainsKey(clientId))
        {
            playerReadyStatus.Add(clientId, false);
            Debug.Log("Player Registered");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerPhaseServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"Client {clientId} is ready.");
        playerReadyStatus[clientId] = true;

        if (AllPlayersReady() && !playersSpawned)
        {
            playersSpawned = true;
            RevealCoinsToOtherPlayers();
            SpawnAllPlayerB();
        }

    }

    private bool AllPlayersReady()
    {
        foreach (var ready in playerReadyStatus.Values)
        {
            
            if (!ready)
                return false;
        }
        return true;
    }

    private void SpawnAllPlayerB()
    {
        DespawnAllCursors(); // Despawn the cursor (playerPrefabA)

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;
            PlayerSpawner.Instance.SpawnPlayerBForClient(clientId);
        }
    }

}
