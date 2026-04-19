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

    // --- 🌟 UI로 보낼 이벤트들 ---
    public event Action<int, float> OnGaugeUpdated;
    public event Action<int, ElementType> OnSlotCompleted;

    // [핵심 추가] 5칸이 모두 다 찼을 때 매니저에게 알릴 전역 이벤트!
    public static event Action OnAllSlotsFilled;

    [Header("Current Status (Read Only)")]
    public int currentSlotIndex = 0;
    public ElementType currentElement = ElementType.None;
    private int grassPoints = 0, waterPoints = 0, lavaPoints = 0;

    private PlayerMain mainScript;
    private PlayerTileDetector tileDetector;
    private bool isAttacking = false;
    private float lastAttackEndTime = 0f, nextPointGainTime = 0f;

    private void Awake()
    {
        mainScript = GetComponent<PlayerMain>();
        tileDetector = GetComponent<PlayerTileDetector>();
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
            case TileType.Grass: grassPoints++; increasedElement = "풀(Grass)"; break;
            case TileType.Water: waterPoints++; increasedElement = "물(Water)"; break;
            case TileType.Lava: lavaPoints++; increasedElement = "용암(Lava)"; break;
            default: return;
        }

        int totalPoints = grassPoints + waterPoints + lavaPoints;
        float fillRatio = (float)totalPoints / maxTotalPoints;

        Debug.Log($"[점수 획득] <color=green>{increasedElement}</color> +1 ➡ {currentSlotIndex + 1}번째 칸 총점: {totalPoints} / {maxTotalPoints} ({(fillRatio * 100):F1}%)");

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
        Debug.Log($"🌟 {currentSlotIndex + 1}번째 칸 완성! (풀: {grassPoints}, 물: {waterPoints}, 용암: {lavaPoints})");
        Debug.Log($"최종 결정된 속성: <color=yellow>{currentElement}</color>");
        Debug.Log("=====================================");

        OnSlotCompleted?.Invoke(currentSlotIndex, currentElement);

        currentSlotIndex++;
        grassPoints = waterPoints = lavaPoints = 0;

        if (currentSlotIndex >= 5)
        {
            Debug.Log("<color=orange>5개의 속성 칸이 모두 가득 찼습니다! 새로운 타겟을 소환합니다!</color>");

            // 🌟 [추가] 5칸이 다 찼으니 매니저에게 타겟을 달라고 신호를 쏩니다.
            OnAllSlotsFilled?.Invoke();

            // 5칸이 찬 후 다시 처음부터 모으게 하려면 아래 주석을 푸세요.
            currentSlotIndex = 0; 
        }
    }
}