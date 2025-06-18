using UnityEngine;
using Unity.Netcode;

public class Coin : NetworkBehaviour
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

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsServer) return;

        NetworkObject playerObj = col.GetComponent<NetworkObject>();
        if (playerObj == null) return;

        ulong triggeringClientId = playerObj.OwnerClientId;

        // Allow only the intended player to collect the coin
        if (triggeringClientId == visibleToClientId)
        {
            GameManager.Instance.SendCoinMsg(triggeringClientId);
            NetworkObject.Despawn();
        }
    }



}
