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

    public void OnMoveToPoint(InputAction.CallbackContext context)
    {
        if (context.started && !isMoving)
        {
            // [수정됨] 현재 TargetManager에 맞게 괄호 안의 인수를 제거했습니다.
            currentTarget = manager.GetCurrentTargetTransform();

            if (currentTarget != null)
            {
                isMoving = true;
                attackStartPos = transform.position;
                Debug.Log($"공격 이동 시작: {currentTarget.name}");

                if (moveCoroutine != null) StopCoroutine(moveCoroutine);

                // 공격 시작 이벤트 발송
                OnAttackMoveStarted?.Invoke(attackStartPos);

                moveCoroutine = StartCoroutine(MoveToTargetCoroutine());
            }
        }
    }

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

    private void ExecuteCollection(GameObject targetObject)
    {
        isMoving = false;
        Debug.Log($"{targetObject.name} 중심 도착 및 수집 완료");

        // [수정됨] 현재 TargetManager는 알아서 대상을 비활성화하므로 SetActive(false) 중복 호출 제거
        // [수정됨] TargetEaten() 호출 시 괄호 안의 인수를 제거했습니다.
        manager.TargetEaten();

        // 공격 이동 종료 이벤트 발송
        OnAttackMoveEnded?.Invoke();
    }
}