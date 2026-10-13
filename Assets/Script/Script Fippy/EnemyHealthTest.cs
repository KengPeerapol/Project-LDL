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

    [Header("Spawn On Death (เกิดตัวใหม่/แต้มเมื่อตาย - เป็นตัวเลือก)")]
    [Tooltip("Prefab ศัตรูที่จะเกิดออกมาแทนที่เมื่อตัวนี้ตาย (เช่น Boomer แตกตัวเป็นตัวปกติ ถ้าไม่ใส่จะข้าม)")]
    public GameObject nextEnemyPrefab;

    [Tooltip("Prefab แต้มคะแนนที่จะเกิดตอนตายจากการยิง (ถ้าไม่ใส่จะไม่ดรอปแต้ม)")]
    public GameObject pointPrefab;

    [Tooltip("คะแนนที่จะได้รับเมื่อกำจัดศัตรูตัวนี้ได้")]
    public int scoreReward = 20;

    [Tooltip("ให้ตัวที่เกิดใหม่ (ศัตรู/แต้ม) เคลื่อนที่ตามทิศทางและความเร็วเดิมของตัวนี้")]
    public bool inheritVelocity = true;

    [Header("Player Collision (การชนกับผู้เล่น)")]
    [Tooltip("ดาเมจที่ทำใส่ Player เมื่อบินชนกัน")]
    public int contactDamage = 15;

    [Tooltip("ติ๊กถูกเพื่อให้ผู้เล่นได้รับคะแนนทันทีเมื่อบินชนกัน")]
    public bool giveScoreOnPlayerHit = true; // ⭐ ให้แต้มทันทีเมื่อชนผู้เล่น

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
    private bool killedByPlayerCollision = false; // ตรวจจับว่าตายจากการชน Player หรือไม่

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
                healthTest.TakeDamage(contactDamage);
                Debug.Log($"<color=red>[Enemy Collision] ชน Player! ลดเลือด {contactDamage}</color>");
            }

            // ⭐ 1. เพิ่มแต้มให้ผู้เล่นทันทีเมื่อเกิดการบินชนกัน
            if (giveScoreOnPlayerHit && GameScoreManager.Instance != null)
            {
                GameScoreManager.Instance.AddScore(scoreReward);
                Debug.Log($"<color=green>[Enemy Collision] ชนศัตรูสำเร็จ! ได้รับคะแนน: +{scoreReward}</color>");
            }

            if (destroyOnHitPlayer)
            {
                killedByPlayerCollision = true; // มาร์กไว้ว่าตายจากการชน
                Die();
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Vector2 enemyVelocity = Vector2.left * 4f;
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            if (rb.linearVelocity.sqrMagnitude > 0.05f)
            {
                enemyVelocity = rb.linearVelocity;
            }
        }

        // 1. ตัวเลือก: เสกศัตรูตัวใหม่ (เช่น Boomer แตกตัวเป็น Enemy ปกติ)
        if (nextEnemyPrefab != null)
        {
            GameObject spawnedEnemy = Instantiate(nextEnemyPrefab, transform.position, Quaternion.identity);

            if (inheritVelocity && spawnedEnemy.TryGetComponent<Rigidbody2D>(out var spawnedRb))
            {
                spawnedRb.linearVelocity = enemyVelocity;

                if (spawnedEnemy.TryGetComponent<MovementTest>(out var moveScript))
                {
                    moveScript.UpdateRotation();
                }
            }
        }

        // ⭐ 2. ดรอปแต้ม Point Prefab (จะดรอปเฉพาะตอนโดนยิงตายเท่านั้น เพื่อไม่ให้ได้แต้มซ้ำซ้อนตอนบินชน)
        if (pointPrefab != null && (!killedByPlayerCollision || !giveScoreOnPlayerHit))
        {
            GameObject dropPoint = Instantiate(pointPrefab, transform.position, Quaternion.identity);

            if (dropPoint.TryGetComponent(out PointItem pointScript))
            {
                pointScript.Setup(scoreReward, inheritVelocity ? enemyVelocity : Vector2.left * 4f);
            }
        }

        // 3. ปล่อยหนามกระจายรอบตัว
        if (spawnSpikesOnDeath && spikePrefab != null)
        {
            SpawnBouncingSpikes();
        }

        // 4. เอฟเฟกต์ระเบิด
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