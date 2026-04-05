using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMain))] // 이제 PlayerMain과 항상 같이 붙어있어야 함
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float slashThickness = 1.0f; // 타격 및 기즈모 두께
    public LayerMask enemyLayer;        // 적 판정용 레이어

    [Header("Visual Effects (Line Renderer)")]
    public LineRenderer slashEffect;    // 이동에서 가져온 검기 효과
    public float slashFadeTime = 0.3f;

    private PlayerMain mainScript;

    private float originalSlashWidth;
    private Coroutine fadeCoroutine;

    // 판정 및 기즈모용 상태 변수
    private bool isAttacking = false;
    private Vector2 startPos;
    private Vector2 currentPos;

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
        // 🌟 메인 스크립트의 이동 이벤트 구독
        mainScript.OnAttackMoveStarted += HandleAttackStart;
        mainScript.OnAttackMoveUpdated += HandleAttackUpdate;
        mainScript.OnAttackMoveEnded += HandleAttackEnd;
    }

    private void OnDisable()
    {
        // 🌟 구독 해제
        mainScript.OnAttackMoveStarted -= HandleAttackStart;
        mainScript.OnAttackMoveUpdated -= HandleAttackUpdate;
        mainScript.OnAttackMoveEnded -= HandleAttackEnd;
    }

    private void HandleAttackStart(Vector2 attackStart)
    {
        isAttacking = true;
        startPos = attackStart;
        currentPos = attackStart;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // 이동 시작 시 검기(Line Renderer) 켜기
        if (slashEffect != null)
        {
            slashEffect.widthMultiplier = originalSlashWidth;
            slashEffect.enabled = true;
            slashEffect.SetPosition(0, startPos);
            slashEffect.SetPosition(1, startPos);
        }
    }

    private void HandleAttackUpdate(Vector2 attackStart, Vector2 attackCurrent)
    {
        currentPos = attackCurrent;

        // 1. 라인 렌더러 시각 효과 실시간 업데이트
        if (slashEffect != null)
        {
            slashEffect.SetPosition(0, attackStart);
            slashEffect.SetPosition(1, attackCurrent);
        }

        // 2. 실시간 타격 판정
        CheckSlashAttack(attackStart, attackCurrent);
    }

    private void HandleAttackEnd()
    {
        isAttacking = false;

        // 이동이 끝나면 검기가 서서히 사라지는 연출 시작
        if (slashEffect != null)
        {
            fadeCoroutine = StartCoroutine(FadeOutSlash());
        }
    }

    // [핵심] 실제 적을 베는 로직
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
            // 투사체 발사나 특수 공격 연계는 향후 이 부분에 추가
            Debug.Log($"<color=red>{hit.collider.name}</color> 타격 판정!");
        }
    }

    // 검기 페이드아웃 코루틴
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

    // 판정 범위 기즈모
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

            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(Vector3.zero, boxSize);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }
}