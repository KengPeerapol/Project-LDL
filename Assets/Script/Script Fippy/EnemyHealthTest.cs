using System.Collections;
using UnityEngine;

public class EnemyHealthTest : MonoBehaviour
{
    [System.Serializable]
    public class EvolutionStage
    {
        [Tooltip("ชื่อร่างช่วยจำ (เช่น ร่างที่ 2, Point วงกลม, Point 1)")]
        public string stageName = "Next Stage";

        [Tooltip("Prefab ของศัตรู/แต้มร่างนี้")]
        public GameObject enemyPrefab;

        [Tooltip("เลือดของร่างนี้ (ใส่ 0 = ใช้เลือดตาม Prefab เดิม)")]
        public float maxHealth = 0f;

        [Tooltip("ดาเมจที่ทำใส่ Player เมื่อชน (ใส่ 0 = ชนแล้วไม่เสียเลือด เป็นแต้มฟรี)")]
        public int contactDamage = 0;

        [Tooltip("คะแนนที่จะได้รับจากร่างนี้")]
        public int scoreReward = 15;

        [Tooltip("⭐ ติ๊กถูก = ร่างนี้จะไม่สามารถโดนยิงหรือตายได้ (กระสุนยิงทะลุผ่าน) แต่ผู้เล่นยังบินชนเก็บแต้มได้")]
        public bool isInvulnerable = false; // ช่องติ๊กถูกในแต่ละร่าง
    }

    [Header("Lifetime Settings (ระยะเวลาการคงอยู่)")]
    public bool autoDestroy = true;
    public float lifeTime = 5f;

    [Header("Health Settings (การตั้งค่าเลือด)")]
    public float maxHealth = 50f;
    private float currentHealth;

    [Header("Current Enemy Settings (ค่าประจำตัวของร่างนี้)")]
    [Tooltip("ดาเมจที่ร่างนี้ทำใส่ Player เมื่อชน (ใส่ 0 = ชนฟรีไม่เสียเลือด)")]
    public int contactDamage = 10;

    [Tooltip("คะแนนที่ได้รับจากร่างนี้")]
    public int scoreReward = 5;

    [Tooltip("⭐ ติ๊กถูก = ตัวนี้จะไม่โดนยิงและยิงไม่ตาย (ใช้สำหรับร่างตั้งต้น)")]
    public bool isInvulnerable = false;

    [Header("Sequential Evolution (ตั้งค่าสายเปลี่ยนร่าง)")]
    public EvolutionStage[] evolutionChain;
    public int currentChainIndex = 0;

    [Tooltip("ความเร็วพื้นฐานในการบินไปทางซ้ายของร่างใหม่")]
    public float defaultFlySpeed = 4f;

    [Tooltip("ระยะเวลาป้องกันการตายซ้อนในเฟรมเดียวกัน (ใช้เฉพาะร่างที่มีดาเมจ)")]
    public float spawnGracePeriod = 0.25f;
    private float graceTimer = 0f;

    [Tooltip("Prefab แต้มคะแนนสำรอง (ใช้เฉพาะศัตรูที่ไม่มีสายเปลี่ยนร่าง)")]
    public GameObject pointPrefab;
    public bool inheritVelocity = true;

    [Header("Player Collision (การชนกับผู้เล่น)")]
    public bool giveScoreOnPlayerHit = true;
    public bool destroyOnHitPlayer = true;
    public float damageCooldown = 0.3f;
    private float lastHitPlayerTime = -999f;

    [Header("Spike Death Settings (ปล่อยหนามตอนตาย)")]
    [Tooltip("ติ๊กถูกถ้าต้องการให้ตัวนี้ปล่อยหนามตอนตาย (ตัวที่เป็นแต้มดาเมจ 0 จะไม่ปล่อยหนาม)")]
    public bool spawnSpikesOnDeath = true;
    public GameObject spikePrefab;
    public int spikeCount = 4;
    public float spikeSpeed = 6f;

    [Header("Spike Rotation Variation")]
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
    private bool killedByPlayerCollision = false;

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

    private void Update()
    {
        if (graceTimer > 0f)
        {
            graceTimer -= Time.deltaTime;
        }
    }

    public void SetSpawnGrace(float duration)
    {
        graceTimer = duration;
    }

    // ⭐ ระบบรับดาเมจ: หากติดสถานะ isInvulnerable จะไม่รับดาเมจและไม่ตาย
    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

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

