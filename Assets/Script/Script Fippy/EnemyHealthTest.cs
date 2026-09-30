using System.Collections;
using UnityEngine;

public class EnemyHealthTest : MonoBehaviour
{
    [Header("Lifetime Settings (ระยะเวลาการคงอยู่)")]
    [Tooltip("เปิดใช้งานให้ศัตรูหายไปเองอัตโนมัติหรือไม่")]
    public bool autoDestroy = true;

    [Tooltip("ระยะเวลาที่ศัตรูจะมีชีวิตอยู่ก่อนจะหายไป (วินาที)")]
    public float lifeTime = 4f; // ⭐ ตั้งค่า 4 วินาที

    [Header("Health Settings (การตั้งค่าเลือด)")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Score Reward (คะแนนที่ได้รับ)")]
    public int scoreReward = 20;

    [Header("Player Collision (การชนกับผู้เล่น)")]
    [Tooltip("ดาเมจที่ทำใส่ Player เมื่อบินชนกัน")]
    public int contactDamage = 15; // จำนวนเลือดที่ลดเมื่อชนตัวผู้เล่น

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
        if (spriteRenderer != null) originalColor = spriteRenderer.color; //
    }

    private void Start()
    {
        currentHealth = maxHealth; //[cite: 13]

        // ⭐ ตั้งเวลานับถอยหลัง 4 วินาทีเพื่อทำลายตัวเองทิ้งอัตโนมัติ
        if (autoDestroy)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; //[cite: 13]

        currentHealth -= damage; //[cite: 13]

        if (spriteRenderer != null && gameObject.activeInHierarchy) //[cite: 13]
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine); //[cite: 13]
            flashCoroutine = StartCoroutine(FlashHitRoutine()); //[cite: 13]
        }

        if (currentHealth <= 0f) //[cite: 13]
        {
            currentHealth = 0f; //[cite: 13]
            Die(); //[cite: 13]
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage((float)damage); //[cite: 13]
    }

    private IEnumerator FlashHitRoutine()
    {
        spriteRenderer.color = hitFlashColor; //[cite: 13]
        yield return new WaitForSeconds(flashDuration); //[cite: 13]
        if (spriteRenderer != null) spriteRenderer.color = originalColor; //[cite: 13]
    }

    // ตรวจจับการชนแบบ Collider ปกติ
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject); //[cite: 13]
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject); //[cite: 13]
    }

    // ตรวจจับการชนแบบ Is Trigger
    private void OnTriggerEnter2D(Collider2D collider)
    {
        HandlePlayerCollision(collider.gameObject); //[cite: 13]
    }

    private void HandlePlayerCollision(GameObject target)
    {
        if (isDead) return; //[cite: 13]

        if (target.CompareTag("Player") || target.transform.root.CompareTag("Player")) //[cite: 13]
        {
            if (Time.time < lastHitPlayerTime + damageCooldown) return; //[cite: 13]
            lastHitPlayerTime = Time.time; //[cite: 13]

            PlayerHealthTest healthTest = target.GetComponentInParent<PlayerHealthTest>(); //[cite: 13]
            if (healthTest != null) //[cite: 13]
            {
                healthTest.TakeDamage(contactDamage); //[cite: 13]
                Debug.Log($"<color=red>[Enemy Collision] ชน Player! ลดเลือด {contactDamage}</color>"); //[cite: 13]
            }
            else
            {
                PlayerHealthTest standardHealth = target.GetComponentInParent<PlayerHealthTest>(); //[cite: 13]
                if (standardHealth != null) //[cite: 13]
                {
                    standardHealth.TakeDamage(contactDamage); //[cite: 13]
                }
            }

            if (destroyOnHitPlayer) //[cite: 13]
            {
                Die(); //[cite: 13]
            }
        }
    }

    private void Die()
    {
        if (isDead) return; //[cite: 13]
        isDead = true; //[cite: 13]

        // 1. เพิ่มแต้มเข้า GameManager
        if (GameScoreManager.Instance != null) //[cite: 13]
        {
            GameScoreManager.Instance.AddScore(scoreReward); //[cite: 13]
        }

        // 2. ปล่อยหนามพุ่งกระจายรอบตัว
        if (spawnSpikesOnDeath && spikePrefab != null) //[cite: 13]
        {
            SpawnBouncingSpikes(); //[cite: 13]
        }

        // 3. เอฟเฟกต์ระเบิด
        if (deathEffectPrefab != null) //[cite: 13]
        {
            Quaternion randomRot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)); //[cite: 13]
            Instantiate(deathEffectPrefab, transform.position, randomRot); //[cite: 13]
        }

        Destroy(gameObject); //[cite: 13]
    }

    private void SpawnBouncingSpikes()
    {
        float startAngle = 0f; //[cite: 13]

        if (useEnemyRotation) //[cite: 13]
        {
            startAngle += transform.eulerAngles.z; //[cite: 13]
        }

        if (randomizeSpikeAngle) //[cite: 13]
        {
            startAngle += Random.Range(0f, 360f); //[cite: 13]
        }

        float angleStep = 360f / spikeCount; //[cite: 13]

        for (int i = 0; i < spikeCount; i++) //[cite: 13]
        {
            float angle = startAngle + (i * angleStep); //[cite: 13]
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right; //[cite: 13]

            GameObject newSpike = Instantiate(spikePrefab, transform.position, Quaternion.identity); //[cite: 13]

            if (newSpike.TryGetComponent(out BouncingSpikeTest spikeScript)) //[cite: 13]
            {
                spikeScript.Setup(direction, spikeSpeed); //[cite: 13]
            }
        }
    }
}