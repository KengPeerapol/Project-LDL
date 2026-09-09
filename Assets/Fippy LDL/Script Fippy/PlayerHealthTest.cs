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

    [Header("Debug Settings")]
    public bool showDebugOnScreen = true;
    public bool enableConsoleLogs = true;

    private bool isDead = false;

    private SpriteRenderer[] allSprites;
    private PlayerControllerTest playerController;
    private Collider2D playerCollider;

    private Color color100, color80, color60, color40, color20;
    private WaitForSeconds deathWaitTime = new WaitForSeconds(0.3f);

    private void Start()
    {
        currentHealth = maxHealth;

        allSprites = GetComponentsInChildren<SpriteRenderer>();
        playerController = GetComponent<PlayerControllerTest>();
        playerCollider = GetComponent<Collider2D>();

        // ตรวจสอบการลากเชื่อมโยง Component
        ValidateComponents();

        // โค้ดสีตามระดับเลือด
        ColorUtility.TryParseHtmlString("#e9ff69", out color100);
        ColorUtility.TryParseHtmlString("#fff56e", out color80);
        ColorUtility.TryParseHtmlString("#ffd869", out color60);
        ColorUtility.TryParseHtmlString("#ffaf4a", out color40);
        ColorUtility.TryParseHtmlString("#fb5017", out color20);

        UpdateHealthBar();
    }

    private void ValidateComponents()
    {
        if (hpBarFill == null)
            Debug.LogWarning("<color=orange>[Health Debug] ยังไม่ได้ลาก Image หลอดเลือดมาใส่ในช่อง 'Hp Bar Fill'</color>");

        if (gameOverUI == null)
            Debug.LogWarning("<color=orange>[Health Debug] ยังไม่ได้ลาก GameOverPanel มาใส่ในช่อง 'Game Over UI'</color>");

        if (playerController == null)
            Debug.LogWarning("<color=orange>[Health Debug] ไม่พบคอมโพเนนต์ PlayerControllerTest บนตัว Player</color>");

        if (playerCollider == null)
            Debug.LogWarning("<color=orange>[Health Debug] ไม่พบคอมโพเนนต์ Collider2D บนตัว Player</color>");
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (enableConsoleLogs)
        {
            Debug.Log($"<color=red>[Health Debug] ได้รับดาเมจ: -{damageAmount} | HP คงเหลือ: {Mathf.Max(0, currentHealth):F0}/{maxHealth}</color>");
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

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        if (enableConsoleLogs)
        {
            Debug.Log($"<color=green>[Health Debug] ได้รับการฮีล: +{healAmount} | HP ปัจจุบัน: {currentHealth:F0}/{maxHealth}</color>");
        }

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = currentHealth / maxHealth;
        }

        HandlePlayerColor();
    }

    private void HandlePlayerColor()
    {
        if (allSprites == null || allSprites.Length == 0) return;

        Color targetColor;

        if (currentHealth <= 20f) targetColor = color20;
        else if (currentHealth <= 40f) targetColor = color40;
        else if (currentHealth <= 60f) targetColor = color60;
        else if (currentHealth <= 80f) targetColor = color80;
        else targetColor = color100;

        foreach (SpriteRenderer sprite in allSprites)
        {
            if (sprite != null) sprite.color = targetColor;
        }
    }

    private void Die()
    {
        isDead = true;

        if (hpBarFill != null) hpBarFill.fillAmount = 0f;

        if (enableConsoleLogs)
        {
            Debug.Log("<color=red><b>[Health Debug] Player เลือดหมดแล้ว! กำลังเริ่มอนิเมชันการตาย...</b></color>");
        }

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

        if (gameOverUI != null) gameOverUI.SetActive(true);

        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        if (!showDebugOnScreen) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 17;
        style.fontStyle = FontStyle.Bold;

        // วางกล่องแสดงเลือดไว้ด้านบนซ้าย (ใต้จุดแสดงสถานะยิงปืน)
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
            else style.normal.textColor = new Color(1f, 0.3f, 0f); // ส้ม-แดง

            GUI.Label(new Rect(posX, posY, 350, 30), $"HP: {currentHealth:F0} / {maxHealth:F0} ({hpPercent:F0}%)", style);
        }
    }
}