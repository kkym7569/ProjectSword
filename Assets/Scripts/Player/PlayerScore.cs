using UnityEngine;
using System;

/// <summary>
/// 플레이어의 점수를 관리합니다.
/// EnemyBase.OnEnemyDiedWithScore 이벤트를 구독해서 점수를 자동으로 누적합니다.
/// </summary>
[RequireComponent(typeof(PlayerMain))]
public class PlayerScore : MonoBehaviour
{
    // ──────────────────────────────────────────
    //  이벤트
    // ──────────────────────────────────────────

    /// <summary>점수가 변경될 때마다 발생. (현재 총점, 이번에 획득한 점수)</summary>
    public event Action<int, int> OnScoreChanged;

    // ──────────────────────────────────────────
    //  상태
    // ──────────────────────────────────────────
    [Header("Score (Read Only)")]
    [SerializeField] private int _totalScore = 0;
    public int TotalScore => _totalScore;

    // ──────────────────────────────────────────
    //  라이프사이클
    // ──────────────────────────────────────────
    private void OnEnable()
    {
        EnemyBase.OnEnemyDiedWithScore += HandleEnemyDied;
    }

    private void OnDisable()
    {
        EnemyBase.OnEnemyDiedWithScore -= HandleEnemyDied;
    }

    // ──────────────────────────────────────────
    //  이벤트 핸들러
    // ──────────────────────────────────────────
    private void HandleEnemyDied(GameObject enemyObj, int score)
    {
        if (score <= 0) return;

        _totalScore += score;
        OnScoreChanged?.Invoke(_totalScore, score);

        Debug.Log($"[PlayerScore] +{score}점 획득! 총점: {_totalScore}");
    }

    /// <summary>점수를 0으로 초기화합니다.</summary>
    public void ResetScore()
    {
        _totalScore = 0;
        OnScoreChanged?.Invoke(_totalScore, 0);
    }
}
