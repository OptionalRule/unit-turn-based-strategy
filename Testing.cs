using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            GridPosition mouseGridPosition = LevelGrid.Instance.WorldPositionToGridPosition(MouseController.GetPosition());
            GridPosition startGridPosition = new GridPosition(0, 0);
            List<GridPosition> path = Pathfinding.Instance.FindPath(startGridPosition, mouseGridPosition);
            if (path == null) return;

            GridPosition previousPosition = startGridPosition;
            foreach (GridPosition gridPosition in path)
            {
                Debug.DrawLine(
                    LevelGrid.Instance.GridPositionToWorldPosition(previousPosition),
                    LevelGrid.Instance.GridPositionToWorldPosition(gridPosition),
                    Color.green,
                    3f
                    );

                previousPosition = gridPosition;
            }
        }
    }
}
