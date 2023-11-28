using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private static UnitManager _instance;

    public static UnitManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UnitManager instance not found!");
            }
            return _instance;
        }
    }

    private List<Unit> unitList;
    private List<Unit> playerUnitList;
    private List<Unit> enemyUnitList;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("UnitManager instance already exists!");
            Destroy(gameObject);
            return;
        }

        _instance = this;

        unitList = new List<Unit>();
        playerUnitList = new List<Unit>();
        enemyUnitList = new List<Unit>();
    }

    private void Start()
    {
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    private void Unit_OnAnyUnitSpawned(object sender, System.EventArgs e)
    {
        Unit unit = sender as Unit;
        if (unit != null)
        {
            unitList.Add(unit);
            if (unit.IsEnemy())
            {
                enemyUnitList.Add(unit);
            }
            else
            {
                playerUnitList.Add(unit);
            }
        }
    }

    private void Unit_OnAnyUnitDead(object sender, System.EventArgs e)
    {
        Unit unit = sender as Unit;
        Debug.Log($"Unit {unit.name} died!");
        if (unit != null)
        {
            unitList.Remove(unit);
            if (unit.IsEnemy())
            {
                enemyUnitList.Remove(unit);
            }
            else
            {
                playerUnitList.Remove(unit);
            }
        }
    }

    public List<Unit> GetPlayerUnits()
    {
        return playerUnitList;
    }

    public List<Unit> GetEnemyUnits()
    {
        return enemyUnitList;
    }

    public List<Unit> GetAllUnits()
    {
        return unitList;
    }

    public List<Unit> GetEnemyUnitsOf(Unit unit)
    {
        if (unit.IsEnemy())
        {
            return playerUnitList;
        }
        else
        {
            return enemyUnitList;
        }
    }
}
