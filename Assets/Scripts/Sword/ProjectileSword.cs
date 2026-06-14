using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 발사체 검: 대시 출발 직전, 출발 자리에서 가장 가까운 적 N개를 향해 구체를 발사합니다.
/// 본체는 기본 이동 베기와 동일하게 작동합니다.
/// </summary>
public class ProjectileSword : SwordBehaviour
{
    public override void OnBeforeAttackMove(Vector2 playerPos)
    {
        if (data.projectilePrefab == null)
        {
            Debug.LogWarning("[ProjectileSword] projectilePrefab이 TargetData에 설정되지 않았습니다.");
            return;
        }

        // 씬의 모든 적을 거리순으로 정렬
        EnemyBase[] allEnemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        if (allEnemies.Length == 0) return;

        List<EnemyBase> sorted = allEnemies
            .OrderBy(e => Vector2.Distance(playerPos, e.transform.position))
            .ToList();

        int count = Mathf.Min(data.projectileCount, sorted.Count);

        for (int i = 0; i < count; i++)
        {
            FireProjectile(playerPos, sorted[i].transform.position);
        }
    }

    private void FireProjectile(Vector2 from, Vector2 targetPos)
    {
        GameObject proj = Object.Instantiate(data.projectilePrefab, from, Quaternion.identity);

        SwordProjectile sp = proj.GetComponent<SwordProjectile>();
        if (sp != null)
        {
            sp.Launch(targetPos, data.projectileSpeed, data.projectileDamage);
        }
        else
        {
            // SwordProjectile 컴포넌트가 없으면 자동으로 붙여줌
            sp = proj.AddComponent<SwordProjectile>();
            sp.Launch(targetPos, data.projectileSpeed, data.projectileDamage);
        }
    }
}
