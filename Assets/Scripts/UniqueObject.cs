using UnityEngine;
using Unity.Netcode;

public class UniqueObject : NetworkBehaviour
{
    public enum UniqueType { Start, Exit }
    public UniqueType uniqueType;

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
        {
            // Only the host can despawn objects, so ask the server to do it
            foreach (var obj in FindObjectsOfType<UniqueObject>())
            {
                if (obj != this && obj.uniqueType == uniqueType && obj.NetworkObject.IsSpawned)
                {
                    // Ask the server to despawn this object's NetworkObjectId
                    DespawnObjectServerRpc(obj.NetworkObject.NetworkObjectId);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnObjectServerRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
        {
            netObj.Despawn();
            Destroy(netObj.gameObject); // optional, only if you want to remove it entirely
        }
    }
}
