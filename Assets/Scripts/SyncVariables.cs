using UnityEngine;
using Unity.Netcode;
public class SyncVariables : NetworkBehaviour
{
    [SerializeField] private PlayerSpawner variables;
    [ClientRpc]
    public void SetClientVariablesClientRpc(ulong clientId,NetworkObjectReference serverVariables)
    {
        Debug.Log($"Setting client variables for client {clientId}");
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            if(serverVariables.TryGet(out NetworkObject serverVariablesObject))
            {
                variables = serverVariablesObject.GetComponent<PlayerSpawner>();
            }
            else
            {
                Debug.LogError("Failed to get server variables object.");
            }
        }
    }
}
