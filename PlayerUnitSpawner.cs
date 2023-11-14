using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitSpawner : MonoBehaviour
{

    [SerializeField] private Unit PlayerUnitPrefab;

    private void OnValidate()
    {
        if (PlayerUnitPrefab == null)
        {
            Debug.LogError("PlayerUnitPrefab is not set in the inspector!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // if the game object is not on a valid grid square, log error and return.
        SpawnPlayerUnits(2, 1);
    }

    private void SpawnPlayerUnits(int spawnNumber, int maxSpawnDistance)
    {
        GridPosition spawnerGridPosition = LevelGrid.Instance.WorldPositionToGridPosition(transform.position);

        for (int i = 0; i < spawnNumber; i++)
        {
            List<GridPosition> validGridPositions = GetValidGridPositions(spawnerGridPosition, maxSpawnDistance);

            // Check if there are valid positions available
            if (validGridPositions.Count == 0)
            {
                Debug.LogError("No valid grid positions available for spawning!");
                return;
            }

            // Get a random position from the list of valid grid positions
            int randomIndex = Random.Range(0, validGridPositions.Count);
            GridPosition randomGridPosition = validGridPositions[randomIndex];
            validGridPositions.RemoveAt(randomIndex); // Remove the chosen position

            // Spawn player unit at the world position of the chosen grid position
            Vector3 spawnPosition = LevelGrid.Instance.GridPositionToWorldPosition(randomGridPosition);
            Unit playerUnit = Instantiate(PlayerUnitPrefab, spawnPosition, Quaternion.identity, transform);

            LevelGrid.Instance.AddUnitToGrid(randomGridPosition, playerUnit);
        }

    }

    private List<GridPosition> GetValidGridPositions(GridPosition gridPosition, int maxDistance)
    {
        List<GridPosition> validGridPositions = new List<GridPosition>();
        for (int x = -maxDistance; x <= maxDistance; x++)
        {
            for (int z = -maxDistance; z <= maxDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition targetGridPosition = offsetGridPosition + gridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(targetGridPosition)) { continue; }
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(targetGridPosition)) { continue; }
                validGridPositions.Add(targetGridPosition);
            }
        }
        return validGridPositions;
    }

    // This method is called in the editor to draw gizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue; // Set the color of the gizmo
        Gizmos.DrawWireSphere(transform.position, 1f); // Draw a wireframe sphere at the spawner's position
    }
}
