using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GridManager : NetworkBehaviour
{
    public static GridManager Instance;

    [SerializeField] private float gridCellSize = 1f;

    private HashSet<Vector2Int> occupiedCells = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Vector3 GetSnappedPosition(Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGrid(worldPosition);
        return GridToWorldCenter(gridPos);
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / gridCellSize),
            Mathf.FloorToInt(worldPosition.y / gridCellSize)
        );
    }

    public Vector3 GridToWorldCenter(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * gridCellSize + gridCellSize / 2f,
            gridPos.y * gridCellSize + gridCellSize / 2f,
            0
        );
    }

    public bool IsOccupied(Vector2Int gridPos)
    {
        return occupiedCells.Contains(gridPos);
    }

    public void MarkOccupied(Vector2Int gridPos)
    {
        occupiedCells.Add(gridPos);
    }
}
