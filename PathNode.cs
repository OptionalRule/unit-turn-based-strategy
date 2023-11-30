using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode

{
    private GridPosition gridPosition;
    private int gCost;
    private int hCost;
    private int fCost;

    private PathNode previousPathNode;

    public PathNode(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public override string ToString()
    {
        return gridPosition.ToString();
    }

    public int GetFCost()
    {
        return fCost;
    }

    public int GetGCost()
    {
        return gCost;
    }

    public int GetHCost()
    {
         return hCost;
    }
}