    private void OnTriggerStay2D(Collider2D collider)
    {
        HandlePlayerCollision(collider.gameObject);
    }

    // ⭐ ผู้เล่นยังคงบินชนเพื่อเก็บแต้มได้ตามปกติ 100%
    private void HandlePlayerCollision(GameObject target)
    {
        if (isDead) return;

        if (contactDamage > 0 && graceTimer > 0f) return;

        if (target.CompareTag("Player") || target.transform.root.CompareTag("Player"))
        {
            if (Time.time < lastHitPlayerTime + damageCooldown) return;
            lastHitPlayerTime = Time.time;

            PlayerHealthTest healthTest = target.GetComponentInParent<PlayerHealthTest>();
            if (healthTest != null)
            {
                if (contactDamage > 0)
                {
                    healthTest.TakeDamage(contactDamage, "Enemy");
                    Debug.Log($"<color=red>[Collision] ชน Player! เสียเลือด: -{contactDamage}</color>");
                }
                else
                {
                    Debug.Log($"<color=cyan>[Collision] เก็บแต้มสำเร็จ! (ดาเมจ 0 ไม่เสียเลือด)</color>");
                }
            }

            if (giveScoreOnPlayerHit && GameScoreManager.Instance != null && scoreReward > 0)
            {
                GameScoreManager.Instance.AddScore(scoreReward);
                Debug.Log($"<color=green>[Score] ได้รับคะแนน: +{scoreReward}</color>");
            }

            if (destroyOnHitPlayer)
            {
                killedByPlayerCollision = true;
                Die();
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Vector2 forwardVelocity = Vector2.left * defaultFlySpeed;
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            float speedX = Mathf.Abs(rb.linearVelocity.x);
            if (speedX < 1f) speedX = defaultFlySpeed;

            forwardVelocity = new Vector2(-speedX, rb.linearVelocity.y);
        }

        // 1. เสกตัวถัดไปตาม Evolution Chain เฉพาะเมื่อโดนยิงตายเท่านั้น
        bool hasNextStage = false;
        if (!killedByPlayerCollision && evolutionChain != null && currentChainIndex < evolutionChain.Length)
        {
            EvolutionStage nextStage = evolutionChain[currentChainIndex];
            if (nextStage != null && nextStage.enemyPrefab != null)
            {
                hasNextStage = true;

                GameObject spawnedEnemy = Instantiate(nextStage.enemyPrefab, transform.position, Quaternion.identity);

                if (spawnedEnemy.TryGetComponent(out EnemyHealthTest nextHealth))
                {
                    nextHealth.evolutionChain = this.evolutionChain;
                    nextHealth.currentChainIndex = this.currentChainIndex + 1;
                    nextHealth.contactDamage = nextStage.contactDamage;
                    nextHealth.scoreReward = nextStage.scoreReward;

                    // ⭐ ส่งต่อค่าสถานะอมตะไปยังร่างใหม่ที่เกิดมา
                    nextHealth.isInvulnerable = nextStage.isInvulnerable;

                    if (nextStage.maxHealth > 0f)
                    {
                        nextHealth.maxHealth = nextStage.maxHealth;
                        nextHealth.currentHealth = nextStage.maxHealth;
                    }

                    if (nextStage.contactDamage > 0)
                    {
                        nextHealth.SetSpawnGrace(spawnGracePeriod);
                    }
                    else
                    {
                        nextHealth.SetSpawnGrace(0f);
                    }
                }

                if (inheritVelocity && spawnedEnemy.TryGetComponent<Rigidbody2D>(out var spawnedRb))
                {
                    spawnedRb.linearVelocity = forwardVelocity;

                    if (spawnedEnemy.TryGetComponent<MovementTest>(out var moveScript))
                    {
                        moveScript.UpdateRotation();
                    }
                }
            }
        }

        // 2. ป้องกันการเกิดแต้มเบิ้ล
        if (!hasNextStage && pointPrefab != null && (!killedByPlayerCollision || !giveScoreOnPlayerHit))
        {
            GameObject dropPoint = Instantiate(pointPrefab, transform.position, Quaternion.identity);
            if (dropPoint.TryGetComponent(out PointItem pointScript))
            {
                pointScript.Setup(scoreReward, inheritVelocity ? forwardVelocity : Vector2.left * defaultFlySpeed);
            }
        }

        // 3. ปล่อยหนามเฉพาะตัวศัตรูที่มีดาเมจ
        if (contactDamage > 0 && spawnSpikesOnDeath && spikePrefab != null)
        {
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