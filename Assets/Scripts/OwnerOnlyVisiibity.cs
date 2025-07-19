using Unity.Netcode;
using UnityEngine;

public class OwnerOnlyVisibility : NetworkBehaviour
{   
    public ulong visibleToClientId;

    private void Start()
    {
        //transform.parent = GameObject.Find("MainLevel").transform;

        if (IsServer)
        {
            NetworkObject.CheckObjectVisibility = CheckVisibility;
        }
    }

    public bool CheckVisibility(ulong clientId)
    {
        return clientId == visibleToClientId;
    }


}
