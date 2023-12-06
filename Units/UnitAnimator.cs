using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletProjectilePrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    MoveAction moveAction;
    ShootAction shootAction;
    HealthSystem healthSystem;
    DodgeAction dodgeAction;

    private void OnValidate()
    {
        if (animator == null) Debug.LogError("UnitAnimator is missing an animator component!");
        if (bulletProjectilePrefab == null) Debug.LogError("UnitAnimator is missing a bulletProjectilePrefab reference!");
    }

    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out moveAction))
        {
            moveAction.OnMoveActionStart += MoveAction_OnMoveActionStart;
            moveAction.OnMoveActionStop += MoveAction_OnMoveActionStop;
        } else
        {
            Debug.LogError("UnitAnimator is unable to get the moveaction component!");
        }
        if(TryGetComponent<ShootAction>(out shootAction))
        {
            shootAction.OnShootActionStart += ShootAction_OnShootActionStart;
        } else
        {
            Debug.LogError("UnitAnimator is unable to get the shootaction component!");
        }
        if (TryGetComponent<HealthSystem>(out healthSystem))
        {
            healthSystem.OnRecieveDamage += HealthSystem_OnRecieveDamage;
        }
        else
        {
            Debug.LogError("UnitAnimator is unable to get the healthsystem component!");
        }
        if (TryGetComponent<DodgeAction>(out dodgeAction))
        {
            dodgeAction.OnDodgeActionStart += DodgeAction_OnDodgeActionStart;
        }
        else
        {
            Debug.LogError("UnitAnimator is unable to get the dodgeaction component!");
        }
    }

    private void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.OnMoveActionStart -= MoveAction_OnMoveActionStart;
            moveAction.OnMoveActionStop -= MoveAction_OnMoveActionStop;
        }
        if(shootAction != null)
        {
            shootAction.OnShootActionStart -= ShootAction_OnShootActionStart;
        }
        if(healthSystem != null)
        {
            healthSystem.OnRecieveDamage -= HealthSystem_OnRecieveDamage;
        }
        if(dodgeAction != null)
        {
            dodgeAction.OnDodgeActionStart -= DodgeAction_OnDodgeActionStart;
        }
    }

    private void MoveAction_OnMoveActionStart(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", true);
    }

    private void MoveAction_OnMoveActionStop(object sender, EventArgs e)
    {
        animator.SetBool("IsWalking", false);
    }

    private void ShootAction_OnShootActionStart(object sender, ShootAction.OnShootEventArgs e)
    {
        animator.SetTrigger("DoShootRifle");

        Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        Vector3 aimPoint = e.targetUnit.GetWorldPosition();
        aimPoint.y = bulletSpawnPoint.position.y;

        bulletProjectile.Setup(aimPoint);
    }

    private void HealthSystem_OnRecieveDamage(object sender, EventArgs e)
    {
        animator.SetTrigger("DoHitReaction");
    }

    private void DodgeAction_OnDodgeActionStart(object sender, EventArgs e)
    {
        animator.SetTrigger("DoDodge");
    }
}
