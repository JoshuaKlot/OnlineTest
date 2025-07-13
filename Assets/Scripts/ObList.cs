using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ObList : NetworkBehaviour
{
    private bool selectedSpawned;
    [SerializeField] private GameObject SelectionPrefab;
    private GameObject activeSelection;
    public Vector3 Position;
    public enum ObsticalType { Entrances, Sidewalk, Grass }
    [System.Serializable]
    public struct ObsticalList
    {
        public ObsticalType Type;
        public List<GameObject> Obsticles;
    }

    [SerializeField] private List<ObsticalList> MasterList;
    [SerializeField] private MasterObstacleListSO masterObstacleListSO;
    [SerializeField] private LayerMask obsticles;
    public List<GameObject> MasterObstacleList => masterObstacleListSO.MasterObstacleList;

    [SerializeField] private Cursor cursor;
    public void SpawnSelection(Vector3 spawnPosition)
    {
        if (selectedSpawned)
        {
            return;
        }
        Position = spawnPosition;
        GameObject selectionObj = Instantiate(SelectionPrefab);
        selectionObj.transform.position = spawnPosition;
        Selection selectionScript = selectionObj.GetComponent<Selection>();
        selectionScript.cursor = cursor;
        selectionScript.position = spawnPosition;
        selectedSpawned = true;
        activeSelection = selectionObj; // Store the active selection for later use
        //selectionScript.SetSelection(); // Call this after assignment
    }
    //public void GetObsticle(Vector2 PlaceHere)
    //{
    //    //Place this inside of this statement inside of a different function, prefeable in oblist
    //    // Only check for selectable objects using the layer mask
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, selectableLayer);
    //    Debug.Log("Clicked on:" + hit.collider?.name);
    //    if (hit.collider != null)
    //    {
    //        var selectable = hit.collider.GetComponent<SelectableObject>();
    //        if (selectable != null)
    //        {
    //            // Make the selectable spawn at the selection position
    //            PlaceObsticle(Position, selectable.SetNum);
    //            DeleteSelection();
    //            return; // Don't process map click
    //        }
    //    }
    //    if (ClickMap)
    //    {
    //        Collider2D hitSidewalk = Physics2D.OverlapPoint(PlaceHere, sidewalk);
    //        Collider2D hitGrass = Physics2D.OverlapPoint(PlaceHere, grass);
    //        Collider2D hitEntrance = Physics2D.OverlapPoint(PlaceHere, entrances);
    //        selectedPosition = PlaceHere;
    //        if (hitSidewalk != null)
    //            Debug.Log("Tile Type: Sidewalk");
    //        if (hitGrass != null)
    //            Debug.Log("Tile Type: Grass");
    //        if (hitEntrance != null)
    //            Debug.Log("Tile Type: Entrance");

    //        ani.SetTrigger("Click");
    //        if (!SetUpObsticles)
    //        {
    //            if (hitEntrance == null)
    //            {
    //                NetworkLogger.Instance.AddLog("Please click on an entrance.");
    //            }
    //            else
    //            {
    //                SpawnSelection(PlaceHere);
    //                Signals(ObList.ObsticalType.Entrances);
    //                ClickMap = false;

    //            }
    //        }
    //        else
    //        {
    //            Collider2D hitStart = Physics2D.OverlapCircle(PlaceHere, 0.1f, obsticles);
    //            if (hitStart != null)
    //            {
    //                if (hitStart.gameObject.tag == "Unmovable")
    //                {
    //                    NetworkLogger.Instance.AddLog("Please do not place obsticles over entrances and exits");

    //                }
    //            }
    //            else
    //            {
    //                SpawnSelection(PlaceHere);
    //                if (hitSidewalk != null)
    //                {
    //                    Signals(ObList.ObsticalType.Sidewalk);
    //                }
    //                else if (hitGrass != null)
    //                {
    //                    Signals(ObList.ObsticalType.Grass);
    //                }
    //            }
    //        }
    //    }
    //}

    //public void getTile()
    //{

    //}
    //public void PlaceObsticle(Vector3 spawnPosition, int selNum)
    //{
    //    Debug.Log("Selecting Object: " + selNum);
    //    ClickMap = true;
    //    PlaceObjectServerRpc(spawnPosition, selNum);
    //}

    //[ServerRpc]
    //private void PlaceObjectServerRpc(Vector3 spawnPosition, int selNum, ServerRpcParams rpcParams = default)
    //{
    //    GameObject selectionPrefab = null;
    //    for (int i = 0; i < MasterObstacleList.Count; i++)
    //    {

    //        if (selNum == i)
    //        {
    //            selectionPrefab = MasterObstacleList[i];
    //        }
    //    }
    //    // Check for coin at the position
    //    Collider2D hit = Physics2D.OverlapCircle(spawnPosition, 0.1f, obsticles);
    //    Debug.Log(hit);
    //    if (hit != null)
    //    {
    //        OwnerOnlyVisibility coin = hit.GetComponent<OwnerOnlyVisibility>();
    //        if (coin != null && coin.visibleToClientId == NetworkManager.Singleton.ConnectedClients[OwnerClientId].ClientId)
    //        {


    //            coin.NetworkObject.Despawn();
    //        }
    //    }
    //    if (selNum < 0 || selNum >= currentSelection.Count)
    //    {
    //        return;
    //    }
    //    GameObject placedObject = Instantiate(selectionPrefab, spawnPosition, Quaternion.identity);
    //    Debug.Log("Placing Object: " + placedObject.name);
    //    OwnerOnlyVisibility visibleComponent = placedObject.GetComponent<OwnerOnlyVisibility>();
    //    visibleComponent.visibleToClientId = rpcParams.Receive.SenderClientId;

    //    NetworkObject netObj = placedObject.GetComponent<NetworkObject>();
    //    netObj.CheckObjectVisibility = visibleComponent.CheckVisibility;

    //    netObj.Spawn();
    //}

    public void DeleteSelection()
    {
        if (activeSelection != null)
        {
            Destroy(activeSelection);
            activeSelection = null; // Clear the reference after deletion
            selectedSpawned = false;
        }
        else
        {
            Debug.Log("No active selection to delete.");
        }
    }
    public void Signals(ObsticalType type)
    {
        foreach(var list in MasterList)
        {
            if (list.Type == type)
            {
                if (activeSelection != null)
                {
                    activeSelection.GetComponent<Selection>().AddList(list.Obsticles);
                }
                else
                {
                    Debug.Log("No active selection to set obstacles for.");
                }
                return; // Exit after finding the first matching type
            }
        }
    }
    public void AddObs(List<GameObject> obList,Vector3 spawnPosition)
    {
     if (activeSelection == null)
        {
            Debug.Log("No active selection to add obstacles to.");
        }
        activeSelection.GetComponent<Selection>().AddList(obList);
    }
}
