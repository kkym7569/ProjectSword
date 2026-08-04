using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(PlayerMain))]
[RequireComponent(typeof(PlayerTileDetector))]
public class PlayerAttribute : MonoBehaviour
{
    public enum ElementType { None, Grass, Water, Lava }

    [Header("Attribute Settings")]
    public int maxTotalPoints = 100;
    public float delayAfterAttack = 2.0f;
    public float pointGainInterval = 0.1f;

    // --- ?ŒŸ UIë¡?ë³´ë‚¼ ?´ë²¤?¸ë“¤ ---
    public event Action<int, float> OnGaugeUpdated;
    public event Action<int, ElementType> OnSlotCompleted;

    // [?µì‹¬ ì¶”ê?] 5ì¹¸ì´ ëª¨ë‘ ??ì°¼ì„ ??ë§¤ë‹ˆ?€?ê²Œ ?Œë¦´ ?„ì—­ ?´ë²¤??
    public static event Action OnAllSlotsFilled;

    [Header("Current Status (Read Only)")]
    public int currentSlotIndex = 0;
    public ElementType currentElement = ElementType.None;
    private int grassPoints = 0, waterPoints = 0, lavaPoints = 0;

    private PlayerMain mainScript;
    private PlayerTileDetector tileDetector;
    private TargetManager targetManager;
    private bool isAttacking = false;
    private float lastAttackEndTime = 0f, nextPointGainTime = 0f;

    private void Awake()
    {
        mainScript = GetComponent<PlayerMain>();
        tileDetector = GetComponent<PlayerTileDetector>();
        targetManager = FindObjectOfType<TargetManager>();
    }

    private void OnEnable()
    {
        mainScript.OnAttackMoveStarted += pos => isAttacking = true;
        mainScript.OnAttackMoveEnded += () => { isAttacking = false; lastAttackEndTime = Time.time; };
    }

    private void Update()
    {
        if (currentSlotIndex >= 5) return;
        if (isAttacking) return;

        if (Time.time >= lastAttackEndTime + delayAfterAttack && Time.time >= nextPointGainTime)
        {
            GainPointBasedOnTile();
            nextPointGainTime = Time.time + pointGainInterval;
        }
    }

    private void GainPointBasedOnTile()
    {
        string increasedElement = "";

        switch (tileDetector.currentTile)
        {
            case TileType.Grass: grassPoints++; increasedElement = "?€(Grass)"; break;
            case TileType.Water: waterPoints++; increasedElement = "ë¬?Water)"; break;
            case TileType.Lava: lavaPoints++; increasedElement = "?©ì•”(Lava)"; break;
            default: return;
        }

        int totalPoints = grassPoints + waterPoints + lavaPoints;
        float fillRatio = (float)totalPoints / maxTotalPoints;

        Debug.Log($"[?ìˆ˜ ?ë“] <color=green>{increasedElement}</color> +1 ??{currentSlotIndex + 1}ë²ˆì§¸ ì¹?ì´ì : {totalPoints} / {maxTotalPoints} ({(fillRatio * 100):F1}%)");

        OnGaugeUpdated?.Invoke(currentSlotIndex, fillRatio);

        if (totalPoints >= maxTotalPoints)
        {
            CalculateFinalElement();
        }
    }

    private void CalculateFinalElement()
    {
        Dictionary<ElementType, int> scores = new Dictionary<ElementType, int>()
        {
            { ElementType.Grass, grassPoints },
            { ElementType.Water, waterPoints }, { ElementType.Lava, lavaPoints }
        };

        int maxScore = scores.Values.Max();
        var candidates = scores.Where(kvp => kvp.Value == maxScore).Select(kvp => kvp.Key).ToList();

        currentElement = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        Debug.Log("=====================================");
        Debug.Log($"?ŒŸ {currentSlotIndex + 1}ë²ˆì§¸ ì¹??„ì„±! (?€: {grassPoints}, ë¬? {waterPoints}, ?©ì•”: {lavaPoints})");
        Debug.Log($"ìµœì¢… ê²°ì •???ì„±: <color=yellow>{currentElement}</color>");
        Debug.Log("=====================================");

        OnSlotCompleted?.Invoke(currentSlotIndex, currentElement);

        currentSlotIndex++;
        grassPoints = waterPoints = lavaPoints = 0;

        if (currentSlotIndex >= 5)
        {
            Debug.Log("<color=orange>5ê°œì˜ ?ì„± ì¹¸ì´ ëª¨ë‘ ê°€??ì°¼ìŠµ?ˆë‹¤! ?ˆë¡œ???€ê²Ÿì„ ?Œí™˜?©ë‹ˆ??</color>");

            // ?ŒŸ [ì¶”ê?] 5ì¹¸ì´ ??ì°¼ìœ¼??ë§¤ë‹ˆ?€?ê²Œ ?€ê²Ÿì„ ?¬ë¼ê³?? í˜¸ë¥??©ë‹ˆ??
            if (targetManager != null && targetManager.HasReachedTargetLimit)
            {
                Debug.Log("Maximum target count reached. Attribute slots remain filled.");
                return;
            }
            OnAllSlotsFilled?.Invoke();

            // 5ì¹¸ì´ ì°????¤ì‹œ ì²˜ìŒë¶€??ëª¨ìœ¼ê²??˜ë ¤ë©??„ë˜ ì£¼ì„???¸ì„¸??
            currentSlotIndex = 0; 
        }
    }
}