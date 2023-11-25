using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * * This uses the code from the Spin action for now and serves as a stub for the Dodge action.
 * * When implemented, this action should add a dodge condition to the unit that lasts for one turn.
 * * The dodge condition provides a chance to avoid damage from an attack.
 * */

public class DodgeAction : BaseAction
{
    private const float SPIN_SPEED = 360f;

    // Some stopping variables.
    private float totalSpinAmount;
    private Vector3 startAngle;

    public void Update()
    {
        if (IsActive)
        {
            Spin();
        }
    }

    public void Dodge()
    {
        if (!unit.HasCondition(UnitCondition.Dodging))
        {
            unit.AddCondition(UnitCondition.Dodging);
        }
    }

    public void Spin()
    {
        float spinAmount = SPIN_SPEED * Time.deltaTime;
        totalSpinAmount += spinAmount;
        transform.eulerAngles += new Vector3(0, spinAmount, 0);
        if (totalSpinAmount >= 360)
        {
            ActionComplete();

            totalSpinAmount = 0f;
            transform.eulerAngles = startAngle; // corrects for small drift from angle update.
        }
    }

    public override bool CanTakeAction(GridPosition gridPosition)
    {
        return unit.GetGridPosition() == gridPosition;
    }

    public override void TakeAction(GridPosition gridPosition, Action callback)
    {
        Dodge();
        ActionStart(callback);
        startAngle = transform.eulerAngles;
    }

    public override string Label()
    {
        return "Dodge";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        return new List<GridPosition> { unit.GetGridPosition() };
    }

    public override EnemyAIAction GetEnemyAIActionValueForPosition(GridPosition gridPosition)
    {
        int _actionValue = 90;
        if (unit.HasCondition(UnitCondition.Dodging))
        {
            _actionValue = 0;
        }

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = _actionValue
        };
    }
}
