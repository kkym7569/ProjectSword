using System;
using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Target Data (Scriptable Object)")]
    public TargetData myData; // 현재 검이 가지고 있는 데이터

    [Header("Position Settings")]
    public float rangeX = 8f, rangeY = 4.5f;
    public float minDistance = 3.0f; // 다른 검이나 플레이어와 유지할 최소 거리

    [Header("Throw Settings")]
    public float throwSpeed = 25f;   // 검이 날아가는 속도
    public float spinSpeed = 1500f;  // 날아갈 때 팽이처럼 도는 속도

    // [상태] 검이 땅에 꽂혀서 대시(공격) 가능한 상태인지 확인하는 변수
    public bool IsReady { get; private set; } = false;

    // [이벤트] 땅에 꽂혔을 때 매니저에게 "나 도착했어!"라고 보낼 신호
    public event Action OnLanded;

    private Transform playerTransform;
    private SpriteRenderer sr;

    void Awake()
    {
        // 렌더러 컴포넌트를 미리 찾아둡니다.
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    // [핵심 추가] 매니저가 이 검을 새로 생성할 때 데이터를 주입해주는 함수
    public void InitData(TargetData data)
    {
        myData = data;
        if (sr != null && myData != null)
        {
            // 검의 색상을 SO 데이터에 설정된 색상으로 즉시 변경
            sr.color = myData.trailColor;
        }

        // 하이어라키 창에서 보기 편하도록 이름 변경
        gameObject.name = $"Target_{myData.targetName}";
    }

    public void Relocate()
    {
        if (playerTransform == null) return;

        // 위치 재배치(비행)를 시작하므로 '준비 안 됨' 상태로 변경
        IsReady = false;

        Vector2 targetPos; // 도착해야 할 목적지
        int safetyBreak = 0;
        bool isOverlapping;

        // --- 1. 목적지 좌표 계산 ---
        do
        {
            // 랜덤 좌표 뽑기 (UnityEngine 명시하여 에러 방지)
            targetPos = new Vector2(UnityEngine.Random.Range(-rangeX, rangeX), UnityEngine.Random.Range(-rangeY, rangeY));

            // 다른 타겟과 겹치는지 확인
            Collider2D hit = Physics2D.OverlapCircle(targetPos, minDistance, LayerMask.GetMask("Target"));
            isOverlapping = (hit != null && hit.gameObject != this.gameObject);

            safetyBreak++;
            if (safetyBreak > 100) break; // 무한 루프 방지

            // 플레이어와 너무 가깝거나 다른 타겟과 겹치면 다시 뽑기
        } while (Vector2.Distance(targetPos, playerTransform.position) < minDistance || isOverlapping);

        // --- 2. 검 투척 연출 시작 ---
        StartCoroutine(ThrowRoutine(targetPos));
    }

    // 플레이어 위치에서 목적지까지 부드럽게 날아가는 코루틴
    private IEnumerator ThrowRoutine(Vector2 destination)
    {
        // 출발선을 플레이어의 현재 위치로 맞춤
        transform.position = playerTransform.position;

        // 목적지에 도달할 때까지 반복
        while (Vector2.Distance(transform.position, destination) > 0.01f)
        {
            // MoveTowards를 사용해 일정한 속도로 목적지를 향해 날아감
            transform.position = Vector2.MoveTowards(transform.position, destination, throwSpeed * Time.deltaTime);

            // 날아가는 동안 역동적으로 회전하는 연출
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

            yield return null;
        }

        // 오차 보정: 목적지에 정확히 안착
        transform.position = destination;

        // 땅에 꽂혔을 때 검이 똑바로 서 있도록 회전 초기화
        transform.rotation = Quaternion.identity;

        // 목적지에 무사히 안착했으므로 대시 준비 완료 상태로 변경!
        IsReady = true;

        // 매니저에게 "나 무사히 꽂혔다!" 하고 신호(이벤트) 발송
        OnLanded?.Invoke();
    }
}