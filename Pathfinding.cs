using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static Pathfinding Instance { get; private set; }
    
    [SerializeField] private Transform gridDebugObjectPrefab;
    
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
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition targetGridPosition)
    {
        List<PathNode> openSet = new List<PathNode>();
        List<PathNode> closedSet = new List<PathNode>();

        PathNode startPathNode = gridSystem.GetGridObject(startGridPosition);
        PathNode targetPathNode = gridSystem.GetGridObject(targetGridPosition);
        openSet.Add(startPathNode);

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

        startPathNode.SetGCost(0);
        startPathNode.SetHCost(CalculateDistanceCost(startGridPosition, targetGridPosition));
        startPathNode.CalculateFCost();

        while (openSet.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openSet);

            if (currentNode == targetPathNode)
            {
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
        return null;
    }

    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }

    private List<PathNode> GetNeighborList(PathNode currentNode)  // TODO move this to GridSystem and use Generics.
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition targetGridPosition = currentNode.GetGridPosition();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                {
                    // Skip the current cell
                    continue;
                }

                if (gridSystem.IsValidGridPosition(targetGridPosition.X + x, targetGridPosition.Z + z))
                {
                    neighbourList.Add(GetNode(targetGridPosition.X + x, targetGridPosition.Z + z));
                }
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
}
