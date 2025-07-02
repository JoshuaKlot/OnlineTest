using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    [SerializeField] private List<GameObject> obstaclePrefabs;
    public Cursor cursor;
    public Vector3 position;

    // Call this after assigning cursor
    public void SetSelection()
    {
        if (cursor != null)
        {
            cursor.SetSelection(obstaclePrefabs);

        //    // Find indices of selected prefabs in the master list
        //    List<int> indices = new List<int>();
        //    foreach (var prefab in obstaclePrefabs)
        //    {
        //        int idx = cursor.MasterObstacleList.IndexOf(prefab);
        //        if (idx >= 0) indices.Add(idx);
        //    }
        //    cursor.SetSelectionServerRpc(indices.ToArray());
        }
        else
        {
            Debug.LogError("Selection: cursor not assigned before SetSelection call.");
        }
    }

    public void SetObject(int SetNum)
    {
        if (cursor != null)
            cursor.PlaceObsticle(position, SetNum);
        else
            Debug.LogError("Selection: cursor not assigned before SetObject call.");
        Destroy(gameObject);
    }
}
