using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs & Positions")]
    public GameObject[] enemyPrefabs;
    public Transform player;
    public float spawnRadius = 8f;

    private void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // 매니저가 호출할 퍼블릭 함수
    public void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos2D = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = player.position + new Vector3(spawnPos2D.x, spawnPos2D.y, 0);

            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);
        }
    }
}