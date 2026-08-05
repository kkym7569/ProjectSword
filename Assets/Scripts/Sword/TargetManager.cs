using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    [Header("?€ê²?ë°?UI ?°ê²°")]
    public List<GameObject> targets = new List<GameObject>();
    public List<Button> targetButtons = new List<Button>();

    [Header("?™ì  ?ì„± ?„ë¦¬??")]
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

    [SerializeField] private BulletTimeManager bulletTimeManager;

    [Header("Target Limit")]
    [SerializeField] private int maxTargetCount = 6;

    public bool HasReachedTargetLimit => targets.Count >= maxTargetCount;

    private int activeTargetCount;
    private bool isGlobalThrowing = false; // ?„ì²´ ë¦¬ìŠ¤??ì¤‘ì—ë§?true
    private bool isFirstRespawn = true;
    private bool isDirectionSelecting;
    private bool isDispatchingTargets;
    private readonly HashSet<int> directionThrownTargets = new HashSet<int>();
    private readonly HashSet<int> suppressedButtonClicks = new HashSet<int>();
    private int heldTargetIndex = -1;

    public int HeldTargetIndex => heldTargetIndex;
    public bool HasHeldTarget => IsHeldTarget(heldTargetIndex);

    private void OnEnable()
    {
        PlayerAttribute.OnAllSlotsFilled += GiveNewTargetReward;
    }

    private void OnDisable()
    {
        PlayerAttribute.OnAllSlotsFilled -= GiveNewTargetReward;
        if (bulletTimeManager != null) bulletTimeManager.EndBulletTime();
    }

    void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerMain>();

        if (directionSelector == null && player != null)
        {
            directionSelector = DirectionSelector.CreateRuntime(player.transform);
        }

        if (bulletTimeManager == null) bulletTimeManager = FindObjectOfType<BulletTimeManager>();
        if (bulletTimeManager == null) bulletTimeManager = gameObject.AddComponent<BulletTimeManager>();
        bulletTimeManager.SetDirectionSelector(directionSelector);
        activeTargetCount = targets.Count;

        for (int i = 0; i < targets.Count; i++)
        {
            int index = i;
            targetButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));
            ConfigureButtonDrag(targetButtons[i], index);
            Target targetScript = targets[i].GetComponent<Target>();
            if (targetScript != null) targetScript.OnLanded += HandleTargetLanded;
            if (targetScript != null) targetScript.SetHeld();

            targets[i].SetActive(false);
            targetButtons[i].gameObject.SetActive(false);
        }

        int existingTargetCount = targets.Count;
        while (targets.Count < maxTargetCount
            && targetPrefab != null
            && buttonPrefab != null
            && buttonParent != null
            && availableRewards != null
            && availableRewards.Count > 0)
        {
            TargetData startingData = availableRewards[targets.Count % availableRewards.Count];
            AddNewTarget(startingData);
        }

        for (int i = existingTargetCount; i < targets.Count; i++)
        {
            Target targetScript = targets[i].GetComponent<Target>();
            if (targetScript != null) targetScript.SetHeld();
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
        ConfigureButtonDrag(newBtn, newIndex);
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
        if (suppressedButtonClicks.Remove(index)) return;

        if (isDirectionSelecting)
        {
            int maxThrowCount = Mathf.Max(0, targets.Count - 1);
            if (directionThrownTargets.Count >= maxThrowCount) return;

            Target selectedTarget = targets[index].GetComponent<Target>();
            if (directionThrownTargets.Contains(index)
                || bulletTimeManager == null
                || selectedTarget == null
                || selectedTarget.State != Target.TargetState.Held
                || !bulletTimeManager.TrySelectDirection(out Vector2 direction)) return;

            directionThrownTargets.Add(index);
            SetButtonSelectedVisual(targetButtons[index], true);
            targetButtons[index].interactable = false;

            targets[index].SetActive(true);
            selectedTarget.Relocate(direction);
            bulletTimeManager.CompleteSelectionIfReady();
            return;
        }

        // ?ŒŸ [?˜ì •] ?„ì²´ ?˜ì´ì¦ˆê? ?„ë‹ˆ?”ë¼?? '?´ë‹¹' ì¹¼ì´ ? ì•„ê°€??ì¤‘ì´ë©??´ë¦­ ë¬´ì‹œ
        Target targetScript = targets[index].GetComponent<Target>();
        if (isGlobalThrowing || !targets[index].activeSelf || targetScript.State != Target.TargetState.Landed) return;

        player.StartAttackToTarget(targets[index].transform);
    }

    public void TargetEaten(GameObject eatenTarget)
    {
        Target eatenTargetScript = eatenTarget.GetComponent<Target>();
        if (eatenTargetScript != null) eatenTargetScript.SetHeld();
        eatenTarget.SetActive(false);
        int index = targets.IndexOf(eatenTarget);
        if (index != -1)
        {
            if (heldTargetIndex >= 0 && heldTargetIndex < targetButtons.Count)
            {
                SetButtonHeldVisual(targetButtons[heldTargetIndex], false);
            }

            heldTargetIndex = index;
            targetButtons[index].gameObject.SetActive(true);
            targetButtons[index].interactable = false;
            SetButtonSelectedVisual(targetButtons[index], false);
            SetButtonHeldVisual(targetButtons[index], true);
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
        activeTargetCount = isFirstRespawn ? targets.Count : Mathf.Max(0, targets.Count - 1);
        isGlobalThrowing = true;
        directionThrownTargets.Clear();
        heldTargetIndex = -1;

        SetButtonsInteractable(false);

        if (!isFirstRespawn)
        {
            for (int i = 0; i < targetButtons.Count; i++)
            {
                targetButtons[i].gameObject.SetActive(true);
                targetButtons[i].interactable = true;
                SetButtonSelectedVisual(targetButtons[i], false);
                SetButtonHeldVisual(targetButtons[i], false);
            }

            isDirectionSelecting = true;
            bulletTimeManager.BeginDirectionSelection(
                Mathf.Max(0, targets.Count - 1),
                HandleDirectionSelectionFinished);

            while (isDirectionSelecting)
            {
                yield return null;
            }
        }

        isFirstRespawn = false;
        isDispatchingTargets = true;

        for (int i = 0; i < targets.Count; i++)
        {
            if (directionThrownTargets.Contains(i) || i == heldTargetIndex) continue;

            targets[i].SetActive(true);
            targetButtons[i].gameObject.SetActive(true);
            targets[i].GetComponent<Target>().Relocate();
            yield return new WaitForSecondsRealtime(throwDelay);
        }

        isDispatchingTargets = false;
        TryCompleteGlobalThrow();
    }

    private void HandleDirectionSelectionFinished(bool timedOut)
    {
        if (timedOut)
        {
            int maxThrowCount = Mathf.Max(0, targets.Count - 1);
            for (int i = 0; i < targets.Count && directionThrownTargets.Count < maxThrowCount; i++)
            {
                if (directionThrownTargets.Contains(i)) continue;

                Target target = targets[i].GetComponent<Target>();
                if (target == null || target.State != Target.TargetState.Held) continue;

                directionThrownTargets.Add(i);
                targetButtons[i].interactable = false;
                SetButtonSelectedVisual(targetButtons[i], true);
                targets[i].SetActive(true);
                target.Relocate(bulletTimeManager.GetRandomDirection());
            }
        }

        heldTargetIndex = SelectHeldTargetIndex();
        ApplyHeldTarget(heldTargetIndex);
        SetButtonsInteractable(false);
        isDirectionSelecting = false;
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

        for (int i = 0; i < targets.Count; i++)
        {
            Target target = targets[i].GetComponent<Target>();

            if (i == heldTargetIndex)
            {
                if (target == null || target.State != Target.TargetState.Held) return;
                continue;
            }

            if (!targets[i].activeSelf
                || target == null
                || target.State != Target.TargetState.Landed)
            {
                return;
            }
        }

        isGlobalThrowing = false;

        for (int i = 0; i < targetButtons.Count; i++)
        {
            bool isHeld = i == heldTargetIndex;
            targetButtons[i].gameObject.SetActive(true);
            targetButtons[i].interactable = !isHeld;
            SetButtonSelectedVisual(targetButtons[i], false);
            SetButtonHeldVisual(targetButtons[i], isHeld);
        }
    }

    private void ApplyHeldTarget(int index)
    {
        if (index < 0 || index >= targets.Count) return;

        Target heldTarget = targets[index].GetComponent<Target>();
        if (heldTarget != null) heldTarget.SetHeld();
        targets[index].SetActive(false);
        targetButtons[index].gameObject.SetActive(true);
        targetButtons[index].interactable = false;
        SetButtonSelectedVisual(targetButtons[index], false);
        SetButtonHeldVisual(targetButtons[index], true);
        activeTargetCount = Mathf.Max(0, targets.Count - 1);
    }
    private int SelectHeldTargetIndex()
    {
        List<int> remainingTargets = new List<int>();

        for (int i = 0; i < targets.Count; i++)
        {
            if (!directionThrownTargets.Contains(i))
            {
                remainingTargets.Add(i);
            }
        }

        if (remainingTargets.Count == 0) return -1;
        return remainingTargets[Random.Range(0, remainingTargets.Count)];
    }

    public bool IsHeldTarget(int index)
    {
        return index >= 0
            && index < targets.Count
            && index == heldTargetIndex
            && targets[index] != null
            && targets[index].GetComponent<Target>().State == Target.TargetState.Held;
    }

    public void SuppressNextButtonClick(int index)
    {
        suppressedButtonClicks.Add(index);
    }

    public bool TryDestroyHeldTarget(int index)
    {
        if (!IsHeldTarget(index) || targets.Count <= 2 || isDirectionSelecting || isGlobalThrowing)
        {
            return false;
        }

        GameObject targetObject = targets[index];
        Button targetButton = targetButtons[index];
        Target target = targetObject != null ? targetObject.GetComponent<Target>() : null;
        if (target != null) target.OnLanded -= HandleTargetLanded;

        targets.RemoveAt(index);
        targetButtons.RemoveAt(index);
        heldTargetIndex = -1;
        suppressedButtonClicks.Clear();

        if (targetObject != null) Destroy(targetObject);
        if (targetButton != null) Destroy(targetButton.gameObject);

        RebindTargetButtons();
        return true;
    }

    private void ConfigureButtonDrag(Button button, int index)
    {
        TargetButtonDragHandler dragHandler = button.GetComponent<TargetButtonDragHandler>();
        if (dragHandler == null)
        {
            dragHandler = button.gameObject.AddComponent<TargetButtonDragHandler>();
        }

        dragHandler.Configure(this, index);
    }

    private void RebindTargetButtons()
    {
        for (int i = 0; i < targetButtons.Count; i++)
        {
            int index = i;
            targetButtons[i].onClick.RemoveAllListeners();
            targetButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));
            ConfigureButtonDrag(targetButtons[i], index);
        }
    }
    private void SetButtonHeldVisual(Button button, bool isHeld)
    {
        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.75f, 0.1f, 1f);
            outline.effectDistance = new Vector2(3f, -3f);
        }

        outline.enabled = isHeld;
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
                targetButtons[i].interactable = targets[i].GetComponent<Target>().State == Target.TargetState.Landed;
            }
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in targetButtons) btn.interactable = state;
    }

}
