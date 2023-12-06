using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * * Setup the Pathfinding after LevelGrid in Script Execution Order.
 * */

public class Pathfinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static Pathfinding Instance { get; private set; }
    
    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstacleLayerMask;
    
    private int width;
    private int height;
    private float cellSize;
    private GridSystem<PathNode> gridSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.LogError("Singleton Error! Pathfinding already exists!");
            Destroy(gameObject);
            return;
        }
    }

    public void SetUp(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        gridSystem = new GridSystem<PathNode>(width, height, cellSize,
                       (GridSystem<PathNode> gameObject, GridPosition gridPosition) => new PathNode(gridPosition));
        gridSystem.CreateDebugObjects(gridDebugObjectPrefab, this.transform);
        SetNonWalkableGridPositions();
    }

    public void SetNonWalkableGridPositions()
    {
        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int z = 0; z < gridSystem.GetHeight(); z++)
            {
                bool isBlocked = DetectObstacle(new GridPosition(x, z));
                GetNode(x, z).IsWalkable = !isBlocked;
            }
        }
    }

    public bool DetectObstacle(GridPosition gridPosition)
    {
        float halfCellSize = (cellSize / 2f) * 0.95f;
        Vector3 boxSize = new Vector3(halfCellSize, halfCellSize, halfCellSize); // Define the box size

        float boxcastOffset = 5f;
        Vector3 offset = Vector3.down * boxcastOffset;
        Vector3 worldPosition = gridSystem.GetWorldPosition(gridPosition) + offset;

        if (Physics.BoxCast(worldPosition, boxSize / 2, Vector3.up, out RaycastHit hit, Quaternion.identity, boxcastOffset * 3f, obstacleLayerMask))
        {
            return true;
        }

        return false;
    }

    public bool TryGetPath(GridPosition startGridPosition, GridPosition targetGridPosition, out List<GridPosition> path, out int pathLength)
    {
        if (IsWalkable(startGridPosition) == false || IsWalkable(targetGridPosition) == false)
        {
            path = null;
            pathLength = 0;
            return false;
        }

        path = FindPath(startGridPosition, targetGridPosition, out pathLength);
        if (path == null)
        {
            return false;
        }
        return true;
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition targetGridPosition, out int pathLength)
    {
        return FindPath(startGridPosition, targetGridPosition, out pathLength, out List<PathNode> allPositions);
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition targetGridPosition, out int pathLength, out List<PathNode> allPositions)
    {
        ResetPathNodes();
        
        List<PathNode> openSet = new List<PathNode>();
        List<PathNode> closedSet = new List<PathNode>();

        PathNode startPathNode = gridSystem.GetGridObject(startGridPosition);
        PathNode targetPathNode = gridSystem.GetGridObject(targetGridPosition);

        startPathNode.SetGCost(0);
        startPathNode.SetHCost(CalculateDistanceCost(startGridPosition, targetGridPosition));
        startPathNode.CalculateFCost();

        openSet.Add(startPathNode);

        while (openSet.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openSet);

            if (currentNode == targetPathNode)
            {
                pathLength = targetPathNode.GetFCost();
                allPositions = closedSet;
                return CalculatePath(targetPathNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighborList(currentNode))
            {
                if (closedSet.Contains(neighbourNode)) continue;

                if (!neighbourNode.IsWalkable)
                {
                    closedSet.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.GetGCost() + CalculateDistanceCost(currentNode.GetGridPosition(), neighbourNode.GetGridPosition());
                if (tentativeGCost < neighbourNode.GetGCost())
                {
                    neighbourNode.SetPreviousPathNode(currentNode);
                    neighbourNode.SetGCost(tentativeGCost);
                    neighbourNode.SetHCost(CalculateDistanceCost(neighbourNode.GetGridPosition(), targetPathNode.GetGridPosition()));
                    neighbourNode.CalculateFCost();

                    if (!openSet.Contains(neighbourNode))
                    {
                        openSet.Add(neighbourNode);
                    }
                }
            }
        }

        // Out of nodes on the openList
        Debug.Log("Path not found");
        pathLength = 0;
        allPositions = closedSet;
        return null;
    }

    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }

    private void ResetPathNodes()
    {
        // Reset all nodes
        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int z = 0; z < gridSystem.GetHeight(); z++)
            {
                PathNode pathNode = gridSystem.GetGridObject(new GridPosition(x, z));
                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(int.MaxValue);
                pathNode.CalculateFCost();
                pathNode.ResetPreviousPathNode();
            }
        }
    }

    private List<PathNode> GetNeighborList(PathNode centerNode)  // TODO move this to GridSystem and use Generics.
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition targetGridPosition = centerNode.GetGridPosition();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if(gridSystem.IsValidGridPosition(targetGridPosition.X + x, targetGridPosition.Z + z) == false) { continue; }

                PathNode neighborNode = GetNode(targetGridPosition.X + x, targetGridPosition.Z + z);
                if (neighborNode == null) { continue; }
                if (neighborNode == centerNode)  { continue; }
                if (neighborNode.IsWalkable == false) { continue; }

                neighbourList.Add(neighborNode);
            }
        }
        return neighbourList;
    }

    private List<GridPosition> CalculatePath(PathNode endPathNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endPathNode);
        PathNode currentNode = endPathNode;
        while (currentNode.GetPreviousPathNode() != null)
        {
            path.Add(currentNode.GetPreviousPathNode());
            currentNode = currentNode.GetPreviousPathNode();
        }
        path.Reverse();
        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach (PathNode pathNode in path)
        {
            gridPositionList.Add(pathNode.GetGridPosition());
        }
        return gridPositionList;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GetFCost() < lowestFCostNode.GetFCost())
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    public int CalculateDistanceCost(GridPosition a, GridPosition b)
    {
        int dstX = Mathf.Abs(a.X - b.X);
        int dstZ = Mathf.Abs(a.Z - b.Z);

        if (dstX > dstZ)
            return MOVE_DIAGONAL_COST * dstZ + MOVE_STRAIGHT_COST * (dstX - dstZ);
        return MOVE_DIAGONAL_COST * dstX + MOVE_STRAIGHT_COST * (dstZ - dstX);
    }

    public bool IsWalkable(GridPosition gridPosition)
    {
        return gridSystem.GetGridObject(gridPosition).IsWalkable;
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition targetGridPosition)
    {
        FindPath(startGridPosition, targetGridPosition, out int pathLength);
        return pathLength;
    }
}
