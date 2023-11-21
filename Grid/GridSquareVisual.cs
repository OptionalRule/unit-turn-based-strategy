using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquareVisual : MonoBehaviour
{
    [SerializeField] private Color defaultColor = Color.white;

    [SerializeField] private MeshRenderer meshRenderer;

    private void OnValidate()
    {
        if(meshRenderer == null) { Debug.LogError($"{name} meshRenderer not assigned"); }
    }

    private void Awake()
    {
        defaultColor.a = 0.35f;
        meshRenderer.material.color = defaultColor;
    }

    public void Show()
    {
        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled=false;
    }

    public void SetColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    public void ResetColor()
    {
        meshRenderer.material.color = defaultColor;
    }
}
