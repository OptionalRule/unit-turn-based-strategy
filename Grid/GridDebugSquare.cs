using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridDebugSquare : MonoBehaviour
{
    [SerializeField] private TextMeshPro _textMeshPro;

    private object gridSquare;

    protected virtual void Update()
    {
        _textMeshPro.text = gridSquare.ToString();
    }

    public virtual void SetGridSquare(object gridSquare)
    {
        this.gridSquare = gridSquare;
        
    }

    void OnValidate()
    {
        //Simple check and complain
        if (_textMeshPro == null) Debug.LogWarning($"{name} does not have a text mesh pro element assigned");
    }
}
