using UnityEngine;
using Unity.Netcode;// Gets the Unity Netcode to use the function that come with it

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance;
    [SerializeField] private GameObject cameraTracker;
    [SerializeField] private GameObject playerPrefabA;
    [SerializeField] private GameObject playerPrefabB;
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
        GameObject newPlayer = Instantiate(playerPrefabA);
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

        newPlayer.SetActive(true);
        netObj.SpawnWithOwnership(clientId, true);
        netObj2.SpawnWithOwnership(clientId, true);

        Debug.Log("Register Cursor");

        // Register cursor in GameManager
        GameManager.Instance.RegisterCursor(clientId, netObj);
        GameManager.Instance.RegisterCameraTracker(clientId, netObj2);
    }


    public void SpawnPlayerBForClient(ulong clientId)
    {
        GameObject newPlayer = Instantiate(playerPrefabB,StartHere);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();

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
