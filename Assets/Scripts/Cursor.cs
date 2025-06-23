using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Collections.Specialized;

public class Cursor : NetworkBehaviour
{
    [SerializeField] private GameObject SpawnHere;
    [SerializeField] private GameObject Highlight;
    [SerializeField] private Animator ani;
    [SerializeField] private GameObject coins;
    [SerializeField] private LayerMask obsticles;
    [SerializeField] private LayerMask sidewalk;
    [SerializeField] private LayerMask grass;
    [SerializeField] private LayerMask entrances;
    private void Awake()
    {
    }
    void Update()
    {
        if (!IsOwner) return;
        Vector3 mouseWorld;
        if(SpawnHere != null)
            mouseWorld = SpawnHere.transform.position;
        else
            mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(mouseWorld);
        Vector3 snappedPosition = GridManager.Instance.GridToWorldCenter(gridPos);
        Highlight.transform.position = snappedPosition;
        transform.position = new Vector3(mouseWorld.x, mouseWorld.y, 0);
        //transform.position = snappedPosition;

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            ani.SetTrigger("Click");
            if (!GridManager.Instance.IsOccupied(gridPos))
            {
                Debug.Log("Isnt Occupied");
                GridManager.Instance.MarkOccupied(gridPos);
                SpawnCoinServerRpc(snappedPosition); // send snapped pos to avoid desync
            }
            else
            {
                GridManager.Instance.MarkUnoccupied(gridPos);
                Debug.Log("Occupied");
                TryDeleteCoinServerRpc(gridPos);
            }
        }
    }

    [ServerRpc]
    private void SpawnCoinServerRpc(Vector3 spawnPosition, ServerRpcParams rpcParams = default)
    {
        Collider2D hitSidewalk = Physics2D.OverlapCircle(spawnPosition, 0.1f, sidewalk);
        Collider2D hitGrass = Physics2D.OverlapCircle(spawnPosition, 0.1f, grass);
        Collider2D hitEntrance = Physics2D.OverlapCircle(spawnPosition, 0.1f, entrances);
        if (hitSidewalk != null)
            Debug.Log("Hit Sidewalk");
        if (hitGrass != null)
            Debug.Log("Hit Grass");
        if (hitEntrance != null)
            Debug.Log("Hit Entrance");
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
        Vector3 start = new Vector3(worldCenter.x, worldCenter.y, 5);
        Vector3 direction = new Vector3(0, 0, -10);


        // Check for coin at the position
        Collider2D hit = Physics2D.OverlapCircle(worldCenter, 0.1f, obsticles);
        Debug.Log(hit);
        if (hit != null)
        {
            Coin coin = hit.GetComponent<Coin>();
            if (coin != null && coin.visibleToClientId == NetworkManager.Singleton.ConnectedClients[OwnerClientId].ClientId)
            {
                Debug.Log("Got a coin lololol");
                
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
