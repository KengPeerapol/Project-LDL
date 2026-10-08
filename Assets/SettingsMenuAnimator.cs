using System.Collections;
using UnityEngine;

public class SettingsMenuAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("ระยะเวลาอนิเมชัน (วินาที)")]
    public float animationDuration = 0.4f;

    [Tooltip("ขนาดเริ่มต้นตอนย่อตัว")]
    [Range(0f, 1f)]
    public float initialScale = 0.1f;

    [Tooltip("เส้นความโค้งของอนิเมชัน")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Coroutine currentRoutine;
    private bool isOpened = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        // หากเปิดเกมมาแล้วหน้าต่างเปิดค้างอยู่ ให้ซ่อนเก็บไว้ใต้จอ
        if (!isOpened)
        {
            rectTransform.anchoredPosition = GetBottomPosition();
            rectTransform.localScale = Vector3.one * initialScale;
            gameObject.SetActive(false);
        }
    }

    private void InitializeComponents()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        if (rectTransform != null)
        {
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    // ⭐ เรียกใช้จากปุ่มเปิด Setting
    public void OpenSettings()
    {
        // ⭐ บล็อกทันทีถ้าหน้าต่างเปิดอยู่แล้ว หรือกำลังเปิดอยู่ ป้องกันการกดปุ่มรัว
        if (isOpened) return;

        isOpened = true;
        InitializeComponents();

        Time.timeScale = 1f;

        // หยุดแอนิเมชันเก่าก่อนเริ่มใหม่
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        gameObject.SetActive(true);
        rectTransform.SetAsLastSibling(); // ยกมาอยู่เลเยอร์บนสุด

        Vector2 startPos = GetBottomPosition();
        Vector2 targetPos = Vector2.zero;

        Vector3 startScale = Vector3.one * initialScale;
        Vector3 targetScale = Vector3.one;

        // วางพิกัดเริ่มต้นไว้ใต้จอและย่อขนาด
        rectTransform.anchoredPosition = startPos;
        rectTransform.localScale = startScale;

        currentRoutine = StartCoroutine(AnimatePanel(startPos, targetPos, startScale, targetScale, null));
    }

    // ⭐ เรียกใช้จากปุ่มปิด Setting
    public void CloseSettings()
    {
        // ⭐ บล็อกทันทีถ้าหน้าต่างปิดอยู่แล้ว ป้องกันการสั่งปิดซ้ำ
        if (!isOpened) return;

        isOpened = false;
        InitializeComponents();

        Time.timeScale = 1f;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = GetBottomPosition();

        Vector3 startScale = rectTransform.localScale;
        Vector3 targetScale = Vector3.one * initialScale;

        currentRoutine = StartCoroutine(AnimatePanel(startPos, targetPos, startScale, targetScale, () =>
        {
            gameObject.SetActive(false); // ซ่อน Panel เมื่อหดและเลื่อนลงสุด
        }));
    }

    private IEnumerator AnimatePanel(Vector2 startPos, Vector2 targetPos, Vector3 startScale, Vector3 targetScale, System.Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = easeCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);

            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        rectTransform.localScale = targetScale;

        currentRoutine = null;
        onComplete?.Invoke();
    }

    private Vector2 GetBottomPosition()
    {
        float screenHeight = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>().rect.height : Screen.height;
        return new Vector2(0f, -screenHeight);
    }
}