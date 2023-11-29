using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float gridCellSize = 2f;

    private GridSystem<GridSquare> gridSystem;
    private void OnValidate()
    {
        if(gridDebugObjectPrefab == null)
        {
            Debug.LogError("GridDebugObjectPrefab is not set in the inspector!");
        }
    }

    private void Awake()
    {
        if(gridDebugObjectPrefab == null)
        {
            Debug.LogError("GridDebugObjectPrefab is not set in the inspector!");
        }
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.LogError("Singleton Error! Level Grid already exists!");
            Destroy(gameObject);
            return;
        }
        Instance.gridSystem = new GridSystem<GridSquare>(gridWidth, gridHeight, gridCellSize, 
            (GridSystem<GridSquare> g, GridPosition gridPosition) => new GridSquare(g, gridPosition));
        Instance.gridSystem.CreateDebugObjects(gridDebugObjectPrefab, this.transform);
    }

    public void AddUnitToGrid(GridPosition gridPosition, Unit unit)
    {
        GridSquare gridSquare = Instance.gridSystem.GetGridObject(gridPosition);
        gridSquare.AddUnit(unit);
    }

    public void RemoveUnitFromGrid(GridPosition gridPosition, Unit unit)
    {
        GridSquare gridSquare = Instance.gridSystem.GetGridObject(gridPosition);
        gridSquare.RemoveUnit(unit);
    }

    //Passthroughs for encapsulation
    public GridPosition WorldPositionToGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);

    public Vector3 GridPositionToWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);

    public bool IsValidGridPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);

    public float GetDistanceBetween(GridPosition gridPositionA, GridPosition gridPositionB) => gridSystem.GetDistanceBetween(gridPositionA, gridPositionB);

    public int GetWidth() => gridSystem.GetWidth();

    public int GetHeight() => gridSystem.GetHeight();

    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridSquare gridSquare = gridSystem.GetGridObject(gridPosition);
        if (gridSquare != null)
        {
            return gridSquare.HasAnyUnit();
        }
        return false;
    }

    public Unit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridSquare gridSquare = gridSystem.GetGridObject(gridPosition);
        return gridSquare.GetUnit();
    }
}
