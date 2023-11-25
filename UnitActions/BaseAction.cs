using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    protected Unit unit;
    protected bool IsActive = false;
    protected int actionPointCost = 1;
    protected Color actionColor = Color.white;

    public Action OnActionCompleted;

    protected virtual void Awake()
    {
        if(!TryGetComponent<Unit>(out unit))
        {
            Debug.LogError("Action is unable to get the unit component!");
        }
    }

    protected void ActionStart(Action callback)
    {
        IsActive = true;
        OnActionCompleted = callback;
    }

    protected void ActionComplete()
    {
        IsActive = false;
        OnActionCompleted?.Invoke();
        OnActionCompleted = null;
    }

    public abstract string Label();
    
    public abstract bool CanTakeAction(GridPosition gridPosition);

    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList().Contains(gridPosition);
    }

    public abstract List<GridPosition> GetValidActionGridPositionList();

    public virtual int GetActionPointCost()
    {
        return actionPointCost;
    }

    public override string ToString()
    {
        return Label();
    }

    public virtual Color GetActionColor()
    {
        return actionColor;
    }

    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActions = new List<EnemyAIAction>();
        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();
        foreach (GridPosition gridPosition in validActionGridPositionList)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPosition);
            //enemyAIAction.gridPosition = gridPosition;
            //enemyAIAction.actionValue = GetActionValue(gridPosition);
            enemyAIActions.Add(enemyAIAction);
        }

        if (enemyAIActions.Count == 0)
        {
            return null;
        }

        enemyAIActions.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue.CompareTo(a.actionValue));
        return enemyAIActions[0];
    }

    public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);
}
