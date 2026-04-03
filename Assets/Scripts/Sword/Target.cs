using UnityEngine;

public class Target : MonoBehaviour
{
    public float rangeX = 8f, rangeY = 4.5f;
    public float minDistance = 3.0f; // 다른 물체와 유지해야 할 최소 거리
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    public void Relocate()
    {
        if (playerTransform == null) return;

        Vector2 newPos;
        int safetyBreak = 0;
        bool isOverlapping;

        do
        {
            // 1. 랜덤 좌표 생성
            newPos = new Vector2(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY));

            // 2. 다른 타겟과 겹치는지 체크 (반지름 minDistance 내에 Target 레이어가 있는지)
            // 본인을 제외하고 체크하기 위해 OverlapCircle 사용
            Collider2D hit = Physics2D.OverlapCircle(newPos, minDistance, LayerMask.GetMask("Target"));

            // 만약 감지된게 있다면(null이 아니면) 그리고 그게 내가 아니라면 겹친 것으로 간주
            isOverlapping = (hit != null && hit.gameObject != gameObject);

            safetyBreak++;
            if (safetyBreak > 100) break; // 무한 루프 방지

            // 조건: 플레이어와 너무 가깝거나 OR 다른 타겟과 겹치면 다시 뽑기
        } while (Vector2.Distance(newPos, playerTransform.position) < minDistance || isOverlapping);

        transform.position = newPos;
   }
}