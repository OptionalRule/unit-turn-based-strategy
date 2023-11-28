using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShootAction;

/*
 * * This uses the code from the Spin action for now and serves as a stub for the Dodge action.
 * * When implemented, this action should add a dodge condition to the unit that lasts for one turn.
 * * The dodge condition provides a chance to avoid damage from an attack.
 * */

public class DodgeAction : BaseAction
{
    public event EventHandler<EventArgs> OnDodgeActionStart;
    //public event EventHandler<EventArgs> OnDodgeActionEnd;

    public void Update()
    {
    }

    public void Dodge()
    {
        if (!unit.HasCondition(UnitCondition.Dodging))
        {
            unit.AddCondition(UnitCondition.Dodging);
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
        OnDodgeActionStart?.Invoke(this, EventArgs.Empty);
        ActionComplete();
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
        int _actionValue = 95;
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
