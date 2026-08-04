using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    [Header("2D Chaser Settings")]
    public float moveSpeed = 3f;

    [Header("Spirit Targeting")]
    [SerializeField] private float targetRefreshInterval = 0.25f;

    private Transform targetSpirit;
    private float nextTargetRefreshTime;

    protected override void Start()
    {
        base.Start();
        FindNearestSpirit();
    }

    private void Update()
    {
        if (targetSpirit == null || Time.time >= nextTargetRefreshTime)
        {
            FindNearestSpirit();
        }

        Transform currentTarget = targetSpirit != null ? targetSpirit : targetPlayer;
        if (currentTarget == null) return;

        Vector2 direction = (currentTarget.position - transform.position).normalized;

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

    private void FindNearestSpirit()
    {
        nextTargetRefreshTime = Time.time + targetRefreshInterval;
        Spirit[] spirits = FindObjectsByType<Spirit>(FindObjectsSortMode.None);

        targetSpirit = null;
        float nearestDistanceSqr = float.MaxValue;

        foreach (Spirit spirit in spirits)
        {
            if (spirit == null || !spirit.gameObject.activeInHierarchy) continue;

            float distanceSqr = ((Vector2)spirit.transform.position
                - (Vector2)transform.position).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                targetSpirit = spirit.transform;
            }
        }
    }
}
