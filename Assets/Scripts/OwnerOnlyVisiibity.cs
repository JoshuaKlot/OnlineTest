using Unity.Netcode;
using UnityEngine;

public class OwnerOnlyVisibility : NetworkBehaviour
{
    private NetworkObject netObj;

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server needs to set visibility callbacks
        {
            NetworkObject netObject = GetComponent<NetworkObject>();
            
            if (netObject != null)
            {
                netObject.NetworkShow(OwnerClientId);
                Debug.Log($"[Server] Setting visibility callback for {netObj.name} | Owner: {OwnerClientId}");
                //netObj.CheckObjectVisibility = IsVisibleToClient;
            }
            else
            {
                Debug.LogWarning("[Server] No NetworkObject found on object with OwnerOnlyVisibility.");
            }
        }
    }

    private bool IsVisibleToClient(ulong clientId)
    {
        bool visible = clientId == OwnerClientId;
        Debug.Log($"Visibility check for object {netObj.name} | clientId: {clientId} | ownerId: {OwnerClientId} => {visible}");
        return visible;
    }

}
