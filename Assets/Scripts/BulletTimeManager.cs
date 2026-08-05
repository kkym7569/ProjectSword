using System.Collections;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BulletTimeManager : MonoBehaviour
{
    [Header("Bullet Time Settings")]
    [Range(0.01f, 1f)]
    [SerializeField] private float timeScale = 0.2f;

    [Header("Screen Overlay")]
    [SerializeField] private CanvasGroup overlay;
    [Range(0f, 1f)]
    [SerializeField] private float overlayAlpha = 0.3f;
    [Min(0f)]
    [SerializeField] private float overlayFadeDuration = 0.1f;

    [Header("Direction Selection")]
    [SerializeField] private DirectionSelector directionSelector;
    [Min(0f)]
    [SerializeField] private float selectionDuration = 3f;

    public bool IsActive { get; private set; }
    public bool IsSelecting { get; private set; }

    private float previousTimeScale = 1f;
    private float previousFixedDeltaTime = 0.02f;
    private Coroutine overlayFadeCoroutine;
    private Coroutine selectionCoroutine;
    private int selectionCount;
    private int maxSelectionCount;
    private Action<bool> selectionFinished;

    public void SetDirectionSelector(DirectionSelector selector)
    {
        directionSelector = selector;
    }

    public void BeginDirectionSelection(int maxSelections, Action<bool> onFinished)
    {
        FinishDirectionSelection(false, false);

        maxSelectionCount = Mathf.Max(0, maxSelections);
        selectionCount = 0;
        selectionFinished = onFinished;
        IsSelecting = maxSelectionCount > 0;

        if (!IsSelecting)
        {
            Action<bool> callback = selectionFinished;
            selectionFinished = null;
            callback?.Invoke(false);
            return;
        }

        BeginBulletTime();
        if (directionSelector != null) directionSelector.Show();
        selectionCoroutine = StartCoroutine(DirectionSelectionRoutine());
    }

    public bool TrySelectDirection(out Vector2 direction)
    {
        direction = Vector2.zero;
        if (!IsSelecting || directionSelector == null) return false;

        direction = directionSelector.GetDirection();
        selectionCount++;
        return true;
    }

    public void CompleteSelectionIfReady()
    {
        if (IsSelecting && selectionCount >= maxSelectionCount)
        {
            FinishDirectionSelection(false, true);
        }
    }

    public Vector2 GetRandomDirection()
    {
        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
        return direction.sqrMagnitude > 0f ? direction : Vector2.up;
    }

    private void Awake()
    {
        if (overlay == null) return;

        overlay.alpha = 0f;
        overlay.interactable = false;
        overlay.blocksRaycasts = false;
    }

    public void BeginBulletTime()
    {
        if (IsActive) return;

        previousTimeScale = Time.timeScale;
        previousFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = timeScale;
        Time.fixedDeltaTime = previousFixedDeltaTime * timeScale;
        IsActive = true;
        FadeOverlay(overlayAlpha);
    }

    public void EndBulletTime()
    {
        if (!IsActive) return;

        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = previousFixedDeltaTime;
        IsActive = false;
        FadeOverlay(0f);
    }

    private void OnDisable()
    {
        FinishDirectionSelection(false, false);

        if (IsActive)
        {
            Time.timeScale = previousTimeScale;
            Time.fixedDeltaTime = previousFixedDeltaTime;
            IsActive = false;
        }

        if (overlayFadeCoroutine != null)
        {
            StopCoroutine(overlayFadeCoroutine);
            overlayFadeCoroutine = null;
        }

        if (overlay != null)
        {
            overlay.alpha = 0f;
        }
    }

    private IEnumerator DirectionSelectionRoutine()
    {
        float elapsed = 0f;
        while (IsSelecting && elapsed < selectionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        selectionCoroutine = null;
        if (IsSelecting) FinishDirectionSelection(true, true);
    }

    private void FinishDirectionSelection(bool timedOut, bool invokeCallback)
    {
        if (selectionCoroutine != null)
        {
            StopCoroutine(selectionCoroutine);
            selectionCoroutine = null;
        }

        bool wasSelecting = IsSelecting;
        IsSelecting = false;
        if (directionSelector != null) directionSelector.Hide();
        EndBulletTime();

        Action<bool> callback = selectionFinished;
        selectionFinished = null;
        if (wasSelecting && invokeCallback) callback?.Invoke(timedOut);
    }

    private void FadeOverlay(float targetAlpha)
    {
        if (overlay == null) return;

        if (overlayFadeCoroutine != null)
        {
            StopCoroutine(overlayFadeCoroutine);
        }

        overlayFadeCoroutine = StartCoroutine(FadeOverlayRoutine(targetAlpha));
    }

    private IEnumerator FadeOverlayRoutine(float targetAlpha)
    {
        float startAlpha = overlay.alpha;

        if (overlayFadeDuration <= 0f)
        {
            overlay.alpha = targetAlpha;
            overlayFadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < overlayFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / overlayFadeDuration));
            yield return null;
        }

        overlay.alpha = targetAlpha;
        overlayFadeCoroutine = null;
    }
}
