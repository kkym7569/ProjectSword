using UnityEngine;

public class Spirit : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;

    private Vector2 targetPosition;

    public void SetTargetPosition(Vector2 position)
    {
        targetPosition = position;
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }
}