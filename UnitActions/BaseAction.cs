using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public virtual EnemyAIAction GetBestEnemyAIAction()
    {
        /*
         * This determines the action value for this action for each valid grid position.
         * It returns the grid position with the highest action value.
         * This will be used in another class to compare all best action values to determine the best action.
         */
        List<EnemyAIAction> enemyAIActions = GetHighestActionValueList();

        if (enemyAIActions.Count == 0)
        {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, enemyAIActions.Count);
        return enemyAIActions[randomIndex];
    }

    /*
     * This gets a list of the EnemyAIActions that share the highest actionValue score.
     *     */
    public virtual List<EnemyAIAction> GetHighestActionValueList()
    {
        List<EnemyAIAction> enemyAIActions = new List<EnemyAIAction>();
        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();
        foreach (GridPosition gridPosition in validActionGridPositionList)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIActionValueForPosition(gridPosition);
            enemyAIActions.Add(enemyAIAction);
        }

        if (enemyAIActions.Count == 0)
        {
            return enemyAIActions;
        }

        enemyAIActions.Sort();
        enemyAIActions.Reverse();
        int highestValue = enemyAIActions[0].actionValue;

        // Filter out all actions with this top actionValue
        List<EnemyAIAction> topActions = enemyAIActions.Where(action => action.actionValue == highestValue).ToList();

        // Now topActions contains all EnemyAIAction objects with the highest actionValue
        return topActions;
    }

    public abstract EnemyAIAction GetEnemyAIActionValueForPosition(GridPosition gridPosition);
    /*
     * * This is the value of the action for the enemy AI for a specific grid position.
     * * The higher the value, the more likely the enemy AI will take this action.
     * */
}
