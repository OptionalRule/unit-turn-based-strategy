using System.Collections.Generic;
using System;
using UnityEngine;

public class Movement
{
    private Queue<GridPosition> pathQueue = new Queue<GridPosition>();
    private Animator unitAnimator;
    private Transform unitTransform;

    private float moveSpeed;
    private float rotateSpeed;
    private float stopDistance;

    public bool IsMoving => pathQueue.Count > 0;

    public Movement(Animator animator, Transform transform, float speed, float rotationSpeed, float distanceToStop)
    {
        unitAnimator = animator;
        unitTransform = transform;
        moveSpeed = speed;
        rotateSpeed = rotationSpeed;
        stopDistance = distanceToStop;
    }

    public void StartMove(List<GridPosition> path, Action onMoveStart = null)
    {
        pathQueue.Clear();
        foreach (var position in path)
        {
            pathQueue.Enqueue(position);
        }

        onMoveStart?.Invoke();
    }

    public void ContinueMove()
    {
        if (pathQueue.Count == 0)
        {
            return;
        }

        GridPosition targetGridPosition = pathQueue.Peek();
        Vector3 targetWorldPosition = LevelGrid.Instance.GridPositionToWorldPosition(targetGridPosition);
        RotateTowards(targetWorldPosition);
        MoveTowards(targetWorldPosition);

        if (IsAtTargetPosition(targetWorldPosition))
        {
            pathQueue.Dequeue();
            if (pathQueue.Count == 0)
            {
                StopMove();
            }
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 moveDirection = (targetPosition - unitTransform.position).normalized;
        unitTransform.position += moveDirection * Time.deltaTime * moveSpeed;
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - unitTransform.position).normalized;
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            unitTransform.rotation = Quaternion.Lerp(unitTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
        }
    }

    private bool IsAtTargetPosition(Vector3 targetPosition)
    {
        float distanceToTarget = Vector3.Distance(unitTransform.position, targetPosition);
        return distanceToTarget <= stopDistance;
    }

    public void StopMove()
    {
        // Stop animation or any other cleanup if needed
        unitAnimator?.SetBool("IsWalking", false);
    }
}