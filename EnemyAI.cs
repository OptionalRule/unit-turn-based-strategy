using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    public enum State
    {
        WaitingForEnemyTurn,
        StartingEnemyTurn,
        SelectingActiveUnit,
        SelectingAction,
        TakingUnitAction,
        Busy,
        EndingTurn
    }

    private State currentState;
    private Unit activeUnit;
    // private BaseAction selectedAction;
    private EnemyAIAction selectedEnemyAIAction;
    private Queue<Unit> enemyUnits;

    private float timer;

    private void Awake()
    {
        currentState = State.WaitingForEnemyTurn;
    }

    void Start()
    {
        TurnSystem.Instance.OnNextTurn += TurnSystem_OnTurnChanged;
    }

    private void OnDestroy()
    {
        TurnSystem.Instance.OnNextTurn -= TurnSystem_OnTurnChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }
        
        switch (currentState)
        {
            case State.WaitingForEnemyTurn:
                break;
            case State.StartingEnemyTurn:
                StartEnemyTurn();
                break;
            case State.SelectingActiveUnit:
                SelectActiveUnit();
                break;
            case State.SelectingAction:
                SelectAction();
                break;
            case State.TakingUnitAction:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    currentState = State.Busy;
                    HandleSelectedAction();
                }
                break;
            case State.Busy:
                break;
            case State.EndingTurn:
                TurnSystem.Instance.NextTurn();
                break;
            default:
                break;
        }
    }

    private void StartEnemyTurn()
    {
        enemyUnits = new Queue<Unit>(UnitManager.Instance.GetEnemyUnits());
        if (enemyUnits.Count > 0)
        {
               currentState = State.SelectingActiveUnit;
        } else
        {
            TurnSystem.Instance.NextTurn(); // Waiting state set in TurnSystem_OnTurnChanged
        }
    }

    private void SelectActiveUnit()
    {
        ClearEnemyAI();
        if (activeUnit != null && activeUnit.GetActionPoints() > 0)
        {
            currentState = State.SelectingAction;
            return;
        }
        
        if (enemyUnits.Count > 0)
        {
            ChangeActiveUnit(enemyUnits.Dequeue());
        } else
        {
            TurnSystem.Instance.NextTurn(); // Waiting state set in TurnSystem_OnTurnChanged
        }
    }

    private void SelectAction()
    {
        if (activeUnit.GetActionPoints() <= 0)
        {
            currentState = State.SelectingActiveUnit;
            return;
        }

        List<EnemyAIAction> bestEnemyAIActions = GetBestEnemyAIActionOptions(activeUnit);

        if(bestEnemyAIActions.Count > 0)
        {
            bestEnemyAIActions.Sort((a, b) => b.actionValue.CompareTo(a.actionValue));
            selectedEnemyAIAction = bestEnemyAIActions[0];
            //selectedAction = selectedEnemyAIAction.baseAction;
            timer = 1f;
            currentState = State.TakingUnitAction;
        } else
        {
            activeUnit.SpendActionPoints(activeUnit.GetActionPoints());
            currentState = State.SelectingActiveUnit;
        }
    }

    private List<EnemyAIAction> GetBestEnemyAIActionOptions(Unit actingUnit) { 
        List<EnemyAIAction> bestEnemyAIActions = new List<EnemyAIAction>();
           foreach (BaseAction baseAction in actingUnit.GetBaseActions())
        {
            if (!actingUnit.CanSpendActionPointsToTakeAction(baseAction))
            {
                continue;
            }

            EnemyAIAction possibleEnemyAIAction = baseAction.GetBestEnemyAIAction();
            if (possibleEnemyAIAction != null)
            {
                possibleEnemyAIAction.action = baseAction;
                bestEnemyAIActions.Add(possibleEnemyAIAction);
            }
        }
        return bestEnemyAIActions;
    }

    private void HandleSelectedAction()
    {
        if (selectedEnemyAIAction == null)
        {
            Debug.LogError("No action selected!");
            currentState = State.SelectingAction;
            return;
        }

        if (activeUnit.CanSpendActionPointsToTakeAction(selectedEnemyAIAction.action) && 
            selectedEnemyAIAction.action.CanTakeAction(selectedEnemyAIAction.gridPosition))
        {
            activeUnit.SpendActionPoints(selectedEnemyAIAction.action.GetActionPointCost());
            selectedEnemyAIAction.action.TakeAction(selectedEnemyAIAction.gridPosition, () =>
            {
                currentState = State.SelectingAction;
            });
        } 
    }

    private GridPosition GetRandomPlayerGridPosition()
    {
        List<Unit> playerUnits = UnitManager.Instance.GetPlayerUnits();
        Unit randomPlayerUnit = playerUnits[UnityEngine.Random.Range(0, playerUnits.Count)];
        return randomPlayerUnit.GetGridPosition();
    }

    private void ChangeActiveUnit(Unit unit) {
        activeUnit = unit;
        selectedEnemyAIAction = null;
        currentState = State.SelectingAction;
        Debug.Log("Active unit changed to " + activeUnit.name); 
    }

    private void ClearEnemyAI()
    {
        activeUnit = null;
        selectedEnemyAIAction = null;
    }

    private void TurnSystem_OnTurnChanged(object sender, System.EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            currentState = State.StartingEnemyTurn;
            timer = 1f;
        }
        else
        {
            ClearEnemyAI();
            enemyUnits.Clear();
            currentState = State.WaitingForEnemyTurn;
        }
    }

    public State CurrentState
    {
        get
        {
            return currentState;
        }
    }
}
