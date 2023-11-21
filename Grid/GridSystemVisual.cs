using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{
    public static GridSystemVisual Instance { get; private set; }

    private GridSquareVisual[,] gridSquareVisualArray;

    [SerializeField] private Transform gridVisualSystemPrefab;

    private void OnValidate()
    {
        if (gridVisualSystemPrefab == null) { Debug.LogError($"{name} gridVisualSystemPrefab not set in inspector!"); }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("An instance of GridSystemVisual exists!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        gridSquareVisualArray = new GridSquareVisual[
            LevelGrid.Instance.GetWidth(),
            LevelGrid.Instance.GetHeight()
            ];
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Transform gridSquareVisualTransform =
                    Instantiate(gridVisualSystemPrefab, LevelGrid.Instance.GridPositionToWorldPosition(gridPosition), 
                        Quaternion.identity, this.transform);

                if(!gridSquareVisualTransform.TryGetComponent<GridSquareVisual>(out gridSquareVisualArray[x, z]))
                {
                    Debug.LogError("Unable to get GridSquareVisual from the GridSquareVisualTransform!");
                }
            }
        }
        // HideAllGridPositions();
    }

    public void HideAllGridPositions()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                gridSquareVisualArray[x, z].Hide();
            }
        }
    }

    public void ResetAllGridPositions()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                gridSquareVisualArray[x, z].ResetColor();
            }
        }
    }

    public void ShowGridPositions(List<GridPosition> gridPositions, Color actionColor)
    {
        foreach(GridPosition gridPosition in gridPositions)
        {
            GridSquareVisual gs = gridSquareVisualArray[gridPosition.X, gridPosition.Z];
            gs.SetColor(actionColor);
            gs.Show();
        }
    }
}
