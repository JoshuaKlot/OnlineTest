using UnityEngine;
using Unity.Netcode; // Gets the Unity Netcode to use the function that come with it

public class PlayerSpawner : NetworkBehaviour// Network behaviour instead of the standard monobehavior
{
    [SerializeField] private GameObject playerPrefabA;
    [SerializeField] private GameObject playerPrefabB;

    void Start()// This is the PlayerPrefab that will automatically spawn once the host or client kis started
    {
        if (IsOwner) //Makes sure the owner is the only one who can use the RPC so both game doesnt spawn a player at once
        {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)//an RPC that will spawn the player on all computers within the network. A normal function will spawn it on the one its running on.
    {
        GameObject newPlayer;

        if (clientId == NetworkManager.Singleton.LocalClientId) //Check if we're running on the host
        {
            newPlayer = Instantiate(playerPrefabA);// If so we spanw in playerPrefabA( A cursor that spawns coins in)
        }
        else // Non-host client
        {
            newPlayer = Instantiate(playerPrefabB);// Otherwise we spawn in a player object that moves around collecting the coins
        }

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();// Sets the spawned object as a network object
        newPlayer.SetActive(true);//Set the player as active
        netObj.SpawnAsPlayerObject(clientId, true);// Make sure the player has the same owner as this object
    }

}