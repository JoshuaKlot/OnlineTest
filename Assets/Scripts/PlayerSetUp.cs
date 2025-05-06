using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefabA; //add prefab in inspector
    [SerializeField] private GameObject playerPrefabB; //add prefab in inspector

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        Transform cursor = this.transform.Find("Cursor");
        Transform player = this.transform.Find("Player");
        Transform activeObject;

        if (clientId == NetworkManager.Singleton.LocalClientId) // Host
        {
            
            cursor.gameObject.SetActive(true);
            activeObject = cursor;
            player.gameObject.SetActive(false);  
        }
        else // Non-host client
        {
            player.gameObject.SetActive(true);
            activeObject = player;
            cursor.gameObject.SetActive(false);
        }

        NetworkObject netObj = activeObject.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
