using System;
using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Target Data (Scriptable Object)")]
    public TargetData myData;

    [Header("Position Settings")]
    public float rangeX = 8f, rangeY = 4.5f;
    public float minDistance = 3.0f;

    [Header("Throw Settings")]
    public float throwSpeed = 25f;
    public float spinSpeed  = 1500f;

    public bool IsReady { get; private set; } = false;
    public event Action OnLanded;

    private Transform    playerTransform;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // ★ Awake에서 플레이어를 찾아둠 (Start보다 먼저 실행되므로 안전)
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) playerTransform = p.transform;
    }

    public void InitData(TargetData data)
    {
        myData = data;
        if (sr != null && myData != null)
            sr.color = myData.trailColor;

        gameObject.name = $"Target_{myData.targetName}";
    }

    public void Relocate()
    {
        // ★ playerTransform이 없으면 다시 찾기 시도
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (playerTransform == null)
        {
            Debug.LogError($"[Target] {gameObject.name} — Player 태그 오브젝트를 찾지 못했습니다!");
            return;
        }

        IsReady = false;
        StopAllCoroutines();
        StartCoroutine(ThrowRoutine(GetSpawnPosition()));
    }

    private Vector2 GetSpawnPosition()
    {
        Vector2 pos;
        int     safety = 0;

        do
        {
            pos = new Vector2(
                UnityEngine.Random.Range(-rangeX, rangeX),
                UnityEngine.Random.Range(-rangeY, rangeY));

            Collider2D hit = Physics2D.OverlapCircle(pos, minDistance, LayerMask.GetMask("Target"));
            bool overlap   = hit != null && hit.gameObject != gameObject;

            if (!overlap && Vector2.Distance(pos, playerTransform.position) >= minDistance)
                break;

        } while (++safety < 100);

        return pos;
    }

    private IEnumerator ThrowRoutine(Vector2 destination)
    {
        transform.position = playerTransform.position;

        while (Vector2.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, destination, throwSpeed * Time.deltaTime);
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        transform.rotation = Quaternion.identity;
        IsReady = true;

        Debug.Log($"[Target] {gameObject.name} 착지 완료 → OnLanded 발송");
        OnLanded?.Invoke();
    }
}
