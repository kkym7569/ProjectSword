using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    [Header("타겟 및 UI 연결")]
    public List<GameObject> targets = new List<GameObject>();
    public List<Button> targetButtons = new List<Button>();

    [Header("동적 생성 프리팹")]
    public GameObject targetPrefab;
    public Button buttonPrefab;
    public Transform buttonParent;

    [Header("보상 타겟 목록")]
    public List<TargetData> availableRewards;

    [Header("플레이어 및 연출")]
    public PlayerMain player;
    [Range(0f, 1f)] public float throwDelay = 0.15f;

    [Header("Direction Selection")]
    [SerializeField] private DirectionSelector directionSelector;
    [SerializeField] private float directionSelectDelay = 3f;

    [Header("Target Limit")]
    [SerializeField] private int maxTargetCount = 6;

    public bool HasReachedTargetLimit => targets.Count >= maxTargetCount;

    private int activeTargetCount;
    private bool isGlobalThrowing = false; // 전체 리스폰 중에만 true
    private bool isFirstRespawn = true;
    private bool isDirectionSelecting;
    private bool isDispatchingTargets;
    private readonly HashSet<int> directionThrownTargets = new HashSet<int>();

    private void OnEnable()
    {
        PlayerAttribute.OnAllSlotsFilled += GiveNewTargetReward;
    }

    private void OnDisable()
    {
        PlayerAttribute.OnAllSlotsFilled -= GiveNewTargetReward;
    }

    void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerMain>();
        activeTargetCount = targets.Count;

        for (int i = 0; i < targets.Count; i++)
        {
            int index = i;
            targetButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));
            Target targetScript = targets[i].GetComponent<Target>();
            if (targetScript != null) targetScript.OnLanded += HandleTargetLanded;

            targets[i].SetActive(false);
            targetButtons[i].gameObject.SetActive(false);
        }
        RespawnAll();
    }

    private void GiveNewTargetReward()
    {
        if (targets.Count >= maxTargetCount) return;
        if (availableRewards == null || availableRewards.Count == 0) return;

        int randomIndex = Random.Range(0, availableRewards.Count);
        TargetData selectedData = availableRewards[randomIndex];
        AddNewTarget(selectedData);
    }

    private void AddNewTarget(TargetData newTargetData)
    {
        if (targets.Count >= maxTargetCount) return;

        int newIndex = targets.Count;

        // 1. UI 생성
        Button newBtn = Instantiate(buttonPrefab, buttonParent);
        newBtn.onClick.AddListener(() => OnTargetButtonClicked(newIndex));
        targetButtons.Add(newBtn);

        // 2. 칼 생성
        GameObject newObj = Instantiate(targetPrefab);
        Target targetScript = newObj.GetComponent<Target>();
        targetScript.InitData(newTargetData);
        targetScript.OnLanded += HandleTargetLanded;
        targets.Add(newObj);

        // 🌟 [수정] 전체 잠금을 하지 않고, 이 버튼만 임시로 비활성화
        newBtn.interactable = false;

        newObj.SetActive(true);
        newBtn.gameObject.SetActive(true);
        targetScript.Relocate();

        // 주의: activeTargetCount는 리스폰 로직에서 관리하므로 여기서 건드리지 않아도 됨
    }

    public void OnTargetButtonClicked(int index)
    {
        if (index < 0 || index >= targets.Count) return;

        if (isDirectionSelecting)
        {
            if (directionThrownTargets.Contains(index) || directionSelector == null) return;

            directionThrownTargets.Add(index);
            SetButtonSelectedVisual(targetButtons[index], true);
            targetButtons[index].interactable = false;

            targets[index].SetActive(true);
            targets[index].GetComponent<Target>().Relocate(directionSelector.GetDirection());
            return;
        }

        // 🌟 [수정] 전체 페이즈가 아니더라도, '해당' 칼이 날아가는 중이면 클릭 무시
        Target targetScript = targets[index].GetComponent<Target>();
        if (isGlobalThrowing || !targets[index].activeSelf || !targetScript.IsReady) return;

        player.StartAttackToTarget(targets[index].transform);
    }

    public void TargetEaten(GameObject eatenTarget)
    {
        eatenTarget.SetActive(false);
        int index = targets.IndexOf(eatenTarget);
        if (index != -1)
        {
            targetButtons[index].gameObject.SetActive(true);
            targetButtons[index].interactable = false;
            SetButtonSelectedVisual(targetButtons[index], true);
        }

        activeTargetCount--;

        if (activeTargetCount <= 0)
        {
            Invoke("RespawnAll", 0.5f);
        }
    }

    void RespawnAll()
    {
        StartCoroutine(RespawnAllRoutine());
    }

    IEnumerator RespawnAllRoutine()
    {
        activeTargetCount = targets.Count;
        isGlobalThrowing = true; // 전체 리스폰 시작
        directionThrownTargets.Clear();

        SetButtonsInteractable(false);

        if (!isFirstRespawn)
        {
            for (int i = 0; i < targetButtons.Count; i++)
            {
                targetButtons[i].gameObject.SetActive(true);
                targetButtons[i].interactable = true;
                SetButtonSelectedVisual(targetButtons[i], false);
            }

            isDirectionSelecting = true;

            if (directionSelector != null)
            {
                directionSelector.Show();
            }

            float directionSelectTimer = 0f;
            while (directionSelectTimer < directionSelectDelay
                && directionThrownTargets.Count < targets.Count)
            {
                directionSelectTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            if (directionSelector != null)
            {
                directionSelector.Hide();
            }

            isDirectionSelecting = false;
            SetButtonsInteractable(false);
        }

        isFirstRespawn = false;
        isDispatchingTargets = true;

        for (int i = 0; i < targets.Count; i++)
        {
            if (directionThrownTargets.Contains(i)) continue;

            targets[i].SetActive(true);
            targetButtons[i].gameObject.SetActive(true);
            targets[i].GetComponent<Target>().Relocate();
            yield return new WaitForSeconds(throwDelay);
        }

        isDispatchingTargets = false;
        TryCompleteGlobalThrow();
    }

    private void HandleTargetLanded()
    {
        if (isDirectionSelecting || isDispatchingTargets) return;

        // 🌟 [수정] 개별 칼이 도착할 때마다 해당 버튼을 활성화 시도
        // (전체 리스폰 중이 아닐 때 새로 추가된 칼이 도착하면 즉시 버튼 활성화)
        if (!isGlobalThrowing)
        {
            UpdateIndividualButtonState();
        }

        TryCompleteGlobalThrow();
    }

    private void TryCompleteGlobalThrow()
    {
        if (!isGlobalThrowing || isDirectionSelecting || isDispatchingTargets) return;

        foreach (GameObject targetObject in targets)
        {
            if (!targetObject.activeSelf || !targetObject.GetComponent<Target>().IsReady)
            {
                return;
            }
        }

        isGlobalThrowing = false;

        for (int i = 0; i < targetButtons.Count; i++)
        {
            targetButtons[i].gameObject.SetActive(true);
            targetButtons[i].interactable = true;
            SetButtonSelectedVisual(targetButtons[i], false);
        }
    }

    private void SetButtonSelectedVisual(Button button, bool selected)
    {
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = selected ? 0.5f : 1f;
    }

    // 🌟 [추가] 각 칼의 IsReady 상태에 맞춰 버튼 상태를 동기화
    private void UpdateIndividualButtonState()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].activeSelf)
            {
                targetButtons[i].interactable = targets[i].GetComponent<Target>().IsReady;
            }
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in targetButtons) btn.interactable = state;
    }
}
