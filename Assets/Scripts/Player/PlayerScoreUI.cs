using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 플레이어 머리 위에 World Space Canvas로 점수를 표시합니다.
///
/// [씬 설정 방법]
/// 1. 플레이어 오브젝트 하위에 빈 GameObject 생성 → 이름: "ScoreCanvas"
/// 2. Canvas 컴포넌트 추가 → Render Mode: World Space
/// 3. Canvas 하위에 TextMeshPro - Text 오브젝트 생성
/// 4. 이 스크립트를 플레이어에 붙이고 Inspector에서 연결
/// </summary>
[RequireComponent(typeof(PlayerScore))]
public class PlayerScoreUI : MonoBehaviour
{
    // ──────────────────────────────────────────
    //  Inspector
    // ──────────────────────────────────────────
    [Header("UI 연결")]
    [Tooltip("플레이어 머리 위 World Space Canvas 하위의 총점 텍스트")]
    public TextMeshProUGUI totalScoreText;

    [Tooltip("플레이어 머리 위 오프셋 (기본: 위쪽 1유닛)")]
    public Vector3 uiOffset = new Vector3(0f, 1.2f, 0f);

    [Header("팝업 획득 점수 설정")]
    [Tooltip("+N 팝업이 올라갈 TextMeshPro (없으면 팝업 비활성)")]
    public TextMeshProUGUI popupScoreText;

    [Tooltip("팝업이 올라가는 거리 (유닛)")]
    public float popupRiseDistance = 0.6f;

    [Tooltip("팝업 지속 시간 (초)")]
    public float popupDuration = 0.8f;

    [Header("색상")]
    public Color normalColor   = Color.white;
    public Color highlightColor = new Color(1f, 0.9f, 0.2f); // 획득 직후 노란색
    public float highlightDuration = 0.3f;

    // ──────────────────────────────────────────
    //  내부
    // ──────────────────────────────────────────
    private PlayerScore _playerScore;
    private Coroutine   _highlightCoroutine;
    private Coroutine   _popupCoroutine;

    // ──────────────────────────────────────────
    //  라이프사이클
    // ──────────────────────────────────────────
    private void Awake()
    {
        _playerScore = GetComponent<PlayerScore>();
    }

    private void OnEnable()
    {
        _playerScore.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        _playerScore.OnScoreChanged -= HandleScoreChanged;
    }

    private void Start()
    {
        RefreshTotalText(0);

        if (popupScoreText != null)
        {
            popupScoreText.gameObject.SetActive(false);
        }
    }

    // ──────────────────────────────────────────
    //  이벤트 핸들러
    // ──────────────────────────────────────────
    private void HandleScoreChanged(int total, int gained)
    {
        // 총점 갱신
        RefreshTotalText(total);

        // 노란 하이라이트 깜빡임
        if (totalScoreText != null)
        {
            if (_highlightCoroutine != null) StopCoroutine(_highlightCoroutine);
            _highlightCoroutine = StartCoroutine(HighlightRoutine());
        }

        // +N 팝업 표시
        if (popupScoreText != null && gained > 0)
        {
            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(PopupRoutine(gained));
        }
    }

    // ──────────────────────────────────────────
    //  텍스트 갱신
    // ──────────────────────────────────────────
    private void RefreshTotalText(int total)
    {
        if (totalScoreText == null) return;
        totalScoreText.text = $"{total}";
    }

    // ──────────────────────────────────────────
    //  연출 코루틴
    // ──────────────────────────────────────────

    /// <summary>총점 텍스트를 노랗게 번쩍인 뒤 원래 색으로 돌아옵니다.</summary>
    private IEnumerator HighlightRoutine()
    {
        totalScoreText.color = highlightColor;
        yield return new WaitForSeconds(highlightDuration);
        totalScoreText.color = normalColor;
    }

    /// <summary>"+N" 텍스트가 위로 떠오르다 사라집니다.</summary>
    private IEnumerator PopupRoutine(int gained)
    {
        popupScoreText.gameObject.SetActive(true);
        popupScoreText.text = $"+{gained}";

        // 팝업 시작 위치: 총점 텍스트 바로 위
        Vector3 startLocal = uiOffset + new Vector3(0f, 0.3f, 0f);
        Vector3 endLocal   = startLocal + new Vector3(0f, popupRiseDistance, 0f);

        float timer = 0f;
        Color popupColor = popupScoreText.color;

        while (timer < popupDuration)
        {
            timer += Time.deltaTime;
            float t = timer / popupDuration;

            // 위로 이동
            popupScoreText.transform.localPosition = Vector3.Lerp(startLocal, endLocal, t);

            // 후반부 페이드아웃
            float alpha = (t < 0.5f) ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
            popupScoreText.color = new Color(popupColor.r, popupColor.g, popupColor.b, alpha);

            yield return null;
        }

        popupScoreText.gameObject.SetActive(false);
        // 알파 복구
        popupScoreText.color = new Color(popupColor.r, popupColor.g, popupColor.b, 1f);
    }
}
