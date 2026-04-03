using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public TargetManager manager;
    public float speed = 5.0f;

    private Transform currentTarget;
    private bool isMoving = false;

    public void OnMoveToPoint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // 매니저에게 현재 몇 번 타겟으로 가야 하는지 물어봄
            currentTarget = manager.GetCurrentTargetTransform();

            if (currentTarget != null)
            {
                isMoving = true;
            }
        }
    }

    void Update()
    {
        if (isMoving && currentTarget != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                currentTarget.position,
                speed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 게 "Target" 태그이고, 현재 내가 목표로 하던 바로 그 타겟이라면
        if (collision.CompareTag("Target") && collision.transform == currentTarget)
        {
            isMoving = false;
            manager.TargetEaten(); // 매니저에게 먹었다고 알림
        }
    }
}