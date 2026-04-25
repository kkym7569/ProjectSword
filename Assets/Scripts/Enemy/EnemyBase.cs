using UnityEngine;
using System;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Common Stats")]
    public int maxHp = 100;
    protected int currentHp;
    protected Transform targetPlayer;

    public static Action<GameObject> OnEnemyDied;

    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;

    protected virtual void Start()
    {
        currentHp = maxHp;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) targetPlayer = playerObj.transform;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public Color GetOriginalColor() => originalColor;

    public virtual void TakeDamage(int damage)
    {
        currentHp -= damage;

        if (spriteRenderer != null && gameObject.activeInHierarchy)
        {
            // 이전에 돌던 피격 깜빡임만 멈추고 새로 시작
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
        // 1. 논리적 즉시 사망 처리
        OnEnemyDied?.Invoke(gameObject);

        // 2. 물리 및 추적 로직 즉시 정지
        this.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 3. 사망 연출 시작
        if (spriteRenderer != null)
        {
            // 🌟 중요: StopAllCoroutines를 쓰지 않고 피격 깜빡임만 멈춤
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
        // 🌟 일섬 연출(HitEffectManager)이 끝날 때까지 실제 시간 기준으로 대기
        // 연출 시간(0.4초)보다 조금 더 길게 대기하여 씹힘 방지
        yield return new WaitForSecondsRealtime(0.5f);

        float duration = 1.0f; // 서서히 사라지는 시간
        float timer = 0f;

        // 연출 종료 후 현재 색상(originalColor일 가능성이 높음)에서 시작
        Color startColor = spriteRenderer.color;

        while (timer < duration)
        {
            timer += Time.deltaTime; // 연출 종료 후이므로 정상 작동
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}