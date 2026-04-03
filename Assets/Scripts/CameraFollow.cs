using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 추적할 캐릭터
    public float smoothSpeed = 0.125f; // 부드러움 정도
    public Vector3 offset; // 카메라와 캐릭터 사이의 간격 (보통 Z값은 -10)

    void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;
        // 현재 위치에서 목표 위치로 부드럽게 이동 (Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}