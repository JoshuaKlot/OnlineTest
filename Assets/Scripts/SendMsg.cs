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
    public void ExitReached(ulong clientId)
    {
        ExitReachedClientRpc(clientId);
    }
    public void Ready(ulong clientId)
    {
        ReadyMsgClientRpc(clientId);
    }

    public void SetUpEntrances(ulong clientId)
    {
        SetUpEntrancesClientRpc(clientId);
    }
    [ClientRpc]
    private void CoinCollectedClientRpc(ulong collectorClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == collectorClientId)
        {
            NetworkLogger.Log("You collected a coin");
        }
        else
        {
            NetworkLogger.Log("Player " + collectorClientId + " collected a coin");
        }
    }
    [ClientRpc]
    private void ExitReachedClientRpc(ulong collectorClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == collectorClientId)
        {
            NetworkLogger.Log("You reached the exit");
        }
        else
        {
            NetworkLogger.Log("Player " + collectorClientId + " reached the exit");
        }
    }
    [ClientRpc]
    private void ReadyMsgClientRpc(ulong ReadyClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == ReadyClientId)
        {
            NetworkLogger.Log("You are ready");
        }
        else
        {
            NetworkLogger.Log("Player " + ReadyClientId + " is ready");
        }
    }
    [ClientRpc]
    private void SetUpEntrancesClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            NetworkLogger.Log("You set up the entrances");
        }
        else
        {
            NetworkLogger.Log("Player " + clientId + " set up the entrances");
        }
    }
}
