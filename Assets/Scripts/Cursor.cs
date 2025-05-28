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
               
                Debug.Log("Despawning cursor");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetPlayerPhaseServerRpc();
                } 
                this.gameObject.SetActive(false);
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

}
