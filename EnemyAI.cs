using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    private enum State
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
    private BaseAction selectedAction;
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
        selectedAction = null;
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

        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;
        foreach (BaseAction baseAction in activeUnit.GetBaseActions())
        {
            if(!activeUnit.CanSpendActionPointsToTakeAction(baseAction))
            {
                continue;
            }

            if (bestEnemyAIAction == null)
            {
                bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                bestBaseAction = baseAction;
            } else
            {
                EnemyAIAction possibleEnemyAIAction = baseAction.GetBestEnemyAIAction();
                if (possibleEnemyAIAction != null &&
                    possibleEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
                {
                    bestEnemyAIAction = possibleEnemyAIAction;
                    bestBaseAction = baseAction;
                }
            }
        }

        Debug.Log($"Best action for {activeUnit.name} is {bestBaseAction} with value {bestEnemyAIAction.actionValue}");

        if (bestBaseAction != null && activeUnit.CanSpendActionPointsToTakeAction(bestBaseAction))
        {
            selectedAction = bestBaseAction;
            selectedEnemyAIAction = bestEnemyAIAction;
            timer = 1f;
            currentState = State.TakingUnitAction;
        } else
        {
            activeUnit.SpendActionPoints(activeUnit.GetActionPoints());
            currentState = State.SelectingActiveUnit;
        }
    }

    private void HandleSelectedAction()
    {
        if (selectedAction == null)
        {
            Debug.LogError("No action selected!");
            currentState = State.SelectingAction;
            return;
        }
        GridPosition gridPosition = selectedEnemyAIAction.gridPosition;
        if (activeUnit.CanSpendActionPointsToTakeAction(selectedAction) && 
            selectedAction.CanTakeAction(gridPosition))
        {
            activeUnit.SpendActionPoints(selectedAction.GetActionPointCost());
            selectedAction.TakeAction(gridPosition, () =>
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
        selectedAction = null;
        currentState = State.SelectingAction;
        Debug.Log("Active unit changed to " + activeUnit.name); 
    }

    private void ClearEnemyAI()
    {
        activeUnit = null;
        selectedAction = null;
        selectedEnemyAIAction = null;
        enemyUnits = null;
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
            currentState = State.WaitingForEnemyTurn;
        }
    }

}
