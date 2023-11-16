using System.Collections;
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

    private TrailRenderer GetTrailRenderer()
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
            
            // Handles the hit or miss.  For this game need to refactor to however I want to handle in game.
            if(Physics.Raycast(_model.transform.position, shootDirection, out RaycastHit hit, float.MaxValue, ShootConfiguration.HitMask))
            {
                _behaviour.StartCoroutine(PlayTrail(_shootParticleSystem.transform.position, hit.point, hit));
            } else
            {
                _behaviour.StartCoroutine(PlayTrail(_shootParticleSystem.transform.position, shootDirection * TrailConfiguration.MissDistance, new RaycastHit()));
            }
        }
    }

    private IEnumerator PlayTrail(Vector3 startPosition, Vector3 endPosition, RaycastHit hit)
    {
        TrailRenderer trailRenderer = _trailRendererPool.Get();
        trailRenderer.gameObject.SetActive(true);
        trailRenderer.transform.position = startPosition;
        yield return null; // hack to give is a moment so as not to pull attributes from previous trail render.
        trailRenderer.transform.LookAt(endPosition);
        trailRenderer.emitting = true;
        float distance = Vector3.Distance(startPosition, endPosition);
        float remainingDistance = distance;
        while(remainingDistance > 0)
        {
            trailRenderer.transform.position = Vector3.Lerp(startPosition, endPosition, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= TrailConfiguration.SimulationSpeed * Time.deltaTime;
            yield return null;
        }
        trailRenderer.transform.position = endPosition;

        // If I had surface impact an audio, I would handle it here.

        yield return new WaitForSeconds(TrailConfiguration.TimeToLive);
        yield return null;
        trailRenderer.emitting = false;
        trailRenderer.gameObject.SetActive(false);
        _trailRendererPool.Release(trailRenderer);
    }
}
