using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera actionCamera;

    private void OnEnable()
    {
        ShootAction.OnAnyShootActionStart += ShootAction_OnAnyShootActionStart;
        ShootAction.OnAnyShootActionEnd += ShootAction_OnAnyShootActionEnd;
    }

    private void OnDisable()
    {
        ShootAction.OnAnyShootActionStart -= ShootAction_OnAnyShootActionStart;
        ShootAction.OnAnyShootActionEnd -= ShootAction_OnAnyShootActionEnd;
    }

    private void Start()
    {
        DisableActionCamera();
    }

    private void EnabeActionCamera()
    {
        actionCamera.gameObject.SetActive(true);
    }

    private void DisableActionCamera()
    {
        actionCamera.gameObject.SetActive(false);
    }

    private void ShootAction_OnAnyShootActionStart(object sender, ShootAction.OnShootEventArgs e)
    {
        Vector3 actionCameraPosition = e.shootingUnit.GetRandomActionCameraPosition();
        actionCamera.transform.position = actionCameraPosition;
        actionCamera.transform.LookAt(e.targetUnit.GetTargetPoint());
        EnabeActionCamera();
    }

    private void ShootAction_OnAnyShootActionEnd(object sender, System.EventArgs e)
    {
        DisableActionCamera();
    }
}
