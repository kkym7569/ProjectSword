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
    public int specialKillThreshold = 3; // 3명 이상 벨 때 연출 발동

    private PlayerMain mainScript;
    private float originalSlashWidth;
    private Coroutine fadeCoroutine;

    private bool isAttacking = false;
    private Vector2 startPos;
    private Vector2 currentPos;

    // 중복 타격 방지용 명부
    private HashSet<Collider2D> hitEnemiesThisSlash = new HashSet<Collider2D>();
    // 연출용 피해자 명단
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
        isAttacking = true;
        startPos = attackStart;
        currentPos = attackStart;

        hitEnemiesThisSlash.Clear();
        victimsThisSlash.Clear();

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (slashEffect != null)
        {
            slashEffect.widthMultiplier = originalSlashWidth;
            slashEffect.enabled = true;
            slashEffect.SetPosition(0, startPos);
            slashEffect.SetPosition(1, startPos);

            // 🌟 검기 이펙트 초기 색상 복구 (연출 후를 대비)
            slashEffect.startColor = Color.white;
            slashEffect.endColor = Color.white;
        }
    }

    private void HandleAttackUpdate(Vector2 attackStart, Vector2 attackCurrent)
    {
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

        // 🌟 [핵심] 3명 이상 베었을 때 특수 연출 호출 (플레이어 객체 포함)
        if (victimsThisSlash.Count >= specialKillThreshold)
        {
            if (HitEffectManager.Instance != null)
            {
                // 플레이어 본인(gameObject)과 검기(slashEffect)를 함께 전달할 수 있도록 설계
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