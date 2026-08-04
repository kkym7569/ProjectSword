using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 한 라운드에서 스폰할 적 종류와 수를 정의하는 데이터 컨테이너.
/// Project 창에서 우클릭 → Create → Defense Game → Round Data 로 생성.
/// </summary>
[CreateAssetMenu(fileName = "Round_01", menuName = "Defense Game/Round Data")]
public class RoundData : ScriptableObject
{
    [System.Serializable]
    public class EnemyWave
    {
        [Tooltip("스폰할 적 프리팹")]
        public GameObject enemyPrefab;

        [Tooltip("이 라운드에서 스폰할 총 수")]
        public int count = 10;

        [Tooltip("적 사이 스폰 간격 (초). 0이면 즉시 전부 소환")]
        public float spawnInterval = 0.5f;
    }

    [Header("라운드 기본 설정")]
    [Tooltip("라운드 번호 (표시용)")]
    public int roundNumber = 1;

    [Tooltip("라운드 시작 전 대기 시간 (초)")]
    public float startDelay = 3f;

    [Tooltip("라운드 제한 시간 (초). 0 이하면 무제한")]
    public float roundDuration = 60f;

    [Tooltip("라운드 클리어 후 다음 라운드까지 휴식 시간 (초)")]
    public float restDuration = 5f;

    [Header("적 구성")]
    [Tooltip("이 라운드에 등장하는 적 목록")]
    public List<EnemyWave> enemyWaves = new List<EnemyWave>();

    /// <summary>이 라운드의 총 적 수를 반환합니다.</summary>
    public int TotalEnemyCount
    {
        get
        {
            int total = 0;
            foreach (var wave in enemyWaves) total += wave.count;
            return total;
        }
    }
}
