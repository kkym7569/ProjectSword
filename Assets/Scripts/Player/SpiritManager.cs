using UnityEngine;

public class SpiritManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMain player;
    [SerializeField] private Spirit[] spirits;

    [Header("Spirit Distance")]
    [SerializeField] private float spiritRadius = 2f;

    [Header("Idle Orbit")]
    [SerializeField] private float orbitSpeed = 60f;


    private Vector2 moveDirection = Vector2.up;

    private bool isPlayerMoving = false;

    private float orbitAngle = 0f;


    private void Awake()
    {
        player.OnAttackMoveStarted += HandleAttackMoveStarted;
        player.OnAttackMoveUpdated += HandleAttackMoveUpdated;
        player.OnAttackMoveEnded += HandleAttackMoveEnded;
    }


    private void OnDestroy()
    {
        player.OnAttackMoveStarted -= HandleAttackMoveStarted;
        player.OnAttackMoveUpdated -= HandleAttackMoveUpdated;
        player.OnAttackMoveEnded -= HandleAttackMoveEnded;
    }


    private void Update()
    {
        // 플레이어가 멈춰있으면 정령 회전
        if (!isPlayerMoving)
        {
            UpdateOrbit();
        }
    }


    private void HandleAttackMoveStarted(Vector2 startPos)
    {
        isPlayerMoving = true;
    }


    private void HandleAttackMoveUpdated(Vector2 startPos, Vector2 currentPos)
    {
        Vector2 dir = currentPos - startPos;

        if (dir.sqrMagnitude > 0.001f)
        {
            moveDirection = dir.normalized;
        }

        UpdateFormation(currentPos);
    }


    private void HandleAttackMoveEnded()
    {
        isPlayerMoving = false;
    }


    // 공격 이동 중 삼각형 진형
    private void UpdateFormation(Vector2 playerPosition)
    {
        if (spirits.Length < 3)
            return;


        Vector2 right = new Vector2(
            moveDirection.y,
            -moveDirection.x
        );


        Vector2[] offsets =
        {
            -moveDirection * spiritRadius + right * spiritRadius,
            -moveDirection * spiritRadius - right * spiritRadius,
             moveDirection * spiritRadius
        };


        for (int i = 0; i < spirits.Length; i++)
        {
            spirits[i].SetTargetPosition(
                playerPosition + offsets[i]
            );
        }
    }


    // 대기 상태 원형 회전
    private void UpdateOrbit()
    {
        if (spirits.Length < 3)
            return;


        orbitAngle += orbitSpeed * Time.deltaTime;


        Vector2 playerPosition = player.transform.position;


        for (int i = 0; i < spirits.Length; i++)
        {
            float angle =
                orbitAngle + (360f / spirits.Length) * i;


            float rad = angle * Mathf.Deg2Rad;


            Vector2 offset = new Vector2(
                Mathf.Cos(rad),
                Mathf.Sin(rad)
            ) * spiritRadius;


            spirits[i].SetTargetPosition(
                playerPosition + offset
            );
        }
    }
}