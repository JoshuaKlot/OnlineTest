using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;

public class UniqueBlockCode : NetworkBehaviour
{
    public string BlockCode; // Assign a unique string or int per block type in the Inspector

    [ClientRpc]
    private void DespawnBlocksOfTypeClientRpc(string blockCode, ClientRpcParams clientRpcParams = default)
    {
        var allBlocks = FindObjectsOfType<UniqueBlockCode>();
        foreach (var block in allBlocks)
        {
            if (block.BlockCode == blockCode)
            {
                var netObj = block.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                {
                    netObj.Despawn();
                }
            }
        }
    }
}