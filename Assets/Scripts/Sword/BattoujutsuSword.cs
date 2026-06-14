using System.Collections;
using UnityEngine;

/// <summary>
/// 발도 검: 도착 즉시 자신 위치에서 반경 내 모든 적에게 회전 베기 데미지를 줍니다.
/// </summary>
public class BattoujutsuSword : SwordBehaviour
{
    public override void OnAfterAttackMove(Vector2 arrivalPos)
    {
        // MonoBehaviour가 아니므로 playerCombat을 통해 코루틴 실행
        playerCombat.StartCoroutine(SpinRoutine(arrivalPos));
    }

    private IEnumerator SpinRoutine(Vector2 center)
    {
        float timer    = 0f;
        float duration = data.spinDuration;

        // 이미 맞은 적 중복 방지
        System.Collections.Generic.HashSet<EnemyBase> alreadyHit
            = new System.Collections.Generic.HashSet<EnemyBase>();

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // 반경 내 적 전부 감지
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                center, data.spinRadius,
                LayerMask.GetMask("Enemy"));   // Enemy 레이어에 맞게 조정

            foreach (var col in hits)
            {
                EnemyBase enemy = col.GetComponent<EnemyBase>();
                if (enemy == null || alreadyHit.Contains(enemy)) continue;

                alreadyHit.Add(enemy);
                enemy.TakeDamage(data.spinDamage);
            }

            yield return null;
        }

        Debug.Log($"[BattoujutsuSword] 회전 베기 완료 — {alreadyHit.Count}명 적중");
    }
}
