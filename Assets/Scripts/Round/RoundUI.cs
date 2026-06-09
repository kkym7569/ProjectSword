using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// RoundManager 이벤트를 받아 UI를 갱신하는 스크립트.
/// TextMeshPro를 사용합니다.
/// </summary>
public class RoundUI : MonoBehaviour
{
    [Header("UI 참조")]
    public TextMeshProUGUI roundLabel;       // "Round 3"
    public TextMeshProUGUI enemyCountLabel;  // "남은 적: 12 / 30"
    public TextMeshProUGUI statusLabel;      // "전투 중!" 등
    public TextMeshProUGUI timerLabel;       // "00:45" 남은 시간
    public GameObject allClearPanel;

    [Header("타이머 색상")]
    [Tooltip("남은 시간이 넉넉할 때 색상")]
    public Color timerNormalColor  = Color.white;
    [Tooltip("남은 시간이 10초 이하일 때 경고 색상")]
    public Color timerWarningColor = Color.red;

    [Header("라운드 매니저")]
    public RoundManager roundManager;

    // ──────────────────────────────────────────
    private void OnEnable()
    {
        RoundManager.OnRoundStarted     += HandleRoundStarted;
        RoundManager.OnRoundCleared     += HandleRoundCleared;
        RoundManager.OnAllRoundsCleared += HandleAllCleared;
        RoundManager.OnEnemyCountChanged+= HandleEnemyCount;
        RoundManager.OnTimerChanged     += HandleTimer;
        RoundManager.OnRoundTimeOut     += HandleTimeOut;
    }

    private void OnDisable()
    {
        RoundManager.OnRoundStarted     -= HandleRoundStarted;
        RoundManager.OnRoundCleared     -= HandleRoundCleared;
        RoundManager.OnAllRoundsCleared -= HandleAllCleared;
        RoundManager.OnEnemyCountChanged-= HandleEnemyCount;
        RoundManager.OnTimerChanged     -= HandleTimer;
        RoundManager.OnRoundTimeOut     -= HandleTimeOut;
    }

    private void Start()
    {
        if (allClearPanel != null) allClearPanel.SetActive(false);
        SetStatus("게임 시작 대기 중...");
        SetTimerText(0f);
    }

    // ──────────────────────────────────────────
    //  이벤트 핸들러
    // ──────────────────────────────────────────
    private void HandleRoundStarted(int roundNumber)
    {
        if (roundLabel != null) roundLabel.text = $"Round {roundNumber}";
        SetStatus("전투 중!");
    }

    private void HandleRoundCleared(int roundNumber)
    {
        SetStatus($"Round {roundNumber} 클리어!");
        StartCoroutine(ClearMessageTimer());
    }

    private void HandleAllCleared()
    {
        SetStatus("모든 라운드 클리어!");
        if (allClearPanel != null) allClearPanel.SetActive(true);
        SetTimerText(0f);
    }

    private void HandleEnemyCount(int alive, int total)
    {
        if (enemyCountLabel != null)
            enemyCountLabel.text = $"남은 적: {alive} / {total}";
    }

    private void HandleTimer(float remaining, float total)
    {
        SetTimerText(remaining);

        // 10초 이하 경고색
        if (timerLabel != null)
            timerLabel.color = (remaining <= 10f) ? timerWarningColor : timerNormalColor;
    }

    private void HandleTimeOut()
    {
        SetStatus("시간 초과! 다음 라운드 시작...");
    }

    // ──────────────────────────────────────────
    //  유틸
    // ──────────────────────────────────────────
    private void SetStatus(string msg)
    {
        if (statusLabel != null) statusLabel.text = msg;
    }

    /// <summary>남은 시간을 MM:SS 형식으로 표시합니다.</summary>
    private void SetTimerText(float seconds)
    {
        if (timerLabel == null) return;

        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        timerLabel.text = $"{mins:00}:{secs:00}";
    }

    private IEnumerator ClearMessageTimer()
    {
        yield return new WaitForSeconds(2f);
        SetStatus("다음 라운드 준비 중...");
    }
}
