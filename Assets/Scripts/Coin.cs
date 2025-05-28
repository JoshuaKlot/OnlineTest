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
        if (IsServer)
        {
            Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
            NetworkObject.Despawn(true);
        }
    }
}
