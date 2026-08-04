using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 라운드 진행을 총괄하는 매니저.
/// RoundData 배열을 Inspector에서 순서대로 등록하면 자동으로 라운드를 진행합니다.
///
/// [타이머 규칙]
/// - 제한 시간 내 처치 완료 & 남은 시간 > 5초 → 남은 시간을 5초로 줄여 다음 라운드 시작
/// - 제한 시간 내 처치 완료 & 남은 시간 ≤ 5초 → 남은 시간 그대로 다음 라운드 시작
/// - 제한 시간 초과 (적 생존)            → 살아있는 적 유지, 즉시 다음 라운드 적 추가 스폰 후 새 타이머 시작
/// </summary>
public class RoundManager : MonoBehaviour
{
    // ──────────────────────────────────────────
    //  이벤트 (UI 연결용)
    // ──────────────────────────────────────────
    public static event Action<int>        OnRoundStarted;       // 라운드 번호
    public static event Action<int>        OnRoundCleared;       // 라운드 번호
    public static event Action             OnAllRoundsCleared;
    public static event Action<int, int>   OnEnemyCountChanged;  // (남은 적, 전체 적)
    public static event Action<float, float> OnTimerChanged;     // (남은 시간, 전체 시간)
    public static event Action             OnRoundTimeOut;       // 시간 초과 알림

    // ──────────────────────────────────────────
    //  Inspector 설정
    // ──────────────────────────────────────────
    [Header("라운드 목록 (순서대로 진행)")]
    [Tooltip("Project에서 만든 RoundData 에셋을 순서대로 드래그하세요.")]
    public List<RoundData> rounds = new List<RoundData>();

    [Header("스폰 설정")]
    [Tooltip("플레이어 Transform (null이면 'Player' 태그로 자동 탐색)")]
    public Transform player;

    [Tooltip("플레이어 주변 스폰 반경")]
    public float spawnRadius = 8f;

    [Header("무한 반복 설정")]
    [Tooltip("마지막 라운드 클리어 후 처음부터 반복할지 여부")]
    public bool loopRounds = false;

    [Tooltip("반복 시 적 수 배율 (루프마다 누적 곱셈)")]
    public float loopScaleMultiplier = 1.2f;

    // ──────────────────────────────────────────
    //  내부 상태
    // ──────────────────────────────────────────
    private int   _currentRoundIndex = 0;
    private int   _loopCount         = 0;
    private int   _aliveEnemies      = 0;
    private int   _totalEnemies      = 0;
    private bool  _isRunning         = false;

    // 타이머
    private float _roundTimeRemaining = 0f;   // 현재 라운드 남은 시간
    private float _roundTimeDuration  = 0f;   // 현재 라운드 전체 시간
    private bool  _timerActive        = false;
    private bool  _roundCleared       = false; // 이번 라운드 적을 모두 처치했는지

    public int   CurrentRoundNumber   => _currentRoundIndex < rounds.Count
                                            ? rounds[_currentRoundIndex].roundNumber : -1;
    public bool  IsRunning            => _isRunning;
    public float TimeRemaining        => _roundTimeRemaining;

