using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    [Header("2D Chaser Settings")]
    public float moveSpeed = 3f;

    private void Update()
    {
        // 타겟(플레이어)이 없으면 이동하지 않음
        if (targetPlayer == null) return;

        // 1. 플레이어를 향한 방향 계산 (2D이므로 Z축은 무시되고 X, Y만 계산됨)
        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        // 2. 회전 코드는 전부 삭제! 오직 플레이어를 향해 직진
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        // 3. [선택/추천] 애니메이션용 좌우 반전 로직
        // 스프라이트가 기본적으로 오른쪽을 보고 그려졌다고 가정합니다.
        if (direction.x > 0)
        {
            // 오른쪽으로 이동 중: 원래 방향 유지
            transform.localScale = new Vector3(1, 1, 1);

            // 나중에 애니메이터를 쓴다면: animator.SetFloat("DirX", 1f);
        }
        else if (direction.x < 0)
        {
            // 왼쪽으로 이동 중: X축 스케일을 -1로 만들어 좌우 반전시킴
            transform.localScale = new Vector3(-1, 1, 1);

            // 나중에 애니메이터를 쓴다면: animator.SetFloat("DirX", -1f);
        }
    }
}