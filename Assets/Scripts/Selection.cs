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
            cursor.SetSelection(obstaclePrefabs);
        else
            Debug.LogError("Selection: cursor not assigned before SetSelection call.");
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
