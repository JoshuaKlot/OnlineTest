using UnityEngine;
using Unity.Netcode;// Gets the Unity Netcode to use the function that come with it

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] private GameObject playerPrefabA;
    [SerializeField] private GameObject playerPrefabB;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    void Start()
    {
        if (!IsServer)
        {
            GameManager.Instance.RegisterPlayerServerRpc(); // Only register for approval
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerACursorServerRpc(ulong clientId)
    {
        GameObject newPlayer = Instantiate(playerPrefabA);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();

        netObj.CheckObjectVisibility = (targetClientId) =>
        {
            bool visible = targetClientId == clientId;
            Debug.Log($"[Server] Visibility check for {netObj.name} | TargetClientID: {targetClientId} | OwnerID: {clientId} => {visible}");
            return visible;
        };

        newPlayer.SetActive(true);
        netObj.SpawnWithOwnership(clientId, true);

        Debug.Log("Register Cursor");

        // Register cursor in GameManager
        GameManager.Instance.RegisterCursor(clientId, netObj);
    }


    public void SpawnPlayerBForClient(ulong clientId)
    {
        GameObject newPlayer = Instantiate(playerPrefabB);
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
