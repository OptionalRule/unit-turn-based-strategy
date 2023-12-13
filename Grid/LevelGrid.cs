using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/* 
 * Setup the LevelGrid after TurnSystem in Script Execution Order.
 */

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float gridCellSize = 2f;
    public LayerMask ObstacleLayerMask;

    private GridSystem<GridSquare> gridSystem;
    private void OnValidate()
    {
        if(gridDebugObjectPrefab == null)
        {
            Debug.LogError("GridDebugObjectPrefab is not set in the inspector!");
        }
    }

    private void OnEnable()
    {
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    private void OnDisable()
    {
        Unit.OnAnyUnitDead -= Unit_OnAnyUnitDead;
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
        
    }

    private void Start()
    {
        Pathfinding.Instance.SetUp(gridWidth, gridHeight, gridCellSize);
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

    public void MoveUnitToGridPosition(GridPosition oldGridPosition, GridPosition newGridPosition, Unit unit)
    {
        RemoveUnitFromGrid(oldGridPosition, unit);
        AddUnitToGrid(newGridPosition, unit);
    }

    //Passthroughs for encapsulation
    public GridPosition WorldPositionToGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);

    public Vector3 GridPositionToWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);

    public bool IsValidGridPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);

    public float GetWorldDistanceBetween(GridPosition gridPositionA, GridPosition gridPositionB) => gridSystem.GetWorldDistanceBetween(gridPositionA, gridPositionB);

    public int GetGridDistanceBetween(GridPosition gridPositionA, GridPosition gridPositionB) => gridSystem.GetGridDistanceBetween(gridPositionA, gridPositionB);

    public int GetWidth() => gridSystem.GetWidth();

    public int GetHeight() => gridSystem.GetHeight();
    public float GetCellSize() => gridSystem.GetCellSize(); 

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

    public List<GridPosition> GetGridPositionsWithinRange(GridPosition centerPosition, int gridDistance)
    {
        List<GridPosition> gridPositionsWithinRange = new List<GridPosition>();
        for (int x = -gridDistance; x <= gridDistance; x++)
        {
            for (int z = -gridDistance; z <= gridDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition targetGridPosition = offsetGridPosition + centerPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(targetGridPosition)) continue;
                if (LevelGrid.Instance.GetGridDistanceBetween(centerPosition, targetGridPosition) > gridDistance) continue;

                gridPositionsWithinRange.Add(targetGridPosition);
            }
        }
        return gridPositionsWithinRange;
    }

    private void OnDrawGizmos()
    {
        // Draw the outer perimeter of the grid
        Gizmos.color = Color.white;

        // Draw the four sides of the rectangle representing the outer grid
        Vector3 bottomLeft = GetWorldPosition(0, 0);
        Vector3 bottomRight = GetWorldPosition(gridWidth, 0);
        Vector3 topLeft = GetWorldPosition(0, gridHeight);
        Vector3 topRight = GetWorldPosition(gridWidth, gridHeight);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * gridCellSize;
    }

    private void Unit_OnAnyUnitDead(object sender, System.EventArgs e)
    {
        Unit unit = sender as Unit;
        GridPosition gridPosition = unit.GetGridPosition();
        if (gridPosition != null)
        {
            RemoveUnitFromGrid(gridPosition, unit);
        }
    }
}
