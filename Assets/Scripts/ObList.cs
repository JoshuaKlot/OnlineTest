using UnityEngine;
using System.Collections.Generic;

public class ObList : MonoBehaviour
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
