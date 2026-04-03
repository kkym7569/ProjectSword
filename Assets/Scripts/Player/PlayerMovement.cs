using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public TargetManager manager;
    public float speed = 10.0f; // 조금 더 빠르게 설정

    [Header("BoxCast Settings")]
    public float slashThickness = 1.0f;

    private Transform currentTarget;
    private bool isMoving = false;

    private Vector2 dashStartPos;

    // [추가] 정밀한 도착 판정을 위한 변수
    public float stoppingDistance = 0.01f; // 이 거리 이하로 가까워지면 도착으로 간주

    public void OnMoveToPoint(InputAction.CallbackContext context)
    {
        // 이동 중이 아닐 때만 입력을 받도록 보완
        if (context.started && !isMoving)
        {
            currentTarget = manager.GetCurrentTargetTransform();

            if (currentTarget != null)
            {
                isMoving = true;
                dashStartPos = transform.position;
                Debug.Log($"추적 시작: {currentTarget.name}");
            }
        }
    }

    void Update()
    {
        if (isMoving && currentTarget != null)
        {
            // 목표(타겟 중심)를 향해 이동
            transform.position = Vector2.MoveTowards(
                transform.position,
                currentTarget.position,
                speed * Time.deltaTime
            );

            // [핵심] 이제 충돌체 체크 대신 거리로 도착을 판단합니다.
            // 플레이어 중심과 타겟 중심 사이의 거리를 계산
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);

            // 설정한 정지 거리 이하로 가까워졌다면
            if (distanceToTarget <= stoppingDistance)
            {
                // 강제로 완벽한 타겟 좌표로 맞춤 (오차 보정)
                transform.position = currentTarget.position;

                // 도착 처리 함수 실행
                ExecuteCollection(currentTarget.gameObject);
            }
        }
    }

    // 이제 OnTriggerEnter2D, OnTriggerStay2D는 도착 용도로 쓰지 않습니다.
    // (다른 용도가 없다면 주석 처리하거나 지우셔도 됩니다.)

    private void ExecuteCollection(GameObject targetObject)
    {
        isMoving = false;
        // canCollect 변수도 이제 필요 없습니다.

        Debug.Log($"{targetObject.name} 중심 도착 및 수집 완료");
        targetObject.SetActive(false);
        manager.TargetEaten();
    }

    // OnDrawGizmos 함수는 이전과 동일합니다.
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
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(Vector3.zero, boxSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }
}