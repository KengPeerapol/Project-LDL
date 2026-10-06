using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthTest : MonoBehaviour
{
    [Header("ตั้งค่าเลือด")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI หลอดเลือด (ตั้ง Image Type เป็น Filled)")]
    public Image hpBarFill;

    [Header("หน้าต่าง UI ตอนแพ้ (GameOverPanel)")]
    public GameObject gameOverUI;

    [Header("Sprite Settings (สไปรต์ของตัว Player)")]
    [Tooltip("ลาก SpriteRenderer ลำตัวมาใส่ (ถ้าเว้นว่างไว้จะค้นหาให้อัตโนมัติ)")]
    public SpriteRenderer playerBodySprite;

    [Header("Hit Flash Effect (แสงกะพริบตอนโดนโจมตี)")]
    [Tooltip("เปิดใช้งานเอฟเฟกต์กะพริบตอนโดนดาเมจหรือไม่")]
    public bool enableHitFlash = true;

    [Tooltip("สีแฟลช (แนะนำสีแดงจัด Color.red หรือสีส้ม เพื่อให้ตัดกับสีเดิมชัดเจน)")]
    public Color hitFlashColor = Color.red; // ⭐ เปลี่ยนเป็นสีแดงเพื่อให้เห็นชัดเจน 100%

    [Tooltip("ระยะเวลาที่กะพริบ (วินาที แนะนำ 0.1 - 0.15)")]
    public float flashDuration = 0.12f;

    [Header("Screen Shake Settings (กล้องสั่นเมื่อโดนดาเมจ)")]
    [Tooltip("เปิดใช้งานระบบจอสั่นหรือไม่")]
    public bool enableScreenShake = true;
    public float shakeDuration = 0.18f;
    public float shakeMagnitude = 0.22f;
    public Camera targetCamera;

    [Header("Debug Settings")]
    public bool showDebugOnScreen = true;
    public bool enableConsoleLogs = true;

    private bool isDead = false;

    private SpriteRenderer[] allSprites;
    private PlayerControllerTest playerController;
    private Collider2D playerCollider;

    private Color color100, color80, color60, color40, color20;
    private WaitForSeconds deathWaitTime = new WaitForSeconds(0.3f);

    private Coroutine flashRoutine;
    private Coroutine shakeRoutine;
    private Vector3 cameraOriginalPos;

    private void Start()
    {
        currentHealth = maxHealth;

        // 1. ค้นหา SpriteRenderer (ตรวจทั้งตัวแม่และตัวลูก)
        if (playerBodySprite == null)
        {
            playerBodySprite = GetComponent<SpriteRenderer>();
            if (playerBodySprite == null)
            {
                playerBodySprite = GetComponentInChildren<SpriteRenderer>();
            }
        }

        allSprites = GetComponentsInChildren<SpriteRenderer>();
        playerController = GetComponent<PlayerControllerTest>();
        playerCollider = GetComponent<Collider2D>();

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            cameraOriginalPos = targetCamera.transform.localPosition;
        }

        if (gameOverUI == null && GameScoreManager.Instance != null)
        {
            gameOverUI = GameScoreManager.Instance.gameOverPanel;
        }

        ValidateComponents();

        // ระดับสีเลือด (100% -> 20%)
        ColorUtility.TryParseHtmlString("#e9ff69", out color100);
        ColorUtility.TryParseHtmlString("#fff56e", out color80);
        ColorUtility.TryParseHtmlString("#ffd869", out color60);
        ColorUtility.TryParseHtmlString("#ffaf4a", out color40);
        ColorUtility.TryParseHtmlString("#fb5017", out color20);

        UpdateHealthBar();
    }

    private void ValidateComponents()
    {
        if (playerBodySprite == null)
            Debug.LogError("<color=red>[Health Debug] ไม่พบ SpriteRenderer สำหรับทำ Hit Flash! กรุณาลากใส่ในช่อง 'Player Body Sprite'</color>");

        if (hpBarFill == null)
            Debug.LogWarning("<color=orange>[Health Debug] ยังไม่ได้ลาก Image หลอดเลือดมาใส่ในช่อง 'Hp Bar Fill'</color>");

        if (gameOverUI == null)
            Debug.LogWarning("<color=orange>[Health Debug] ยังไม่ได้ลาก GameOverPanel มาใส่ในช่อง 'Game Over UI'</color>");
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (enableConsoleLogs)
        {
            Debug.Log($"<color=orange>[Health Debug] ได้รับดาเมจ: -{damageAmount} | HP คงเหลือ: {Mathf.Max(0, currentHealth):F0}/{maxHealth}</color>");
        }

        // ⭐ ทำงาน Hit Flash
        if (enableHitFlash && playerBodySprite != null && gameObject.activeInHierarchy)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(HitFlashRoutine());
        }

        // ⭐ ทำงาน Screen Shake
        if (enableScreenShake && targetCamera != null)
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
                targetCamera.transform.localPosition = cameraOriginalPos;
            }
            shakeRoutine = StartCoroutine(ScreenShakeRoutine());
        }

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
        else
        {
            UpdateHealthBar();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        TakeDamage((float)damageAmount);
    }

    // ⭐ Coroutine แฟลชสีแดง แล้วสลับกลับเป็นสีเดิม
    private IEnumerator HitFlashRoutine()
    {
        playerBodySprite.color = hitFlashColor;

        yield return new WaitForSeconds(flashDuration);

        HandlePlayerColor(); // คืนค่าสีตามระดับเลือดปัจจุบัน
        flashRoutine = null;
    }

    private IEnumerator ScreenShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            targetCamera.transform.localPosition = new Vector3(
                cameraOriginalPos.x + offsetX,
                cameraOriginalPos.y + offsetY,
                cameraOriginalPos.z
            );

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        targetCamera.transform.localPosition = cameraOriginalPos;
        shakeRoutine = null;
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = currentHealth / maxHealth;
        }

        // ถ้าไม่ได้กำลังเล่น Hit Flash อยู่ ให้ปรับสีตามเลือดปกติ
        if (flashRoutine == null)
        {
            HandlePlayerColor();
        }
    }

    private void HandlePlayerColor()
    {
        if (playerBodySprite == null) return;

        Color targetColor;

        if (currentHealth <= 20f) targetColor = color20;
        else if (currentHealth <= 40f) targetColor = color40;
        else if (currentHealth <= 60f) targetColor = color60;
        else if (currentHealth <= 80f) targetColor = color80;
        else targetColor = color100;

        playerBodySprite.color = targetColor;
    }

    private void Die()
    {
        isDead = true;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            if (targetCamera != null) targetCamera.transform.localPosition = cameraOriginalPos;
        }

        if (hpBarFill != null) hpBarFill.fillAmount = 0f;

        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        if (playerController != null) playerController.TriggerDeath();

        float shakeDuration = 1f;
        float elapsed = 0f;
        Vector3 originalPos = transform.position;

        while (elapsed < shakeDuration)
        {
            float x = originalPos.x + Random.Range(-0.2f, 0.2f);
            float y = originalPos.y + Random.Range(-0.2f, 0.2f);
            transform.position = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;

        foreach (SpriteRenderer sprite in allSprites)
        {
            if (sprite != null) sprite.enabled = false;
        }

        if (playerCollider != null) playerCollider.enabled = false;

        yield return deathWaitTime;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (gameOverUI != null) gameOverUI.SetActive(true);

        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        if (!showDebugOnScreen) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 17;
        style.fontStyle = FontStyle.Bold;

        float posX = 20f;
        float posY = 80f;

        if (isDead)
        {
            style.normal.textColor = Color.red;
            GUI.Label(new Rect(posX, posY, 300, 30), "PLAYER STATUS: DEAD", style);
        }
        else
        {
            float hpPercent = (currentHealth / maxHealth) * 100f;

            if (hpPercent > 60f) style.normal.textColor = Color.green;
            else if (hpPercent > 30f) style.normal.textColor = Color.yellow;
            else style.normal.textColor = new Color(1f, 0.3f, 0f);

            GUI.Label(new Rect(posX, posY, 350, 30), $"HP: {currentHealth:F0} / {maxHealth:F0} ({hpPercent:F0}%)", style);
        }
    }
}