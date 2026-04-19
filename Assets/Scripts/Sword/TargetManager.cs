using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필수
using System.Collections.Generic;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    [Header("타겟 및 UI 연결")]
    public List<GameObject> targets = new List<GameObject>();
    public List<Button> targetButtons = new List<Button>();

    [Header("플레이어 연결")]
    public PlayerMain player;

    [Header("연출 설정")]
    [Range(0f, 1f)]
    public float throwDelay = 0.15f; // 칼과 칼 사이의 발사 간격 (인스펙터에서 조절 가능)

    private int activeTargetCount;
    private int landedTargetCount = 0;
    private bool isThrowingPhase = false;

    void Start()
    {
        // 만약 인스펙터에서 player를 연결하지 않았다면 자동으로 찾음
        if (player == null) player = FindObjectOfType<PlayerMain>();

        activeTargetCount = targets.Count;

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

    public void OnTargetButtonClicked(int index)
    {
        if (isThrowingPhase || !targets[index].activeSelf) return;
        player.StartAttackToTarget(targets[index].transform);
    }

    public void TargetEaten(GameObject eatenTarget)
    {
        eatenTarget.SetActive(false);
        int index = targets.IndexOf(eatenTarget);
        if (index != -1) targetButtons[index].gameObject.SetActive(false);

        activeTargetCount--;

        if (activeTargetCount <= 0)
        {
            // 0.5초 뒤에 리스폰 코루틴 실행
            Invoke("StartRespawnRoutine", 0.5f);
        }
    }

    // Invoke용 징검다리 함수
    void StartRespawnRoutine()
    {
        StartCoroutine(RespawnAllRoutine());
    }

    void RespawnAll()
    {
        StartCoroutine(RespawnAllRoutine());
    }

    // [핵심] 순차적으로 칼을 뿌리는 코루틴
    IEnumerator RespawnAllRoutine()
    {
        activeTargetCount = targets.Count;
        landedTargetCount = 0;
        isThrowingPhase = true;

        SetButtonsInteractable(false);

        for (int i = 0; i < targets.Count; i++)
        {
            // 1. 해당 칼과 버튼을 활성화
            targets[i].SetActive(true);
            targetButtons[i].gameObject.SetActive(true);

            // 2. 해당 칼에게 날아가라고 명령 (Target.cs 내부에서 플레이어 위치로 순간이동 후 비행 시작)
            targets[i].GetComponent<Target>().Relocate();

            // 3. 인스펙터에서 설정한 시간만큼 대기 후 다음 칼 생성
            yield return new WaitForSeconds(throwDelay);
        }
    }

    private void HandleTargetLanded()
    {
        if (!isThrowingPhase) return;

        landedTargetCount++;

        if (landedTargetCount >= targets.Count)
        {
            isThrowingPhase = false;
            SetButtonsInteractable(true);
            Debug.Log("<color=cyan>모든 칼이 안착되었습니다.</color>");
        }
    }

    private void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in targetButtons)
        {
            btn.interactable = state;
        }
    }
}