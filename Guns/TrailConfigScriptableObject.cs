using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Trail Configuration", order = 4)]
public class TrailConfigScriptableObject : ScriptableObject
{
    public Material TrailMaterial;
    public AnimationCurve WidthCurve;
    public float TimeToLive = 0.5f;
    public float MinVertexDistance = 0.1f;
    public Gradient Color;

    public float MissDistance = 100f;
    public float SimulationSpeed = 100f; // 100m/s
}
