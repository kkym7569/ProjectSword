using UnityEngine;

public class DirectionSelector : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float rotateSpeed = 180f;

    private float angle;
    public static DirectionSelector CreateRuntime(Transform parent)
    {
        GameObject selectorObject = new GameObject("DirectionSelector");
        selectorObject.transform.SetParent(parent, false);

        Mesh mesh = new Mesh { name = "DirectionTriangle" };
        mesh.vertices = new[]
        {
            new Vector3(0f, 0.65f, 0f),
            new Vector3(-0.45f, -0.4f, 0f),
            new Vector3(0.45f, -0.4f, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2 };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = selectorObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = selectorObject.AddComponent<MeshRenderer>();
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
        {
            meshRenderer.material = new Material(spriteShader);
        }
        meshRenderer.sortingOrder = 40;

        return selectorObject.AddComponent<DirectionSelector>();
    }

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