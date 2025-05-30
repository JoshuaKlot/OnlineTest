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
        Debug.Log("FUCKEREKEREOIOIROOIRIOIORROIR");
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!playerReadyStatus.ContainsKey(clientId))
        {
            Debug.Log($"[Server] Player {clientId} Registered");
            playerReadyStatus[clientId] = false;
        }
        else
        {
            Debug.LogWarning($"[Server] Player {clientId} was already registered.");
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void MarkPlayerDonePlacingCoinsServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("This function was called corretlty");
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!playerReadyStatus.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} was not registered but attempted to mark done.");
            return;
        }

        playerReadyStatus[clientId] = true;
        Debug.Log($"Client {clientId} marked as done placing coins.");

        CheckIfAllPlayersAreDone(); // NEW: separate logic to advance phase
    }

    private void CheckIfAllPlayersAreDone()
    {
        Debug.Log("Checking if all players are done...");
        foreach (var kvp in playerReadyStatus)
        {
            Debug.Log($"Player {kvp.Key}: ready = {kvp.Value}");
        }

        if (playersSpawned)
        {
            Debug.Log("Players already spawned. Skipping.");
            return;
        }

        if (AllPlayersReady())
        {
            Debug.Log("All players ready! Advancing phase.");
            playersSpawned = true;
            RevealCoinsToOtherPlayers();
            SpawnAllPlayerB();
        }
        else
        {
            Debug.Log("Not all players are ready yet.");
        }
    }


    private bool AllPlayersReady()
    {
        Debug.Log("There are "+playerReadyStatus.Count);
        foreach (var ready in playerReadyStatus.Values)
        {
            
            if (!ready)
                return false;
        }
        return true;
    }

    private void SpawnAllPlayerB()
    {
        DespawnAllCursors(); // Despawn cursors

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;

            //Skip the host
            if (clientId == NetworkManager.ServerClientId)
            {
                Debug.Log("Skipping host client when spawning Player B.");
                continue;
            }

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
