using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class Cursor : NetworkBehaviour
{

    [SerializeField] private GameObject coins;
    [SerializeField] private int numOfCoins;

    private void Awake()
    {
    }
    void Update()
    {
        if (!IsOwner) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(mouseWorld);
        Vector3 snappedPosition = GridManager.Instance.GridToWorldCenter(gridPos);
        transform.position = new Vector3(mouseWorld.x, mouseWorld.y, 0);
        //transform.position = snappedPosition;

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI() && numOfCoins > 0)
        {
            if (!GridManager.Instance.IsOccupied(gridPos))
            {
                GridManager.Instance.MarkOccupied(gridPos);
                SpawnCoinServerRpc(snappedPosition); // send snapped pos to avoid desync
            }
            else
            {
                TryDeleteCoinServerRpc(gridPos);
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

        netObj.Spawn();
    }

    [ServerRpc]
    private void TryDeleteCoinServerRpc(Vector2Int gridPos)
    {
        Vector3 worldCenter = GridManager.Instance.GridToWorldCenter(gridPos);

        // Check for coin at the position
        Collider2D hit = Physics2D.OverlapCircle(worldCenter, 0.1f);
        if (hit != null)
        {
            Coin coin = hit.GetComponent<Coin>();
            if (coin != null && coin.visibleToClientId == NetworkManager.Singleton.ConnectedClients[OwnerClientId].ClientId)
            {
                GridManager.Instance.MarkUnoccupied(gridPos);
                coin.NetworkObject.Despawn();
            }
        }
    }


    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
            {
                return true; // Pointer is over a button
            }
        }

        return false; // Pointer is not over any interactive UI
    }

}
