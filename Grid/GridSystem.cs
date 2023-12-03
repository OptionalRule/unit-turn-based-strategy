using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;

    private TGridObject[,] gridObjectArray;

    public GridSystem(int width, int height, float cellSize,
        Func<GridSystem<TGridObject>, GridPosition, TGridObject> gridObjectConstructor)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        this.gridObjectArray = new TGridObject[width, height];


        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                gridObjectArray[x, z] = gridObjectConstructor(this, new GridPosition(x, z));
            }
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.X, 0, gridPosition.Z) * cellSize;
    }

    public float GetDistanceBetween(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        return Vector3.Distance(GetWorldPosition(gridPositionA), GetWorldPosition(gridPositionB));
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        return new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize), 
            Mathf.RoundToInt(worldPosition.z / cellSize)
            );
    }

    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjectArray[gridPosition.X, gridPosition.Z];
    }

    public void CreateDebugObjects(Transform debugPrefab, Transform parent)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition) + new Vector3(0f, 0.01f, 0), Quaternion.identity, parent);
                GridDebugSquare gridDebugSquare = debugTransform.GetComponent<GridDebugSquare>();
                gridDebugSquare.SetGridSquare(GetGridObject(gridPosition));
            }
        }
    }

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        return gridPosition.X >= 0 &&
               gridPosition.Z >= 0 &&
               gridPosition.X < width &&
               gridPosition.Z < height;
    }

    public bool IsValidGridPosition(int x, int z)
    {
        return x >= 0 &&
               z >= 0 &&
               x < width &&
               z < height;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }
}
