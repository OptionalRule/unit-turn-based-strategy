using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

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
        List<GridPosition> validGridPositions = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition targetGridPosition = offsetGridPosition + unitGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(targetGridPosition)) { continue; }

                // Check if the target is within the max shoot distance radius
                float distance = Vector3.Distance(LevelGrid.Instance.GridPositionToWorldPosition(unitGridPosition), LevelGrid.Instance.GridPositionToWorldPosition(targetGridPosition));
                if (distance > maxShootDistance) { continue; }  // TODO:  Check distance between unit and target transforms instead.

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(targetGridPosition)) { continue; }
                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(targetGridPosition);
                if (!targetUnit.IsEnemyOf(unit)) { continue; }
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
        if (!targetUnit || !targetUnit.IsEnemy())
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
        targetUnit.ApplyDamage(RollDice(7, 10), transform.position);
    }

    private int RollDice(int numberOfDice, int sides)
    {
        int total = 0;
        for (int i = 0; i < numberOfDice; i++)
        {
            total += UnityEngine.Random.Range(1, sides + 1); // Random.Range is inclusive for min, exclusive for max
        }
        return total;
    }
}
