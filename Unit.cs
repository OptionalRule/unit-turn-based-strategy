using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveAction))]
public class Unit : MonoBehaviour
{
    private GridPosition unitGridPosition;
    private MoveAction moveAction;
    private SpinAction spinAction;
    private BaseAction[] baseActionArray;
    private int actionPoints;
    private const int ACTION_POINT_MAX = 3;

    [SerializeField]
    private bool isEnemy = false;

    public static event EventHandler OnAnyActionPointChanged;

    private HealthSystem healthSystem;

    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        spinAction = GetComponent<SpinAction>();
        baseActionArray = GetComponents<BaseAction>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void OnDisable()
    {
        if (LevelGrid.Instance != null)
        {
            LevelGrid.Instance.RemoveUnitFromGrid(unitGridPosition, this);
        }
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.OnNextTurn -= TurnSystem_OnEndTurn;
        }
        if (healthSystem != null)
        {
            healthSystem.OnDead -= HealthSystem_OnDead;
        }
    }


    private void OnEnable()
    {
        if (TurnSystem.Instance == null)
        {
            Debug.LogError("Turn System is not set!");
        }
        ResetActionPoints();
        TurnSystem.Instance.OnNextTurn += TurnSystem_OnEndTurn;
        healthSystem.OnDead += HealthSystem_OnDead;
    }

    private void Start()
    {
        if (LevelGrid.Instance == null)
        {
            Debug.LogError("Level Grid is not set!");
        }
        unitGridPosition = LevelGrid.Instance.WorldPositionToGridPosition(this.transform.position);

        // Removed as spawner now adds unit to grid.  TODO, refactor move action to handle grid position.
        // LevelGrid.Instance.AddUnitToGrid(unitGridPosition, this);
    }

    public GridPosition GetGridPosition()
    {
        return unitGridPosition;
    }

    public void SetGridPosition(GridPosition gridPosition)
    {
        unitGridPosition = gridPosition;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public MoveAction GetMoveAction()
    {
        return moveAction;
    }

    public SpinAction GetSpinAction()
    {
        return spinAction;
    }

    public BaseAction[] GetBaseActions()
    {
        return baseActionArray;
    }

    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        return baseAction.GetActionPointCost() <= actionPoints;
    }

    private void ResetActionPoints()
    {
        actionPoints = ACTION_POINT_MAX;
        OnAnyActionPointChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SpendActionPoints(int actionPointCost)
    {
        actionPoints -= actionPointCost;
        OnAnyActionPointChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }

    private void TurnSystem_OnEndTurn(object sender, EventArgs e)
    {
        if ((IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) || (!IsEnemy() && TurnSystem.Instance.IsPlayerTurn()))
        {
            ResetActionPoints();
        }
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        Die();
    }

    public bool IsEnemy()
    {
        return isEnemy;
    }

    public bool IsEnemyOf(Unit otherUnit)
    {
        return IsEnemy() != otherUnit.IsEnemy();
    }

    public void ApplyDamage(int damageAmount)
    {
        healthSystem.ApplyDamage(damageAmount);
    }

    public bool IsDead()
    {
        return healthSystem.IsDead();
    }

    public void Die()
    {
        Debug.Log($"Oh noes!  {name} died!");
        Destroy(gameObject);
    }
}
