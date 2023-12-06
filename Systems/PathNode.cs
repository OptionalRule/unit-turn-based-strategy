using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode

{
    private GridPosition gridPosition;
    private int gCost;
    private int hCost;
    private int fCost;
    public bool isWalkable;

    private PathNode previousPathNode;

    public PathNode(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
        this.IsWalkable = true;
    }

    public override string ToString()
    {
        return gridPosition.ToString();
    }

    public int GetFCost()
    {
        return fCost;
    }

    public void CalculateFCost()
    {
        this.fCost = GetHCost() + GetGCost();
    }

    public int GetGCost()
    {
        return gCost;
    }

    public void SetGCost(int gCost)
    {
        this.gCost = gCost;
    }   

    public int GetHCost()
    {
         return hCost;
    }

    public void SetHCost(int hCost)
    {
        this.hCost = hCost;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public bool IsWalkable
    {
        get { return isWalkable; }
        set { isWalkable = value; }
    }

    public void ResetPreviousPathNode()
    {
        previousPathNode = null;
    }  
    
    public void SetPreviousPathNode(PathNode previousPathNode)
    {
        this.previousPathNode = previousPathNode;
    }

    public PathNode GetPreviousPathNode()
    {
        return previousPathNode;
    }
}
