using Unity.Netcode;
using UnityEngine;

public class SendMsg : NetworkBehaviour
{
    public static SendMsg Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
    }

    public void CoinCollected(ulong clientId)
    {
        CoinCollectedClientRpc(clientId);
    }
    [ClientRpc]
    private void CoinCollectedClientRpc(ulong collectorClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == collectorClientId)
        {
            Debug.Log("You collected a coin");
        }
        else
        {
            Debug.Log("Player " + collectorClientId + " collected a coin");
        }
    }

}
