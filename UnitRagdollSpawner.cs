using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdollSpawner : MonoBehaviour
{
    [SerializeField] private Transform _ragdollPrefab;

    public void SpawnRagdoll(Unit originalUnit, Vector3 damageSourcePosition)
    {
        Transform ragdoll = Instantiate(_ragdollPrefab, originalUnit.transform.position, originalUnit.transform.rotation);
        ragdoll.GetComponent<UnitRagdoll>().Setup(originalUnit.GetRootBone(), damageSourcePosition);
    }

}