    // ──────────────────────────────────────────
    //  유니티 라이프사이클
    // ──────────────────────────────────────────
    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        EnemyBase.OnEnemyDied += HandleEnemyDied;
        StartGame();
    }

    private void OnDestroy()
    {
        EnemyBase.OnEnemyDied -= HandleEnemyDied;
    }

    private void Update()
    {
        if (!_timerActive) return;

        _roundTimeRemaining -= Time.deltaTime;

        // UI 갱신
        OnTimerChanged?.Invoke(Mathf.Max(0f, _roundTimeRemaining), _roundTimeDuration);

        if (_roundTimeRemaining <= 0f)
        {
            _timerActive = false;
            HandleTimeOut();
        }
    }

    // ──────────────────────────────────────────
    //  공개 API
    // ──────────────────────────────────────────

    /// <summary>첫 라운드부터 게임을 시작합니다.</summary>
    public void StartGame()
    {
        if (_isRunning) return;

        if (rounds == null || rounds.Count == 0)
        {
            Debug.LogError("[RoundManager] Rounds 목록이 비어 있습니다. RoundData 에셋을 Inspector에 연결하세요.");
            return;
        }

        _currentRoundIndex = 0;
        _loopCount         = 0;
        StartCoroutine(RunRound(rounds[_currentRoundIndex]));
    }

    /// <summary>현재 라운드를 강제 스킵하고 다음 라운드로 넘어갑니다.</summary>
    public void SkipToNextRound()
    {
        StopAllCoroutines();
        _timerActive = false;

        foreach (var enemy in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
            Destroy(enemy.gameObject);

        AdvanceRound();
    }

    // ──────────────────────────────────────────
    //  라운드 진행 코루틴
    // ──────────────────────────────────────────
    private IEnumerator RunRound(RoundData data)
    {
        _isRunning     = true;
        _roundCleared  = false;

        // 라운드 시작 딜레이
        if (data.startDelay > 0f)
            yield return new WaitForSeconds(data.startDelay);

        // 총 적 수 계산 (루프 배율 적용)
        float scale   = Mathf.Pow(loopScaleMultiplier, _loopCount);
        _totalEnemies = 0;
        foreach (var wave in data.enemyWaves)
            _totalEnemies += Mathf.RoundToInt(wave.count * scale);

        _aliveEnemies = 0;

        OnRoundStarted?.Invoke(data.roundNumber);
        Debug.Log($"[RoundManager] Round {data.roundNumber} START  (총 {_totalEnemies}마리)");

        // 타이머 시작
        if (data.roundDuration > 0f)
        {
            _roundTimeDuration  = data.roundDuration;
            _roundTimeRemaining = data.roundDuration;
            _timerActive        = true;
            OnTimerChanged?.Invoke(_roundTimeRemaining, _roundTimeDuration);
        }

        // 각 EnemyWave 스폰
        foreach (var wave in data.enemyWaves)
        {
            if (wave.enemyPrefab == null)
            {
                Debug.LogWarning("[RoundManager] enemyPrefab이 비어 있는 Wave가 있습니다.");
                continue;
            }

            int spawnCount = Mathf.RoundToInt(wave.count * scale);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemy(wave.enemyPrefab);

                if (wave.spawnInterval > 0f)
                    yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        // 모든 적이 처치될 때까지 대기 (타임아웃이 먼저 발생할 수도 있음)
        yield return new WaitUntil(() => _aliveEnemies <= 0 || !_timerActive);

        // 타이머가 꺼진 이유가 타임아웃이면 HandleTimeOut에서 처리 → 여기선 종료
        if (!_timerActive && !_roundCleared)
            yield break;

        // ── 라운드 클리어 처리 ──────────────────
        _timerActive   = false;
        _roundCleared  = true;

        Debug.Log($"[RoundManager] Round {data.roundNumber} CLEAR!  (남은 시간: {_roundTimeRemaining:F1}초)");
        OnRoundCleared?.Invoke(data.roundNumber);

        // 남은 시간 처리: 5초 초과면 5초로 단축, 5초 이하면 그대로
        float waitTime = (data.roundDuration > 0f)
            ? ((_roundTimeRemaining > 5f) ? 5f : _roundTimeRemaining)
            : data.restDuration;

        // UI에 남은 대기 시간 반영
        _roundTimeRemaining = waitTime;
        _roundTimeDuration  = waitTime;
        OnTimerChanged?.Invoke(_roundTimeRemaining, _roundTimeDuration);

        yield return new WaitForSeconds(waitTime);

        AdvanceRound();
    }

    // ──────────────────────────────────────────
    //  시간 초과 처리
    // ──────────────────────────────────────────
    private void HandleTimeOut()
    {
        Debug.Log($"[RoundManager] Round {CurrentRoundNumber} 시간 초과! 살아있는 적 {_aliveEnemies}마리 유지, 다음 라운드 추가 스폰");
        OnRoundTimeOut?.Invoke();

        // 현재 라운드 코루틴은 WaitUntil에서 이미 탈출 대기 중이므로
        // AdvanceRound를 직접 호출하면 안 됨 → 별도 코루틴으로 다음 라운드 실행
        StopAllCoroutines();
        AdvanceRound();
    }

    // ──────────────────────────────────────────
    //  라운드 전환
    // ──────────────────────────────────────────
    private void AdvanceRound()
    {
        _currentRoundIndex++;

        if (_currentRoundIndex < rounds.Count)
        {
            StartCoroutine(RunRound(rounds[_currentRoundIndex]));
        }
        else if (loopRounds && rounds.Count > 0)
        {
            _currentRoundIndex = 0;
            _loopCount++;
            Debug.Log($"[RoundManager] 루프 {_loopCount}회차 시작 (배율 x{Mathf.Pow(loopScaleMultiplier, _loopCount):F2})");
            StartCoroutine(RunRound(rounds[_currentRoundIndex]));
        }
        else
        {
            _isRunning = false;
            _timerActive = false;
            Debug.Log("[RoundManager] 모든 라운드 클리어!");
            OnAllRoundsCleared?.Invoke();
        }
    }

    // ──────────────────────────────────────────
    //  적 스폰
    // ──────────────────────────────────────────
    private void SpawnEnemy(GameObject prefab)
    {
        Vector3 spawnPos = GetSpawnPosition();
        Instantiate(prefab, spawnPos, Quaternion.identity);
        _aliveEnemies++;
        OnEnemyCountChanged?.Invoke(_aliveEnemies, _totalEnemies);
    }

    private Vector3 GetSpawnPosition()
    {
        if (player == null) return Vector3.zero;

        Vector2 rand = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
        return player.position + new Vector3(rand.x, rand.y, 0f);
    }

    // ──────────────────────────────────────────
    //  이벤트 핸들러
    // ──────────────────────────────────────────
    private void HandleEnemyDied(GameObject enemyObj)
    {
        _aliveEnemies = Mathf.Max(0, _aliveEnemies - 1);
        OnEnemyCountChanged?.Invoke(_aliveEnemies, _totalEnemies);
    }
}
