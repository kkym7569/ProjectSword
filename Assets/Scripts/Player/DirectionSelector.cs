using UnityEngine;

public class DirectionSelector : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float rotateSpeed = 180f;

    private float angle;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        angle += rotateSpeed * Time.unscaledDeltaTime;

        float rad = angle * Mathf.Deg2Rad;

        Vector2 dir = new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        );

        transform.localPosition = dir * radius;

        // 화살표가 진행 방향을 바라보게
        transform.up = dir;
    }

    public void Show()
    {
        angle = 0f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public Vector2 GetDirection()
    {
        float rad = angle * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        ).normalized;
    }
}