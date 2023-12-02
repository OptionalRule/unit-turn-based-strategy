using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [SerializeField] private GameObject circlePrefab;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PathToMouseCursor()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.WorldPositionToGridPosition(MouseController.GetPosition());
            GridPosition startGridPosition = LevelGrid.Instance.WorldPositionToGridPosition(gameObject.transform.position);
            List<GridPosition> path = Pathfinding.Instance.FindPath(startGridPosition, mouseGridPosition, out int pathLength);
            if (path == null) return;

            UpdateLineRenderer(path);
        }
    }

    private void UpdateLineRenderer(List<GridPosition> path)
    {
        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPosition = LevelGrid.Instance.GridPositionToWorldPosition(path[i]);
            lineRenderer.SetPosition(i, worldPosition + Vector3.up * 0.03f);
        }
        AddCirclesToLine(path);
    }

    private void AddCirclesToLine(List<GridPosition> path)
    {
        foreach (var position in path)
        {
            Vector3 worldPosition = LevelGrid.Instance.GridPositionToWorldPosition(position);
            Instantiate(circlePrefab, worldPosition + Vector3.up * 0.04f, Quaternion.identity, transform);
        }
    }
}
