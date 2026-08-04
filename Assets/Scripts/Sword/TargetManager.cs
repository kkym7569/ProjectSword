using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    [Header("?€ê²?ë°?UI ?°ê²°")]
    public List<GameObject> targets = new List<GameObject>();
    public List<Button> targetButtons = new List<Button>();

    [Header("?™ì  ?ì„± ?„ë¦¬??)]
    public GameObject targetPrefab;
    public Button buttonPrefab;
    public Transform buttonParent;

    [Header("ë³´ìƒ ?€ê²?ëª©ë¡")]
    public List<TargetData> availableRewards;

    [Header("?Œë ˆ?´ì–´ ë°??°ì¶œ")]
    public PlayerMain player;
    [Range(0f, 1f)] public float throwDelay = 0.15f;

    [Header("Direction Selection")]
    [SerializeField] private DirectionSelector directionSelector;
    [SerializeField] private float directionSelectDelay = 3f;

    [Header("Target Limit")]
    [SerializeField] private int maxTargetCount = 6;

    public bool HasReachedTargetLimit => targets.Count >= maxTargetCount;

    private int activeTargetCount;
    private bool isGlobalThrowing = false; // ?„ì²´ ë¦¬ìŠ¤??ì¤‘ì—ë§?true
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

        if (directionSelector == null && player != null)
        {
            directionSelector = DirectionSelector.CreateRuntime(player.transform);
        }
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

        // 1. UI ?ì„±
        Button newBtn = Instantiate(buttonPrefab, buttonParent);
        newBtn.onClick.AddListener(() => OnTargetButtonClicked(newIndex));
        targetButtons.Add(newBtn);

        // 2. ì¹??ì„±
        GameObject newObj = Instantiate(targetPrefab);
        Target targetScript = newObj.GetComponent<Target>();
        targetScript.InitData(newTargetData);
        targetScript.OnLanded += HandleTargetLanded;
        targets.Add(newObj);

        // ?ŒŸ [?˜ì •] ?„ì²´ ? ê¸ˆ???˜ì? ?Šê³ , ??ë²„íŠ¼ë§??„ì‹œë¡?ë¹„í™œ?±í™”
        newBtn.interactable = false;

        newObj.SetActive(true);
        newBtn.gameObject.SetActive(true);
        targetScript.Relocate();

        // ì£¼ì˜: activeTargetCount??ë¦¬ìŠ¤??ë¡œì§?ì„œ ê´€ë¦¬í•˜ë¯€ë¡??¬ê¸°??ê±´ë“œë¦¬ì? ?Šì•„????
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

        // ?ŒŸ [?˜ì •] ?„ì²´ ?˜ì´ì¦ˆê? ?„ë‹ˆ?”ë¼?? '?´ë‹¹' ì¹¼ì´ ? ì•„ê°€??ì¤‘ì´ë©??´ë¦­ ë¬´ì‹œ
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
        isGlobalThrowing = true; // ?„ì²´ ë¦¬ìŠ¤???œì‘
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

        // ?ŒŸ [?˜ì •] ê°œë³„ ì¹¼ì´ ?„ì°©???Œë§ˆ???´ë‹¹ ë²„íŠ¼???œì„±???œë„
        // (?„ì²´ ë¦¬ìŠ¤??ì¤‘ì´ ?„ë‹ ???ˆë¡œ ì¶”ê???ì¹¼ì´ ?„ì°©?˜ë©´ ì¦‰ì‹œ ë²„íŠ¼ ?œì„±??
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

    // ?ŒŸ [ì¶”ê?] ê°?ì¹¼ì˜ IsReady ?íƒœ??ë§ì¶° ë²„íŠ¼ ?íƒœë¥??™ê¸°??
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
