using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    private enum State
    {
        WaitingForTurn,
        TakingTurn,
        Busy,
        Dead
    }

    private State currentState;

    private float timer;


    void Start()
    {
        currentState = State.WaitingForTurn;
        TurnSystem.Instance.OnNextTurn += TurnSystem_OnTurnChanged;
    }

    private void OnDestroy()
    {
        TurnSystem.Instance.OnNextTurn -= TurnSystem_OnTurnChanged;
    }

    // Update is called once per frame
    void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn() || currentState == State.Dead)
        {
            return;
        }
        
        switch (currentState)
        {
            case State.WaitingForTurn:
                // Debug.Log($"{gameObject.name} State: {currentState}");
                break;
            case State.TakingTurn:
                TakeTurn();
                break;
            case State.Busy:
                break;
            case State.Dead:
                break;
            default:
                break;
        }
    }

    private void TakeTurn()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            TakeEnemyAITurn(() =>
            {
                currentState = State.WaitingForTurn;
                TurnSystem.Instance.NextTurn();
            });
        }
    }

    private void TakeEnemyAITurn(Action onEnemyAIActionComplete)
    {
        foreach (Unit unit in UnitManager.Instance.GetEnemyUnits())
        {
            if (unit.IsDead())
            {
                continue;
            }

            TakeEnemyUnitAIAction(unit, () =>
            {
                Debug.Log($"{unit.name} done with turn.");
            });
        }
        onEnemyAIActionComplete?.Invoke();
    }

    private void TakeEnemyUnitAIAction(Unit enemyUnit, Action onEnemyUnitAIActionComplete)
    {
        SpinAction spinAction = enemyUnit.GetSpinAction();

        GridPosition gridPosition = enemyUnit.GetGridPosition();
        if(!spinAction.CanTakeAction(gridPosition))
        {
            onEnemyUnitAIActionComplete?.Invoke();
            return;
        }
        if(!enemyUnit.CanSpendActionPointsToTakeAction(spinAction))
        {
            onEnemyUnitAIActionComplete?.Invoke();
            return;
        }
        spinAction.TakeAction(gridPosition, () =>
        {
            onEnemyUnitAIActionComplete?.Invoke();
        });
    }

    private void TurnSystem_OnTurnChanged(object sender, System.EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            currentState = State.TakingTurn;
            timer = 2f;
        }
        else
        {
            currentState = State.WaitingForTurn;
        }
    }
}
