using UnityEngine;

public class FixedEnemy : EnemyBase
{
    private void Update()
    {
        if (targetPlayer == null) return;

        // 이동 없이 회전만 수행
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }
    }
}