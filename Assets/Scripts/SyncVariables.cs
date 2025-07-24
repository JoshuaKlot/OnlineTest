using UnityEngine;
using Unity.Netcode;
public class SyncVariables : NetworkBehaviour
{
    [SerializeField] private PlayerSpawner variables;
    [ClientRpc]
    public void SetClientVariablesClientRpc(ulong clientId, NetworkObjectReference serverVariables)
    {
        Debug.Log($"Setting client variables for client {clientId}");
        Debug.Log("This is the client with ID: " + NetworkManager.Singleton.LocalClientId);
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (serverVariables.TryGet(out NetworkObject serverVariablesObject))
            {
                var serverPlayerSpawner = serverVariablesObject.GetComponent<PlayerSpawner>();
                if (variables == null)
                {
                    variables = GetComponent<PlayerSpawner>();
                }

                // Copy all public variables from server to client
                variables.activeObject = serverPlayerSpawner.activeObject;
                variables.activeCamera = serverPlayerSpawner.activeCamera;
                variables.ready = serverPlayerSpawner.ready;
                variables.StartHere = serverPlayerSpawner.StartHere;

                Debug.Log("Variables synchronized: " +
                    $"activeObject={variables.activeObject}, " +
                    $"activeCamera={variables.activeCamera}, " +
                    $"ready={variables.ready}, " +
                    $"StartHere={variables.StartHere}");
            }
            else
            {
                Debug.LogError("Failed to get server variables object.");
            }
        }
    }
}
