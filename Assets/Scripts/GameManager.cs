using System.Collections;
using System.Collections.Generic;
using System.Linq; // Add this at the top
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    /// <summary>
    /// Set Up a dicttionary of playerSetUpObjects 
    /// and have them keep track of whatcursors have spawned and what playerSetUpObjects ae ready
    /// It will simplify the code much better
    /// </summary>
    private Dictionary<ulong, NetworkObject> playerSetUpObjects = new Dictionary<ulong, NetworkObject>();
    private bool playersSpawned = false;
    //private Dictionary<ulong, bool> playerReadyStatus = new Dictionary<ulong, bool>();
    //private Dictionary<ulong, NetworkObject> playerSetUpObjects = new Dictionary<ulong, NetworkObject>();
    //private Dictionary<ulong, NetworkObject> cameraTracker = new Dictionary<ulong, NetworkObject>();
    //private Dictionary<ulong, NetworkObject> playerSetUpObjects = new Dictionary<ulong, NetworkObject>();
    private bool gameStarted = false;
    private bool obsticlephase= false;

    // Add this field to GameManager
    //private Dictionary<ulong, Vector2> playerStartPositions = new Dictionary<ulong, Vector2>();

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
        

        AudioManager.Instance.PlayWaiting();
        PanelManager.Instance.ShowSetUpPhasePanelOnClients();

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                PlayerSpawner[] players = GameObject.FindObjectsOfType<PlayerSpawner>();
                foreach(PlayerSpawner player in players)
                {
                    if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
                    {
                        Debug.Log($"Registering Player Set Up Object for client {clientId}: {player.gameObject.name}");
                        // Register the playerSetUpObject with the GameManager
                        RegisterPlayerSetUpObjectServerRpc(clientId, new NetworkObjectReference(player.GetComponent<NetworkObject>()));
                    }
                }
                //playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().RegisterPlayerClientRpc();
                playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().SpawnPlayerACursorServerRpc(clientId);
            }
        }
        StartCoroutine(WaitForAllPlayersToRegister());
    }
    private void SyncVariables()
    {
        foreach (var kvp in playerSetUpObjects)
        {
            ulong clientId = kvp.Key;
            NetworkObject playerSetUpObject = kvp.Value;
            playerSetUpObjects[clientId].GetComponent<SyncVariables>().SetClientVariablesClientRpc(clientId, new NetworkObjectReference(playerSetUpObject));
        }
    }
    private IEnumerator WaitForAllPlayersToRegister()
    {
        // Get list of all expected non-host client IDs
        List<ulong> expectedClients = NetworkManager.Singleton.ConnectedClientsIds
            .Where(id => id != NetworkManager.ServerClientId).ToList();

        // Wait until all are present in the playerReadyStatus dictionary
        while (!expectedClients.All(id => playerSetUpObjects.ContainsKey(id)))
        {
            yield return null; // Wait a frame
        }

        Debug.Log("All non-host clients are registered.");
        gameStarted = true;
    }

    //Called From PlayerSetUp
    //public void RegisterCursor(ulong clientId, NetworkObject cursorObject)
    //{
    //    if (!playerSetUpObjects.ContainsKey(clientId))
    //    {
    //        Debug.Log("Adding " + cursorObject + " for client " + clientId);
    //        playerSetUpObjects.Add(clientId, cursorObject);
    //    }
    //}
    //public void RegisterPlayer(ulong clientId, NetworkObject playerObject)
    //{
    //    if (!playerSetUpObjects.ContainsKey(clientId))
    //    {
    //        playerSetUpObjects.Add(clientId, playerObject);

    //    }
    //}
    //public void RegisterCameraTracker(ulong clientId, NetworkObject trackerObject)
    //{
    //    if (!cameraTracker.ContainsKey(clientId))
    //    {
    //        cameraTracker.Add(clientId, trackerObject);

    //    }
    //}
    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerSetUpObjectServerRpc(ulong clientId, NetworkObjectReference playerSetUpObject)
    {
    if (!playerSetUpObject.TryGet(out NetworkObject playerSetUpObjectInstance))
    {
        Debug.LogError($"Failed to resolve player set up object for client {clientId}");
        return;
    }

    if (!playerSetUpObjects.ContainsKey(clientId))
    {
        playerSetUpObjects.Add(clientId, playerSetUpObjectInstance);
        Debug.Log($"Registered Player Set Up Object for client {clientId}: {playerSetUpObjectInstance.name}");
    }
    else
    {
        Debug.LogWarning($"Player Set Up Object for client {clientId} is already registered.");
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
    //private void DespawnAllCursors()
    //{

    //    foreach (var cursor in playerSetUpObjects.Values)
    //    {
    //        if (cursor.IsSpawned)
    //        {
    //            cursor.GetComponent<ObList>().DeleteSelection();
    //            cursor.Despawn();
    //        }
    //    }
    //}


    //private void DespawnTrackers()
    //{
    //    foreach (var tracker in cameraTracker.Values)
    //    {
    //        tracker.Despawn();
    //    }
    //}
    //private void DespawnAllPlayers()
    //{
    //    foreach (var player in playerSetUpObjects.Values)
    //    {
    //        Debug.Log("Despawning player " + player);
    //        player.Despawn();
    //    }
    //}
    private void Awake()
    {

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }
    [ClientRpc]
    private void DestroyActiveObjectClientRpc()
    {
        PlayerSpawner.Instance.DestroyActiveObject();
    }
    [ClientRpc]
    private void DestroyActiveCameraClientRpc()
    {
        PlayerSpawner.Instance.DestroyActiveTracker();
    }
    private void Start()
    {
        PanelManager.Instance.ShowLobbyOnClients();
    }
    //Called from PlayerSpawner
    //[ServerRpc(RequireOwnership = false)]
    //public void RegisterPlayerServerRpc(ServerRpcParams rpcParams = default)
    //{
    //    ulong clientId = rpcParams.Receive.SenderClientId;

    //    if (gameStarted)
    //    {
    //        Debug.LogWarning($"[Server] Game already started. Rejecting new player {clientId}.");
    //        return; // Prevent late joins
    //    }

    //    if (!playerSetUpObjects.ContainsKey(clientId))
    //    {
    //        Debug.Log($"[Server] Player {clientId} Registered");
    //        playerSetUpObjects(clientId).GetComponent<PlayerSpawner>.ready = false;
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"[Server] Player {clientId} was already registered.");
    //    }
    //}
    //Called from PickEntrancePanelUI
    //Called from PickEntrancePanelUI
    [ClientRpc]
    public void SetUpObsticalsClientRpc()
    {
        foreach (var kvp in playerSetUpObjects)
        {
            ulong clientId = kvp.Key;
            NetworkObject cursor = kvp.Value;
            Debug.Log("Setting Up " + cursor);

            // Optionally call on the server too:
            //cursor.GetComponent<Cursor>().ObsticleTime();
            bool found = playerSetUpObjects.TryGetValue(clientId, out NetworkObject cursorObj);

            // Then tell the client to run it
            if (found)
            {
                TriggerObsticleTimeClientRpc(clientId, cursorObj);
                Debug.Log($"Triggering ObsticleTime for client {clientId} with cursor {cursorObj.name}");
            }
            else
                Debug.LogError($"Cursor object for client {clientId} not found or not spawned.");
        }

        PanelManager.Instance.ShowCursorPhaseOnClients();
    }
    public void SetStartPoint(ulong clientId, Vector2 playerStart)
    {
        SetStartPoint(clientId, playerStart);
        Debug.Log("Setting start position for client " + clientId + " to " + playerStart);
        playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().SetStartPosition(playerStart);
    }


    [ClientRpc]
    public void TriggerObsticleTimeClientRpc(ulong clientId,NetworkObjectReference cursorRef, ClientRpcParams clientRpcParams = default)
    {

        playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().ObsticleSetUp();
        ////Debug.Log("Triggering ObsticleTime for client " + clientId);
        ////Debug.Log("LocalClientId: " + NetworkManager.Singleton.LocalClientId);
        ////Debug.Log("Cursors count: " + playerSetUpObjects.Count);
        //if (NetworkManager.Singleton.LocalClientId == clientId)
        //{
        //    //Debug.Log("This is the local client, calling ObsticleTime directly.");
        //    //bool found = playerSetUpObjects.TryGetValue(clientId, out NetworkObject cursorObj);
        //    //Debug.Log(found + " and we got " + cursorObj);
        //    if (cursorRef.TryGet(out NetworkObject cursorObj))
        //    {
        //        Debug.LogError($"Cursor object for client {clientId} not found or not spawned.");

        //        Debug.Log("Cursor Object found: " + cursorObj.name);
        //        var cursorComponent = cursorObj.GetComponent<Cursor>();
        //        if (cursorComponent != null)
        //        {
        //            Debug.Log("Cursor component found, calling ObsticleTime()");
        //            cursorComponent.ObsticleTime();
        //        }
        //        else
        //        {
        //            Debug.LogWarning("Cursor component NOT found on object: " + cursorObj.name);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError($"Cursor object for client {clientId} not found or not spawned.");
        //    }
        //}
        //else
        //{
        //    Debug.LogWarning($"Client {clientId} does not have a cursor registered.");
        //}
    }


    [ServerRpc(RequireOwnership = false)]
    public void MarkPlayerDonePlacingCoinsServerRpc(ServerRpcParams rpcParams = default)
    {
        
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!playerSetUpObjects.ContainsKey(clientId))
        {
            Debug.LogWarning($"Client {clientId} was not registered but attempted to mark done.");
            return;
        }

        playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().ready = true;
        Debug.Log($"Client {clientId} marked as done placing coins.");
        SendMsg.Instance.Ready(clientId);
        CheckIfAllPlayersAreDone(); // NEW: separate logic to advance phase
    }

    private void CheckIfAllPlayersAreDone()
    {
        Debug.Log("Checking if all playerSetUpObjects are done...");
        foreach (var kvp in playerSetUpObjects)
        {
            //Debug.Log($"Player {kvp.Key}: ready = {playerSetUpObjects[kvp].GetComponent<PlayerSpawner>().ready}");
        }

        //if (playersSpawned)
        //{
        //    Debug.Log("Players already spawned. Skipping.");
        //    return;
        //}

        if (AllPlayersReady())
        {
            Debug.Log("All playerSetUpObjects ready! Advancing phase.");
            if(obsticlephase==false)
            {
                
                obsticlephase = true;
                RevealCoinsToOtherPlayers();
                foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if(clientId !=0)
                        playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().ready=false; // Reset player ready status for the next phase
                }
                SetUpObsticalsClientRpc();

               SyncVariables();

            }
            else {
                
                Debug.Log("All playerSetUpObjects ready! Spawning playerSetUpObjects.");
                playersSpawned = true;
                DestroyActiveObjectClientRpc();
                RevealCoinsToOtherPlayers();
                SpawnAllPlayerB();
                PanelManager.Instance.ShowPlayerPhaseOnClients();
                SyncVariables();
            }
        }
        else
        {
            Debug.Log("Not all playerSetUpObjects are ready yet.");
        }
    }
    [ClientRpc]
    private void SpawnPlayerClientRpc(ulong clientId, Vector2 playerStart)
    {
        foreach (var cId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId == cId)
            {
                playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().StartHere=playerStart;
            }
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
        DestroyActiveObjectClientRpc();
        DestroyActiveCameraClientRpc();
        DespawnCoins();
        PanelManager.Instance.ShowLobbyOnClients();
        AudioManager.Instance.StopPlaying();
        playersSpawned = false;
        playerSetUpObjects.Clear();
        gameStarted = false;
        obsticlephase = false;
}

    private bool AllPlayersReady()
    {
        Debug.Log("There are "+ playerSetUpObjects.Count);
        foreach (var ready in playerSetUpObjects.Values)
        {
            
            if (!ready.GetComponent<PlayerSpawner>().ready)
                return false;
        }
        return true;
    }
    //Move this to PlayerSpawner
    private void SpawnAllPlayerB()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        Debug.Log("Spawning Player B for local client " + localId);
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.ServerClientId)
            {

                playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().SpawnPlayerBClientRpc(clientId);
                AudioManager.Instance.PlayPlaying();
                //StartCoroutine(WaitAndAttachCamera(clientId));

            }
            else
            {
                Debug.Log("Skipping host client when spawning Player B.");
            }
        }

    }




    //To Here
    void RevealCoinsToOtherPlayers()
    {
        // Group coins by their original owner
        Dictionary<ulong, List<OwnerOnlyVisibility>> coinsByOwner = new Dictionary<ulong, List<OwnerOnlyVisibility>>();
        OwnerOnlyVisibility[] allCoins = GameObject.FindObjectsOfType<OwnerOnlyVisibility>();

        foreach (OwnerOnlyVisibility coin in allCoins)
        {
            Debug.Log($"Coin {coin.name} visible to client {coin.visibleToClientId}");
            if (!coinsByOwner.ContainsKey(coin.visibleToClientId))
            {
                coinsByOwner[coin.visibleToClientId] = new List<OwnerOnlyVisibility>();
            }
            coinsByOwner[coin.visibleToClientId].Add(coin);
        }

        List<ulong> allClients = new List<ulong>(playerSetUpObjects.Keys);
        if (allClients.Count < 2)
        {
            Debug.LogWarning("Not enough playerSetUpObjects to exchange coins.");
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

    // Add this ServerRpc to receive the start position from the client
    [ServerRpc(RequireOwnership = false)]
    public void SubmitStartPositionServerRpc(ulong clientId,Vector2 startPosition, ServerRpcParams rpcParams = default)
    {
        //ulong clientId = rpcParams.Receive.SenderClientId;
        playerSetUpObjects[clientId].GetComponent<PlayerSpawner>().StartHere = startPosition;
        Debug.Log($"Received start position {startPosition} from client {clientId}");
    }

    //[ClientRpc]
    //private void SyncPlayersClientRpc(NetworkObjectReference netObj)
    //{
    //    NetworkObject pSetUp = netObj.TryGet(out NetworkObject playerSetUpObject);
    //    ulong clientId = NetworkManager.Singleton.LocalClientId;
    //    NetworkObject Obj = GameObject.FindObjectOfType<PlayerSpawner>();

    //}
}
