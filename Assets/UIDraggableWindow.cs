using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggableWindow : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Window References")]
    [Tooltip("RectTransform ของหน้าต่าง (ปล่อยว่างจะดึงตัวมันเอง)")]
    public RectTransform windowRectTransform;

    [Tooltip("Canvas หลัก (ปล่อยว่างจะหาให้อัตโนมัติ)")]
    public Canvas canvas;

    [Header("Header Drag Settings (ตั้งค่าการลาก)")]
    [Tooltip("เปิดใช้งานให้ลากได้เฉพาะแถบด้านบนเท่านั้น")]
    public bool dragOnlyHeader = true;

    [Tooltip("ความสูงของแถบสีฟ้าด้านบน (วัดจากขอบบนลงมา มีหน่วยเป็นพิกเซล)")]
    public float headerHeight = 45f;

    [Tooltip("Dead Zone ฝั่งขวาของแถบ Header (เว้นพื้นที่ช่วงปุ่มปิดสีแดง ไม่ให้เผลอลากติด)")]
    public float headerRightDeadZone = 40f; // ⭐ Dead Zone ป้องกันคลิกโดนปุ่มปิดแล้วหน้าต่างขยับ

    [Header("Screen Boundary Dead Zone (จำกัดขอบเขตไม่ให้หลุดจอ)")]
    [Tooltip("ล็อกไม่ให้หน้าต่างถูกลากหลุดออกนอกจอ")]
    public bool clampToScreen = true;

    [Tooltip("ระยะ Dead Zone ห่างจากขอบจอ (Padding: X ซ้ายขวา, Y บนล่าง)")]
    public Vector2 screenDeadZone = new Vector2(10f, 10f); // ⭐ ระยะเว้นขอบจอ

    [Header("Scale Animation Settings (อนิเมชันย่อ-ขยาย)")]
    [Tooltip("เปิดใช้งานอนิเมชันย่อ-ขยายหรือไม่")]
    public bool useAnimation = true;

    [Tooltip("ติ๊กถูกเพื่อให้ซ่อนหน้าต่างอัตโนมัติเมื่อเปิดเกม")]
    public bool hideOnStart = true;

    [Tooltip("ระยะเวลาอนิเมชัน (วินาที)")]
    public float animationDuration = 0.25f;

    [Tooltip("ขนาดย่อเริ่มต้น (0 = หดหายไป)")]
    [Range(0f, 1f)]
    public float initialScale = 0f;

    [Tooltip("เส้นความโค้งของอนิเมชัน")]
    public AnimationCurve easeCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.7f, 1.08f),
        new Keyframe(1f, 1f)
    );

    private bool isDraggingAllowed = false;
    private Coroutine animRoutine;
    private bool isInitialized = false;
    private bool isClosing = false;
    private bool isUserOpening = false;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        if (!isUserOpening && hideOnStart)
        {
            if (windowRectTransform != null)
            {
                windowRectTransform.localScale = Vector3.one * initialScale;
            }
            gameObject.SetActive(false);
        }
    }

    private void Init()
    {
        if (isInitialized) return;

        if (windowRectTransform == null)
            windowRectTransform = GetComponent<RectTransform>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (windowRectTransform != null)
        {
            windowRectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        isInitialized = true;
    }

    public void OpenWindow()
    {
        isUserOpening = true;
        isClosing = false;

        Init();
        Time.timeScale = 1f;

        if (gameObject.activeSelf && animRoutine == null && windowRectTransform.localScale.x >= 0.95f)
        {
            windowRectTransform.SetAsLastSibling();
            return;
        }

        Vector3 startScale = Vector3.one * initialScale;
        if (gameObject.activeSelf && animRoutine != null)
        {
            startScale = windowRectTransform.localScale;
        }

        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }

        windowRectTransform.localScale = startScale;
        gameObject.SetActive(true);
        windowRectTransform.SetAsLastSibling();

        if (useAnimation)
        {
            Vector3 targetScale = Vector3.one;
            animRoutine = StartCoroutine(AnimateScale(startScale, targetScale, () =>
            {
                animRoutine = null;
            }));
        }
        else
        {
            windowRectTransform.localScale = Vector3.one;
        }
    }

    public void CloseWindow()
    {
        Init();
        Time.timeScale = 1f;

        if (!gameObject.activeSelf || isClosing) return;

        isClosing = true;
        isUserOpening = false;

        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }

        if (useAnimation && gameObject.activeInHierarchy)
        {
            Vector3 startScale = windowRectTransform.localScale;
            Vector3 targetScale = Vector3.one * initialScale;

            animRoutine = StartCoroutine(AnimateScale(startScale, targetScale, () =>
            {
                gameObject.SetActive(false);
                isClosing = false;
                animRoutine = null;
            }));
        }
        else
        {
            gameObject.SetActive(false);
            isClosing = false;
        }
    }

    private IEnumerator AnimateScale(Vector3 startScale, Vector3 targetScale, System.Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = easeCurve.Evaluate(t);

            windowRectTransform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);

            yield return null;
        }

        windowRectTransform.localScale = targetScale;
        onComplete?.Invoke();
    }

    // ----------------- ระบบตรวจจับการลากเมาส์ (Drag) + Dead Zone -----------------

    public void OnPointerDown(PointerEventData eventData)
    {
        if (windowRectTransform != null)
        {
            windowRectTransform.SetAsLastSibling();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (windowRectTransform == null) return;

        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
            windowRectTransform.localScale = Vector3.one;
            isClosing = false;
        }

        if (dragOnlyHeader)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                windowRectTransform,
                eventData.pressPosition,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                float topEdge = windowRectTransform.rect.yMax;
                float rightEdge = windowRectTransform.rect.xMax;

                // ตรวจสอบแกน Y: อยู่ในความสูงของแถบสีฟ้าหรือไม่
                bool insideHeaderY = (localPoint.y >= (topEdge - headerHeight) && localPoint.y <= topEdge);

                // ตรวจสอบแกน X: ตกอยู่ใน Dead Zone ฝั่งขวา (ตรงปุ่มปิดสีแดง) หรือไม่
                bool insideCloseButtonDeadZone = (localPoint.x >= (rightEdge - headerRightDeadZone));

                // ยอมให้ลากเฉพาะเมื่อกดโดนแถบ Header และ "ต้องไม่โดน Dead Zone ปุ่มปิด"
                if (insideHeaderY && !insideCloseButtonDeadZone)
                {
                    isDraggingAllowed = true;
                }
                else
                {
                    isDraggingAllowed = false;
                }
            }
        }
        else
        {
            isDraggingAllowed = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggingAllowed) return;
        if (windowRectTransform == null || canvas == null) return;

        // ขยับพิกัดตามเมาส์
        windowRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // ตรวจสอบและดักให้อยู่ในขอบจอ (Screen Boundary Dead Zone)
        if (clampToScreen)
        {
            ClampToScreenBounds();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDraggingAllowed = false;
    }

    // ⭐ คำนวณ Dead Zone ขอบจอ ไม่ให้หน้าต่างหลุดออกนอก Canvas
    private void ClampToScreenBounds()
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null) return;

        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 windowSize = windowRectTransform.rect.size;

        float minX = (-canvasSize.x * 0.5f) + (windowSize.x * 0.5f) + screenDeadZone.x;
        float maxX = (canvasSize.x * 0.5f) - (windowSize.x * 0.5f) - screenDeadZone.x;
        float minY = (-canvasSize.y * 0.5f) + (windowSize.y * 0.5f) + screenDeadZone.y;
        float maxY = (canvasSize.y * 0.5f) - (windowSize.y * 0.5f) - screenDeadZone.y;

        // กรณีขนาดหน้าต่างใหญ่กว่าจอ ให้ตรึงไว้ตรงกลาง
        if (minX > maxX) { minX = 0f; maxX = 0f; }
        if (minY > maxY) { minY = 0f; maxY = 0f; }

        Vector2 currentPos = windowRectTransform.anchoredPosition;
        currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
        currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);

        windowRectTransform.anchoredPosition = currentPos;
    }

    // ⭐ แสดงขอบเขตพื้นที่ลาก (สีเขียว) และ Dead Zone ของปุ่มปิด (สีแดง) ใน Scene View
    private void OnDrawGizmosSelected()
    {
        if (windowRectTransform == null)
            windowRectTransform = GetComponent<RectTransform>();

        if (windowRectTransform == null || !dragOnlyHeader) return;

        Rect rect = windowRectTransform.rect;
        Matrix4x4 prevMatrix = Gizmos.matrix;
        Gizmos.matrix = windowRectTransform.localToWorldMatrix;

        // 1. พื้นที่แถบ Header ที่อนุญาตให้ลากได้ (สีเขียว)
        float activeWidth = Mathf.Max(0.01f, rect.width - headerRightDeadZone);
        Vector3 dragCenter = new Vector3(rect.xMin + (activeWidth * 0.5f), rect.yMax - (headerHeight * 0.5f), 0f);
        Vector3 dragSize = new Vector3(activeWidth, Mathf.Max(0.01f, headerHeight), 0f);

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.35f);
        Gizmos.DrawCube(dragCenter, dragSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(dragCenter, dragSize);

        // 2. Dead Zone ปุ่มปิดมุมขวาบน (สีแดง)
        if (headerRightDeadZone > 0f)
        {
            Vector3 deadZoneCenter = new Vector3(rect.xMax - (headerRightDeadZone * 0.5f), rect.yMax - (headerHeight * 0.5f), 0f);
            Vector3 deadZoneSize = new Vector3(headerRightDeadZone, Mathf.Max(0.01f, headerHeight), 0f);

            Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
            Gizmos.DrawCube(deadZoneCenter, deadZoneSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(deadZoneCenter, deadZoneSize);
        }

        Gizmos.matrix = prevMatrix;
    }
}