using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    [Header("타겟 및 UI 연결")]
    public List<GameObject> targets = new List<GameObject>(); // 맵에 배치된 타겟(검)들
    public List<Button> targetButtons = new List<Button>();   // 타겟과 1:1로 매칭될 버튼들

    [Header("동적 생성(보상)용 프리팹")]
    public GameObject targetPrefab; // 복사해낼 검의 원본 프리팹
    public Button buttonPrefab;     // 복사해낼 UI 버튼 원본 프리팹
    public Transform buttonParent;  // 새 버튼이 들어갈 UI 패널 (Layout Group 권장)

    // 🌟 [핵심] 5칸이 다 찼을 때 보상으로 줄 수 있는 타겟(SO) 목록
    [Header("보상 타겟(SO) 목록")]
    public List<TargetData> availableRewards;

    [Header("플레이어 및 연출")]
    public PlayerMain player;
    [Range(0f, 1f)] public float throwDelay = 0.15f; // 칼과 칼 사이의 발사 간격

    private int activeTargetCount; // 현재 맵에 살아있는 타겟 개수
    private bool isThrowingPhase = false;  // 현재 검들이 공중을 날아가는 중인지 여부

    // --- 이벤트 구독 (보상 시스템 연결) ---
    private void OnEnable()
    {
        // PlayerAttribute에서 5칸이 다 찼다는 신호가 오면 GiveNewTargetReward 실행
        PlayerAttribute.OnAllSlotsFilled += GiveNewTargetReward;
    }

    private void OnDisable()
    {
        PlayerAttribute.OnAllSlotsFilled -= GiveNewTargetReward;
    }

    void Start()
    {
        // 인스펙터에서 플레이어를 안 넣었다면 자동으로 찾기
        if (player == null) player = FindObjectOfType<PlayerMain>();

        activeTargetCount = targets.Count;

        // 초기에 배치된 타겟들 세팅
        for (int i = 0; i < targets.Count; i++)
        {
            int index = i;
            targetButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));

            Target targetScript = targets[i].GetComponent<Target>();
            if (targetScript != null)
            {
                targetScript.OnLanded += HandleTargetLanded;
            }

            // 시작 시에는 모두 꺼둔 상태로 시작 (RespawnAll에서 하나씩 켬)
            targets[i].SetActive(false);
            targetButtons[i].gameObject.SetActive(false);
        }

        // 게임 시작 시 첫 투척 실행
        RespawnAll();
    }

    // 🌟 [보상 시스템 1] 5칸이 다 차면 리스트에서 무작위로 뽑아서 생성 명령
    private void GiveNewTargetReward()
    {
        if (availableRewards == null || availableRewards.Count == 0)
        {
            Debug.LogWarning("보상으로 줄 타겟 데이터(SO)가 리스트에 없습니다!");
            return;
        }

        int randomIndex = Random.Range(0, availableRewards.Count);
        TargetData selectedData = availableRewards[randomIndex];

        Debug.Log($"<color=magenta>보상 획득: {selectedData.targetName} 타겟이 추가됩니다!</color>");

        // 맵에 실제로 생성
        AddNewTarget(selectedData);
    }

    // 🌟 [보상 시스템 2] 게임 도중 새로운 검을 찍어내서 리스트에 추가하는 함수
    private void AddNewTarget(TargetData newTargetData)
    {
        int newIndex = targets.Count;

        // 1. UI 버튼 생성 및 연결
        Button newBtn = Instantiate(buttonPrefab, buttonParent);
        newBtn.onClick.AddListener(() => OnTargetButtonClicked(newIndex));
        targetButtons.Add(newBtn);

        // 2. 실제 검(GameObject) 생성 및 연결
        GameObject newObj = Instantiate(targetPrefab);
        Target targetScript = newObj.GetComponent<Target>();

        targetScript.InitData(newTargetData); // SO 데이터 주입 (색상 등 변경)
        targetScript.OnLanded += HandleTargetLanded; // 도착 이벤트 구독

        targets.Add(newObj); // 관리 리스트에 추가
        activeTargetCount++; // 전체 타겟 개수 증가

        // 3. 새 검 하나만 즉시 투척 연출 시작!
        isThrowingPhase = true;
        SetButtonsInteractable(false); // 날아가는 동안 전체 버튼 잠금

        newObj.SetActive(true);
        newBtn.gameObject.SetActive(true);
        targetScript.Relocate(); // 플레이어 위치에서 새 검만 촥! 하고 날아감
    }

    // UI 버튼을 클릭했을 때 실행되는 함수
    public void OnTargetButtonClicked(int index)
    {
        // 검이 날아가는 중(Throwing Phase)이거나 이미 먹은 타겟이면 공격 무시
        if (isThrowingPhase || !targets[index].activeSelf) return;

        // 플레이어에게 해당 검으로 공격(이동)하라고 명령
        player.StartAttackToTarget(targets[index].transform);
    }

    // 플레이어가 특정 타겟을 공격 완료(도착)했을 때 실행
    public void TargetEaten(GameObject eatenTarget)
    {
        // 1. 공격당한 타겟과 UI 버튼 비활성화
        eatenTarget.SetActive(false);
        int index = targets.IndexOf(eatenTarget);
        if (index != -1) targetButtons[index].gameObject.SetActive(false);

        // 2. 남은 타겟 개수 줄이기
        activeTargetCount--;

        // 3. 모든 타겟을 다 잡았다면 0.5초 뒤에 전체 리스폰
        if (activeTargetCount <= 0)
        {
            Invoke("StartRespawnRoutine", 0.5f);
        }
    }

    // 코루틴 실행을 위한 징검다리 함수
    void StartRespawnRoutine()
    {
        StartCoroutine(RespawnAllRoutine());
    }

    void RespawnAll()
    {
        StartCoroutine(RespawnAllRoutine());
    }

    // 모든 검을 다시 사방으로 순차적으로 뿌리는 코루틴
    IEnumerator RespawnAllRoutine()
    {
        activeTargetCount = targets.Count;
        isThrowingPhase = true;

        // 검이 날아가는 동안 모든 버튼을 누를 수 없게 회색으로 잠금!
        SetButtonsInteractable(false);

        for (int i = 0; i < targets.Count; i++)
        {
            // 1. 해당 칼과 버튼을 활성화
            targets[i].SetActive(true);
            targetButtons[i].gameObject.SetActive(true);

            // 2. 해당 칼에게 날아가라고 명령
            targets[i].GetComponent<Target>().Relocate();

            // 3. 인스펙터에서 설정한 시간만큼 대기 후 다음 칼 발사
            yield return new WaitForSeconds(throwDelay);
        }
    }

    // [핵심] 타겟 하나가 땅에 꽂힐 때마다 이 함수가 실행됨
    private void HandleTargetLanded()
    {
        // 투척 페이즈가 아닐 때는 무시
        if (!isThrowingPhase) return;

        // 현재 화면에 켜져 있는 모든 검이 'IsReady' 상태인지 전수 검사
        bool allLanded = true;
        foreach (GameObject t in targets)
        {
            if (t.activeSelf)
            {
                Target script = t.GetComponent<Target>();
                if (script != null && !script.IsReady)
                {
                    allLanded = false; // 하나라도 꽂히지 않은 검이 있다면 false
                    break;
                }
            }
        }

        // 모든 검이 무사히 다 꽂혔다면 잠금 해제!
        if (allLanded)
        {
            isThrowingPhase = false;
            SetButtonsInteractable(true);
            Debug.Log("<color=cyan>모든 검 안착 완료! 공격 버튼 잠금 해제.</color>");
        }
    }

    // UI 버튼들의 클릭 가능 상태를 한 번에 끄고 켜는 편의성 함수
    private void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in targetButtons)
        {
            btn.interactable = state;
        }
    }
}