using UnityEngine;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    public List<GameObject> targets = new List<GameObject>(); // 맵에 배치된 타겟들
    private int currentTargetIndex = 0; // 현재 플레이어가 노리는 타겟 번호

    void Start()
    {
        // 시작할 때 모든 타겟을 활성화 상태로 둡니다.
        foreach (GameObject t in targets) t.SetActive(true);
    }

    // 플레이어가 현재 가야 할 타겟의 위치를 반환
    public Transform GetCurrentTargetTransform()
    {
        if (currentTargetIndex < targets.Count)
        {
            return targets[currentTargetIndex].transform;
        }
        return null;
    }

    // 타겟이 제거될 때 실행
    public void TargetEaten()
    {
        // 현재 타겟을 화면에서 제거
        targets[currentTargetIndex].SetActive(false);

        // 다음 순서로 이동
        currentTargetIndex++;

        // 만약 마지막 타겟까지 다 먹었다면?
        if (currentTargetIndex >= targets.Count)
        {
            Invoke("RespawnAll", 0.5f); // 0.5초 뒤에 전체 리스폰
        }
    }

    void RespawnAll()
    {
        currentTargetIndex = 0; // 순서 초기화
        foreach (GameObject t in targets)
        {
            // Target 스크립트의 Relocate를 호출해 위치를 섞고 다시 켬
            t.GetComponent<Target>().Relocate();
            t.SetActive(true);
        }
        //Debug.Log("모든 타겟이 새로운 위치에 생성되었습니다!");
    }
}