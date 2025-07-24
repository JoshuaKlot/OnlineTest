using UnityEngine;
using Unity.Netcode;
public class SyncVariables : NetworkBehaviour
{
    [SerializeField] private PlayerSpawner variables;
    [ClientRpc]
    public void SetClientVariablesClientRpc(ulong clientId,NetworkObjectReference serverVariables)
    {
        Debug.Log($"Setting client variables for client {clientId}");
        Debug.Log("This is the client with ID: " + NetworkManager.Singleton.LocalClientId);
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            if(serverVariables.TryGet(out NetworkObject serverVariablesObject))
            {
                variables = serverVariablesObject.GetComponent<PlayerSpawner>();
                Debug.Log("Now " + variables.StartHere + " is equal to " + serverVariablesObject.GetComponent<PlayerSpawner>().StartHere);
            }
            else
            {
                Debug.LogError("Failed to get server variables object.");
            }
           
        }
        
    }
}
