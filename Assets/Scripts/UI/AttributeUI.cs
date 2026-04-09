using UnityEngine;
using UnityEngine.UI;

public class AttributeUI : MonoBehaviour
{
    [Header("연결 설정")]
    public PlayerAttribute targetAttribute;

    [Header("UI 세팅")]
    public Image[] slotImages;
    public Color fillingColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 회색 (알파 100%)
    public Color grassColor = Color.green;
    public Color waterColor = Color.blue;
    public Color lavaColor = Color.red;

    private void Start()
    {
        // 🌟 [안전장치] 혹시 인스펙터에서 깜빡하고 안 넣었을 경우 자동으로 플레이어를 찾음
        if (targetAttribute == null)
        {
            targetAttribute = FindObjectOfType<PlayerAttribute>();
            Debug.Log("<color=orange>[UI] 인스펙터에 플레이어가 없어서 자동으로 찾았습니다!</color>");
        }

        // 이벤트 강제 재구독 (놓친 신호 방지)
        if (targetAttribute != null)
        {
            targetAttribute.OnGaugeUpdated -= UpdateSlotFill; // 중복 방지
            targetAttribute.OnSlotCompleted -= UpdateSlotColor;

            targetAttribute.OnGaugeUpdated += UpdateSlotFill;
            targetAttribute.OnSlotCompleted += UpdateSlotColor;
            Debug.Log("<color=cyan>[UI] 플레이어의 신호를 성공적으로 연결했습니다!</color>");
        }
        else
        {
            Debug.LogError("[UI] 🚨 맵에 PlayerAttribute를 가진 플레이어가 없습니다!");
        }

        // 시작 시 초기화
        foreach (var img in slotImages)
        {
            if (img != null)
            {
                img.fillAmount = 0f;
                img.color = fillingColor;
            }
        }
    }

    // 신호를 받으면 실행되는 함수 1: 게이지 채우기
    private void UpdateSlotFill(int slotIndex, float fillAmount)
    {

        if (slotIndex < slotImages.Length && slotImages[slotIndex] != null)
        {
            slotImages[slotIndex].color = fillingColor;
            slotImages[slotIndex].fillAmount = fillAmount;
        }
    }

    // 신호를 받으면 실행되는 함수 2: 다 찬 게이지 색깔 바꾸기
    private void UpdateSlotColor(int slotIndex, PlayerAttribute.ElementType element)
    {
      
        if (slotIndex < slotImages.Length && slotImages[slotIndex] != null)
        {
            switch (element)
            {
                case PlayerAttribute.ElementType.Grass: slotImages[slotIndex].color = grassColor; break;
                case PlayerAttribute.ElementType.Water: slotImages[slotIndex].color = waterColor; break;
                case PlayerAttribute.ElementType.Lava: slotImages[slotIndex].color = lavaColor; break;
            }
        }
    }
}