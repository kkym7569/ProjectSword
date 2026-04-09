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

    // --- 🌟 UI로 보낼 이벤트들 (허공에 소리치기) ---
    // (현재 채우는 칸 인덱스, 0.0~1.0 사이의 채워진 비율)
    public event Action<int, float> OnGaugeUpdated;
    // (완성된 칸 인덱스, 최종 결정된 속성)
    public event Action<int, ElementType> OnSlotCompleted;

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
        // 총 5칸(인덱스 4)을 넘어가면 중지
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
        string increasedElement = ""; // 로그 출력을 위한 문자열

        switch (tileDetector.currentTile)
        {
            case TileType.Grass: grassPoints++; increasedElement = "풀(Grass)"; break;
            case TileType.Water: waterPoints++; increasedElement = "물(Water)"; break;
            case TileType.Lava: lavaPoints++; increasedElement = "용암(Lava)"; break;
            default: return;
        }

        int totalPoints = grassPoints + waterPoints + lavaPoints;
        float fillRatio = (float)totalPoints / maxTotalPoints;

        // 🌟 [디버그 로그 추가] 점수가 오를 때마다 현재 상태 출력
        Debug.Log($"[점수 획득] <color=green>{increasedElement}</color> +1 ➡ {currentSlotIndex + 1}번째 칸 총점: {totalPoints} / {maxTotalPoints} ({(fillRatio * 100):F1}%)");

        // 🌟 이벤트 발송: "현재 칸이 이만큼 찼어!"
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

        // 🌟 [디버그 로그 추가] 한 칸이 모두 채워졌을 때 결과 출력
        Debug.Log("=====================================");
        Debug.Log($"🌟 {currentSlotIndex + 1}번째 칸 완성! (풀: {grassPoints}, 물: {waterPoints}, 용암: {lavaPoints})");
        Debug.Log($"최종 결정된 속성: <color=yellow>{currentElement}</color>");
        Debug.Log("=====================================");

        // 🌟 이벤트 발송: "현재 칸이 이 속성으로 끝났어!"
        OnSlotCompleted?.Invoke(currentSlotIndex, currentElement);

        currentSlotIndex++;
        grassPoints = waterPoints = lavaPoints = 0;

        if (currentSlotIndex >= 5)
        {
            Debug.Log("<color=orange>5개의 속성 칸이 모두 가득 찼습니다!</color>");
        }
    }
}