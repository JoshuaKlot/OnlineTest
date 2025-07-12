using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;

public class Cursor : NetworkBehaviour
{
    [SerializeField] private GameObject SpawnHere;
    [SerializeField] private GameObject Highlight;
    [SerializeField] private List<GameObject> currentSelection;
    [SerializeField] private Animator ani;
    [SerializeField] private GameObject coins;

    [SerializeField] private LayerMask obsticles;
    [SerializeField] private LayerMask sidewalk;
    [SerializeField] private LayerMask grass;
    [SerializeField] private LayerMask entrances;
    [SerializeField] private LayerMask selectableLayer;
    [SerializeField] private ObList obList;
    [SerializeField] public bool SetUpObsticles;
    [SerializeField] private bool ClickMap;
    private Vector3 selectedPosition;
    [SerializeField] private MasterObstacleListSO masterObstacleListSO;

    public List<GameObject> MasterObstacleList => masterObstacleListSO.MasterObstacleList;

    private void Awake()
    {
        SetUpObsticles = false;
        ClickMap = true;

    }

    void Update()
    {
        if (!IsOwner) return;
        Vector3 mouseWorld;
        if (SpawnHere != null)
            mouseWorld = SpawnHere.transform.position;
        else
            mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(mouseWorld);
        Vector3 snappedPosition = GridManager.Instance.GridToWorldCenter(gridPos);
        if (ClickMap)
        {
            Highlight.SetActive(true);
        }
        else
        {
            Highlight.SetActive(false);
        }
        
        Highlight.transform.position = snappedPosition;
        transform.position = new Vector3(mouseWorld.x, mouseWorld.y, 0);
        //transform.position = snappedPosition;

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            // Only check for selectable objects using the layer mask
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, selectableLayer);
            Debug.Log("Clicked on:" + hit.collider?.name);
            if (hit.collider != null)
            {
                var selectable = hit.collider.GetComponent<SelectableObject>();
                if (selectable != null)
                {
                    // Make the selectable spawn at the selection position
                    PlaceObsticle(obList.Position, selectable.SetNum);
                    obList.DeleteSelection();
                    return; // Don't process map click
                }
            }
            if (ClickMap)
            {
                Collider2D hitSidewalk = Physics2D.OverlapPoint(snappedPosition, sidewalk);
                Collider2D hitGrass = Physics2D.OverlapPoint(snappedPosition, grass);
                Collider2D hitEntrance = Physics2D.OverlapPoint(snappedPosition, entrances);
                selectedPosition = snappedPosition;
                if (hitSidewalk != null)
                    Debug.Log("Tile Type: Sidewalk");
                if (hitGrass != null)
                    Debug.Log("Tile Type: Grass");
                if (hitEntrance != null)
                    Debug.Log("Tile Type: Entrance");

                ani.SetTrigger("Click");
                if (!SetUpObsticles)
                {
                    if (hitEntrance == null)
                    {
                        NetworkLogger.Instance.AddLog("Please click on an entrance.");
                    }
                    else
                    {
                        obList.SpawnSelection(snappedPosition);
                        obList.Signals(ObList.ObsticalType.Entrances);
                        ClickMap = false;

                    }
                }
                else
                {
                    Collider2D hitStart = Physics2D.OverlapCircle(snappedPosition, 0.1f, obsticles);
                    if (hitStart.gameObject.tag == "Unmovable")
                    {
                        NetworkLogger.Instance.AddLog("Please do not place obsticles over entrances and exits");
                    }
                    else
                    {
                        obList.SpawnSelection(snappedPosition);
                        if (hitSidewalk != null)
                        {
                            obList.Signals(ObList.ObsticalType.Sidewalk);
                        }
                        else if (hitGrass != null)
                        {
                            obList.Signals(ObList.ObsticalType.Grass);
                        }
                    }
                }
            }
        }
    }

    public void SetSelection(List<GameObject> Selection)
    {
        currentSelection = Selection;
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
        Vector3 start = new Vector3(worldCenter.x, worldCenter.y, 5);
        Vector3 direction = new Vector3(0, 0, -10);


        // Check for coin at the position
        Collider2D hit = Physics2D.OverlapCircle(worldCenter, 0.1f, obsticles);
        Debug.Log(hit);
        if (hit != null)
        {
            OwnerOnlyVisibility coin = hit.GetComponent<OwnerOnlyVisibility>();
            if (coin != null && coin.visibleToClientId == NetworkManager.Singleton.ConnectedClients[OwnerClientId].ClientId)
            {
          
                
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

    private void SelectObsticle(GameObject selectionPrefab,Vector3 spawnAt)
    {
        GameObject selectionObj = Instantiate(selectionPrefab);
        selectionObj.transform.position = spawnAt;
        Selection selectionScript = selectionObj.GetComponent<Selection>();
        selectionScript.cursor = this;
        selectionScript.position = selectedPosition;
        selectionScript.SetSelection(); // Call this after assignment
        ClickMap = false;
    }

    public void PlaceObsticle(Vector3 spawnPosition, int selNum)
    {
        Debug.Log("Selecting Object: " + selNum);
        ClickMap = true;
        PlaceObjectServerRpc(spawnPosition, selNum);
    }

    [ServerRpc]
    private void PlaceObjectServerRpc(Vector3 spawnPosition, int selNum, ServerRpcParams rpcParams = default)
    {
        GameObject selectionPrefab = null;
        for (int i=0;i<MasterObstacleList.Count;i++)
        {
            
            if (selNum == i)
            {
                selectionPrefab = MasterObstacleList[i];
            }
        }
        // Check for coin at the position
        Collider2D hit = Physics2D.OverlapCircle(spawnPosition, 0.1f, obsticles);
        Debug.Log(hit);
        if (hit != null)
        {
            OwnerOnlyVisibility coin = hit.GetComponent<OwnerOnlyVisibility>();
            if (coin != null && coin.visibleToClientId == NetworkManager.Singleton.ConnectedClients[OwnerClientId].ClientId)
            {


                coin.NetworkObject.Despawn();
            }
        }
        if (selNum < 0 || selNum >= currentSelection.Count)
        {
            return;
        }
        GameObject placedObject = Instantiate(selectionPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Placing Object: " + placedObject.name);
        OwnerOnlyVisibility visibleComponent = placedObject.GetComponent<OwnerOnlyVisibility>();
        visibleComponent.visibleToClientId = rpcParams.Receive.SenderClientId;

        NetworkObject netObj = placedObject.GetComponent<NetworkObject>();
        netObj.CheckObjectVisibility = visibleComponent.CheckVisibility;

        netObj.Spawn();
    }

    [ServerRpc]
    public void SetSelectionServerRpc(int[] selectionIndices)
    {
        // Rebuild the currentSelection list on the server using indices
        // You need a master list of all possible prefabs on both client and server
        currentSelection = new List<GameObject>();
        foreach (int idx in selectionIndices)
        {
            currentSelection.Add(MasterObstacleList[idx]);
        }
    }
}
