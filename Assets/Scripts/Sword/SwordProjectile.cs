using UnityEngine;

/// <summary>
/// 발사체 구체. 지정된 적을 실시간으로 추적하며 닿으면 데미지를 줍니다.
/// 적의 IsDead가 true가 되는 순간 추적을 중단하고 그 자리에서 소멸합니다.
///
/// [프리팹 설정]
/// - Circle Collider 2D → Is Trigger: ON
/// - Rigidbody 2D → Gravity Scale: 0, Collision Detection: Continuous
/// </summary>
public class SwordProjectile : MonoBehaviour
{
    private EnemyBase _target;
    private float _speed;
    private int _damage;
    private bool _launched = false;

    public void Launch(EnemyBase target, float speed, int damage)
    {
        _target = target;
        _speed = speed;
        _damage = damage;
        _launched = true;
    }

    private void Update()
    {
        if (!_launched) return;

        // 적이 사망 판정되면 즉시 소멸 (페이드아웃 중인 시체 추적 안 함)
        if (_target == null || _target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        // 살아있는 적을 실시간 추적
        transform.position = Vector2.MoveTowards(
            transform.position,
            _target.transform.position,
            _speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || enemy.IsDead) return;

        enemy.TakeDamage(_damage);
        Destroy(gameObject);
    }
}
