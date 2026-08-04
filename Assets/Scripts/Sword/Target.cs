using System;
using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Target Data (Scriptable Object)")]
    public TargetData myData; // ?„ì¬ ê²€??ê°€ì§€ê³??ˆëŠ” ?°ì´??

    [Header("Position Settings")]
    public float rangeX = 8f, rangeY = 4.5f;
    public float minDistance = 3.0f; // ?¤ë¥¸ ê²€?´ë‚˜ ?Œë ˆ?´ì–´?€ ? ì???ìµœì†Œ ê±°ë¦¬

    [Header("Throw Settings")]
    public float throwSpeed = 25f;   // ê²€??? ì•„ê°€???ë„
    public float spinSpeed = 1500f;
    public float directionThrowDistance = 6f;  // ? ì•„ê°????½ì´ì²˜ëŸ¼ ?„ëŠ” ?ë„

    // [?íƒœ] ê²€???…ì— ê½‚í????€??ê³µê²©) ê°€?¥í•œ ?íƒœ?¸ì? ?•ì¸?˜ëŠ” ë³€??
    public bool IsReady { get; private set; } = false;

    // [?´ë²¤?? ?…ì— ê½‚í˜”????ë§¤ë‹ˆ?€?ê²Œ "???„ì°©?ˆì–´!"?¼ê³  ë³´ë‚¼ ? í˜¸
    public event Action OnLanded;

    private Transform playerTransform;
    private SpriteRenderer sr;

    void Awake()
    {
        // ?Œë”??ì»´í¬?ŒíŠ¸ë¥?ë¯¸ë¦¬ ì°¾ì•„?¡ë‹ˆ??
        sr = GetComponent<SpriteRenderer>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    // [?µì‹¬ ì¶”ê?] ë§¤ë‹ˆ?€ê°€ ??ê²€???ˆë¡œ ?ì„±?????°ì´?°ë? ì£¼ì…?´ì£¼???¨ìˆ˜
    public void InitData(TargetData data)
    {
        myData = data;
        if (sr != null && myData != null)
        {
            // ê²€???‰ìƒ??SO ?°ì´?°ì— ?¤ì •???‰ìƒ?¼ë¡œ ì¦‰ì‹œ ë³€ê²?
            sr.color = myData.trailColor;
        }

        // ?˜ì´?´ë¼??ì°½ì—??ë³´ê¸° ?¸í•˜?„ë¡ ?´ë¦„ ë³€ê²?
        gameObject.name = $"Target_{myData.targetName}";
    }

    public void Relocate()
    {
        if (!TryFindPlayer()) return;

        // ?„ì¹˜ ?¬ë°°ì¹?ë¹„í–‰)ë¥??œì‘?˜ë?ë¡?'ì¤€ë¹????? ?íƒœë¡?ë³€ê²?
        IsReady = false;

        Vector2 targetPos; // ?„ì°©?´ì•¼ ??ëª©ì ì§€
        int safetyBreak = 0;
        bool isOverlapping;

        // --- 1. ëª©ì ì§€ ì¢Œí‘œ ê³„ì‚° ---
        do
        {
            // ?œë¤ ì¢Œí‘œ ë½‘ê¸° (UnityEngine ëª…ì‹œ?˜ì—¬ ?ëŸ¬ ë°©ì?)
            targetPos = new Vector2(UnityEngine.Random.Range(-rangeX, rangeX), UnityEngine.Random.Range(-rangeY, rangeY));

            // ?¤ë¥¸ ?€ê²Ÿê³¼ ê²¹ì¹˜?”ì? ?•ì¸
            Collider2D hit = Physics2D.OverlapCircle(targetPos, minDistance, LayerMask.GetMask("Target"));
            isOverlapping = (hit != null && hit.gameObject != this.gameObject);

            safetyBreak++;
            if (safetyBreak > 100) break; // ë¬´í•œ ë£¨í”„ ë°©ì?

            // ?Œë ˆ?´ì–´?€ ?ˆë¬´ ê°€ê¹ê±°???¤ë¥¸ ?€ê²Ÿê³¼ ê²¹ì¹˜ë©??¤ì‹œ ë½‘ê¸°
        } while (Vector2.Distance(targetPos, playerTransform.position) < minDistance || isOverlapping);

        // --- 2. ê²€ ?¬ì²™ ?°ì¶œ ?œì‘ ---
        StartCoroutine(ThrowRoutine(targetPos));
    }

    // ?Œë ˆ?´ì–´ ?„ì¹˜?ì„œ ëª©ì ì§€ê¹Œì? ë¶€?œëŸ½ê²?? ì•„ê°€??ì½”ë£¨??
    public void Relocate(Vector2 direction)
    {
        if (!TryFindPlayer() || direction.sqrMagnitude <= 0f) return;

        IsReady = false;
        Vector2 destination = (Vector2)playerTransform.position
            + direction.normalized * directionThrowDistance;

        StartCoroutine(ThrowRoutine(destination));
    }
    private bool TryFindPlayer()
    {
        if (playerTransform != null) return true;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        return playerTransform != null;
    }

    private IEnumerator ThrowRoutine(Vector2 destination)
    {
        // ì¶œë°œ? ì„ ?Œë ˆ?´ì–´???„ì¬ ?„ì¹˜ë¡?ë§ì¶¤
        transform.position = playerTransform.position;

        // ëª©ì ì§€???„ë‹¬???Œê¹Œì§€ ë°˜ë³µ
        while (Vector2.Distance(transform.position, destination) > 0.01f)
        {
            // MoveTowardsë¥??¬ìš©???¼ì •???ë„ë¡?ëª©ì ì§€ë¥??¥í•´ ? ì•„ê°?
            transform.position = Vector2.MoveTowards(transform.position, destination, throwSpeed * Time.deltaTime);

            // ? ì•„ê°€???™ì•ˆ ??™?ìœ¼ë¡??Œì „?˜ëŠ” ?°ì¶œ
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

            yield return null;
        }

        // ?¤ì°¨ ë³´ì •: ëª©ì ì§€???•í™•???ˆì°©
        transform.position = destination;

        // ?…ì— ê½‚í˜”????ê²€???‘ë°”ë¡????ˆë„ë¡??Œì „ ì´ˆê¸°??
        transform.rotation = Quaternion.identity;

        // ëª©ì ì§€??ë¬´ì‚¬???ˆì°©?ˆìœ¼ë¯€ë¡??€??ì¤€ë¹??„ë£Œ ?íƒœë¡?ë³€ê²?
        IsReady = true;

        // ë§¤ë‹ˆ?€?ê²Œ "??ë¬´ì‚¬??ê½‚í˜”??" ?˜ê³  ? í˜¸(?´ë²¤?? ë°œì†¡
        OnLanded?.Invoke();
    }
}