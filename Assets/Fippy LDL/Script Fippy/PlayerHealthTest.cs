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

    private bool isDead = false;

    private SpriteRenderer[] allSprites;
    private PlayerControllerTest playerController; // เชื่อมกับ PlayerControllerTest
    private Collider2D playerCollider;

    private Color color100, color80, color60, color40, color20;
    private WaitForSeconds deathWaitTime = new WaitForSeconds(0.3f);

    private void Start()
    {
        currentHealth = maxHealth;

        // แคชคอมโพเนนต์ล่วงหน้าเพื่อประหยัดทรัพยากร
        allSprites = GetComponentsInChildren<SpriteRenderer>();
        playerController = GetComponent<PlayerControllerTest>();
        playerCollider = GetComponent<Collider2D>();

        // โค้ดสีตามระดับเลือด
        ColorUtility.TryParseHtmlString("#e9ff69", out color100);
        ColorUtility.TryParseHtmlString("#fff56e", out color80);
        ColorUtility.TryParseHtmlString("#ffd869", out color60);
        ColorUtility.TryParseHtmlString("#ffaf4a", out color40);
        ColorUtility.TryParseHtmlString("#fb5017", out color20);

        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

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

    // ฟังก์ชันรองรับดาเมจแบบ int จากสคริปต์ศัตรู
    public void TakeDamage(int damageAmount)
    {
        TakeDamage((float)damageAmount);
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

        Debug.Log("Player เลือดหมดแล้ว!");
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        if (playerController != null) playerController.TriggerDeath();

        // อนิเมชันตัวสั่นก่อนตาย 1 วินาที
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

        // ซ่อน Sprite และปิด Collider
        foreach (SpriteRenderer sprite in allSprites)
        {
            if (sprite != null) sprite.enabled = false;
        }

        if (playerCollider != null) playerCollider.enabled = false;

        yield return deathWaitTime;

        if (gameOverUI != null) gameOverUI.SetActive(true);

        Time.timeScale = 0f; // หยุดเวลาเกม
    }
}