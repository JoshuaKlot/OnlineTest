using UnityEngine;
using Unity.Netcode;// Gets the Unity Netcode to use the function that come with it

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance;
    [SerializeField] private GameObject cameraTracker;
    [SerializeField] private GameObject cursor;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject activeObject;
    [SerializeField] public bool ready;
    private Vector2 StartHere;
    [SerializeField] private GameObject cmCamera;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetStartPosition(Vector2 startPosition)
    {
        StartHere = startPosition;
    }
    [ClientRpc]
    public void TriggerSpawnPlayersClientRpc()
    {
        if (IsClient && !IsHost)
        {
            SpawnPlayers();
        }
    }

    public void SpawnPlayers()
    {
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] SpawnPlayers called.");
        if (IsClient && !IsHost)
        {
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Registering with server.");
            GameManager.Instance.RegisterPlayerServerRpc();
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
        GameManager.Instance.RegisterCursor(clientId, netObj);
        GameManager.Instance.RegisterCameraTracker(clientId, netObj2);
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
    public void SpawnPlayerBForClient(ulong clientId)
    {
        Destroy(activeObject);
        GameObject newPlayer = Instantiate(player,StartHere,Quaternion.EulerRotation(0,0,0));
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        activeObject = newPlayer;
        //Set visibility callback
        netObj.CheckObjectVisibility = (targetClientId) =>
        {
            return targetClientId == clientId;
        };

        newPlayer.SetActive(true);
        netObj.SpawnWithOwnership(clientId, true);
        GameManager.Instance.RegisterPlayer(clientId, netObj);
    }

}
