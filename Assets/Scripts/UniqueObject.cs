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
            // Find all UniqueObjects of the same type except this one
            foreach (var obj in FindObjectsOfType<UniqueObject>())
            {
                if (obj != this && obj.uniqueType == uniqueType && obj.NetworkObject.IsSpawned)
                {
                    obj.NetworkObject.Despawn();
                    Destroy(obj.gameObject);
                }
            }
        }
    }
}
