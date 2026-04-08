using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("추적할 대상")]
    public Transform target; // 플레이어 오브젝트를 연결할 칸

    [Header("카메라 설정")]
    [Range(1f, 10f)]
    public float followSpeed = 5f; // 카메라가 따라가는 속도 (숫자가 클수록 빠름)

    // 2D 게임에서 카메라는 항상 Z축으로 떨어져 있어야 화면이 보입니다.
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    // Update가 아닌 LateUpdate를 사용하는 것이 핵심입니다!
    void LateUpdate()
    {
        // 타겟이 없으면 에러가 나지 않도록 방지
        if (target == null) return;

        // 카메라가 최종적으로 가야 할 목표 위치 (플레이어 위치 + 오프셋)
        Vector3 targetPosition = target.position + offset;

        // Vector3.Lerp(현재 위치, 목표 위치, 속도) : 현재 위치에서 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}