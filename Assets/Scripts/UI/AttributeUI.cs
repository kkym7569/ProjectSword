using UnityEngine;
using UnityEngine.UI;

public class AttributeUI : MonoBehaviour
{
    [Header("연결 설정")]
    public PlayerAttribute targetAttribute;

    [Header("UI 세팅")]
    public Image[] slotImages;
    public Color fillingColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 채워지는 중 (회색)
    public Color grassColor = Color.green;
    public Color waterColor = Color.blue;
    public Color lavaColor = Color.red;

    private void Awake()
    {
        // 인스펙터에서 할당 안 했을 경우 자동 탐색
        if (targetAttribute == null)
        {
            targetAttribute = FindObjectOfType<PlayerAttribute>();
        }
    }

    private void OnEnable()
    {
        if (targetAttribute != null)
        {
            // 이벤트 구독
            targetAttribute.OnGaugeUpdated += UpdateSlotFill;
            targetAttribute.OnSlotCompleted += UpdateSlotColor;

            // 🌟 5칸이 다 찼을 때 리셋 신호를 받기 위해 구독
            PlayerAttribute.OnAllSlotsFilled += ResetAllSlots;
        }
    }

    private void OnDisable()
    {
        if (targetAttribute != null)
        {
            // 메모리 누수 및 중복 방지를 위한 구독 해제
            targetAttribute.OnGaugeUpdated -= UpdateSlotFill;
            targetAttribute.OnSlotCompleted -= UpdateSlotColor;

            PlayerAttribute.OnAllSlotsFilled -= ResetAllSlots;
        }
    }

    private void Start()
    {
        // 시작 시 모든 슬롯 초기화
        ResetAllSlots();
    }

    // --- 이벤트 콜백 함수들 ---

    // 1. 게이지가 차오를 때 (매 프레임 호출)
    private void UpdateSlotFill(int slotIndex, float fillAmount)
    {
        if (slotIndex < slotImages.Length && slotImages[slotIndex] != null)
        {
            // 채워지는 동안은 기본 색상 유지
            slotImages[slotIndex].color = fillingColor;
            slotImages[slotIndex].fillAmount = fillAmount;
        }
    }

    // 2. 한 칸이 완전히 완성되었을 때 (색상 결정)
    private void UpdateSlotColor(int slotIndex, PlayerAttribute.ElementType element)
    {
        if (slotIndex < slotImages.Length && slotImages[slotIndex] != null)
        {
            // 완성되었으므로 fillAmount를 1로 확실히 고정
            slotImages[slotIndex].fillAmount = 1f;

            switch (element)
            {
                case PlayerAttribute.ElementType.Grass: slotImages[slotIndex].color = grassColor; break;
                case PlayerAttribute.ElementType.Water: slotImages[slotIndex].color = waterColor; break;
                case PlayerAttribute.ElementType.Lava: slotImages[slotIndex].color = lavaColor; break;
            }
        }
    }

    // 3. 🌟 5칸이 모두 차서 리셋될 때 호출
    private void ResetAllSlots()
    {
        Debug.Log("<color=yellow>[UI] 모든 슬롯이 리셋되었습니다.</color>");
        foreach (var img in slotImages)
        {
            if (img != null)
            {
                img.fillAmount = 0f;
                img.color = fillingColor;
            }
        }
    }
}