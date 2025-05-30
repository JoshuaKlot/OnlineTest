using UnityEngine;
using Unity.Netcode;


public class Cursor : NetworkBehaviour
{
    [SerializeField] private GameObject coins;
    [SerializeField] private int numOfCoins;
    [SerializeField] private GameObject mainLevel;
    void Awake()
    {
        Debug.Log("spawning cursor");
    }

    void Update()
    {
        
        if (!IsOwner) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(cursorWorldPosition.x, cursorWorldPosition.y, 0);

        if (Input.GetMouseButtonDown(0) && numOfCoins > 0)
        {
            SpawnCoinServerRpc(transform.position);
            numOfCoins--;
            if (numOfCoins == 0)
            {
                Debug.Log("Player finished placing all coins.");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.MarkPlayerDonePlacingCoinsServerRpc();
                }
                else
                {
                    Debug.Log("Theres no game manager. Wierd");
                }
                    DespawnCursorServerRpc();
            }

        }


    }

    [ServerRpc]
    private void SpawnCoinServerRpc(Vector3 spawnPosition, ServerRpcParams rpcParams = default)
    {
        GameObject placedCoin = Instantiate(coins, spawnPosition, Quaternion.identity);

        Coin coinComponent = placedCoin.GetComponent<Coin>();
        coinComponent.visibleToClientId = rpcParams.Receive.SenderClientId;

        NetworkObject netObj = placedCoin.GetComponent<NetworkObject>();
        netObj.CheckObjectVisibility = coinComponent.CheckVisibility;

        netObj.Spawn();  // do not pass ownership unless needed
    }
    [ServerRpc]
    private void DespawnCursorServerRpc(ServerRpcParams rpcParams = default)
    {
        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

}
