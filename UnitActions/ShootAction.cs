using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using OptionalRule.Utility;

public class ShootAction : BaseAction
{
    private enum State
    {
        Idle,
        Aiming,
        Shooting,
        Reloading
    }

    private State state;
    private float stateTimer = 0.5f;
    private float stepTimer;
    private float rotationSpeed = 10f;
    private int maxShootDistance = 9;
    private Unit targetUnit;

    public event EventHandler<OnShootEventArgs> OnShootActionStart;

    public static event EventHandler<OnShootEventArgs> OnAnyShootActionStart;
    public static event EventHandler<EventArgs> OnAnyShootActionEnd;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }

    private void Start()
    {
        actionColor = Color.red;
    }

    private void Update()
    {
        if (!IsActive) { return; }

        stepTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                UpdateAim();
                break;
            case State.Shooting:
                if (stepTimer <= 0f)
                {
                    Shoot();
                    state = State.Reloading;
                    ResetStepTimer();
                }
                break;
            case State.Reloading:
                if (stepTimer <= 0f)
                {
                    ActionComplete();
                    state = State.Idle;
                    OnAnyShootActionEnd?.Invoke(this, EventArgs.Empty);
                }
                break;
        }
    }

    private void UpdateAim()
    {
        // Determine which direction to rotate towards
        Vector3 targetDirection = targetUnit.transform.position - transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = rotationSpeed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);

        // Check if the GameObject is facing the target direction within a small threshold.
        if (Vector3.Angle(transform.forward, targetDirection) < 1f)
        {
            // If yes, then the aiming is done, and we should start shooting.
            OnAnyShootActionStart?.Invoke(this, new OnShootEventArgs
            {
                targetUnit = targetUnit,
                shootingUnit = unit
            });
            state = State.Shooting;
            ResetStepTimer(stateTimer * 1.5f); // Assuming you also want to reset the timer for the shooting phase.
        }
    }

    private void ResetStepTimer(float seconds)
    {
        stepTimer = seconds;
    }

    private void ResetStepTimer()
    {
        ResetStepTimer(stateTimer);
    }

    public override bool CanTakeAction(GridPosition gridPosition)
    {
        return IsValidActionGridPosition(gridPosition);
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition fromGridPosition)
    {
        List<GridPosition> validGridPositions = new List<GridPosition>();
        foreach (Unit targetUnit in UnitManager.Instance.GetEnemyUnitsOf(unit))
        {
            GridPosition targetGridPosition = targetUnit.GetGridPosition();
            if (targetGridPosition == null) { continue; }
            if (LevelGrid.Instance.GetDistanceBetween(fromGridPosition, targetGridPosition) <= maxShootDistance)
            {
                validGridPositions.Add(targetGridPosition);
            }
        }
        return validGridPositions;
    }

    public override string Label()
    {
        return "Shoot";
    }

    public override void TakeAction(GridPosition gridPosition, Action callback)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        if (!targetUnit || !targetUnit.IsEnemyOf(unit))
        {
            Debug.LogError("ShootAction: Invalid target unit");
            return;
        }
        ActionStart(callback);

        state = State.Aiming;
        ResetStepTimer();
    }

    private void Shoot()
    {
        OnShootActionStart?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit
        });
        int damage = GameDice.Instance.Roll(7, 10);
        targetUnit.ApplyDamage(damage, transform.position);
    }

    public override EnemyAIAction GetEnemyAIActionValueForPosition(GridPosition gridPosition)
    {
        int _actionValue = 100;

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = _actionValue
        };
    }

    public int GetTargetCountAtGridPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }
}
