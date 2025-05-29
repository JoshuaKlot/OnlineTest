using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq; // Add this at the top

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
        if (clientId == NetworkManager.ServerClientId)
        {
            Debug.Log("Host is not a player. Skipping registration.");
            return;
        }

        if (!playerReadyStatus.ContainsKey(clientId))
        {
            playerReadyStatus.Add(clientId, false);
            Debug.Log($"Player {clientId} Registered");
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
        Debug.Log(playerReadyStatus.Count);
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

    void RevealCoinsToOtherPlayers()
    {
        Coin[] allCoins = GameObject.FindObjectsOfType<Coin>();
        List<ulong> clients = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);

        foreach (Coin coin in allCoins)
        {
            ulong originalClient = coin.visibleToClientId;
            ulong newClient = PickRandomOtherClient(clients, originalClient);

            if (coin.NetworkObject.IsSpawned)
                coin.NetworkObject.Despawn(false); // Don’t destroy, reuse

            coin.visibleToClientId = newClient;
            coin.NetworkObject.CheckObjectVisibility = coin.CheckVisibility;

            coin.NetworkObject.Spawn(false); // Re-spawn with updated visibility
        }
    }


    ulong PickRandomOtherClient(List<ulong> clients, ulong exclude)
    {
        List<ulong> others = clients.FindAll(id => id != exclude);
        return others[Random.Range(0, others.Count)];
    }

}
