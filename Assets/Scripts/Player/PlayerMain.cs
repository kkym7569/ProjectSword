using System;
using System.Collections;
using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    [Header("Manager")]
    public TargetManager manager;

    [Header("Attack Move Settings")]
    public float speed = 15.0f;
    public float stoppingDistance = 0.01f;

    public event Action<Vector2>         OnAttackMoveStarted;
    public event Action<Vector2, Vector2> OnAttackMoveUpdated;
    public event Action                  OnAttackMoveEnded;

    private Transform    currentTarget;
    private bool         isMoving = false;
    private Vector2      attackStartPos;
    private Coroutine    moveCoroutine;
    private SwordBehaviour _currentSwordBehaviour; // 현재 검의 특수 동작

    private IEnumerator MoveToTargetCoroutine()
    {
        while (isMoving && currentTarget != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                currentTarget.position,
                speed * Time.deltaTime
            );

            OnAttackMoveUpdated?.Invoke(attackStartPos, transform.position);

            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= stoppingDistance)
            {
                transform.position = currentTarget.position;
                ExecuteCollection(currentTarget.gameObject);
                break;
            }
            yield return null;
        }
    }

    /// <param name="swordBehaviour">이 검의 특수 동작 (없으면 null)</param>
    public void StartAttackToTarget(Transform targetTransform, SwordBehaviour swordBehaviour = null)
    {
        if (isMoving || targetTransform == null) return;

        currentTarget          = targetTransform;
        isMoving               = true;
        attackStartPos         = transform.position;
        _currentSwordBehaviour = swordBehaviour;

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        // ① 출발 전 특수 동작 (발사체 검: 구체 발사)
        _currentSwordBehaviour?.OnBeforeAttackMove(attackStartPos);

        OnAttackMoveStarted?.Invoke(attackStartPos);
        moveCoroutine = StartCoroutine(MoveToTargetCoroutine());
    }

    private void ExecuteCollection(GameObject targetObject)
    {
        isMoving = false;

        // ② 도착 후 특수 동작 (발도 검: 회전 베기)
        _currentSwordBehaviour?.OnAfterAttackMove(transform.position);

        manager.TargetEaten(targetObject);
        OnAttackMoveEnded?.Invoke();
    }
}
