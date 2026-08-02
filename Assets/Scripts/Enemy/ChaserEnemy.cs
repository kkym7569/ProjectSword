using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    [Header("2D Chaser Settings")]
    public float moveSpeed = 3f;

    private Transform currentTarget;

    private void Update()
    {
        FindNearestSpirit();

        if (currentTarget == null) return;

        Vector2 direction =
            (currentTarget.position - transform.position).normalized;

        transform.position +=
            (Vector3)direction * moveSpeed * Time.deltaTime;
    }


    private void FindNearestSpirit()
    {
        GameObject[] spirits = GameObject.FindGameObjectsWithTag("Spirit");

        float minDistance = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject spirit in spirits)
        {
            float distance = Vector2.Distance(
                transform.position,
                spirit.transform.position
            );

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = spirit.transform;
            }
        }

        currentTarget = nearest;
    }
}