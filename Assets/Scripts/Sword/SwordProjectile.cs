using UnityEngine;

/// <summary>
/// 발사체 검이 쏘는 구체. 목표 지점을 향해 날아가 적에게 데미지를 줍니다.
/// 구체 프리팹에 Collider2D(IsTrigger=true)가 있어야 합니다.
/// </summary>
public class SwordProjectile : MonoBehaviour
{
    private Vector2  _targetPos;
    private float    _speed;
    private int      _damage;
    private bool     _launched = false;

    /// <summary>ProjectileSword가 생성 직후 호출합니다.</summary>
    public void Launch(Vector2 targetPos, float speed, int damage)
    {
        _targetPos = targetPos;
        _speed     = speed;
        _damage    = damage;
        _launched  = true;
    }

    private void Update()
    {
        if (!_launched) return;

        transform.position = Vector2.MoveTowards(
            transform.position, _targetPos, _speed * Time.deltaTime);

        // 목표 지점 도달 시 소멸
        if (Vector2.Distance(transform.position, _targetPos) < 0.05f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null) return;

        enemy.TakeDamage(_damage);
        Destroy(gameObject);
    }
}
