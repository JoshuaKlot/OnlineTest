using UnityEngine;
using Unity.Netcode;

public class Coin : NetworkBehaviour
{
    public ulong visibleToClientId;

    private void Start()
    {
        transform.parent = GameObject.Find("MainLevel").transform;

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

        ulong triggeringClientId = col.GetComponent<NetworkObject>()?.OwnerClientId ?? ulong.MaxValue;

        // Only allow interaction if the player who touched it is the one assigned
        if (triggeringClientId == visibleToClientId)
        {
            Debug.Log($"{col.gameObject.name} picked up coin {gameObject.name} at time {Time.time}");
            NetworkObject.Despawn(true);
        }
    }

}
