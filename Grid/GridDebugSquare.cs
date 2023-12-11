using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridDebugSquare : MonoBehaviour
{
    [SerializeField] private TextMeshPro coordinatesText;

    private object gridSquare;

    protected virtual void Update()
    {
        coordinatesText.text = gridSquare.ToString();
    }

    public virtual void SetGridSquare(object gridSquare)
    {
        this.gridSquare = gridSquare;
        
    }

    void OnValidate()
    {
        //Simple check and complain
        if (coordinatesText == null) Debug.LogWarning($"{name} does not have a coordinates text mesh pro element assigned");
    }
}
