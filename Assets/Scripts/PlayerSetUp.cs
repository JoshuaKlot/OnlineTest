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
        if (IsOwner)
        {
            Debug.Log("I am a host");
            GameManager.Instance.RegisterPlayer(NetworkManager.Singleton.LocalClientId);
            SpawnPlayerACursorServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            Debug.Log("I am not a host");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerACursorServerRpc(ulong clientId)
    {
        GameObject newPlayer;
        newPlayer = Instantiate(playerPrefabA);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        
        netObj.SpawnAsPlayerObject(clientId, true); 
        Debug.Log("Register Cursor");
        // Register cursor in GameManager
        GameManager.Instance.RegisterCursor(clientId, netObj);
    }



    public void SpawnPlayerBForClient(ulong clientId)
    {
        GameObject newPlayer = Instantiate(playerPrefabB);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }

}
