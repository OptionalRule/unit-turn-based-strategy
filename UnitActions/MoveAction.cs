using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class MoveAction : BaseAction
{

    private Queue<GridPosition> pathQueue = new Queue<GridPosition>();

    private Vector3 targetMovePosition;

    private int maxMoveDistance = 7;

    public event EventHandler OnMoveActionStart;
    public event EventHandler OnMoveActionStop;

    [SerializeField] private Animator unitAnimator;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float stopDistance = 0.1f;

    private void Start()
    {
        actionColor = Color.green;
    }

    private void Update()
    {
        if (IsActive)
        {
            if (pathQueue.Count > 0)
            {
                Move();
            }
            else
            {
                StopMoving();
                ActionComplete();
            }
        }
    }
    private void Move()
    {
        if (pathQueue.Count == 0)
        {
            StopMoving();
            ActionComplete();
            return;
        }

        targetMovePosition = LevelGrid.Instance.GridPositionToWorldPosition(pathQueue.Peek());
        Vector3 moveDirection = (targetMovePosition - transform.position).normalized;
        transform.position += moveDirection * Time.deltaTime * this.moveSpeed;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
        float distanceToMoveTarget = Vector3.Distance(transform.position, targetMovePosition);

        if (distanceToMoveTarget <= stopDistance)
        {
            // Reached the current target position, move to the next
            pathQueue.Dequeue();
            UpdateUnitGridPosition();
            if (pathQueue.Count == 0)
            {
                StopMoving();
                ActionComplete();
            }
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
            callback.Invoke();
            return;
        }
        ActionStart(callback);
        List<GridPosition> path = Pathfinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out int pathLength);
        if (path != null && path.Count > 0)
        {
            foreach (var position in path)
            {
                pathQueue.Enqueue(position);
            }
            StartMoving();
        }
        else
        {
            callback?.Invoke();
        }
    }

    private void StartMoving()
    {
        OnMoveActionStart?.Invoke(this, EventArgs.Empty);
    }

    private void StopMoving()
    {
        OnMoveActionStop?.Invoke(this, EventArgs.Empty);
    }

    /* 
     * Returns all of the grid positions within range that do not have a unit on them.
     */
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositions = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition targetGridPosition = offsetGridPosition + unitGridPosition;

                // Check if distance between grid centers is within the max move distance radius.
                float distance = Vector3.Distance(LevelGrid.Instance.GridPositionToWorldPosition(unitGridPosition), LevelGrid.Instance.GridPositionToWorldPosition(targetGridPosition));
                if (distance > maxMoveDistance) { continue; }  // TODO:  Check distance between unit and target transforms instead.

                if (!LevelGrid.Instance.IsValidGridPosition(targetGridPosition)) {  continue; }
                if(LevelGrid.Instance.HasAnyUnitOnGridPosition(targetGridPosition)) { continue; } 
                if(!Pathfinding.Instance.IsWalkable(targetGridPosition)) { continue; }
                int pathLength = Pathfinding.Instance.GetPathLength(unitGridPosition, targetGridPosition);
                if(pathLength == 0 || pathLength > maxMoveDistance * 10) { continue; }
                validGridPositions.Add(targetGridPosition);
            }
        }
        return validGridPositions;
    }

    /* 
     ** Update the units position on the level grid if it has changed.
     **/
    public void UpdateUnitGridPosition()
    {
        GridPosition newGridPosition = LevelGrid.Instance.WorldPositionToGridPosition(this.transform.position);
        if (unit.GetGridPosition() != newGridPosition)
        {
            LevelGrid.Instance.RemoveUnitFromGrid(unit.GetGridPosition(), unit);
            unit.SetGridPosition(newGridPosition);
            LevelGrid.Instance.AddUnitToGrid(unit.GetGridPosition(), unit);
        }
    }

    public override string Label()
    {
        return "Move";
    }

    public override EnemyAIAction GetBestEnemyAIAction()
    {
        EnemyAIAction bestEnemyAIAction = base.GetBestEnemyAIAction();
        EnemyAIAction currentPositionActionValue = GetEnemyAIActionValueForPosition(unit.GetGridPosition());
        if (currentPositionActionValue.actionValue > 0)
        {
            bestEnemyAIAction = currentPositionActionValue;
            bestEnemyAIAction.actionValue = 1;
        }

        if (bestEnemyAIAction.actionValue == 0)
        {
            Unit closestPlayerUnit = FindNearestPlayer();
            GridPosition closestPlayerGridPosition = FindClosestValidPositionToUnit(closestPlayerUnit);
            bestEnemyAIAction = new EnemyAIAction
            {
                gridPosition = closestPlayerGridPosition,
                actionValue = 100
            };
        }
        return bestEnemyAIAction;
    }

    public override EnemyAIAction GetEnemyAIActionValueForPosition(GridPosition gridPosition)
    {
        int _actionValue = 0;
        int targetCountAtPosition = unit.GetAction<ShootAction>().GetTargetCountAtGridPosition(gridPosition);
        if (targetCountAtPosition > 0)
        {
            _actionValue = Mathf.Clamp(100 - (10 * targetCountAtPosition), 0, 100);
        }

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = _actionValue
        };
    }

    public GridPosition FindClosestValidPositionToUnit(Unit targetUnit)
    {
        Vector3 targetUnitPosition = targetUnit.transform.position;

        List<GridPosition> validPositions = GetValidActionGridPositionList();
        GridPosition closestPosition = unit.GetGridPosition();
        float closestDistance = Mathf.Infinity;

        foreach (GridPosition gridPosition in validPositions)
        {
            Vector3 gridWorldPosition = LevelGrid.Instance.GridPositionToWorldPosition(gridPosition);
            float distanceToTargetUnit = Vector3.Distance(targetUnitPosition, gridWorldPosition);

            if (distanceToTargetUnit < closestDistance)
            {
                closestDistance = distanceToTargetUnit;
                closestPosition = gridPosition;
            }
        }

        return closestPosition;
    }


    private Unit FindNearestPlayer()
    {
        // Assuming you have a way to get all enemy units in the game
        List<Unit> playerUnits = UnitManager.Instance.GetPlayerUnits();
        Unit nearestPlayer = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Unit playerUnit in playerUnits)
        {
            float distance = Vector3.Distance(this.transform.position, playerUnit.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPlayer = playerUnit;
            }
        }

        return nearestPlayer;
    }

}
