using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // UI(Button) 제어를 위해 필수!

public class TargetManager : MonoBehaviour
{
    [Header("타겟 및 UI 연결")]
    public List<GameObject> targets = new List<GameObject>(); // 맵에 배치된 타겟들
    public List<Button> targetButtons = new List<Button>();   // 타겟과 1:1로 매칭될 버튼들

    private PlayerMain player;
    private int activeTargetCount; // 현재 맵에 살아있는 타겟 개수

    void Start()
    {
        player = FindObjectOfType<PlayerMain>();
        activeTargetCount = targets.Count;

        // 시작할 때 모든 타겟과 버튼을 켜고, 버튼을 누를 때의 이벤트를 연결합니다.
        for (int i = 0; i < targets.Count; i++)
        {
            int index = i; // (중요) for문 안에서 이벤트(Listener)를 달 때는 반드시 로컬 변수로 빼주어야 오류가 안 납니다.

            // 버튼을 클릭하면 OnTargetButtonClicked 함수에 자신의 번호(index)를 넘겨주며 실행
            targetButtons[i].onClick.AddListener(() => OnTargetButtonClicked(index));

            targets[i].SetActive(true);
            targetButtons[i].gameObject.SetActive(true);
        }
    }

    // UI 버튼을 클릭했을 때 실행되는 함수
    public void OnTargetButtonClicked(int index)
    {
        // 타겟이 살아있을 때만 공격 명령을 내림
        if (targets[index].activeSelf)
        {
            player.StartAttackToTarget(targets[index].transform);
        }
    }

    // 플레이어가 특정 타겟을 공격 완료(도착)했을 때 실행
    public void TargetEaten(GameObject eatenTarget)
    {
        // 1. 공격당한 타겟 비활성화
        eatenTarget.SetActive(false);

        // 2. 이 타겟과 연결된 UI 버튼도 찾아서 숨기기
        int index = targets.IndexOf(eatenTarget);
        if (index != -1)
        {
            targetButtons[index].gameObject.SetActive(false);
        }

        // 3. 남은 타겟 개수 줄이기
        activeTargetCount--;

        // 4. 모든 타겟을 다 잡았다면 리스폰
        if (activeTargetCount <= 0)
        {
            Invoke("RespawnAll", 0.5f);
        }
    }

    void RespawnAll()
    {
        activeTargetCount = targets.Count;

        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].GetComponent<Target>().Relocate();
            targets[i].SetActive(true);
            targetButtons[i].gameObject.SetActive(true); // 사라졌던 버튼들도 다시 등장!
        }
    }
}