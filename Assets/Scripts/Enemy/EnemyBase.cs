using UnityEngine;
using System;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Common Stats")]
    public int maxHp = 100;
    protected int currentHp;
    protected Transform targetPlayer;

    [Header("Score")]
    [Tooltip("이 적을 처치했을 때 플레이어가 얻는 점수")]
    public int scoreValue = 10;

    /// <summary>
    /// 사망 판정 완료 여부. Die() 호출 즉시 true가 됩니다.
    /// 페이드아웃 애니메이션 중에도 true이므로
    /// 발사체 등 외부 스크립트가 이 값으로 생사를 판단합니다.
    /// </summary>
    public bool IsDead { get; private set; } = false;

    public static Action<GameObject> OnEnemyDied;
    public static Action<GameObject, int> OnEnemyDiedWithScore;

    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;

    protected virtual void Start()
    {
        currentHp = maxHp;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) targetPlayer = playerObj.transform;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public Color GetOriginalColor() => originalColor;

    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;   // 이미 사망 판정된 적은 추가 데미지 무시

        currentHp -= damage;

        if (spriteRenderer != null && gameObject.activeInHierarchy)
        {
            StopCoroutine("FlashRoutine");
            StartCoroutine("FlashRoutine");
        }

        if (currentHp <= 0) Die();
    }

    protected IEnumerator FlashRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    protected virtual void Die()
    {
        // ① 즉시 사망 판정 — 발사체/외부 스크립트가 IsDead로 확인
        IsDead = true;

        // ② 이벤트 발송 (RoundManager, PlayerScore)
        OnEnemyDied?.Invoke(gameObject);
        OnEnemyDiedWithScore?.Invoke(gameObject, scoreValue);

        // ③ 이동·공격 로직 즉시 정지
        this.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // ④ 페이드아웃 애니메이션 시작
        if (spriteRenderer != null)
        {
            StopCoroutine("FlashRoutine");
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float duration = 1.0f;
        float timer = 0f;
        Color startColor = originalColor; // 원래 색에서 시작

        // 즉시 페이드아웃 시작 (기존 0.5초 대기 제거)
        spriteRenderer.color = startColor;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
