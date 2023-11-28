using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

[RequireComponent(typeof(Unit))]
public class MoveAction : BaseAction
{
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

    // Update is called once per frame
    void Update()
    {
        if (IsActive)
        {
            Move();
        }
    }

    private void Move()
    {
        Vector3 moveDirection = (targetMovePosition - transform.position).normalized;
        transform.position += moveDirection * Time.deltaTime * this.moveSpeed;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
        float distancToMoveTarget = Vector3.Distance(transform.position, targetMovePosition);
        if (distancToMoveTarget <= stopDistance)
        {
            StopMoving();
            ActionComplete();
        }
        UpdateGridPosition();
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
        targetMovePosition = LevelGrid.Instance.GridPositionToWorldPosition(gridPosition);
        StartMoving();
    }

    private void StartMoving()
    {
        OnMoveActionStart?.Invoke(this, EventArgs.Empty);
    }

    private void StopMoving()
    {
        OnMoveActionStop?.Invoke(this, EventArgs.Empty);
    }

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
                validGridPositions.Add(targetGridPosition);
            }
        }
        return validGridPositions;
    }

    public void UpdateGridPosition()
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
        if (bestEnemyAIAction.actionValue <= 0)
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
        int targetCountAtPosition = unit.GetShootAction().GetTargetCountAtGridPosition(gridPosition);
        if (targetCountAtPosition > 0)
        {
            _actionValue = Mathf.Clamp(100 - (6 * targetCountAtPosition), 0, 100);
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

        // 2. Find valid grid positions
        List<GridPosition> validPositions = GetValidActionGridPositionList();
        GridPosition closestPosition = unit.GetGridPosition();
        float closestDistance = Mathf.Infinity;

        // 3. Calculate the closest position
        foreach (GridPosition position in validPositions)
        {
            Vector3 gridWorldPosition = LevelGrid.Instance.GridPositionToWorldPosition(position);
            float distance = Vector3.Distance(targetUnitPosition, gridWorldPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPosition = position;
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
