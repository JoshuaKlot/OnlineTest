using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class Cursor : NetworkBehaviour
{
    [SerializeField] private GameObject coins;
    [SerializeField] private int numOfCoins;

    void Update()
    {
        if (!IsOwner) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(cursorWorldPosition.x, cursorWorldPosition.y, 0);

        // Prevent coin placement if clicking UI
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI() && numOfCoins > 0)
        {
            SpawnCoinServerRpc(transform.position);
            
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

        netObj.Spawn();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
