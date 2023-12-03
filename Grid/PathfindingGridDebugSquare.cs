using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PathfindingGridDebugSquare : GridDebugSquare
{
    [SerializeField] private TextMeshPro gCostText;
    [SerializeField] private TextMeshPro hCostText;
    [SerializeField] private TextMeshPro fCostText;

    private PathNode pathNode;

    public override void SetGridSquare(object gridSquare)
    {
        base.SetGridSquare(gridSquare);
        pathNode = (PathNode) gridSquare;
    }

    protected override void Update()
    {
        base.Update();
        gCostText.text = "G: " + pathNode.GetGCost().ToString();
        hCostText.text = "H: " + pathNode.GetHCost().ToString();
        fCostText.text = "F: " + pathNode.GetFCost().ToString();
    }
}
