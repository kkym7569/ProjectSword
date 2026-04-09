using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMain : MonoBehaviour
{
    [Header("Manager")]
    public TargetManager manager;

    [Header("Attack Move Settings")]
    public float speed = 15.0f; // 공격 이동 속도
    public float stoppingDistance = 0.01f;

    // --- 전투/연출 스크립트로 보낼 이벤트들 (Action) ---
    public event Action<Vector2> OnAttackMoveStarted;             // 공격 이동 시작
    public event Action<Vector2, Vector2> OnAttackMoveUpdated;    // 공격 이동 중 (매 프레임)
    public event Action OnAttackMoveEnded;                        // 공격 이동 종료

    private Transform currentTarget;
    private bool isMoving = false;
    private Vector2 attackStartPos;
    private Coroutine moveCoroutine;


    private IEnumerator MoveToTargetCoroutine()
    {
        while (isMoving && currentTarget != null)
        {
            // 타겟(검)을 향해 이동
            transform.position = Vector2.MoveTowards(
                transform.position,
                currentTarget.position,
                speed * Time.deltaTime
            );

            // 매 프레임 현재 위치를 전투 스크립트로 전달
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

    public void StartAttackToTarget(Transform targetTransform)
    {
        // 플레이어가 이미 이동(공격) 중이 아닐 때만 새로운 공격 시작
        if (!isMoving && targetTransform != null)
        {
            currentTarget = targetTransform;
            isMoving = true;
            attackStartPos = transform.position;

            if (moveCoroutine != null) StopCoroutine(moveCoroutine);

            OnAttackMoveStarted?.Invoke(attackStartPos);
            moveCoroutine = StartCoroutine(MoveToTargetCoroutine());
        }
    }


    private void ExecuteCollection(GameObject targetObject)
    {
        isMoving = false;

        // [수정됨] 매니저에게 "나 이 타겟 먹었어!" 라고 구체적으로(targetObject) 알려줍니다.
        manager.TargetEaten(targetObject);

        OnAttackMoveEnded?.Invoke();
    }
}