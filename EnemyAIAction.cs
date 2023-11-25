using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIAction : IComparable<EnemyAIAction>
{
    public GridPosition gridPosition;
    public int actionValue;

    public int CompareTo(EnemyAIAction other)
    {
        if (other == null)
        {
            return 1;
        }
        return actionValue.CompareTo(other.actionValue);
    }
}
