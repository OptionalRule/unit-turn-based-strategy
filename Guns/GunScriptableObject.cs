using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun Config", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public ShootConfigurationScriptableObject ShootConfiguration;
    public TrailConfigScriptableObject TrailConfiguration;

    private MonoBehaviour _behaviour;
    private GameObject _model;
    private float _lastShotTime;
    private ParticleSystem _shootParticleSystem;
    private ObjectPool<TrailRenderer> _trailRendererPool;

    public void Spawn(Transform parent, MonoBehaviour behaviour)
    {
        _behaviour = behaviour;
        _model = Instantiate(ModelPrefab, parent);
        _model.transform.SetParent(parent);
        _model.transform.localPosition = SpawnPoint;
        _model.transform.localRotation = Quaternion.Euler(SpawnRotation);
        _shootParticleSystem = _model.GetComponentInChildren<ParticleSystem>();
    }

    private TrailRenderer GetTrail()
    {
        GameObject trail = new GameObject("Bullet Trail");
        TrailRenderer trailRenderer = trail.AddComponent<TrailRenderer>();
        trailRenderer.colorGradient = TrailConfiguration.Color;
        trailRenderer.widthCurve = TrailConfiguration.WidthCurve;
        trailRenderer.material = TrailConfiguration.TrailMaterial;
        trailRenderer.time = TrailConfiguration.TimeToLive;
        trailRenderer.minVertexDistance = TrailConfiguration.MinVertexDistance;

        trailRenderer.emitting = false;
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trailRenderer;
    }

    public void Shoot()
    {
        if (Time.time > ShootConfiguration.FireRate + _lastShotTime)
        {
            _lastShotTime = Time.time;
            _shootParticleSystem.Play();
            Vector3 shootDirection = _model.transform.forward 
                + new Vector3(
                    Random.Range(-ShootConfiguration.Spread.x, ShootConfiguration.Spread.x),
                    Random.Range(-ShootConfiguration.Spread.y, ShootConfiguration.Spread.y),
                    Random.Range(-ShootConfiguration.Spread.z, ShootConfiguration.Spread.z)
                    );
            shootDirection.Normalize(); // Assumes forward is from the muzzel spawn position.
            // _behaviour.StartCoroutine(ShootCoroutine());
        }
    }
}
