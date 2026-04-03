using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public TargetManager manager;
    public float speed = 5.0f;

    private Transform currentTarget;
    private bool isMoving = false;
    private bool canCollect = false; // 수집 가능 여부 체크포인트

    public void OnMoveToPoint(InputAction.CallbackContext context)
    {
        // A 키를 누르는 순간 실행
        if (context.started)
        {
            currentTarget = manager.GetCurrentTargetTransform();

            if (currentTarget != null)
            {
                isMoving = true;
                canCollect = true; // 이동 시작 시 수집 허용
                Debug.Log($"추적 시작: {currentTarget.name}");
            }
            else
            {
                Debug.Log("추적할 타겟이 없습니다.");
            }
        }
    }

    void Update()
    {
        if (isMoving && currentTarget != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                currentTarget.position,
                speed * Time.deltaTime
            );
        }
    }

    // 새로운 충돌 진입 시
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckAndProcessCollision(collision);
    }

    // 이미 겹쳐 있는 상태에서 A키를 눌렀을 때를 대비
    private void OnTriggerStay2D(Collider2D collision)
    {
        CheckAndProcessCollision(collision);
    }

    // 공통 충돌 처리 로직
    private void CheckAndProcessCollision(Collider2D collision)
    {
        // 1. 수집 가능한 상태(이동 중)인지 확인
        // 2. 충돌체가 Target 태그인지 확인
        if (canCollect && collision.CompareTag("Target"))
        {
            // 3. 충돌한 타겟이 현재 매니저가 지정한 목표가 맞는지 확인
            if (collision.transform == currentTarget)
            {
                ExecuteCollection(collision.gameObject);
            }
        }
    }

    private void ExecuteCollection(GameObject targetObject)
    {
        isMoving = false;
        canCollect = false; // [핵심] 먹자마자 즉시 잠금 (중복 충돌 방지)

        Debug.Log($"{targetObject.name} 수집 완료 및 상태 잠금");

        // 플레이어가 직접 타겟을 화면에서 지움 (비활성화)
        targetObject.SetActive(false);

        // 매니저에게 다음 타겟 준비 요청
        manager.TargetEaten();
    }
}