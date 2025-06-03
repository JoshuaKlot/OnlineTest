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

    private bool gameStarted = false;

    public void StartGame()
    {
        if (!IsServer) return;

        Debug.Log("Host started the game.");
        gameStarted = true;

        PanelManager.Instance.ShowCursorPhaseOnClients();

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                PlayerSpawner.Instance.SpawnPlayerACursorServerRpc(clientId);
            }
        }
    }



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
        PanelManager.Instance.ShowLobbyOnClients();
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (gameStarted)
        {
            Debug.LogWarning($"[Server] Game already started. Rejecting new player {clientId}.");
            return; // Prevent late joins
        }

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
        
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!playerReadyStatus.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} was not registered but attempted to mark done.");
            return;
        }

        playerReadyStatus[clientId] = true;
        cursors[clientId].Despawn();
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
        // Group coins by their original owner
        Dictionary<ulong, List<Coin>> coinsByOwner = new Dictionary<ulong, List<Coin>>();
        Coin[] allCoins = GameObject.FindObjectsOfType<Coin>();

        foreach (Coin coin in allCoins)
        {
            if (!coinsByOwner.ContainsKey(coin.visibleToClientId))
            {
                coinsByOwner[coin.visibleToClientId] = new List<Coin>();
            }
            coinsByOwner[coin.visibleToClientId].Add(coin);
        }

        List<ulong> allClients = new List<ulong>(playerReadyStatus.Keys);
        if (allClients.Count < 2)
        {
            Debug.LogWarning("Not enough players to exchange coins.");
            return;
        }

        // Shuffle clients to avoid deterministic patterns
        List<ulong> givers = new List<ulong>(allClients);
        List<ulong> receivers = new List<ulong>(allClients);
        ShuffleList(givers);
        ShuffleList(receivers);

        // Try to build a fair mapping
        Dictionary<ulong, ulong> giverToReceiver = new Dictionary<ulong, ulong>();
        HashSet<ulong> assignedReceivers = new HashSet<ulong>();

        foreach (ulong giver in givers)
        {
            ulong chosen = receivers
                .Where(r => r != giver && !assignedReceivers.Contains(r))
                .FirstOrDefault();

            // If no unassigned receiver available, allow duplicates
            if (chosen == 0 && receivers.Contains(0))
            {
                chosen = receivers.First(r => r != giver); // fallback
            }

            // Fallback: allow someone to receive multiple sets
            if (chosen == 0 || chosen == giver)
            {
                foreach (ulong r in receivers)
                {
                    if (r != giver)
                    {
                        chosen = r;
                        break;
                    }
                }
            }

            giverToReceiver[giver] = chosen;
            assignedReceivers.Add(chosen);
        }

        // Apply coin transfer
        foreach (var kvp in coinsByOwner)
        {
            ulong originalOwner = kvp.Key;
            if (!giverToReceiver.ContainsKey(originalOwner)) continue;

            ulong newOwner = giverToReceiver[originalOwner];
            foreach (Coin coin in kvp.Value)
            {
                if (coin.NetworkObject.IsSpawned)
                    coin.NetworkObject.Despawn(false);

                coin.visibleToClientId = newOwner;
                coin.NetworkObject.CheckObjectVisibility = coin.CheckVisibility;
                coin.NetworkObject.Spawn(false);
            }

            Debug.Log($"Transferred {kvp.Value.Count} coins from player {originalOwner} to player {newOwner}");
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int k = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[k];
            list[k] = temp;
        }
    }

    ulong PickRandomOtherClient(List<ulong> clients, ulong exclude)
    {
        List<ulong> others = clients.FindAll(id => id != exclude);
        return others[Random.Range(1, others.Count)];
    }

}
