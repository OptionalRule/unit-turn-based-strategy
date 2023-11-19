using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollRootBone;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(Transform originalRootBone, Vector3 damageSourcePosition)
    {
        MatchAllChildTransforms(originalRootBone, ragdollRootBone);
        ApplyExplosiveForceToRagdoll(ragdollRootBone, 300f, damageSourcePosition, 10f);
    }

    private void MatchAllChildTransforms(Transform originalTransform, Transform ragdollTransform)
    {
        foreach (Transform originalChild in originalTransform)
        {
            Transform ragdollChild = ragdollTransform.Find(originalChild.name);
            if (ragdollChild != null)
            {
                ragdollChild.position = originalChild.position;
                ragdollChild.rotation = originalChild.rotation;
                MatchAllChildTransforms(originalChild, ragdollChild);
            }
        }
    }

    private void ApplyExplosiveForceToRagdoll(Transform root, float force, Vector3 forcePosition, float forceRange)
    {
        Vector3 forceDirection = (forcePosition - root.position).normalized;
        Vector3 explosionPoint = root.position + forceDirection;
        foreach (Transform child in root)
        {
            Rigidbody rigidbody = child.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.AddExplosionForce(force, explosionPoint, forceRange);
            }
            ApplyExplosiveForceToRagdoll(child, force, forcePosition, forceRange);
        }
    }

    public void SpawnDebugSphere(Vector3 position, float radius = 0.5f)
    {
        GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.transform.position = position;
        debugSphere.transform.localScale = new Vector3(radius, radius, radius);

        // Set the color to blue
        Renderer renderer = debugSphere.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.blue;

        // Optional: Add a Rigidbody if you want the sphere to be affected by physics
        // debugSphere.AddComponent<Rigidbody>();
    }
}
