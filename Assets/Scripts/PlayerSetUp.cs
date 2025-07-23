using UnityEngine;
using Unity.Netcode;// Gets the Unity Netcode to use the function that come with it

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance;
    [SerializeField] private GameObject cameraTracker;
    [SerializeField] private GameObject cursor;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject activeObject;
    [SerializeField] private GameObject activeCamera;
    [SerializeField] public bool ready=true;
    [SerializeField] public Vector2 StartHere;
    [SerializeField] private GameObject cmCamera;
    private NetworkObject netObj;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        netObj = GetComponent<NetworkObject>();
    }
    [ClientRpc]
    public void RegisterPlayerClientRpc()
    {
        if (!IsHost)
        {
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Requesting player registration.");
            GameManager.Instance.RegisterPlayerSetUpObjectServerRpc(NetworkManager.Singleton.LocalClientId, netObj);
        }
    }
    public void SetStartPosition(Vector2 startPosition)
    {
        StartHere = startPosition;
    }
    //[ClientRpc]
    //public void TriggerSpawnPlayersClientRpc()
    //{
    //    if (IsClient && !IsHost)
    //    {
    //        SpawnPlayers();
    //    }
    //}

    //public void SpawnPlayers()
    //{
    //    Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] SpawnPlayers called.");
    //    if (IsClient && !IsHost)
    //    {
    //        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Registering with server.");
    //        GameManager.Instance.RegisterPlayerServerRpc();
    //    }
    //}
    
    public void DestroyActiveTracker()
    {
        if (activeCamera != null)
        {
            Debug.Log("Destroying active object: " + activeCamera.name);
            NetworkObject netObj = activeCamera.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                DespawnActiveObjectServerRpc(netObj);
            }
            Destroy(activeCamera);
            activeCamera = null;
        }
    }
    
    public void DestroyActiveObject()
    {
        if (activeObject != null)
        {
            Debug.Log("Destroying active object: " + activeObject.name);
            NetworkObject netObj = activeObject.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                DespawnActiveObjectServerRpc(netObj);
            }
            Destroy(activeObject);
            activeObject = null;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void DespawnActiveObjectServerRpc(NetworkObjectReference activeRef)
    {
        NetworkObject activeObj = activeRef.TryGet(out NetworkObject obj) ? obj : null;
        if(activeObject != null)
        {
            Debug.Log("Despawned active object: " + activeObj.name);

            if (activeObj != null && activeObj.IsSpawned)
            {
                activeObj.Despawn();
            }

        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerACursorServerRpc(ulong clientId)
    {
        Debug.Log("Spawning PLayer");
        GameObject newPlayer = Instantiate(cursor);
        activeObject = newPlayer;
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        Debug.Log("Spawning Camera Tracker");
        GameObject cTracker = Instantiate(cameraTracker);
        NetworkObject netObj2 = cTracker.GetComponent<NetworkObject>();
        netObj.CheckObjectVisibility = (targetClientId) =>
        {
            bool visible = targetClientId == clientId;
            Debug.Log($"[Server] Visibility check for {netObj.name} | TargetClientID: {targetClientId} | OwnerID: {clientId} => {visible}");
            return visible;
        };

        netObj2.CheckObjectVisibility = (targetClientId) =>
        {
            bool visible = targetClientId == clientId;
            Debug.Log($"[Server] Visibility check for {netObj2.name} | TargetClientID: {targetClientId} | OwnerID: {clientId} => {visible}");
            return visible;
        };

        newPlayer.SetActive(true);
        netObj.SpawnWithOwnership(clientId, true);
        netObj2.SpawnWithOwnership(clientId, true);

        Debug.Log("Register Cursor");

        // Register cursor in GameManager
        //GameManager.Instance.RegisterCursor(clientId, netObj);
        //GameManager.Instance.RegisterCameraTracker(clientId, netObj2);
    }

    public void MarkReady()
    {
        ready = true;
        Debug.Log("Player is ready");
    }
    public void resetReady()
    {
        ready = false;
        Debug.Log("Player is not ready");
    }
    [ClientRpc]
    public void SpawnPlayerBClientRpc(ulong clientId)
    {
        DeleteSelectionsClientRpc();
        Debug.Log("SPAWNING Da PLAYER for " + clientId + " on " + StartHere);
        GameObject newPlayer = Instantiate(player, StartHere, Quaternion.Euler(0, 0, 0));
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        activeObject = newPlayer;
        netObj.CheckObjectVisibility = (targetClientId) =>
        {
            bool visible = targetClientId == clientId;
            Debug.Log($"[Server] Visibility check for {netObj.name} | TargetClientID: {targetClientId} | OwnerID: {clientId} => {visible}");
            return visible;
        };
        SetUpFollow(activeCamera, activeObject);

        newPlayer.SetActive(true);
        SpawnOnServerRpc(clientId);
    }
    //private IEnumerator WaitAndAttachCamera(ulong clientId)
    //{
    //    float timeout = Time.time + 5f; // Timeout after 5 seconds

    //    while (!(playerSetUpObjects.ContainsKey(clientId) && playerSetUpObjects[clientId] != null))
    //    {
    //        if (Time.time > timeout)
    //        {
    //            Debug.LogError($"Timeout waiting for player with clientId {clientId} to spawn.");
    //            yield break;
    //        }

    //        Debug.Log($"Waiting for player {clientId}...");
    //        yield return null;
    //    }

    //    NetworkObject cameraNetObj = cameraObj.GetComponent<NetworkObject>();
    //    NetworkObject playerNetObj = playerObj.GetComponent<NetworkObject>();

        

    //}

    
    void SetUpFollow(NetworkObjectReference cameraRef, NetworkObjectReference playerRef, ClientRpcParams clientRpcParams = default)
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

    [ClientRpc]
    private void DeleteSelectionsClientRpc()
    {
        Selection[] sel = GameObject.FindObjectsOfType<Selection>();
        for (int i = 0; i < sel.Length; i++)
        {
            Destroy(sel[i].gameObject);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SpawnOnServerRpc(ulong clientId)
    {
        NetworkObject netObj = activeObject.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId, true);
        //GameManager.Instance.RegisterPlayer(clientId, netObj);
    }



}
