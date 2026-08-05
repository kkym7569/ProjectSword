using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetButtonDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private float destroyDragDistance = 100f;
    [SerializeField] private float maxVisualDragDistance = 60f;
    [SerializeField] private float returnDuration = 0.15f;

    private TargetManager manager;
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private int targetIndex;
    private Vector2 pointerDownPosition;
    private Vector2 originalAnchoredPosition;
    private bool tracking;
    private bool crossedDestroyDistance;
    private Coroutine returnCoroutine;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void Configure(TargetManager targetManager, int index)
    {
        manager = targetManager;
        targetIndex = index;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        tracking = manager != null && manager.IsHeldTarget(targetIndex);
        crossedDestroyDistance = false;
        pointerDownPosition = eventData.position;

        if (!tracking || rectTransform == null) return;

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        originalAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!tracking) return;

        Vector2 dragDelta = eventData.position - pointerDownPosition;
        crossedDestroyDistance = dragDelta.y >= destroyDragDistance
            && Mathf.Abs(dragDelta.y) > Mathf.Abs(dragDelta.x);

        if (rectTransform != null)
        {
            float canvasScale = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            float upwardDistance = Mathf.Max(0f, dragDelta.y / Mathf.Max(0.01f, canvasScale));
            float visualDistance = Mathf.Min(upwardDistance * 0.6f, maxVisualDragDistance);
            rectTransform.anchoredPosition = originalAnchoredPosition + Vector2.up * visualDistance;
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!tracking) return;

        Vector2 dragDelta = eventData.position - pointerDownPosition;
        bool shouldDestroy = crossedDestroyDistance
            && dragDelta.y >= destroyDragDistance
            && Mathf.Abs(dragDelta.y) > Mathf.Abs(dragDelta.x);

        tracking = false;
        crossedDestroyDistance = false;

        if (shouldDestroy)
        {
            if (manager.TryDestroyHeldTarget(targetIndex)) return;
        }

        ReturnToOriginalPosition();
    }

    private void ReturnToOriginalPosition()
    {
        if (rectTransform == null) return;

        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;

        if (returnDuration <= 0f)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
            returnCoroutine = null;
            yield break;
        }

        while (elapsed < returnDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            t = 1f - Mathf.Pow(1f - t, 3f);
            rectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                originalAnchoredPosition,
                t);
            yield return null;
        }

        rectTransform.anchoredPosition = originalAnchoredPosition;
        returnCoroutine = null;
    }
}
