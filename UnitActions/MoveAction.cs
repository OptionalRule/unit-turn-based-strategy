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
    // private List<GridPosition> validGridPositionCache = new List<GridPosition>();

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
        // pathCache.Clear();
        // validGridPositionCache.Clear();
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
        return GetValidActionGridPositionList(unit.GetGridPosition());
    }

    /*
     ** Returns a list of valid grid positions that the unit can move to.
     ** This method is used by the EnemyAI to determine the best move action
     ** to use around a specific target or goal position.
     **/
    public List<GridPosition> GetValidActionGridPositionList(GridPosition centerPosition)
    {
        List<GridPosition> validGridPositions = new List<GridPosition>();
        List<GridPosition> allPossiblePositions = LevelGrid.Instance.GetGridPositionsWithinRange(centerPosition, maxGridMoveDistance);

        foreach (GridPosition gridPosition in allPossiblePositions)
        {
            //if (pathCache.ContainsKey(gridPosition))
            //{
            //    validGridPositions.Add(gridPosition);
            //    continue;
            //}

            if (!IsValidMovePosition(gridPosition)) continue;

            List<GridPosition> pathToPosition = Pathfinding.Instance.FindPath(centerPosition, gridPosition, out int pathLength);

            if (pathLength == 0 || pathLength > maxGridMoveDistance * 10) continue;

            // pathCache.Add(gridPosition, pathToPosition);
            validGridPositions.Add(gridPosition);
        }

        // validGridPositionCache.AddRange(validGridPositions);
        return validGridPositions;
    }

    public bool IsValidMovePosition(GridPosition targetGridPosition)
    {
        bool result =   !LevelGrid.Instance.IsValidGridPosition(targetGridPosition) ||
                        LevelGrid.Instance.HasAnyUnitOnGridPosition(targetGridPosition) ||
                        !Pathfinding.Instance.IsWalkable(targetGridPosition);
        return !result;
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


        /* Refactor all of this.  All I need to do is.
         * 1. Find the nearest player by path.
         * 2. Return a list of valid grid positions within range of the player and their action value.
         * 3. Return all path options for a choosen location.
         * 4. Pick the open position within move range with the lowest F value and Lowest G value.
         */
        if (bestEnemyAIAction.actionValue == 0)
        {

            bestEnemyAIAction = GetBestEnemyAIMoveWhenNoTarget();
        }
        return bestEnemyAIAction;
    }

    private EnemyAIAction GetBestEnemyAIMoveWhenNoTarget()
    {
        Unit closestPlayer = FindClosestPlayerByPath(out List<GridPosition> path);
        if (closestPlayer == null)
        {
            return null;
        }

        return new EnemyAIAction
        {
            gridPosition = FindDistantPointOnPath(path, maxWorldMoveDistance),
            actionValue = 90
        };
    }

    public override EnemyAIAction GetEnemyAIActionValueForPosition(GridPosition gridPosition)
    { // Abstract Class from Parent.  This is the method that is called by the parent class.
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = CalculateTacticalActionValue(gridPosition)
        };
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
            float pointDistance = LevelGrid.Instance.GetWorldDistanceBetween(previousPoint, currentPoint);

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

    private Unit FindClosestPlayerByPath(out List<GridPosition> path)
    {
        List<Unit> playerUnits = UnitManager.Instance.GetPlayerUnits();
        Unit nearestPlayer = null;
        float shortestPathLength = Mathf.Infinity;
        List<GridPosition> pathToNearestPlayer = null;

        foreach (Unit playerUnit in playerUnits)
        {
            List<GridPosition> _path = Pathfinding.Instance.FindPath(unit.GetGridPosition(), playerUnit.GetGridPosition(), out int pathLength);
            if (pathLength == 0) { continue; } // No path found

            if (pathLength < shortestPathLength)
            {
                shortestPathLength = pathLength;
                nearestPlayer = playerUnit;
                pathToNearestPlayer = _path;
            }
        }
        path = pathToNearestPlayer;
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