using System.Linq;
using UnityEngine;

/// <summary>
/// 발사체 검: 이동공격 출발 직전, 플레이어 주변 4방향(상/하/좌/우)에서
/// 발사체를 소환합니다. 각 발사체는 가장 가까운 적을 자동추적합니다.
/// </summary>
public class ProjectileSword : SwordBehaviour
{
    // 4방향 오프셋 (상, 하, 좌, 우)
    private static readonly Vector2[] _directions = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
    };

    public override void OnBeforeAttackMove(Vector2 playerPos)
    {
        if (data.projectilePrefab == null)
        {
            Debug.LogWarning("[ProjectileSword] TargetData에 projectilePrefab이 없습니다.");
            return;
        }

        EnemyBase[] allEnemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        if (allEnemies.Length == 0)
        {
            Debug.Log("[ProjectileSword] 적이 없어 발사체를 생성하지 않습니다.");
            return;
        }

        int count = Mathf.Min(data.projectileCount, _directions.Length); // 최대 4개

        for (int i = 0; i < count; i++)
        {
            // 4방향 위치에서 소환
            Vector2 spawnPos = playerPos + _directions[i] * data.spawnOffset;

            // 각 발사체마다 가장 가까운 적 배정
            EnemyBase target = allEnemies
                .OrderBy(e => Vector2.Distance(spawnPos, e.transform.position))
                .First();

            GameObject proj = Object.Instantiate(
                data.projectilePrefab, spawnPos, Quaternion.identity);

            SwordProjectile sp = proj.GetComponent<SwordProjectile>();
            if (sp == null) sp = proj.AddComponent<SwordProjectile>();

            sp.Launch(target, data.projectileSpeed, data.projectileDamage);

            Debug.Log($"[ProjectileSword] 발사체 {i + 1} ({_directions[i]}) → {target.gameObject.name}");
        }
    }
}
