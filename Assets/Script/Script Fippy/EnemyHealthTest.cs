using System.Collections;
using UnityEngine;

public class EnemyHealthTest : MonoBehaviour
{
    [Header("Lifetime Settings (ระยะเวลาการคงอยู่)")]
    [Tooltip("เปิดใช้งานให้ศัตรูหายไปเองอัตโนมัติหรือไม่")]
    public bool autoDestroy = true;

    [Tooltip("ระยะเวลาที่ศัตรูจะมีชีวิตอยู่ก่อนจะหายไป (วินาที)")]
    public float lifeTime = 4f;

    [Header("Health Settings (การตั้งค่าเลือด)")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Score Reward (คะแนนที่ได้รับ)")]
    public int scoreReward = 20;

    [Header("Player Collision (การชนกับผู้เล่น)")]
    [Tooltip("พิมพ์ชื่อประเภทศัตรูเพื่อเล่นเสียงเจ็บให้ถูก (เช่น LDL, Plaque)")]
    public string enemyDamageType = "LDL"; // ⭐ เพิ่มช่องตั้งชื่อประเภทศัตรูตรงนี้

    [Tooltip("ดาเมจที่ทำใส่ Player เมื่อบินชนกัน")]
    public int contactDamage = 15;

    [Tooltip("เมื่อชน Player แล้ว ศัตรูตัวนี้จะระเบิด/ตายทันทีหรือไม่")]
    public bool destroyOnHitPlayer = true;

    [Tooltip("คูลดาวน์ดาเมจ (กรณี destroyOnHitPlayer = false)")]
    public float damageCooldown = 0.5f;
    private float lastHitPlayerTime = -999f;

    [Header("Spike Death Settings (ปล่อยหนามตอนตาย)")]
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

    public GameObject pointPrefab;

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

        if (autoDestroy)
        {
            Destroy(gameObject, lifeTime);
        }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        HandlePlayerCollision(collider.gameObject);
    }

    private void HandlePlayerCollision(GameObject target)
    {
        if (isDead) return;

        if (target.CompareTag("Player") || target.transform.root.CompareTag("Player"))
        {
            if (Time.time < lastHitPlayerTime + damageCooldown) return;
            lastHitPlayerTime = Time.time;

            PlayerHealthTest healthTest = target.GetComponentInParent<PlayerHealthTest>();
            if (healthTest != null)
            {
                // ⭐ ส่งข้อความประเภทศัตรูไปหาผู้เล่น (เช่น ส่งคำว่า LDL หรือ Plaque)
                healthTest.TakeDamage(contactDamage, enemyDamageType);
                GameScoreManager.Instance.AddScore(scoreReward);
                Debug.Log($"<color=red>[Enemy Collision] ชน Player! ลดเลือด {contactDamage}</color>");
            }
            else
            {
                PlayerHealthTest standardHealth = target.GetComponentInParent<PlayerHealthTest>();
                if (standardHealth != null)
                {
                    // ⭐ ส่งข้อความประเภทศัตรูไปหาผู้เล่น
                    standardHealth.TakeDamage(contactDamage, enemyDamageType);
                }
            }

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

        if (spawnSpikesOnDeath && spikePrefab != null)
        {
            Instantiate(pointPrefab, transform.position, Quaternion.identity);
            SpawnBouncingSpikes();
        }

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