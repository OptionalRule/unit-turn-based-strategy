using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public static float UNIT_SHOULDER_HEIGHT = 1.7f;
    
    private GridPosition unitGridPosition;
    private MoveAction moveAction;
    private DodgeAction dodgeAction;
    private ShootAction shootAction;
    private BaseAction[] baseActionArray;
    private int actionPoints;
    private const int ACTION_POINT_MAX = 2;
    private Vector3 lastDamageSourcePosition;

    [SerializeField] private bool isEnemy = false;
    [SerializeField] private Transform _rootBone;
    [SerializeField] private List<Transform> _actionCameraPositions;
    [SerializeField] private Transform _targetSpot;

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;

    private HealthSystem healthSystem;
    private UnitRagdollSpawner unitRagdollSpawner;

    private List<UnitCondition> unitConditions = new List<UnitCondition>();

    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        dodgeAction = GetComponent<DodgeAction>();
        shootAction = GetComponent<ShootAction>();
        baseActionArray = GetComponents<BaseAction>();
        healthSystem = GetComponent<HealthSystem>();
        unitRagdollSpawner = GetComponent<UnitRagdollSpawner>();
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

        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
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

    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction baseAction in GetBaseActions())
        {
            if (baseAction is T)
            {
                return (T)baseAction;
            }
        }
        return null;
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
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SpendActionPoints(int actionPointCost)
    {
        actionPoints -= actionPointCost;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
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

        unitConditions.Clear();
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
        lastDamageSourcePosition = transform.position;
    }

    public void ApplyDamage(int damageAmount, Vector3 damageSourcePosition)
    {
        lastDamageSourcePosition = damageSourcePosition; // This needs to get set first or the evne tis called before it's set.
        healthSystem.ApplyDamage(damageAmount);
    }

    public bool IsDead()
    {
        return healthSystem.IsDead();
    }

    public void Die()
    {
        unitRagdollSpawner.SpawnRagdoll(this, lastDamageSourcePosition);
        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);
        Destroy(gameObject);
    }

    public Transform GetRootBone()
    {
        return _rootBone;
    }

    public Vector3 GetRandomActionCameraPosition()
    {
        Transform actionCamera = _actionCameraPositions[UnityEngine.Random.Range(0, _actionCameraPositions.Count)];
        return actionCamera.position;
    }

    public Vector3 GetTargetPoint()
    {
        return _targetSpot.position;
    }

    public void AddCondition(UnitCondition unitCondition)
    {
        unitConditions.Add(unitCondition);
    }

    public bool HasCondition(UnitCondition unitCondition)
    {
        return unitConditions.Contains(unitCondition);
    }
}
