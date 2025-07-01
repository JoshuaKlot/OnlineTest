using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{

    [SerializeField] private List<GameObject> obstaclePrefabs;
    public Cursor cursor;
    public Vector3 position;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetObject(int SetNum)
    {
        cursor.PlaceObsticle(position, SetNum);
    }
}
