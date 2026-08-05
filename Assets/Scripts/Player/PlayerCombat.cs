using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float slashThickness = 1.0f;
    public LayerMask enemyLayer;
    public int attackDamage = 20;

    [Header("Visual Effects")]
    public LineRenderer slashEffect;
    public float slashFadeTime = 0.3f;

    [Header("Special Kill Setting")]
    public int specialKillThreshold = 3; // 3ëª??´ìƒ ë²????°ì¶œ ë°œë™

    private PlayerMain mainScript;
    private float originalSlashWidth;
    private Coroutine fadeCoroutine;

    private bool isAttacking = false;
    private bool hasSwordForCurrentMove;
    private Vector2 startPos;
    private Vector2 currentPos;

    // ì¤‘ë³µ ?€ê²?ë°©ì???ëª…ë?
    private HashSet<Collider2D> hitEnemiesThisSlash = new HashSet<Collider2D>();
    // ?°ì¶œ???¼í•´??ëª…ë‹¨
    private List<EnemyBase> victimsThisSlash = new List<EnemyBase>();

    private void Awake()
    {
        mainScript = GetComponent<PlayerMain>();
    }

    private void Start()
    {
        if (slashEffect != null)
        {
            originalSlashWidth = slashEffect.widthMultiplier;
            slashEffect.enabled = false;
        }
    }

    private void OnEnable()
    {
        mainScript.OnAttackMoveStarted += HandleAttackStart;
        mainScript.OnAttackMoveUpdated += HandleAttackUpdate;
        mainScript.OnAttackMoveEnded += HandleAttackEnd;
    }

    private void OnDisable()
    {
        mainScript.OnAttackMoveStarted -= HandleAttackStart;
        mainScript.OnAttackMoveUpdated -= HandleAttackUpdate;
        mainScript.OnAttackMoveEnded -= HandleAttackEnd;
    }

    private void HandleAttackStart(Vector2 attackStart)
    {
        hasSwordForCurrentMove = mainScript.manager != null
            && mainScript.manager.HasHeldTarget;
        isAttacking = hasSwordForCurrentMove;
        startPos = attackStart;
        currentPos = attackStart;

        hitEnemiesThisSlash.Clear();
        victimsThisSlash.Clear();

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (!hasSwordForCurrentMove)
        {
            if (slashEffect != null) slashEffect.enabled = false;
            return;
        }

        if (slashEffect != null)
        {
            slashEffect.widthMultiplier = originalSlashWidth;
            slashEffect.enabled = true;
            slashEffect.SetPosition(0, startPos);
            slashEffect.SetPosition(1, startPos);

            // ?ŒŸ ê²€ê¸??´í™??ì´ˆê¸° ?‰ìƒ ë³µêµ¬ (?°ì¶œ ?„ë? ?€ë¹?
            slashEffect.startColor = Color.white;
            slashEffect.endColor = Color.white;
        }
    }

    private void HandleAttackUpdate(Vector2 attackStart, Vector2 attackCurrent)
    {
        if (!hasSwordForCurrentMove) return;

        currentPos = attackCurrent;

        if (slashEffect != null)
        {
            slashEffect.SetPosition(0, attackStart);
            slashEffect.SetPosition(1, attackCurrent);
        }

        CheckSlashAttack(attackStart, attackCurrent);
    }

    private void HandleAttackEnd()
    {
        isAttacking = false;

        if (!hasSwordForCurrentMove)
        {
            hasSwordForCurrentMove = false;
            return;
        }

        hasSwordForCurrentMove = false;

        // ?ŒŸ [?µì‹¬] 3ëª??´ìƒ ë² ì—ˆ?????¹ìˆ˜ ?°ì¶œ ?¸ì¶œ (?Œë ˆ?´ì–´ ê°ì²´ ?¬í•¨)
        if (victimsThisSlash.Count >= specialKillThreshold)
        {
            if (HitEffectManager.Instance != null)
            {
                // ?Œë ˆ?´ì–´ ë³¸ì¸(gameObject)ê³?ê²€ê¸?slashEffect)ë¥??¨ê»˜ ?„ë‹¬?????ˆë„ë¡??¤ê³„
                HitEffectManager.Instance.PlaySpecialKillEffect(victimsThisSlash, gameObject, slashEffect);
            }
        }

        if (slashEffect != null)
        {
            fadeCoroutine = StartCoroutine(FadeOutSlash());
        }
    }

    private void CheckSlashAttack(Vector2 start, Vector2 current)
    {
        float distance = Vector2.Distance(start, current);
        if (distance < 0.01f) return;

        Vector2 direction = (current - start).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector2 boxSize = new Vector2(distance, slashThickness);
        Vector2 centerPos = start + (direction * (distance / 2));

        RaycastHit2D[] hits = Physics2D.BoxCastAll(centerPos, boxSize, angle, Vector2.zero, 0f, enemyLayer);

        foreach (var hit in hits)
        {
            if (hitEnemiesThisSlash.Contains(hit.collider)) continue;
            hitEnemiesThisSlash.Add(hit.collider);

            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                victimsThisSlash.Add(enemy);
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    private IEnumerator FadeOutSlash()
    {
        float timer = 0f;
        while (timer < slashFadeTime)
        {
            timer += Time.deltaTime;
            slashEffect.widthMultiplier = Mathf.Lerp(originalSlashWidth, 0f, timer / slashFadeTime);
            yield return null;
        }
        slashEffect.enabled = false;
    }

    private void OnDrawGizmos()
    {
        if (isAttacking)
        {
            float distance = Vector2.Distance(startPos, currentPos);
            if (distance < 0.01f) return;

            Vector2 direction = (currentPos - startPos).normalized;
            Vector2 centerPosition = startPos + (direction * (distance / 2));
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Vector2 boxSize = new Vector2(distance, slashThickness);

            Matrix4x4 rotationMatrix = Matrix4x4.TRS(centerPosition, Quaternion.Euler(0, 0, angle), Vector3.one);
            Gizmos.matrix = rotationMatrix;

            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(Vector3.zero, boxSize);
        }
    }
}