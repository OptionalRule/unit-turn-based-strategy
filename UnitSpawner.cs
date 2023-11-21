using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{

    [SerializeField] private Unit unitPrefab;
    [SerializeField] private int spawnNumber = 1;
    [SerializeField] private int maxSpawnDistance = 1;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private string unitBaseName = "Unit";

    private void OnValidate()
    {
        if (unitPrefab == null)
        {
            Debug.LogError("PlayerUnitPrefab is not set in the inspector!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // if the game object is not on a valid grid square, log error and return.
        if (spawnOnStart)
        {
            SpawnUnits();
        }
    }

    private void SpawnUnits()
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
            Unit unit = Instantiate(unitPrefab, spawnPosition, transform.rotation, transform);
            unit.name = $"{unitBaseName} {i}";
 
            LevelGrid.Instance.AddUnitToGrid(randomGridPosition, unit);
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
        Gizmos.color = Color.blue; // Set the color of the gizmo for the sphere
        Gizmos.DrawWireSphere(transform.position, 0.5f); // Draw a wireframe sphere at the spawner's position

        // Draw an arrow indicating the forward direction
        Vector3 forward = transform.forward * 1.0f; // Length of the arrow
        Gizmos.color = Color.green; // Set a different color for the arrow for visibility
        Gizmos.DrawRay(transform.position, forward); // Draw the arrow as a line

        // Optionally, you can also draw a small triangle at the end of the arrow for a more arrow-like appearance
        // Adjust the size and angle of the triangle to your preference
        //Vector3 right = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 180 + 30, 0) * new Vector3(0, 0, 1);
        //Vector3 left = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 180 - 30, 0) * new Vector3(0, 0, 1);
        //Gizmos.DrawRay(transform.position + forward, right * 0.25f);
        //Gizmos.DrawRay(transform.position + forward, left * 0.25f);
    }
}
