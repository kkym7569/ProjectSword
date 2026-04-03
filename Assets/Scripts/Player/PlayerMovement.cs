using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Manager")]
    public TargetManager manager;

    [Header("Dash Settings")]
    public float speed = 15.0f; // 대시 속도 (시원한 타격감을 위해 조금 높였습니다)
    public float stoppingDistance = 0.01f; // 도착 판정 거리

    [Header("BoxCast (Hit Area) Settings")]
    public float slashThickness = 1.0f; // 타격 판정 및 기즈모의 두께

    [Header("Visual Effects (Line Renderer)")]
    public LineRenderer slashEffect;
    public float slashFadeTime = 0.3f; // 검기가 머무르며 사라지는 시간

    // 내부 상태 관리 변수들
    private Transform currentTarget;
    private bool isMoving = false;
    private Vector2 dashStartPos;

    private float originalSlashWidth;
    private Coroutine moveCoroutine;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // 시작할 때 Line Renderer의 원래 굵기를 저장해두고 꺼둡니다.
        if (slashEffect != null)
        {
            originalSlashWidth = slashEffect.widthMultiplier;
            slashEffect.enabled = false;
        }
    }

    // A키 입력 시 실행
    public void OnMoveToPoint(InputAction.CallbackContext context)
    {
        // 이동 중이 아닐 때만 새로운 입력을 받음
        if (context.started && !isMoving)
        {
            currentTarget = manager.GetCurrentTargetTransform();

            if (currentTarget != null)
            {
                isMoving = true;
                dashStartPos = transform.position;
                Debug.Log($"추적 시작: {currentTarget.name}");

                // 1. 기존에 실행 중이던 이동 코루틴 멈춤
                if (moveCoroutine != null) StopCoroutine(moveCoroutine);

                // 2. 검기가 아직 사라지는 중이었다면 멈추고 굵기를 원상복구
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                if (slashEffect != null)
                {
                    slashEffect.widthMultiplier = originalSlashWidth;
                    slashEffect.enabled = true;
                }

                // 3. 새로운 대시 시작
                moveCoroutine = StartCoroutine(MoveToTargetCoroutine());
            }
        }
    }

    // [핵심] 이동을 처리하는 코루틴 (Update 함수 대체)
    private IEnumerator MoveToTargetCoroutine()
    {
        while (isMoving && currentTarget != null)
        {
            // 목표(타겟 중심)를 향해 이동
            transform.position = Vector2.MoveTowards(
                transform.position,
                currentTarget.position,
                speed * Time.deltaTime
            );

            // 라인 렌더러(시각 효과)의 시작점과 끝점 실시간 업데이트
            if (slashEffect != null)
            {
                slashEffect.SetPosition(0, dashStartPos);          // 꼬리 (출발점)
                slashEffect.SetPosition(1, transform.position);    // 머리 (현재 위치)
            }

            // 플레이어 중심과 타겟 중심 사이의 거리 계산
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            // 설정한 정지 거리 이하로 가까워졌다면 (도착)
            if (distanceToTarget <= stoppingDistance)
            {
                // 강제로 완벽한 타겟 좌표로 맞춤 (오차 보정)
                transform.position = currentTarget.position;

                // 타겟 수집 처리
                ExecuteCollection(currentTarget.gameObject);
                break; // 반복문 탈출
            }

            // 다음 프레임까지 대기
            yield return null;
        }

        // 이동이 완전히 끝난 후, 검기가 서서히 사라지는 연출 실행
        if (slashEffect != null)
        {
            fadeCoroutine = StartCoroutine(FadeOutSlash());
        }
    }

    // [연출] 검기의 굵기를 서서히 0으로 만들어 날카롭게 사라지게 하는 코루틴
    private IEnumerator FadeOutSlash()
    {
        float timer = 0f;

        while (timer < slashFadeTime)
        {
            timer += Time.deltaTime;

            // 시간 비율(0~1)에 따라 굵기를 부드럽게 줄임
            slashEffect.widthMultiplier = Mathf.Lerp(originalSlashWidth, 0f, timer / slashFadeTime);

            yield return null;
        }

        // 완전히 얇아지면 비활성화
        slashEffect.enabled = false;
    }

    // 타겟 수집 및 매니저 보고 처리
    private void ExecuteCollection(GameObject targetObject)
    {
        isMoving = false;
        Debug.Log($"{targetObject.name} 중심 도착 및 수집 완료");

        targetObject.SetActive(false); // 타겟 비활성화
        manager.TargetEaten();         // 다음 타겟 준비
    }

    // [디버그] 플레이어 이동 경로에 맞춰 BoxCast 판정 영역 그려주기
    private void OnDrawGizmos()
    {
        if (isMoving && currentTarget != null)
        {
            Vector2 startPos = dashStartPos;
            Vector2 currentPos = transform.position;
            float distance = Vector2.Distance(startPos, currentPos);

            if (distance < 0.01f) return;

            Vector2 direction = (currentPos - startPos).normalized;
            Vector2 centerPos = startPos + (direction * (distance / 2));
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Vector2 boxSize = new Vector2(distance, slashThickness);

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(centerPos, Quaternion.Euler(0, 0, angle), Vector3.one);
            Gizmos.matrix = rotationMatrix;

            // 반투명 빨간색 채우기
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(Vector3.zero, boxSize);

            // 진한 빨간색 테두리
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }
}