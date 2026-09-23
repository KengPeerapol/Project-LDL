using System.Collections;
using UnityEngine;

public class EnemyHealthTest : MonoBehaviour
{
    [Header("Health Settings (การตั้งค่าเลือด)")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Score Reward (คะแนนที่ได้รับ)")]
    public int scoreReward = 20;

    [Header("Player Collision (การชนกับผู้เล่น)")]
    [Tooltip("ดาเมจที่ทำใส่ Player เมื่อบินชนกัน")]
    public int contactDamage = 15; // ⭐ จำนวนเลือดที่ลดเมื่อชนตัวผู้เล่น

    [Tooltip("เมื่อชน Player แล้ว ศัตรูตัวนี้จะระเบิด/ตายทันทีหรือไม่")]
    public bool destroyOnHitPlayer = true;

    [Tooltip("คูลดาวน์ดาเมจ (กรณี destroyOnHitPlayer = false)")]
    public float damageCooldown = 0.5f;
    private float lastHitPlayerTime = -999f;

    [Header("Spike Death Settings (ปล่อยหนามตอนตาย)")]
    [Tooltip("ติ๊กถูกถ้าต้องการให้ตัวนี้ปล่อยหนามตอนตาย")]
    public bool spawnSpikesOnDeath = true;
    public GameObject spikePrefab;
    public int spikeCount = 4;
    public float spikeSpeed = 6f;

    [Header("Spike Rotation Variation (การหมุนทิศทางหนาม)")]
    public bool randomizeSpikeAngle = true;
    public bool useEnemyRotation = true;

    [Header("Hit Flash Effect")]
    public Color hitFlashColor = Color.red;
    public float flashDuration = 0.08f;

    [Header("Death Effects")]
    public GameObject deathEffectPrefab;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;
    private bool isDead = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (spriteRenderer != null && gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashHitRoutine());
        }

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage((float)damage);
    }

    private IEnumerator FlashHitRoutine()
    {
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(flashDuration);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    // ⭐ 1. ตรวจจับการชนแบบ Collider ปกติ (แข็ง ชนแล้วเด้ง)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    // ⭐ 2. ตรวจจับการชนแบบ Is Trigger (ทะลุผ่าน)
    private void OnTriggerEnter2D(Collider2D collider)
    {
        HandlePlayerCollision(collider.gameObject);
    }

    private void HandlePlayerCollision(GameObject target)
    {
        if (isDead) return;

        // เช็กว่าวัตถุที่ชนมี Tag "Player" หรือเป็นส่วนลูกของ Player หรือไม่
        if (target.CompareTag("Player") || target.transform.root.CompareTag("Player"))
        {
            if (Time.time < lastHitPlayerTime + damageCooldown) return;
            lastHitPlayerTime = Time.time;

            // ค้นหาสคริปต์เลือดของ Player
            PlayerHealthTest healthTest = target.GetComponentInParent<PlayerHealthTest>();
            if (healthTest != null)
            {
                healthTest.TakeDamage(contactDamage);
                Debug.Log($"<color=red>[Enemy Collision] ชน Player! ลดเลือด {contactDamage}</color>");
            }
            else
            {
                PlayerHealth standardHealth = target.GetComponentInParent<PlayerHealth>();
                if (standardHealth != null)
                {
                    standardHealth.TakeDamage(contactDamage);
                }
            }

            // ถ้ากำหนดให้ชนแล้วศัตรูสลาย/ตายทันที
            if (destroyOnHitPlayer)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. เพิ่มแต้มเข้า GameManager
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.AddScore(scoreReward);
        }

        // 2. ปล่อยหนามพุ่งกระจายรอบตัว
        if (spawnSpikesOnDeath && spikePrefab != null)
        {
            SpawnBouncingSpikes();
        }

        // 3. เอฟเฟกต์ระเบิด
        if (deathEffectPrefab != null)
        {
            Quaternion randomRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            Instantiate(deathEffectPrefab, transform.position, randomRot);
        }

        Destroy(gameObject);
    }

    private void SpawnBouncingSpikes()
    {
        float startAngle = 0f;

        if (useEnemyRotation)
        {
            startAngle += transform.eulerAngles.z;
        }

        if (randomizeSpikeAngle)
        {
            startAngle += Random.Range(0f, 360f);
        }

        float angleStep = 360f / spikeCount;

        for (int i = 0; i < spikeCount; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;

            GameObject newSpike = Instantiate(spikePrefab, transform.position, Quaternion.identity);

            if (newSpike.TryGetComponent(out BouncingSpikeTest spikeScript))
            {
                spikeScript.Setup(direction, spikeSpeed);
            }
        }
    }
}