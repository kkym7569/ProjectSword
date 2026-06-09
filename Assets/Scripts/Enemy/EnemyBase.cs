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

    // 기존 이벤트 유지 (RoundManager 호환)
    public static Action<GameObject> OnEnemyDied;

    // 🌟 점수 전달 이벤트: (처치된 적 GameObject, 획득 점수)
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
        // 기존 이벤트 (RoundManager용)
        OnEnemyDied?.Invoke(gameObject);

        // 🌟 점수 포함 이벤트 (PlayerScore용)
        OnEnemyDiedWithScore?.Invoke(gameObject, scoreValue);

        this.enabled = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

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
        yield return new WaitForSecondsRealtime(0.5f);

        float duration = 1.0f;
        float timer = 0f;
        Color startColor = spriteRenderer.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
