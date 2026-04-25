using UnityEngine;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    [Header("Manager References")]
    public EnemySpawner spawner; // 분리된 스포너 스크립트 연결

    [Header("Round Rules")]
    public int baseEnemyCount = 5;
    public int enemiesPerRound = 2;
    public float roundDuration = 30f;

    [Header("Current Status")]
    public int currentRound = 0;
    private float currentTimer;
    private int aliveEnemyCount = 0;
    private bool isWaitingNextRound = false;

    private void OnEnable() => EnemyBase.OnEnemyDied += HandleEnemyDied;
    private void OnDisable() => EnemyBase.OnEnemyDied -= HandleEnemyDied;

    private void Start()
    {
        StartNextRound();
    }

    private void Update()
    {
        if (isWaitingNextRound) return;

        // 30초 타이머 진행
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            StartNextRound();
        }
    }

    private void StartNextRound()
    {
        isWaitingNextRound = false;
        currentRound++;
        currentTimer = roundDuration; // 30초 리셋

        // 몇 마리를 뽑을지 계산
        int spawnCount = baseEnemyCount + (currentRound - 1) * enemiesPerRound;
        aliveEnemyCount += spawnCount; // 살아있는 적 숫자 누적

        Debug.Log($"<color=cyan>라운드 {currentRound} 시작! ({spawnCount}마리 생성, 남은 시간: {currentTimer}초)</color>");

        // 🌟 스포너에게 생성을 '외주' 맡김
        spawner.SpawnEnemies(spawnCount);
    }

    private void HandleEnemyDied(GameObject enemy)
    {
        aliveEnemyCount--;

        // 적을 다 잡았을 경우
        if (aliveEnemyCount <= 0 && !isWaitingNextRound)
        {
            Debug.Log("<color=yellow>모든 적 처치! 5초 뒤 다음 라운드 시작.</color>");
            StartCoroutine(WaitAndNextRound(5f));
        }
    }

    private IEnumerator WaitAndNextRound(float delay)
    {
        isWaitingNextRound = true;
        yield return new WaitForSeconds(delay);
        StartNextRound();
    }
}