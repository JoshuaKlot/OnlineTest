using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System.Linq; // Add this at the top

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    /// <summary>
    /// Set Up a dicttionary of playerSetUpObjects 
    /// and have them keep track of whatcursors have spawned and what players ae ready
    /// It will simplify the code much better
    /// </summary>
    private Dictionary<ulong, bool> playerReadyStatus = new Dictionary<ulong, bool>();
    private bool playersSpawned = false;
    private Dictionary<ulong, NetworkObject> cursors = new Dictionary<ulong, NetworkObject>();
    private Dictionary<ulong, NetworkObject> cameraTracker = new Dictionary<ulong, NetworkObject>();
    private Dictionary<ulong, NetworkObject> players = new Dictionary<ulong, NetworkObject>();
    private LayerMask sidewalk;
    private LayerMask grass;
    private bool gameStarted = false;
    private bool obsticlephase= false;

    //Called From NetworkUI
    public void StartGame()
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton.ConnectedClientsList.Count < 2)
        {
            NetworkLogger.Log("Connect at least one player before starting");
            return;
        }
        Debug.Log("Host started the game.");
        PlayerSpawner.Instance.TriggerSpawnPlayersClientRpc();

        AudioManager.Instance.PlayWaiting();
        PanelManager.Instance.ShowSetUpPhasePanelOnClients();

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                PlayerSpawner.Instance.SpawnPlayerACursorServerRpc(clientId);
            }
        }
        StartCoroutine(WaitForAllPlayersToRegister());
    }

    private IEnumerator WaitForAllPlayersToRegister()
    {
        // Get list of all expected non-host client IDs
        List<ulong> expectedClients = NetworkManager.Singleton.ConnectedClientsIds
            .Where(id => id != NetworkManager.ServerClientId).ToList();

        // Wait until all are present in the playerReadyStatus dictionary
        while (!expectedClients.All(id => playerReadyStatus.ContainsKey(id)))
        {
            yield return null; // Wait a frame
        }

        Debug.Log("All non-host clients are registered.");
        gameStarted = true;
    }

    //Called From PlayerSetUp
    public void RegisterCursor(ulong clientId, NetworkObject cursorObject)
    {
        if (!cursors.ContainsKey(clientId))
        {
            Debug.Log("Adding " + cursorObject + " for client " + clientId);
            cursors.Add(clientId, cursorObject);
        }
    }
    public void RegisterPlayer(ulong clientId, NetworkObject playerObject)
    {
        if (!players.ContainsKey(clientId))
        {
            players.Add(clientId, playerObject);

        }
    }
    public void RegisterCameraTracker(ulong clientId, NetworkObject trackerObject)
    {
        if (!cameraTracker.ContainsKey(clientId))
        {
            cameraTracker.Add(clientId, trackerObject);

        }
    }

    public void SetUpEntrances()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                PlayerSpawner.Instance.SpawnPlayerACursorServerRpc(clientId);
            }
        }
    }
    private void DespawnAllCursors()
    {

        foreach (var cursor in cursors.Values)
        {
            if (cursor.IsSpawned)
                cursor.Despawn();
        }
    }
    private void DespawnTrackers()
    {
        foreach (var tracker in cameraTracker.Values)
        {
            tracker.Despawn();
        }
    }
    private void DespawnAllPlayers()
    {
        foreach (var player in players.Values)
        {
            Debug.Log("Despawning player " + player);
            player.Despawn();
        }
    }
    private void Awake()
    {

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }
    private void Update()
    {

    }
    private void Start()
    {
        PanelManager.Instance.ShowLobbyOnClients();
    }
    //Called from PlayerSpawner
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
    //Called from PickEntrancePanelUI
    //Called from PickEntrancePanelUI
    [ServerRpc(RequireOwnership = false)]
    public void SetUpObsticalsServerRpc()
    {
        foreach (var kvp in cursors)
        {
            ulong clientId = kvp.Key;
            NetworkObject cursor = kvp.Value;
            Debug.Log("Setting Up " + cursor);

            // Optionally call on the server too:
            //cursor.GetComponent<Cursor>().ObsticleTime();
            bool found = cursors.TryGetValue(clientId, out NetworkObject cursorObj);

            // Then tell the client to run it
            if(found)
                TriggerObsticleTimeClientRpc(clientId, cursorObj);
            else
                Debug.LogError($"Cursor object for client {clientId} not found or not spawned.");
        }

        PanelManager.Instance.ShowCursorPhaseOnClients();
    }
    public void SetStartPoint(ulong clientId, Vector2 playerStart)
    {
        SetStartPointClientRpc(clientId, playerStart);
    }
    [ClientRpc]
    private void SetStartPointClientRpc(ulong clientId, Vector2 playerStart)
    {
        foreach (var cId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId == cId)
            {
                PlayerSpawner.Instance.SetStartPosition(playerStart);
            }
        }

    }
    [ClientRpc]
    public void TriggerObsticleTimeClientRpc(ulong clientId,NetworkObjectReference cursorRef, ClientRpcParams clientRpcParams = default)
    {
        //Debug.Log("Triggering ObsticleTime for client " + clientId);
        //Debug.Log("LocalClientId: " + NetworkManager.Singleton.LocalClientId);
        //Debug.Log("Cursors count: " + cursors.Count);
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            //Debug.Log("This is the local client, calling ObsticleTime directly.");
            //bool found = cursors.TryGetValue(clientId, out NetworkObject cursorObj);
            //Debug.Log(found + " and we got " + cursorObj);
            if (cursorRef.TryGet(out NetworkObject cursorObj))
            {
                Debug.LogError($"Cursor object for client {clientId} not found or not spawned.");

                Debug.Log("Cursor Object found: " + cursorObj.name);
                var cursorComponent = cursorObj.GetComponent<Cursor>();
                if (cursorComponent != null)
                {
                    Debug.Log("Cursor component found, calling ObsticleTime()");
                    cursorComponent.ObsticleTime();
                }
                else
                {
                    Debug.LogWarning("Cursor component NOT found on object: " + cursorObj.name);
                }
            }
            else
            {
                Debug.LogError($"Cursor object for client {clientId} not found or not spawned.");
            }
        }
        else
        {
            Debug.LogWarning($"Client {clientId} does not have a cursor registered.");
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
        if(obsticlephase)
            cursors[clientId].Despawn();
        Debug.Log($"Client {clientId} marked as done placing coins.");
        SendMsg.Instance.Ready(clientId);
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
            if(obsticlephase==false)
            {
                obsticlephase = true;
                foreach(var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    playerReadyStatus[clientId]=false; // Reset player ready status for the next phase
                }
                SetUpObsticalsServerRpc();
                
            }
            else { 
                playersSpawned = true;
                RevealCoinsToOtherPlayers();
                SpawnAllPlayerB();
                PanelManager.Instance.ShowPlayerPhaseOnClients();
            }
        }
        else
        {
            Debug.Log("Not all players are ready yet.");
        }
    }

    public void SendCoinMsg(ulong clientId)
    {
        Debug.Log(clientId + " collected a coin");
        SendMsg.Instance.CoinCollected(clientId);
    }
    public void SendExitReachedMsg(ulong clientId)
    {
        Debug.Log(clientId + " reached the exit");
        SendMsg.Instance.ExitReached(clientId);
    }

    public void ResetGame()
    {
        Debug.Log("Reseting game");
        DespawnAllPlayers();
        DespawnTrackers();
        DespawnCoins();
        PanelManager.Instance.ShowLobbyOnClients();
        AudioManager.Instance.StopPlaying();
        playersSpawned = false;
        playerReadyStatus.Clear();
        cursors.Clear();
        players.Clear();
        gameStarted = false;
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
        //DespawnAllCursors(); // Despawn cursors

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
            AudioManager.Instance.PlayPlaying();
            StartCoroutine(WaitAndAttachCamera(clientId));
        }
    }
    private IEnumerator WaitAndAttachCamera(ulong clientId)
    {
        float timeout = Time.time + 5f; // Timeout after 5 seconds

        while (!(players.ContainsKey(clientId) && players[clientId] != null))
        {
            if (Time.time > timeout)
            {
                Debug.LogError($"Timeout waiting for player with clientId {clientId} to spawn.");
                yield break;
            }

            Debug.Log($"Waiting for player {clientId}...");
            yield return null;
        }

        GameObject playerObj = players[clientId].gameObject;
        GameObject cameraObj = cameraTracker[clientId].gameObject;

        NetworkObject cameraNetObj = cameraObj.GetComponent<NetworkObject>();
        NetworkObject playerNetObj = playerObj.GetComponent<NetworkObject>();

        SetUpFollowClientRpc(cameraNetObj, playerNetObj);

    }
    [ClientRpc]
    void SetUpFollowClientRpc(NetworkObjectReference cameraRef, NetworkObjectReference playerRef, ClientRpcParams clientRpcParams = default)
    {
        if (cameraRef.TryGet(out NetworkObject cameraObj) && playerRef.TryGet(out NetworkObject playerObj))
        {
            var cameraMovement = cameraObj.GetComponent<CameraMovement>();
            cameraMovement.FollowPlayer(playerObj.gameObject);
            Debug.Log("CameraAttached");
        }
        else
        {
            Debug.LogError("Failed to resolve one or both NetworkObjectReferences.");
        }
    }

    void RevealCoinsToOtherPlayers()
    {
        // Group coins by their original owner
        Dictionary<ulong, List<OwnerOnlyVisibility>> coinsByOwner = new Dictionary<ulong, List<OwnerOnlyVisibility>>();
        OwnerOnlyVisibility[] allCoins = GameObject.FindObjectsOfType<OwnerOnlyVisibility>();

        foreach (OwnerOnlyVisibility coin in allCoins)
        {
            if (!coinsByOwner.ContainsKey(coin.visibleToClientId))
            {
                coinsByOwner[coin.visibleToClientId] = new List<OwnerOnlyVisibility>();
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

        // Build fair giver-to-receiver mapping: everyone gives to someone else, and everyone receives at least once.
        //Dictionary<ulong, ulong> giverToReceiver = new Dictionary<ulong, ulong>();

        int count = allClients.Count;
        for (int i = 0; i < count; i++)
        {
            ulong giver = allClients[i];
            ulong receiver = allClients[(i + 1) % count]; // Shift by 1
            giverToReceiver[giver] = receiver;
        }

        // Apply coin transfer
        foreach (var kvp in coinsByOwner)
        {
            ulong originalOwner = kvp.Key;
            if (!giverToReceiver.ContainsKey(originalOwner)) continue;

            ulong newOwner = giverToReceiver[originalOwner];
            foreach (OwnerOnlyVisibility coin in kvp.Value)
            {
                if (coin.NetworkObject.IsSpawned)
                    coin.NetworkObject.Despawn(false);

                coin.visibleToClientId = newOwner;
                //coin.NetworkObject.ChangeOwnership(newOwner);
                coin.NetworkObject.CheckObjectVisibility = coin.CheckVisibility;
                coin.NetworkObject.Spawn(false);
            }

            Debug.Log($"Transferred {kvp.Value.Count} coins from player {originalOwner} to player {newOwner}");
        }
    }

    void DespawnCoins()
    {
        OwnerOnlyVisibility[] allCoins = GameObject.FindObjectsOfType<OwnerOnlyVisibility>();
        foreach (OwnerOnlyVisibility coin in allCoins)
        {
            coin.NetworkObject.Despawn();
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
