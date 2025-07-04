using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MasterObstacleList", menuName = "ScriptableObjects/MasterObstacleListSO")]
public class MasterObstacleListSO : ScriptableObject
{
    public List<GameObject> MasterObstacleList;
}