
using Unity.Netcode;
using UnityEngine;

public class OwnerOnlyVisibility : NetworkBehaviour
{
    private NetworkObject netObj;

    private void Awake()
    {
        netObj = GetComponent<NetworkObject>();
        if (netObj != null)
        {
            Debug.Log("Owner "+OwnerClientId);
            netObj.CheckObjectVisibility = IsVisibleToClient;
        }
        else
        {
            Debug.LogWarning("No NetworkObject found on object with OwnerOnlyVisibility.");
        }
    }

    private bool IsVisibleToClient(ulong clientId)
    {
        Debug.Log("Client id:"+ clientId);
        return clientId == OwnerClientId;
    }
}
