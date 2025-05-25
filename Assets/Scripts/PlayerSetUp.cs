using UnityEngine;
using Unity.Netcode; // Gets the Unity Netcode to use the function that come with it

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance;

    [SerializeField] private GameObject playerPrefabA;
    [SerializeField] private GameObject playerPrefabB;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (IsOwner)
        {
            GameManager.Instance.RegisterPlayer(NetworkManager.Singleton.LocalClientId);
            SpawnPlayerACursorServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerACursorServerRpc(ulong clientId)
    {
        GameObject newPlayer = Instantiate(playerPrefabA);
        newPlayer.SetActive(true);
        newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    public void SpawnPlayerBForClient(ulong clientId)
    {
        GameObject newPlayer = Instantiate(playerPrefabB);
        newPlayer.SetActive(true);
        newPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }
}
