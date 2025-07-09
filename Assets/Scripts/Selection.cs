using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [SerializeField] private MasterObstacleListSO masterObstacleListSO;
    [SerializeField] private int xSpacing;
    [SerializeField] private GameObject SelecatableObject;
    [SerializeField] private GameObject deleteObject;
    private GameObject delete;
    public Cursor cursor;
    public Vector3 position;

    // Call this after assigning cursor
    public void SetSelection()
    {
        if (cursor != null)
        {
            cursor.SetSelection(obstaclePrefabs);

            // Find indices of selected prefabs in the master list
            List<int> indices = new List<int>();
            foreach (var prefab in obstaclePrefabs)
            {
                int idx = cursor.MasterObstacleList.IndexOf(prefab);
                if (idx >= 0) indices.Add(idx);
            }
            cursor.SetSelectionServerRpc(indices.ToArray());
        }
        else
        {
            Debug.LogError("Selection: cursor not assigned before SetSelection call.");
        }
    }

    public void AddList(List<GameObject> list)
    {
        if(delete != null)
        {
            Destroy(delete);
        }   
        for (int i = 0; i < list.Count; i++)
        {
            GameObject selectedPrefab = list[i];
            Vector2 newPosition = new Vector2(position.x + (i * xSpacing), position.y);
            GameObject newItem = Instantiate(SelecatableObject);
            newItem.transform.parent = this.transform;
            newItem.transform.position = newPosition;
            newItem.GetComponent<SelectableObject>().SetNum= cursor.MasterObstacleList.IndexOf(selectedPrefab);
            newItem.GetComponent<SpriteRenderer>().sprite = selectedPrefab.GetComponent<SpriteRenderer>().sprite;

        }
        Vector2 finalPosition = new Vector2(position.x + (list.Count * xSpacing), position.y);
        GameObject finalItem = Instantiate(deleteObject);
        finalItem.transform.position = finalPosition;
        finalItem.transform.parent = this.transform;
        delete = finalItem;
    }
}
