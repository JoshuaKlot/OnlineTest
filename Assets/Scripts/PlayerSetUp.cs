using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefabA; //add prefab in inspector
    [SerializeField] private GameObject playerPrefabB; //add prefab in inspector

    void Start()
    {
        if (IsOwner) // Make sure only the owning client calls this
        {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        GameObject newPlayer;

        if (clientId == NetworkManager.Singleton.LocalClientId) // Host
        {
            newPlayer = Instantiate(playerPrefabA);
        }
        else // Non-host client
        {
            newPlayer = Instantiate(playerPrefabB);
        }

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }

}