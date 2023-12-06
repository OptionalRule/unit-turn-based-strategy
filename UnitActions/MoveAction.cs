using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class MoveAction : BaseAction
{
    private Movement movement;
    private int maxGridMoveDistance = 4;
    private float maxWorldMoveDistance;
    private Dictionary<GridPosition, List<GridPosition>> pathCache = new Dictionary<GridPosition, List<GridPosition>>();

    public event EventHandler OnMoveActionStart;
    public event EventHandler OnMoveActionStop;

    [SerializeField] private Animator unitAnimator;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float stopDistance = 0.1f;

    private void Start()
    {
        actionColor = Color.green;
        maxWorldMoveDistance = LevelGrid.Instance.GetCellSize() * maxGridMoveDistance;
        movement = new Movement(unitAnimator, gameObject.transform, moveSpeed, rotateSpeed, stopDistance);
    }

    private void Update()
    {
        if (IsActive)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        if (movement.IsMoving)
        {
            movement.ContinueMove();
        }
        else
        {
            StopMoving();
            ActionComplete();
        }
    }

    public override bool CanTakeAction(GridPosition gridPosition)
    {
        return IsValidActionGridPosition(gridPosition);
    }

    public override void TakeAction(GridPosition gridPosition, Action callback)
    {
        if (!CanTakeAction(gridPosition))
        {
            callback?.Invoke();
            return;
        }
        ActionStart(callback);

        if (Pathfinding.Instance.TryGetPath(unit.GetGridPosition(), gridPosition, out var path, out int pathLength))
        {
            movement.StartMove(path, OnMoveStarted);
        }
        else
        {
            callback?.Invoke();
        }
    }

    private void OnMoveStarted()
    {
        OnMoveActionStart?.Invoke(this, EventArgs.Empty);
        pathCache.Clear();
    }

    private void StopMoving()
    {
        OnMoveActionStop?.Invoke(this, EventArgs.Empty);
        movement.StopMove();
        unit.SetGridPosition(LevelGrid.Instance.WorldPositionToGridPosition(unit.transform.position));
    }

    public override string Label()
    {
        return "Move";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositions = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxGridMoveDistance; x <= maxGridMoveDistance; x++)
        {
            for (int z = -maxGridMoveDistance; z <= maxGridMoveDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition targetGridPosition = offsetGridPosition + unitGridPosition;

                if (pathCache.ContainsKey(targetGridPosition))
                {
                    validGridPositions.Add(targetGridPosition);
                    continue;
                }

                if (!LevelGrid.Instance.IsValidGridPosition(targetGridPosition)) continue;
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(targetGridPosition)) continue;
                if (!Pathfinding.Instance.IsWalkable(targetGridPosition)) continue;

                if (!pathCache.ContainsKey(targetGridPosition))
                {
                    List<GridPosition> pathToPosition = Pathfinding.Instance.FindPath(unitGridPosition, targetGridPosition, out int pathLength);
                    if (pathLength == 0 || pathLength > maxGridMoveDistance * 10) continue;

                    pathCache.Add(targetGridPosition, pathToPosition);
                }

                validGridPositions.Add(targetGridPosition);
            }
        }
        return validGridPositions;
    }

    public Dictionary<GridPosition, List<GridPosition>> PathCache
    {
        get { return pathCache; }
        private set { pathCache = value; }
    }

    public override EnemyAIAction GetBestEnemyAIAction()
    {
        EnemyAIAction bestEnemyAIAction = base.GetBestEnemyAIAction();
        EnemyAIAction currentPositionActionValue = GetEnemyAIActionValueForPosition(unit.GetGridPosition());

        // If the current position ties with highest value then stay where you are.
        if (currentPositionActionValue.actionValue >= bestEnemyAIAction.actionValue)
        {
            bestEnemyAIAction = currentPositionActionValue;
        }

        // If the current position is not the highest value, then find the best position in a path to nearest player.
        if (bestEnemyAIAction.actionValue == 0)
        {
            Unit closestPlayerUnit = FindNearestPlayerByPath();

            GridPosition moveToPosition = FindDistantPositionOnPath(closestPlayerUnit, maxWorldMoveDistance);
            bestEnemyAIAction = new EnemyAIAction
            {
                gridPosition = moveToPosition,
                actionValue = moveToPosition == unit.GetGridPosition() ? 0 : 100
            };
        }
        return bestEnemyAIAction;
    }

    public override EnemyAIAction GetEnemyAIActionValueForPosition(GridPosition gridPosition)
    { // Abstract Class from Parent.  This is the method that is called by the parent class.
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = CalculateTacticalActionValue(gridPosition)
        };
    }

    public GridPosition FindDistantPositionOnPath(Unit targetUnit, float worldDistance)
    {
        List<GridPosition> path = Pathfinding.Instance.FindPath(unit.GetGridPosition(), targetUnit.GetGridPosition(), out int pathLength);
        if (pathLength == 0) { return unit.GetGridPosition(); }

        GridPosition distantPoint = FindDistantPointOnPath(path, worldDistance);
        return distantPoint;
    }

    private GridPosition FindDistantPointOnPath(List<GridPosition> path, float maxWorldDistance)
    {
        // Ensure the path is not empty and the distance is positive
        if (path == null || path.Count == 0 || maxWorldDistance <= 0)
        {
            Debug.LogError("Invalid path or distance");
        }

        GridPosition startPoint = path[0];
        GridPosition previousPoint = startPoint;
        GridPosition endPoint = previousPoint;
        float accumulatedDistance = 0;

        for (int i = 1; i < path.Count; i++)
        {
            GridPosition currentPoint = path[i];
            float pointDistance = LevelGrid.Instance.GetDistanceBetween(previousPoint, currentPoint);

            if (accumulatedDistance + pointDistance > maxWorldDistance)
            {
                break;
            }

            previousPoint = currentPoint;
            accumulatedDistance += pointDistance;

            if (accumulatedDistance == maxWorldDistance) // Handle edge case of exact match.
            {
                endPoint = previousPoint;
                break;
            }
        }

        if (accumulatedDistance < maxWorldDistance)
        {
            endPoint = previousPoint;  // If exact distance not met, use the last point reached
        }

        // Check if endPoint has a Unit and backtrack if necessary
        while (LevelGrid.Instance.HasAnyUnitOnGridPosition(endPoint) && endPoint != startPoint)
        {
            int currentIndex = path.IndexOf(endPoint);
            if (currentIndex > 0)
            {
                endPoint = path[currentIndex - 1];
            }
            else
            {
                // No valid position found, return the start point or handle as needed
                return startPoint;
            }
        }
        return endPoint;
    }

    private Unit FindNearestPlayerByPath()
    {
        List<Unit> playerUnits = UnitManager.Instance.GetPlayerUnits();
        Unit nearestPlayer = null;
        float shortestPath = Mathf.Infinity;

        foreach (Unit playerUnit in playerUnits)
        {
            Pathfinding.Instance.FindPath(unit.GetGridPosition(), playerUnit.GetGridPosition(), out int pathLength);
            if (pathLength == 0) { continue; } // No path found

            if (pathLength < shortestPath)
            {
                shortestPath = pathLength;
                nearestPlayer = playerUnit;
            }
        }

        return nearestPlayer;
    }

    // ENEMY AI ACTION VALUE METHODS

    private int CalculateTacticalActionValue(GridPosition position)
    {
        int actionValue = 0;

        // 1. Targets: Prefer positions with more targets in line of sight
        if (IsInLineOfSightOfEnemy(position, out int targetCount))
        {
            actionValue += 40 + (20 * targetCount); // Increase value for safer positions
            Mathf.Clamp(actionValue, 0, 80);
        } else
        {
            return 0; // No targets, no value.
        }

        // 2. Consider a proximity to target.  Closer targets are more valuable for melee, but less valuable for ranged.

        // 3. Strategic Position: Bonus for higher ground or cover
        if (IsHighGround(position) || ProvidesCover(position))
        {
            actionValue += 20; // Increase value for strategic positions
        }

        // Ensure action value is within a valid range
        return Mathf.Clamp(actionValue, 0, 100);
    }

    private bool IsInLineOfSightOfEnemy(GridPosition gridPosition, out int targetCountAtPosition)
    {
        targetCountAtPosition = unit.GetAction<ShootAction>().GetTargetCountAtGridPosition(gridPosition);
        return targetCountAtPosition > 0;
    }

    private bool IsHighGround(GridPosition position)
    {
        return false; // Implement logic to check if the position is on high ground
    }

    private bool ProvidesCover(GridPosition position)
    {
        return false; // Implement logic to check if the position provides cover
    }
}