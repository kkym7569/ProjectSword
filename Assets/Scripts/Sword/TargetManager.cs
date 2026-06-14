using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    [Header("동적 생성 프리팹 (필수 연결)")]
    public GameObject targetPrefab;   // Target 프리팹
    public Button     buttonPrefab;   // Button 프리팹
    public Transform  buttonParent;   // TargetButtonGroup

    [Header("기본 검 데이터")]
    [Tooltip("게임 시작 시 생성할 기본 검. 비워두면 Normal 타입.")]
    public TargetData defaultSwordData;

    [Header("추가 가능한 특수 검 목록")]
    [Tooltip("게이지 5칸 달성 시 여기서 랜덤 1개 추가됩니다.")]
    public List<TargetData> addableSwords;

    [Header("플레이어 연결 (비워두면 자동 탐색)")]
    public PlayerMain   player;
    public PlayerCombat playerCombat;

    [Range(0f, 1f)]
    public float throwDelay = 0.05f;

    // ── 내부 상태 ──────────────────────────────
    private List<GameObject>     _targets      = new List<GameObject>();
    private List<Button>         _buttons      = new List<Button>();
    private List<SwordBehaviour> _behaviours   = new List<SwordBehaviour>();
    private int                  _aliveCount   = 0;
    private Dictionary<TargetData, int> _dataCountMap = new Dictionary<TargetData, int>();

    // ──────────────────────────────────────────
    private void OnEnable()  { PlayerAttribute.OnAllSlotsFilled += AddRandomSword; }
    private void OnDisable() { PlayerAttribute.OnAllSlotsFilled -= AddRandomSword; }

    // ──────────────────────────────────────────
    //  초기화
    // ──────────────────────────────────────────
    private void Start()
    {
        // 플레이어 자동 탐색
        if (player       == null) player       = FindObjectOfType<PlayerMain>();
        if (playerCombat == null) playerCombat = FindObjectOfType<PlayerCombat>();
        if (player != null && player.manager == null) player.manager = this;

        if (targetPrefab == null || buttonPrefab == null || buttonParent == null)
        {
            Debug.LogError("[TargetManager] targetPrefab / buttonPrefab / buttonParent 가 비어 있습니다!");
            return;
        }

        // 기본 검 1개 생성 후 날리기
        SpawnOne(defaultSwordData);
    }

    // ──────────────────────────────────────────
    //  검 1개 생성 + 즉시 날리기
    // ──────────────────────────────────────────
    private void SpawnOne(TargetData data)
    {
        int idx = _targets.Count;   // 추가 전 index 확정

        // ── 같은 타입 번호 계산 (항상 1부터 시작, 중복 시 2, 3, 4...) ──
        int number = 1;
        if (data != null)
        {
            if (!_dataCountMap.ContainsKey(data)) _dataCountMap[data] = 0;
            _dataCountMap[data]++;
            number = _dataCountMap[data];
        }
        string numberText = number.ToString();

        // ── 버튼 생성 ──
        Button btn = Instantiate(buttonPrefab, buttonParent);
        _buttons.Add(btn);
        int captured = idx;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnButtonClicked(captured));
        btn.interactable = false;
        btn.gameObject.SetActive(true);

        // ── 버튼 색상 적용 ──
        if (data != null)
        {
            Image btnImage = btn.GetComponent<Image>();
            if (btnImage != null) btnImage.color = data.trailColor;
        }

        // ── 버튼 하위 TMP 텍스트에 번호 적용 ──
        var btnTmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (btnTmp != null) btnTmp.text = numberText;

        // ── 검 오브젝트 생성 ──
        GameObject obj = Instantiate(targetPrefab);
        Target     t   = obj.GetComponent<Target>();

        if (data != null) t.InitData(data);

        // 타겟 하위 TMP 텍스트에 번호 적용
        var targetTmp = obj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (targetTmp != null) targetTmp.text = numberText;

        // Hierarchy 이름에도 번호 반영
        string baseName = data != null ? data.targetName : "Default";
        obj.name = $"Target_{baseName}_{number}";

        t.OnLanded += () => OnTargetLanded(captured);

        _targets.Add(obj);

        // ── 특수 동작 부착 ──
        SwordBehaviour sb = (data != null && playerCombat != null)
            ? SwordBehaviourFactory.Create(obj, data, playerCombat)
            : null;
        _behaviours.Add(sb);

        // ── 날리기 ──
        obj.SetActive(true);
        _aliveCount++;
        t.Relocate();

        Debug.Log($"[TargetManager] 검 생성 index={idx} name={obj.name}");
    }

    // ──────────────────────────────────────────
    //  착지 완료 콜백
    // ──────────────────────────────────────────
    private void OnTargetLanded(int idx)
    {
        Debug.Log($"[TargetManager] 검 index={idx} 착지 → 버튼 활성화");
        if (idx < _buttons.Count)
        {
            _buttons[idx].interactable = true;
            _buttons[idx].gameObject.SetActive(true);
        }
    }

    // ──────────────────────────────────────────
    //  버튼 클릭 → 공격
    // ──────────────────────────────────────────
    private void OnButtonClicked(int idx)
    {
        if (idx >= _targets.Count) return;

        Target t = _targets[idx].GetComponent<Target>();

        if (t == null || !_targets[idx].activeSelf || !t.IsReady)
        {
            Debug.Log($"[TargetManager] 버튼 {idx} 무시 — active:{_targets[idx].activeSelf} ready:{t?.IsReady}");
            return;
        }

        if (player == null)
        {
            Debug.LogError("[TargetManager] player가 null! Inspector에서 연결하세요.");
            return;
        }

        Debug.Log($"[TargetManager] 버튼 {idx} → 공격 출발!");
        _buttons[idx].interactable = false;   // 공격 중 중복 클릭 방지
        player.StartAttackToTarget(_targets[idx].transform, _behaviours[idx]);
    }

    // ──────────────────────────────────────────
    //  수집 처리 (PlayerMain이 도착 후 호출)
    // ──────────────────────────────────────────
    public void TargetEaten(GameObject eaten)
    {
        int idx = _targets.IndexOf(eaten);
        eaten.SetActive(false);
        if (idx != -1) _buttons[idx].gameObject.SetActive(false);

        _aliveCount--;
        Debug.Log($"[TargetManager] 검 수집 index={idx} 남은 검:{_aliveCount}");

        if (_aliveCount <= 0)
            StartCoroutine(RespawnAllAfterDelay(0.5f));
    }

    // ──────────────────────────────────────────
    //  전체 리스폰
    // ──────────────────────────────────────────
    private IEnumerator RespawnAllAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        _aliveCount = _targets.Count;

        for (int i = 0; i < _targets.Count; i++)
        {
            _buttons[i].interactable = false;
            _buttons[i].gameObject.SetActive(true);
            _targets[i].SetActive(true);
            _targets[i].GetComponent<Target>().Relocate();
            yield return new WaitForSeconds(throwDelay);
        }
    }

    // ──────────────────────────────────────────
    //  게이지 5칸 달성 → 랜덤 검 추가
    // ──────────────────────────────────────────
    public void AddRandomSword()
    {
        if (addableSwords == null || addableSwords.Count == 0)
        {
            Debug.LogWarning("[TargetManager] addableSwords가 비어 있습니다.");
            return;
        }

        TargetData data = addableSwords[Random.Range(0, addableSwords.Count)];
        SpawnOne(data);
        Debug.Log($"[TargetManager] 특수 검 추가 → {data.targetName} (총 슬롯: {_targets.Count})");
    }
}
